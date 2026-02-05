## Represents a jump lane connection between two systems.
## Tracks the connection type (green/yellow/orange) based on distance and bridging.
class_name JumpLaneConnection
extends RefCounted


## Connection types based on distance and bridging rules.
enum ConnectionType {
	GREEN, ## Direct connection within 3-5 parsecs
	YELLOW, ## Connection via bridge system
	ORANGE ## Direct connection at 7 parsecs (no bridge available)
}


## ID of the source system (lower population).
var source_id: String = ""

## ID of the destination system (higher population).
var destination_id: String = ""

## Type of connection.
var connection_type: ConnectionType = ConnectionType.GREEN

## Distance between the systems in parsecs.
var distance_pc: float = 0.0


## Creates a new JumpLaneConnection.
## @param p_source_id: Source system ID.
## @param p_destination_id: Destination system ID.
## @param p_type: Connection type.
## @param p_distance: Distance in parsecs.
func _init(
	p_source_id: String = "",
	p_destination_id: String = "",
	p_type: ConnectionType = ConnectionType.GREEN,
	p_distance: float = 0.0
) -> void:
	source_id = p_source_id
	destination_id = p_destination_id
	connection_type = p_type
	distance_pc = p_distance


## Returns the color for this connection type.
## @return: Color for rendering.
func get_color() -> Color:
	match connection_type:
		ConnectionType.GREEN:
			return Color.GREEN
		ConnectionType.YELLOW:
			return Color.YELLOW
		ConnectionType.ORANGE:
			return Color.ORANGE
	return Color.WHITE


## Returns a human-readable name for the connection type.
## @return: Type name string.
func get_type_name() -> String:
	match connection_type:
		ConnectionType.GREEN:
			return "Direct (3-5 pc)"
		ConnectionType.YELLOW:
			return "Bridged"
		ConnectionType.ORANGE:
			return "Direct (7 pc)"
	return "Unknown"


## Creates a dictionary representation for serialization.
## @return: Dictionary with connection data.
func to_dict() -> Dictionary:
	return {
		"source_id": source_id,
		"destination_id": destination_id,
		"connection_type": connection_type,
		"distance_pc": distance_pc
	}


## Creates a JumpLaneConnection from a dictionary.
## @param data: Dictionary with connection data.
## @return: New JumpLaneConnection instance.
static func from_dict(data: Dictionary) -> RefCounted:
	return JumpLaneConnection.new(
		data.get("source_id", ""),
		data.get("destination_id", ""),
		data.get("connection_type", ConnectionType.GREEN) as ConnectionType,
		data.get("distance_pc", 0.0)
	)
