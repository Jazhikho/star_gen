## Unit tests for GalaxySaveData.
extends TestCase


func get_test_name() -> String:
	return "TestGalaxySaveData"


func test_create_sets_timestamp() -> void:
	var data: GalaxySaveData = GalaxySaveData.create()

	assert_greater_than(data.saved_at, 0, "Should have timestamp")


func test_default_values() -> void:
	var data: GalaxySaveData = GalaxySaveData.new()

	assert_equal(data.version, GalaxySaveData.FORMAT_VERSION, "Should have current version")
	assert_equal(data.galaxy_seed, 42, "Should have default seed")
	assert_equal(data.zoom_level, GalaxyCoordinates.ZoomLevel.SUBSECTOR, "Should have default zoom")


func test_is_valid_with_defaults() -> void:
	var data: GalaxySaveData = GalaxySaveData.new()

	assert_true(data.is_valid(), "Default data should be valid")


func test_is_valid_rejects_zero_seed() -> void:
	var data: GalaxySaveData = GalaxySaveData.new()
	data.galaxy_seed = 0

	assert_false(data.is_valid(), "Zero seed should be invalid")


func test_is_valid_rejects_invalid_zoom() -> void:
	var data: GalaxySaveData = GalaxySaveData.new()
	data.zoom_level = -1

	assert_false(data.is_valid(), "Negative zoom should be invalid")


func test_to_dict_contains_required_fields() -> void:
	var data: GalaxySaveData = GalaxySaveData.create()
	data.galaxy_seed = 12345
	data.zoom_level = GalaxyCoordinates.ZoomLevel.QUADRANT

	var dict: Dictionary = data.to_dict()

	assert_true(dict.has("version"), "Should have version")
	assert_true(dict.has("galaxy_seed"), "Should have galaxy_seed")
	assert_true(dict.has("zoom_level"), "Should have zoom_level")
	assert_true(dict.has("saved_at"), "Should have saved_at")


func test_round_trip_basic() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.galaxy_seed = 99999
	original.zoom_level = GalaxyCoordinates.ZoomLevel.SECTOR

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_not_null(restored, "Should deserialize")
	assert_equal(restored.galaxy_seed, original.galaxy_seed, "Seed should match")
	assert_equal(restored.zoom_level, original.zoom_level, "Zoom should match")


func test_round_trip_with_quadrant() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.selected_quadrant = Vector3i(7, 0, 3)

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_not_null(restored.selected_quadrant, "Should have quadrant")
	assert_equal(restored.selected_quadrant, Vector3i(7, 0, 3), "Quadrant should match")


func test_round_trip_with_sector() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.selected_sector = Vector3i(5, 2, 8)

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_not_null(restored.selected_sector, "Should have sector")
	assert_equal(restored.selected_sector, Vector3i(5, 2, 8), "Sector should match")


func test_round_trip_with_camera() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.camera_position = Vector3(8000.5, 20.3, 150.7)
	original.camera_rotation = Vector3(0.1, 0.5, 0.0)

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_true(restored.camera_position.is_equal_approx(original.camera_position),
		"Camera position should match")
	assert_true(restored.camera_rotation.is_equal_approx(original.camera_rotation),
		"Camera rotation should match")


func test_round_trip_with_star_selection() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.has_star_selection = true
	original.selected_star_seed = 55555
	original.selected_star_position = Vector3(8001.2, 19.8, 155.3)

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_true(restored.has_star_selection, "Should have star selection")
	assert_equal(restored.selected_star_seed, 55555, "Star seed should match")
	assert_true(restored.selected_star_position.is_equal_approx(original.selected_star_position),
		"Star position should match")


func test_null_quadrant_serializes() -> void:
	var original: GalaxySaveData = GalaxySaveData.create()
	original.selected_quadrant = null

	var dict: Dictionary = original.to_dict()
	var restored: GalaxySaveData = GalaxySaveData.from_dict(dict)

	assert_null(restored.selected_quadrant, "Null quadrant should deserialize as null")


func test_from_dict_returns_null_for_invalid() -> void:
	var invalid: Dictionary = {"foo": "bar"}
	var result: GalaxySaveData = GalaxySaveData.from_dict(invalid)

	assert_null(result, "Should return null for invalid dict")


func test_from_dict_returns_null_for_empty() -> void:
	var empty: Dictionary = {}
	var result: GalaxySaveData = GalaxySaveData.from_dict(empty)

	assert_null(result, "Should return null for empty dict")


func test_get_summary() -> void:
	var data: GalaxySaveData = GalaxySaveData.create()
	data.galaxy_seed = 42
	data.zoom_level = GalaxyCoordinates.ZoomLevel.SUBSECTOR

	var summary: String = data.get_summary()

	assert_true(summary.contains("42"), "Summary should contain seed")
	assert_true(summary.contains("Star Field"), "Summary should contain zoom level")


func test_vector3_conversion() -> void:
	var original: Vector3 = Vector3(1.5, 2.7, 3.9)
	var arr: Array = GalaxySaveData._vector3_to_array(original)
	var restored: Vector3 = GalaxySaveData._array_to_vector3(arr)

	assert_true(restored.is_equal_approx(original), "Vector3 should round-trip")


func test_vector3i_conversion() -> void:
	var original: Vector3i = Vector3i(5, -3, 8)
	var arr: Array = GalaxySaveData._vector3i_to_array(original)
	var restored: Vector3i = GalaxySaveData._array_to_vector3i(arr)

	assert_equal(restored, original, "Vector3i should round-trip")
