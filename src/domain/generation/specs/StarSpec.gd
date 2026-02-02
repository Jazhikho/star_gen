## Specification for star generation.
## Defines the archetype and constraints for generating a star.
class_name StarSpec
extends BaseSpec

const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")


## Target spectral class (O through M, or -1 for random).
var spectral_class: int

## Target subclass (0-9, or -1 for random within class).
var subclass: int

## Metallicity relative to solar (1.0 = solar, -1 for random).
var metallicity: float

## Age hint in years (-1 for random appropriate to spectral class).
var age_years: float


## Creates a new StarSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_spectral_class: Target spectral class enum, or -1 for random.
## @param p_subclass: Target subclass (0-9), or -1 for random.
## @param p_metallicity: Metallicity, or -1 for random.
## @param p_age_years: Age in years, or -1 for random.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_spectral_class: int = -1,
	p_subclass: int = -1,
	p_metallicity: float = -1.0,
	p_age_years: float = -1.0,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	super(p_generation_seed, p_name_hint, p_overrides)
	spectral_class = p_spectral_class
	subclass = p_subclass
	metallicity = p_metallicity
	age_years = p_age_years


## Creates a spec for a random star with the given seed.
## @param seed_value: The generation seed.
## @return: A new StarSpec with all random values.
static func random(seed_value: int) -> StarSpec:
	return StarSpec.new(seed_value)


## Creates a spec for a Sun-like star.
## @param seed_value: The generation seed.
## @return: A new StarSpec configured for G2V star.
static func sun_like(seed_value: int) -> StarSpec:
	return StarSpec.new(
		seed_value,
		StarClass.SpectralClass.G,
		2,
		1.0,
		-1.0
	)


## Creates a spec for a red dwarf (M class).
## @param seed_value: The generation seed.
## @return: A new StarSpec configured for M-class star.
static func red_dwarf(seed_value: int) -> StarSpec:
	return StarSpec.new(
		seed_value,
		StarClass.SpectralClass.M,
		-1,
		-1.0,
		-1.0
	)


## Creates a spec for a hot blue star (O or B class).
## @param seed_value: The generation seed.
## @return: A new StarSpec configured for early-type star.
static func hot_blue(seed_value: int) -> StarSpec:
	return StarSpec.new(
		seed_value,
		StarClass.SpectralClass.B,
		-1,
		-1.0,
		-1.0
	)


## Returns whether spectral class is specified.
## @return: True if spectral class is set.
func has_spectral_class() -> bool:
	return spectral_class >= 0


## Returns whether subclass is specified.
## @return: True if subclass is set.
func has_subclass() -> bool:
	return subclass >= 0


## Returns whether metallicity is specified.
## @return: True if metallicity is set.
func has_metallicity() -> bool:
	return metallicity >= 0.0


## Returns whether age is specified.
## @return: True if age is set.
func has_age() -> bool:
	return age_years >= 0.0


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = _base_to_dict()
	data["spec_type"] = "star"
	data["spectral_class"] = spectral_class
	data["subclass"] = subclass
	data["metallicity"] = metallicity
	data["age_years"] = age_years
	return data


## Creates a StarSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new StarSpec instance.
static func from_dict(data: Dictionary) -> StarSpec:
	var spec: StarSpec = StarSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("spectral_class", -1) as int,
		data.get("subclass", -1) as int,
		data.get("metallicity", -1.0) as float,
		data.get("age_years", -1.0) as float,
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)
	return spec
