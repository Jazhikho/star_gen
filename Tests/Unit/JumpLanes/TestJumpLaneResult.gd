## Tests for JumpLaneResult data model.
extends TestCase


func test_add_connection_and_total() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.add_connection(JumpLaneConnection.new("a", "b", JumpLaneConnection.ConnectionType.GREEN, 2.5))
	result.add_connection(JumpLaneConnection.new("b", "c", JumpLaneConnection.ConnectionType.YELLOW, 4.0))

	assert_equal(result.get_total_connections(), 2)


func test_add_orphan_and_total() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.add_orphan("orphan_1")
	result.add_orphan("orphan_2")

	assert_equal(result.get_total_orphans(), 2)


func test_is_orphan() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.add_orphan("orphan_x")

	assert_true(result.is_orphan("orphan_x"))
	assert_false(result.is_orphan("other"))


func test_get_connection_counts() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.add_connection(JumpLaneConnection.new("a", "b", JumpLaneConnection.ConnectionType.GREEN))
	result.add_connection(JumpLaneConnection.new("b", "c", JumpLaneConnection.ConnectionType.GREEN))
	result.add_connection(JumpLaneConnection.new("c", "d", JumpLaneConnection.ConnectionType.YELLOW))
	result.add_connection(JumpLaneConnection.new("d", "e", JumpLaneConnection.ConnectionType.ORANGE))

	var counts: Dictionary = result.get_connection_counts()

	assert_equal(counts[JumpLaneConnection.ConnectionType.GREEN], 2)
	assert_equal(counts[JumpLaneConnection.ConnectionType.YELLOW], 1)
	assert_equal(counts[JumpLaneConnection.ConnectionType.ORANGE], 1)


func test_get_connections_for_system() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.add_connection(JumpLaneConnection.new("a", "b", JumpLaneConnection.ConnectionType.GREEN))
	result.add_connection(JumpLaneConnection.new("b", "c", JumpLaneConnection.ConnectionType.GREEN))
	result.add_connection(JumpLaneConnection.new("d", "e", JumpLaneConnection.ConnectionType.ORANGE))

	var b_connections: Array[JumpLaneConnection] = result.get_connections_for_system("b")

	assert_equal(b_connections.size(), 2)


func test_register_system() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	var system: JumpLaneSystem = JumpLaneSystem.new("s1", Vector3.ZERO, 100)
	result.register_system(system)

	assert_true(result.systems.has("s1"))
	assert_equal(result.systems["s1"].id, "s1")


func test_serialization_round_trip() -> void:
	var result: JumpLaneResult = JumpLaneResult.new()
	result.register_system(JumpLaneSystem.new("s1", Vector3.ZERO, 100))
	result.register_system(JumpLaneSystem.new("s2", Vector3.ONE, 200))
	result.add_connection(JumpLaneConnection.new("s1", "s2", JumpLaneConnection.ConnectionType.GREEN, 2.0))
	result.add_orphan("orphan_x")

	var data: Dictionary = result.to_dict()
	var restored: JumpLaneResult = JumpLaneResult.from_dict(data)

	assert_equal(restored.get_total_connections(), 1)
	assert_equal(restored.get_total_orphans(), 1)
	assert_equal(restored.systems.size(), 2)
	assert_true(restored.systems.has("s1"))
	assert_true(restored.systems.has("s2"))
	assert_true(restored.is_orphan("orphan_x"))
