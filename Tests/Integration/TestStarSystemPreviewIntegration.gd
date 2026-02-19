## Integration tests for the star-click preview flow.
## Verifies that selecting a star in GalaxyViewer generates a preview and
## that the cached system is available for open_system_requested.
class_name TestStarSystemPreviewIntegration
extends TestCase

const _main_app_scene: PackedScene = preload("res://src/app/MainApp.tscn")
const _galaxy_coordinates: GDScript = preload("res://src/domain/galaxy/GalaxyCoordinates.gd")

var _main_app: MainApp = null
var _wrapper: Node = null
var _skip: bool = false


func get_test_name() -> String:
	return "TestStarSystemPreviewIntegration"


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


func test_no_preview_before_star_click() -> void:
	if _skip:
		return
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "GalaxyViewer must exist")
	assert_null(
		viewer.get_star_preview(),
		"No preview should exist before any star is clicked"
	)


func test_preview_generated_after_simulate_star_select() -> void:
	if _skip:
		return
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "GalaxyViewer must exist")
	assert_not_null(viewer.get_zoom_machine(), "ZoomStateMachine must be initialized")
	var spec: GalaxySpec = viewer.get_spec()
	assert_not_null(spec, "GalaxySpec must be available")

	var fake_seed: int = 99991
	var fake_pos: Vector3 = Vector3(8000.0, 0.0, 0.0)
	viewer.simulate_star_selected(fake_seed, fake_pos)

	var preview: StarSystemPreview.PreviewData = viewer.get_star_preview()
	assert_not_null(preview, "Preview should be generated after star selected")
	if preview == null:
		return
	assert_equal(preview.star_seed, fake_seed, "Preview seed should match selected star")
	assert_true(preview.star_count >= 1, "Preview must have at least one star")


func test_preview_cleared_on_empty_click() -> void:
	if _skip:
		return
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "GalaxyViewer must exist")
	assert_not_null(viewer.get_zoom_machine(), "ZoomStateMachine must be initialized")

	viewer.simulate_star_selected(99992, Vector3(8000.0, 0.0, 0.0))
	var preview_before: StarSystemPreview.PreviewData = viewer.get_star_preview()
	assert_not_null(preview_before, "Preview should exist after select")

	viewer.simulate_star_deselected()
	assert_null(viewer.get_star_preview(), "Preview should be cleared after deselect")


func test_open_system_requested_emits_with_correct_seed() -> void:
	if _skip:
		return
	var viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(viewer, "GalaxyViewer must exist")
	assert_not_null(viewer.get_zoom_machine(), "ZoomStateMachine must be initialized")

	var fake_seed: int = 99993
	var fake_pos: Vector3 = Vector3(8000.0, 0.0, 0.0)
	viewer.simulate_star_selected(fake_seed, fake_pos)

	var preview: StarSystemPreview.PreviewData = viewer.get_star_preview()
	assert_not_null(preview, "Preview must exist before opening system")
	if preview == null:
		return

	# Use array so callback can mutate and test can read (avoids lambda capture reassignment).
	var received: Array = [0, Vector3.ZERO]
	var callback: Callable = func(s: int, p: Vector3) -> void:
		received[0] = s
		received[1] = p
	viewer.open_system_requested.connect(callback)
	# Force subsector level so _try_open_selected_system passes its level guard.
	viewer.get_zoom_machine().set_level(GalaxyCoordinates.ZoomLevel.SUBSECTOR)
	viewer.simulate_open_selected_system()

	assert_equal(received[0], fake_seed, "open_system_requested should fire with correct seed")
	assert_equal(received[1], fake_pos, "open_system_requested should fire with correct position")
	if viewer.open_system_requested.is_connected(callback):
		viewer.open_system_requested.disconnect(callback)
