## Camera controller for the solar system viewer.
## Provides top-down orbital view with smooth zoom and pan.
## Optimized for viewing orbital planes from above.
class_name SystemCameraController
extends Camera3D

const _units: GDScript = preload("res://src/domain/math/Units.gd")


## Zoom speed (scroll sensitivity).
@export var zoom_speed: float = 0.15

## Pan speed multiplier.
@export var pan_speed: float = 1.0

## Orbit speed multiplier for rotation.
@export var orbit_speed: float = 2.0

## Zoom smoothing factor.
@export var zoom_smooth: float = 5.0

## Movement smoothing factor.
@export var move_smooth: float = 8.0

## Minimum camera height (closest zoom).
@export var min_height: float = 0.5

## Maximum camera height (farthest zoom).
@export var max_height: float = 200.0

## Minimum pitch angle in degrees (how far down the camera can look).
@export var min_pitch_deg: float = 10.0

## Maximum pitch angle in degrees (straight down).
@export var max_pitch_deg: float = 89.0

## Current camera height (distance from orbital plane).
var _height: float = 20.0

## Target height for smooth zoom.
var _target_height: float = 20.0

## Current look-at target position (in XZ plane).
var _target_position: Vector3 = Vector3.ZERO

## Smooth target position.
var _smooth_target: Vector3 = Vector3.ZERO

## Camera yaw (rotation around Y axis) in radians.
var _yaw: float = 0.0

## Camera pitch (angle from horizontal) in radians.
var _pitch: float = deg_to_rad(60.0)

## Target pitch for smooth transitions.
var _target_pitch: float = deg_to_rad(60.0)

## Input state tracking.
var _orbiting: bool = false
var _panning: bool = false
var _last_mouse_position: Vector2 = Vector2.ZERO

## Signal emitted when camera moves (for LOD updates).
signal camera_moved(position: Vector3, height: float)


func _ready() -> void:
	_update_transform()


func _input(event: InputEvent) -> void:
	# Don't process input if mouse is over UI
	if _is_mouse_over_ui():
		if event is InputEventMouseMotion:
			_orbiting = false
			_panning = false
			return
		elif event is InputEventMouseButton:
			var btn: InputEventMouseButton = event as InputEventMouseButton
			if btn.pressed:
				_orbiting = false
				_panning = false
				return
	
	if event is InputEventMouseButton:
		var mouse_event: InputEventMouseButton = event as InputEventMouseButton
		
		match mouse_event.button_index:
			MOUSE_BUTTON_LEFT:
				_orbiting = mouse_event.pressed
				_last_mouse_position = mouse_event.position
			
			MOUSE_BUTTON_RIGHT:
				_panning = mouse_event.pressed
				_last_mouse_position = mouse_event.position
			
			MOUSE_BUTTON_MIDDLE:
				_orbiting = mouse_event.pressed
				_last_mouse_position = mouse_event.position
			
			MOUSE_BUTTON_WHEEL_UP:
				if _is_mouse_over_ui():
					return
				_target_height *= (1.0 - zoom_speed)
				_target_height = clampf(_target_height, min_height, max_height)
			
			MOUSE_BUTTON_WHEEL_DOWN:
				if _is_mouse_over_ui():
					return
				_target_height *= (1.0 + zoom_speed)
				_target_height = clampf(_target_height, min_height, max_height)
	
	elif event is InputEventMouseMotion:
		var motion_event: InputEventMouseMotion = event as InputEventMouseMotion
		
		if _is_mouse_over_ui():
			_orbiting = false
			_panning = false
			return
		
		var delta: Vector2 = motion_event.position - _last_mouse_position
		
		if _orbiting:
			_yaw -= delta.x * orbit_speed * 0.005
			_target_pitch += delta.y * orbit_speed * 0.005
			_target_pitch = clampf(
				_target_pitch,
				deg_to_rad(min_pitch_deg),
				deg_to_rad(max_pitch_deg)
			)
		
		elif _panning:
			# Pan in the camera's local XZ plane
			var pan_scale: float = _height * pan_speed * 0.002
			var pan_x: float = -delta.x * pan_scale
			var pan_z: float = -delta.y * pan_scale
			
			# Rotate pan direction by camera yaw
			_target_position.x += pan_x * cos(_yaw) + pan_z * sin(_yaw)
			_target_position.z += -pan_x * sin(_yaw) + pan_z * cos(_yaw)
		
		_last_mouse_position = motion_event.position
	
	elif event is InputEventKey:
		var key_event: InputEventKey = event as InputEventKey
		if key_event.pressed:
			match key_event.keycode:
				KEY_F:
					focus_on_origin()
				KEY_T:
					# Toggle between top-down and angled view
					_toggle_view_angle()


func _process(delta: float) -> void:
	# Smooth interpolation
	_height = lerpf(_height, _target_height, zoom_smooth * delta)
	_pitch = lerpf(_pitch, _target_pitch, move_smooth * delta)
	_smooth_target = _smooth_target.lerp(_target_position, move_smooth * delta)
	
	_update_transform()
	
	# Dynamic clip planes based on zoom level
	near = maxf(0.001, _height * 0.001)
	far = maxf(100.0, _height * 50.0)
	
	camera_moved.emit(global_position, _height)


## Updates the camera transform based on current state.
func _update_transform() -> void:
	# Calculate camera position
	var horizontal_dist: float = _height / tan(_pitch)
	
	var cam_offset: Vector3 = Vector3(
		horizontal_dist * sin(_yaw),
		_height,
		horizontal_dist * cos(_yaw)
	)
	
	global_position = _smooth_target + cam_offset
	look_at(_smooth_target, Vector3.UP)


## Focuses the camera on the origin (center of system).
func focus_on_origin() -> void:
	_target_position = Vector3.ZERO
	_target_height = 20.0
	_target_pitch = deg_to_rad(60.0)
	_yaw = 0.0


## Focuses on a specific position at an appropriate zoom level.
## @param target: The world position to focus on.
## @param zoom_to_distance: Optional distance to zoom to (in viewport units).
func focus_on_position(target: Vector3, zoom_to_distance: float = -1.0) -> void:
	_target_position = Vector3(target.x, 0.0, target.z)
	if zoom_to_distance > 0.0:
		_target_height = clampf(zoom_to_distance * 1.5, min_height, max_height)


## Sets the camera height directly (for programmatic control).
## @param height: Height in viewport units.
func set_height(height: float) -> void:
	_height = clampf(height, min_height, max_height)
	_target_height = _height


## Gets the current camera height.
## @return: Height in viewport units.
func get_height() -> float:
	return _height


## Toggles between top-down and angled view.
func _toggle_view_angle() -> void:
	if _target_pitch > deg_to_rad(70.0):
		_target_pitch = deg_to_rad(30.0)
	else:
		_target_pitch = deg_to_rad(80.0)


## Checks if the mouse is over a UI element.
## @return: True if mouse is over UI.
func _is_mouse_over_ui() -> bool:
	var viewport: Viewport = get_viewport()
	if not viewport:
		return false
	var ui_control: Control = viewport.gui_get_hovered_control()
	return ui_control != null
