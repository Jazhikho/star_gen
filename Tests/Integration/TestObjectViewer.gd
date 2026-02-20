## Integration tests for the ObjectViewer scene.
extends TestCase

const _object_viewer_scene: PackedScene = preload("res://src/app/viewer/ObjectViewer.tscn")
const _phase1_deps: GDScript = preload("res://Tests/Phase1Deps.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _camera_controller: GDScript = preload("res://src/app/viewer/CameraController.gd")


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
	
	# Test that camera can be focused. Distance after focus depends on min_distance (body size);
	# _fit_camera sets min_distance from body radius, so we only assert distance is in a sensible range.
	if camera is CameraController:
		var controller: CameraController = camera as CameraController
		controller.focus_on_target()
		for _i in range(120):
			await scene_tree.process_frame
		var dist: float = controller.get_distance()
		assert_true(dist >= 1.0 and dist <= 100.0, "Camera should be at a sensible distance after focus (got %f)" % dist)
	
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
	
	viewer.set_error("Test error", true)
	assert_true(viewer.status_label.text.contains("Error"), "Should show error prefix")
	
	# Clean up
	viewer.queue_free()


## Tests that generate button creates objects.
func test_generate_button_creates_objects() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Should have auto-generated an object on ready
	assert_not_null(viewer.current_body, "Should have generated initial object")
	
	# Try generating each type
	viewer.generate_object(viewer.ObjectType.STAR, 12345)
	await scene_tree.process_frame
	assert_equal(viewer.current_body.type, CelestialType.Type.STAR, "Should generate star")
	
	viewer.generate_object(viewer.ObjectType.PLANET, 23456)
	await scene_tree.process_frame
	assert_equal(viewer.current_body.type, CelestialType.Type.PLANET, "Should generate planet")
	
	viewer.generate_object(viewer.ObjectType.MOON, 34567)
	await scene_tree.process_frame
	assert_equal(viewer.current_body.type, CelestialType.Type.MOON, "Should generate moon")
	
	viewer.generate_object(viewer.ObjectType.ASTEROID, 45678)
	await scene_tree.process_frame
	assert_equal(viewer.current_body.type, CelestialType.Type.ASTEROID, "Should generate asteroid")
	
	# Clean up
	viewer.queue_free()


## Tests that different object types have appropriate scales.
func test_object_scaling() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Generate a star
	viewer.generate_object(viewer.ObjectType.STAR, 11111)
	await scene_tree.process_frame
	var star_scale: Vector3 = viewer.body_renderer.body_mesh.scale
	
	# Generate a planet
	viewer.generate_object(viewer.ObjectType.PLANET, 22222)
	await scene_tree.process_frame
	var planet_scale: Vector3 = viewer.body_renderer.body_mesh.scale
	
	# Generate an asteroid
	viewer.generate_object(viewer.ObjectType.ASTEROID, 33333)
	await scene_tree.process_frame
	var asteroid_scale: Vector3 = viewer.body_renderer.body_mesh.scale
	
	# Scales should be reasonable (not zero, not huge)
	assert_true(star_scale.x > 0.0 and star_scale.x < 10.0, "Star scale should be reasonable")
	assert_true(planet_scale.x > 0.0 and planet_scale.x < 10.0, "Planet scale should be reasonable")
	assert_true(asteroid_scale.x > 0.0 and asteroid_scale.x < 10.0, "Asteroid scale should be reasonable")
	
	# Clean up
	viewer.queue_free()


## Tests that inspector panel displays properties when objects are generated.
func test_info_labels_update() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Generate a planet
	viewer.generate_object(viewer.ObjectType.PLANET, 12345)
	await scene_tree.process_frame
	
	# Check that inspector panel has content
	assert_not_null(viewer.inspector_panel, "Should have inspector panel")
	
	# Inspector should have created child nodes for the properties
	var inspector_container: VBoxContainer = viewer.inspector_panel.get_node_or_null("InspectorContainer")
	assert_not_null(inspector_container, "Should have inspector container")
	assert_true(inspector_container.get_child_count() > 0, "Inspector should have property sections")
	
	# Clean up
	viewer.queue_free()


## Tests that inspector displays all component sections.
func test_inspector_shows_all_sections() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Generate a planet (has physical, orbital, possibly atmosphere/surface)
	viewer.generate_object(viewer.ObjectType.PLANET, 55555)
	await scene_tree.process_frame
	
	# The inspector should have created multiple sections
	var inspector_container: VBoxContainer = viewer.inspector_panel.get_node_or_null("InspectorContainer")
	assert_not_null(inspector_container, "Should have inspector container")
	
	# Count section headers (VBoxContainers with Button children)
	var section_count: int = 0
	for child in inspector_container.get_children():
		if child is VBoxContainer:
			section_count += 1
	
	# Should have at least Basic Info, Physical, and Orbital sections
	assert_true(section_count >= 3, "Should have at least 3 sections (got %d)" % section_count)
	
	# Clean up
	viewer.queue_free()


## Tests that collapsible sections work.
func test_collapsible_sections() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Generate a planet
	viewer.generate_object(viewer.ObjectType.PLANET, 12345)
	await scene_tree.process_frame
	
	# Find the first section (Basic Info)
	var inspector_container: VBoxContainer = viewer.inspector_panel.get_node_or_null("InspectorContainer")
	assert_not_null(inspector_container, "Should have inspector container")
	
	if inspector_container.get_child_count() > 0:
		var first_section: VBoxContainer = inspector_container.get_child(0) as VBoxContainer
		if first_section and first_section.get_child_count() >= 2:
			var header: Button = first_section.get_child(0) as Button
			var content: VBoxContainer = first_section.get_child(1) as VBoxContainer
			
			# Content should be visible by default
			assert_true(content.visible, "Section content should be visible initially")
			
			# Click the header to collapse
			header.pressed.emit()
			await scene_tree.process_frame
			await scene_tree.process_frame
			
			assert_false(content.visible, "Section content should be hidden after toggle")
			assert_true(header.text.begins_with("▶"), "Header should show collapsed arrow")
			
			# Click again to expand
			header.pressed.emit()
			await scene_tree.process_frame
			
			assert_true(content.visible, "Section content should be visible after second toggle")
			assert_true(header.text.begins_with("▼"), "Header should show expanded arrow")
	
	# Clean up
	viewer.queue_free()


## Tests deterministic generation with same seed.
func test_deterministic_generation() -> void:
	var viewer: ObjectViewer = _object_viewer_scene.instantiate() as ObjectViewer
	
	# Add to tree deferred
	var scene_tree: SceneTree = _add_viewer_deferred(viewer)
	if not scene_tree:
		viewer.free()
		return
	
	# Wait for deferred call and ready
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Generate with specific seed
	viewer.generate_object(viewer.ObjectType.PLANET, 99999)
	await scene_tree.process_frame
	var first_mass: float = viewer.current_body.physical.mass_kg
	var first_radius: float = viewer.current_body.physical.radius_m
	
	# Generate again with same seed
	viewer.generate_object(viewer.ObjectType.PLANET, 99999)
	await scene_tree.process_frame
	var second_mass: float = viewer.current_body.physical.mass_kg
	var second_radius: float = viewer.current_body.physical.radius_m
	
	# Should be identical
	assert_equal(first_mass, second_mass, "Same seed should produce same mass")
	assert_equal(first_radius, second_radius, "Same seed should produce same radius")
	
	# Clean up
	viewer.queue_free()
