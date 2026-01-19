## Shared utility functions for all generators.
## Provides common operations like ID generation.
class_name GeneratorUtils
extends RefCounted

const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")


## Generates a unique ID for a celestial body.
## @param body_type: The type of body (star, planet, moon, asteroid).
## @param rng: The random number generator.
## @return: A unique ID string.
static func generate_id(body_type: String, rng: SeededRng) -> String:
	var random_part: int = rng.randi() % 1000000
	return "%s_%06d" % [body_type, random_part]
