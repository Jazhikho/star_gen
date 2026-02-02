## First-person exploration camera for the star view.
## WASD forward/back, E/C vertical, right-drag yaw+pitch, scroll speed.
class_name StarViewCamera
extends Camera3D


## Emitted when the camera crosses into a new subsector.
## @param new_origin: World-space origin of the new center subsector.
signal subsector_changed(new_origin: Vector3)

## Base movement speed in parsecs per second.
const BASE_MOVE_SPEED: float = 5.0

## Minimum movement speed.
const MIN_SPEED: float = 0.5

## Maximum movement speed.
const MAX_SPEED: float = 50.0

## Speed multiplier per scroll tick.
const SPEED_SCROLL_FACTOR: float = 1.25

## Mouse sensitivity in degrees per pixel for right-drag.
const MOUSE_SENSITIVITY: float = 0.3

## Camera yaw in degrees.
var _yaw_deg: float = 0.0

## Camera pitch in degrees.
var _pitch_deg: float = 0.0

## Current movement speed in parsecs per second.
var _move_speed: float = BASE_MOVE_SPEED

## Whether the user is right-dragging to look around.
var _mouse_looking: bool = false

## Cached subsector origin for change detection.
var _current_subsector_origin: Vector3 = Vector3.ZERO


## Places the camera at a starting position and sets initial orientation.
## @param start_position: World-space position in parsecs.
## @param initial_yaw_deg: Starting yaw in degrees.
func configure(start_position: Vector3, initial_yaw_deg: float = 0.0) -> void:
	global_position = start_position
	_yaw_deg = initial_yaw_deg
	_pitch_deg = 0.0
	_move_speed = BASE_MOVE_SPEED
	_current_subsector_origin = GalaxyCoordinates.get_subsector_world_origin(start_position)
	_update_orientation()


## Returns the current movement speed.
## @return: Speed in parsecs per second.
func get_move_speed() -> float:
	return _move_speed


## Returns the current subsector origin the camera is in.
## @return: World-space subsector origin.
func get_current_subsector_origin() -> Vector3:
	return _current_subsector_origin


func _process(delta: float) -> void:
	_handle_movement(delta)
	_update_orientation()
	_check_subsector_change()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		_handle_mouse_button(event as InputEventMouseButton)
	elif event is InputEventMouseMotion and _mouse_looking:
		_handle_mouse_motion(event as InputEventMouseMotion)


func _handle_mouse_button(event: InputEventMouseButton) -> void:
	match event.button_index:
		MOUSE_BUTTON_RIGHT:
			_mouse_looking = event.pressed
		MOUSE_BUTTON_WHEEL_UP:
			if event.pressed:
				_move_speed = clampf(
					_move_speed * SPEED_SCROLL_FACTOR, MIN_SPEED, MAX_SPEED
				)
		MOUSE_BUTTON_WHEEL_DOWN:
			if event.pressed:
				_move_speed = clampf(
					_move_speed / SPEED_SCROLL_FACTOR, MIN_SPEED, MAX_SPEED
				)


## Handles right-drag mouse motion for yaw and pitch.
## @param event: Mouse motion event.
func _handle_mouse_motion(event: InputEventMouseMotion) -> void:
	_yaw_deg -= event.relative.x * MOUSE_SENSITIVITY
	_pitch_deg -= event.relative.y * MOUSE_SENSITIVITY
	_pitch_deg = clampf(_pitch_deg, -89.0, 89.0)


## Processes WASD+EC+AD movement each frame.
## @param delta: Frame delta time.
func _handle_movement(delta: float) -> void:
	var forward: Vector3 = _get_forward()
	var right: Vector3 = _get_right()
	var move_delta: float = _move_speed * delta

	if Input.is_key_pressed(KEY_W):
		global_position += forward * move_delta
	if Input.is_key_pressed(KEY_S):
		global_position -= forward * move_delta
	if Input.is_key_pressed(KEY_A):
		global_position -= right * move_delta
	if Input.is_key_pressed(KEY_D):
		global_position += right * move_delta
	if Input.is_key_pressed(KEY_E):
		global_position.y += move_delta
	if Input.is_key_pressed(KEY_C):
		global_position.y -= move_delta


## Returns the camera's forward direction in the XZ plane (ignoring pitch).
## This keeps WASD movement level regardless of where you're looking.
## @return: Normalised forward direction.
func _get_forward() -> Vector3:
	var yaw_rad: float = deg_to_rad(_yaw_deg)
	return Vector3(-sin(yaw_rad), 0.0, -cos(yaw_rad))


## Returns the camera's right direction in the XZ plane.
## @return: Normalised right direction.
func _get_right() -> Vector3:
	var yaw_rad: float = deg_to_rad(_yaw_deg)
	return Vector3(cos(yaw_rad), 0.0, -sin(yaw_rad))


## Applies yaw and pitch to the camera orientation.
func _update_orientation() -> void:
	transform.basis = Basis.IDENTITY
	rotate_y(deg_to_rad(_yaw_deg))
	rotate_object_local(Vector3.RIGHT, deg_to_rad(_pitch_deg))


## Checks whether the camera has entered a new subsector and emits signal.
func _check_subsector_change() -> void:
	var new_origin: Vector3 = GalaxyCoordinates.get_subsector_world_origin(global_position)
	if not new_origin.is_equal_approx(_current_subsector_origin):
		_current_subsector_origin = new_origin
		subsector_changed.emit(new_origin)
