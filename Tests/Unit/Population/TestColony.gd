## Tests for Colony data model.
extends TestCase

const _colony: GDScript = preload("res://src/domain/population/Colony.gd")
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## Creates a test colony.
func _create_test_colony() -> Colony:
	var colony: Colony = Colony.new()
	colony.id = "test_colony_001"
	colony.name = "New Terra"
	colony.body_id = "planet_001"
	colony.colony_type = ColonyType.Type.SETTLEMENT
	colony.founding_civilization_id = "civ_001"
	colony.founding_civilization_name = "Human Federation"
	colony.founding_year = -200
	colony.population = 500000
	colony.peak_population = 600000
	colony.peak_population_year = -50
	colony.tech_level = TechnologyLevel.Level.INTERSTELLAR
	colony.is_active = true
	colony.territorial_control = 0.3
	colony.primary_industry = "mixed economy"
	colony.self_sufficiency = 0.7

	colony.government.regime = GovernmentType.Regime.CONSTITUTIONAL
	colony.government.legitimacy = 0.8

	colony.history.add_new_event(
		HistoryEvent.EventType.FOUNDING,
		-200,
		"Colony Founded",
		"The colony was established"
	)

	return colony


## Tests default creation.
func test_creation_default() -> void:
	var colony: Colony = Colony.new()
	assert_equal(colony.id, "")
	assert_equal(colony.colony_type, ColonyType.Type.SETTLEMENT)
	assert_equal(colony.population, 0)
	assert_true(colony.is_active)
	assert_false(colony.is_independent)
	assert_not_null(colony.government)
	assert_not_null(colony.history)


## Tests get_age for active colony.
func test_get_age_active() -> void:
	var colony: Colony = _create_test_colony()
	colony.founding_year = -200

	var age: int = colony.get_age(0)
	assert_equal(age, 200)


## Tests get_age for abandoned colony.
func test_get_age_abandoned() -> void:
	var colony: Colony = _create_test_colony()
	colony.founding_year = -200
	colony.is_active = false
	colony.abandonment_year = -50

	var age: int = colony.get_age(0)
	assert_equal(age, 150)


## Tests get_growth_state.
func test_get_growth_state() -> void:
	var colony: Colony = _create_test_colony()

	colony.population = 600000
	colony.peak_population = 600000
	assert_equal(colony.get_growth_state(), "growing")

	colony.population = 400000
	assert_equal(colony.get_growth_state(), "stable")

	colony.population = 200000
	assert_equal(colony.get_growth_state(), "declining")

	colony.is_active = false
	assert_equal(colony.get_growth_state(), "abandoned")


## Tests get_regime.
func test_get_regime() -> void:
	var colony: Colony = _create_test_colony()
	assert_equal(colony.get_regime(), GovernmentType.Regime.CONSTITUTIONAL)


## Tests is_politically_stable.
func test_is_politically_stable() -> void:
	var colony: Colony = _create_test_colony()
	colony.government.legitimacy = 0.8
	assert_true(colony.is_politically_stable())

	colony.government.legitimacy = 0.1
	assert_false(colony.is_politically_stable())


## Tests native relation management.
func test_native_relations() -> void:
	var colony: Colony = _create_test_colony()
	assert_false(colony.has_native_relations())

	var relation: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 20)
	colony.set_native_relation(relation)

	assert_true(colony.has_native_relations())
	assert_not_null(colony.get_native_relation("native_001"))
	assert_null(colony.get_native_relation("nonexistent"))


## Tests get_all_native_relations.
func test_get_all_native_relations() -> void:
	var colony: Colony = _create_test_colony()

	var rel1: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 20)
	var rel2: NativeRelation = NativeRelation.create_first_contact("native_002", -100, -30)
	colony.set_native_relation(rel1)
	colony.set_native_relation(rel2)

	var all_relations: Array[NativeRelation] = colony.get_all_native_relations()
	assert_equal(all_relations.size(), 2)


## Tests has_hostile_native_relations.
func test_has_hostile_native_relations() -> void:
	var colony: Colony = _create_test_colony()

	var peaceful: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 50)
	peaceful.status = NativeRelation.Status.PEACEFUL
	colony.set_native_relation(peaceful)

	assert_false(colony.has_hostile_native_relations())

	var hostile: NativeRelation = NativeRelation.create_first_contact("native_002", -100, -80)
	hostile.status = NativeRelation.Status.HOSTILE
	colony.set_native_relation(hostile)

	assert_true(colony.has_hostile_native_relations())


## Tests get_overall_native_status.
func test_get_overall_native_status() -> void:
	var colony: Colony = _create_test_colony()
	assert_equal(colony.get_overall_native_status(), "none")

	var peaceful: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 50)
	peaceful.status = NativeRelation.Status.PEACEFUL
	colony.set_native_relation(peaceful)
	assert_equal(colony.get_overall_native_status(), "peaceful")

	var hostile: NativeRelation = NativeRelation.create_first_contact("native_002", -100, -80)
	hostile.status = NativeRelation.Status.HOSTILE
	colony.set_native_relation(hostile)
	assert_equal(colony.get_overall_native_status(), "mixed")


## Tests record_abandonment.
func test_record_abandonment() -> void:
	var colony: Colony = _create_test_colony()
	assert_true(colony.is_active)

	colony.record_abandonment(-50, "resource depletion")

	assert_false(colony.is_active)
	assert_equal(colony.abandonment_year, -50)
	assert_equal(colony.abandonment_reason, "resource depletion")
	assert_equal(colony.population, 0)


## Tests record_independence.
func test_record_independence() -> void:
	var colony: Colony = _create_test_colony()
	assert_false(colony.is_independent)

	colony.record_independence(-30)

	assert_true(colony.is_independent)
	assert_equal(colony.independence_year, -30)


## Tests update_peak_population.
func test_update_peak_population() -> void:
	var colony: Colony = _create_test_colony()
	colony.population = 700000
	colony.peak_population = 600000

	colony.update_peak_population(-10)

	assert_equal(colony.peak_population, 700000)
	assert_equal(colony.peak_population_year, -10)


## Tests get_summary.
func test_get_summary() -> void:
	var colony: Colony = _create_test_colony()
	var summary: Dictionary = colony.get_summary()

	assert_equal(summary["id"], "test_colony_001")
	assert_equal(summary["name"], "New Terra")
	assert_equal(summary["colony_type"], "Settlement")
	assert_equal(summary["population"], 500000)
	assert_true(summary["is_active"] as bool)
	assert_false(summary["is_independent"] as bool)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: Colony = _create_test_colony()

	var rel: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 30)
	original.set_native_relation(rel)

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.body_id, original.body_id)
	assert_equal(restored.colony_type, original.colony_type)
	assert_equal(restored.founding_civilization_id, original.founding_civilization_id)
	assert_equal(restored.founding_year, original.founding_year)
	assert_equal(restored.population, original.population)
	assert_equal(restored.tech_level, original.tech_level)
	assert_equal(restored.is_active, original.is_active)
	assert_float_equal(restored.territorial_control, original.territorial_control, 0.001)
	assert_float_equal(restored.self_sufficiency, original.self_sufficiency, 0.001)


## Tests government serialization.
func test_government_serialization() -> void:
	var original: Colony = _create_test_colony()

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_equal(restored.government.regime, original.government.regime)
	assert_float_equal(restored.government.legitimacy, original.government.legitimacy, 0.001)


## Tests history serialization.
func test_history_serialization() -> void:
	var original: Colony = _create_test_colony()

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_equal(restored.history.size(), original.history.size())


## Tests native_relations serialization.
func test_native_relations_serialization() -> void:
	var original: Colony = _create_test_colony()

	var rel1: NativeRelation = NativeRelation.create_first_contact("native_001", -150, 30)
	var rel2: NativeRelation = NativeRelation.create_first_contact("native_002", -100, -20)
	original.set_native_relation(rel1)
	original.set_native_relation(rel2)

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_equal(restored.native_relations.size(), 2)
	assert_not_null(restored.get_native_relation("native_001"))
	assert_not_null(restored.get_native_relation("native_002"))


## Tests abandoned colony serialization.
func test_abandoned_colony_serialization() -> void:
	var original: Colony = _create_test_colony()
	original.record_abandonment(-50, "hostile natives")

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_false(restored.is_active)
	assert_equal(restored.abandonment_year, -50)
	assert_equal(restored.abandonment_reason, "hostile natives")


## Tests independent colony serialization.
func test_independent_colony_serialization() -> void:
	var original: Colony = _create_test_colony()
	original.record_independence(-30)

	var data: Dictionary = original.to_dict()
	var restored: Colony = Colony.from_dict(data)

	assert_true(restored.is_independent)
	assert_equal(restored.independence_year, -30)
