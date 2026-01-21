## Handles serialization and deserialization of solar systems.
## Converts between SolarSystem objects and dictionaries.
class_name SystemSerializer
extends RefCounted

const _solar_system := preload("res://src/domain/system/SolarSystem.gd")
const _versions := preload("res://src/domain/constants/Versions.gd")


## Serializes a solar system to a dictionary.
## @param system: The solar system to serialize.
## @return: Dictionary representation suitable for JSON.
static func to_dict(system: SolarSystem) -> Dictionary:
	# Delegate to SolarSystem's own serialization method
	var data: Dictionary = system.to_dict()
	
	# Add schema and generator version metadata if not present
	if not data.has("schema_version"):
		data["schema_version"] = Versions.SCHEMA_VERSION
	
	if not data.has("generator_version"):
		data["generator_version"] = Versions.GENERATOR_VERSION
	
	if not data.has("type"):
		data["type"] = "solar_system"
	
	return data


## Deserializes a dictionary to a solar system.
## @param data: The dictionary to deserialize.
## @return: A new SolarSystem, or null if data is invalid.
static func from_dict(data: Dictionary) -> SolarSystem:
	if data.is_empty():
		return null
	
	# Check type (allow empty for backward compatibility)
	var data_type: String = data.get("type", "") as String
	if data_type != "solar_system" and data_type != "":
		return null
	
	# Delegate to SolarSystem's own deserialization method
	return SolarSystem.from_dict(data)


## Serializes a solar system to a JSON string.
## @param system: The solar system to serialize.
## @param pretty: If true, format with indentation.
## @return: JSON string representation.
static func to_json(system: SolarSystem, pretty: bool = true) -> String:
	var data: Dictionary = to_dict(system)
	if pretty:
		return JSON.stringify(data, "\t")
	return JSON.stringify(data)


## Deserializes a JSON string to a solar system.
## @param json_string: The JSON string to parse.
## @return: A new SolarSystem, or null if parsing fails.
static func from_json(json_string: String) -> SolarSystem:
	var json: JSON = JSON.new()
	var error: Error = json.parse(json_string)
	if error != OK:
		return null
	
	var data: Variant = json.data
	if not data is Dictionary:
		return null
	
	return from_dict(data as Dictionary)
