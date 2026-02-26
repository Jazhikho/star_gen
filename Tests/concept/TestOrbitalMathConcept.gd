## Tests for the orbital mechanics utility class.
## Covers Kepler equation solving, anomaly conversions, and position computation.
class_name TestOrbitalMathConcept
extends TestCase


## For a circular orbit (e=0), true anomaly equals mean anomaly.
func test_kepler_circular_orbit_identity() -> void:
	var test_angles: Array[float] = [0.0, PI / 4.0, PI / 2.0, PI, 3.0 * PI / 2.0, TAU - 0.01]
	for m in test_angles:
		var nu: float = OrbitalMathConcept.mean_anomaly_to_true_anomaly(m, 0.0)
		assert_float_equal(nu, m, 1.0e-8,
			"e=0: true anomaly should equal mean anomaly for M=%.4f" % m)
		if has_failed():
			return


## Verifies Kepler solver converges for moderate eccentricity (e=0.5, M=1.0).
## Expected: E ≈ 1.4987, ν ≈ 2.028 rad (computed from standard tables).
func test_kepler_moderate_eccentricity() -> void:
	var e: float = 0.5
	var m: float = 1.0
	var eccentric: float = OrbitalMathConcept.solve_kepler_equation(m, e)

	# Verify Kepler equation: M = E - e*sin(E)
	var residual: float = absf(eccentric - e * sin(eccentric) - m)
	assert_less_than(residual, 1.0e-8, "Kepler residual should be near zero")

	var nu: float = OrbitalMathConcept.mean_anomaly_to_true_anomaly(m, e)
	assert_float_equal(nu, 2.028, 0.01,
		"True anomaly for e=0.5 M=1.0 should be ≈2.028 rad")


## Verifies Kepler solver works for very small eccentricity.
func test_kepler_small_eccentricity() -> void:
	var e: float = 0.01
	var m: float = PI / 2.0
	var nu: float = OrbitalMathConcept.mean_anomaly_to_true_anomaly(m, e)
	# For tiny e, ν ≈ M
	assert_float_equal(nu, m, 0.05,
		"Very small eccentricity: true anomaly ≈ mean anomaly")


## Verifies Kepler solver residual for a range of eccentricities.
func test_kepler_solver_residual_sweep() -> void:
	var eccentricities: Array[float] = [0.05, 0.1, 0.2, 0.4, 0.6, 0.8, 0.9]
	var mean_anomalies: Array[float] = [0.5, 1.0, 2.0, 3.0, 5.0]

	for e in eccentricities:
		for m in mean_anomalies:
			var ea: float = OrbitalMathConcept.solve_kepler_equation(m, e)
			var residual: float = absf(ea - e * sin(ea) - m)
			assert_less_than(residual, 1.0e-8,
				"Kepler residual for e=%.2f M=%.2f" % [e, m])
			if has_failed():
				return


## With e=0, i=0 the position should lie in the XZ plane at distance = a.
func test_position_circular_flat() -> void:
	var a: float = 3.0
	var test_angles: Array[float] = [0.0, PI / 3.0, PI, 5.0 * PI / 3.0]

	for nu in test_angles:
		var pos: Vector3 = OrbitalMathConcept.orbital_elements_to_position(
			a, 0.0, 0.0, 0.0, 0.0, nu
		)
		var horiz_dist: float = sqrt(pos.x * pos.x + pos.z * pos.z)
		assert_float_equal(horiz_dist, a, 0.001,
			"Circular orbit at ν=%.2f should have r=a" % nu)
		assert_float_equal(pos.y, 0.0, 0.001,
			"Zero inclination gives y=0 at ν=%.2f" % nu)
		if has_failed():
			return


## Nonzero inclination should produce nonzero Y component.
func test_position_inclined_orbit() -> void:
	var a: float = 2.5
	var incl: float = deg_to_rad(30.0)
	# At ω+ν = π/2, sin(ω+ν) = 1 → maximum Y displacement
	var pos: Vector3 = OrbitalMathConcept.orbital_elements_to_position(
		a, 0.0, incl, 0.0, 0.0, PI / 2.0
	)
	# Y = r * sin(i) * sin(ω+ν) = a * sin(30°) * sin(π/2) = a * 0.5
	assert_float_equal(pos.y, a * 0.5, 0.01,
		"Y displacement matches a*sin(i) at peak")


## Eccentric orbit should have correct periapsis/apoapsis distances.
func test_position_eccentric_periapsis_apoapsis() -> void:
	var a: float = 3.0
	var e: float = 0.3
	# At ν=0 (periapsis): r = a*(1-e) = 2.1
	var pos_peri: Vector3 = OrbitalMathConcept.orbital_elements_to_position(
		a, e, 0.0, 0.0, 0.0, 0.0
	)
	var r_peri: float = pos_peri.length()
	assert_float_equal(r_peri, a * (1.0 - e), 0.01, "Periapsis distance = a*(1-e)")

	# At ν=π (apoapsis): r = a*(1+e) = 3.9
	var pos_apo: Vector3 = OrbitalMathConcept.orbital_elements_to_position(
		a, e, 0.0, 0.0, 0.0, PI
	)
	var r_apo: float = pos_apo.length()
	assert_float_equal(r_apo, a * (1.0 + e), 0.01, "Apoapsis distance = a*(1+e)")
