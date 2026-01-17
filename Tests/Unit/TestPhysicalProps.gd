## Tests for PhysicalProps component.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var props: PhysicalProps = PhysicalProps.new()
	assert_equal(props.mass_kg, 0.0)
	assert_equal(props.radius_m, 0.0)
	assert_equal(props.rotation_period_s, 0.0)
	assert_equal(props.axial_tilt_deg, 0.0)
	assert_equal(props.oblateness, 0.0)
	assert_equal(props.magnetic_moment, 0.0)
	assert_equal(props.internal_heat_watts, 0.0)


## Tests creation with specified values.
func test_initialization() -> void:
	var props: PhysicalProps = PhysicalProps.new(1.0e24, 6.0e6, 86400.0, 23.5)
	assert_equal(props.mass_kg, 1.0e24)
	assert_equal(props.radius_m, 6.0e6)
	assert_equal(props.rotation_period_s, 86400.0)
	assert_equal(props.axial_tilt_deg, 23.5)


## Tests volume calculation.
func test_volume_calculation() -> void:
	var props: PhysicalProps = PhysicalProps.new(1.0e24, 1000.0)
	var expected_volume: float = (4.0 / 3.0) * PI * pow(1000.0, 3.0)
	assert_float_equal(props.get_volume_m3(), expected_volume, 1.0)


## Tests volume with zero radius.
func test_volume_zero_radius() -> void:
	var props: PhysicalProps = PhysicalProps.new(1.0e24, 0.0)
	assert_equal(props.get_volume_m3(), 0.0)


## Tests density calculation.
func test_density_calculation() -> void:
	var mass: float = 5.972e24
	var radius: float = 6.371e6
	var props: PhysicalProps = PhysicalProps.new(mass, radius)
	var density: float = props.get_density_kg_m3()
	assert_in_range(density, 5000.0, 6000.0)


## Tests surface gravity calculation.
func test_surface_gravity() -> void:
	var mass: float = 5.972e24
	var radius: float = 6.371e6
	var props: PhysicalProps = PhysicalProps.new(mass, radius)
	var gravity: float = props.get_surface_gravity_m_s2()
	assert_in_range(gravity, 9.5, 10.1)


## Tests escape velocity calculation.
func test_escape_velocity() -> void:
	var mass: float = 5.972e24
	var radius: float = 6.371e6
	var props: PhysicalProps = PhysicalProps.new(mass, radius)
	var escape_v: float = props.get_escape_velocity_m_s()
	assert_in_range(escape_v, 11000.0, 11400.0)


## Tests oblateness values.
func test_oblateness() -> void:
	# Jupiter has oblateness ~0.065
	var props: PhysicalProps = PhysicalProps.new(
		1.898e27, 6.991e7, 35730.0, 3.1, 0.065
	)
	assert_float_equal(props.oblateness, 0.065)
	
	var eq_radius: float = props.get_equatorial_radius_m()
	assert_greater_than(eq_radius, props.radius_m)
	
	var polar_radius: float = props.get_polar_radius_m()
	assert_less_than(polar_radius, props.radius_m)


## Tests internal heat.
func test_internal_heat() -> void:
	var props: PhysicalProps = PhysicalProps.new(5.972e24, 6.371e6, 86400.0, 23.5, 0.003, 0.0, 4.4e13)
	assert_equal(props.internal_heat_watts, 4.4e13)


## Tests to_dict produces correct structure.
func test_to_dict() -> void:
	var props: PhysicalProps = PhysicalProps.new(1.0e24, 6.0e6, 86400.0, 23.5)
	var data: Dictionary = props.to_dict()
	assert_equal(data["mass_kg"], 1.0e24)
	assert_equal(data["radius_m"], 6.0e6)
	assert_equal(data["rotation_period_s"], 86400.0)
	assert_equal(data["axial_tilt_deg"], 23.5)


## Tests from_dict correctly restores values.
func test_from_dict() -> void:
	var data: Dictionary = {
		"mass_kg": 2.0e24,
		"radius_m": 7.0e6,
		"rotation_period_s": 43200.0,
		"axial_tilt_deg": 15.0
	}
	var props: PhysicalProps = PhysicalProps.from_dict(data)
	assert_equal(props.mass_kg, 2.0e24)
	assert_equal(props.radius_m, 7.0e6)
	assert_equal(props.rotation_period_s, 43200.0)
	assert_equal(props.axial_tilt_deg, 15.0)


## Tests round-trip serialization.
func test_round_trip() -> void:
	var original: PhysicalProps = PhysicalProps.new(3.5e24, 8.0e6, 72000.0, 45.0)
	var data: Dictionary = original.to_dict()
	var restored: PhysicalProps = PhysicalProps.from_dict(data)
	assert_equal(restored.mass_kg, original.mass_kg)
	assert_equal(restored.radius_m, original.radius_m)
	assert_equal(restored.rotation_period_s, original.rotation_period_s)
	assert_equal(restored.axial_tilt_deg, original.axial_tilt_deg)
	assert_equal(restored.oblateness, original.oblateness)
	assert_equal(restored.magnetic_moment, original.magnetic_moment)
	assert_equal(restored.internal_heat_watts, original.internal_heat_watts)