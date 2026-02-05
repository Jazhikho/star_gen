## Tests for JumpLaneCalculator.
extends TestCase


## Preload calculator script so tests compile without depending on class_name load order.
const _calculator_script: GDScript = preload("res://src/domain/jumplanes/JumpLaneCalculator.gd")

## Calculator instance (JumpLaneCalculator); typed as RefCounted to avoid class_name dependency.
var calculator: RefCounted


func before_each() -> void:
	calculator = _calculator_script.new()


## Helper to create a system at a position.
func _make_system(id: String, x: float, y: float, z: float, pop: int) -> JumpLaneSystem:
	return JumpLaneSystem.new(id, Vector3(x, y, z), pop)


# =============================================================================
# Basic connection tests
# =============================================================================

func test_empty_region_returns_empty_result() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 0)


func test_single_system_is_orphan() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("only", 0, 0, 0, 1000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 1)
	assert_true(result.is_orphan("only"))


func test_two_systems_within_3pc_green_connection() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 2, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.get_total_orphans(), 0)

	var conn: JumpLaneConnection = result.connections[0]
	assert_equal(conn.source_id, "low")
	assert_equal(conn.destination_id, "high")
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.GREEN)
	assert_float_equal(conn.distance_pc, 2.0)


func test_two_systems_within_5pc_green_connection() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 4, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	var conn: JumpLaneConnection = result.connections[0]
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.GREEN)


func test_two_systems_at_7pc_no_bridge_orange_connection() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 7, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	var conn: JumpLaneConnection = result.connections[0]
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.ORANGE)
	assert_float_equal(conn.distance_pc, 7.0)


func test_two_systems_at_9pc_no_bridge_no_connection() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 9, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 2)


func test_two_systems_beyond_9pc_no_connection() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 15, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 2)


# =============================================================================
# Bridge tests
# =============================================================================

func test_bridge_creates_yellow_connections() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 8, 0, 0, 5000))
	region.add_system(_make_system("bridge", 4, 0, 0, 0))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 2)
	assert_equal(result.get_total_orphans(), 0)

	var counts: Dictionary = result.get_connection_counts()
	assert_equal(counts[JumpLaneConnection.ConnectionType.YELLOW], 2)


func test_bridge_gets_false_population() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 8, 0, 0, 50000))
	region.add_system(_make_system("bridge", 4, 0, 0, 0))

	var result: JumpLaneResult = calculator.calculate(region)

	var bridge_system: JumpLaneSystem = result.systems["bridge"]
	assert_true(bridge_system.is_bridge)
	assert_equal(bridge_system.false_population, 40000)


func test_bridge_not_used_as_source() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 8, 0, 0, 50000))
	region.add_system(_make_system("bridge", 4, 0, 0, 0))
	region.add_system(_make_system("mid", 5, 1, 0, 20000))

	var result: JumpLaneResult = calculator.calculate(region)

	var bridge_as_source_count: int = 0
	for conn in result.connections:
		if conn.source_id == "bridge":
			bridge_as_source_count += 1

	assert_equal(bridge_as_source_count, 1, "Bridge should only be source in its yellow connection to high")


func test_bridge_must_be_within_5pc_of_both() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("high", 8, 0, 0, 5000))
	region.add_system(_make_system("bad_bridge", 6, 0, 0, 0))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 2)


# =============================================================================
# Selection priority tests
# =============================================================================

func test_connects_to_highest_populated_within_threshold() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("source", 0, 0, 0, 1000))
	region.add_system(_make_system("near_low", 2, 0, 0, 2000))
	region.add_system(_make_system("near_high", 2.5, 0, 0, 8000))

	var result: JumpLaneResult = calculator.calculate(region)

	var source_conns: Array[JumpLaneConnection] = result.get_connections_for_system("source")
	var found_high: bool = false
	for conn in source_conns:
		if conn.source_id == "source" and conn.destination_id == "near_high":
			found_high = true
	assert_true(found_high, "Should connect to highest populated within threshold")


func test_processes_lowest_population_first() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("low", 0, 0, 0, 1000))
	region.add_system(_make_system("mid", 3, 0, 0, 5000))
	region.add_system(_make_system("high", 6, 0, 0, 10000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 2)

	var low_to_mid: bool = false
	var mid_to_high: bool = false
	for conn in result.connections:
		if conn.source_id == "low" and conn.destination_id == "mid":
			low_to_mid = true
		if conn.source_id == "mid" and conn.destination_id == "high":
			mid_to_high = true

	assert_true(low_to_mid, "low should connect to mid")
	assert_true(mid_to_high, "mid should connect to high")


# =============================================================================
# Complex scenario tests
# =============================================================================

func test_multiple_orphans() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 20, 0, 0, 2000))
	region.add_system(_make_system("c", 40, 0, 0, 3000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 3)


func test_chain_of_connections() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 3, 0, 0, 2000))
	region.add_system(_make_system("c", 6, 0, 0, 3000))
	region.add_system(_make_system("d", 9, 0, 0, 4000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 3)
	assert_equal(result.get_total_orphans(), 0)


func test_mixed_connection_types() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 2, 0, 0, 2000))
	region.add_system(_make_system("c", 9, 0, 0, 3000))
	region.add_system(_make_system("d", 17, 0, 0, 4000))
	region.add_system(_make_system("bridge", 13, 0, 0, 0))

	var result: JumpLaneResult = calculator.calculate(region)

	var counts: Dictionary = result.get_connection_counts()
	assert_greater_than(counts[JumpLaneConnection.ConnectionType.GREEN], 0)
	assert_greater_than(counts[JumpLaneConnection.ConnectionType.ORANGE], 0)
	assert_greater_than(counts[JumpLaneConnection.ConnectionType.YELLOW], 0)


func test_system_can_receive_multiple_inbound_connections() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("hub", 0, 0, 0, 100000))
	region.add_system(_make_system("spoke_a", 2, 0, 0, 1000))
	region.add_system(_make_system("spoke_b", 0, 2, 0, 2000))
	region.add_system(_make_system("spoke_c", 0, 0, 2, 3000))

	var result: JumpLaneResult = calculator.calculate(region)

	var hub_connections: Array[JumpLaneConnection] = result.get_connections_for_system("hub")
	assert_equal(hub_connections.size(), 3, "Hub should have 3 inbound connections")

	assert_equal(result.get_total_orphans(), 0)


func test_highest_pop_system_is_not_orphan_when_receiving_connections() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("top", 0, 0, 0, 100000))
	region.add_system(_make_system("low", 2, 0, 0, 1000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_false(result.is_orphan("top"), "Top should not be orphan - it receives connection")
	assert_false(result.is_orphan("low"), "Low should not be orphan - it makes connection")
	assert_equal(result.get_total_connections(), 1)


func test_unpopulated_systems_ignored_as_destinations() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("pop", 0, 0, 0, 1000))
	region.add_system(_make_system("unpop", 2, 0, 0, 0))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 1)
	assert_true(result.is_orphan("pop"))


func test_3d_distances() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 2, 2, 1, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	var conn: JumpLaneConnection = result.connections[0]
	assert_equal(conn.connection_type, JumpLaneConnection.ConnectionType.GREEN)
	assert_float_equal(conn.distance_pc, 3.0)


# =============================================================================
# Edge cases
# =============================================================================

func test_equal_population_still_connects() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 2, 0, 0, 1000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_orphans(), 2)


func test_exactly_at_threshold_boundaries() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 3, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.connections[0].connection_type, JumpLaneConnection.ConnectionType.GREEN)


func test_just_beyond_threshold() -> void:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	region.add_system(_make_system("a", 0, 0, 0, 1000))
	region.add_system(_make_system("b", 3.01, 0, 0, 5000))

	var result: JumpLaneResult = calculator.calculate(region)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.connections[0].connection_type, JumpLaneConnection.ConnectionType.GREEN)
