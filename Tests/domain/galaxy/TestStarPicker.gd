## Tests for StarPicker — ray-based star selection.
class_name TestStarPicker
extends TestCase


var _positions: PackedVector3Array
var _seeds: PackedInt64Array


func before_each() -> void:
	_positions = PackedVector3Array([
		Vector3(0.0, 0.0, 10.0),
		Vector3(5.0, 0.0, 10.0),
		Vector3(0.0, 5.0, 10.0),
		Vector3(100.0, 100.0, 100.0),
	])
	_seeds = PackedInt64Array([111, 222, 333, 444])


func test_picks_star_directly_on_ray() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(0.0, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 1.0
	)
	assert_not_null(result, "Should pick star directly on the ray")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	assert_equal(pick.star_index, 0, "Should pick star at index 0")
	assert_equal(pick.star_seed, 111, "Should return correct seed")
	assert_float_equal(pick.lateral_distance, 0.0, 0.01, "Lateral distance should be 0")


func test_picks_nearest_when_multiple_in_range() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(0.0, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 6.0
	)
	assert_not_null(result, "Should pick a star")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	assert_equal(pick.star_index, 0, "Should pick the closest star to the ray")


func test_returns_null_when_none_in_range() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(50.0, 50.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 1.0
	)
	assert_null(result, "Should return null when no stars are near the ray")


func test_ignores_stars_behind_ray() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(0.0, 0.0, 20.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 5.0
	)
	# All test stars are at z=10 or z=100, ray starts at z=20 going +Z
	# Stars at z=10 are behind, star at z=100 is ahead but laterally far
	assert_null(result, "Should not pick stars behind the ray origin")


func test_returns_null_for_empty_arrays() -> void:
	var empty_pos: PackedVector3Array = PackedVector3Array()
	var empty_seeds: PackedInt64Array = PackedInt64Array()
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3.ZERO, Vector3(0.0, 0.0, 1.0),
		empty_pos, empty_seeds, 5.0
	)
	assert_null(result, "Should return null for empty star arrays")


func test_pick_result_has_correct_position() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(4.5, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 1.0
	)
	assert_not_null(result, "Should pick nearby star")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	assert_equal(pick.star_index, 1, "Should pick star at (5,0,10)")
	assert_true(
		pick.world_position.is_equal_approx(Vector3(5.0, 0.0, 10.0)),
		"Position should match star position"
	)


func test_ray_distance_is_positive() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(0.0, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 1.0
	)
	assert_not_null(result, "Should pick a star")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	assert_float_equal(pick.ray_distance, 10.0, 0.01, "Ray distance should be 10")


func test_lateral_distance_correct() -> void:
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3(0.0, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_positions, _seeds, 6.0
	)
	assert_not_null(result, "Should pick a star")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	# Star at (0,0,10) — lateral distance is 0
	assert_float_equal(pick.lateral_distance, 0.0, 0.01, "Should be right on the ray")


func test_picks_from_diagonal_ray() -> void:
	var direction: Vector3 = Vector3(5.0, 0.0, 10.0).normalized()
	var result: Variant = StarPicker.pick_nearest_to_ray(
		Vector3.ZERO, direction, _positions, _seeds, 1.0
	)
	assert_not_null(result, "Diagonal ray should reach star at (5,0,10)")
	var pick: StarPicker.PickResult = result as StarPicker.PickResult
	assert_equal(pick.star_index, 1, "Should pick star aligned with diagonal ray")
