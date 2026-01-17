## Tests for BaseSpec and override functionality.
extends TestCase

const _phase1_deps := preload("res://Tests/Phase1Deps.gd")


## Tests creation with default values.
func test_default_values() -> void:
	var spec: BaseSpec = BaseSpec.new()
	assert_equal(spec.generation_seed, 0)
	assert_equal(spec.name_hint, "")
	assert_equal(spec.overrides.size(), 0)


## Tests creation with all parameters.
func test_initialization() -> void:
	var overrides: Dictionary = {"physical.mass_kg": 5.0e24}
	var spec: BaseSpec = BaseSpec.new(12345, "Test Body", overrides)
	
	assert_equal(spec.generation_seed, 12345)
	assert_equal(spec.name_hint, "Test Body")
	assert_true(spec.has_override("physical.mass_kg"))


## Tests override methods.
func test_overrides() -> void:
	var spec: BaseSpec = BaseSpec.new()
	
	assert_false(spec.has_override("test.field"))
	assert_equal(spec.get_override("test.field", 100.0), 100.0)
	
	spec.set_override("test.field", 50.0)
	assert_true(spec.has_override("test.field"))
	assert_equal(spec.get_override("test.field", 100.0), 50.0)
	
	spec.remove_override("test.field")
	assert_false(spec.has_override("test.field"))


## Tests typed override getters.
func test_typed_overrides() -> void:
	var spec: BaseSpec = BaseSpec.new()
	spec.set_override("float_field", 3.14)
	spec.set_override("int_field", 42)
	
	assert_float_equal(spec.get_override_float("float_field", 0.0), 3.14)
	assert_equal(spec.get_override_int("int_field", 0), 42)
	
	# Test defaults when not set
	assert_float_equal(spec.get_override_float("missing", 99.9), 99.9)
	assert_equal(spec.get_override_int("missing", 99), 99)


## Tests clear_overrides.
func test_clear_overrides() -> void:
	var spec: BaseSpec = BaseSpec.new()
	spec.set_override("field1", 1)
	spec.set_override("field2", 2)
	
	assert_equal(spec.overrides.size(), 2)
	spec.clear_overrides()
	assert_equal(spec.overrides.size(), 0)


## Tests StarSpec presets.
func test_star_spec_presets() -> void:
	var sun: StarSpec = StarSpec.sun_like(100)
	assert_equal(sun.spectral_class, StarClass.SpectralClass.G)
	assert_equal(sun.subclass, 2)
	
	var red: StarSpec = StarSpec.red_dwarf(200)
	assert_equal(red.spectral_class, StarClass.SpectralClass.M)


## Tests PlanetSpec presets.
func test_planet_spec_presets() -> void:
	var earth: PlanetSpec = PlanetSpec.earth_like(100)
	assert_equal(earth.size_category, SizeCategory.Category.TERRESTRIAL)
	assert_equal(earth.orbit_zone, OrbitZone.Zone.TEMPERATE)
	
	var jupiter: PlanetSpec = PlanetSpec.hot_jupiter(200)
	assert_equal(jupiter.size_category, SizeCategory.Category.GAS_GIANT)
	assert_equal(jupiter.orbit_zone, OrbitZone.Zone.HOT)


## Tests MoonSpec presets.
func test_moon_spec_presets() -> void:
	var europa: MoonSpec = MoonSpec.europa_like(100)
	assert_equal(europa.has_subsurface_ocean, true)
	assert_false(europa.is_captured)
	
	var captured: MoonSpec = MoonSpec.captured(200)
	assert_true(captured.is_captured)


## Tests AsteroidSpec presets.
func test_asteroid_spec_presets() -> void:
	var c_type: AsteroidSpec = AsteroidSpec.carbonaceous(100)
	assert_equal(c_type.asteroid_type, AsteroidType.Type.C_TYPE)
	
	var ceres: AsteroidSpec = AsteroidSpec.ceres_like(200)
	assert_true(ceres.is_large)


## Tests spec serialization round-trip.
func test_spec_serialization() -> void:
	var original: StarSpec = StarSpec.new(12345, StarClass.SpectralClass.G, 2, 1.0, 4.6e9, "Sol", {"mass": 1.0})
	var data: Dictionary = original.to_dict()
	var restored: StarSpec = StarSpec.from_dict(data)
	
	assert_equal(restored.generation_seed, original.generation_seed)
	assert_equal(restored.spectral_class, original.spectral_class)
	assert_equal(restored.subclass, original.subclass)
	assert_equal(restored.name_hint, original.name_hint)
	assert_true(restored.has_override("mass"))
