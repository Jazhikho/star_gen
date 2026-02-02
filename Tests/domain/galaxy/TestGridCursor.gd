## Tests for GridCursor — directional navigation through occupied grids.
class_name TestGridCursor
extends TestCase


var _occupied: Array[Vector3i]


func before_each() -> void:
	_occupied = [
		Vector3i(0, 0, 0),
		Vector3i(1, 0, 0),
		Vector3i(2, 0, 0),
		Vector3i(0, 1, 0),
		Vector3i(0, 0, 1),
		Vector3i(-1, 0, 0),
		Vector3i(0, -1, 0),
		Vector3i(3, 2, 1),
	] as Array[Vector3i]


func test_find_nearest_positive_x() -> void:
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(1, 0, 0), _occupied
	)
	assert_not_null(result, "Should find a coord in +X direction")
	assert_equal(result as Vector3i, Vector3i(1, 0, 0),
		"Should find the nearest in +X")


func test_find_nearest_negative_x() -> void:
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(-1, 0, 0), _occupied
	)
	assert_not_null(result, "Should find a coord in -X direction")
	assert_equal(result as Vector3i, Vector3i(-1, 0, 0),
		"Should find the nearest in -X")


func test_find_nearest_positive_y() -> void:
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(0, 1, 0), _occupied
	)
	assert_not_null(result, "Should find a coord in +Y direction")
	assert_equal(result as Vector3i, Vector3i(0, 1, 0),
		"Should find the nearest in +Y")


func test_find_nearest_positive_z() -> void:
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(0, 0, 1), _occupied
	)
	assert_not_null(result, "Should find a coord in +Z direction")
	assert_equal(result as Vector3i, Vector3i(0, 0, 1),
		"Should find the nearest in +Z")


func test_returns_null_when_nothing_in_direction() -> void:
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(3, 2, 1), Vector3i(1, 0, 0), _occupied
	)
	assert_null(result, "Should return null when no coords in +X from (3,2,1)")


func test_returns_null_for_empty_list() -> void:
	var empty: Array[Vector3i] = [] as Array[Vector3i]
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(1, 0, 0), empty
	)
	assert_null(result, "Should return null for empty occupied list")


func test_skips_current_position() -> void:
	var single: Array[Vector3i] = [Vector3i(0, 0, 0)] as Array[Vector3i]
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(1, 0, 0), single
	)
	assert_null(result, "Should not return current position")


func test_finds_closest_among_multiple_candidates() -> void:
	# From origin, +X has candidates at (1,0,0), (2,0,0), (3,2,1)
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(0, 0, 0), Vector3i(1, 0, 0), _occupied
	)
	assert_equal(result as Vector3i, Vector3i(1, 0, 0),
		"Should pick closest candidate in +X direction")


func test_finds_diagonal_candidate_in_direction() -> void:
	# (3,2,1) is in +X from (2,0,0) even though it's offset in Y and Z
	var result: Variant = GridCursor.find_nearest_in_direction(
		Vector3i(2, 0, 0), Vector3i(1, 0, 0), _occupied
	)
	assert_not_null(result, "Should find diagonal candidate in +X")
	assert_equal(result as Vector3i, Vector3i(3, 2, 1),
		"Should find (3,2,1) as next in +X from (2,0,0)")


func test_find_nearest_to_position() -> void:
	var result: Variant = GridCursor.find_nearest(
		Vector3i(1, 1, 0), _occupied
	)
	assert_not_null(result, "Should find nearest overall")
	# (1,0,0) and (0,1,0) are both distance 1, but (1,0,0) should come first
	# Either is acceptable as long as it's distance 1
	var coords: Vector3i = result as Vector3i
	var delta: Vector3i = coords - Vector3i(1, 1, 0)
	var dist_sq: float = float(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z)
	assert_float_equal(dist_sq, 1.0, 0.01,
		"Nearest should be at distance 1 from (1,1,0)")


func test_find_nearest_empty_list() -> void:
	var empty: Array[Vector3i] = [] as Array[Vector3i]
	var result: Variant = GridCursor.find_nearest(Vector3i(0, 0, 0), empty)
	assert_null(result, "Should return null for empty list")


func test_move_updates_position() -> void:
	var cursor: GridCursor = GridCursor.new()
	cursor.position = Vector3i(0, 0, 0)
	var result: Variant = cursor.move_in_direction(Vector3i(1, 0, 0), _occupied)

	assert_not_null(result, "Move should succeed")
	assert_equal(cursor.position, Vector3i(1, 0, 0),
		"Cursor position should update after move")


func test_move_returns_null_when_blocked() -> void:
	var cursor: GridCursor = GridCursor.new()
	cursor.position = Vector3i(3, 2, 1)
	var result: Variant = cursor.move_in_direction(Vector3i(1, 0, 0), _occupied)

	assert_null(result, "Move should return null when no target in direction")
	assert_equal(cursor.position, Vector3i(3, 2, 1),
		"Cursor position should not change on failed move")


func test_snap_to_nearest() -> void:
	var cursor: GridCursor = GridCursor.new()
	cursor.position = Vector3i(5, 5, 5)
	var result: Variant = cursor.snap_to_nearest(_occupied)

	assert_not_null(result, "Snap should find a target")
	assert_equal(cursor.position, result as Vector3i,
		"Cursor position should match snap result")


func test_sequential_moves() -> void:
	var cursor: GridCursor = GridCursor.new()
	cursor.position = Vector3i(0, 0, 0)

	cursor.move_in_direction(Vector3i(1, 0, 0), _occupied)
	assert_equal(cursor.position, Vector3i(1, 0, 0), "First move +X")

	cursor.move_in_direction(Vector3i(1, 0, 0), _occupied)
	assert_equal(cursor.position, Vector3i(2, 0, 0), "Second move +X")

	cursor.move_in_direction(Vector3i(-1, 0, 0), _occupied)
	assert_equal(cursor.position, Vector3i(1, 0, 0), "Move back -X")
