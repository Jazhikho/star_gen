## Represents a candidate orbital position within an orbit host's stable zone.
## Used during system generation to determine where planets can form.
class_name OrbitSlot
extends RefCounted

const _units := preload("res://src/domain/math/Units.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")


## Unique identifier for this slot.
var id: String

## ID of the orbit host this slot belongs to.
var orbit_host_id: String

## Semi-major axis (orbital distance) in meters.
var semi_major_axis_m: float

## Suggested eccentricity for this slot (can be overridden during planet generation).
var suggested_eccentricity: float

## Orbital zone classification.
var zone: OrbitZone.Zone

## Whether this slot is dynamically stable.
var is_stable: bool

## Probability (0-1) that this slot will be filled with a planet.
var fill_probability: float

## Whether this slot has been filled with a planet.
var is_filled: bool

## ID of the planet occupying this slot (empty if not filled).
var planet_id: String


## Creates a new OrbitSlot.
## @param p_id: Unique identifier.
## @param p_orbit_host_id: ID of the orbit host.
## @param p_semi_major_axis_m: Orbital distance in meters.
func _init(
	p_id: String = "",
	p_orbit_host_id: String = "",
	p_semi_major_axis_m: float = 0.0
) -> void:
	id = p_id
	orbit_host_id = p_orbit_host_id
	semi_major_axis_m = p_semi_major_axis_m
	suggested_eccentricity = 0.0
	zone = OrbitZone.Zone.TEMPERATE
	is_stable = true
	fill_probability = 0.5
	is_filled = false
	planet_id = ""


## Returns the semi-major axis in AU.
## @return: Distance in AU.
func get_semi_major_axis_au() -> float:
	return semi_major_axis_m / Units.AU_METERS


## Returns the zone as a string.
## @return: Zone name.
func get_zone_string() -> String:
	return OrbitZone.to_string_name(zone)


## Marks this slot as filled by a planet.
## @param p_planet_id: ID of the planet.
func fill_with_planet(p_planet_id: String) -> void:
	is_filled = true
	planet_id = p_planet_id


## Clears the planet from this slot.
func clear_planet() -> void:
	is_filled = false
	planet_id = ""


## Returns whether this slot is available (stable and unfilled).
## @return: True if available for planet placement.
func is_available() -> bool:
	return is_stable and not is_filled


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"id": id,
		"orbit_host_id": orbit_host_id,
		"semi_major_axis_m": semi_major_axis_m,
		"suggested_eccentricity": suggested_eccentricity,
		"zone": OrbitZone.to_string_name(zone).to_lower(),
		"is_stable": is_stable,
		"fill_probability": fill_probability,
		"is_filled": is_filled,
		"planet_id": planet_id,
	}


## Creates an OrbitSlot from a dictionary.
## @param data: Dictionary to parse.
## @return: A new OrbitSlot.
static func from_dict(data: Dictionary) -> OrbitSlot:
	var slot: OrbitSlot = OrbitSlot.new(
		data.get("id", "") as String,
		data.get("orbit_host_id", "") as String,
		data.get("semi_major_axis_m", 0.0) as float
	)
	
	slot.suggested_eccentricity = data.get("suggested_eccentricity", 0.0) as float
	
	var zone_str: String = data.get("zone", "temperate") as String
	var zone_int: int = OrbitZone.from_string(zone_str)
	slot.zone = (zone_int as OrbitZone.Zone) if zone_int >= 0 else OrbitZone.Zone.TEMPERATE
	
	slot.is_stable = data.get("is_stable", true) as bool
	slot.fill_probability = data.get("fill_probability", 0.5) as float
	slot.is_filled = data.get("is_filled", false) as bool
	slot.planet_id = data.get("planet_id", "") as String
	
	return slot
