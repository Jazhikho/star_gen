## Tests for MathUtils functions.
extends TestCase


## Tests is_in_range_float with values inside the range.
func test_is_in_range_float_inside() -> void:
	assert_true(MathUtils.is_in_range_float(5.0, 0.0, 10.0))
	assert_true(MathUtils.is_in_range_float(0.0, 0.0, 10.0))
	assert_true(MathUtils.is_in_range_float(10.0, 0.0, 10.0))


## Tests is_in_range_float with values outside the range.
func test_is_in_range_float_outside() -> void:
	assert_false(MathUtils.is_in_range_float(-0.1, 0.0, 10.0))
	assert_false(MathUtils.is_in_range_float(10.1, 0.0, 10.0))


## Tests is_in_range_int with values inside the range.
func test_is_in_range_int_inside() -> void:
	assert_true(MathUtils.is_in_range_int(5, 0, 10))
	assert_true(MathUtils.is_in_range_int(0, 0, 10))
	assert_true(MathUtils.is_in_range_int(10, 0, 10))


## Tests is_in_range_int with values outside the range.
func test_is_in_range_int_outside() -> void:
	assert_false(MathUtils.is_in_range_int(-1, 0, 10))
	assert_false(MathUtils.is_in_range_int(11, 0, 10))


## Tests remap from one range to another.
func test_remap_standard() -> void:
	# 5 is halfway in [0, 10], should map to 50 in [0, 100]
	var result: float = MathUtils.remap(5.0, 0.0, 10.0, 0.0, 100.0)
	assert_float_equal(result, 50.0)


## Tests remap at range boundaries.
func test_remap_boundaries() -> void:
	assert_float_equal(MathUtils.remap(0.0, 0.0, 10.0, 0.0, 100.0), 0.0)
	assert_float_equal(MathUtils.remap(10.0, 0.0, 10.0, 0.0, 100.0), 100.0)


## Tests remap with inverted target range.
func test_remap_inverted_range() -> void:
	# 0 in [0, 10] should map to 100 in [100, 0]
	var result: float = MathUtils.remap(0.0, 0.0, 10.0, 100.0, 0.0)
	assert_float_equal(result, 100.0)


## Tests remap with same source min and max (edge case).
func test_remap_zero_source_range() -> void:
	var result: float = MathUtils.remap(5.0, 5.0, 5.0, 0.0, 100.0)
	assert_float_equal(result, 0.0, 0.00001, "Should return to_min when source range is zero")


## Tests remap_clamped stays within target range.
func test_remap_clamped() -> void:
	# Value outside source range should be clamped in target
	var result: float = MathUtils.remap_clamped(15.0, 0.0, 10.0, 0.0, 100.0)
	assert_float_equal(result, 100.0)
	
	result = MathUtils.remap_clamped(-5.0, 0.0, 10.0, 0.0, 100.0)
	assert_float_equal(result, 0.0)


## Tests inverse_lerp calculates correct position.
func test_inverse_lerp() -> void:
	assert_float_equal(MathUtils.inverse_lerp(0.0, 10.0, 5.0), 0.5)
	assert_float_equal(MathUtils.inverse_lerp(0.0, 10.0, 0.0), 0.0)
	assert_float_equal(MathUtils.inverse_lerp(0.0, 10.0, 10.0), 1.0)


## Tests inverse_lerp with same from and to (edge case).
func test_inverse_lerp_zero_range() -> void:
	var result: float = MathUtils.inverse_lerp(5.0, 5.0, 5.0)
	assert_float_equal(result, 0.0)


## Tests smooth_lerp produces values in expected range.
func test_smooth_lerp_range() -> void:
	assert_float_equal(MathUtils.smooth_lerp(0.0, 100.0, 0.0), 0.0)
	assert_float_equal(MathUtils.smooth_lerp(0.0, 100.0, 1.0), 100.0)
	
	# Midpoint should still be 50 for smoothstep
	assert_float_equal(MathUtils.smooth_lerp(0.0, 100.0, 0.5), 50.0)


## Tests smooth_lerp clamps weight to [0, 1].
func test_smooth_lerp_clamped_weight() -> void:
	assert_float_equal(MathUtils.smooth_lerp(0.0, 100.0, -1.0), 0.0)
	assert_float_equal(MathUtils.smooth_lerp(0.0, 100.0, 2.0), 100.0)
