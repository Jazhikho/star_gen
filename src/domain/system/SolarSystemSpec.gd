## Specification for solar system generation.
## Defines constraints and hints for the generator.
class_name SolarSystemSpec
extends RefCounted

const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")


## The seed used for deterministic generation.
var generation_seed: int

## Optional name hint for the system.
var name_hint: String

## Minimum number of stars (1-10).
var star_count_min: int

## Maximum number of stars (1-10).
var star_count_max: int

## Hints for spectral classes of stars (optional, can be empty).
## If provided, stars will be generated with these classes in order.
var spectral_class_hints: Array[int]

## System age in years (all stars will have similar age).
## -1 = random age per star.
var system_age_years: float

## System metallicity relative to solar (1.0 = solar).
## -1 = random metallicity per star.
var system_metallicity: float

## Whether to include asteroid belts in generation.
var include_asteroid_belts: bool

## Whether to generate population data for planets and moons.
var generate_population: bool

## Field overrides for generation.
var overrides: Dictionary


## Creates a new SolarSystemSpec.
## @param p_generation_seed: The generation seed.
## @param p_star_count_min: Minimum star count.
## @param p_star_count_max: Maximum star count.
func _init(
	p_generation_seed: int = 0,
	p_star_count_min: int = 1,
	p_star_count_max: int = 1
) -> void:
	generation_seed = p_generation_seed
	name_hint = ""
	star_count_min = clampi(p_star_count_min, 1, 10)
	star_count_max = clampi(p_star_count_max, star_count_min, 10)
	spectral_class_hints = []
	system_age_years = -1.0
	system_metallicity = -1.0
	include_asteroid_belts = true
	generate_population = false
	overrides = {}


## Creates a spec for a single-star system (like our Solar System).
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec for a single star system.
static func single_star(seed_value: int) -> SolarSystemSpec:
	return SolarSystemSpec.new(seed_value, 1, 1)


## Creates a spec for a binary star system.
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec for a binary system.
static func binary(seed_value: int) -> SolarSystemSpec:
	return SolarSystemSpec.new(seed_value, 2, 2)


## Creates a spec for a random system with 1-3 stars.
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec.
static func random_small(seed_value: int) -> SolarSystemSpec:
	return SolarSystemSpec.new(seed_value, 1, 3)


## Creates a spec for a random system with any number of stars.
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec.
static func random(seed_value: int) -> SolarSystemSpec:
	return SolarSystemSpec.new(seed_value, 1, 10)


## Creates a spec for a Sun-like single star system.
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec.
static func sun_like(seed_value: int) -> SolarSystemSpec:
	var spec: SolarSystemSpec = SolarSystemSpec.new(seed_value, 1, 1)
	spec.spectral_class_hints = [StarClass.SpectralClass.G]
	return spec


## Creates a spec for an Alpha Centauri-like triple system.
## @param seed_value: The generation seed.
## @return: A new SolarSystemSpec.
static func alpha_centauri_like(seed_value: int) -> SolarSystemSpec:
	var spec: SolarSystemSpec = SolarSystemSpec.new(seed_value, 3, 3)
	spec.spectral_class_hints = [
		StarClass.SpectralClass.G, # Alpha Centauri A
		StarClass.SpectralClass.K, # Alpha Centauri B
		StarClass.SpectralClass.M, # Proxima Centauri
	]
	return spec


## Returns whether a specific field has an override.
## @param field_path: The field path to check.
## @return: True if the field is overridden.
func has_override(field_path: String) -> bool:
	return overrides.has(field_path)


## Gets the override value for a field, or a default if not overridden.
## @param field_path: The field path to get.
## @param default_value: Value to return if not overridden.
## @return: The override value or default.
func get_override(field_path: String, default_value: Variant) -> Variant:
	if overrides.has(field_path):
		return overrides[field_path]
	return default_value


## Sets an override value.
## @param field_path: The field path to set.
## @param value: The value to lock.
func set_override(field_path: String, value: Variant) -> void:
	overrides[field_path] = value


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var hints_array: Array[int] = []
	for hint in spectral_class_hints:
		hints_array.append(hint)
	
	return {
		"generation_seed": generation_seed,
		"name_hint": name_hint,
		"star_count_min": star_count_min,
		"star_count_max": star_count_max,
		"spectral_class_hints": hints_array,
		"system_age_years": system_age_years,
		"system_metallicity": system_metallicity,
		"include_asteroid_belts": include_asteroid_belts,
		"generate_population": generate_population,
		"overrides": overrides.duplicate(),
	}


## Creates a SolarSystemSpec from a dictionary.
## @param data: Dictionary to parse.
## @return: A new SolarSystemSpec.
static func from_dict(data: Dictionary) -> SolarSystemSpec:
	var spec: SolarSystemSpec = SolarSystemSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("star_count_min", 1) as int,
		data.get("star_count_max", 1) as int
	)
	spec.name_hint = data.get("name_hint", "") as String
	spec.system_age_years = data.get("system_age_years", -1.0) as float
	spec.system_metallicity = data.get("system_metallicity", -1.0) as float
	spec.include_asteroid_belts = data.get("include_asteroid_belts", true) as bool
	spec.generate_population = data.get("generate_population", false) as bool
	spec.overrides = (data.get("overrides", {}) as Dictionary).duplicate()
	
	var hints: Array = data.get("spectral_class_hints", []) as Array
	for hint in hints:
		spec.spectral_class_hints.append(hint as int)
	
	return spec
