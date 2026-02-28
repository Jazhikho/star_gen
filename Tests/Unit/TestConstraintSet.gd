## Tests for ConstraintSet.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


func _make_constraint(path: String, minv: float, maxv: float, cur: float) -> PropertyConstraint:
	return PropertyConstraint.new(path, minv, maxv, cur)


func test_set_and_get() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	var c: PropertyConstraint = _make_constraint("p", 0.0, 10.0, 5.0)
	cs.set_constraint(c)
	assert_true(cs.has_constraint("p"))
	var got: PropertyConstraint = cs.get_constraint("p")
	assert_not_null(got)
	assert_equal(got.property_path, "p")


func test_get_missing_returns_null() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	assert_null(cs.get_constraint("missing"))
	assert_false(cs.has_constraint("missing"))


func test_lock_and_unlock() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("p", 0.0, 10.0, 5.0))
	assert_false(cs.get_constraint("p").is_locked)
	cs.lock("p")
	assert_true(cs.get_constraint("p").is_locked)
	cs.unlock("p")
	assert_false(cs.get_constraint("p").is_locked)


func test_lock_missing_is_noop() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.lock("missing")
	assert_equal(cs.size(), 0)


func test_get_locked_paths() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("a", 0.0, 1.0, 0.5))
	cs.set_constraint(_make_constraint("b", 0.0, 1.0, 0.5))
	cs.lock("a")
	var locked: Array[String] = cs.get_locked_paths()
	assert_equal(locked.size(), 1)
	assert_equal(locked[0], "a")


func test_get_locked_overrides_matches_spec_format() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("physical.mass_kg", 0.0, 1.0e30, 5.0e24))
	cs.set_constraint(_make_constraint("physical.radius_m", 0.0, 1.0e8, 6.0e6))
	cs.lock("physical.mass_kg")

	var overrides: Dictionary = cs.get_locked_overrides()
	assert_equal(overrides.size(), 1)
	assert_true(overrides.has("physical.mass_kg"))
	assert_float_equal(overrides["physical.mass_kg"], 5.0e24, 1.0)

	# Verify the shape matches BaseSpec expectations.
	var spec: BaseSpec = BaseSpec.new()
	spec.overrides = overrides
	assert_true(spec.has_override("physical.mass_kg"))
	assert_float_equal(spec.get_override_float("physical.mass_kg", 0.0), 5.0e24, 1.0)


func test_set_value_does_not_clamp() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("p", 0.0, 10.0, 5.0))
	cs.set_value("p", 999.0)
	assert_float_equal(cs.get_constraint("p").current_value, 999.0, 0.0,
		"set_value is raw write; caller decides whether to clamp")


func test_is_consistent_all_good() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("a", 0.0, 10.0, 5.0))
	cs.set_constraint(_make_constraint("b", 0.0, 1.0, 0.5))
	assert_true(cs.is_consistent())


func test_is_consistent_detects_out_of_range() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("a", 0.0, 10.0, 15.0))
	assert_false(cs.is_consistent())
	var v: Array[String] = cs.get_violations()
	assert_equal(v.size(), 1)
	assert_equal(v[0], "a")


func test_is_consistent_detects_unsatisfiable() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new("a", 10.0, 0.0, 5.0))
	assert_false(cs.is_consistent())


func test_clamp_unlocked_clamps_only_unlocked() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("free", 0.0, 10.0, 50.0))
	cs.set_constraint(_make_constraint("pinned", 0.0, 10.0, 50.0))
	cs.lock("pinned")

	var modified: Array[String] = cs.clamp_unlocked()
	assert_equal(modified.size(), 1)
	assert_equal(modified[0], "free")
	assert_float_equal(cs.get_constraint("free").current_value, 10.0)
	assert_float_equal(cs.get_constraint("pinned").current_value, 50.0, 0.0,
		"locked values are authoritative and never clamped")


func test_clamp_unlocked_returns_empty_when_all_in_range() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(_make_constraint("a", 0.0, 10.0, 5.0))
	var modified: Array[String] = cs.clamp_unlocked()
	assert_true(modified.is_empty())
