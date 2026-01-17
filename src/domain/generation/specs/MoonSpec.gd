## Specification for moon generation.
## Moons use the same size categories as planets but are constrained by parent.
class_name MoonSpec
extends BaseSpec

const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")


## Size category (typically DWARF through TERRESTRIAL for most moons).
var size_category: int

## Whether this is a captured irregular moon.
var is_captured: bool

## Whether to generate an atmosphere (typically only large moons).
var has_atmosphere: Variant

## Whether to generate a subsurface ocean.
var has_subsurface_ocean: Variant


## Creates a new MoonSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_size_category: Size category enum, or -1 for random.
## @param p_is_captured: Whether this is a captured moon.
## @param p_has_atmosphere: Whether to have atmosphere, or null for auto.
## @param p_has_subsurface_ocean: Whether to have subsurface ocean, or null for auto.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_size_category: int = -1,
	p_is_captured: bool = false,
	p_has_atmosphere: Variant = null,
	p_has_subsurface_ocean: Variant = null,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	super(p_generation_seed, p_name_hint, p_overrides)
	size_category = p_size_category
	is_captured = p_is_captured
	has_atmosphere = p_has_atmosphere
	has_subsurface_ocean = p_has_subsurface_ocean


## Creates a spec for a random moon with the given seed.
## @param seed_value: The generation seed.
## @return: A new MoonSpec with all random values.
static func random(seed_value: int) -> MoonSpec:
	return MoonSpec.new(seed_value)


## Creates a spec for a Luna-like rocky moon.
## @param seed_value: The generation seed.
## @return: A new MoonSpec configured for sub-terrestrial rocky.
static func luna_like(seed_value: int) -> MoonSpec:
	return MoonSpec.new(
		seed_value,
		SizeCategory.Category.SUB_TERRESTRIAL,
		false,
		false,
		false
	)


## Creates a spec for an icy moon with subsurface ocean (Europa-like).
## @param seed_value: The generation seed.
## @return: A new MoonSpec configured for icy with ocean.
static func europa_like(seed_value: int) -> MoonSpec:
	return MoonSpec.new(
		seed_value,
		SizeCategory.Category.SUB_TERRESTRIAL,
		false,
		false,
		true
	)


## Creates a spec for a large moon with atmosphere (Titan-like).
## @param seed_value: The generation seed.
## @return: A new MoonSpec configured for large with atmosphere.
static func titan_like(seed_value: int) -> MoonSpec:
	return MoonSpec.new(
		seed_value,
		SizeCategory.Category.SUB_TERRESTRIAL,
		false,
		true,
		true
	)


## Creates a spec for a small captured irregular moon.
## @param seed_value: The generation seed.
## @return: A new MoonSpec configured for captured body.
static func captured(seed_value: int) -> MoonSpec:
	return MoonSpec.new(
		seed_value,
		SizeCategory.Category.DWARF,
		true,
		false,
		false
	)


## Returns whether size category is specified.
## @return: True if size is set.
func has_size_category() -> bool:
	return size_category >= 0


## Returns whether atmosphere preference is specified.
## @return: True if atmosphere choice is set.
func has_atmosphere_preference() -> bool:
	return has_atmosphere != null


## Returns whether subsurface ocean preference is specified.
## @return: True if ocean choice is set.
func has_ocean_preference() -> bool:
	return has_subsurface_ocean != null


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = _base_to_dict()
	data["spec_type"] = "moon"
	data["size_category"] = size_category
	data["is_captured"] = is_captured
	data["has_atmosphere"] = has_atmosphere
	data["has_subsurface_ocean"] = has_subsurface_ocean
	return data


## Creates a MoonSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new MoonSpec instance.
static func from_dict(data: Dictionary) -> MoonSpec:
	var spec: MoonSpec = MoonSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("size_category", -1) as int,
		data.get("is_captured", false) as bool,
		data.get("has_atmosphere"),
		data.get("has_subsurface_ocean"),
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)
	return spec
