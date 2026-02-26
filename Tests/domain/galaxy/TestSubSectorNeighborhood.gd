## Tests for SubSectorNeighborhood — 11x11x11 grid generation, shells, and boundary handling.
class_name TestSubSectorNeighborhood
extends TestCase


var _spec: GalaxySpec
var _model: DensityModelInterface
var _ref_density: float


func before_each() -> void:
	_spec = GalaxySpec.create_milky_way(42)
	_model = DensityModelInterface.create_for_spec(_spec)
	_ref_density = _model.get_density(Vector3(8000.0, 0.0, 0.0))


func test_produces_343_subsector_origins() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, _ref_density
	)
	assert_equal(data.subsector_origins.size(), 1331,
		"Should have exactly 1331 subsector origins for 11x11x11 grid")


func test_produces_343_shell_tags() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, _ref_density
	)
	assert_equal(data.subsector_shells.size(), 1331,
		"Should have 1331 shell tags matching origins")


func test_center_origin_matches_camera_subsector() -> void:
	var camera_pos: Vector3 = Vector3(505.0, 505.0, 505.0)
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		camera_pos, 42, _model, _ref_density
	)
	var expected_origin: Vector3 = GalaxyCoordinates.get_subsector_world_origin(camera_pos)
	assert_true(data.center_origin.is_equal_approx(expected_origin),
		"Center origin should match camera's subsector origin")


func test_shell_0_is_center_only() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_0_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 0:
			shell_0_count += 1
	assert_equal(shell_0_count, 1, "Only 1 subsector should be in shell 0 (center)")


func test_shell_1_has_26_subsectors() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_1_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 1:
			shell_1_count += 1
	# 3^3 - 1^3 = 26
	assert_equal(shell_1_count, 26, "Shell 1 should have 26 subsectors")


func test_shell_2_has_98_subsectors() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_2_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 2:
			shell_2_count += 1
	# 5^3 - 3^3 = 125 - 27 = 98
	assert_equal(shell_2_count, 98, "Shell 2 should have 98 subsectors")


func test_shell_3_has_218_subsectors() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_3_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 3:
			shell_3_count += 1
	# 7^3 - 5^3 = 343 - 125 = 218
	assert_equal(shell_3_count, 218, "Shell 3 should have 218 subsectors")


func test_shell_4_has_386_subsectors() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_4_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 4:
			shell_4_count += 1
	# 9^3 - 7^3 = 729 - 343 = 386
	assert_equal(shell_4_count, 386, "Shell 4 should have 386 subsectors")


func test_shell_5_has_602_subsectors() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(505.0, 505.0, 505.0), 42, _model, _ref_density
	)
	var shell_5_count: int = 0
	for i in range(data.subsector_shells.size()):
		if data.subsector_shells[i] == 5:
			shell_5_count += 1
	# 11^3 - 9^3 = 1331 - 729 = 602
	assert_equal(shell_5_count, 602, "Shell 5 should have 602 subsectors")


func test_star_shells_match_count() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, _ref_density
	)
	assert_equal(data.star_shells.size(), data.star_positions.size(),
		"star_shells must have same count as star_positions")
	assert_equal(data.star_shells.size(), data.star_seeds.size(),
		"star_shells must have same count as star_seeds")


func test_star_shells_are_valid_range() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, _ref_density
	)
	for i in range(data.star_shells.size()):
		assert_in_range(data.star_shells[i], 0, 5,
			"Star shell %d must be in range [0, 5]" % i)


func test_deterministic() -> void:
	var pos: Vector3 = Vector3(300.0, 50.0, 300.0)
	var data_a: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		pos, 42, _model, _ref_density
	)
	var data_b: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		pos, 42, _model, _ref_density
	)

	assert_equal(data_a.get_star_count(), data_b.get_star_count(),
		"Same inputs must produce same star count")

	for i in range(mini(10, data_a.get_star_count())):
		assert_true(
			data_a.star_positions[i].is_equal_approx(data_b.star_positions[i]),
			"Star position %d must be identical" % i
		)
		assert_equal(data_a.star_shells[i], data_b.star_shells[i],
			"Star shell %d must be identical" % i)


func test_different_position_different_neighborhood() -> void:
	var data_a: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, _ref_density
	)
	var data_b: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(5000.0, 500.0, 5000.0), 42, _model, _ref_density
	)

	assert_false(
		data_a.center_origin.is_equal_approx(data_b.center_origin),
		"Different positions should have different centers"
	)


func test_origins_form_11x11x11_grid() -> void:
	var camera_pos: Vector3 = Vector3(505.0, 505.0, 505.0)
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		camera_pos, 42, _model, _ref_density
	)

	var ss_size: float = GalaxyCoordinates.SUBSECTOR_SIZE_PC
	var center: Vector3 = data.center_origin

	var expected_count: int = 0
	for dx in range(-5, 6):
		for dy in range(-5, 6):
			for dz in range(-5, 6):
				var expected: Vector3 = center + Vector3(
					float(dx) * ss_size,
					float(dy) * ss_size,
					float(dz) * ss_size
				)
				var found: bool = false
				for origin in data.subsector_origins:
					if origin.is_equal_approx(expected):
						found = true
						break
				if found:
					expected_count += 1

	assert_equal(expected_count, 1331, "All 1331 grid positions should be present")


func test_handles_sector_boundary_crossing() -> void:
	var camera_pos: Vector3 = Vector3(95.0, 5.0, 5.0)
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		camera_pos, 42, _model, _ref_density
	)

	var has_sector_0: bool = false
	var has_sector_1: bool = false
	for origin in data.subsector_origins:
		var ss_center: Vector3 = origin + Vector3.ONE * GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5
		var hierarchy: GalaxyCoordinates.HierarchyCoords = (
			GalaxyCoordinates.parsec_to_hierarchy(ss_center)
		)
		if hierarchy.sector_local_coords.x == 0:
			has_sector_0 = true
		elif hierarchy.sector_local_coords.x == 1:
			has_sector_1 = true

	assert_true(has_sector_0, "Should have subsectors in sector 0")
	assert_true(has_sector_1, "Should have subsectors crossing into sector 1")


func test_handles_quadrant_boundary_crossing() -> void:
	var camera_pos: Vector3 = Vector3(995.0, 5.0, 5.0)
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		camera_pos, 42, _model, _ref_density
	)

	var has_quadrant_0: bool = false
	var has_quadrant_1: bool = false
	for origin in data.subsector_origins:
		var ss_center: Vector3 = origin + Vector3.ONE * GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5
		var hierarchy: GalaxyCoordinates.HierarchyCoords = (
			GalaxyCoordinates.parsec_to_hierarchy(ss_center)
		)
		if hierarchy.quadrant_coords.x == 0:
			has_quadrant_0 = true
		elif hierarchy.quadrant_coords.x == 1:
			has_quadrant_1 = true

	assert_true(has_quadrant_0, "Should have subsectors in quadrant 0")
	assert_true(has_quadrant_1, "Should have subsectors crossing into quadrant 1")


func test_get_center_origin_matches_build() -> void:
	var camera_pos: Vector3 = Vector3(505.0, 55.0, 305.0)
	var center: Vector3 = SubSectorNeighborhood.get_center_origin(camera_pos)
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		camera_pos, 42, _model, _ref_density
	)
	assert_true(center.is_equal_approx(data.center_origin),
		"get_center_origin should match build center")


func test_produces_stars_near_galactic_center() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(5.0, 5.0, 5.0), 42, _model, _ref_density
	)
	assert_greater_than(data.get_star_count(), 0,
		"Should produce stars near galactic center")


func test_zero_reference_density_produces_no_stars() -> void:
	var data: SubSectorNeighborhood.NeighborhoodData = SubSectorNeighborhood.build(
		Vector3(500.0, 500.0, 500.0), 42, _model, 0.0
	)
	assert_equal(data.get_star_count(), 0,
		"Zero reference density should produce no stars")
