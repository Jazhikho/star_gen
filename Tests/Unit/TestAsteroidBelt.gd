## Tests for AsteroidBelt.
extends TestCase

const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Tests basic construction.
func test_construction() -> void:
	var belt: AsteroidBelt = AsteroidBelt.new("belt_1", "Main Belt")
	
	assert_equal(belt.id, "belt_1")
	assert_equal(belt.name, "Main Belt")
	assert_equal(belt.composition, AsteroidBelt.Composition.ROCKY)
	assert_equal(belt.major_asteroid_ids.size(), 0)


## Tests width calculations.
func test_get_width() -> void:
	var belt: AsteroidBelt = AsteroidBelt.new("b1", "Test")
	belt.inner_radius_m = 2.0 * Units.AU_METERS
	belt.outer_radius_m = 3.5 * Units.AU_METERS
	
	var width_au: float = belt.get_width_au()
	assert_float_equal(width_au, 1.5, 0.01)


## Tests center calculation.
func test_get_center() -> void:
	var belt: AsteroidBelt = AsteroidBelt.new("b1", "Test")
	belt.inner_radius_m = 2.0 * Units.AU_METERS
	belt.outer_radius_m = 4.0 * Units.AU_METERS
	
	var center_au: float = belt.get_center_au()
	assert_float_equal(center_au, 3.0, 0.01)


## Tests composition string conversion.
func test_composition_to_string() -> void:
	assert_equal(AsteroidBelt.composition_to_string(AsteroidBelt.Composition.ROCKY), "rocky")
	assert_equal(AsteroidBelt.composition_to_string(AsteroidBelt.Composition.ICY), "icy")
	assert_equal(AsteroidBelt.composition_to_string(AsteroidBelt.Composition.MIXED), "mixed")
	assert_equal(AsteroidBelt.composition_to_string(AsteroidBelt.Composition.METALLIC), "metallic")


## Tests string to composition parsing.
func test_string_to_composition() -> void:
	assert_equal(AsteroidBelt.string_to_composition("rocky"), AsteroidBelt.Composition.ROCKY)
	assert_equal(AsteroidBelt.string_to_composition("ICY"), AsteroidBelt.Composition.ICY)
	assert_equal(AsteroidBelt.string_to_composition("Mixed"), AsteroidBelt.Composition.MIXED)
	assert_equal(AsteroidBelt.string_to_composition("unknown"), AsteroidBelt.Composition.ROCKY)


## Tests major asteroid tracking.
func test_major_asteroid_ids() -> void:
	var belt: AsteroidBelt = AsteroidBelt.new("b1", "Test")
	belt.major_asteroid_ids.append("ceres")
	belt.major_asteroid_ids.append("vesta")
	belt.major_asteroid_ids.append("pallas")
	
	assert_equal(belt.get_major_asteroid_count(), 3)
	assert_true(belt.major_asteroid_ids.has("ceres"))


## Tests serialization round-trip.
func test_round_trip() -> void:
	var original: AsteroidBelt = AsteroidBelt.new("main_belt", "Main Asteroid Belt")
	original.orbit_host_id = "sol"
	original.inner_radius_m = 2.2 * Units.AU_METERS
	original.outer_radius_m = 3.2 * Units.AU_METERS
	original.total_mass_kg = 3.0e21
	original.composition = AsteroidBelt.Composition.MIXED
	original.major_asteroid_ids = ["ceres", "vesta", "pallas"]
	
	var data: Dictionary = original.to_dict()
	var restored: AsteroidBelt = AsteroidBelt.from_dict(data)
	
	assert_equal(restored.id, original.id)
	assert_equal(restored.name, original.name)
	assert_equal(restored.orbit_host_id, original.orbit_host_id)
	assert_float_equal(restored.inner_radius_m, original.inner_radius_m)
	assert_float_equal(restored.outer_radius_m, original.outer_radius_m)
	assert_float_equal(restored.total_mass_kg, original.total_mass_kg)
	assert_equal(restored.composition, original.composition)
	assert_equal(restored.major_asteroid_ids.size(), 3)
	assert_true(restored.major_asteroid_ids.has("vesta"))
