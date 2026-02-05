## Tests for JumpLaneSystem data model.
extends TestCase


func test_init_defaults() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new()

	assert_equal(system.id, "")
	assert_equal(system.position, Vector3.ZERO)
	assert_equal(system.population, 0)
	assert_equal(system.false_population, -1)
	assert_false(system.is_bridge)


func test_init_with_values() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3(10, 0, 5), 50000)

	assert_equal(system.id, "sys_001")
	assert_equal(system.position, Vector3(10, 0, 5))
	assert_equal(system.population, 50000)


func test_is_populated_true() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3.ZERO, 1000)

	assert_true(system.is_populated())


func test_is_populated_false() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3.ZERO, 0)

	assert_false(system.is_populated())


func test_effective_population_without_false() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3.ZERO, 50000)

	assert_equal(system.get_effective_population(), 50000)


func test_effective_population_with_false() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3.ZERO, 0)
	system.false_population = 40000

	assert_equal(system.get_effective_population(), 40000)


func test_distance_to() -> void:
	var system_a: JumpLaneSystem = JumpLaneSystem.new("a", Vector3(0, 0, 0), 1000)
	var system_b: JumpLaneSystem = JumpLaneSystem.new("b", Vector3(3, 4, 0), 2000)

	assert_float_equal(system_a.distance_to(system_b), 5.0)


func test_distance_to_3d() -> void:
	var system_a: JumpLaneSystem = JumpLaneSystem.new("a", Vector3(0, 0, 0), 1000)
	var system_b: JumpLaneSystem = JumpLaneSystem.new("b", Vector3(1, 2, 2), 2000)

	assert_float_equal(system_a.distance_to(system_b), 3.0)


func test_make_bridge() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("bridge", Vector3.ZERO, 0)

	system.make_bridge(50000)

	assert_true(system.is_bridge)
	assert_equal(system.false_population, 40000)


func test_make_bridge_clamps_negative() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("bridge", Vector3.ZERO, 0)

	system.make_bridge(5000)

	assert_true(system.is_bridge)
	assert_equal(system.false_population, 0)


func test_serialization_round_trip() -> void:
	var system: JumpLaneSystem = JumpLaneSystem.new("sys_001", Vector3(10, 5, 3), 75000)
	system.make_bridge(100000)

	var data: Dictionary = system.to_dict()
	var restored: JumpLaneSystem = JumpLaneSystem.from_dict(data)

	assert_equal(restored.id, "sys_001")
	assert_equal(restored.position, Vector3(10, 5, 3))
	assert_equal(restored.population, 75000)
	assert_equal(restored.false_population, 90000)
	assert_true(restored.is_bridge)
