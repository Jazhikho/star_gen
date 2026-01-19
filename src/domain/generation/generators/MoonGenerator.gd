## Generates moon CelestialBody objects from MoonSpec.
## Produces deterministic output based on spec, parent context, and seed.
class_name MoonGenerator
extends RefCounted

const _moon_spec := preload("res://src/domain/generation/specs/MoonSpec.gd")
const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _size_table := preload("res://src/domain/generation/tables/SizeTable.gd")
const _orbit_table := preload("res://src/domain/generation/tables/OrbitTable.gd")
const _generator_utils := preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere_props := preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _surface_props := preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _terrain_props := preload("res://src/domain/celestial/components/TerrainProps.gd")
const _hydrosphere_props := preload("res://src/domain/celestial/components/HydrosphereProps.gd")
const _cryosphere_props := preload("res://src/domain/celestial/components/CryosphereProps.gd")
const _provenance := preload("res://src/domain/celestial/Provenance.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Size category distribution weights for regular moons.
const SIZE_CATEGORY_WEIGHTS_REGULAR: Array[float] = [
	30.0,  # DWARF - common for moons
	50.0,  # SUB_TERRESTRIAL - most common (Luna, Europa, Titan)
	15.0,  # TERRESTRIAL - rare but possible
	5.0,   # SUPER_EARTH - very rare
]

## Size category distribution weights for captured moons.
const SIZE_CATEGORY_WEIGHTS_CAPTURED: Array[float] = [
	80.0,  # DWARF - most captured moons are small
	18.0,  # SUB_TERRESTRIAL - uncommon
	2.0,   # TERRESTRIAL - very rare
	0.0,   # SUPER_EARTH - essentially impossible
]

## Earth's atmospheric pressure in Pascals.
const EARTH_ATMOSPHERE_PA: float = 101325.0

## Boltzmann constant in J/K.
const BOLTZMANN_K: float = 1.380649e-23

## Hydrogen molecular mass in kg.
const HYDROGEN_MASS_KG: float = 1.6735575e-27

## Nitrogen molecular mass in kg.
const NITROGEN_MASS_KG: float = 4.6518e-26

## Water freezing point in Kelvin.
const WATER_FREEZE_K: float = 273.15

## Minimum Hill sphere fraction for stable orbits.
const MIN_HILL_FRACTION: float = 0.05

## Maximum Hill sphere fraction for stable orbits (prograde).
const MAX_HILL_FRACTION_PROGRADE: float = 0.5

## Maximum Hill sphere fraction for stable orbits (retrograde/captured).
const MAX_HILL_FRACTION_RETROGRADE: float = 0.7


## Generates a moon from a specification and parent context.
## @param spec: The moon specification.
## @param context: The parent planet/star context.
## @param rng: The random number generator (will be advanced).
## @return: A new CelestialBody configured as a moon.
static func generate(spec: MoonSpec, context: ParentContext, rng: SeededRng) -> CelestialBody:
	# Validate context has parent body (planet)
	if not context.has_parent_body():
		push_error("MoonGenerator requires a ParentContext with parent body data")
		return null
	
	# Determine size category
	var size_cat: SizeCategory.Category = _determine_size_category(spec, rng)
	
	# Generate orbital properties first (needed for tidal effects)
	var orbital: OrbitalProps = _generate_orbital_props(spec, context, size_cat, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _generate_physical_props(spec, context, size_cat, orbital, rng)
	
	# Calculate equilibrium temperature from star
	var equilibrium_temp_k: float = context.get_equilibrium_temperature_k(0.3)
	
	# Calculate tidal heating contribution
	var tidal_heat_watts: float = _calculate_tidal_heating(
		physical, orbital, context
	)
	
	# Add tidal heating to internal heat
	physical = PhysicalProps.new(
		physical.mass_kg,
		physical.radius_m,
		physical.rotation_period_s,
		physical.axial_tilt_deg,
		physical.oblateness,
		physical.magnetic_moment,
		physical.internal_heat_watts + tidal_heat_watts
	)
	
	# Determine if moon should have atmosphere
	var should_have_atmosphere: bool = _should_have_atmosphere(
		spec, physical, size_cat, context, rng
	)
	
	# Generate atmosphere if applicable
	var atmosphere: AtmosphereProps = null
	if should_have_atmosphere:
		atmosphere = _generate_atmosphere(spec, physical, size_cat, equilibrium_temp_k, rng)
	
	# Calculate surface temperature
	var surface_temp_k: float = equilibrium_temp_k
	if atmosphere != null:
		surface_temp_k = equilibrium_temp_k * atmosphere.greenhouse_factor
	
	# Generate surface properties (moons always have surfaces, even icy ones)
	var surface: SurfaceProps = _generate_surface(
		spec, physical, size_cat, surface_temp_k, tidal_heat_watts, context, rng
	)
	
	# Generate ID
	var body_id: String = _generate_id(spec, rng)
	
	# Create provenance
	var provenance: Provenance = Provenance.new(
		spec.generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		spec.to_dict()
	)
	
	# Assemble the celestial body
	var body: CelestialBody = CelestialBody.new(
		body_id,
		spec.name_hint,
		CelestialType.Type.MOON,
		physical,
		provenance
	)
	body.orbital = orbital
	body.atmosphere = atmosphere
	body.surface = surface
	
	return body


## Determines the size category from spec or random selection.
## @param spec: The moon specification.
## @param rng: The random number generator.
## @return: The selected size category.
static func _determine_size_category(spec: MoonSpec, rng: SeededRng) -> SizeCategory.Category:
	if spec.has_size_category():
		return spec.size_category as SizeCategory.Category
	
	# Moons are limited to rocky categories
	var categories: Array = [
		SizeCategory.Category.DWARF,
		SizeCategory.Category.SUB_TERRESTRIAL,
		SizeCategory.Category.TERRESTRIAL,
		SizeCategory.Category.SUPER_EARTH,
	]
	
	# Use different weights for captured vs regular moons
	var weights: Array[float]
	if spec.is_captured:
		weights = SIZE_CATEGORY_WEIGHTS_CAPTURED
	else:
		weights = SIZE_CATEGORY_WEIGHTS_REGULAR
	
	var selected: Variant = rng.weighted_choice(categories, weights)
	return selected as SizeCategory.Category


## Generates orbital properties for the moon.
## Constrained by Hill sphere of parent planet.
## @param spec: The moon specification.
## @param context: The parent context.
## @param size_cat: The size category.
## @param rng: The random number generator.
## @return: OrbitalProps for the moon.
static func _generate_orbital_props(
	spec: MoonSpec,
	context: ParentContext,
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> OrbitalProps:
	# Get Hill sphere radius
	var hill_radius_m: float = context.get_hill_sphere_radius_m()
	
	# Estimate moon density for Roche limit calculation
	var density_range: Dictionary = SizeTable.get_density_range(size_cat)
	var estimated_density: float = (density_range["min"] + density_range["max"]) / 2.0
	
	# Get Roche limit
	var roche_limit_m: float = context.get_roche_limit_m(estimated_density)
	
	# Determine min/max orbital distance
	var min_distance_m: float = maxf(roche_limit_m * 1.5, context.parent_body_radius_m * 2.0)
	
	var max_fraction: float
	if spec.is_captured:
		max_fraction = MAX_HILL_FRACTION_RETROGRADE
	else:
		max_fraction = MAX_HILL_FRACTION_PROGRADE
	var max_distance_m: float = hill_radius_m * max_fraction
	
	# Ensure valid range
	if min_distance_m >= max_distance_m:
		min_distance_m = context.parent_body_radius_m * 3.0
		max_distance_m = context.parent_body_radius_m * 100.0
	
	# Semi-major axis
	var semi_major_axis_m: float = spec.get_override_float(
		"orbital.semi_major_axis_m",
		-1.0
	)
	if semi_major_axis_m < 0.0:
		# Log-uniform distribution for orbital spacing
		var log_min: float = log(min_distance_m)
		var log_max: float = log(max_distance_m)
		var log_val: float = rng.randf_range(log_min, log_max)
		semi_major_axis_m = exp(log_val)
	
	# Eccentricity - regular moons tend to be circular, captured can be eccentric
	var eccentricity: float = spec.get_override_float(
		"orbital.eccentricity",
		-1.0
	)
	if eccentricity < 0.0:
		if spec.is_captured:
			eccentricity = rng.randf_range(0.1, 0.5)
		else:
			# Bias toward circular
			var raw: float = rng.randf()
			eccentricity = raw * raw * 0.1  # Max ~0.1 for regular
	
	# Inclination - regular moons in equatorial plane, captured can be inclined
	var inclination_deg: float = spec.get_override_float(
		"orbital.inclination_deg",
		-1.0
	)
	if inclination_deg < 0.0:
		if spec.is_captured:
			# Captured moons can have any inclination, including retrograde
			inclination_deg = rng.randf_range(0.0, 180.0)
		else:
			# Regular moons: low inclination
			inclination_deg = rng.randf_range(0.0, 5.0)
	
	# Other orbital elements
	var longitude_of_ascending_node_deg: float = spec.get_override_float(
		"orbital.longitude_of_ascending_node_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	var argument_of_periapsis_deg: float = spec.get_override_float(
		"orbital.argument_of_periapsis_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	var mean_anomaly_deg: float = spec.get_override_float(
		"orbital.mean_anomaly_deg",
		rng.randf_range(0.0, 360.0)
	)
	
	return OrbitalProps.new(
		semi_major_axis_m,
		eccentricity,
		inclination_deg,
		longitude_of_ascending_node_deg,
		argument_of_periapsis_deg,
		mean_anomaly_deg,
		""  # Parent ID set later by system generator
	)


## Generates physical properties for the moon.
## @param spec: The moon specification.
## @param context: The parent context.
## @param size_cat: The size category.
## @param orbital: The orbital properties (for tidal locking).
## @param rng: The random number generator.
## @return: PhysicalProps for the moon.
static func _generate_physical_props(
	spec: MoonSpec,
	context: ParentContext,
	size_cat: SizeCategory.Category,
	orbital: OrbitalProps,
	rng: SeededRng
) -> PhysicalProps:
	# Generate density first
	var density_kg_m3: float = spec.get_override_float(
		"physical.density_kg_m3",
		-1.0
	)
	if density_kg_m3 < 0.0:
		density_kg_m3 = SizeTable.random_density(size_cat, rng)
	
	# Generate mass - constrained by parent mass
	var mass_kg: float = spec.get_override_float("physical.mass_kg", -1.0)
	var mass_earth: float = -1.0
	
	if mass_kg < 0.0:
		mass_earth = spec.get_override_float("physical.mass_earth", -1.0)
		if mass_earth < 0.0:
			mass_earth = SizeTable.random_mass_earth(size_cat, rng)
			# Constrain: moon mass should be << parent mass
			var max_moon_mass_kg: float = context.parent_body_mass_kg * 0.1
			var max_moon_mass_earth: float = max_moon_mass_kg / Units.EARTH_MASS_KG
			mass_earth = minf(mass_earth, max_moon_mass_earth)
		mass_kg = mass_earth * Units.EARTH_MASS_KG
	else:
		mass_earth = mass_kg / Units.EARTH_MASS_KG
	
	# Calculate radius from mass and density
	var radius_m: float = spec.get_override_float("physical.radius_m", -1.0)
	if radius_m < 0.0:
		var radius_earth: float = spec.get_override_float("physical.radius_earth", -1.0)
		if radius_earth < 0.0:
			radius_m = SizeTable.radius_from_mass_density(mass_kg, density_kg_m3)
		else:
			radius_m = radius_earth * Units.EARTH_RADIUS_METERS
	
	# Determine if tidally locked to parent
	var is_locked: bool = _is_tidally_locked_to_parent(
		orbital.semi_major_axis_m,
		mass_kg,
		radius_m,
		context.parent_body_mass_kg,
		context.stellar_age_years
	)
	
	# Rotation period
	var rotation_period_s: float = spec.get_override_float("physical.rotation_period_s", -1.0)
	if rotation_period_s < 0.0:
		rotation_period_s = _calculate_rotation_period(
			orbital,
			context.parent_body_mass_kg,
			is_locked,
			rng
		)
	
	# Axial tilt
	var axial_tilt_deg: float = spec.get_override_float("physical.axial_tilt_deg", -1.0)
	if axial_tilt_deg < 0.0:
		axial_tilt_deg = _calculate_axial_tilt(is_locked, rng)
	
	# Oblateness (very small for moons due to slow rotation)
	var oblateness: float = spec.get_override_float("physical.oblateness", -1.0)
	if oblateness < 0.0:
		if is_locked:
			oblateness = rng.randf_range(0.0, 0.005)
		else:
			oblateness = rng.randf_range(0.0, 0.02)
	
	# Magnetic moment (most moons lack dynamos)
	var magnetic_moment: float = spec.get_override_float("physical.magnetic_moment", -1.0)
	if magnetic_moment < 0.0:
		magnetic_moment = _calculate_magnetic_moment(mass_kg, radius_m, rotation_period_s, rng)
	
	# Internal heat (base, before tidal contribution)
	var internal_heat_watts: float = spec.get_override_float("physical.internal_heat_watts", -1.0)
	if internal_heat_watts < 0.0:
		internal_heat_watts = _calculate_internal_heat(mass_kg, context.stellar_age_years, rng)
	
	return PhysicalProps.new(
		mass_kg,
		radius_m,
		rotation_period_s,
		axial_tilt_deg,
		oblateness,
		magnetic_moment,
		internal_heat_watts
	)


## Checks if the moon would be tidally locked to its parent.
## @param orbital_distance_m: Distance from parent planet.
## @param mass_kg: Moon mass.
## @param radius_m: Moon radius.
## @param parent_mass_kg: Parent planet mass.
## @param system_age_years: Age of the system.
## @return: True if tidally locked.
static func _is_tidally_locked_to_parent(
	orbital_distance_m: float,
	mass_kg: float,
	radius_m: float,
	parent_mass_kg: float,
	system_age_years: float
) -> bool:
	# Use similar formula to planet-star tidal locking
	# But moons lock much faster due to closer proximity
	if radius_m <= 0.0 or parent_mass_kg <= 0.0:
		return false
	
	# Tidal locking timescale for moons (much shorter than planets)
	# τ ∝ a^6 * m / (M^2 * R^3)
	var a_km: float = orbital_distance_m / 1000.0
	var m_moon_kg: float = mass_kg
	var m_parent_kg: float = parent_mass_kg
	var r_km: float = radius_m / 1000.0
	
	# Reference: Luna locked to Earth in < 1 billion years
	# Luna: a = 384,400 km, m = 7.35e22 kg, R = 1737 km, M_earth = 5.97e24 kg
	var tau: float = 1.0e8 * pow(a_km / 384400.0, 6.0) * (m_moon_kg / 7.35e22) / (pow(m_parent_kg / 5.97e24, 2.0) * pow(r_km / 1737.0, 3.0))
	
	return system_age_years > tau


## Calculates the rotation period.
## @param orbital: Orbital properties.
## @param parent_mass_kg: Parent planet mass.
## @param is_locked: Whether tidally locked.
## @param rng: Random number generator.
## @return: Rotation period in seconds.
static func _calculate_rotation_period(
	orbital: OrbitalProps,
	parent_mass_kg: float,
	is_locked: bool,
	rng: SeededRng
) -> float:
	if is_locked:
		# Rotation period equals orbital period
		return orbital.get_orbital_period_s(parent_mass_kg)
	
	# Non-locked moons: faster rotation
	var base_hours: float = rng.randf_range(5.0, 50.0)
	return base_hours * 3600.0


## Calculates axial tilt.
## @param is_locked: Whether tidally locked.
## @param rng: Random number generator.
## @return: Axial tilt in degrees.
static func _calculate_axial_tilt(is_locked: bool, rng: SeededRng) -> float:
	if is_locked:
		# Tidally locked bodies have very low obliquity
		return rng.randf_range(0.0, 5.0)
	
	# Non-locked: can have varied tilt
	return rng.randf_range(0.0, 30.0)


## Calculates magnetic dipole moment.
## Most moons lack dynamos due to small size and slow rotation.
## @param mass_kg: Moon mass.
## @param _radius_m: Moon radius (reserved for future use).
## @param rotation_period_s: Rotation period.
## @param rng: Random number generator.
## @return: Magnetic moment in T·m³.
static func _calculate_magnetic_moment(
	mass_kg: float,
	_radius_m: float,
	rotation_period_s: float,
	rng: SeededRng
) -> float:
	var mass_earth: float = mass_kg / Units.EARTH_MASS_KG
	var rotation_hours: float = absf(rotation_period_s) / 3600.0
	
	# Most moons are too small/slow for dynamos
	if mass_earth < 0.01:
		return 0.0
	
	if rotation_hours > 100.0:
		return 0.0
	
	# Only large, fast-rotating moons might have fields
	# Ganymede is the only moon with significant field
	if rng.randf() < 0.9:
		return 0.0  # 90% chance of no field
	
	# Weak field if present
	var earth_moment: float = 8.0e22
	var base_moment: float = earth_moment * pow(mass_earth, 0.5) * (24.0 / maxf(rotation_hours, 1.0)) * 0.1
	
	return base_moment * rng.randf_range(0.01, 0.5)


## Calculates internal heat generation (radiogenic only, before tidal).
## @param mass_kg: Moon mass.
## @param age_years: System age.
## @param rng: Random number generator.
## @return: Internal heat in watts.
static func _calculate_internal_heat(
	mass_kg: float,
	age_years: float,
	rng: SeededRng
) -> float:
	# Radiogenic heating scales with mass
	var earth_heat: float = 4.7e13
	var mass_earth: float = mass_kg / Units.EARTH_MASS_KG
	
	# Age decay factor
	var age_factor: float = 1.0
	if age_years > 0.0:
		age_factor = pow(0.5, age_years / 2.0e9)
		age_factor = maxf(age_factor, 0.1)
	
	# Smaller bodies cool faster
	var size_factor: float = pow(mass_earth, 0.8)
	
	var base_heat: float = earth_heat * size_factor * age_factor * 0.5  # Moons have less radiogenic material
	
	return base_heat * rng.randf_range(0.3, 1.5)


## Calculates tidal heating from parent planet.
## @param physical: Physical properties.
## @param orbital: Orbital properties.
## @param context: Parent context.
## @return: Tidal heating in watts.
static func _calculate_tidal_heating(
	physical: PhysicalProps,
	orbital: OrbitalProps,
	context: ParentContext
) -> float:
	# Tidal heating depends on eccentricity and orbital distance
	# Io: ~1e14 W with e~0.004 at 421,800 km from Jupiter
	
	var eccentricity: float = orbital.eccentricity
	if eccentricity < 0.001:
		return 0.0  # Circular orbit = no tidal flexing
	
	var distance_m: float = orbital.semi_major_axis_m
	var radius_m: float = physical.radius_m
	var parent_mass_kg: float = context.parent_body_mass_kg
	
	if distance_m <= 0.0 or radius_m <= 0.0:
		return 0.0
	
	# Simplified tidal heating formula
	# Q_tidal ∝ (M_parent² * R⁵ * e²) / (a⁶)
	# Reference: Io produces ~1e14 W
	var io_ref_heat: float = 1.0e14
	var io_e: float = 0.004
	var io_a: float = 4.218e8  # 421,800 km
	var io_r: float = 1.8216e6  # 1821.6 km
	var jupiter_mass: float = 1.898e27
	
	var mass_ratio: float = pow(parent_mass_kg / jupiter_mass, 2.0)
	var radius_ratio: float = pow(radius_m / io_r, 5.0)
	var ecc_ratio: float = pow(eccentricity / io_e, 2.0)
	var distance_ratio: float = pow(io_a / distance_m, 6.0)
	
	var tidal_heat: float = io_ref_heat * mass_ratio * radius_ratio * ecc_ratio * distance_ratio
	
	# Cap at reasonable maximum
	return minf(tidal_heat, 1.0e16)


## Determines whether this moon should have an atmosphere.
## @param spec: The moon specification.
## @param physical: The physical properties.
## @param size_cat: The size category.
## @param context: The parent context.
## @param rng: Random number generator.
## @return: True if atmosphere should be generated.
static func _should_have_atmosphere(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext,
	rng: SeededRng
) -> bool:
	# Check spec preference
	if spec.has_atmosphere_preference():
		return spec.has_atmosphere as bool
	
	# Most moons don't have atmospheres
	# Only large moons (Titan-sized) can retain significant atmospheres
	
	var escape_velocity: float = physical.get_escape_velocity_m_s()
	var equilibrium_temp: float = context.get_equilibrium_temperature_k(0.3)
	
	# Thermal velocity for nitrogen (Titan's main gas)
	var thermal_velocity: float = sqrt(3.0 * BOLTZMANN_K * equilibrium_temp / NITROGEN_MASS_KG)
	
	var jeans_param: float = escape_velocity / thermal_velocity
	
	# Need jeans > 6 for long-term retention
	if jeans_param < 6.0:
		return false
	
	# Size requirement: at least sub-terrestrial
	if size_cat == SizeCategory.Category.DWARF:
		return false
	
	# Probability based on size
	var prob: float
	match size_cat:
		SizeCategory.Category.SUB_TERRESTRIAL:
			prob = 0.1  # Rare (only Titan in our solar system)
		SizeCategory.Category.TERRESTRIAL:
			prob = 0.3
		SizeCategory.Category.SUPER_EARTH:
			prob = 0.5
		_:
			prob = 0.0
	
	return rng.randf() < prob


## Generates atmosphere properties for large moons.
## @param spec: Moon specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param equilibrium_temp_k: Equilibrium temperature.
## @param rng: Random number generator.
## @return: AtmosphereProps or null.
static func _generate_atmosphere(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> AtmosphereProps:
	# Moon atmospheres are typically thin and N2/CH4 dominated (Titan-like)
	
	# Surface pressure
	var surface_pressure_pa: float = spec.get_override_float("atmosphere.surface_pressure_pa", -1.0)
	if surface_pressure_pa < 0.0:
		match size_cat:
			SizeCategory.Category.SUB_TERRESTRIAL:
				# Titan: ~1.5 bar
				surface_pressure_pa = rng.randf_range(1000.0, 200000.0)
			SizeCategory.Category.TERRESTRIAL:
				surface_pressure_pa = rng.randf_range(10000.0, 500000.0)
			SizeCategory.Category.SUPER_EARTH:
				surface_pressure_pa = rng.randf_range(50000.0, 1000000.0)
			_:
				surface_pressure_pa = rng.randf_range(100.0, 10000.0)
	
	# Composition - typically N2/CH4 for cold moons
	var composition: Dictionary = {}
	if equilibrium_temp_k < 200.0:
		# Very cold: Titan-like N2/CH4
		composition["N2"] = rng.randf_range(0.90, 0.98)
		composition["CH4"] = rng.randf_range(0.01, 0.06)
		composition["Ar"] = rng.randf_range(0.001, 0.01)
	else:
		# Warmer: more varied
		var roll: float = rng.randf()
		if roll < 0.5:
			composition["N2"] = rng.randf_range(0.70, 0.90)
			composition["CO2"] = rng.randf_range(0.05, 0.20)
		else:
			composition["CO2"] = rng.randf_range(0.85, 0.95)
			composition["N2"] = rng.randf_range(0.03, 0.10)
	
	# Normalize composition
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for gas in composition.keys():
			composition[gas] = (composition[gas] as float) / total
	
	# Scale height
	var avg_molecular_mass: float = _get_average_molecular_mass(composition)
	var gravity: float = physical.get_surface_gravity_m_s2()
	var scale_height_m: float = 0.0
	if gravity > 0.0 and avg_molecular_mass > 0.0:
		scale_height_m = BOLTZMANN_K * equilibrium_temp_k / (avg_molecular_mass * gravity)
	
	# Greenhouse factor (modest for thin atmospheres)
	var greenhouse_factor: float = 1.0 + rng.randf_range(0.0, 0.2)
	
	return AtmosphereProps.new(
		surface_pressure_pa,
		scale_height_m,
		composition,
		greenhouse_factor
	)


## Gets average molecular mass for a gas mixture.
## @param composition: Gas composition dictionary.
## @return: Average molecular mass in kg.
static func _get_average_molecular_mass(composition: Dictionary) -> float:
	var masses: Dictionary = {
		"H2": 2.016 * 1.6605e-27,
		"He": 4.003 * 1.6605e-27,
		"CH4": 16.04 * 1.6605e-27,
		"NH3": 17.03 * 1.6605e-27,
		"H2O": 18.02 * 1.6605e-27,
		"N2": 28.01 * 1.6605e-27,
		"O2": 32.00 * 1.6605e-27,
		"Ar": 39.95 * 1.6605e-27,
		"CO2": 44.01 * 1.6605e-27,
		"SO2": 64.07 * 1.6605e-27,
	}
	
	var avg_mass: float = 0.0
	var total_fraction: float = 0.0
	
	for gas in composition.keys():
		var fraction: float = composition[gas] as float
		var mass: float = masses.get(gas, 28.0 * 1.6605e-27)
		avg_mass += fraction * mass
		total_fraction += fraction
	
	if total_fraction > 0.0:
		return avg_mass / total_fraction
	
	return 28.0 * 1.6605e-27


## Generates surface properties for the moon.
## @param spec: Moon specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param surface_temp_k: Surface temperature.
## @param tidal_heat_watts: Tidal heating contribution.
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: SurfaceProps.
static func _generate_surface(
	spec: MoonSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	surface_temp_k: float,
	tidal_heat_watts: float,
	_context: ParentContext,
	rng: SeededRng
) -> SurfaceProps:
	# Determine albedo
	var albedo: float = _calculate_albedo(surface_temp_k, rng)
	
	# Determine surface type
	var surface_type: String = _determine_surface_type(surface_temp_k, tidal_heat_watts, rng)
	
	# Volcanism level (can be high for tidally heated moons)
	var volcanism_level: float = _calculate_volcanism(physical, tidal_heat_watts, rng)
	
	# Surface composition
	var surface_composition: Dictionary = _generate_surface_composition(surface_temp_k, rng)
	
	# Create surface props
	var surface: SurfaceProps = SurfaceProps.new(
		surface_temp_k,
		albedo,
		surface_type,
		volcanism_level,
		surface_composition
	)
	
	# Generate terrain
	surface.terrain = _generate_terrain(physical, size_cat, volcanism_level, rng)
	
	# Generate cryosphere for cold moons
	if surface_temp_k < WATER_FREEZE_K:
		surface.cryosphere = _generate_cryosphere(
			spec, surface_temp_k, physical, tidal_heat_watts, rng
		)
	
	return surface


## Calculates surface albedo for moons.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Albedo (0-1).
static func _calculate_albedo(surface_temp_k: float, rng: SeededRng) -> float:
	if surface_temp_k < 150.0:
		# Very cold icy moon: high albedo (Europa: 0.67, Enceladus: 0.99)
		return rng.randf_range(0.4, 0.95)
	elif surface_temp_k < WATER_FREEZE_K:
		# Cold: moderate to high albedo
		return rng.randf_range(0.2, 0.6)
	else:
		# Rocky: low to moderate
		return rng.randf_range(0.05, 0.3)


## Determines the surface type classification.
## @param surface_temp_k: Surface temperature.
## @param tidal_heat_watts: Tidal heating.
## @param rng: Random number generator.
## @return: Surface type string.
static func _determine_surface_type(
	surface_temp_k: float,
	tidal_heat_watts: float,
	rng: SeededRng
) -> String:
	# High tidal heating = volcanic
	if tidal_heat_watts > 1.0e13:
		return "volcanic"
	
	if surface_temp_k < 100.0:
		var roll: float = rng.randf()
		if roll < 0.6:
			return "icy"
		elif roll < 0.8:
			return "icy_cratered"
		else:
			return "icy_smooth"
	elif surface_temp_k < WATER_FREEZE_K:
		if rng.randf() < 0.5:
			return "icy_rocky"
		else:
			return "rocky_cold"
	else:
		return "rocky"


## Calculates volcanism level.
## @param physical: Physical properties.
## @param tidal_heat_watts: Tidal heating contribution.
## @param rng: Random number generator.
## @return: Volcanism level (0-1).
static func _calculate_volcanism(
	physical: PhysicalProps,
	tidal_heat_watts: float,
	rng: SeededRng
) -> float:
	# Tidal heating is primary driver for moon volcanism
	# Io: ~1e14 W, highly volcanic
	
	var total_heat: float = physical.internal_heat_watts + tidal_heat_watts
	var io_heat: float = 1.0e14
	
	var heat_ratio: float = total_heat / io_heat
	var base_volcanism: float = clampf(heat_ratio, 0.0, 1.0)
	
	# Variation
	var variation: float = rng.randf_range(0.7, 1.3)
	
	return clampf(base_volcanism * variation, 0.0, 1.0)


## Generates surface material composition.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_surface_composition(
	surface_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if surface_temp_k < 100.0:
		# Very cold: water ice and exotic ices
		composition["water_ice"] = rng.randf_range(0.5, 0.8)
		composition["silicates"] = rng.randf_range(0.1, 0.3)
		if rng.randf() < 0.5:
			composition["nitrogen_ice"] = rng.randf_range(0.01, 0.1)
		if rng.randf() < 0.3:
			composition["methane_ice"] = rng.randf_range(0.01, 0.05)
	elif surface_temp_k < WATER_FREEZE_K:
		# Cold: water ice and rock
		composition["water_ice"] = rng.randf_range(0.3, 0.6)
		composition["silicates"] = rng.randf_range(0.3, 0.5)
		composition["carbon_compounds"] = rng.randf_range(0.05, 0.15)
	else:
		# Rocky
		composition["silicates"] = rng.randf_range(0.5, 0.7)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.3)
		composition["sulfur_compounds"] = rng.randf_range(0.05, 0.15)
	
	# Normalize
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	if total > 0.0:
		for material in composition.keys():
			composition[material] = (composition[material] as float) / total
	
	return composition


## Generates terrain properties.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param volcanism_level: Volcanism level.
## @param rng: Random number generator.
## @return: TerrainProps.
static func _generate_terrain(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	rng: SeededRng
) -> TerrainProps:
	var gravity: float = physical.get_surface_gravity_m_s2()
	var earth_gravity: float = 9.81
	
	# Elevation range scales inversely with gravity
	var gravity_factor: float = earth_gravity / maxf(gravity, 0.01)
	var base_elevation: float = 10000.0  # Moons have lower elevation range than planets
	var elevation_range_m: float = base_elevation * gravity_factor * rng.randf_range(0.2, 1.0)
	elevation_range_m = clampf(elevation_range_m, 500.0, 50000.0)
	
	# Roughness
	var roughness: float = rng.randf_range(0.3, 0.9)
	
	# Crater density - moons tend to be heavily cratered (no erosion)
	var crater_density: float
	if volcanism_level > 0.5:
		# Active volcanism resurfaces
		crater_density = rng.randf_range(0.0, 0.2)
	elif size_cat == SizeCategory.Category.DWARF:
		# Small moons: heavily cratered
		crater_density = rng.randf_range(0.7, 0.95)
	else:
		crater_density = rng.randf_range(0.4, 0.8)
	
	# Tectonic activity (low for most moons)
	var tectonic_activity: float = volcanism_level * rng.randf_range(0.5, 1.0)
	
	# Erosion (very low without atmosphere)
	var erosion_level: float = rng.randf_range(0.0, 0.1)
	
	# Terrain type
	var terrain_type: String
	if volcanism_level > 0.5:
		terrain_type = "volcanic"
	elif crater_density > 0.6:
		terrain_type = "cratered"
	else:
		terrain_type = "plains"
	
	return TerrainProps.new(
		elevation_range_m,
		roughness,
		crater_density,
		tectonic_activity,
		erosion_level,
		terrain_type
	)


## Generates cryosphere properties for icy moons.
## @param spec: Moon specification.
## @param surface_temp_k: Surface temperature.
## @param physical: Physical properties.
## @param tidal_heat_watts: Tidal heating.
## @param rng: Random number generator.
## @return: CryosphereProps.
static func _generate_cryosphere(
	spec: MoonSpec,
	surface_temp_k: float,
	physical: PhysicalProps,
	tidal_heat_watts: float,
	rng: SeededRng
) -> CryosphereProps:
	# Polar cap coverage (most icy moons are entirely ice-covered)
	var polar_cap_coverage: float
	if surface_temp_k < 150.0:
		polar_cap_coverage = rng.randf_range(0.8, 1.0)
	else:
		polar_cap_coverage = rng.randf_range(0.3, 0.8)
	
	# Permafrost depth
	var permafrost_depth_m: float = rng.randf_range(1000.0, 50000.0)
	
	# Subsurface ocean - key feature for icy moons
	var has_subsurface_ocean: bool = false
	var subsurface_ocean_depth_m: float = 0.0
	
	# Check spec preference first
	if spec.has_ocean_preference():
		has_subsurface_ocean = spec.has_subsurface_ocean as bool
	else:
		# Subsurface ocean requires sufficient internal heat (tidal or radiogenic)
		var total_heat: float = physical.internal_heat_watts + tidal_heat_watts
		# Europa: ~1e12 W tidal + radiogenic
		if total_heat > 5.0e11:
			# High probability with significant heat
			has_subsurface_ocean = rng.randf() < 0.7
		elif total_heat > 1.0e11:
			# Moderate probability
			has_subsurface_ocean = rng.randf() < 0.3
	
	if has_subsurface_ocean:
		# Ocean depth: Europa ~100 km, Enceladus ~10 km
		subsurface_ocean_depth_m = rng.randf_range(5000.0, 150000.0)
	
	# Cryovolcanism (geysers, plumes)
	var cryovolcanism_level: float = 0.0
	if has_subsurface_ocean and tidal_heat_watts > 1.0e11:
		cryovolcanism_level = rng.randf_range(0.1, 0.8)
	
	# Ice type
	var ice_type: String = "water_ice"
	if surface_temp_k < 50.0:
		# Very cold: can have exotic ices
		var roll: float = rng.randf()
		if roll < 0.2:
			ice_type = "nitrogen_ice"
		elif roll < 0.3:
			ice_type = "methane_ice"
	
	return CryosphereProps.new(
		polar_cap_coverage,
		permafrost_depth_m,
		has_subsurface_ocean,
		subsurface_ocean_depth_m,
		cryovolcanism_level,
		ice_type
	)


## Generates an ID for the moon.
## @param spec: The moon specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: MoonSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return GeneratorUtils.generate_id("moon", rng)
