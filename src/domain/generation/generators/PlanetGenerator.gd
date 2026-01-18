## Generates planet CelestialBody objects from PlanetSpec.
## Produces deterministic output based on spec, parent context, and seed.
class_name PlanetGenerator
extends RefCounted

const _planet_spec := preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _size_table := preload("res://src/domain/generation/tables/SizeTable.gd")
const _orbit_table := preload("res://src/domain/generation/tables/OrbitTable.gd")
const _generator_utils := preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props := preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _provenance := preload("res://src/domain/celestial/Provenance.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Size category distribution weights for random selection.
const SIZE_CATEGORY_WEIGHTS: Array[float] = [
	5.0,   # DWARF - uncommon
	10.0,  # SUB_TERRESTRIAL - moderate
	15.0,  # TERRESTRIAL - common
	20.0,  # SUPER_EARTH - very common (most discovered exoplanets)
	20.0,  # MINI_NEPTUNE - very common
	15.0,  # NEPTUNE_CLASS - common
	15.0,  # GAS_GIANT - common
]

## Orbit zone distribution weights for random selection.
const ORBIT_ZONE_WEIGHTS: Array[float] = [
	20.0,  # HOT - moderate (detection bias)
	30.0,  # TEMPERATE - common
	50.0,  # COLD - most common (more orbital space)
]


## Generates a planet from a specification and parent context.
## @param spec: The planet specification.
## @param context: The parent star/system context.
## @param rng: The random number generator (will be advanced).
## @return: A new CelestialBody configured as a planet.
static func generate(spec: PlanetSpec, context: ParentContext, rng: SeededRng) -> CelestialBody:
	# Determine archetypes
	var size_cat: SizeCategory.Category = _determine_size_category(spec, rng)
	var zone: OrbitZone.Zone = _determine_orbit_zone(spec, rng)
	
	# Generate orbital properties first (needed for tidal locking)
	var orbital: OrbitalProps = _generate_orbital_props(spec, context, zone, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _generate_physical_props(spec, context, size_cat, orbital, rng)
	
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
		CelestialType.Type.PLANET,
		physical,
		provenance
	)
	body.orbital = orbital
	
	return body


## Determines the size category from spec or random selection.
## @param spec: The planet specification.
## @param rng: The random number generator.
## @return: The selected size category.
static func _determine_size_category(spec: PlanetSpec, rng: SeededRng) -> SizeCategory.Category:
	if spec.has_size_category():
		return spec.size_category as SizeCategory.Category
	
	var categories: Array = [
		SizeCategory.Category.DWARF,
		SizeCategory.Category.SUB_TERRESTRIAL,
		SizeCategory.Category.TERRESTRIAL,
		SizeCategory.Category.SUPER_EARTH,
		SizeCategory.Category.MINI_NEPTUNE,
		SizeCategory.Category.NEPTUNE_CLASS,
		SizeCategory.Category.GAS_GIANT,
	]
	
	var selected: Variant = GeneratorUtils.weighted_choice(categories, SIZE_CATEGORY_WEIGHTS, rng)
	return selected as SizeCategory.Category


## Determines the orbit zone from spec or random selection.
## @param spec: The planet specification.
## @param rng: The random number generator.
## @return: The selected orbit zone.
static func _determine_orbit_zone(spec: PlanetSpec, rng: SeededRng) -> OrbitZone.Zone:
	if spec.has_orbit_zone():
		return spec.orbit_zone as OrbitZone.Zone
	
	var zones: Array = [
		OrbitZone.Zone.HOT,
		OrbitZone.Zone.TEMPERATE,
		OrbitZone.Zone.COLD,
	]
	
	var selected: Variant = GeneratorUtils.weighted_choice(zones, ORBIT_ZONE_WEIGHTS, rng)
	return selected as OrbitZone.Zone


## Generates orbital properties for the planet.
## @param spec: The planet specification.
## @param context: The parent context.
## @param zone: The orbit zone.
## @param rng: The random number generator.
## @return: OrbitalProps for the planet.
static func _generate_orbital_props(
	spec: PlanetSpec,
	context: ParentContext,
	zone: OrbitZone.Zone,
	rng: SeededRng
) -> OrbitalProps:
	# Semi-major axis
	var semi_major_axis_m: float = spec.get_override_float(
		"orbital.semi_major_axis_m",
		-1.0
	)
	if semi_major_axis_m < 0.0:
		semi_major_axis_m = OrbitTable.random_distance(zone, context.stellar_luminosity_watts, rng)
	
	# Eccentricity
	var eccentricity: float = spec.get_override_float(
		"orbital.eccentricity",
		-1.0
	)
	if eccentricity < 0.0:
		eccentricity = OrbitTable.random_eccentricity(zone, rng)
	
	# Inclination
	var inclination_deg: float = spec.get_override_float(
		"orbital.inclination_deg",
		-1.0
	)
	if inclination_deg < 0.0:
		inclination_deg = OrbitTable.random_inclination(rng)
	
	# Other orbital elements (random 0-360)
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


## Generates physical properties for the planet.
## @param spec: The planet specification.
## @param context: The parent context.
## @param size_cat: The size category.
## @param orbital: The orbital properties (for tidal locking).
## @param rng: The random number generator.
## @return: PhysicalProps for the planet.
static func _generate_physical_props(
	spec: PlanetSpec,
	context: ParentContext,
	size_cat: SizeCategory.Category,
	orbital: OrbitalProps,
	rng: SeededRng
) -> PhysicalProps:
	# Generate density first (determines composition character)
	var density_kg_m3: float = spec.get_override_float(
		"physical.density_kg_m3",
		-1.0
	)
	if density_kg_m3 < 0.0:
		density_kg_m3 = SizeTable.random_density(size_cat, rng)
	
	# Generate mass
	var mass_kg: float = spec.get_override_float("physical.mass_kg", -1.0)
	var mass_earth: float = -1.0
	
	if mass_kg < 0.0:
		mass_earth = spec.get_override_float("physical.mass_earth", -1.0)
		if mass_earth < 0.0:
			mass_earth = SizeTable.random_mass_earth(size_cat, rng)
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
	
	# Determine if tidally locked
	var is_locked: bool = _is_tidally_locked(
		orbital.semi_major_axis_m,
		mass_kg,
		radius_m,
		context.stellar_mass_kg,
		context.stellar_age_years
	)
	
	# Rotation period
	var rotation_period_s: float = spec.get_override_float("physical.rotation_period_s", -1.0)
	if rotation_period_s < 0.0:
		rotation_period_s = _calculate_rotation_period(
			mass_kg,
			radius_m,
			orbital,
			context.stellar_mass_kg,
			is_locked,
			rng
		)
	
	# Axial tilt
	var axial_tilt_deg: float = spec.get_override_float("physical.axial_tilt_deg", -1.0)
	if axial_tilt_deg < 0.0:
		axial_tilt_deg = _calculate_axial_tilt(is_locked, rng)
	
	# Oblateness (from rotation)
	var oblateness: float = spec.get_override_float("physical.oblateness", -1.0)
	if oblateness < 0.0:
		oblateness = _calculate_oblateness(mass_kg, radius_m, rotation_period_s, size_cat)
	
	# Magnetic field
	var magnetic_moment: float = spec.get_override_float("physical.magnetic_moment", -1.0)
	if magnetic_moment < 0.0:
		magnetic_moment = _calculate_magnetic_moment(
			mass_kg,
			radius_m,
			rotation_period_s,
			size_cat,
			rng
		)
	
	# Internal heat
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


## Checks if the planet would be tidally locked.
## @param orbital_distance_m: Distance from star.
## @param mass_kg: Planet mass.
## @param radius_m: Planet radius.
## @param stellar_mass_kg: Star mass.
## @param system_age_years: Age of the system.
## @return: True if tidally locked.
static func _is_tidally_locked(
	orbital_distance_m: float,
	mass_kg: float,
	radius_m: float,
	stellar_mass_kg: float,
	system_age_years: float
) -> bool:
	return OrbitTable.is_tidally_locked(
		orbital_distance_m,
		mass_kg,
		radius_m,
		stellar_mass_kg,
		system_age_years
	)


## Calculates the rotation period.
## @param mass_kg: Planet mass.
## @param _radius_m: Planet radius (reserved for future use).
## @param orbital: Orbital properties.
## @param stellar_mass_kg: Star mass.
## @param is_locked: Whether tidally locked.
## @param rng: Random number generator.
## @return: Rotation period in seconds.
static func _calculate_rotation_period(
	mass_kg: float,
	_radius_m: float,
	orbital: OrbitalProps,
	stellar_mass_kg: float,
	is_locked: bool,
	rng: SeededRng
) -> float:
	if is_locked:
		# Rotation period equals orbital period
		return orbital.get_orbital_period_s(stellar_mass_kg)
	
	# Non-locked planets: generate based on mass
	# Larger planets tend to rotate faster (conserved angular momentum)
	# Earth: ~24 hours, Jupiter: ~10 hours
	var mass_earth: float = mass_kg / Units.EARTH_MASS_KG
	
	# Base period scales inversely with sqrt of mass (rough approximation)
	# More massive planets have more angular momentum from formation
	var base_hours: float
	if mass_earth < 1.0:
		# Small planets: 15-50 hours
		base_hours = rng.randf_range(15.0, 50.0)
	elif mass_earth < 10.0:
		# Terrestrial to super-Earth: 10-30 hours
		base_hours = rng.randf_range(10.0, 30.0)
	elif mass_earth < 100.0:
		# Mini-Neptune to Neptune: 8-20 hours
		base_hours = rng.randf_range(8.0, 20.0)
	else:
		# Gas giants: 8-15 hours
		base_hours = rng.randf_range(8.0, 15.0)
	
	# Add some variation
	var variation: float = rng.randf_range(0.8, 1.2)
	var period_hours: float = base_hours * variation
	
	# Rarely, retrograde rotation (negative period)
	if rng.randf() < 0.05:
		period_hours = -period_hours
	
	return period_hours * 3600.0


## Calculates axial tilt.
## @param is_locked: Whether tidally locked.
## @param rng: Random number generator.
## @return: Axial tilt in degrees.
static func _calculate_axial_tilt(is_locked: bool, rng: SeededRng) -> float:
	if is_locked:
		# Tidally locked planets tend toward low obliquity
		return rng.randf_range(0.0, 10.0)
	
	# Distribution based on solar system observations
	# Most planets have moderate tilt, with occasional extreme cases
	var roll: float = rng.randf()
	
	if roll < 0.6:
		# 60% chance: low tilt (0-30 degrees)
		return rng.randf_range(0.0, 30.0)
	elif roll < 0.9:
		# 30% chance: moderate tilt (30-60 degrees)
		return rng.randf_range(30.0, 60.0)
	elif roll < 0.98:
		# 8% chance: high tilt (60-90 degrees)
		return rng.randf_range(60.0, 90.0)
	else:
		# 2% chance: extreme/retrograde (90-180 degrees, like Uranus/Venus)
		return rng.randf_range(90.0, 180.0)


## Calculates oblateness from rotation.
## @param mass_kg: Planet mass.
## @param radius_m: Planet radius.
## @param rotation_period_s: Rotation period.
## @param size_cat: Size category.
## @return: Oblateness factor.
static func _calculate_oblateness(
	mass_kg: float,
	radius_m: float,
	rotation_period_s: float,
	size_cat: SizeCategory.Category
) -> float:
	if rotation_period_s == 0.0 or radius_m <= 0.0:
		return 0.0
	
	# Oblateness depends on rotation rate and body rigidity
	# f ≈ (5/4) * (ω²R³)/(GM) for fluid body
	# Reduced for rocky bodies due to rigidity
	
	var omega: float = 2.0 * PI / absf(rotation_period_s)
	var g: float = PhysicalProps.G
	
	var fluid_oblateness: float = (5.0 / 4.0) * omega * omega * pow(radius_m, 3.0) / (g * mass_kg)
	
	# Rigidity factor: rocky planets resist deformation more
	var rigidity_factor: float
	if SizeCategory.is_rocky(size_cat):
		rigidity_factor = 0.3  # Rocky bodies are more rigid
	else:
		rigidity_factor = 0.8  # Gas/ice giants are more fluid
	
	var oblateness: float = fluid_oblateness * rigidity_factor
	
	# Clamp to reasonable range (Jupiter is ~0.065)
	return clampf(oblateness, 0.0, 0.15)


## Calculates magnetic dipole moment.
## @param mass_kg: Planet mass.
## @param radius_m: Planet radius.
## @param rotation_period_s: Rotation period.
## @param size_cat: Size category.
## @param rng: Random number generator.
## @return: Magnetic moment in T·m³.
static func _calculate_magnetic_moment(
	mass_kg: float,
	radius_m: float,
	rotation_period_s: float,
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> float:
	# Magnetic field requires:
	# 1. Conducting fluid core (iron for rocky, metallic hydrogen for giants)
	# 2. Rotation (convection needs Coriolis force)
	# 3. Sufficient heat flux (drives convection)
	
	# Earth's magnetic moment: ~8e22 A·m² ≈ 8e22 T·m³
	var earth_moment: float = 8.0e22
	
	# No field if too slow rotation (tidally locked small bodies)
	var rotation_hours: float = absf(rotation_period_s) / 3600.0
	if rotation_hours > 100.0 and SizeCategory.is_rocky(size_cat):
		return 0.0
	
	var mass_earth: float = mass_kg / Units.EARTH_MASS_KG
	var radius_earth: float = radius_m / Units.EARTH_RADIUS_METERS
	
	# Scaling: magnetic moment roughly scales with core size and rotation rate
	# M ∝ ρ_core * r_core³ * ω
	# Simplified: M ∝ M^0.5 * R * (24/P)
	
	var base_moment: float
	if SizeCategory.is_gaseous(size_cat):
		# Gas giants have strong fields from metallic hydrogen
		# Jupiter: ~1.5e27 T·m³
		base_moment = earth_moment * pow(mass_earth, 0.8) * (24.0 / maxf(rotation_hours, 1.0))
	else:
		# Rocky planets: need differentiated iron core
		# Smaller planets may lack dynamo
		if mass_earth < 0.1:
			# Very small: no dynamo
			return 0.0
		
		base_moment = earth_moment * pow(mass_earth, 0.5) * radius_earth * (24.0 / maxf(rotation_hours, 1.0))
	
	# Random variation (factor of 2-3 based on core composition uncertainty)
	var variation: float = rng.randf_range(0.3, 3.0)
	
	# Some planets may have weak or no field (Mars, Venus)
	if rng.randf() < 0.15:
		return 0.0  # Dead dynamo
	
	return base_moment * variation


## Calculates internal heat generation.
## @param mass_kg: Planet mass.
## @param age_years: System age.
## @param rng: Random number generator.
## @return: Internal heat in watts.
static func _calculate_internal_heat(
	mass_kg: float,
	age_years: float,
	rng: SeededRng
) -> float:
	# Internal heat sources:
	# 1. Primordial heat (from formation)
	# 2. Radioactive decay (K-40, U-238, Th-232)
	# 3. Gravitational contraction (gas giants)
	
	# Earth's internal heat: ~4.7e13 W
	var earth_heat: float = 4.7e13
	var mass_earth: float = mass_kg / Units.EARTH_MASS_KG
	
	# Heat scales with mass (more radioisotopes)
	# Decays over time (half-lives of billions of years)
	var age_factor: float = 1.0
	if age_years > 0.0:
		# Rough decay: heat halves every ~2 billion years
		age_factor = pow(0.5, age_years / 2.0e9)
		age_factor = maxf(age_factor, 0.1)  # Minimum 10% remains
	
	var base_heat: float = earth_heat * pow(mass_earth, 0.9) * age_factor
	
	# Variation based on composition uncertainty
	var variation: float = rng.randf_range(0.5, 2.0)
	
	return base_heat * variation


## Generates an ID for the planet.
## @param spec: The planet specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: PlanetSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return GeneratorUtils.generate_id("planet", rng)
