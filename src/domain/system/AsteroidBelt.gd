## Defines an asteroid belt region within a solar system.
## Contains belt boundaries, estimated mass, and references to major bodies.
class_name AsteroidBelt
extends RefCounted

const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Belt composition types.
enum Composition {
	ROCKY,   ## Silicate-dominated (inner belts)
	ICY,     ## Ice-dominated (outer belts, Kuiper-like)
	MIXED,   ## Mixed composition (transitional regions)
	METALLIC ## Metal-rich (rare, near frost line)
}


## Unique identifier for this belt.
var id: String

## Display name of the belt.
var name: String

## ID of the orbit host this belt orbits.
var orbit_host_id: String

## Inner edge of the belt in meters.
var inner_radius_m: float

## Outer edge of the belt in meters.
var outer_radius_m: float

## Estimated total mass of the belt in kg.
var total_mass_kg: float

## Primary composition of belt objects.
var composition: Composition

## IDs of the major (largest) asteroids in this belt.
## Limited to top 10 by size.
var major_asteroid_ids: Array[String]


## Creates a new AsteroidBelt.
## @param p_id: Unique identifier.
## @param p_name: Display name.
func _init(
	p_id: String = "",
	p_name: String = ""
) -> void:
	id = p_id
	name = p_name
	orbit_host_id = ""
	inner_radius_m = 0.0
	outer_radius_m = 0.0
	total_mass_kg = 0.0
	composition = Composition.ROCKY
	major_asteroid_ids = []


## Returns the belt width in meters.
## @return: Width of belt.
func get_width_m() -> float:
	return maxf(0.0, outer_radius_m - inner_radius_m)


## Returns the belt width in AU.
## @return: Width of belt in AU.
func get_width_au() -> float:
	return get_width_m() / Units.AU_METERS


## Returns the center distance of the belt in meters.
## @return: Center distance.
func get_center_m() -> float:
	return (inner_radius_m + outer_radius_m) / 2.0


## Returns the center distance of the belt in AU.
## @return: Center distance in AU.
func get_center_au() -> float:
	return get_center_m() / Units.AU_METERS


## Returns the composition as a string.
## @return: Composition name.
func get_composition_string() -> String:
	match composition:
		Composition.ROCKY:
			return "Rocky"
		Composition.ICY:
			return "Icy"
		Composition.MIXED:
			return "Mixed"
		Composition.METALLIC:
			return "Metallic"
		_:
			return "Unknown"


## Returns the number of identified major asteroids.
## @return: Count of major asteroids.
func get_major_asteroid_count() -> int:
	return major_asteroid_ids.size()


## Converts composition enum to string for serialization.
## @param comp: Composition enum value.
## @return: String representation.
static func composition_to_string(comp: Composition) -> String:
	match comp:
		Composition.ROCKY:
			return "rocky"
		Composition.ICY:
			return "icy"
		Composition.MIXED:
			return "mixed"
		Composition.METALLIC:
			return "metallic"
		_:
			return "rocky"


## Parses string to composition enum.
## @param comp_str: String to parse.
## @return: Composition enum value.
static func string_to_composition(comp_str: String) -> Composition:
	match comp_str.to_lower():
		"rocky":
			return Composition.ROCKY
		"icy":
			return Composition.ICY
		"mixed":
			return Composition.MIXED
		"metallic":
			return Composition.METALLIC
		_:
			return Composition.ROCKY


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"id": id,
		"name": name,
		"orbit_host_id": orbit_host_id,
		"inner_radius_m": inner_radius_m,
		"outer_radius_m": outer_radius_m,
		"total_mass_kg": total_mass_kg,
		"composition": composition_to_string(composition),
		"major_asteroid_ids": major_asteroid_ids.duplicate(),
	}


## Creates an AsteroidBelt from a dictionary.
## @param data: Dictionary to parse.
## @return: A new AsteroidBelt.
static func from_dict(data: Dictionary) -> AsteroidBelt:
	var belt: AsteroidBelt = AsteroidBelt.new(
		data.get("id", "") as String,
		data.get("name", "") as String
	)
	belt.orbit_host_id = data.get("orbit_host_id", "") as String
	belt.inner_radius_m = data.get("inner_radius_m", 0.0) as float
	belt.outer_radius_m = data.get("outer_radius_m", 0.0) as float
	belt.total_mass_kg = data.get("total_mass_kg", 0.0) as float
	belt.composition = string_to_composition(data.get("composition", "rocky") as String)
	
	var ids: Array = data.get("major_asteroid_ids", []) as Array
	for id_val in ids:
		belt.major_asteroid_ids.append(id_val as String)
	
	return belt
