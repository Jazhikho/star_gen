## Tests for PropertyConstraintSolver.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Earth-like current values in base SI units.
func _earth_values() -> Dictionary:
	return {
		"physical.mass_kg": 5.972e24,
		"physical.radius_m": 6.371e6,
		"physical.rotation_period_s": 86164.0,
		"physical.axial_tilt_deg": 23.44,
		"physical.oblateness": 0.00335,
	}


func test_seeds_physical_constraints_for_planet() -> void:
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, _earth_values(), []
	)
	assert_true(cs.has_constraint("physical.mass_kg"))
	assert_true(cs.has_constraint("physical.radius_m"))
	assert_true(cs.has_constraint("physical.rotation_period_s"))
	assert_true(cs.has_constraint("physical.axial_tilt_deg"))
	assert_true(cs.has_constraint("physical.oblateness"))


func test_earth_values_fall_within_planet_bounds() -> void:
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, _earth_values(), []
	)
	assert_true(cs.is_consistent(), "Earth should satisfy planet bounds")


func test_star_has_stellar_constraints_planet_does_not() -> void:
	var star_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.STAR, {}, []
	)
	var planet_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, {}, []
	)
	assert_true(star_cs.has_constraint("stellar.temperature_k"))
	assert_false(planet_cs.has_constraint("stellar.temperature_k"))


func test_locking_mass_narrows_radius_range() -> void:
	var values: Dictionary = _earth_values()
	var no_locks: Array[String] = []
	var mass_locked: Array[String] = ["physical.mass_kg"]

	var unlocked_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, no_locks
	)
	var locked_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, mass_locked
	)

	var r_free: PropertyConstraint = unlocked_cs.get_constraint("physical.radius_m")
	var r_constrained: PropertyConstraint = locked_cs.get_constraint("physical.radius_m")

	# Locked mass should narrow the radius window, not widen it.
	assert_true(
		r_constrained.min_value >= r_free.min_value,
		"locking mass should not lower radius min"
	)
	assert_true(
		r_constrained.max_value <= r_free.max_value,
		"locking mass should not raise radius max"
	)
	# And it should actually narrow on at least one side.
	var narrowed: bool = (
		r_constrained.min_value > r_free.min_value
		or r_constrained.max_value < r_free.max_value
	)
	assert_true(narrowed, "locking mass should narrow radius range")


func test_earth_radius_still_valid_after_mass_lock() -> void:
	var values: Dictionary = _earth_values()
	var mass_locked: Array[String] = ["physical.mass_kg"]
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, mass_locked
	)
	var r: PropertyConstraint = cs.get_constraint("physical.radius_m")
	assert_true(r.is_value_in_range(),
		"Earth radius must remain valid when Earth mass is locked")


func test_locking_radius_narrows_mass_range() -> void:
	var values: Dictionary = _earth_values()
	var radius_locked: Array[String] = ["physical.radius_m"]

	var unlocked_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, []
	)
	var locked_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, radius_locked
	)

	var m_free: PropertyConstraint = unlocked_cs.get_constraint("physical.mass_kg")
	var m_constrained: PropertyConstraint = locked_cs.get_constraint("physical.mass_kg")

	assert_true(m_constrained.min_value >= m_free.min_value)
	assert_true(m_constrained.max_value <= m_free.max_value)


func test_locking_slow_rotation_caps_oblateness() -> void:
	var values: Dictionary = _earth_values()
	values["physical.rotation_period_s"] = 500.0 * 3600.0 # 500 hr rotation
	var rot_locked: Array[String] = ["physical.rotation_period_s"]

	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, rot_locked
	)
	var obl: PropertyConstraint = cs.get_constraint("physical.oblateness")
	# Very slow rotation should cap oblateness well below the 0.5 default.
	assert_less_than(obl.max_value, 0.1)


func test_locking_fast_rotation_allows_high_oblateness() -> void:
	var values: Dictionary = _earth_values()
	values["physical.rotation_period_s"] = 1.5 * 3600.0 # 1.5 hr rotation
	var rot_locked: Array[String] = ["physical.rotation_period_s"]

	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, rot_locked
	)
	var obl: PropertyConstraint = cs.get_constraint("physical.oblateness")
	# Fast rotation should keep the full range available.
	assert_float_equal(obl.max_value, 0.5, 0.01)


func test_both_mass_and_radius_locked_no_narrowing() -> void:
	var values: Dictionary = _earth_values()
	var both_locked: Array[String] = ["physical.mass_kg", "physical.radius_m"]

	var free_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, []
	)
	var both_cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, both_locked
	)

	# When both are locked, coupling rule skips narrowing —
	# both are authoritative and may contradict density bounds.
	# That's a user choice; the solver doesn't override locked values.
	var r_free: PropertyConstraint = free_cs.get_constraint("physical.radius_m")
	var r_both: PropertyConstraint = both_cs.get_constraint("physical.radius_m")
	assert_float_equal(r_both.min_value, r_free.min_value, 1.0)
	assert_float_equal(r_both.max_value, r_free.max_value, 1.0)


func test_locked_paths_are_marked_locked_in_output() -> void:
	var values: Dictionary = _earth_values()
	var mass_locked: Array[String] = ["physical.mass_kg"]
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, mass_locked
	)
	assert_true(cs.get_constraint("physical.mass_kg").is_locked)
	assert_false(cs.get_constraint("physical.radius_m").is_locked)


func test_determinism_same_input_same_output() -> void:
	var values: Dictionary = _earth_values()
	var locked: Array[String] = ["physical.mass_kg"]

	var a: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, locked
	)
	var b: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, locked
	)

	for path: String in a.get_all_paths():
		var ca: PropertyConstraint = a.get_constraint(path)
		var cb: PropertyConstraint = b.get_constraint(path)
		assert_not_null(cb, "path present in both: " + path)
		assert_float_equal(ca.min_value, cb.min_value, 0.0, path + " min")
		assert_float_equal(ca.max_value, cb.max_value, 0.0, path + " max")


func test_axial_tilt_matches_validator_bounds() -> void:
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, {}, []
	)
	var tilt: PropertyConstraint = cs.get_constraint("physical.axial_tilt_deg")
	assert_float_equal(tilt.min_value, 0.0)
	assert_float_equal(tilt.max_value, 180.0)


func test_albedo_matches_validator_bounds() -> void:
	var cs: ConstraintSet = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, {}, []
	)
	var albedo: PropertyConstraint = cs.get_constraint("surface.albedo")
	assert_float_equal(albedo.min_value, 0.0)
	assert_float_equal(albedo.max_value, 1.0)


func test_extra_constraints_narrow_radius() -> void:
	var values: Dictionary = _earth_values()
	var extra: Dictionary = {
		"physical.radius_m": Vector2(6.0e6, 6.5e6),
	}
	var cs: ConstraintSet = PropertyConstraintSolver.solve_with_extra_constraints(
		CelestialType.Type.PLANET, values, [], extra
	)
	var r: PropertyConstraint = cs.get_constraint("physical.radius_m")
	assert_true(r.min_value >= 6.0e6, "extra min applied")
	assert_true(r.max_value <= 6.5e6, "extra max applied")


func test_extra_constraints_feed_into_coupling() -> void:
	var values: Dictionary = _earth_values()
	# Narrow radius window tightly, then lock radius.
	var extra: Dictionary = {
		"physical.radius_m": Vector2(6.3e6, 6.4e6),
	}
	var locked: Array[String] = ["physical.radius_m"]
	var cs: ConstraintSet = PropertyConstraintSolver.solve_with_extra_constraints(
		CelestialType.Type.PLANET, values, locked, extra
	)
	# Mass should now be narrowed by the tight radius + density coupling.
	var m: PropertyConstraint = cs.get_constraint("physical.mass_kg")
	var m_free: PropertyConstraint = PropertyConstraintSolver.solve(
		CelestialType.Type.PLANET, values, []
	).get_constraint("physical.mass_kg")
	assert_true(m.max_value < m_free.max_value, "coupling should tighten mass from narrowed radius")
