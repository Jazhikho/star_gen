## Main scene for the galaxy viewer.
## Manages zoom-level transitions, click-to-select picking, compass navigation,
## and first-person star exploration with WASD movement. On star click, generates
## a system preview via StarSystemPreview and caches the result for reuse when
## the user opens the system.
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
const _GalaxyConfigRef: GDScript = preload("res://src/domain/galaxy/GalaxyConfig.gd")
const _SaveLoadClass: GDScript = preload("res://src/app/galaxy_viewer/GalaxyViewerSaveLoad.gd")
const _GalaxyClass: GDScript = preload("res://src/domain/galaxy/Galaxy.gd")
const _StarSystemPreviewClass: GDScript = preload("res://src/domain/galaxy/StarSystemPreview.gd")
# Jump lane domain scripts — preloaded here to ensure class_names are registered.
const _JumpLaneCalculatorScript: GDScript = preload("res://src/domain/jumplanes/JumpLaneCalculator.gd")
const _JumpLaneRegionScript: GDScript = preload("res://src/domain/jumplanes/JumpLaneRegion.gd")
const _JumpLaneSystemScript: GDScript = preload("res://src/domain/jumplanes/JumpLaneSystem.gd")
const _JumpLaneResultScript: GDScript = preload("res://src/domain/jumplanes/JumpLaneResult.gd")
const _JumpLaneConnectionScript: GDScript = preload("res://src/domain/jumplanes/JumpLaneConnection.gd")
const _GalaxySystemGeneratorScript: GDScript = preload("res://src/domain/galaxy/GalaxySystemGenerator.gd")
const _GalaxyStarScript: GDScript = preload("res://src/domain/galaxy/GalaxyStar.gd")
const _SectorJumpLaneRendererScript: GDScript = preload("res://src/app/galaxy_viewer/SectorJumpLaneRenderer.gd")


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

## Emitted when the galaxy seed changes (e.g., after loading a saved galaxy).
## @param new_seed: The new galaxy seed value.
signal galaxy_seed_changed(new_seed: int)

## Emitted when the user requests to create a new galaxy.
## The MainApp should handle this by showing the WelcomeScreen or galaxy config dialog.
signal new_galaxy_requested()

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
@onready var _new_galaxy_button: Button = $UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/NewGalaxyButton

## The Galaxy data model instance (lazy generation, caching).
var _galaxy: Galaxy

var _spec: GalaxySpec
var _galaxy_config: GalaxyConfig = null
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

## Cached preview for the currently selected star (null if none).
var _star_preview: StarSystemPreview.PreviewData = null

## Saved state for restoration after returning from system view.
var _saved_zoom_level: int = -1
var _saved_quadrant: Variant = null
var _saved_sector: Variant = null
var _saved_star_camera_position: Vector3 = Vector3.ZERO
var _saved_star_camera_rotation: Vector3 = Vector3.ZERO

## Saved star selection for restoration after returning from system view.
var _saved_star_seed: int = 0
var _saved_star_position: Vector3 = Vector3.ZERO

var _save_load: RefCounted = _SaveLoadClass.new()

## Jump lane state.
## The region is retained so incremental recalculation can merge new stars in.
var _jump_lane_region: JumpLaneRegion = null

## The renderer is a sibling to NeighborhoodRenderer; hidden until routes are calculated.
var _sector_jump_lane_renderer: Node3D = null
## Cached result for the last calculated neighborhood; null when stale.
var _jump_lane_result: JumpLaneResult = null
## Calculator instance (stateless; reused across calls).
var _jump_calculator: JumpLaneCalculator = null

## Progress overlay nodes (created in _build_progress_overlay).
var _progress_canvas: CanvasLayer = null
var _progress_panel: PanelContainer = null
var _progress_label: Label = null


func _ready() -> void:
	if _galaxy_config == null:
		_galaxy_config = GalaxyConfig.create_default()

	# Create Galaxy data model (handles lazy generation and caching)
	_galaxy = Galaxy.new(_galaxy_config, galaxy_seed)
	_spec = _galaxy.spec

	_zoom_machine = ZoomStateMachine.new()
	_zoom_machine.level_changed.connect(_on_zoom_level_changed)

	_quadrant_selector = QuadrantSelector.new()
	_quadrant_cursor = GridCursor.new()
	_sector_cursor = GridCursor.new()

	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = _spec.galaxy_seed

	var sample: GalaxySample = DensitySampler.sample_galaxy(
		_spec, num_points, rng
	)

	_build_galaxy_renderer(sample)
	_build_quadrant_renderer()
	_build_sector_renderer()
	_build_neighborhood_renderer()
	_build_selection_indicator()
	_build_orbit_camera()
	_build_star_camera()
	_build_compass()

	_build_sector_jump_lane_renderer()
	_build_progress_overlay()
	_jump_calculator = _JumpLaneCalculatorScript.new() as JumpLaneCalculator

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
		_sector_renderer.build_for_quadrant(hierarchy.quadrant_coords, _galaxy.density_model)

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
		_clear_star_selection_state()


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
		var density: float = _galaxy.density_model.get_density(center)
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
	if _zoom_machine == null:
		return
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
		_apply_star_selection(pick.world_position, pick.star_seed, true)
	else:
		_selection_indicator.hide_indicator()
		_clear_star_selection_state()


## Applies a star selection: updates state, inspector, triggers preview generation.
## @param world_position: World-space position of the star.
## @param star_seed: Deterministic seed of the star.
## @param show_indicator: If true, show the selection indicator at the star position.
func _apply_star_selection(world_position: Vector3, star_seed: int, show_indicator: bool = true) -> void:
	_selected_star_position = world_position
	_selected_star_seed = star_seed
	_star_preview = null
	star_selected.emit(world_position, star_seed)
	if _inspector_panel:
		_inspector_panel.display_selected_star(world_position, star_seed)
	set_status("Selected star (seed: %d) — generating preview…" % star_seed)

	if show_indicator:
		_selection_indicator.show_at(world_position)

	# Generate the preview (and cache it). This runs synchronously; for large
	# galaxies this is fast enough (<1 ms typical for system generation).
	if _spec != null:
		_star_preview = StarSystemPreview.generate(star_seed, world_position, _spec)
	else:
		_star_preview = null
	if _inspector_panel:
		_inspector_panel.display_system_preview(_star_preview)
	set_status("Selected star (seed: %d)" % star_seed)


## Clears star selection state and inspector.
func _clear_star_selection_state() -> void:
	_selected_star_position = Vector3.ZERO
	_selected_star_seed = 0
	_star_preview = null
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
		var density: float = _galaxy.density_model.get_density(sector_center)
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
	# Hide jump lanes when leaving star view — they are neighborhood-specific.
	if _sector_jump_lane_renderer:
		_sector_jump_lane_renderer.visible = false
	# Do NOT clear jump lane state here — lanes persist until explicit recalc or new galaxy.

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
	if _sector_jump_lane_renderer:
		_sector_jump_lane_renderer.visible = false
	# Do NOT clear jump lane state here.
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
	_sector_renderer.build_for_quadrant(quadrant_coords, _galaxy.density_model)
	_sector_renderer.visible = true
	_neighborhood_renderer.visible = false
	_selection_indicator.hide_indicator()
	if _sector_jump_lane_renderer:
		_sector_jump_lane_renderer.visible = false
	# Do NOT clear jump lane state here.
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
		sector_center, galaxy_seed, _galaxy.density_model, _galaxy.reference_density
	)
	_neighborhood_renderer.visible = true

	if _inspector_panel:
		_inspector_panel.clear_selection()
	set_status("Star field - WASD to move, click to select star, Enter to open system")

	# Restore jump lane visibility if routes are available.
	if _jump_lane_result != null and _sector_jump_lane_renderer:
		var show_now: bool = true
		if _inspector_panel:
			show_now = _inspector_panel.get_show_routes_checked()
		_sector_jump_lane_renderer.visible = show_now


## Creates the galaxy point cloud renderer.
func _build_galaxy_renderer(sample: GalaxySample) -> void:
	_galaxy_renderer = GalaxyRenderer.new()
	_galaxy_renderer.name = "GalaxyRenderer"
	add_child(_galaxy_renderer)
	_galaxy_renderer.build_from_sample(sample, star_size, _spec.galaxy_type)


## Creates the quadrant grid renderer (starts hidden).
func _build_quadrant_renderer() -> void:
	_quadrant_renderer = QuadrantRenderer.new()
	_quadrant_renderer.name = "QuadrantRenderer"
	add_child(_quadrant_renderer)
	_quadrant_renderer.build_from_density(_spec, _galaxy.density_model)
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
		_star_camera.global_position, galaxy_seed, _galaxy.density_model, _galaxy.reference_density
	)
	# Moving to a new subsector does NOT clear jump lanes — they persist until
	# explicit recalculation or a new galaxy. The existing result is still valid
	# (it covers the same neighborhood region); the user can recalculate if needed.
	# Restore renderer visibility to match current checkbox state.
	if _jump_lane_result != null and _sector_jump_lane_renderer:
		var show_now: bool = true
		if _inspector_panel:
			show_now = _inspector_panel.get_show_routes_checked()
		_sector_jump_lane_renderer.visible = show_now


## Clears jump lane result and notifies the inspector panel that recalculation is needed.
func _clear_jump_lane_state() -> void:
	_jump_lane_result = null
	_jump_lane_region = null
	if _sector_jump_lane_renderer:
		(_sector_jump_lane_renderer as SectorJumpLaneRenderer).clear()
		_sector_jump_lane_renderer.visible = false
	if _inspector_panel:
		_inspector_panel.set_jump_routes_available(false)


# =============================================================================
# Jump lane renderer and progress overlay construction
# =============================================================================

## Creates the SectorJumpLaneRenderer as a sibling of NeighborhoodRenderer.
func _build_sector_jump_lane_renderer() -> void:
	_sector_jump_lane_renderer = _SectorJumpLaneRendererScript.new() as SectorJumpLaneRenderer
	_sector_jump_lane_renderer.name = "SectorJumpLaneRenderer"
	_sector_jump_lane_renderer.visible = false
	add_child(_sector_jump_lane_renderer)


## Creates the progress overlay panel (hidden by default).
## Shown during jump lane calculation so the user knows work is happening.
func _build_progress_overlay() -> void:
	_progress_canvas = CanvasLayer.new()
	_progress_canvas.name = "JumpRouteProgress"
	_progress_canvas.layer = 20
	add_child(_progress_canvas)

	_progress_panel = PanelContainer.new()
	_progress_panel.anchor_left = 0.5
	_progress_panel.anchor_top = 0.5
	_progress_panel.anchor_right = 0.5
	_progress_panel.anchor_bottom = 0.5
	_progress_panel.offset_left = -180.0
	_progress_panel.offset_top = -55.0
	_progress_panel.offset_right = 180.0
	_progress_panel.offset_bottom = 55.0
	_progress_panel.visible = false
	_progress_canvas.add_child(_progress_panel)

	var margin: MarginContainer = MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 16)
	margin.add_theme_constant_override("margin_right", 16)
	margin.add_theme_constant_override("margin_top", 12)
	margin.add_theme_constant_override("margin_bottom", 12)
	_progress_panel.add_child(margin)

	var vbox: VBoxContainer = VBoxContainer.new()
	vbox.add_theme_constant_override("separation", 8)
	margin.add_child(vbox)

	var title: Label = Label.new()
	title.text = "Calculating Jump Routes"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 14)
	vbox.add_child(title)

	_progress_label = Label.new()
	_progress_label.text = ""
	_progress_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_progress_label.custom_minimum_size = Vector2(320.0, 0.0)
	vbox.add_child(_progress_label)


## Shows the progress overlay with an initial message.
## @param message: Status text to display.
func _show_progress(message: String) -> void:
	if _progress_label:
		_progress_label.text = message
	if _progress_panel:
		_progress_panel.visible = true


## Updates the progress overlay message without hiding it.
## @param message: New status text.
func _update_progress(message: String) -> void:
	if _progress_label:
		_progress_label.text = message


## Hides the progress overlay.
func _hide_progress() -> void:
	if _progress_panel:
		_progress_panel.visible = false


# =============================================================================
# Jump lane calculation
# =============================================================================

## Handles the inspector panel's calculate request.
## Guards against calls outside star view, then fires the async coroutine.
func _on_calculate_jump_routes_requested() -> void:
	if _zoom_machine.get_current_level() != GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		set_status("Jump routes require star field view (press ] to navigate there)")
		return
	if _neighborhood_renderer == null or _neighborhood_renderer.get_neighborhood_data() == null:
		set_status("No neighborhood data — move to a star field first")
		return
	# Notify inspector panel so it disables the button while work is in progress.
	if _inspector_panel:
		_inspector_panel.set_jump_routes_calculating(true)
	# Launch coroutine — not awaited so control returns to the caller immediately.
	_calculate_jump_lanes_async()


## Handles the inspector panel's show/hide toggle.
## @param show_routes: Whether to make the renderer visible.
func _on_jump_routes_visibility_toggled(show_routes: bool) -> void:
	if _sector_jump_lane_renderer and _jump_lane_result != null:
		_sector_jump_lane_renderer.visible = show_routes


## Progress callback for jump lane calculation.
## Updates the progress overlay with the current phase and progress.
## @param phase: Current calculation phase name.
## @param current: Current progress value.
## @param total: Total items to process.
func _on_jump_calc_progress(phase: String, current: int, total: int) -> void:
	var pct: int = int((float(current) / float(maxi(total, 1))) * 100.0)
	_update_progress("%s: %d / %d (%d%%)" % [phase, current, total, pct])


## Async coroutine: polls population for every star in the neighborhood,
## builds a JumpLaneRegion, runs the calculator, and renders the result.
## Uses process-frame yields so the progress overlay updates are visible.
func _calculate_jump_lanes_async() -> void:
	var neighborhood_data: SubSectorNeighborhood.NeighborhoodData = (
		_neighborhood_renderer.get_neighborhood_data()
	)
	if neighborhood_data == null:
		_hide_progress()
		if _inspector_panel:
			_inspector_panel.set_jump_routes_calculating(false)
		return

	var star_count: int = neighborhood_data.star_positions.size()
	var start_time: int = Time.get_ticks_msec()

	_show_progress("Polling %d star systems (0%%)…" % star_count)
	set_status("Calculating jump routes for %d stars…" % star_count)
	await get_tree().process_frame
	await get_tree().process_frame

	# Build or extend the jump lane region.
	# On first calculation, create fresh. On recalculation, merge in any new stars
	# (stars not already in the region from a previous run).
	if _jump_lane_region == null:
		_jump_lane_region = JumpLaneRegion.new(
			JumpLaneRegion.RegionScope.SECTOR,
			"neighborhood_%d" % galaxy_seed
		)

	# Build a set of already-known system IDs for fast lookup.
	var known_ids: Dictionary = {}
	for existing_system in _jump_lane_region.systems:
		known_ids[existing_system.id] = true

	var new_stars_added: int = 0
	for i in range(star_count):
		var pos: Vector3 = neighborhood_data.star_positions[i]
		var seed_val: int = neighborhood_data.star_seeds[i]
		var system_id: String = str(seed_val)

		# Update population on existing systems (may have been generated since last calc).
		if known_ids.has(system_id):
			var existing: JumpLaneSystem = _jump_lane_region.get_system(system_id)
			if existing != null and existing.population == 0:
				# Try to fill in population if we now have it cached.
				if _galaxy.has_cached_system(seed_val):
					var cached: SolarSystem = _galaxy.get_cached_system(seed_val)
					existing.population = cached.get_total_population()
			if (i + 1) % 50 == 0 or i == star_count - 1:
				var elapsed: float = (Time.get_ticks_msec() - start_time) / 1000.0
				var pct: int = int((float(i + 1) / float(star_count)) * 100.0)
				_update_progress("Checking existing systems: %d / %d (%d%%) — %.1fs" % [i + 1, star_count, pct, elapsed])
				await get_tree().process_frame
			continue

		var population: int = 0
		if _galaxy.has_cached_system(seed_val):
			var cached: SolarSystem = _galaxy.get_cached_system(seed_val)
			population = cached.get_total_population()
		else:
			var galaxy_star: GalaxyStar = GalaxyStar.create_with_derived_properties(
				pos, seed_val, _spec
			)
			var system: SolarSystem = GalaxySystemGenerator.generate_system(
				galaxy_star, false, true
			)
			if system != null:
				_galaxy.cache_system(seed_val, system)
				population = system.get_total_population()

		_jump_lane_region.add_system(JumpLaneSystem.new(system_id, pos, population))
		known_ids[system_id] = true
		new_stars_added += 1

		if (i + 1) % 50 == 0 or i == star_count - 1:
			var elapsed: float = (Time.get_ticks_msec() - start_time) / 1000.0
			var pct: int = int((float(i + 1) / float(star_count)) * 100.0)
			_update_progress("Polling star systems: %d / %d (%d%%) — %.1fs" % [i + 1, star_count, pct, elapsed])
			await get_tree().process_frame

	var poll_time: float = (Time.get_ticks_msec() - start_time) / 1000.0
	var region_size: int = _jump_lane_region.get_system_count()
	_update_progress("Building jump network for %d systems (%d new)…" % [region_size, new_stars_added])
	set_status("Building jump network (%d systems, %d new, polled in %.1fs)…" % [region_size, new_stars_added, poll_time])

	var calc_start: int = Time.get_ticks_msec()
	var progress_cb: Callable = Callable(self , "_on_jump_calc_progress")
	_jump_lane_result = await _jump_calculator.calculate_async(_jump_lane_region, get_tree(), progress_cb)
	var calc_time: float = (Time.get_ticks_msec() - calc_start) / 1000.0
	var total_time: float = (Time.get_ticks_msec() - start_time) / 1000.0

	# Render — respect the current checkbox state.
	var renderer: SectorJumpLaneRenderer = _sector_jump_lane_renderer as SectorJumpLaneRenderer
	if renderer:
		renderer.render(_jump_lane_result)
		var show_now: bool = true
		if _inspector_panel:
			show_now = _inspector_panel.get_show_routes_checked()
		renderer.visible = show_now

	_hide_progress()

	if _inspector_panel:
		_inspector_panel.set_jump_routes_available(true)

	var counts: Dictionary = _jump_lane_result.get_connection_counts()
	set_status(
		"Jump routes: %d conn (%dG %dY %dO %dR), %d orphans — %.1fs (poll %.1fs, calc %.1fs)" % [
			_jump_lane_result.get_total_connections(),
			counts.get(JumpLaneConnection.ConnectionType.GREEN, 0),
			counts.get(JumpLaneConnection.ConnectionType.YELLOW, 0),
			counts.get(JumpLaneConnection.ConnectionType.ORANGE, 0),
			counts.get(JumpLaneConnection.ConnectionType.RED, 0),
			_jump_lane_result.get_total_orphans(),
			total_time,
			poll_time,
			calc_time,
		]
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


## Connects UI element signals.
func _connect_ui_signals() -> void:
	if _show_compass_check:
		_show_compass_check.toggled.connect(_on_show_compass_toggled)
	if _inspector_panel:
		_inspector_panel.open_system_requested.connect(_on_inspector_open_system)
		_inspector_panel.calculate_jump_routes_requested.connect(_on_calculate_jump_routes_requested)
		_inspector_panel.jump_routes_visibility_toggled.connect(_on_jump_routes_visibility_toggled)
	if _save_button:
		_save_button.pressed.connect(_on_save_pressed)
	if _load_button:
		_load_button.pressed.connect(_on_load_pressed)
	if _new_galaxy_button:
		_new_galaxy_button.pressed.connect(_on_new_galaxy_pressed)


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


## Returns the Galaxy data model instance.
## @return: Galaxy instance.
func get_galaxy() -> Galaxy:
	return _galaxy


## Returns the current galaxy config (used for generation).
## @return: GalaxyConfig instance.
func get_galaxy_config() -> GalaxyConfig:
	if _galaxy_config == null:
		return GalaxyConfig.create_default()
	return _galaxy_config


## Sets the galaxy config (call before _ready or before apply_save_data).
## @param config: GalaxyConfig to use for next build.
func set_galaxy_config(config: GalaxyConfig) -> void:
	if config != null:
		_galaxy_config = config
	else:
		_galaxy_config = GalaxyConfig.create_default()


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
	_save_load.save_state(self )


## Restores the previously saved viewer state.
## Called when returning from system viewer.
func restore_state() -> void:
	_save_load.restore_state(self )


## Clears saved state.
func clear_saved_state() -> void:
	_save_load.clear_saved_state(self )


## Returns true if there is saved state to restore.
## @return: True if state was previously saved.
func has_saved_state() -> bool:
	return _save_load.has_saved_state(self )


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
	_save_load.on_save_pressed(self )


## Handles load button press.
func _on_load_pressed() -> void:
	_save_load.on_load_pressed(self )


## Handles new galaxy button press.
func _on_new_galaxy_pressed() -> void:
	new_galaxy_requested.emit()


## Changes the galaxy seed and regenerates galaxy data.
## @param new_seed: New galaxy seed.
func _change_galaxy_seed(new_seed: int) -> void:
	galaxy_seed = new_seed
	if _galaxy_config == null:
		_galaxy_config = GalaxyConfig.create_default()
	_galaxy = Galaxy.new(_galaxy_config, galaxy_seed)
	_spec = _galaxy.spec

	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = _spec.galaxy_seed

	var sample: GalaxySample = DensitySampler.sample_galaxy(
		_spec, num_points, rng
	)

	# Guard against calls before _ready() completes
	if _galaxy_renderer:
		_galaxy_renderer.build_from_sample(sample, star_size, _spec.galaxy_type)
	if _quadrant_renderer:
		_quadrant_renderer.build_from_density(_spec, _galaxy.density_model)

	if _quadrant_selector:
		_quadrant_selector.clear_selection()
	_selected_sector = null

	_update_seed_display()
	galaxy_seed_changed.emit(new_seed)


## Returns current save data (for testing).
## @return: GalaxySaveData with current state.
func get_save_data() -> GalaxySaveData:
	return _save_load.create_save_data(self )


## Applies save data (for testing).
## @param data: GalaxySaveData to apply.
func apply_save_data(data: GalaxySaveData) -> void:
	_save_load.apply_save_data(self , data)


## Returns the zoom state machine.
## @return: The ZoomStateMachine instance.
func get_zoom_machine() -> ZoomStateMachine:
	return _zoom_machine


## Returns the quadrant selector.
## @return: The QuadrantSelector instance.
func get_quadrant_selector() -> QuadrantSelector:
	return _quadrant_selector


## Returns the quadrant-level grid cursor.
## @return: The GridCursor for quadrant navigation.
func get_quadrant_cursor() -> GridCursor:
	return _quadrant_cursor


## Returns the sector-level grid cursor.
## @return: The GridCursor for sector navigation.
func get_sector_cursor() -> GridCursor:
	return _sector_cursor


## Returns the sector renderer.
## @return: The SectorRenderer instance.
func get_sector_renderer() -> SectorRenderer:
	return _sector_renderer


## Returns the star-view camera.
## @return: The StarViewCamera instance.
func get_star_camera() -> StarViewCamera:
	return _star_camera


## Returns the neighborhood renderer.
## @return: The NeighborhoodRenderer instance.
func get_neighborhood_renderer() -> NeighborhoodRenderer:
	return _neighborhood_renderer

## Returns the density model for the current galaxy.
## @return: The DensityModelInterface instance.
func get_density_model() -> DensityModelInterface:
	return _galaxy.density_model


## Returns the reference density used for star brightness.
## @return: Reference density value.
func get_reference_density() -> float:
	return _galaxy.reference_density


## Returns the selection indicator.
## @return: The SelectionIndicator instance.
func get_selection_indicator() -> SelectionIndicator:
	return _selection_indicator


## Returns the saved zoom level for state restoration.
## @return: Saved zoom level (-1 if none).
func get_saved_zoom_level() -> int:
	return _saved_zoom_level


## Sets the saved zoom level for state restoration.
## @param level: Zoom level to save.
func set_saved_zoom_level(level: int) -> void:
	_saved_zoom_level = level


## Returns the saved quadrant coordinates.
## @return: Saved quadrant (Vector3i or null).
func get_saved_quadrant() -> Variant:
	return _saved_quadrant


## Sets the saved quadrant coordinates.
## @param v: Quadrant to save (Vector3i or null).
func set_saved_quadrant(v: Variant) -> void:
	_saved_quadrant = v


## Returns the saved sector coordinates.
## @return: Saved sector (Vector3i or null).
func get_saved_sector() -> Variant:
	return _saved_sector


## Sets the saved sector coordinates.
## @param v: Sector to save (Vector3i or null).
func set_saved_sector(v: Variant) -> void:
	_saved_sector = v


## Returns the saved star camera position.
## @return: Saved camera position.
func get_saved_star_camera_position() -> Vector3:
	return _saved_star_camera_position


## Sets the saved star camera position.
## @param v: Camera position to save.
func set_saved_star_camera_position(v: Vector3) -> void:
	_saved_star_camera_position = v


## Returns the saved star camera rotation.
## @return: Saved camera rotation (Euler angles).
func get_saved_star_camera_rotation() -> Vector3:
	return _saved_star_camera_rotation


## Sets the saved star camera rotation.
## @param v: Camera rotation to save (Euler angles).
func set_saved_star_camera_rotation(v: Vector3) -> void:
	_saved_star_camera_rotation = v


## Calls the internal _apply_star_selection method (for save/load restore).
## @param world_position: Star world position.
## @param star_seed: Star seed.
func call_apply_star_selection(world_position: Vector3, star_seed: int) -> void:
	_apply_star_selection(world_position, star_seed, true)


## Returns the saved star seed for state restoration.
## @return: Saved star seed (0 if none).
func get_saved_star_seed() -> int:
	return _saved_star_seed


## Sets the saved star seed for state restoration.
## @param v: Star seed to save.
func set_saved_star_seed(v: int) -> void:
	_saved_star_seed = v


## Returns the saved star position for state restoration.
## @return: Saved star position.
func get_saved_star_position() -> Vector3:
	return _saved_star_position


## Sets the saved star position for state restoration.
## @param v: Star position to save.
func set_saved_star_position(v: Vector3) -> void:
	_saved_star_position = v


## Returns the currently selected sector (internal accessor).
## @return: Selected sector (Vector3i or null).
func get_selected_sector_internal() -> Variant:
	return _selected_sector


## Sets the currently selected sector (internal accessor).
## @param v: Sector to select (Vector3i or null).
func set_selected_sector_internal(v: Variant) -> void:
	_selected_sector = v


## Returns the currently selected star seed (internal accessor).
## @return: Selected star seed (0 if none).
func get_selected_star_seed_internal() -> int:
	return _selected_star_seed


## Sets the currently selected star seed (internal accessor).
## @param v: Star seed to select.
func set_selected_star_seed_internal(v: int) -> void:
	_selected_star_seed = v


## Returns the currently selected star position (internal accessor).
## @return: Selected star position.
func get_selected_star_position_internal() -> Vector3:
	return _selected_star_position


## Sets the currently selected star position (internal accessor).
## @param v: Star position to select.
func set_selected_star_position_internal(v: Vector3) -> void:
	_selected_star_position = v


## Calls the internal initialize_at_home method (for save/load).
func call_initialize_at_home() -> void:
	_initialize_at_home()


## Calls the internal apply_zoom_level method (for save/load).
## @param level: Zoom level to apply.
func call_apply_zoom_level(level: int) -> void:
	_apply_zoom_level(level)


## Calls the internal update_inspector method (for save/load).
func call_update_inspector() -> void:
	_update_inspector()


## Calls the internal change_galaxy_seed method (for save/load).
## @param seed_value: New galaxy seed.
func call_change_galaxy_seed(seed_value: int) -> void:
	_change_galaxy_seed(seed_value)


## Returns the current star preview (for testing and MainApp handoff).
## @return: PreviewData or null if no star selected or preview not yet generated.
func get_star_preview() -> StarSystemPreview.PreviewData:
	return _star_preview


## Returns the current jump lane region (for save/load).
## @return: JumpLaneRegion or null.
func get_jump_lane_region() -> JumpLaneRegion:
	return _jump_lane_region


## Sets the jump lane region (for save/load restore).
## @param region: JumpLaneRegion to restore.
func set_jump_lane_region(region: JumpLaneRegion) -> void:
	_jump_lane_region = region


## Returns the current jump lane result (for save/load).
## @return: JumpLaneResult or null.
func get_jump_lane_result() -> JumpLaneResult:
	return _jump_lane_result


## Sets the jump lane result and re-renders (for save/load restore).
## @param result: JumpLaneResult to restore.
func set_jump_lane_result(result: JumpLaneResult) -> void:
	_jump_lane_result = result
	if result != null and _sector_jump_lane_renderer != null:
		(_sector_jump_lane_renderer as SectorJumpLaneRenderer).render(result)
		var show_now: bool = true
		if _inspector_panel:
			show_now = _inspector_panel.get_show_routes_checked()
		_sector_jump_lane_renderer.visible = show_now
		if _inspector_panel:
			_inspector_panel.set_jump_routes_available(true)
	elif result == null and _sector_jump_lane_renderer != null:
		(_sector_jump_lane_renderer as SectorJumpLaneRenderer).clear()
		_sector_jump_lane_renderer.visible = false


## Simulates a star being selected (for integration testing without real raycast).
## @param star_seed: Seed of the star to select.
## @param world_position: World position of the star.
func simulate_star_selected(star_seed: int, world_position: Vector3) -> void:
	_apply_star_selection(world_position, star_seed)


## Simulates clearing the star selection (for integration testing).
func simulate_star_deselected() -> void:
	_clear_star_selection_state()


## Simulates the user pressing Enter to open the selected system (for integration testing).
func simulate_open_selected_system() -> void:
	_try_open_selected_system()
