## Generates physical properties for moons.
## Handles mass, radius, rotation, oblateness, magnetic field, internal heat, and tidal heating.
class_name MoonPhysicalGenerator
extends RefCounted

const _moon_spec := preload("res://src/domain/generation/specs/MoonSpec.gd")
const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _size_table := preload("res://src/domain/generation/tables/SizeTable.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Generates physical properties for a moon.
## @param spec: The moon specification.
## @param context: The parent context.
## @param size_cat: The size category.
## @param orbital: The orbital properties (for tidal locking).
## @param rng: The random number generator.
## @return: PhysicalProps for the moon.
static func generate_physical_props(
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


## Calculates tidal heating from parent planet.
## @param physical: Physical properties.
## @param orbital: Orbital properties.
## @param context: Parent context.
## @return: Tidal heating in watts.
static func calculate_tidal_heating(
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
