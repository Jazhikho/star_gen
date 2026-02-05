## Identifies clusters of connected systems and connects nearby clusters.
class_name JumpLaneClusterConnector
extends RefCounted


## Maximum distance for inter-cluster connections.
const MAX_CLUSTER_DISTANCE: float = 9.0

## Maximum distance for extended (red) connections: direct or per-hop.
const MAX_EXTENDED_DISTANCE: float = 10.0

## Distance thresholds (same as calculator).
const THRESHOLD_GREEN: float = 5.0
const THRESHOLD_ORANGE: float = 7.0
const BRIDGE_MAX_DISTANCE: float = 5.0


## Connects isolated clusters until no more connections possible.
## @param region: The region containing all systems.
## @param result: The current result to modify in place.
func connect_clusters(region: JumpLaneRegion, result: JumpLaneResult) -> void:
	var iterations: int = 0
	var max_iterations: int = 100

	while iterations < max_iterations:
		var new_connection: bool = _try_connect_one_cluster_pair(region, result)
		if not new_connection:
			break
		iterations += 1

	_try_extended_connections(region, result)


## Attempts to connect one pair of clusters.
## @return: True if a connection was made.
func _try_connect_one_cluster_pair(region: JumpLaneRegion, result: JumpLaneResult) -> bool:
	var clusters: Array = _identify_clusters(result)

	if clusters.size() < 2:
		return false

	var best_pair: Dictionary = _find_closest_cluster_pair(clusters, region, result)

	if best_pair.is_empty():
		return false

	_create_cluster_connection(best_pair, region, result)
	return true


## After standard connections, tries to connect remaining isolated clusters
## with extended (red) links: direct ≤10 pc or multi-hop path (each hop ≤10 pc).
func _try_extended_connections(region: JumpLaneRegion, result: JumpLaneResult) -> void:
	var ext_iterations: int = 0
	var ext_max: int = 100

	while ext_iterations < ext_max:
		var clusters: Array = _identify_clusters(result)
		if clusters.size() < 2:
			break

		var best_pair: Dictionary = _find_closest_cluster_pair_within(
			clusters,
			region,
			result,
			MAX_EXTENDED_DISTANCE
		)
		if not best_pair.is_empty():
			_create_direct_red_connection(best_pair, result)
			ext_iterations += 1
			continue

		var path_info: Dictionary = _find_multi_hop_path(clusters, region, result)
		if path_info.is_empty():
			break
		_create_multi_hop_red_connections(path_info, result)
		ext_iterations += 1


## Finds the closest pair of clusters within max_distance (for extended phase).
## @return: Dictionary with cluster indices and closest systems, or empty.
func _find_closest_cluster_pair_within(
	clusters: Array,
	region: JumpLaneRegion,
	result: JumpLaneResult,
	max_distance: float
) -> Dictionary:
	var best_distance: float = INF
	var best_pair: Dictionary = {}

	for i in range(clusters.size()):
		for j in range(i + 1, clusters.size()):
			var pair_info: Dictionary = _find_closest_systems_between_clusters(
				clusters[i],
				clusters[j],
				result
			)
			if pair_info.distance < best_distance and pair_info.distance <= max_distance:
				best_distance = pair_info.distance
				best_pair = pair_info
				best_pair.cluster_a = clusters[i]
				best_pair.cluster_b = clusters[j]
				best_pair.region = region

	return best_pair


## Creates a single direct red connection between two clusters (≤10 pc).
func _create_direct_red_connection(pair_info: Dictionary, result: JumpLaneResult) -> void:
	var system_a: JumpLaneSystem = result.systems.get(pair_info.system_a)
	var system_b: JumpLaneSystem = result.systems.get(pair_info.system_b)
	if system_a == null or system_b == null:
		return

	var source: JumpLaneSystem = system_a
	var dest: JumpLaneSystem = system_b
	if system_b.get_effective_population() < system_a.get_effective_population():
		source = system_b
		dest = system_a

	var distance: float = source.distance_to(dest)
	var conn: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		dest.id,
		JumpLaneConnection.ConnectionType.RED,
		distance
	)
	result.add_connection(conn)
	_remove_from_orphans(source.id, result)
	_remove_from_orphans(dest.id, result)


## Finds a multi-hop path between any two clusters where each hop is ≤10 pc.
## Tries all cluster pairs; returns first path found.
## @return: Dictionary with "path" (Array of system IDs) or empty.
func _find_multi_hop_path(
	clusters: Array,
	_region: JumpLaneRegion,
	result: JumpLaneResult
) -> Dictionary:
	var graph: Dictionary = _build_extended_graph(result, MAX_EXTENDED_DISTANCE)

	for i in range(clusters.size()):
		for j in range(i + 1, clusters.size()):
			var path: Array = _bfs_path(
				clusters[i],
				clusters[j],
				graph,
				result
			)
			if path.size() > 0:
				return {"path": path}
	return {}


## Builds adjacency for extended phase: each system -> list of system IDs within max_pc.
func _build_extended_graph(result: JumpLaneResult, max_pc: float) -> Dictionary:
	var graph: Dictionary = {}
	var ids: Array = result.systems.keys()

	for id_a in ids:
		graph[id_a] = []
		var sys_a: JumpLaneSystem = result.systems.get(id_a)
		if sys_a == null:
			continue
		for id_b in ids:
			if id_a == id_b:
				continue
			var sys_b: JumpLaneSystem = result.systems.get(id_b)
			if sys_b == null:
				continue
			if sys_a.distance_to(sys_b) <= max_pc:
				graph[id_a].append(id_b)
	return graph


## BFS from any node in cluster_a to any node in cluster_b; returns path of system IDs.
func _bfs_path(
	cluster_a: Array,
	cluster_b: Array,
	graph: Dictionary,
	_result: JumpLaneResult
) -> Array:
	var cluster_b_set: Dictionary = {}
	for id_b in cluster_b:
		cluster_b_set[id_b] = true

	var queue: Array = []
	var parent: Dictionary = {}
	for id_a in cluster_a:
		queue.append(id_a)
		parent[id_a] = ""

	while queue.size() > 0:
		var current: String = queue.pop_front()
		if cluster_b_set.has(current):
			var path: Array = []
			var node: String = current
			while node != "":
				path.insert(0, node)
				node = parent.get(node, "")
			return path

		for neighbor in graph.get(current, []):
			if not parent.has(neighbor):
				parent[neighbor] = current
				queue.append(neighbor)
	return []


## Creates red connections along a multi-hop path and marks intermediates as bridge.
func _create_multi_hop_red_connections(path_info: Dictionary, result: JumpLaneResult) -> void:
	var path: Array = path_info.get("path", [])
	if path.size() < 2:
		return

	for k in range(path.size() - 1):
		var id_a: String = path[k]
		var id_b: String = path[k + 1]
		if _connection_exists(result, id_a, id_b):
			continue
		var sys_a: JumpLaneSystem = result.systems.get(id_a)
		var sys_b: JumpLaneSystem = result.systems.get(id_b)
		if sys_a == null or sys_b == null:
			continue

		var source: JumpLaneSystem = sys_a
		var dest: JumpLaneSystem = sys_b
		if sys_b.get_effective_population() < sys_a.get_effective_population():
			source = sys_b
			dest = sys_a

		var dist: float = source.distance_to(dest)
		var conn: JumpLaneConnection = JumpLaneConnection.new(
			source.id,
			dest.id,
			JumpLaneConnection.ConnectionType.RED,
			dist
		)
		result.add_connection(conn)

		var from_sys: JumpLaneSystem = result.systems.get(id_a)
		if k > 0 and from_sys != null:
			from_sys.is_bridge = true
			var higher_pop: int = dest.get_effective_population()
			if from_sys.get_effective_population() < higher_pop:
				from_sys.make_bridge(higher_pop)

	_remove_from_orphans(path[0], result)
	_remove_from_orphans(path[path.size() - 1], result)
	for idx in range(1, path.size() - 1):
		_remove_from_orphans(path[idx], result)


## Returns true if a connection already exists between the two systems.
func _connection_exists(result: JumpLaneResult, id_a: String, id_b: String) -> bool:
	for conn in result.connections:
		if (conn.source_id == id_a and conn.destination_id == id_b) or (conn.source_id == id_b and conn.destination_id == id_a):
			return true
	return false


## Identifies all clusters of connected systems.
## Only considers populated systems (or bridges with false_population).
## Unpopulated non-bridge systems are not part of any cluster.
## @return: Array of clusters, each cluster is Array of system IDs.
func _identify_clusters(result: JumpLaneResult) -> Array:
	var adjacency: Dictionary = {}

	for system_id in result.systems.keys():
		var system: JumpLaneSystem = result.systems.get(system_id)
		if system.is_populated() or system.is_bridge:
			adjacency[system_id] = []

	for conn in result.connections:
		if adjacency.has(conn.source_id) and adjacency.has(conn.destination_id):
			adjacency[conn.source_id].append(conn.destination_id)
			adjacency[conn.destination_id].append(conn.source_id)

	var visited: Dictionary = {}
	var clusters: Array = []

	for system_id in adjacency.keys():
		if visited.has(system_id):
			continue

		var cluster: Array[String] = []
		_flood_fill(system_id, adjacency, visited, cluster)

		if cluster.size() > 0:
			clusters.append(cluster)

	return clusters


## Flood fill to find all systems in a cluster.
func _flood_fill(
	start_id: String,
	adjacency: Dictionary,
	visited: Dictionary,
	cluster: Array[String]
) -> void:
	var stack: Array[String] = [start_id]

	while stack.size() > 0:
		var current: String = stack.pop_back()

		if visited.has(current):
			continue

		visited[current] = true
		cluster.append(current)

		for neighbor in adjacency.get(current, []):
			if not visited.has(neighbor):
				stack.append(neighbor)


## Finds the closest pair of clusters within connection range.
## @return: Dictionary with cluster indices and closest systems, or empty.
func _find_closest_cluster_pair(
	clusters: Array,
	region: JumpLaneRegion,
	result: JumpLaneResult
) -> Dictionary:
	var best_distance: float = INF
	var best_pair: Dictionary = {}

	for i in range(clusters.size()):
		for j in range(i + 1, clusters.size()):
			var pair_info: Dictionary = _find_closest_systems_between_clusters(
				clusters[i],
				clusters[j],
				result
			)

			if pair_info.distance < best_distance and pair_info.distance <= MAX_CLUSTER_DISTANCE:
				best_distance = pair_info.distance
				best_pair = pair_info
				best_pair.cluster_a = clusters[i]
				best_pair.cluster_b = clusters[j]
				best_pair.region = region

	return best_pair


## Finds the closest pair of systems between two clusters.
func _find_closest_systems_between_clusters(
	cluster_a: Array,
	cluster_b: Array,
	result: JumpLaneResult
) -> Dictionary:
	var best_distance: float = INF
	var best_system_a: String = ""
	var best_system_b: String = ""

	for id_a in cluster_a:
		var system_a: JumpLaneSystem = result.systems.get(id_a)
		if system_a == null:
			continue

		for id_b in cluster_b:
			var system_b: JumpLaneSystem = result.systems.get(id_b)
			if system_b == null:
				continue

			var dist: float = system_a.distance_to(system_b)
			if dist < best_distance:
				best_distance = dist
				best_system_a = id_a
				best_system_b = id_b

	return {
		"system_a": best_system_a,
		"system_b": best_system_b,
		"distance": best_distance
	}


## Creates a connection between two clusters.
func _create_cluster_connection(pair_info: Dictionary, region: JumpLaneRegion, result: JumpLaneResult) -> void:
	var system_a: JumpLaneSystem = result.systems.get(pair_info.system_a)
	var system_b: JumpLaneSystem = result.systems.get(pair_info.system_b)

	if system_a == null or system_b == null:
		return

	var source: JumpLaneSystem = system_a
	var dest: JumpLaneSystem = system_b
	if system_b.get_effective_population() < system_a.get_effective_population():
		source = system_b
		dest = system_a

	var distance: float = source.distance_to(dest)

	if distance <= THRESHOLD_GREEN:
		_add_green_connection(source, dest, distance, result)
	elif distance <= THRESHOLD_ORANGE:
		var bridge: JumpLaneSystem = _find_bridge(source, dest, region)
		if bridge != null:
			_add_bridged_connection(source, dest, bridge, result)
		else:
			_add_orange_connection(source, dest, distance, result)
	else:
		var bridge: JumpLaneSystem = _find_bridge(source, dest, region)
		if bridge != null:
			_add_bridged_connection(source, dest, bridge, result)


## Finds a valid bridge between two systems.
## Allows unpopulated systems and already-assigned bridges.
func _find_bridge(
	source: JumpLaneSystem,
	dest: JumpLaneSystem,
	region: JumpLaneRegion
) -> JumpLaneSystem:
	var best_bridge: JumpLaneSystem = null
	var best_total_distance: float = INF

	for system in region.systems:
		if system.id == source.id or system.id == dest.id:
			continue
		if system.is_populated() and not system.is_bridge:
			continue

		var dist_to_source: float = system.distance_to(source)
		var dist_to_dest: float = system.distance_to(dest)

		if dist_to_source <= BRIDGE_MAX_DISTANCE and dist_to_dest <= BRIDGE_MAX_DISTANCE:
			var total: float = dist_to_source + dist_to_dest
			if total < best_total_distance:
				best_bridge = system
				best_total_distance = total

	return best_bridge


## Adds a green connection.
func _add_green_connection(
	source: JumpLaneSystem,
	dest: JumpLaneSystem,
	distance: float,
	result: JumpLaneResult
) -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		dest.id,
		JumpLaneConnection.ConnectionType.GREEN,
		distance
	)
	result.add_connection(conn)
	_remove_from_orphans(source.id, result)
	_remove_from_orphans(dest.id, result)


## Adds an orange connection.
func _add_orange_connection(
	source: JumpLaneSystem,
	dest: JumpLaneSystem,
	distance: float,
	result: JumpLaneResult
) -> void:
	var conn: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		dest.id,
		JumpLaneConnection.ConnectionType.ORANGE,
		distance
	)
	result.add_connection(conn)
	_remove_from_orphans(source.id, result)
	_remove_from_orphans(dest.id, result)


## Adds a bridged (yellow) connection.
func _add_bridged_connection(
	source: JumpLaneSystem,
	dest: JumpLaneSystem,
	bridge: JumpLaneSystem,
	result: JumpLaneResult
) -> void:
	bridge.make_bridge(dest.get_effective_population())

	var conn1: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		bridge.id,
		JumpLaneConnection.ConnectionType.YELLOW,
		source.distance_to(bridge)
	)
	result.add_connection(conn1)

	var conn2: JumpLaneConnection = JumpLaneConnection.new(
		bridge.id,
		dest.id,
		JumpLaneConnection.ConnectionType.YELLOW,
		bridge.distance_to(dest)
	)
	result.add_connection(conn2)

	_remove_from_orphans(source.id, result)
	_remove_from_orphans(dest.id, result)
	_remove_from_orphans(bridge.id, result)


## Removes a system from the orphan list if present.
func _remove_from_orphans(system_id: String, result: JumpLaneResult) -> void:
	var idx: int = result.orphan_ids.find(system_id)
	if idx >= 0:
		result.orphan_ids.remove_at(idx)
