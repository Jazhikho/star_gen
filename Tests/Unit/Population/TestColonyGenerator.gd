## Tests for ColonyGenerator.
extends TestCase

const _colony_generator: GDScript = preload("res://src/domain/population/ColonyGenerator.gd")
const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _colony_suitability: GDScript = preload("res://src/domain/population/ColonySuitability.gd")
const _suitability_calculator: GDScript = preload("res://src/domain/population/SuitabilityCalculator.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")


## Creates a habitable profile.
func _create_habitable_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "habitable_001"
	profile.habitability_score = 8
	profile.avg_temperature_k = 290.0
	profile.pressure_atm = 1.0
	profile.has_liquid_water = true
	profile.has_breathable_atmosphere = true
	profile.ocean_coverage = 0.6
	profile.land_coverage = 0.35
	profile.gravity_g = 1.0
	profile.radiation_level = 0.1
	profile.weather_severity = 0.3

	profile.biomes = {
		BiomeType.Type.OCEAN as int: 0.6,
		BiomeType.Type.FOREST as int: 0.2,
		BiomeType.Type.GRASSLAND as int: 0.15,
	}

	profile.resources = {
		ResourceType.Type.WATER as int: 0.9,
		ResourceType.Type.METALS as int: 0.5,
		ResourceType.Type.SILICATES as int: 0.7,
	}

	return profile


## Creates an unsuitable profile.
func _create_unsuitable_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "unsuitable_001"
	profile.habitability_score = 0
	profile.avg_temperature_k = 800.0
	profile.pressure_atm = 90.0
	return profile


## Creates a test native population.
func _create_test_native() -> NativePopulation:
	var native: NativePopulation = NativePopulation.new()
	native.id = "native_001"
	native.name = "Testani"
	native.body_id = "habitable_001"
	native.population = 1000000
	native.is_extant = true
	native.territorial_control = 0.4
	native.tech_level = TechnologyLevel.Level.MEDIEVAL
	return native


## Creates a default spec.
func _create_default_spec() -> ColonyGenerator.ColonySpec:
	var spec: ColonyGenerator.ColonySpec = ColonyGenerator.ColonySpec.new()
	spec.seed = 12345
	spec.current_year = 0
	spec.min_history_years = 50
	spec.max_history_years = 300
	spec.founding_tech_level = TechnologyLevel.Level.INTERSTELLAR
	spec.founding_civilization_id = "civ_001"
	spec.founding_civilization_name = "Test Civilization"
	return spec


## Tests generation on suitable world produces colony.
func test_generate_suitable_world() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_not_null(colony, "Should generate colony on suitable world")
	assert_not_equal(colony.id, "")
	assert_not_equal(colony.name, "")
	assert_equal(colony.body_id, profile.body_id)


## Tests generation on unsuitable world returns null.
func test_generate_unsuitable_world() -> void:
	var profile: PlanetProfile = _create_unsuitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_null(colony, "Should not generate colony on unsuitable world")


## Tests generated colony has valid data.
func test_generated_colony_validity() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_less_than(colony.founding_year, spec.current_year, "Founding should be in past")
	assert_greater_than(colony.population, 0, "Should have population")
	assert_not_null(colony.government, "Should have government")
	assert_not_null(colony.history, "Should have history")
	assert_greater_than(colony.history.size(), 0, "Should have history events")


## Tests spec.colony_type is respected.
func test_spec_colony_type_respected() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.colony_type = ColonyType.Type.MILITARY
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.colony_type, ColonyType.Type.MILITARY)


## Tests spec.name is respected.
func test_spec_name_respected() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.name = "Custom Colony Name"
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.name, "Custom Colony Name")


## Tests spec.founding_year is respected.
func test_spec_founding_year_respected() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.founding_year = -100
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.founding_year, -100)


## Tests native relations are established.
func test_native_relations_established() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var native: NativePopulation = _create_test_native()
	var natives: Array[NativePopulation] = [native]
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.establish_native_relations = true
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_true(colony.has_native_relations(), "Should have native relations")
	assert_not_null(colony.get_native_relation("native_001"))


## Tests native relations not established when disabled.
func test_native_relations_disabled() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var native: NativePopulation = _create_test_native()
	var natives: Array[NativePopulation] = [native]
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.establish_native_relations = false
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_false(colony.has_native_relations())


## Tests determinism - same seed produces same results.
func test_determinism() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()

	var rng1: SeededRng = SeededRng.new(42)
	var colony1: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng1)

	var rng2: SeededRng = SeededRng.new(42)
	var colony2: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng2)

	assert_equal(colony1.name, colony2.name, "Same seed should produce same name")
	assert_equal(colony1.colony_type, colony2.colony_type, "Same seed should produce same type")
	assert_equal(colony1.founding_year, colony2.founding_year, "Same seed should produce same founding")
	assert_equal(colony1.population, colony2.population, "Same seed should produce same population")


## Tests government matches colony type.
func test_government_matches_type() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.colony_type = ColonyType.Type.CORPORATE
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.government.regime, GovernmentType.Regime.CORPORATE)


## Tests territorial control is reasonable.
func test_territorial_control_reasonable() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var native: NativePopulation = _create_test_native()
	native.territorial_control = 0.5
	var natives: Array[NativePopulation] = [native]
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	# Colony shouldn't take over everything when natives present
	assert_in_range(colony.territorial_control, 0.01, 0.6)


## Tests self-sufficiency calculated.
func test_self_sufficiency_calculated() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_in_range(colony.self_sufficiency, 0.1, 1.0)


## Tests history has founding event.
func test_history_has_founding() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	var founding = colony.history.get_founding_event()
	assert_not_null(founding)
	assert_equal(founding.year, colony.founding_year)


## Tests tech level inherited from spec.
func test_tech_level_inherited() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)
	var natives: Array[NativePopulation] = []
	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	spec.founding_tech_level = TechnologyLevel.Level.ADVANCED
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.tech_level, TechnologyLevel.Level.ADVANCED)


## Tests multiple natives get relations.
func test_multiple_native_relations() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var suitability: ColonySuitability = SuitabilityCalculator.calculate(profile)

	var native1: NativePopulation = _create_test_native()
	native1.id = "native_001"
	var native2: NativePopulation = _create_test_native()
	native2.id = "native_002"
	var natives: Array[NativePopulation] = [native1, native2]

	var spec: ColonyGenerator.ColonySpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var colony: Colony = ColonyGenerator.generate(profile, suitability, natives, spec, rng)

	assert_equal(colony.native_relations.size(), 2)
