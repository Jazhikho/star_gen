## Unit tests for SystemScaleManager.
## Tests distance conversion, body sizing, orbital position calculation, and Kepler solving.
extends TestCase

const _system_scale_manager: GDScript = preload("res://src/app/system_viewer/SystemScaleManager.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Helper to create a body with physical properties.
## @param type: Body type.
## @param radius_m: Radius in meters.
## @param mass_kg: Mass in kg.
## @return: CelestialBody.
func _make_body(
	type: CelestialType.Type,
	radius_m: float,
	mass_kg: float = 1.0e24
) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		mass_kg,
		radius_m,
		86400.0,
		0.0,
		0.0,
		0.0,
		0.0
	)
	return CelestialBody.new(
		"test_body",
		"Test Body",
		type,
		physical,
		null
	)


## Helper to create a body with orbital properties.
## @param semi_major_axis_m: Orbit distance.
## @param eccentricity: Orbital eccentricity.
## @param mean_anomaly_deg: Mean anomaly.
## @return: CelestialBody with orbital props.
func _make_orbiting_body(
	semi_major_axis_m: float,
	eccentricity: float = 0.0,
	mean_anomaly_deg: float = 0.0,
	inclination_deg: float = 0.0
) -> CelestialBody:
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0, 0.0, 0.0, 0.0, 0.0
	)
	var orbital: OrbitalProps = OrbitalProps.new(
		semi_major_axis_m,
		eccentricity,
		inclination_deg,
		0.0, 0.0,
		mean_anomaly_deg,
		"star_1"
	)
	var body: CelestialBody = CelestialBody.new(
		"test_orbiter",
		"Test Orbiter",
		CelestialType.Type.PLANET,
		physical,
		null
	)
	body.orbital = orbital
	return body


# =============================================================================
# DISTANCE CONVERSION TESTS
# =============================================================================


## Tests default scale is 1 AU per unit.
func test_default_scale_is_1_au() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	assert_float_equal(manager.distance_scale_m_per_unit, Units.AU_METERS, 1.0,
		"Default scale should be 1 AU per unit")


## Tests distance to units conversion at 1 AU.
func test_distance_to_units_1_au() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var result: float = manager.distance_to_units(Units.AU_METERS)
	assert_float_equal(result, 1.0, 0.001,
		"1 AU should convert to 1.0 viewport units")


## Tests distance to units conversion at 5 AU.
func test_distance_to_units_5_au() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var result: float = manager.distance_to_units(5.0 * Units.AU_METERS)
	assert_float_equal(result, 5.0, 0.001,
		"5 AU should convert to 5.0 viewport units")


## Tests units to distance conversion round trip.
func test_units_to_distance_round_trip() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var original_m: float = 3.5 * Units.AU_METERS
	var units: float = manager.distance_to_units(original_m)
	var back_to_m: float = manager.units_to_distance(units)
	assert_float_equal(back_to_m, original_m, 1.0,
		"Round trip conversion should preserve distance")


## Tests custom scale initialization.
func test_custom_scale() -> void:
	# 1 unit = 0.1 AU (zoomed in)
	var custom_scale: float = Units.AU_METERS * 0.1
	var manager: SystemScaleManager = SystemScaleManager.new(custom_scale)
	var result: float = manager.distance_to_units(Units.AU_METERS)
	assert_float_equal(result, 10.0, 0.001,
		"1 AU at 0.1 AU/unit scale should be 10 units")


## Tests zero distance converts to zero.
func test_distance_zero() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	assert_float_equal(manager.distance_to_units(0.0), 0.0, 0.001,
		"Zero distance should convert to zero units")


## Tests negative scale is clamped to minimum.
func test_negative_scale_clamped() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new(-100.0)
	assert_true(manager.distance_scale_m_per_unit >= 1.0,
		"Negative scale should be clamped to at least 1.0")


# =============================================================================
# BODY DISPLAY RADIUS TESTS
# =============================================================================


## Tests star display radius is within bounds.
func test_star_display_radius_in_bounds() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var star: CelestialBody = _make_body(
		CelestialType.Type.STAR,
		Units.SOLAR_RADIUS_METERS,
		Units.SOLAR_MASS_KG
	)
	var radius: float = manager.get_body_display_radius(star)
	assert_true(radius >= SystemScaleManager.MIN_BODY_DISPLAY_RADIUS,
		"Star display radius should be >= minimum")
	assert_true(radius <= SystemScaleManager.MAX_BODY_DISPLAY_RADIUS,
		"Star display radius should be <= maximum")


## Tests planet display radius is within bounds.
func test_planet_display_radius_in_bounds() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var planet: CelestialBody = _make_body(
		CelestialType.Type.PLANET,
		Units.EARTH_RADIUS_METERS,
		Units.EARTH_MASS_KG
	)
	var radius: float = manager.get_body_display_radius(planet)
	assert_true(radius >= SystemScaleManager.MIN_BODY_DISPLAY_RADIUS,
		"Planet display radius should be >= minimum")
	assert_true(radius <= SystemScaleManager.MAX_BODY_DISPLAY_RADIUS,
		"Planet display radius should be <= maximum")


## Tests null body returns minimum radius.
func test_null_body_returns_minimum() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var radius: float = manager.get_body_display_radius(null)
	assert_float_equal(radius, SystemScaleManager.MIN_BODY_DISPLAY_RADIUS, 0.001,
		"Null body should return minimum display radius")


## Tests that stars are displayed larger than planets of same physical size.
func test_star_larger_than_planet_same_radius() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var test_radius: float = Units.EARTH_RADIUS_METERS * 10.0
	
	var star: CelestialBody = _make_body(CelestialType.Type.STAR, test_radius)
	var planet: CelestialBody = _make_body(CelestialType.Type.PLANET, test_radius)
	
	var star_display: float = manager.get_body_display_radius(star)
	var planet_display: float = manager.get_body_display_radius(planet)
	
	# Star multiplier > planet multiplier, so star should appear larger
	# (unless both are clamped to MAX)
	assert_true(star_display >= planet_display,
		"Star should be displayed >= planet of same physical size")


## Tests that larger physical bodies get larger display (same type).
func test_larger_body_larger_display() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var small_planet: CelestialBody = _make_body(
		CelestialType.Type.PLANET,
		Units.EARTH_RADIUS_METERS
	)
	var large_planet: CelestialBody = _make_body(
		CelestialType.Type.PLANET,
		Units.EARTH_RADIUS_METERS * 10.0
	)
	
	var small_display: float = manager.get_body_display_radius(small_planet)
	var large_display: float = manager.get_body_display_radius(large_planet)
	
	assert_true(large_display >= small_display,
		"Larger body should have >= display radius")


# =============================================================================
# ORBITAL POSITION TESTS
# =============================================================================


## Tests circular orbit at 0 degrees places body on positive X axis.
func test_circular_orbit_zero_anomaly() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, # 1 AU
		0.0, # circular
		0.0, # no inclination
		0.0, # no ascending node rotation
		0.0, # no periapsis rotation
		0.0 # mean anomaly 0
	)
	
	# At mean anomaly 0, circular orbit: body should be at (a, 0, 0) - periapsis
	# For circular orbit with all angles zero, should be at distance 1.0 from origin
	var distance: float = pos.length()
	assert_float_equal(distance, 1.0, 0.01,
		"Circular orbit at 1 AU should place body at distance 1.0 units")


## Tests circular orbit at 180 degrees places body on opposite side.
func test_circular_orbit_180_anomaly() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS,
		0.0,
		0.0, 0.0, 0.0,
		180.0 # Opposite side
	)
	
	var distance: float = pos.length()
	assert_float_equal(distance, 1.0, 0.01,
		"Circular orbit at 180 deg should still be at distance 1.0 units")


## Tests circular orbit at 90 degrees.
func test_circular_orbit_90_anomaly() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS,
		0.0,
		0.0, 0.0, 0.0,
		90.0
	)
	
	var distance: float = pos.length()
	assert_float_equal(distance, 1.0, 0.01,
		"Circular orbit at 90 deg should be at distance 1.0 units")


## Tests eccentric orbit periapsis is closer than apoapsis.
func test_eccentric_orbit_periapsis_vs_apoapsis() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var ecc: float = 0.5
	
	# Mean anomaly 0 = periapsis for eccentric orbit
	var pos_peri: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, ecc, 0.0, 0.0, 0.0, 0.0
	)
	
	# Mean anomaly 180 = apoapsis
	var pos_apo: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, ecc, 0.0, 0.0, 0.0, 180.0
	)
	
	var dist_peri: float = pos_peri.length()
	var dist_apo: float = pos_apo.length()
	
	# Expected: periapsis = a(1-e) = 0.5 AU, apoapsis = a(1+e) = 1.5 AU
	assert_true(dist_peri < dist_apo,
		"Periapsis should be closer than apoapsis (peri=%.3f, apo=%.3f)" % [dist_peri, dist_apo])
	assert_float_equal(dist_peri, 0.5, 0.05,
		"Periapsis distance should be ~0.5 AU units")
	assert_float_equal(dist_apo, 1.5, 0.05,
		"Apoapsis distance should be ~1.5 AU units")


## Tests inclined orbit has Y component.
func test_inclined_orbit_has_y_component() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	
	# 45 degree inclination, at 90 degrees true anomaly should have max Y
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, 0.0,
		45.0, # 45 degree inclination
		0.0, 0.0,
		90.0
	)
	
	assert_true(absf(pos.y) > 0.1,
		"Inclined orbit should have significant Y component (got %.3f)" % pos.y)


## Tests zero inclination orbit stays in XZ plane.
func test_flat_orbit_no_y_component() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, 0.2,
		0.0, # No inclination
		0.0, 0.0,
		45.0
	)
	
	assert_float_equal(pos.y, 0.0, 0.001,
		"Flat orbit should have no Y component")


## Tests get_body_orbital_position with null body.
func test_body_orbital_position_null() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var pos: Vector3 = manager.get_body_orbital_position(null)
	assert_float_equal(pos.length(), 0.0, 0.001,
		"Null body should return origin")


## Tests get_body_orbital_position with body lacking orbital props.
func test_body_orbital_position_no_orbital() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var body: CelestialBody = _make_body(CelestialType.Type.PLANET, Units.EARTH_RADIUS_METERS)
	var pos: Vector3 = manager.get_body_orbital_position(body)
	assert_float_equal(pos.length(), 0.0, 0.001,
		"Body without orbital props should return origin")


## Tests get_body_orbital_position uses orbital props correctly.
func test_body_orbital_position_uses_props() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var body: CelestialBody = _make_orbiting_body(Units.AU_METERS, 0.0, 0.0)
	var pos: Vector3 = manager.get_body_orbital_position(body)
	
	var distance: float = pos.length()
	assert_float_equal(distance, 1.0, 0.05,
		"Body at 1 AU circular orbit should be at ~1.0 units from origin")


# =============================================================================
# ORBIT POINTS GENERATION TESTS
# =============================================================================


## Tests orbit points generation returns correct count.
func test_orbit_points_count() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 64
	)
	# Should be num_points + 1 (closed loop)
	assert_equal(points.size(), 65,
		"Should generate num_points + 1 points for closed loop")


## Tests orbit points form a closed loop.
func test_orbit_points_closed_loop() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 64
	)
	
	var first: Vector3 = points[0]
	var last: Vector3 = points[points.size() - 1]
	var gap: float = first.distance_to(last)
	
	assert_true(gap < 0.01,
		"First and last orbit points should be nearly identical (gap=%.5f)" % gap)


## Tests circular orbit points are all equidistant from origin.
func test_circular_orbit_points_equidistant() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 32
	)
	
	var expected_dist: float = 1.0 # 1 AU in units
	for i in range(points.size()):
		var dist: float = points[i].length()
		assert_float_equal(dist, expected_dist, 0.05,
			"Circular orbit point %d should be at distance 1.0 (got %.3f)" % [i, dist])


## Tests eccentric orbit points vary in distance.
func test_eccentric_orbit_points_vary_distance() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.5, 0.0, 0.0, 0.0, 64
	)
	
	var min_dist: float = INF
	var max_dist: float = 0.0
	for point in points:
		var dist: float = point.length()
		min_dist = minf(min_dist, dist)
		max_dist = maxf(max_dist, dist)
	
	# With e=0.5: periapsis=0.5 AU, apoapsis=1.5 AU
	assert_true(min_dist < 0.7,
		"Eccentric orbit min distance should be < 0.7 (got %.3f)" % min_dist)
	assert_true(max_dist > 1.3,
		"Eccentric orbit max distance should be > 1.3 (got %.3f)" % max_dist)


## Tests zero semi-major axis returns empty points.
func test_orbit_points_zero_sma() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		0.0, 0.0, 0.0, 0.0, 0.0, 32
	)
	assert_equal(points.size(), 0,
		"Zero semi-major axis should return empty points array")


## Tests flat orbit points have zero Y.
func test_flat_orbit_points_no_y() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.3, 0.0, 0.0, 0.0, 32
	)
	
	for i in range(points.size()):
		assert_float_equal(points[i].y, 0.0, 0.001,
			"Flat orbit point %d should have zero Y" % i)


## Tests inclined orbit points have non-zero Y.
func test_inclined_orbit_points_have_y() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	var points: PackedVector3Array = manager.generate_orbit_points(
		Units.AU_METERS, 0.0, 30.0, 0.0, 0.0, 32
	)
	
	var has_nonzero_y: bool = false
	for point in points:
		if absf(point.y) > 0.01:
			has_nonzero_y = true
			break
	
	assert_true(has_nonzero_y,
		"Inclined orbit should have points with non-zero Y")


# =============================================================================
# KEPLER EQUATION TESTS
# =============================================================================


## Tests Kepler equation for circular orbit (E = M when e = 0).
func test_kepler_circular() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	# For circular orbit, position at mean anomaly 90 should be at 90 degrees true anomaly
	var pos: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, 90.0
	)
	
	# Should be approximately at (0, 0, 1) in viewport units (Z axis for 90 deg)
	var distance: float = pos.length()
	assert_float_equal(distance, 1.0, 0.01,
		"Circular orbit at 90 deg should be at distance 1.0")


## Tests highly eccentric orbit doesn't produce NaN.
func test_kepler_high_eccentricity_no_nan() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	
	# Test various anomalies with high eccentricity
	var eccentricities: Array[float] = [0.9, 0.95, 0.99]
	var anomalies: Array[float] = [0.0, 45.0, 90.0, 135.0, 180.0, 270.0]
	
	for ecc in eccentricities:
		for anomaly in anomalies:
			var pos: Vector3 = manager.get_orbital_position(
				Units.AU_METERS, ecc, 0.0, 0.0, 0.0, anomaly
			)
			assert_false(is_nan(pos.x) or is_nan(pos.y) or is_nan(pos.z),
				"Position should not be NaN for e=%.2f, M=%.1f" % [ecc, anomaly])
			assert_false(is_inf(pos.x) or is_inf(pos.y) or is_inf(pos.z),
				"Position should not be INF for e=%.2f, M=%.1f" % [ecc, anomaly])


## Tests symmetry: opposite mean anomalies produce opposite positions.
func test_orbit_symmetry() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	
	var pos_0: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, 0.3, 0.0, 0.0, 0.0, 0.0
	)
	var pos_180: Vector3 = manager.get_orbital_position(
		Units.AU_METERS, 0.3, 0.0, 0.0, 0.0, 180.0
	)
	
	# Positions should be on opposite sides (not identical)
	var dot: float = pos_0.normalized().dot(pos_180.normalized())
	assert_true(dot < 0.0,
		"0 and 180 degree positions should be on opposite sides (dot=%.3f)" % dot)


## Tests full orbit: sum of positions should approximately cancel out for circular.
func test_circular_orbit_positions_cancel() -> void:
	var manager: SystemScaleManager = SystemScaleManager.new()
	
	var sum: Vector3 = Vector3.ZERO
	var num_samples: int = 36
	for i in range(num_samples):
		var anomaly: float = float(i) / float(num_samples) * 360.0
		var pos: Vector3 = manager.get_orbital_position(
			Units.AU_METERS, 0.0, 0.0, 0.0, 0.0, anomaly
		)
		sum += pos
	
	# For circular orbit, evenly spaced positions should sum to ~zero
	var avg: Vector3 = sum / float(num_samples)
	assert_true(avg.length() < 0.1,
		"Average position of circular orbit should be near origin (got %.3f)" % avg.length())
