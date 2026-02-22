## Main viewer scene for inspecting solar systems.
## Handles system display, body selection, and user interactions.
class_name SystemViewer
extends Node3D

## Emitted when a body should be opened in the object viewer.
## Carries the body and its associated moons (empty array if none).
signal open_body_in_viewer(body: CelestialBody, moons: Array[CelestialBody])

## Signal emitted when the user wants to go back to the galaxy viewer.
signal back_to_galaxy_requested

const _solar_system: GDScript = preload("res://src/domain/system/SolarSystem.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")
const _system_fixture_generator: GDScript = preload("res://src/domain/system/fixtures/SystemFixtureGenerator.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _system_display_layout: GDScript = preload("res://src/app/system_viewer/SystemDisplayLayout.gd")
const _system_body_node_scene: PackedScene = preload("res://src/app/system_viewer/SystemBodyNode.tscn")
const _orbit_renderer: GDScript = preload("res://src/app/system_viewer/OrbitRenderer.gd")
const _save_load_class: GDScript = preload("res://src/app/system_viewer/SystemViewerSaveLoad.gd")
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

# Save/load controls
@onready var save_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton
@onready var load_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton

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

## Current layout (calculated positions and sizes).
var current_layout: SystemDisplayLayout.SystemLayout = null

## Currently selected body ID
var selected_body_id: String = ""

## Body node references (body_id -> SystemBodyNode)
var body_nodes: Dictionary = {}

## Orbit renderer instance
var orbit_renderer: OrbitRenderer = null

## Whether the viewer is ready
var is_ready: bool = false

## Whether orbital animation is enabled
var animation_enabled: bool = true

## Save/load helper instance
var _save_load: RefCounted = _save_load_class.new()


func _ready() -> void:
	_setup_viewport()
	_setup_camera()
	_setup_generation_ui()
	_setup_view_ui()
	_setup_orbit_renderer()
	_setup_save_load_ui()
	_setup_tooltips()
	_connect_signals()
	
	set_status("System viewer initialized")
	is_ready = true
	
	# Generate an initial system
	_on_generate_pressed()


func _process(delta: float) -> void:
	if animation_enabled and current_layout != null:
		_update_orbital_animation(delta)


## Updates orbital animation for all bodies and orbit paths.
## @param delta: Time elapsed since last frame.
func _update_orbital_animation(delta: float) -> void:
	_system_display_layout.update_orbits(current_layout, delta)
	for body_id in body_nodes:
		var node: SystemBodyNode = body_nodes[body_id] as SystemBodyNode
		var layout: SystemDisplayLayout.BodyLayout = current_layout.get_body_layout(body_id)
		if node and layout:
			node.global_position = layout.position
	if orbit_renderer:
		orbit_renderer.update_orbit_positions(current_layout.host_positions)


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
		star_count_spin.max_value = 10
		star_count_spin.value = 1
	
	if seed_input:
		seed_input.value = randi() % 1000000


## Sets up view options UI.
func _setup_view_ui() -> void:
	if show_orbits_check:
		show_orbits_check.button_pressed = true

	if show_zones_check:
		show_zones_check.button_pressed = false # Zones less meaningful with simplified layout


## Sets up orbit renderer.
func _setup_orbit_renderer() -> void:
	if orbits_container:
		orbit_renderer = OrbitRenderer.new()
		orbit_renderer.name = "OrbitRenderer"
		orbits_container.add_child(orbit_renderer)


## Sets up save/load UI initial state.
func _setup_save_load_ui() -> void:
	_update_save_button_state()


## Sets up tooltips for all interactive UI elements.
func _setup_tooltips() -> void:
	if generate_button:
		generate_button.tooltip_text = "Generate system with current settings"
	if reroll_button:
		reroll_button.tooltip_text = "Generate with a new random seed"
	if star_count_spin:
		star_count_spin.tooltip_text = "Number of stars in the system (1-10)"
	if seed_input:
		seed_input.tooltip_text = "Generation seed for deterministic results"
	if show_orbits_check:
		show_orbits_check.tooltip_text = "Toggle orbital path visibility"
	if show_zones_check:
		show_zones_check.tooltip_text = "Toggle habitable zone visibility"
	if save_button:
		save_button.tooltip_text = "Save current system to file (Ctrl+S)"
	if load_button:
		load_button.tooltip_text = "Load system from file (Ctrl+O)"


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

	# Connect save/load buttons
	if save_button:
		save_button.pressed.connect(_on_save_pressed)
	if load_button:
		load_button.pressed.connect(_on_load_pressed)

	# Connect back button if present (back to galaxy)
	var back_button: Button = get_node_or_null("UI/TopBar/MarginContainer/HBoxContainer/BackButton")
	if back_button:
		back_button.pressed.connect(_on_back_pressed)


## Handles back button press to return to galaxy view.
func _on_back_pressed() -> void:
	back_to_galaxy_requested.emit()


## Handles keyboard shortcuts for the system viewer.
func _unhandled_key_input(event: InputEvent) -> void:
	var key_event: InputEventKey = event as InputEventKey
	if key_event == null or not key_event.pressed or key_event.echo:
		return

	if key_event.keycode == KEY_S and key_event.ctrl_pressed:
		_on_save_pressed()
		get_viewport().set_input_as_handled()
	elif key_event.keycode == KEY_O and key_event.ctrl_pressed:
		_on_load_pressed()
		get_viewport().set_input_as_handled()
	elif key_event.keycode == KEY_ESCAPE:
		back_to_galaxy_requested.emit()
		get_viewport().set_input_as_handled()


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


## Handles save button press.
func _on_save_pressed() -> void:
	_save_load.on_save_pressed(self)


## Handles load button press.
func _on_load_pressed() -> void:
	_save_load.on_load_pressed(self)


## Updates save button enabled state based on whether a system exists.
func _update_save_button_state() -> void:
	if save_button:
		save_button.disabled = current_system == null


## Generates a solar system.
## @param seed_value: Generation seed.
## @param min_stars: Minimum star count.
## @param max_stars: Maximum star count.
func generate_system(seed_value: int, min_stars: int = 1, max_stars: int = 1) -> void:
	set_status("Generating system with seed %d..." % seed_value)
	
	var spec: SolarSystemSpec = SolarSystemSpec.new(seed_value, min_stars, max_stars)
	
	# Generate with population data so inspector panel shows population stats.
	var system: SolarSystem = SystemFixtureGenerator.generate_system(spec, true)
	
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

	current_layout = SystemDisplayLayout.calculate_layout(system)

	_clear_bodies()
	_update_save_button_state()

	_clear_orbits()
	_clear_zones()

	_create_body_nodes()
	_create_orbit_visualizations()

	if show_zones_check and show_zones_check.button_pressed:
		_create_zone_visualizations()

	_update_inspector_system()

	_fit_camera_to_system()

	if system.name and not system.name.is_empty():
		set_status("Viewing: %s" % system.name)
	else:
		set_status("Generated: %s" % system.get_summary())


## Fits the camera zoom to show the entire system.
func _fit_camera_to_system() -> void:
	if not camera:
		return
	if not current_system or not current_layout:
		camera.focus_on_origin()
		return

	var max_extent: float = current_layout.total_extent

	if max_extent < 10.0:
		max_extent = 20.0

	var target_height: float = max_extent * 2.0

	if camera.max_height > 0:
		target_height = minf(target_height, camera.max_height)
	target_height = maxf(target_height, 20.0)

	camera.min_height = minf(10.0, target_height * 0.1)
	if target_height > camera.max_height:
		camera.max_height = target_height * 1.5

	camera._target_position = Vector3.ZERO
	camera._target_height = target_height
	camera._height = target_height
	camera._target_pitch = deg_to_rad(60.0)
	camera._yaw = 0.0
	camera._smooth_target = Vector3.ZERO

## Clears the current display.
func clear_display() -> void:
	current_system = null
	current_layout = null
	selected_body_id = ""

	_clear_bodies()
	_clear_orbits()
	_clear_zones()
	_update_save_button_state()

	set_status("No system loaded")


## Creates 3D nodes for all bodies in the system (stars and planets only; moons not displayed).
func _create_body_nodes() -> void:
	if not current_system or not current_layout or not bodies_container:
		return

	for star in current_system.get_stars():
		_create_body_node_from_layout(star)

	for planet in current_system.get_planets():
		_create_body_node_from_layout(planet)


## Creates a 3D node for a single body using pre-calculated layout.
## @param body: The celestial body.
func _create_body_node_from_layout(body: CelestialBody) -> void:
	if not body:
		return

	var layout: SystemDisplayLayout.BodyLayout = current_layout.get_body_layout(body.id)
	if not layout:
		push_warning("No layout found for body: %s" % body.id)
		return

	var body_node: SystemBodyNode = _system_body_node_scene.instantiate() as SystemBodyNode
	if not body_node:
		return

	body_node.setup(body, layout.display_radius, layout.position)
	body_node.body_selected.connect(_on_body_clicked)

	bodies_container.add_child(body_node)
	body_nodes[body.id] = body_node


## Creates orbit path visualizations for planets and stars.
func _create_orbit_visualizations() -> void:
	if not current_system or not current_layout or not orbit_renderer:
		return

	for planet in current_system.get_planets():
		var body_layout: SystemDisplayLayout.BodyLayout = current_layout.get_body_layout(planet.id)
		if body_layout and body_layout.orbit_radius > 0:
			_create_circular_orbit(planet.id, body_layout, planet.type)

	for star in current_system.get_stars():
		var star_orbit: SystemDisplayLayout.BodyLayout = current_layout.get_star_orbit(star.id)
		if star_orbit and star_orbit.is_orbiting and star_orbit.orbit_radius > 0:
			_create_circular_orbit(star.id + "_orbit", star_orbit, _celestial_type.Type.STAR)


## Creates a circular orbit visualization for a body.
## @param orbit_id: Unique ID for this orbit.
## @param body_layout: The body's layout data with orbit info.
## @param body_type: Type of body (for coloring).
func _create_circular_orbit(orbit_id: String, body_layout: SystemDisplayLayout.BodyLayout, body_type: int) -> void:
	var center: Vector3 = body_layout.orbit_center
	var radius: float = body_layout.orbit_radius

	var points: PackedVector3Array = PackedVector3Array()
	var segments: int = 64

	for i in range(segments + 1):
		var angle: float = (float(i) / float(segments)) * TAU
		var point: Vector3 = center + Vector3(
			cos(angle) * radius,
			0.0,
			sin(angle) * radius
		)
		points.append(point)

	orbit_renderer.add_orbit(
		orbit_id,
		points,
		body_type,
		body_layout.orbit_parent_id,
		center
	)


## Creates zone visualizations (habitable zone, frost line).
## Skipped for simplified layout; zones are less meaningful with fixed orbit spacing.
func _create_zone_visualizations() -> void:
	pass


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


## Handles open-in-viewer request from the inspector panel.
## Collects all moons whose orbital parent_id matches this body, then emits
## open_body_in_viewer with both the body and its moons.
## @param body: The body the user wants to inspect in detail.
func _on_open_body_in_viewer(body: CelestialBody) -> void:
	if not body:
		return

	var moons: Array[CelestialBody] = []
	if current_system and body.type == CelestialType.Type.PLANET:
		moons = current_system.get_moons_of_planet(body.id)

	open_body_in_viewer.emit(body, moons)


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


## Returns the current solar system.
## @return: The current system, or null if none loaded.
func get_current_system() -> SolarSystem:
	return current_system


## Updates the seed display to a specific value.
## Used when loading a system to reflect its original seed.
## @param seed_value: The seed to display.
func update_seed_display(seed_value: int) -> void:
	if seed_input:
		seed_input.value = seed_value


## Returns the save/load helper for testing.
## @return: The SystemViewerSaveLoad instance.
func get_save_load() -> RefCounted:
	return _save_load


## Enables or disables orbital animation.
## @param enabled: Whether animation should be enabled.
func set_animation_enabled(enabled: bool) -> void:
	animation_enabled = enabled
