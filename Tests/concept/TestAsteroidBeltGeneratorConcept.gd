## Tests for the asteroid belt generator concept.
## Covers determinism, bounds, distribution shape, gaps, clustering,
## provenance, and major asteroid integration. Uses SeededRng.
class_name TestAsteroidBeltGeneratorConcept
extends TestCase


var _generator: AsteroidBeltGeneratorConcept
var _default_spec: AsteroidBeltSpecConcept


## Sets up a fresh generator and default spec before each test.
func before_each() -> void:
	_generator = AsteroidBeltGeneratorConcept.new()
	_default_spec = AsteroidBeltSpecConcept.new()
	_default_spec.inner_radius_au = 2.0
	_default_spec.outer_radius_au = 3.5
	_default_spec.asteroid_count = 500
	_default_spec.max_inclination_deg = 20.0
	_default_spec.max_eccentricity = 0.25
	_default_spec.min_body_radius_km = 0.5
	_default_spec.max_body_radius_km = 500.0
	_default_spec.size_power_law_exponent = 2.5
	_default_spec.radial_concentration = 2.0


# ---------- Determinism ----------

## Verifies identical seeds produce byte-identical belt output.
func test_determinism_same_seed() -> void:
	var rng1: SeededRng = SeededRng.new(12345)
	var belt1: AsteroidBeltDataConcept = _generator.generate_belt(_default_spec, rng1)

	var rng2: SeededRng = SeededRng.new(12345)
	var belt2: AsteroidBeltDataConcept = _generator.generate_belt(_default_spec, rng2)

	assert_equal(belt1.asteroids.size(), belt2.asteroids.size(), "Same asteroid count")
	for idx in range(belt1.asteroids.size()):
		var a1: AsteroidDataConcept = belt1.asteroids[idx]
		var a2: AsteroidDataConcept = belt2.asteroids[idx]
		assert_float_equal(a1.position_au.x, a2.position_au.x, 0.0001,
			"Position X at %d" % idx)
		assert_float_equal(a1.position_au.y, a2.position_au.y, 0.0001,
			"Position Y at %d" % idx)
		assert_float_equal(a1.position_au.z, a2.position_au.z, 0.0001,
			"Position Z at %d" % idx)
		assert_float_equal(a1.body_radius_km, a2.body_radius_km, 0.0001,
			"Size at %d" % idx)
		if has_failed():
			return


## Verifies different seeds produce different belt output.
func test_determinism_different_seeds() -> void:
	var belt1: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(12345))
	var belt2: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(67890))

	var diffs: int = 0
	for idx in range(mini(belt1.asteroids.size(), belt2.asteroids.size())):
		if belt1.asteroids[idx].position_au.distance_to(belt2.asteroids[idx].position_au) > 0.01:
			diffs += 1
	assert_greater_than(diffs, 0, "Different seeds produce different belts")


# ---------- Counts ----------

## Verifies the background asteroid count matches spec.
func test_correct_background_count() -> void:
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(42))
	assert_equal(belt.get_background_count(), _default_spec.asteroid_count,
		"Background count matches spec.asteroid_count")


## Verifies total count = background + major.
func test_total_equals_background_plus_major() -> void:
	_default_spec.asteroid_count = 200
	var input: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
	input.body_id = "test_major"
	input.semi_major_axis_m = 2.75 * OrbitalMathConcept.AU_METERS
	input.body_radius_km = 400.0
	_default_spec.major_asteroid_inputs = [input]

	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(42))

	assert_equal(belt.asteroids.size(), 201, "Total = 200 background + 1 major")
	assert_equal(belt.get_background_count(), 200, "Background count")
	assert_equal(belt.get_major_count(), 1, "Major count")


# ---------- Bounds ----------

## Verifies background semi-major axes are within [inner_radius, outer_radius].
func test_semi_major_axes_within_radial_bounds() -> void:
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(99))
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		assert_true(a.semi_major_axis_au >= _default_spec.inner_radius_au - 0.001,
			"Asteroid %d SMA >= inner" % idx)
		assert_true(a.semi_major_axis_au <= _default_spec.outer_radius_au + 0.001,
			"Asteroid %d SMA <= outer" % idx)
		if has_failed():
			return


## Verifies all eccentricities are in [0, max_eccentricity].
func test_eccentricities_within_bounds() -> void:
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(77))
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		assert_true(a.eccentricity >= 0.0, "ecc >= 0 at %d" % idx)
		assert_true(a.eccentricity <= _default_spec.max_eccentricity + 0.001,
			"ecc <= max at %d" % idx)
		if has_failed():
			return


## Verifies all background inclinations are within [0, max].
func test_inclinations_within_bounds() -> void:
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(55))
	var max_incl_rad: float = deg_to_rad(_default_spec.max_inclination_deg)
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		assert_true(a.inclination_rad >= 0.0, "incl >= 0 at %d" % idx)
		assert_true(a.inclination_rad <= max_incl_rad + 0.001,
			"incl <= max at %d" % idx)
		if has_failed():
			return


## Verifies all background body sizes are within spec bounds.
func test_body_sizes_within_bounds() -> void:
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(33))
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		assert_true(a.body_radius_km >= _default_spec.min_body_radius_km - 0.01,
			"radius >= min at %d" % idx)
		assert_true(a.body_radius_km <= _default_spec.max_body_radius_km + 0.01,
			"radius <= max at %d" % idx)
		if has_failed():
			return


# ---------- Distribution shape ----------

## Power law produces a majority of small asteroids.
func test_size_distribution_favors_small() -> void:
	_default_spec.asteroid_count = 1000
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(111))
	var median: float = (_default_spec.min_body_radius_km + _default_spec.max_body_radius_km) * 0.5
	var small_count: int = 0
	for a in belt.get_background_asteroids():
		if a.body_radius_km < median:
			small_count += 1
	var frac: float = float(small_count) / float(belt.get_background_count())
	assert_greater_than(frac, 0.8,
		"Power law mostly small (got %.1f%%)" % [frac * 100.0])


## Radial distribution concentrated toward belt center.
func test_radial_distribution_concentrated_in_center() -> void:
	_default_spec.asteroid_count = 1000
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(222))
	var center: float = (_default_spec.inner_radius_au + _default_spec.outer_radius_au) * 0.5
	var qw: float = (_default_spec.outer_radius_au - _default_spec.inner_radius_au) * 0.25
	var in_center: int = 0
	for a in belt.get_background_asteroids():
		if absf(a.semi_major_axis_au - center) < qw:
			in_center += 1
	var frac: float = float(in_center) / float(belt.get_background_count())
	assert_greater_than(frac, 0.50,
		"Center-half holds >50%% (got %.1f%%)" % [frac * 100.0])


## Eccentricities biased toward low values.
func test_eccentricity_biased_low() -> void:
	_default_spec.asteroid_count = 1000
	_default_spec.max_eccentricity = 0.3
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(888))
	var low_count: int = 0
	var threshold: float = _default_spec.max_eccentricity * 0.25
	for a in belt.get_background_asteroids():
		if a.eccentricity < threshold:
			low_count += 1
	var frac: float = float(low_count) / float(belt.get_background_count())
	assert_greater_than(frac, 0.35,
		"Eccentricity biased low (got %.1f%%)" % [frac * 100.0])


# ---------- Gaps ----------

## No asteroids placed inside defined gaps.
func test_gaps_respected() -> void:
	_default_spec.asteroid_count = 500
	_default_spec.gap_centers_au = [2.5, 3.0]
	_default_spec.gap_half_widths_au = [0.1, 0.08]
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(333))
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		assert_false(absf(a.semi_major_axis_au - 2.5) < 0.1,
			"Not in gap 1 at %d (SMA=%.4f)" % [idx, a.semi_major_axis_au])
		assert_false(absf(a.semi_major_axis_au - 3.0) < 0.08,
			"Not in gap 2 at %d (SMA=%.4f)" % [idx, a.semi_major_axis_au])
		if has_failed():
			return


# ---------- Clustering ----------

## Clustering increases angular density near cluster center.
func test_clustering_affects_angular_distribution() -> void:
	_default_spec.asteroid_count = 1000
	_default_spec.cluster_count = 1
	_default_spec.cluster_longitudes_rad = [PI]
	_default_spec.cluster_concentration = 5.0
	_default_spec.cluster_fraction = 0.5
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(444))
	var near_cluster: int = 0
	for a in belt.get_background_asteroids():
		var lon: float = fmod(a.true_anomaly_rad + a.argument_periapsis_rad, TAU)
		var diff: float = absf(lon - PI)
		if diff > PI:
			diff = TAU - diff
		if diff < PI / 4.0:
			near_cluster += 1
	var frac: float = float(near_cluster) / float(belt.get_background_count())
	assert_greater_than(frac, 0.15,
		"Clustering visible (got %.1f%%)" % [frac * 100.0])


# ---------- 3D structure ----------

## Belt has vertical structure from inclination.
func test_positions_are_3d_not_flat() -> void:
	_default_spec.asteroid_count = 200
	_default_spec.max_inclination_deg = 15.0
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(666))
	var nonzero_y: int = 0
	for a in belt.get_background_asteroids():
		if absf(a.position_au.y) > 0.001:
			nonzero_y += 1
	var frac: float = float(nonzero_y) / float(belt.get_background_count())
	assert_greater_than(frac, 0.5,
		"Vertical structure present (got %.1f%%)" % [frac * 100.0])


## Zero eccentricity + zero inclination gives flat circular positions.
func test_zero_eccentricity_circular_flat() -> void:
	_default_spec.asteroid_count = 100
	_default_spec.max_eccentricity = 0.0
	_default_spec.max_inclination_deg = 0.0
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(777))
	for idx in range(belt.asteroids.size()):
		var a: AsteroidDataConcept = belt.asteroids[idx]
		if a.is_major:
			continue
		var hd: float = sqrt(a.position_au.x * a.position_au.x + a.position_au.z * a.position_au.z)
		assert_float_equal(hd, a.semi_major_axis_au, 0.001,
			"Circular radius at %d" % idx)
		assert_float_equal(a.position_au.y, 0.0, 0.001,
			"Flat at %d" % idx)
		if has_failed():
			return


# ---------- Provenance ----------

## Belt stores provenance fields correctly.
func test_belt_stores_provenance() -> void:
	_default_spec.asteroid_count = 10
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(555))
	assert_not_null(belt.spec, "Spec reference stored")
	assert_equal(belt.generation_seed, 555, "Seed stored")
	assert_equal(belt.generator_version, "concept-1.1", "Version stored")


# ---------- Major asteroid integration ----------

## Major asteroids appear in output with correct flags.
func test_major_asteroids_flagged_correctly() -> void:
	_default_spec.asteroid_count = 50
	var input1: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
	input1.body_id = "ceres"
	input1.semi_major_axis_m = 2.77 * OrbitalMathConcept.AU_METERS
	input1.body_radius_km = 470.0
	input1.asteroid_type = 0 # C_TYPE

	var input2: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
	input2.body_id = "vesta"
	input2.semi_major_axis_m = 2.36 * OrbitalMathConcept.AU_METERS
	input2.body_radius_km = 263.0
	input2.asteroid_type = 1 # S_TYPE

	_default_spec.major_asteroid_inputs = [input1, input2]

	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(900))

	assert_equal(belt.get_major_count(), 2, "Two major asteroids")
	assert_equal(belt.get_background_count(), 50, "Background count unchanged")

	var majors: Array[AsteroidDataConcept] = belt.get_major_asteroids()
	assert_equal(majors[0].body_id, "ceres", "First major is ceres")
	assert_true(majors[0].is_major, "Ceres flagged major")
	assert_equal(majors[0].asteroid_type, 0, "Ceres type preserved")
	assert_float_equal(majors[0].body_radius_km, 470.0, 0.1, "Ceres radius preserved")

	assert_equal(majors[1].body_id, "vesta", "Second major is vesta")
	assert_equal(majors[1].asteroid_type, 1, "Vesta type preserved")


## Major asteroid positions are derived from their orbital elements.
func test_major_asteroid_position_from_orbital_elements() -> void:
	_default_spec.asteroid_count = 0

	# Circular orbit, zero inclination, zero angles → position at (a, 0, 0) for M=0
	var input: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
	input.body_id = "test_circular"
	input.semi_major_axis_m = 3.0 * OrbitalMathConcept.AU_METERS
	input.eccentricity = 0.0
	input.inclination_deg = 0.0
	input.longitude_ascending_node_deg = 0.0
	input.argument_periapsis_deg = 0.0
	input.mean_anomaly_deg = 0.0
	input.body_radius_km = 200.0
	_default_spec.major_asteroid_inputs = [input]

	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(901))

	var major: AsteroidDataConcept = belt.get_major_asteroids()[0]
	# Circular, all angles zero → position at (3.0, 0, 0) AU
	assert_float_equal(major.position_au.x, 3.0, 0.01, "X = 3.0 AU")
	assert_float_equal(major.position_au.y, 0.0, 0.01, "Y = 0")
	assert_float_equal(major.position_au.z, 0.0, 0.01, "Z = 0")


## Background asteroids are not flagged as major.
func test_background_asteroids_not_major() -> void:
	_default_spec.asteroid_count = 100
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(
		_default_spec, SeededRng.new(902))
	for a in belt.get_background_asteroids():
		assert_false(a.is_major, "Background asteroid not flagged major")
		assert_equal(a.body_id, "", "Background asteroid has empty body_id")
		if has_failed():
			return
