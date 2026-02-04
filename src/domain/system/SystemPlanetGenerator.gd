## Generates planets for a solar system by filling orbit slots.
## Determines planet archetypes based on zone and probability tables.
class_name SystemPlanetGenerator
extends RefCounted

const _orbit_slot: GDScript = preload("res://src/domain/system/OrbitSlot.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _size_category: GDScript = preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_validator: GDScript = preload("res://src/domain/celestial/validation/CelestialValidator.gd")
const _validation_result: GDScript = preload("res://src/domain/celestial/validation/ValidationResult.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")


## Planet archetype weights by zone and size category.
## HOT zone favors rocky and super-Earths, occasional hot Jupiters.
const HOT_ZONE_WEIGHTS: Dictionary = {
	SizeCategory.Category.DWARF: 5.0,
	SizeCategory.Category.SUB_TERRESTRIAL: 15.0,
	SizeCategory.Category.TERRESTRIAL: 25.0,
	SizeCategory.Category.SUPER_EARTH: 30.0,
	SizeCategory.Category.MINI_NEPTUNE: 15.0,
	SizeCategory.Category.NEPTUNE_CLASS: 5.0,
	SizeCategory.Category.GAS_GIANT: 5.0,  # Hot Jupiters are rare
}

## TEMPERATE zone has balanced mix with slight rocky bias.
const TEMPERATE_ZONE_WEIGHTS: Dictionary = {
	SizeCategory.Category.DWARF: 8.0,
	SizeCategory.Category.SUB_TERRESTRIAL: 18.0,
	SizeCategory.Category.TERRESTRIAL: 25.0,
	SizeCategory.Category.SUPER_EARTH: 20.0,
	SizeCategory.Category.MINI_NEPTUNE: 12.0,
	SizeCategory.Category.NEPTUNE_CLASS: 10.0,
	SizeCategory.Category.GAS_GIANT: 7.0,
}

## COLD zone favors gas giants and ice giants.
const COLD_ZONE_WEIGHTS: Dictionary = {
	SizeCategory.Category.DWARF: 10.0,
	SizeCategory.Category.SUB_TERRESTRIAL: 8.0,
	SizeCategory.Category.TERRESTRIAL: 5.0,
	SizeCategory.Category.SUPER_EARTH: 7.0,
	SizeCategory.Category.MINI_NEPTUNE: 15.0,
	SizeCategory.Category.NEPTUNE_CLASS: 25.0,
	SizeCategory.Category.GAS_GIANT: 30.0,
}


## Result of planet generation for a system.
class PlanetGenerationResult:
	extends RefCounted
	
	## Generated planets.
	var planets: Array[CelestialBody]
	
	## Updated slots (with filled status).
	var slots: Array[OrbitSlot]
	
	## Whether generation succeeded.
	var success: bool
	
	## Error message if failed.
	var error_message: String
	
	func _init() -> void:
		planets = []
		slots = []
		success = false
		error_message = ""


## Generates planets for orbit slots.
## @param slots: Array of available orbit slots.
## @param orbit_hosts: Array of orbit hosts (for context lookup).
## @param stars: Array of star bodies (for parent context).
## @param rng: Random number generator.
## @return: PlanetGenerationResult with generated planets.
static func generate(
	slots: Array[OrbitSlot],
	orbit_hosts: Array[OrbitHost],
	stars: Array[CelestialBody],
	rng: SeededRng
) -> PlanetGenerationResult:
	var result: PlanetGenerationResult = PlanetGenerationResult.new()
	result.slots = slots.duplicate()
	
	# Build host lookup
	var host_map: Dictionary = {}
	for host in orbit_hosts:
		host_map[host.node_id] = host
	
	# Process each slot
	for slot in result.slots:
		# Skip unstable or already filled slots
		if not slot.is_available():
			continue
		
		# Check if this slot should be filled
		if not _should_fill_slot(slot, rng):
			continue
		
		# Get orbit host for this slot
		var host: OrbitHost = host_map.get(slot.orbit_host_id) as OrbitHost
		if host == null:
			continue
		
		# Generate planet for this slot
		var planet: CelestialBody = _generate_planet_for_slot(slot, host, stars, rng)
		if planet != null:
			result.planets.append(planet)
			slot.fill_with_planet(planet.id)
	
	result.success = true
	return result


## Generates planets with a specific count target.
## Will attempt to fill slots until target is reached or no more available.
## @param slots: Array of orbit slots (sorted by preference).
## @param orbit_hosts: Array of orbit hosts.
## @param stars: Array of star bodies.
## @param target_count: Desired number of planets.
## @param rng: Random number generator.
## @return: PlanetGenerationResult.
static func generate_targeted(
	slots: Array[OrbitSlot],
	orbit_hosts: Array[OrbitHost],
	stars: Array[CelestialBody],
	target_count: int,
	rng: SeededRng
) -> PlanetGenerationResult:
	var result: PlanetGenerationResult = PlanetGenerationResult.new()
	result.slots = slots.duplicate()
	
	# Build host lookup
	var host_map: Dictionary = {}
	for host in orbit_hosts:
		host_map[host.node_id] = host
	
	var planet_index: int = 0
	var available_slots: Array[OrbitSlot] = []
	
	# Collect available slots
	for slot in result.slots:
		if slot.is_available():
			available_slots.append(slot)
	
	# Pre-calculate scores for consistent sorting
	var slot_scores: Dictionary = {}
	for slot in available_slots:
		slot_scores[slot] = slot.fill_probability + rng.randf() * 0.3
	
	# Sort by fill probability (higher first) with some randomness
	available_slots.sort_custom(func(a: OrbitSlot, b: OrbitSlot) -> bool:
		var a_score: float = slot_scores.get(a, 0.0)
		var b_score: float = slot_scores.get(b, 0.0)
		if a_score == b_score:
			return a.id < b.id  # Tiebreaker for stability
		return a_score > b_score
	)
	
	for slot in available_slots:
		if planet_index >= target_count:
			break
		
		var host: OrbitHost = host_map.get(slot.orbit_host_id) as OrbitHost
		if host == null:
			continue
		
		var planet: CelestialBody = _generate_planet_for_slot(slot, host, stars, rng)
		if planet != null:
			result.planets.append(planet)
			slot.fill_with_planet(planet.id)
			planet_index += 1
	
	result.success = true
	return result


## Determines if a slot should be filled based on its probability.
## @param slot: The orbit slot.
## @param rng: Random number generator.
## @return: True if slot should be filled.
static func _should_fill_slot(slot: OrbitSlot, rng: SeededRng) -> bool:
	return rng.randf() < slot.fill_probability


## Generates a planet for a specific slot.
## @param slot: The orbit slot.
## @param host: The orbit host.
## @param stars: Array of star bodies.
## @param rng: Random number generator.
## @return: Generated planet, or null if failed.
static func _generate_planet_for_slot(
	slot: OrbitSlot,
	host: OrbitHost,
	stars: Array[CelestialBody],
	rng: SeededRng
) -> CelestialBody:
	# Determine size category based on zone
	var size_category: SizeCategory.Category = _determine_size_category(slot.zone, rng)
	
	# Create planet spec
	var planet_seed: int = rng.randi()
	var spec: PlanetSpec = PlanetSpec.new(
		planet_seed,
		size_category,
		slot.zone
	)
	
	# Override orbital distance to match slot
	spec.set_override("orbital.semi_major_axis_m", slot.semi_major_axis_m)
	
	# Use suggested eccentricity if available
	if slot.suggested_eccentricity > 0.0:
		spec.set_override("orbital.eccentricity", slot.suggested_eccentricity)
	
	# Create parent context from orbit host
	var context: ParentContext = _create_parent_context(host, stars, slot.semi_major_axis_m)
	
	# Generate planet
	var planet_rng: SeededRng = SeededRng.new(planet_seed)
	var planet: CelestialBody = PlanetGenerator.generate(spec, context, planet_rng)
	
	if planet != null:
		# Generate planet ID and name
		planet.id = "planet_%s" % slot.id
		if planet.name.is_empty():
			planet.name = _generate_planet_name(slot)
		
		# Set orbital parent to host
		if planet.has_orbital():
			planet.orbital.parent_id = host.node_id
	
	return planet


## Determines planet size category based on zone.
## @param zone: The orbital zone.
## @param rng: Random number generator.
## @return: Selected size category.
static func _determine_size_category(zone: OrbitZone.Zone, rng: SeededRng) -> SizeCategory.Category:
	var weights: Dictionary
	
	match zone:
		OrbitZone.Zone.HOT:
			weights = HOT_ZONE_WEIGHTS
		OrbitZone.Zone.TEMPERATE:
			weights = TEMPERATE_ZONE_WEIGHTS
		OrbitZone.Zone.COLD:
			weights = COLD_ZONE_WEIGHTS
		_:
			weights = TEMPERATE_ZONE_WEIGHTS
	
	# Build arrays for weighted choice
	var categories: Array[int] = []
	var weight_array: Array[float] = []
	
	for category in weights:
		categories.append(category)
		weight_array.append(weights[category])
	
	return rng.weighted_choice(categories, weight_array) as SizeCategory.Category


## Creates a parent context from an orbit host.
## @param host: The orbit host.
## @param stars: Array of star bodies.
## @param orbital_distance_m: Distance from host.
## @return: ParentContext for planet generation.
static func _create_parent_context(
	host: OrbitHost,
	stars: Array[CelestialBody],
	orbital_distance_m: float
) -> ParentContext:
	# Get star age (use first star's age as system age)
	var system_age: float = 4.6e9  # Default to 4.6 billion years
	for star in stars:
		if star.has_stellar():
			system_age = star.stellar.age_years
			break
	
	return ParentContext.for_planet(
		host.combined_mass_kg,
		host.combined_luminosity_watts,
		host.effective_temperature_k,
		system_age,
		orbital_distance_m
	)


## Generates a default planet name based on slot.
## @param slot: The orbit slot.
## @return: Planet name.
static func _generate_planet_name(slot: OrbitSlot) -> String:
	var distance_au: float = slot.get_semi_major_axis_au()
	var zone_name: String = slot.get_zone_string()
	return "%s Planet (%.1f AU)" % [zone_name, distance_au]


## Calculates statistics about generated planets.
## @param planets: Array of planets.
## @return: Dictionary with statistics.
static func get_statistics(planets: Array[CelestialBody]) -> Dictionary:
	var stats: Dictionary = {
		"total": planets.size(),
		"rocky": 0,
		"gaseous": 0,
		"has_atmosphere": 0,
		"has_rings": 0,
		"min_mass_earth": 0.0,
		"max_mass_earth": 0.0,
		"avg_mass_earth": 0.0,
	}
	
	if planets.is_empty():
		return stats
	
	var mass_sum: float = 0.0
	var min_mass: float = planets[0].physical.mass_kg
	var max_mass: float = planets[0].physical.mass_kg
	
	for planet in planets:
		var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
		mass_sum += mass_earth
		min_mass = minf(min_mass, planet.physical.mass_kg)
		max_mass = maxf(max_mass, planet.physical.mass_kg)
		
		# Count by composition (approximate from mass)
		if mass_earth < 10.0:
			stats["rocky"] += 1
		else:
			stats["gaseous"] += 1
		
		if planet.has_atmosphere():
			stats["has_atmosphere"] += 1
		
		if planet.has_ring_system():
			stats["has_rings"] += 1
	
	stats["min_mass_earth"] = min_mass / Units.EARTH_MASS_KG
	stats["max_mass_earth"] = max_mass / Units.EARTH_MASS_KG
	stats["avg_mass_earth"] = mass_sum / float(planets.size())
	
	return stats


## Filters planets by zone.
## @param planets: Array of planets.
## @param slots: Array of slots (for zone lookup).
## @param zone: Zone to filter by.
## @return: Array of planets in the specified zone.
static func filter_by_zone(
	planets: Array[CelestialBody],
	slots: Array[OrbitSlot],
	zone: OrbitZone.Zone
) -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	
	# Build planet ID to slot map
	var planet_to_slot: Dictionary = {}
	for slot in slots:
		if slot.is_filled:
			planet_to_slot[slot.planet_id] = slot
	
	for planet in planets:
		var slot: OrbitSlot = planet_to_slot.get(planet.id) as OrbitSlot
		if slot != null and slot.zone == zone:
			result.append(planet)
	
	return result


## Sorts planets by distance from star (innermost first).
## @param planets: Array of planets to sort.
static func sort_by_distance(planets: Array[CelestialBody]) -> void:
	planets.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
		var a_has: bool = a.has_orbital()
		var b_has: bool = b.has_orbital()
		
		# Both have orbital data - compare normally
		if a_has and b_has:
			return a.orbital.semi_major_axis_m < b.orbital.semi_major_axis_m
		
		# Bodies with orbital data come before those without
		if a_has and not b_has:
			return true
		if not a_has and b_has:
			return false
		
		# Neither has orbital - they're equal, maintain stability
		return false
	)


## Sorts planets by mass (largest first).
## @param planets: Array of planets to sort.
static func sort_by_mass(planets: Array[CelestialBody]) -> void:
	planets.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
		return a.physical.mass_kg > b.physical.mass_kg
	)


## Returns planets that can host moons (gas giants and large planets).
## @param planets: Array of planets.
## @return: Array of planets suitable for moon generation.
static func get_moon_candidates(planets: Array[CelestialBody]) -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	
	for planet in planets:
		var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
		
		# Gas giants and large planets can have moons
		if mass_earth >= 10.0:
			result.append(planet)
		# Even smaller planets can have small moons (like Mars)
		elif mass_earth >= 0.1:
			result.append(planet)
	
	return result


## Assigns Roman numeral names to planets in order.
## @param planets: Array of planets (should be sorted by distance).
## @param system_name: Optional system name prefix.
static func assign_roman_numeral_names(planets: Array[CelestialBody], system_name: String = "") -> void:
	var numerals: Array[String] = [
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
		"XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX"
	]
	
	for i in range(planets.size()):
		var numeral: String = numerals[i] if i < numerals.size() else str(i + 1)
		
		if system_name.is_empty():
			planets[i].name = "Planet %s" % numeral
		else:
			planets[i].name = "%s %s" % [system_name, numeral]


## Estimates the number of planets likely to be generated.
## @param slots: Array of orbit slots.
## @return: Expected planet count.
static func estimate_planet_count(slots: Array[OrbitSlot]) -> int:
	var expected: float = 0.0
	
	for slot in slots:
		if slot.is_available():
			expected += slot.fill_probability
	
	return roundi(expected)


## Validates that planet orbital distances match their slots.
## @param planets: Array of planets.
## @param slots: Array of slots.
## @return: True if all planets match their slots.
static func validate_planet_slot_consistency(
	planets: Array[CelestialBody],
	slots: Array[OrbitSlot]
) -> bool:
	# Build planet ID to slot map
	var planet_to_slot: Dictionary = {}
	for slot in slots:
		if slot.is_filled:
			planet_to_slot[slot.planet_id] = slot
	
	for planet in planets:
		var slot: OrbitSlot = planet_to_slot.get(planet.id) as OrbitSlot
		if slot == null:
			return false
		
		if not planet.has_orbital():
			return false
		
		# Check distance matches (with small tolerance)
		var distance_diff: float = absf(planet.orbital.semi_major_axis_m - slot.semi_major_axis_m)
		if distance_diff > 1000.0:  # 1 km tolerance
			return false
	
	return true
