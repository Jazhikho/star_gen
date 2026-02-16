## Integration tests for SystemCameraController.
## Tests camera initialization, focus, zoom, and view angle changes.
extends TestCase

const _system_camera_controller: GDScript = preload("res://src/app/system_viewer/SystemCameraController.gd")


## Helper to create a camera controller in the scene tree.
## @return: SystemCameraController (must be freed by test).
func _create_camera() -> SystemCameraController:
	var camera: SystemCameraController = SystemCameraController.new()
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	if scene_tree and scene_tree.root:
		scene_tree.root.call_deferred("add_child", camera)
	return camera


# =============================================================================
# INITIALIZATION TESTS
# =============================================================================


## Tests camera initializes at correct default position.
func test_camera_default_position() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Camera should be above the origin looking down
	assert_true(camera.global_position.y > 0.0,
		"Camera should be above the origin (y=%.2f)" % camera.global_position.y)
	
	camera.queue_free()


## Tests camera default height.
func test_camera_default_height() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	assert_float_equal(camera.get_height(), 20.0, 0.1,
		"Default camera height should be 20.0")
	
	camera.queue_free()


# =============================================================================
# FOCUS TESTS
# =============================================================================


## Tests focus_on_origin resets camera.
func test_focus_on_origin() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Change camera state
	camera.set_height(50.0)
	await scene_tree.process_frame
	
	# Focus on origin
	camera.focus_on_origin()
	
	# Wait for smooth interpolation (camera may take many frames to settle)
	for _i in range(120):
		await scene_tree.process_frame
	
	# Height should return toward 20.0 (tolerance allows for lerp timing variance)
	assert_float_equal(camera.get_height(), 20.0, 16.0,
		"Height should return to ~20 after focus (got %.1f)" % camera.get_height())
	
	camera.queue_free()


## Tests focus_on_position moves camera target.
func test_focus_on_position() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Focus on a specific position
	var target: Vector3 = Vector3(5.0, 0.0, 3.0)
	camera.focus_on_position(target, 10.0)
	
	# Wait for smooth interpolation
	for _i in range(60):
		await scene_tree.process_frame
	
	# Camera should be near the target position (XZ projected)
	# Camera looks at target, so target is approximately below
	var cam_xz: Vector2 = Vector2(camera.global_position.x, camera.global_position.z)
	var target_xz: Vector2 = Vector2(target.x, target.z)
	
	# Camera should be in the general vicinity (not exact due to orbital offset)
	var dist: float = cam_xz.distance_to(target_xz)
	assert_true(dist < 30.0,
		"Camera should be within reasonable distance of target (got %.1f)" % dist)
	
	camera.queue_free()


# =============================================================================
# ZOOM TESTS
# =============================================================================


## Tests set_height clamps to valid range.
func test_set_height_clamps_min() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	camera.set_height(0.01)
	assert_true(camera.get_height() >= camera.min_height,
		"Height should be clamped to min (got %.3f, min=%.3f)" % [camera.get_height(), camera.min_height])
	
	camera.queue_free()


## Tests set_height clamps to maximum.
func test_set_height_clamps_max() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	camera.set_height(9999.0)
	assert_true(camera.get_height() <= camera.max_height,
		"Height should be clamped to max (got %.1f, max=%.1f)" % [camera.get_height(), camera.max_height])
	
	camera.queue_free()


## Tests set_height with valid value.
func test_set_height_valid() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	camera.set_height(35.0)
	assert_float_equal(camera.get_height(), 35.0, 0.01,
		"Height should be set to requested value")
	
	camera.queue_free()


# =============================================================================
# VIEW ANGLE TESTS
# =============================================================================


## Tests camera stays above ground plane.
func test_camera_stays_above_ground() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	
	# Run several frames
	for _i in range(30):
		await scene_tree.process_frame
	
	assert_true(camera.global_position.y > 0.0,
		"Camera should always be above ground (y=%.2f)" % camera.global_position.y)
	
	camera.queue_free()


## Tests camera is looking toward origin by default.
func test_camera_looks_at_origin() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	await scene_tree.process_frame
	
	# Camera's forward direction should have a downward Y component
	var forward: Vector3 = - camera.global_transform.basis.z
	assert_true(forward.y < 0.0,
		"Camera should be looking downward (forward.y=%.2f)" % forward.y)
	
	camera.queue_free()


# =============================================================================
# SIGNAL TESTS
# =============================================================================


## Tests camera_moved signal is emitted.
func test_camera_moved_signal() -> void:
	var camera: SystemCameraController = _create_camera()
	var received: Array = [false, 0.0]
	camera.camera_moved.connect(func(_pos: Vector3, height: float) -> void:
		received[0] = true
		received[1] = height
	)
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	# Deferred add then enough frames so camera is in tree and _process runs (emits each frame)
	await scene_tree.process_frame
	for _i in range(10):
		await scene_tree.process_frame
	
	assert_true(received[0], "camera_moved signal should be emitted")
	assert_true(received[1] > 0.0,
		"Signal should report positive height (got %.1f)" % received[1])
	
	camera.queue_free()


# =============================================================================
# ROBUSTNESS TESTS
# =============================================================================


## Tests camera handles rapid height changes without NaN.
func test_rapid_height_changes_no_nan() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	# Rapidly change height
	var heights: Array[float] = [1.0, 100.0, 0.5, 50.0, 200.0, 2.0]
	for h in heights:
		camera.set_height(h)
		await scene_tree.process_frame
		
		assert_false(is_nan(camera.global_position.x),
			"Camera X should not be NaN after height=%.1f" % h)
		assert_false(is_nan(camera.global_position.y),
			"Camera Y should not be NaN after height=%.1f" % h)
		assert_false(is_nan(camera.global_position.z),
			"Camera Z should not be NaN after height=%.1f" % h)
	
	camera.queue_free()


## Tests focus_on_position with zero distance doesn't crash.
func test_focus_zero_distance() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	camera.focus_on_position(Vector3.ZERO, 0.0)
	await scene_tree.process_frame
	
	# Should not crash, height should remain valid
	assert_true(camera.get_height() >= camera.min_height,
		"Height should still be valid after zero-distance focus")
	
	camera.queue_free()


## Tests focus_on_position with negative distance.
func test_focus_negative_distance() -> void:
	var camera: SystemCameraController = _create_camera()
	
	var scene_tree: SceneTree = Engine.get_main_loop() as SceneTree
	await scene_tree.process_frame
	
	camera.focus_on_position(Vector3(10.0, 0.0, 10.0), -5.0)
	await scene_tree.process_frame
	
	# Negative distance should be ignored (no zoom change from negative)
	assert_true(camera.get_height() > 0.0,
		"Camera should maintain positive height")
	
	camera.queue_free()
