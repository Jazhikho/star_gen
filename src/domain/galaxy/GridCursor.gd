## Pure-logic navigation through a set of occupied 3D grid coordinates.
## Finds the nearest occupied cell in a given direction from the current position.
class_name GridCursor
extends RefCounted


## Current cursor position in grid coordinates.
var position: Vector3i = Vector3i.ZERO


## Finds the nearest occupied coordinate in a given cardinal direction.
## Direction should be a single-axis unit vector: (±1,0,0), (0,±1,0), or (0,0,±1).
## Candidates must have a positive displacement along the direction axis,
## and the closest by Euclidean distance is chosen.
## @param current: Starting grid position.
## @param direction: Cardinal direction as Vector3i.
## @param occupied: List of occupied grid coordinates.
## @return: Nearest occupied Vector3i in that direction, or null if none found.
static func find_nearest_in_direction(
	current: Vector3i,
	direction: Vector3i,
	occupied: Array[Vector3i]
) -> Variant:
	var best: Variant = null
	var best_dist_sq: float = INF

	for coords in occupied:
		if coords == current:
			continue

		var delta: Vector3i = coords - current

		if not _is_in_direction(delta, direction):
			continue

		var dist_sq: float = _distance_squared(delta)
		if dist_sq < best_dist_sq:
			best_dist_sq = dist_sq
			best = coords

	return best


## Finds the nearest occupied coordinate to a given position regardless of direction.
## @param target: Position to search from.
## @param occupied: List of occupied grid coordinates.
## @return: Nearest occupied Vector3i, or null if list is empty.
static func find_nearest(target: Vector3i, occupied: Array[Vector3i]) -> Variant:
	var best: Variant = null
	var best_dist_sq: float = INF

	for coords in occupied:
		var delta: Vector3i = coords - target
		var dist_sq: float = _distance_squared(delta)
		if dist_sq < best_dist_sq:
			best_dist_sq = dist_sq
			best = coords

	return best


## Moves the cursor in a direction among occupied coordinates.
## Updates position if a valid target is found.
## @param direction: Cardinal direction as Vector3i.
## @param occupied: List of occupied grid coordinates.
## @return: The new position if moved, or null if no valid target.
func move_in_direction(direction: Vector3i, occupied: Array[Vector3i]) -> Variant:
	var target: Variant = find_nearest_in_direction(position, direction, occupied)
	if target != null:
		position = target as Vector3i
	return target


## Snaps the cursor to the nearest occupied coordinate to its current position.
## @param occupied: List of occupied grid coordinates.
## @return: The snapped position, or null if list is empty.
func snap_to_nearest(occupied: Array[Vector3i]) -> Variant:
	var target: Variant = find_nearest(position, occupied)
	if target != null:
		position = target as Vector3i
	return target


## Checks whether a displacement vector is in the given cardinal direction.
## The delta must have a positive component along the direction's non-zero axis.
## @param delta: Displacement from current to candidate.
## @param direction: Cardinal direction vector.
## @return: True if the delta is in the correct direction.
static func _is_in_direction(delta: Vector3i, direction: Vector3i) -> bool:
	if direction.x != 0:
		if signi(delta.x) != signi(direction.x):
			return false
	if direction.y != 0:
		if signi(delta.y) != signi(direction.y):
			return false
	if direction.z != 0:
		if signi(delta.z) != signi(direction.z):
			return false
	return true


## Computes squared Euclidean distance from a Vector3i displacement.
## @param delta: Displacement vector.
## @return: Squared distance as float.
static func _distance_squared(delta: Vector3i) -> float:
	return float(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z)
