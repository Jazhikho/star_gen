## Tests for OrbitalProps component.
extends TestCase

const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var props: OrbitalProps = OrbitalProps.new()
	assert_equal(props.semi_major_axis_m, 0.0)
	assert_equal(props.eccentricity, 0.0)
	assert_equal(props.inclination_deg, 0.0)
	assert_equal(props.parent_id, "")


## Tests periapsis calculation.
func test_periapsis() -> void:
	var props: OrbitalProps = OrbitalProps.new(1.0e11, 0.2)
	var expected: float = 1.0e11 * (1.0 - 0.2)
	assert_float_equal(props.get_periapsis_m(), expected)


## Tests apoapsis calculation.
func test_apoapsis() -> void:
	var props: OrbitalProps = OrbitalProps.new(1.0e11, 0.2)
	var expected: float = 1.0e11 * (1.0 + 0.2)
	assert_float_equal(props.get_apoapsis_m(), expected)


## Tests circular orbit (eccentricity = 0).
func test_circular_orbit() -> void:
	var props: OrbitalProps = OrbitalProps.new(1.0e11, 0.0)
	assert_float_equal(props.get_periapsis_m(), props.get_apoapsis_m())


## Tests orbital period calculation with Sun-like parent.
func test_orbital_period() -> void:
	var props: OrbitalProps = OrbitalProps.new(Units.AU_METERS)
	var period: float = props.get_orbital_period_s(Units.SOLAR_MASS_KG)
	var one_year_s: float = 365.25 * 24.0 * 3600.0
	assert_in_range(period, one_year_s * 0.99, one_year_s * 1.01)


## Tests round-trip serialization.
func test_round_trip() -> void:
	var original: OrbitalProps = OrbitalProps.new(
		1.5e11, 0.1, 5.0, 100.0, 200.0, 50.0, "star_001"
	)
	var data: Dictionary = original.to_dict()
	var restored: OrbitalProps = OrbitalProps.from_dict(data)

	assert_float_equal(restored.semi_major_axis_m, original.semi_major_axis_m)
	assert_float_equal(restored.eccentricity, original.eccentricity)
	assert_float_equal(restored.inclination_deg, original.inclination_deg)
	assert_equal(restored.parent_id, original.parent_id)
