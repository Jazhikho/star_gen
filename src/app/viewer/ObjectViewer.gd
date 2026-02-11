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

## Moons displayed around the primary body (may be empty).
var current_moons: Array[CelestialBody] = []

## The moon that currently has camera/inspector focus (null = primary body).
var _focused_moon: CelestialBody = null

## BodyRenderer instances for each moon (index matches current_moons).
var _moon_renderers: Array[BodyRenderer] = []

## Rig under body_renderer with rotation_degrees.z = planet axial tilt so moons sit in equatorial plane.
var _moon_system_rig: Node3D = null

## Container that holds all moon BodyRenderer instances.
var _moon_bodies_node: Node3D = null

## Container that holds all moon orbit line meshes.
var _moon_orbits_node: Node3D = null

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

## Visual period for a moon at ORBIT_REFERENCE_SMA_M (seconds per full orbit). ~2 minutes for inner moons.
const ORBIT_BASE_PERIOD_S: float = 120.0
## Reference semi-major axis for Kepler period scaling (metres); ~Luna distance.
const ORBIT_REFERENCE_SMA_M: float = 3.844e8

## Accumulated visual time driving moon orbital animation (seconds).
var _orbit_visual_time: float = 0.0

## Cached display scale for the primary body (updated in display_body_with_moons).
var _primary_display_scale: float = 1.0


func _ready() -> void:
	_setup_viewport()
	_setup_camera()
	_setup_generation_ui()
	_setup_file_dialogs()
	_setup_moon_containers()
	_connect_signals()

	set_status("Viewer initialized")
	is_ready = true
	_on_generate_pressed()


## Drives body rotation, moon orbital motion, and camera follow each frame.
func _process(delta: float) -> void:
	if animate_rotation and body_renderer and current_body:
		body_renderer.rotate_body(delta, rotation_speed)

	if not current_moons.is_empty():
		_orbit_visual_time += delta
		_update_moon_orbital_positions()

	if _focused_moon != null:
		_update_camera_follow()


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


## Creates the persistent Node3D containers used to hold moon renderers and
## orbit lines. Moon system rig is under body_renderer and gets the planet's
## axial tilt so orbits align with the equator without rotating with the planet.
func _setup_moon_containers() -> void:
	_moon_system_rig = Node3D.new()
	_moon_system_rig.name = "MoonSystemRig"
	body_renderer.add_child(_moon_system_rig)

	_moon_bodies_node = Node3D.new()
	_moon_bodies_node.name = "MoonBodies"
	_moon_system_rig.add_child(_moon_bodies_node)

	_moon_orbits_node = Node3D.new()
	_moon_orbits_node.name = "MoonOrbits"
	_moon_system_rig.add_child(_moon_orbits_node)


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


## Generates a celestial object of the given type.
## For planets, also generates a small set of illustrative moons.
## @param object_type: The type of object to generate.
## @param seed_value: Deterministic generation seed.
func generate_object(object_type: ObjectType, seed_value: int) -> void:
	var rng: SeededRng = SeededRng.new(seed_value)
	var body: CelestialBody = null
	var moons: Array[CelestialBody] = []

	set_status("Generating %s with seed %d…" % [_get_type_name(object_type), seed_value])

	match object_type:
		ObjectType.STAR:
			body = _generate_star(seed_value, rng)
		ObjectType.PLANET:
			body = _generate_planet(seed_value, rng)
			if body:
				moons = _generate_moons_for_planet(body, seed_value, rng)
		ObjectType.MOON:
			body = _generate_moon(seed_value, rng)
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
## @return: Generated planet body.
func _generate_planet(seed_value: int, rng: SeededRng) -> CelestialBody:
	var spec: PlanetSpec = PlanetSpec.random(seed_value)
	var context: ParentContext = ParentContext.sun_like()
	return PlanetGenerator.generate(spec, context, rng, true)


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
## @return: Array of generated moon bodies, may be empty.
func _generate_moons_for_planet(
	planet: CelestialBody,
	base_seed: int,
	rng: SeededRng
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
		var moon: CelestialBody = MoonGenerator.generate(spec, context, moon_rng, false)
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
## @return: Generated moon body.
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
		set_error("Cannot display null body")
		return

	current_body = body
	current_moons = moons
	_focused_moon = null
	_orbit_visual_time = 0.0

	_primary_display_scale = _calculate_display_scale(body)
	if body_renderer:
		body_renderer.render_body(body, _primary_display_scale)

	_adjust_lighting_for_body(body)

	if body.type == CelestialType.Type.STAR:
		_enable_star_glow(body)
	else:
		_disable_star_glow()

	_clear_moon_display()
	if not moons.is_empty() and body.type == CelestialType.Type.PLANET:
		# Align moon system with planet equator (same tilt as BodyRenderer body_mesh).
		_moon_system_rig.rotation_degrees.z = body.physical.axial_tilt_deg
		_build_moon_display()

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
	var suffix: String
	if moons.is_empty():
		suffix = ""
	else:
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


## Returns the scale factor that converts physical meters to display units
## for the moon system around the current primary body.
## @return: Display units per meter.
func _get_moon_system_scale() -> float:
	if not current_body:
		return 1.0
	var r: float = current_body.physical.radius_m
	if r <= 0.0:
		return 1.0
	return _primary_display_scale / r


## Solves Kepler's equation for eccentric anomaly using Newton–Raphson.
## @param mean_anomaly: Mean anomaly in radians.
## @param eccentricity: Orbital eccentricity [0, 1).
## @return: Eccentric anomaly in radians.
func _solve_kepler(mean_anomaly: float, eccentricity: float) -> float:
	var ea: float = mean_anomaly
	for _i: int in range(5):
		ea = ea - (ea - eccentricity * sin(ea) - mean_anomaly) \
			/ (1.0 - eccentricity * cos(ea))
	return ea


## Returns the display-space position for a moon relative to the planet origin.
## Uses a Y-up coordinate system matching Godot's convention.
## @param moon: The moon body (must have orbital properties).
## @return: 3D display-space position; Vector3.ZERO if no orbital data.
func get_moon_display_position(moon: CelestialBody) -> Vector3:
	if not moon.has_orbital():
		return Vector3.ZERO

	var ms: float = _get_moon_system_scale()
	var a: float = moon.orbital.semi_major_axis_m * ms
	var e: float = clampf(moon.orbital.eccentricity, 0.0, 0.99)
	var inc: float = deg_to_rad(moon.orbital.inclination_deg)
	var lan: float = deg_to_rad(moon.orbital.longitude_of_ascending_node_deg)
	var aop: float = deg_to_rad(moon.orbital.argument_of_periapsis_deg)
	var ma: float = deg_to_rad(moon.orbital.mean_anomaly_deg)

	var ea: float = _solve_kepler(ma, e)

	var ta: float = 2.0 * atan2(
		sqrt(1.0 + e) * sin(ea / 2.0),
		sqrt(1.0 - e) * cos(ea / 2.0)
	)
	var r: float = a * (1.0 - e * cos(ea))
	var px: float = r * cos(ta)
	var py: float = r * sin(ta)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
		+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
	var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
		+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
	var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

	return Vector3(x, y, z)


## Creates and places all moon BodyRenderers and their orbit line meshes.
func _build_moon_display() -> void:
	var moon_scale: float = _get_moon_system_scale()

	for i: int in range(current_moons.size()):
		var moon: CelestialBody = current_moons[i]

		var renderer: BodyRenderer = _body_renderer_scene.instantiate() as BodyRenderer
		if not renderer:
			continue
		renderer.name = "MoonRenderer_%d" % i
		var moon_display_radius: float = moon.physical.radius_m * moon_scale
		var live_ma: float = _compute_live_mean_anomaly(moon)
		renderer.position = _get_moon_position_at_ma(moon, live_ma)
		_moon_bodies_node.add_child(renderer)
		renderer.render_body(moon, moon_display_radius)
		_moon_renderers.append(renderer)

		var orbit_line: MeshInstance3D = _create_moon_orbit_line(moon, moon_scale)
		if orbit_line:
			_moon_orbits_node.add_child(orbit_line)


## Recomputes and applies each moon renderer's position for the current
## _orbit_visual_time. Called every frame when moons are displayed.
func _update_moon_orbital_positions() -> void:
	for i: int in range(current_moons.size()):
		if i >= _moon_renderers.size():
			break
		var moon: CelestialBody = current_moons[i]
		var renderer: BodyRenderer = _moon_renderers[i]
		if not moon.has_orbital() or not is_instance_valid(renderer):
			continue
		var live_ma: float = _compute_live_mean_anomaly(moon)
		renderer.position = _get_moon_position_at_ma(moon, live_ma)


## If a moon is focused, updates the camera target to follow it each frame (world space).
func _update_camera_follow() -> void:
	if _focused_moon == null or not current_moons.has(_focused_moon):
		return
	var idx: int = current_moons.find(_focused_moon)
	if idx < 0 or idx >= _moon_renderers.size():
		return
	var ctrl: CameraController = camera as CameraController
	if not ctrl:
		return
	var renderer: BodyRenderer = _moon_renderers[idx]
	if not is_instance_valid(renderer):
		return
	ctrl.set_target_position(renderer.global_position)


## Returns the live mean anomaly (radians) for a moon at the current
## _orbit_visual_time, scaled by Kepler's third law so outer moons orbit more slowly.
func _compute_live_mean_anomaly(moon: CelestialBody) -> float:
	var sma: float = moon.orbital.semi_major_axis_m
	var period_scale: float = pow(sma / ORBIT_REFERENCE_SMA_M, 1.5)
	var visual_period: float = ORBIT_BASE_PERIOD_S * period_scale
	if visual_period < 0.001:
		visual_period = 0.001
	var initial_ma: float = deg_to_rad(moon.orbital.mean_anomaly_deg)
	return initial_ma + (TAU / visual_period) * _orbit_visual_time


## Computes display-space position for a moon at an explicit mean anomaly.
## Does not modify the body's stored mean_anomaly_deg.
func _get_moon_position_at_ma(moon: CelestialBody, mean_anomaly_rad: float) -> Vector3:
	if not moon.has_orbital():
		return Vector3.ZERO

	var ms: float = _get_moon_system_scale()
	var a: float = moon.orbital.semi_major_axis_m * ms
	var e: float = clampf(moon.orbital.eccentricity, 0.0, 0.99)
	var inc: float = deg_to_rad(moon.orbital.inclination_deg)
	var lan: float = deg_to_rad(moon.orbital.longitude_of_ascending_node_deg)
	var aop: float = deg_to_rad(moon.orbital.argument_of_periapsis_deg)

	var ea: float = _solve_kepler(mean_anomaly_rad, e)
	var ta: float = 2.0 * atan2(
		sqrt(1.0 + e) * sin(ea / 2.0),
		sqrt(1.0 - e) * cos(ea / 2.0)
	)
	var r: float = a * (1.0 - e * cos(ea))
	var px: float = r * cos(ta)
	var py: float = r * sin(ta)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
		+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
	var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
		+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
	var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

	return Vector3(x, y, z)


## Creates a closed elliptical orbit line for a moon.
## @param moon: The moon body (must have orbital properties).
## @param moon_scale: Display units per meter.
## @return: A MeshInstance3D with the line strip, or null if no orbital data.
func _create_moon_orbit_line(
	moon: CelestialBody,
	moon_scale: float
) -> MeshInstance3D:
	if not moon.has_orbital():
		return null

	var a: float = moon.orbital.semi_major_axis_m * moon_scale
	var e: float = clampf(moon.orbital.eccentricity, 0.0, 0.99)
	var b: float = a * sqrt(1.0 - e * e)
	var inc: float = deg_to_rad(moon.orbital.inclination_deg)
	var lan: float = deg_to_rad(moon.orbital.longitude_of_ascending_node_deg)
	var aop: float = deg_to_rad(moon.orbital.argument_of_periapsis_deg)

	var c_lan: float = cos(lan)
	var s_lan: float = sin(lan)
	var c_aop: float = cos(aop)
	var s_aop: float = sin(aop)
	var c_inc: float = cos(inc)
	var s_inc: float = sin(inc)

	var f: float = a * e

	var orbit_mesh: ImmediateMesh = ImmediateMesh.new()
	orbit_mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)

	const SEGMENTS: int = 128
	for i: int in range(SEGMENTS + 1):
		var angle: float = (float(i) / float(SEGMENTS)) * TAU
		var px: float = a * cos(angle) - f
		var py: float = b * sin(angle)

		var x: float = (c_lan * c_aop - s_lan * s_aop * c_inc) * px \
			+ (-c_lan * s_aop - s_lan * c_aop * c_inc) * py
		var z: float = (s_lan * c_aop + c_lan * s_aop * c_inc) * px \
			+ (-s_lan * s_aop + c_lan * c_aop * c_inc) * py
		var y: float = (s_aop * s_inc) * px + (c_aop * s_inc) * py

		orbit_mesh.surface_add_vertex(Vector3(x, y, z))

	orbit_mesh.surface_end()

	var instance: MeshInstance3D = MeshInstance3D.new()
	instance.mesh = orbit_mesh
	instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = Color(0.45, 0.65, 0.85, 0.55)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	instance.material_override = mat

	return instance


## Removes all moon renderers, orbit lines, and clears tracking state.
func _clear_moon_display() -> void:
	for child: Node in _moon_bodies_node.get_children():
		child.queue_free()
	for child: Node in _moon_orbits_node.get_children():
		child.queue_free()
	_moon_renderers.clear()
	_focused_moon = null
	_orbit_visual_time = 0.0


## Fits the camera to frame the primary body and all its displayed moons.
func _fit_camera() -> void:
	if not camera:
		return
	var ctrl: CameraController = camera as CameraController
	if ctrl:
		ctrl.set_target_position(Vector3.ZERO)
		ctrl.set_distance(_calculate_framing_distance())
		ctrl.focus_on_target()


## Returns the camera distance needed to frame the full moon system.
## @return: Camera distance in display units.
func _calculate_framing_distance() -> float:
	if not current_body:
		return 10.0
	if current_moons.is_empty():
		return _primary_display_scale * 4.0

	var moon_scale: float = _get_moon_system_scale()
	var farthest: float = 0.0
	for moon: CelestialBody in current_moons:
		if moon.has_orbital():
			var apoapsis: float = moon.orbital.semi_major_axis_m \
				* (1.0 + moon.orbital.eccentricity)
			farthest = maxf(farthest, apoapsis * moon_scale)

	if farthest <= 0.0:
		return _primary_display_scale * 4.0

	return farthest * 1.5


## Shifts camera and inspector focus to the given moon.
## Emits moon_focused. Call with null to return focus to the planet.
## @param moon: Moon to focus on (must be in current_moons).
func focus_on_moon(moon: CelestialBody) -> void:
	if not current_moons.has(moon):
		return

	_focused_moon = moon
	var idx: int = current_moons.find(moon)
	var moon_display_r: float = moon.physical.radius_m * _get_moon_system_scale()

	var ctrl: CameraController = camera as CameraController
	if ctrl:
		if idx >= 0 and idx < _moon_renderers.size() and is_instance_valid(_moon_renderers[idx]):
			ctrl.set_target_position(_moon_renderers[idx].global_position)
		ctrl.set_distance(moon_display_r * 4.0)

	if inspector_panel:
		inspector_panel.display_focused_moon(moon, current_body, current_moons)

	set_status("Focused: %s" % moon.name)
	moon_focused.emit(moon)


## Returns focus to the primary planet and resets the camera.
func focus_on_planet() -> void:
	_focused_moon = null

	var ctrl: CameraController = camera as CameraController
	if ctrl:
		ctrl.set_target_position(Vector3.ZERO)
		ctrl.set_distance(_calculate_framing_distance())
		ctrl.focus_on_target()

	_update_inspector()
	if current_body:
		set_status("Viewing: %s" % current_body.name)
	moon_focused.emit(null)


## Handles a moon selection coming from the inspector panel.
## @param moon: The selected moon, or null.
func _on_inspector_moon_selected(moon: CelestialBody) -> void:
	if moon == null:
		focus_on_planet()
	else:
		focus_on_moon(moon)


## Detects 3D left-clicks on moon meshes via ray-sphere intersection.
func _unhandled_input(event: InputEvent) -> void:
	if current_moons.is_empty() or not camera:
		return
	if not event is InputEventMouseButton:
		return
	var mb: InputEventMouseButton = event as InputEventMouseButton
	if not mb.pressed or mb.button_index != MOUSE_BUTTON_LEFT:
		return

	var ray_origin: Vector3 = camera.project_ray_origin(mb.position)
	var ray_dir: Vector3 = camera.project_ray_normal(mb.position)

	var best_moon: CelestialBody = null
	var best_t: float = INF

	var moon_scale: float = _get_moon_system_scale()
	for i: int in range(current_moons.size()):
		if i >= _moon_renderers.size():
			break
		var moon: CelestialBody = current_moons[i]
		var renderer: BodyRenderer = _moon_renderers[i]
		if not is_instance_valid(renderer):
			continue
		var centre: Vector3 = renderer.global_position
		var radius: float = moon.physical.radius_m * moon_scale
		var oc: Vector3 = ray_origin - centre
		var b: float = oc.dot(ray_dir)
		var c: float = oc.dot(oc) - radius * radius
		var disc: float = b * b - c

		if disc >= 0.0:
			var t: float = -b - sqrt(disc)
			if t > 0.0 and t < best_t:
				best_t = t
				best_moon = moon

	if best_moon != null:
		focus_on_moon(best_moon)
		get_viewport().set_input_as_handled()


## Updates the inspector to show the primary body and moon list.
func _update_inspector() -> void:
	if inspector_panel:
		inspector_panel.display_body_with_moons(current_body, current_moons)


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
	current_moons.clear()
	_clear_moon_display()
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
