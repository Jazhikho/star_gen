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
const _population_likelihood: GDScript = preload("res://src/domain/population/PopulationLikelihood.gd")

# Rendering
const _body_renderer: GDScript = preload("res://src/app/rendering/BodyRenderer.gd")
const _moon_system_script: GDScript = preload("res://src/app/viewer/ObjectViewerMoonSystem.gd")

## UI element references
@onready var status_label: Label = $UI/TopBar/MarginContainer/HBoxContainer/StatusLabel
@onready var side_panel: Panel = $UI/SidePanel
@onready var panel_container: VBoxContainer = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer

# Generation controls
@onready var type_option: OptionButton = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/TypeContainer/TypeOption
@onready var seed_input: SpinBox = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput
@onready var population_container: HBoxContainer = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationContainer
@onready var population_option: OptionButton = $UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationContainer/PopulationOption
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

## Moon system manager (handles display, orbital animation, focus).
var _moon_system: RefCounted = null

## Whether the viewer is ready
var is_ready: bool = false

## Signal emitted when user requests to go back to system viewer.
signal back_to_system_requested

## Emitted when focus shifts to a moon (null = back to planet).
signal moon_focused(moon: CelestialBody)

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

## Cached display scale for the primary body (updated in display_body_with_moons).
var _primary_display_scale: float = 1.0


func _ready() -> void:
	_setup_viewport()
	_setup_camera()
	_setup_generation_ui()
	_setup_file_dialogs()
	_setup_moon_system()
	_connect_signals()

	set_status("Viewer initialized")
	is_ready = true
	_on_generate_pressed()


## Drives body rotation, moon orbital motion, and camera follow each frame.
func _process(delta: float) -> void:
	if animate_rotation and body_renderer and current_body:
		body_renderer.rotate_body(delta, rotation_speed)

	if _moon_system and _moon_system.has_moons():
		_moon_system.update_orbital_positions(delta)

		if _moon_system.get_focused_moon() != null:
			var ctrl: CameraController = camera as CameraController
			if ctrl:
				ctrl.set_target_position(_moon_system.get_focused_moon_position())


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

	# Populate population override options (visible only for planet/moon)
	if population_option:
		population_option.clear()
		population_option.add_item("Auto", _population_likelihood.Override.AUTO)
		population_option.add_item("None", _population_likelihood.Override.NONE)
		population_option.add_item("Natural populace", _population_likelihood.Override.FORCE_NATIVES)
		population_option.add_item("Colony", _population_likelihood.Override.FORCE_COLONY)
		population_option.selected = 0

	if type_option:
		type_option.item_selected.connect(_on_type_selected)

	# Set initial seed
	if seed_input:
		seed_input.value = randi() % 1000000

	_update_population_visibility()


## Sets up file dialogs.
func _setup_file_dialogs() -> void:
	if save_file_dialog:
		save_file_dialog.current_dir = OS.get_user_data_dir()
		save_file_dialog.current_file = "celestial_object.sgb"
	
	if load_file_dialog:
		load_file_dialog.current_dir = OS.get_user_data_dir()


## Creates and initializes the moon system manager.
func _setup_moon_system() -> void:
	_moon_system = _moon_system_script.new()
	_moon_system.setup(body_renderer)
	_moon_system.moon_focused.connect(_on_moon_system_focus_changed)


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
	if inspector_panel:
		inspector_panel.moon_selected.connect(_on_inspector_moon_selected)


## Handles type selection change; updates population option visibility.
func _on_type_selected(_index: int) -> void:
	_update_population_visibility()


## Shows or hides the population override option based on object type.
func _update_population_visibility() -> void:
	if not population_container or not type_option:
		return
	var object_type: ObjectType = type_option.get_selected_id() as ObjectType
	population_container.visible = object_type == ObjectType.PLANET or object_type == ObjectType.MOON


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


## Returns the selected population override mode.
## @return: PopulationLikelihood.Override value.
func _get_population_override() -> int:
	if not population_option or not population_container.visible:
		return _population_likelihood.Override.AUTO
	return population_option.get_selected_id() as int


## Generates a celestial object of the given type.
## For planets, also generates a small set of illustrative moons.
## @param object_type: The type of object to generate.
## @param seed_value: Deterministic generation seed.
func generate_object(object_type: ObjectType, seed_value: int) -> void:
	var rng: SeededRng = SeededRng.new(seed_value)
	var body: CelestialBody = null
	var moons: Array[CelestialBody] = []

	set_status("Generating %s with seed %d…" % [_get_type_name(object_type), seed_value])

	var pop_override: int = _get_population_override()

	match object_type:
		ObjectType.STAR:
			body = _generate_star(seed_value, rng)
		ObjectType.PLANET:
			body = _generate_planet(seed_value, rng, pop_override)
			if body:
				moons = _generate_moons_for_planet(body, seed_value, rng, pop_override)
		ObjectType.MOON:
			body = _generate_moon(seed_value, rng, pop_override)
		ObjectType.ASTEROID:
			body = _generate_asteroid(seed_value, rng)

	if body:
		display_body_with_moons(body, moons)
		if moons.is_empty():
			set_status("Generated: %s" % body.name)
		else:
			set_status("Generated: %s with %d moon(s)" % [body.name, moons.size()])
	else:
		set_error("Failed to generate object")


## Generates a star.
## @param seed_value: The generation seed.
## @param rng: The random number generator.
## @return: Generated star body.
func _generate_star(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: StarSpec = StarSpec.random(seed_value)
	return StarGenerator.generate(spec, rng)


## Generates a planet from a random spec at 1 AU from a solar-type star.
## @param seed_value: Generation seed.
## @param rng: Seeded RNG.
## @param population_override: PopulationLikelihood.Override for population mode.
## @return: Generated planet body.
func _generate_planet(seed_value: int, rng: SeededRng, population_override: int = 0) -> CelestialBody:
	var spec: PlanetSpec = PlanetSpec.random(seed_value)
	var context: ParentContext = ParentContext.sun_like()
	var enable_pop: bool = population_override != _population_likelihood.Override.NONE
	return PlanetGenerator.generate(spec, context, rng, enable_pop, population_override)


## Moon count ranges by planet mass category (aligned with SystemMoonGenerator).
## Format: [min_moons, max_moons, probability_of_having_moons]
const _MOON_COUNT_GAS_GIANT: Array = [2, 8, 0.95]
const _MOON_COUNT_ICE_GIANT: Array = [1, 5, 0.90]
const _MOON_COUNT_SUPER_EARTH: Array = [0, 2, 0.40]
const _MOON_COUNT_TERRESTRIAL: Array = [0, 2, 0.30]
const _MOON_COUNT_SUB_TERRESTRIAL: Array = [0, 1, 0.15]
const _MOON_COUNT_DWARF: Array = [0, 1, 0.05]


## Generates illustrative moons for a planet.
## Count is determined by planet mass category (same ranges as system generation).
## Seeds are offset from the planet seed so each moon is independent.
## @param planet: The generated planet body.
## @param base_seed: The planet's generation seed.
## @param rng: Planet's RNG (already advanced past planet generation).
## @param population_override: PopulationLikelihood.Override for moon population mode.
## @return: Array of generated moon bodies, may be empty.
func _generate_moons_for_planet(
	planet: CelestialBody,
	base_seed: int,
	rng: SeededRng,
	population_override: int = 0
) -> Array[CelestialBody]:
	var moons: Array[CelestialBody] = []
	var earth_masses: float = planet.physical.mass_kg / Units.EARTH_MASS_KG

	var moon_count: int = _determine_moon_count(earth_masses, rng)
	if moon_count == 0:
		return moons

	var min_orbit_m: float = planet.physical.radius_m * 10.0
	if planet.has_ring_system():
		var ring_outer_m: float = planet.ring_system.get_outer_radius_m()
		min_orbit_m = maxf(min_orbit_m, ring_outer_m * 1.2)

	var context: ParentContext = ParentContext.for_moon(
		Units.SOLAR_MASS_KG,
		StellarProps.SOLAR_LUMINOSITY_WATTS,
		5778.0,
		4.6e9,
		Units.AU_METERS,
		planet.physical.mass_kg,
		planet.physical.radius_m,
		min_orbit_m
	)

	for i: int in range(moon_count):
		var moon_seed: int = base_seed + 100 + i * 37
		var moon_rng: SeededRng = SeededRng.new(moon_seed)
		var spec: MoonSpec = MoonSpec.random(moon_seed)
		spec.set_override("orbital.min_semi_major_axis_m", min_orbit_m)
		var enable_moon_pop: bool = population_override != _population_likelihood.Override.NONE
		var moon: CelestialBody = MoonGenerator.generate(spec, context, moon_rng, enable_moon_pop, planet, population_override)
		if moon:
			moon.name = "%s %s" % [planet.name, _to_roman(i + 1)]
			moons.append(moon)

	if planet.has_ring_system():
		var ring_outer_m: float = planet.ring_system.get_outer_radius_m()
		var filtered: Array[CelestialBody] = []
		for m: CelestialBody in moons:
			if not m.has_orbital():
				filtered.append(m)
			elif m.orbital.semi_major_axis_m > ring_outer_m:
				filtered.append(m)
		moons = filtered

	return moons


## Determines how many moons to generate for a planet based on mass category.
## Uses same categories and probabilities as system moon generation.
## @param earth_masses: Planet mass in Earth masses.
## @param rng: Random number generator.
## @return: Number of moons (0 or within min–max for the category).
func _determine_moon_count(earth_masses: float, rng: SeededRng) -> int:
	var moon_params: Array
	if earth_masses >= 50.0:
		moon_params = _MOON_COUNT_GAS_GIANT
	elif earth_masses >= 10.0:
		moon_params = _MOON_COUNT_ICE_GIANT
	elif earth_masses >= 2.0:
		moon_params = _MOON_COUNT_SUPER_EARTH
	elif earth_masses >= 0.3:
		moon_params = _MOON_COUNT_TERRESTRIAL
	elif earth_masses >= 0.01:
		moon_params = _MOON_COUNT_SUB_TERRESTRIAL
	else:
		moon_params = _MOON_COUNT_DWARF

	var has_moons_probability: float = moon_params[2]
	if rng.randf() > has_moons_probability:
		return 0

	var min_moons: int = moon_params[0]
	var max_moons: int = moon_params[1]
	if min_moons >= max_moons:
		return min_moons

	var raw: float = rng.randf()
	var biased: float = pow(raw, 0.7)
	return int(lerpf(float(min_moons), float(max_moons) + 0.99, biased))


## Converts a small positive integer to a Roman numeral string.
## @param n: The integer (1–10 supported; falls back to str(n)).
## @return: Roman numeral string.
func _to_roman(n: int) -> String:
	const NUMERALS: Array[String] = [
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"
	]
	if n >= 1 and n <= NUMERALS.size():
		return NUMERALS[n - 1]
	return str(n)


## Generates a standalone moon around a Jupiter-like parent.
## @param seed_value: Generation seed.
## @param rng: Seeded RNG.
## @param population_override: PopulationLikelihood.Override for moon population mode.
## @return: Generated moon body.
func _generate_moon(seed_value: int, rng: SeededRng, population_override: int = 0) -> CelestialBody:
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
	var enable_pop: bool = population_override != _population_likelihood.Override.NONE
	return MoonGenerator.generate(spec, context, rng, enable_pop, null, population_override)


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
## @param suppress_console: If true, do not call push_error (e.g. for tests asserting error display).
func set_error(message: String, suppress_console: bool = false) -> void:
	if status_label:
		status_label.text = "Error: " + message
		status_label.modulate = Color(1.0, 0.3, 0.3)
	if not suppress_console:
		push_error(message)


## Displays a body with no moons.
## Convenience wrapper for external callers that do not have moons.
## @param body: The celestial body to display.
func display_body(body: CelestialBody) -> void:
	display_body_with_moons(body, [])


## Displays a body together with an optional set of orbiting moons.
## Moons are rendered in their correct orbital positions and can be selected.
## @param body: The primary celestial body.
## @param moons: Moons to render (pass [] for none).
func display_body_with_moons(
	body: CelestialBody,
	moons: Array[CelestialBody]
) -> void:
	if not body:
		clear_display()
		set_error("Cannot display null body", true)
		return

	current_body = body

	_primary_display_scale = _calculate_display_scale(body)
	if body_renderer:
		body_renderer.render_body(body, _primary_display_scale)

	_adjust_lighting_for_body(body)

	if body.type == CelestialType.Type.STAR:
		_enable_star_glow(body)
	else:
		_disable_star_glow()

	_moon_system.set_primary_body(body, _primary_display_scale)
	if not moons.is_empty() and body.type == CelestialType.Type.PLANET:
		var tilt_deg: float = body.physical.axial_tilt_deg
		_moon_system.build_moon_display(moons, tilt_deg)
	else:
		_moon_system.clear()

	_fit_camera()
	_update_inspector()


## Displays an externally-provided body (e.g. from the system viewer).
## Shows the back button and disables generation controls for the session.
## @param body: The celestial body to display.
## @param moons: Associated moons from the system (may be empty).
func display_external_body(
	body: CelestialBody,
	moons: Array[CelestialBody] = []
) -> void:
	if not body:
		return
	_navigated_from_system = true
	_show_back_button()
	_set_generation_controls_enabled(false)
	display_body_with_moons(body, moons)
	var suffix: String = ""
	if not moons.is_empty():
		suffix = " with %d moon(s)" % moons.size()
	set_status("Viewing: %s%s (from system)" % [body.name, suffix])


## Calculates display scale for a body (no artificial clamps).
## Star: solar radii; planet/moon: Earth radii; asteroid: 1 unit = 1 km.
## @param body: The celestial body to scale.
## @return: Scale factor for the mesh.
func _calculate_display_scale(body: CelestialBody) -> float:
	var r: float = body.physical.radius_m
	match body.type:
		CelestialType.Type.STAR:
			return r / Units.SOLAR_RADIUS_METERS
		CelestialType.Type.PLANET, CelestialType.Type.MOON:
			return r / Units.EARTH_RADIUS_METERS
		CelestialType.Type.ASTEROID:
			return r / 1000.0
		_:
			return 1.0


## Fits the camera to frame the primary body and all its displayed moons.
func _fit_camera() -> void:
	if not camera:
		return
	var ctrl: CameraController = camera as CameraController
	if not ctrl:
		return

	# Set min_distance so the camera can never enter the body.
	# Use 1.2× the body's display radius for a small clearance buffer.
	var body_radius: float = _primary_display_scale
	ctrl.min_distance = maxf(body_radius * 1.2, 0.5)

	ctrl.set_target_position(Vector3.ZERO)
	ctrl.set_distance(_moon_system.get_framing_distance())
	ctrl.focus_on_target()


## Shifts camera and inspector focus to the given moon.
## @param moon: Moon to focus on (must be in current_moons).
func focus_on_moon(moon: CelestialBody) -> void:
	if not _moon_system.focus_on_moon(moon):
		return

	var moon_display_r: float = _moon_system.get_focused_moon_display_radius()

	var ctrl: CameraController = camera as CameraController
	if ctrl:
		ctrl.min_distance = maxf(moon_display_r * 1.2, 0.5)
		ctrl.set_target_position(_moon_system.get_focused_moon_position())
		ctrl.set_distance(moon_display_r * 4.0)

	if inspector_panel:
		inspector_panel.display_focused_moon(moon, current_body, _moon_system.get_moons())

	set_status("Focused: %s" % moon.name)


## Returns focus to the primary planet and resets the camera.
func focus_on_planet() -> void:
	_moon_system.focus_on_planet()

	var ctrl: CameraController = camera as CameraController
	if ctrl:
		var body_radius: float = _primary_display_scale
		ctrl.min_distance = maxf(body_radius * 1.2, 0.5)

		ctrl.set_target_position(Vector3.ZERO)
		ctrl.set_distance(_moon_system.get_framing_distance())
		ctrl.focus_on_target()

	_update_inspector()
	if current_body:
		set_status("Viewing: %s" % current_body.name)


## Handles focus change from moon system. Re-emits so existing connections to moon_focused still work.
## @param moon: The newly focused moon, or null for planet.
func _on_moon_system_focus_changed(moon: CelestialBody) -> void:
	moon_focused.emit(moon)


## Handles a moon selection coming from the inspector panel.
## @param moon: The selected moon, or null.
func _on_inspector_moon_selected(moon: CelestialBody) -> void:
	if moon == null:
		focus_on_planet()
	else:
		focus_on_moon(moon)


## Detects 3D left-clicks on moon meshes via ray-sphere intersection.
func _unhandled_input(event: InputEvent) -> void:
	if not _moon_system or not _moon_system.has_moons() or not camera:
		return
	if not event is InputEventMouseButton:
		return
	var mb: InputEventMouseButton = event as InputEventMouseButton
	if not mb.pressed or mb.button_index != MOUSE_BUTTON_LEFT:
		return

	var clicked_moon: CelestialBody = _moon_system.detect_moon_click(camera, mb.position)
	if clicked_moon != null:
		focus_on_moon(clicked_moon)
		get_viewport().set_input_as_handled()


## Updates the inspector to show the primary body and moon list.
func _update_inspector() -> void:
	if inspector_panel:
		inspector_panel.display_body_with_moons(current_body, _moon_system.get_moons())


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


## Clears the entire display (primary body + moons).
func clear_display() -> void:
	current_body = null
	if _moon_system:
		_moon_system.clear()
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
		for i: int in range(type_option.get_item_count()):
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
	_back_button.text = "← Back to System"
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
