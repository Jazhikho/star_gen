## Pure-math ray intersection utilities.
## No Nodes, no scene tree — just geometry.
class_name RaycastUtils
extends RefCounted


## Sentinel value returned when a ray does not hit the target.
const NO_HIT: float = -1.0

## Tolerance for treating a ray component as parallel to a slab.
const PARALLEL_EPSILON: float = 1e-8


## Tests whether a ray intersects an axis-aligned bounding box using the slab method.
## @param ray_origin: Starting point of the ray in world space.
## @param ray_direction: Normalised direction of the ray.
## @param aabb_min: Minimum corner of the bounding box.
## @param aabb_max: Maximum corner of the bounding box.
## @return: Distance to the nearest intersection point, or NO_HIT if no hit.
static func ray_intersects_aabb(
	ray_origin: Vector3,
	ray_direction: Vector3,
	aabb_min: Vector3,
	aabb_max: Vector3
) -> float:
	var t_near: float = -1e20
	var t_far: float = 1e20

	for axis in range(3):
		if absf(ray_direction[axis]) < PARALLEL_EPSILON:
			# Ray is parallel to this slab — must be inside it to hit
			if ray_origin[axis] < aabb_min[axis] or ray_origin[axis] > aabb_max[axis]:
				return NO_HIT
		else:
			var inv_d: float = 1.0 / ray_direction[axis]
			var t1: float = (aabb_min[axis] - ray_origin[axis]) * inv_d
			var t2: float = (aabb_max[axis] - ray_origin[axis]) * inv_d

			# Ensure t1 is the near plane and t2 is the far plane
			if t1 > t2:
				var temp: float = t1
				t1 = t2
				t2 = temp

			t_near = maxf(t_near, t1)
			t_far = minf(t_far, t2)

			if t_near > t_far:
				return NO_HIT

	# AABB is entirely behind the ray origin
	if t_far < 0.0:
		return NO_HIT

	# If ray starts inside the AABB, return 0 (immediate hit)
	return maxf(t_near, 0.0)
