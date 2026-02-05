## Tests for JumpLaneConnection data model.
extends TestCase


func test_init_defaults() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()

	assert_equal(conn.source_id, "")
	assert_equal(conn.destination_id, "")
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.GREEN)
	assert_float_equal(conn.distance_pc, 0.0)


func test_init_with_values() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new(
		"sys_a",
		"sys_b",
		JumpLaneConnection.ConnectionType.YELLOW,
		4.5
	)

	assert_equal(conn.source_id, "sys_a")
	assert_equal(conn.destination_id, "sys_b")
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.YELLOW)
	assert_float_equal(conn.distance_pc, 4.5)


func test_get_color_green() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.GREEN

	assert_equal(conn.get_color(), Color.GREEN)


func test_get_color_yellow() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.YELLOW

	assert_equal(conn.get_color(), Color.YELLOW)


func test_get_color_orange() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.ORANGE

	assert_equal(conn.get_color(), Color.ORANGE)


func test_get_type_name_green() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.GREEN

	assert_equal(conn.get_type_name(), "Direct (3-5 pc)")


func test_get_type_name_yellow() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.YELLOW

	assert_equal(conn.get_type_name(), "Bridged")


func test_get_type_name_orange() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new()
	conn.connection_type = JumpLaneConnection.ConnectionType.ORANGE

	assert_equal(conn.get_type_name(), "Direct (7 pc)")


func test_serialization_round_trip() -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new(
		"source",
		"dest",
		JumpLaneConnection.ConnectionType.ORANGE,
		7.0
	)

	var data: Dictionary = conn.to_dict()
	var restored: JumpLaneConnection = JumpLaneConnection.from_dict(data)

	assert_equal(restored.source_id, "source")
	assert_equal(restored.destination_id, "dest")
	assert_equal(restored.connection_type, JumpLaneConnection.ConnectionType.ORANGE)
	assert_float_equal(restored.distance_pc, 7.0)
