## Specification for asteroid generation.
class_name AsteroidSpec
extends BaseSpec

const _asteroid_type := preload("res://src/domain/generation/archetypes/AsteroidType.gd")


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
	return AsteroidSpec.new(seed_value)


## Creates a spec for a carbonaceous asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for C-type.
static func carbonaceous(seed_value: int) -> AsteroidSpec:
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.C_TYPE,
		false
	)


## Creates a spec for a metallic asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for M-type.
static func metallic(seed_value: int) -> AsteroidSpec:
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.M_TYPE,
		false
	)


## Creates a spec for a stony asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for S-type.
static func stony(seed_value: int) -> AsteroidSpec:
	return AsteroidSpec.new(
		seed_value,
		AsteroidType.Type.S_TYPE,
		false
	)


## Creates a spec for a Ceres-like large asteroid.
## @param seed_value: The generation seed.
## @return: A new AsteroidSpec configured for large body.
static func ceres_like(seed_value: int) -> AsteroidSpec:
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
	return data


## Creates an AsteroidSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new AsteroidSpec instance.
static func from_dict(data: Dictionary) -> AsteroidSpec:
	var spec: AsteroidSpec = AsteroidSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("asteroid_type", -1) as int,
		data.get("is_large", false) as bool,
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)
	return spec
