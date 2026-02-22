## Generates planet CelestialBody objects from PlanetSpec.
## Produces deterministic output based on spec, parent context, and seed.
class_name PlanetGenerator
extends RefCounted

const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _size_table: GDScript = preload("res://src/domain/generation/tables/SizeTable.gd")
const _orbit_table: GDScript = preload("res://src/domain/generation/tables/OrbitTable.gd")
const _generator_utils: GDScript = preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _atmosphere_utils: GDScript = preload("res://src/domain/generation/utils/AtmosphereUtils.gd")
const _planet_physical_generator: GDScript = preload("res://src/domain/generation/generators/planet/PlanetPhysicalGenerator.gd")
const _planet_atmosphere_generator: GDScript = preload("res://src/domain/generation/generators/planet/PlanetAtmosphereGenerator.gd")
const _planet_surface_generator: GDScript = preload("res://src/domain/generation/generators/planet/PlanetSurfaceGenerator.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _surface_props: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _ring_system_generator: GDScript = preload("res://src/domain/generation/generators/RingSystemGenerator.gd")
const _ring_system_props: GDScript = preload("res://src/domain/celestial/components/RingSystemProps.gd")
const _population_generator: GDScript = preload("res://src/domain/population/PopulationGenerator.gd")
const _population_likelihood: GDScript = preload("res://src/domain/population/PopulationLikelihood.gd")
const _population_seeding: GDScript = preload("res://src/domain/population/PopulationSeeding.gd")


## Size category distribution weights for random selection.
const SIZE_CATEGORY_WEIGHTS: Array[float] = [
	5.0, # DWARF - uncommon
	10.0, # SUB_TERRESTRIAL - moderate
	15.0, # TERRESTRIAL - common
	20.0, # SUPER_EARTH - very common (most discovered exoplanets)
	20.0, # MINI_NEPTUNE - very common
	15.0, # NEPTUNE_CLASS - common
	15.0, # GAS_GIANT - common
]

## Orbit zone distribution weights for random selection.
const ORBIT_ZONE_WEIGHTS: Array[float] = [
	20.0, # HOT - moderate (detection bias)
	30.0, # TEMPERATE - common
	50.0, # COLD - most common (more orbital space)
]


## Generates a planet from a specification and parent context.
## @param spec: The planet specification.
## @param context: The parent star/system context.
## @param rng: The random number generator (will be advanced).
## @param enable_population: If true, generate population data.
## @param population_override: Override for population mode (AUTO=likelihood vs seed, NONE=no population, FORCE_NATIVES, FORCE_COLONY).
## @return: A new CelestialBody configured as a planet.
static func generate(
	spec: PlanetSpec,
	context: ParentContext,
	rng: SeededRng,
	enable_population: bool = false,
	population_override: int = 0
) -> CelestialBody:
	# Determine archetypes
	var size_cat: SizeCategory.Category = _determine_size_category(spec, rng)
	var zone: OrbitZone.Zone = _determine_orbit_zone(spec, rng)
	
	# Generate orbital properties first (needed for tidal locking)
	var orbital: OrbitalProps = _generate_orbital_props(spec, context, zone, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _planet_physical_generator.generate_physical_props(spec, context, size_cat, orbital, rng)
	
	# Calculate equilibrium temperature (needed for atmosphere and surface)
	var equilibrium_temp_k: float = context.get_equilibrium_temperature_k(0.3)
	
	# Determine if planet can/should have atmosphere
	var should_have_atmosphere: bool = _planet_atmosphere_generator.should_have_atmosphere(
		spec, physical, size_cat, context, rng
	)
	
	# Generate atmosphere if applicable
	var atmosphere: AtmosphereProps = null
	if should_have_atmosphere:
		atmosphere = _planet_atmosphere_generator.generate_atmosphere(spec, physical, size_cat, zone, equilibrium_temp_k, rng)
	
	# Calculate actual surface temperature (with greenhouse effect)
	var surface_temp_k: float = _atmosphere_utils.calculate_surface_temperature(equilibrium_temp_k, atmosphere)
	
	# Generate surface properties (null for gas giants)
	var surface: SurfaceProps = null
	if SizeCategory.is_rocky(size_cat):
		surface = _planet_surface_generator.generate_surface(spec, physical, size_cat, zone, surface_temp_k, context, rng)
	
	# Generate ID
	var body_id: String = _generate_id(spec, rng)
	
	# Create provenance
	var provenance: Provenance = _generator_utils.create_provenance(spec, context)
	
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
	
	# Generate population data if enabled
	if enable_population:
		body.population_data = _generate_population(body, context, spec.generation_seed, population_override)
	
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
		"" # Parent ID set later by system generator
	)


## Generates an ID for the planet.
## @param spec: The planet specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: PlanetSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return _generator_utils.generate_id("planet", rng)


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


## Generates population data for a planet using order-independent seeding.
## Uses PopulationLikelihood (likelihood vs derived seed) when override is AUTO,
## or forces natives/colony when override is FORCE_NATIVES or FORCE_COLONY.
## @param body: The generated celestial body.
## @param context: The parent context (star data).
## @param base_seed: The generation base seed.
## @param population_override: PopulationLikelihood.Override (AUTO, NONE, FORCE_NATIVES, FORCE_COLONY).
## @return: PlanetPopulationData, or null if no population generated.
static func _generate_population(
	body: CelestialBody,
	context: ParentContext,
	base_seed: int,
	population_override: int = 0
) -> PlanetPopulationData:
	if population_override == _population_likelihood.Override.NONE:
		return null

	# Generate order-independent seed for this body's population
	var pop_seed: int = _population_seeding.generate_population_seed(body.id, base_seed)

	# Always generate profile and suitability (lightweight, useful for display)
	var pop_data: PlanetPopulationData = _population_generator.generate_profile_only(body, context)
	pop_data.generation_seed = pop_seed

	var generate_natives: bool = false
	var generate_colony: bool = false

	if population_override == _population_likelihood.Override.FORCE_NATIVES:
		generate_natives = true
	elif population_override == _population_likelihood.Override.FORCE_COLONY:
		generate_colony = pop_data.suitability != null and pop_data.suitability.is_colonizable()
	else:
		# AUTO: use likelihood vs derived deterministic seed
		generate_natives = _population_likelihood.should_generate_natives(pop_data.profile, pop_seed)
		if pop_data.suitability != null:
			generate_colony = _population_likelihood.should_generate_colony(
				pop_data.profile, pop_data.suitability, pop_seed
			)

	if not generate_natives and not generate_colony:
		return pop_data

	var spec: _population_generator.PopulationSpec = _population_generator.PopulationSpec.create_default(pop_seed)
	spec.generate_natives = generate_natives
	spec.generate_colonies = generate_colony
	pop_data = _population_generator.generate(body, context, spec)
	pop_data.generation_seed = pop_seed

	return pop_data
