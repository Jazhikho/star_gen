## Handles galaxy viewer save/load and state restore.
## Extracted from GalaxyViewer to keep script size under CLAUDE limit.
class_name GalaxyViewerSaveLoad
extends RefCounted

const _GalaxyConfigRef: GDScript = preload("res://src/domain/galaxy/GalaxyConfig.gd")
const _GalaxyClass: GDScript = preload("res://src/domain/galaxy/Galaxy.gd")


## Saves the current viewer state for later restoration.
## @param viewer: GalaxyViewer instance (must expose getters used below).
func save_state(viewer: Node) -> void:
	var zoom_machine: ZoomStateMachine = viewer.get_zoom_machine()
	if zoom_machine:
		viewer.set_saved_zoom_level(zoom_machine.get_current_level())
	else:
		viewer.set_saved_zoom_level(GalaxyCoordinates.ZoomLevel.SUBSECTOR)

	var quadrant_selector: QuadrantSelector = viewer.get_quadrant_selector()
	if quadrant_selector and quadrant_selector.has_selection():
		viewer.set_saved_quadrant(quadrant_selector.selected_coords)
	else:
		viewer.set_saved_quadrant(null)

	viewer.set_saved_sector(viewer.get_selected_sector_internal())

	var saved_level: int = viewer.get_saved_zoom_level()
	if saved_level == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		var star_cam: StarViewCamera = viewer.get_star_camera()
		if star_cam and star_cam.is_inside_tree():
			viewer.set_saved_star_camera_position(star_cam.global_position)
			viewer.set_saved_star_camera_rotation(star_cam.rotation)

	# Save star selection so it can be restored when returning from system view.
	viewer.set_saved_star_seed(viewer.get_selected_star_seed_internal())
	viewer.set_saved_star_position(viewer.get_selected_star_position_internal())


## Restores the previously saved viewer state.
## @param viewer: GalaxyViewer instance.
func restore_state(viewer: Node) -> void:
	if viewer.get_saved_zoom_level() < 0:
		viewer.call_initialize_at_home()
		return
	# Restore jump lane state first (set_jump_lane_result renders if in subsector view).
	# We restore region separately so incremental recalc works after returning.
	# (These are stored on the viewer directly, not in saved_* fields, so they
	#  survive the transition without needing explicit save/restore here — but we
	#  DO need to re-render if we're returning to subsector view.)
	var jl_result: JumpLaneResult = viewer.get_jump_lane_result()
	if jl_result != null:
		# Re-render so the mesh is recreated after the scene transition.
		viewer.set_jump_lane_result(jl_result)

	if viewer.get_saved_quadrant() != null and viewer.get_quadrant_selector():
		var quadrant_coords: Vector3i = viewer.get_saved_quadrant() as Vector3i
		if viewer.get_quadrant_cursor():
			viewer.get_quadrant_cursor().position = quadrant_coords
		viewer.get_quadrant_selector().set_selection(quadrant_coords)

	viewer.set_selected_sector_internal(viewer.get_saved_sector())
	if viewer.get_saved_sector() != null and viewer.get_sector_cursor():
		viewer.get_sector_cursor().position = viewer.get_saved_sector() as Vector3i

	if viewer.get_saved_quadrant() != null and viewer.get_sector_renderer() and \
	   (viewer.get_saved_zoom_level() == GalaxyCoordinates.ZoomLevel.SECTOR or \
		viewer.get_saved_zoom_level() == GalaxyCoordinates.ZoomLevel.SUBSECTOR):
		viewer.get_sector_renderer().build_for_quadrant(
			viewer.get_saved_quadrant() as Vector3i, viewer.get_density_model()
		)

	if viewer.get_zoom_machine():
		viewer.get_zoom_machine().set_level(viewer.get_saved_zoom_level())
	viewer.call_apply_zoom_level(viewer.get_saved_zoom_level())

	if viewer.get_saved_zoom_level() == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		var star_cam: StarViewCamera = viewer.get_star_camera()
		if star_cam and star_cam.is_inside_tree():
			star_cam.global_position = viewer.get_saved_star_camera_position()
			star_cam.rotation = viewer.get_saved_star_camera_rotation()
			if viewer.get_neighborhood_renderer():
				viewer.get_neighborhood_renderer().build_neighborhood(
					star_cam.global_position, viewer.galaxy_seed,
					viewer.get_density_model(), viewer.get_reference_density()
				)

	# Restore star selection and regenerate preview if a star was selected.
	var saved_star_seed: int = viewer.get_saved_star_seed()
	var saved_star_pos: Vector3 = viewer.get_saved_star_position()
	if saved_star_seed != 0 and viewer.get_saved_zoom_level() == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		# Re-apply the full star selection (updates indicator, inspector, and preview).
		viewer.call_apply_star_selection(saved_star_pos, saved_star_seed)
	else:
		# No star was selected — ensure indicator is hidden and inspector is clear.
		if viewer.get_selection_indicator():
			viewer.get_selection_indicator().hide_indicator()
	viewer.call_update_inspector()
	viewer.set_status("Returned to galaxy view")


## Clears saved state.
func clear_saved_state(viewer: Node) -> void:
	viewer.set_saved_zoom_level(-1)
	viewer.set_saved_quadrant(null)
	viewer.set_saved_sector(null)
	viewer.set_saved_star_camera_position(Vector3.ZERO)
	viewer.set_saved_star_camera_rotation(Vector3.ZERO)
	viewer.set_saved_star_seed(0)
	viewer.set_saved_star_position(Vector3.ZERO)


## Returns true if there is saved state to restore.
func has_saved_state(viewer: Node) -> bool:
	return viewer.get_saved_zoom_level() >= 0


## Creates save data from current viewer state.
## @param viewer: GalaxyViewer instance.
## @return: GalaxySaveData with current state.
func create_save_data(viewer: Node) -> GalaxySaveData:
	var data: GalaxySaveData = GalaxySaveData.create(int(Time.get_unix_time_from_system()))
	data.galaxy_seed = viewer.galaxy_seed
	var config: GalaxyConfig = viewer.get_galaxy_config()
	if config != null:
		data.set_config(config)
	var galaxy: Galaxy = viewer.get_galaxy()
	if galaxy != null:
		data.cached_system_count = galaxy.get_cached_system_count()
	if viewer.get_zoom_machine():
		data.zoom_level = viewer.get_zoom_machine().get_current_level()
	if viewer.get_quadrant_selector() and viewer.get_quadrant_selector().has_selection():
		data.selected_quadrant = viewer.get_quadrant_selector().selected_coords
	data.selected_sector = viewer.get_selected_sector_internal()
	var star_cam: StarViewCamera = viewer.get_star_camera()
	if star_cam and star_cam.is_inside_tree():
		data.camera_position = star_cam.global_position
		data.camera_rotation = star_cam.rotation
	data.has_star_selection = viewer.get_selected_star_seed_internal() != 0
	data.selected_star_seed = viewer.get_selected_star_seed_internal()
	data.selected_star_position = viewer.get_selected_star_position_internal()

	# Persist jump lane state so routes survive save/load round-trips.
	var jl_region: JumpLaneRegion = viewer.get_jump_lane_region()
	if jl_region != null:
		data.jump_lane_region_data = jl_region.to_dict()

	var jl_result: JumpLaneResult = viewer.get_jump_lane_result()
	if jl_result != null:
		data.jump_lane_result_data = jl_result.to_dict()

	# Persist body overrides from MainApp (viewer's grandparent).
	var main_app: Node = viewer.get_parent()
	if main_app:
		main_app = main_app.get_parent()
	if main_app and main_app.has_method("get_body_overrides"):
		var ov: Variant = main_app.get_body_overrides()
		if ov != null:
			data.set_body_overrides(ov)

	return data


## Applies save data to restore viewer state.
## @param viewer: GalaxyViewer instance.
## @param data: GalaxySaveData to apply.
func apply_save_data(viewer: Node, data: GalaxySaveData) -> void:
	if data.has_config():
		var loaded_config: GalaxyConfig = data.get_config()
		if loaded_config != null:
			viewer.set_galaxy_config(loaded_config)
	if data.galaxy_seed != viewer.galaxy_seed:
		viewer.call_change_galaxy_seed(data.galaxy_seed)

	if data.selected_quadrant != null and viewer.get_quadrant_selector():
		var quadrant_coords: Vector3i = data.selected_quadrant as Vector3i
		if viewer.get_quadrant_cursor():
			viewer.get_quadrant_cursor().position = quadrant_coords
		viewer.get_quadrant_selector().set_selection(quadrant_coords)
	else:
		if viewer.get_quadrant_selector():
			viewer.get_quadrant_selector().clear_selection()

	viewer.set_selected_sector_internal(data.selected_sector)
	if data.selected_sector != null and viewer.get_sector_cursor():
		viewer.get_sector_cursor().position = data.selected_sector as Vector3i

	if data.selected_quadrant != null and viewer.get_sector_renderer() and \
	   (data.zoom_level == GalaxyCoordinates.ZoomLevel.SECTOR or \
		data.zoom_level == GalaxyCoordinates.ZoomLevel.SUBSECTOR):
		viewer.get_sector_renderer().build_for_quadrant(
			data.selected_quadrant as Vector3i, viewer.get_density_model()
		)

	if viewer.get_zoom_machine():
		viewer.get_zoom_machine().set_level(data.zoom_level)
	viewer.call_apply_zoom_level(data.zoom_level)

	if data.zoom_level == GalaxyCoordinates.ZoomLevel.SUBSECTOR:
		var star_cam: StarViewCamera = viewer.get_star_camera()
		if star_cam and star_cam.is_inside_tree():
			star_cam.global_position = data.camera_position
			star_cam.rotation = data.camera_rotation
			if viewer.get_neighborhood_renderer():
				viewer.get_neighborhood_renderer().build_neighborhood(
					star_cam.global_position, viewer.galaxy_seed,
					viewer.get_density_model(), viewer.get_reference_density()
				)

	if data.has_star_selection:
		viewer.set_selected_star_seed_internal(data.selected_star_seed)
		viewer.set_selected_star_position_internal(data.selected_star_position)
		if viewer.get_selection_indicator():
			viewer.get_selection_indicator().show_at(data.selected_star_position)
		if viewer.get_inspector_panel():
			viewer.get_inspector_panel().display_selected_star(
				data.selected_star_position, data.selected_star_seed
			)
	else:
		viewer.set_selected_star_seed_internal(0)
		viewer.set_selected_star_position_internal(Vector3.ZERO)
		if viewer.get_selection_indicator():
			viewer.get_selection_indicator().hide_indicator()

	# Restore jump lane state.
	if not data.jump_lane_region_data.is_empty():
		var region: JumpLaneRegion = JumpLaneRegion.from_dict(data.jump_lane_region_data) as JumpLaneRegion
		viewer.set_jump_lane_region(region)

	if not data.jump_lane_result_data.is_empty():
		var result: JumpLaneResult = JumpLaneResult.from_dict(data.jump_lane_result_data) as JumpLaneResult
		viewer.set_jump_lane_result(result)

	viewer.call_update_inspector()


## Handles save button: opens file dialog.
func on_save_pressed(viewer: Node) -> void:
	var dialog: FileDialog = FileDialog.new()
	dialog.file_mode = FileDialog.FILE_MODE_SAVE_FILE
	dialog.access = FileDialog.ACCESS_USERDATA
	dialog.filters = PackedStringArray(["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"])
	dialog.current_file = "galaxy_%d.sgg" % viewer.galaxy_seed
	dialog.file_selected.connect(func(path: String) -> void: on_save_file_selected(viewer, path))
	dialog.canceled.connect(func() -> void: dialog.queue_free())
	viewer.add_child(dialog)
	dialog.popup_centered(Vector2i(800, 600))


## Handles load button: opens file dialog.
func on_load_pressed(viewer: Node) -> void:
	var dialog: FileDialog = FileDialog.new()
	dialog.file_mode = FileDialog.FILE_MODE_OPEN_FILE
	dialog.access = FileDialog.ACCESS_USERDATA
	dialog.filters = PackedStringArray(["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"])
	dialog.file_selected.connect(func(path: String) -> void: on_load_file_selected(viewer, path))
	dialog.canceled.connect(func() -> void: dialog.queue_free())
	viewer.add_child(dialog)
	dialog.popup_centered(Vector2i(800, 600))


## Handles save file selection from dialog.
func on_save_file_selected(viewer: Node, path: String) -> void:
	var data: GalaxySaveData = create_save_data(viewer)
	var error: String = ""
	if path.ends_with(".json"):
		error = GalaxyPersistence.save_json(path, data)
	else:
		error = GalaxyPersistence.save_binary(path, data)
	if error.is_empty():
		viewer.set_status("Saved to %s" % path.get_file())
	else:
		viewer.set_status("Save failed: %s" % error)
		push_error(error)


## Handles load file selection from dialog.
func on_load_file_selected(viewer: Node, path: String) -> void:
	var data: GalaxySaveData = GalaxyPersistence.load_auto(path)
	if data == null:
		viewer.set_status("Failed to load file")
		return
	if not data.is_valid():
		viewer.set_status("Invalid save data")
		return
	apply_save_data(viewer, data)
	viewer.set_status("Loaded from %s" % path.get_file())
