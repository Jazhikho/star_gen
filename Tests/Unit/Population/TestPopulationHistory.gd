## Tests for PopulationHistory timeline management.
extends TestCase

const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")


## Creates a sample history for testing.
func _create_sample_history() -> PopulationHistory:
	var history: PopulationHistory = PopulationHistory.new()

	history.add_event(HistoryEvent.new(HistoryEvent.EventType.FOUNDING, -1000, "The Founding", "", 0.5))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.EXPANSION, -800, "First Expansion", "", 0.4))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, -500, "The Great War", "", -0.7))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.GOLDEN_AGE, -200, "The Golden Age", "", 0.8))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.PLAGUE, 0, "The Plague", "", -0.6))

	var events: Array[HistoryEvent] = history.get_all_events()
	events[0].population_delta = 1000
	events[1].population_delta = 5000
	events[2].population_delta = -10000
	events[3].population_delta = 20000
	events[4].population_delta = -15000

	return history


## Tests empty history.
func test_empty_history() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	assert_equal(history.size(), 0)
	assert_true(history.is_empty())
	assert_null(history.get_first_event())
	assert_null(history.get_last_event())


## Tests adding events.
func test_add_event() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	var event: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.FOUNDING, -1000, "Test", "", 0.5)

	history.add_event(event)

	assert_equal(history.size(), 1)
	assert_false(history.is_empty())


## Tests add_new_event convenience method.
func test_add_new_event() -> void:
	var history: PopulationHistory = PopulationHistory.new()

	var event: HistoryEvent = history.add_new_event(
		HistoryEvent.EventType.WAR,
		-500,
		"The War",
		"A great conflict.",
		-0.5
	)

	assert_equal(history.size(), 1)
	assert_equal(event.event_type, HistoryEvent.EventType.WAR)
	assert_equal(event.year, -500)


## Tests events are sorted by year.
func test_events_sorted() -> void:
	var history: PopulationHistory = PopulationHistory.new()

	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, 0, "Middle", "", 0.0))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.FOUNDING, -1000, "First", "", 0.0))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.EXPANSION, 500, "Last", "", 0.0))

	var events: Array[HistoryEvent] = history.get_all_events()

	assert_equal(events[0].year, -1000)
	assert_equal(events[1].year, 0)
	assert_equal(events[2].year, 500)


## Tests get_event by index.
func test_get_event() -> void:
	var history: PopulationHistory = _create_sample_history()

	var first: HistoryEvent = history.get_event(0)
	assert_not_null(first)
	assert_equal(first.year, -1000)

	var out_of_bounds: HistoryEvent = history.get_event(100)
	assert_null(out_of_bounds)

	var negative: HistoryEvent = history.get_event(-1)
	assert_null(negative)


## Tests get_first_event.
func test_get_first_event() -> void:
	var history: PopulationHistory = _create_sample_history()
	var first: HistoryEvent = history.get_first_event()

	assert_not_null(first)
	assert_equal(first.year, -1000)
	assert_equal(first.event_type, HistoryEvent.EventType.FOUNDING)


## Tests get_last_event.
func test_get_last_event() -> void:
	var history: PopulationHistory = _create_sample_history()
	var last: HistoryEvent = history.get_last_event()

	assert_not_null(last)
	assert_equal(last.year, 0)
	assert_equal(last.event_type, HistoryEvent.EventType.PLAGUE)


## Tests get_founding_event.
func test_get_founding_event() -> void:
	var history: PopulationHistory = _create_sample_history()
	var founding: HistoryEvent = history.get_founding_event()

	assert_not_null(founding)
	assert_equal(founding.event_type, HistoryEvent.EventType.FOUNDING)


## Tests get_founding_event when none exists.
func test_get_founding_event_none() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, 0, "War", "", 0.0))

	var founding: HistoryEvent = history.get_founding_event()
	assert_null(founding)


## Tests get_events_by_type.
func test_get_events_by_type() -> void:
	var history: PopulationHistory = _create_sample_history()

	var wars: Array[HistoryEvent] = history.get_events_by_type(HistoryEvent.EventType.WAR)
	assert_equal(wars.size(), 1)
	assert_equal(wars[0].title, "The Great War")

	var expansions: Array[HistoryEvent] = history.get_events_by_type(HistoryEvent.EventType.EXPANSION)
	assert_equal(expansions.size(), 1)

	var migrations: Array[HistoryEvent] = history.get_events_by_type(HistoryEvent.EventType.MIGRATION)
	assert_equal(migrations.size(), 0)


## Tests get_events_in_range.
func test_get_events_in_range() -> void:
	var history: PopulationHistory = _create_sample_history()

	var events: Array[HistoryEvent] = history.get_events_in_range(-600, -100)
	assert_equal(events.size(), 2)


## Tests get_events_before.
func test_get_events_before() -> void:
	var history: PopulationHistory = _create_sample_history()

	var events: Array[HistoryEvent] = history.get_events_before(-500)
	assert_equal(events.size(), 2)


## Tests get_events_after.
func test_get_events_after() -> void:
	var history: PopulationHistory = _create_sample_history()

	var events: Array[HistoryEvent] = history.get_events_after(-500)
	assert_equal(events.size(), 2)


## Tests get_harmful_events.
func test_get_harmful_events() -> void:
	var history: PopulationHistory = _create_sample_history()

	var harmful: Array[HistoryEvent] = history.get_harmful_events()
	assert_equal(harmful.size(), 2)


## Tests get_beneficial_events.
func test_get_beneficial_events() -> void:
	var history: PopulationHistory = _create_sample_history()

	var beneficial: Array[HistoryEvent] = history.get_beneficial_events()
	assert_equal(beneficial.size(), 3)


## Tests get_events_involving.
func test_get_events_involving() -> void:
	var history: PopulationHistory = PopulationHistory.new()

	var event1: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.WAR, -500, "War", "", -0.5)
	event1.related_population_id = "enemy_001"
	history.add_event(event1)

	var event2: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.TREATY, -400, "Peace", "", 0.3)
	event2.related_population_id = "enemy_001"
	history.add_event(event2)

	var event3: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.EXPANSION, -300, "Expand", "", 0.4)
	history.add_event(event3)

	var related: Array[HistoryEvent] = history.get_events_involving("enemy_001")
	assert_equal(related.size(), 2)


## Tests get_total_population_delta.
func test_get_total_population_delta() -> void:
	var history: PopulationHistory = _create_sample_history()

	var total: int = history.get_total_population_delta()
	assert_equal(total, 1000)


## Tests get_population_delta_in_range.
func test_get_population_delta_in_range() -> void:
	var history: PopulationHistory = _create_sample_history()

	var delta: int = history.get_population_delta_in_range(-600, -100)
	assert_equal(delta, 10000)


## Tests get_year_span.
func test_get_year_span() -> void:
	var history: PopulationHistory = _create_sample_history()

	var span: Dictionary = history.get_year_span()
	assert_equal(span["start"], -1000)
	assert_equal(span["end"], 0)


## Tests get_year_span empty.
func test_get_year_span_empty() -> void:
	var history: PopulationHistory = PopulationHistory.new()

	var span: Dictionary = history.get_year_span()
	assert_true(span.is_empty())


## Tests get_duration_years.
func test_get_duration_years() -> void:
	var history: PopulationHistory = _create_sample_history()

	var duration: int = history.get_duration_years()
	assert_equal(duration, 1000)


## Tests get_duration_years with single event.
func test_get_duration_years_single() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.FOUNDING, 0, "Start", "", 0.0))

	var duration: int = history.get_duration_years()
	assert_equal(duration, 0)


## Tests get_event_type_counts.
func test_get_event_type_counts() -> void:
	var history: PopulationHistory = _create_sample_history()

	var counts: Dictionary = history.get_event_type_counts()
	assert_equal(counts[HistoryEvent.EventType.FOUNDING as int], 1)
	assert_equal(counts[HistoryEvent.EventType.WAR as int], 1)
	assert_equal(counts[HistoryEvent.EventType.GOLDEN_AGE as int], 1)


## Tests get_most_common_event_type.
func test_get_most_common_event_type() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, -500, "War 1", "", 0.0))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, -400, "War 2", "", 0.0))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, -300, "War 3", "", 0.0))
	history.add_event(HistoryEvent.new(HistoryEvent.EventType.PLAGUE, -200, "Plague", "", 0.0))

	var most_common: HistoryEvent.EventType = history.get_most_common_event_type()
	assert_equal(most_common, HistoryEvent.EventType.WAR)


## Tests clear.
func test_clear() -> void:
	var history: PopulationHistory = _create_sample_history()
	assert_false(history.is_empty())

	history.clear()
	assert_true(history.is_empty())


## Tests remove_event.
func test_remove_event() -> void:
	var history: PopulationHistory = PopulationHistory.new()
	var event1: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.FOUNDING, -1000, "First", "", 0.0)
	var event2: HistoryEvent = HistoryEvent.new(HistoryEvent.EventType.WAR, -500, "Second", "", 0.0)

	history.add_event(event1)
	history.add_event(event2)
	assert_equal(history.size(), 2)

	var removed: bool = history.remove_event(event1)
	assert_true(removed)
	assert_equal(history.size(), 1)

	var not_found: bool = history.remove_event(event1)
	assert_false(not_found)


## Tests duplicate.
func test_duplicate() -> void:
	var original: PopulationHistory = _create_sample_history()
	var copy: PopulationHistory = original.duplicate()

	assert_equal(copy.size(), original.size())

	original.clear()
	assert_equal(copy.size(), 5)


## Tests merge.
func test_merge() -> void:
	var history1: PopulationHistory = PopulationHistory.new()
	history1.add_event(HistoryEvent.new(HistoryEvent.EventType.FOUNDING, -1000, "First", "", 0.0))

	var history2: PopulationHistory = PopulationHistory.new()
	history2.add_event(HistoryEvent.new(HistoryEvent.EventType.WAR, -500, "Second", "", 0.0))

	history1.merge(history2)

	assert_equal(history1.size(), 2)

	var events: Array[HistoryEvent] = history1.get_all_events()
	assert_equal(events[0].year, -1000)
	assert_equal(events[1].year, -500)


## Tests serialization round-trip.
func test_serialization_round_trip() -> void:
	var original: PopulationHistory = _create_sample_history()

	var data: Dictionary = original.to_dict()
	var restored: PopulationHistory = PopulationHistory.from_dict(data)

	assert_equal(restored.size(), original.size())

	var original_events: Array[HistoryEvent] = original.get_all_events()
	var restored_events: Array[HistoryEvent] = restored.get_all_events()

	for i in range(original_events.size()):
		assert_equal(restored_events[i].year, original_events[i].year)
		assert_equal(restored_events[i].event_type, original_events[i].event_type)
		assert_equal(restored_events[i].title, original_events[i].title)


## Tests empty history serialization.
func test_empty_serialization() -> void:
	var original: PopulationHistory = PopulationHistory.new()

	var data: Dictionary = original.to_dict()
	var restored: PopulationHistory = PopulationHistory.from_dict(data)

	assert_true(restored.is_empty())
