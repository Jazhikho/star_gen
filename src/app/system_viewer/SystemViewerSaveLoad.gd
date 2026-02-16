## Handles solar system viewer save/load operations.
## Extracted from SystemViewer to keep script size manageable.
class_name SystemViewerSaveLoad
extends RefCounted

const _system_persistence: GDScript = preload("res://src/services/persistence/SystemPersistence.gd")
const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")


## Handles save button: opens file dialog.
## @param viewer: SystemViewer instance.
func on_save_pressed(viewer: Node) -> void:
	var current_system: SolarSystem = viewer.get_current_system()
	if current_system == null:
		viewer.set_status("No system to save")
		return

	var dialog: FileDialog = FileDialog.new()
	dialog.file_mode = FileDialog.FILE_MODE_SAVE_FILE
	dialog.access = FileDialog.ACCESS_USERDATA
	dialog.filters = PackedStringArray(["*.sgs ; StarGen System", "*.json ; JSON Debug"])

	# Generate default filename from system name or seed
	var default_name: String = "system"
	if current_system.name and not current_system.name.is_empty():
		default_name = current_system.name.to_lower().replace(" ", "_")
	elif current_system.provenance:
		default_name = "system_%d" % current_system.provenance.generation_seed

	dialog.current_file = "%s.sgs" % default_name
	dialog.file_selected.connect(func(path: String) -> void: _on_save_file_selected(viewer, path))
	dialog.canceled.connect(func() -> void: dialog.queue_free())
	viewer.add_child(dialog)
	dialog.popup_centered(Vector2i(800, 600))


## Handles load button: opens file dialog.
## @param viewer: SystemViewer instance.
func on_load_pressed(viewer: Node) -> void:
	var dialog: FileDialog = FileDialog.new()
	dialog.file_mode = FileDialog.FILE_MODE_OPEN_FILE
	dialog.access = FileDialog.ACCESS_USERDATA
	dialog.filters = PackedStringArray(["*.sgs ; StarGen System", "*.json ; JSON Debug"])
	dialog.file_selected.connect(func(path: String) -> void: _on_load_file_selected(viewer, path))
	dialog.canceled.connect(func() -> void: dialog.queue_free())
	viewer.add_child(dialog)
	dialog.popup_centered(Vector2i(800, 600))


## Handles save file selection from dialog.
## @param viewer: SystemViewer instance.
## @param path: Selected file path.
func _on_save_file_selected(viewer: Node, path: String) -> void:
	var current_system: SolarSystem = viewer.get_current_system()
	if current_system == null:
		viewer.set_status("No system to save")
		return

	var compress: bool = not path.ends_with(".json")

	var error: Error = SystemPersistence.save(current_system, path, compress)

	if error == OK:
		var size: int = SystemPersistence.get_file_size(path)
		var size_str: String = SystemPersistence.format_file_size(size)
		viewer.set_status("Saved to %s (%s)" % [path.get_file(), size_str])
	else:
		viewer.set_error("Failed to save: %s" % error_string(error))


## Handles load file selection from dialog.
## @param viewer: SystemViewer instance.
## @param path: Selected file path.
func _on_load_file_selected(viewer: Node, path: String) -> void:
	var result: SystemPersistence.LoadResult = SystemPersistence.load(path)

	if not result.success:
		viewer.set_error("Failed to load: %s" % result.error_message)
		return

	if result.system == null:
		viewer.set_error("Loaded file contains no system data")
		return

	viewer.display_system(result.system)

	# Update seed display if available
	if result.system.provenance:
		viewer.update_seed_display(result.system.provenance.generation_seed)

	viewer.set_status("Loaded: %s" % path.get_file())


## Saves the current system to a path (for programmatic use/testing).
## @param viewer: SystemViewer instance.
## @param path: File path to save to.
## @param compress: Whether to compress the output.
## @return: Error code.
func save_to_path(viewer: Node, path: String, compress: bool = true) -> Error:
	var current_system: SolarSystem = viewer.get_current_system()
	if current_system == null:
		return ERR_INVALID_DATA
	return SystemPersistence.save(current_system, path, compress)


## Loads a system from a path (for programmatic use and testing).
## @param path: File path to load from.
## @return: LoadResult with system or error.
func load_from_path(path: String) -> SystemPersistence.LoadResult:
	return SystemPersistence.load(path)
