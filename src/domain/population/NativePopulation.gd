## A native/indigenous population that evolved or emerged on a planet.
## Tracks origin, demographics, development, government, and history.
class_name NativePopulation
extends RefCounted

# Preload dependencies.
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _technology_level: GDScript = preload("res://src/domain/population/TechnologyLevel.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")


## Unique identifier for this population.
var id: String = ""

## Display name of this population/civilization.
var name: String = ""

## ID of the planet this population is native to.
var body_id: String = ""

## Year this population emerged/first appeared (negative = past).
var origin_year: int = 0

## Current population count.
var population: int = 0

## Peak historical population.
var peak_population: int = 0

## Year of peak population.
var peak_population_year: int = 0

## Current technology level.
var tech_level: TechnologyLevel.Level = TechnologyLevel.Level.STONE_AGE

## Government/political structure.
var government: Government = null

## Whether this population is still extant.
var is_extant: bool = true

## Year of extinction/collapse (if not extant).
var extinction_year: int = 0

## Cause of extinction (if not extant).
var extinction_cause: String = ""

## Cultural traits (string identifiers).
var cultural_traits: Array[String] = []

## Primary biome/region occupied.
var primary_biome: String = ""

## Fraction of planet controlled (0-1).
var territorial_control: float = 0.0

## Historical timeline.
var history: PopulationHistory = null

## Optional metadata.
var metadata: Dictionary = {}


## Creates a new NativePopulation with default values.
func _init() -> void:
	government = Government.new()
	history = PopulationHistory.new()


## Returns the age of this population in years (from origin to present/extinction).
## @param current_year: The current year (default 0 = present).
## @return: Age in years.
func get_age(current_year: int = 0) -> int:
	if is_extant:
		return current_year - origin_year
	else:
		return extinction_year - origin_year


## Returns the current population growth state.
## @return: "growing", "stable", "declining", or "extinct".
func get_growth_state() -> String:
	if not is_extant:
		return "extinct"
	if population > peak_population * 0.95:
		return "growing"
	elif population > peak_population * 0.5:
		return "stable"
	else:
		return "declining"


## Returns whether this population has achieved spaceflight.
## @return: True if tech level allows spaceflight.
func can_spaceflight() -> bool:
	return TechnologyLevel.can_spaceflight(tech_level)


## Returns whether this population could have founded colonies.
## @return: True if tech level allows interstellar travel.
func can_colonize() -> bool:
	return TechnologyLevel.can_interstellar(tech_level)


## Returns the government regime type.
## @return: The current regime.
func get_regime() -> GovernmentType.Regime:
	if government != null:
		return government.regime
	return GovernmentType.Regime.TRIBAL


## Returns whether this population is in a stable political state.
## @return: True if government is stable.
func is_politically_stable() -> bool:
	if government != null:
		return government.is_stable()
	return true


## Records extinction of this population.
## @param year: Year of extinction.
## @param cause: Cause of extinction.
func record_extinction(year: int, cause: String) -> void:
	is_extant = false
	extinction_year = year
	extinction_cause = cause
	population = 0


## Updates peak population if current population exceeds it.
## @param current_year: The current year for recording.
func update_peak_population(current_year: int) -> void:
	if population > peak_population:
		peak_population = population
		peak_population_year = current_year


## Returns a summary of this population.
## @return: Dictionary with key information.
func get_summary() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"population": population,
		"tech_level": TechnologyLevel.to_string_name(tech_level),
		"regime": GovernmentType.to_string_name(get_regime()),
		"is_extant": is_extant,
		"age": get_age(),
		"territorial_control": territorial_control,
	}


## Converts this population to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = {
		"id": id,
		"name": name,
		"body_id": body_id,
		"origin_year": origin_year,
		"population": population,
		"peak_population": peak_population,
		"peak_population_year": peak_population_year,
		"tech_level": tech_level as int,
		"is_extant": is_extant,
		"extinction_year": extinction_year,
		"extinction_cause": extinction_cause,
		"cultural_traits": cultural_traits.duplicate(),
		"primary_biome": primary_biome,
		"territorial_control": territorial_control,
		"metadata": metadata.duplicate(),
	}

	if government != null:
		data["government"] = government.to_dict()

	if history != null:
		data["history"] = history.to_dict()

	return data


## Creates a NativePopulation from a dictionary.
## @param data: The dictionary to parse.
## @return: A new NativePopulation instance.
static func from_dict(data: Dictionary) -> NativePopulation:
	var script_ref: GDScript = load("res://src/domain/population/NativePopulation.gd") as GDScript
	var pop: NativePopulation = script_ref.new() as NativePopulation

	pop.id = data.get("id", "") as String
	pop.name = data.get("name", "") as String
	pop.body_id = data.get("body_id", "") as String
	pop.origin_year = data.get("origin_year", 0) as int
	pop.population = data.get("population", 0) as int
	pop.peak_population = data.get("peak_population", 0) as int
	pop.peak_population_year = data.get("peak_population_year", 0) as int

	var tech_val: Variant = data.get("tech_level", 0)
	if tech_val is String:
		tech_val = int(tech_val as String)
	pop.tech_level = tech_val as TechnologyLevel.Level

	pop.is_extant = data.get("is_extant", true) as bool
	pop.extinction_year = data.get("extinction_year", 0) as int
	pop.extinction_cause = data.get("extinction_cause", "") as String

	var traits: Array = data.get("cultural_traits", []) as Array
	pop.cultural_traits = []
	for t in traits:
		pop.cultural_traits.append(t as String)

	pop.primary_biome = data.get("primary_biome", "") as String
	pop.territorial_control = clampf(data.get("territorial_control", 0.0) as float, 0.0, 1.0)
	pop.metadata = (data.get("metadata", {}) as Dictionary).duplicate()

	if data.has("government"):
		pop.government = Government.from_dict(data["government"] as Dictionary)

	if data.has("history"):
		pop.history = PopulationHistory.from_dict(data["history"] as Dictionary)

	return pop
