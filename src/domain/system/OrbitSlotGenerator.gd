## Generates candidate orbital slots for planets within orbit hosts.
## Uses resonance-based spacing with probability curves favoring inner orbits.
class_name OrbitSlotGenerator
extends RefCounted

const _orbit_slot: GDScript = preload("res://src/domain/system/OrbitSlot.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _orbital_mechanics: GDScript = preload("res://src/domain/system/OrbitalMechanics.gd")
const _orbit_zone: GDScript = preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _system_hierarchy: GDScript = preload("res://src/domain/system/SystemHierarchy.gd")
const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")


## Minimum spacing factor between adjacent slots (as fraction of inner orbit).
const MIN_SPACING_FACTOR: float = 0.15

## Maximum number of slots to generate per host.
const MAX_SLOTS_PER_HOST: int = 20

## Resonance variation (Â±20%).
const RESONANCE_VARIATION: float = 0.20

## Exponential decay constant for fill probability.
## Higher = steeper drop-off with distance.
const PROBABILITY_DECAY: float = 0.15

## Safety margin for star radius (orbit must be this many radii away).
const STAR_RADIUS_SAFETY_MARGIN: float = 3.0


## Result of orbit slot generation for a single host.
class SlotGenerationResult:
	extends RefCounted
	
	## Generated slots.
	var slots: Array[OrbitSlot]
	
	## The orbit host these slots belong to.
	var orbit_host_id: String
	
	## Whether generation succeeded.
	var success: bool
	
	func _init() -> void:
		slots = []
		orbit_host_id = ""
		success = false


## Generates orbit slots for a single orbit host.
## @param host: The orbit host.
## @param star_radius_m: Radius of host star(s) for safety margin check.
## @param companion_positions_m: Positions of companion stars (for stability).
## @param companion_masses_kg: Masses of companion stars.
## @param rng: Random number generator.
## @return: SlotGenerationResult with generated slots.
static func generate_for_host(
	host: OrbitHost,
	star_radius_m: float,
	companion_positions_m: Array[float],
	companion_masses_kg: Array[float],
	rng: SeededRng
) -> SlotGenerationResult:
	var result: SlotGenerationResult = SlotGenerationResult.new()
	result.orbit_host_id = host.node_id
	
	if not host.has_valid_zone():
		result.success = false
		return result
	
	var slots: Array[OrbitSlot] = []
	var slot_index: int = 0
	
	# Determine starting position (just inside inner stability limit with safety margin)
	var inner_limit: float = host.inner_stability_m
	var outer_limit: float = host.outer_stability_m
	
	# Start first orbit at inner limit or star radius safety margin, whichever is larger
	var min_safe_distance: float = star_radius_m * STAR_RADIUS_SAFETY_MARGIN
	var starting_distance: float = maxf(inner_limit, min_safe_distance)
	
	# Add small random offset
	var first_orbit_factor: float = rng.randf_range(1.05, 1.2)
	var current_distance: float = starting_distance * first_orbit_factor
	
	# Get resonance ratios
	var resonance_ratios: Array[float] = OrbitalMechanics.get_common_resonance_ratios()
	
	# Generate slots outward
	while current_distance < outer_limit and slot_index < MAX_SLOTS_PER_HOST:
		# Create slot
		var slot: OrbitSlot = OrbitSlot.new(
			"slot_%s_%d" % [host.node_id, slot_index],
			host.node_id,
			current_distance
		)
		
		# Classify zone
		slot.zone = _classify_zone(current_distance, host)
		
		# Calculate suggested eccentricity (lower for inner orbits)
		slot.suggested_eccentricity = _calculate_suggested_eccentricity(
			current_distance,
			host,
			rng
		)
		
		# Check stability
		slot.is_stable = _check_slot_stability(
			current_distance,
			host,
			companion_positions_m,
			companion_masses_kg
		)
		
		# Calculate fill probability (exponential decay with distance)
		slot.fill_probability = _calculate_fill_probability(current_distance, host)
		
		slots.append(slot)
		slot_index += 1
		
		# Calculate next slot distance using resonance spacing
		var ratio_index: int = rng.randi_range(0, resonance_ratios.size() - 1)
		var ratio: float = resonance_ratios[ratio_index]
		
		var next_distance: float = OrbitalMechanics.calculate_resonance_spacing(
			current_distance,
			ratio,
			RESONANCE_VARIATION,
			rng
		)
		
		# Ensure minimum spacing
		var min_spacing: float = current_distance * MIN_SPACING_FACTOR
		if next_distance - current_distance < min_spacing:
			next_distance = current_distance + min_spacing
		
		current_distance = next_distance
	
	result.slots = slots
	result.success = true
	return result


## Generates orbit slots for multiple hosts.
## @param hosts: Array of orbit hosts.
## @param stars: Array of star bodies (for radius lookups).
## @param hierarchy: System hierarchy (for companion detection).
## @param rng: Random number generator.
## @return: Dictionary mapping host node_id to Array[OrbitSlot].
static func generate_all_slots(
	hosts: Array[OrbitHost],
	stars: Array[CelestialBody],
	hierarchy: SystemHierarchy,
	rng: SeededRng
) -> Dictionary:
	var all_slots: Dictionary = {}  # node_id -> Array[OrbitSlot]
	
	for host in hosts:
		# Get star radius for this host
		var star_radius_m: float = _get_host_star_radius(host, stars, hierarchy)
		
		# Get companion positions/masses (simplified for now - empty arrays)
		var companion_positions: Array[float] = []
		var companion_masses: Array[float] = []
		
		# Generate slots
		var result: SlotGenerationResult = generate_for_host(
			host,
			star_radius_m,
			companion_positions,
			companion_masses,
			rng
		)
		
		if result.success:
			all_slots[host.node_id] = result.slots
	
	return all_slots


## Gets the radius of the star(s) for an orbit host.
## @param host: The orbit host.
## @param stars: Array of star bodies.
## @param hierarchy: System hierarchy.
## @return: Star radius in meters (or max if multiple).
static func _get_host_star_radius(
	host: OrbitHost,
	stars: Array[CelestialBody],
	hierarchy: SystemHierarchy
) -> float:
	var node: HierarchyNode = hierarchy.find_node(host.node_id)
	if node == null:
		return Units.SOLAR_RADIUS_METERS  # Fallback
	
	# Get all stars under this node
	var star_ids: Array[String] = node.get_all_star_ids()
	
	if star_ids.is_empty():
		return Units.SOLAR_RADIUS_METERS
	
	# For S-type, single star
	if host.host_type == OrbitHost.HostType.S_TYPE and star_ids.size() == 1:
		for star in stars:
			if star.id == star_ids[0]:
				return star.physical.radius_m
	
	# For P-type or multiple stars, use max radius for safety
	var max_radius: float = 0.0
	for star_id in star_ids:
		for star in stars:
			if star.id == star_id:
				max_radius = maxf(max_radius, star.physical.radius_m)
	
	return max_radius if max_radius > 0.0 else Units.SOLAR_RADIUS_METERS


## Classifies an orbital distance into a zone.
## @param distance_m: Orbital distance in meters.
## @param host: The orbit host.
## @return: OrbitZone.Zone value.
static func _classify_zone(distance_m: float, host: OrbitHost) -> OrbitZone.Zone:
	if distance_m < host.habitable_zone_inner_m:
		return OrbitZone.Zone.HOT
	elif distance_m > host.frost_line_m:
		return OrbitZone.Zone.COLD
	else:
		return OrbitZone.Zone.TEMPERATE


## Calculates suggested eccentricity for a slot.
## Closer orbits tend to be more circular (tidal circularization).
## @param distance_m: Distance from host.
## @param host: The orbit host.
## @param rng: Random number generator.
## @return: Suggested eccentricity (0.0-1.0).
static func _calculate_suggested_eccentricity(
	distance_m: float,
	host: OrbitHost,
	rng: SeededRng
) -> float:
	# Use distance as fraction of total zone
	var zone_width: float = host.outer_stability_m - host.inner_stability_m
	if zone_width <= 0.0:
		return 0.0
	
	var distance_fraction: float = (distance_m - host.inner_stability_m) / zone_width
	
	# Max eccentricity increases with distance
	var max_ecc: float = lerpf(0.05, 0.3, distance_fraction)
	
	# Random value biased toward circular
	var raw: float = rng.randf()
	return raw * raw * max_ecc


## Calculates fill probability based on distance.
## Uses exponential decay - planets more likely closer in.
## @param distance_m: Distance from host.
## @param host: The orbit host.
## @return: Probability (0.0-1.0).
static func _calculate_fill_probability(
	distance_m: float,
	host: OrbitHost
) -> float:
	# Ensure slots outside stability zone get zero
	if distance_m < host.inner_stability_m or distance_m > host.outer_stability_m:
		return 0.0
	
	# Normalize to AU for consistent probability regardless of star size
	var distance_au: float = distance_m / Units.AU_METERS
	
	# Gentle exponential decay in AU:
	# 0.1 AU: P â‰ˆ 0.99
	# 0.5 AU: P â‰ˆ 0.93
	# 1.0 AU: P â‰ˆ 0.86
	# 5.0 AU: P â‰ˆ 0.47
	# 10 AU:  P â‰ˆ 0.22
	# 30 AU:  P â‰ˆ 0.01 (clamped to 0.02)
	var probability: float = exp(-PROBABILITY_DECAY * distance_au)
	
	return clampf(probability, 0.02, 1.0)


## Checks if a slot is dynamically stable.
## @param distance_m: Distance from host.
## @param host: The orbit host.
## @param companion_positions_m: Companion star positions.
## @param companion_masses_kg: Companion star masses.
## @return: True if stable.
static func _check_slot_stability(
	distance_m: float,
	host: OrbitHost,
	companion_positions_m: Array[float],
	companion_masses_kg: Array[float]
) -> bool:
	# Already within host's stability zone by construction
	# Check against companion perturbations
	
	if companion_positions_m.is_empty():
		return true
	
	return OrbitalMechanics.is_orbit_stable(
		distance_m,
		host.combined_mass_kg,
		0.0,  # Simplified: host at origin
		companion_masses_kg,
		companion_positions_m
	)


## Marks slots as unstable if they're perturbed by companion bodies.
## @param slots: Array of slots to check.
## @param host: The orbit host for these slots.
## @param companion_masses_kg: Masses of companion stars/bodies.
## @param companion_distances_m: Distances of companions from system barycenter.
## @param host_position_m: Position of the host from system barycenter.
static func check_stability(
	slots: Array[OrbitSlot],
	host: OrbitHost,
	companion_masses_kg: Array[float],
	companion_distances_m: Array[float],
	host_position_m: float = 0.0
) -> void:
	for slot in slots:
		var is_stable: bool = OrbitalMechanics.is_orbit_stable(
			slot.semi_major_axis_m,
			host.combined_mass_kg,
			host_position_m,
			companion_masses_kg,
			companion_distances_m
		)
		slot.is_stable = is_stable


## Filters slots to only return stable ones.
## @param slots: Array of slots.
## @return: Array of stable slots only.
static func filter_stable(slots: Array[OrbitSlot]) -> Array[OrbitSlot]:
	var result: Array[OrbitSlot] = []
	for slot in slots:
		if slot.is_stable:
			result.append(slot)
	return result


## Filters slots to only return available ones (stable and unfilled).
## @param slots: Array of slots.
## @return: Array of available slots only.
static func filter_available(slots: Array[OrbitSlot]) -> Array[OrbitSlot]:
	var result: Array[OrbitSlot] = []
	for slot in slots:
		if slot.is_available():
			result.append(slot)
	return result


## Filters slots by zone.
## @param slots: Array of slots.
## @param zone: Zone to filter for.
## @return: Array of slots in the specified zone.
static func filter_by_zone(slots: Array[OrbitSlot], zone: OrbitZone.Zone) -> Array[OrbitSlot]:
	var result: Array[OrbitSlot] = []
	for slot in slots:
		if slot.zone == zone:
			result.append(slot)
	return result


## Sorts slots by distance (innermost first).
## @param slots: Array of slots to sort.
static func sort_by_distance(slots: Array[OrbitSlot]) -> void:
	slots.sort_custom(func(a: OrbitSlot, b: OrbitSlot) -> bool:
		return a.semi_major_axis_m < b.semi_major_axis_m
	)


## Sorts slots by fill probability (highest first).
## @param slots: Array of slots to sort.
static func sort_by_probability(slots: Array[OrbitSlot]) -> void:
	slots.sort_custom(func(a: OrbitSlot, b: OrbitSlot) -> bool:
		return a.fill_probability > b.fill_probability
	)


## Returns statistics about the slots.
## @param slots: Array of slots.
## @return: Dictionary with statistics.
static func get_statistics(slots: Array[OrbitSlot]) -> Dictionary:
	var stats: Dictionary = {
		"total": slots.size(),
		"stable": 0,
		"unstable": 0,
		"filled": 0,
		"available": 0,
		"hot": 0,
		"temperate": 0,
		"cold": 0,
		"min_distance_au": 0.0,
		"max_distance_au": 0.0,
		"avg_fill_probability": 0.0,
	}
	
	if slots.is_empty():
		return stats
	
	var prob_sum: float = 0.0
	var min_dist: float = slots[0].semi_major_axis_m
	var max_dist: float = slots[0].semi_major_axis_m
	
	for slot in slots:
		if slot.is_stable:
			stats["stable"] += 1
		else:
			stats["unstable"] += 1
		
		if slot.is_filled:
			stats["filled"] += 1
		
		if slot.is_available():
			stats["available"] += 1
		
		match slot.zone:
			OrbitZone.Zone.HOT:
				stats["hot"] += 1
			OrbitZone.Zone.TEMPERATE:
				stats["temperate"] += 1
			OrbitZone.Zone.COLD:
				stats["cold"] += 1
		
		prob_sum += slot.fill_probability
		min_dist = minf(min_dist, slot.semi_major_axis_m)
		max_dist = maxf(max_dist, slot.semi_major_axis_m)
	
	stats["min_distance_au"] = min_dist / Units.AU_METERS
	stats["max_distance_au"] = max_dist / Units.AU_METERS
	stats["avg_fill_probability"] = prob_sum / float(slots.size())
	
	return stats
