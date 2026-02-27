## Renders asteroid belts as torus bands and background asteroid fields.
class_name BeltRenderer
extends Node3D

const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _belt_field_generator: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldGenerator.gd")
const _belt_field_spec: GDScript = preload("res://src/domain/system/asteroid_belt/BeltFieldSpec.gd")
const _belt_major_input: GDScript = preload("res://src/domain/system/asteroid_belt/BeltMajorAsteroidInput.gd")

## Emitted when a belt's ring area is clicked.
signal belt_clicked(belt_id: String)

## Per-belt root Node3D positioned at host center. Children use local coords.
var _belt_roots: Dictionary = {}

## Host ID per belt, for position updates in binary systems.
var _belt_host_ids: Dictionary = {}

## Angular rotation speed for belt visuals in radians per second.
const BELT_ROTATION_SPEED_RAD_PER_SEC: float = 0.15


## Clears all rendered belt visuals.
func clear() -> void:
	for belt_id in _belt_roots:
		var root: Node3D = _belt_roots[belt_id] as Node3D
		if root != null:
			root.queue_free()
	_belt_roots.clear()
	_belt_host_ids.clear()


## Renders all belts in the current system layout.
func render_belts(system: SolarSystem, layout: SystemDisplayLayout.SystemLayout, base_seed: int) -> void:
	clear()
	if system == null or layout == null:
		return

	for belt in system.asteroid_belts:
		var belt_layout: RefCounted = layout.get_belt_layout(belt.id)
		if belt_layout == null:
			continue
		var root: Node3D = _create_belt_root(belt_layout)
		_render_belt_torus(belt_layout, root)
		_render_belt_background(system, belt, belt_layout, base_seed, root)


## Creates a root Node3D for one belt at the host center.
## @param belt_layout: Belt layout data.
## @return: The root Node3D (child of this BeltRenderer).
func _create_belt_root(belt_layout: RefCounted) -> Node3D:
	var root: Node3D = Node3D.new()
	root.name = "Belt_" + belt_layout.belt_id
	root.position = belt_layout.host_center
	add_child(root)
	_belt_roots[belt_layout.belt_id] = root
	_belt_host_ids[belt_layout.belt_id] = belt_layout.host_id
	return root


## Updates belt root positions when orbit hosts move (binary systems).
## @param host_positions: Dictionary mapping host ID -> Vector3 position.
func update_belt_positions(host_positions: Dictionary) -> void:
	for belt_id in _belt_roots:
		var host_id: String = _belt_host_ids.get(belt_id, "") as String
		if host_id.is_empty():
			continue
		if host_positions.has(host_id):
			var root: Node3D = _belt_roots[belt_id] as Node3D
			if root != null:
				root.position = host_positions[host_id] as Vector3


## Updates belt rotation around each belt's own center.
## @param delta: Time step in seconds.
func update_belt_rotation(delta: float) -> void:
	var angle_step: float = BELT_ROTATION_SPEED_RAD_PER_SEC * delta
	for belt_id in _belt_roots:
		var root: Node3D = _belt_roots[belt_id] as Node3D
		if root != null:
			root.rotate_y(-angle_step)


## Renders one flattened torus mesh to represent belt volume.
## @param belt_layout: Belt layout data.
## @param root: Parent Node3D for this belt.
func _render_belt_torus(belt_layout: RefCounted, root: Node3D) -> void:
	var torus_mesh: TorusMesh = TorusMesh.new()
	torus_mesh.inner_radius = belt_layout.inner_display_radius
	torus_mesh.outer_radius = belt_layout.outer_display_radius
	torus_mesh.rings = 48
	torus_mesh.ring_segments = 16

	var mesh_instance: MeshInstance3D = MeshInstance3D.new()
	mesh_instance.mesh = torus_mesh
	# Flatten vertically so the torus looks like a disc rather than a tube.
	mesh_instance.scale = Vector3(1.0, 0.12, 1.0)

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.albedo_color = Color(0.55, 0.62, 0.78, 0.25)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	mesh_instance.material_override = mat
	mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

	root.add_child(mesh_instance)


## Creates an invisible click area covering the belt ring.
## @param belt_layout: Belt layout data.
## @param root: Parent Node3D for this belt.
func _render_belt_click_area(belt_layout: RefCounted, root: Node3D) -> void:
	var click_area: Area3D = Area3D.new()
	click_area.name = "BeltClickArea"
	click_area.input_ray_pickable = true

	var collision: CollisionShape3D = CollisionShape3D.new()
	var cylinder: CylinderShape3D = CylinderShape3D.new()
	cylinder.radius = belt_layout.outer_display_radius
	cylinder.height = 0.5
	collision.shape = cylinder
	click_area.add_child(collision)
	root.add_child(click_area)

	var inner_r: float = belt_layout.inner_display_radius
	var bid: String = belt_layout.belt_id
	click_area.input_event.connect(
		func(_camera: Node, event: InputEvent, pos: Vector3, _normal: Vector3, _shape_idx: int) -> void:
			if event is InputEventMouseButton:
				var mouse: InputEventMouseButton = event as InputEventMouseButton
				if mouse.pressed and mouse.button_index == MOUSE_BUTTON_LEFT:
					var local_pos: Vector3 = pos - root.global_position
					var dist: float = Vector2(local_pos.x, local_pos.z).length()
					if dist >= inner_r:
						belt_clicked.emit(bid)
	)


## Renders dense background asteroids for one belt.
## Generates asteroid positions in AU space using the field generator,
## then maps them to display coordinates local to the belt root node.
## The root node is already positioned at the host center.
## @param system: The solar system (for major asteroid data).
## @param belt: The asteroid belt domain object.
## @param belt_layout: Belt layout data.
## @param base_seed: System generation seed for deterministic placement.
## @param root: Parent Node3D for this belt.
func _render_belt_background(
	system: SolarSystem,
	belt: AsteroidBelt,
	belt_layout: RefCounted,
	base_seed: int,
	root: Node3D
) -> void:
	var generator: RefCounted = _belt_field_generator.new()
	var spec: RefCounted = _build_field_spec(system, belt, belt_layout)
	var seed_value: int = _derive_belt_seed(base_seed, belt.id)
	var rng: SeededRng = SeededRng.new(seed_value)
	var data: RefCounted = generator.generate_field(spec, rng)
	var background: Array = data.get_background_asteroids()
	if background.is_empty():
		push_warning("BeltRenderer: No background asteroids generated for belt %s" % belt.id)
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
		var vis_radius: float = lerpf(0.10, 0.30, size_t)
		mm.set_instance_transform(i, Transform3D(Basis().scaled(Vector3.ONE * vis_radius), display_pos))

	var mesh_instance: MultiMeshInstance3D = MultiMeshInstance3D.new()
	mesh_instance.multimesh = mm
	mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = Color(0.7, 0.65, 0.55)
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.transparency = BaseMaterial3D.TRANSPARENCY_DISABLED
	mesh_instance.material_override = material
	root.add_child(mesh_instance)


## Builds field generation spec for one belt.
## Uses belt layout AU values for radial bounds and the display layout's
## inclination. Density scales with belt width so narrow belts aren't
## overpopulated and wide belts aren't sparse.
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
	# major_asteroid_ids links belt → CelestialBody asteroid entries
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
		input.longitude_ascending_node_deg = body.orbital.longitude_of_ascending_node_deg
		input.argument_periapsis_deg = body.orbital.argument_of_periapsis_deg
		input.mean_anomaly_deg = body.orbital.mean_anomaly_deg
		input.body_radius_km = body.physical.radius_m / 1000.0
		input.asteroid_type = -1
		inputs.append(input)
	return inputs


## Maps a belt-field AU position into local display coordinates.
## The belt root Node3D is positioned at host_center, so positions
## returned here are relative to that root (no host_center offset).
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

	return Vector3(
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
