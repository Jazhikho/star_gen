## Unit tests for belt field generation.
extends TestCase

const _belt_field_generator: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldGenerator.gd")
const _belt_field_spec: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldSpec.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")


func _make_spec() -> RefCounted:
	var spec: RefCounted = _belt_field_spec.new()
	spec.inner_radius_au = 2.0
	spec.outer_radius_au = 3.2
	spec.asteroid_count = 120
	spec.max_inclination_deg = 12.0
	spec.max_eccentricity = 0.2
	spec.min_body_radius_km = 0.5
	spec.max_body_radius_km = 50.0
	return spec


## Field generation should be deterministic for equal seeds.
func test_generate_field_deterministic() -> void:
	var generator_a: RefCounted = _belt_field_generator.new()
	var generator_b: RefCounted = _belt_field_generator.new()
	var spec: RefCounted = _make_spec()
	var data_a: RefCounted = generator_a.generate_field(spec, _seeded_rng.new(10101))
	var data_b: RefCounted = generator_b.generate_field(spec, _seeded_rng.new(10101))
	assert_equal(data_a.asteroids.size(), data_b.asteroids.size(), "Counts should match")
	for i in range(data_a.asteroids.size()):
		var asteroid_a: RefCounted = data_a.asteroids[i]
		var asteroid_b: RefCounted = data_b.asteroids[i]
		assert_float_equal(asteroid_a.position_au.x, asteroid_b.position_au.x, 0.0001, "X match")
		assert_float_equal(asteroid_a.position_au.y, asteroid_b.position_au.y, 0.0001, "Y match")
		assert_float_equal(asteroid_a.position_au.z, asteroid_b.position_au.z, 0.0001, "Z match")
		if has_failed():
			return


## Background asteroids should stay inside radial bounds.
func test_generate_field_background_within_bounds() -> void:
	var generator: RefCounted = _belt_field_generator.new()
	var spec: RefCounted = _make_spec()
	var data: RefCounted = generator.generate_field(spec, _seeded_rng.new(20202))
	for asteroid in data.get_background_asteroids():
		assert_true(asteroid.semi_major_axis_au >= spec.inner_radius_au - 0.001, "SMA above inner bound")
		assert_true(asteroid.semi_major_axis_au <= spec.outer_radius_au + 0.001, "SMA below outer bound")
		if has_failed():
			return
