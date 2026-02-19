## Integration tests for SystemViewer save/load functionality.
extends TestCase

const _system_viewer_scene: PackedScene = preload("res://src/app/system_viewer/SystemViewer.tscn")
const _system_viewer_save_load: GDScript = preload("res://src/app/system_viewer/SystemViewerSaveLoad.gd")
const _system_persistence: GDScript = preload("res://src/services/persistence/SystemPersistence.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _system_fixture_generator: GDScript = preload("res://src/domain/system/fixtures/SystemFixtureGenerator.gd")


## Test file paths
var _test_json_path: String = "user://test_viewer_system.json"
var _test_binary_path: String = "user://test_viewer_system.sgs"


func get_test_name() -> String:
	return "TestSystemViewerSaveLoad"


## Cleans up test files after each test.
func after_each() -> void:
	if FileAccess.file_exists(_test_json_path):
		DirAccess.remove_absolute(_test_json_path)
	if FileAccess.file_exists(_test_binary_path):
		DirAccess.remove_absolute(_test_binary_path)


## Tests that SystemViewerSaveLoad class exists and instantiates.
func test_save_load_class_exists() -> void:
	var save_load: RefCounted = _system_viewer_save_load.new()
	assert_not_null(save_load, "SystemViewerSaveLoad should instantiate")


## Tests that viewer has save/load UI elements.
func test_viewer_has_save_load_ui() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var save_btn: Node = viewer.get_node_or_null(
		"UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton"
	)
	assert_not_null(save_btn, "Should have SaveButton")
	assert_true(save_btn is Button, "SaveButton should be Button")

	var load_btn: Node = viewer.get_node_or_null(
		"UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton"
	)
	assert_not_null(load_btn, "Should have LoadButton")
	assert_true(load_btn is Button, "LoadButton should be Button")

	viewer.free()


## Tests that viewer exposes get_current_system method.
func test_viewer_has_get_current_system() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	assert_true(viewer.has_method("get_current_system"), "Viewer should have get_current_system method")

	# Initially null before generation (call verifies the method works).
	var _system: Variant = viewer.get_current_system()
	# Note: _ready() calls _on_generate_pressed() so system may exist

	viewer.free()


## Tests that viewer exposes update_seed_display method.
func test_viewer_has_update_seed_display() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	assert_true(viewer.has_method("update_seed_display"), "Viewer should have update_seed_display method")

	viewer.free()


## Tests that viewer has keyboard shortcut handler.
func test_viewer_has_keyboard_handler() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	assert_true(viewer.has_method("_unhandled_key_input"), "Viewer should have _unhandled_key_input method")

	viewer.free()


## Tests that viewer has tooltip setup.
func test_viewer_has_tooltip_setup() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	assert_true(viewer.has_method("_setup_tooltips"), "Viewer should have _setup_tooltips method")

	viewer.free()


## Tests programmatic save via save_load helper.
func test_programmatic_save() -> void:
	var save_load: RefCounted = _system_viewer_save_load.new()

	# Create a mock viewer with required methods
	var mock_viewer: MockViewer = MockViewer.new()
	var spec: SolarSystemSpec = SolarSystemSpec.new(12345, 1, 1)
	mock_viewer.system = SystemFixtureGenerator.generate_system(spec)

	var error: Error = save_load.save_to_path(mock_viewer, _test_binary_path, true)
	assert_equal(error, OK, "Save should succeed")

	assert_true(FileAccess.file_exists(_test_binary_path), "File should exist")


## Tests programmatic load via save_load helper.
func test_programmatic_load() -> void:
	var save_load: RefCounted = _system_viewer_save_load.new()

	# First save a system
	var mock_viewer: MockViewer = MockViewer.new()
	var spec: SolarSystemSpec = SolarSystemSpec.new(54321, 1, 1)
	mock_viewer.system = SystemFixtureGenerator.generate_system(spec)

	save_load.save_to_path(mock_viewer, _test_binary_path, true)

	# Then load it
	var result: SystemPersistence.LoadResult = save_load.load_from_path(_test_binary_path)

	assert_true(result.success, "Load should succeed")
	assert_not_null(result.system, "Should have loaded system")


## Tests round-trip preserves system data.
func test_round_trip_preserves_data() -> void:
	var save_load: RefCounted = _system_viewer_save_load.new()

	var mock_viewer: MockViewer = MockViewer.new()
	var spec: SolarSystemSpec = SolarSystemSpec.new(99999, 2, 2)
	var original: SolarSystem = SystemFixtureGenerator.generate_system(spec)
	mock_viewer.system = original

	# Save
	save_load.save_to_path(mock_viewer, _test_binary_path, true)

	# Load
	var result: SystemPersistence.LoadResult = save_load.load_from_path(_test_binary_path)

	assert_true(result.success, "Load should succeed")
	assert_equal(result.system.id, original.id, "ID should match")
	assert_equal(result.system.star_ids.size(), original.star_ids.size(), "Star count should match")
	assert_equal(result.system.planet_ids.size(), original.planet_ids.size(), "Planet count should match")


## Tests load of nonexistent file.
func test_load_nonexistent_file() -> void:
	var save_load: RefCounted = _system_viewer_save_load.new()

	var result: SystemPersistence.LoadResult = save_load.load_from_path("user://nonexistent_system.sgs")

	assert_false(result.success, "Load should fail")
	assert_true(result.error_message.length() > 0, "Should have error message")


## Mock viewer for testing save/load without full scene instantiation.
## Extends Node so it is accepted by SystemViewerSaveLoad.save_to_path(viewer: Node, ...).
class MockViewer:
	extends Node

	var system: SolarSystem = null
	var status_message: String = ""

	func get_current_system() -> SolarSystem:
		return system

	func set_status(msg: String) -> void:
		status_message = msg

	func set_error(msg: String) -> void:
		status_message = "Error: " + msg

	func update_seed_display(_seed: int) -> void:
		pass

	func display_system(sys: SolarSystem) -> void:
		system = sys
