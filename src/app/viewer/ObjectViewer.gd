## Main viewer scene for inspecting celestial objects.
## Handles object display, UI updates, and user interactions.
class_name ObjectViewer
extends Node3D

# Generators
const _star_generator := preload("res://src/domain/generation/generators/StarGenerator.gd")
const _planet_generator := preload("res://src/domain/generation/generators/PlanetGenerator.gd")
const _moon_generator := preload("res://src/domain/generation/generators/MoonGenerator.gd")
const _asteroid_generator := preload("res://src/domain/generation/generators/AsteroidGenerator.gd")

# Specs
const _star_spec := preload("res://src/domain/generation/specs/StarSpec.gd")
const _planet_spec := preload("res://src/domain/generation/specs/PlanetSpec.gd")
const _moon_spec := preload("res://src/domain/generation/specs/MoonSpec.gd")
const _asteroid_spec := preload("res://src/domain/generation/specs/AsteroidSpec.gd")

# Supporting classes
const _parent_context := preload("res://src/domain/generation/ParentContext.gd")
const _seeded_rng := preload("res://src/domain/rng/SeededRng.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _units := preload("res://src/domain/math/Units.gd")
const _stellar_props := preload("res://src/domain/celestial/components/StellarProps.gd")
const _inspector_panel := preload("res://src/app/viewer/InspectorPanel.gd")

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

## 3D element references
@onready var celestial_object_node: Node3D = $CelestialObject
@onready var placeholder_mesh: MeshInstance3D = $CelestialObject/Placeholder
@onready var camera_rig: Node3D = $CameraRig
@onready var camera_arm: Node3D = $CameraRig/CameraArm
@onready var camera: Camera3D = $CameraRig/CameraArm/Camera3D

## Currently displayed celestial body
var current_body: CelestialBody = null

## Whether the viewer is ready
var is_ready: bool = false

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
	_connect_signals()
	
	# Hide placeholder initially
	if placeholder_mesh:
		placeholder_mesh.visible = false
	
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


## Connects UI signals.
func _connect_signals() -> void:
	if generate_button:
		generate_button.pressed.connect(_on_generate_pressed)
	
	if reroll_button:
		reroll_button.pressed.connect(_on_reroll_pressed)


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
	return PlanetGenerator.generate(spec, context, rng)


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
		1.898e27,  # Jupiter mass
		6.9911e7,  # Jupiter radius
		5.0e8      # 500,000 km from Jupiter
	)
	return MoonGenerator.generate(spec, context, rng)


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
	if placeholder_mesh:
		placeholder_mesh.visible = true
		
		# Scale based on object type and radius
		var scale_factor: float = _calculate_display_scale(body)
		placeholder_mesh.scale = Vector3.ONE * scale_factor
	
	# Update camera distance based on object size
	if camera:
		var cam_controller: CameraController = camera as CameraController
		if cam_controller:
			var distance: float = _calculate_camera_distance(body)
			cam_controller.set_distance(distance)
			cam_controller.focus_on_target()
	
	# Update inspector panel
	_update_inspector()


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
				return 0.5  # Very small asteroids get minimum size
			elif km < 100:
				return 0.5 + (km - 10) / 90.0  # Scale 0.5 to 1.5
			else:
				return 1.5  # Large asteroids cap at 1.5
		_:
			return 1.0


## Calculates appropriate camera distance for viewing a body.
## @param body: The celestial body to view.
## @return: Camera distance in units.
func _calculate_camera_distance(body: CelestialBody) -> float:
	var scale: float = _calculate_display_scale(body)
	
	# Camera should be about 3-5x the display radius for good framing
	return clampf(scale * 4.0, 2.0, 50.0)


## Updates the inspector panel with current body properties.
func _update_inspector() -> void:
	if inspector_panel:
		inspector_panel.display_body(current_body)


## Clears the current display.
func clear_display() -> void:
	current_body = null
	
	if placeholder_mesh:
		placeholder_mesh.visible = false
	
	set_status("No object loaded")
	_update_inspector()
