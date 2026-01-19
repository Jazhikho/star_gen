## Integration tests for the ObjectViewer scene.
extends TestCase

const _object_viewer_scene := preload("res://src/app/viewer/ObjectViewer.tscn")
const _phase1_deps := preload("res://Tests/Phase1Deps.gd")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _camera_controller := preload("res://src/app/viewer/CameraController.gd")


## Helper to safely add a viewer to the tree using deferred call.
## @param viewer: The viewer to add.
## @return: The scene tree, or null if unavailable.
func _add_viewer_deferred(viewer: Node) -> SceneTree:
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	if scene_tree and scene_tree.root:
		scene_tree.root.call_deferred("add_child", viewer)
		return scene_tree
	return null


## Tests that the viewer scene can be instantiated.
func test_viewer_scene_instantiates() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	assert_not_null(viewer, "Should instantiate ObjectViewer scene")
	
	# Clean up (no tree addition needed for this test)
	if viewer:
		viewer.free()


## Tests that the viewer scene can be added to tree and run one frame.
func test_viewer_runs_one_frame() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	assert_not_null(viewer, "Should instantiate ObjectViewer scene")
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		# Fallback: just check instantiation works
		assert_not_null(viewer, "Viewer should instantiate")
		viewer.free()
		return
	
	# Wait for deferred call to execute
	await scene_tree.process_frame
	# Wait one more frame for _ready() to complete
	await scene_tree.process_frame
	
	# Check that it's ready
	assert_true(viewer.is_ready, "Viewer should be ready after frames")
	
	# Clean up
	viewer.queue_free()


## Tests that viewer can display a body without crashing.
func test_viewer_displays_body() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Create a test body
	var physical: PhysicalProps = PhysicalProps.new(
		Units.EARTH_MASS_KG,
		Units.EARTH_RADIUS_METERS,
		86400.0,
		23.5,
		0.003,
		8.0e22,
		4.7e13
	)
	
	var body: CelestialBody = CelestialBody.new(
		"test_planet",
		"Test Planet",
		CelestialType.Type.PLANET,
		physical,
		null
	)
	
	# Display it
	viewer.display_body(body)
	
	# Wait a frame
	await scene_tree.process_frame
	
	# Check that body is set
	assert_equal(viewer.current_body, body, "Should have current body set")
	
	# Clean up
	viewer.queue_free()


## Tests that viewer handles null body gracefully.
func test_viewer_handles_null_body() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Try to display null
	viewer.display_body(null)
	
	# Should not crash, should show error
	await scene_tree.process_frame
	
	assert_null(viewer.current_body, "Should not have current body")
	
	# Clean up
	viewer.queue_free()


## Tests that camera controller exists and responds to input.
func test_camera_controller_exists() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	var camera: Camera3D = viewer.camera
	assert_not_null(camera, "Should have camera")
	assert_true(camera.has_method("focus_on_target"), "Camera should have controller script")
	
	# Test that camera can be focused
	if camera is CameraController:
		var controller: CameraController = camera as CameraController
		controller.focus_on_target()
		assert_equal(controller.get_distance(), 10.0, "Camera should reset to default distance")
	
	# Clean up
	viewer.queue_free()


## Tests that UI elements exist.
func test_ui_elements_exist() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	assert_not_null(viewer.status_label, "Should have status label")
	assert_not_null(viewer.side_panel, "Should have side panel")
	assert_not_null(viewer.panel_container, "Should have panel container")
	
	# Clean up
	viewer.queue_free()


## Tests status message setting.
func test_status_messages() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	viewer.set_status("Test status")
	assert_equal(viewer.status_label.text, "Test status", "Should set status text")
	
	viewer.set_error("Test error")
	assert_true(viewer.status_label.text.contains("Error"), "Should show error prefix")
	
	# Clean up
	viewer.queue_free()
