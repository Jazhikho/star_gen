## Main viewer scene for inspecting solar systems.
## Handles system display, body selection, and user interactions.
class_name SystemViewer
extends Node3D

## Signal emitted when a body should be opened in the object viewer.
signal open_body_in_viewer(body: CelestialBody)

## Signal emitted when the user wants to go back to the galaxy viewer.
signal back_to_galaxy_requested

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _system_fixture_generator: GDScript = preload("res://src/domain/system/fixtures/SystemFixtureGenerator.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _system_scale_manager: GDScript = preload("res://src/app/system_viewer/SystemScaleManager.gd")
const _system_body_node_scene: PackedScene = preload("res://src/app/system_viewer/SystemBodyNode.tscn")
const _orbit_renderer: GDScript = preload("res://src/app/system_viewer/OrbitRenderer.gd")
const _orbit_host: GDScript = preload("res://src/domain/system/OrbitHost.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")

## UI element references
@onready var status_label: Label = $UI/TopBar/MarginContainer/HBoxContainer/StatusLabel
@onready var side_panel: Panel = $UI/SidePanel
@onready var inspector_panel: SystemInspectorPanel = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel

# Generation controls
@onready var star_count_spin: SpinBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/StarCountContainer/StarCountSpin
@onready var seed_input: SpinBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput
@onready var generate_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton
@onready var reroll_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/RerollButton

# View controls
@onready var show_orbits_check: CheckBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowOrbitsCheck
@onready var show_zones_check: CheckBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowZonesCheck

## 3D element references
@onready var camera: SystemCameraController = $CameraRig/Camera3D
@onready var bodies_container: Node3D = $BodiesContainer
@onready var orbits_container: Node3D = $OrbitsContainer
@onready var zones_container: Node3D = $ZonesContainer
@onready var world_environment: WorldEnvironment = $Environment/WorldEnvironment

## Currently displayed solar system
var current_system: SolarSystem = null

## Currently selected body ID
var selected_body_id: String = ""

## Scale manager for coordinate conversion
var scale_manager: SystemScaleManager = SystemScaleManager.new()

## Body node references (body_id -> SystemBodyNode)
var body_nodes: Dictionary = {}

## Orbit renderer instance
var orbit_renderer: OrbitRenderer = null

## Whether the viewer is ready
var is_ready: bool = false


func _ready() -> void:
	_setup_viewport()
	_setup_camera()
	_setup_generation_ui()
	_setup_view_ui()
	_setup_orbit_renderer()
	_connect_signals()
	
	set_status("System viewer initialized")
	is_ready = true
	
	# Generate an initial system
	_on_generate_pressed()


## Sets up viewport settings.
func _setup_viewport() -> void:
	var viewport: Viewport = get_viewport()
	if viewport:
		viewport.use_hdr_2d = true


## Sets up initial camera position.
func _setup_camera() -> void:
	if camera:
		camera.focus_on_origin()


## Sets up the generation UI.
func _setup_generation_ui() -> void:
	if star_count_spin:
		star_count_spin.min_value = 1
		star_count_spin.max_value = 4
		star_count_spin.value = 1
	
	if seed_input:
		seed_input.value = randi() % 1000000


## Sets up view options UI.
func _setup_view_ui() -> void:
	if show_orbits_check:
		show_orbits_check.button_pressed = true
	
	if show_zones_check:
		show_zones_check.button_pressed = true


## Sets up orbit renderer.
func _setup_orbit_renderer() -> void:
	if orbits_container:
		orbit_renderer = OrbitRenderer.new()
		orbit_renderer.name = "OrbitRenderer"
		orbits_container.add_child(orbit_renderer)


## Connects UI signals.
func _connect_signals() -> void:
	if generate_button:
		generate_button.pressed.connect(_on_generate_pressed)

	if reroll_button:
		reroll_button.pressed.connect(_on_reroll_pressed)

	if show_orbits_check:
		show_orbits_check.toggled.connect(_on_show_orbits_toggled)

	if show_zones_check:
		show_zones_check.toggled.connect(_on_show_zones_toggled)

	# Connect inspector panel signals
	if inspector_panel:
		inspector_panel.open_in_viewer_requested.connect(_on_open_body_in_viewer)

	# Connect back button if present (back to galaxy)
	var back_button: Button = get_node_or_null("UI/TopBar/MarginContainer/HBoxContainer/BackButton")
	if back_button:
		back_button.pressed.connect(_on_back_pressed)


## Handles back button press to return to galaxy view.
func _on_back_pressed() -> void:
	back_to_galaxy_requested.emit()


## TODO: Add Escape key in _unhandled_input to emit back_to_galaxy_requested for consistency with back button.


## Handles generate button press.
func _on_generate_pressed() -> void:
	var star_count: int = int(star_count_spin.value) if star_count_spin else 1
	var seed_value: int = int(seed_input.value) if seed_input else randi()
	
	generate_system(seed_value, star_count, star_count)


## Handles re-roll button press.
func _on_reroll_pressed() -> void:
	var new_seed: int = randi() % 1000000
	if seed_input:
		seed_input.value = new_seed
	
	_on_generate_pressed()


## Handles orbit visibility toggle.
func _on_show_orbits_toggled(enabled: bool) -> void:
	if orbits_container:
		orbits_container.visible = enabled


## Handles zone visibility toggle.
func _on_show_zones_toggled(enabled: bool) -> void:
	if zones_container:
		zones_container.visible = enabled


## Generates a solar system.
## @param seed_value: Generation seed.
## @param min_stars: Minimum star count.
## @param max_stars: Maximum star count.
func generate_system(seed_value: int, min_stars: int = 1, max_stars: int = 1) -> void:
	set_status("Generating system with seed %d..." % seed_value)
	
	var spec: SolarSystemSpec = SolarSystemSpec.new(seed_value, min_stars, max_stars)
	
	var system: SolarSystem = SystemFixtureGenerator.generate_system(spec)
	
	if system:
		display_system(system)
		set_status("Generated: %s" % system.get_summary())
	else:
		set_error("Failed to generate system")


## Displays a solar system.
## @param system: The system to display.
func display_system(system: SolarSystem) -> void:
	if not system:
		clear_display()
		return

	current_system = system

	_clear_bodies()
	_clear_orbits()
	_clear_zones()

	_create_body_nodes()
	_create_orbit_visualizations()
	_create_zone_visualizations()

	_update_inspector_system()

	_debug_system_distances()
	_fit_camera_to_system()

	# Update status with system summary
	if system.name and not system.name.is_empty():
		set_status("Viewing: %s" % system.name)
	else:
		set_status("Generated: %s" % system.get_summary())


## Prints orbital distance debug information for the current system.
func _debug_system_distances() -> void:
	if not current_system:
		print("[SystemViewer] No system to debug")
		return
	
	print("=== System Distance Debug ===")
	
	# Format scale information
	var scale_m: float = scale_manager.distance_scale_m_per_unit
	var scale_au: float = scale_m / _units.AU_METERS
	var scale_m_str: String = _format_scientific(scale_m)
	print("  Scale: 1 unit = %s m (%.4f AU)" % [scale_m_str, scale_au])
	
	for star in current_system.get_stars():
		var star_pos: Vector3 = _calculate_body_position(star)
		print("  STAR %s: position = %s" % [star.name, star_pos])
	
	for planet in current_system.get_planets():
		if planet.has_orbital():
			var sma_m: float = planet.orbital.semi_major_axis_m
			var sma_au: float = sma_m / _units.AU_METERS
			var sma_units: float = scale_manager.distance_to_units(sma_m)
			var pos: Vector3 = _calculate_body_position(planet)
			var sma_m_str: String = _format_scientific(sma_m)
			print("  PLANET %s: sma = %s m = %.4f AU = %.4f units | pos = %s" % [
				planet.name, sma_m_str, sma_au, sma_units, pos
			])
	
	for moon in current_system.get_moons():
		if moon.has_orbital():
			var sma_m: float = moon.orbital.semi_major_axis_m
			var sma_units: float = scale_manager.distance_to_units(sma_m)
			var sma_m_str: String = _format_scientific(sma_m)
			print("  MOON %s: sma = %s m = %.6f units | parent = %s" % [
				moon.name, sma_m_str, sma_units, moon.orbital.parent_id
			])
	
	print("=============================")


## Formats a number in scientific notation (e.g., 1.496e+11).
## @param value: The value to format.
## @return: Formatted string.
func _format_scientific(value: float) -> String:
	if value == 0.0:
		return "0.000e+00"
	
	var abs_value: float = absf(value)
	var exponent: int = int(floor(log(abs_value) / log(10.0)))
	var mantissa: float = value / pow(10.0, exponent)
	
	var exp_str: String = str(exponent)
	if exponent >= 0:
		exp_str = "+" + exp_str
	
	return "%.3f" % mantissa + "e" + exp_str


## Fits the camera zoom to show the entire system.
func _fit_camera_to_system() -> void:
	if not camera:
		return
	if not current_system:
		camera.focus_on_origin()
		return
	
	var max_distance_units: float = 0.0
	
	for planet in current_system.get_planets():
		if planet.has_orbital():
			var d: float = scale_manager.distance_to_units(planet.orbital.semi_major_axis_m)
			max_distance_units = maxf(max_distance_units, d)
	
	if max_distance_units < 0.001:
		push_warning("[SystemViewer] Max planet distance is very small: %f units" % max_distance_units)
		# Still try to show it - just zoom in more
		max_distance_units = maxf(max_distance_units, 0.01)
	
	# Set camera height to show full system with margin
	var target_height: float = max_distance_units * 2.5
	target_height = clampf(target_height, 0.01, camera.max_height)
	
	# Also adjust camera min height for very small systems
	camera.min_height = minf(0.005, target_height * 0.1)
	
	camera._target_position = Vector3.ZERO
	camera._target_height = target_height
	camera._height = target_height
	camera._target_pitch = deg_to_rad(60.0)
	camera._yaw = 0.0
	camera._smooth_target = Vector3.ZERO


## Clears the current display.
func clear_display() -> void:
	current_system = null
	selected_body_id = ""
	
	_clear_bodies()
	_clear_orbits()
	_clear_zones()
	
	set_status("No system loaded")


## Creates 3D nodes for all bodies in the system.
func _create_body_nodes() -> void:
	if not current_system or not bodies_container:
		return
	
	# Calculate adaptive display sizes based on system layout
	var size_params: Dictionary = _calculate_adaptive_sizes()
	
	# Create star nodes
	for star in current_system.get_stars():
		_create_body_node(star, size_params)
	
	# Create planet nodes
	for planet in current_system.get_planets():
		_create_body_node(planet, size_params)
	
	# Create moon nodes
	for moon in current_system.get_moons():
		_create_body_node(moon, size_params)
	
	# Create asteroid nodes (limit for performance)
	var asteroids: Array[CelestialBody] = current_system.get_asteroids()
	var max_asteroids: int = mini(asteroids.size(), 50)
	for i in range(max_asteroids):
		_create_body_node(asteroids[i], size_params)


## Creates a 3D node for a single body.
## @param body: The celestial body.
## @param size_params: Adaptive size parameters from _calculate_adaptive_sizes.
func _create_body_node(body: CelestialBody, size_params: Dictionary) -> void:
	if not body:
		return
	
	var body_node: SystemBodyNode = _system_body_node_scene.instantiate() as SystemBodyNode
	if not body_node:
		return
	
	# Calculate adaptive display radius
	var display_radius: float = _get_adaptive_body_radius(body, size_params)
	
	# Calculate position
	var body_position: Vector3 = _calculate_body_position(body)
	
	# Setup the body node
	body_node.setup(body, display_radius, body_position)
	body_node.body_selected.connect(_on_body_clicked)
	
	bodies_container.add_child(body_node)
	body_nodes[body.id] = body_node


## Calculates adaptive size parameters based on actual system layout.
## Ensures bodies are sized relative to orbital spacing so they don't overlap.
## @return: Dictionary with sizing parameters.
func _calculate_adaptive_sizes() -> Dictionary:
	var min_planet_spacing: float = INF
	var max_planet_distance: float = 0.0
	var min_planet_distance: float = INF
	
	# Collect all planet distances in viewport units
	var planet_distances: Array[float] = []
	for planet in current_system.get_planets():
		if planet.has_orbital():
			var d: float = scale_manager.distance_to_units(planet.orbital.semi_major_axis_m)
			planet_distances.append(d)
			max_planet_distance = maxf(max_planet_distance, d)
			min_planet_distance = minf(min_planet_distance, d)
	
	# Sort and find minimum spacing between adjacent planets
	planet_distances.sort()
	for i in range(1, planet_distances.size()):
		var gap: float = planet_distances[i] - planet_distances[i - 1]
		if gap > 1e-6:
			min_planet_spacing = minf(min_planet_spacing, gap)
	
	# If only one planet or no spacing found, use distance to star
	if min_planet_spacing == INF:
		if planet_distances.size() > 0:
			min_planet_spacing = planet_distances[0]
		else:
			min_planet_spacing = 1.0
	
	# Max body radius should be a fraction of the smallest gap
	var max_body_display: float = min_planet_spacing * 0.2
	
	# Star should be largest, planet next, moon smaller, asteroid smallest
	var star_radius: float = min_planet_spacing * 0.3
	var planet_radius: float = min_planet_spacing * 0.12
	var moon_radius: float = min_planet_spacing * 0.06
	var asteroid_radius: float = min_planet_spacing * 0.04
	
	# Apply minimum visibility threshold
	var min_visible: float = 0.001
	star_radius = maxf(star_radius, min_visible * 3.0)
	planet_radius = maxf(planet_radius, min_visible * 2.0)
	moon_radius = maxf(moon_radius, min_visible)
	asteroid_radius = maxf(asteroid_radius, min_visible * 0.5)
	
	return {
		"star_radius": star_radius,
		"planet_radius": planet_radius,
		"moon_radius": moon_radius,
		"asteroid_radius": asteroid_radius,
		"max_planet_distance": max_planet_distance,
		"min_planet_spacing": min_planet_spacing,
	}


## Gets the adaptive display radius for a body.
## Uses relative sizing from physical properties within the type's size budget.
## @param body: The celestial body.
## @param size_params: Adaptive size parameters.
## @return: Display radius in viewport units.
func _get_adaptive_body_radius(body: CelestialBody, size_params: Dictionary) -> float:
	var base_radius: float = 0.0
	var type_budget: float = 0.0
	
	match body.type:
		_celestial_type.Type.STAR:
			type_budget = size_params["star_radius"] as float
			# Scale stars by relative radius (use solar radii)
			var r_solar: float = body.physical.radius_m / _units.SOLAR_RADIUS_METERS
			# Typical range: 0.1 to 10 solar radii -> map to 0.5x to 1.5x budget
			var scale_factor: float = clampf(0.5 + log(maxf(r_solar, 0.01)) / log(10.0) * 0.5, 0.3, 2.0)
			base_radius = type_budget * scale_factor
		
		_celestial_type.Type.PLANET:
			type_budget = size_params["planet_radius"] as float
			# Scale planets by relative radius (use earth radii)
			var r_earth: float = body.physical.radius_m / _units.EARTH_RADIUS_METERS
			# Typical range: 0.3 to 11 earth radii -> map to 0.4x to 1.5x budget
			var scale_factor: float = clampf(0.4 + log(maxf(r_earth, 0.1)) / log(10.0) * 0.5, 0.3, 1.5)
			base_radius = type_budget * scale_factor
		
		_celestial_type.Type.MOON:
			type_budget = size_params["moon_radius"] as float
			var r_earth: float = body.physical.radius_m / _units.EARTH_RADIUS_METERS
			var scale_factor: float = clampf(0.5 + log(maxf(r_earth, 0.001)) / log(10.0) * 0.3, 0.3, 1.5)
			base_radius = type_budget * scale_factor
		
		_celestial_type.Type.ASTEROID:
			type_budget = size_params["asteroid_radius"] as float
			base_radius = type_budget
		
		_:
			type_budget = size_params["planet_radius"] as float
			base_radius = type_budget
	
	return maxf(base_radius, 0.0005)


## Calculates the display position for a body.
## @param body: The celestial body.
## @return: 3D position in display space.
func _calculate_body_position(body: CelestialBody) -> Vector3:
	if not body.has_orbital():
		# Stars at system center (or barycenter for binaries)
		return Vector3.ZERO
	
	var orbital = body.orbital
	
	# Get parent position
	var parent_pos: Vector3 = Vector3.ZERO
	if not orbital.parent_id.is_empty() and current_system:
		var parent: CelestialBody = current_system.get_body(orbital.parent_id)
		if parent:
			parent_pos = _calculate_body_position(parent)
	
	# Calculate orbital position relative to parent
	var orbital_pos: Vector3 = scale_manager.get_orbital_position(
		orbital.semi_major_axis_m,
		orbital.eccentricity,
		orbital.inclination_deg,
		orbital.longitude_of_ascending_node_deg,
		orbital.argument_of_periapsis_deg,
		orbital.mean_anomaly_deg
	)
	
	return parent_pos + orbital_pos


## Creates orbit path visualizations.
func _create_orbit_visualizations() -> void:
	if not current_system or not orbit_renderer:
		return
	
	# Create orbits for planets
	for planet in current_system.get_planets():
		if planet.has_orbital():
			_create_orbit_path(planet)
	
	# Create orbits for moons (optional, can be dense)
	if show_orbits_check and show_orbits_check.button_pressed:
		for moon in current_system.get_moons():
			if moon.has_orbital():
				_create_orbit_path(moon)


## Creates an orbit path for a body.
## @param body: The celestial body with orbital parameters.
func _create_orbit_path(body: CelestialBody) -> void:
	if not body.has_orbital():
		return
	
	var orbital = body.orbital
	
	# Generate orbit points
	var points: PackedVector3Array = scale_manager.generate_orbit_points(
		orbital.semi_major_axis_m,
		orbital.eccentricity,
		orbital.inclination_deg,
		orbital.longitude_of_ascending_node_deg,
		orbital.argument_of_periapsis_deg,
		128
	)
	
	# Get parent position offset
	var parent_pos: Vector3 = Vector3.ZERO
	if not orbital.parent_id.is_empty() and current_system:
		var parent: CelestialBody = current_system.get_body(orbital.parent_id)
		if parent:
			parent_pos = _calculate_body_position(parent)
	
	# Offset all points by parent position
	for i in range(points.size()):
		points[i] += parent_pos
	
	# Add orbit to renderer
	orbit_renderer.add_orbit(body.id, points, body.type)


## Creates zone visualizations (habitable zone, frost line).
func _create_zone_visualizations() -> void:
	if not current_system or not zones_container:
		return
	
	# Create zones for each orbit host
	for host in current_system.orbit_hosts:
		if host.has_valid_zone():
			_create_habitable_zone_ring(host)
			_create_frost_line_ring(host)


## Creates a habitable zone ring visualization.
## @param host: The orbit host.
func _create_habitable_zone_ring(host: OrbitHost) -> void:
	var inner_radius: float = scale_manager.distance_to_units(host.habitable_zone_inner_m)
	var outer_radius: float = scale_manager.distance_to_units(host.habitable_zone_outer_m)
	
	if outer_radius <= inner_radius:
		return
	
	# Create inner and outer rings
	var inner_ring: MeshInstance3D = _create_zone_ring_mesh(
		inner_radius,
		Color(0.2, 0.8, 0.3, 0.15),
		"HZ_Inner_" + host.node_id
	)
	
	var outer_ring: MeshInstance3D = _create_zone_ring_mesh(
		outer_radius,
		Color(0.1, 0.4, 0.1, 0.1),
		"HZ_Outer_" + host.node_id
	)
	
	zones_container.add_child(inner_ring)
	zones_container.add_child(outer_ring)


## Creates a frost line ring visualization.
## @param host: The orbit host.
func _create_frost_line_ring(host: OrbitHost) -> void:
	var frost_radius: float = scale_manager.distance_to_units(host.frost_line_m)
	
	var ring_mesh: MeshInstance3D = _create_zone_ring_mesh(
		frost_radius,
		Color(0.3, 0.5, 0.8, 0.2),
		"Frost_" + host.node_id
	)
	
	zones_container.add_child(ring_mesh)


## Creates a ring mesh for zone visualization.
## @param radius: Ring radius.
## @param color: Ring color.
## @param node_name: Node name.
## @return: MeshInstance3D with ring.
func _create_zone_ring_mesh(
	radius: float,
	color: Color,
	node_name: String
) -> MeshInstance3D:
	var ring: MeshInstance3D = MeshInstance3D.new()
	ring.name = node_name
	
	# Use ImmediateMesh with LINE_STRIP (consistent with OrbitRenderer)
	var mesh: ImmediateMesh = ImmediateMesh.new()
	var segments: int = 64
	
	mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	for i in range(segments + 1):
		var angle: float = (float(i) / float(segments)) * TAU
		mesh.surface_add_vertex(Vector3(cos(angle) * radius, 0.0, sin(angle) * radius))
	mesh.surface_end()
	
	ring.mesh = mesh
	
	# Material
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = color
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	ring.material_override = material
	
	ring.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	
	return ring


## Clears all body nodes.
func _clear_bodies() -> void:
	body_nodes.clear()
	if bodies_container:
		for child in bodies_container.get_children():
			child.queue_free()


## Clears all orbit visualizations.
func _clear_orbits() -> void:
	if orbit_renderer:
		orbit_renderer.clear()


## Clears all zone visualizations.
func _clear_zones() -> void:
	if zones_container:
		for child in zones_container.get_children():
			child.queue_free()


## Handles body click.
## @param body_id: The clicked body ID.
func _on_body_clicked(body_id: String) -> void:
	select_body(body_id)


## Selects a body.
## @param body_id: The body ID to select.
func select_body(body_id: String) -> void:
	# Deselect previous body
	if not selected_body_id.is_empty() and body_nodes.has(selected_body_id):
		var prev_node: SystemBodyNode = body_nodes[selected_body_id] as SystemBodyNode
		if prev_node:
			prev_node.set_selected(false)
	
	selected_body_id = body_id
	
	# Focus camera on body
	if body_nodes.has(body_id):
		var node: SystemBodyNode = body_nodes[body_id] as SystemBodyNode
		if node:
			node.set_selected(true)
			camera.focus_on_position(node.global_position)
	
	# Highlight orbit
	if orbit_renderer:
		orbit_renderer.highlight_orbit(body_id)
	
	_update_inspector_body()
	set_status("Selected: %s" % body_id)


## Deselects the current body.
func deselect_body() -> void:
	# Deselect current body node
	if not selected_body_id.is_empty() and body_nodes.has(selected_body_id):
		var node: SystemBodyNode = body_nodes[selected_body_id] as SystemBodyNode
		if node:
			node.set_selected(false)
	
	selected_body_id = ""
	
	if orbit_renderer:
		orbit_renderer.highlight_orbit("")
	
	_update_inspector_system()


## Updates inspector to show system info.
func _update_inspector_system() -> void:
	if inspector_panel and current_system:
		inspector_panel.display_system(current_system)


## Updates inspector to show selected body info.
func _update_inspector_body() -> void:
	if not inspector_panel or not current_system or selected_body_id.is_empty():
		return
	
	var body: CelestialBody = current_system.get_body(selected_body_id)
	if body:
		inspector_panel.display_selected_body(body)


## Handles unhandled input for deselection.
func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var mouse_event: InputEventMouseButton = event as InputEventMouseButton
		if mouse_event.pressed and mouse_event.button_index == MOUSE_BUTTON_LEFT:
			# Click on empty space deselects
			if not selected_body_id.is_empty():
				deselect_body()


## Handles open in viewer request from inspector panel.
## @param body: The body to open in the object viewer.
func _on_open_body_in_viewer(body: CelestialBody) -> void:
	if body:
		open_body_in_viewer.emit(body)


## Sets the status message.
## @param message: The status message.
func set_status(message: String) -> void:
	if status_label:
		status_label.text = message
		status_label.modulate = Color(0.7, 0.7, 0.7, 1)


## Sets an error message.
## @param message: The error message.
func set_error(message: String) -> void:
	if status_label:
		status_label.text = "Error: " + message
		status_label.modulate = Color(1.0, 0.3, 0.3)
	push_error(message)
