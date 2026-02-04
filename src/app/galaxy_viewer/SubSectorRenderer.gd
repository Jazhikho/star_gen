## Renders individual star systems within all sub-sectors of a sector.
## Stars are shown as colored billboard points using MultiMesh.
class_name SubSectorRenderer
extends MultiMeshInstance3D


## Billboard size for individual stars at subsector scale.
const STAR_SIZE: float = 1.5

## Base star color — will vary by seed in future.
const STAR_COLOR: Color = Color(0.95, 0.9, 0.8, 1.0)

## Reference to the shader material for opacity control.
var _material: ShaderMaterial

## Cached star data for the current sector.
var _star_data: SubSectorGenerator.SectorStarData


## Builds the star point cloud for a given sector.
## @param galaxy_seed: Galaxy master seed.
## @param quadrant_coords: Quadrant grid coordinates.
## @param sector_local_coords: Sector local coords within quadrant.
## @param density_model: Density model for star count.
## @param reference_density: Max density for normalization.
func build_for_sector(
	galaxy_seed: int,
	quadrant_coords: Vector3i,
	sector_local_coords: Vector3i,
	density_model: DensityModelInterface,
	reference_density: float
) -> void:
	_star_data = SubSectorGenerator.generate_sector_stars(
		galaxy_seed, quadrant_coords, sector_local_coords,
		density_model, reference_density
	)

	if _star_data.get_count() == 0:
		multimesh = null
		return

	_build_multimesh(_star_data)


## Returns the generated star data, or null if not yet built.
## @return: SectorStarData or null.
func get_star_data() -> SubSectorGenerator.SectorStarData:
	return _star_data


## Sets the global opacity of the star points.
## @param alpha: Opacity in [0.0, 1.0].
func set_opacity(alpha: float) -> void:
	if _material:
		_material.set_shader_parameter("global_alpha", clampf(alpha, 0.0, 1.0))


## Constructs the MultiMesh from generated star data.
## @param star_data: Generated star positions and seeds.
func _build_multimesh(star_data: SubSectorGenerator.SectorStarData) -> void:
	var count: int = star_data.get_count()

	var quad: QuadMesh = QuadMesh.new()
	quad.size = Vector2(1.0, 1.0)

	_material = ShaderMaterial.new()
	_material.shader = preload("res://src/app/galaxy_viewer/shaders/star_billboard.gdshader")
	quad.material = _material

	var mm: MultiMesh = MultiMesh.new()
	mm.mesh = quad
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.instance_count = count

	var star_basis: Basis = Basis.IDENTITY.scaled(Vector3(STAR_SIZE, STAR_SIZE, STAR_SIZE))

	for i in range(count):
		var t: Transform3D = Transform3D(star_basis, star_data.positions[i])
		mm.set_instance_transform(i, t)

		var color: Color = _color_from_seed(star_data.star_seeds[i])
		mm.set_instance_color(i, color)

	multimesh = mm


## Derives a rough star color from its seed.
## Maps hash to a spectral-class-inspired color range.
## @param star_seed: Deterministic seed for this star.
## @return: Star display color.
func _color_from_seed(star_seed: int) -> Color:
	# Use low bits of seed to pick a spectral temperature hint
	var hash_val: int = StableHash.hash_integers([star_seed] as Array[int])
	var t: float = float(hash_val & 0xFFFF) / 65535.0

	# Weight toward yellow-white with occasional blue or orange
	var weighted_t: float = t * t

	if weighted_t < 0.1:
		# Hot blue-white (O/B type)
		return Color(0.7, 0.8, 1.0, 1.0)
	elif weighted_t < 0.3:
		# White (A/F type)
		return Color(0.95, 0.95, 1.0, 1.0)
	elif weighted_t < 0.7:
		# Yellow-white (G type, Sun-like)
		return Color(1.0, 0.95, 0.8, 1.0)
	elif weighted_t < 0.9:
		# Orange (K type)
		return Color(1.0, 0.8, 0.5, 1.0)
	else:
		# Red dwarf (M type)
		return Color(1.0, 0.5, 0.3, 0.9)
