## 3D compass rose rendered in a SubViewport corner overlay.
## Shows 6 axis arrows that rotate with the main camera.
## Click an arrow to navigate the grid cursor in that direction.
class_name NavigationCompass
extends SubViewportContainer


## Emitted when the user clicks a compass direction arrow.
signal direction_pressed(direction: Vector3i)

## Size of the compass viewport in pixels.
const VIEWPORT_SIZE: int = 150

## Distance of the compass camera from origin.
const CAMERA_DISTANCE: float = 3.8

## Camera field of view for the compass.
const CAMERA_FOV: float = 45.0

## Length of each arrow shaft.
const SHAFT_LENGTH: float = 0.7

## Thickness of each arrow shaft.
const SHAFT_THICKNESS: float = 0.1

## Size of each arrow head.
const HEAD_SIZE: float = 0.22

## Padding added to click detection AABBs beyond visual bounds.
const CLICK_PADDING: float = 0.15

## Near limit of click AABB along primary axis.
const CLICK_ZONE_NEAR: float = 0.15

## Far limit of click AABB along primary axis.
const CLICK_ZONE_FAR: float = 1.3

## Cross-axis half-width of click AABB.
const CLICK_ZONE_WIDTH: float = 0.3

## Arrow definitions: direction vector and display color.
const ARROW_POSITIVE_X: Dictionary = {"dir": Vector3i(1, 0, 0), "color": Color(0.9, 0.2, 0.2)}
const ARROW_NEGATIVE_X: Dictionary = {"dir": Vector3i(-1, 0, 0), "color": Color(0.5, 0.1, 0.1)}
const ARROW_POSITIVE_Y: Dictionary = {"dir": Vector3i(0, 1, 0), "color": Color(0.2, 0.9, 0.2)}
const ARROW_NEGATIVE_Y: Dictionary = {"dir": Vector3i(0, -1, 0), "color": Color(0.1, 0.5, 0.1)}
const ARROW_POSITIVE_Z: Dictionary = {"dir": Vector3i(0, 0, 1), "color": Color(0.2, 0.4, 0.9)}
const ARROW_NEGATIVE_Z: Dictionary = {"dir": Vector3i(0, 0, -1), "color": Color(0.1, 0.15, 0.5)}

## The SubViewport that renders the compass scene.
var _sub_viewport: SubViewport

## Camera inside the compass viewport.
var _compass_camera: Camera3D

## Root node for compass 3D content.
var _compass_root: Node3D

## Click detection zones for each arrow: Array of {dir, aabb_min, aabb_max}.
var _click_zones: Array[Dictionary] = []


func _ready() -> void:
	_configure_container()
	_build_sub_viewport()
	_build_compass_scene()


## Syncs the compass camera to match the main camera's viewing angle.
## Call this each frame from the parent viewer.
## @param yaw_deg: Main camera yaw in degrees.
## @param pitch_deg: Main camera pitch in degrees.
func sync_rotation(yaw_deg: float, pitch_deg: float) -> void:
	if _compass_camera == null:
		return

	var yaw_rad: float = deg_to_rad(yaw_deg)
	var pitch_rad: float = deg_to_rad(pitch_deg)

	var offset: Vector3 = Vector3(
		CAMERA_DISTANCE * cos(pitch_rad) * sin(yaw_rad),
		CAMERA_DISTANCE * sin(pitch_rad),
		CAMERA_DISTANCE * cos(pitch_rad) * cos(yaw_rad)
	)

	_compass_camera.global_position = offset
	_compass_camera.look_at(Vector3.ZERO, Vector3.UP)


## Configures the SubViewportContainer sizing and anchoring.
func _configure_container() -> void:
	stretch = true
	custom_minimum_size = Vector2(VIEWPORT_SIZE, VIEWPORT_SIZE)
	size = Vector2(VIEWPORT_SIZE, VIEWPORT_SIZE)
	mouse_filter = Control.MOUSE_FILTER_STOP


## Builds the SubViewport with transparent background.
func _build_sub_viewport() -> void:
	_sub_viewport = SubViewport.new()
	_sub_viewport.size = Vector2i(VIEWPORT_SIZE, VIEWPORT_SIZE)
	_sub_viewport.transparent_bg = true
	_sub_viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	_sub_viewport.handle_input_locally = true
	add_child(_sub_viewport)


## Builds the full 3D compass scene inside the SubViewport.
func _build_compass_scene() -> void:
	_compass_root = Node3D.new()
	_compass_root.name = "CompassRoot"
	_sub_viewport.add_child(_compass_root)

	_build_compass_camera()
	_build_center_sphere()

	var arrow_defs: Array[Dictionary] = [
		ARROW_POSITIVE_X, ARROW_NEGATIVE_X,
		ARROW_POSITIVE_Y, ARROW_NEGATIVE_Y,
		ARROW_POSITIVE_Z, ARROW_NEGATIVE_Z,
	]

	for arrow_def in arrow_defs:
		_build_arrow(arrow_def["dir"] as Vector3i, arrow_def["color"] as Color)


## Creates the compass camera.
func _build_compass_camera() -> void:
	_compass_camera = Camera3D.new()
	_compass_camera.name = "CompassCamera"
	_compass_camera.fov = CAMERA_FOV
	_compass_camera.near = 0.01
	_compass_camera.far = 20.0
	_compass_camera.current = true
	_compass_root.add_child(_compass_camera)


## Creates a small sphere at the compass center for visual reference.
func _build_center_sphere() -> void:
	var sphere_mesh: SphereMesh = SphereMesh.new()
	sphere_mesh.radius = 0.1
	sphere_mesh.height = 0.2

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = Color(0.4, 0.4, 0.4)
	sphere_mesh.material = mat

	var mesh_inst: MeshInstance3D = MeshInstance3D.new()
	mesh_inst.mesh = sphere_mesh
	_compass_root.add_child(mesh_inst)


## Builds a single arrow (shaft + head) and registers its click zone.
## @param direction: Cardinal direction as Vector3i.
## @param color: Display color for the arrow.
func _build_arrow(direction: Vector3i, color: Color) -> void:
	var dir_f: Vector3 = Vector3(direction)
	var mat: StandardMaterial3D = _make_unshaded_material(color)

	# Shaft mesh
	var shaft_mesh: BoxMesh = BoxMesh.new()
	shaft_mesh.size = _get_shaft_size(direction)
	shaft_mesh.material = mat

	var shaft_inst: MeshInstance3D = MeshInstance3D.new()
	shaft_inst.mesh = shaft_mesh
	shaft_inst.position = dir_f * 0.6
	_compass_root.add_child(shaft_inst)

	# Arrow head mesh (wider block at tip)
	var head_mesh: BoxMesh = BoxMesh.new()
	head_mesh.size = _get_head_size(direction)
	head_mesh.material = mat

	var head_inst: MeshInstance3D = MeshInstance3D.new()
	head_inst.mesh = head_mesh
	head_inst.position = dir_f * 1.05
	_compass_root.add_child(head_inst)

	# Register click zone
	var zone: Dictionary = _make_click_zone(direction)
	_click_zones.append(zone)


## Returns the shaft box size for a given axis direction.
## @param direction: Cardinal direction.
## @return: Box size with elongation along the correct axis.
func _get_shaft_size(direction: Vector3i) -> Vector3:
	if direction.x != 0:
		return Vector3(SHAFT_LENGTH, SHAFT_THICKNESS, SHAFT_THICKNESS)
	elif direction.y != 0:
		return Vector3(SHAFT_THICKNESS, SHAFT_LENGTH, SHAFT_THICKNESS)
	else:
		return Vector3(SHAFT_THICKNESS, SHAFT_THICKNESS, SHAFT_LENGTH)


## Returns the arrow head box size for a given axis direction.
## @param direction: Cardinal direction.
## @return: Box size wider on cross-axes, short on primary axis.
func _get_head_size(direction: Vector3i) -> Vector3:
	if direction.x != 0:
		return Vector3(0.12, HEAD_SIZE, HEAD_SIZE)
	elif direction.y != 0:
		return Vector3(HEAD_SIZE, 0.12, HEAD_SIZE)
	else:
		return Vector3(HEAD_SIZE, HEAD_SIZE, 0.12)


## Constructs a click detection AABB for an arrow direction.
## @param direction: Cardinal direction.
## @return: Dictionary with "dir", "aabb_min", "aabb_max".
func _make_click_zone(direction: Vector3i) -> Dictionary:
	var aabb_min: Vector3 = Vector3(-CLICK_ZONE_WIDTH, -CLICK_ZONE_WIDTH, -CLICK_ZONE_WIDTH)
	var aabb_max: Vector3 = Vector3(CLICK_ZONE_WIDTH, CLICK_ZONE_WIDTH, CLICK_ZONE_WIDTH)

	if direction.x > 0:
		aabb_min.x = CLICK_ZONE_NEAR
		aabb_max.x = CLICK_ZONE_FAR
	elif direction.x < 0:
		aabb_min.x = -CLICK_ZONE_FAR
		aabb_max.x = -CLICK_ZONE_NEAR

	if direction.y > 0:
		aabb_min.y = CLICK_ZONE_NEAR
		aabb_max.y = CLICK_ZONE_FAR
	elif direction.y < 0:
		aabb_min.y = -CLICK_ZONE_FAR
		aabb_max.y = -CLICK_ZONE_NEAR

	if direction.z > 0:
		aabb_min.z = CLICK_ZONE_NEAR
		aabb_max.z = CLICK_ZONE_FAR
	elif direction.z < 0:
		aabb_min.z = -CLICK_ZONE_FAR
		aabb_max.z = -CLICK_ZONE_NEAR

	return {
		"dir": direction,
		"aabb_min": aabb_min,
		"aabb_max": aabb_max,
	}


## Creates an unshaded material with the given color.
## @param color: Albedo color.
## @return: Configured StandardMaterial3D.
func _make_unshaded_material(color: Color) -> StandardMaterial3D:
	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = color
	return mat


func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var mb: InputEventMouseButton = event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_LEFT and mb.pressed:
			_handle_compass_click(mb.position)
			accept_event()


## Raycasts from the compass camera through the click position
## and tests against all arrow click zones.
## @param screen_pos: Click position relative to the SubViewportContainer.
func _handle_compass_click(screen_pos: Vector2) -> void:
	if _compass_camera == null:
		return

	var ray_origin: Vector3 = _compass_camera.project_ray_origin(screen_pos)
	var ray_direction: Vector3 = _compass_camera.project_ray_normal(screen_pos)

	var best_direction: Variant = null
	var best_distance: float = INF

	for zone in _click_zones:
		var hit_dist: float = RaycastUtils.ray_intersects_aabb(
			ray_origin, ray_direction,
			zone["aabb_min"] as Vector3, zone["aabb_max"] as Vector3
		)
		if hit_dist >= 0.0 and hit_dist < best_distance:
			best_distance = hit_dist
			best_direction = zone["dir"]

	if best_direction != null:
		direction_pressed.emit(best_direction as Vector3i)
