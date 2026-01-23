## Renders orbital paths as 3D line meshes.
## Creates and manages orbit ellipse visualizations.
class_name OrbitRenderer
extends Node3D

const _system_scale_manager := preload("res://src/app/system_viewer/SystemScaleManager.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")


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

## Currently selected orbit mesh (for highlighting).
var _selected_orbit: MeshInstance3D = null

## Map of body_id -> orbit MeshInstance3D for selection.
var _orbit_meshes: Dictionary = {}


## Clears all rendered orbits.
func clear() -> void:
	for child in get_children():
		child.queue_free()
	_orbit_meshes.clear()
	_selected_orbit = null


## Renders an orbital path for a body.
## @param body_id: Unique ID for this orbit (for selection).
## @param points: Array of Vector3 points defining the orbit.
## @param body_type: Type of the orbiting body (for color selection).
## @return: The created MeshInstance3D.
func add_orbit(
	body_id: String,
	points: PackedVector3Array,
	body_type: CelestialType.Type = CelestialType.Type.PLANET
) -> MeshInstance3D:
	if points.is_empty():
		return null
	
	var color: Color = _get_orbit_color(body_type)
	var mesh_instance: MeshInstance3D = _create_line_mesh(points, color)
	mesh_instance.name = "Orbit_" + body_id
	
	add_child(mesh_instance)
	_orbit_meshes[body_id] = mesh_instance
	
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
	# Restore previous selection
	if _selected_orbit != null:
		var prev_material: StandardMaterial3D = _selected_orbit.material_override as StandardMaterial3D
		if prev_material:
			# Find the original body type from the name
			var original_color: Color = PLANET_ORBIT_COLOR
			_selected_orbit.material_override = _create_line_material(original_color)
		_selected_orbit = null
	
	# Apply new selection
	if body_id.is_empty() or not _orbit_meshes.has(body_id):
		return
	
	_selected_orbit = _orbit_meshes[body_id] as MeshInstance3D
	if _selected_orbit:
		_selected_orbit.material_override = _create_line_material(SELECTED_ORBIT_COLOR)


## Sets visibility of moon orbits.
## @param visible: Whether moon orbits should be visible.
func set_moon_orbits_visible(visible: bool) -> void:
	for body_id in _orbit_meshes:
		var mesh: MeshInstance3D = _orbit_meshes[body_id] as MeshInstance3D
		if mesh and mesh.name.begins_with("Orbit_moon_"):
			mesh.visible = visible


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
