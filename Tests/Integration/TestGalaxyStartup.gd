## Integration tests for galaxy startup flow (welcome screen first, then galaxy).
extends TestCase

const _main_app_scene: PackedScene = preload("res://src/app/MainApp.tscn")

var _main_app: MainApp = null
var _wrapper: Node = null
var _skip: bool = false


func get_test_name() -> String:
	return "TestGalaxyStartup"


func before_each() -> void:
	_main_app = null
	_wrapper = null
	_skip = false
	var tree: SceneTree = Engine.get_main_loop() as SceneTree
	if tree == null or tree.root == null:
		_skip = true
		return
	_wrapper = Node.new()
	var runner_tree: SceneTree = get("runner_scene_tree") as SceneTree
	if runner_tree != null:
		tree.root.add_child.call_deferred(_wrapper)
	else:
		tree.root.add_child(_wrapper)
	_main_app = _main_app_scene.instantiate() as MainApp
	_wrapper.add_child(_main_app)
	_main_app._ready()


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


func test_after_ready_galaxy_viewer_is_null_until_started() -> void:
	if _skip:
		return
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_null(viewer, "Galaxy viewer should be null until user starts galaxy")


func test_start_galaxy_with_defaults_creates_viewer() -> void:
	if _skip:
		return
	_main_app.start_galaxy_with_defaults()
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "Should have galaxy viewer after start_galaxy_with_defaults")
	assert_equal(_main_app.get_active_viewer(), "galaxy",
		"Active viewer should be galaxy")


func test_start_galaxy_with_defaults_sets_positive_seed() -> void:
	if _skip:
		return
	_main_app.start_galaxy_with_defaults()
	assert_greater_than(_main_app.get_galaxy_seed(), 0,
		"Galaxy seed should be positive after start_galaxy_with_defaults")
