## Renders the 7x7x7 subsector neighborhood: stars as shell-faded billboards
## and ghostly wireframe boxes for each subsector boundary.
## Stars in inner shells are brighter; outer shells fade out.
## Transitioning subsectors animate their opacity.
class_name NeighborhoodRenderer
extends Node3D


## Billboard size for individual stars at subsector scale.
const STAR_SIZE: float = 1.5

## Distance (parsecs) below which stars are fully bright.
const FADE_NEAR: float = 25.0

## Distance (parsecs) beyond which stars are fully faded.
const FADE_FAR: float = 45.0

## Maximum lateral distance for ray picking (parsecs).
const PICK_RADIUS: float = 2.0

## Per-shell alpha multipliers: [center, shell1, shell2, shell3].
const SHELL_ALPHAS: Array[float] = [1.0, 0.85, 0.5, 0.2]

## Per-shell wireframe alpha: [center, shell1, shell2, shell3].
const WIRE_SHELL_ALPHAS: Array[float] = [0.2, 0.12, 0.06, 0.03]

## Wireframe base color (RGB, alpha set per shell).
const WIRE_BASE_COLOR: Color = Color(0.3, 0.5, 0.9)

## Center subsector wireframe tint.
const WIRE_CENTER_COLOR: Color = Color(0.4, 0.7, 1.0)

## Duration of fade transitions in seconds.
const TRANSITION_DURATION: float = 0.5

## MultiMeshInstance3D for star rendering.
var _star_mesh_instance: MultiMeshInstance3D

## Shader material for stars (shared, uniforms set globally).
var _star_material: ShaderMaterial

## Wireframe box MeshInstance3D nodes.
var _wireframe_nodes: Array[MeshInstance3D] = []

## Cached neighborhood data for picking.
var _neighborhood_data: SubSectorNeighborhood.NeighborhoodData

## Prebuilt wireframe mesh (shared by all boxes).
var _wireframe_mesh: ArrayMesh

## Shader for wireframe rendering.
var _wire_shader: Shader

## Transition tracking: origin → {material, elapsed, direction}
var _transitioning_wires: Array[Dictionary] = []

## Transition tracking for star opacity changes.
var _transition_elapsed: float = 0.0
var _is_transitioning: bool = false
var _old_star_count: int = 0


func _ready() -> void:
	_wire_shader = preload("shaders/subsector_wire.gdshader")
	_wireframe_mesh = _create_wireframe_box(GalaxyCoordinates.SUBSECTOR_SIZE_PC)
	_star_mesh_instance = MultiMeshInstance3D.new()
	_star_mesh_instance.name = "NeighborhoodStars"
	add_child(_star_mesh_instance)


func _process(delta: float) -> void:
	if _is_transitioning:
		_transition_elapsed += delta
		if _transition_elapsed >= TRANSITION_DURATION:
			_is_transitioning = false
			_finalize_transition()
		else:
			_update_transition()

	_update_wire_transitions(delta)


## Builds the full neighborhood for a camera position with fade transition.
## @param camera_position: World-space camera position.
## @param galaxy_seed: Galaxy master seed.
## @param density_model: Density model.
## @param reference_density: Normalization reference.
func build_neighborhood(
	camera_position: Vector3,
	galaxy_seed: int,
	density_model: DensityModelInterface,
	reference_density: float
) -> void:
	if _neighborhood_data != null:
		_old_star_count = _neighborhood_data.get_star_count()

	_neighborhood_data = SubSectorNeighborhood.build(
		camera_position, galaxy_seed, density_model, reference_density
	)

	_rebuild_stars()
	_rebuild_wireframes()

	# Start transition
	_transition_elapsed = 0.0
	_is_transitioning = true


## Returns the cached neighborhood data for external queries.
## @return: NeighborhoodData or null.
func get_neighborhood_data() -> SubSectorNeighborhood.NeighborhoodData:
	return _neighborhood_data


## Performs a ray pick against all neighborhood stars.
## @param ray_origin: World-space ray origin.
## @param ray_direction: Normalised ray direction.
## @return: StarPicker.PickResult or null.
func pick_star(ray_origin: Vector3, ray_direction: Vector3) -> Variant:
	if _neighborhood_data == null or _neighborhood_data.get_star_count() == 0:
		return null

	return StarPicker.pick_nearest_to_ray(
		ray_origin, ray_direction,
		_neighborhood_data.star_positions, _neighborhood_data.star_seeds,
		PICK_RADIUS
	)


## Returns the shell alpha for a given shell index.
## @param shell: Chebyshev shell index (0-3).
## @return: Alpha multiplier.
static func get_shell_alpha(shell: int) -> float:
	if shell >= 0 and shell < SHELL_ALPHAS.size():
		return SHELL_ALPHAS[shell]
	return 0.1


## Rebuilds the star MultiMesh from neighborhood data with per-instance shell alpha.
func _rebuild_stars() -> void:
	var count: int = _neighborhood_data.get_star_count()
	if count == 0:
		_star_mesh_instance.multimesh = null
		return

	var quad: QuadMesh = QuadMesh.new()
	quad.size = Vector2(1.0, 1.0)

	_star_material = ShaderMaterial.new()
	_star_material.shader = preload("shaders/star_sector_view.gdshader")
	_star_material.set_shader_parameter("fade_near", FADE_NEAR)
	_star_material.set_shader_parameter("fade_far", FADE_FAR)
	quad.material = _star_material

	var mm: MultiMesh = MultiMesh.new()
	mm.mesh = quad
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.use_custom_data = true
	mm.instance_count = count

	var scale_basis: Basis = Basis.IDENTITY.scaled(Vector3(STAR_SIZE, STAR_SIZE, STAR_SIZE))

	for i in range(count):
		var t: Transform3D = Transform3D(scale_basis, _neighborhood_data.star_positions[i])
		mm.set_instance_transform(i, t)
		mm.set_instance_color(i, _color_from_seed(_neighborhood_data.star_seeds[i]))

		var shell: int = _neighborhood_data.star_shells[i]
		var shell_alpha: float = get_shell_alpha(shell)
		mm.set_instance_custom_data(i, Color(shell_alpha, 0.0, 0.0, 0.0))

	_star_mesh_instance.multimesh = mm


## Rebuilds wireframe boxes for all subsectors with per-shell opacity.
func _rebuild_wireframes() -> void:
	for node in _wireframe_nodes:
		node.queue_free()
	_wireframe_nodes.clear()
	_transitioning_wires.clear()

	var center: Vector3 = _neighborhood_data.center_origin
	var half_size: float = GalaxyCoordinates.SUBSECTOR_SIZE_PC * 0.5

	for i in range(_neighborhood_data.subsector_origins.size()):
		var origin: Vector3 = _neighborhood_data.subsector_origins[i]
		var shell: int = _neighborhood_data.subsector_shells[i]
		var is_center: bool = origin.is_equal_approx(center)

		var base_rgb: Color = WIRE_CENTER_COLOR if is_center else WIRE_BASE_COLOR
		var wire_alpha: float = WIRE_SHELL_ALPHAS[mini(shell, WIRE_SHELL_ALPHAS.size() - 1)]
		var wire_color: Color = Color(base_rgb.r, base_rgb.g, base_rgb.b, wire_alpha)

		var wire_node: MeshInstance3D = MeshInstance3D.new()
		wire_node.mesh = _wireframe_mesh
		wire_node.position = origin + Vector3.ONE * half_size

		var mat: ShaderMaterial = ShaderMaterial.new()
		mat.shader = _wire_shader
		mat.set_shader_parameter("wire_color", wire_color)
		wire_node.material_override = mat

		add_child(wire_node)
		_wireframe_nodes.append(wire_node)

		# Outer shell wires fade in
		if shell == SubSectorNeighborhood.EXTENT:
			_transitioning_wires.append({
				"material": mat,
				"target_alpha": wire_alpha,
				"elapsed": 0.0,
				"color_rgb": base_rgb,
			})
			mat.set_shader_parameter("wire_color", Color(base_rgb.r, base_rgb.g, base_rgb.b, 0.0))


## Updates wire fade-in transitions.
## @param delta: Frame delta time.
func _update_wire_transitions(delta: float) -> void:
	var completed: Array[int] = []

	for i in range(_transitioning_wires.size()):
		var tw: Dictionary = _transitioning_wires[i]
		tw["elapsed"] = (tw["elapsed"] as float) + delta
		var t: float = clampf((tw["elapsed"] as float) / TRANSITION_DURATION, 0.0, 1.0)
		var current_alpha: float = lerpf(0.0, tw["target_alpha"] as float, t)
		var rgb: Color = tw["color_rgb"] as Color
		(tw["material"] as ShaderMaterial).set_shader_parameter(
			"wire_color", Color(rgb.r, rgb.g, rgb.b, current_alpha)
		)
		if t >= 1.0:
			completed.append(i)

	# Remove completed transitions in reverse order
	for i in range(completed.size() - 1, -1, -1):
		_transitioning_wires.remove_at(completed[i])


## Updates star transition (global alpha ramp for new stars).
func _update_transition() -> void:
	if _star_material == null:
		return
	var t: float = clampf(_transition_elapsed / TRANSITION_DURATION, 0.0, 1.0)
	_star_material.set_shader_parameter("global_alpha", lerpf(0.5, 1.0, t))


## Finalizes the star transition to full opacity.
func _finalize_transition() -> void:
	if _star_material != null:
		_star_material.set_shader_parameter("global_alpha", 1.0)


## Creates an ArrayMesh of 12 box edges as line primitives.
## @param box_size: Edge length of the cube.
## @return: ArrayMesh with a single PRIMITIVE_LINES surface.
func _create_wireframe_box(box_size: float) -> ArrayMesh:
	var half: float = box_size * 0.5

	var corners: Array[Vector3] = [
		Vector3(-half, -half, -half),
		Vector3(half, -half, -half),
		Vector3(half, half, -half),
		Vector3(-half, half, -half),
		Vector3(-half, -half, half),
		Vector3(half, -half, half),
		Vector3(half, half, half),
		Vector3(-half, half, half),
	]

	var edge_indices: Array[int] = [
		0, 1, 1, 2, 2, 3, 3, 0,
		4, 5, 5, 6, 6, 7, 7, 4,
		0, 4, 1, 5, 2, 6, 3, 7,
	]

	var vertices: PackedVector3Array = PackedVector3Array()
	for idx in edge_indices:
		vertices.append(corners[idx])

	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices

	var arr_mesh: ArrayMesh = ArrayMesh.new()
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_LINES, arrays)
	return arr_mesh


## Derives a star color from its seed.
## @param star_seed: Deterministic seed.
## @return: Display color.
func _color_from_seed(star_seed: int) -> Color:
	var hash_val: int = StableHash.hash_integers([star_seed] as Array[int])
	var t: float = float(hash_val & 0xFFFF) / 65535.0
	var weighted_t: float = t * t

	if weighted_t < 0.1:
		return Color(0.7, 0.8, 1.0, 1.0)
	elif weighted_t < 0.3:
		return Color(0.95, 0.95, 1.0, 1.0)
	elif weighted_t < 0.7:
		return Color(1.0, 0.95, 0.8, 1.0)
	elif weighted_t < 0.9:
		return Color(1.0, 0.8, 0.5, 1.0)
	else:
		return Color(1.0, 0.5, 0.3, 0.9)
