## Root script for the asteroid belt viewer concept scene.
## Wires camera, renderer, UI, generator, and test major-body creation together.
extends Node3D


## Metres per AU, used to build test major asteroid inputs.
const AU_M: float = 1.496e11

var _pivot: Node3D = null
var _camera: Camera3D = null
var _camera_controller: BeltViewerCameraControllerConcept = null
var _renderer: BeltViewerRendererConcept = null
var _ui: BeltViewerUIConcept = null
var _generator: AsteroidBeltGeneratorConcept = null
var _star_mesh: MeshInstance3D = null


## Sets up all subsystems and generates the initial belt.
func _ready() -> void:
	_setup_environment()
	_setup_star()
	_setup_camera()
	_setup_renderer()
	_setup_ui()
	_generator = AsteroidBeltGeneratorConcept.new()
	_generate_default()


## Forwards input to the camera controller.
func _input(event: InputEvent) -> void:
	_camera_controller.handle_input(event)


## Generates the default belt so the scene opens with content.
func _generate_default() -> void:
	var spec: AsteroidBeltSpecConcept = AsteroidBeltSpecConcept.new()
	spec.asteroid_count = 1000
	_run_generation(spec, 12345, 5, true, false)


## Runs generation, injects test major asteroids, and renders.
## @param spec: Belt spec (major_asteroid_inputs will be populated here).
## @param seed_val: RNG seed.
## @param major_count: Number of test major bodies to create.
## @param show_labels: Whether to show labels on major bodies.
## @param show_gap_rings: Whether to draw orange rings at gap boundaries.
func _run_generation(
	spec: AsteroidBeltSpecConcept,
	seed_val: int,
	major_count: int,
	show_labels: bool,
	show_gap_rings: bool
) -> void:
	var rng: SeededRng = SeededRng.new(seed_val)

	# Build test major asteroid inputs using a forked RNG so background is unaffected
	var major_rng: SeededRng = rng.fork()
	spec.major_asteroid_inputs = _create_test_major_inputs(spec, major_count, major_rng)

	var start: int = Time.get_ticks_msec()
	var belt: AsteroidBeltDataConcept = _generator.generate_belt(spec, rng)
	var elapsed: float = float(Time.get_ticks_msec() - start)

	_renderer.render_belt(belt, spec, show_labels, show_gap_rings)
	_ui.update_status(belt, elapsed)


## Creates plausible test major asteroid inputs spread across the belt.
## @param spec: The belt spec for boundary reference.
## @param count: Number of major bodies to create.
## @param rng: RNG for randomising orbital elements.
## @return: Array of MajorAsteroidInputConcept.
func _create_test_major_inputs(
	spec: AsteroidBeltSpecConcept,
	count: int,
	rng: SeededRng
) -> Array[MajorAsteroidInputConcept]:
	var inputs: Array[MajorAsteroidInputConcept] = []

	## Asteroid type weights: 50% C, 35% S, 15% M (index 0,1,2).
	var type_options: Array = [0, 1, 2]
	var type_weights: Array[float] = [50.0, 35.0, 15.0]

	for i in range(count):
		var input: MajorAsteroidInputConcept = MajorAsteroidInputConcept.new()
		input.body_id = "Major-%d" % (i + 1)

		# Spread evenly across belt with jitter
		var base_frac: float = 0.15 + 0.70 * (float(i) / maxf(float(count - 1), 1.0))
		var jitter: float = rng.randf_range(-0.05, 0.05)
		var frac: float = clampf(base_frac + jitter, 0.05, 0.95)
		var distance_au: float = lerpf(spec.inner_radius_au, spec.outer_radius_au, frac)
		input.semi_major_axis_m = distance_au * AU_M

		input.eccentricity = rng.randf() * 0.15
		input.inclination_deg = rng.randf() * 12.0
		input.longitude_ascending_node_deg = rng.randf_range(0.0, 360.0)
		input.argument_periapsis_deg = rng.randf_range(0.0, 360.0)
		input.mean_anomaly_deg = rng.randf_range(0.0, 360.0)

		# Largest first, sizes tapering down
		var size_t: float = float(i) / maxf(float(count - 1), 1.0)
		input.body_radius_km = lerpf(500.0, 110.0, size_t)

		input.asteroid_type = rng.weighted_choice(type_options, type_weights) as int

		inputs.append(input)

	return inputs


## Called when UI requests a new generation.
func _on_regenerate_requested(spec: AsteroidBeltSpecConcept, seed_val: int) -> void:
	_run_generation(
		spec, seed_val,
		_ui.get_major_count(),
		_ui.get_show_labels(),
		_ui.get_show_gap_rings()
	)


## Called when user presses Reset Camera.
func _on_reset_view_requested() -> void:
	_camera_controller.reset_view()


# ---------- Setup helpers ----------

func _setup_environment() -> void:
	var env: WorldEnvironment = WorldEnvironment.new()
	var environment: Environment = Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color(0.02, 0.02, 0.04)
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.15, 0.15, 0.2)
	environment.ambient_light_energy = 0.4
	env.environment = environment
	add_child(env)

	var light: DirectionalLight3D = DirectionalLight3D.new()
	light.rotation_degrees = Vector3(-35.0, 45.0, 0.0)
	light.light_energy = 1.2
	light.light_color = Color(1.0, 0.95, 0.85)
	add_child(light)


func _setup_star() -> void:
	var sphere: SphereMesh = SphereMesh.new()
	sphere.radius = 0.12
	sphere.height = 0.24
	var mat: StandardMaterial3D = StandardMaterial3D.new()
	mat.albedo_color = Color(1.0, 0.9, 0.5)
	mat.emission_enabled = true
	mat.emission = Color(1.0, 0.85, 0.3)
	mat.emission_energy_multiplier = 3.0
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	_star_mesh = MeshInstance3D.new()
	_star_mesh.mesh = sphere
	_star_mesh.material_override = mat
	add_child(_star_mesh)


func _setup_camera() -> void:
	_pivot = Node3D.new()
	_pivot.name = "CameraPivot"
	add_child(_pivot)
	_camera = Camera3D.new()
	_camera.name = "Camera3D"
	_camera.far = 200.0
	_pivot.add_child(_camera)
	_camera_controller = BeltViewerCameraControllerConcept.new()
	add_child(_camera_controller)
	_camera_controller.setup(_pivot, _camera)


func _setup_renderer() -> void:
	_renderer = BeltViewerRendererConcept.new()
	_renderer.name = "BeltRenderer"
	add_child(_renderer)
	_renderer.setup()


func _setup_ui() -> void:
	var canvas: CanvasLayer = CanvasLayer.new()
	add_child(canvas)
	_ui = BeltViewerUIConcept.new()
	_ui.set_anchors_preset(Control.PRESET_TOP_LEFT)
	_ui.setup(canvas)
	_ui.regenerate_requested.connect(_on_regenerate_requested)
	_ui.reset_view_requested.connect(_on_reset_view_requested)
