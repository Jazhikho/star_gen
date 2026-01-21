## Generates asteroid belts for a solar system.
## Creates belt definitions and generates the top 10 largest asteroids per belt.
class_name SystemAsteroidGenerator
extends RefCounted

const _asteroid_belt := preload("res://src/domain/system/AsteroidBelt.gd")
const _orbit_host := preload("res://src/domain/system/OrbitHost.gd")
const _orbit_slot := preload("res://src/domain/system/OrbitSlot.gd")
const _orbital_mechanics := preload("res://src/domain/system/OrbitalMechanics.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _asteroid_spec := preload("res://src/domain/generation/specs/AsteroidSpec.gd")
const _asteroid_generator := preload("res://src/domain/generation/generators/AsteroidGenerator.gd")
const _asteroid_type := preload("res://src/domain/generation/archetypes/AsteroidType.gd")
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")


## Maximum number of major asteroids to generate per belt.
const MAX_MAJOR_ASTEROIDS: int = 10

## Minimum belt width as fraction of center distance.
const MIN_BELT_WIDTH_FRACTION: float = 0.1

## Maximum belt width as fraction of center distance.
const MAX_BELT_WIDTH_FRACTION: float = 0.4

## Probability of generating an inner (rocky) asteroid belt.
const INNER_BELT_PROBABILITY: float = 0.60

## Probability of generating an outer (icy) belt (Kuiper-like).
const OUTER_BELT_PROBABILITY: float = 0.50

## Typical inner belt mass range in kg (Main belt is ~3e21 kg).
const INNER_BELT_MASS_MIN_KG: float = 1.0e20
const INNER_BELT_MASS_MAX_KG: float = 1.0e22

## Typical outer belt mass range in kg (Kuiper belt is ~1e23 kg estimated).
const OUTER_BELT_MASS_MIN_KG: float = 1.0e21
const OUTER_BELT_MASS_MAX_KG: float = 1.0e24

## Major asteroid radius threshold (km) - asteroids above this are "major".
const MAJOR_ASTEROID_THRESHOLD_KM: float = 100.0

## Power law exponent for asteroid size distribution (N(>D) ∝ D^-alpha).
const POWER_LAW_ALPHA: float = 2.5


## Result of asteroid belt generation.
class BeltGenerationResult:
	extends RefCounted
	
	## Generated asteroid belts.
	var belts: Array[AsteroidBelt]
	
	## Generated major asteroids (across all belts).
	var asteroids: Array[CelestialBody]
	
	## Mapping of belt ID to asteroid IDs.
	var belt_asteroid_map: Dictionary  # String -> Array[String]
	
	## Whether generation succeeded.
	var success: bool
	
	## Error message if failed.
	var error_message: String
	
	func _init() -> void:
		belts = []
		asteroids = []
		belt_asteroid_map = {}
		success = false
		error_message = ""


## Generates asteroid belts for a system.
## @param orbit_hosts: Array of orbit hosts.
## @param filled_slots: Array of filled orbit slots (to avoid belt-planet overlap).
## @param stars: Array of star bodies.
## @param rng: Random number generator.
## @return: BeltGenerationResult with belts and major asteroids.
static func generate(
	orbit_hosts: Array[OrbitHost],
	filled_slots: Array[OrbitSlot],
	stars: Array[CelestialBody],
	rng: SeededRng
) -> BeltGenerationResult:
	var result: BeltGenerationResult = BeltGenerationResult.new()
	
	# Process each orbit host
	for host in orbit_hosts:
		var host_belts: Array[AsteroidBelt] = _generate_belts_for_host(
			host,
			filled_slots,
			stars,
			rng
		)
		
		for belt in host_belts:
			result.belts.append(belt)
			
			# Generate major asteroids for this belt
			var belt_asteroids: Array[CelestialBody] = _generate_major_asteroids(
				belt,
				host,
				stars,
				rng
			)
			
			var asteroid_ids: Array[String] = []
			for asteroid in belt_asteroids:
				result.asteroids.append(asteroid)
				asteroid_ids.append(asteroid.id)
			belt.major_asteroid_ids = asteroid_ids
			result.belt_asteroid_map[belt.id] = asteroid_ids
	
	result.success = true
	return result


## Generates belts for a single orbit host.
## @param host: The orbit host.
## @param filled_slots: Slots with planets (to avoid).
## @param _stars: Star bodies (reserved for future use).
## @param rng: Random number generator.
## @return: Array of generated belts.
static func _generate_belts_for_host(
	host: OrbitHost,
	filled_slots: Array[OrbitSlot],
	_stars: Array[CelestialBody],
	rng: SeededRng
) -> Array[AsteroidBelt]:
	var belts: Array[AsteroidBelt] = []
	
	if not host.has_valid_zone():
		return belts
	
	# Get planet positions for this host
	var planet_distances: Array[float] = []
	for slot in filled_slots:
		if slot.orbit_host_id == host.node_id and slot.is_filled:
			planet_distances.append(slot.semi_major_axis_m)
	planet_distances.sort()
	
	# Try to generate inner belt (between frost line and outer planets)
	if rng.randf() < INNER_BELT_PROBABILITY:
		var inner_belt: AsteroidBelt = _try_generate_inner_belt(
			host,
			planet_distances,
			rng
		)
		if inner_belt != null:
			belts.append(inner_belt)
	
	# Try to generate outer belt (Kuiper-like)
	if rng.randf() < OUTER_BELT_PROBABILITY:
		var outer_belt: AsteroidBelt = _try_generate_outer_belt(
			host,
			planet_distances,
			rng
		)
		if outer_belt != null:
			belts.append(outer_belt)
	
	return belts


## Tries to generate an inner (rocky) asteroid belt.
## @param host: The orbit host.
## @param planet_distances: Sorted array of planet distances.
## @param rng: Random number generator.
## @return: AsteroidBelt, or null if no suitable location.
static func _try_generate_inner_belt(
	host: OrbitHost,
	planet_distances: Array[float],
	rng: SeededRng
) -> AsteroidBelt:
	# Inner belt should be near the frost line (like our Main Belt)
	var target_center: float = host.frost_line_m * rng.randf_range(0.7, 1.1)
	
	# Ensure it's within stable zone
	if target_center < host.inner_stability_m or target_center > host.outer_stability_m:
		return null
	
	# Find a gap between planets
	var belt_location: Dictionary = _find_belt_gap(
		target_center,
		planet_distances,
		host.inner_stability_m,
		host.outer_stability_m,
		rng
	)
	
	if belt_location.is_empty():
		return null
	
	var inner_radius: float = belt_location["inner"]
	var outer_radius: float = belt_location["outer"]
	
	# Create belt
	var belt: AsteroidBelt = AsteroidBelt.new(
		"belt_%s_inner" % host.node_id,
		"Inner Asteroid Belt"
	)
	belt.orbit_host_id = host.node_id
	belt.inner_radius_m = inner_radius
	belt.outer_radius_m = outer_radius
	
	# Determine composition (inner belts are rocky/metallic)
	belt.composition = _determine_inner_belt_composition(rng)
	
	# Estimate total mass
	belt.total_mass_kg = _estimate_belt_mass(inner_radius, outer_radius, false, rng)
	
	return belt


## Tries to generate an outer (icy) asteroid belt.
## @param host: The orbit host.
## @param planet_distances: Sorted array of planet distances.
## @param rng: Random number generator.
## @return: AsteroidBelt, or null if no suitable location.
static func _try_generate_outer_belt(
	host: OrbitHost,
	planet_distances: Array[float],
	rng: SeededRng
) -> AsteroidBelt:
	# Outer belt should be well beyond the frost line (like our Kuiper Belt)
	# Typically 30-50 AU for Sun-like star
	var min_distance: float = host.frost_line_m * 5.0
	var max_distance: float = host.outer_stability_m * 0.8
	
	if min_distance >= max_distance:
		return null
	
	# Find outermost planet
	var outermost_planet: float = 0.0
	if planet_distances.size() > 0:
		outermost_planet = planet_distances[planet_distances.size() - 1]
	
	# Belt should be beyond outermost planet
	var inner_radius: float = maxf(min_distance, outermost_planet * 1.5)
	
	if inner_radius >= max_distance:
		return null
	
	# Belt width
	var width_fraction: float = rng.randf_range(MIN_BELT_WIDTH_FRACTION, MAX_BELT_WIDTH_FRACTION)
	var outer_radius: float = inner_radius * (1.0 + width_fraction)
	outer_radius = minf(outer_radius, max_distance)
	
	if outer_radius <= inner_radius:
		return null
	
	# Create belt
	var belt: AsteroidBelt = AsteroidBelt.new(
		"belt_%s_outer" % host.node_id,
		"Outer Asteroid Belt"
	)
	belt.orbit_host_id = host.node_id
	belt.inner_radius_m = inner_radius
	belt.outer_radius_m = outer_radius
	
	# Outer belts are icy
	belt.composition = _determine_outer_belt_composition(rng)
	
	# Estimate total mass (outer belts can be more massive)
	belt.total_mass_kg = _estimate_belt_mass(inner_radius, outer_radius, true, rng)
	
	return belt


## Finds a gap between planets for a belt.
## @param target_center: Desired center distance.
## @param planet_distances: Sorted planet distances.
## @param min_distance: Minimum allowed distance.
## @param max_distance: Maximum allowed distance.
## @param rng: Random number generator.
## @return: Dictionary with "inner" and "outer" keys, or empty if no gap.
static func _find_belt_gap(
	target_center: float,
	planet_distances: Array[float],
	min_distance: float,
	max_distance: float,
	rng: SeededRng
) -> Dictionary:
	if planet_distances.is_empty():
		# No planets - belt can go anywhere
		var empty_width_fraction: float = rng.randf_range(MIN_BELT_WIDTH_FRACTION, MAX_BELT_WIDTH_FRACTION)
		var empty_half_width: float = target_center * empty_width_fraction * 0.5
		return {
			"inner": maxf(min_distance, target_center - empty_half_width),
			"outer": minf(max_distance, target_center + empty_half_width),
		}
	
	# Find the gap that contains or is closest to target_center
	var best_gap_inner: float = min_distance
	var best_gap_outer: float = planet_distances[0] * 0.8  # Leave margin
	var best_gap_score: float = _score_gap(target_center, best_gap_inner, best_gap_outer)
	
	# Check gaps between planets
	for i in range(planet_distances.size() - 1):
		var gap_inner: float = planet_distances[i] * 1.2  # Leave margin
		var gap_outer: float = planet_distances[i + 1] * 0.8
		
		if gap_outer > gap_inner:
			var score: float = _score_gap(target_center, gap_inner, gap_outer)
			if score > best_gap_score:
				best_gap_inner = gap_inner
				best_gap_outer = gap_outer
				best_gap_score = score
	
	# Check gap after last planet
	var last_planet: float = planet_distances[planet_distances.size() - 1]
	var final_gap_inner: float = last_planet * 1.2
	var final_gap_outer: float = max_distance
	
	if final_gap_outer > final_gap_inner:
		var score: float = _score_gap(target_center, final_gap_inner, final_gap_outer)
		if score > best_gap_score:
			best_gap_inner = final_gap_inner
			best_gap_outer = final_gap_outer
			best_gap_score = score
	
	# Check if best gap is usable
	var gap_width: float = best_gap_outer - best_gap_inner
	var min_gap_width: float = best_gap_inner * MIN_BELT_WIDTH_FRACTION
	
	if gap_width < min_gap_width:
		return {}
	
	# Adjust to fit target if possible
	var belt_center: float = clampf(target_center, best_gap_inner, best_gap_outer)
	var width_fraction: float = rng.randf_range(MIN_BELT_WIDTH_FRACTION, MAX_BELT_WIDTH_FRACTION)
	var half_width: float = belt_center * width_fraction * 0.5
	
	return {
		"inner": maxf(best_gap_inner, belt_center - half_width),
		"outer": minf(best_gap_outer, belt_center + half_width),
	}


## Scores a gap for belt placement (higher is better).
## @param target: Target center distance.
## @param inner: Inner edge of gap.
## @param outer: Outer edge of gap.
## @return: Score value.
static func _score_gap(target: float, inner: float, outer: float) -> float:
	var width: float = outer - inner
	if width <= 0.0:
		return -1.0
	
	var center: float = (inner + outer) / 2.0
	var distance_to_target: float = absf(center - target)
	
	# Prefer wider gaps closer to target
	return width / (1.0 + distance_to_target / target)


## Determines composition for inner belt.
## @param rng: Random number generator.
## @return: Belt composition.
static func _determine_inner_belt_composition(rng: SeededRng) -> AsteroidBelt.Composition:
	var roll: float = rng.randf()
	
	if roll < 0.50:
		return AsteroidBelt.Composition.ROCKY
	elif roll < 0.80:
		return AsteroidBelt.Composition.MIXED
	else:
		return AsteroidBelt.Composition.METALLIC


## Determines composition for outer belt.
## @param rng: Random number generator.
## @return: Belt composition.
static func _determine_outer_belt_composition(rng: SeededRng) -> AsteroidBelt.Composition:
	var roll: float = rng.randf()
	
	if roll < 0.70:
		return AsteroidBelt.Composition.ICY
	else:
		return AsteroidBelt.Composition.MIXED


## Estimates belt mass.
## @param inner_radius: Inner belt radius.
## @param outer_radius: Outer belt radius.
## @param is_outer: Whether this is an outer belt.
## @param rng: Random number generator.
## @return: Estimated mass in kg.
static func _estimate_belt_mass(
	inner_radius: float,
	outer_radius: float,
	is_outer: bool,
	rng: SeededRng
) -> float:
	var min_mass: float
	var max_mass: float
	
	if is_outer:
		min_mass = OUTER_BELT_MASS_MIN_KG
		max_mass = OUTER_BELT_MASS_MAX_KG
	else:
		min_mass = INNER_BELT_MASS_MIN_KG
		max_mass = INNER_BELT_MASS_MAX_KG
	
	# Scale by belt width
	var width_factor: float = (outer_radius - inner_radius) / inner_radius
	var scale: float = clampf(width_factor * 2.0, 0.5, 2.0)
	
	# Log-uniform distribution
	var log_min: float = log(min_mass * scale)
	var log_max: float = log(max_mass * scale)
	var log_mass: float = rng.randf_range(log_min, log_max)
	
	return exp(log_mass)


## Generates major asteroids for a belt.
## @param belt: The asteroid belt.
## @param host: The orbit host.
## @param stars: Star bodies.
## @param rng: Random number generator.
## @return: Array of major asteroids.
static func _generate_major_asteroids(
	belt: AsteroidBelt,
	host: OrbitHost,
	stars: Array[CelestialBody],
	rng: SeededRng
) -> Array[CelestialBody]:
	var asteroids: Array[CelestialBody] = []
	
	# Determine number of major asteroids (3-10)
	var count: int = rng.randi_range(3, MAX_MAJOR_ASTEROIDS)
	
	# Get stellar context
	var stellar_mass_kg: float = host.combined_mass_kg
	var stellar_luminosity_watts: float = host.combined_luminosity_watts
	var stellar_temperature_k: float = host.effective_temperature_k
	var stellar_age_years: float = 4.6e9
	
	if stars.size() > 0 and stars[0].has_stellar():
		stellar_age_years = stars[0].stellar.age_years
	
	# Generate asteroid sizes using power law distribution
	var sizes_km: Array[float] = _generate_asteroid_sizes(count, rng)
	
	# Generate asteroids with decreasing size
	for i in range(sizes_km.size()):
		var size_km: float = sizes_km[i]
		
		var asteroid: CelestialBody = _generate_single_major_asteroid(
			belt,
			host,
			stellar_mass_kg,
			stellar_luminosity_watts,
			stellar_temperature_k,
			stellar_age_years,
			size_km,
			i,
			rng
		)
		
		if asteroid != null:
			asteroids.append(asteroid)
	
	return asteroids


## Generates asteroid sizes using power law distribution.
## @param count: Number of asteroids to generate.
## @param rng: Random number generator.
## @return: Array of sizes in km (largest to smallest).
static func _generate_asteroid_sizes(count: int, rng: SeededRng) -> Array[float]:
	var sizes: Array[float] = []
	
	# Power law: N(>D) ∝ D^-alpha (more small than large)
	# Generate sizes from largest to smallest
	# Largest asteroids: 200-1000 km (Ceres-like)
	# Smallest major asteroids: ~100 km
	
	var max_size_km: float = 1000.0
	var min_size_km: float = MAJOR_ASTEROID_THRESHOLD_KM
	
	for i in range(count):
		# Power law sampling
		var u: float = rng.randf()
		var size_km: float = pow(
			pow(min_size_km, 1.0 - POWER_LAW_ALPHA) + u * (pow(max_size_km, 1.0 - POWER_LAW_ALPHA) - pow(min_size_km, 1.0 - POWER_LAW_ALPHA)),
			1.0 / (1.0 - POWER_LAW_ALPHA)
		)
		
		sizes.append(size_km)
	
	# Sort largest to smallest
	sizes.sort_custom(func(a: float, b: float) -> bool: return a > b)
	
	return sizes


## Generates a single major asteroid.
## @param belt: The asteroid belt.
## @param host: The orbit host.
## @param stellar_mass_kg: Star mass.
## @param stellar_luminosity_watts: Star luminosity.
## @param stellar_temperature_k: Star temperature.
## @param stellar_age_years: System age.
## @param size_km: Asteroid size in km.
## @param asteroid_index: Index of this asteroid (0 = largest).
## @param rng: Random number generator.
## @return: Generated asteroid.
static func _generate_single_major_asteroid(
	belt: AsteroidBelt,
	host: OrbitHost,
	stellar_mass_kg: float,
	stellar_luminosity_watts: float,
	stellar_temperature_k: float,
	stellar_age_years: float,
	size_km: float,
	asteroid_index: int,
	rng: SeededRng
) -> CelestialBody:
	# Determine orbital distance within belt
	var distance_fraction: float = rng.randf_range(0.1, 0.9)
	var orbital_distance: float = lerpf(belt.inner_radius_m, belt.outer_radius_m, distance_fraction)
	
	# Determine asteroid type based on belt composition
	var asteroid_type: int = _get_asteroid_type_for_composition(belt.composition, rng)
	
	# Create spec
	var asteroid_seed: int = rng.randi()
	var spec: AsteroidSpec
	
	# First asteroid could be Ceres-like (dwarf planet sized)
	if asteroid_index == 0 and size_km >= 400.0:
		spec = AsteroidSpec.ceres_like(asteroid_seed)
		spec.asteroid_type = asteroid_type  # Override type for belt composition
	else:
		spec = AsteroidSpec.new(asteroid_seed, asteroid_type)
		spec.is_large = size_km >= 400.0
	
	# Override size to match power law distribution
	var radius_m: float = size_km * 1000.0  # km to meters
	spec.set_override("physical.radius_m", radius_m)
	
	# Override orbital distance
	spec.set_override("orbital.semi_major_axis_m", orbital_distance)
	
	# Create parent context
	var context: ParentContext = ParentContext.for_planet(
		stellar_mass_kg,
		stellar_luminosity_watts,
		stellar_temperature_k,
		stellar_age_years,
		orbital_distance
	)
	
	# Generate asteroid
	var asteroid_rng: SeededRng = SeededRng.new(asteroid_seed)
	var asteroid: CelestialBody = AsteroidGenerator.generate(spec, context, asteroid_rng)
	
	if asteroid != null:
		asteroid.id = "asteroid_%s_%d" % [belt.id, asteroid_index]
		asteroid.name = _generate_asteroid_name(belt, asteroid_index)
		
		# Set orbital parent reference
		if asteroid.has_orbital():
			asteroid.orbital.parent_id = host.node_id
	
	return asteroid


## Gets asteroid type for belt composition.
## @param composition: Belt composition.
## @param rng: Random number generator.
## @return: AsteroidType enum value.
static func _get_asteroid_type_for_composition(
	composition: AsteroidBelt.Composition,
	rng: SeededRng
) -> int:
	match composition:
		AsteroidBelt.Composition.ROCKY:
			var roll: float = rng.randf()
			if roll < 0.75:
				return AsteroidType.Type.S_TYPE
			else:
				return AsteroidType.Type.C_TYPE
		
		AsteroidBelt.Composition.ICY:
			return AsteroidType.Type.C_TYPE  # C-type includes icy objects
		
		AsteroidBelt.Composition.METALLIC:
			var roll: float = rng.randf()
			if roll < 0.60:
				return AsteroidType.Type.M_TYPE
			else:
				return AsteroidType.Type.S_TYPE
		
		AsteroidBelt.Composition.MIXED:
			var roll: float = rng.randf()
			if roll < 0.50:
				return AsteroidType.Type.C_TYPE
			elif roll < 0.85:
				return AsteroidType.Type.S_TYPE
			else:
				return AsteroidType.Type.M_TYPE
		
		_:
			return AsteroidType.Type.C_TYPE


## Generates a name for a major asteroid.
## @param belt: The belt containing the asteroid.
## @param asteroid_index: Index of the asteroid.
## @return: Asteroid name.
static func _generate_asteroid_name(belt: AsteroidBelt, asteroid_index: int) -> String:
	# Use numbered naming like real asteroids
	var base_number: int = 1 + asteroid_index
	
	if belt.name.contains("Inner"):
		return "%d %s" % [base_number, belt.name.replace("Inner Asteroid Belt", "Ceres-family")]
	elif belt.name.contains("Outer"):
		return "%d %s" % [base_number, belt.name.replace("Outer Asteroid Belt", "TNO")]
	else:
		return "%d %s" % [base_number, belt.id]


## Returns asteroids for a specific belt.
## @param asteroids: Array of all asteroids.
## @param belt: The belt.
## @return: Array of asteroids in the belt.
static func get_asteroids_for_belt(
	asteroids: Array[CelestialBody],
	belt: AsteroidBelt
) -> Array[CelestialBody]:
	var result: Array[CelestialBody] = []
	
	for asteroid in asteroids:
		if belt.major_asteroid_ids.has(asteroid.id):
			result.append(asteroid)
	
	return result


## Sorts asteroids by mass (largest first).
## @param asteroids: Array of asteroids.
static func sort_by_mass(asteroids: Array[CelestialBody]) -> void:
	asteroids.sort_custom(func(a: CelestialBody, b: CelestialBody) -> bool:
		return a.physical.mass_kg > b.physical.mass_kg
	)


## Calculates statistics about belts and asteroids.
## @param belts: Array of belts.
## @param asteroids: Array of asteroids.
## @return: Dictionary with statistics.
static func get_statistics(belts: Array[AsteroidBelt], asteroids: Array[CelestialBody]) -> Dictionary:
	var stats: Dictionary = {
		"total_belts": belts.size(),
		"total_asteroids": asteroids.size(),
		"inner_belts": 0,
		"outer_belts": 0,
		"rocky_belts": 0,
		"icy_belts": 0,
		"mixed_belts": 0,
		"metallic_belts": 0,
		"total_belt_mass_kg": 0.0,
		"avg_asteroids_per_belt": 0.0,
	}
	
	for belt in belts:
		stats["total_belt_mass_kg"] += belt.total_mass_kg
		
		if belt.name.contains("Inner"):
			stats["inner_belts"] += 1
		elif belt.name.contains("Outer"):
			stats["outer_belts"] += 1
		
		match belt.composition:
			AsteroidBelt.Composition.ROCKY:
				stats["rocky_belts"] += 1
			AsteroidBelt.Composition.ICY:
				stats["icy_belts"] += 1
			AsteroidBelt.Composition.MIXED:
				stats["mixed_belts"] += 1
			AsteroidBelt.Composition.METALLIC:
				stats["metallic_belts"] += 1
	
	if belts.size() > 0:
		stats["avg_asteroids_per_belt"] = float(asteroids.size()) / float(belts.size())
	
	return stats


## Validates belt placement.
## @param belts: Array of belts.
## @param filled_slots: Array of filled orbit slots.
## @return: True if no belts overlap with planets.
static func validate_belt_placement(
	belts: Array[AsteroidBelt],
	filled_slots: Array[OrbitSlot]
) -> bool:
	for belt in belts:
		for slot in filled_slots:
			if slot.orbit_host_id != belt.orbit_host_id:
				continue
			
			if not slot.is_filled:
				continue
			
			# Check if planet is inside belt
			var planet_distance: float = slot.semi_major_axis_m
			if planet_distance >= belt.inner_radius_m and planet_distance <= belt.outer_radius_m:
				return false
	
	return true
