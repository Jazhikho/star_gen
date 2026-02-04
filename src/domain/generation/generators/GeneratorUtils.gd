## Shared utility functions for all generators.
## Provides common operations like ID generation and provenance creation.
class_name GeneratorUtils
extends RefCounted

const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _provenance: GDScript = preload("res://src/domain/celestial/Provenance.gd")
const _versions: GDScript = preload("res://src/domain/constants/Versions.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _base_spec: GDScript = preload("res://src/domain/generation/specs/BaseSpec.gd")


## Generates a unique ID for a celestial body.
## @param body_type: The type of body (star, planet, moon, asteroid).
## @param rng: The random number generator.
## @return: A unique ID string.
static func generate_id(body_type: String, rng: SeededRng) -> String:
	var random_part: int = rng.randi() % 1000000
	return "%s_%06d" % [body_type, random_part]


## Creates a Provenance object from a spec and optional context.
## @param spec: The generation specification (BaseSpec or subclass).
## @param context: Optional parent context (for planets, moons, asteroids).
## @return: A new Provenance instance.
static func create_provenance(spec: BaseSpec, context: ParentContext = null) -> Provenance:
	var spec_dict: Dictionary = spec.to_dict()
	if context != null:
		spec_dict["context"] = context.to_dict()
	
	return Provenance.new(
		spec.generation_seed,
		Versions.GENERATOR_VERSION,
		Versions.SCHEMA_VERSION,
		int(Time.get_unix_time_from_system()),
		spec_dict
	)
