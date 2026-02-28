## Computes valid property ranges given a body type and a set of locked values.
## Pure domain logic: no Nodes, no RNG, no file IO.
##
## The solver has two responsibilities:
##   1. Produce absolute (type-gated) bounds for each editable property.
##   2. Narrow those bounds when other properties are locked.
##
## Coupling rules currently implemented:
##   - mass <-> radius via density bounds per body category
##   - oblateness upper bound from rotation period (centrifugal breakup)
##
## Extend by adding rules to _apply_coupling.
class_name PropertyConstraintSolver
extends RefCounted

const _size_table: GDScript = preload("res://src/domain/generation/tables/SizeTable.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Density bounds in kg/m^3 per body type.
## Planets do NOT use these directly — they are looked up via
## SizeTable using the current size category (gas giant vs rocky, etc.).
## These values mirror the bounds currently hard-coded in EditDialog.gd,
## so moving to the solver keeps behaviour identical.
const _STAR_DENSITY_MIN: float = 0.0001
const _STAR_DENSITY_MAX: float = 150000.0
const _MOON_DENSITY_MIN: float = 800.0
const _MOON_DENSITY_MAX: float = 6000.0
const _ASTEROID_DENSITY_MIN: float = 500.0
const _ASTEROID_DENSITY_MAX: float = 8000.0
## Fallback for unknown body types.
const _FALLBACK_DENSITY_MIN: float = 500.0
const _FALLBACK_DENSITY_MAX: float = 10000.0

## Absolute mass bounds per body type (kg).
const _STAR_MASS_MIN: float = 0.08 * 1.989e30 # brown-dwarf limit
const _STAR_MASS_MAX: float = 300.0 * 1.989e30 # most massive known stars
const _PLANET_MASS_MIN: float = 0.0001 * 5.972e24
const _PLANET_MASS_MAX: float = 5000.0 * 5.972e24
const _MOON_MASS_MIN: float = 1.0e15
const _MOON_MASS_MAX: float = 2.0 * 5.972e24
const _ASTEROID_MASS_MIN: float = 1.0e10
const _ASTEROID_MASS_MAX: float = 1.0e22

## Absolute radius bounds per body type (m).
const _STAR_RADIUS_MIN: float = 0.001 * 6.957e8 # white dwarfs
const _STAR_RADIUS_MAX: float = 2000.0 * 6.957e8 # red supergiants
const _PLANET_RADIUS_MIN: float = 0.01 * 6.371e6
const _PLANET_RADIUS_MAX: float = 30.0 * 6.371e6
const _MOON_RADIUS_MIN: float = 1.0e3
const _MOON_RADIUS_MAX: float = 5.0e6
const _ASTEROID_RADIUS_MIN: float = 1.0
const _ASTEROID_RADIUS_MAX: float = 1.0e6

## Rotation period bounds in seconds.
const _ROTATION_MIN_S: float = 360.0 # 0.1 hr; below this, breakup for most rocky bodies
const _ROTATION_MAX_S: float = 3.6e7 # ~10,000 hr (tidally-locked extreme)

## Stellar temperature bounds (K).
const _STELLAR_TEMP_MIN_K: float = 2000.0
const _STELLAR_TEMP_MAX_K: float = 50000.0

## Stellar luminosity bounds (W).
const _STELLAR_LUM_MIN_W: float = 1.0e-5 * 3.828e26
const _STELLAR_LUM_MAX_W: float = 1.0e7 * 3.828e26

## Stellar age bounds (yr).
const _STELLAR_AGE_MIN_YR: float = 1.0e6
const _STELLAR_AGE_MAX_YR: float = 1.5e10


## Builds a ConstraintSet for the given body.
## Starts from absolute type-gated bounds, applies initial values, then
## narrows using coupling rules on whatever is already locked.
## @param body_type: The CelestialType.Type of the body being edited.
## @param current_values: Dict of property_path -> base-unit value.
## @param locked_paths: Array of property paths the user has locked.
## @return: Populated ConstraintSet.
static func solve(
	body_type: int,
	current_values: Dictionary,
	locked_paths: Array[String]
) -> ConstraintSet:
	var cs: ConstraintSet = ConstraintSet.new()

	_seed_absolute_bounds(body_type, current_values, cs)

	# Flag locked properties before coupling so coupling rules know
	# which side of a relation is authoritative.
	for path: String in locked_paths:
		cs.lock(path)

	_apply_coupling(body_type, cs)

	return cs


## Builds a ConstraintSet and additionally narrows it with caller-supplied
## per-property bounds (e.g. from a Traveller size code).
## Extra bounds are applied *before* coupling so the narrowed ranges
## feed into the mass/radius coupling calculation.
## @param body_type: The CelestialType.Type of the body being edited.
## @param current_values: Dict of property_path -> base-unit value.
## @param locked_paths: Array of property paths the user has locked.
## @param extra_bounds: Dict of property_path -> Vector2(min, max) in base units.
## @return: Populated ConstraintSet.
static func solve_with_extra_constraints(
	body_type: int,
	current_values: Dictionary,
	locked_paths: Array[String],
	extra_bounds: Dictionary
) -> ConstraintSet:
	var cs: ConstraintSet = ConstraintSet.new()

	_seed_absolute_bounds(body_type, current_values, cs)

	# Layer extra bounds on top of absolute bounds before locking so that
	# when coupling runs, it sees the already-narrowed Traveller window.
	for path: Variant in extra_bounds.keys():
		var path_str: String = path as String
		var c: PropertyConstraint = cs.get_constraint(path_str)
		if c == null:
			continue
		var bounds: Vector2 = extra_bounds[path] as Vector2
		cs.set_constraint(c.intersected_with(bounds.x, bounds.y, "Traveller UWP"))

	for path: String in locked_paths:
		cs.lock(path)

	_apply_coupling(body_type, cs)

	return cs


## Narrows constraints for properties coupled to locked ones.
## @param body_type: The body type.
## @param cs: The constraint set to mutate.
static func _apply_coupling(body_type: int, cs: ConstraintSet) -> void:
	_apply_mass_radius_coupling(body_type, cs)
	_apply_oblateness_rotation_coupling(cs)


## Narrows mass or radius based on density bounds when the other is locked.
## @param body_type: The body type (drives density bounds).
## @param cs: Constraint set to mutate.
static func _apply_mass_radius_coupling(body_type: int, cs: ConstraintSet) -> void:
	var mass_c: PropertyConstraint = cs.get_constraint("physical.mass_kg")
	var radius_c: PropertyConstraint = cs.get_constraint("physical.radius_m")
	if mass_c == null or radius_c == null:
		return

	var density_bounds: Vector2 = _density_bounds_for(body_type, mass_c.current_value)

	# When mass is locked, derive radius window from density bounds.
	# volume = mass / density; r = (3V / 4π)^(1/3)
	if mass_c.is_locked and not radius_c.is_locked:
		var r_min: float = _radius_from_mass_density(mass_c.current_value, density_bounds.y)
		var r_max: float = _radius_from_mass_density(mass_c.current_value, density_bounds.x)
		cs.set_constraint(radius_c.intersected_with(r_min, r_max, "density bounds from locked mass"))

	# When radius is locked, derive mass window from density bounds.
	# mass = density * volume
	elif radius_c.is_locked and not mass_c.is_locked:
		var vol: float = (4.0 / 3.0) * PI * pow(radius_c.current_value, 3.0)
		var m_min: float = vol * density_bounds.x
		var m_max: float = vol * density_bounds.y
		cs.set_constraint(mass_c.intersected_with(m_min, m_max, "density bounds from locked radius"))


## Narrows oblateness upper bound when rotation period is locked.
## Fast rotation permits high oblateness; slow rotation does not.
## @param cs: The constraint set to mutate.
static func _apply_oblateness_rotation_coupling(cs: ConstraintSet) -> void:
	var rot_c: PropertyConstraint = cs.get_constraint("physical.rotation_period_s")
	var obl_c: PropertyConstraint = cs.get_constraint("physical.oblateness")
	if rot_c == null or obl_c == null:
		return
	if not rot_c.is_locked:
		return
	# Very rough empirical cap: bodies rotating slower than ~10 hrs
	# rarely exceed ~0.1 oblateness (Saturn is 0.1 at 10.7 hrs).
	# Faster rotation -> higher allowed oblateness, up to 0.5.
	var hours: float = absf(rot_c.current_value) / 3600.0
	var max_obl: float = 0.5
	if hours > 2.0:
		# Linear falloff from 0.5 at 2 hrs to 0.02 at 200 hrs.
		var t: float = clampf((hours - 2.0) / 198.0, 0.0, 1.0)
		max_obl = lerpf(0.5, 0.02, t)
	cs.set_constraint(obl_c.intersected_with(0.0, max_obl, "rotation period limits oblateness"))


## Seeds absolute per-type bounds and current values into the set.
## @param body_type: The body type.
## @param current_values: Dict of path -> current base-unit value.
## @param cs: Constraint set to populate.
static func _seed_absolute_bounds(
	body_type: int,
	current_values: Dictionary,
	cs: ConstraintSet
) -> void:
	var mass_range: Vector2 = _absolute_mass_range(body_type)
	var radius_range: Vector2 = _absolute_radius_range(body_type)

	cs.set_constraint(PropertyConstraint.new(
		"physical.mass_kg",
		mass_range.x, mass_range.y,
		_cv(current_values, "physical.mass_kg", 1.0),
		false, "body type"
	))
	cs.set_constraint(PropertyConstraint.new(
		"physical.radius_m",
		radius_range.x, radius_range.y,
		_cv(current_values, "physical.radius_m", 1.0),
		false, "body type"
	))
	cs.set_constraint(PropertyConstraint.new(
		"physical.rotation_period_s",
		_ROTATION_MIN_S, _ROTATION_MAX_S,
		_cv(current_values, "physical.rotation_period_s", 86400.0),
		false, ""
	))
	cs.set_constraint(PropertyConstraint.new(
		"physical.axial_tilt_deg",
		0.0, 180.0,
		_cv(current_values, "physical.axial_tilt_deg", 0.0),
		false, "validator"
	))
	cs.set_constraint(PropertyConstraint.new(
		"physical.oblateness",
		0.0, 0.5,
		_cv(current_values, "physical.oblateness", 0.0),
		false, ""
	))

	# Stellar properties only for stars. Use stellar.temperature_k to match EditDialog override key.
	if body_type == CelestialType.Type.STAR:
		cs.set_constraint(PropertyConstraint.new(
			"stellar.temperature_k",
			_STELLAR_TEMP_MIN_K, _STELLAR_TEMP_MAX_K,
			_cv(current_values, "stellar.temperature_k", 5778.0),
			false, ""
		))
		cs.set_constraint(PropertyConstraint.new(
			"stellar.luminosity_watts",
			_STELLAR_LUM_MIN_W, _STELLAR_LUM_MAX_W,
			_cv(current_values, "stellar.luminosity_watts", 3.828e26),
			false, ""
		))
		cs.set_constraint(PropertyConstraint.new(
			"stellar.age_years",
			_STELLAR_AGE_MIN_YR, _STELLAR_AGE_MAX_YR,
			_cv(current_values, "stellar.age_years", 4.6e9),
			false, ""
		))
		cs.set_constraint(PropertyConstraint.new(
			"stellar.metallicity",
			0.001, 10.0,
			_cv(current_values, "stellar.metallicity", 1.0),
			false, ""
		))

	# Orbital bounds — these depend on parent context which the solver
	# doesn't have yet. For now, provide generous absolute bounds.
	# A later slice will add a ParentContext parameter to solve().
	cs.set_constraint(PropertyConstraint.new(
		"orbital.semi_major_axis_m",
		1.0e3, 1.0e15,
		_cv(current_values, "orbital.semi_major_axis_m", Units.AU_METERS),
		false, "unbounded (no parent context)"
	))
	cs.set_constraint(PropertyConstraint.new(
		"orbital.eccentricity",
		0.0, 0.99,
		_cv(current_values, "orbital.eccentricity", 0.0),
		false, "validator"
	))
	cs.set_constraint(PropertyConstraint.new(
		"orbital.inclination_deg",
		0.0, 180.0,
		_cv(current_values, "orbital.inclination_deg", 0.0),
		false, "validator"
	))

	# Atmosphere / surface bounds.
	cs.set_constraint(PropertyConstraint.new(
		"atmosphere.surface_pressure_pa",
		0.0, 1.0e9,
		_cv(current_values, "atmosphere.surface_pressure_pa", 0.0),
		false, ""
	))
	cs.set_constraint(PropertyConstraint.new(
		"atmosphere.scale_height_m",
		1.0, 5.0e5,
		_cv(current_values, "atmosphere.scale_height_m", 8500.0),
		false, ""
	))
	cs.set_constraint(PropertyConstraint.new(
		"atmosphere.greenhouse_factor",
		1.0, 100.0,
		_cv(current_values, "atmosphere.greenhouse_factor", 1.0),
		false, ""
	))
	cs.set_constraint(PropertyConstraint.new(
		"surface.temperature_k",
		0.0, 5000.0,
		_cv(current_values, "surface.temperature_k", 288.0),
		false, ""
	))
	cs.set_constraint(PropertyConstraint.new(
		"surface.albedo",
		0.0, 1.0,
		_cv(current_values, "surface.albedo", 0.3),
		false, "validator"
	))
	cs.set_constraint(PropertyConstraint.new(
		"surface.volcanism_level",
		0.0, 1.0,
		_cv(current_values, "surface.volcanism_level", 0.0),
		false, "validator"
	))


## Density bounds (min, max) in kg/m^3 for the given body.
## Planets use SizeTable based on current mass to get category-specific
## ranges — gas giants and rocky planets have very different densities.
## @param body_type: The body type.
## @param current_mass_kg: Current mass, used for planet category lookup.
## @return: Vector2(min_density, max_density) in kg/m^3.
static func _density_bounds_for(body_type: int, current_mass_kg: float) -> Vector2:
	match body_type:
		CelestialType.Type.STAR:
			return Vector2(_STAR_DENSITY_MIN, _STAR_DENSITY_MAX)
		CelestialType.Type.PLANET:
			var mass_earth: float = current_mass_kg / Units.EARTH_MASS_KG
			var cat: int = SizeTable.category_from_mass(mass_earth)
			var r: Dictionary = SizeTable.get_density_range(cat)
			# Widen SizeTable ranges slightly: those are *typical*
			# generation ranges, not hard physical limits.
			var min_d: float = (r.get("min", _FALLBACK_DENSITY_MIN) as float) * 0.7
			var max_d: float = (r.get("max", _FALLBACK_DENSITY_MAX) as float) * 1.3
			return Vector2(min_d, max_d)
		CelestialType.Type.MOON:
			return Vector2(_MOON_DENSITY_MIN, _MOON_DENSITY_MAX)
		CelestialType.Type.ASTEROID:
			return Vector2(_ASTEROID_DENSITY_MIN, _ASTEROID_DENSITY_MAX)
		_:
			return Vector2(_FALLBACK_DENSITY_MIN, _FALLBACK_DENSITY_MAX)


## Absolute mass range (min, max) in kg for the given body type.
## @param body_type: The body type.
## @return: Vector2(min_mass, max_mass) in kg.
static func _absolute_mass_range(body_type: int) -> Vector2:
	match body_type:
		CelestialType.Type.STAR:
			return Vector2(_STAR_MASS_MIN, _STAR_MASS_MAX)
		CelestialType.Type.PLANET:
			return Vector2(_PLANET_MASS_MIN, _PLANET_MASS_MAX)
		CelestialType.Type.MOON:
			return Vector2(_MOON_MASS_MIN, _MOON_MASS_MAX)
		CelestialType.Type.ASTEROID:
			return Vector2(_ASTEROID_MASS_MIN, _ASTEROID_MASS_MAX)
		_:
			return Vector2(1.0, 1.0e40)


## Absolute radius range (min, max) in m for the given body type.
## @param body_type: The body type.
## @return: Vector2(min_radius, max_radius) in m.
static func _absolute_radius_range(body_type: int) -> Vector2:
	match body_type:
		CelestialType.Type.STAR:
			return Vector2(_STAR_RADIUS_MIN, _STAR_RADIUS_MAX)
		CelestialType.Type.PLANET:
			return Vector2(_PLANET_RADIUS_MIN, _PLANET_RADIUS_MAX)
		CelestialType.Type.MOON:
			return Vector2(_MOON_RADIUS_MIN, _MOON_RADIUS_MAX)
		CelestialType.Type.ASTEROID:
			return Vector2(_ASTEROID_RADIUS_MIN, _ASTEROID_RADIUS_MAX)
		_:
			return Vector2(1.0, 1.0e12)


## Computes radius from mass and density assuming a uniform sphere.
## @param mass_kg: Mass in kg.
## @param density_kg_m3: Density in kg/m^3.
## @return: Radius in m (0 if inputs non-positive).
static func _radius_from_mass_density(mass_kg: float, density_kg_m3: float) -> float:
	if mass_kg <= 0.0 or density_kg_m3 <= 0.0:
		return 0.0
	var vol: float = mass_kg / density_kg_m3
	return pow(vol * 3.0 / (4.0 * PI), 1.0 / 3.0)


## Typed helper to read a float from a current-values dictionary.
## @param values: The values dict.
## @param path: Property path key.
## @param default_value: Fallback if absent.
## @return: The value as float.
static func _cv(values: Dictionary, path: String, default_value: float) -> float:
	if values.has(path):
		return values[path] as float
	return default_value
