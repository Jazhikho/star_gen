## Tests for EditSpecBuilder.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _builder: GDScript = preload("res://src/domain/editing/EditSpecBuilder.gd")


func _make_cs(locked: Dictionary) -> ConstraintSet:
	var cs: ConstraintSet = ConstraintSet.new()
	for path: Variant in locked.keys():
		var val: float = locked[path] as float
		cs.set_constraint(PropertyConstraint.new(path as String, -INF, INF, val, true, ""))
	return cs


func test_planet_locks_pass_through_unchanged() -> void:
	var cs: ConstraintSet = _make_cs({
		"physical.mass_kg": 5.972e24,
		"orbital.eccentricity": 0.017,
	})
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.PLANET, cs)
	assert_float_equal(overrides["physical.mass_kg"], 5.972e24, 1.0)
	assert_float_equal(overrides["orbital.eccentricity"], 0.017, 1e-6)
	assert_false(overrides.has("physical.mass_solar"))


func test_star_mass_lock_writes_solar_alias() -> void:
	var sun_mass_kg: float = 1.989e30
	var cs: ConstraintSet = _make_cs({"physical.mass_kg": sun_mass_kg})
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.STAR, cs)
	assert_true(overrides.has("physical.mass_kg"), "base path present")
	assert_true(overrides.has("physical.mass_solar"), "alias present")
	assert_float_equal(overrides["physical.mass_solar"], 1.0, 0.01)


func test_star_luminosity_lock_writes_solar_alias() -> void:
	var sun_lum_w: float = 3.828e26
	var cs: ConstraintSet = _make_cs({"stellar.luminosity_watts": sun_lum_w})
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.STAR, cs)
	assert_true(overrides.has("stellar.luminosity_solar"))
	assert_float_equal(overrides["stellar.luminosity_solar"], 1.0, 0.01)


func test_star_temperature_k_written_as_base_path() -> void:
	var cs: ConstraintSet = _make_cs({"stellar.temperature_k": 5778.0})
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.STAR, cs)
	assert_true(overrides.has("stellar.temperature_k"))
	assert_float_equal(overrides["stellar.temperature_k"], 5778.0, 0.1)


func test_unlocked_properties_excluded() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new("physical.mass_kg", 0.0, 1e31, 5.972e24, true, ""))
	cs.set_constraint(PropertyConstraint.new("physical.radius_m", 0.0, 1e10, 6.371e6, false, ""))
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.PLANET, cs)
	assert_true(overrides.has("physical.mass_kg"))
	assert_false(overrides.has("physical.radius_m"), "unlocked radius must not become an override")


func test_apply_to_spec_clears_and_populates() -> void:
	var cs: ConstraintSet = _make_cs({"orbital.eccentricity": 0.5})
	var spec: BaseSpec = BaseSpec.new(12345)
	spec.set_override("stale.key", 99.0)
	_builder.apply_to_spec(spec, CelestialType.Type.PLANET, cs)
	assert_false(spec.has_override("stale.key"), "existing overrides cleared")
	assert_true(spec.has_override("orbital.eccentricity"))
	assert_float_equal(spec.get_override_float("orbital.eccentricity", -1.0), 0.5, 1e-6)


func test_empty_locks_produce_empty_overrides() -> void:
	var cs: ConstraintSet = ConstraintSet.new()
	cs.set_constraint(PropertyConstraint.new("p", 0.0, 1.0, 0.5, false, ""))
	var overrides: Dictionary = _builder.build_overrides(CelestialType.Type.PLANET, cs)
	assert_true(overrides.is_empty())
