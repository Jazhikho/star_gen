## Generates moon CelestialBody objects from MoonSpec.
## Produces deterministic output based on spec, parent context, and seed.
class_name MoonGenerator
extends RefCounted

const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _size_table: GDScript = preload("res://src/domain/generation/tables/SizeTable.gd")
const _orbit_table: GDScript = preload("res://src/domain/generation/tables/OrbitTable.gd")
const _generator_utils: GDScript = preload("res://src/domain/generation/generators/GeneratorUtils.gd")
const _atmosphere_utils: GDScript = preload("res://src/domain/generation/utils/AtmosphereUtils.gd")
const _moon_physical_generator: GDScript = preload("res://src/domain/generation/generators/moon/MoonPhysicalGenerator.gd")
const _moon_atmosphere_generator: GDScript = preload("res://src/domain/generation/generators/moon/MoonAtmosphereGenerator.gd")
const _moon_surface_generator: GDScript = preload("res://src/domain/generation/generators/moon/MoonSurfaceGenerator.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _atmosphere_props: GDScript = preload("res://src/domain/celestial/components/AtmosphereProps.gd")
const _surface_props: GDScript = preload("res://src/domain/celestial/components/SurfaceProps.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _population_generator: GDScript = preload("res://src/domain/population/PopulationGenerator.gd")
const _population_probability: GDScript = preload("res://src/domain/population/PopulationProbability.gd")
const _population_seeding: GDScript = preload("res://src/domain/population/PopulationSeeding.gd")


## Size category distribution weights for regular moons.
const SIZE_CATEGORY_WEIGHTS_REGULAR: Array[float] = [
	30.0, # DWARF - common for moons
	50.0, # SUB_TERRESTRIAL - most common (Luna, Europa, Titan)
	15.0, # TERRESTRIAL - rare but possible
	5.0, # SUPER_EARTH - very rare
]

## Size category distribution weights for captured moons.
const SIZE_CATEGORY_WEIGHTS_CAPTURED: Array[float] = [
	80.0, # DWARF - most captured moons are small
	18.0, # SUB_TERRESTRIAL - uncommon
	2.0, # TERRESTRIAL - very rare
	0.0, # SUPER_EARTH - essentially impossible
]

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
## @param enable_population: If true, generate population data based on probability.
## @param parent_body: Optional parent body for population context.
## @return: A new CelestialBody configured as a moon.
static func generate(spec: MoonSpec, context: ParentContext, rng: SeededRng, enable_population: bool = false, parent_body: CelestialBody = null) -> CelestialBody:
	# Validate context has parent body (planet)
	if not context.has_parent_body():
		push_error("MoonGenerator requires a ParentContext with parent body data")
		return null
	
	# Determine size category
	var size_cat: SizeCategory.Category = _determine_size_category(spec, rng)
	
	# Generate orbital properties first (needed for tidal effects)
	var orbital: OrbitalProps = _generate_orbital_props(spec, context, size_cat, rng)
	
	# Generate physical properties
	var physical: PhysicalProps = _moon_physical_generator.generate_physical_props(spec, context, size_cat, orbital, rng)
	
	# Calculate equilibrium temperature from star
	var equilibrium_temp_k: float = context.get_equilibrium_temperature_k(0.3)
	
	# Calculate tidal heating contribution
	var tidal_heat_watts: float = _moon_physical_generator.calculate_tidal_heating(
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
	var should_have_atmosphere: bool = _moon_atmosphere_generator.should_have_atmosphere(
		spec, physical, size_cat, context, rng
	)
	
	# Generate atmosphere if applicable
	var atmosphere: AtmosphereProps = null
	if should_have_atmosphere:
		atmosphere = _moon_atmosphere_generator.generate_atmosphere(spec, physical, size_cat, equilibrium_temp_k, rng)
	
	# Calculate surface temperature
	var surface_temp_k: float = _atmosphere_utils.calculate_surface_temperature(equilibrium_temp_k, atmosphere)
	
	# Generate surface properties (moons always have surfaces, even icy ones)
	var surface: SurfaceProps = _moon_surface_generator.generate_surface(
		spec, physical, size_cat, surface_temp_k, tidal_heat_watts, context, rng
	)
	
	# Generate ID
	var body_id: String = _generate_id(spec, rng)
	
	# Create provenance
	var provenance: Provenance = _generator_utils.create_provenance(spec, context)
	
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
	
	# Generate population data if enabled
	if enable_population:
		body.population_data = _generate_population(body, context, spec.generation_seed, parent_body)
	
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
			eccentricity = raw * raw * 0.1 # Max ~0.1 for regular
	
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
		"" # Parent ID set later by system generator
	)


## Generates an ID for the moon.
## @param spec: The moon specification.
## @param rng: The random number generator.
## @return: The body ID.
static func _generate_id(spec: MoonSpec, rng: SeededRng) -> String:
	var override_id: Variant = spec.get_override("id", null)
	if override_id != null and override_id is String and not (override_id as String).is_empty():
		return override_id as String
	return _generator_utils.generate_id("moon", rng)


## Generates population data for a moon using order-independent seeding.
## Moons pass parent body for context (tidal heating, radiation).
## @param body: The generated moon body.
## @param context: The parent context (star + planet data).
## @param base_seed: The generation base seed.
## @param parent_body: The parent planet body (for population context).
## @return: PlanetPopulationData, or null if no population generated.
static func _generate_population(
	body: CelestialBody,
	context: ParentContext,
	base_seed: int,
	parent_body: CelestialBody = null
) -> PlanetPopulationData:
	# Generate order-independent seed for this moon's population
	var pop_seed: int = _population_seeding.generate_population_seed(body.id, base_seed)
	var pop_rng: SeededRng = SeededRng.new(pop_seed)
	
	# Generate profile and suitability (passes parent body for moon-specific factors)
	var pop_data: PlanetPopulationData = _population_generator.generate_profile_only(
		body, context, parent_body
	)
	pop_data.generation_seed = pop_seed
	
	# Check if natives should be generated
	var generate_natives: bool = _population_probability.should_generate_natives(pop_data.profile, pop_rng)
	
	# Check if colony should be generated
	var generate_colony: bool = false
	if pop_data.suitability != null:
		generate_colony = _population_probability.should_generate_colony(
			pop_data.profile, pop_data.suitability, pop_rng
		)
	
	# If either should be generated, run full generation
	if generate_natives or generate_colony:
		var spec: _population_generator.PopulationSpec = _population_generator.PopulationSpec.create_default(pop_seed)
		spec.generate_natives = generate_natives
		spec.generate_colonies = generate_colony
		pop_data = _population_generator.generate(body, context, spec, parent_body)
		pop_data.generation_seed = pop_seed
	
	return pop_data
