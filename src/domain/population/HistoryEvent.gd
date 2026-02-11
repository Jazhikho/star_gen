## A single historical event in a population's timeline.
## Used by both NativePopulation and Colony for consistent history tracking.
class_name HistoryEvent
extends RefCounted


## Event type categories.
enum EventType {
	FOUNDING, ## Initial settlement or emergence
	NATURAL_DISASTER, ## Earthquake, volcano, flood, meteor, etc.
	PLAGUE, ## Disease outbreak, pandemic
	FAMINE, ## Food shortage, crop failure
	WAR, ## External conflict with another population
	CIVIL_WAR, ## Internal conflict, revolution
	TECH_ADVANCEMENT, ## Significant technological progress
	EXPANSION, ## Territory growth, new settlements
	POLITICAL_CHANGE, ## Government change, reform, coup
	MIGRATION, ## Large population movement
	COLLAPSE, ## Civilization collapse, abandonment
	GOLDEN_AGE, ## Period of prosperity and growth
	CULTURAL_SHIFT, ## Major cultural or religious change
	CONTACT, ## First contact with another population
	TREATY, ## Peace treaty, alliance, trade agreement
	INDEPENDENCE, ## Colony independence, secession
	ANNEXATION, ## Absorption by another power
	DISCOVERY, ## Scientific or geographic discovery
	CONSTRUCTION, ## Major infrastructure or monument
	LEADER, ## Notable leader rises or falls
}


## The type of this event.
var event_type: EventType = EventType.FOUNDING

## Year the event occurred (negative = past, positive = future, 0 = present).
var year: int = 0

## Short title or name for the event.
var title: String = ""

## Longer description of what happened.
var description: String = ""

## Severity/magnitude of the event (-1.0 to 1.0).
## Negative = harmful, Positive = beneficial, 0 = neutral.
var magnitude: float = 0.0

## Population change caused by this event (can be negative).
var population_delta: int = 0

## Optional: ID of related population (for wars, contact, treaties).
var related_population_id: String = ""

## Optional: Additional data specific to event type.
var metadata: Dictionary = {}


## Creates a new HistoryEvent.
## @param p_type: The event type.
## @param p_year: Year of the event.
## @param p_title: Short title.
## @param p_description: Longer description.
## @param p_magnitude: Severity (-1.0 to 1.0).
func _init(
	p_type: EventType = EventType.FOUNDING,
	p_year: int = 0,
	p_title: String = "",
	p_description: String = "",
	p_magnitude: float = 0.0
) -> void:
	event_type = p_type
	year = p_year
	title = p_title
	description = p_description
	magnitude = clampf(p_magnitude, -1.0, 1.0)


## Returns whether this event is harmful (negative magnitude).
## @return: True if magnitude < 0.
func is_harmful() -> bool:
	return magnitude < 0.0


## Returns whether this event is beneficial (positive magnitude).
## @return: True if magnitude > 0.
func is_beneficial() -> bool:
	return magnitude > 0.0


## Returns whether this event is neutral.
## @return: True if magnitude == 0.
func is_neutral() -> bool:
	return magnitude == 0.0


## Returns whether this event involves another population.
## @return: True if related_population_id is set.
func involves_other_population() -> bool:
	return related_population_id != ""


## Returns the absolute year (for sorting purposes).
## @return: The year value.
func get_sort_key() -> int:
	return year


## Converts event type to display string.
## @param type: The event type enum value.
## @return: Human-readable string.
static func type_to_string(type: EventType) -> String:
	match type:
		EventType.FOUNDING:
			return "Founding"
		EventType.NATURAL_DISASTER:
			return "Natural Disaster"
		EventType.PLAGUE:
			return "Plague"
		EventType.FAMINE:
			return "Famine"
		EventType.WAR:
			return "War"
		EventType.CIVIL_WAR:
			return "Civil War"
		EventType.TECH_ADVANCEMENT:
			return "Technological Advancement"
		EventType.EXPANSION:
			return "Expansion"
		EventType.POLITICAL_CHANGE:
			return "Political Change"
		EventType.MIGRATION:
			return "Migration"
		EventType.COLLAPSE:
			return "Collapse"
		EventType.GOLDEN_AGE:
			return "Golden Age"
		EventType.CULTURAL_SHIFT:
			return "Cultural Shift"
		EventType.CONTACT:
			return "First Contact"
		EventType.TREATY:
			return "Treaty"
		EventType.INDEPENDENCE:
			return "Independence"
		EventType.ANNEXATION:
			return "Annexation"
		EventType.DISCOVERY:
			return "Discovery"
		EventType.CONSTRUCTION:
			return "Construction"
		EventType.LEADER:
			return "Notable Leader"
		_:
			return "Unknown"


## Converts string to event type.
## @param name: The string name (case-insensitive).
## @return: The event type, or FOUNDING if not found.
static func type_from_string(name: String) -> EventType:
	match name.to_lower().replace(" ", "_"):
		"founding":
			return EventType.FOUNDING
		"natural_disaster":
			return EventType.NATURAL_DISASTER
		"plague":
			return EventType.PLAGUE
		"famine":
			return EventType.FAMINE
		"war":
			return EventType.WAR
		"civil_war":
			return EventType.CIVIL_WAR
		"tech_advancement", "technological_advancement":
			return EventType.TECH_ADVANCEMENT
		"expansion":
			return EventType.EXPANSION
		"political_change":
			return EventType.POLITICAL_CHANGE
		"migration":
			return EventType.MIGRATION
		"collapse":
			return EventType.COLLAPSE
		"golden_age":
			return EventType.GOLDEN_AGE
		"cultural_shift":
			return EventType.CULTURAL_SHIFT
		"contact", "first_contact":
			return EventType.CONTACT
		"treaty":
			return EventType.TREATY
		"independence":
			return EventType.INDEPENDENCE
		"annexation":
			return EventType.ANNEXATION
		"discovery":
			return EventType.DISCOVERY
		"construction":
			return EventType.CONSTRUCTION
		"leader", "notable_leader":
			return EventType.LEADER
		_:
			return EventType.FOUNDING


## Returns whether an event type is typically harmful.
## @param type: The event type to check.
## @return: True if typically harmful.
static func is_typically_harmful(type: EventType) -> bool:
	match type:
		EventType.NATURAL_DISASTER, EventType.PLAGUE, EventType.FAMINE, \
		EventType.WAR, EventType.CIVIL_WAR, EventType.COLLAPSE, EventType.ANNEXATION:
			return true
		_:
			return false


## Returns whether an event type is typically beneficial.
## @param type: The event type to check.
## @return: True if typically beneficial.
static func is_typically_beneficial(type: EventType) -> bool:
	match type:
		EventType.TECH_ADVANCEMENT, EventType.EXPANSION, EventType.GOLDEN_AGE, \
		EventType.TREATY, EventType.INDEPENDENCE, EventType.DISCOVERY, EventType.CONSTRUCTION:
			return true
		_:
			return false


## Returns the number of event types.
## @return: Count of EventType enum values.
static func type_count() -> int:
	return 20


## Converts this event to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"event_type": event_type as int,
		"year": year,
		"title": title,
		"description": description,
		"magnitude": magnitude,
		"population_delta": population_delta,
		"related_population_id": related_population_id,
		"metadata": metadata.duplicate(),
	}


## Creates a HistoryEvent from a dictionary.
## @param data: The dictionary to parse.
## @return: A new HistoryEvent instance.
static func from_dict(data: Dictionary) -> HistoryEvent:
	var event: HistoryEvent = HistoryEvent.new()

	var type_val: Variant = data.get("event_type", 0)
	if type_val is String:
		type_val = int(type_val as String)
	event.event_type = type_val as EventType

	event.year = data.get("year", 0) as int
	event.title = data.get("title", "") as String
	event.description = data.get("description", "") as String
	event.magnitude = clampf(data.get("magnitude", 0.0) as float, -1.0, 1.0)
	event.population_delta = data.get("population_delta", 0) as int
	event.related_population_id = data.get("related_population_id", "") as String
	event.metadata = (data.get("metadata", {}) as Dictionary).duplicate()

	return event
