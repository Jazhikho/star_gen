## Renders an asteroid belt with distinct visual treatment for major vs background bodies.
## Background: small grey MultiMesh. Major: larger colored MultiMesh + optional labels.
## Also draws reference rings for belt boundaries and optionally for gaps.
class_name BeltViewerRendererConcept
extends Node3D


## Ring segment count for boundary circles.
const RING_SEGMENTS: int = 128

## Visual radius range for background asteroids (AU scene units).
const MIN_BG_VISUAL_RADIUS: float = 0.008
const MAX_BG_VISUAL_RADIUS: float = 0.04

## Visual radius range for major asteroids (always larger than background).
const MIN_MAJOR_VISUAL_RADIUS: float = 0.06
const MAX_MAJOR_VISUAL_RADIUS: float = 0.15

## Log-radius bounds assumed for major body scaling.
const MAJOR_LOG_MIN_KM: float = 100.0
const MAJOR_LOG_MAX_KM: float = 1000.0

## MultiMeshInstance3D for background asteroid bodies.
var _bg_mesh_instance: MultiMeshInstance3D = null

## MultiMeshInstance3D for major asteroid bodies.
var _major_mesh_instance: MultiMeshInstance3D = null

## MeshInstance3D nodes for boundary and gap rings.
var _ring_meshes: Array[MeshInstance3D] = []

## Label3D nodes for major asteroid names.
var _major_labels: Array[Label3D] = []

## Low-poly sphere for background asteroids.
var _bg_sphere: SphereMesh = null

## Higher-poly sphere for major asteroids.
var _major_sphere: SphereMesh = null

## Material for background asteroid bodies.
var _bg_material: StandardMaterial3D = null

## Material for major asteroid bodies (uses per-instance color).
var _major_material: StandardMaterial3D = null

## Material template for reference rings.
var _ring_material: StandardMaterial3D = null


## Initialises reusable mesh and material assets.
func setup() -> void:
	_bg_sphere = SphereMesh.new()
	_bg_sphere.radius = 1.0
	_bg_sphere.height = 2.0
	_bg_sphere.radial_segments = 6
	_bg_sphere.rings = 4

	_major_sphere = SphereMesh.new()
	_major_sphere.radius = 1.0
	_major_sphere.height = 2.0
	_major_sphere.radial_segments = 16
	_major_sphere.rings = 10

	_bg_material = StandardMaterial3D.new()
	_bg_material.albedo_color = Color(0.72, 0.65, 0.55)
	_bg_material.roughness = 0.9
	_bg_material.metallic = 0.1

	_major_material = StandardMaterial3D.new()
	_major_material.vertex_color_use_as_albedo = true
	_major_material.emission_enabled = true
	_major_material.emission = Color(0.9, 0.85, 0.7)
	_major_material.emission_energy_multiplier = 0.4
	_major_material.roughness = 0.5
	_major_material.metallic = 0.2

	_ring_material = StandardMaterial3D.new()
	_ring_material.albedo_color = Color(0.4, 0.6, 1.0, 0.35)
	_ring_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	_ring_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED


## Clears all previously rendered content from the scene tree.
func clear() -> void:
	if _bg_mesh_instance != null:
		_bg_mesh_instance.queue_free()
		_bg_mesh_instance = null
	if _major_mesh_instance != null:
		_major_mesh_instance.queue_free()
		_major_mesh_instance = null
	for ring in _ring_meshes:
		ring.queue_free()
	_ring_meshes.clear()
	for label in _major_labels:
		label.queue_free()
	_major_labels.clear()


## Renders a generated belt: background + major MultiMeshes, boundary rings,
## optional gap rings, and optional labels.
## @param belt: The belt data to render.
## @param spec: The spec (for ring radii and gap display).
## @param show_labels: Whether to show Label3D nodes on major asteroids.
## @param show_gap_rings: Whether to draw orange rings at gap boundaries.
func render_belt(
	belt: AsteroidBeltDataConcept,
	spec: AsteroidBeltSpecConcept,
	show_labels: bool,
	show_gap_rings: bool
) -> void:
	clear()
	_render_background_asteroids(belt, spec)
	_render_major_asteroids(belt, show_labels)

	# Belt boundary rings (green)
	_draw_ring(spec.inner_radius_au, Color(0.3, 0.8, 0.3, 0.5))
	_draw_ring(spec.outer_radius_au, Color(0.3, 0.8, 0.3, 0.5))

	# Gap boundary rings (orange) — only if requested
	if show_gap_rings:
		var gap_count: int = mini(spec.gap_centers_au.size(), spec.gap_half_widths_au.size())
		for i in range(gap_count):
			_draw_ring(spec.gap_centers_au[i] - spec.gap_half_widths_au[i], Color(1.0, 0.4, 0.2, 0.4))
			_draw_ring(spec.gap_centers_au[i] + spec.gap_half_widths_au[i], Color(1.0, 0.4, 0.2, 0.4))


## Builds the background asteroid MultiMesh.
## @param belt: Belt data containing all asteroids.
## @param spec: Spec for size range reference.
func _render_background_asteroids(
	belt: AsteroidBeltDataConcept,
	spec: AsteroidBeltSpecConcept
) -> void:
	var bg: Array[AsteroidDataConcept] = belt.get_background_asteroids()
	if bg.is_empty():
		return

	var mm: MultiMesh = MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.instance_count = bg.size()
	mm.mesh = _bg_sphere

	var log_min: float = log(maxf(spec.min_body_radius_km, 0.01))
	var log_max: float = log(maxf(spec.max_body_radius_km, 1.0))
	var log_range: float = maxf(log_max - log_min, 0.001)

	for i in range(bg.size()):
		var a: AsteroidDataConcept = bg[i]
		var log_r: float = log(maxf(a.body_radius_km, 0.01))
		var t: float = clampf((log_r - log_min) / log_range, 0.0, 1.0)
		var vr: float = lerpf(MIN_BG_VISUAL_RADIUS, MAX_BG_VISUAL_RADIUS, t)
		mm.set_instance_transform(i, Transform3D(
			Basis().scaled(Vector3.ONE * vr), a.position_au))

	_bg_mesh_instance = MultiMeshInstance3D.new()
	_bg_mesh_instance.multimesh = mm
	_bg_mesh_instance.material_override = _bg_material
	add_child(_bg_mesh_instance)


## Builds the major asteroid MultiMesh with per-instance type colors, and optional labels.
## @param belt: Belt data containing all asteroids.
## @param show_labels: Whether to create Label3D nodes.
func _render_major_asteroids(
	belt: AsteroidBeltDataConcept,
	show_labels: bool
) -> void:
	var majors: Array[AsteroidDataConcept] = belt.get_major_asteroids()
	if majors.is_empty():
		return

	var mm: MultiMesh = MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.instance_count = majors.size()
	mm.mesh = _major_sphere

	var log_lo: float = log(MAJOR_LOG_MIN_KM)
	var log_hi: float = log(MAJOR_LOG_MAX_KM)
	var log_range: float = maxf(log_hi - log_lo, 0.001)

	for i in range(majors.size()):
		var a: AsteroidDataConcept = majors[i]
		var log_r: float = log(maxf(a.body_radius_km, 1.0))
		var t: float = clampf((log_r - log_lo) / log_range, 0.0, 1.0)
		var vr: float = lerpf(MIN_MAJOR_VISUAL_RADIUS, MAX_MAJOR_VISUAL_RADIUS, t)

		mm.set_instance_transform(i, Transform3D(
			Basis().scaled(Vector3.ONE * vr), a.position_au))
		mm.set_instance_color(i, _get_type_color(a.asteroid_type))

		if show_labels:
			var label: Label3D = Label3D.new()
			label.text = a.body_id if not a.body_id.is_empty() else "Major %d" % (i + 1)
			label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
			label.position = a.position_au + Vector3(0.0, vr * 2.5, 0.0)
			label.font_size = 48
			label.outline_size = 8
			label.modulate = _get_type_color(a.asteroid_type).lightened(0.3)
			label.pixel_size = 0.002
			label.no_depth_test = true
			add_child(label)
			_major_labels.append(label)

	_major_mesh_instance = MultiMeshInstance3D.new()
	_major_mesh_instance.multimesh = mm
	_major_mesh_instance.material_override = _major_material
	add_child(_major_mesh_instance)


## Returns a display colour for an asteroid compositional type.
## @param ast_type: AsteroidType.Type value, or -1 for unknown.
## @return: The display colour.
func _get_type_color(ast_type: int) -> Color:
	match ast_type:
		0: return Color(0.55, 0.45, 0.35) # C-Type: dark brown
		1: return Color(0.90, 0.80, 0.55) # S-Type: warm tan
		2: return Color(0.80, 0.82, 0.88) # M-Type: steel silver
		_: return Color(0.75, 0.75, 0.75) # Unknown: neutral grey


## Draws a horizontal reference ring using an ImmediateMesh.
## @param radius_au: Ring radius in AU.
## @param color: Ring colour with alpha.
func _draw_ring(radius_au: float, color: Color) -> void:
	if radius_au <= 0.0:
		return

	var im: ImmediateMesh = ImmediateMesh.new()
	im.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	for s in range(RING_SEGMENTS + 1):
		var angle: float = float(s) / float(RING_SEGMENTS) * TAU
		im.surface_add_vertex(Vector3(cos(angle) * radius_au, 0.0, sin(angle) * radius_au))
	im.surface_end()

	var mat: StandardMaterial3D = _ring_material.duplicate() as StandardMaterial3D
	mat.albedo_color = color

	var ring_node: MeshInstance3D = MeshInstance3D.new()
	ring_node.mesh = im
	ring_node.material_override = mat
	add_child(ring_node)
	_ring_meshes.append(ring_node)
