## Represents the relationship between a colony and a native population.
## Tracks status, history, and dynamics of the relationship.
class_name NativeRelation
extends RefCounted


## Relationship status categories.
enum Status {
	UNKNOWN, ## No contact or awareness
	FIRST_CONTACT, ## Recently discovered each other
	PEACEFUL, ## Cooperative coexistence
	TRADING, ## Active trade relationships
	TENSE, ## Strained relations, potential conflict
	HOSTILE, ## Open conflict or war
	SUBJUGATED, ## Native population under colony control
	INTEGRATED, ## Populations have merged/assimilated
	EXTINCT, ## Native population no longer exists
}


## ID of the native population this relation refers to.
var native_population_id: String = ""

## Current relationship status.
var status: Status = Status.UNKNOWN

## Year first contact occurred (0 if no contact).
var first_contact_year: int = 0

## Relationship score (-100 to 100). Negative = hostile, positive = friendly.
var relation_score: int = 0

## Whether the colony has a treaty with this native population.
var has_treaty: bool = false

## Year of current treaty (if any).
var treaty_year: int = 0

## Trade volume (0-1 normalized).
var trade_level: float = 0.0

## Cultural exchange level (0-1).
var cultural_exchange: float = 0.0

## Conflict intensity if hostile (0-1).
var conflict_intensity: float = 0.0

## Percentage of native territory taken by colony (0-1).
var territory_taken: float = 0.0

## Notable events in the relationship.
var relationship_events: Array[String] = []


## Creates a new NativeRelation with default unknown status.
func _init() -> void:
	pass


## Creates a relation for first contact.
## @param native_id: ID of the native population.
## @param year: Year of first contact.
## @param initial_disposition: Starting relation score (-100 to 100).
## @return: A new NativeRelation.
static func create_first_contact(
	native_id: String,
	year: int,
	initial_disposition: int = 0
) -> NativeRelation:
	var relation: NativeRelation = NativeRelation.new()
	relation.native_population_id = native_id
	relation.status = Status.FIRST_CONTACT
	relation.first_contact_year = year
	relation.relation_score = clampi(initial_disposition, -100, 100)
	relation.relationship_events.append("First contact in year %d" % year)
	return relation


## Updates status based on relation score and other factors.
func update_status() -> void:
	if status == Status.EXTINCT:
		return # Can't change from extinct

	if status == Status.UNKNOWN:
		return # Need explicit first contact

	# Determine status from score and factors
	if territory_taken > 0.8:
		status = Status.SUBJUGATED
	elif cultural_exchange > 0.7 and relation_score > 50:
		status = Status.INTEGRATED
	elif conflict_intensity > 0.5:
		status = Status.HOSTILE
	elif relation_score < -50:
		status = Status.TENSE
	elif trade_level > 0.3 and relation_score > 20:
		status = Status.TRADING
	elif relation_score > 0:
		status = Status.PEACEFUL
	elif relation_score < -20:
		status = Status.TENSE
	else:
		if first_contact_year > 0:
			status = Status.PEACEFUL # Default post-contact


## Records that the native population has gone extinct.
## @param year: Year of extinction.
## @param cause: Cause of extinction.
func record_extinction(year: int, cause: String) -> void:
	status = Status.EXTINCT
	relationship_events.append("Native population extinct in year %d: %s" % [year, cause])


## Records a treaty being signed.
## @param year: Year of treaty.
## @param description: Treaty description.
func record_treaty(year: int, description: String) -> void:
	has_treaty = true
	treaty_year = year
	relation_score = mini(relation_score + 20, 100)
	relationship_events.append("Treaty signed in year %d: %s" % [year, description])
	update_status()


## Records a conflict event.
## @param year: Year of conflict.
## @param description: Conflict description.
## @param intensity: Conflict intensity (0-1).
func record_conflict(year: int, description: String, intensity: float) -> void:
	conflict_intensity = maxf(conflict_intensity, intensity)
	relation_score = maxi(relation_score - roundi(intensity * 30), -100)
	has_treaty = false # Treaties broken by conflict
	relationship_events.append("Conflict in year %d: %s" % [year, description])
	update_status()


## Returns whether the relationship is positive.
## @return: True if relation_score > 0.
func is_positive() -> bool:
	return relation_score > 0


## Returns whether the relationship is hostile.
## @return: True if in hostile or tense state with negative score.
func is_hostile() -> bool:
	return status == Status.HOSTILE or (status == Status.TENSE and relation_score < -30)


## Returns whether there is active trade.
## @return: True if trading status and trade_level > 0.
func has_active_trade() -> bool:
	return status == Status.TRADING and trade_level > 0.0


## Converts status enum to display string.
## @param s: The status enum value.
## @return: Human-readable string.
static func status_to_string(s: Status) -> String:
	match s:
		Status.UNKNOWN:
			return "Unknown"
		Status.FIRST_CONTACT:
			return "First Contact"
		Status.PEACEFUL:
			return "Peaceful"
		Status.TRADING:
			return "Trading"
		Status.TENSE:
			return "Tense"
		Status.HOSTILE:
			return "Hostile"
		Status.SUBJUGATED:
			return "Subjugated"
		Status.INTEGRATED:
			return "Integrated"
		Status.EXTINCT:
			return "Extinct"
		_:
			return "Unknown"


## Converts string to status enum.
## @param name: The string name.
## @return: The status, or UNKNOWN if not found.
static func status_from_string(name: String) -> Status:
	match name.to_lower().replace(" ", "_"):
		"unknown":
			return Status.UNKNOWN
		"first_contact":
			return Status.FIRST_CONTACT
		"peaceful":
			return Status.PEACEFUL
		"trading":
			return Status.TRADING
		"tense":
			return Status.TENSE
		"hostile":
			return Status.HOSTILE
		"subjugated":
			return Status.SUBJUGATED
		"integrated":
			return Status.INTEGRATED
		"extinct":
			return Status.EXTINCT
		_:
			return Status.UNKNOWN


## Returns the number of status types.
## @return: Count of Status enum values.
static func status_count() -> int:
	return 9


## Converts this relation to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"native_population_id": native_population_id,
		"status": status as int,
		"first_contact_year": first_contact_year,
		"relation_score": relation_score,
		"has_treaty": has_treaty,
		"treaty_year": treaty_year,
		"trade_level": trade_level,
		"cultural_exchange": cultural_exchange,
		"conflict_intensity": conflict_intensity,
		"territory_taken": territory_taken,
		"relationship_events": relationship_events.duplicate(),
	}


## Creates a NativeRelation from a dictionary.
## @param data: The dictionary to parse.
## @return: A new NativeRelation instance.
static func from_dict(data: Dictionary) -> NativeRelation:
	var relation: NativeRelation = NativeRelation.new()

	relation.native_population_id = data.get("native_population_id", "") as String

	var status_val: Variant = data.get("status", 0)
	if status_val is String:
		status_val = int(status_val)
	relation.status = status_val as Status

	relation.first_contact_year = data.get("first_contact_year", 0) as int
	relation.relation_score = clampi(data.get("relation_score", 0) as int, -100, 100)
	relation.has_treaty = data.get("has_treaty", false) as bool
	relation.treaty_year = data.get("treaty_year", 0) as int
	relation.trade_level = clampf(data.get("trade_level", 0.0) as float, 0.0, 1.0)
	relation.cultural_exchange = clampf(data.get("cultural_exchange", 0.0) as float, 0.0, 1.0)
	relation.conflict_intensity = clampf(data.get("conflict_intensity", 0.0) as float, 0.0, 1.0)
	relation.territory_taken = clampf(data.get("territory_taken", 0.0) as float, 0.0, 1.0)

	var events: Array = data.get("relationship_events", []) as Array
	relation.relationship_events = []
	for event in events:
		relation.relationship_events.append(event as String)

	return relation
