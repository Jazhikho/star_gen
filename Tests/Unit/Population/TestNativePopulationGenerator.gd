## Tests for NativePopulationGenerator.
extends TestCase

const _native_population_generator: GDScript = preload("res://src/domain/population/NativePopulationGenerator.gd")
const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _planet_profile: GDScript = preload("res://src/domain/population/PlanetProfile.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _resource_type: GDScript = preload("res://src/domain/population/ResourceType.gd")
const _biome_type: GDScript = preload("res://src/domain/population/BiomeType.gd")


## Creates a habitable Earth-like profile.
func _create_habitable_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "habitable_001"
	profile.habitability_score = 9
	profile.avg_temperature_k = 288.0
	profile.pressure_atm = 1.0
	profile.has_liquid_water = true
	profile.ocean_coverage = 0.7
	profile.land_coverage = 0.25
	profile.continent_count = 5
	profile.gravity_g = 1.0
	profile.volcanism_level = 0.2
	profile.tectonic_activity = 0.4

	profile.biomes = {
		BiomeType.Type.OCEAN as int: 0.7,
		BiomeType.Type.FOREST as int: 0.12,
		BiomeType.Type.GRASSLAND as int: 0.08,
		BiomeType.Type.DESERT as int: 0.05,
		BiomeType.Type.TUNDRA as int: 0.05,
	}

	profile.resources = {
		ResourceType.Type.WATER as int: 0.9,
		ResourceType.Type.METALS as int: 0.5,
		ResourceType.Type.SILICATES as int: 0.8,
		ResourceType.Type.ORGANICS as int: 0.6,
		ResourceType.Type.RARE_ELEMENTS as int: 0.3,
	}

	return profile


## Creates an uninhabitable profile.
func _create_uninhabitable_profile() -> PlanetProfile:
	var profile: PlanetProfile = PlanetProfile.new()
	profile.body_id = "uninhabitable_001"
	profile.habitability_score = 1
	profile.avg_temperature_k = 150.0
	profile.pressure_atm = 0.0
	profile.has_liquid_water = false
	profile.ocean_coverage = 0.0

	profile.biomes = {BiomeType.Type.BARREN as int: 1.0}

	return profile


## Creates a default spec.
func _create_default_spec() -> NativePopulationGenerator.NativePopulationSpec:
	var spec: NativePopulationGenerator.NativePopulationSpec = NativePopulationGenerator.NativePopulationSpec.new()
	spec.max_populations = 3
	spec.force_population = false
	spec.current_year = 0
	spec.min_history_years = 1000
	spec.max_history_years = 50000
	return spec


## Tests generation on habitable world produces populations.
func test_generate_habitable_world() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	assert_greater_than(populations.size(), 0, "Habitable world with force should have populations")


## Tests generation on uninhabitable world produces no populations.
func test_generate_uninhabitable_world() -> void:
	var profile: PlanetProfile = _create_uninhabitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	assert_equal(populations.size(), 0, "Uninhabitable world should have no populations")


## Tests generated populations have valid data.
func test_generated_population_validity() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		assert_not_equal(pop.id, "", "Should have ID")
		assert_not_equal(pop.name, "", "Should have name")
		assert_equal(pop.body_id, profile.body_id, "Should link to planet")
		assert_less_than(pop.origin_year, spec.current_year, "Origin should be in past")
		assert_greater_than(pop.population, 0, "Should have population")
		assert_not_null(pop.government, "Should have government")
		assert_not_null(pop.history, "Should have history")


## Tests generated populations have history with founding event.
func test_generated_population_has_founding() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		var founding: HistoryEvent = pop.history.get_founding_event()
		assert_not_null(founding, "Should have founding event")
		assert_equal(founding.year, pop.origin_year, "Founding year should match origin")


## Tests determinism - same seed produces same results.
func test_determinism() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true

	var rng1: SeededRng = SeededRng.new(42)
	var pop1: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng1)

	var rng2: SeededRng = SeededRng.new(42)
	var pop2: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng2)

	assert_equal(pop1.size(), pop2.size(), "Same seed should produce same count")

	for i in range(pop1.size()):
		assert_equal(pop1[i].name, pop2[i].name, "Same seed should produce same names")
		assert_equal(pop1[i].origin_year, pop2[i].origin_year, "Same seed should produce same origin")
		assert_equal(pop1[i].tech_level, pop2[i].tech_level, "Same seed should produce same tech")


## Tests max_populations is respected.
func test_max_populations_respected() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.max_populations = 1
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	assert_less_than(populations.size(), 2, "Should respect max_populations")


## Tests territorial control sums reasonably.
func test_territorial_control_reasonable() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	var total_control: float = 0.0
	for pop in populations:
		total_control += pop.territorial_control
		assert_in_range(pop.territorial_control, 0.0, 1.0, "Individual control should be 0-1")

	assert_less_than(total_control, 1.5, "Total territorial control should be reasonable")


## Tests tech level appropriate for history length.
func test_tech_level_appropriate() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	spec.min_history_years = 100
	spec.max_history_years = 500
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		assert_less_than(pop.tech_level as int, TechnologyLevel.Level.SPACEFARING as int,
			"Short history should not produce spacefaring tech")


## Tests cultural traits are generated.
func test_cultural_traits_generated() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		assert_greater_than(pop.cultural_traits.size(), 0, "Should have cultural traits")


## Tests primary biome is from profile biomes.
func test_primary_biome_valid() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		assert_not_equal(pop.primary_biome, "", "Should have primary biome")


## Tests government is appropriate for tech level.
func test_government_appropriate() -> void:
	var profile: PlanetProfile = _create_habitable_profile()
	var spec: NativePopulationGenerator.NativePopulationSpec = _create_default_spec()
	spec.force_population = true
	var rng: SeededRng = SeededRng.new(12345)

	var populations: Array[NativePopulation] = NativePopulationGenerator.generate(profile, spec, rng)

	for pop in populations:
		assert_not_null(pop.government)
		if pop.tech_level == TechnologyLevel.Level.STONE_AGE:
			assert_equal(pop.government.regime, GovernmentType.Regime.TRIBAL,
				"Stone age should have tribal government")
