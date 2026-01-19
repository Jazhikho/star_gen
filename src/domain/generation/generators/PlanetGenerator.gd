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
const _ring_system_generator := preload("res://src/domain/generation/generators/RingSystemGenerator.gd")
const _ring_system_props := preload("res://src/domain/celestial/components/RingSystemProps.gd")


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

## Earth's atmospheric pressure in Pascals.
const EARTH_ATMOSPHERE_PA: float = 101325.0

## Boltzmann constant in J/K.
const BOLTZMANN_K: float = 1.380649e-23

## Hydrogen molecular mass in kg.
const HYDROGEN_MASS_KG: float = 1.6735575e-27

## Water freezing point in Kelvin.
const WATER_FREEZE_K: float = 273.15

## Water boiling point at 1 atm in Kelvin.
const WATER_BOIL_K: float = 373.15


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
	
	# Calculate equilibrium temperature (needed for atmosphere and surface)
	var equilibrium_temp_k: float = context.get_equilibrium_temperature_k(0.3)
	
	# Determine if planet can/should have atmosphere
	var should_have_atmosphere: bool = _should_have_atmosphere(
		spec, physical, size_cat, context, rng
	)
	
	# Generate atmosphere if applicable
	var atmosphere: AtmosphereProps = null
	if should_have_atmosphere:
		atmosphere = _generate_atmosphere(spec, physical, size_cat, zone, equilibrium_temp_k, rng)
	
	# Calculate actual surface temperature (with greenhouse effect)
	var surface_temp_k: float = equilibrium_temp_k
	if atmosphere != null:
		surface_temp_k = equilibrium_temp_k * atmosphere.greenhouse_factor
	
	# Generate surface properties (null for gas giants)
	var surface: SurfaceProps = null
	if SizeCategory.is_rocky(size_cat):
		surface = _generate_surface(spec, physical, size_cat, zone, surface_temp_k, context, rng)
	
	# Generate ID
	var body_id: String = _generate_id(spec, rng)
	
	# Store context in spec snapshot for save/load
	var spec_dict: Dictionary = spec.to_dict()
	spec_dict["context"] = context.to_dict()
	
	# Create provenance
	var provenance: Provenance = Provenance.new(
		spec.generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		spec_dict
	)
	
	# Generate ring system if applicable (gas giants and large planets)
	var ring_system: RingSystemProps = null
	if _should_generate_rings(spec, size_cat):
		if RingSystemGenerator.should_have_rings(physical, context, rng):
			ring_system = RingSystemGenerator.generate(null, physical, context, rng)
	
	# Assemble the celestial body
	var body: CelestialBody = CelestialBody.new(
		body_id,
		spec.name_hint,
		CelestialType.Type.PLANET,
		physical,
		provenance
	)
	body.orbital = orbital
	body.atmosphere = atmosphere
	body.surface = surface
	body.ring_system = ring_system
	
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
	
	var selected: Variant = rng.weighted_choice(categories, SIZE_CATEGORY_WEIGHTS)
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
	
	var selected: Variant = rng.weighted_choice(zones, ORBIT_ZONE_WEIGHTS)
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


# =============================================================================
# ATMOSPHERE GENERATION (Stage 4)
# =============================================================================


## Determines whether this planet should have an atmosphere.
## @param spec: The planet specification.
## @param physical: The physical properties.
## @param size_cat: The size category.
## @param context: The parent context.
## @param rng: Random number generator.
## @return: True if atmosphere should be generated.
static func _should_have_atmosphere(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext,
	rng: SeededRng
) -> bool:
	# Check spec preference first
	if spec.has_atmosphere_preference():
		return spec.has_atmosphere as bool
	
	# Gas giants always have atmospheres
	if SizeCategory.is_gaseous(size_cat):
		return true
	
	# Check atmospheric retention capability
	var can_retain: bool = _can_retain_atmosphere(physical, size_cat, context)
	
	if not can_retain:
		return false
	
	# Rocky planets: probability based on size and zone
	var retention_probability: float = _get_atmosphere_probability(size_cat)
	return rng.randf() < retention_probability


## Checks if a planet can retain an atmosphere against thermal escape and stellar wind.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param context: Parent context.
## @return: True if atmosphere can be retained.
static func _can_retain_atmosphere(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	context: ParentContext
) -> bool:
	var escape_velocity: float = physical.get_escape_velocity_m_s()
	
	# Calculate thermal velocity for hydrogen (lightest gas)
	# v_thermal = sqrt(3kT/m)
	var equilibrium_temp: float = context.get_equilibrium_temperature_k(0.3)
	var thermal_velocity: float = sqrt(3.0 * BOLTZMANN_K * equilibrium_temp / HYDROGEN_MASS_KG)
	
	# Jeans escape parameter: λ = v_escape / v_thermal
	# Need λ > 6 to retain gas over geological time
	# For heavier gases (N2, CO2), the threshold is lower
	var jeans_param: float = escape_velocity / thermal_velocity
	
	# Dwarf planets need very high jeans parameter due to small size
	if size_cat == SizeCategory.Category.DWARF:
		return jeans_param > 10.0  # Very hard for dwarfs
	
	# Rocky planets need jeans > 6 for heavy gases
	if SizeCategory.is_rocky(size_cat):
		return jeans_param > 4.0  # Can retain CO2, N2
	
	# Gaseous planets always retain
	return true


## Gets the probability of having an atmosphere based on size category.
## @param size_cat: The size category.
## @return: Probability (0-1).
static func _get_atmosphere_probability(size_cat: SizeCategory.Category) -> float:
	match size_cat:
		SizeCategory.Category.DWARF:
			return 0.1  # Very rare (like Pluto's tenuous atmosphere)
		SizeCategory.Category.SUB_TERRESTRIAL:
			return 0.4  # Moderate (Mars has thin atmosphere)
		SizeCategory.Category.TERRESTRIAL:
			return 0.8  # Common (Earth, Venus)
		SizeCategory.Category.SUPER_EARTH:
			return 0.95  # Almost certain
		_:
			return 1.0  # Gas giants always


## Determines if ring generation should be attempted for this planet.
## @param spec: The planet specification.
## @param size_cat: The size category.
## @return: True if rings should be considered.
static func _should_generate_rings(spec: PlanetSpec, size_cat: SizeCategory.Category) -> bool:
	# Check for explicit override
	var override_rings: Variant = spec.get_override("has_rings", null)
	if override_rings != null:
		return override_rings as bool
	
	# Only gas giants and ice giants typically have significant rings
	return SizeCategory.is_gaseous(size_cat)


## Generates atmosphere properties.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Equilibrium temperature.
## @param rng: Random number generator.
## @return: AtmosphereProps or null.
static func _generate_atmosphere(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> AtmosphereProps:
	# Generate surface pressure
	var surface_pressure_pa: float = _calculate_surface_pressure(
		spec, physical, size_cat, rng
	)
	
	# Generate composition based on size and zone
	var composition: Dictionary = _generate_atmosphere_composition(
		size_cat, zone, equilibrium_temp_k, rng
	)
	
	# Calculate scale height: H = kT/(mg)
	# Use average molecular mass based on composition
	var avg_molecular_mass: float = _get_average_molecular_mass(composition)
	var gravity: float = physical.get_surface_gravity_m_s2()
	var scale_height_m: float = 0.0
	if gravity > 0.0 and avg_molecular_mass > 0.0:
		scale_height_m = BOLTZMANN_K * equilibrium_temp_k / (avg_molecular_mass * gravity)
	
	# Calculate greenhouse factor
	var greenhouse_factor: float = _calculate_greenhouse_factor(
		composition, surface_pressure_pa, rng
	)
	
	return AtmosphereProps.new(
		surface_pressure_pa,
		scale_height_m,
		composition,
		greenhouse_factor
	)


## Calculates surface pressure based on planet properties.
## @param spec: Planet specification.
## @param _physical: Physical properties (unused, reserved for future use).
## @param size_cat: Size category.
## @param rng: Random number generator.
## @return: Surface pressure in Pascals.
static func _calculate_surface_pressure(
	spec: PlanetSpec,
	_physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> float:
	var override_pressure: float = spec.get_override_float("atmosphere.surface_pressure_pa", -1.0)
	if override_pressure >= 0.0:
		return override_pressure
	
	match size_cat:
		SizeCategory.Category.DWARF:
			# Trace atmosphere (Pluto: ~1 Pa)
			return rng.randf_range(0.1, 100.0)
		
		SizeCategory.Category.SUB_TERRESTRIAL:
			# Thin atmosphere (Mars: ~600 Pa)
			return rng.randf_range(100.0, 10000.0)
		
		SizeCategory.Category.TERRESTRIAL:
			# Earth-like range (Earth: 101325 Pa, Venus: 9.2e6 Pa)
			var log_pressure: float = rng.randf_range(3.0, 7.0)  # 1000 Pa to 10 MPa
			return pow(10.0, log_pressure)
		
		SizeCategory.Category.SUPER_EARTH:
			# Likely thick atmosphere
			var log_pressure: float = rng.randf_range(4.0, 8.0)
			return pow(10.0, log_pressure)
		
		SizeCategory.Category.MINI_NEPTUNE, SizeCategory.Category.NEPTUNE_CLASS, SizeCategory.Category.GAS_GIANT:
			# Defined at cloud tops, very high deep pressure
			# Use "1 bar" level for gas giants
			return rng.randf_range(0.5e5, 2.0e5)
		
		_:
			return EARTH_ATMOSPHERE_PA


## Generates atmospheric composition based on planet type and conditions.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Temperature in Kelvin.
## @param rng: Random number generator.
## @return: Dictionary of gas name -> fraction.
static func _generate_atmosphere_composition(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if SizeCategory.is_gaseous(size_cat):
		# Gas/ice giant composition
		composition = _generate_gas_giant_composition(size_cat, rng)
	else:
		# Rocky planet composition
		composition = _generate_rocky_atmosphere_composition(zone, equilibrium_temp_k, rng)
	
	# Normalize to sum to 1.0
	var total: float = 0.0
	for fraction in composition.values():
		total += fraction as float
	
	if total > 0.0:
		for gas in composition.keys():
			composition[gas] = (composition[gas] as float) / total
	
	return composition


## Generates gas giant atmosphere composition.
## @param size_cat: Size category.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_gas_giant_composition(
	size_cat: SizeCategory.Category,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if size_cat == SizeCategory.Category.GAS_GIANT:
		# Jupiter/Saturn-like: dominated by H2 and He
		composition["H2"] = rng.randf_range(0.82, 0.92)
		composition["He"] = rng.randf_range(0.06, 0.12)
		composition["CH4"] = rng.randf_range(0.001, 0.005)
		composition["NH3"] = rng.randf_range(0.0001, 0.001)
	else:
		# Neptune-class/Mini-Neptune: more heavy elements
		composition["H2"] = rng.randf_range(0.70, 0.85)
		composition["He"] = rng.randf_range(0.10, 0.20)
		composition["CH4"] = rng.randf_range(0.01, 0.05)
		composition["H2O"] = rng.randf_range(0.001, 0.01)
	
	return composition


## Generates rocky planet atmosphere composition.
## @param zone: Orbit zone.
## @param equilibrium_temp_k: Temperature in Kelvin.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_rocky_atmosphere_composition(
	zone: OrbitZone.Zone,
	equilibrium_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	# Base composition type based on zone and temperature
	var roll: float = rng.randf()
	
	if zone == OrbitZone.Zone.HOT or equilibrium_temp_k > 500.0:
		# Hot planets: CO2 dominated or stripped
		composition["CO2"] = rng.randf_range(0.80, 0.98)
		composition["N2"] = rng.randf_range(0.01, 0.15)
		composition["SO2"] = rng.randf_range(0.001, 0.05)
	elif zone == OrbitZone.Zone.TEMPERATE:
		if roll < 0.3:
			# Earth-like: N2 dominated with O2
			composition["N2"] = rng.randf_range(0.70, 0.80)
			composition["O2"] = rng.randf_range(0.15, 0.25)
			composition["Ar"] = rng.randf_range(0.005, 0.02)
			composition["CO2"] = rng.randf_range(0.0001, 0.001)
			composition["H2O"] = rng.randf_range(0.001, 0.04)
		elif roll < 0.7:
			# Venus-like: CO2 dominated
			composition["CO2"] = rng.randf_range(0.90, 0.98)
			composition["N2"] = rng.randf_range(0.02, 0.08)
			composition["SO2"] = rng.randf_range(0.0001, 0.001)
		else:
			# Mars-like: thin CO2
			composition["CO2"] = rng.randf_range(0.90, 0.97)
			composition["N2"] = rng.randf_range(0.02, 0.05)
			composition["Ar"] = rng.randf_range(0.01, 0.03)
	else:
		# Cold zone: N2/CH4 possible (Titan-like)
		if roll < 0.4:
			composition["N2"] = rng.randf_range(0.90, 0.98)
			composition["CH4"] = rng.randf_range(0.01, 0.06)
		else:
			composition["CO2"] = rng.randf_range(0.85, 0.95)
			composition["N2"] = rng.randf_range(0.03, 0.10)
	
	return composition


## Gets average molecular mass for a gas mixture.
## @param composition: Gas composition dictionary.
## @return: Average molecular mass in kg.
static func _get_average_molecular_mass(composition: Dictionary) -> float:
	# Molecular masses in kg
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
		var mass: float = masses.get(gas, 28.0 * 1.6605e-27)  # Default to N2 mass
		avg_mass += fraction * mass
		total_fraction += fraction
	
	if total_fraction > 0.0:
		return avg_mass / total_fraction
	
	return 28.0 * 1.6605e-27  # Default N2


## Calculates greenhouse warming factor.
## @param composition: Atmospheric composition.
## @param surface_pressure_pa: Surface pressure.
## @param rng: Random number generator.
## @return: Greenhouse factor (>=1.0).
static func _calculate_greenhouse_factor(
	composition: Dictionary,
	surface_pressure_pa: float,
	rng: SeededRng
) -> float:
	# Greenhouse gases and their relative effectiveness
	var co2_fraction: float = composition.get("CO2", 0.0) as float
	var ch4_fraction: float = composition.get("CH4", 0.0) as float
	var h2o_fraction: float = composition.get("H2O", 0.0) as float
	
	# Pressure factor (more atmosphere = more greenhouse effect)
	# log10(x) = log(x) / log(10)
	var pressure_ratio: float = maxf(surface_pressure_pa / EARTH_ATMOSPHERE_PA, 0.001)
	var pressure_factor: float = log(pressure_ratio) / log(10.0)
	pressure_factor = clampf(pressure_factor, -2.0, 3.0)
	
	# Base greenhouse contribution
	# Venus: ~500K surface with ~230K equilibrium = factor of ~2.2
	# Earth: ~288K surface with ~255K equilibrium = factor of ~1.13
	var greenhouse: float = 1.0
	
	# CO2 contribution (logarithmic with concentration and pressure)
	if co2_fraction > 0.0:
		# log10(x) = log(x) / log(10)
		var co2_effect: float = 0.1 * (log(co2_fraction * 1e6 + 1) / log(10.0)) * (1.0 + pressure_factor * 0.3)
		greenhouse += co2_effect
	
	# CH4 contribution (more potent per molecule but usually trace)
	if ch4_fraction > 0.0:
		greenhouse += ch4_fraction * 25.0  # CH4 is ~25x more potent than CO2
	
	# H2O contribution
	if h2o_fraction > 0.0:
		greenhouse += h2o_fraction * 2.0
	
	# Add some variation
	var variation: float = rng.randf_range(0.9, 1.1)
	greenhouse *= variation
	
	# Clamp to reasonable range
	return clampf(greenhouse, 1.0, 3.0)


# =============================================================================
# SURFACE GENERATION (Stage 4)
# =============================================================================


## Generates surface properties for rocky planets.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature (with greenhouse).
## @param context: Parent context.
## @param rng: Random number generator.
## @return: SurfaceProps or null.
static func _generate_surface(
	spec: PlanetSpec,
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	context: ParentContext,
	rng: SeededRng
) -> SurfaceProps:
	# Determine albedo
	var albedo: float = _calculate_albedo(spec, zone, surface_temp_k, rng)
	
	# Determine surface type
	var surface_type: String = _determine_surface_type(size_cat, zone, surface_temp_k, rng)
	
	# Determine volcanism level
	var volcanism_level: float = _calculate_volcanism(
		spec, physical, context.stellar_age_years, rng
	)
	
	# Generate surface composition
	var surface_composition: Dictionary = _generate_surface_composition(
		size_cat, zone, surface_temp_k, rng
	)
	
	# Create surface props
	var surface: SurfaceProps = SurfaceProps.new(
		surface_temp_k,
		albedo,
		surface_type,
		volcanism_level,
		surface_composition
	)
	
	# Generate terrain
	surface.terrain = _generate_terrain(physical, size_cat, volcanism_level, context, rng)
	
	# Generate hydrosphere if conditions allow
	if _can_have_liquid_water(surface_temp_k, physical):
		surface.hydrosphere = _generate_hydrosphere(
			surface_temp_k, size_cat, volcanism_level, rng
		)
	
	# Generate cryosphere if cold enough
	if surface_temp_k < WATER_FREEZE_K or zone == OrbitZone.Zone.COLD:
		surface.cryosphere = _generate_cryosphere(
			surface_temp_k, size_cat, physical, context, rng
		)
	
	return surface


## Calculates surface albedo.
## @param spec: Planet specification.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Albedo (0-1).
static func _calculate_albedo(
	spec: PlanetSpec,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> float:
	var override_albedo: float = spec.get_override_float("surface.albedo", -1.0)
	if override_albedo >= 0.0:
		return clampf(override_albedo, 0.0, 1.0)
	
	# Base albedo depends on surface type
	var base_albedo: float
	
	if surface_temp_k < WATER_FREEZE_K:
		# Ice-covered: high albedo
		base_albedo = rng.randf_range(0.5, 0.8)
	elif zone == OrbitZone.Zone.HOT:
		# Hot rocky: low albedo
		base_albedo = rng.randf_range(0.05, 0.2)
	else:
		# Temperate: varies widely
		base_albedo = rng.randf_range(0.1, 0.5)
	
	return base_albedo


## Determines the surface type classification.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Surface type string.
static func _determine_surface_type(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> String:
	# Determine primary surface characteristic
	if surface_temp_k > 700.0:
		return "molten"
	elif surface_temp_k > 500.0:
		return "volcanic"
	elif surface_temp_k < 100.0:
		return "frozen"
	elif surface_temp_k < WATER_FREEZE_K:
		if rng.randf() < 0.5:
			return "icy"
		else:
			return "rocky_cold"
	elif zone == OrbitZone.Zone.TEMPERATE:
		var roll: float = rng.randf()
		if roll < 0.2:
			return "oceanic"
		elif roll < 0.5:
			return "continental"
		elif roll < 0.7:
			return "desert"
		else:
			return "rocky"
	else:
		if size_cat == SizeCategory.Category.DWARF:
			return "cratered"
		return "rocky"


## Calculates volcanism level.
## @param spec: Planet specification.
## @param physical: Physical properties.
## @param age_years: System age.
## @param rng: Random number generator.
## @return: Volcanism level (0-1).
static func _calculate_volcanism(
	spec: PlanetSpec,
	physical: PhysicalProps,
	age_years: float,
	rng: SeededRng
) -> float:
	var override_volcanism: float = spec.get_override_float("surface.volcanism_level", -1.0)
	if override_volcanism >= 0.0:
		return clampf(override_volcanism, 0.0, 1.0)
	
	# Volcanism depends on internal heat
	var earth_heat: float = 4.7e13
	var heat_ratio: float = physical.internal_heat_watts / earth_heat
	
	# Scale with internal heat and add variation
	var base_volcanism: float = clampf(heat_ratio * 0.5, 0.0, 1.0)
	
	# Age factor: older planets are less active
	if age_years > 0.0:
		var age_factor: float = pow(0.5, age_years / 5.0e9)
		base_volcanism *= age_factor
	
	# Random variation
	var variation: float = rng.randf_range(0.5, 1.5)
	
	return clampf(base_volcanism * variation, 0.0, 1.0)


## Generates surface material composition.
## @param size_cat: Size category.
## @param zone: Orbit zone.
## @param surface_temp_k: Surface temperature.
## @param rng: Random number generator.
## @return: Composition dictionary.
static func _generate_surface_composition(
	size_cat: SizeCategory.Category,
	zone: OrbitZone.Zone,
	surface_temp_k: float,
	rng: SeededRng
) -> Dictionary:
	var composition: Dictionary = {}
	
	if surface_temp_k < WATER_FREEZE_K:
		# Cold: ice and rock
		composition["water_ice"] = rng.randf_range(0.2, 0.6)
		composition["silicates"] = rng.randf_range(0.3, 0.6)
		composition["carbon_compounds"] = rng.randf_range(0.05, 0.2)
	elif zone == OrbitZone.Zone.HOT:
		# Hot: silicates and metals
		composition["silicates"] = rng.randf_range(0.5, 0.7)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.3)
		composition["sulfur_compounds"] = rng.randf_range(0.05, 0.15)
	else:
		# Temperate: varied
		composition["silicates"] = rng.randf_range(0.4, 0.6)
		composition["iron_oxides"] = rng.randf_range(0.1, 0.2)
		composition["carbonates"] = rng.randf_range(0.05, 0.15)
		if size_cat >= SizeCategory.Category.TERRESTRIAL:
			composition["water"] = rng.randf_range(0.01, 0.1)
	
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
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: TerrainProps.
static func _generate_terrain(
	physical: PhysicalProps,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	_context: ParentContext,
	rng: SeededRng
) -> TerrainProps:
	var gravity: float = physical.get_surface_gravity_m_s2()
	var earth_gravity: float = 9.81
	
	# Elevation range scales inversely with gravity
	# Mars has taller mountains than Earth due to lower gravity
	var gravity_factor: float = earth_gravity / maxf(gravity, 0.1)
	var base_elevation: float = 20000.0  # Earth's max is ~20km from trench to peak
	var elevation_range_m: float = base_elevation * gravity_factor * rng.randf_range(0.3, 1.5)
	elevation_range_m = clampf(elevation_range_m, 1000.0, 100000.0)
	
	# Roughness
	var roughness: float = rng.randf_range(0.2, 0.8)
	
	# Crater density: higher for smaller, less active bodies
	var crater_density: float
	if size_cat == SizeCategory.Category.DWARF:
		crater_density = rng.randf_range(0.6, 0.95)
	elif volcanism_level > 0.3:
		# Active volcanism resurfaces, reducing craters
		crater_density = rng.randf_range(0.0, 0.3)
	else:
		crater_density = rng.randf_range(0.2, 0.7)
	
	# Tectonic activity correlates with volcanism and internal heat
	var tectonic_activity: float = clampf(volcanism_level * rng.randf_range(0.8, 1.2), 0.0, 1.0)
	
	# Erosion depends on atmosphere and water
	var erosion_level: float = rng.randf_range(0.1, 0.5)
	
	# Terrain type
	var terrain_type: String
	if volcanism_level > 0.5:
		terrain_type = "volcanic"
	elif crater_density > 0.6:
		terrain_type = "cratered"
	elif tectonic_activity > 0.4:
		terrain_type = "tectonic"
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


## Checks if liquid water can exist on the surface.
## @param surface_temp_k: Surface temperature.
## @param physical: Physical properties.
## @return: True if liquid water possible.
static func _can_have_liquid_water(surface_temp_k: float, physical: PhysicalProps) -> bool:
	# Need temperature in liquid water range
	# Pressure affects boiling point, but simplified here
	if surface_temp_k < WATER_FREEZE_K:
		return false
	if surface_temp_k > WATER_BOIL_K * 1.5:  # Allow for high pressure
		return false
	
	# Need sufficient gravity to retain water vapor
	var escape_v: float = physical.get_escape_velocity_m_s()
	return escape_v > 3000.0  # Rough threshold


## Generates hydrosphere properties.
## @param surface_temp_k: Surface temperature.
## @param size_cat: Size category.
## @param volcanism_level: Volcanism level.
## @param rng: Random number generator.
## @return: HydrosphereProps.
static func _generate_hydrosphere(
	surface_temp_k: float,
	size_cat: SizeCategory.Category,
	volcanism_level: float,
	rng: SeededRng
) -> HydrosphereProps:
	# Ocean coverage
	var ocean_coverage: float
	if size_cat >= SizeCategory.Category.TERRESTRIAL:
		ocean_coverage = rng.randf_range(0.1, 0.95)
	else:
		ocean_coverage = rng.randf_range(0.0, 0.3)
	
	# Ocean depth scales with coverage and planet size
	var ocean_depth_m: float = ocean_coverage * rng.randf_range(1000.0, 10000.0)
	
	# Ice coverage based on temperature
	var ice_coverage: float = 0.0
	if surface_temp_k < WATER_FREEZE_K + 20.0:
		# Near freezing: partial ice
		ice_coverage = rng.randf_range(0.3, 0.8)
	elif surface_temp_k < WATER_FREEZE_K + 50.0:
		# Cool: polar ice
		ice_coverage = rng.randf_range(0.05, 0.3)
	
	# Salinity
	var salinity_ppt: float = rng.randf_range(10.0, 50.0)
	
	# Water type based on volcanism
	var water_type: String = "water"
	if volcanism_level > 0.5 and rng.randf() < 0.3:
		water_type = "acidic_water"
	
	return HydrosphereProps.new(
		ocean_coverage,
		ocean_depth_m,
		ice_coverage,
		salinity_ppt,
		water_type
	)


## Generates cryosphere properties.
## @param surface_temp_k: Surface temperature.
## @param _size_cat: Size category (reserved for future use).
## @param physical: Physical properties.
## @param _context: Parent context (reserved for future use).
## @param rng: Random number generator.
## @return: CryosphereProps.
static func _generate_cryosphere(
	surface_temp_k: float,
	_size_cat: SizeCategory.Category,
	physical: PhysicalProps,
	_context: ParentContext,
	rng: SeededRng
) -> CryosphereProps:
	# Polar cap coverage based on temperature
	var polar_cap_coverage: float = 0.0
	if surface_temp_k < WATER_FREEZE_K:
		polar_cap_coverage = rng.randf_range(0.3, 1.0)
	elif surface_temp_k < WATER_FREEZE_K + 30.0:
		polar_cap_coverage = rng.randf_range(0.05, 0.4)
	else:
		polar_cap_coverage = rng.randf_range(0.0, 0.1)
	
	# Permafrost depth
	var permafrost_depth_m: float = 0.0
	if surface_temp_k < WATER_FREEZE_K:
		permafrost_depth_m = rng.randf_range(100.0, 5000.0)
	elif polar_cap_coverage > 0.1:
		permafrost_depth_m = rng.randf_range(10.0, 500.0)
	
	# Subsurface ocean (more likely in cold moons with tidal heating)
	var has_subsurface_ocean: bool = false
	var subsurface_ocean_depth_m: float = 0.0
	
	# Check for subsurface ocean conditions
	if surface_temp_k < WATER_FREEZE_K and physical.internal_heat_watts > 1.0e12:
		if rng.randf() < 0.4:
			has_subsurface_ocean = true
			subsurface_ocean_depth_m = rng.randf_range(10000.0, 100000.0)
	
	# Cryovolcanism
	var cryovolcanism_level: float = 0.0
	if has_subsurface_ocean and physical.internal_heat_watts > 5.0e12:
		cryovolcanism_level = rng.randf_range(0.1, 0.6)
	
	# Ice type based on distance from star
	var ice_type: String = "water_ice"
	if surface_temp_k < 100.0:
		# Very cold: exotic ices
		var roll: float = rng.randf()
		if roll < 0.3:
			ice_type = "nitrogen_ice"
		elif roll < 0.5:
			ice_type = "methane_ice"
		elif roll < 0.7:
			ice_type = "co2_ice"
	
	return CryosphereProps.new(
		polar_cap_coverage,
		permafrost_depth_m,
		has_subsurface_ocean,
		subsurface_ocean_depth_m,
		cryovolcanism_level,
		ice_type
	)
