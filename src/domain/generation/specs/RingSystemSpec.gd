## Specification for ring system generation.
## Ring systems are generated as part of planet generation.
class_name RingSystemSpec
extends BaseSpec

const RING_SYSTEM_SPEC_BRIDGE_CLASS: StringName = &"CSharpRingSystemSpecBridge"


## Complexity level (TRACE, SIMPLE, COMPLEX, or -1 for random).
var complexity: int

## Whether rings should be icy (true), rocky (false), or auto-detect (null).
var is_icy: Variant


## Creates a new RingSystemSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_complexity: Complexity level enum, or -1 for random.
## @param p_is_icy: Whether icy composition, or null for auto.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_complexity: int = -1,
	p_is_icy: Variant = null,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	super(p_generation_seed, p_name_hint, p_overrides)
	complexity = p_complexity
	is_icy = p_is_icy


## Creates a spec for a random ring system with the given seed.
## @param seed_value: The generation seed.
## @return: A new RingSystemSpec with all random values.
static func random(seed_value: int) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Random"):
		var payload: Variant = bridge.call("Random", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(seed_value)


## Creates a spec for a trace/faint ring system.
## @param seed_value: The generation seed.
## @return: A new RingSystemSpec configured for trace rings.
static func trace(seed_value: int) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Trace"):
		var payload: Variant = bridge.call("Trace", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(
		seed_value,
		RingComplexity.Level.TRACE
	)


## Creates a spec for a simple ring system (2-3 bands).
## @param seed_value: The generation seed.
## @return: A new RingSystemSpec configured for simple rings.
static func simple(seed_value: int) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Simple"):
		var payload: Variant = bridge.call("Simple", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(
		seed_value,
		RingComplexity.Level.SIMPLE
	)


## Creates a spec for a complex ring system (Saturn-like).
## @param seed_value: The generation seed.
## @return: A new RingSystemSpec configured for complex rings.
static func complex(seed_value: int) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Complex"):
		var payload: Variant = bridge.call("Complex", seed_value)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(
		seed_value,
		RingComplexity.Level.COMPLEX
	)


## Creates a spec for icy rings (outer solar system).
## @param seed_value: The generation seed.
## @param p_complexity: Complexity level, or -1 for random.
## @return: A new RingSystemSpec configured for icy rings.
static func icy(seed_value: int, p_complexity: int = -1) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Icy"):
		var payload: Variant = bridge.call("Icy", seed_value, p_complexity)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(
		seed_value,
		p_complexity,
		true
	)


## Creates a spec for rocky rings (inner solar system).
## @param seed_value: The generation seed.
## @param p_complexity: Complexity level, or -1 for random.
## @return: A new RingSystemSpec configured for rocky rings.
static func rocky(seed_value: int, p_complexity: int = -1) -> RingSystemSpec:
	var bridge: Object = _instantiate_bridge()
	if bridge != null and bridge.has_method("Rocky"):
		var payload: Variant = bridge.call("Rocky", seed_value, p_complexity)
		if payload is Dictionary:
			return _from_payload(payload as Dictionary)
	return RingSystemSpec.new(
		seed_value,
		p_complexity,
		false
	)


## Returns whether complexity is specified.
## @return: True if complexity is set.
func has_complexity() -> bool:
	return complexity >= 0


## Returns whether composition preference is specified.
## @return: True if icy/rocky choice is set.
func has_composition_preference() -> bool:
	return is_icy != null


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = _base_to_dict()
	data["spec_type"] = "ring_system"
	data["complexity"] = complexity
	data["is_icy"] = is_icy
	return _normalize_payload(data)


## Creates a RingSystemSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new RingSystemSpec instance.
static func from_dict(data: Dictionary) -> RingSystemSpec:
	return _from_payload(_normalize_payload(data))


## Creates a RingSystemSpec from a normalized payload.
## @param data: The normalized dictionary payload.
## @return: A new RingSystemSpec instance.
static func _from_payload(data: Dictionary) -> RingSystemSpec:
	return RingSystemSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("complexity", -1) as int,
		data.get("is_icy"),
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)


## Returns an optional C# bridge instance for spec helpers.
## @return: A bridge object when the C# bridge is registered, otherwise null.
static func _instantiate_bridge() -> Object:
	if not ClassDB.class_exists(RING_SYSTEM_SPEC_BRIDGE_CLASS):
		return null
	return ClassDB.instantiate(RING_SYSTEM_SPEC_BRIDGE_CLASS)


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
