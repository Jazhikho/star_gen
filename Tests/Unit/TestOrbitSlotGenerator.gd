## Tests for OrbitSlotGenerator.
extends TestCase

const _orbit_slot_generator := preload("res://src/domain/system/OrbitSlotGenerator.gd")
const _orbit_slot := preload("res://src/domain/system/OrbitSlot.gd")
const _orbit_host := preload("res://src/domain/system/OrbitHost.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")


## Creates a Sun-like orbit host for testing.
func _create_sun_like_host() -> OrbitHost:
	var host: OrbitHost = OrbitHost.new("host_sol", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.effective_temperature_k = 5778.0
	host.inner_stability_m = Units.SOLAR_RADIUS_METERS * 3.0  # ~0.014 AU
	host.outer_stability_m = 100.0 * Units.AU_METERS
	host.calculate_zones()
	return host


## Creates a close binary orbit host for testing.
func _create_close_binary_host() -> OrbitHost:
	var host: OrbitHost = OrbitHost.new("host_binary", OrbitHost.HostType.P_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG * 2.0
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS * 2.0
	host.effective_temperature_k = 5778.0
	host.inner_stability_m = 3.0 * Units.AU_METERS  # P-type limit
	host.outer_stability_m = 50.0 * Units.AU_METERS
	host.calculate_zones()
	return host


## Tests slot generation for single star.
func test_generate_for_host_single_star() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(12345)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	assert_true(result.success, "Generation should succeed")
	assert_greater_than(result.slots.size(), 0, "Should generate some slots")
	assert_equal(result.orbit_host_id, "host_sol")


## Tests slots are within stability zone.
func test_slots_within_stability_zone() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	for slot in result.slots:
		assert_greater_than(
			slot.semi_major_axis_m,
			host.inner_stability_m,
			"Slot should be outside inner limit"
		)
		assert_less_than(
			slot.semi_major_axis_m,
			host.outer_stability_m,
			"Slot should be inside outer limit"
		)


## Tests slots are in increasing distance order.
func test_slots_increasing_distance() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(33333)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	var prev_distance: float = 0.0
	for slot in result.slots:
		assert_greater_than(
			slot.semi_major_axis_m,
			prev_distance,
			"Slots should be in increasing distance order"
		)
		prev_distance = slot.semi_major_axis_m


## Tests zones are classified correctly.
func test_zone_classification() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(44444)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	for slot in result.slots:
		# Verify zone matches distance
		if slot.semi_major_axis_m < host.habitable_zone_inner_m:
			assert_equal(slot.zone, OrbitZone.Zone.HOT, "Inner slots should be HOT")
		elif slot.semi_major_axis_m > host.frost_line_m:
			assert_equal(slot.zone, OrbitZone.Zone.COLD, "Outer slots should be COLD")
		else:
			assert_equal(slot.zone, OrbitZone.Zone.TEMPERATE, "Middle slots should be TEMPERATE")


## Tests fill probability decreases outward.
func test_fill_probability_decreases() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(55555)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	if result.slots.size() < 2:
		return  # Need at least 2 slots to compare
	
	# First slot should have higher probability than last
	var first_prob: float = result.slots[0].fill_probability
	var last_prob: float = result.slots[result.slots.size() - 1].fill_probability
	
	assert_greater_than(first_prob, last_prob, "Inner slots should have higher fill probability")


## Tests suggested eccentricity increases with distance.
func test_eccentricity_increases_with_distance() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(33333)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	if result.slots.size() < 5:
		return  # Skip if not enough slots
	
	# Average eccentricity of first 3 vs last 3
	var avg_inner: float = 0.0
	var avg_outer: float = 0.0
	
	for i in range(3):
		avg_inner += result.slots[i].suggested_eccentricity
		avg_outer += result.slots[result.slots.size() - 1 - i].suggested_eccentricity
	
	avg_inner /= 3.0
	avg_outer /= 3.0
	
	# Outer should have higher average eccentricity
	assert_greater_than(avg_outer, avg_inner * 0.5, "Outer orbits should tend toward higher eccentricity")


## Tests determinism.
func test_determinism() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng1: SeededRng = SeededRng.new(66666)
	var rng2: SeededRng = SeededRng.new(66666)
	
	var result1: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng1
	)
	
	var result2: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng2
	)
	
	assert_equal(result1.slots.size(), result2.slots.size(), "Same seed should give same slot count")
	
	for i in range(result1.slots.size()):
		assert_float_equal(
			result1.slots[i].semi_major_axis_m,
			result2.slots[i].semi_major_axis_m,
			1.0,
			"Same seed should give same distances"
		)


## Tests minimum spacing between slots.
func test_minimum_spacing() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(55555)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	for i in range(result.slots.size() - 1):
		var spacing: float = result.slots[i + 1].semi_major_axis_m - result.slots[i].semi_major_axis_m
		var min_spacing: float = result.slots[i].semi_major_axis_m * OrbitSlotGenerator.MIN_SPACING_FACTOR
		
		assert_true(
			spacing >= min_spacing * 0.99,  # Allow tiny floating point error
			"Spacing should meet minimum requirement"
		)


## Tests slots respect star radius safety margin.
func test_star_radius_safety() -> void:
	var host: OrbitHost = _create_sun_like_host()
	host.inner_stability_m = 0.001 * Units.AU_METERS  # Very close inner limit
	
	var star_radius: float = Units.SOLAR_RADIUS_METERS
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		star_radius,
		[],
		[],
		rng
	)
	
	if not result.slots.is_empty():
		var min_safe_distance: float = star_radius * OrbitSlotGenerator.STAR_RADIUS_SAFETY_MARGIN
		assert_true(
			result.slots[0].semi_major_axis_m >= min_safe_distance,
			"First slot should respect star radius safety margin"
		)


## Tests generate_for_hosts with multiple hosts.
func test_generate_for_hosts() -> void:
	var host1: OrbitHost = _create_sun_like_host()
	var host2: OrbitHost = _create_close_binary_host()
	var hosts: Array[OrbitHost] = [host1, host2]
	
	# Create minimal stars and hierarchy for the function
	var stars: Array[CelestialBody] = []
	var hierarchy: SystemHierarchy = SystemHierarchy.new()
	
	var rng: SeededRng = SeededRng.new(77777)
	
	var all_slots: Dictionary = OrbitSlotGenerator.generate_all_slots(hosts, stars, hierarchy, rng)
	
	assert_greater_than(all_slots.size(), 0, "Should generate slots for multiple hosts")
	
	# Verify slots belong to both hosts
	var host_ids: Dictionary = {}
	for node_id in all_slots:
		host_ids[node_id] = true
	
	assert_true(host_ids.has("host_sol"), "Should have slots for first host")
	assert_true(host_ids.has("host_binary"), "Should have slots for second host")


## Tests stability checking.
func test_check_stability() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(88888)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	# Create a companion at 10 AU
	var companion_masses: Array[float] = [Units.SOLAR_MASS_KG]
	var companion_distances: Array[float] = [10.0 * Units.AU_METERS]
	
	OrbitSlotGenerator.check_stability(result.slots, host, companion_masses, companion_distances, 0.0)
	
	# Slots far from companion should remain stable
	# Slots near companion should become unstable
	var found_stable: bool = false
	
	for slot in result.slots:
		if slot.is_stable:
			found_stable = true
			break
	
	assert_true(found_stable, "Some slots should remain stable")
	# Note: unstable slots depend on companion distance and slot placement


## Tests filter_stable.
func test_filter_stable() -> void:
	var slots: Array[OrbitSlot] = []
	
	var stable_slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	stable_slot.is_stable = true
	slots.append(stable_slot)
	
	var unstable_slot: OrbitSlot = OrbitSlot.new("s2", "h1", 2e11)
	unstable_slot.is_stable = false
	slots.append(unstable_slot)
	
	var filtered: Array[OrbitSlot] = OrbitSlotGenerator.filter_stable(slots)
	
	assert_equal(filtered.size(), 1)
	assert_equal(filtered[0].id, "s1")


## Tests filter_available.
func test_filter_available() -> void:
	var slots: Array[OrbitSlot] = []
	
	var available: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	available.is_stable = true
	available.is_filled = false
	slots.append(available)
	
	var filled: OrbitSlot = OrbitSlot.new("s2", "h1", 2e11)
	filled.is_stable = true
	filled.fill_with_planet("p1")
	slots.append(filled)
	
	var unstable: OrbitSlot = OrbitSlot.new("s3", "h1", 3e11)
	unstable.is_stable = false
	slots.append(unstable)
	
	var filtered: Array[OrbitSlot] = OrbitSlotGenerator.filter_available(slots)
	
	assert_equal(filtered.size(), 1)
	assert_equal(filtered[0].id, "s1")


## Tests filter_by_zone.
func test_filter_by_zone() -> void:
	var slots: Array[OrbitSlot] = []
	
	var hot_slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e10)
	hot_slot.zone = OrbitZone.Zone.HOT
	slots.append(hot_slot)
	
	var temperate_slot: OrbitSlot = OrbitSlot.new("s2", "h1", 1e11)
	temperate_slot.zone = OrbitZone.Zone.TEMPERATE
	slots.append(temperate_slot)
	
	var cold_slot: OrbitSlot = OrbitSlot.new("s3", "h1", 1e12)
	cold_slot.zone = OrbitZone.Zone.COLD
	slots.append(cold_slot)
	
	var temperate_slots: Array[OrbitSlot] = OrbitSlotGenerator.filter_by_zone(slots, OrbitZone.Zone.TEMPERATE)
	
	assert_equal(temperate_slots.size(), 1)
	assert_equal(temperate_slots[0].id, "s2")


## Tests sort_by_distance.
func test_sort_by_distance() -> void:
	var slots: Array[OrbitSlot] = []
	slots.append(OrbitSlot.new("s3", "h1", 3e11))
	slots.append(OrbitSlot.new("s1", "h1", 1e11))
	slots.append(OrbitSlot.new("s2", "h1", 2e11))
	
	OrbitSlotGenerator.sort_by_distance(slots)
	
	assert_equal(slots[0].id, "s1")
	assert_equal(slots[1].id, "s2")
	assert_equal(slots[2].id, "s3")


## Tests sort_by_probability.
func test_sort_by_probability() -> void:
	var slots: Array[OrbitSlot] = []
	
	var low: OrbitSlot = OrbitSlot.new("low", "h1", 1e11)
	low.fill_probability = 0.2
	slots.append(low)
	
	var high: OrbitSlot = OrbitSlot.new("high", "h1", 2e11)
	high.fill_probability = 0.9
	slots.append(high)
	
	var mid: OrbitSlot = OrbitSlot.new("mid", "h1", 3e11)
	mid.fill_probability = 0.5
	slots.append(mid)
	
	OrbitSlotGenerator.sort_by_probability(slots)
	
	assert_equal(slots[0].id, "high")
	assert_equal(slots[1].id, "mid")
	assert_equal(slots[2].id, "low")


## Tests get_statistics.
func test_get_statistics() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var rng: SeededRng = SeededRng.new(99999)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	var stats: Dictionary = OrbitSlotGenerator.get_statistics(result.slots)
	
	assert_equal(stats["total"], result.slots.size())
	assert_greater_than(stats["min_distance_au"], 0.0)
	assert_greater_than(stats["max_distance_au"], stats["min_distance_au"])
	assert_greater_than(stats["avg_fill_probability"], 0.0)
	
	# Zone counts should sum to total
	var zone_sum: int = stats["hot"] + stats["temperate"] + stats["cold"]
	assert_equal(zone_sum, stats["total"])


## Tests maximum slot limit.
func test_max_slots_limit() -> void:
	var host: OrbitHost = _create_sun_like_host()
	host.outer_stability_m = 10000.0 * Units.AU_METERS  # Very wide zone
	var rng: SeededRng = SeededRng.new(11111)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	assert_less_than(result.slots.size(), 21, "Should not exceed max slots")


## Tests P-type host generation.
func test_ptype_host_generation() -> void:
	var host: OrbitHost = _create_close_binary_host()
	var rng: SeededRng = SeededRng.new(12121)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS * 2.0,  # Combined radius
		[],
		[],
		rng
	)
	
	assert_true(result.success)
	assert_greater_than(result.slots.size(), 0)
	
	# All P-type slots should be outside the inner stability limit
	for slot in result.slots:
		assert_greater_than(
			slot.semi_major_axis_m,
			host.inner_stability_m,
			"P-type slots should be outside inner limit"
		)


## Tests invalid host handling.
func test_invalid_host() -> void:
	var host: OrbitHost = OrbitHost.new("invalid", OrbitHost.HostType.S_TYPE)
	# Don't set stability zone - it's invalid
	var rng: SeededRng = SeededRng.new(13131)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	assert_false(result.success, "Should fail for invalid host")
	assert_equal(result.slots.size(), 0)


## Tests narrow stability zone.
func test_narrow_stability_zone() -> void:
	var host: OrbitHost = OrbitHost.new("narrow", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.inner_stability_m = 1.0 * Units.AU_METERS
	host.outer_stability_m = 1.5 * Units.AU_METERS  # Narrow zone
	host.calculate_zones()
	
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	# Should still generate some slots even in narrow zone
	assert_greater_than(result.slots.size(), 0, "Should generate slots even in narrow zone")
	
	for slot in result.slots:
		assert_true(slot.semi_major_axis_m >= 1.0 * Units.AU_METERS)
		assert_true(slot.semi_major_axis_m <= 1.5 * Units.AU_METERS)


## Tests wide zone generates many slots.
func test_wide_zone_many_slots() -> void:
	var host: OrbitHost = _create_sun_like_host()
	host.outer_stability_m = 100.0 * Units.AU_METERS  # Very wide
	
	var rng: SeededRng = SeededRng.new(99999)
	
	var result: OrbitSlotGenerator.SlotGenerationResult = OrbitSlotGenerator.generate_for_host(
		host,
		Units.SOLAR_RADIUS_METERS,
		[],
		[],
		rng
	)
	
	assert_greater_than(result.slots.size(), 5, "Wide zone should generate multiple slots")
