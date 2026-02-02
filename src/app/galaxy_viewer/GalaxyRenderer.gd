## Renders a sampled galaxy as a MultiMesh point cloud with billboard quads.
class_name GalaxyRenderer
extends MultiMeshInstance3D


## World-space half-size of each star billboard quad.
const DEFAULT_STAR_SIZE: float = 80.0

## Base color for bulge-population stars.
const BULGE_COLOR: Color = Color(1.0, 0.85, 0.5, 0.9)

## Base color for disk-population stars.
const DISK_COLOR: Color = Color(0.6, 0.75, 1.0, 0.8)

## Reference to the shader material for opacity control.
var _material: ShaderMaterial


## Builds and assigns the MultiMesh from a galaxy sample.
## @param sample: The sampled galaxy populations.
## @param star_size: Billboard quad half-size in parsecs.
func build_from_sample(sample: GalaxySample, star_size: float = DEFAULT_STAR_SIZE) -> void:
	var total: int = sample.get_total_count()
	if total == 0:
		return

	var quad: QuadMesh = QuadMesh.new()
	quad.size = Vector2(1.0, 1.0)

	_material = ShaderMaterial.new()
	_material.shader = preload("res://src/app/galaxy_viewer/shaders/star_billboard.gdshader")
	quad.material = _material

	var mm: MultiMesh = MultiMesh.new()
	mm.mesh = quad
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.instance_count = total

	var idx: int = 0
	idx = _fill_population(mm, sample.bulge_points, BULGE_COLOR, star_size, idx)
	idx = _fill_population(mm, sample.disk_points, DISK_COLOR, star_size, idx)

	multimesh = mm


## Sets the global opacity of all star points.
## @param alpha: Opacity in [0.0, 1.0].
func set_opacity(alpha: float) -> void:
	if _material:
		_material.set_shader_parameter("global_alpha", clampf(alpha, 0.0, 1.0))


## Writes one population's transforms and colors into the MultiMesh.
## @param mm: The MultiMesh to write into.
## @param points: Positions for this population.
## @param base_color: The base color for this population.
## @param size: Billboard quad size.
## @param start_idx: First instance index to write.
## @return: Next available instance index.
func _fill_population(
	mm: MultiMesh,
	points: PackedVector3Array,
	base_color: Color,
	size: float,
	start_idx: int
) -> int:
	var basis: Basis = Basis.IDENTITY.scaled(Vector3(size, size, size))
	var idx: int = start_idx

	for i in range(points.size()):
		var t: Transform3D = Transform3D(basis, points[i])
		mm.set_instance_transform(idx, t)
		mm.set_instance_color(idx, base_color)
		idx += 1

	return idx
