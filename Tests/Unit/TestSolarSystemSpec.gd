## Tests for SolarSystemSpec.
extends TestCase

const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _star_class: GDScript = preload("res://src/domain/generation/archetypes/StarClass.gd")


## Tests basic construction.
func test_construction() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(12345, 1, 3)
	
	assert_equal(spec.generation_seed, 12345)
	assert_equal(spec.star_count_min, 1)
	assert_equal(spec.star_count_max, 3)


## Tests clamping of star count.
func test_star_count_clamping() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(123, 0, 20)
	
	assert_equal(spec.star_count_min, 1, "Min should be clamped to 1")
	assert_equal(spec.star_count_max, 10, "Max should be clamped to 10")


## Tests min > max handling.
func test_star_count_min_greater_than_max() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(123, 5, 3)
	
	# Max should be at least min
	assert_equal(spec.star_count_min, 5)
	assert_equal(spec.star_count_max, 5)


## Tests single_star preset.
func test_single_star_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.single_star(999)
	
	assert_equal(spec.generation_seed, 999)
	assert_equal(spec.star_count_min, 1)
	assert_equal(spec.star_count_max, 1)


## Tests binary preset.
func test_binary_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.binary(888)
	
	assert_equal(spec.star_count_min, 2)
	assert_equal(spec.star_count_max, 2)


## Tests random_small preset.
func test_random_small_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.random_small(777)
	
	assert_equal(spec.star_count_min, 1)
	assert_equal(spec.star_count_max, 3)


## Tests random preset.
func test_random_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.random(666)
	
	assert_equal(spec.star_count_min, 1)
	assert_equal(spec.star_count_max, 10)


## Tests sun_like preset.
func test_sun_like_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.sun_like(555)
	
	assert_equal(spec.star_count_min, 1)
	assert_equal(spec.star_count_max, 1)
	assert_equal(spec.spectral_class_hints.size(), 1)
	assert_equal(spec.spectral_class_hints[0], StarClass.SpectralClass.G)


## Tests alpha_centauri_like preset.
func test_alpha_centauri_like_preset() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.alpha_centauri_like(444)
	
	assert_equal(spec.star_count_min, 3)
	assert_equal(spec.star_count_max, 3)
	assert_equal(spec.spectral_class_hints.size(), 3)


## Tests override functionality.
func test_overrides() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(123, 1, 1)
	
	assert_false(spec.has_override("star_count"))
	
	spec.set_override("star_count", 2)
	
	assert_true(spec.has_override("star_count"))
	assert_equal(spec.get_override("star_count", 1), 2)


## Tests system age and metallicity defaults.
func test_system_age_metallicity_defaults() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(123, 1, 1)
	
	assert_float_equal(spec.system_age_years, -1.0, 0.1, "Default age should be -1 (random)")
	assert_float_equal(spec.system_metallicity, -1.0, 0.1, "Default metallicity should be -1 (random)")


## Tests system age and metallicity can be set.
func test_system_age_metallicity_setting() -> void:
	var spec: SolarSystemSpec = SolarSystemSpec.new(123, 1, 1)
	spec.system_age_years = 5.0e9
	spec.system_metallicity = 1.2
	
	assert_float_equal(spec.system_age_years, 5.0e9)
	assert_float_equal(spec.system_metallicity, 1.2)


## Tests serialization round-trip.
func test_round_trip() -> void:
	var original: SolarSystemSpec = SolarSystemSpec.new(12345, 2, 5)
	original.name_hint = "Test System"
	original.spectral_class_hints = [StarClass.SpectralClass.G, StarClass.SpectralClass.K]
	original.system_age_years = 4.5e9
	original.system_metallicity = 0.8
	original.set_override("test", "value")
	
	var data: Dictionary = original.to_dict()
	var restored: SolarSystemSpec = SolarSystemSpec.from_dict(data)
	
	assert_equal(restored.generation_seed, original.generation_seed)
	assert_equal(restored.name_hint, original.name_hint)
	assert_equal(restored.star_count_min, original.star_count_min)
	assert_equal(restored.star_count_max, original.star_count_max)
	assert_equal(restored.spectral_class_hints.size(), 2)
	assert_float_equal(restored.system_age_years, original.system_age_years)
	assert_float_equal(restored.system_metallicity, original.system_metallicity)
	assert_true(restored.has_override("test"))
