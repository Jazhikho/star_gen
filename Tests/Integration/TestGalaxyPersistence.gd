## Integration tests for GalaxyPersistence.
extends TestCase

## Preload domain and service so class_name types are in scope.
const _galaxy_save_data := preload("res://src/domain/galaxy/GalaxySaveData.gd")
const _galaxy_persistence := preload("res://src/services/persistence/GalaxyPersistence.gd")


## Test file paths for galaxy persistence tests.
const TEST_JSON_PATH: String = "user://test_galaxy_save/test_galaxy.json"
const TEST_BINARY_PATH: String = "user://test_galaxy_save/test_galaxy.sgg"
const TEST_DIR: String = "user://test_galaxy_save/"


## Ensures test directory exists before each test.
func before_each() -> void:
	DirAccess.make_dir_recursive_absolute(TEST_DIR)


## Creates valid GalaxySaveData for tests.
func _create_test_data() -> GalaxySaveData:
	var data: GalaxySaveData = GalaxySaveData.create()
	data.galaxy_seed = 12345
	data.zoom_level = GalaxyCoordinates.ZoomLevel.SECTOR
	data.selected_quadrant = Vector3i(1, 0, 2)
	data.selected_sector = Vector3i(5, 3, 7)
	data.camera_position = Vector3(100.0, 50.0, 200.0)
	data.camera_rotation = Vector3(0.1, 0.5, 0.0)
	data.has_star_selection = true
	data.selected_star_seed = 99999
	data.selected_star_position = Vector3(105.0, 52.0, 210.0)
	return data


## Ensures test directory exists and removes test files after each test.
func after_each() -> void:
	if FileAccess.file_exists(TEST_JSON_PATH):
		DirAccess.remove_absolute(TEST_JSON_PATH)
	if FileAccess.file_exists(TEST_BINARY_PATH):
		DirAccess.remove_absolute(TEST_BINARY_PATH)


func get_test_name() -> String:
	return "TestGalaxyPersistence"


## Tests save_json and load_json round-trip.
func test_save_json_and_load_json_round_trip() -> void:
	var original: GalaxySaveData = _create_test_data()

	var err_msg: String = GalaxyPersistence.save_json(TEST_JSON_PATH, original)
	assert_equal(err_msg, "", "Save should succeed")

	var loaded: GalaxySaveData = GalaxyPersistence.load_json(TEST_JSON_PATH)
	assert_not_null(loaded, "Load should return data")

	assert_equal(loaded.galaxy_seed, original.galaxy_seed, "Seed should match")
	assert_equal(loaded.zoom_level, original.zoom_level, "Zoom level should match")
	assert_equal(loaded.selected_quadrant, original.selected_quadrant, "Quadrant should match")
	assert_equal(loaded.selected_sector, original.selected_sector, "Sector should match")
	assert_true(loaded.camera_position.is_equal_approx(original.camera_position),
		"Camera position should match")
	assert_true(loaded.camera_rotation.is_equal_approx(original.camera_rotation),
		"Camera rotation should match")
	assert_equal(loaded.has_star_selection, original.has_star_selection, "Star selection flag should match")
	assert_equal(loaded.selected_star_seed, original.selected_star_seed, "Star seed should match")
	assert_true(loaded.selected_star_position.is_equal_approx(original.selected_star_position),
		"Star position should match")


## Tests save_binary and load_binary round-trip.
func test_save_binary_and_load_binary_round_trip() -> void:
	var original: GalaxySaveData = _create_test_data()

	var err_msg: String = GalaxyPersistence.save_binary(TEST_BINARY_PATH, original)
	assert_equal(err_msg, "", "Save should succeed")

	var loaded: GalaxySaveData = GalaxyPersistence.load_binary(TEST_BINARY_PATH)
	assert_not_null(loaded, "Load should return data")

	assert_equal(loaded.galaxy_seed, original.galaxy_seed, "Seed should match")
	assert_equal(loaded.zoom_level, original.zoom_level, "Zoom level should match")
	assert_true(loaded.camera_position.is_equal_approx(original.camera_position),
		"Camera position should match")
	assert_equal(loaded.selected_star_seed, original.selected_star_seed, "Star seed should match")


## Tests load_auto with JSON extension.
func test_load_auto_json() -> void:
	var original: GalaxySaveData = _create_test_data()
	GalaxyPersistence.save_json(TEST_JSON_PATH, original)

	var loaded: GalaxySaveData = GalaxyPersistence.load_auto(TEST_JSON_PATH)
	assert_not_null(loaded, "Load auto should return data for .json")
	assert_equal(loaded.galaxy_seed, original.galaxy_seed, "Seed should match")


## Tests load_auto with binary extension.
func test_load_auto_binary() -> void:
	var original: GalaxySaveData = _create_test_data()
	GalaxyPersistence.save_binary(TEST_BINARY_PATH, original)

	var loaded: GalaxySaveData = GalaxyPersistence.load_auto(TEST_BINARY_PATH)
	assert_not_null(loaded, "Load auto should return data for .sgg")
	assert_equal(loaded.galaxy_seed, original.galaxy_seed, "Seed should match")


## Tests load_json from non-existent file returns null.
func test_load_json_nonexistent_returns_null() -> void:
	var loaded: GalaxySaveData = GalaxyPersistence.load_json("user://does_not_exist_galaxy.json")
	assert_null(loaded, "Load should return null for missing file")


## Tests save_json with null data returns error.
func test_save_json_null_data_returns_error() -> void:
	var err_msg: String = GalaxyPersistence.save_json(TEST_JSON_PATH, null)
	assert_true(err_msg.length() > 0, "Should return error for null data")


## Tests save_json with invalid data returns error.
func test_save_json_invalid_data_returns_error() -> void:
	var invalid: GalaxySaveData = GalaxySaveData.new()
	invalid.galaxy_seed = 0

	var err_msg: String = GalaxyPersistence.save_json(TEST_JSON_PATH, invalid)
	assert_true(err_msg.length() > 0, "Should return error for invalid data")


## Tests save_binary with null data returns error.
func test_save_binary_null_data_returns_error() -> void:
	var err_msg: String = GalaxyPersistence.save_binary(TEST_BINARY_PATH, null)
	assert_true(err_msg.length() > 0, "Should return error for null data")


## Tests get_file_filter returns expected format.
func test_get_file_filter() -> void:
	var filter_str: String = GalaxyPersistence.get_file_filter()
	assert_true(filter_str.contains("sgg"), "Filter should mention sgg")
	assert_true(filter_str.contains("json"), "Filter should mention json")
