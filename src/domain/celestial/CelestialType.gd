## Enumeration of celestial body types.
## Used to determine which components are valid/required for a body.
class_name CelestialType
extends RefCounted


## The type of celestial body.
enum Type {
	STAR,
	PLANET,
	MOON,
	ASTEROID,
}


## Returns the string name of a celestial type.
## @param type: The type enum value.
## @return: Human-readable type name.
static func type_to_string(type: Type) -> String:
	match type:
		Type.STAR:
			return "Star"
		Type.PLANET:
			return "Planet"
		Type.MOON:
			return "Moon"
		Type.ASTEROID:
			return "Asteroid"
		_:
			return "Unknown"


## Parses a string to a celestial type.
## @param type_str: The string to parse.
## @return: The corresponding type, or -1 if invalid.
static func string_to_type(type_str: String) -> int:
	match type_str.to_lower():
		"star":
			return Type.STAR
		"planet":
			return Type.PLANET
		"moon":
			return Type.MOON
		"asteroid":
			return Type.ASTEROID
		_:
			return -1
