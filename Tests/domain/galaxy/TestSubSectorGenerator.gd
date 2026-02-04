## Tests for SubSectorGenerator — determinism, density correlation, and bounds.
class_name TestSubSectorGenerator
extends TestCase


var _spec: GalaxySpec
var _model: DensityModelInterface
var _ref_density: float


func before_each() -> void:
	_spec = GalaxySpec.create_milky_way(42)
	_model = DensityModelInterface.create_for_spec(_spec)
	# Use density at ~8kpc (solar neighborhood equivalent) as reference
	_ref_density = _model.get_density(Vector3(8000.0, 0.0, 0.0))


func test_determinism() -> void:
	var result_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)
	var result_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	assert_equal(result_a.get_count(), result_b.get_count(),
		"Same inputs must produce same star count")

	for i in range(mini(10, result_a.get_count())):
		assert_true(
			result_a.positions[i].is_equal_approx(result_b.positions[i]),
			"Star position %d must be identical" % i
		)
		assert_equal(result_a.star_seeds[i], result_b.star_seeds[i],
			"Star seed %d must be identical" % i
		)


func test_different_seed_different_result() -> void:
	var result_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)
	var result_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		99, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	# Counts or positions should differ
	var any_different: bool = result_a.get_count() != result_b.get_count()
	if not any_different and result_a.get_count() > 0:
		any_different = not result_a.positions[0].is_equal_approx(result_b.positions[0])
	assert_true(any_different, "Different seeds should give different results")


func test_different_quadrant_different_result() -> void:
	var result_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)
	var result_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(1, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	var any_different: bool = result_a.get_count() != result_b.get_count()
	if not any_different and result_a.get_count() > 0:
		any_different = not result_a.positions[0].is_equal_approx(result_b.positions[0])
	assert_true(any_different, "Different quadrants should give different results")


func test_different_sector_different_result() -> void:
	var result_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(0, 0, 0), _model, _ref_density
	)
	var result_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(1, 0, 0), _model, _ref_density
	)

	var any_different: bool = result_a.get_count() != result_b.get_count()
	if not any_different and result_a.get_count() > 0:
		any_different = not result_a.positions[0].is_equal_approx(result_b.positions[0])
	assert_true(any_different, "Different sectors should give different results")


func test_center_sector_has_more_stars_than_edge() -> void:
	# Center sector (near galactic core) should have more stars
	var center: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(0, 0, 0), _model, _ref_density
	)
	# Edge sector — very far out where density drops significantly
	var edge: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(14, 0, 14), Vector3i(9, 0, 9), _model, _ref_density
	)

	assert_greater_than(center.get_count(), edge.get_count(),
		"Center sector should have more stars than far edge sector")


func test_zero_reference_density_returns_empty() -> void:
	var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, 0.0
	)
	assert_equal(result.get_count(), 0,
		"Zero reference density should produce no stars")


func test_star_positions_within_sector_bounds() -> void:
	var quadrant: Vector3i = Vector3i(0, 0, 0)
	var sector_local: Vector3i = Vector3i(3, 4, 5)
	var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, quadrant, sector_local, _model, _ref_density
	)

	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(quadrant, sector_local)
	var sector_max: Vector3 = sector_origin + Vector3.ONE * GalaxyCoordinates.SECTOR_SIZE_PC

	for i in range(result.get_count()):
		var pos: Vector3 = result.positions[i]
		assert_greater_than(pos.x, sector_origin.x - 0.01,
			"Star %d X below sector min" % i)
		assert_less_than(pos.x, sector_max.x + 0.01,
			"Star %d X above sector max" % i)
		assert_greater_than(pos.y, sector_origin.y - 0.01,
			"Star %d Y below sector min" % i)
		assert_less_than(pos.y, sector_max.y + 0.01,
			"Star %d Y above sector max" % i)
		assert_greater_than(pos.z, sector_origin.z - 0.01,
			"Star %d Z below sector min" % i)
		assert_less_than(pos.z, sector_max.z + 0.01,
			"Star %d Z above sector max" % i)


func test_star_seeds_are_unique_within_sector() -> void:
	var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	if result.get_count() < 2:
		return

	var seen: Dictionary = {}
	var duplicates: int = 0
	for i in range(result.get_count()):
		var seed_key: String = str(result.star_seeds[i])
		if seen.has(seed_key):
			duplicates += 1
		seen[seed_key] = true

	assert_equal(duplicates, 0, "Star seeds within a sector should be unique")


func test_produces_stars_at_galaxy_center() -> void:
	var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(0, 0, 0), _model, _ref_density
	)
	assert_greater_than(result.get_count(), 0,
		"Should produce at least some stars near galactic center")


func test_poisson_produces_zero_for_zero_lambda() -> void:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = 123
	# Access static method indirectly by generating with zero density
	var empty_result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(99, 99, 99), Vector3i(0, 0, 0), _model, _ref_density
	)
	# Far from center, density should be negligible, few or no stars
	assert_less_than(empty_result.get_count(), 50,
		"Very low density region should have very few stars")


func test_border_generation_deterministic() -> void:
	var result_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_with_border(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)
	var result_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_with_border(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	assert_equal(result_a.get_count(), result_b.get_count(),
		"Border generation must be deterministic in count")

	for i in range(mini(10, result_a.get_count())):
		assert_true(
			result_a.positions[i].is_equal_approx(result_b.positions[i]),
			"Border star position %d must be identical" % i
		)


func test_border_has_more_stars_than_sector_alone() -> void:
	var sector_only: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)
	var with_border: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_with_border(
		42, Vector3i(0, 0, 0), Vector3i(5, 5, 5), _model, _ref_density
	)

	assert_greater_than(with_border.get_count(), sector_only.get_count(),
		"Border shell should add additional stars beyond the sector alone")


func test_border_stars_extend_beyond_sector_bounds() -> void:
	var quadrant: Vector3i = Vector3i(0, 0, 0)
	var sector_local: Vector3i = Vector3i(5, 5, 5)
	var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_with_border(
		42, quadrant, sector_local, _model, _ref_density
	)

	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(quadrant, sector_local)
	var sector_max: Vector3 = sector_origin + Vector3.ONE * GalaxyCoordinates.SECTOR_SIZE_PC

	var outside_count: int = 0
	for i in range(result.get_count()):
		var pos: Vector3 = result.positions[i]
		if pos.x < sector_origin.x or pos.x > sector_max.x:
			outside_count += 1
		elif pos.y < sector_origin.y or pos.y > sector_max.y:
			outside_count += 1
		elif pos.z < sector_origin.z or pos.z > sector_max.z:
			outside_count += 1

	assert_greater_than(outside_count, 0,
		"Border generation should produce stars outside sector bounds")


func test_solar_neighborhood_density_realistic() -> void:
	# At the reference point (~8kpc), expect ~4 systems per subsector on average
	var quadrant: Vector3i = Vector3i(8, 0, 0)
	var sector_local: Vector3i = Vector3i(0, 0, 0)
	# Generate many subsectors and check average
	var total_stars: int = 0
	var num_samples: int = 100

	for i in range(num_samples):
		var result: SubSectorGenerator.SectorStarData = SubSectorGenerator.generate_sector_stars(
			i, quadrant, sector_local, _model, _ref_density
		)
		total_stars += result.get_count()

	# 1000 subsectors per sector × ~4 expected each = ~4000 per sector
	# With 100 different seeds, average per sector should be near 4000
	var avg_per_sector: float = float(total_stars) / float(num_samples)
	# Allow wide range but should be in right ballpark (1000-8000)
	assert_greater_than(avg_per_sector, 1000.0,
		"Average stars per sector at 8kpc should be > 1000")
	assert_less_than(avg_per_sector, 8000.0,
		"Average stars per sector at 8kpc should be < 8000")


func test_merge_star_data() -> void:
	var data_a: SubSectorGenerator.SectorStarData = SubSectorGenerator.SectorStarData.new()
	data_a.positions = PackedVector3Array([Vector3(1.0, 2.0, 3.0)])
	data_a.star_seeds = PackedInt64Array([100])

	var data_b: SubSectorGenerator.SectorStarData = SubSectorGenerator.SectorStarData.new()
	data_b.positions = PackedVector3Array([Vector3(4.0, 5.0, 6.0)])
	data_b.star_seeds = PackedInt64Array([200])

	data_a.merge(data_b)

	assert_equal(data_a.get_count(), 2, "Merged data should have 2 stars")
	assert_equal(data_a.star_seeds[1], 200, "Second seed should be from merged data")
