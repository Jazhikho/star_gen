## Timeline of historical events for a population.
## Provides ordered storage and query methods for HistoryEvent instances.
## Used by both NativePopulation and Colony.
class_name PopulationHistory
extends RefCounted

# Preload for headless/editor compilation.
const _history_event: GDScript = preload("res://src/domain/population/HistoryEvent.gd")


## All events in this history, sorted by year.
var _events: Array[HistoryEvent] = []

## Whether the events array needs re-sorting.
var _needs_sort: bool = false


## Returns the number of events in this history.
## @return: Event count.
func size() -> int:
	return _events.size()


## Returns whether this history is empty.
## @return: True if no events.
func is_empty() -> bool:
	return _events.is_empty()


## Adds an event to the history.
## @param event: The event to add.
func add_event(event: HistoryEvent) -> void:
	_events.append(event)
	_needs_sort = true


## Creates and adds a new event with the given parameters.
## @param type: Event type.
## @param year: Year of event.
## @param title: Event title.
## @param description: Event description.
## @param magnitude: Event magnitude (-1.0 to 1.0).
## @return: The created event.
func add_new_event(
	type: HistoryEvent.EventType,
	year: int,
	title: String,
	description: String = "",
	magnitude: float = 0.0
) -> HistoryEvent:
	var event: HistoryEvent = HistoryEvent.new(type, year, title, description, magnitude)
	add_event(event)
	return event


## Ensures events are sorted by year (ascending).
func _ensure_sorted() -> void:
	if _needs_sort:
		_events.sort_custom(func(a: HistoryEvent, b: HistoryEvent) -> bool:
			return a.year < b.year
		)
		_needs_sort = false


## Returns all events, sorted by year.
## @return: Array of all events.
func get_all_events() -> Array[HistoryEvent]:
	_ensure_sorted()
	return _events.duplicate()


## Returns the event at a specific index.
## @param index: The index to retrieve.
## @return: The event, or null if index out of bounds.
func get_event(index: int) -> HistoryEvent:
	_ensure_sorted()
	if index < 0 or index >= _events.size():
		return null
	return _events[index]


## Returns the first (earliest) event.
## @return: The earliest event, or null if empty.
func get_first_event() -> HistoryEvent:
	_ensure_sorted()
	if _events.is_empty():
		return null
	return _events[0]


## Returns the last (most recent) event.
## @return: The most recent event, or null if empty.
func get_last_event() -> HistoryEvent:
	_ensure_sorted()
	if _events.is_empty():
		return null
	return _events[_events.size() - 1]


## Returns the founding event if one exists.
## @return: The founding event, or null if not found.
func get_founding_event() -> HistoryEvent:
	for event in _events:
		if event.event_type == HistoryEvent.EventType.FOUNDING:
			return event
	return null


## Returns all events of a specific type.
## @param type: The event type to filter by.
## @return: Array of matching events.
func get_events_by_type(type: HistoryEvent.EventType) -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.event_type == type:
			result.append(event)
	return result


## Returns all events within a year range (inclusive).
## @param start_year: The earliest year to include.
## @param end_year: The latest year to include.
## @return: Array of events in the range.
func get_events_in_range(start_year: int, end_year: int) -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.year >= start_year and event.year <= end_year:
			result.append(event)
	return result


## Returns all events before a given year.
## @param year: The cutoff year (exclusive).
## @return: Array of earlier events.
func get_events_before(year: int) -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.year < year:
			result.append(event)
	return result


## Returns all events after a given year.
## @param year: The cutoff year (exclusive).
## @return: Array of later events.
func get_events_after(year: int) -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.year > year:
			result.append(event)
	return result


## Returns all harmful events (negative magnitude).
## @return: Array of harmful events.
func get_harmful_events() -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.is_harmful():
			result.append(event)
	return result


## Returns all beneficial events (positive magnitude).
## @return: Array of beneficial events.
func get_beneficial_events() -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.is_beneficial():
			result.append(event)
	return result


## Returns events involving a specific other population.
## @param population_id: The related population ID.
## @return: Array of related events.
func get_events_involving(population_id: String) -> Array[HistoryEvent]:
	_ensure_sorted()
	var result: Array[HistoryEvent] = []
	for event in _events:
		if event.related_population_id == population_id:
			result.append(event)
	return result


## Calculates the total population change from all events.
## @return: Net population delta.
func get_total_population_delta() -> int:
	var total: int = 0
	for event in _events:
		total += event.population_delta
	return total


## Calculates the population change within a year range.
## @param start_year: The earliest year to include.
## @param end_year: The latest year to include.
## @return: Net population delta in range.
func get_population_delta_in_range(start_year: int, end_year: int) -> int:
	var total: int = 0
	for event in _events:
		if event.year >= start_year and event.year <= end_year:
			total += event.population_delta
	return total


## Returns the year span of this history.
## @return: Dictionary with "start" and "end" years, or empty if no events.
func get_year_span() -> Dictionary:
	if _events.is_empty():
		return {}
	_ensure_sorted()
	return {
		"start": _events[0].year,
		"end": _events[_events.size() - 1].year,
	}


## Returns the duration of this history in years.
## @return: Number of years from first to last event, or 0 if less than 2 events.
func get_duration_years() -> int:
	if _events.size() < 2:
		return 0
	_ensure_sorted()
	return _events[_events.size() - 1].year - _events[0].year


## Returns statistics about event types in this history.
## @return: Dictionary of EventType (as int) -> count.
func get_event_type_counts() -> Dictionary:
	var counts: Dictionary = {}
	for event in _events:
		var key: int = event.event_type as int
		counts[key] = (counts.get(key, 0) as int) + 1
	return counts


## Returns the most common event type.
## @return: The most frequent EventType, or FOUNDING if empty.
func get_most_common_event_type() -> HistoryEvent.EventType:
	var counts: Dictionary = get_event_type_counts()
	var max_count: int = 0
	var most_common: HistoryEvent.EventType = HistoryEvent.EventType.FOUNDING

	for type_key in counts.keys():
		var count: int = counts[type_key] as int
		if count > max_count:
			max_count = count
			most_common = type_key as HistoryEvent.EventType

	return most_common


## Removes all events from this history.
func clear() -> void:
	_events.clear()
	_needs_sort = false


## Removes a specific event from this history.
## @param event: The event to remove.
## @return: True if the event was found and removed.
func remove_event(event: HistoryEvent) -> bool:
	var index: int = _events.find(event)
	if index >= 0:
		_events.remove_at(index)
		return true
	return false


## Creates a copy of this history.
## @return: A new PopulationHistory with copied events.
func duplicate() -> PopulationHistory:
	var copy: PopulationHistory = PopulationHistory.new()
	for event in _events:
		var event_copy: HistoryEvent = HistoryEvent.from_dict(event.to_dict())
		copy.add_event(event_copy)
	return copy


## Merges another history into this one.
## @param other: The history to merge.
func merge(other: PopulationHistory) -> void:
	for event in other._events:
		_events.append(event)
	_needs_sort = true


## Converts this history to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	_ensure_sorted()
	var events_data: Array[Dictionary] = []
	for event in _events:
		events_data.append(event.to_dict())

	return {
		"events": events_data,
	}


## Creates a PopulationHistory from a dictionary.
## @param data: The dictionary to parse.
## @return: A new PopulationHistory instance.
static func from_dict(data: Dictionary) -> PopulationHistory:
	var history: PopulationHistory = PopulationHistory.new()

	var events_data: Array = data.get("events", []) as Array
	for event_data in events_data:
		var event: HistoryEvent = HistoryEvent.from_dict(event_data as Dictionary)
		history._events.append(event)

	history._needs_sort = true
	return history
