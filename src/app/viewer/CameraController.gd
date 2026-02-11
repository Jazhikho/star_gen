## Orbital camera controller for the object viewer.
## Handles mouse input for orbiting, panning, and zooming.
class_name CameraController
extends Camera3D

## Speed multiplier for orbit rotation.
@export var orbit_speed: float = 2.0

## Speed multiplier for panning.
@export var pan_speed: float = 1.0

## Speed multiplier for zoom per scroll tick.
@export var zoom_speed: float = 0.1

## Smoothing factor for zoom interpolation.
@export var zoom_smooth: float = 5.0

## Minimum camera distance from target.
@export var min_distance: float = 0.5

## Maximum camera distance from target.
@export var max_distance: float = 100.0

## Minimum pitch angle in degrees.
@export var min_pitch: float = -89.0

## Maximum pitch angle in degrees.
@export var max_pitch: float = 89.0

## Current camera state
var _distance: float = 10.0
var _target_distance: float = 10.0
var _rotation: Vector2 = Vector2.ZERO # x = yaw, y = pitch
## World-space position the camera orbits around and looks at. Updated each frame when following a moon.
var _target_position: Vector3 = Vector3.ZERO

## Input state
var _orbiting: bool = false
var _panning: bool = false
var _last_mouse_position: Vector2 = Vector2.ZERO


## Checks if the mouse is currently over a UI element.
## @return: True if mouse is over UI.
func _is_mouse_over_ui() -> bool:
	var viewport: Viewport = get_viewport()
	if not viewport:
		return false
	
	# Check if any Control node is capturing the mouse
	var ui_control: Control = viewport.gui_get_hovered_control()
	return ui_control != null


func _ready() -> void:
	# Initialize camera position
	_update_camera_transform()


func _input(event: InputEvent) -> void:
	# Don't process 3D camera input if mouse is over UI
	if _is_mouse_over_ui():
		# If we're already orbiting/panning and mouse moves to UI, stop the operation
		if event is InputEventMouseMotion:
			_orbiting = false
			_panning = false
			return
		# Don't start new operations if mouse is over UI
		elif event is InputEventMouseButton:
			var mouse_event: InputEventMouseButton = event as InputEventMouseButton
			# Always allow button release to properly end drag operations
			if mouse_event.pressed:
				_orbiting = false
				_panning = false
				return
	
	# Mouse button press
	if event is InputEventMouseButton:
		var mouse_event: InputEventMouseButton = event as InputEventMouseButton
		
		match mouse_event.button_index:
			MOUSE_BUTTON_LEFT:
				_orbiting = mouse_event.pressed
				_last_mouse_position = mouse_event.position
			
			MOUSE_BUTTON_RIGHT:
				_panning = mouse_event.pressed
				_last_mouse_position = mouse_event.position
			
			MOUSE_BUTTON_WHEEL_UP:
				if _is_mouse_over_ui():
					return
				_target_distance *= (1.0 - zoom_speed)
				_target_distance = clampf(_target_distance, min_distance, max_distance)
			
			MOUSE_BUTTON_WHEEL_DOWN:
				if _is_mouse_over_ui():
					return
				_target_distance *= (1.0 + zoom_speed)
				_target_distance = clampf(_target_distance, min_distance, max_distance)
	
	# Mouse motion
	elif event is InputEventMouseMotion:
		var motion_event: InputEventMouseMotion = event as InputEventMouseMotion
		
		# Don't process motion if over UI
		if _is_mouse_over_ui():
			_orbiting = false
			_panning = false
			return
		
		var delta: Vector2 = motion_event.position - _last_mouse_position
		
		if _orbiting:
			_rotation.x -= delta.x * orbit_speed * 0.01
			_rotation.y -= delta.y * orbit_speed * 0.01
			_rotation.y = clampf(_rotation.y, deg_to_rad(min_pitch), deg_to_rad(max_pitch))
		
		elif _panning:
			var pan_delta: Vector3 = Vector3(
				- delta.x * pan_speed * 0.01 * _distance * 0.1,
				delta.y * pan_speed * 0.01 * _distance * 0.1,
				0.0
			)
			# Transform pan delta by camera rotation
			var cam_transform: Transform3D = get_global_transform()
			pan_delta = cam_transform.basis * pan_delta
			_target_position += pan_delta
		
		_last_mouse_position = motion_event.position
	
	# Keyboard shortcuts
	elif event is InputEventKey:
		var key_event: InputEventKey = event as InputEventKey
		if key_event.pressed and key_event.keycode == KEY_F:
			focus_on_target()


func _process(delta: float) -> void:
	# Smooth zoom
	_distance = lerpf(_distance, _target_distance, zoom_smooth * delta)
	
	# Update camera transform
	_update_camera_transform()


## Updates the camera transform based on current state.
func _update_camera_transform() -> void:
	# Calculate camera position in spherical coordinates
	var cam_pos: Vector3 = Vector3(
		_distance * cos(_rotation.y) * sin(_rotation.x),
		_distance * sin(_rotation.y),
		_distance * cos(_rotation.y) * cos(_rotation.x)
	)
	
	# Apply target offset
	cam_pos += _target_position
	
	# Update camera transform
	global_position = cam_pos
	look_at(_target_position, Vector3.UP)


## Focuses the camera on the target.
func focus_on_target() -> void:
	_target_position = Vector3.ZERO
	_target_distance = 10.0
	_rotation = Vector2.ZERO


## Sets the point the camera orbits around. Call every frame to follow a moving object (e.g. a moon).
## @param pos: World-space position to look at.
func set_target_position(pos: Vector3) -> void:
	_target_position = pos


## Sets the camera distance.
## @param distance: The distance from the target.
func set_distance(distance: float) -> void:
	_distance = clampf(distance, min_distance, max_distance)
	_target_distance = _distance


## Gets the current camera distance.
## @return: The current distance from the target.
func get_distance() -> float:
	return _distance
