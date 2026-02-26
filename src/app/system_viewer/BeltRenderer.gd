## Renders asteroid belts as torus bands and background asteroid fields.
class_name BeltRenderer
extends Node3D

const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _belt_field_generator: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldGenerator.gd")
const _belt_field_spec: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldSpec.gd")
const _belt_major_input: GDScript = preload("res://src/domain/system/asteroid_belt/BeltMajorAsteroidInput.gd")

## Background asteroid instance containers by belt ID.
var _background_instances: Dictionary = {}

## Torus meshes by belt ID.
var _torus_meshes: Dictionary = {}


## Clears all rendered belt visuals.
func clear() -> void:
	for belt_id in _background_instances:
		var bg_node: Node3D = _background_instances[belt_id] as Node3D
		if bg_node != null:
			bg_node.queue_free()
	_background_instances.clear()

	for belt_id in _torus_meshes:
		var torus_node: Node3D = _torus_meshes[belt_id] as Node3D
		if torus_node != null:
			torus_node.queue_free()
	_torus_meshes.clear()


## Renders all belts in the current system layout.
func render_belts(system: SolarSystem, layout: SystemDisplayLayout.SystemLayout, base_seed: int) -> void:
	clear()
	if system == null or layout == null:
		return

	for belt in system.asteroid_belts:
		var belt_layout: RefCounted = layout.get_belt_layout(belt.id)
		if belt_layout == null:
			continue
		_render_belt_torus(belt_layout)
		_render_belt_background(system, belt, belt_layout, base_seed)


## Renders one torus mesh to represent belt volume.
func _render_belt_torus(belt_layout: RefCounted) -> void:
	var torus_mesh: TorusMesh = TorusMesh.new()
	torus_mesh.ring_radius = belt_layout.center_display_radius
	torus_mesh.ring_sides = 16
	torus_mesh.radial_segments = 48
	torus_mesh.inner_radius = maxf(0.05, belt_layout.outer_display_radius - belt_layout.center_display_radius)
	torus_mesh.outer_radius = torus_mesh.inner_radius + 0.07

	var mesh_instance: MeshInstance3D = MeshInstance3D.new()
	mesh_instance.mesh = torus_mesh
	mesh_instance.position = belt_layout.host_center

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.albedo_color = Color(0.55, 0.62, 0.78, 0.35)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mesh_instance.material_override = mat
	mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

	add_child(mesh_instance)
	_torus_meshes[belt_layout.belt_id] = mesh_instance


## Renders dense background asteroids for one belt.
func _render_belt_background(
	system: SolarSystem,
	belt: AsteroidBelt,
	belt_layout: RefCounted,
	base_seed: int
) -> void:
	var generator: RefCounted = _belt_field_generator.new()
	var spec: RefCounted = _build_field_spec(system, belt, belt_layout)
	var seed_value: int = _derive_belt_seed(base_seed, belt.id)
	var rng: SeededRng = SeededRng.new(seed_value)
	var data: RefCounted = generator.generate_field(spec, rng)
	var background: Array = data.get_background_asteroids()
	if background.is_empty():
		return

	var mesh: SphereMesh = SphereMesh.new()
	mesh.radius = 1.0
	mesh.height = 2.0
	mesh.radial_segments = 5
	mesh.rings = 4

	var mm: MultiMesh = MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.instance_count = background.size()
	mm.mesh = mesh

	var min_log: float = log(maxf(spec.min_body_radius_km, 0.01))
	var max_log: float = log(maxf(spec.max_body_radius_km, 1.0))
	var log_range: float = maxf(max_log - min_log, 0.001)
	var band_au: float = maxf(0.001, belt_layout.outer_au - belt_layout.inner_au)
	for i in range(background.size()):
		var asteroid: RefCounted = background[i]
		var display_pos: Vector3 = _map_au_position_to_display(asteroid.position_au, belt_layout, band_au)
		var size_t: float = clampf((log(maxf(asteroid.body_radius_km, 0.01)) - min_log) / log_range, 0.0, 1.0)
		var vis_radius: float = lerpf(0.02, 0.08, size_t)
		mm.set_instance_transform(i, Transform3D(Basis().scaled(Vector3.ONE * vis_radius), display_pos))

	var mesh_instance: MultiMeshInstance3D = MultiMeshInstance3D.new()
	mesh_instance.multimesh = mm
	mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = Color(0.62, 0.60, 0.54)
	material.roughness = 0.95
	material.metallic = 0.05
	mesh_instance.material_override = material
	add_child(mesh_instance)
	_background_instances[belt.id] = mesh_instance


## Builds field generation spec for one belt.
func _build_field_spec(
	system: SolarSystem,
	belt: AsteroidBelt,
	belt_layout: RefCounted
) -> RefCounted:
	var spec: RefCounted = _belt_field_spec.new()
	spec.inner_radius_au = belt_layout.inner_au
	spec.outer_radius_au = belt_layout.outer_au
	spec.max_inclination_deg = belt_layout.max_inclination_deg
	spec.max_eccentricity = 0.18
	var density_from_width: int = int(clampf(belt.get_width_au() * 450.0, 250.0, 1500.0))
	spec.asteroid_count = density_from_width
	spec.min_body_radius_km = 0.25
	spec.max_body_radius_km = 180.0
	spec.radial_concentration = 1.8
	spec.size_power_law_exponent = 2.4
	spec.major_asteroid_inputs = _build_major_inputs(system, belt)
	return spec


## Converts major asteroid bodies into field major input records.
func _build_major_inputs(system: SolarSystem, belt: AsteroidBelt) -> Array:
	var inputs: Array = []
	for asteroid_id in belt.major_asteroid_ids:
		var body: CelestialBody = system.get_body(asteroid_id)
		if body == null:
			continue
		if not body.has_orbital():
			continue
		var input: RefCounted = _belt_major_input.new()
		input.body_id = body.id
		input.semi_major_axis_m = body.orbital.semi_major_axis_m
		input.eccentricity = body.orbital.eccentricity
		input.inclination_deg = body.orbital.inclination_deg
		input.longitude_ascending_node_deg = body.orbital.longitude_ascending_node_deg
		input.argument_periapsis_deg = body.orbital.argument_of_periapsis_deg
		input.mean_anomaly_deg = body.orbital.mean_anomaly_deg
		input.body_radius_km = body.physical.radius_m / 1000.0
		input.asteroid_type = -1
		inputs.append(input)
	return inputs


## Maps a belt-field AU position into display coordinates for this belt slot.
func _map_au_position_to_display(
	position_au: Vector3,
	belt_layout: RefCounted,
	band_au: float
) -> Vector3:
	var radial_au: float = Vector2(position_au.x, position_au.z).length()
	var angle: float = atan2(position_au.z, position_au.x)
	var radial_t: float = clampf((radial_au - belt_layout.inner_au) / band_au, 0.0, 1.0)
	var display_radius: float = lerpf(
		belt_layout.inner_display_radius,
		belt_layout.outer_display_radius,
		radial_t
	)

	var y_norm: float = 0.0
	if radial_au > 1.0e-6:
		y_norm = position_au.y / radial_au
	var max_y_norm: float = sin(deg_to_rad(belt_layout.max_inclination_deg))
	y_norm = clampf(y_norm, -max_y_norm, max_y_norm)

	return belt_layout.host_center + Vector3(
		cos(angle) * display_radius,
		y_norm * display_radius,
		sin(angle) * display_radius
	)


## Derives a deterministic seed per belt.
func _derive_belt_seed(base_seed: int, belt_id: String) -> int:
	var hash_value: int = base_seed
	for i in range(belt_id.length()):
		hash_value = int((hash_value * 31 + belt_id.unicode_at(i)) % 2147483647)
	return abs(hash_value)
