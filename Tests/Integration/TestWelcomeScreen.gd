## Integration tests for WelcomeScreen (startup screen).
extends TestCase

const _welcome_screen_scene: PackedScene = preload("res://src/app/WelcomeScreen.tscn")
const _SeededRngClass: GDScript = preload("res://src/domain/rng/SeededRng.gd")

var _welcome: Control = null
var _wrapper: Node = null
var _skip: bool = false


func get_test_name() -> String:
	return "TestWelcomeScreen"


func before_each() -> void:
	_welcome = null
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
	_wrapper.add_child(_welcome_screen_scene.instantiate())
	_welcome = _wrapper.get_child(0) as Control


func after_each() -> void:
	if _welcome != null and is_instance_valid(_welcome):
		var p: Node = _welcome.get_parent()
		if p != null:
			p.remove_child(_welcome)
		_welcome.free()
	_welcome = null
	if _wrapper != null and is_instance_valid(_wrapper):
		var tree: SceneTree = Engine.get_main_loop() as SceneTree
		if tree != null and tree.root != null and _wrapper.get_parent() != null:
			tree.root.remove_child(_wrapper)
		_wrapper.free()
	_wrapper = null


func test_welcome_screen_instantiates() -> void:
	if _skip:
		return
	assert_not_null(_welcome, "Welcome screen should instantiate")


func test_get_current_config_returns_valid_config() -> void:
	if _skip:
		return
	if not _welcome.has_method("get_current_config"):
		return
	var config: GalaxyConfig = _welcome.get_current_config()
	assert_not_null(config, "Should return config")
	assert_true(config.is_valid(), "Config should be valid")


func test_set_seeded_rng_accepts_rng() -> void:
	if _skip:
		return
	if not _welcome.has_method("set_seeded_rng"):
		return
	var rng: RefCounted = _SeededRngClass.new(42)
	_welcome.set_seeded_rng(rng)
	# No assert; we only check it doesn't crash
