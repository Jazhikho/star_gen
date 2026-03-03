## Handles serialization and deserialization of celestial bodies.
## Converts between CelestialBody objects and dictionaries.
class_name CelestialSerializer
extends RefCounted

const CELESTIAL_SERIALIZATION_BRIDGE_CLASS: StringName = &"CSharpCelestialSerializationBridge"


## Serializes a celestial body to a dictionary.
## @param body: The celestial body to serialize.
## @return: Dictionary representation suitable for JSON.
static func to_dict(body: CelestialBody) -> Dictionary:
	var data: Dictionary = _to_dict_native(body)
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("NormalizeBodyData"):
		var bridged_data: Variant = bridge.call("NormalizeBodyData", data)
		if bridged_data is Dictionary:
			var normalized_data: Dictionary = bridged_data as Dictionary
			if not normalized_data.is_empty():
				return normalized_data
	return data


## Serializes a celestial body to a dictionary without C# bridge delegation.
## @param body: The celestial body to serialize.
## @return: Dictionary representation suitable for JSON.
static func _to_dict_native(body: CelestialBody) -> Dictionary:
	var data: Dictionary = {
		"schema_version": Versions.get_schema_version(),
		"id": body.id,
		"name": body.name,
		"type": CelestialType.type_to_string(body.type),
		"physical": body.physical.to_dict(),
	}

	if body.has_orbital():
		data["orbital"] = body.orbital.to_dict()
	
	if body.has_stellar():
		data["stellar"] = body.stellar.to_dict()
	
	if body.has_surface():
		data["surface"] = body.surface.to_dict()
	
	if body.has_atmosphere():
		data["atmosphere"] = body.atmosphere.to_dict()
	
	if body.has_ring_system():
		data["ring_system"] = body.ring_system.to_dict()
	
	if body.has_population_data():
		data["population_data"] = body.population_data.to_dict()
	
	if body.provenance != null:
		data["provenance"] = body.provenance.to_dict()

	return data


## Deserializes a dictionary to a celestial body.
## @param data: The dictionary to deserialize.
## @return: A new CelestialBody, or null if data is invalid.
static func from_dict(data: Dictionary) -> CelestialBody:
	if data.is_empty():
		return null

	var source_data: Dictionary = data
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("NormalizeBodyData"):
		var bridged_data: Variant = bridge.call("NormalizeBodyData", data)
		if bridged_data is Dictionary:
			var normalized_data: Dictionary = bridged_data as Dictionary
			if not normalized_data.is_empty():
				source_data = normalized_data

	return _from_dict_native(source_data)


## Deserializes a dictionary to a celestial body without C# bridge delegation.
## @param data: The dictionary to deserialize.
## @return: A new CelestialBody, or null if data is invalid.
static func _from_dict_native(data: Dictionary) -> CelestialBody:
	if data.is_empty():
		return null
	
	var type_int: int = CelestialType.string_to_type(data.get("type", "planet"))
	if type_int < 0:
		type_int = CelestialType.Type.PLANET

	var physical_data: Dictionary = data.get("physical", {})
	var physical: PhysicalProps = PhysicalProps.from_dict(physical_data)

	var provenance: Provenance = null
	if data.has("provenance"):
		provenance = Provenance.from_dict(data["provenance"])

	var body: CelestialBody = CelestialBody.new(
		data.get("id", "") as String,
		data.get("name", "") as String,
		type_int as CelestialType.Type,
		physical,
		provenance
	) as CelestialBody

	if data.has("orbital"):
		body.orbital = OrbitalProps.from_dict(data["orbital"])
	
	if data.has("stellar"):
		body.stellar = StellarProps.from_dict(data["stellar"])
	
	if data.has("surface"):
		body.surface = SurfaceProps.from_dict(data["surface"])
	
	if data.has("atmosphere"):
		body.atmosphere = AtmosphereProps.from_dict(data["atmosphere"])
	
	if data.has("ring_system"):
		body.ring_system = RingSystemProps.from_dict(data["ring_system"])
	
	if data.has("population_data"):
		body.population_data = PlanetPopulationData.from_dict(data["population_data"])
	
	return body


## Serializes a celestial body to a JSON string.
## @param body: The celestial body to serialize.
## @param pretty: If true, format with indentation.
## @return: JSON string representation.
static func to_json(body: CelestialBody, pretty: bool = true) -> String:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("ToJsonFromBodyData"):
		var bridged_json: Variant = bridge.call("ToJsonFromBodyData", _to_dict_native(body), pretty)
		if bridged_json is String:
			var bridged_json_string: String = bridged_json as String
			if not bridged_json_string.is_empty():
				return bridged_json_string

	var data: Dictionary = _to_dict_native(body)
	if pretty:
		return JSON.stringify(data, "\t")
	return JSON.stringify(data)


## Deserializes a JSON string to a celestial body.
## @param json_string: The JSON string to parse.
## @return: A new CelestialBody, or null if parsing fails.
static func from_json(json_string: String) -> CelestialBody:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("ParseJsonToBodyData"):
		var bridged_data: Variant = bridge.call("ParseJsonToBodyData", json_string)
		if bridged_data is Dictionary:
			var parsed_data: Dictionary = bridged_data as Dictionary
			if not parsed_data.is_empty():
				return _from_dict_native(parsed_data)

	var json: JSON = JSON.new()
	var error: Error = json.parse(json_string)
	if error != OK:
		return null

	var data: Variant = json.data
	if not data is Dictionary:
		return null

	return _from_dict_native(data as Dictionary)


## Creates the optional C# serializer bridge if it is registered.
## @return: Instantiated bridge object, or null if unavailable.
static func _instantiate_bridge() -> Object:
	if not ClassDB.class_exists(CELESTIAL_SERIALIZATION_BRIDGE_CLASS):
		return null
	return ClassDB.instantiate(CELESTIAL_SERIALIZATION_BRIDGE_CLASS)
