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


## Emitted when the user clicks a star in star view.
## @param world_position: World-space position of the selected star.
## @param star_seed: Deterministic seed of the selected star system.
signal star_selected(world_position: Vector3, star_seed: int)

## Master seed for the galaxy.
@export var galaxy_seed: int = 42

## Total number of sample points to render at galaxy level.
@export var num_points: int = 100000

## World-space size of each star billboard quad.
@export var star_size: float = 80.0

## Opacity of the galaxy point cloud when dimmed.
const GALAXY_DIMMED_OPACITY: float = 0.15

## Opacity of the galaxy point cloud when in galaxy view.
const GALAXY_FULL_OPACITY: float = 1.0

## Opacity of the galaxy point cloud in star view — dimmer to let local stars pop.
const GALAXY_STAR_VIEW_OPACITY: float = 0.05

## Opacity of the galaxy point cloud in star view — dimmer to let local stars stand out.
const GALAXY_STARVIEW_OPACITY: float = 0.07

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
	_build_environment()

	_apply_zoom_level(GalaxyCoordinates.ZoomLevel.GALAXY)


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


## Handles escape to deselect star within subsector view.
func _handle_escape() -> void:
	if _zoom_machine.get_current_level() == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		_selection_indicator.hide_indicator()


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


## Raycasts to pick a star using the star view camera.
func _pick_star_at(screen_position: Vector2) -> void:
	var ray_origin: Vector3 = _star_camera.project_ray_origin(screen_position)
	var ray_direction: Vector3 = _star_camera.project_ray_normal(screen_position)

	var result: Variant = _neighborhood_renderer.pick_star(ray_origin, ray_direction)

	if result != null:
		var pick: StarPicker.PickResult = result as StarPicker.PickResult
		_selection_indicator.show_at(pick.world_position)
		star_selected.emit(pick.world_position, pick.star_seed)
	else:
		_selection_indicator.hide_indicator()


## Sets the selected sector.
func _select_sector(coords: Vector3i) -> void:
	_sector_cursor.position = coords
	_selected_sector = coords
	_sector_renderer.set_highlight(coords)


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
	_compass.visible = true

	if _quadrant_selector.has_selection():
		_quadrant_renderer.set_highlight(_quadrant_selector.selected_coords)

	_orbit_camera.reconfigure_constraints(200.0, 60000.0, Vector3.ZERO)


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
	_compass.visible = true

	if _selected_sector != null:
		_sector_renderer.set_highlight(_selected_sector)

	_orbit_camera.reconfigure_constraints(50.0, 5000.0, quadrant_center)


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


## Sets up a dark environment with glow post-processing.
func _build_environment() -> void:
	var env: Environment = Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.0, 0.0, 0.02)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color.BLACK
	env.glow_enabled = true
	env.glow_intensity = 0.8
	env.glow_bloom = 0.2
	env.glow_hdr_threshold = 0.0

	var world_env: WorldEnvironment = WorldEnvironment.new()
	world_env.name = "WorldEnvironment"
	world_env.environment = env
	add_child(world_env)
