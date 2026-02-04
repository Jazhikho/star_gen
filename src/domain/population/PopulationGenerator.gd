## Main entry point for generating all population data for a planet.
## Orchestrates profile, suitability, native, and colony generation.
## All generation is deterministic when given a seed.
class_name PopulationGenerator
extends RefCounted

# Preload dependencies.
const _planet_population_data: GDScript = preload("res://src/domain/population/PlanetPopulationData.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _profile_generator: GDScript = preload("res://src/domain/population/ProfileGenerator.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _native_population_generator: GDScript = preload("res://src/domain/population/NativePopulationGenerator.gd")
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _colony_generator: GDScript = preload("res://src/domain/population/ColonyGenerator.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")


## Specification for full population generation.
class PopulationSpec:
	## Seed for deterministic generation.
	var seed: int = 0

	## Current year (0 = present, negative = past, positive = future).
	var current_year: int = 0

	## Whether to generate native populations.
	var generate_natives: bool = true

	## Native generation settings.
	var native_spec: NativePopulationGenerator.NativePopulationSpec = null

	## Whether to generate colonies.
	var generate_colonies: bool = true

	## Colony generation settings (array for multiple colonies).
	var colony_specs: Array = [] # Array of ColonyGenerator.ColonySpec

	## Maximum number of colonies to generate if no specs provided.
	var max_auto_colonies: int = 2

	## Chance of generating a colony if suitable (0-1).
	var colony_chance: float = 0.3


	## Creates default spec with reasonable settings.
	static func create_default(p_seed: int = 0) -> PopulationSpec:
		var spec: PopulationSpec = PopulationSpec.new()
		spec.seed = p_seed
		spec.current_year = 0
		spec.generate_natives = true
		spec.generate_colonies = true

		# Default native spec
		spec.native_spec = NativePopulationGenerator.NativePopulationSpec.new()
		spec.native_spec.seed_value = p_seed
		spec.native_spec.max_populations = 3
		spec.native_spec.force_population = false
		spec.native_spec.current_year = 0
		spec.native_spec.min_history_years = 1000
		spec.native_spec.max_history_years = 50000

		return spec


## Generates complete population data for a celestial body.
## @param body: The celestial body (planet or moon).
## @param context: Parent context (star data for planets, planet data for moons).
## @param spec: Generation specification.
## @param parent_body: Optional parent body for moons.
## @return: Complete PlanetPopulationData.
static func generate(
	body: CelestialBody,
	context: ParentContext,
	spec: PopulationSpec,
	parent_body: CelestialBody = null
) -> PlanetPopulationData:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.body_id = body.id
	data.generation_seed = spec.seed

	# Create RNG from seed
	var rng: SeededRng = SeededRng.new(spec.seed)

	# Generate profile
	data.profile = ProfileGenerator.generate(body, context, parent_body)

	# Generate suitability
	data.suitability = SuitabilityCalculator.calculate(data.profile)

	# Generate native populations
	if spec.generate_natives:
		data.native_populations = _generate_natives(data.profile, spec, rng)

	# Generate colonies
	if spec.generate_colonies:
		data.colonies = _generate_colonies(
			data.profile,
			data.suitability,
			data.native_populations,
			spec,
			rng
		)

	return data


## Generates complete population data from an existing profile.
## Use this when you already have a profile and don't need to regenerate it.
## @param profile: The planet profile.
## @param spec: Generation specification.
## @return: Complete PlanetPopulationData.
static func generate_from_profile(
	profile: PlanetProfile,
	spec: PopulationSpec
) -> PlanetPopulationData:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.body_id = profile.body_id
	data.generation_seed = spec.seed
	data.profile = profile

	# Create RNG from seed
	var rng: SeededRng = SeededRng.new(spec.seed)

	# Generate suitability
	data.suitability = SuitabilityCalculator.calculate(profile)

	# Generate native populations
	if spec.generate_natives:
		data.native_populations = _generate_natives(profile, spec, rng)

	# Generate colonies
	if spec.generate_colonies:
		data.colonies = _generate_colonies(
			profile,
			data.suitability,
			data.native_populations,
			spec,
			rng
		)

	return data


## Generates native populations.
static func _generate_natives(
	profile: PlanetProfile,
	spec: PopulationSpec,
	rng: SeededRng
) -> Array[NativePopulation]:
	if spec.native_spec == null:
		return []

	# Update native spec with current year from main spec
	spec.native_spec.current_year = spec.current_year

	# Fork RNG for native generation
	var native_rng: SeededRng = rng.fork()

	return NativePopulationGenerator.generate(profile, spec.native_spec, native_rng)


## Generates colonies.
static func _generate_colonies(
	profile: PlanetProfile,
	suitability: ColonySuitability,
	existing_natives: Array[NativePopulation],
	spec: PopulationSpec,
	rng: SeededRng
) -> Array[Colony]:
	var colonies: Array[Colony] = []

	# Check if colonization is possible
	if not suitability.is_colonizable():
		return colonies

	# Fork RNG for colony generation
	var colony_rng: SeededRng = rng.fork()

	# If specific colony specs provided, use them
	if not spec.colony_specs.is_empty():
		for colony_spec in spec.colony_specs:
			var cs: ColonyGenerator.ColonySpec = colony_spec as ColonyGenerator.ColonySpec
			cs.current_year = spec.current_year

			var colony: Colony = ColonyGenerator.generate(
				profile, suitability, existing_natives, cs, colony_rng.fork()
			)
			if colony != null:
				colonies.append(colony)
		return colonies

	# Otherwise, auto-generate based on chance and suitability
	var num_colonies: int = 0

	# Higher suitability = higher chance of colonization
	var adjusted_chance: float = spec.colony_chance * (suitability.overall_score / 50.0)
	adjusted_chance = clampf(adjusted_chance, 0.0, 0.9)

	# Roll for first colony
	if colony_rng.randf() < adjusted_chance:
		num_colonies = 1

		# Chance for additional colonies (diminishing)
		var additional_chance: float = adjusted_chance * 0.3
		while num_colonies < spec.max_auto_colonies and colony_rng.randf() < additional_chance:
			num_colonies += 1
			additional_chance *= 0.3

	# Generate each colony
	for i in range(num_colonies):
		var colony_spec: ColonyGenerator.ColonySpec = ColonyGenerator.ColonySpec.new()
		colony_spec.seed = colony_rng.randi()
		colony_spec.current_year = spec.current_year
		colony_spec.min_history_years = 50
		colony_spec.max_history_years = 500
		colony_spec.founding_civilization_id = "civ_auto_%d" % i
		colony_spec.founding_civilization_name = "Unknown Civilization"

		var colony: Colony = ColonyGenerator.generate(
			profile, suitability, existing_natives, colony_spec, colony_rng.fork()
		)
		if colony != null:
			colonies.append(colony)

	return colonies


## Generates only the profile and suitability (no populations).
## Useful for quick assessments without full generation.
## @param body: The celestial body.
## @param context: Parent context.
## @param parent_body: Optional parent body for moons.
## @return: PlanetPopulationData with only profile and suitability.
static func generate_profile_only(
	body: CelestialBody,
	context: ParentContext,
	parent_body: CelestialBody = null
) -> PlanetPopulationData:
	var data: PlanetPopulationData = PlanetPopulationData.new()
	data.body_id = body.id

	data.profile = ProfileGenerator.generate(body, context, parent_body)
	data.suitability = SuitabilityCalculator.calculate(data.profile)

	return data
