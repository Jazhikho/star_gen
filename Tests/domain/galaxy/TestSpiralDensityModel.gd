## Tests for SpiralDensityModel — density distribution correctness.
class_name TestSpiralDensityModel
extends TestCase


var _spec: GalaxySpec
var _model: SpiralDensityModel


func before_each() -> void:
	_spec = GalaxySpec.create_milky_way(1)
	_model = SpiralDensityModel.new(_spec)


func test_center_has_highest_density() -> void:
	var center_d: float = _model.get_density(Vector3.ZERO)
	var edge_d: float = _model.get_density(Vector3(10000.0, 0.0, 0.0))
	assert_greater_than(center_d, edge_d, "Center should be denser than edge")


func test_density_is_non_negative() -> void:
	var positions: Array[Vector3] = [
		Vector3.ZERO,
		Vector3(5000.0, 0.0, 0.0),
		Vector3(0.0, 500.0, 0.0),
		Vector3(15000.0, 1000.0, 15000.0),
		Vector3(-3000.0, -200.0, 7000.0),
	]
	for pos in positions:
		var d: float = _model.get_density(pos)
		assert_true(d >= 0.0, "Density must be non-negative at %s" % str(pos))


func test_density_falls_with_height() -> void:
	var d_plane: float = _model.get_density(Vector3(3000.0, 0.0, 0.0))
	var d_above: float = _model.get_density(Vector3(3000.0, 800.0, 0.0))
	assert_greater_than(d_plane, d_above, "Density should fall above the disk plane")


func test_density_falls_with_radius() -> void:
	var d_inner: float = _model.get_density(Vector3(2000.0, 0.0, 0.0))
	var d_outer: float = _model.get_density(Vector3(12000.0, 0.0, 0.0))
	assert_greater_than(d_inner, d_outer, "Density should fall with galactic radius")


func test_arm_factor_peaks_on_arm() -> void:
	# At some radius, scan theta to find the arm peak and trough
	var r: float = 5000.0
	var best_factor: float = 0.0
	var worst_factor: float = 999.0

	for step in range(360):
		var theta: float = float(step) * TAU / 360.0
		var x: float = r * cos(theta)
		var z: float = r * sin(theta)
		var af: float = _model.get_arm_factor(r, x, z)
		best_factor = maxf(best_factor, af)
		worst_factor = minf(worst_factor, af)

	assert_float_equal(best_factor, 1.0, 0.01, "Arm peak factor should be ~1.0")

	# The inter-arm minimum sits above the raw base (1 - amplitude) because
	# the Gaussian tails of adjacent arms overlap at the midpoint.
	# Midpoint angular distance from nearest arm = PI / num_arms.
	# Proximity there = exp(-midpoint² / (2 * width²)).
	# Expected minimum factor = base + amplitude * overlap_proximity.
	var midpoint_delta: float = PI / float(_spec.num_arms)
	var overlap_proximity: float = exp(
		-midpoint_delta * midpoint_delta
		/ (2.0 * _spec.arm_width * _spec.arm_width)
	)
	var expected_min: float = (
		(1.0 - _spec.arm_amplitude)
		+ _spec.arm_amplitude * overlap_proximity
	)
	assert_float_equal(
		worst_factor, expected_min, 0.05,
		"Inter-arm factor should match expected overlap minimum"
	)


func test_arm_factor_near_center_is_one() -> void:
	var af: float = _model.get_arm_factor(0.5, 0.5, 0.0)
	assert_float_equal(af, 1.0, 0.001, "Near center, arm factor should be 1.0")


func test_determinism() -> void:
	var d1: float = _model.get_density(Vector3(4000.0, 100.0, 3000.0))
	var d2: float = _model.get_density(Vector3(4000.0, 100.0, 3000.0))
	assert_equal(d1, d2, "Same input must give same density (pure function)")


func test_different_spec_gives_different_density() -> void:
	var spec2: GalaxySpec = GalaxySpec.create_milky_way(1)
	spec2.num_arms = 2
	spec2.arm_pitch_angle_deg = 25.0
	var model2: SpiralDensityModel = SpiralDensityModel.new(spec2)

	var pos: Vector3 = Vector3(5000.0, 0.0, 3000.0)
	var d1: float = _model.get_density(pos)
	var d2: float = model2.get_density(pos)
	assert_not_equal(d1, d2, "Different spec should give different density")
