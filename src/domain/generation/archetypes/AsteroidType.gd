## Asteroid compositional types based on spectral classification.
class_name AsteroidType
extends RefCounted


## Asteroid type enumeration.
enum Type {
	C_TYPE, ## Carbonaceous, dark, primitive composition
	S_TYPE, ## Silicaceous, stony, moderate albedo
	M_TYPE, ## Metallic, high albedo, iron-nickel
}


## Returns the string name of an asteroid type.
## @param asteroid_type: The type enum value.
## @return: Human-readable type name.
static func to_string_name(asteroid_type: Type) -> String:
	match asteroid_type:
		Type.C_TYPE:
			return "C-Type (Carbonaceous)"
		Type.S_TYPE:
			return "S-Type (Silicaceous)"
		Type.M_TYPE:
			return "M-Type (Metallic)"
		_:
			return "Unknown"


## Returns the short designation.
## @param asteroid_type: The type enum value.
## @return: Short designation like "C", "S", "M".
static func to_short_name(asteroid_type: Type) -> String:
	match asteroid_type:
		Type.C_TYPE:
			return "C"
		Type.S_TYPE:
			return "S"
		Type.M_TYPE:
			return "M"
		_:
			return "?"


## Parses a string to an asteroid type.
## @param type_str: The string to parse.
## @return: The corresponding type, or -1 if invalid.
static func from_string(type_str: String) -> int:
	var normalized: String = type_str.to_lower().replace("-", "_").replace(" ", "_")
	match normalized:
		"c", "c_type", "carbonaceous":
			return Type.C_TYPE
		"s", "s_type", "silicaceous", "stony":
			return Type.S_TYPE
		"m", "m_type", "metallic":
			return Type.M_TYPE
		_:
			return -1


## Returns typical albedo for an asteroid type.
## @param asteroid_type: The type to query.
## @return: Typical albedo value.
static func get_typical_albedo(asteroid_type: Type) -> float:
	match asteroid_type:
		Type.C_TYPE:
			return 0.05
		Type.S_TYPE:
			return 0.20
		Type.M_TYPE:
			return 0.15
		_:
			return 0.10


## Returns the number of types.
## @return: Total count of asteroid types.
static func count() -> int:
	return 3
