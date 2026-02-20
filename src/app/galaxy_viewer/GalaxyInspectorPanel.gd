## Inspector panel for displaying galaxy and selection information.
## Shows galaxy overview, current zoom level, selected star details, and system preview.
class_name GalaxyInspectorPanel
extends VBoxContainer


## Emitted when user requests to open selected star as a system.
signal open_system_requested(star_seed: int, world_position: Vector3)

## Currently selected star data.
var _selected_star_seed: int = 0
var _selected_star_position: Vector3 = Vector3.ZERO

## Cached preview data for the currently selected star (null if none / not yet generated).
var _current_preview: StarSystemPreview.PreviewData = null

## Cached references to dynamic content containers.
var _overview_container: VBoxContainer = null
var _selection_container: VBoxContainer = null
var _preview_container: VBoxContainer = null
var _open_system_button: Button = null


func _ready() -> void:
	_build_ui()


## Builds the inspector UI structure.
func _build_ui() -> void:
	add_theme_constant_override("separation", 4)

	# Title
	var title_label: Label = Label.new()
	title_label.text = "Galaxy Inspector"
	title_label.add_theme_font_size_override("font_size", 14)
	add_child(title_label)

	# Separator
	add_child(HSeparator.new())

	# Overview section label
	var overview_label: Label = Label.new()
	overview_label.text = "Overview"
	overview_label.add_theme_font_size_override("font_size", 12)
	overview_label.modulate = Color(0.8, 0.8, 0.8)
	add_child(overview_label)

	# Overview content container
	_overview_container = VBoxContainer.new()
	_overview_container.name = "OverviewContainer"
	_overview_container.add_theme_constant_override("separation", 2)
	add_child(_overview_container)

	# Separator
	add_child(HSeparator.new())

	# Selection section label
	var selection_label: Label = Label.new()
	selection_label.text = "Selection"
	selection_label.add_theme_font_size_override("font_size", 12)
	selection_label.modulate = Color(0.8, 0.8, 0.8)
	add_child(selection_label)

	# Selection content container
	_selection_container = VBoxContainer.new()
	_selection_container.name = "SelectionContainer"
	_selection_container.add_theme_constant_override("separation", 2)
	add_child(_selection_container)

	# Separator
	add_child(HSeparator.new())

	# System preview section label
	var preview_label: Label = Label.new()
	preview_label.text = "System Preview"
	preview_label.add_theme_font_size_override("font_size", 12)
	preview_label.modulate = Color(0.8, 0.8, 0.8)
	add_child(preview_label)

	# Preview content container
	_preview_container = VBoxContainer.new()
	_preview_container.name = "PreviewContainer"
	_preview_container.add_theme_constant_override("separation", 2)
	add_child(_preview_container)

	# Open system button (initially hidden)
	_open_system_button = Button.new()
	_open_system_button.name = "OpenSystemButton"
	_open_system_button.text = "Open System"
	_open_system_button.visible = false
	_open_system_button.pressed.connect(_on_open_system_pressed)
	add_child(_open_system_button)

	# Initial state
	_add_property(_selection_container, "Status", "Nothing selected")
	_add_property(_preview_container, "Status", "Select a star to preview")


## Displays galaxy overview information.
## @param spec: The galaxy specification.
## @param zoom_level: Current zoom level.
func display_galaxy(spec: GalaxySpec, zoom_level: int) -> void:
	_clear_container(_overview_container)

	if spec == null:
		_add_property(_overview_container, "Status", "No galaxy loaded")
		return

	var type_name: String = _get_galaxy_type_name(spec.galaxy_type)
	_add_property(_overview_container, "Type", type_name)
	_add_property(_overview_container, "Seed", str(spec.galaxy_seed))

	var radius_kpc: float = spec.radius_pc / 1000.0
	_add_property(_overview_container, "Radius", "%.1f kpc" % radius_kpc)

	var height_kpc: float = spec.height_pc / 1000.0
	_add_property(_overview_container, "Height", "%.1f kpc" % height_kpc)

	_add_property(_overview_container, "Spiral Arms", str(spec.num_arms))
	_add_property(_overview_container, "Arm Pitch", "%.1f°" % spec.arm_pitch_angle_deg)

	var zoom_name: String = _get_zoom_level_name(zoom_level)
	_add_property(_overview_container, "View", zoom_name)


## Updates the zoom level display.
## @param zoom_level: Current zoom level.
func update_zoom_level(zoom_level: int) -> void:
	for child in _overview_container.get_children():
		if child is HBoxContainer:
			var key_label: Label = child.get_node_or_null("Key") as Label
			if key_label and key_label.text == "View:":
				var value_label: Label = child.get_node_or_null("Value") as Label
				if value_label:
					value_label.text = _get_zoom_level_name(zoom_level)
				return

	# If not found, add it
	_add_property(_overview_container, "View", _get_zoom_level_name(zoom_level))


## Displays selected quadrant information.
## @param coords: Quadrant grid coordinates.
## @param density: Density at quadrant center.
func display_selected_quadrant(coords: Vector3i, density: float) -> void:
	_clear_container(_selection_container)
	_clear_star_selection()

	_add_property(_selection_container, "Type", "Quadrant")
	_add_property(_selection_container, "Coordinates", "(%d, %d, %d)" % [coords.x, coords.y, coords.z])
	_add_property(_selection_container, "Density", "%.4f" % density)

	var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)
	var dist_kpc: float = center.length() / 1000.0
	_add_property(_selection_container, "Distance", "%.2f kpc" % dist_kpc)


## Displays selected sector information.
## @param quadrant_coords: Parent quadrant coordinates.
## @param sector_coords: Sector local coordinates.
## @param density: Density at sector center.
func display_selected_sector(quadrant_coords: Vector3i, sector_coords: Vector3i, density: float) -> void:
	_clear_container(_selection_container)
	_clear_star_selection()

	_add_property(_selection_container, "Type", "Sector")
	_add_property(_selection_container, "Quadrant", "(%d, %d, %d)" % [quadrant_coords.x, quadrant_coords.y, quadrant_coords.z])
	_add_property(_selection_container, "Local", "(%d, %d, %d)" % [sector_coords.x, sector_coords.y, sector_coords.z])
	_add_property(_selection_container, "Density", "%.4f" % density)


## Displays selected star information.
## @param world_position: Star world position in parsecs.
## @param star_seed: Star's deterministic seed.
func display_selected_star(world_position: Vector3, star_seed: int) -> void:
	_clear_container(_selection_container)

	_selected_star_seed = star_seed
	_current_preview = null
	_selected_star_position = world_position

	_add_property(_selection_container, "Type", "Star System")
	_add_property(_selection_container, "Seed", str(star_seed))
	_add_property(_selection_container, "X", "%.2f pc" % world_position.x)
	_add_property(_selection_container, "Y", "%.2f pc" % world_position.y)
	_add_property(_selection_container, "Z", "%.2f pc" % world_position.z)

	var dist_pc: float = world_position.length()
	if dist_pc > 1000.0:
		_add_property(_selection_container, "From Center", "%.2f kpc" % (dist_pc / 1000.0))
	else:
		_add_property(_selection_container, "From Center", "%.1f pc" % dist_pc)

	_open_system_button.visible = true

	# Show generating indicator until preview arrives.
	_clear_container(_preview_container)
	_add_property(_preview_container, "Status", "Generating preview…")


## Displays a system preview for the selected star.
## Called after StarSystemPreview.generate() completes.
## @param preview: The generated PreviewData (null clears the preview section).
func display_system_preview(preview: StarSystemPreview.PreviewData) -> void:
	_current_preview = preview
	_clear_container(_preview_container)

	if preview == null:
		_add_property(_preview_container, "Status", "Preview unavailable")
		return

	# Stars.
	_add_property(_preview_container, "Stars", str(preview.star_count))

	for i in range(preview.spectral_classes.size()):
		var temp: float = preview.star_temperatures[i]
		var temp_str: String = "?"
		if temp > 0.0:
			temp_str = "%d K" % int(temp)
		_add_property(
			_preview_container,
			"  Star %d" % (i + 1),
			"%s  %s" % [preview.spectral_classes[i], temp_str]
		)

	# Bodies.
	_add_property(_preview_container, "Planets", str(preview.planet_count))
	_add_property(_preview_container, "Moons", str(preview.moon_count))
	_add_property(_preview_container, "Belts", str(preview.belt_count))

	# Galactic context.
	_add_property(_preview_container, "Metallicity", "%.2f Z☉" % preview.metallicity)


## Clears the current selection display.
func clear_selection() -> void:
	_clear_container(_selection_container)
	_clear_container(_preview_container)
	_clear_star_selection()
	_add_property(_selection_container, "Status", "Nothing selected")
	_add_property(_preview_container, "Status", "Select a star to preview")


## Clears star selection state.
func _clear_star_selection() -> void:
	_selected_star_seed = 0
	_current_preview = null
	_selected_star_position = Vector3.ZERO
	_open_system_button.visible = false


## Handles open system button press.
func _on_open_system_pressed() -> void:
	if _selected_star_seed != 0:
		open_system_requested.emit(_selected_star_seed, _selected_star_position)


## Clears all children from a container.
## @param container: The container to clear.
func _clear_container(container: VBoxContainer) -> void:
	if container == null:
		return
	for child in container.get_children():
		child.queue_free()


## Adds a key-value property row to a container.
## @param container: Target container.
## @param key: Property name.
## @param value: Property value.
func _add_property(container: VBoxContainer, key: String, value: String) -> void:
	var row: HBoxContainer = HBoxContainer.new()

	var key_label: Label = Label.new()
	key_label.name = "Key"
	key_label.text = key + ":"
	key_label.custom_minimum_size.x = 100
	key_label.modulate = Color(0.7, 0.7, 0.7)
	key_label.add_theme_font_size_override("font_size", 11)
	row.add_child(key_label)

	var value_label: Label = Label.new()
	value_label.name = "Value"
	value_label.text = value
	value_label.add_theme_font_size_override("font_size", 11)
	row.add_child(value_label)

	container.add_child(row)


## Returns human-readable galaxy type name.
## @param galaxy_type: Galaxy type enum value.
## @return: Type name string.
func _get_galaxy_type_name(galaxy_type: GalaxySpec.GalaxyType) -> String:
	match galaxy_type:
		GalaxySpec.GalaxyType.SPIRAL:
			return "Spiral"
		GalaxySpec.GalaxyType.ELLIPTICAL:
			return "Elliptical"
		GalaxySpec.GalaxyType.IRREGULAR:
			return "Irregular"
		_:
			return "Unknown"


## Returns human-readable zoom level name.
## @param zoom_level: Zoom level enum value.
## @return: Level name string.
func _get_zoom_level_name(zoom_level: int) -> String:
	match zoom_level:
		GalaxyCoordinates.ZoomLevel.GALAXY:
			return "Galaxy"
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			return "Quadrant"
		GalaxyCoordinates.ZoomLevel.SECTOR:
			return "Sector"
		GalaxyCoordinates.ZoomLevel.SUBSECTOR:
			return "Star Field"
		_:
			return "Unknown"


## Returns true if a star is currently selected.
## @return: True if star selected.
func has_star_selected() -> bool:
	return _selected_star_seed != 0


## Returns the selected star seed.
## @return: Star seed or 0 if none selected.
func get_selected_star_seed() -> int:
	return _selected_star_seed


## Returns the selected star position.
## @return: World position or zero vector if none selected.
func get_selected_star_position() -> Vector3:
	return _selected_star_position


## Returns the current preview data (for testing).
## @return: PreviewData or null.
func get_current_preview() -> StarSystemPreview.PreviewData:
	return _current_preview
