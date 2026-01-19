## Main viewer scene for inspecting celestial objects.
## Handles object display, UI updates, and user interactions.
class_name ObjectViewer
extends Node3D

## UI element references
@onready var status_label: Label = $UI/TopBar/MarginContainer/HBoxContainer/StatusLabel
@onready var side_panel: Panel = $UI/SidePanel
@onready var panel_container: VBoxContainer = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer

## 3D element references
@onready var celestial_object_node: Node3D = $CelestialObject
@onready var placeholder_mesh: MeshInstance3D = $CelestialObject/Placeholder
@onready var camera_rig: Node3D = $CameraRig
@onready var camera_arm: Node3D = $CameraRig/CameraArm
@onready var camera: Camera3D = $CameraRig/CameraArm/Camera3D

## Currently displayed celestial body
var current_body: CelestialBody = null

## Whether the viewer is ready
var is_ready: bool = false


func _ready() -> void:
	# Set up initial state
	_setup_viewport()
	_setup_camera()
	
	# Hide placeholder initially
	if placeholder_mesh:
		placeholder_mesh.visible = false
	
	set_status("Viewer initialized")
	is_ready = true


## Sets up viewport settings.
func _setup_viewport() -> void:
	# Get the viewport
	var viewport: Viewport = get_viewport()
	if not viewport:
		return
	
	# Enable HDR for better star rendering later
	viewport.use_hdr_2d = true


## Sets up initial camera position.
func _setup_camera() -> void:
	if not camera or not camera_rig:
		return
	
	# Set initial camera distance
	if camera is CameraController:
		var controller: CameraController = camera as CameraController
		controller.set_distance(10.0)
	
	# Point camera at origin
	camera.look_at(Vector3.ZERO, Vector3.UP)


## Sets the status message in the UI.
## @param message: The status message to display.
func set_status(message: String) -> void:
	if status_label:
		status_label.text = message
		status_label.modulate = Color(0.7, 0.7, 0.7, 1)


## Sets the error message in the UI.
## @param message: The error message to display.
func set_error(message: String) -> void:
	if status_label:
		status_label.text = "Error: " + message
		status_label.modulate = Color(1.0, 0.3, 0.3)
	push_error(message)


## Displays a celestial body in the viewer.
## @param body: The celestial body to display.
func display_body(body: CelestialBody) -> void:
	if not body:
		set_error("Cannot display null body")
		return
	
	current_body = body
	
	# For now, just show the placeholder sphere
	if placeholder_mesh:
		placeholder_mesh.visible = true
		
		# Scale based on radius (normalize to reasonable view size)
		# We'll improve this in later stages
		var radius_m: float = body.physical.radius_m
		var scale_factor: float = 1.0
		
		# Very basic scaling for now
		if radius_m > 0:
			# Normalize to ~1 unit = Earth radius for viewing
			scale_factor = radius_m / 6.371e6  # Earth radius in meters
			scale_factor = clampf(scale_factor, 0.1, 10.0)
		
		placeholder_mesh.scale = Vector3.ONE * scale_factor
	
	set_status("Displaying: %s" % body.name)
	
	# Update UI (we'll implement this in Stage 3)
	_update_inspector()


## Updates the inspector panel with current body properties.
func _update_inspector() -> void:
	# Placeholder for Stage 3
	pass


## Clears the current display.
func clear_display() -> void:
	current_body = null
	
	if placeholder_mesh:
		placeholder_mesh.visible = false
	
	set_status("No object loaded")
	_update_inspector()
