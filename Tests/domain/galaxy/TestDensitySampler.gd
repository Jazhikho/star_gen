## Tests for DensitySampler — determinism, distribution, and population split.
class_name TestDensitySampler
extends TestCase


var _spec: GalaxySpec
var _model: SpiralDensityModel


func before_each() -> void:
	_spec = GalaxySpec.create_milky_way(123)
	_model = SpiralDensityModel.new(_spec)


func test_determinism() -> void:
	var rng1: RandomNumberGenerator = RandomNumberGenerator.new()
	rng1.seed = 999
	var sample1: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 500, rng1)

	var rng2: RandomNumberGenerator = RandomNumberGenerator.new()
	rng2.seed = 999
	var sample2: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 500, rng2)

	assert_equal(
		sample1.bulge_points.size(), sample2.bulge_points.size(),
		"Bulge counts must match across runs"
	)
	assert_equal(
		sample1.disk_points.size(), sample2.disk_points.size(),
		"Disk counts must match across runs"
	)

	# Spot-check a few positions
	for i in range(mini(10, sample1.bulge_points.size())):
		assert_true(
			sample1.bulge_points[i].is_equal_approx(sample2.bulge_points[i]),
			"Bulge point %d must be identical" % i
		)
	for i in range(mini(10, sample1.disk_points.size())):
		assert_true(
			sample1.disk_points[i].is_equal_approx(sample2.disk_points[i]),
			"Disk point %d must be identical" % i
		)


func test_produces_requested_count() -> void:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = 42
	var sample: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 1000, rng)

	# Allow small shortfall from disk rejection cap
	assert_greater_than(sample.get_total_count(), 900,
		"Should produce close to requested count")
	assert_less_than(sample.get_total_count(), 1001,
		"Should not exceed requested count")


func test_has_both_populations() -> void:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = 77
	var sample: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 2000, rng)

	assert_greater_than(sample.bulge_points.size(), 0, "Should have bulge points")
	assert_greater_than(sample.disk_points.size(), 0, "Should have disk points")


func test_bulge_points_near_center() -> void:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = 55
	var sample: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 2000, rng)

	# Most bulge points should be within 3 sigma of the bulge radius
	var max_r: float = _spec.bulge_radius_pc * 4.0
	var outliers: int = 0
	for i in range(sample.bulge_points.size()):
		var p: Vector3 = sample.bulge_points[i]
		var r: float = sqrt(p.x * p.x + p.z * p.z)
		if r > max_r:
			outliers += 1

	var outlier_frac: float = float(outliers) / float(sample.bulge_points.size())
	assert_less_than(outlier_frac, 0.01, "Very few bulge stars should be far from center")


func test_disk_points_within_galaxy_bounds() -> void:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = 66
	var sample: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 2000, rng)

	for i in range(sample.disk_points.size()):
		var p: Vector3 = sample.disk_points[i]
		var r: float = sqrt(p.x * p.x + p.z * p.z)
		assert_less_than(r, _spec.radius_pc + 1.0,
			"Disk point %d radius must be within galaxy radius" % i)
		assert_less_than(absf(p.y), _spec.height_pc + 1.0,
			"Disk point %d height must be within galaxy height" % i)


func test_different_seed_different_result() -> void:
	var rng1: RandomNumberGenerator = RandomNumberGenerator.new()
	rng1.seed = 1
	var sample1: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 500, rng1)

	var rng2: RandomNumberGenerator = RandomNumberGenerator.new()
	rng2.seed = 2
	var sample2: GalaxySample = DensitySampler.sample_galaxy(_spec, _model, 500, rng2)

	# At least one point should differ
	var any_different: bool = false
	var check_count: int = mini(sample1.bulge_points.size(), sample2.bulge_points.size())
	for i in range(mini(10, check_count)):
		if not sample1.bulge_points[i].is_equal_approx(sample2.bulge_points[i]):
			any_different = true
			break
	assert_true(any_different, "Different seeds should produce different galaxies")
