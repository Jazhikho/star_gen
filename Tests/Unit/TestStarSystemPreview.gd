## Unit tests for StarSystemPreview.
class_name TestStarSystemPreview
extends TestCase

const _preview_script: GDScript = preload("res://src/domain/galaxy/StarSystemPreview.gd")
const _galaxy_config_script: GDScript = preload("res://src/domain/galaxy/GalaxyConfig.gd")
const _galaxy_script: GDScript = preload("res://src/domain/galaxy/Galaxy.gd")


func get_test_name() -> String:
	return "TestStarSystemPreview"


func test_generate_returns_null_for_zero_seed() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		0, Vector3(100.0, 0.0, 0.0), galaxy.spec
	)
	assert_null(result, "Zero seed should return null")


func test_generate_returns_null_for_null_spec() -> void:
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		12345, Vector3(100.0, 0.0, 0.0), null
	)
	assert_null(result, "Null spec should return null")


func test_generate_returns_preview_data() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		99999, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Valid inputs should produce PreviewData")


func test_generate_seeds_match() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var seed_value: int = 77777
	var pos: Vector3 = Vector3(8000.0, 0.0, 0.0)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		seed_value, pos, galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_equal(result.star_seed, seed_value, "star_seed should match input")


func test_generate_caches_system() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		55555, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_not_null(result.system, "Cached system should not be null")
	assert_true(result.system.is_valid(), "Cached system should be valid")


func test_generate_star_count_at_least_one() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		11111, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_true(result.star_count >= 1, "System must have at least one star")


func test_generate_spectral_classes_match_star_count() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		22222, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_equal(
		result.spectral_classes.size(),
		result.star_count,
		"Spectral classes array length should equal star_count"
	)


func test_generate_temperatures_match_star_count() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		33333, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_equal(
		result.star_temperatures.size(),
		result.star_count,
		"Temperatures array length should equal star_count"
	)


func test_generate_is_deterministic() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var pos: Vector3 = Vector3(8000.0, 50.0, 200.0)
	var seed_value: int = 44444

	var result_a: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		seed_value, pos, galaxy.spec
	)
	var result_b: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		seed_value, pos, galaxy.spec
	)

	assert_not_null(result_a, "First call should produce data")
	assert_not_null(result_b, "Second call should produce data")
	assert_equal(result_a.star_count, result_b.star_count, "Star count must be deterministic")
	assert_equal(result_a.planet_count, result_b.planet_count, "Planet count must be deterministic")
	assert_equal(result_a.belt_count, result_b.belt_count, "Belt count must be deterministic")


func test_generate_metallicity_positive() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	var galaxy: Galaxy = Galaxy.new(config, 42)
	var result: StarSystemPreview.PreviewData = StarSystemPreview.generate(
		66666, Vector3(8000.0, 0.0, 0.0), galaxy.spec
	)
	assert_not_null(result, "Should produce data")
	assert_true(result.metallicity > 0.0, "Metallicity must be positive")
