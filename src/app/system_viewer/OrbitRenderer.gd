## Renders orbital paths as 3D line meshes.
## Creates and manages orbit ellipse visualizations.
## Supports both static and moving orbits (e.g. planets following stars in binary systems).
class_name OrbitRenderer
extends Node3D

const _system_scale_manager: GDScript = preload("res://src/app/system_viewer/SystemScaleManager.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")


## Default orbit line color for planets.
const PLANET_ORBIT_COLOR: Color = Color(0.3, 0.4, 0.6, 0.6)

## Orbit line color for moons.
const MOON_ORBIT_COLOR: Color = Color(0.4, 0.4, 0.5, 0.4)

## Habitable zone inner color.
const HZ_INNER_COLOR: Color = Color(0.2, 0.6, 0.2, 0.15)

## Habitable zone outer color.
const HZ_OUTER_COLOR: Color = Color(0.1, 0.4, 0.1, 0.1)

## Frost line color.
const FROST_LINE_COLOR: Color = Color(0.3, 0.5, 0.8, 0.2)

## Selected orbit highlight color.
const SELECTED_ORBIT_COLOR: Color = Color(0.8, 0.8, 0.2, 0.9)

## Number of segments per orbit ellipse.
const ORBIT_SEGMENTS: int = 128

## Per-orbit data for position updates and highlighting.
class OrbitData:
	extends RefCounted
	var mesh_instance: MeshInstance3D
	var parent_id: String
	var current_center: Vector3
	var base_color: Color

	func _init() -> void:
		mesh_instance = null
		parent_id = ""
		current_center = Vector3.ZERO
		base_color = Color(0.3, 0.4, 0.6, 0.6)

## Map of orbit_id -> OrbitData (for selection, highlighting, and position updates).
var _orbits: Dictionary = {}

## Currently selected orbit mesh (for highlighting).
var _selected_orbit: MeshInstance3D = null

## ID of the currently highlighted orbit (for restoring base color).
var _selected_orbit_id: String = ""


## Clears all rendered orbits.
func clear() -> void:
	for orbit_id in _orbits:
		var data: OrbitData = _orbits[orbit_id] as OrbitData
		if data.mesh_instance != null:
			data.mesh_instance.queue_free()
	_orbits.clear()
	_selected_orbit = null
	_selected_orbit_id = ""


## Removes a single orbit by ID.
## @param orbit_id: The orbit ID to remove.
func remove_orbit(orbit_id: String) -> void:
	if _orbits.has(orbit_id):
		var data: OrbitData = _orbits[orbit_id] as OrbitData
		if data.mesh_instance != null:
			data.mesh_instance.queue_free()
		_orbits.erase(orbit_id)
		if _selected_orbit_id == orbit_id:
			_selected_orbit = null
			_selected_orbit_id = ""


## Renders an orbital path for a body.
## @param body_id: Unique ID for this orbit (for selection).
## @param points: Array of Vector3 points defining the orbit (world or center-relative).
## @param body_type: Type of the orbiting body (for color selection).
## @param parent_id: Optional host ID for position updates during animation (empty = static).
## @param center: Center of the orbit; when set with parent_id, orbit moves with host.
## @return: The created MeshInstance3D, or null if points empty.
func add_orbit(
	body_id: String,
	points: PackedVector3Array,
	body_type: CelestialType.Type = CelestialType.Type.PLANET,
	parent_id: String = "",
	center: Vector3 = Vector3.ZERO
) -> MeshInstance3D:
	if points.is_empty():
		return null

	if _orbits.has(body_id):
		remove_orbit(body_id)

	var color: Color = _get_orbit_color(body_type)
	var use_relative: bool = not parent_id.is_empty() or center != Vector3.ZERO

	var relative_points: PackedVector3Array = PackedVector3Array()
	if use_relative:
		for i in range(points.size()):
			relative_points.append(points[i] - center)
	else:
		relative_points = points

	var mesh_instance: MeshInstance3D = _create_line_mesh(relative_points, color)
	mesh_instance.name = "Orbit_" + body_id
	if use_relative:
		mesh_instance.position = center

	add_child(mesh_instance)

	var data: OrbitData = OrbitData.new()
	data.mesh_instance = mesh_instance
	data.parent_id = parent_id
	data.current_center = center
	data.base_color = color
	_orbits[body_id] = data

	return mesh_instance


## Renders a zone ring (habitable zone boundary, frost line, etc.).
## @param zone_name: Display name for the zone.
## @param radius_units: Radius in viewport units.
## @param color: Color for the zone ring.
## @param num_points: Number of points in the circle.
## @return: The created MeshInstance3D.
func add_zone_ring(
	zone_name: String,
	radius_units: float,
	color: Color,
	num_points: int = 128
) -> MeshInstance3D:
	if radius_units <= 0.0:
		return null
	
	var points: PackedVector3Array = _generate_circle_points(radius_units, num_points)
	var mesh_instance: MeshInstance3D = _create_line_mesh(points, color)
	mesh_instance.name = "Zone_" + zone_name
	
	add_child(mesh_instance)
	return mesh_instance


## Highlights a specific orbit by body ID.
## @param body_id: The body whose orbit to highlight. Empty string clears highlight.
func highlight_orbit(body_id: String) -> void:
	if _selected_orbit != null and not _selected_orbit_id.is_empty() and _orbits.has(_selected_orbit_id):
		var prev_data: OrbitData = _orbits[_selected_orbit_id] as OrbitData
		_selected_orbit.material_override = _create_line_material(prev_data.base_color)
	_selected_orbit = null
	_selected_orbit_id = ""

	if body_id.is_empty() or not _orbits.has(body_id):
		return

	var data: OrbitData = _orbits[body_id] as OrbitData
	_selected_orbit = data.mesh_instance
	_selected_orbit_id = body_id
	if _selected_orbit:
		_selected_orbit.material_override = _create_line_material(SELECTED_ORBIT_COLOR)


## Sets visibility of moon orbits.
## @param show_moons: Whether moon orbits should be visible.
func set_moon_orbits_visible(show_moons: bool) -> void:
	for orbit_id in _orbits:
		var data: OrbitData = _orbits[orbit_id] as OrbitData
		if data.mesh_instance != null and data.mesh_instance.name.begins_with("Orbit_moon_"):
			data.mesh_instance.visible = show_moons


## Updates orbit positions from current host positions (for moving orbits).
## Call during animation so orbits follow their parent bodies.
## @param host_positions: Dictionary mapping host/parent ID -> Vector3 position.
func update_orbit_positions(host_positions: Dictionary) -> void:
	for orbit_id in _orbits:
		var data: OrbitData = _orbits[orbit_id] as OrbitData
		if data.parent_id.is_empty():
			continue
		if host_positions.has(data.parent_id):
			var new_center: Vector3 = host_positions[data.parent_id] as Vector3
			if new_center != data.current_center:
				data.current_center = new_center
				if data.mesh_instance != null:
					data.mesh_instance.position = new_center


## Returns the number of orbits being rendered.
func get_orbit_count() -> int:
	return _orbits.size()


## Returns true if an orbit with the given ID exists.
func has_orbit(orbit_id: String) -> bool:
	return _orbits.has(orbit_id)


## Creates a line mesh from an array of points.
## @param points: Points defining the line.
## @param color: Color for the line.
## @return: MeshInstance3D with the line mesh.
func _create_line_mesh(points: PackedVector3Array, color: Color) -> MeshInstance3D:
	var mesh_instance: MeshInstance3D = MeshInstance3D.new()
	var immediate_mesh: ImmediateMesh = ImmediateMesh.new()
	
	immediate_mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	for point in points:
		immediate_mesh.surface_add_vertex(point)
	immediate_mesh.surface_end()
	
	mesh_instance.mesh = immediate_mesh
	mesh_instance.material_override = _create_line_material(color)
	
	# Disable shadow casting for lines
	mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	
	return mesh_instance


## Creates a material for orbit lines.
## @param color: The line color.
## @return: StandardMaterial3D configured for lines.
func _create_line_material(color: Color) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = color
	material.emission_enabled = true
	material.emission = Color(color.r, color.g, color.b)
	material.emission_energy_multiplier = 0.3
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	material.no_depth_test = false
	material.render_priority = -1
	return material


## Generates points for a circle in the XZ plane.
## @param radius: Circle radius.
## @param num_points: Number of points.
## @return: Array of Vector3 points.
func _generate_circle_points(radius: float, num_points: int) -> PackedVector3Array:
	var points: PackedVector3Array = PackedVector3Array()
	for i in range(num_points + 1):
		var angle: float = (float(i) / float(num_points)) * TAU
		points.append(Vector3(cos(angle) * radius, 0.0, sin(angle) * radius))
	return points


## Gets the orbit color for a body type.
## @param body_type: The celestial body type.
## @return: Color for the orbit line.
func _get_orbit_color(body_type: CelestialType.Type) -> Color:
	match body_type:
		CelestialType.Type.PLANET:
			return PLANET_ORBIT_COLOR
		CelestialType.Type.MOON:
			return MOON_ORBIT_COLOR
		CelestialType.Type.ASTEROID:
			return Color(0.5, 0.4, 0.3, 0.3)
		_:
			return PLANET_ORBIT_COLOR
