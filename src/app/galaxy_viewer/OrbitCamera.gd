## Orbit camera for the galaxy viewer.
## Right-drag to rotate, scroll to zoom.
class_name OrbitCamera
extends Camera3D


## Degrees per pixel of mouse drag.
const ROTATE_SPEED: float = 0.3

## Multiplicative zoom factor per scroll tick.
const ZOOM_FACTOR: float = 0.1

## Minimum orbit distance.
@export var min_distance: float = 500.0

## Maximum orbit distance.
@export var max_distance: float = 120000.0

## Current orbit yaw in degrees.
var _yaw_deg: float = 0.0

## Current orbit pitch in degrees.
var _pitch_deg: float = -35.0

## Current orbit distance.
var _distance: float = 40000.0

## Point the camera orbits around.
var _target: Vector3 = Vector3.ZERO

## Whether the user is currently dragging to rotate.
var _rotating: bool = false


func _ready() -> void:
	_update_transform()


## Returns the current orbit yaw in degrees.
## @return: Yaw angle in degrees.
func get_yaw_deg() -> float:
	return _yaw_deg


## Returns the current orbit pitch in degrees.
## @return: Pitch angle in degrees.
func get_pitch_deg() -> float:
	return _pitch_deg


## Returns the current orbit target point.
## @return: World-space orbit target.
func get_target() -> Vector3:
	return _target


## Returns the current orbit distance.
## @return: Distance from target.
func get_distance() -> float:
	return _distance


## Sets the orbit target and distance, then refreshes the transform.
## @param target: World-space point to orbit around.
## @param distance: Initial orbit distance.
func configure(target: Vector3, distance: float) -> void:
	_target = target
	_distance = clampf(distance, min_distance, max_distance)
	_update_transform()


## Reconfigures the camera constraints for a different zoom level.
## Smoothly clamps current distance into the new range.
## @param new_min: New minimum orbit distance.
## @param new_max: New maximum orbit distance.
## @param new_target: New orbit target point.
func reconfigure_constraints(new_min: float, new_max: float, new_target: Vector3) -> void:
	min_distance = new_min
	max_distance = new_max
	_target = new_target
	_distance = clampf(_distance, min_distance, max_distance)
	_update_transform()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		_handle_mouse_button(event as InputEventMouseButton)
	elif event is InputEventMouseMotion and _rotating:
		_handle_mouse_motion(event as InputEventMouseMotion)


func _handle_mouse_button(event: InputEventMouseButton) -> void:
	match event.button_index:
		MOUSE_BUTTON_RIGHT:
			_rotating = event.pressed
		MOUSE_BUTTON_WHEEL_UP:
			if event.pressed:
				_distance *= (1.0 - ZOOM_FACTOR)
				_distance = clampf(_distance, min_distance, max_distance)
				_update_transform()
		MOUSE_BUTTON_WHEEL_DOWN:
			if event.pressed:
				_distance *= (1.0 + ZOOM_FACTOR)
				_distance = clampf(_distance, min_distance, max_distance)
				_update_transform()


func _handle_mouse_motion(event: InputEventMouseMotion) -> void:
	_yaw_deg -= event.relative.x * ROTATE_SPEED
	_pitch_deg -= event.relative.y * ROTATE_SPEED
	_pitch_deg = clampf(_pitch_deg, -89.0, 89.0)
	_update_transform()


## Recomputes global_position and look_at from yaw, pitch, distance.
func _update_transform() -> void:
	var yaw_rad: float = deg_to_rad(_yaw_deg)
	var pitch_rad: float = deg_to_rad(_pitch_deg)

	var offset: Vector3 = Vector3(
		_distance * cos(pitch_rad) * sin(yaw_rad),
		_distance * sin(pitch_rad),
		_distance * cos(pitch_rad) * cos(yaw_rad)
	)

	global_position = _target + offset
	look_at(_target, Vector3.UP)
