## Root application controller that manages navigation between viewers.
## Switches between SystemViewer and ObjectViewer by adding/removing from tree.
class_name MainApp
extends Node

const _object_viewer_scene := preload("res://src/app/viewer/ObjectViewer.tscn")
const _system_viewer_scene := preload("res://src/app/system_viewer/SystemViewer.tscn")

## Currently active viewer ("system" or "object").
var _active_viewer: String = ""

## The system viewer instance.
var _system_viewer: SystemViewer = null

## The object viewer instance.
var _object_viewer: ObjectViewer = null

## Container for viewer scenes.
@onready var viewer_container: Node = $ViewerContainer


func _ready() -> void:
	_create_system_viewer()
	_show_system_viewer()


## Creates the system viewer instance (does not add to tree yet).
func _create_system_viewer() -> void:
	_system_viewer = _system_viewer_scene.instantiate() as SystemViewer
	_system_viewer.name = "SystemViewer"

	# Connect navigation signal
	_system_viewer.open_body_in_viewer.connect(_on_open_in_object_viewer)


## Creates the object viewer instance (lazy, does not add to tree yet).
func _create_object_viewer() -> void:
	if _object_viewer != null:
		return

	_object_viewer = _object_viewer_scene.instantiate() as ObjectViewer
	_object_viewer.name = "ObjectViewer"

	# Connect back navigation signal
	_object_viewer.back_to_system_requested.connect(_on_back_to_system)


## Shows the system viewer and removes the object viewer from tree.
func _show_system_viewer() -> void:
	if _active_viewer == "system":
		return

	_active_viewer = "system"

	# Remove object viewer from tree (if present)
	if _object_viewer and _object_viewer.is_inside_tree():
		viewer_container.remove_child(_object_viewer)

	# Add system viewer to tree (if not already)
	if _system_viewer and not _system_viewer.is_inside_tree():
		viewer_container.add_child(_system_viewer)


## Shows the object viewer and removes the system viewer from tree.
func _show_object_viewer() -> void:
	if _active_viewer == "object":
		return

	_active_viewer = "object"

	# Remove system viewer from tree (if present)
	if _system_viewer and _system_viewer.is_inside_tree():
		viewer_container.remove_child(_system_viewer)

	# Ensure object viewer exists
	_create_object_viewer()

	# Add object viewer to tree (if not already)
	if _object_viewer and not _object_viewer.is_inside_tree():
		viewer_container.add_child(_object_viewer)


## Handles request to open a body in the object viewer.
## @param body: The celestial body to display.
func _on_open_in_object_viewer(body: CelestialBody) -> void:
	if body == null:
		return

	_show_object_viewer()
	_object_viewer.display_external_body(body)


## Handles request to go back to the system viewer.
func _on_back_to_system() -> void:
	_show_system_viewer()
