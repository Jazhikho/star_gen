## Station location type classification.
## Describes where in a system the station is positioned.
class_name StationType
extends RefCounted


## Station location types.
enum Type {
	ORBITAL, ## Orbiting a planet or moon
	DEEP_SPACE, ## Free-floating in system, not orbiting a body
	LAGRANGE, ## At a Lagrange point
	ASTEROID_BELT, ## Within or near an asteroid belt
}


## Converts a type to a display string.
## @param station_type: The type enum value.
## @return: Human-readable string.
static func to_string_name(station_type: Type) -> String:
	match station_type:
		Type.ORBITAL:
			return "Orbital"
		Type.DEEP_SPACE:
			return "Deep Space"
		Type.LAGRANGE:
			return "Lagrange Point"
		Type.ASTEROID_BELT:
			return "Asteroid Belt"
		_:
			return "Unknown"


## Converts a string to a type enum.
## @param name: The string name (case-insensitive).
## @return: The type, or Type.ORBITAL if not found.
static func from_string(name: String) -> Type:
	match name.to_lower().replace(" ", "_").strip_edges():
		"orbital":
			return Type.ORBITAL
		"deep_space":
			return Type.DEEP_SPACE
		"lagrange", "lagrange_point":
			return Type.LAGRANGE
		"asteroid_belt", "belt":
			return Type.ASTEROID_BELT
		_:
			return Type.ORBITAL


## Returns whether this type is associated with a specific body.
## @param station_type: The type enum value.
## @return: True if typically orbits or is near a body.
static func is_body_associated(station_type: Type) -> bool:
	return station_type == Type.ORBITAL


## Returns whether this type is free-floating in the system.
## @param station_type: The type enum value.
## @return: True if not orbiting a specific body.
static func is_free_floating(station_type: Type) -> bool:
	return station_type == Type.DEEP_SPACE or station_type == Type.LAGRANGE or station_type == Type.ASTEROID_BELT


## Returns the number of station types.
## @return: Count of Type enum values.
static func count() -> int:
	return 4
