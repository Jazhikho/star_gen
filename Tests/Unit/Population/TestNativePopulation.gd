## Tests for NativePopulation data model.
extends TestCase

const _native_population: GDScript = preload("res://src/domain/population/NativePopulation.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## Creates a test population.
func _create_test_population() -> NativePopulation:
	var pop: NativePopulation = NativePopulation.new()
	pop.id = "test_native_001"
	pop.name = "Testani"
	pop.body_id = "planet_001"
	pop.origin_year = -10000
	pop.population = 1000000
	pop.peak_population = 1200000
	pop.peak_population_year = -500
	pop.tech_level = TechnologyLevel.Level.INDUSTRIAL
	pop.is_extant = true
	pop.primary_biome = "Forest"
	pop.territorial_control = 0.6
	pop.cultural_traits = ["seafaring", "mercantile"]

	pop.government.regime = GovernmentType.Regime.CONSTITUTIONAL
	pop.government.legitimacy = 0.7

	pop.history.add_new_event(
		HistoryEvent.EventType.FOUNDING,
		-10000,
		"Emergence",
		"The beginning"
	)

	return pop


## Tests default creation.
func test_creation_default() -> void:
	var pop: NativePopulation = NativePopulation.new()
	assert_equal(pop.id, "")
	assert_equal(pop.population, 0)
	assert_equal(pop.tech_level, TechnologyLevel.Level.STONE_AGE)
	assert_true(pop.is_extant)
	assert_not_null(pop.government)
	assert_not_null(pop.history)


## Tests get_age for extant population.
func test_get_age_extant() -> void:
	var pop: NativePopulation = _create_test_population()
	pop.origin_year = -5000

	var age: int = pop.get_age(0)
	assert_equal(age, 5000)


## Tests get_age for extinct population.
func test_get_age_extinct() -> void:
	var pop: NativePopulation = _create_test_population()
	pop.origin_year = -5000
	pop.is_extant = false
	pop.extinction_year = -1000

	var age: int = pop.get_age(0)
	assert_equal(age, 4000)


## Tests get_growth_state.
func test_get_growth_state() -> void:
	var pop: NativePopulation = _create_test_population()

	pop.population = 1200000
	pop.peak_population = 1200000
	assert_equal(pop.get_growth_state(), "growing")

	pop.population = 900000
	assert_equal(pop.get_growth_state(), "stable")

	pop.population = 400000
	assert_equal(pop.get_growth_state(), "declining")

	pop.is_extant = false
	assert_equal(pop.get_growth_state(), "extinct")


## Tests can_spaceflight.
func test_can_spaceflight() -> void:
	var pop: NativePopulation = NativePopulation.new()

	pop.tech_level = TechnologyLevel.Level.INDUSTRIAL
	assert_false(pop.can_spaceflight())

	pop.tech_level = TechnologyLevel.Level.SPACEFARING
	assert_true(pop.can_spaceflight())


## Tests can_colonize.
func test_can_colonize() -> void:
	var pop: NativePopulation = NativePopulation.new()

	pop.tech_level = TechnologyLevel.Level.SPACEFARING
	assert_false(pop.can_colonize())

	pop.tech_level = TechnologyLevel.Level.INTERSTELLAR
	assert_true(pop.can_colonize())


## Tests get_regime.
func test_get_regime() -> void:
	var pop: NativePopulation = _create_test_population()
	assert_equal(pop.get_regime(), GovernmentType.Regime.CONSTITUTIONAL)


## Tests is_politically_stable.
func test_is_politically_stable() -> void:
	var pop: NativePopulation = _create_test_population()
	pop.government.legitimacy = 0.7
	pop.government.regime = GovernmentType.Regime.CONSTITUTIONAL

	assert_true(pop.is_politically_stable())

	pop.government.legitimacy = 0.1
	assert_false(pop.is_politically_stable())


## Tests record_extinction.
func test_record_extinction() -> void:
	var pop: NativePopulation = _create_test_population()
	assert_true(pop.is_extant)

	pop.record_extinction(-100, "asteroid impact")

	assert_false(pop.is_extant)
	assert_equal(pop.extinction_year, -100)
	assert_equal(pop.extinction_cause, "asteroid impact")
	assert_equal(pop.population, 0)


## Tests update_peak_population.
func test_update_peak_population() -> void:
	var pop: NativePopulation = _create_test_population()
	pop.population = 500000
	pop.peak_population = 400000

	pop.update_peak_population(-50)

	assert_equal(pop.peak_population, 500000)
	assert_equal(pop.peak_population_year, -50)


## Tests update_peak_population does not update if lower.
func test_update_peak_population_no_update() -> void:
	var pop: NativePopulation = _create_test_population()
	pop.population = 300000
	pop.peak_population = 400000
	pop.peak_population_year = -100

	pop.update_peak_population(0)

	assert_equal(pop.peak_population, 400000)
	assert_equal(pop.peak_population_year, -100)


## Tests get_summary.
func test_get_summary() -> void:
	var pop: NativePopulation = _create_test_population()
	var summary: Dictionary = pop.get_summary()

	assert_equal(summary["id"], "test_native_001")
	assert_equal(summary["name"], "Testani")
	assert_equal(summary["population"], 1000000)
	assert_equal(summary["tech_level"], "Industrial")
	assert_equal(summary["regime"], "Constitutional Government")
	assert_true(summary["is_extant"] as bool)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: NativePopulation = _create_test_population()

	var data: Dictionary = original.to_dict()
	var restored: NativePopulation = NativePopulation.from_dict(data)

	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.origin_year, original.origin_year)
	assert_equal(restored.population, original.population)
	assert_equal(restored.peak_population, original.peak_population)
	assert_equal(restored.tech_level, original.tech_level)
	assert_equal(restored.is_extant, original.is_extant)
	assert_equal(restored.primary_biome, original.primary_biome)
	assert_float_equal(restored.territorial_control, original.territorial_control, 0.001)


## Tests cultural_traits serialization.
func test_cultural_traits_serialization() -> void:
	var original: NativePopulation = _create_test_population()

	var data: Dictionary = original.to_dict()
	var restored: NativePopulation = NativePopulation.from_dict(data)

	assert_equal(restored.cultural_traits.size(), original.cultural_traits.size())
	for cultural_trait in original.cultural_traits:
		assert_true(cultural_trait in restored.cultural_traits)


## Tests government serialization.
func test_government_serialization() -> void:
	var original: NativePopulation = _create_test_population()

	var data: Dictionary = original.to_dict()
	var restored: NativePopulation = NativePopulation.from_dict(data)

	assert_equal(restored.government.regime, original.government.regime)
	assert_float_equal(restored.government.legitimacy, original.government.legitimacy, 0.001)


## Tests history serialization.
func test_history_serialization() -> void:
	var original: NativePopulation = _create_test_population()

	var data: Dictionary = original.to_dict()
	var restored: NativePopulation = NativePopulation.from_dict(data)

	assert_equal(restored.history.size(), original.history.size())


## Tests extinct population serialization.
func test_extinct_population_serialization() -> void:
	var original: NativePopulation = _create_test_population()
	original.record_extinction(-100, "plague")

	var data: Dictionary = original.to_dict()
	var restored: NativePopulation = NativePopulation.from_dict(data)

	assert_false(restored.is_extant)
	assert_equal(restored.extinction_year, -100)
	assert_equal(restored.extinction_cause, "plague")
