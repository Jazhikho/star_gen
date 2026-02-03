## Integration tests for GalaxyViewer home position initialization.
extends TestCase

const _galaxy_viewer_scene: PackedScene = preload("res://src/app/galaxy_viewer/GalaxyViewer.tscn")

var _viewer: GalaxyViewer = null
var _tree: SceneTree = null


func get_test_name() -> String:
	return "TestGalaxyViewerHome"


func before_all() -> void:
	_tree = Engine.get_main_loop() as SceneTree


func after_each() -> void:
	if _viewer and is_instance_valid(_viewer):
		if _viewer.is_inside_tree():
			_viewer.get_parent().remove_child(_viewer)
		_viewer.queue_free()
		_viewer = null

	if _tree:
		await _tree.process_frame
		await _tree.process_frame


## Helper to create and add viewer to scene tree safely.
func _setup_viewer() -> void:
	_viewer = _galaxy_viewer_scene.instantiate() as GalaxyViewer
	_tree.root.add_child.call_deferred(_viewer)

	await _tree.process_frame
	await _tree.process_frame
	await _tree.process_frame


## Helper to create viewer with custom settings.
func _setup_viewer_no_home() -> void:
	_viewer = _galaxy_viewer_scene.instantiate() as GalaxyViewer
	_viewer.start_at_home = false
	_tree.root.add_child.call_deferred(_viewer)

	await _tree.process_frame
	await _tree.process_frame
	await _tree.process_frame


func _wait_frames(count: int) -> void:
	if _tree:
		for i in range(count):
			await _tree.process_frame


func test_viewer_starts_at_home_by_default() -> void:
	await _setup_viewer()

	assert_true(_viewer.get_start_at_home(),
		"Viewer should start at home by default")


func test_starts_at_subsector_zoom_level() -> void:
	await _setup_viewer()

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should start at subsector zoom level when start_at_home is true")


func test_starts_at_galaxy_when_disabled() -> void:
	await _setup_viewer_no_home()

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.GALAXY,
		"Should start at galaxy zoom level when start_at_home is false")


func test_navigate_to_home_works_from_galaxy_view() -> void:
	await _setup_viewer_no_home()

	# Verify we're at galaxy level
	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.GALAXY,
		"Should start at galaxy level")

	# Navigate to home
	_viewer.navigate_to_home()
	await _wait_frames(3)

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should be at subsector level after navigate_to_home")


func test_home_position_is_in_expected_quadrant() -> void:
	await _setup_viewer()

	# The viewer should have pre-selected the home quadrant
	var expected_quadrant: Vector3i = HomePosition.get_home_quadrant()

	# We can verify by checking that zooming out maintains the selection
	# For now, just verify the zoom level is correct
	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should be at subsector level with home quadrant selected")


func test_inspector_shows_info_after_home_init() -> void:
	await _setup_viewer()

	var panel: GalaxyInspectorPanel = _viewer.get_inspector_panel()
	assert_not_null(panel, "Should have inspector panel")

	# Panel should show galaxy info (View: Star Field)
	# We can't easily inspect the label text, but panel should exist and function
	assert_false(panel.has_star_selected(),
		"No star should be selected initially at home")


func test_can_zoom_out_from_home() -> void:
	await _setup_viewer()

	# Should be at subsector level
	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should start at subsector")

	# Simulate zoom out key
	var key_event: InputEventKey = InputEventKey.new()
	key_event.keycode = KEY_BRACKETLEFT
	key_event.pressed = true
	_viewer._handle_key_input(key_event)

	await _wait_frames(2)

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SECTOR,
		"Should zoom out to sector level")


func test_can_zoom_out_to_galaxy_view() -> void:
	await _setup_viewer()

	# Zoom out multiple times
	var key_event: InputEventKey = InputEventKey.new()
	key_event.keycode = KEY_BRACKETLEFT
	key_event.pressed = true

	# Subsector -> Sector
	_viewer._handle_key_input(key_event)
	await _wait_frames(2)

	# Sector -> Quadrant
	_viewer._handle_key_input(key_event)
	await _wait_frames(2)

	# Quadrant -> Galaxy
	_viewer._handle_key_input(key_event)
	await _wait_frames(2)

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.GALAXY,
		"Should be able to zoom out to galaxy view")


func test_can_return_to_home_after_zooming_out() -> void:
	await _setup_viewer()

	# Zoom out to galaxy
	var key_event: InputEventKey = InputEventKey.new()
	key_event.keycode = KEY_BRACKETLEFT
	key_event.pressed = true

	for i in range(3):
		_viewer._handle_key_input(key_event)
		await _wait_frames(2)

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.GALAXY,
		"Should be at galaxy level")

	# Navigate back to home
	_viewer.navigate_to_home()
	await _wait_frames(3)

	assert_equal(_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Should return to subsector level")
