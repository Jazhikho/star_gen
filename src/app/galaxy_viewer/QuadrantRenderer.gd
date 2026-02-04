## Renders the quadrant grid as translucent colored cubes via MultiMesh.
## Each cell is sized and colored based on stellar density at its center.
class_name QuadrantRenderer
extends MultiMeshInstance3D


## Fraction of quadrant size that each cube occupies (leaves gaps to show grid).
const CELL_FILL_FRACTION: float = 0.7

## Minimum normalized density to include a cell (filters empty corners).
const DENSITY_VISIBILITY_THRESHOLD: float = 0.01

## Low-density color stop.
const COLOR_LOW: Color = Color(0.1, 0.2, 0.8)

## Mid-density color stop.
const COLOR_MID: Color = Color(0.1, 0.8, 0.8)

## High-density color stop.
const COLOR_HIGH: Color = Color(1.0, 0.85, 0.3)

## Highlight color — distinct but not blinding with blend_mix.
const HIGHLIGHT_COLOR: Color = Color(0.5, 0.8, 1.0, 0.4)

## Reference to the shader material.
var _material: ShaderMaterial

## Maps grid coordinates to instance index for highlighting.
var _coords_to_index: Dictionary = {}

## Stored colors for restoring after highlight removal.
var _original_colors: PackedColorArray = PackedColorArray()

## Currently highlighted quadrant coords, or null.
var _highlighted_coords: Variant = null

## List of occupied quadrant coordinates matching instance order.
var _occupied_coords: Array[Vector3i] = []


## Returns the list of occupied quadrant grid coordinates.
## @return: Array of Vector3i coordinates.
func get_occupied_coords() -> Array[Vector3i]:
	return _occupied_coords


## Builds the grid MultiMesh from a density model and galaxy spec.
## @param spec: Galaxy specification for grid bounds.
## @param density_model: Density model to query at each cell center.
func build_from_density(spec: GalaxySpec, density_model: DensityModelInterface) -> void:
	var grid_min: Vector3i = GalaxyCoordinates.get_quadrant_grid_min(spec)
	var grid_max: Vector3i = GalaxyCoordinates.get_quadrant_grid_max(spec)

	var cell_positions: PackedVector3Array = PackedVector3Array()
	var cell_densities: PackedFloat64Array = PackedFloat64Array()
	var cell_coords: Array[Vector3i] = []
	var max_density: float = 0.0

	for qx in range(grid_min.x, grid_max.x + 1):
		for qy in range(grid_min.y, grid_max.y + 1):
			for qz in range(grid_min.z, grid_max.z + 1):
				var coords: Vector3i = Vector3i(qx, qy, qz)
				var center: Vector3 = GalaxyCoordinates.quadrant_to_parsec_center(coords)
				var density: float = density_model.get_density(center)

				cell_positions.append(center)
				cell_densities.append(density)
				cell_coords.append(coords)

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


## Highlights a specific quadrant cell by coords.
## @param coords: Quadrant grid coordinates to highlight, or null to clear.
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


## Maps a normalized density [0, 1] to a display color with alpha.
## @param normalized_density: Density value normalized to [0, 1].
## @return: RGBA color for the cell.
static func _density_to_color(normalized_density: float) -> Color:
	var t: float = clampf(normalized_density, 0.0, 1.0)
	var alpha: float = lerpf(0.03, 0.25, t)

	var rgb: Color
	if t < 0.5:
		var local_t: float = t / 0.5
		rgb = COLOR_LOW.lerp(COLOR_MID, local_t)
	else:
		var local_t: float = (t - 0.5) / 0.5
		rgb = COLOR_MID.lerp(COLOR_HIGH, local_t)

	return Color(rgb.r, rgb.g, rgb.b, alpha)


## Constructs the MultiMesh from filtered positions and colors.
## @param positions: World-space cell centers.
## @param colors: Per-cell RGBA colors.
func _build_multimesh(positions: PackedVector3Array, colors: PackedColorArray) -> void:
	var count: int = positions.size()
	if count == 0:
		return

	var cell_size: float = GalaxyCoordinates.QUADRANT_SIZE_PC * CELL_FILL_FRACTION
	var box: BoxMesh = BoxMesh.new()
	box.size = Vector3.ONE

	_material = ShaderMaterial.new()
	_material.shader = preload("shaders/quadrant_cell.gdshader")
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
## @param coords_list: Ordered list of grid coordinates matching instance indices.
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
