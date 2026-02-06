## Specification for station generation.
## Controls how stations are generated for a system.
class_name StationSpec
extends RefCounted

# Preload dependencies.
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")


## Seed for deterministic generation (0 = use system seed or random).
var seed: int = 0

## Whether to generate stations at all.
var generate_stations: bool = true

## Override: force specific placement context (null = auto-detect).
var force_context: Variant = null # StationPlacementContext.Context or null

## Minimum number of stations to generate (0 = use rules).
var min_stations: int = 0

## Maximum number of stations to generate (0 = no limit).
var max_stations: int = 0

## Whether to allow utility (U-class) stations.
var allow_utility: bool = true

## Whether to allow outposts (O-class).
var allow_outposts: bool = true

## Whether to allow larger stations (B/A/S class).
var allow_large_stations: bool = true

## Whether to allow deep space stations.
var allow_deep_space: bool = true

## Whether to allow asteroid belt stations.
var allow_belt_stations: bool = true

## Minimum year for station establishment (negative = past).
var min_established_year: int = -500

## Maximum year for station establishment (usually 0 = present).
var max_established_year: int = 0

## Population density modifier (1.0 = normal).
var population_density: float = 1.0

## Chance for a station to be decommissioned (0-1).
var decommission_chance: float = 0.05

## Specific purposes to include (empty = use rules).
var required_purposes: Array[StationPurpose.Purpose] = []

## Purposes to exclude.
var excluded_purposes: Array[StationPurpose.Purpose] = []

## ID prefix for generated stations.
var id_prefix: String = "station"

## Founding civilization ID (for larger stations).
var founding_civilization_id: String = ""

## Founding civilization name.
var founding_civilization_name: String = ""


## Creates a default spec.
func _init() -> void:
	pass


## Creates a spec for minimal station generation.
## @return: A spec that generates few/no stations.
static func minimal() -> StationSpec:
	var spec: StationSpec = StationSpec.new()
	spec.generate_stations = true
	spec.allow_large_stations = false
	spec.max_stations = 2
	spec.population_density = 0.5
	return spec


## Creates a spec for standard station generation.
## @return: A balanced spec.
static func standard() -> StationSpec:
	var spec: StationSpec = StationSpec.new()
	return spec


## Creates a spec for dense station generation.
## @return: A spec that generates many stations.
static func dense() -> StationSpec:
	var spec: StationSpec = StationSpec.new()
	spec.population_density = 2.0
	spec.min_stations = 2
	return spec


## Creates a spec for a specific context.
## @param context: The placement context to force.
## @return: A spec with forced context.
static func for_context(context: StationPlacementContext.Context) -> StationSpec:
	var spec: StationSpec = StationSpec.new()
	spec.force_context = context
	return spec


## Returns whether a purpose is allowed by this spec.
## @param purpose: The purpose to check.
## @return: True if purpose is allowed.
func is_purpose_allowed(purpose: StationPurpose.Purpose) -> bool:
	if purpose in excluded_purposes:
		return false
	if required_purposes.is_empty():
		return true
	return purpose in required_purposes


## Returns whether a station class is allowed.
## @param station_class: The class to check.
## @return: True if class is allowed.
func is_class_allowed(station_class: StationClass.Class) -> bool:
	match station_class:
		StationClass.Class.U:
			return allow_utility
		StationClass.Class.O:
			return allow_outposts
		StationClass.Class.B, StationClass.Class.A, StationClass.Class.S:
			return allow_large_stations
		_:
			return true


## Validates the spec.
## @return: Array of error strings (empty if valid).
func validate() -> Array[String]:
	var errors: Array[String] = []

	if min_stations > max_stations and max_stations > 0:
		errors.append("min_stations (%d) cannot exceed max_stations (%d)" % [min_stations, max_stations])

	if population_density < 0.0:
		errors.append("population_density cannot be negative")

	if decommission_chance < 0.0 or decommission_chance > 1.0:
		errors.append("decommission_chance must be between 0 and 1")

	if min_established_year > max_established_year:
		errors.append("min_established_year cannot exceed max_established_year")

	return errors


## Returns whether the spec is valid.
## @return: True if valid.
func is_valid() -> bool:
	return validate().is_empty()


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var required_int: Array[int] = []
	for p in required_purposes:
		required_int.append(p as int)

	var excluded_int: Array[int] = []
	for p in excluded_purposes:
		excluded_int.append(p as int)

	var data: Dictionary = {
		"seed": seed,
		"generate_stations": generate_stations,
		"min_stations": min_stations,
		"max_stations": max_stations,
		"allow_utility": allow_utility,
		"allow_outposts": allow_outposts,
		"allow_large_stations": allow_large_stations,
		"allow_deep_space": allow_deep_space,
		"allow_belt_stations": allow_belt_stations,
		"min_established_year": min_established_year,
		"max_established_year": max_established_year,
		"population_density": population_density,
		"decommission_chance": decommission_chance,
		"required_purposes": required_int,
		"excluded_purposes": excluded_int,
		"id_prefix": id_prefix,
		"founding_civilization_id": founding_civilization_id,
		"founding_civilization_name": founding_civilization_name,
	}

	if force_context != null:
		data["force_context"] = force_context as int

	return data


## Creates a StationSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new StationSpec instance.
static func from_dict(data: Dictionary) -> StationSpec:
	var spec: StationSpec = StationSpec.new()

	spec.seed = data.get("seed", 0) as int
	spec.generate_stations = data.get("generate_stations", true) as bool
	spec.min_stations = data.get("min_stations", 0) as int
	spec.max_stations = data.get("max_stations", 0) as int
	spec.allow_utility = data.get("allow_utility", true) as bool
	spec.allow_outposts = data.get("allow_outposts", true) as bool
	spec.allow_large_stations = data.get("allow_large_stations", true) as bool
	spec.allow_deep_space = data.get("allow_deep_space", true) as bool
	spec.allow_belt_stations = data.get("allow_belt_stations", true) as bool
	spec.min_established_year = data.get("min_established_year", -500) as int
	spec.max_established_year = data.get("max_established_year", 0) as int
	spec.population_density = data.get("population_density", 1.0) as float
	spec.decommission_chance = data.get("decommission_chance", 0.05) as float
	spec.id_prefix = data.get("id_prefix", "station") as String
	spec.founding_civilization_id = data.get("founding_civilization_id", "") as String
	spec.founding_civilization_name = data.get("founding_civilization_name", "") as String

	if data.has("force_context"):
		spec.force_context = data["force_context"] as int

	var required_arr: Array = data.get("required_purposes", []) as Array
	spec.required_purposes = []
	for p in required_arr:
		spec.required_purposes.append(p as StationPurpose.Purpose)

	var excluded_arr: Array = data.get("excluded_purposes", []) as Array
	spec.excluded_purposes = []
	for p in excluded_arr:
		spec.excluded_purposes.append(p as StationPurpose.Purpose)

	return spec
