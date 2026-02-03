## Integration tests for Galaxy → System → Galaxy transitions.
extends TestCase

const _main_app_scene: PackedScene = preload("res://src/app/MainApp.tscn")

var _main_app: MainApp = null
var _tree: SceneTree = null


func get_test_name() -> String:
	return "TestGalaxySystemTransition"


func before_all() -> void:
	_tree = Engine.get_main_loop() as SceneTree


func after_each() -> void:
	if _main_app and is_instance_valid(_main_app):
		if _main_app.is_inside_tree():
			_main_app.get_parent().remove_child(_main_app)
		_main_app.queue_free()
		_main_app = null

	if _tree:
		await _tree.process_frame
		await _tree.process_frame


## Helper to create and add MainApp to scene tree safely.
func _setup_main_app() -> void:
	_main_app = _main_app_scene.instantiate() as MainApp
	_tree.root.add_child.call_deferred(_main_app)

	await _tree.process_frame
	await _tree.process_frame
	await _tree.process_frame


func _wait_frames(count: int) -> void:
	if _tree:
		for i in range(count):
			await _tree.process_frame


func test_galaxy_viewer_starts_at_subsector() -> void:
	await _setup_main_app()

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	assert_not_null(galaxy_viewer, "Should have galaxy viewer")

	assert_equal(galaxy_viewer.get_zoom_level(), GalaxyCoordinates.ZoomLevel.SUBSECTOR,
		"Galaxy viewer should start at subsector (home) level")


func test_open_system_saves_galaxy_state() -> void:
	await _setup_main_app()

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()

	# Galaxy viewer should not have saved state initially
	assert_false(galaxy_viewer.has_saved_state(),
		"Should not have saved state initially")

	# Open a system
	_main_app._on_open_system_requested(12345, Vector3(8000, 20, 0))
	await _wait_frames(5)

	# Now galaxy viewer should have saved state
	assert_true(galaxy_viewer.has_saved_state(),
		"Should have saved state after opening system")


func test_back_to_galaxy_restores_state() -> void:
	await _setup_main_app()

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var initial_zoom: int = galaxy_viewer.get_zoom_level()

	# Open a system
	_main_app._on_open_system_requested(12345, Vector3(8000, 20, 0))
	await _wait_frames(5)

	assert_equal(_main_app.get_active_viewer(), "system",
		"Should be at system viewer")

	# Go back to galaxy
	_main_app._on_back_to_galaxy()
	await _wait_frames(5)

	assert_equal(_main_app.get_active_viewer(), "galaxy",
		"Should be back at galaxy viewer")

	assert_equal(galaxy_viewer.get_zoom_level(), initial_zoom,
		"Should restore zoom level")


func test_system_is_cached_after_generation() -> void:
	await _setup_main_app()

	var star_seed: int = 99999

	# Open system
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	await _wait_frames(5)

	# Check cache
	var cache: SystemCache = _main_app.get_system_cache()
	assert_true(cache.has_system(star_seed),
		"System should be cached after generation")


func test_returning_to_same_system_uses_cache() -> void:
	await _setup_main_app()

	var star_seed: int = 77777

	# Open system first time
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	await _wait_frames(5)

	var cache: SystemCache = _main_app.get_system_cache()
	var first_system: SolarSystem = cache.get_system(star_seed)
	assert_not_null(first_system, "Should have cached system")

	# Go back to galaxy
	_main_app._on_back_to_galaxy()
	await _wait_frames(5)

	# Open same system again
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	await _wait_frames(5)

	var second_system: SolarSystem = cache.get_system(star_seed)

	# Should be exact same instance
	assert_true(first_system == second_system,
		"Should reuse cached system instance")


func test_different_stars_have_different_systems() -> void:
	await _setup_main_app()

	var seed_a: int = 11111
	var seed_b: int = 22222

	# Open first system
	_main_app._on_open_system_requested(seed_a, Vector3.ZERO)
	await _wait_frames(5)

	# Go back
	_main_app._on_back_to_galaxy()
	await _wait_frames(3)

	# Open second system
	_main_app._on_open_system_requested(seed_b, Vector3.ZERO)
	await _wait_frames(5)

	var cache: SystemCache = _main_app.get_system_cache()
	var system_a: SolarSystem = cache.get_system(seed_a)
	var system_b: SolarSystem = cache.get_system(seed_b)

	assert_not_null(system_a, "First system should be cached")
	assert_not_null(system_b, "Second system should be cached")
	assert_true(system_a != system_b, "Different seeds should produce different systems")


func test_current_star_seed_tracked() -> void:
	await _setup_main_app()

	var star_seed: int = 55555

	# Initially no star seed
	assert_equal(_main_app.get_current_star_seed(), 0,
		"Should have no star seed initially")

	# Open system
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	await _wait_frames(5)

	assert_equal(_main_app.get_current_star_seed(), star_seed,
		"Should track current star seed")


func test_full_round_trip_galaxy_system_object_system_galaxy() -> void:
	await _setup_main_app()

	# Start at galaxy
	assert_equal(_main_app.get_active_viewer(), "galaxy", "Start at galaxy")

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()
	var initial_zoom: int = galaxy_viewer.get_zoom_level()

	# Galaxy → System
	_main_app._on_open_system_requested(12345, Vector3(8000, 20, 0))
	await _wait_frames(5)
	assert_equal(_main_app.get_active_viewer(), "system", "At system")

	# System → Object
	var body: CelestialBody = CelestialBody.new()
	body.id = "test_body"
	body.type = CelestialType.Type.STAR
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 1.989e30
	body.physical.radius_m = 6.96e8

	_main_app._on_open_in_object_viewer(body)
	await _wait_frames(5)
	assert_equal(_main_app.get_active_viewer(), "object", "At object")

	# Object → System
	_main_app._on_back_to_system()
	await _wait_frames(5)
	assert_equal(_main_app.get_active_viewer(), "system", "Back at system")

	# System → Galaxy
	_main_app._on_back_to_galaxy()
	await _wait_frames(5)
	assert_equal(_main_app.get_active_viewer(), "galaxy", "Back at galaxy")

	# Verify state restored
	assert_equal(galaxy_viewer.get_zoom_level(), initial_zoom,
		"Zoom level should be restored")


func test_zoom_out_then_open_system_then_return() -> void:
	await _setup_main_app()

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()

	# Zoom out to sector level
	var key_event: InputEventKey = InputEventKey.new()
	key_event.keycode = KEY_BRACKETLEFT
	key_event.pressed = true
	galaxy_viewer._handle_key_input(key_event)
	await _wait_frames(3)

	var zoomed_out_level: int = galaxy_viewer.get_zoom_level()
	assert_equal(zoomed_out_level, GalaxyCoordinates.ZoomLevel.SECTOR,
		"Should be at sector level after zoom out")

	# Go to system (simulating selection at sector level isn't possible,
	# so we directly call the handler)
	_main_app._on_open_system_requested(33333, Vector3.ZERO)
	await _wait_frames(5)

	# Return to galaxy
	_main_app._on_back_to_galaxy()
	await _wait_frames(5)

	# Should restore to sector level
	assert_equal(galaxy_viewer.get_zoom_level(), zoomed_out_level,
		"Should restore to sector zoom level")


func test_clear_saved_state() -> void:
	await _setup_main_app()

	var galaxy_viewer: GalaxyViewer = _main_app.get_galaxy_viewer()

	# Open system to create saved state
	_main_app._on_open_system_requested(12345, Vector3.ZERO)
	await _wait_frames(5)

	assert_true(galaxy_viewer.has_saved_state(), "Should have saved state")

	# Clear it
	galaxy_viewer.clear_saved_state()

	assert_false(galaxy_viewer.has_saved_state(), "Should not have saved state after clear")
