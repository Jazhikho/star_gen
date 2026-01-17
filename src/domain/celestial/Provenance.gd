## Tracks the origin and version of generated celestial data.
## Used for migration detection and reproducibility.
class_name Provenance
extends RefCounted


## The generation seed used to create this object.
var generation_seed: int

## The generator version that created this object.
var generator_version: String

## The schema version of the serialized format.
var schema_version: int

## Unix timestamp when the object was created.
var created_timestamp: int

## Optional snapshot of the spec used for generation.
var spec_snapshot: Dictionary


## Creates a new Provenance instance.
## @param p_generation_seed: The generation seed.
## @param p_generator_version: The generator version string.
## @param p_schema_version: The schema version number.
## @param p_created_timestamp: Unix timestamp of creation.
## @param p_spec_snapshot: Optional spec data used for generation.
func _init(
	p_generation_seed: int = 0,
	p_generator_version: String = "",
	p_schema_version: int = 0,
	p_created_timestamp: int = 0,
	p_spec_snapshot: Dictionary = {}
) -> void:
	generation_seed = p_generation_seed
	generator_version = p_generator_version
	schema_version = p_schema_version
	created_timestamp = p_created_timestamp
	spec_snapshot = p_spec_snapshot


## Creates a Provenance with current version info.
## @param p_generation_seed: The generation seed.
## @param p_spec_snapshot: Optional spec data.
## @return: A new Provenance instance.
static func create_current(p_generation_seed: int, p_spec_snapshot: Dictionary = {}) -> Provenance:
	var script: GDScript = load("res://src/domain/celestial/Provenance.gd") as GDScript
	return script.new(
		p_generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		p_spec_snapshot
	) as Provenance


## Converts this provenance to a dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	return {
		"generation_seed": generation_seed,
		"generator_version": generator_version,
		"schema_version": schema_version,
		"created_timestamp": created_timestamp,
		"spec_snapshot": spec_snapshot,
	}


## Creates a Provenance from a dictionary.
## @param data: The dictionary to parse.
## @return: A new Provenance instance, or null if invalid.
static func from_dict(data: Dictionary) -> Provenance:
	if data.is_empty():
		return null

	var script: GDScript = load("res://src/domain/celestial/Provenance.gd") as GDScript
	return script.new(
		data.get("generation_seed", 0) as int,
		data.get("generator_version", "") as String,
		data.get("schema_version", 0) as int,
		data.get("created_timestamp", 0) as int,
		data.get("spec_snapshot", {}) as Dictionary
	) as Provenance
