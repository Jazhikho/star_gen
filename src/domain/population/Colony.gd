## A colony established on a planet by an external civilization.
## Tracks founding, demographics, government, native relations, and history.
class_name Colony
extends RefCounted

# Preload dependencies.
const _colony_type: GDScript = preload("res://src/domain/population/ColonyType.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _native_relation: GDScript = preload("res://src/domain/population/NativeRelation.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")


## Unique identifier for this colony.
var id: String = ""

## Display name of this colony.
var name: String = ""

## ID of the planet this colony is on.
var body_id: String = ""

## Type/purpose of the colony.
var colony_type: ColonyType.Type = ColonyType.Type.SETTLEMENT

## ID of the founding civilization (placeholder for future Civilization model).
## TODO: Replace with proper civilization reference when model exists.
var founding_civilization_id: String = ""

## Name of the founding civilization (for display).
var founding_civilization_name: String = ""

## Year the colony was founded (negative = past).
var founding_year: int = 0

## Current population count.
var population: int = 0

## Peak historical population.
var peak_population: int = 0

## Year of peak population.
var peak_population_year: int = 0

## Technology level (usually inherited from parent civilization).
var tech_level: TechnologyLevel.Level = TechnologyLevel.Level.SPACEFARING

## Government/political structure.
var government: Government = null

## Whether this colony is still active.
var is_active: bool = true

## Year of abandonment/collapse (if not active).
var abandonment_year: int = 0

## Reason for abandonment (if not active).
var abandonment_reason: String = ""

## Whether this colony has declared independence.
var is_independent: bool = false

## Year of independence (if independent).
var independence_year: int = 0

## Relations with native populations. Key = native_population_id.
var native_relations: Dictionary = {} # String -> NativeRelation

## Fraction of planet controlled (0-1).
var territorial_control: float = 0.0

## Primary industry/focus.
var primary_industry: String = ""

## Self-sufficiency level (0-1). 1.0 = fully self-sufficient.
var self_sufficiency: float = 0.0

## Historical timeline.
var history: PopulationHistory = null

## Optional metadata.
var metadata: Dictionary = {}


## Creates a new Colony with default values.
func _init() -> void:
	government = Government.new()
	history = PopulationHistory.new()


## Returns the age of this colony in years.
## @param current_year: The current year (default 0 = present).
## @return: Age in years.
func get_age(current_year: int = 0) -> int:
	if is_active:
		return current_year - founding_year
	else:
		return abandonment_year - founding_year


## Returns the current growth state.
## @return: "growing", "stable", "declining", or "abandoned".
func get_growth_state() -> String:
	if not is_active:
		return "abandoned"
	if population > peak_population * 0.95:
		return "growing"
	elif population > peak_population * 0.5:
		return "stable"
	else:
		return "declining"


## Returns the government regime type.
## @return: The current regime.
func get_regime() -> GovernmentType.Regime:
	if government:
		return government.regime
	return GovernmentType.Regime.CONSTITUTIONAL


## Returns whether this colony is politically stable.
## @return: True if government is stable.
func is_politically_stable() -> bool:
	if government:
		return government.is_stable()
	return true


## Returns whether this colony has any native relations.
## @return: True if there are native relations.
func has_native_relations() -> bool:
	return not native_relations.is_empty()


## Returns the relation with a specific native population.
## @param native_id: The native population ID.
## @return: The NativeRelation, or null if none.
func get_native_relation(native_id: String) -> NativeRelation:
	if native_relations.has(native_id):
		return native_relations[native_id] as NativeRelation
	return null


## Adds or updates a native relation.
## @param relation: The NativeRelation to add.
func set_native_relation(relation: NativeRelation) -> void:
	native_relations[relation.native_population_id] = relation


## Returns all native relations as an array.
## @return: Array of NativeRelation.
func get_all_native_relations() -> Array[NativeRelation]:
	var result: Array[NativeRelation] = []
	for relation in native_relations.values():
		result.append(relation as NativeRelation)
	return result


## Returns whether any native relations are hostile.
## @return: True if any relation is hostile.
func has_hostile_native_relations() -> bool:
	for relation in native_relations.values():
		var rel: NativeRelation = relation as NativeRelation
		if rel.is_hostile():
			return true
	return false


## Returns the overall native relation status.
## @return: "none", "peaceful", "mixed", or "hostile".
func get_overall_native_status() -> String:
	if native_relations.is_empty():
		return "none"

	var hostile_count: int = 0
	var peaceful_count: int = 0

	for relation in native_relations.values():
		var rel: NativeRelation = relation as NativeRelation
		if rel.is_hostile():
			hostile_count += 1
		elif rel.is_positive():
			peaceful_count += 1

	if hostile_count > 0 and peaceful_count > 0:
		return "mixed"
	elif hostile_count > 0:
		return "hostile"
	else:
		return "peaceful"


## Records the colony's abandonment.
## @param year: Year of abandonment.
## @param reason: Reason for abandonment.
func record_abandonment(year: int, reason: String) -> void:
	is_active = false
	abandonment_year = year
	abandonment_reason = reason
	population = 0


## Records the colony declaring independence.
## @param year: Year of independence.
func record_independence(year: int) -> void:
	is_independent = true
	independence_year = year


## Updates peak population if current exceeds it.
## @param current_year: The current year for recording.
func update_peak_population(current_year: int) -> void:
	if population > peak_population:
		peak_population = population
		peak_population_year = current_year


## Returns a summary of this colony.
## @return: Dictionary with key information.
func get_summary() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"colony_type": ColonyType.to_string_name(colony_type),
		"population": population,
		"tech_level": TechnologyLevel.to_string_name(tech_level),
		"regime": GovernmentType.to_string_name(get_regime()),
		"is_active": is_active,
		"is_independent": is_independent,
		"age": get_age(),
		"territorial_control": territorial_control,
		"native_status": get_overall_native_status(),
		"self_sufficiency": self_sufficiency,
	}


## Converts this colony to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var native_relations_data: Dictionary = {}
	for native_id in native_relations.keys():
		var relation: NativeRelation = native_relations[native_id] as NativeRelation
		native_relations_data[native_id] = relation.to_dict()

	var data: Dictionary = {
		"id": id,
		"name": name,
		"body_id": body_id,
		"colony_type": colony_type as int,
		"founding_civilization_id": founding_civilization_id,
		"founding_civilization_name": founding_civilization_name,
		"founding_year": founding_year,
		"population": population,
		"peak_population": peak_population,
		"peak_population_year": peak_population_year,
		"tech_level": tech_level as int,
		"is_active": is_active,
		"abandonment_year": abandonment_year,
		"abandonment_reason": abandonment_reason,
		"is_independent": is_independent,
		"independence_year": independence_year,
		"native_relations": native_relations_data,
		"territorial_control": territorial_control,
		"primary_industry": primary_industry,
		"self_sufficiency": self_sufficiency,
		"metadata": metadata.duplicate(),
	}

	if government:
		data["government"] = government.to_dict()

	if history:
		data["history"] = history.to_dict()

	return data


## Creates a Colony from a dictionary.
## @param data: The dictionary to parse.
## @return: A new Colony instance.
static func from_dict(data: Dictionary) -> Colony:
	var colony: Colony = Colony.new()

	colony.id = data.get("id", "") as String
	colony.name = data.get("name", "") as String
	colony.body_id = data.get("body_id", "") as String

	var type_val: Variant = data.get("colony_type", 0)
	if type_val is String:
		type_val = int(type_val)
	colony.colony_type = type_val as ColonyType.Type

	colony.founding_civilization_id = data.get("founding_civilization_id", "") as String
	colony.founding_civilization_name = data.get("founding_civilization_name", "") as String
	colony.founding_year = data.get("founding_year", 0) as int
	colony.population = data.get("population", 0) as int
	colony.peak_population = data.get("peak_population", 0) as int
	colony.peak_population_year = data.get("peak_population_year", 0) as int

	var tech_val: Variant = data.get("tech_level", 0)
	if tech_val is String:
		tech_val = int(tech_val)
	colony.tech_level = tech_val as TechnologyLevel.Level

	colony.is_active = data.get("is_active", true) as bool
	colony.abandonment_year = data.get("abandonment_year", 0) as int
	colony.abandonment_reason = data.get("abandonment_reason", "") as String
	colony.is_independent = data.get("is_independent", false) as bool
	colony.independence_year = data.get("independence_year", 0) as int
	colony.territorial_control = clampf(data.get("territorial_control", 0.0) as float, 0.0, 1.0)
	colony.primary_industry = data.get("primary_industry", "") as String
	colony.self_sufficiency = clampf(data.get("self_sufficiency", 0.0) as float, 0.0, 1.0)
	colony.metadata = (data.get("metadata", {}) as Dictionary).duplicate()

	# Parse native relations
	var relations_data: Dictionary = data.get("native_relations", {}) as Dictionary
	for native_id in relations_data.keys():
		var relation: NativeRelation = NativeRelation.from_dict(relations_data[native_id] as Dictionary)
		colony.native_relations[native_id] = relation

	if data.has("government"):
		colony.government = Government.from_dict(data["government"] as Dictionary)

	if data.has("history"):
		colony.history = PopulationHistory.from_dict(data["history"] as Dictionary)

	return colony
