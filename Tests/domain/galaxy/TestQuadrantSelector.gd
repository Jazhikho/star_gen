## Tests for QuadrantSelector — ray-based click picking and selection state.
class_name TestQuadrantSelector
extends TestCase


var _selector: QuadrantSelector


func before_each() -> void:
	_selector = QuadrantSelector.new()


func test_picks_quadrant_when_ray_hits() -> void:
	# Quadrant (0,0,0) spans parsecs (0,0,0) to (1000,1000,1000), center at (500,500,500)
	var target: Vector3 = Vector3(500.0, 500.0, 500.0)
	var ray_origin: Vector3 = Vector3(500.0, 500.0, -5000.0)
	var ray_direction: Vector3 = (target - ray_origin).normalized()

	var coords: Array[Vector3i] = [Vector3i(0, 0, 0)]
	var result: Variant = _selector.pick_from_ray(ray_origin, ray_direction, coords)

	assert_not_null(result, "Should pick the quadrant the ray hits")
	assert_equal(result as Vector3i, Vector3i(0, 0, 0), "Should pick quadrant (0,0,0)")


func test_returns_null_when_ray_misses_all() -> void:
	# Ray pointing straight up, not at any quadrant along Z
	var ray_origin: Vector3 = Vector3(500.0, -5000.0, 500.0)
	var ray_direction: Vector3 = Vector3(1.0, 0.0, 0.0)

	var coords: Array[Vector3i] = [Vector3i(0, 0, 0)]
	var result: Variant = _selector.pick_from_ray(ray_origin, ray_direction, coords)

	assert_null(result, "Should return null when ray misses all quadrants")


func test_returns_null_for_empty_coords_list() -> void:
	var ray_origin: Vector3 = Vector3(0.0, 0.0, -5000.0)
	var ray_direction: Vector3 = Vector3(0.0, 0.0, 1.0)

	var coords: Array[Vector3i] = []
	var result: Variant = _selector.pick_from_ray(ray_origin, ray_direction, coords)

	assert_null(result, "Should return null for empty occupied coords")


func test_picks_nearest_when_ray_hits_multiple() -> void:
	# Ray along +Z axis passes through multiple quadrants stacked in Z
	var ray_origin: Vector3 = Vector3(500.0, 500.0, -5000.0)
	var ray_direction: Vector3 = Vector3(0.0, 0.0, 1.0)

	var coords: Array[Vector3i] = [
		Vector3i(0, 0, 0),   # z: 0 to 1000 — distance 5000
		Vector3i(0, 0, 2),   # z: 2000 to 3000 — distance 7000
		Vector3i(0, 0, -3),  # z: -3000 to -2000 — distance 2000 (nearest)
	]
	var result: Variant = _selector.pick_from_ray(ray_origin, ray_direction, coords)

	assert_not_null(result, "Should pick the nearest quadrant")
	assert_equal(result as Vector3i, Vector3i(0, 0, -3),
		"Should pick the closest quadrant along the ray")


func test_picks_correct_quadrant_at_negative_coords() -> void:
	# Target center of quadrant (-2, -1, -1) = (-1500, -500, -500)
	var target: Vector3 = Vector3(-1500.0, -500.0, -500.0)
	var ray_origin: Vector3 = Vector3(-1500.0, -500.0, -5000.0)
	var ray_direction: Vector3 = (target - ray_origin).normalized()

	var coords: Array[Vector3i] = [
		Vector3i(0, 0, 0),
		Vector3i(-2, -1, -1),
		Vector3i(3, 0, 2),
	]
	var result: Variant = _selector.pick_from_ray(ray_origin, ray_direction, coords)

	assert_not_null(result, "Should pick negative-coord quadrant")
	assert_equal(result as Vector3i, Vector3i(-2, -1, -1),
		"Should pick the quadrant at negative coordinates")


func test_initial_selection_is_null() -> void:
	assert_null(_selector.selected_coords, "Should start with no selection")
	assert_false(_selector.has_selection(), "has_selection should be false initially")


func test_set_selection() -> void:
	_selector.set_selection(Vector3i(1, 2, 3))
	assert_true(_selector.has_selection(), "has_selection should be true after set")
	assert_equal(
		_selector.selected_coords as Vector3i, Vector3i(1, 2, 3),
		"selected_coords should match what was set"
	)


func test_clear_selection() -> void:
	_selector.set_selection(Vector3i(5, 5, 5))
	_selector.clear_selection()
	assert_null(_selector.selected_coords, "Should be null after clear")
	assert_false(_selector.has_selection(), "has_selection should be false after clear")


func test_set_selection_to_null() -> void:
	_selector.set_selection(Vector3i(1, 1, 1))
	_selector.set_selection(null)
	assert_null(_selector.selected_coords, "Setting null should clear selection")
	assert_false(_selector.has_selection(), "has_selection should be false after null set")
