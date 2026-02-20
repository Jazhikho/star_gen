## Calculates jump lane connections between star systems.
## Processes systems from lowest to highest population, connecting each
## to the highest-populated system within allowed distance thresholds.
## Then connects isolated clusters as a post-processing step.
## Script exceeds ~10 functions to keep the full algorithm in one place for readability and testability.
class_name JumpLaneCalculator
extends RefCounted


## Distance thresholds in parsecs.
const THRESHOLD_DIRECT_SHORT: float = 3.0 ## Green: direct connection
const THRESHOLD_DIRECT_MEDIUM: float = 5.0 ## Green: direct connection
const THRESHOLD_BRIDGE_MAX: float = 7.0 ## Orange if no bridge, or yellow with bridge
const THRESHOLD_BRIDGE_ONLY: float = 9.0 ## Only with bridge, else no connection
const BRIDGE_MAX_DISTANCE: float = 5.0 ## Max distance from bridge to either endpoint

## Cluster connector script (preloaded to avoid class_name load order).
const _cluster_connector_script: GDScript = preload("res://src/domain/jumplanes/JumpLaneClusterConnector.gd")

## Cluster connector for post-processing.
var _cluster_connector: RefCounted


func _init() -> void:
	_cluster_connector = _cluster_connector_script.new()


## Calculates jump lanes for all systems in a region.
## @param region: The region containing systems to process.
## @return: JumpLaneResult with connections and orphans.
func calculate(region: JumpLaneRegion) -> JumpLaneResult:
	var result: JumpLaneResult = JumpLaneResult.new()

	for system in region.systems:
		result.register_system(system)

	var connected_systems: Dictionary = {} # id -> bool
	var sorted_systems: Array[JumpLaneSystem] = region.get_systems_sorted_by_population()
	var bridge_ids: Dictionary = {} # id -> bool

	for system in sorted_systems:
		if bridge_ids.has(system.id):
			continue

		var connection_made: bool = _try_connect_system(
			system,
			region,
			result,
			connected_systems,
			bridge_ids
		)

		if connection_made:
			connected_systems[system.id] = true

	for system in sorted_systems:
		if not connected_systems.has(system.id):
			result.add_orphan(system.id)

	_cluster_connector.connect_clusters(region, result)

	return result


## Attempts to connect a system to a higher-populated system.
## Tries distance thresholds in order: 3pc, 5pc, 7pc (with bridge), 9pc (bridge only).
## @param system: The system to connect (source).
## @param region: The region containing all systems.
## @param result: The result to add connections to.
## @param connected_systems: Dictionary tracking connected system IDs.
## @param bridge_ids: Dictionary tracking bridge system IDs.
## @return: True if a connection was made.
func _try_connect_system(
	system: JumpLaneSystem,
	region: JumpLaneRegion,
	result: JumpLaneResult,
	connected_systems: Dictionary,
	bridge_ids: Dictionary
) -> bool:
	var candidates: Array[JumpLaneSystem] = _get_higher_populated_systems(system, region)

	if candidates.is_empty():
		return false

	if _try_threshold(system, candidates, THRESHOLD_DIRECT_SHORT, region, result, connected_systems, bridge_ids):
		return true
	if _try_threshold(system, candidates, THRESHOLD_DIRECT_MEDIUM, region, result, connected_systems, bridge_ids):
		return true
	if _try_extended_threshold(system, candidates, region, result, connected_systems, bridge_ids):
		return true

	return false


## Tries to connect within a direct threshold (green connection).
func _try_threshold(
	system: JumpLaneSystem,
	candidates: Array[JumpLaneSystem],
	threshold: float,
	_region: JumpLaneRegion,
	result: JumpLaneResult,
	connected_systems: Dictionary,
	_bridge_ids: Dictionary
) -> bool:
	var target: JumpLaneSystem = _find_highest_populated_within(system, candidates, threshold)
	if target != null:
		_add_direct_connection(system, target, result, connected_systems)
		return true
	return false


## Tries extended thresholds (7pc orange or 9pc with bridge).
func _try_extended_threshold(
	system: JumpLaneSystem,
	candidates: Array[JumpLaneSystem],
	region: JumpLaneRegion,
	result: JumpLaneResult,
	connected_systems: Dictionary,
	bridge_ids: Dictionary
) -> bool:
	var target: JumpLaneSystem = _find_highest_populated_within(system, candidates, THRESHOLD_BRIDGE_MAX)
	if target != null:
		var bridge: JumpLaneSystem = _find_bridge(system, target, region)
		if bridge != null:
			_add_bridged_connection(system, target, bridge, result, connected_systems, bridge_ids)
		else:
			_add_orange_connection(system, target, result, connected_systems)
		return true

	target = _find_highest_populated_within(system, candidates, THRESHOLD_BRIDGE_ONLY)
	if target != null:
		var distance: float = system.distance_to(target)
		var bridge: JumpLaneSystem = _find_bridge(system, target, region)
		if bridge != null:
			_add_bridged_connection(system, target, bridge, result, connected_systems, bridge_ids)
			return true
		elif distance <= THRESHOLD_BRIDGE_MAX:
			_add_orange_connection(system, target, result, connected_systems)
			return true

	return false


## Gets all systems with higher effective population than the source.
## @param source: The source system.
## @param region: The region to search.
## @return: Array of higher-populated systems.
func _get_higher_populated_systems(source: JumpLaneSystem, region: JumpLaneRegion) -> Array[JumpLaneSystem]:
	var out: Array[JumpLaneSystem] = []
	var source_pop: int = source.get_effective_population()

	for system in region.systems:
		if system.id == source.id:
			continue
		if not system.is_populated() and system.false_population < 0:
			continue
		if system.get_effective_population() > source_pop:
			out.append(system)

	return out


## Finds the highest-populated system within a distance threshold.
## @param source: The source system.
## @param candidates: Array of candidate destination systems.
## @param max_distance: Maximum distance in parsecs.
## @return: The highest-populated system within range, or null.
func _find_highest_populated_within(
	source: JumpLaneSystem,
	candidates: Array[JumpLaneSystem],
	max_distance: float
) -> JumpLaneSystem:
	var best: JumpLaneSystem = null
	var best_pop: int = -1

	for candidate in candidates:
		var distance: float = source.distance_to(candidate)
		if distance <= max_distance:
			var pop: int = candidate.get_effective_population()
			if pop > best_pop:
				best = candidate
				best_pop = pop

	return best


## Finds a bridge system between two systems.
## A bridge must be unpopulated (or already a bridge) and within 5pc of both systems.
## @param source: The source system.
## @param target: The target system.
## @param region: The region to search for bridges.
## @return: A suitable bridge system, or null.
func _find_bridge(
	source: JumpLaneSystem,
	target: JumpLaneSystem,
	region: JumpLaneRegion
) -> JumpLaneSystem:
	var best_bridge: JumpLaneSystem = null
	var best_total_distance: float = INF

	for system in region.systems:
		if system.id == source.id or system.id == target.id:
			continue
		if system.is_populated():
			continue

		var dist_to_source: float = system.distance_to(source)
		var dist_to_target: float = system.distance_to(target)

		if dist_to_source <= BRIDGE_MAX_DISTANCE and dist_to_target <= BRIDGE_MAX_DISTANCE:
			var total_distance: float = dist_to_source + dist_to_target
			if total_distance < best_total_distance:
				best_bridge = system
				best_total_distance = total_distance

	return best_bridge


## Adds a direct green connection.
## @param source: The source system.
## @param target: The target system.
## @param result: The result to add to.
## @param connected_systems: Dictionary to update.
func _add_direct_connection(
	source: JumpLaneSystem,
	target: JumpLaneSystem,
	result: JumpLaneResult,
	connected_systems: Dictionary
) -> void:
	var distance: float = source.distance_to(target)
	var connection: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		target.id,
		JumpLaneConnection.ConnectionType.GREEN,
		distance
	)
	result.add_connection(connection)
	connected_systems[source.id] = true
	connected_systems[target.id] = true


## Adds a direct orange connection (7pc, no bridge).
## @param source: The source system.
## @param target: The target system.
## @param result: The result to add to.
## @param connected_systems: Dictionary to update.
func _add_orange_connection(
	source: JumpLaneSystem,
	target: JumpLaneSystem,
	result: JumpLaneResult,
	connected_systems: Dictionary
) -> void:
	var distance: float = source.distance_to(target)
	var connection: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		target.id,
		JumpLaneConnection.ConnectionType.ORANGE,
		distance
	)
	result.add_connection(connection)
	connected_systems[source.id] = true
	connected_systems[target.id] = true


## Adds a bridged yellow connection (two segments).
## @param source: The source system.
## @param target: The target system.
## @param bridge: The bridge system.
## @param result: The result to add to.
## @param connected_systems: Dictionary to update.
## @param bridge_ids: Dictionary to track bridge systems.
func _add_bridged_connection(
	source: JumpLaneSystem,
	target: JumpLaneSystem,
	bridge: JumpLaneSystem,
	result: JumpLaneResult,
	connected_systems: Dictionary,
	bridge_ids: Dictionary
) -> void:
	bridge.make_bridge(target.get_effective_population())
	bridge_ids[bridge.id] = true

	var dist_to_bridge: float = source.distance_to(bridge)
	var conn1: JumpLaneConnection = JumpLaneConnection.new(
		source.id,
		bridge.id,
		JumpLaneConnection.ConnectionType.YELLOW,
		dist_to_bridge
	)
	result.add_connection(conn1)

	var dist_to_target: float = bridge.distance_to(target)
	var conn2: JumpLaneConnection = JumpLaneConnection.new(
		bridge.id,
		target.id,
		JumpLaneConnection.ConnectionType.YELLOW,
		dist_to_target
	)
	result.add_connection(conn2)

	connected_systems[source.id] = true
	connected_systems[target.id] = true
	connected_systems[bridge.id] = true
