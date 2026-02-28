## Tests for PropertyConstraint.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func test_in_range_when_inside() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 5.0)
	assert_true(c.is_value_in_range())


func test_in_range_at_bounds() -> void:
	var low: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 0.0)
	var high: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 10.0)
	assert_true(low.is_value_in_range(), "min bound is inclusive")
	assert_true(high.is_value_in_range(), "max bound is inclusive")


func test_out_of_range() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 11.0)
	assert_false(c.is_value_in_range())


func test_clamp_value() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 5.0)
	assert_float_equal(c.clamp_value(-5.0), 0.0)
	assert_float_equal(c.clamp_value(15.0), 10.0)
	assert_float_equal(c.clamp_value(5.0), 5.0)


func test_has_bounds_with_infinities() -> void:
	var unbounded: PropertyConstraint = PropertyConstraint.new("p", -INF, INF, 0.0)
	var bounded: PropertyConstraint = PropertyConstraint.new("p", 0.0, INF, 0.0)
	assert_false(unbounded.has_bounds())
	assert_true(bounded.has_bounds())


func test_with_lock_returns_new_instance() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 5.0, false)
	var locked: PropertyConstraint = c.with_lock(true)
	assert_false(c.is_locked, "original unchanged")
	assert_true(locked.is_locked)
	assert_equal(locked.property_path, c.property_path)


func test_with_value_returns_new_instance() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 5.0)
	var updated: PropertyConstraint = c.with_value(7.0)
	assert_float_equal(c.current_value, 5.0, 0.0, "original unchanged")
	assert_float_equal(updated.current_value, 7.0)


func test_intersected_with_narrows_range() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 100.0, 50.0)
	var narrowed: PropertyConstraint = c.intersected_with(10.0, 80.0)
	assert_float_equal(narrowed.min_value, 10.0)
	assert_float_equal(narrowed.max_value, 80.0)


func test_intersected_with_cannot_widen() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 10.0, 20.0, 15.0)
	var attempt: PropertyConstraint = c.intersected_with(0.0, 100.0)
	assert_float_equal(attempt.min_value, 10.0)
	assert_float_equal(attempt.max_value, 20.0)


func test_intersected_with_appends_reason_only_when_narrowed() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("p", 0.0, 100.0, 50.0, false, "base")
	var narrowed: PropertyConstraint = c.intersected_with(10.0, 90.0, "lock")
	assert_true(narrowed.constraint_reason.contains("lock"), "reason appended on narrow")

	var unchanged: PropertyConstraint = c.intersected_with(-10.0, 200.0, "noop")
	assert_false(unchanged.constraint_reason.contains("noop"), "reason NOT appended when no narrowing")


func test_is_satisfiable() -> void:
	var good: PropertyConstraint = PropertyConstraint.new("p", 0.0, 10.0, 5.0)
	var bad: PropertyConstraint = PropertyConstraint.new("p", 10.0, 0.0, 5.0)
	assert_true(good.is_satisfiable())
	assert_false(bad.is_satisfiable())


func test_to_dict_round_trip_fields() -> void:
	var c: PropertyConstraint = PropertyConstraint.new("x.y", 1.0, 2.0, 1.5, true, "reason")
	var d: Dictionary = c.to_dict()
	assert_equal(d["property_path"], "x.y")
	assert_float_equal(d["min_value"], 1.0)
	assert_float_equal(d["max_value"], 2.0)
	assert_float_equal(d["current_value"], 1.5)
	assert_true(d["is_locked"])
	assert_equal(d["constraint_reason"], "reason")
