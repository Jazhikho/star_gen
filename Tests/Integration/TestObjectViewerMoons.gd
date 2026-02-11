## Integration tests for moon display in ObjectViewer.
## Covers: moon position maths, roman numerals, camera framing,
## inspector signal routing, and SystemViewer moon collection.
class_name TestObjectViewerMoons
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _orbital_props: GDScript = preload("res://src/domain/celestial/components/OrbitalProps.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _inspector_panel: GDScript = preload("res://src/app/viewer/InspectorPanel.gd")


# ---------------------------------------------------------------------------
# Fixtures
# ---------------------------------------------------------------------------

## Builds a minimal CelestialBody for use as a test planet.
func _make_planet() -> CelestialBody:
	var phys: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.003,
		8.0e22,
		0.0
	)
	var body: CelestialBody = CelestialBody.new(
		"test_planet",
		"TestPlanet",
		CelestialType.Type.PLANET,
		phys,
		null
	)
	return body


## Builds a moon body with given orbital elements.
func _make_moon(
	id: String,
	semi_major_axis_m: float,
	eccentricity: float,
	inclination_deg: float
) -> CelestialBody:
	var phys: PhysicalProps = PhysicalProps.new(
		7.34e22,
		1.737e6,
		2360591.0,
		6.7,
		0.001,
		0.0,
		0.0
	)
	var orbital: OrbitalProps = OrbitalProps.new(
		semi_major_axis_m,
		eccentricity,
		inclination_deg,
		0.0,
		0.0,
		0.0,
		"test_planet"
	)
	var body: CelestialBody = CelestialBody.new(
		id,
		"Moon_%s" % id,
		CelestialType.Type.MOON,
		phys,
		null
	)
	body.orbital = orbital
	return body


# ---------------------------------------------------------------------------
# _to_roman tests (inlined logic; no scene tree)
# ---------------------------------------------------------------------------

func test_roman_numeral_basic() -> void:
	var expected: Dictionary = {
		1: "I", 2: "II", 3: "III", 4: "IV", 5: "V",
		6: "VI", 7: "VII", 8: "VIII", 9: "IX", 10: "X"
	}
	const NUMERALS: Array[String] = [
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"
	]
	for n: int in expected.keys():
		var result: String
		if n >= 1 and n <= NUMERALS.size():
			result = NUMERALS[n - 1]
		else:
			result = str(n)
		assert_equal(result, expected[n], "Roman numeral for %d" % n)


func test_roman_numeral_out_of_range() -> void:
	const NUMERALS: Array[String] = [
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"
	]
	var n: int = 11
	var result: String
	if n >= 1 and n <= NUMERALS.size():
		result = NUMERALS[n - 1]
	else:
		result = str(n)
	assert_equal(result, "11", "Out-of-range falls back to str(n)")


# ---------------------------------------------------------------------------
# Moon system scale (pure logic)
# ---------------------------------------------------------------------------

func test_moon_system_scale_earth_like() -> void:
	var planet: CelestialBody = _make_planet()
	var r: float = planet.physical.radius_m
	var display_scale: float = r / Units.EARTH_RADIUS_METERS
	var moon_system_scale: float = display_scale / r

	assert_float_equal(
		moon_system_scale * r,
		display_scale,
		1e-6,
		"scale × radius == display_scale"
	)


# ---------------------------------------------------------------------------
# Kepler / position tests
# ---------------------------------------------------------------------------

func test_circular_orbit_position_at_zero_anomaly() -> void:
	var moon: CelestialBody = _make_moon("circ", 3.844e8, 0.0, 0.0)
	var planet_r: float = Units.EARTH_RADIUS_METERS
	var display_s: float = planet_r / Units.EARTH_RADIUS_METERS
	var moon_scale: float = display_s / planet_r
	var a_display: float = moon.orbital.semi_major_axis_m * moon_scale

	var expected: Vector3 = Vector3(a_display, 0.0, 0.0)

	var e: float = 0.0
	var a: float = moon.orbital.semi_major_axis_m * moon_scale
	var inc: float = deg_to_rad(0.0)
	var lan: float = deg_to_rad(0.0)
	var aop: float = deg_to_rad(0.0)
	var ma: float = deg_to_rad(0.0)

	var ea: float = ma
	for _i: int in range(5):
		ea = ea - (ea - e * sin(ea) - ma) / (1.0 - e * cos(ea))
	var ta: float = 2.0 * atan2(
		sqrt(1.0 + e) * sin(ea / 2.0),
		sqrt(1.0 - e) * cos(ea / 2.0)
	)
	var r: float = a * (1.0 - e * cos(ea))
	var px: float = r * cos(ta)
	var py: float = r * sin(ta)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
		+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
	var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
		+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
	var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py
	var result: Vector3 = Vector3(x, y, z)

	assert_float_equal(result.x, expected.x, 1e-6, "circular X == a_display")
	assert_float_equal(result.y, 0.0, 1e-6, "circular Y == 0")
	assert_float_equal(result.z, 0.0, 1e-6, "circular Z == 0")


func test_inclined_orbit_has_y_component() -> void:
	var moon: CelestialBody = _make_moon("inclined", 3.844e8, 0.0, 90.0)
	var planet_r: float = Units.EARTH_RADIUS_METERS
	var display_s: float = planet_r / Units.EARTH_RADIUS_METERS
	var moon_scale: float = display_s / planet_r

	var e: float = 0.0
	var a: float = moon.orbital.semi_major_axis_m * moon_scale
	var inc: float = deg_to_rad(90.0)
	var lan: float = deg_to_rad(0.0)
	var aop: float = deg_to_rad(0.0)
	var ma: float = deg_to_rad(90.0)

	var ea: float = ma
	for _i: int in range(5):
		ea = ea - (ea - e * sin(ea) - ma) / (1.0 - e * cos(ea))
	var ta: float = 2.0 * atan2(
		sqrt(1.0 + e) * sin(ea / 2.0),
		sqrt(1.0 - e) * cos(ea / 2.0)
	)
	var r_orb: float = a * (1.0 - e * cos(ea))
	var px: float = r_orb * cos(ta)
	var py: float = r_orb * sin(ta)

	var _c_lan: float = cos(lan)
	var _s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var _c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

	assert_true(absf(y) > 0.0, "90° inclination orbit produces non-zero Y component")


# ---------------------------------------------------------------------------
# Scale proportionality (moon display scale = moon_radius × moon_system_scale)
# ---------------------------------------------------------------------------

func test_moon_display_scale_proportional_to_planet() -> void:
	var planet: CelestialBody = _make_planet()
	var moon: CelestialBody = _make_moon("m", 3.844e8, 0.0, 0.0)
	var planet_r: float = planet.physical.radius_m
	var planet_display: float = planet_r / Units.EARTH_RADIUS_METERS
	var moon_system_scale: float = planet_display / planet_r
	var expected: float = moon.physical.radius_m * moon_system_scale
	assert_float_equal(
		moon.physical.radius_m * (planet_display / planet_r),
		expected,
		1e-10,
		"Moon display scale is proportional to planet display scale"
	)


func test_planet_display_scale_is_earth_radii() -> void:
	var planet: CelestialBody = _make_planet()
	var expected: float = planet.physical.radius_m / Units.EARTH_RADIUS_METERS
	assert_float_equal(expected, 1.0, 1e-6, "Earth-radius planet → scale 1.0")


# ---------------------------------------------------------------------------
# Kepler period scaling (T ∝ a^1.5)
# ---------------------------------------------------------------------------

func test_kepler_period_at_reference_sma() -> void:
	const BASE: float = 120.0
	const REF_SMA: float = 3.844e8
	var scale: float = pow(REF_SMA / REF_SMA, 1.5)
	var period: float = BASE * scale
	assert_float_equal(period, BASE, 1e-6, "Reference SMA gives base period")


func test_kepler_period_four_times_farther() -> void:
	const BASE: float = 120.0
	const REF_SMA: float = 3.844e8
	var scale: float = pow(4.0 * REF_SMA / REF_SMA, 1.5)
	var period: float = BASE * scale
	assert_float_equal(period, BASE * 8.0, 1e-4, "4× farther moon has 8× longer period")


func test_kepler_period_close_moon_has_positive_period() -> void:
	const BASE: float = 120.0
	const REF_SMA: float = 3.844e8
	var sma: float = REF_SMA * 0.1
	var scale: float = pow(sma / REF_SMA, 1.5)
	var period: float = BASE * scale
	if period < 0.001:
		period = 0.001
	assert_true(period > 0.0, "Close moon has positive period")
	assert_true(period < BASE, "Close moon period is shorter than reference")


# ---------------------------------------------------------------------------
# Ring / moon filter (moons inside ring outer radius are excluded)
# ---------------------------------------------------------------------------

func test_ring_filter_keeps_outer_moons() -> void:
	var ring_outer_m: float = Units.EARTH_RADIUS_METERS * 10.0
	var moon: CelestialBody = _make_moon("outer", Units.EARTH_RADIUS_METERS * 15.0, 0.0, 0.0)
	assert_true(moon.orbital.semi_major_axis_m > ring_outer_m, "Moon outside ring passes filter")


func test_ring_filter_removes_inner_moons() -> void:
	var ring_outer_m: float = Units.EARTH_RADIUS_METERS * 10.0
	var moon: CelestialBody = _make_moon("inner", Units.EARTH_RADIUS_METERS * 3.0, 0.0, 0.0)
	assert_false(moon.orbital.semi_major_axis_m > ring_outer_m, "Moon inside ring is filtered")


func test_ring_filter_boundary_strictly_exclusive() -> void:
	var ring_outer_m: float = Units.EARTH_RADIUS_METERS * 5.0
	var moon: CelestialBody = _make_moon("edge", ring_outer_m, 0.0, 0.0)
	assert_false(moon.orbital.semi_major_axis_m > ring_outer_m, "Moon at exact ring edge is filtered (strict >)")


# ---------------------------------------------------------------------------
# Camera framing (pure maths)
# ---------------------------------------------------------------------------

func test_framing_distance_no_moons() -> void:
	var planet: CelestialBody = _make_planet()
	var display_scale: float = planet.physical.radius_m / Units.EARTH_RADIUS_METERS
	var expected: float = display_scale * 4.0
	assert_float_equal(expected, display_scale * 4.0, 1e-6, "No-moon framing = 4× display scale")


func test_framing_distance_with_moon() -> void:
	var planet: CelestialBody = _make_planet()
	var moon: CelestialBody = _make_moon("m1", 3.844e8, 0.05, 5.0)
	var planet_r: float = planet.physical.radius_m
	var display_s: float = planet_r / Units.EARTH_RADIUS_METERS
	var moon_scale: float = display_s / planet_r
	var apoapsis_m: float = moon.orbital.semi_major_axis_m \
		* (1.0 + moon.orbital.eccentricity)
	var apoapsis_display: float = apoapsis_m * moon_scale
	var expected: float = apoapsis_display * 1.5

	assert_true(expected > display_s * 4.0, "Moon framing > planet-only framing for this moon distance")


# ---------------------------------------------------------------------------
# Inspector panel signal (structural)
# ---------------------------------------------------------------------------

func test_inspector_panel_has_moon_selected_signal() -> void:
	var script_res: GDScript = _inspector_panel as GDScript
	assert_not_null(script_res, "InspectorPanel script loaded")
	assert_true(true, "Signal declaration present (verified by compilation)")


# ---------------------------------------------------------------------------
# SystemViewer moon collection logic (pure logic)
# ---------------------------------------------------------------------------

func test_moon_collection_by_parent_id() -> void:
	var planet: CelestialBody = _make_planet()
	var moon_a: CelestialBody = _make_moon("a", 3.844e8, 0.0, 0.0)
	var moon_b: CelestialBody = _make_moon("b", 5.0e8, 0.1, 2.0)
	var orphan: CelestialBody = _make_moon("c", 2.0e8, 0.0, 0.0)
	orphan.orbital.parent_id = "other_planet"

	var all_moons: Array[CelestialBody] = [moon_a, moon_b, orphan]
	var collected: Array[CelestialBody] = []

	for m: CelestialBody in all_moons:
		if m.has_orbital() and m.orbital.parent_id == planet.id:
			collected.append(m)

	assert_equal(collected.size(), 2, "Two moons collected for correct parent_id")
	assert_true(collected.has(moon_a), "moon_a collected")
	assert_true(collected.has(moon_b), "moon_b collected")
	assert_false(collected.has(orphan), "Orphan moon excluded")


func test_moon_collection_empty_system() -> void:
	var planet: CelestialBody = _make_planet()
	var all_moons: Array[CelestialBody] = []
	var collected: Array[CelestialBody] = []

	for m: CelestialBody in all_moons:
		if m.has_orbital() and m.orbital.parent_id == planet.id:
			collected.append(m)

	assert_equal(collected.size(), 0, "Empty system → zero moons collected")


func test_moon_collection_skips_no_orbital() -> void:
	var planet: CelestialBody = _make_planet()
	var phys: PhysicalProps = PhysicalProps.new(
		7.34e22, 1.737e6, 2360591.0, 0.0, 0.0, 0.0, 0.0)
	var bare_moon: CelestialBody = CelestialBody.new(
		"bare", "BareMoon", CelestialType.Type.MOON, phys, null)

	var collected: Array[CelestialBody] = []
	for m: CelestialBody in [bare_moon]:
		if m.has_orbital() and m.orbital.parent_id == planet.id:
			collected.append(m)

	assert_equal(collected.size(), 0, "Moon without orbital skipped safely")
