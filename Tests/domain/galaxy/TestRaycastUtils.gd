## Tests for RaycastUtils — ray-AABB intersection correctness.
class_name TestRaycastUtils
extends TestCase


## Standard unit AABB centered at origin for most tests.
var _unit_min: Vector3 = Vector3(-1.0, -1.0, -1.0)
var _unit_max: Vector3 = Vector3(1.0, 1.0, 1.0)


func test_ray_hits_aabb_from_front() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(0.0, 0.0, -5.0), Vector3(0.0, 0.0, 1.0),
		_unit_min, _unit_max
	)
	# Ray at z=-5 pointing +Z hits face at z=-1, distance = 4
	assert_float_equal(dist, 4.0, 0.001, "Should hit front face at distance 4")


func test_ray_hits_aabb_from_side() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(-5.0, 0.0, 0.0), Vector3(1.0, 0.0, 0.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, 4.0, 0.001, "Should hit left face at distance 4")


func test_ray_hits_aabb_from_above() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(0.0, 5.0, 0.0), Vector3(0.0, -1.0, 0.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, 4.0, 0.001, "Should hit top face at distance 4")


func test_ray_misses_aabb_pointing_away() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(0.0, 0.0, -5.0), Vector3(0.0, 1.0, 0.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, RaycastUtils.NO_HIT, 0.001, "Should miss when pointing away")


func test_ray_misses_aabb_behind() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(0.0, 0.0, 5.0), Vector3(0.0, 0.0, 1.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, RaycastUtils.NO_HIT, 0.001,
		"Should miss when AABB is behind ray")


func test_ray_origin_inside_aabb() -> void:
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(0.0, 0.0, 0.0), Vector3(0.0, 0.0, 1.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, 0.0, 0.001, "Should return 0 when ray starts inside AABB")


func test_ray_parallel_to_slab_inside() -> void:
	# Ray parallel to X axis, origin within Y and Z slabs
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(-5.0, 0.0, 0.0), Vector3(1.0, 0.0, 0.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, 4.0, 0.001, "Parallel ray inside slab should hit")


func test_ray_parallel_to_slab_outside() -> void:
	# Ray parallel to X axis, origin outside Y slab
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(-5.0, 5.0, 0.0), Vector3(1.0, 0.0, 0.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, RaycastUtils.NO_HIT, 0.001,
		"Parallel ray outside slab should miss")


func test_diagonal_ray_hits() -> void:
	var direction: Vector3 = Vector3(1.0, 1.0, 1.0).normalized()
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(-5.0, -5.0, -5.0), direction,
		_unit_min, _unit_max
	)
	assert_greater_than(dist, 0.0, "Diagonal ray toward AABB should hit")


func test_offset_aabb() -> void:
	# AABB at a non-origin position
	var aabb_min: Vector3 = Vector3(10.0, 10.0, 10.0)
	var aabb_max: Vector3 = Vector3(12.0, 12.0, 12.0)
	var target: Vector3 = Vector3(11.0, 11.0, 11.0)
	var origin: Vector3 = Vector3(11.0, 11.0, 0.0)
	var direction: Vector3 = (target - origin).normalized()

	var dist: float = RaycastUtils.ray_intersects_aabb(
		origin, direction, aabb_min, aabb_max
	)
	assert_float_equal(dist, 10.0, 0.001, "Should hit offset AABB at distance 10")


func test_near_miss() -> void:
	# Ray just barely missing the corner
	var dist: float = RaycastUtils.ray_intersects_aabb(
		Vector3(1.1, 1.1, -5.0), Vector3(0.0, 0.0, 1.0),
		_unit_min, _unit_max
	)
	assert_float_equal(dist, RaycastUtils.NO_HIT, 0.001,
		"Ray just outside corner should miss")
