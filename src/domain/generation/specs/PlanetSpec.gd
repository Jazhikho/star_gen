## Specification for planet generation.
## Uses size category and orbit zone to define the archetype.
class_name PlanetSpec
extends BaseSpec

const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _ring_complexity: GDScript = preload("res://src/domain/generation/archetypes/RingComplexity.gd")


## Size category (DWARF through GAS_GIANT, or -1 for random).
var size_category: int

## Orbit zone (HOT, TEMPERATE, COLD, or -1 for random).
var orbit_zone: int

## Whether to generate an atmosphere (null for auto-determine).
var has_atmosphere: Variant

## Whether to generate rings (null for auto-determine).
var has_rings: Variant

## Ring complexity if rings are present.
var ring_complexity: int


## Creates a new PlanetSpec instance.
## @param p_generation_seed: The generation seed.
## @param p_size_category: Size category enum, or -1 for random.
## @param p_orbit_zone: Orbit zone enum, or -1 for random.
## @param p_has_atmosphere: Whether to have atmosphere, or null for auto.
## @param p_has_rings: Whether to have rings, or null for auto.
## @param p_ring_complexity: Ring complexity if has rings.
## @param p_name_hint: Optional name hint.
## @param p_overrides: Optional field overrides.
func _init(
	p_generation_seed: int = 0,
	p_size_category: int = -1,
	p_orbit_zone: int = -1,
	p_has_atmosphere: Variant = null,
	p_has_rings: Variant = null,
	p_ring_complexity: int = -1,
	p_name_hint: String = "",
	p_overrides: Dictionary = {}
) -> void:
	super(p_generation_seed, p_name_hint, p_overrides)
	size_category = p_size_category
	orbit_zone = p_orbit_zone
	has_atmosphere = p_has_atmosphere
	has_rings = p_has_rings
	ring_complexity = p_ring_complexity


## Creates a spec for a random planet with the given seed.
## @param seed_value: The generation seed.
## @return: A new PlanetSpec with all random values.
static func random(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(seed_value)


## Creates a spec for an Earth-like planet.
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for terrestrial temperate.
static func earth_like(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.TERRESTRIAL,
		OrbitZone.Zone.TEMPERATE,
		true,
		false
	)


## Creates a spec for a hot Jupiter.
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for hot gas giant.
static func hot_jupiter(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.GAS_GIANT,
		OrbitZone.Zone.HOT,
		true,
		false
	)


## Creates a spec for a cold gas giant like Jupiter/Saturn.
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for cold gas giant.
static func cold_giant(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.GAS_GIANT,
		OrbitZone.Zone.COLD,
		true,
		null
	)


## Creates a spec for a Mars-like planet.
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for sub-terrestrial cold.
static func mars_like(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.SUB_TERRESTRIAL,
		OrbitZone.Zone.COLD,
		true,
		false
	)


## Creates a spec for a dwarf planet (Pluto-like).
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for dwarf cold.
static func dwarf_planet(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.DWARF,
		OrbitZone.Zone.COLD,
		false,
		false
	)


## Creates a spec for an ice giant (Neptune-like).
## @param seed_value: The generation seed.
## @return: A new PlanetSpec configured for Neptune-class cold.
static func ice_giant(seed_value: int) -> PlanetSpec:
	return PlanetSpec.new(
		seed_value,
		SizeCategory.Category.NEPTUNE_CLASS,
		OrbitZone.Zone.COLD,
		true,
		true,
		RingComplexity.Level.TRACE
	)


## Returns whether size category is specified.
## @return: True if size is set.
func has_size_category() -> bool:
	return size_category >= 0


## Returns whether orbit zone is specified.
## @return: True if zone is set.
func has_orbit_zone() -> bool:
	return orbit_zone >= 0


## Returns whether atmosphere preference is specified.
## @return: True if atmosphere choice is set.
func has_atmosphere_preference() -> bool:
	return has_atmosphere != null


## Returns whether ring preference is specified.
## @return: True if ring choice is set.
func has_ring_preference() -> bool:
	return has_rings != null


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = _base_to_dict()
	data["spec_type"] = "planet"
	data["size_category"] = size_category
	data["orbit_zone"] = orbit_zone
	data["has_atmosphere"] = has_atmosphere
	data["has_rings"] = has_rings
	data["ring_complexity"] = ring_complexity
	return data


## Creates a PlanetSpec from a dictionary.
## @param data: The dictionary to parse.
## @return: A new PlanetSpec instance.
static func from_dict(data: Dictionary) -> PlanetSpec:
	var spec: PlanetSpec = PlanetSpec.new(
		data.get("generation_seed", 0) as int,
		data.get("size_category", -1) as int,
		data.get("orbit_zone", -1) as int,
		data.get("has_atmosphere"),
		data.get("has_rings"),
		data.get("ring_complexity", -1) as int,
		data.get("name_hint", "") as String,
		data.get("overrides", {}) as Dictionary
	)
	return spec
