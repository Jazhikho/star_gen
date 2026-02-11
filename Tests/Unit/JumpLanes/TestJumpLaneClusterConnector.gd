## Tests for JumpLaneClusterConnector.
extends TestCase


## Preload to avoid class_name load order.
const _connector_script: GDScript = preload("res://src/domain/jumplanes/JumpLaneClusterConnector.gd")

## Connector instance; typed as RefCounted to avoid class_name dependency.
var connector: RefCounted


func before_each() -> void:
	connector = _connector_script.new()


func _make_system(id: String, x: float, y: float, z: float, pop: int) -> JumpLaneSystem:
	return JumpLaneSystem.new(id, Vector3(x, y, z), pop)


func _make_region_with_systems(systems: Array[JumpLaneSystem]) -> JumpLaneRegion:
	var region: JumpLaneRegion = JumpLaneRegion.new()
	for system in systems:
		region.add_system(system)
	return region


## Creates a result with systems registered. Only populated systems become orphans.
func _make_result_with_systems(systems: Array[JumpLaneSystem]) -> JumpLaneResult:
	var result: JumpLaneResult = JumpLaneResult.new()
	for system in systems:
		result.register_system(system)
		if system.is_populated():
			result.add_orphan(system.id)
	return result


# =============================================================================
# Cluster identification tests
# =============================================================================

func test_single_system_is_one_cluster() -> void:
	var systems: Array[JumpLaneSystem] = [_make_system("a", 0, 0, 0, 1000)]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 0)


func test_two_disconnected_systems_within_range_get_connected() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 5, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.get_total_orphans(), 0)


func test_two_disconnected_systems_beyond_range_stay_disconnected() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 15, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 2)


func test_three_clusters_connected_iteratively() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 6, 0, 0, 2000)
	var sys_c: JumpLaneSystem = _make_system("c", 12, 0, 0, 3000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b, sys_c]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 2)
	assert_equal(result.get_total_orphans(), 0)


func test_already_connected_systems_form_single_cluster() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 3, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = JumpLaneResult.new()
	result.register_system(sys_a)
	result.register_system(sys_b)

	result.add_connection(JumpLaneConnection.new("a", "b", JumpLaneConnection.ConnectionType.GREEN, 3.0))

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 1)


# =============================================================================
# Connection type tests
# =============================================================================

func test_cluster_connection_uses_green_within_5pc() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 4, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.connections[0].connection_type, JumpLaneConnection.ConnectionType.GREEN)


func test_cluster_connection_uses_orange_at_7pc_no_bridge() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 7, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.connections[0].connection_type, JumpLaneConnection.ConnectionType.ORANGE)


func test_cluster_connection_uses_bridge_when_available() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 8, 0, 0, 2000)
	var bridge: JumpLaneSystem = _make_system("bridge", 4, 0, 0, 0)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b, bridge]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	var counts: Dictionary = result.get_connection_counts()
	assert_equal(counts[JumpLaneConnection.ConnectionType.YELLOW], 2)
	assert_equal(result.get_total_orphans(), 0)


# =============================================================================
# Edge cases
# =============================================================================

func test_connects_closest_clusters_first() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 4, 0, 0, 2000)
	var sys_c: JumpLaneSystem = _make_system("c", 20, 0, 0, 3000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b, sys_c]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.get_total_orphans(), 1)
	assert_true(result.is_orphan("c"))


func test_orphan_removed_when_connected() -> void:
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 3, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	assert_equal(result.get_total_orphans(), 2)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_orphans(), 0)
	assert_false(result.is_orphan("a"))
	assert_false(result.is_orphan("b"))


func test_unpopulated_systems_not_in_clusters() -> void:
	# a and b within 9pc; unpop between them as bridge (not its own cluster).
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 8, 0, 0, 2000)
	var unpop: JumpLaneSystem = _make_system("unpop", 4, 0, 0, 0)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b, unpop]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	# a-b connected via unpop (yellow); unpop not treated as cluster endpoint.
	assert_greater_than(result.get_total_connections(), 0)
	assert_equal(result.get_total_orphans(), 0)


# =============================================================================
# Extended (red) connection tests
# =============================================================================

func test_extended_direct_red_when_clusters_within_10pc() -> void:
	# Beyond standard 9pc; within extended 10pc.
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var sys_b: JumpLaneSystem = _make_system("b", 10, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	assert_equal(result.get_total_connections(), 1)
	assert_equal(result.connections[0].connection_type, JumpLaneConnection.ConnectionType.RED)
	assert_equal(result.get_total_orphans(), 0)


func test_extended_multi_hop_red_when_stepping_stones_within_10pc() -> void:
	# a at 0, b at 16; mid at 8 gives hops 8 and 8 (each ≤10).
	var sys_a: JumpLaneSystem = _make_system("a", 0, 0, 0, 1000)
	var mid: JumpLaneSystem = _make_system("mid", 8, 0, 0, 0)
	var sys_b: JumpLaneSystem = _make_system("b", 16, 0, 0, 2000)

	var systems: Array[JumpLaneSystem] = [sys_a, mid, sys_b]
	var region: JumpLaneRegion = _make_region_with_systems(systems)
	var result: JumpLaneResult = _make_result_with_systems(systems)

	connector.connect_clusters(region, result)

	# Multi-hop red: a-mid and mid-b (or one path with two red edges).
	assert_equal(result.get_total_connections(), 2)
	var counts: Dictionary = result.get_connection_counts()
	assert_equal(counts.get(JumpLaneConnection.ConnectionType.RED, 0), 2)
	assert_equal(result.get_total_orphans(), 0)
	assert_true(result.systems.get("mid").is_bridge)
