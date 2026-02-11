## A small orbital or deep-space habitat with population ≤10,000.
## Purpose-driven installations like mining stations, research outposts, or waypoints.
class_name Outpost
extends RefCounted

# Preload dependencies.
const _station_class: GDScript = preload("res://src/domain/population/StationClass.gd")
const _station_type: GDScript = preload("res://src/domain/population/StationType.gd")
const _station_purpose: GDScript = preload("res://src/domain/population/StationPurpose.gd")
const _station_service: GDScript = preload("res://src/domain/population/StationService.gd")
const _station_placement_context: GDScript = preload("res://src/domain/population/StationPlacementContext.gd")
const _outpost_authority: GDScript = preload("res://src/domain/population/OutpostAuthority.gd")


## Maximum population for an outpost.
const MAX_POPULATION: int = 10000


## Unique identifier for this outpost.
var id: String = ""

## Display name of this outpost.
var name: String = ""

## Station class (U for Utility or O for Outpost).
var station_class: StationClass.Class = StationClass.Class.O

## Location type (orbital, deep space, lagrange, asteroid belt).
var station_type: StationType.Type = StationType.Type.ORBITAL

## Primary purpose of this outpost.
var primary_purpose: StationPurpose.Purpose = StationPurpose.Purpose.UTILITY

## Secondary purposes (if any).
var secondary_purposes: Array[StationPurpose.Purpose] = []

## Services offered by this outpost.
var services: Array[StationService.Service] = []

## Placement context (why this outpost exists here).
var placement_context: StationPlacementContext.Context = StationPlacementContext.Context.OTHER

## Authority/governance type.
var authority: OutpostAuthority.Type = OutpostAuthority.Type.INDEPENDENT

## ID of the parent organization (corporation, military, etc.) if applicable.
var parent_organization_id: String = ""

## Name of the parent organization for display.
var parent_organization_name: String = ""

## Current population count (0 to MAX_POPULATION).
var population: int = 0

## Year the outpost was established (negative = past).
var established_year: int = 0

## ID of the body this outpost orbits (if orbital type).
var orbiting_body_id: String = ""

## ID of the system this outpost is in.
var system_id: String = ""

## Whether this outpost is currently operational.
var is_operational: bool = true

## Year of decommissioning (if not operational).
var decommissioned_year: int = 0

## Reason for decommissioning (if not operational).
var decommissioned_reason: String = ""

## Commander/manager title (derived from authority type).
var commander_title: String = ""

## Commander/manager name (for narrative).
var commander_name: String = ""

## Optional metadata for extensions.
var metadata: Dictionary = {}


## Creates a new Outpost with default values.
func _init() -> void:
	commander_title = OutpostAuthority.typical_commander_title(authority)


## Returns the age of this outpost in years.
## @param current_year: The current year (default 0 = present).
## @return: Age in years.
func get_age(current_year: int = 0) -> int:
	if is_operational:
		return current_year - established_year
	else:
		return decommissioned_year - established_year


## Returns whether this is a utility-class station.
## @return: True if U-class.
func is_utility() -> bool:
	return station_class == StationClass.Class.U


## Returns whether this outpost is associated with a specific body.
## @return: True if orbiting a body.
func is_body_associated() -> bool:
	return StationType.is_body_associated(station_type) and orbiting_body_id != ""


## Returns whether this outpost has a parent organization.
## @return: True if has parent org.
func has_parent_organization() -> bool:
	return OutpostAuthority.has_parent_organization(authority) and parent_organization_id != ""


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


## Sets population, clamped to valid range.
## @param new_population: The new population value.
func set_population(new_population: int) -> void:
	population = clampi(new_population, 0, MAX_POPULATION)


## Records the outpost's decommissioning.
## @param year: Year of decommissioning.
## @param reason: Reason for decommissioning.
func record_decommissioning(year: int, reason: String) -> void:
	is_operational = false
	decommissioned_year = year
	decommissioned_reason = reason


## Updates the commander title based on authority type.
func update_commander_title() -> void:
	commander_title = OutpostAuthority.typical_commander_title(authority)


## Returns a summary of this outpost.
## @return: Dictionary with key information.
func get_summary() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"class": StationClass.to_letter(station_class),
		"type": StationType.to_string_name(station_type),
		"purpose": StationPurpose.to_string_name(primary_purpose),
		"authority": OutpostAuthority.to_string_name(authority),
		"population": population,
		"is_operational": is_operational,
		"age": get_age(),
		"services_count": services.size(),
	}


## Validates the outpost data.
## @return: Array of validation error strings (empty if valid).
func validate() -> Array[String]:
	var errors: Array[String] = []

	if id.is_empty():
		errors.append("Outpost ID is required")

	if population < 0:
		errors.append("Population cannot be negative")

	if population > MAX_POPULATION:
		errors.append("Population exceeds outpost maximum of %d" % MAX_POPULATION)

	# U and O classes only for outposts
	if station_class != StationClass.Class.U and station_class != StationClass.Class.O:
		errors.append("Outpost must be U or O class, not %s" % StationClass.to_letter(station_class))

	# If orbital, should have orbiting body
	if station_type == StationType.Type.ORBITAL and orbiting_body_id.is_empty():
		errors.append("Orbital outpost should specify orbiting_body_id")

	return errors


## Returns whether the outpost data is valid.
## @return: True if valid.
func is_valid() -> bool:
	return validate().is_empty()


## Converts this outpost to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var secondary_purposes_int: Array[int] = []
	for p in secondary_purposes:
		secondary_purposes_int.append(p as int)

	var services_int: Array[int] = []
	for s in services:
		services_int.append(s as int)

	return {
		"id": id,
		"name": name,
		"station_class": station_class as int,
		"station_type": station_type as int,
		"primary_purpose": primary_purpose as int,
		"secondary_purposes": secondary_purposes_int,
		"services": services_int,
		"placement_context": placement_context as int,
		"authority": authority as int,
		"parent_organization_id": parent_organization_id,
		"parent_organization_name": parent_organization_name,
		"population": population,
		"established_year": established_year,
		"orbiting_body_id": orbiting_body_id,
		"system_id": system_id,
		"is_operational": is_operational,
		"decommissioned_year": decommissioned_year,
		"decommissioned_reason": decommissioned_reason,
		"commander_title": commander_title,
		"commander_name": commander_name,
		"metadata": metadata.duplicate(),
	}


## Creates an Outpost from a dictionary.
## @param data: The dictionary to parse.
## @return: A new Outpost instance.
static func from_dict(data: Dictionary) -> Outpost:
	var outpost: Outpost = Outpost.new()

	outpost.id = data.get("id", "") as String
	outpost.name = data.get("name", "") as String

	var class_val: Variant = data.get("station_class", StationClass.Class.O)
	if class_val is String:
		class_val = int(class_val)
	outpost.station_class = class_val as StationClass.Class

	var type_val: Variant = data.get("station_type", StationType.Type.ORBITAL)
	if type_val is String:
		type_val = int(type_val)
	outpost.station_type = type_val as StationType.Type

	var purpose_val: Variant = data.get("primary_purpose", StationPurpose.Purpose.UTILITY)
	if purpose_val is String:
		purpose_val = int(purpose_val)
	outpost.primary_purpose = purpose_val as StationPurpose.Purpose

	var secondary_arr: Array = data.get("secondary_purposes", []) as Array
	outpost.secondary_purposes = []
	for p in secondary_arr:
		if p is String:
			p = int(p)
		outpost.secondary_purposes.append(p as StationPurpose.Purpose)

	var services_arr: Array = data.get("services", []) as Array
	outpost.services = []
	for s in services_arr:
		if s is String:
			s = int(s)
		outpost.services.append(s as StationService.Service)

	var context_val: Variant = data.get("placement_context", StationPlacementContext.Context.OTHER)
	if context_val is String:
		context_val = int(context_val)
	outpost.placement_context = context_val as StationPlacementContext.Context

	var auth_val: Variant = data.get("authority", OutpostAuthority.Type.INDEPENDENT)
	if auth_val is String:
		auth_val = int(auth_val)
	outpost.authority = auth_val as OutpostAuthority.Type

	outpost.parent_organization_id = data.get("parent_organization_id", "") as String
	outpost.parent_organization_name = data.get("parent_organization_name", "") as String
	outpost.population = clampi(data.get("population", 0) as int, 0, MAX_POPULATION)
	outpost.established_year = data.get("established_year", 0) as int
	outpost.orbiting_body_id = data.get("orbiting_body_id", "") as String
	outpost.system_id = data.get("system_id", "") as String
	outpost.is_operational = data.get("is_operational", true) as bool
	outpost.decommissioned_year = data.get("decommissioned_year", 0) as int
	outpost.decommissioned_reason = data.get("decommissioned_reason", "") as String
	outpost.commander_title = data.get("commander_title", "") as String
	outpost.commander_name = data.get("commander_name", "") as String
	outpost.metadata = (data.get("metadata", {}) as Dictionary).duplicate()

	# Default commander title if not provided
	if outpost.commander_title.is_empty():
		outpost.update_commander_title()

	return outpost


## Creates a basic utility outpost.
## @param outpost_id: Unique ID.
## @param outpost_name: Display name.
## @param system: System ID.
## @return: A new utility outpost.
static func create_utility(outpost_id: String, outpost_name: String, system: String) -> Outpost:
	var outpost: Outpost = Outpost.new()
	outpost.id = outpost_id
	outpost.name = outpost_name
	outpost.system_id = system
	outpost.station_class = StationClass.Class.U
	outpost.primary_purpose = StationPurpose.Purpose.UTILITY
	outpost.placement_context = StationPlacementContext.Context.BRIDGE_SYSTEM
	outpost.services = StationService.basic_utility_services()
	outpost.update_commander_title()
	return outpost


## Creates a basic mining outpost.
## @param outpost_id: Unique ID.
## @param outpost_name: Display name.
## @param system: System ID.
## @param body_id: ID of body being mined (asteroid, moon, etc.).
## @return: A new mining outpost.
static func create_mining(outpost_id: String, outpost_name: String, system: String, body_id: String) -> Outpost:
	var outpost: Outpost = Outpost.new()
	outpost.id = outpost_id
	outpost.name = outpost_name
	outpost.system_id = system
	outpost.station_class = StationClass.Class.O
	outpost.station_type = StationType.Type.ORBITAL
	outpost.orbiting_body_id = body_id
	outpost.primary_purpose = StationPurpose.Purpose.MINING
	outpost.placement_context = StationPlacementContext.Context.RESOURCE_SYSTEM
	outpost.authority = OutpostAuthority.Type.CORPORATE
	outpost.services = [StationService.Service.REFUEL, StationService.Service.STORAGE]
	outpost.update_commander_title()
	return outpost


## Creates a basic science outpost.
## @param outpost_id: Unique ID.
## @param outpost_name: Display name.
## @param system: System ID.
## @return: A new science outpost.
static func create_science(outpost_id: String, outpost_name: String, system: String) -> Outpost:
	var outpost: Outpost = Outpost.new()
	outpost.id = outpost_id
	outpost.name = outpost_name
	outpost.system_id = system
	outpost.station_class = StationClass.Class.O
	outpost.station_type = StationType.Type.DEEP_SPACE
	outpost.primary_purpose = StationPurpose.Purpose.SCIENCE
	outpost.placement_context = StationPlacementContext.Context.SCIENTIFIC
	outpost.authority = OutpostAuthority.Type.GOVERNMENT
	outpost.services = [StationService.Service.COMMUNICATIONS, StationService.Service.LODGING]
	outpost.update_commander_title()
	return outpost
