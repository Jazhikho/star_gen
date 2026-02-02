## Tests for GalaxyCoordinates — grid conversions and bounds.
class_name TestGalaxyCoordinates
extends TestCase


func test_origin_maps_to_quadrant_zero() -> void:
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(Vector3(0.0, 0.0, 0.0))
	assert_equal(coords, Vector3i(0, 0, 0), "Origin should be in quadrant (0,0,0)")


func test_positive_position_correct_quadrant() -> void:
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(Vector3(500.0, 200.0, 999.9))
	assert_equal(coords, Vector3i(0, 0, 0), "Position inside first quadrant")


func test_position_at_boundary_rolls_to_next() -> void:
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(Vector3(1000.0, 0.0, 0.0))
	assert_equal(coords, Vector3i(1, 0, 0), "Exactly at 1000 should be quadrant 1")


func test_negative_position_correct_quadrant() -> void:
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(Vector3(-1.0, 0.0, 0.0))
	assert_equal(coords, Vector3i(-1, 0, 0), "Just below zero should be quadrant -1")


func test_negative_position_deep() -> void:
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(Vector3(-1500.0, -500.0, -2001.0))
	assert_equal(coords, Vector3i(-2, -1, -3), "Deep negative position")


func test_quadrant_center_positive() -> void:
	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(Vector3i(0, 0, 0))
	assert_float_equal(center.x, 500.0, 0.01, "Center X of quadrant (0,0,0)")
	assert_float_equal(center.y, 500.0, 0.01, "Center Y of quadrant (0,0,0)")
	assert_float_equal(center.z, 500.0, 0.01, "Center Z of quadrant (0,0,0)")


func test_quadrant_center_negative() -> void:
	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(Vector3i(-1, -1, -1))
	assert_float_equal(center.x, -500.0, 0.01, "Center X of quadrant (-1,-1,-1)")
	assert_float_equal(center.y, -500.0, 0.01, "Center Y of quadrant (-1,-1,-1)")
	assert_float_equal(center.z, -500.0, 0.01, "Center Z of quadrant (-1,-1,-1)")


func test_round_trip_positive() -> void:
	var original: Vector3 = Vector3(3456.0, 789.0, 1234.0)
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(original)
	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)

	# Center should be within the same quadrant as the original
	var recoords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(center)
	assert_equal(coords, recoords, "Round-trip: center maps back to same quadrant")


func test_round_trip_negative() -> void:
	var original: Vector3 = Vector3(-7890.0, -456.0, -123.0)
	var coords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(original)
	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)

	var recoords: Vector3i = GalaxyCoordinates.parsec_to_quadrant(center)
	assert_equal(coords, recoords, "Round-trip negative: center maps back to same quadrant")


func test_center_is_within_cell() -> void:
	var coords: Vector3i = Vector3i(3, -2, 7)
	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)

	# Center should be at the midpoint of the cell range
	var cell_min_x: float = float(coords.x) * GalaxyCoordinates.QUADRANT_SIZE_PC
	var cell_max_x: float = cell_min_x + GalaxyCoordinates.QUADRANT_SIZE_PC
	assert_greater_than(center.x, cell_min_x, "Center X above cell minimum")
	assert_less_than(center.x, cell_max_x, "Center X below cell maximum")


func test_grid_bounds_cover_galaxy() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var grid_min: Vector3i = GalaxyCoordinates.get_quadrant_grid_min(spec)
	var grid_max: Vector3i = GalaxyCoordinates.get_quadrant_grid_max(spec)

	# Grid should extend to cover the full radius and height
	var min_parsec: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(grid_min)
	var max_parsec: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(grid_max)

	assert_less_than(min_parsec.x, -spec.radius_pc + GalaxyCoordinates.QUADRANT_SIZE_PC,
		"Grid min X should cover negative radius")
	assert_greater_than(max_parsec.x, spec.radius_pc - GalaxyCoordinates.QUADRANT_SIZE_PC,
		"Grid max X should cover positive radius")


func test_grid_bounds_symmetric() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var grid_min: Vector3i = GalaxyCoordinates.get_quadrant_grid_min(spec)
	var grid_max: Vector3i = GalaxyCoordinates.get_quadrant_grid_max(spec)

	# X and Z should be symmetric (galaxy is round in the plane)
	assert_equal(grid_min.x, grid_min.z, "Min X and Z should be equal for round galaxy")
	assert_equal(grid_max.x, grid_max.z, "Max X and Z should be equal for round galaxy")


func test_is_position_in_quadrant_true() -> void:
	var pos: Vector3 = Vector3(500.0, 500.0, 500.0)
	var result: bool = GalaxyCoordinates.is_position_in_quadrant(pos, Vector3i(0, 0, 0))
	assert_true(result, "Position at center of quadrant 0 should be in quadrant 0")


func test_is_position_in_quadrant_false() -> void:
	var pos: Vector3 = Vector3(1500.0, 500.0, 500.0)
	var result: bool = GalaxyCoordinates.is_position_in_quadrant(pos, Vector3i(0, 0, 0))
	assert_false(result, "Position in quadrant 1 should not be in quadrant 0")


func test_effective_half_height_uses_bulge_when_larger() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var effective: float = GalaxyCoordinates.get_effective_half_height(spec)
	var expected: float = spec.bulge_height_pc * GalaxyCoordinates.BULGE_SIGMA_COVERAGE

	assert_float_equal(effective, expected, 0.01,
		"Effective half-height should use bulge 3-sigma when larger than height_pc")


func test_effective_half_height_uses_height_when_larger() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	spec.height_pc = 5000.0
	spec.bulge_height_pc = 100.0
	var effective: float = GalaxyCoordinates.get_effective_half_height(spec)

	assert_float_equal(effective, 5000.0, 0.01,
		"Effective half-height should use height_pc when larger than bulge extent")


func test_effective_radius_uses_disk_when_larger() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var effective: float = GalaxyCoordinates.get_effective_radius(spec)

	assert_float_equal(effective, spec.radius_pc, 0.01,
		"Effective radius should use disk radius when larger than bulge extent")


func test_effective_radius_uses_bulge_when_larger() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	spec.radius_pc = 1000.0
	spec.bulge_radius_pc = 5000.0
	var effective: float = GalaxyCoordinates.get_effective_radius(spec)
	var expected: float = 5000.0 * GalaxyCoordinates.BULGE_SIGMA_COVERAGE

	assert_float_equal(effective, expected, 0.01,
		"Effective radius should use bulge 3-sigma when larger than disk radius")


func test_grid_bounds_cover_bulge_vertically() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var grid_min: Vector3i = GalaxyCoordinates.get_quadrant_grid_min(spec)
	var grid_max: Vector3i = GalaxyCoordinates.get_quadrant_grid_max(spec)

	var bulge_extent: float = spec.bulge_height_pc * GalaxyCoordinates.BULGE_SIGMA_COVERAGE
	var min_center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(grid_min)
	var max_center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(grid_max)

	assert_less_than(min_center.y, -bulge_extent + GalaxyCoordinates.QUADRANT_SIZE_PC,
		"Grid min should cover negative bulge extent")
	assert_greater_than(max_center.y, bulge_extent - GalaxyCoordinates.QUADRANT_SIZE_PC,
		"Grid max should cover positive bulge extent")


func test_grid_has_symmetric_y_layers() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(1)
	var grid_min: Vector3i = GalaxyCoordinates.get_quadrant_grid_min(spec)
	var grid_max: Vector3i = GalaxyCoordinates.get_quadrant_grid_max(spec)

	var y_layers: int = grid_max.y - grid_min.y + 1
	assert_greater_than(y_layers, 3,
		"Should have more than 3 y-layers to show structure above and below plane")
