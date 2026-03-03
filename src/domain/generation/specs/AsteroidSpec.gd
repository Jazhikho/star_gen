## Specification for asteroid generation.
class_name AsteroidSpec
extends BaseSpec

const ASTEROID_SPEC_BRIDGE_CLASS: StringName = &"CSharpAsteroidSpecBridge"


## Compositional type (C, S, M, or -1 for random).
var asteroid_type: int

## Whether this is a large asteroid (Ceres-scale) vs typical.
var is_large: bool


## Creates a new AsteroidSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_asteroid_type: Asteroid type enum, or -1 for random.
## @param p_is_large: Whether this is a large/dwarf-planet-scale asteroid.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_asteroid_type: int = -1,
	p_is_large: bool = false,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	super(p_generation_seed, p_name_hint, p_overrides)
	asteroid_type = p_asteroid_type
	is_large = p_is_large


## Creates a spec for a random asteroid with the given seed.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec with all random values.
static func random(seed_value: int) -> AsteroidSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Random"):
		var payload: Variant = bridge.call("Random", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return AsteroidSpec.new(seed_value)


## Creates a spec for a carbonaceous asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for C-type.
static func carbonaceous(seed_value: int) -> AsteroidSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Carbonaceous"):
		var payload: Variant = bridge.call("Carbonaceous", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.C_TYPE,
		false
	)


## Creates a spec for a metallic asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for M-type.
static func metallic(seed_value: int) -> AsteroidSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Metallic"):
		var payload: Variant = bridge.call("Metallic", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.M_TYPE,
		false
	)


## Creates a spec for a stony asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for S-type.
static func stony(seed_value: int) -> AsteroidSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Stony"):
		var payload: Variant = bridge.call("Stony", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.S_TYPE,
		false
	)


## Creates a spec for a Ceres-like large asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for large body.
static func ceres_like(seed_value: int) -> AsteroidSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("CeresLike"):
		var payload: Variant = bridge.call("CeresLike", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.C_TYPE,
		true
	)


## Returns whether asteroid type is specified.
## @return: True if type is set.
func has_asteroid_type() -> bool:
	return asteroid_type >= 0


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = _base_to_dict()
	data["spec_type"] = "asteroid"
	data["asteroid_type"] = asteroid_type
	data["is_large"] = is_large
	return _normalize_payload(data)


## Creates an AsteroidSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new AsteroidSpec instance.
static func from_dict(data: Dictionary) -> AsteroidSpec:
	return _from_payload(_normalize_payload(data))


## Creates an AsteroidSpec from a normalized payload.
## @param data: The normalized dictionary payload.
## @return: A new AsteroidSpec instance.
static func _from_payload(data: Dictionary) -> AsteroidSpec:
	return AsteroidSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("asteroid_type", -1) as int,
		data.get("is_large", false) as bool,
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)


## Returns an optional C# bridge instance for spec helpers.
## @return: A bridge object when the C# bridge is registered, otherwise null.
static func _instantiate_bridge() -> Object:
	if not ClassDB.class_exists(ASTEROID_SPEC_BRIDGE_CLASS):
		return null
	return ClassDB.instantiate(ASTEROID_SPEC_BRIDGE_CLASS)


## Normalizes a spec payload through the C# bridge when available.
## @param data: The payload to normalize.
## @return: A normalized payload that preserves the current schema.
static func _normalize_payload(data: Dictionary) -> Dictionary:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Normalize"):
		var payload: Variant = bridge.call("Normalize", data)
		if payload is Dictionary:
			return payload as Dictionary
	return data
