## Pure-math star picking via ray proximity.
## Finds the star whose position is closest to a ray, within an angular threshold.
class_name StarPicker
extends RefCounted


## Result of a pick operation.
class PickResult:
	extends RefCounted

	## Index of the picked star in the positions array.
	var star_index: int = -1

	## World-space position of the picked star.
	var world_position: Vector3 = Vector3.ZERO

	## Seed of the picked star.
	var star_seed: int = 0

	## Distance from ray origin to the closest point on the ray to the star.
	var ray_distance: float = 0.0

	## Perpendicular distance from the star to the ray.
	var lateral_distance: float = 0.0


## Picks the star nearest to a ray, within a maximum lateral distance.
## @param ray_origin: World-space origin of the pick ray.
## @param ray_direction: Normalised direction of the pick ray.
## @param positions: Packed array of star world positions.
## @param star_seeds: Packed array of star seeds (parallel to positions).
## @param max_lateral_distance: Maximum perpendicular distance to count as a hit.
## @return: PickResult if a star was found, null otherwise.
static func pick_nearest_to_ray(
	ray_origin: Vector3,
	ray_direction: Vector3,
	positions: PackedVector3Array,
	star_seeds: PackedInt64Array,
	max_lateral_distance: float
) -> Variant:
	var best_index: int = -1
	var best_lateral: float = INF
	var best_ray_dist: float = 0.0

	for i in range(positions.size()):
		var to_star: Vector3 = positions[i] - ray_origin
		var ray_dist: float = to_star.dot(ray_direction)

		# Star must be in front of the ray
		if ray_dist < 0.0:
			continue

		var closest_on_ray: Vector3 = ray_origin + ray_direction * ray_dist
		var lateral: float = closest_on_ray.distance_to(positions[i])

		if lateral < max_lateral_distance and lateral < best_lateral:
			best_lateral = lateral
			best_index = i
			best_ray_dist = ray_dist

	if best_index < 0:
		return null

	var result: PickResult = PickResult.new()
	result.star_index = best_index
	result.world_position = positions[best_index]
	result.star_seed = star_seeds[best_index]
	result.ray_distance = best_ray_dist
	result.lateral_distance = best_lateral
	return result
