## Tests for AtmosphereProps component.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var props: AtmosphereProps = AtmosphereProps.new()
	assert_equal(props.surface_pressure_pa, 0.0)
	assert_equal(props.scale_height_m, 0.0)
	assert_equal(props.composition.size(), 0)
	assert_equal(props.greenhouse_factor, 1.0)


## Tests composition sum calculation.
func test_composition_sum() -> void:
	var comp: Dictionary = {"N2": 0.78, "O2": 0.21, "Ar": 0.01}
	var props: AtmosphereProps = AtmosphereProps.new(101325.0, 8500.0, comp, 1.0)
	assert_float_equal(props.get_composition_sum(), 1.0, 0.001)


## Tests dominant gas detection.
func test_dominant_gas() -> void:
	var comp: Dictionary = {"N2": 0.78, "O2": 0.21, "Ar": 0.01}
	var props: AtmosphereProps = AtmosphereProps.new(101325.0, 8500.0, comp)
	assert_equal(props.get_dominant_gas(), "N2")


## Tests dominant gas with single gas.
func test_dominant_gas_single() -> void:
	var comp: Dictionary = {"CO2": 0.95}
	var props: AtmosphereProps = AtmosphereProps.new(9.2e6, 15900.0, comp)
	assert_equal(props.get_dominant_gas(), "CO2")


## Tests empty composition.
func test_empty_composition() -> void:
	var props: AtmosphereProps = AtmosphereProps.new()
	assert_equal(props.get_composition_sum(), 0.0)
	assert_equal(props.get_dominant_gas(), "")


## Tests round-trip serialization.
func test_round_trip() -> void:
	var comp: Dictionary = {"H2": 0.9, "He": 0.1}
	var original: AtmosphereProps = AtmosphereProps.new(1.0e5, 27000.0, comp, 1.5)
	var data: Dictionary = original.to_dict()
	var restored: AtmosphereProps = AtmosphereProps.from_dict(data)

	assert_float_equal(restored.surface_pressure_pa, original.surface_pressure_pa)
	assert_float_equal(restored.scale_height_m, original.scale_height_m)
	assert_float_equal(restored.greenhouse_factor, original.greenhouse_factor)
	assert_float_equal(restored.composition["H2"], 0.9)
	assert_float_equal(restored.composition["He"], 0.1)
