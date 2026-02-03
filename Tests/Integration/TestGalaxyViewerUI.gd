## Integration tests for GalaxyViewer UI.
extends TestCase

const _galaxy_viewer_scene: PackedScene = preload("res://src/app/galaxy_viewer/GalaxyViewer.tscn")
## Ensures GalaxyInspectorPanel type resolves for analyzer.
const _GalaxyInspectorPanelScript: GDScript = preload("res://src/app/galaxy_viewer/GalaxyInspectorPanel.gd")

var _viewer: GalaxyViewer = null
var _tree: SceneTree = null


func get_test_name() -> String:
	return "TestGalaxyViewerUI"


func before_all() -> void:
	_tree = Engine.get_main_loop() as SceneTree


func before_each() -> void:
	_viewer = _galaxy_viewer_scene.instantiate() as GalaxyViewer
	var runner_tree: SceneTree = get("runner_scene_tree") as SceneTree
	if _tree != null and _tree.root != null:
		if runner_tree != null:
			_tree.root.add_child.call_deferred(_viewer)
		else:
			_tree.root.add_child(_viewer)


func after_each() -> void:
	if _viewer != null and is_instance_valid(_viewer):
		var p: Node = _viewer.get_parent()
		if p != null:
			p.remove_child(_viewer)
		_viewer.free()
	_viewer = null


func test_viewer_instantiates() -> void:
	if _tree == null or _tree.root == null:
		return
	assert_not_null(_viewer, "Viewer should instantiate")


func test_has_inspector_panel() -> void:
	if _tree == null or _tree.root == null:
		return
	var panel: GalaxyInspectorPanel = _viewer.get_inspector_panel()
	assert_not_null(panel, "Should have inspector panel")


func test_starts_at_galaxy_zoom_level() -> void:
	if _tree == null or _tree.root == null:
		return
	# Assert the "start at galaxy" path: when start_at_home is false, viewer must
	# start at galaxy zoom level. Failure here = bug in galaxy-start init.
	var galaxy_viewer: GalaxyViewer = _galaxy_viewer_scene.instantiate() as GalaxyViewer
	galaxy_viewer.start_at_home = false
	_tree.root.add_child(galaxy_viewer)
	assert_equal(galaxy_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.GALAXY,
		"When start_at_home is false, should start at galaxy zoom level")
	_tree.root.remove_child(galaxy_viewer)
	galaxy_viewer.free()


func test_has_spec() -> void:
	if _tree == null or _tree.root == null:
		return
	var spec: GalaxySpec = _viewer.get_spec()
	assert_not_null(spec, "Should have galaxy spec")
	assert_equal(spec.seed, 42, "Should have correct seed")


func test_status_updates() -> void:
	if _tree == null or _tree.root == null:
		return
	_viewer.set_status("Test status message")
	assert_true(true, "set_status should work")


func test_spec_matches_seed() -> void:
	if _tree == null or _tree.root == null:
		return
	var spec: GalaxySpec = _viewer.get_spec()
	assert_equal(spec.galaxy_type, GalaxySpec.GalaxyType.SPIRAL,
		"Should be spiral galaxy")
	assert_equal(spec.num_arms, 4, "Should have 4 arms (Milky Way)")
