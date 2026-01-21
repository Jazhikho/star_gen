## Tests for OrbitSlot.
extends TestCase

const _orbit_slot := preload("res://src/domain/system/OrbitSlot.gd")
const _orbit_zone := preload("res://src/domain/generation/archetypes/OrbitZone.gd")
const _units := preload("res://src/domain/math/Units.gd")


## Tests basic construction.
func test_construction() -> void:
	var slot: OrbitSlot = OrbitSlot.new("slot_1", "host_1", 1.5e11)
	
	assert_equal(slot.id, "slot_1")
	assert_equal(slot.orbit_host_id, "host_1")
	assert_equal(slot.semi_major_axis_m, 1.5e11)
	assert_float_equal(slot.suggested_eccentricity, 0.0)
	assert_true(slot.is_stable)
	assert_false(slot.is_filled)


## Tests get_semi_major_axis_au.
func test_get_semi_major_axis_au() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", Units.AU_METERS * 2.5)
	
	assert_float_equal(slot.get_semi_major_axis_au(), 2.5, 0.01)


## Tests zone string conversion.
func test_get_zone_string() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	
	slot.zone = OrbitZone.Zone.HOT
	assert_equal(slot.get_zone_string(), "Hot")
	
	slot.zone = OrbitZone.Zone.TEMPERATE
	assert_equal(slot.get_zone_string(), "Temperate")
	
	slot.zone = OrbitZone.Zone.COLD
	assert_equal(slot.get_zone_string(), "Cold")


## Tests fill_with_planet.
func test_fill_with_planet() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	
	assert_false(slot.is_filled)
	assert_equal(slot.planet_id, "")
	
	slot.fill_with_planet("planet_42")
	
	assert_true(slot.is_filled)
	assert_equal(slot.planet_id, "planet_42")


## Tests clear_planet.
func test_clear_planet() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	slot.fill_with_planet("planet_42")
	
	slot.clear_planet()
	
	assert_false(slot.is_filled)
	assert_equal(slot.planet_id, "")


## Tests is_available.
func test_is_available() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	
	# Stable and unfilled = available
	slot.is_stable = true
	slot.is_filled = false
	assert_true(slot.is_available())
	
	# Unstable = not available
	slot.is_stable = false
	assert_false(slot.is_available())
	
	# Filled = not available
	slot.is_stable = true
	slot.fill_with_planet("p1")
	assert_false(slot.is_available())


## Tests suggested_eccentricity can be set.
func test_suggested_eccentricity() -> void:
	var slot: OrbitSlot = OrbitSlot.new("s1", "h1", 1e11)
	
	slot.suggested_eccentricity = 0.15
	assert_float_equal(slot.suggested_eccentricity, 0.15, 0.01)


## Tests serialization round-trip.
func test_round_trip() -> void:
	var original: OrbitSlot = OrbitSlot.new("slot_test", "host_test", 2.5e11)
	original.suggested_eccentricity = 0.12
	original.zone = OrbitZone.Zone.COLD
	original.is_stable = true
	original.fill_probability = 0.75
	original.fill_with_planet("planet_99")
	
	var data: Dictionary = original.to_dict()
	var restored: OrbitSlot = OrbitSlot.from_dict(data)
	
	assert_equal(restored.id, original.id)
	assert_equal(restored.orbit_host_id, original.orbit_host_id)
	assert_float_equal(restored.semi_major_axis_m, original.semi_major_axis_m)
	assert_float_equal(restored.suggested_eccentricity, original.suggested_eccentricity)
	assert_equal(restored.zone, original.zone)
	assert_equal(restored.is_stable, original.is_stable)
	assert_float_equal(restored.fill_probability, original.fill_probability)
	assert_equal(restored.is_filled, original.is_filled)
	assert_equal(restored.planet_id, original.planet_id)
