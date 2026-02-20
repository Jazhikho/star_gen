## Unit tests for GalaxyConfig.
extends TestCase


func get_test_name() -> String:
	return "TestGalaxyConfig"


func test_create_default_returns_valid_config() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()

	assert_not_null(config, "Should return config")
	assert_true(config.is_valid(), "Default config should be valid")
	assert_equal(config.galaxy_type, GalaxySpec.GalaxyType.SPIRAL, "Default type should be spiral")
	assert_equal(config.num_arms, 4, "Default arms should be 4")


func test_create_milky_way_sets_spiral_params() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_milky_way()

	assert_equal(config.galaxy_type, GalaxySpec.GalaxyType.SPIRAL, "Should be spiral")
	assert_equal(config.num_arms, 4, "Should have 4 arms")
	assert_equal(config.arm_pitch_angle_deg, 14.0, "Pitch should match")
	assert_equal(config.arm_amplitude, 0.65, "Amplitude should match")
	assert_equal(config.radius_pc, 15000.0, "Radius should match")


func test_is_valid_rejects_bad_type() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	config.galaxy_type = -1

	assert_false(config.is_valid(), "Invalid type should be rejected")


func test_is_valid_rejects_bad_num_arms() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	config.num_arms = 1

	assert_false(config.is_valid(), "Too few arms should be rejected")

	config.num_arms = 7
	assert_false(config.is_valid(), "Too many arms should be rejected")


func test_is_valid_rejects_bad_radius() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	config.radius_pc = 5000.0

	assert_false(config.is_valid(), "Radius below range should be rejected")

	config.radius_pc = 30000.0
	assert_false(config.is_valid(), "Radius above range should be rejected")


func test_to_dict_round_trip() -> void:
	var original: GalaxyConfig = GalaxyConfig.create_milky_way()
	var dict: Dictionary = original.to_dict()
	var restored: GalaxyConfig = GalaxyConfig.from_dict(dict)

	assert_not_null(restored, "Should deserialize")
	assert_equal(restored.galaxy_type, original.galaxy_type, "Type should match")
	assert_equal(restored.num_arms, original.num_arms, "Arms should match")
	assert_equal(restored.radius_pc, original.radius_pc, "Radius should match")
	assert_true(restored.is_valid(), "Restored config should be valid")


func test_from_dict_empty_returns_null() -> void:
	var result: GalaxyConfig = GalaxyConfig.from_dict({})

	assert_null(result, "Empty dict should return null")


func test_get_type_name_spiral() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	config.galaxy_type = GalaxySpec.GalaxyType.SPIRAL

	assert_equal(config.get_type_name(), "Spiral", "Spiral type name should match")


func test_get_type_name_elliptical() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_default()
	config.galaxy_type = GalaxySpec.GalaxyType.ELLIPTICAL

	assert_equal(config.get_type_name(), "Elliptical", "Elliptical type name should match")


func test_apply_to_spec() -> void:
	var config: GalaxyConfig = GalaxyConfig.create_milky_way()
	config.num_arms = 5
	config.radius_pc = 20000.0
	var spec: GalaxySpec = GalaxySpec.new()
	spec.galaxy_seed = 12345

	config.apply_to_spec(spec)

	assert_equal(spec.num_arms, 5, "Spec should have 5 arms")
	assert_equal(spec.radius_pc, 20000.0, "Spec radius should match")
