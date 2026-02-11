## Represents a star system for jump lane calculations.
## Contains position, population, and tracking for bridge status.
class_name JumpLaneSystem
extends RefCounted


## Unique identifier for this system.
var id: String = ""

## 3D position in parsecs.
var position: Vector3 = Vector3.ZERO

## Population of the system. 0 means unpopulated.
var population: int = 0

## False population assigned when this system becomes a bridge.
## -1 means no false population assigned.
var false_population: int = -1

## Whether this system is a bridge (destination-only, no outbound connections).
var is_bridge: bool = false


## Creates a new JumpLaneSystem.
## @param p_id: Unique identifier.
## @param p_position: 3D position in parsecs.
## @param p_population: Population (0 = unpopulated).
func _init(p_id: String = "", p_position: Vector3 = Vector3.ZERO, p_population: int = 0) -> void:
	id = p_id
	position = p_position
	population = p_population


## Returns the effective population for sorting/connection purposes.
## Uses false_population if assigned, otherwise actual population.
## @return: Effective population value.
func get_effective_population() -> int:
	if false_population >= 0:
		return false_population
	return population


## Returns whether this system is populated (has population > 0).
## @return: True if populated.
func is_populated() -> bool:
	return population > 0


## Calculates the distance to another system in parsecs.
## @param other: The other system.
## @return: Distance in parsecs.
func distance_to(other: JumpLaneSystem) -> float:
	return position.distance_to(other.position)


## Marks this system as a bridge with the given false population.
## @param higher_population: Population of the higher-populated endpoint.
func make_bridge(higher_population: int) -> void:
	is_bridge = true
	false_population = higher_population - 10000
	if false_population < 0:
		false_population = 0


## Creates a dictionary representation for serialization.
## @return: Dictionary with system data.
func to_dict() -> Dictionary:
	return {
		"id": id,
		"position": {"x": position.x, "y": position.y, "z": position.z},
		"population": population,
		"false_population": false_population,
		"is_bridge": is_bridge
	}


## Creates a JumpLaneSystem from a dictionary.
## @param data: Dictionary with system data.
## @return: New JumpLaneSystem instance.
static func from_dict(data: Dictionary) -> RefCounted:
	var pos_data: Dictionary = data.get("position", {})
	var pos: Vector3 = Vector3(
		pos_data.get("x", 0.0),
		pos_data.get("y", 0.0),
		pos_data.get("z", 0.0)
	)
	var system: JumpLaneSystem = JumpLaneSystem.new(
		data.get("id", ""),
		pos,
		data.get("population", 0)
	)
	system.false_population = data.get("false_population", -1)
	system.is_bridge = data.get("is_bridge", false)
	return system
