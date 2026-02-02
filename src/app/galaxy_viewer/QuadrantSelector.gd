## Click-based quadrant selection using ray-AABB intersection.
##
## Given a camera ray and the list of occupied quadrant coordinates,
## finds the nearest quadrant the ray passes through.
class_name QuadrantSelector
extends RefCounted


## The currently selected quadrant coordinates, or null if none selected.
var selected_coords: Variant = null


## Picks the nearest occupied quadrant along a camera ray.
## @param ray_origin: World-space origin of the ray (from Camera3D.project_ray_origin).
## @param ray_direction: Normalised ray direction (from Camera3D.project_ray_normal).
## @param occupied_coords: List of quadrant grid coordinates that have visible cells.
## @return: The Vector3i of the nearest hit quadrant, or null if the ray misses all.
func pick_from_ray(
	ray_origin: Vector3,
	ray_direction: Vector3,
	occupied_coords: Array[Vector3i]
) -> Variant:
	var best_coords: Variant = null
	var best_distance: float = INF

	var quadrant_size: float = GalaxyCoordinates.QUADRANT_SIZE_PC

	for coords in occupied_coords:
		var aabb_min: Vector3 = Vector3(
			float(coords.x) * quadrant_size,
			float(coords.y) * quadrant_size,
			float(coords.z) * quadrant_size
		)
		var aabb_max: Vector3 = aabb_min + Vector3(
			quadrant_size, quadrant_size, quadrant_size
		)

		var hit_distance: float = RaycastUtils.ray_intersects_aabb(
			ray_origin, ray_direction, aabb_min, aabb_max
		)

		if hit_distance >= 0.0 and hit_distance < best_distance:
			best_distance = hit_distance
			best_coords = coords

	return best_coords


## Sets the current selection to specific coordinates.
## @param coords: Vector3i to select, or null to clear.
func set_selection(coords: Variant) -> void:
	selected_coords = coords


## Clears the current selection.
func clear_selection() -> void:
	selected_coords = null


## Returns whether a quadrant is currently selected.
## @return: True if a selection exists.
func has_selection() -> bool:
	return selected_coords != null
