## Main viewer scene for inspecting celestial objects.
## Handles object display, UI updates, and user interactions.
class_name ObjectViewer
extends Node3D

# Generators
const _star_generator: GDScript = preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator: GDScript = preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator: GDScript = preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator: GDScript = preload("res://src/domain/generation/generators/AsteroidGenerator.gd")

# Specs
const _star_spec: GDScript = preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec: GDScript = preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec: GDScript = preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec: GDScript = preload("res://src/domain/generation/specs/AsteroidSpec.gd")

# Supporting classes
const _parent_context: GDScript = preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng: GDScript = preload("res://src/domain/rng/SeededRng.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _units: GDScript = preload("res://src/domain/math/Units.gd")
const _stellar_props: GDScript = preload("res://src/domain/celestial/components/StellarProps.gd")
const _inspector_panel: GDScript = preload("res://src/app/viewer/InspectorPanel.gd")
const _save_data: GDScript = preload("res://src/services/persistence/SaveData.gd")

# Rendering
const _body_renderer_scene: PackedScene = preload("res://src/app/rendering/BodyRenderer.tscn")
const _body_renderer: GDScript = preload("res://src/app/rendering/BodyRenderer.gd")

## UI element references
@onready var status_label: Label = $UI/TopBar/MarginContainer/HBoxContainer/StatusLabel
@onready var side_panel: Panel = $UI/SidePanel
@onready var panel_container: VBoxContainer = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer

# Generation controls
@onready var type_option: OptionButton = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/TypeContainer/TypeOption
@onready var seed_input: SpinBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput
@onready var generate_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton
@onready var reroll_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/RerollButton

# Inspector panel
@onready var inspector_panel: InspectorPanel = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer

# File operations
@onready var save_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/SaveButton
@onready var load_button: Button = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/LoadButton
@onready var file_info: Label = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileInfo
@onready var save_file_dialog: FileDialog = $SaveFileDialog
@onready var load_file_dialog: FileDialog = $LoadFileDialog

## 3D element references
@onready var body_renderer: BodyRenderer = $BodyRenderer
@onready var camera_rig: Node3D = $CameraRig
@onready var camera_arm: Node3D = $CameraRig/CameraArm
@onready var camera: Camera3D = $CameraRig/CameraArm/Camera3D
@onready var world_environment: WorldEnvironment = $Environment/WorldEnvironment

## Currently displayed celestial body
var current_body: CelestialBody = null

## Whether the viewer is ready
var is_ready: bool = false

## Signal emitted when user requests to go back to system viewer.
signal back_to_system_requested

## Whether this viewer was opened from the system viewer (shows back button).
var _navigated_from_system: bool = false

## The back button node (created dynamically when navigated from system).
var _back_button: Button = null

## Whether to animate body rotation
@export var animate_rotation: bool = true

## Rotation animation speed multiplier
@export var rotation_speed: float = 1.0

## Object types for generation
enum ObjectType {
	STAR,
	PLANET,
	MOON,
	ASTEROID
}


func _ready() -> void:
	# Set up initial state
	_setup_viewport()
	_setup_camera()
	_setup_generation_ui()
	_setup_file_dialogs()
	_connect_signals()
	
	set_status("Viewer initialized")
	is_ready = true
	
	# Generate an initial object
	_on_generate_pressed()


## Sets up viewport settings.
func _setup_viewport() -> void:
	# Get the viewport
	var viewport: Viewport = get_viewport()
	if not viewport:
		return
	
	# Enable HDR for better star rendering later
	viewport.use_hdr_2d = true


## Sets up initial camera position.
func _setup_camera() -> void:
	if not camera or not camera_rig:
		return
	
	# Set initial camera distance
	if camera is CameraController:
		var controller: CameraController = camera as CameraController
		controller.set_distance(10.0)
	
	# Point camera at origin
	camera.look_at(Vector3.ZERO, Vector3.UP)


## Sets up the generation UI.
func _setup_generation_ui() -> void:
	if not type_option:
		return
	
	# Populate type options
	type_option.clear()
	type_option.add_item("Star", ObjectType.STAR)
	type_option.add_item("Planet", ObjectType.PLANET)
	type_option.add_item("Moon", ObjectType.MOON)
	type_option.add_item("Asteroid", ObjectType.ASTEROID)
	
	# Default to planet
	type_option.selected = ObjectType.PLANET
	
	# Set initial seed
	if seed_input:
		seed_input.value = randi() % 1000000


## Sets up file dialogs.
func _setup_file_dialogs() -> void:
	if save_file_dialog:
		save_file_dialog.current_dir = OS.get_user_data_dir()
		save_file_dialog.current_file = "celestial_object.sgb"
	
	if load_file_dialog:
		load_file_dialog.current_dir = OS.get_user_data_dir()


## Connects UI signals.
func _connect_signals() -> void:
	if generate_button:
		generate_button.pressed.connect(_on_generate_pressed)
	
	if reroll_button:
		reroll_button.pressed.connect(_on_reroll_pressed)
	
	if save_button:
		save_button.pressed.connect(_on_save_pressed)
	
	if load_button:
		load_button.pressed.connect(_on_load_pressed)
	
	if save_file_dialog:
		save_file_dialog.file_selected.connect(_on_save_file_selected)
	
	if load_file_dialog:
		load_file_dialog.file_selected.connect(_on_load_file_selected)


## Handles generate button press.
func _on_generate_pressed() -> void:
	if not type_option or not seed_input:
		return
	
	var object_type: ObjectType = type_option.get_selected_id() as ObjectType
	var seed_value: int = int(seed_input.value)
	
	generate_object(object_type, seed_value)


## Handles re-roll button press.
func _on_reroll_pressed() -> void:
	# Generate new random seed
	var new_seed: int = randi() % 1000000
	if seed_input:
		seed_input.value = new_seed
	
	_on_generate_pressed()


## Generates a celestial object of the specified type.
## @param object_type: The type of object to generate.
## @param seed_value: The seed for generation.
func generate_object(object_type: ObjectType, seed_value: int) -> void:
	var body: CelestialBody = null
	var rng: SeededRng = SeededRng.new(seed_value)
	
	set_status("Generating %s with seed %d..." % [_get_type_name(object_type), seed_value])
	
	match object_type:
		ObjectType.STAR:
			body = _generate_star(seed_value, rng)
		ObjectType.PLANET:
			body = _generate_planet(seed_value, rng)
		ObjectType.MOON:
			body = _generate_moon(seed_value, rng)
		ObjectType.ASTEROID:
			body = _generate_asteroid(seed_value, rng)
	
	if body:
		display_body(body)
		set_status("Generated: %s" % body.name)
	else:
		set_error("Failed to generate object")


## Generates a star.
## @param seed_value: The generation seed.
## @param rng: The random number generator.
## @return: Generated star body.
func _generate_star(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: StarSpec = StarSpec.random(seed_value)
	return StarGenerator.generate(spec, rng)


## Generates a planet.
## @param seed_value: The generation seed.
## @param rng: The random number generator.
## @return: Generated planet body.
func _generate_planet(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: PlanetSpec = PlanetSpec.random(seed_value)
	var context: ParentContext = ParentContext.sun_like()
	return PlanetGenerator.generate(spec, context, rng, true)


## Generates a moon.
## @param seed_value: The generation seed.
## @param rng: The random number generator.
## @return: Generated moon body.
func _generate_moon(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: MoonSpec = MoonSpec.random(seed_value)
	# Create a Jupiter-like parent context
	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		5.2 * Units.AU_METERS,
		1.898e27, # Jupiter mass
		6.9911e7, # Jupiter radius
		5.0e8 # 500,000 km from Jupiter
	)
	return MoonGenerator.generate(spec, context, rng, true)


## Generates an asteroid.
## @param seed_value: The generation seed.
## @param rng: The random number generator.
## @return: Generated asteroid body.
func _generate_asteroid(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: AsteroidSpec = AsteroidSpec.random(seed_value)
	var context: ParentContext = ParentContext.sun_like(2.7 * Units.AU_METERS)
	return AsteroidGenerator.generate(spec, context, rng)


## Gets the display name for an object type.
## @param object_type: The object type enum.
## @return: Human-readable type name.
func _get_type_name(object_type: ObjectType) -> String:
	match object_type:
		ObjectType.STAR:
			return "star"
		ObjectType.PLANET:
			return "planet"
		ObjectType.MOON:
			return "moon"
		ObjectType.ASTEROID:
			return "asteroid"
		_:
			return "unknown"


## Sets the status message in the UI.
## @param message: The status message to display.
func set_status(message: String) -> void:
	if status_label:
		status_label.text = message
		status_label.modulate = Color(0.7, 0.7, 0.7, 1)


## Sets the error message in the UI.
## @param message: The error message to display.
func set_error(message: String) -> void:
	if status_label:
		status_label.text = "Error: " + message
		status_label.modulate = Color(1.0, 0.3, 0.3)
	push_error(message)


## Displays a celestial body in the viewer.
## @param body: The celestial body to display.
func display_body(body: CelestialBody) -> void:
	if not body:
		set_error("Cannot display null body")
		return
	
	current_body = body
	
	# Update 3D display
	var scale_factor: float = _calculate_display_scale(body)
	
	if body_renderer:
		body_renderer.render_body(body, scale_factor)
	
	# Adjust lighting for body type
	_adjust_lighting_for_body(body)
	
	# Add glow effect for stars
	if body.type == CelestialType.Type.STAR:
		_enable_star_glow(body)
	else:
		_disable_star_glow()
	
	# Update camera distance based on object size
	if camera:
		var cam_controller: CameraController = camera as CameraController
		if cam_controller:
			var distance: float = _calculate_camera_distance(body)
			cam_controller.set_distance(distance)
			cam_controller.focus_on_target()
	
	# Update inspector panel
	_update_inspector()


## Displays an externally-provided celestial body (e.g., from system viewer).
## Shows the back button and disables generation controls.
## @param body: The celestial body to display.
func display_external_body(body: CelestialBody) -> void:
	if not body:
		return

	_navigated_from_system = true
	_show_back_button()
	_set_generation_controls_enabled(false)

	display_body(body)
	set_status("Viewing: %s (from system)" % body.name)


## Calculates appropriate display scale for a body.
## @param body: The celestial body to scale.
## @return: Scale factor for the mesh.
func _calculate_display_scale(body: CelestialBody) -> float:
	var radius_m: float = body.physical.radius_m
	
	# Different scales for different object types
	match body.type:
		CelestialType.Type.STAR:
			# Stars: 1 unit = 1 solar radius
			return clampf(radius_m / Units.SOLAR_RADIUS_METERS, 0.5, 5.0)
		
		CelestialType.Type.PLANET:
			# Planets: 1 unit = 1 Earth radius
			return clampf(radius_m / Units.EARTH_RADIUS_METERS, 0.1, 3.0)
		
		CelestialType.Type.MOON:
			# Moons: similar to planets but smaller range
			return clampf(radius_m / Units.EARTH_RADIUS_METERS * 2.0, 0.1, 2.0)
		
		CelestialType.Type.ASTEROID:
			# Asteroids: scale up significantly for visibility
			var km: float = radius_m / 1000.0
			if km < 10:
				return 0.5 # Very small asteroids get minimum size
			elif km < 100:
				return 0.5 + (km - 10) / 90.0 # Scale 0.5 to 1.5
			else:
				return 1.5 # Large asteroids cap at 1.5
		_:
			return 1.0


## Calculates appropriate camera distance for viewing a body.
## @param body: The celestial body to view.
## @return: Camera distance in units.
func _calculate_camera_distance(body: CelestialBody) -> float:
	var display_scale: float = _calculate_display_scale(body)
	
	# Camera should be about 3-5x the display radius for good framing
	return clampf(display_scale * 4.0, 2.0, 50.0)


## Updates the inspector panel with current body properties.
func _update_inspector() -> void:
	if inspector_panel:
		inspector_panel.display_body(current_body)


## Adjusts scene lighting based on body type.
func _adjust_lighting_for_body(body: CelestialBody) -> void:
	var dir_light: DirectionalLight3D = $Environment/DirectionalLight3D
	if not dir_light:
		return
	
	match body.type:
		CelestialType.Type.STAR:
			# Dim external light for stars (they self-illuminate)
			dir_light.light_energy = 0.1
		CelestialType.Type.ASTEROID:
			# Brighter light for dark asteroids
			dir_light.light_energy = 1.0
		_:
			# Normal lighting for planets/moons
			dir_light.light_energy = 0.5


## Enables glow/bloom for star rendering.
func _enable_star_glow(body: CelestialBody) -> void:
	if not world_environment or not world_environment.environment:
		return
	
	var env: Environment = world_environment.environment
	env.glow_enabled = true
	env.glow_intensity = 1.0
	env.glow_strength = 1.2
	env.glow_bloom = 0.5
	env.glow_blend_mode = Environment.GLOW_BLEND_MODE_SCREEN
	
	# Adjust glow based on star luminosity
	if body.has_stellar():
		var luminosity_solar: float = body.stellar.luminosity_watts / 3.828e26
		env.glow_intensity = clampf(luminosity_solar, 0.5, 2.0)


## Disables glow effects.
func _disable_star_glow() -> void:
	if not world_environment or not world_environment.environment:
		return
	
	var env: Environment = world_environment.environment
	env.glow_enabled = false


## Clears the current display.
func clear_display() -> void:
	current_body = null
	
	if body_renderer:
		body_renderer.clear()
	
	_disable_star_glow()
	
	set_status("No object loaded")
	_update_inspector()


## Handles save button press.
func _on_save_pressed() -> void:
	if not current_body:
		set_error("No object to save")
		return
	
	if save_file_dialog:
		save_file_dialog.popup_centered()


## Handles load button press.
func _on_load_pressed() -> void:
	if load_file_dialog:
		load_file_dialog.popup_centered()


## Handles save file selection.
## @param path: The selected file path.
func _on_save_file_selected(path: String) -> void:
	if not current_body:
		set_error("No object to save")
		return
	
	var error: Error = SaveData.save_body(current_body, path, SaveData.SaveMode.COMPACT, true)
	if error == OK:
		var file_size: int = SaveData.get_file_size(path)
		var size_str: String = SaveData.format_file_size(file_size)
		set_status("Saved to: %s (%s)" % [path.get_file(), size_str])
		if file_info:
			file_info.text = "File: %s (%s)" % [path.get_file(), size_str]
	else:
		set_error("Failed to save: %s" % error_string(error))


## Handles load file selection.
## @param path: The selected file path.
func _on_load_file_selected(path: String) -> void:
	var result: SaveData.LoadResult = SaveData.load_body(path)
	
	if not result.success:
		set_error("Failed to load: %s" % result.error_message)
		return
	
	if not result.body:
		set_error("Loaded file contains no body")
		return
	
	# Display the loaded body
	display_body(result.body)
	
	# Update UI to match loaded body
	if type_option:
		var object_type: ObjectType = _get_object_type_from_body(result.body)
		for i in range(type_option.get_item_count()):
			if type_option.get_item_id(i) == object_type:
				type_option.selected = i
				break
	
	if seed_input and result.body.provenance:
		seed_input.value = result.body.provenance.generation_seed
	
	# Update file info
	var file_size: int = SaveData.get_file_size(path)
	var size_str: String = SaveData.format_file_size(file_size)
	set_status("Loaded: %s" % result.body.name)
	if file_info:
		file_info.text = "File: %s (%s)" % [path.get_file(), size_str]


## Gets the object type enum from a celestial body.
## @param body: The celestial body.
## @return: ObjectType enum value.
func _get_object_type_from_body(body: CelestialBody) -> ObjectType:
	match body.type:
		CelestialType.Type.STAR:
			return ObjectType.STAR
		CelestialType.Type.PLANET:
			return ObjectType.PLANET
		CelestialType.Type.MOON:
			return ObjectType.MOON
		CelestialType.Type.ASTEROID:
			return ObjectType.ASTEROID
		_:
			return ObjectType.PLANET


## Shows the back-to-system button in the top bar.
func _show_back_button() -> void:
	if _back_button != null:
		_back_button.visible = true
		return

	var top_bar_container: HBoxContainer = $UI/TopBar/MarginContainer/HBoxContainer
	if not top_bar_container:
		return

	# Insert back button at position 0 (leftmost)
	_back_button = Button.new()
	_back_button.text = "â† Back to System"
	_back_button.tooltip_text = "Return to solar system viewer"
	_back_button.pressed.connect(_on_back_pressed)
	top_bar_container.add_child(_back_button)
	top_bar_container.move_child(_back_button, 0)


## Hides the back-to-system button.
func _hide_back_button() -> void:
	if _back_button != null:
		_back_button.visible = false


## Handles back button press.
func _on_back_pressed() -> void:
	_navigated_from_system = false
	_hide_back_button()
	_set_generation_controls_enabled(true)
	back_to_system_requested.emit()


## Enables or disables generation controls.
## When viewing an external body, generation controls should be disabled.
## @param enabled: Whether controls should be enabled.
func _set_generation_controls_enabled(enabled: bool) -> void:
	if type_option:
		type_option.disabled = not enabled
	if seed_input:
		seed_input.editable = enabled
	if generate_button:
		generate_button.disabled = not enabled
	if reroll_button:
		reroll_button.disabled = not enabled
