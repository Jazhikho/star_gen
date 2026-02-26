## Orbit camera controller for the belt viewer concept scene.
## Left-drag orbits, right-drag pans, scroll wheel zooms.
## Holds a reference to the pivot node; the camera is a child of the pivot.
class_name BeltViewerCameraControllerConcept
extends Node


## Orbit sensitivity in radians per pixel dragged.
const ORBIT_SENSITIVITY: float = 0.005

## Pan sensitivity in AU per pixel dragged (scales with distance).
const PAN_SENSITIVITY: float = 0.003

## Zoom multiplier per scroll tick.
const ZOOM_FACTOR: float = 0.9

## Minimum camera distance from pivot in AU.
const MIN_DISTANCE: float = 0.5

## Maximum camera distance from pivot in AU.
const MAX_DISTANCE: float = 60.0

## The pivot node the camera orbits around.
var _pivot: Node3D = null

## The camera node, child of the pivot.
var _camera: Camera3D = null

## Current horizontal orbit angle in radians.
var _yaw: float = 0.3

## Current vertical orbit angle in radians.
var _pitch: float = 0.5

## Current distance from pivot to camera in AU.
var _distance: float = 8.0

## Whether left mouse button is currently held.
var _left_held: bool = false

## Whether right mouse button is currently held.
var _right_held: bool = false


## Initialises the controller with the pivot and camera nodes it will drive.
## @param pivot: The Node3D to orbit around.
## @param camera: The Camera3D that is a child of the pivot.
func setup(pivot: Node3D, camera: Camera3D) -> void:
	_pivot = pivot
	_camera = camera
	_apply_transform()


## Handles raw input events for orbit, pan, and zoom.
## Call this from _input on the parent scene.
## @param event: The input event to process.
func handle_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		_handle_mouse_button(event as InputEventMouseButton)
	elif event is InputEventMouseMotion:
		_handle_mouse_motion(event as InputEventMouseMotion)


## Resets the camera to a default orbit position centred on the origin.
func reset_view() -> void:
	if _pivot == null:
		return
	_pivot.position = Vector3.ZERO
	_yaw = 0.3
	_pitch = 0.5
	_distance = 8.0
	_apply_transform()


## Processes mouse button events for zoom and held-button tracking.
## @param event: The mouse button event.
func _handle_mouse_button(event: InputEventMouseButton) -> void:
	match event.button_index:
		MOUSE_BUTTON_LEFT:
			_left_held = event.pressed
		MOUSE_BUTTON_RIGHT:
			_right_held = event.pressed
		MOUSE_BUTTON_WHEEL_UP:
			if event.pressed:
				_distance = maxf(_distance * ZOOM_FACTOR, MIN_DISTANCE)
				_apply_transform()
		MOUSE_BUTTON_WHEEL_DOWN:
			if event.pressed:
				_distance = minf(_distance / ZOOM_FACTOR, MAX_DISTANCE)
				_apply_transform()


## Processes mouse motion events for orbit and pan.
## @param event: The mouse motion event.
func _handle_mouse_motion(event: InputEventMouseMotion) -> void:
	if _left_held:
		_yaw -= event.relative.x * ORBIT_SENSITIVITY
		_pitch -= event.relative.y * ORBIT_SENSITIVITY
		_pitch = clampf(_pitch, -PI * 0.48, PI * 0.48)
		_apply_transform()
	elif _right_held and _camera != null:
		var right: Vector3 = _camera.global_transform.basis.x
		var up: Vector3 = _camera.global_transform.basis.y
		var pan_scale: float = _distance * PAN_SENSITIVITY
		_pivot.position -= right * event.relative.x * pan_scale
		_pivot.position += up * event.relative.y * pan_scale


## Applies current yaw, pitch, and distance to the pivot and camera transforms.
func _apply_transform() -> void:
	if _pivot == null or _camera == null:
		return
	_pivot.rotation.y = _yaw
	_pivot.rotation.x = _pitch
	_camera.position = Vector3(0.0, 0.0, _distance)
