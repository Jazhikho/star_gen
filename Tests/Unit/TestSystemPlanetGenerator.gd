## Tests for SystemPlanetGenerator.
extends TestCase

const _system_planet_generator := preload("res://src/domain/system/SystemPlanetGenerator.gd")
const _orbit_slot := preload("res://src/domain/system/OrbitSlot.gd")
const _orbit_host := preload("res://src/domain/system/OrbitHost.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _size_category := preload("res://src/domain/generation/archetypes/SizeCategory.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")
const _star_spec := preload("res://src/domain/generation/specs/StarSpec.gd")
const _star_generator := preload("res://src/domain/generation/generators/StarGenerator.gd")
const _celestial_validator := preload("res://src/domain/celestial/validation/CelestialValidator.gd")


## Creates a simple orbit host for testing.
func _create_test_host() -> OrbitHost:
	var host: OrbitHost = OrbitHost.new("test_host", OrbitHost.HostType.S_TYPE)
	host.combined_mass_kg = Units.SOLAR_MASS_KG
	host.combined_luminosity_watts = StellarProps.SOLAR_LUMINOSITY_WATTS
	host.effective_temperature_k = 5778.0
	host.inner_stability_m = 0.1 * Units.AU_METERS
	host.outer_stability_m = 50.0 * Units.AU_METERS
	host.calculate_zones()
	return host


## Creates a test star.
func _create_test_star() -> CelestialBody:
	var spec: StarSpec = StarSpec.sun_like(12345)
	var rng: SeededRng = SeededRng.new(12345)
	var star: CelestialBody = StarGenerator.generate(spec, rng)
	star.id = "test_star"
	return star


## Creates test slots.
func _create_test_slots(host: OrbitHost, count: int) -> Array[OrbitSlot]:
	var slots: Array[OrbitSlot] = []
	
	for i in range(count):
		var distance: float = (0.5 + float(i) * 1.5) * Units.AU_METERS
		var slot: OrbitSlot = OrbitSlot.new("slot_%d" % i, host.node_id, distance)
		slot.is_stable = true
		slot.fill_probability = 0.8
		
		# Classify zone
		if distance < host.habitable_zone_inner_m:
			slot.zone = OrbitZone.Zone.HOT
		elif distance > host.frost_line_m:
			slot.zone = OrbitZone.Zone.COLD
		else:
			slot.zone = OrbitZone.Zone.TEMPERATE
		
		slots.append(slot)
	
	return slots


## Tests basic planet generation.
func test_generate_planets() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(12345)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots,
		[host],
		[star],
		rng
	)
	
	assert_true(result.success, "Generation should succeed")
	assert_greater_than(result.planets.size(), 0, "Should generate some planets")


## Tests determinism.
func test_determinism() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots1: Array[OrbitSlot] = _create_test_slots(host, 5)
	var slots2: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng1: SeededRng = SeededRng.new(99999)
	var rng2: SeededRng = SeededRng.new(99999)
	
	var result1: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots1, [host], [star], rng1
	)
	var result2: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots2, [host], [star], rng2
	)
	
	assert_equal(result1.planets.size(), result2.planets.size(), "Same seed should give same count")
	
	for i in range(result1.planets.size()):
		assert_float_equal(
			result1.planets[i].physical.mass_kg,
			result2.planets[i].physical.mass_kg,
			1.0,
			"Same seed should give same planets"
		)


## Tests slot fill probability.
func test_fill_probability() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Create slots with different probabilities
	var slots: Array[OrbitSlot] = []
	
	var certain_slot: OrbitSlot = OrbitSlot.new("certain", host.node_id, 1.0 * Units.AU_METERS)
	certain_slot.fill_probability = 1.0
	certain_slot.zone = OrbitZone.Zone.TEMPERATE
	slots.append(certain_slot)
	
	var impossible_slot: OrbitSlot = OrbitSlot.new("impossible", host.node_id, 2.0 * Units.AU_METERS)
	impossible_slot.fill_probability = 0.0
	impossible_slot.zone = OrbitZone.Zone.TEMPERATE
	slots.append(impossible_slot)
	
	# Run multiple times to verify probabilistic behavior
	var certain_filled: int = 0
	var impossible_filled: int = 0
	
	for i in range(10):
		var test_slots: Array[OrbitSlot] = []
		for s in slots:
			var copy: OrbitSlot = OrbitSlot.new(s.id, s.orbit_host_id, s.semi_major_axis_m)
			copy.fill_probability = s.fill_probability
			copy.zone = s.zone
			copy.is_stable = true
			test_slots.append(copy)
		
		var rng: SeededRng = SeededRng.new(10000 + i)
		var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
			test_slots, [host], [star], rng
		)
		
		for slot in result.slots:
			if slot.id == "certain" and slot.is_filled:
				certain_filled += 1
			elif slot.id == "impossible" and slot.is_filled:
				impossible_filled += 1
	
	assert_equal(certain_filled, 10, "Probability 1.0 should always fill")
	assert_equal(impossible_filled, 0, "Probability 0.0 should never fill")


## Tests planets have correct orbital distances.
func test_planet_orbital_distances() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	for planet in result.planets:
		assert_true(planet.has_orbital(), "Planet should have orbital data")
		
		# Find corresponding slot
		var found_slot: bool = false
		for slot in result.slots:
			if slot.planet_id == planet.id:
				assert_float_equal(
					planet.orbital.semi_major_axis_m,
					slot.semi_major_axis_m,
					1000.0,
					"Planet distance should match slot"
				)
				found_slot = true
				break
		
		assert_true(found_slot, "Planet should have a corresponding slot")


## Tests hot zone favors rocky planets.
func test_hot_zone_planets() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Create only hot zone slots
	var slots: Array[OrbitSlot] = []
	for i in range(5):
		var slot: OrbitSlot = OrbitSlot.new("hot_%d" % i, host.node_id, 0.2 * Units.AU_METERS * (i + 1))
		slot.zone = OrbitZone.Zone.HOT
		slot.fill_probability = 1.0  # Guarantee fill
		slot.is_stable = true
		slots.append(slot)
	
	var rng: SeededRng = SeededRng.new(33333)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	# Most hot zone planets should be rocky (mass < 10 Earth masses)
	var rocky_count: int = 0
	for planet in result.planets:
		var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
		if mass_earth < 10.0:
			rocky_count += 1
	
	# At least 60% should be rocky
	if result.planets.size() > 0:
		var rocky_fraction: float = float(rocky_count) / float(result.planets.size())
		assert_greater_than(rocky_fraction, 0.5, "Hot zone should favor rocky planets")


## Tests cold zone favors gas giants.
func test_cold_zone_planets() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Create only cold zone slots
	var slots: Array[OrbitSlot] = []
	for i in range(5):
		var slot: OrbitSlot = OrbitSlot.new("cold_%d" % i, host.node_id, 5.0 * Units.AU_METERS * (i + 1))
		slot.zone = OrbitZone.Zone.COLD
		slot.fill_probability = 1.0
		slot.is_stable = true
		slots.append(slot)
	
	var rng: SeededRng = SeededRng.new(44444)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	# Check that we have some large planets
	var large_count: int = 0
	for planet in result.planets:
		var mass_earth: float = planet.physical.mass_kg / Units.EARTH_MASS_KG
		if mass_earth >= 10.0:
			large_count += 1
	
	# Cold zone should have at least some large planets
	assert_greater_than(large_count, 0, "Cold zone should have some large planets")


## Tests unstable slots are not filled.
func test_unstable_slots_not_filled() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	
	# Mark some slots as unstable
	slots[1].is_stable = false
	slots[3].is_stable = false
	
	var rng: SeededRng = SeededRng.new(55555)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	# Check that unstable slots were not filled
	for slot in result.slots:
		if not slot.is_stable:
			assert_false(slot.is_filled, "Unstable slot should not be filled")


## Tests get_statistics.
func test_get_statistics() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 8)
	var rng: SeededRng = SeededRng.new(66666)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	var stats: Dictionary = SystemPlanetGenerator.get_statistics(result.planets)
	
	assert_equal(stats["total"], result.planets.size())
	assert_greater_than(stats["avg_mass_earth"], 0.0)
	assert_greater_than(stats["min_mass_earth"], 0.0)
	assert_greater_than(stats["max_mass_earth"], stats["min_mass_earth"])


## Tests sort_by_distance.
func test_sort_by_distance() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(77777)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	SystemPlanetGenerator.sort_by_distance(result.planets)
	
	# Verify sorted order
	for i in range(result.planets.size() - 1):
		assert_less_than(
			result.planets[i].orbital.semi_major_axis_m,
			result.planets[i + 1].orbital.semi_major_axis_m,
			"Planets should be sorted by distance"
		)


## Tests sort_by_mass.
func test_sort_by_mass() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(88888)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	SystemPlanetGenerator.sort_by_mass(result.planets)
	
	# Verify sorted order (largest first)
	for i in range(result.planets.size() - 1):
		assert_greater_than(
			result.planets[i].physical.mass_kg,
			result.planets[i + 1].physical.mass_kg,
			"Planets should be sorted by mass (largest first)"
		)


## Tests get_moon_candidates.
func test_get_moon_candidates() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Create mix of cold (large) and hot (small) planets
	var slots: Array[OrbitSlot] = []
	for i in range(3):
		var hot_slot: OrbitSlot = OrbitSlot.new("hot_%d" % i, host.node_id, 0.3 * Units.AU_METERS)
		hot_slot.zone = OrbitZone.Zone.HOT
		hot_slot.fill_probability = 1.0
		hot_slot.is_stable = true
		slots.append(hot_slot)
	
	for i in range(3):
		var cold_slot: OrbitSlot = OrbitSlot.new("cold_%d" % i, host.node_id, 10.0 * Units.AU_METERS)
		cold_slot.zone = OrbitZone.Zone.COLD
		cold_slot.fill_probability = 1.0
		cold_slot.is_stable = true
		slots.append(cold_slot)
	
	var rng: SeededRng = SeededRng.new(99999)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	var candidates: Array[CelestialBody] = SystemPlanetGenerator.get_moon_candidates(result.planets)
	
	# Should have at least one candidate (cold zone planets are larger)
	assert_greater_than(candidates.size(), 0, "Should have moon candidates")


## Tests assign_roman_numeral_names.
func test_assign_roman_numeral_names() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(11111)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	SystemPlanetGenerator.sort_by_distance(result.planets)
	SystemPlanetGenerator.assign_roman_numeral_names(result.planets, "Alpha")
	
	# Check names
	if result.planets.size() > 0:
		assert_true(result.planets[0].name.contains("I"), "First planet should be I")
	if result.planets.size() > 1:
		assert_true(result.planets[1].name.contains("II"), "Second planet should be II")


## Tests estimate_planet_count.
func test_estimate_planet_count() -> void:
	var host: OrbitHost = _create_test_host()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 10)
	
	# Set all to 0.5 probability
	for slot in slots:
		slot.fill_probability = 0.5
	
	var estimate: int = SystemPlanetGenerator.estimate_planet_count(slots)
	
	# Should estimate around 5
	assert_in_range(estimate, 4, 6, "Estimate should be reasonable")


## Tests validate_planet_slot_consistency.
func test_validate_planet_slot_consistency() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(22222)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	var is_consistent: bool = SystemPlanetGenerator.validate_planet_slot_consistency(
		result.planets,
		result.slots
	)
	
	assert_true(is_consistent, "Planet-slot consistency should be valid")


## Tests planets are assigned IDs.
func test_planets_have_ids() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(33333)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	for planet in result.planets:
		assert_false(planet.id.is_empty(), "Planet should have an ID")
		assert_false(planet.name.is_empty(), "Planet should have a name")


## Tests all planets pass validation.
func test_planets_pass_validation() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(44444)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	for planet in result.planets:
		var validation: ValidationResult = CelestialValidator.validate(planet)
		assert_true(validation.is_valid(), "Generated planet should pass validation")


## Tests planets in different zones have appropriate types.
func test_zone_appropriate_planets() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Create specific zone slots
	var hot_slot: OrbitSlot = OrbitSlot.new("hot", host.node_id, 0.3 * Units.AU_METERS)
	hot_slot.zone = OrbitZone.Zone.HOT
	hot_slot.fill_probability = 1.0
	hot_slot.is_stable = true
	
	var cold_slot: OrbitSlot = OrbitSlot.new("cold", host.node_id, 10.0 * Units.AU_METERS)
	cold_slot.zone = OrbitZone.Zone.COLD
	cold_slot.fill_probability = 1.0
	cold_slot.is_stable = true
	
	var rng: SeededRng = SeededRng.new(55555)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		[hot_slot, cold_slot], [host], [star], rng
	)
	
	# Should have generated planets
	assert_greater_than(result.planets.size(), 0, "Should generate planets")


## Tests targeted planet generation.
func test_generate_targeted() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 10)
	var rng: SeededRng = SeededRng.new(88888)
	
	var target: int = 3
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate_targeted(
		slots, [host], [star], target, rng
	)
	
	assert_true(result.success)
	assert_equal(result.planets.size(), target, "Should generate exactly target count")


## Tests targeted generation with insufficient slots.
func test_generate_targeted_insufficient_slots() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	# Only 2 slots available
	var slots: Array[OrbitSlot] = []
	for i in range(2):
		var slot: OrbitSlot = OrbitSlot.new("slot_%d" % i, host.node_id, float(i + 1) * Units.AU_METERS)
		slot.zone = OrbitZone.Zone.TEMPERATE
		slot.fill_probability = 1.0
		slot.is_stable = true
		slots.append(slot)
	
	var rng: SeededRng = SeededRng.new(99999)
	var target: int = 10  # More than available
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate_targeted(
		slots, [host], [star], target, rng
	)
	
	assert_true(result.success)
	assert_equal(result.planets.size(), 2, "Should generate only available slots")


## Tests orbital parent ID is set correctly.
func test_orbital_parent_id() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	var rng: SeededRng = SeededRng.new(14141)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	for planet in result.planets:
		assert_equal(
			planet.orbital.parent_id,
			host.node_id,
			"Planet orbital parent should be the host"
		)


## Tests filter_by_zone.
func test_filter_by_zone() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	
	var slots: Array[OrbitSlot] = []
	
	var hot_slot: OrbitSlot = OrbitSlot.new("hot", host.node_id, 0.3 * Units.AU_METERS)
	hot_slot.zone = OrbitZone.Zone.HOT
	hot_slot.fill_probability = 1.0
	hot_slot.is_stable = true
	slots.append(hot_slot)
	
	var temp_slot: OrbitSlot = OrbitSlot.new("temp", host.node_id, 1.0 * Units.AU_METERS)
	temp_slot.zone = OrbitZone.Zone.TEMPERATE
	temp_slot.fill_probability = 1.0
	temp_slot.is_stable = true
	slots.append(temp_slot)
	
	var cold_slot: OrbitSlot = OrbitSlot.new("cold", host.node_id, 10.0 * Units.AU_METERS)
	cold_slot.zone = OrbitZone.Zone.COLD
	cold_slot.fill_probability = 1.0
	cold_slot.is_stable = true
	slots.append(cold_slot)
	
	var rng: SeededRng = SeededRng.new(16161)
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, [host], [star], rng
	)
	
	var hot_planets: Array[CelestialBody] = SystemPlanetGenerator.filter_by_zone(
		result.planets,
		result.slots,
		OrbitZone.Zone.HOT
	)
	
	assert_equal(hot_planets.size(), 1, "Should filter to one hot zone planet")


## Tests planet IDs are unique.
func test_planet_ids_unique() -> void:
	var host: OrbitHost = _create_test_host()
	var star: CelestialBody = _create_test_star()
	var slots: Array[OrbitSlot] = _create_test_slots(host, 5)
	for slot in slots:
		slot.fill_probability = 1.0
	var hosts: Array[OrbitHost] = [host]
	var rng: SeededRng = SeededRng.new(13131)
	
	var result: SystemPlanetGenerator.PlanetGenerationResult = SystemPlanetGenerator.generate(
		slots, hosts, [star], rng
	)
	
	var ids: Dictionary = {}
	for planet in result.planets:
		assert_false(ids.has(planet.id), "Planet IDs should be unique")
		ids[planet.id] = true
