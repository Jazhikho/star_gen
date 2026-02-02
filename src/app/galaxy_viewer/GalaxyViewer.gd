## Main scene for the galaxy viewer.
## Manages zoom-level transitions, click-to-select picking, compass navigation,
## and first-person star exploration with WASD movement.
##
## Flow:
##   Galaxy → ] → Quadrant view (click/compass to select)
##   Quadrant (selected) → ] → Sector view (click/compass to select sector)
##   Sector (selected) → ] → Star view (WASD explore, click to pick stars)
##   [ backs out at any level.
class_name GalaxyViewer
extends Node3D

## Ensures GalaxyInspectorPanel script is loaded so type resolves for analyzer.
const _GalaxyInspectorPanelScript: GDScript = preload("res://src/app/galaxy_viewer/GalaxyInspectorPanel.gd")
const _SaveLoadClass: GDScript = preload("res://src/app/galaxy_viewer/GalaxyViewerSaveLoad.gd")


## Emitted when the user clicks a star in star view.
## @param world_position: World-space position of the selected star.
## @param star_seed: Deterministic seed of the selected star system.
signal star_selected(world_position: Vector3, star_seed: int)

## Emitted when the user wants to open a star system for detailed viewing.
## Open system: _selected_star_seed/_selected_star_position set in _pick_star_at; Enter emits this.
## Do not look up seed from position (floating-point fragile).
## @param star_seed: Deterministic seed of the selected star system.
## @param world_position: World-space position of the star.
signal open_system_requested(star_seed: int, world_position: Vector3)

## Master seed for the galaxy.
@export var galaxy_seed: int = 42

## Total number of sample points to render at galaxy level.
@export var num_points: int = 100000

## World-space size of each star billboard quad.
@export var star_size: float = 80.0

## Whether to start at home position (true) or galaxy view (false).
@export var start_at_home: bool = true

## Opacity of the galaxy point cloud when dimmed.
const GALAXY_DIMMED_OPACITY: float = 0.15

## Opacity of the galaxy point cloud when in galaxy view.
const GALAXY_FULL_OPACITY: float = 1.0

## Opacity of the galaxy point cloud in star view — dimmer to let local stars pop.
const GALAXY_STAR_VIEW_OPACITY: float = 0.05

## UI element references (from scene).
@onready var _status_label: Label = $UI/UIRoot/TopBar/MarginContainer/HBoxContainer/StatusLabel
@onready var _seed_input: SpinBox = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput
@onready var _show_compass_check: CheckBox = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowCompassCheck
@onready var _inspector_panel: GalaxyInspectorPanel = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel
@onready var _save_button: Button = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton
@onready var _load_button: Button = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton

var _spec: GalaxySpec
var _density_model: SpiralDensityModel
var _reference_density: float = 0.0
var _zoom_machine: ZoomStateMachine
var _galaxy_renderer: GalaxyRenderer
var _quadrant_renderer: QuadrantRenderer
var _sector_renderer: SectorRenderer
var _neighborhood_renderer: NeighborhoodRenderer
var _selection_indicator: SelectionIndicator
var _orbit_camera: OrbitCamera
var _star_camera: StarViewCamera
var _quadrant_selector: QuadrantSelector
var _quadrant_cursor: GridCursor
var _sector_cursor: GridCursor
var _compass: NavigationCompass
var _canvas_layer: CanvasLayer

## Currently selected sector local coords, or null.
var _selected_sector: Variant = null

## Currently selected star's world position (or zero if none).
var _selected_star_position: Vector3 = Vector3.ZERO

## Currently selected star's seed (or 0 if none).
var _selected_star_seed: int = 0

## Saved state for restoration after returning from system view.
var _saved_zoom_level: int = -1
var _saved_quadrant: Variant = null
var _saved_sector: Variant = null
var _saved_star_camera_position: Vector3 = Vector3.ZERO
var _saved_star_camera_rotation: Vector3 = Vector3.ZERO

var _save_load: RefCounted = _SaveLoadClass.new()


func _ready() -> void:
	_spec = GalaxySpec.create_milky_way(galaxy_seed)
	_density_model = SpiralDensityModel.new(_spec)
	_reference_density = _compute_reference_density()

	_zoom_machine = ZoomStateMachine.new()
	_zoom_machine.level_changed.connect(_on_zoom_level_changed)

	_quadrant_selector = QuadrantSelector.new()
	_quadrant_cursor = GridCursor.new()
	_sector_cursor = GridCursor.new()

	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = _spec.seed

	var sample: GalaxySample = DensitySampler.sample_galaxy(
		_spec, _density_model, num_points, rng
	)

	_build_galaxy_renderer(sample)
	_build_quadrant_renderer()
	_build_sector_renderer()
	_build_neighborhood_renderer()
	_build_selection_indicator()
	_build_orbit_camera()
	_build_star_camera()
	_build_compass()

	_connect_ui_signals()
	_update_seed_display()

	# Initialize starting view
	if start_at_home:
		_initialize_at_home()
	else:
		_apply_zoom_level(GalaxyCoordinates.ZoomLevel.GALAXY)
		_update_inspector()
		set_status("Galaxy viewer ready")


## Initializes the viewer at the home subsector position.
## Pre-selects quadrant and sector, then jumps directly to star view.
func _initialize_at_home() -> void:
	if not _zoom_machine:
		return

	var hierarchy: GalaxyCoordinates.HierarchyCoords = HomePosition.get_home_hierarchy()

	# Set quadrant selection (required for sector view)
	if _quadrant_cursor:
		_quadrant_cursor.position = hierarchy.quadrant_coords
	if _quadrant_selector:
		_quadrant_selector.set_selection(hierarchy.quadrant_coords)

	# Set sector selection (required for subsector view)
	_selected_sector = hierarchy.sector_local_coords
	if _sector_cursor:
		_sector_cursor.position = hierarchy.sector_local_coords

	# Build sector renderer for the home quadrant (needed even though we skip sector view)
	if _sector_renderer:
		_sector_renderer.build_for_quadrant(hierarchy.quadrant_coords, _density_model)

	# Jump zoom machine to subsector level
	_zoom_machine.set_level(GalaxyCoordinates.ZoomLevel.SUBSECTOR)

	# Apply the subsector view
	_show_subsector_view()
	_update_inspector()

	var home_pos: Vector3 = HomePosition.get_default_position()
	var dist_kpc: float = home_pos.length() / 1000.0
	set_status("Home sector - %.1f kpc from galactic center" % dist_kpc)


func _process(_delta: float) -> void:
	if _compass != null and _compass.visible:
		_compass.sync_rotation(
			_orbit_camera.get_yaw_deg(), _orbit_camera.get_pitch_deg()
		)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		_handle_key_input(event as InputEventKey)
	elif event is InputEventMouseButton:
		_handle_mouse_click(event as InputEventMouseButton)


## Dispatches key input for zoom control.
func _handle_key_input(event: InputEventKey) -> void:
	match event.keycode:
		KEY_BRACKETRIGHT:
			_try_zoom_in()
		KEY_BRACKETLEFT:
			_try_zoom_out()
		KEY_ESCAPE:
			_handle_escape()
		KEY_ENTER, KEY_KP_ENTER:
			_try_open_selected_system()


## Handles escape to deselect star within subsector view.
func _handle_escape() -> void:
	if _zoom_machine.get_current_level() == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		_selection_indicator.hide_indicator()
		_selected_star_position = Vector3.ZERO
		_selected_star_seed = 0
		if _inspector_panel:
			_inspector_panel.clear_selection()


## Handles left-click for selection at each zoom level.
func _handle_mouse_click(event: InputEventMouseButton) -> void:
	if event.button_index != MOUSE_BUTTON_LEFT or not event.pressed:
		return

	var current_level: int = _zoom_machine.get_current_level()

	match current_level:
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			_pick_quadrant_at(event.position)
		GalaxyCoordinates.ZoomLevel.SECTOR:
			_pick_sector_at(event.position)
		GalaxyCoordinates.ZoomLevel.SUBSECTOR:
			_pick_star_at(event.position)


## Raycasts from the camera through the screen position to pick a quadrant.
## @param screen_position: Mouse click position in screen coordinates.
func _pick_quadrant_at(screen_position: Vector2) -> void:
	var ray_origin: Vector3 = _orbit_camera.project_ray_origin(screen_position)
	var ray_direction: Vector3 = _orbit_camera.project_ray_normal(screen_position)

	var picked: Variant = _quadrant_selector.pick_from_ray(
		ray_origin, ray_direction, _quadrant_renderer.get_occupied_coords()
	)

	if picked != null:
		_select_quadrant(picked as Vector3i)


## Handles compass direction input.
## @param direction: Cardinal direction vector from compass click.
func _on_compass_direction(direction: Vector3i) -> void:
	var current_level: int = _zoom_machine.get_current_level()

	match current_level:
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			_navigate_quadrant(direction)
		GalaxyCoordinates.ZoomLevel.SECTOR:
			_navigate_sector(direction)


## Navigates the quadrant cursor in a direction.
## @param direction: Cardinal direction.
func _navigate_quadrant(direction: Vector3i) -> void:
	var occupied: Array[Vector3i] = _quadrant_renderer.get_occupied_coords()

	if not _quadrant_selector.has_selection():
		var nearest: Variant = _quadrant_cursor.snap_to_nearest(occupied)
		if nearest != null:
			_select_quadrant(nearest as Vector3i)
		return

	var new_pos: Variant = _quadrant_cursor.move_in_direction(direction, occupied)
	if new_pos != null:
		_select_quadrant(new_pos as Vector3i)


## Navigates the sector cursor in a direction.
## @param direction: Cardinal direction.
func _navigate_sector(direction: Vector3i) -> void:
	var occupied: Array[Vector3i] = _sector_renderer.get_occupied_coords()

	if _selected_sector == null:
		var nearest: Variant = _sector_cursor.snap_to_nearest(occupied)
		if nearest != null:
			_select_sector(nearest as Vector3i)
		return

	var new_pos: Variant = _sector_cursor.move_in_direction(direction, occupied)
	if new_pos != null:
		_select_sector(new_pos as Vector3i)


## Sets the selected quadrant and updates all visual state.
## @param coords: Quadrant grid coordinates to select.
func _select_quadrant(coords: Vector3i) -> void:
	_quadrant_cursor.position = coords
	_quadrant_selector.set_selection(coords)
	_quadrant_renderer.set_highlight(coords)

	if _inspector_panel:
		var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)
		var density: float = _density_model.get_density(center)
		_inspector_panel.display_selected_quadrant(coords, density)

	set_status("Selected quadrant (%d, %d, %d)" % [coords.x, coords.y, coords.z])


## Raycasts to pick a sector.
func _pick_sector_at(screen_position: Vector2) -> void:
	var ray_origin: Vector3 = _orbit_camera.project_ray_origin(screen_position)
	var ray_direction: Vector3 = _orbit_camera.project_ray_normal(screen_position)

	var occupied: Array[Vector3i] = _sector_renderer.get_occupied_coords()
	var best_coords: Variant = null
	var best_distance: float = INF

	for coords in occupied:
		var aabb: Array[Vector3] = _sector_renderer.get_sector_world_aabb(coords)
		var hit_dist: float = RaycastUtils.ray_intersects_aabb(
			ray_origin, ray_direction, aabb[0], aabb[1]
		)
		if hit_dist >= 0.0 and hit_dist < best_distance:
			best_distance = hit_dist
			best_coords = coords

	if best_coords != null:
		_select_sector(best_coords as Vector3i)


## Attempts to open the currently selected star as a system (Enter key).
func _try_open_selected_system() -> void:
	if _zoom_machine.get_current_level() != GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		return

	if _selected_star_seed == 0:
		return

	open_system_requested.emit(_selected_star_seed, _selected_star_position)


## Raycasts to pick a star using the star view camera.
func _pick_star_at(screen_position: Vector2) -> void:
	var ray_origin: Vector3 = _star_camera.project_ray_origin(screen_position)
	var ray_direction: Vector3 = _star_camera.project_ray_normal(screen_position)

	var result: Variant = _neighborhood_renderer.pick_star(ray_origin, ray_direction)

	if result != null:
		var pick: StarPicker.PickResult = result as StarPicker.PickResult
		_selection_indicator.show_at(pick.world_position)
		_selected_star_position = pick.world_position
		_selected_star_seed = pick.star_seed
		star_selected.emit(pick.world_position, pick.star_seed)
		if _inspector_panel:
			_inspector_panel.display_selected_star(pick.world_position, pick.star_seed)
		set_status("Selected star (seed: %d)" % pick.star_seed)
	else:
		_selection_indicator.hide_indicator()
		_selected_star_position = Vector3.ZERO
		_selected_star_seed = 0
		if _inspector_panel:
			_inspector_panel.clear_selection()


## Sets the selected sector.
func _select_sector(coords: Vector3i) -> void:
	_sector_cursor.position = coords
	_selected_sector = coords
	_sector_renderer.set_highlight(coords)

	if _inspector_panel and _quadrant_selector.has_selection():
		var quadrant_coords: Vector3i = _quadrant_selector.selected_coords as Vector3i
		var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(quadrant_coords, coords)
		var sector_center: Vector3 = sector_origin + Vector3.ONE * GalaxyCoordinates.SECTOR_SIZE_PC * 0.5
		var density: float = _density_model.get_density(sector_center)
		_inspector_panel.display_selected_sector(quadrant_coords, coords, density)

	set_status("Selected sector (%d, %d, %d)" % [coords.x, coords.y, coords.z])


## Attempts to zoom in with selection requirements.
func _try_zoom_in() -> void:
	var current_level: int = _zoom_machine.get_current_level()

	match current_level:
		GalaxyCoordinates.ZoomLevel.GALAXY:
			_zoom_machine.zoom_in()
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			if _quadrant_selector.has_selection():
				_zoom_machine.zoom_in()
		GalaxyCoordinates.ZoomLevel.SECTOR:
			if _selected_sector != null:
				_zoom_machine.zoom_in()


## Attempts to zoom out.
func _try_zoom_out() -> void:
	_zoom_machine.zoom_out()


## Handles zoom level transitions by updating renderers and camera.
## @param old_level: Previous zoom level (unused, kept for signal signature).
## @param new_level: New zoom level.
func _on_zoom_level_changed(_old_level: int, new_level: int) -> void:
	_apply_zoom_level(new_level)
	_update_inspector()


## Applies visual state for a given zoom level.
## @param level: The zoom level to apply.
func _apply_zoom_level(level: int) -> void:
	match level:
		GalaxyCoordinates.ZoomLevel.GALAXY:
			_show_galaxy_view()
		GalaxyCoordinates.ZoomLevel.QUADRANT:
			_show_quadrant_view()
		GalaxyCoordinates.ZoomLevel.SECTOR:
			_show_sector_view()
		GalaxyCoordinates.ZoomLevel.SUBSECTOR:
			_show_subsector_view()


## Switches to orbit camera, deactivates star camera.
func _activate_orbit_camera() -> void:
	_star_camera.set_process(false)
	_star_camera.current = false
	_orbit_camera.current = true


## Switches to star camera, deactivates orbit camera.
func _activate_star_camera() -> void:
	_orbit_camera.current = false
	_star_camera.current = true
	_star_camera.set_process(true)


## Galaxy view.
func _show_galaxy_view() -> void:
	_activate_orbit_camera()
	_galaxy_renderer.set_opacity(GALAXY_FULL_OPACITY)
	_quadrant_renderer.visible = false
	_quadrant_renderer.set_highlight(null)
	_sector_renderer.visible = false
	_sector_renderer.set_highlight(null)
	_neighborhood_renderer.visible = false
	_selection_indicator.hide_indicator()
	_quadrant_selector.clear_selection()
	_selected_sector = null
	_compass.visible = false

	_orbit_camera.reconfigure_constraints(500.0, 120000.0, Vector3.ZERO)

	if _inspector_panel:
		_inspector_panel.clear_selection()
	set_status("Galaxy view - press ] to zoom in")


## Quadrant view.
func _show_quadrant_view() -> void:
	_activate_orbit_camera()
	_galaxy_renderer.set_opacity(GALAXY_DIMMED_OPACITY)
	_quadrant_renderer.visible = true
	_sector_renderer.visible = false
	_sector_renderer.set_highlight(null)
	_neighborhood_renderer.visible = false
	_selection_indicator.hide_indicator()
	_selected_sector = null
	_compass.visible = _show_compass_check == null or _show_compass_check.button_pressed

	if _quadrant_selector.has_selection():
		_quadrant_renderer.set_highlight(_quadrant_selector.selected_coords)

	_orbit_camera.reconfigure_constraints(200.0, 60000.0, Vector3.ZERO)

	if _inspector_panel:
		_inspector_panel.clear_selection()
	set_status("Quadrant view - click to select, ] to zoom in")


## Sector view — select a sector within the chosen quadrant.
## Quadrant grid shown for context but NOT highlighted to avoid white overlap.
func _show_sector_view() -> void:
	if not _quadrant_selector.has_selection():
		return

	_activate_orbit_camera()
	var quadrant_coords: Vector3i = _quadrant_selector.selected_coords as Vector3i
	var quadrant_center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(quadrant_coords)

	_galaxy_renderer.set_opacity(GALAXY_DIMMED_OPACITY)
	# Show quadrant grid for context but clear highlight to prevent white block
	_quadrant_renderer.visible = true
	_quadrant_renderer.set_highlight(null)
	_sector_renderer.build_for_quadrant(quadrant_coords, _density_model)
	_sector_renderer.visible = true
	_neighborhood_renderer.visible = false
	_selection_indicator.hide_indicator()
	_compass.visible = _show_compass_check == null or _show_compass_check.button_pressed

	if _selected_sector != null:
		_sector_renderer.set_highlight(_selected_sector)

	_orbit_camera.reconfigure_constraints(50.0, 5000.0, quadrant_center)

	if _inspector_panel:
		_inspector_panel.clear_selection()
	set_status("Sector view - click to select, ] to zoom in")


## Star view — first-person exploration with dimmed galaxy backdrop.
func _show_subsector_view() -> void:
	if not _quadrant_selector.has_selection() or _selected_sector == null:
		return

	var quadrant_coords: Vector3i = _quadrant_selector.selected_coords as Vector3i
	var sector_local: Vector3i = _selected_sector as Vector3i
	var sector_origin: Vector3 = GalaxyCoordinates.sector_world_origin(
		quadrant_coords, sector_local
	)
	var sector_center: Vector3 = sector_origin + Vector3.ONE * GalaxyCoordinates.SECTOR_SIZE_PC * 0.5

	# Galaxy visible but much dimmer so local stars stand out
	_galaxy_renderer.visible = true
	_galaxy_renderer.set_opacity(GALAXY_STAR_VIEW_OPACITY)
	_quadrant_renderer.visible = false
	_sector_renderer.visible = false
	_compass.visible = false

	_star_camera.configure(sector_center)
	_activate_star_camera()

	_neighborhood_renderer.build_neighborhood(
		sector_center, galaxy_seed, _density_model, _reference_density
	)
	_neighborhood_renderer.visible = true

	if _inspector_panel:
		_inspector_panel.clear_selection()
	set_status("Star field - WASD to move, click to select star, Enter to open system")


## Creates the galaxy point cloud renderer.
func _build_galaxy_renderer(sample: GalaxySample) -> void:
	_galaxy_renderer = GalaxyRenderer.new()
	_galaxy_renderer.name = "GalaxyRenderer"
	add_child(_galaxy_renderer)
	_galaxy_renderer.build_from_sample(sample, star_size)


## Creates the quadrant grid renderer (starts hidden).
func _build_quadrant_renderer() -> void:
	_quadrant_renderer = QuadrantRenderer.new()
	_quadrant_renderer.name = "QuadrantRenderer"
	add_child(_quadrant_renderer)
	_quadrant_renderer.build_from_density(_spec, _density_model)
	_quadrant_renderer.visible = false


## Creates the sector grid renderer.
func _build_sector_renderer() -> void:
	_sector_renderer = SectorRenderer.new()
	_sector_renderer.name = "SectorRenderer"
	add_child(_sector_renderer)
	_sector_renderer.visible = false


## Creates the neighborhood renderer for 3x3x3 subsector view.
func _build_neighborhood_renderer() -> void:
	_neighborhood_renderer = NeighborhoodRenderer.new()
	_neighborhood_renderer.name = "NeighborhoodRenderer"
	add_child(_neighborhood_renderer)
	_neighborhood_renderer.visible = false


## Creates the star selection indicator.
func _build_selection_indicator() -> void:
	_selection_indicator = SelectionIndicator.new()
	_selection_indicator.name = "SelectionIndicator"
	add_child(_selection_indicator)


## Creates the orbit camera.
func _build_orbit_camera() -> void:
	_orbit_camera = OrbitCamera.new()
	_orbit_camera.name = "OrbitCamera"
	_orbit_camera.far = _spec.radius_pc * 10.0
	add_child(_orbit_camera)
	_orbit_camera.configure(Vector3.ZERO, _spec.radius_pc * 2.5)


## Creates the first-person star view camera.
func _build_star_camera() -> void:
	_star_camera = StarViewCamera.new()
	_star_camera.name = "StarViewCamera"
	_star_camera.near = 0.1
	# Far plane must cover the galaxy so the dimmed point cloud stays visible
	_star_camera.far = _spec.radius_pc * 10.0
	_star_camera.fov = 70.0
	_star_camera.set_process(false)
	_star_camera.subsector_changed.connect(_on_subsector_changed)
	add_child(_star_camera)


## Called when the star camera crosses into a new subsector.
## @param _new_origin: World-space origin of the new subsector (unused, kept for signal signature).
func _on_subsector_changed(_new_origin: Vector3) -> void:
	_neighborhood_renderer.build_neighborhood(
		_star_camera.global_position, galaxy_seed,
		_density_model, _reference_density
	)


## Builds the compass rose UI in a CanvasLayer overlay.
func _build_compass() -> void:
	_canvas_layer = CanvasLayer.new()
	_canvas_layer.name = "CompassLayer"
	_canvas_layer.layer = 10
	add_child(_canvas_layer)

	_compass = NavigationCompass.new()
	_compass.name = "NavigationCompass"
	_compass.anchor_left = 1.0
	_compass.anchor_top = 1.0
	_compass.anchor_right = 1.0
	_compass.anchor_bottom = 1.0
	_compass.offset_left = - NavigationCompass.VIEWPORT_SIZE - 20
	_compass.offset_top = - NavigationCompass.VIEWPORT_SIZE - 20
	_compass.offset_right = -20
	_compass.offset_bottom = -20
	_compass.visible = false
	_compass.direction_pressed.connect(_on_compass_direction)
	_canvas_layer.add_child(_compass)


## Computes the reference density at the solar-neighborhood-equivalent radius.
## This ensures that areas at ~8kpc from center produce ~4 systems per subsector.
## @return: Density value at the reference radius.
func _compute_reference_density() -> float:
	# Solar neighborhood is ~8kpc from galactic center, in the disk plane
	var solar_radius_pc: float = 8000.0
	return _density_model.get_density(Vector3(solar_radius_pc, 0.0, 0.0))


## Connects UI element signals.
func _connect_ui_signals() -> void:
	if _show_compass_check:
		_show_compass_check.toggled.connect(_on_show_compass_toggled)
	if _inspector_panel:
		_inspector_panel.open_system_requested.connect(_on_inspector_open_system)
	if _save_button:
		_save_button.pressed.connect(_on_save_pressed)
	if _load_button:
		_load_button.pressed.connect(_on_load_pressed)


## Updates the seed display to match current galaxy seed.
func _update_seed_display() -> void:
	if _seed_input:
		_seed_input.value = galaxy_seed


## Handles show compass toggle.
## @param enabled: Whether compass should be shown.
func _on_show_compass_toggled(enabled: bool) -> void:
	var current_level: int = _zoom_machine.get_current_level()
	if current_level == GalaxyCoordinates.ZoomLevel.QUADRANT or \
	   current_level == GalaxyCoordinates.ZoomLevel.SECTOR:
		_compass.visible = enabled


## Handles inspector panel request to open system.
## @param star_seed: The star seed.
## @param world_position: The star position.
func _on_inspector_open_system(star_seed: int, world_position: Vector3) -> void:
	open_system_requested.emit(star_seed, world_position)


## Updates the inspector panel with current state.
func _update_inspector() -> void:
	if _inspector_panel:
		_inspector_panel.display_galaxy(_spec, _zoom_machine.get_current_level())


## Sets the status message.
## @param message: Status text.
func set_status(message: String) -> void:
	if _status_label:
		_status_label.text = message


## Returns the galaxy specification.
## @return: GalaxySpec instance.
func get_spec() -> GalaxySpec:
	return _spec


## Returns the current zoom level.
## @return: Zoom level enum value.
func get_zoom_level() -> int:
	if _zoom_machine:
		return _zoom_machine.get_current_level()
	return GalaxyCoordinates.ZoomLevel.GALAXY


## Returns the inspector panel (for testing).
## @return: GalaxyInspectorPanel instance.
func get_inspector_panel() -> GalaxyInspectorPanel:
	return _inspector_panel


## Navigates to the home subsector from any zoom level.
## Useful for "go home" button functionality.
func navigate_to_home() -> void:
	_initialize_at_home()


## Returns whether the viewer starts at home position.
## @return: True if starting at home.
func get_start_at_home() -> bool:
	return start_at_home


## Saves the current viewer state for later restoration.
## Called before transitioning to system viewer.
func save_state() -> void:
	_save_load.save_state(self)


## Restores the previously saved viewer state.
## Called when returning from system viewer.
func restore_state() -> void:
	_save_load.restore_state(self)


## Clears saved state.
func clear_saved_state() -> void:
	_save_load.clear_saved_state(self)


## Returns true if there is saved state to restore.
## @return: True if state was previously saved.
func has_saved_state() -> bool:
	return _save_load.has_saved_state(self)


## Returns the currently selected star seed, or 0 if none.
## @return: Star seed or 0.
func get_selected_star_seed() -> int:
	return _selected_star_seed


## Returns the currently selected star position.
## @return: World position or zero vector.
func get_selected_star_position() -> Vector3:
	return _selected_star_position


## Handles save button press.
func _on_save_pressed() -> void:
	_save_load.on_save_pressed(self)


## Handles load button press.
func _on_load_pressed() -> void:
	_save_load.on_load_pressed(self)


## Changes the galaxy seed and regenerates galaxy data.
## @param new_seed: New galaxy seed.
func _change_galaxy_seed(new_seed: int) -> void:
	galaxy_seed = new_seed

	_spec = GalaxySpec.create_milky_way(galaxy_seed)
	_density_model = SpiralDensityModel.new(_spec)
	_reference_density = _compute_reference_density()

	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = _spec.seed

	var sample: GalaxySample = DensitySampler.sample_galaxy(
		_spec, _density_model, num_points, rng
	)

	_galaxy_renderer.build_from_sample(sample, star_size)
	_quadrant_renderer.build_from_density(_spec, _density_model)

	if _quadrant_selector:
		_quadrant_selector.clear_selection()
	_selected_sector = null

	_update_seed_display()


## Returns current save data (for testing).
## @return: GalaxySaveData with current state.
func get_save_data() -> GalaxySaveData:
	return _save_load.create_save_data(self)


## Applies save data (for testing).
## @param data: GalaxySaveData to apply.
func apply_save_data(data: GalaxySaveData) -> void:
	_save_load.apply_save_data(self, data)


## Getters/setters for GalaxyViewerSaveLoad (and other helpers).
func get_zoom_machine() -> ZoomStateMachine:
	return _zoom_machine

func get_quadrant_selector() -> QuadrantSelector:
	return _quadrant_selector

func get_quadrant_cursor() -> GridCursor:
	return _quadrant_cursor

func get_sector_cursor() -> GridCursor:
	return _sector_cursor

func get_sector_renderer() -> SectorRenderer:
	return _sector_renderer

func get_star_camera() -> StarViewCamera:
	return _star_camera

func get_neighborhood_renderer() -> NeighborhoodRenderer:
	return _neighborhood_renderer

func get_density_model() -> SpiralDensityModel:
	return _density_model

func get_reference_density() -> float:
	return _reference_density

func get_selection_indicator() -> SelectionIndicator:
	return _selection_indicator

func get_saved_zoom_level() -> int:
	return _saved_zoom_level

func set_saved_zoom_level(level: int) -> void:
	_saved_zoom_level = level

func get_saved_quadrant() -> Variant:
	return _saved_quadrant

func set_saved_quadrant(v: Variant) -> void:
	_saved_quadrant = v

func get_saved_sector() -> Variant:
	return _saved_sector

func set_saved_sector(v: Variant) -> void:
	_saved_sector = v

func get_saved_star_camera_position() -> Vector3:
	return _saved_star_camera_position

func set_saved_star_camera_position(v: Vector3) -> void:
	_saved_star_camera_position = v

func get_saved_star_camera_rotation() -> Vector3:
	return _saved_star_camera_rotation

func set_saved_star_camera_rotation(v: Vector3) -> void:
	_saved_star_camera_rotation = v

func get_selected_sector_internal() -> Variant:
	return _selected_sector

func set_selected_sector_internal(v: Variant) -> void:
	_selected_sector = v

func get_selected_star_seed_internal() -> int:
	return _selected_star_seed

func set_selected_star_seed_internal(v: int) -> void:
	_selected_star_seed = v

func get_selected_star_position_internal() -> Vector3:
	return _selected_star_position

func set_selected_star_position_internal(v: Vector3) -> void:
	_selected_star_position = v

func call_initialize_at_home() -> void:
	_initialize_at_home()

func call_apply_zoom_level(level: int) -> void:
	_apply_zoom_level(level)

func call_update_inspector() -> void:
	_update_inspector()

func call_change_galaxy_seed(seed_value: int) -> void:
	_change_galaxy_seed(seed_value)
