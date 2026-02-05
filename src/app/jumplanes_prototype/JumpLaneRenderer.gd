## Renders jump lane systems and connections in 3D.
class_name JumpLaneRenderer
extends Node3D


## Scale factor: 1 parsec = this many units in 3D space.
const PARSEC_SCALE: float = 1.0

## System marker sizes.
const MARKER_SIZE_BASE: float = 0.3
const MARKER_SIZE_ORPHAN: float = 0.4

## Line width (visual thickness via cylinder radius).
const LINE_RADIUS: float = 0.05

## Container for system markers.
var _systems_container: Node3D

## Container for connection lines.
var _connections_container: Node3D


func _ready() -> void:
	_systems_container = Node3D.new()
	_systems_container.name = "Systems"
	add_child(_systems_container)

	_connections_container = Node3D.new()
	_connections_container.name = "Connections"
	add_child(_connections_container)


## Clears all rendered elements.
func clear() -> void:
	for child in _systems_container.get_children():
		child.queue_free()
	for child in _connections_container.get_children():
		child.queue_free()


## Renders a complete jump lane result.
## @param region: The region with systems.
## @param result: The calculation result.
func render(region: JumpLaneRegion, result: JumpLaneResult) -> void:
	clear()
	_render_connections(result)
	_render_systems(region, result)


## Renders all systems as spheres.
func _render_systems(region: JumpLaneRegion, result: JumpLaneResult) -> void:
	for system in region.systems:
		var marker: MeshInstance3D = _create_system_marker(system, result)
		_systems_container.add_child(marker)


## Creates a sphere marker for a system.
func _create_system_marker(system: JumpLaneSystem, result: JumpLaneResult) -> MeshInstance3D:
	var marker: MeshInstance3D = MeshInstance3D.new()
	marker.name = system.id

	var sphere: SphereMesh = SphereMesh.new()
	var is_orphan: bool = result.is_orphan(system.id)
	if is_orphan:
		sphere.radius = MARKER_SIZE_ORPHAN
	else:
		sphere.radius = MARKER_SIZE_BASE
	sphere.height = sphere.radius * 2
	marker.mesh = sphere

	marker.material_override = _create_system_material(system, result)
	marker.position = system.position * PARSEC_SCALE

	return marker


## Creates material for a system marker.
## @param system: The system being rendered.
## @param result: The calculation result.
## @return: StandardMaterial3D for the marker.
func _create_system_material(system: JumpLaneSystem, result: JumpLaneResult) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = _get_system_color(system, result)
	material.emission_enabled = true
	material.emission = material.albedo_color
	material.emission_energy_multiplier = 0.5
	return material


## Returns the color for a system based on its state.
func _get_system_color(system: JumpLaneSystem, result: JumpLaneResult) -> Color:
	if result.is_orphan(system.id):
		return Color.RED
	if system.is_bridge:
		return Color.CYAN
	if not system.is_populated():
		return Color.GRAY
	var pop_factor: float = clampf(system.population / 100000.0, 0.2, 1.0)
	return Color(pop_factor, pop_factor, 1.0)


## Renders all connections as lines.
func _render_connections(result: JumpLaneResult) -> void:
	for conn in result.connections:
		var line: Node3D = _create_connection_line(conn, result)
		if line != null:
			_connections_container.add_child(line)


## Creates a line between two systems for a connection.
## @param conn: The connection to render.
## @param result: The calculation result.
## @return: MeshInstance3D for the line, or null if invalid.
func _create_connection_line(conn: JumpLaneConnection, result: JumpLaneResult) -> Node3D:
	var source: JumpLaneSystem = result.systems.get(conn.source_id)
	var dest: JumpLaneSystem = result.systems.get(conn.destination_id)

	if source == null or dest == null:
		return null

	var start_pos: Vector3 = source.position * PARSEC_SCALE
	var end_pos: Vector3 = dest.position * PARSEC_SCALE

	return _create_line_between_points(start_pos, end_pos, conn.get_color())


## Creates a cylinder mesh representing a line between two points.
## @param start: Start position in 3D space.
## @param end: End position in 3D space.
## @param color: Line color.
## @return: MeshInstance3D for the line, or null for degenerate case.
func _create_line_between_points(start: Vector3, end: Vector3, color: Color) -> Node3D:
	var length: float = start.distance_to(end)
	if length < 0.001:
		return null

	var mesh_instance: MeshInstance3D = MeshInstance3D.new()

	var cylinder: CylinderMesh = CylinderMesh.new()
	cylinder.top_radius = LINE_RADIUS
	cylinder.bottom_radius = LINE_RADIUS
	cylinder.height = length
	mesh_instance.mesh = cylinder

	mesh_instance.material_override = _create_line_material(color)

	var midpoint: Vector3 = (start + end) / 2.0
	mesh_instance.position = midpoint

	var direction: Vector3 = (end - start).normalized()
	mesh_instance.basis = _create_basis_from_direction(direction)

	return mesh_instance


## Creates a basis that aligns the Y axis with the given direction.
## CylinderMesh uses Y as its axis, so this orients the cylinder along the line.
## @param direction: Normalized direction vector.
## @return: Orthonormal basis with Y aligned to direction.
func _create_basis_from_direction(direction: Vector3) -> Basis:
	var up: Vector3 = Vector3.UP
	if absf(direction.dot(up)) > 0.99:
		up = Vector3.FORWARD

	var x_axis: Vector3 = up.cross(direction).normalized()
	var z_axis: Vector3 = direction.cross(x_axis).normalized()

	return Basis(x_axis, direction, z_axis)


## Creates material for a connection line.
## @param color: Line color.
## @return: StandardMaterial3D for the line.
func _create_line_material(color: Color) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = color
	material.emission_enabled = true
	material.emission = color
	material.emission_energy_multiplier = 0.8
	return material
