## Renders a sampled galaxy as a MultiMesh point cloud with billboard quads.
## Supports type-specific coloring for spiral, elliptical, and irregular galaxies.
class_name GalaxyRenderer
extends MultiMeshInstance3D


## World-space half-size of each star billboard quad.
const DEFAULT_STAR_SIZE: float = 80.0

## Base color for bulge-population stars (warm yellow/orange).
const BULGE_COLOR: Color = Color(1.0, 0.85, 0.5, 0.9)

## Base color for disk-population stars (cool blue).
const DISK_COLOR: Color = Color(0.6, 0.75, 1.0, 0.8)

## Color for elliptical galaxy core (warmer, more golden).
const ELLIPTICAL_CORE_COLOR: Color = Color(1.0, 0.8, 0.4, 0.9)

## Color for elliptical galaxy outer regions (slightly cooler gold).
const ELLIPTICAL_OUTER_COLOR: Color = Color(0.95, 0.75, 0.45, 0.85)

## Color for irregular galaxy warm regions (yellow-orange).
const IRREGULAR_WARM_COLOR: Color = Color(1.0, 0.7, 0.4, 0.85)

## Color for irregular galaxy cool regions (blue-white).
const IRREGULAR_COOL_COLOR: Color = Color(0.7, 0.8, 1.0, 0.85)

## Reference to the shader material for opacity control.
var _material: ShaderMaterial

## Current galaxy type for reference.
var _galaxy_type: int = GalaxySpec.GalaxyType.SPIRAL


## Builds and assigns the MultiMesh from a galaxy sample.
## @param sample: The sampled galaxy populations.
## @param star_size: Billboard quad half-size in parsecs.
## @param galaxy_type: The type of galaxy for color selection.
func build_from_sample(
	sample: GalaxySample,
	star_size: float = DEFAULT_STAR_SIZE,
	galaxy_type: int = GalaxySpec.GalaxyType.SPIRAL
) -> void:
	_galaxy_type = galaxy_type
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
	
	match galaxy_type:
		GalaxySpec.GalaxyType.SPIRAL:
			idx = _fill_spiral_galaxy(mm, sample, star_size, idx)
		GalaxySpec.GalaxyType.ELLIPTICAL:
			idx = _fill_elliptical_galaxy(mm, sample, star_size, idx)
		GalaxySpec.GalaxyType.IRREGULAR:
			idx = _fill_irregular_galaxy(mm, sample, star_size, idx)
		_:
			idx = _fill_spiral_galaxy(mm, sample, star_size, idx)

	multimesh = mm


## Sets the global opacity of all star points.
## @param alpha: Opacity in [0.0, 1.0].
func set_opacity(alpha: float) -> void:
	if _material:
		_material.set_shader_parameter("global_alpha", clampf(alpha, 0.0, 1.0))


## Fills the MultiMesh for a spiral galaxy with distinct bulge/disk colors.
## @param mm: The MultiMesh to fill.
## @param sample: Galaxy sample data.
## @param size: Star billboard size.
## @param start_idx: Starting index.
## @return: Next available index.
func _fill_spiral_galaxy(
	mm: MultiMesh,
	sample: GalaxySample,
	star_size: float,
	start_idx: int
) -> int:
	var idx: int = start_idx
	idx = _fill_population(mm, sample.bulge_points, BULGE_COLOR, star_size, idx)
	idx = _fill_population(mm, sample.disk_points, DISK_COLOR, star_size, idx)
	return idx


## Fills the MultiMesh for an elliptical galaxy with warm gradient colors.
## @param mm: The MultiMesh to fill.
## @param sample: Galaxy sample data (only bulge points used).
## @param star_size: Star billboard size.
## @param start_idx: Starting index.
## @return: Next available index.
func _fill_elliptical_galaxy(
	mm: MultiMesh,
	sample: GalaxySample,
	star_size: float,
	start_idx: int
) -> int:
	var idx: int = start_idx
	var scale_basis: Basis = Basis.IDENTITY.scaled(Vector3(star_size, star_size, star_size))
	
	# Find max radius for color gradient
	var max_r: float = 1.0
	for i in range(sample.bulge_points.size()):
		var p: Vector3 = sample.bulge_points[i]
		var r: float = p.length()
		if r > max_r:
			max_r = r
	
	# Fill bulge points with radial color gradient (warmer in center)
	for i in range(sample.bulge_points.size()):
		var p: Vector3 = sample.bulge_points[i]
		var r: float = p.length()
		var t: float = clampf(r / max_r, 0.0, 1.0)
		
		# Interpolate from core to outer color
		var color: Color = ELLIPTICAL_CORE_COLOR.lerp(ELLIPTICAL_OUTER_COLOR, t)
		
		var star_transform: Transform3D = Transform3D(scale_basis, p)
		mm.set_instance_transform(idx, star_transform)
		mm.set_instance_color(idx, color)
		idx += 1
	
	return idx


## Fills the MultiMesh for an irregular galaxy with mixed colors.
## @param mm: The MultiMesh to fill.
## @param sample: Galaxy sample data.
## @param star_size: Star billboard size.
## @param start_idx: Starting index.
## @return: Next available index.
func _fill_irregular_galaxy(
	mm: MultiMesh,
	sample: GalaxySample,
	star_size: float,
	start_idx: int
) -> int:
	var idx: int = start_idx
	var scale_basis: Basis = Basis.IDENTITY.scaled(Vector3(star_size, star_size, star_size))
	
	# Bulge points get warm colors (star-forming regions near center)
	for i in range(sample.bulge_points.size()):
		var p: Vector3 = sample.bulge_points[i]
		# Use distance-based color variation for bulge
		var r: float = p.length()
		var t: float = clampf(r / 5000.0, 0.0, 1.0)
		var color: Color = IRREGULAR_WARM_COLOR.lerp(ELLIPTICAL_CORE_COLOR, t * 0.3)
		
		var star_transform: Transform3D = Transform3D(scale_basis, p)
		mm.set_instance_transform(idx, star_transform)
		mm.set_instance_color(idx, color)
		idx += 1
	
	# Disk points get mixed colors using index-based pseudo-random
	# This avoids spatial patterns entirely
	for i in range(sample.disk_points.size()):
		var p: Vector3 = sample.disk_points[i]
		
		# Use index + position hash for non-spatial randomness
		var hash_input: int = i * 73856093 + int(p.x * 19349663) + int(p.z * 83492791)
		var hash_val: float = float((hash_input & 0xFFFF)) / 65535.0
		
		var color: Color
		if hash_val < 0.35:
			# Young blue stars (star-forming regions)
			color = IRREGULAR_COOL_COLOR
		elif hash_val < 0.65:
			# Mixed intermediate
			var blend: float = (hash_val - 0.35) / 0.3
			color = IRREGULAR_COOL_COLOR.lerp(IRREGULAR_WARM_COLOR, blend)
		else:
			# Older yellow-orange stars
			color = IRREGULAR_WARM_COLOR
		
		var star_transform: Transform3D = Transform3D(scale_basis, p)
		mm.set_instance_transform(idx, star_transform)
		mm.set_instance_color(idx, color)
		idx += 1
	
	return idx


## Writes one population's transforms and colors into the MultiMesh.
## @param mm: The MultiMesh to write into.
## @param points: Positions for this population.
## @param base_color: The base color for this population.
## @param star_size: Billboard quad size.
## @param start_idx: First instance index to write.
## @return: Next available instance index.
func _fill_population(
	mm: MultiMesh,
	points: PackedVector3Array,
	base_color: Color,
	star_size: float,
	start_idx: int
) -> int:
	var scale_basis: Basis = Basis.IDENTITY.scaled(Vector3(star_size, star_size, star_size))
	var idx: int = start_idx

	for i in range(points.size()):
		var star_transform: Transform3D = Transform3D(scale_basis, points[i])
		mm.set_instance_transform(idx, star_transform)
		mm.set_instance_color(idx, base_color)
		idx += 1

	return idx
