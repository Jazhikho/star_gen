## Renders the sector grid within a single quadrant as translucent colored cubes.
## Each of the 10×10×10 sector cells is colored by stellar density at its center.
## Supports click selection and highlight.
class_name SectorRenderer
extends MultiMeshInstance3D


## Number of sectors per quadrant edge.
const SECTORS_PER_EDGE: int = 10

## Fraction of sector size that each cube occupies.
const CELL_FILL_FRACTION: float = 0.7

## Minimum normalized density to show a sector cell.
const DENSITY_VISIBILITY_THRESHOLD: float = 0.005

## Low-density color.
const COLOR_LOW: Color = Color(0.2, 0.1, 0.6)

## Mid-density color.
const COLOR_MID: Color = Color(0.2, 0.7, 0.9)

## High-density color.
const COLOR_HIGH: Color = Color(1.0, 0.9, 0.4)

## Highlight color — distinct but not blinding with blend_mix.
const HIGHLIGHT_COLOR: Color = Color(0.5, 0.8, 1.0, 0.35)

## Reference to the shader material.
var _material: ShaderMaterial

## Maps sector-local coords to instance index for highlighting.
var _coords_to_index: Dictionary = {}

## Stored colors for restoring after highlight removal.
var _original_colors: PackedColorArray = PackedColorArray()

## Currently highlighted sector local coords, or null.
var _highlighted_coords: Variant = null

## List of occupied sector-local coordinates.
var _occupied_coords: Array[Vector3i] = []

## The quadrant this sector grid was built for.
var _current_quadrant: Variant = null


## Returns the list of occupied sector-local grid coordinates.
## @return: Array of Vector3i local coordinates.
func get_occupied_coords() -> Array[Vector3i]:
	return _occupied_coords


## Returns the quadrant this sector grid belongs to.
## @return: Vector3i quadrant coords, or null if not built.
func get_current_quadrant() -> Variant:
	return _current_quadrant


## Builds the sector grid for a specific quadrant.
## @param quadrant_coords: Which quadrant to subdivide.
## @param density_model: Density model for coloring.
func build_for_quadrant(
	quadrant_coords: Vector3i,
	density_model: DensityModelInterface
) -> void:
	_current_quadrant = quadrant_coords
	_highlighted_coords = null
	_coords_to_index.clear()
	_occupied_coords.clear()

	var quadrant_origin: Vector3 = _get_quadrant_origin(quadrant_coords)
	var sector_size: float = GalaxyCoordinates.SECTOR_SIZE_PC

	var cell_positions: PackedVector3Array = PackedVector3Array()
	var cell_densities: PackedFloat64Array = PackedFloat64Array()
	var cell_coords: Array[Vector3i] = []
	var max_density: float = 0.0

	for sx in range(SECTORS_PER_EDGE):
		for sy in range(SECTORS_PER_EDGE):
			for sz in range(SECTORS_PER_EDGE):
				var local_coords: Vector3i = Vector3i(sx, sy, sz)
				var center: Vector3 = quadrant_origin + Vector3(
					(float(sx) + 0.5) * sector_size,
					(float(sy) + 0.5) * sector_size,
					(float(sz) + 0.5) * sector_size
				)
				var density: float = density_model.get_density(center)

				cell_positions.append(center)
				cell_densities.append(density)
				cell_coords.append(local_coords)

				if density > max_density:
					max_density = density

	if max_density <= 0.0:
		return

	var filtered_positions: PackedVector3Array = PackedVector3Array()
	var filtered_colors: PackedColorArray = PackedColorArray()
	var filtered_coords: Array[Vector3i] = []

	for i in range(cell_positions.size()):
		var normalized: float = cell_densities[i] / max_density
		if normalized < DENSITY_VISIBILITY_THRESHOLD:
			continue
		filtered_positions.append(cell_positions[i])
		filtered_colors.append(_density_to_color(normalized))
		filtered_coords.append(cell_coords[i])

	_build_multimesh(filtered_positions, filtered_colors)
	_build_coord_index(filtered_coords)
	_original_colors = filtered_colors
	_occupied_coords = filtered_coords


## Returns the world-space center of a sector given its local coords.
## @param sector_local_coords: Local position within the quadrant.
## @return: World-space center of that sector.
func get_sector_world_center(sector_local_coords: Vector3i) -> Vector3:
	if _current_quadrant == null:
		return Vector3.ZERO
	var quadrant_coords: Vector3i = _current_quadrant as Vector3i
	var quadrant_origin: Vector3 = _get_quadrant_origin(quadrant_coords)
	var sector_size: float = GalaxyCoordinates.SECTOR_SIZE_PC
	return quadrant_origin + Vector3(
		(float(sector_local_coords.x) + 0.5) * sector_size,
		(float(sector_local_coords.y) + 0.5) * sector_size,
		(float(sector_local_coords.z) + 0.5) * sector_size
	)


## Returns the AABB min/max for a sector given its local coords in world space.
## @param sector_local_coords: Local position within the quadrant.
## @return: Array of [aabb_min, aabb_max] as Vector3.
func get_sector_world_aabb(sector_local_coords: Vector3i) -> Array[Vector3]:
	if _current_quadrant == null:
		return [Vector3.ZERO, Vector3.ZERO]
	var quadrant_coords: Vector3i = _current_quadrant as Vector3i
	var quadrant_origin: Vector3 = _get_quadrant_origin(quadrant_coords)
	var sector_size: float = GalaxyCoordinates.SECTOR_SIZE_PC
	var aabb_min: Vector3 = quadrant_origin + Vector3(
		float(sector_local_coords.x) * sector_size,
		float(sector_local_coords.y) * sector_size,
		float(sector_local_coords.z) * sector_size
	)
	var aabb_max: Vector3 = aabb_min + Vector3(sector_size, sector_size, sector_size)
	return [aabb_min, aabb_max]


## Highlights a specific sector cell by local coords.
## @param coords: Sector-local coords to highlight, or null to clear.
func set_highlight(coords: Variant) -> void:
	if _highlighted_coords != null and multimesh != null:
		var prev_key: String = _coords_key(_highlighted_coords as Vector3i)
		if _coords_to_index.has(prev_key):
			var prev_idx: int = _coords_to_index[prev_key] as int
			if prev_idx < _original_colors.size():
				multimesh.set_instance_color(prev_idx, _original_colors[prev_idx])

	_highlighted_coords = coords

	if coords != null and multimesh != null:
		var new_key: String = _coords_key(coords as Vector3i)
		if _coords_to_index.has(new_key):
			var new_idx: int = _coords_to_index[new_key] as int
			multimesh.set_instance_color(new_idx, HIGHLIGHT_COLOR)


## Gets the minimum corner of a quadrant in parsec-space.
## @param quadrant_coords: Quadrant grid coordinates.
## @return: Parsec position of the quadrant's minimum corner.
func _get_quadrant_origin(quadrant_coords: Vector3i) -> Vector3:
	return Vector3(
		float(quadrant_coords.x) * GalaxyCoordinates.QUADRANT_SIZE_PC,
		float(quadrant_coords.y) * GalaxyCoordinates.QUADRANT_SIZE_PC,
		float(quadrant_coords.z) * GalaxyCoordinates.QUADRANT_SIZE_PC
	)


## Maps normalized density to a display color with alpha.
## @param normalized_density: Density in [0, 1].
## @return: RGBA color.
static func _density_to_color(normalized_density: float) -> Color:
	var t: float = clampf(normalized_density, 0.0, 1.0)
	var alpha: float = lerpf(0.03, 0.2, t)

	var rgb: Color
	if t < 0.5:
		var local_t: float = t / 0.5
		rgb = COLOR_LOW.lerp(COLOR_MID, local_t)
	else:
		var local_t: float = (t - 0.5) / 0.5
		rgb = COLOR_MID.lerp(COLOR_HIGH, local_t)

	return Color(rgb.r, rgb.g, rgb.b, alpha)


## Constructs the MultiMesh from positions and colors.
## @param positions: World-space sector centers.
## @param colors: Per-sector RGBA colors.
func _build_multimesh(positions: PackedVector3Array, colors: PackedColorArray) -> void:
	var count: int = positions.size()
	if count == 0:
		return

	var cell_size: float = GalaxyCoordinates.SECTOR_SIZE_PC * CELL_FILL_FRACTION
	var box: BoxMesh = BoxMesh.new()
	box.size = Vector3.ONE

	_material = ShaderMaterial.new()
	_material.shader = preload("shaders/sector_cell.gdshader")
	box.material = _material

	var mm: MultiMesh = MultiMesh.new()
	mm.mesh = box
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.instance_count = count

	var scale_basis: Basis = Basis.IDENTITY.scaled(
		Vector3(cell_size, cell_size, cell_size)
	)

	for i in range(count):
		var t: Transform3D = Transform3D(scale_basis, positions[i])
		mm.set_instance_transform(i, t)
		mm.set_instance_color(i, colors[i])

	multimesh = mm


## Builds the coordinate-to-index lookup dictionary.
## @param coords_list: Ordered list of local coordinates matching instance indices.
func _build_coord_index(coords_list: Array[Vector3i]) -> void:
	_coords_to_index.clear()
	for i in range(coords_list.size()):
		var key: String = _coords_key(coords_list[i])
		_coords_to_index[key] = i


## Creates a string key from Vector3i for dictionary lookup.
## @param coords: Grid coordinates.
## @return: String key.
static func _coords_key(coords: Vector3i) -> String:
	return "%d,%d,%d" % [coords.x, coords.y, coords.z]
