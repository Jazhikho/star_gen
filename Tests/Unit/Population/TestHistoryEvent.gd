## Tests for HistoryEvent data model.
extends TestCase

const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## Tests basic event creation.
func test_creation_default() -> void:
	var event: HistoryEvent = HistoryEvent.new()
	assert_equal(event.event_type, HistoryEvent.EventType.FOUNDING)
	assert_equal(event.year, 0)
	assert_equal(event.title, "")
	assert_equal(event.description, "")
	assert_float_equal(event.magnitude, 0.0, 0.001)
	assert_equal(event.population_delta, 0)
	assert_equal(event.related_population_id, "")


## Tests event creation with parameters.
func test_creation_with_params() -> void:
	var event: HistoryEvent = HistoryEvent.new(
		HistoryEvent.EventType.WAR,
		-500,
		"The Great War",
		"A devastating conflict.",
		-0.7
	)
	assert_equal(event.event_type, HistoryEvent.EventType.WAR)
	assert_equal(event.year, -500)
	assert_equal(event.title, "The Great War")
	assert_equal(event.description, "A devastating conflict.")
	assert_float_equal(event.magnitude, -0.7, 0.001)


## Tests magnitude clamping.
func test_magnitude_clamped() -> void:
	var event1: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.FOUNDING, 0, "", "", 2.0)
	assert_float_equal(event1.magnitude, 1.0, 0.001)

	var event2: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.FOUNDING, 0, "", "", -2.0)
	assert_float_equal(event2.magnitude, -1.0, 0.001)


## Tests is_harmful method.
func test_is_harmful() -> void:
	var event: HistoryEvent = HistoryEvent.new()

	event.magnitude = -0.5
	assert_true(event.is_harmful())

	event.magnitude = 0.0
	assert_false(event.is_harmful())

	event.magnitude = 0.5
	assert_false(event.is_harmful())


## Tests is_beneficial method.
func test_is_beneficial() -> void:
	var event: HistoryEvent = HistoryEvent.new()

	event.magnitude = 0.5
	assert_true(event.is_beneficial())

	event.magnitude = 0.0
	assert_false(event.is_beneficial())

	event.magnitude = -0.5
	assert_false(event.is_beneficial())


## Tests is_neutral method.
func test_is_neutral() -> void:
	var event: HistoryEvent = HistoryEvent.new()

	event.magnitude = 0.0
	assert_true(event.is_neutral())

	event.magnitude = 0.1
	assert_false(event.is_neutral())


## Tests involves_other_population method.
func test_involves_other_population() -> void:
	var event: HistoryEvent = HistoryEvent.new()
	assert_false(event.involves_other_population())

	event.related_population_id = "other_pop_001"
	assert_true(event.involves_other_population())


## Tests get_sort_key method.
func test_get_sort_key() -> void:
	var event: HistoryEvent = HistoryEvent.new()
	event.year = -1000
	assert_equal(event.get_sort_key(), -1000)

	event.year = 500
	assert_equal(event.get_sort_key(), 500)


## Tests type_to_string for all types.
func test_type_to_string() -> void:
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.FOUNDING), "Founding")
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.NATURAL_DISASTER), "Natural Disaster")
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.PLAGUE), "Plague")
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.WAR), "War")
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.GOLDEN_AGE), "Golden Age")
	assert_equal(HistoryEvent.type_to_string(HistoryEvent.EventType.TECH_ADVANCEMENT), "Technological Advancement")


## Tests type_from_string.
func test_type_from_string() -> void:
	assert_equal(HistoryEvent.type_from_string("founding"), HistoryEvent.EventType.FOUNDING)
	assert_equal(HistoryEvent.type_from_string("War"), HistoryEvent.EventType.WAR)
	assert_equal(HistoryEvent.type_from_string("GOLDEN_AGE"), HistoryEvent.EventType.GOLDEN_AGE)
	assert_equal(HistoryEvent.type_from_string("natural_disaster"), HistoryEvent.EventType.NATURAL_DISASTER)
	assert_equal(HistoryEvent.type_from_string("invalid"), HistoryEvent.EventType.FOUNDING)


## Tests is_typically_harmful.
func test_is_typically_harmful() -> void:
	assert_true(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.NATURAL_DISASTER))
	assert_true(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.PLAGUE))
	assert_true(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.WAR))
	assert_true(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.COLLAPSE))
	assert_false(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.GOLDEN_AGE))
	assert_false(HistoryEvent.is_typically_harmful(HistoryEvent.EventType.FOUNDING))


## Tests is_typically_beneficial.
func test_is_typically_beneficial() -> void:
	assert_true(HistoryEvent.is_typically_beneficial(HistoryEvent.EventType.TECH_ADVANCEMENT))
	assert_true(HistoryEvent.is_typically_beneficial(HistoryEvent.EventType.GOLDEN_AGE))
	assert_true(HistoryEvent.is_typically_beneficial(HistoryEvent.EventType.EXPANSION))
	assert_false(HistoryEvent.is_typically_beneficial(HistoryEvent.EventType.WAR))
	assert_false(HistoryEvent.is_typically_beneficial(HistoryEvent.EventType.FOUNDING))


## Tests type_count.
func test_type_count() -> void:
	assert_equal(HistoryEvent.type_count(), 20)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: HistoryEvent = HistoryEvent.new(
		HistoryEvent.EventType.WAR,
		-250,
		"The Border War",
		"A conflict over territory.",
		-0.6
	)
	original.population_delta = -15000
	original.related_population_id = "enemy_001"
	original.metadata = {"location": "northern_border", "duration_years": 5}

	var data: Dictionary = original.to_dict()
	var restored: HistoryEvent = HistoryEvent.from_dict(data)

	assert_equal(restored.event_type, original.event_type)
	assert_equal(restored.year, original.year)
	assert_equal(restored.title, original.title)
	assert_equal(restored.description, original.description)
	assert_float_equal(restored.magnitude, original.magnitude, 0.001)
	assert_equal(restored.population_delta, original.population_delta)
	assert_equal(restored.related_population_id, original.related_population_id)
	assert_equal(restored.metadata.size(), original.metadata.size())


## Tests from_dict with string type value (JSON).
func test_from_dict_string_type() -> void:
	var data: Dictionary = {
		"event_type": "5",
		"year": 100,
		"title": "Test Event",
	}
	var event: HistoryEvent = HistoryEvent.from_dict(data)
	assert_equal(event.event_type, HistoryEvent.EventType.CIVIL_WAR)
