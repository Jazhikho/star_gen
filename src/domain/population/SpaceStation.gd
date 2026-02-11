## A space station that can range from small outpost to city-sized habitat.
## Supports full government model for larger populations (B/A/S class).
class_name SpaceStation
extends RefCounted

# Preload dependencies.
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")
const _government: GDScript = preload("res://src/domain/population/Government.gd")
const _government_type: GDScript = preload("res://src/domain/population/GovernmentType.gd")
const _population_history: GDScript = preload("res://src/domain/population/PopulationHistory.gd")


## Unique identifier for this station.
var id: String = ""

## Display name of this station.
var name: String = ""

## Station class (U/O/B/A/S based on population).
var station_class: StationClass.Class = StationClass.Class.O

## Location type (orbital, deep space, lagrange, asteroid belt).
var station_type: StationType.Type = StationType.Type.ORBITAL

## Primary purpose of this station.
var primary_purpose: StationPurpose.Purpose = StationPurpose.Purpose.TRADE

## Secondary purposes (if any).
var secondary_purposes: Array[StationPurpose.Purpose] = []

## Services offered by this station.
var services: Array[StationService.Service] = []

## Placement context (why this station exists here).
var placement_context: StationPlacementContext.Context = StationPlacementContext.Context.OTHER

## Current population count.
var population: int = 0

## Peak historical population.
var peak_population: int = 0

## Year of peak population.
var peak_population_year: int = 0

## Year the station was established (negative = past).
var established_year: int = 0

## ID of the body this station orbits (if orbital type).
var orbiting_body_id: String = ""

## ID of the system this station is in.
var system_id: String = ""

## Whether this station is currently operational.
var is_operational: bool = true

## Year of decommissioning (if not operational).
var decommissioned_year: int = 0

## Reason for decommissioning (if not operational).
var decommissioned_reason: String = ""

# --- Small station governance (U/O class) ---

## Authority type for small stations (U/O class).
var outpost_authority: OutpostAuthority.Type = OutpostAuthority.Type.INDEPENDENT

## ID of the parent organization (for U/O class).
var parent_organization_id: String = ""

## Name of the parent organization for display.
var parent_organization_name: String = ""

## Commander/manager title.
var commander_title: String = ""

## Commander/manager name.
var commander_name: String = ""

# --- Large station governance (B/A/S class) ---

## Government structure for larger stations (B/A/S class).
## Null for U/O class stations.
var government: Government = null

## Whether this station has declared independence from founding entity.
var is_independent: bool = false

## Year of independence (if independent).
var independence_year: int = 0

## Historical timeline for larger stations.
## Null for U/O class stations.
var history: PopulationHistory = null

# --- Founding information ---

## ID of the founding civilization/faction.
## TODO: Replace with proper civilization reference when model exists.
var founding_civilization_id: String = ""

## Name of the founding civilization for display.
var founding_civilization_name: String = ""

## Optional metadata for extensions.
var metadata: Dictionary = {}


## Creates a new SpaceStation with default values.
func _init() -> void:
	commander_title = OutpostAuthority.typical_commander_title(outpost_authority)


## Returns the age of this station in years.
## @param current_year: The current year (default 0 = present).
## @return: Age in years.
func get_age(current_year: int = 0) -> int:
	if is_operational:
		return current_year - established_year
	else:
		return decommissioned_year - established_year


## Updates station class based on current population.
## Also initializes/clears government and history as needed.
func update_class_from_population() -> void:
	var is_utility: bool = primary_purpose == StationPurpose.Purpose.UTILITY
	var new_class: StationClass.Class = StationClass.get_class_for_population(population, is_utility)

	var was_small: bool = StationClass.uses_outpost_government(station_class)
	var is_small: bool = StationClass.uses_outpost_government(new_class)

	station_class = new_class

	# Transition from small to large: initialize government and history
	if was_small and not is_small:
		if government == null:
			government = Government.new()
		if history == null:
			history = PopulationHistory.new()

	# Note: We don't clear government/history when shrinking,
	# as historical data should be preserved


## Returns whether this station uses outpost-style governance (U/O class).
## @return: True if small station governance.
func uses_outpost_government() -> bool:
	return StationClass.uses_outpost_government(station_class)


## Returns whether this station uses colony-style governance (B/A/S class).
## @return: True if large station governance.
func uses_colony_government() -> bool:
	return StationClass.uses_colony_government(station_class)


## Returns the current government regime (for B/A/S class).
## @return: The regime, or CONSTITUTIONAL as default for small stations.
func get_regime() -> GovernmentType.Regime:
	if government != null:
		return government.regime
	return GovernmentType.Regime.CONSTITUTIONAL


## Returns whether this station is politically stable.
## @return: True if stable (always true for small stations).
func is_politically_stable() -> bool:
	if government != null:
		return government.is_stable()
	return true


## Returns whether this station is associated with a specific body.
## @return: True if orbiting a body.
func is_body_associated() -> bool:
	return StationType.is_body_associated(station_type) and orbiting_body_id != ""


## Returns whether this station has a parent organization (for small stations).
## @return: True if has parent org.
func has_parent_organization() -> bool:
	return OutpostAuthority.has_parent_organization(outpost_authority) and parent_organization_id != ""


## Returns whether a specific service is offered.
## @param service: The service to check.
## @return: True if service is available.
func offers_service(service: StationService.Service) -> bool:
	return service in services


## Adds a service if not already present.
## @param service: The service to add.
func add_service(service: StationService.Service) -> void:
	if service not in services:
		services.append(service)


## Removes a service if present.
## @param service: The service to remove.
func remove_service(service: StationService.Service) -> void:
	var idx: int = services.find(service)
	if idx >= 0:
		services.remove_at(idx)


## Sets population and updates class accordingly.
## @param new_population: The new population value.
func set_population(new_population: int) -> void:
	population = maxi(0, new_population)
	update_class_from_population()


## Updates peak population if current exceeds it.
## @param current_year: The current year for recording.
func update_peak_population(current_year: int) -> void:
	if population > peak_population:
		peak_population = population
		peak_population_year = current_year


## Returns the current growth state.
## @return: "growing", "stable", "declining", or "abandoned".
func get_growth_state() -> String:
	if not is_operational:
		return "abandoned"
	if peak_population == 0:
		return "growing" if population > 0 else "stable"
	if population > peak_population * 0.95:
		return "growing"
	elif population > peak_population * 0.5:
		return "stable"
	else:
		return "declining"


## Records the station's decommissioning.
## @param year: Year of decommissioning.
## @param reason: Reason for decommissioning.
func record_decommissioning(year: int, reason: String) -> void:
	is_operational = false
	decommissioned_year = year
	decommissioned_reason = reason


## Records the station declaring independence.
## @param year: Year of independence.
func record_independence(year: int) -> void:
	is_independent = true
	independence_year = year


## Updates the commander title based on authority type.
func update_commander_title() -> void:
	commander_title = OutpostAuthority.typical_commander_title(outpost_authority)


## Returns a summary of this station.
## @return: Dictionary with key information.
func get_summary() -> Dictionary:
	var summary: Dictionary = {
		"id": id,
		"name": name,
		"class": StationClass.to_letter(station_class),
		"class_name": StationClass.to_string_name(station_class),
		"type": StationType.to_string_name(station_type),
		"purpose": StationPurpose.to_string_name(primary_purpose),
		"population": population,
		"is_operational": is_operational,
		"age": get_age(),
		"services_count": services.size(),
		"growth_state": get_growth_state(),
	}

	if uses_outpost_government():
		summary["authority"] = OutpostAuthority.to_string_name(outpost_authority)
	else:
		summary["regime"] = GovernmentType.to_string_name(get_regime())
		summary["is_independent"] = is_independent

	return summary


## Validates the station data.
## @return: Array of validation error strings (empty if valid).
func validate() -> Array[String]:
	var errors: Array[String] = []

	if id.is_empty():
		errors.append("Station ID is required")

	if population < 0:
		errors.append("Population cannot be negative")

	# If orbital, should have orbiting body
	if station_type == StationType.Type.ORBITAL and orbiting_body_id.is_empty():
		errors.append("Orbital station should specify orbiting_body_id")

	# Class should match population
	var expected_class: StationClass.Class = StationClass.get_class_for_population(
		population,
		primary_purpose == StationPurpose.Purpose.UTILITY
	)
	# Allow U/O mismatch (both are small), but B/A/S should match
	if StationClass.uses_colony_government(station_class) or StationClass.uses_colony_government(expected_class):
		if station_class != expected_class:
			errors.append("Station class %s does not match population %d (expected %s)" % [
				StationClass.to_letter(station_class),
				population,
				StationClass.to_letter(expected_class)
			])

	# Large stations should have government
	if uses_colony_government() and government == null:
		errors.append("B/A/S class station should have government")

	return errors


## Returns whether the station data is valid.
## @return: True if valid.
func is_valid() -> bool:
	return validate().is_empty()


## Converts this station to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var secondary_purposes_int: Array[int] = []
	for p in secondary_purposes:
		secondary_purposes_int.append(p as int)

	var services_int: Array[int] = []
	for s in services:
		services_int.append(s as int)

	var data: Dictionary = {
		"id": id,
		"name": name,
		"station_class": station_class as int,
		"station_type": station_type as int,
		"primary_purpose": primary_purpose as int,
		"secondary_purposes": secondary_purposes_int,
		"services": services_int,
		"placement_context": placement_context as int,
		"population": population,
		"peak_population": peak_population,
		"peak_population_year": peak_population_year,
		"established_year": established_year,
		"orbiting_body_id": orbiting_body_id,
		"system_id": system_id,
		"is_operational": is_operational,
		"decommissioned_year": decommissioned_year,
		"decommissioned_reason": decommissioned_reason,
		"outpost_authority": outpost_authority as int,
		"parent_organization_id": parent_organization_id,
		"parent_organization_name": parent_organization_name,
		"commander_title": commander_title,
		"commander_name": commander_name,
		"is_independent": is_independent,
		"independence_year": independence_year,
		"founding_civilization_id": founding_civilization_id,
		"founding_civilization_name": founding_civilization_name,
		"metadata": metadata.duplicate(),
	}

	if government != null:
		data["government"] = government.to_dict()

	if history != null:
		data["history"] = history.to_dict()

	return data


## Creates a SpaceStation from a dictionary.
## @param data: The dictionary to parse.
## @return: A new SpaceStation instance.
static func from_dict(data: Dictionary) -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()

	station.id = data.get("id", "") as String
	station.name = data.get("name", "") as String

	var class_val: Variant = data.get("station_class", StationClass.Class.O)
	if class_val is String:
		class_val = int(class_val)
	station.station_class = class_val as StationClass.Class

	var type_val: Variant = data.get("station_type", StationType.Type.ORBITAL)
	if type_val is String:
		type_val = int(type_val)
	station.station_type = type_val as StationType.Type

	var purpose_val: Variant = data.get("primary_purpose", StationPurpose.Purpose.TRADE)
	if purpose_val is String:
		purpose_val = int(purpose_val)
	station.primary_purpose = purpose_val as StationPurpose.Purpose

	var secondary_arr: Array = data.get("secondary_purposes", []) as Array
	station.secondary_purposes = []
	for p in secondary_arr:
		if p is String:
			p = int(p)
		station.secondary_purposes.append(p as StationPurpose.Purpose)

	var services_arr: Array = data.get("services", []) as Array
	station.services = []
	for s in services_arr:
		if s is String:
			s = int(s)
		station.services.append(s as StationService.Service)

	var context_val: Variant = data.get("placement_context", StationPlacementContext.Context.OTHER)
	if context_val is String:
		context_val = int(context_val)
	station.placement_context = context_val as StationPlacementContext.Context

	station.population = maxi(data.get("population", 0) as int, 0)
	station.peak_population = maxi(data.get("peak_population", 0) as int, 0)
	station.peak_population_year = data.get("peak_population_year", 0) as int
	station.established_year = data.get("established_year", 0) as int
	station.orbiting_body_id = data.get("orbiting_body_id", "") as String
	station.system_id = data.get("system_id", "") as String
	station.is_operational = data.get("is_operational", true) as bool
	station.decommissioned_year = data.get("decommissioned_year", 0) as int
	station.decommissioned_reason = data.get("decommissioned_reason", "") as String

	var auth_val: Variant = data.get("outpost_authority", OutpostAuthority.Type.INDEPENDENT)
	if auth_val is String:
		auth_val = int(auth_val)
	station.outpost_authority = auth_val as OutpostAuthority.Type

	station.parent_organization_id = data.get("parent_organization_id", "") as String
	station.parent_organization_name = data.get("parent_organization_name", "") as String
	station.commander_title = data.get("commander_title", "") as String
	station.commander_name = data.get("commander_name", "") as String
	station.is_independent = data.get("is_independent", false) as bool
	station.independence_year = data.get("independence_year", 0) as int
	station.founding_civilization_id = data.get("founding_civilization_id", "") as String
	station.founding_civilization_name = data.get("founding_civilization_name", "") as String
	station.metadata = (data.get("metadata", {}) as Dictionary).duplicate()

	if data.has("government"):
		station.government = Government.from_dict(data["government"] as Dictionary)

	if data.has("history"):
		station.history = PopulationHistory.from_dict(data["history"] as Dictionary)

	# Default commander title if not provided
	if station.commander_title.is_empty():
		station.update_commander_title()

	return station


## Creates an orbital station over a colony world.
## @param station_id: Unique ID.
## @param station_name: Display name.
## @param system: System ID.
## @param body_id: ID of the planet being orbited.
## @return: A new orbital station.
static func create_orbital(station_id: String, station_name: String, system: String, body_id: String) -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()
	station.id = station_id
	station.name = station_name
	station.system_id = system
	station.orbiting_body_id = body_id
	station.station_type = StationType.Type.ORBITAL
	station.placement_context = StationPlacementContext.Context.COLONY_WORLD
	station.primary_purpose = StationPurpose.Purpose.TRADE
	station.services = StationService.basic_utility_services()
	station.update_commander_title()
	return station


## Creates a deep space station (not orbiting a body).
## @param station_id: Unique ID.
## @param station_name: Display name.
## @param system: System ID.
## @return: A new deep space station.
static func create_deep_space(station_id: String, station_name: String, system: String) -> SpaceStation:
	var station: SpaceStation = SpaceStation.new()
	station.id = station_id
	station.name = station_name
	station.system_id = system
	station.station_type = StationType.Type.DEEP_SPACE
	station.placement_context = StationPlacementContext.Context.RESOURCE_SYSTEM
	station.primary_purpose = StationPurpose.Purpose.RESIDENTIAL
	station.services = StationService.basic_utility_services()
	station.update_commander_title()
	return station
