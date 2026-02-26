## Renders jump lane connections for the neighborhood view.
## Uses one ArrayMesh (PRIMITIVE_LINES) per connection type for efficiency —
## four draw calls total regardless of connection count.
## Controlled independently from NeighborhoodRenderer; toggled via GalaxyViewer.
class_name SectorJumpLaneRenderer
extends Node3D

# Ensure jump lane class_names are registered before this script parses.
const _jump_lane_connection: GDScript = preload("res://src/domain/jumplanes/JumpLaneConnection.gd")
const _jump_lane_system: GDScript = preload("res://src/domain/jumplanes/JumpLaneSystem.gd")
const _jump_lane_result: GDScript = preload("res://src/domain/jumplanes/JumpLaneResult.gd")

## Connection type colors — slightly desaturated so they read on the dark space background.
const COLOR_GREEN: Color = Color(0.2, 0.9, 0.2, 1.0)
const COLOR_YELLOW: Color = Color(0.9, 0.9, 0.1, 1.0)
const COLOR_ORANGE: Color = Color(0.9, 0.5, 0.1, 1.0)
const COLOR_RED: Color = Color(0.9, 0.15, 0.15, 1.0)

## Emission multiplier applied to all connection lines.
const EMISSION_ENERGY: float = 0.7

## Active MeshInstance3D nodes (one per connection type with any connections).
var _mesh_instances: Array[MeshInstance3D] = []


## Clears all rendered connections.
func clear() -> void:
	for mi in _mesh_instances:
		mi.queue_free()
	_mesh_instances.clear()


## Renders a jump lane result.
## Connections are grouped by type and each group is one ArrayMesh draw call.
## @param result: The JumpLaneResult to visualize. Null clears the renderer.
func render(result: JumpLaneResult) -> void:
	clear()
	if result == null:
		return

	# Accumulate line vertex pairs per connection type.
	var lines: Dictionary = {
		JumpLaneConnection.ConnectionType.GREEN: PackedVector3Array(),
		JumpLaneConnection.ConnectionType.YELLOW: PackedVector3Array(),
		JumpLaneConnection.ConnectionType.ORANGE: PackedVector3Array(),
		JumpLaneConnection.ConnectionType.RED: PackedVector3Array(),
	}

	for conn in result.connections:
		var source: JumpLaneSystem = null
		var dest: JumpLaneSystem = null
		if result.systems.has(conn.source_id):
			source = result.systems[conn.source_id] as JumpLaneSystem
		if result.systems.has(conn.destination_id):
			dest = result.systems[conn.destination_id] as JumpLaneSystem
		if source == null or dest == null:
			push_warning("SectorJumpLaneRenderer: missing system for connection %s -> %s" % [conn.source_id, conn.destination_id])
			continue
		if not lines.has(conn.connection_type):
			push_warning("SectorJumpLaneRenderer: unknown connection type %d" % conn.connection_type)
			continue
		var arr: PackedVector3Array = lines[conn.connection_type] as PackedVector3Array
		arr.append(source.position)
		arr.append(dest.position)
		lines[conn.connection_type] = arr

	for conn_type in lines:
		var vertices: PackedVector3Array = lines[conn_type] as PackedVector3Array
		if vertices.size() < 2:
			continue
		_add_line_mesh(vertices, _get_color(conn_type as JumpLaneConnection.ConnectionType))


## Builds and adds a MeshInstance3D with PRIMITIVE_LINES for the given vertices.
## All vertices share the same material color; emission adds visible glow.
## @param vertices: Even-count array of line endpoint pairs.
## @param color: Albedo and emission color.
func _add_line_mesh(vertices: PackedVector3Array, color: Color) -> void:
	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices

	var arr_mesh: ArrayMesh = ArrayMesh.new()
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_LINES, arrays)

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.albedo_color = color
	mat.emission_enabled = true
	mat.emission = color
	mat.emission_energy_multiplier = EMISSION_ENERGY
	# Unshaded so depth-buffer occlusion doesn't hide distant routes.
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.no_depth_test = false

	var mi: MeshInstance3D = MeshInstance3D.new()
	mi.mesh = arr_mesh
	mi.material_override = mat
	add_child(mi)
	_mesh_instances.append(mi)


## Returns the display color for a connection type.
## @param conn_type: The connection type enum value.
## @return: Color for that type.
func _get_color(conn_type: JumpLaneConnection.ConnectionType) -> Color:
	match conn_type:
		JumpLaneConnection.ConnectionType.GREEN:
			return COLOR_GREEN
		JumpLaneConnection.ConnectionType.YELLOW:
			return COLOR_YELLOW
		JumpLaneConnection.ConnectionType.ORANGE:
			return COLOR_ORANGE
		JumpLaneConnection.ConnectionType.RED:
			return COLOR_RED
	return Color.WHITE
