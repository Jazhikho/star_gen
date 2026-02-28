## Unit tests for GalaxyBodyOverrides.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _overrides_script: GDScript = preload("res://src/domain/galaxy/GalaxyBodyOverrides.gd")


func get_test_name() -> String:
	return "TestGalaxyBodyOverrides"


func _make_body(body_id: String, body_name: String) -> CelestialBody:
	var phys: PhysicalProps = PhysicalProps.new(5.972e24, 6.371e6)
	return CelestialBody.new(body_id, body_name, CelestialType.Type.PLANET, phys, null)


func test_empty_state() -> void:
	var o: RefCounted = _overrides_script.new()
	assert_true(o.is_empty())
	assert_equal(o.total_count(), 0)
	assert_false(o.has_any_for(12345))


func test_set_and_get_override() -> void:
	var o: RefCounted = _overrides_script.new()
	var body: CelestialBody = _make_body("planet_1", "Earth")
	o.set_override(1000, body)
	assert_false(o.is_empty())
	assert_true(o.has_any_for(1000))
	assert_equal(o.total_count(), 1)
	var d: Dictionary = o.get_override_dict(1000, "planet_1")
	assert_false(d.is_empty())
	assert_equal(d.get("id", ""), "planet_1")
	var restored: CelestialBody = o.get_override_body(1000, "planet_1")
	assert_not_null(restored)
	assert_equal(restored.id, "planet_1")


func test_get_override_missing_returns_empty() -> void:
	var o: RefCounted = _overrides_script.new()
	assert_true(o.get_override_dict(999, "x").is_empty())
	assert_null(o.get_override_body(999, "x"))


func test_clear_override() -> void:
	var o: RefCounted = _overrides_script.new()
	var body: CelestialBody = _make_body("b1", "B1")
	o.set_override(1, body)
	assert_true(o.has_any_for(1))
	o.clear_override(1, "b1")
	assert_false(o.has_any_for(1))
	assert_true(o.get_override_dict(1, "b1").is_empty())
	o.clear_override(1, "b1")


func test_multiple_bodies_same_system() -> void:
	var o: RefCounted = _overrides_script.new()
	o.set_override(50, _make_body("p1", "P1"))
	o.set_override(50, _make_body("p2", "P2"))
	assert_equal(o.total_count(), 2)
	var ids: Array[String] = o.get_overridden_ids(50)
	assert_equal(ids.size(), 2)
	assert_true(ids.has("p1"))
	assert_true(ids.has("p2"))


func test_set_override_dict() -> void:
	var o: RefCounted = _overrides_script.new()
	var body_dict: Dictionary = {"id": "custom", "name": "Custom", "type": "planet", "physical": {"mass_kg": 1e24, "radius_m": 1e6}}
	o.set_override_dict(200, "custom", body_dict)
	assert_true(o.has_any_for(200))
	var d: Dictionary = o.get_override_dict(200, "custom")
	assert_equal(d.get("id", ""), "custom")


func test_rejects_null_body() -> void:
	var o: RefCounted = _overrides_script.new()
	o.set_override(1, null)
	assert_true(o.is_empty())


func test_rejects_empty_body_id() -> void:
	var o: RefCounted = _overrides_script.new()
	var body: CelestialBody = _make_body("x", "X")
	body.id = ""
	o.set_override(1, body)
	assert_true(o.is_empty())


func test_to_dict_from_dict_round_trip() -> void:
	var o: RefCounted = _overrides_script.new()
	o.set_override(42, _make_body("a", "A"))
	o.set_override(42, _make_body("b", "B"))
	var d: Dictionary = o.to_dict()
	assert_true(d.has("42"))
	var restored: RefCounted = _overrides_script.from_dict(d)
	assert_equal(restored.total_count(), 2)
	assert_true(restored.has_any_for(42))
	assert_equal(restored.get_override_body(42, "a").name, "A")


func test_apply_to_bodies_replaces_matching() -> void:
	var o: RefCounted = _overrides_script.new()
	var edited: CelestialBody = _make_body("p1", "Edited Planet")
	o.set_override(10, edited)
	var original: CelestialBody = _make_body("p1", "Original")
	var bodies: Array = [original]
	var replaced: int = o.apply_to_bodies(10, bodies)
	assert_equal(replaced, 1)
	assert_equal((bodies[0] as CelestialBody).name, "Edited Planet")


func test_apply_to_bodies_no_op_wrong_seed() -> void:
	var o: RefCounted = _overrides_script.new()
	o.set_override(10, _make_body("p1", "X"))
	var body: CelestialBody = _make_body("p1", "Y")
	var bodies: Array = [body]
	var replaced: int = o.apply_to_bodies(99, bodies)
	assert_equal(replaced, 0)
	assert_equal((bodies[0] as CelestialBody).name, "Y")


func test_apply_to_bodies_handles_nulls_in_array() -> void:
	var o: RefCounted = _overrides_script.new()
	o.set_override(1, _make_body("p1", "P1"))
	var bodies: Array = [null, _make_body("p1", "Old"), null]
	var replaced: int = o.apply_to_bodies(1, bodies)
	assert_equal(replaced, 1)
	assert_null(bodies[0])
	assert_not_null(bodies[1])
	assert_null(bodies[2])
