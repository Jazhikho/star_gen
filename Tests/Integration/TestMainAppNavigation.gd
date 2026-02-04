## Integration tests for MainApp navigation between viewers.
extends TestCase

const _main_app_scene: PackedScene = preload("res://src/app/MainApp.tscn")
const _system_cache: GDScript = preload("res://src/domain/system/SystemCache.gd")
const _celestial_body: GDScript = preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type: GDScript = preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props: GDScript = preload("res://src/domain/celestial/components/PhysicalProps.gd")


var _main_app: MainApp = null

## Wrapper node so we add MainApp to it instead of root; avoids "Parent node is busy setting up children".
var _wrapper: Node = null

## When true, tree root was unavailable (e.g. headless --script); skip assertions that need _ready().
var _skip_nav_tests: bool = false


func get_test_name() -> String:
	return "TestMainAppNavigation"


func before_each() -> void:
	_main_app = _main_app_scene.instantiate() as MainApp
	_wrapper = null
	_skip_nav_tests = false
	var tree: SceneTree = Engine.get_main_loop() as SceneTree
	if tree == null or tree.root == null:
		_skip_nav_tests = true
		return
	_wrapper = Node.new()
	# When runner provides a scene tree (TestScene), use deferred add to avoid "Parent node is busy".
	# When not (headless), add synchronously so the node is in the tree before the test runs.
	var runner_tree: SceneTree = get("runner_scene_tree") as SceneTree
	if runner_tree != null:
		tree.root.add_child.call_deferred(_wrapper)
	else:
		tree.root.add_child(_wrapper)
	_wrapper.add_child(_main_app)
	_main_app._ready()
	_main_app.start_galaxy_with_defaults()


func after_each() -> void:
	if _main_app != null and is_instance_valid(_main_app):
		var p: Node = _main_app.get_parent()
		if p != null:
			p.remove_child(_main_app)
		_main_app.free()
	_main_app = null
	if _wrapper != null and is_instance_valid(_wrapper):
		var tree: SceneTree = Engine.get_main_loop() as SceneTree
		if tree != null and tree.root != null and _wrapper.get_parent() != null:
			tree.root.remove_child(_wrapper)
		_wrapper.free()
	_wrapper = null


func test_starts_with_galaxy_viewer() -> void:
	if _skip_nav_tests:
		return
	assert_equal(_main_app.get_active_viewer(), "galaxy",
		"Should start with galaxy viewer active")


func test_galaxy_seed_is_set() -> void:
	if _skip_nav_tests:
		return
	assert_greater_than(_main_app.get_galaxy_seed(), 0,
		"Galaxy seed should be set to a positive random value")


func test_system_cache_exists() -> void:
	if _skip_nav_tests:
		return
	var cache: RefCounted = _main_app.get_system_cache()
	assert_not_null(cache, "System cache should exist")
	assert_equal(cache.get_cache_size(), 0, "Cache should start empty")


func test_open_system_transitions_to_system_viewer() -> void:
	if _skip_nav_tests:
		return
	_main_app._on_open_system_requested(12345, Vector3.ZERO)

	assert_equal(_main_app.get_active_viewer(), "system",
		"Should transition to system viewer")


func test_open_system_caches_generated_system() -> void:
	if _skip_nav_tests:
		return
	var star_seed: int = 12345
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)

	var cache: RefCounted = _main_app.get_system_cache()
	assert_true(cache.has_system(star_seed),
		"Generated system should be cached")


func test_open_same_system_uses_cache() -> void:
	if _skip_nav_tests:
		return
	var star_seed: int = 12345

	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	var cache: RefCounted = _main_app.get_system_cache()
	var first_system: SolarSystem = cache.get_system(star_seed)

	_main_app._on_back_to_galaxy()
	_main_app._on_open_system_requested(star_seed, Vector3.ZERO)
	var second_system: SolarSystem = cache.get_system(star_seed)

	assert_true(first_system == second_system,
		"Second open should use cached system instance")


func test_back_to_galaxy_from_system() -> void:
	if _skip_nav_tests:
		return
	_main_app._on_open_system_requested(12345, Vector3.ZERO)
	_main_app._on_back_to_galaxy()

	assert_equal(_main_app.get_active_viewer(), "galaxy",
		"Should return to galaxy viewer")


func test_system_to_object_navigation() -> void:
	if _skip_nav_tests:
		return
	_main_app._on_open_system_requested(12345, Vector3.ZERO)

	var body: CelestialBody = CelestialBody.new()
	body.id = "test_body"
	body.name = "Test Body"
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = 5.972e24
	body.physical.radius_m = 6.371e6

	_main_app._on_open_in_object_viewer(body)

	assert_equal(_main_app.get_active_viewer(), "object",
		"Should transition to object viewer")


func test_back_to_system_from_object() -> void:
	if _skip_nav_tests:
		return
	_main_app._on_open_system_requested(12345, Vector3.ZERO)

	var body: CelestialBody = CelestialBody.new()
	body.id = "test_body"
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()

	_main_app._on_open_in_object_viewer(body)
	_main_app._on_back_to_system()

	assert_equal(_main_app.get_active_viewer(), "system",
		"Should return to system viewer")


func test_full_navigation_cycle() -> void:
	if _skip_nav_tests:
		return
	assert_equal(_main_app.get_active_viewer(), "galaxy", "Start at galaxy")

	_main_app._on_open_system_requested(12345, Vector3.ZERO)
	assert_equal(_main_app.get_active_viewer(), "system", "At system")

	var body: CelestialBody = CelestialBody.new()
	body.id = "test"
	body.type = CelestialType.Type.STAR
	body.physical = PhysicalProps.new()
	_main_app._on_open_in_object_viewer(body)
	assert_equal(_main_app.get_active_viewer(), "object", "At object")

	_main_app._on_back_to_system()
	assert_equal(_main_app.get_active_viewer(), "system", "Back at system")

	_main_app._on_back_to_galaxy()
	assert_equal(_main_app.get_active_viewer(), "galaxy", "Back at galaxy")


func test_generated_system_is_deterministic() -> void:
	if _skip_nav_tests:
		return
	var star_seed: int = 99999

	var system1: SolarSystem = _main_app._generate_system_from_seed(star_seed)
	var system2: SolarSystem = _main_app._generate_system_from_seed(star_seed)

	assert_not_null(system1, "First system should generate")
	assert_not_null(system2, "Second system should generate")
	assert_equal(system1.get_stars().size(), system2.get_stars().size(),
		"Same seed should produce same star count")
	assert_equal(system1.get_planets().size(), system2.get_planets().size(),
		"Same seed should produce same planet count")


func test_zero_star_seed_ignored() -> void:
	if _skip_nav_tests:
		return
	_main_app._on_open_system_requested(0, Vector3.ZERO)

	assert_equal(_main_app.get_active_viewer(), "galaxy",
		"Zero star seed should not trigger navigation")
