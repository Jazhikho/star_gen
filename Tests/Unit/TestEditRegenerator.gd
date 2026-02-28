## Tests for EditRegenerator.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _regen: GDScript = preload("res://src/domain/editing/EditRegenerator.gd")


func test_planet_regeneration_produces_valid_body() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	var result: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 42, null)
	assert_true(result.success, result.error_message)
	assert_not_null(result.body)
	assert_equal(result.body.type, CelestialType.Type.PLANET)


func test_star_regeneration_produces_valid_body() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	var result: Variant = _regen.regenerate(CelestialType.Type.STAR, cs, 42, null)
	assert_true(result.success, result.error_message)
	assert_equal(result.body.type, CelestialType.Type.STAR)
	assert_true(result.body.has_stellar())


func test_locked_orbital_eccentricity_survives_planet_regeneration() -> void:
	var target_ecc: float = 0.42
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new(
		"orbital.eccentricity", 0.0, 0.99, target_ecc, true, ""
	))
	var result: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 7, null)
	assert_true(result.success)
	assert_not_null(result.body.orbital)
	assert_float_equal(result.body.orbital.eccentricity, target_ecc, 1e-6,
		"locked eccentricity must survive regeneration unchanged")


func test_locked_semi_major_axis_survives_planet_regeneration() -> void:
	var target_sma: float = 2.0 * 1.496e11
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new(
		"orbital.semi_major_axis_m", 1.0e9, 1.0e14, target_sma, true, ""
	))
	var result: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 99, null)
	assert_true(result.success)
	assert_float_equal(result.body.orbital.semi_major_axis_m, target_sma, 1.0)


func test_locked_star_mass_survives_regeneration() -> void:
	var target_mass_kg: float = 2.0 * 1.989e30
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new(
		"physical.mass_kg", 1e29, 1e32, target_mass_kg, true, ""
	))
	var result: Variant = _regen.regenerate(CelestialType.Type.STAR, cs, 13, null)
	assert_true(result.success)
	assert_float_equal(result.body.physical.mass_kg, target_mass_kg, 1e20,
		"locked star mass must survive regeneration")


func test_same_seed_same_locks_is_deterministic() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new(
		"orbital.eccentricity", 0.0, 0.99, 0.1, true, ""
	))
	var a: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 1234, null)
	var b: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 1234, null)
	assert_true(a.success and b.success)
	assert_float_equal(a.body.physical.mass_kg, b.body.physical.mass_kg, 0.0,
		"identical seed + locks must yield identical mass")
	assert_float_equal(a.body.physical.radius_m, b.body.physical.radius_m, 0.0,
		"identical seed + locks must yield identical radius")


func test_different_seeds_change_unlocked_properties() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new(
		"orbital.eccentricity", 0.0, 0.99, 0.1, true, ""
	))
	var a: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 1, null)
	var b: Variant = _regen.regenerate(CelestialType.Type.PLANET, cs, 2, null)
	assert_true(a.success and b.success)
	assert_float_equal(a.body.orbital.eccentricity, b.body.orbital.eccentricity, 1e-9)
	assert_not_equal(a.body.physical.mass_kg, b.body.physical.mass_kg,
		"unlocked mass should vary with seed")


func test_unsupported_type_returns_error() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	var result: Variant = _regen.regenerate(999, cs, 0, null)
	assert_false(result.success)
	assert_false(result.error_message.is_empty())
