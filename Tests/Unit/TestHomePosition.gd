## Unit tests for HomePosition utilities.
extends TestCase


func get_test_name() -> String:
	return "TestHomePosition"


func test_default_position_distance_from_center() -> void:
	var pos: Vector3 = HomePosition.get_default_position()
	var radial_distance: float = sqrt(pos.x * pos.x + pos.z * pos.z)

	# Should be approximately 8000 parsecs
	assert_almost_equal(radial_distance, HomePosition.SOLAR_DISTANCE_PC, 1.0,
		"Home position should be ~8000 pc from center")


func test_default_position_height() -> void:
	var pos: Vector3 = HomePosition.get_default_position()

	# Should be approximately 20 parsecs above plane
	assert_almost_equal(pos.y, HomePosition.SOLAR_HEIGHT_PC, 0.1,
		"Home position should be ~20 pc above disk plane")


func test_home_quadrant_is_valid() -> void:
	var quadrant: Vector3i = HomePosition.get_home_quadrant()

	# At 8000 pc distance, quadrant coords should be reasonable
	# 8000 / 1000 = 8, so X should be around 7 or 8
	assert_greater_than(quadrant.x, 5, "Quadrant X should be positive and significant")
	assert_less_than(quadrant.x, 10, "Quadrant X should be less than 10")


func test_home_hierarchy_is_consistent() -> void:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = HomePosition.get_home_hierarchy()
	var quadrant: Vector3i = HomePosition.get_home_quadrant()

	assert_equal(hierarchy.quadrant_coords, quadrant,
		"Hierarchy quadrant should match direct quadrant calculation")


func test_home_sector_coords_in_range() -> void:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = HomePosition.get_home_hierarchy()

	# Sector local coords should be 0-9
	assert_greater_than(hierarchy.sector_local_coords.x, -1, "Sector X >= 0")
	assert_less_than(hierarchy.sector_local_coords.x, 10, "Sector X < 10")
	assert_greater_than(hierarchy.sector_local_coords.y, -1, "Sector Y >= 0")
	assert_less_than(hierarchy.sector_local_coords.y, 10, "Sector Y < 10")
	assert_greater_than(hierarchy.sector_local_coords.z, -1, "Sector Z >= 0")
	assert_less_than(hierarchy.sector_local_coords.z, 10, "Sector Z < 10")


func test_home_subsector_coords_in_range() -> void:
	var hierarchy: GalaxyCoordinates.HierarchyCoords = HomePosition.get_home_hierarchy()

	# Subsector local coords should be 0-9
	assert_greater_than(hierarchy.subsector_local_coords.x, -1, "Subsector X >= 0")
	assert_less_than(hierarchy.subsector_local_coords.x, 10, "Subsector X < 10")
	assert_greater_than(hierarchy.subsector_local_coords.y, -1, "Subsector Y >= 0")
	assert_less_than(hierarchy.subsector_local_coords.y, 10, "Subsector Y < 10")
	assert_greater_than(hierarchy.subsector_local_coords.z, -1, "Subsector Z >= 0")
	assert_less_than(hierarchy.subsector_local_coords.z, 10, "Subsector Z < 10")


func test_home_sector_origin_is_within_galaxy() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var origin: Vector3 = HomePosition.get_home_sector_origin()

	assert_true(HomePosition.is_within_galaxy(origin, spec),
		"Home sector origin should be within galaxy bounds")


func test_home_sector_center_is_near_default_position() -> void:
	var center: Vector3 = HomePosition.get_home_sector_center()
	var default_pos: Vector3 = HomePosition.get_default_position()

	# Center should be within one sector size of the default position
	var distance: float = center.distance_to(default_pos)
	var max_distance: float = GalaxyCoordinates.SECTOR_SIZE_PC * 1.5

	assert_less_than(distance, max_distance,
		"Sector center should be near default position")


func test_home_subsector_center_is_near_default_position() -> void:
	var center: Vector3 = HomePosition.get_home_subsector_center()
	var default_pos: Vector3 = HomePosition.get_default_position()

	# Center should be within one subsector diagonal of the default position
	var distance: float = center.distance_to(default_pos)
	var max_distance: float = GalaxyCoordinates.SUBSECTOR_SIZE_PC * 2.0

	assert_less_than(distance, max_distance,
		"Subsector center should be near default position")


func test_is_within_galaxy_accepts_valid_position() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var valid_pos: Vector3 = Vector3(5000.0, 100.0, 3000.0)

	assert_true(HomePosition.is_within_galaxy(valid_pos, spec),
		"Position within bounds should be valid")


func test_is_within_galaxy_rejects_too_far() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var far_pos: Vector3 = Vector3(20000.0, 0.0, 0.0)  # Beyond 15000 pc radius

	assert_false(HomePosition.is_within_galaxy(far_pos, spec),
		"Position beyond radius should be invalid")


func test_is_within_galaxy_rejects_too_high() -> void:
	var spec: GalaxySpec = GalaxySpec.create_milky_way(42)
	var high_pos: Vector3 = Vector3(5000.0, 2000.0, 0.0)  # Beyond 1000 pc height

	assert_false(HomePosition.is_within_galaxy(high_pos, spec),
		"Position beyond height should be invalid")


func test_default_position_is_deterministic() -> void:
	var pos1: Vector3 = HomePosition.get_default_position()
	var pos2: Vector3 = HomePosition.get_default_position()

	assert_true(pos1.is_equal_approx(pos2),
		"Default position should be deterministic")
