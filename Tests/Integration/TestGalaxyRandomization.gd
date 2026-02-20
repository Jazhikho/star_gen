## Integration tests for galaxy randomization and save/load seed handling.
extends TestCase

const _main_app_scene := preload("res://src/app/MainApp.tscn")

## Test file path for save/load round-trip.
const TEST_SAVE_PATH: String = "user://test_galaxy_randomization/test_save.sgg"
const TEST_DIR: String = "user://test_galaxy_randomization/"

var _main_app: MainApp = null
var _wrapper: Node = null
var _skip_tests: bool = false


func get_test_name() -> String:
	return "TestGalaxyRandomization"


func before_each() -> void:
	_main_app = null
	_wrapper = null
	_skip_tests = false
	var tree: SceneTree = Engine.get_main_loop() as SceneTree
	if tree == null or tree.root == null:
		_skip_tests = true
		return
	DirAccess.make_dir_recursive_absolute(TEST_DIR)
	_wrapper = Node.new()
	var runner_tree: SceneTree = get("runner_scene_tree") as SceneTree
	if runner_tree != null:
		tree.root.add_child.call_deferred(_wrapper)
	else:
		tree.root.add_child(_wrapper)
	_main_app = _main_app_scene.instantiate() as MainApp
	_wrapper.add_child(_main_app)
	_main_app._ready()
	_main_app.start_galaxy_with_defaults()


func after_each() -> void:
	if _main_app != null and is_instance_valid(_main_app):
		var p: Node = _main_app.get_parent()
		if p != null:
			p.remove_child(_main_app)
		_main_app.free()
	_main_app = null
	if _wrapper != null and is_instance_valid(_wrapper):
		var tree: SceneTree = Engine.get_main_loop() as SceneTree
		if tree != null and tree.root != null and _wrapper.get_parent() != null:
			tree.root.remove_child(_wrapper)
		_wrapper.free()
	_wrapper = null
	if FileAccess.file_exists(TEST_SAVE_PATH):
		DirAccess.remove_absolute(TEST_SAVE_PATH)


func test_startup_seed_is_positive() -> void:
	if _skip_tests:
		return
	assert_greater_than(_main_app.get_galaxy_seed(), 0,
		"Galaxy seed should be positive")


func test_startup_seed_is_bounded() -> void:
	if _skip_tests:
		return
	assert_less_than(_main_app.get_galaxy_seed(), 1000000,
		"Galaxy seed should be less than 1000000")


func test_galaxy_viewer_receives_startup_seed() -> void:
	if _skip_tests:
		return
	var main_seed: int = _main_app.get_galaxy_seed()
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "Should have galaxy viewer")
	assert_equal(viewer.galaxy_seed, main_seed,
		"Galaxy viewer seed should match MainApp seed")


func test_save_preserves_seed() -> void:
	if _skip_tests:
		return
	var original_seed: int = _main_app.get_galaxy_seed()
	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var save_data: GalaxySaveData = galaxy_viewer.get_save_data()
	assert_equal(save_data.galaxy_seed, original_seed,
		"Save data should contain the current seed")


func test_load_restores_seed_to_galaxy_viewer() -> void:
	if _skip_tests:
		return
	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var save_data: GalaxySaveData = GalaxySaveData.create(0)
	save_data.galaxy_seed = 12345
	save_data.zoom_level = GalaxyCoordinates.ZoomLevel.SUBSECTOR
	galaxy_viewer.apply_save_data(save_data)
	assert_equal(galaxy_viewer.galaxy_seed, 12345,
		"Galaxy viewer seed should be updated after load")


func test_load_updates_mainapp_seed() -> void:
	if _skip_tests:
		return
	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var save_data: GalaxySaveData = GalaxySaveData.create(0)
	save_data.galaxy_seed = 54321
	save_data.zoom_level = GalaxyCoordinates.ZoomLevel.SUBSECTOR
	galaxy_viewer.apply_save_data(save_data)
	assert_equal(_main_app.get_galaxy_seed(), 54321,
		"MainApp seed should be updated after load via signal")


func test_determinism_same_seed_same_spec() -> void:
	if _skip_tests:
		return
	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var save_data: GalaxySaveData = GalaxySaveData.create(0)
	save_data.galaxy_seed = 99999
	save_data.zoom_level = GalaxyCoordinates.ZoomLevel.GALAXY
	galaxy_viewer.apply_save_data(save_data)
	var spec1: GalaxySpec = galaxy_viewer.get_spec()
	galaxy_viewer.apply_save_data(save_data)
	var spec2: GalaxySpec = galaxy_viewer.get_spec()
	assert_equal(spec1.galaxy_seed, spec2.galaxy_seed, "Spec seed should match")
	assert_equal(spec1.radius_pc, spec2.radius_pc, "Spec radius should match")
	assert_equal(spec1.num_arms, spec2.num_arms, "Spec arms should match")


func test_save_load_round_trip_file() -> void:
	if _skip_tests:
		return
	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var original_data: GalaxySaveData = GalaxySaveData.create(0)
	original_data.galaxy_seed = 77777
	original_data.zoom_level = GalaxyCoordinates.ZoomLevel.SECTOR
	original_data.selected_quadrant = Vector3i(1, 0, 2)
	galaxy_viewer.apply_save_data(original_data)
	var save_data: GalaxySaveData = galaxy_viewer.get_save_data()
	var err: String = GalaxyPersistence.save_binary(TEST_SAVE_PATH, save_data)
	assert_equal(err, "", "Save should succeed")
	var different_data: GalaxySaveData = GalaxySaveData.create(0)
	different_data.galaxy_seed = 11111
	different_data.zoom_level = GalaxyCoordinates.ZoomLevel.GALAXY
	galaxy_viewer.apply_save_data(different_data)
	assert_equal(galaxy_viewer.galaxy_seed, 11111, "Seed should be changed")
	var loaded_data: GalaxySaveData = GalaxyPersistence.load_binary(TEST_SAVE_PATH)
	assert_not_null(loaded_data, "Should load data")
	galaxy_viewer.apply_save_data(loaded_data)
	assert_equal(galaxy_viewer.galaxy_seed, 77777,
		"Seed should be restored from file")
	assert_equal(_main_app.get_galaxy_seed(), 77777,
		"MainApp seed should be restored from file")
