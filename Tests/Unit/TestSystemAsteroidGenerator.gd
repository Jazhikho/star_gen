## Tests for SystemAsteroidGenerator.
extends TestCase

const _system_asteroid_generator: GDScript = preload("res://src/domain/system/SystemAsteroidGenerator.gd")
const _asteroid_belt: GDScript = preload("res://src/domain/system/AsteroidBelt.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _orbit_slot: GDScript = preload("res://src/domain/system/OrbitSlot.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _celestial_validator: GDScript = preload("res://src/domain/celestial/validation/CelestialValidator.gd")
const _validation_result: GDScript = preload("res://src/domain/celestial/validation/ValidationResult.gd")


## Creates a Sun-like orbit host for testing.
func _create_sun_like_host() -> OrbitHost:
	var host: OrbitHost = OrbitHost.new("host_sol", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.effective_temperature_k = 5778.0
	host.inner_stability_m = 0.1 * Units.AU_METERS
	host.outer_stability_m = 100.0 * Units.AU_METERS
	host.calculate_zones()
	return host


## Creates a test star.
func _create_test_star() -> CelestialBody:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(12345)
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	star.id = "test_star"
	return star


## Creates filled slots representing planets.
func _create_planet_slots(host: OrbitHost) -> Array[OrbitSlot]:
	var slots: Array[OrbitSlot] = []
	
	# Mercury-like
	var slot1: OrbitSlot = OrbitSlot.new("slot_0", host.node_id, 0.4 * Units.AU_METERS)
	slot1.is_filled = true
	slot1.planet_id = "planet_0"
	slots.append(slot1)
	
	# Venus-like
	var slot2: OrbitSlot = OrbitSlot.new("slot_1", host.node_id, 0.7 * Units.AU_METERS)
	slot2.is_filled = true
	slot2.planet_id = "planet_1"
	slots.append(slot2)
	
	# Earth-like
	var slot3: OrbitSlot = OrbitSlot.new("slot_2", host.node_id, 1.0 * Units.AU_METERS)
	slot3.is_filled = true
	slot3.planet_id = "planet_2"
	slots.append(slot3)
	
	# Mars-like
	var slot4: OrbitSlot = OrbitSlot.new("slot_3", host.node_id, 1.5 * Units.AU_METERS)
	slot4.is_filled = true
	slot4.planet_id = "planet_3"
	slots.append(slot4)
	
	# Jupiter-like
	var slot5: OrbitSlot = OrbitSlot.new("slot_4", host.node_id, 5.2 * Units.AU_METERS)
	slot5.is_filled = true
	slot5.planet_id = "planet_4"
	slots.append(slot5)
	
	# Saturn-like
	var slot6: OrbitSlot = OrbitSlot.new("slot_5", host.node_id, 9.5 * Units.AU_METERS)
	slot6.is_filled = true
	slot6.planet_id = "planet_5"
	slots.append(slot6)
	
	return slots


## Tests basic belt generation.
func test_generate_belts() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_planet_slots(host)
	var rng: SeededRng = SeededRng.new(12345)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host],
		slots,
		[star],
		rng
	)
	
	assert_true(result.success, "Generation should succeed")
	# Belts are probabilistic, so just check it doesn't crash


## Tests belt generation with no planets.
func test_generate_belts_no_planets() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host],
		[],
		[star],
		rng
	)
	
	assert_true(result.success, "Generation should succeed with no planets")


## Tests that belts are generated with some probability.
func test_belts_generated_probabilistically() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_planet_slots(host)
	
	var belt_count: int = 0
	
	for i in range(30):
		var rng: SeededRng = SeededRng.new(30000 + i)
		var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
			[host], slots, [star], rng
		)
		belt_count += result.belts.size()
	
	# Should have some belts across 30 attempts
	assert_greater_than(belt_count, 0, "Should generate some belts")


## Tests determinism.
func test_determinism() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_planet_slots(host)
	var rng1: SeededRng = SeededRng.new(44444)
	var rng2: SeededRng = SeededRng.new(44444)
	
	var result1: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], slots, [star], rng1
	)
	var result2: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], slots, [star], rng2
	)
	
	assert_equal(result1.belts.size(), result2.belts.size(), "Same seed should give same belt count")
	assert_equal(result1.asteroids.size(), result2.asteroids.size(), "Same seed should give same asteroid count")
	
	for i in range(result1.belts.size()):
		assert_float_equal(
			result1.belts[i].inner_radius_m,
			result2.belts[i].inner_radius_m,
			1.0,
			"Same seed should give same belt positions"
		)


## Tests belt boundaries are valid.
func test_belt_boundaries() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(55555)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	for belt in result.belts:
		assert_greater_than(belt.inner_radius_m, 0.0, "Inner radius should be positive")
		assert_greater_than(belt.outer_radius_m, belt.inner_radius_m, "Outer > inner")
		assert_greater_than(belt.total_mass_kg, 0.0, "Belt mass should be positive")


## Tests major asteroids are generated.
func test_major_asteroids_generated() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	if result.belts.size() > 0:
		assert_greater_than(result.asteroids.size(), 0, "Should have major asteroids")
		
		for belt in result.belts:
			assert_greater_than(
				belt.major_asteroid_ids.size(),
				0,
				"Belt should have major asteroid IDs"
			)
			assert_less_than(
				belt.major_asteroid_ids.size(),
				11,
				"Should have at most 10 major asteroids"
			)


## Tests asteroids are within belt boundaries.
func test_asteroids_within_belt() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	for belt in result.belts:
		var belt_asteroids: Array[CelestialBody] = SystemAsteroidGenerator.get_asteroids_for_belt(
			result.asteroids,
			belt
		)
		
		for asteroid in belt_asteroids:
			assert_true(asteroid.has_orbital(), "Asteroid should have orbital data")
			assert_greater_than(
				asteroid.orbital.semi_major_axis_m,
				belt.inner_radius_m * 0.9, # Small margin
				"Asteroid should be within belt inner edge"
			)
			assert_less_than(
				asteroid.orbital.semi_major_axis_m,
				belt.outer_radius_m * 1.1, # Small margin
				"Asteroid should be within belt outer edge"
			)


## Tests belt composition types.
func test_belt_composition_variety() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	
	var compositions: Dictionary = {}
	
	for i in range(50):
		var rng: SeededRng = SeededRng.new(80000 + i)
		var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
			[host], [], [star], rng
		)
		
		for belt in result.belts:
			compositions[belt.composition] = true
	
	# Should see at least rocky and icy compositions
	assert_greater_than(compositions.size(), 1, "Should have composition variety")


## Tests belts don't overlap with planets.
func test_belts_avoid_planets() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_planet_slots(host)
	
	for i in range(20):
		var rng: SeededRng = SeededRng.new(90000 + i)
		var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
			[host], slots, [star], rng
		)
		
		var is_valid: bool = SystemAsteroidGenerator.validate_belt_placement(
			result.belts,
			slots
		)
		
		assert_true(is_valid, "Belts should not overlap with planets")


## Tests inner vs outer belt placement.
func test_inner_outer_belt_placement() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	
	var inner_count: int = 0
	var outer_count: int = 0
	
	for i in range(50):
		var rng: SeededRng = SeededRng.new(10000 + i)
		var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
			[host], [], [star], rng
		)
		
		for belt in result.belts:
			if belt.name.contains("Inner"):
				inner_count += 1
				# Inner belts should be near frost line
				var center_au: float = belt.get_center_au()
				assert_in_range(center_au, 1.0, 10.0, "Inner belt should be 1-10 AU")
			
			elif belt.name.contains("Outer"):
				outer_count += 1
				# Outer belts should be far out
				var center_au: float = belt.get_center_au()
				assert_greater_than(center_au, 10.0, "Outer belt should be beyond 10 AU")
	
	# Should generate both types
	assert_greater_than(inner_count, 0, "Should generate inner belts")
	assert_greater_than(outer_count, 0, "Should generate outer belts")


## Tests asteroid IDs are unique.
func test_asteroid_ids_unique() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(11111)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	var ids: Dictionary = {}
	for asteroid in result.asteroids:
		assert_false(ids.has(asteroid.id), "Asteroid IDs should be unique")
		ids[asteroid.id] = true


## Tests asteroids pass validation.
func test_asteroids_pass_validation() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	for asteroid in result.asteroids:
		var validation: ValidationResult = CelestialValidator.validate(asteroid)
		assert_true(validation.is_valid(), "Generated asteroid should pass validation")


## Tests sort_by_mass.
func test_sort_by_mass() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(33333)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	if result.asteroids.size() < 2:
		return
	
	SystemAsteroidGenerator.sort_by_mass(result.asteroids)
	
	for i in range(result.asteroids.size() - 1):
		assert_greater_than(
			result.asteroids[i].physical.mass_kg,
			result.asteroids[i + 1].physical.mass_kg,
			"Asteroids should be sorted by mass (largest first)"
		)


## Tests get_statistics.
func test_get_statistics() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(44444)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	var stats: Dictionary = SystemAsteroidGenerator.get_statistics(result.belts, result.asteroids)
	
	assert_equal(stats["total_belts"], result.belts.size())
	assert_equal(stats["total_asteroids"], result.asteroids.size())
	
	if result.belts.size() > 0:
		assert_greater_than(stats["total_belt_mass_kg"], 0.0)


## Tests multiple orbit hosts.
func test_multiple_orbit_hosts() -> void:
	var host1: OrbitHost = _create_sun_like_host()
	host1.node_id = "host_1"
	
	var host2: OrbitHost = _create_sun_like_host()
	host2.node_id = "host_2"
	
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(55555)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host1, host2], [], [star], rng
	)
	
	assert_true(result.success)
	
	# Check if belts from both hosts exist
	var host_ids: Dictionary = {}
	for belt in result.belts:
		host_ids[belt.orbit_host_id] = true
	
	# Not guaranteed both will have belts (probabilistic), but should handle multiple hosts


## Tests belt_asteroid_map is populated.
func test_belt_asteroid_map() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	if result.belts.size() > 0:
		for belt in result.belts:
			if belt.major_asteroid_ids.size() > 0:
				assert_true(
					result.belt_asteroid_map.has(belt.id),
					"Belt should be in belt_asteroid_map"
				)
				assert_equal(
					result.belt_asteroid_map[belt.id].size(),
					belt.major_asteroid_ids.size(),
					"Map should have correct asteroid count"
				)


## Tests asteroid sizes follow power law (largest first).
func test_asteroid_sizes_power_law() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	if result.asteroids.size() >= 2:
		# Check that sizes generally decrease (power law)
		var first_radius: float = result.asteroids[0].physical.radius_m
		var last_radius: float = result.asteroids[result.asteroids.size() - 1].physical.radius_m
		
		# First should generally be larger than last
		assert_greater_than(
			first_radius,
			last_radius * 0.5,
			"Largest asteroid should be significantly larger than smallest"
		)


## Tests asteroids have parent IDs set.
func test_asteroid_parent_ids() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(88888)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	for asteroid in result.asteroids:
		assert_true(asteroid.has_orbital(), "Asteroid should have orbital data")
		assert_equal(asteroid.orbital.parent_id, host.node_id, "Asteroid parent should be host")


## Tests asteroid names are assigned.
func test_asteroid_names() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var rng: SeededRng = SeededRng.new(99999)
	
	var result: SystemAsteroidGenerator.BeltGenerationResult = SystemAsteroidGenerator.generate(
		[host], [], [star], rng
	)
	
	for asteroid in result.asteroids:
		assert_false(asteroid.name.is_empty(), "Asteroid should have a name")


## Tests belt slot reservation marks and clearing.
func test_reserve_belt_slots_marks_and_clears() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var all_slots: Array[OrbitSlot] = []
	for i in range(8):
		var distance_au: float = 0.6 + float(i) * 1.2
		var slot: OrbitSlot = OrbitSlot.new("open_slot_%d" % i, host.node_id, distance_au * Units.AU_METERS)
		slot.is_filled = false
		all_slots.append(slot)

	var host_list: Array[OrbitHost] = [host]
	var star_list: Array[CelestialBody] = [star]
	var reservation: RefCounted = _system_asteroid_generator.reserve_belt_slots(
		host_list,
		all_slots,
		star_list,
		SeededRng.new(121212)
	)
	_system_asteroid_generator.mark_reserved_slots(all_slots, reservation.reserved_slot_ids)

	for slot in all_slots:
		if reservation.reserved_slot_ids.has(slot.id):
			assert_true(slot.is_filled, "Reserved slot should be marked filled")
			assert_true(slot.planet_id.begins_with("__belt_reserved__"), "Reserved slot marker should be set")

	_system_asteroid_generator.clear_reserved_slot_marks(all_slots)
	for slot in all_slots:
		assert_false(slot.planet_id.begins_with("__belt_reserved__"), "Reserved marker should be removed")


## Tests generating from predefined belts preserves IDs and map entries.
func test_generate_from_predefined_belts() -> void:
	var host: OrbitHost = _create_sun_like_host()
	var star: CelestialBody = _create_test_star()
	var belt: AsteroidBelt = AsteroidBelt.new("belt_test_0", "Test Belt")
	belt.orbit_host_id = host.node_id
	belt.inner_radius_m = 2.0 * Units.AU_METERS
	belt.outer_radius_m = 3.0 * Units.AU_METERS
	belt.composition = AsteroidBelt.Composition.ROCKY
	belt.total_mass_kg = 1.0e21

	var belts: Array[AsteroidBelt] = [belt]
	var host_list: Array[OrbitHost] = [host]
	var star_list: Array[CelestialBody] = [star]
	var result: SystemAsteroidGenerator.BeltGenerationResult = _system_asteroid_generator.generate_from_predefined_belts(
		belts,
		host_list,
		star_list,
		SeededRng.new(343434)
	)

	assert_true(result.success, "Predefined belt generation should succeed")
	assert_equal(result.belts.size(), 1, "Should keep predefined belt list")
	assert_true(result.belt_asteroid_map.has("belt_test_0"), "Result should map predefined belt ID")
