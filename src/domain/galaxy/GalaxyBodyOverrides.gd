## Sparse map of edited celestial bodies inside a galaxy.
## Keyed by star_seed -> body_id -> serialized body Dictionary.
## Pure domain. No file IO; persistence goes through GalaxySaveData.
##
## Usage:
##   - EditDialog/ObjectViewer stores an edited body via set_override().
##   - GalaxySystemGenerator (or SystemCache) calls apply_to_bodies()
##     after regenerating a system from seed, swapping in edited bodies.
##
## The map is sparse: unedited systems cost nothing. Empty = no overrides.
class_name GalaxyBodyOverrides
extends RefCounted

const _celestial_serializer: GDScript = preload("res://src/domain/celestial/serialization/CelestialSerializer.gd")

## Script self-reference for from_dict instantiation.
const _SCRIPT: GDScript = preload("res://src/domain/galaxy/GalaxyBodyOverrides.gd")

## star_seed (int) -> { body_id (String) -> body_dict (Dictionary) }.
var _overrides: Dictionary


## Creates an empty override set.
func _init() -> void:
	_overrides = {}


## Stores a fully serialized body as an override.
## Overwrites any existing override for the same star_seed + body_id.
## @param star_seed: Seed of the star system this body belongs to.
## @param body: The edited CelestialBody (will be serialized to a dict).
func set_override(star_seed: int, body: CelestialBody) -> void:
	if body == null or body.id.is_empty():
		return
	if not _overrides.has(star_seed):
		_overrides[star_seed] = {}
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	bucket[body.id] = _celestial_serializer.to_dict(body)


## Stores a pre-serialized body dictionary as an override.
## @param star_seed: Star system seed.
## @param body_id: The body's id key.
## @param body_dict: Serialized body (CelestialSerializer.to_dict output).
func set_override_dict(star_seed: int, body_id: String, body_dict: Dictionary) -> void:
	if body_id.is_empty() or body_dict.is_empty():
		return
	if not _overrides.has(star_seed):
		_overrides[star_seed] = {}
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	bucket[body_id] = body_dict


## Removes an override. No-op if not present.
## @param star_seed: Star system seed.
## @param body_id: Body id to remove.
func clear_override(star_seed: int, body_id: String) -> void:
	if not _overrides.has(star_seed):
		return
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	bucket.erase(body_id)
	if bucket.is_empty():
		_overrides.erase(star_seed)


## Returns the serialized body dict if an override exists.
## @param star_seed: Star system seed.
## @param body_id: Body id to look up.
## @return: Body dict, or empty dict if no override.
func get_override_dict(star_seed: int, body_id: String) -> Dictionary:
	if not _overrides.has(star_seed):
		return {}
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	if not bucket.has(body_id):
		return {}
	return bucket[body_id] as Dictionary


## Returns the deserialized override body, or null if not present.
## @param star_seed: Star system seed.
## @param body_id: Body id to look up.
## @return: CelestialBody, or null.
func get_override_body(star_seed: int, body_id: String) -> CelestialBody:
	var d: Dictionary = get_override_dict(star_seed, body_id)
	if d.is_empty():
		return null
	return CelestialSerializer.from_dict(d)


## Returns whether any override exists for the given star system.
## @param star_seed: Star system seed.
## @return: True if at least one body in this system has an override.
func has_any_for(star_seed: int) -> bool:
	return _overrides.has(star_seed)


## Returns all body IDs that have overrides in the given system.
## @param star_seed: Star system seed.
## @return: Array of body_id strings.
func get_overridden_ids(star_seed: int) -> Array[String]:
	var ids: Array[String] = []
	if not _overrides.has(star_seed):
		return ids
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	for k: Variant in bucket.keys():
		ids.append(k as String)
	return ids


## Returns whether the whole override set is empty.
## @return: True if no overrides stored anywhere.
func is_empty() -> bool:
	return _overrides.is_empty()


## Returns the total number of overridden bodies across all systems.
## @return: Count.
func total_count() -> int:
	var n: int = 0
	for seed_key: Variant in _overrides.keys():
		var bucket: Dictionary = _overrides[seed_key] as Dictionary
		n += bucket.size()
	return n


## Applies overrides to a body array in place.
## Any body whose id has an override is replaced with the deserialized version.
## @param star_seed: Star system seed.
## @param bodies: Array of CelestialBody (mutated in place).
## @return: Number of bodies that were replaced.
func apply_to_bodies(star_seed: int, bodies: Array) -> int:
	if not has_any_for(star_seed):
		return 0
	var bucket: Dictionary = _overrides[star_seed] as Dictionary
	var replaced: int = 0
	for i: int in range(bodies.size()):
		var b: CelestialBody = bodies[i] as CelestialBody
		if b == null:
			continue
		if not bucket.has(b.id):
			continue
		var patched: CelestialBody = _celestial_serializer.from_dict(bucket[b.id] as Dictionary)
		if patched != null:
			bodies[i] = patched
			replaced += 1
	return replaced


## Serializes to a dictionary for persistence.
## JSON-safe: int keys become strings (handled on round-trip in from_dict).
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var out: Dictionary = {}
	for seed_key: Variant in _overrides.keys():
		var seed_str: String = str(seed_key as int)
		var bucket: Dictionary = _overrides[seed_key] as Dictionary
		out[seed_str] = bucket.duplicate(true)
	return out


## Deserializes from a dictionary.
## @param data: Dictionary from a prior to_dict() call.
## @return: New GalaxyBodyOverrides instance.
static func from_dict(data: Dictionary) -> GalaxyBodyOverrides:
	var result: GalaxyBodyOverrides = _SCRIPT.new() as GalaxyBodyOverrides
	for seed_key: Variant in data.keys():
		var seed_str: String = seed_key as String
		var seed_int: int = seed_str.to_int()
		var bucket_in: Dictionary = data[seed_key] as Dictionary
		var bucket_out: Dictionary = {}
		for body_id: Variant in bucket_in.keys():
			bucket_out[body_id as String] = (bucket_in[body_id] as Dictionary).duplicate(true)
		result._overrides[seed_int] = bucket_out
	return result
