## Root application controller that manages navigation between viewers.
## Switches between GalaxyViewer, SystemViewer, and ObjectViewer.
## Navigation hierarchy: Galaxy â†’ System â†’ Object (with back navigation)
class_name MainApp
extends Node

## Ensures GalaxySaveData/GalaxyPersistence are in scope before GalaxyViewer loads.
const _galaxy_viewer_deps: GDScript = preload("res://src/app/galaxy_viewer/GalaxyViewerDeps.gd")
const _galaxy_viewer_scene: PackedScene = preload("res://src/app/galaxy_viewer/GalaxyViewer.tscn")
const _object_viewer_scene: PackedScene = preload("res://src/app/viewer/ObjectViewer.tscn")
const _system_viewer_scene: PackedScene = preload("res://src/app/system_viewer/SystemViewer.tscn")
const _system_cache_script: GDScript = preload("res://src/domain/system/SystemCache.gd")
const _system_fixture_generator: GDScript = preload("res://src/domain/system/fixtures/SystemFixtureGenerator.gd")
const _solar_system_spec: GDScript = preload("res://src/domain/system/SolarSystemSpec.gd")

## Currently active viewer ("galaxy", "system", or "object").
var _active_viewer: String = ""

## The galaxy viewer instance.
var _galaxy_viewer: GalaxyViewer = null

## The system viewer instance.
var _system_viewer: SystemViewer = null

## The object viewer instance.
var _object_viewer: ObjectViewer = null

## Session cache for generated systems.
var _system_cache: SystemCache = null

## The galaxy seed (fixed for now, will be configurable later).
var _galaxy_seed: int = 42

## Currently displayed star seed (for back navigation context).
var _current_star_seed: int = 0

## World position of current star (for context).
var _current_star_position: Vector3 = Vector3.ZERO

## Container for viewer scenes.
@onready var viewer_container: Node = $ViewerContainer


func _ready() -> void:
	_system_cache = _system_cache_script.new()
	_create_galaxy_viewer()
	_show_galaxy_viewer()


## Creates the galaxy viewer instance.
func _create_galaxy_viewer() -> void:
	_galaxy_viewer = _galaxy_viewer_scene.instantiate() as GalaxyViewer
	_galaxy_viewer.name = "GalaxyViewer"
	_galaxy_viewer.galaxy_seed = _galaxy_seed

	# Connect navigation signal
	_galaxy_viewer.open_system_requested.connect(_on_open_system_requested)


## Creates the system viewer instance (lazy).
func _create_system_viewer() -> void:
	if _system_viewer != null:
		return

	_system_viewer = _system_viewer_scene.instantiate() as SystemViewer
	_system_viewer.name = "SystemViewer"

	# Connect navigation signals
	_system_viewer.open_body_in_viewer.connect(_on_open_in_object_viewer)
	_system_viewer.back_to_galaxy_requested.connect(_on_back_to_galaxy)


## Creates the object viewer instance (lazy).
func _create_object_viewer() -> void:
	if _object_viewer != null:
		return

	_object_viewer = _object_viewer_scene.instantiate() as ObjectViewer
	_object_viewer.name = "ObjectViewer"

	# Connect back navigation signal
	_object_viewer.back_to_system_requested.connect(_on_back_to_system)


## Shows the galaxy viewer and removes other viewers from tree.
func _show_galaxy_viewer() -> void:
	if _active_viewer == "galaxy":
		return

	_active_viewer = "galaxy"

	# Remove other viewers from tree
	if _system_viewer and _system_viewer.is_inside_tree():
		viewer_container.remove_child(_system_viewer)

	if _object_viewer and _object_viewer.is_inside_tree():
		viewer_container.remove_child(_object_viewer)

	# Add galaxy viewer to tree
	if _galaxy_viewer and not _galaxy_viewer.is_inside_tree():
		viewer_container.add_child(_galaxy_viewer)


## Shows the system viewer and removes other viewers from tree.
func _show_system_viewer() -> void:
	if _active_viewer == "system":
		return

	_active_viewer = "system"

	# Remove other viewers from tree
	if _galaxy_viewer and _galaxy_viewer.is_inside_tree():
		viewer_container.remove_child(_galaxy_viewer)

	if _object_viewer and _object_viewer.is_inside_tree():
		viewer_container.remove_child(_object_viewer)

	# Ensure system viewer exists
	_create_system_viewer()

	# Add system viewer to tree
	if _system_viewer and not _system_viewer.is_inside_tree():
		viewer_container.add_child(_system_viewer)


## Shows the object viewer and removes other viewers from tree.
func _show_object_viewer() -> void:
	if _active_viewer == "object":
		return

	_active_viewer = "object"

	# Remove other viewers from tree
	if _galaxy_viewer and _galaxy_viewer.is_inside_tree():
		viewer_container.remove_child(_galaxy_viewer)

	if _system_viewer and _system_viewer.is_inside_tree():
		viewer_container.remove_child(_system_viewer)

	# Ensure object viewer exists
	_create_object_viewer()

	# Add object viewer to tree
	if _object_viewer and not _object_viewer.is_inside_tree():
		viewer_container.add_child(_object_viewer)


## Handles request to open a star system from galaxy viewer.
## @param star_seed: The deterministic seed of the selected star.
## @param world_position: World position of the star (for context).
func _on_open_system_requested(star_seed: int, world_position: Vector3) -> void:
	if star_seed == 0:
		return

	_current_star_seed = star_seed
	_current_star_position = world_position

	# Save galaxy viewer state before transitioning
	if _galaxy_viewer:
		_galaxy_viewer.save_state()

	# Check cache first
	var system: SolarSystem = _system_cache.get_system(star_seed)

	if system == null:
		# Generate new system from star seed
		system = _generate_system_from_seed(star_seed)
		if system:
			_system_cache.put_system(star_seed, system)

	if system == null:
		push_error("Failed to generate system for star seed: %d" % star_seed)
		return

	_show_system_viewer()
	_system_viewer.display_system(system)
	_system_viewer.set_status("System from star seed %d" % star_seed)


## Generates a solar system from a star seed.
## @param star_seed: The deterministic seed.
## @return: Generated SolarSystem or null on failure.
func _generate_system_from_seed(star_seed: int) -> SolarSystem:
	# Use star seed to create a spec with deterministic properties
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = star_seed

	# Determine star count (weighted toward single stars)
	var star_roll: float = rng.randf()
	var star_count: int = 1
	if star_roll > 0.85:
		star_count = 3
	elif star_roll > 0.55:
		star_count = 2

	var spec: SolarSystemSpec = _solar_system_spec.new(star_seed, star_count, star_count)

	return _system_fixture_generator.generate_system(spec)


## Handles request to open a body in the object viewer.
## @param body: The celestial body to display.
func _on_open_in_object_viewer(body: CelestialBody) -> void:
	if body == null:
		return

	_show_object_viewer()
	_object_viewer.display_external_body(body)


## Handles request to go back to the system viewer from object viewer.
func _on_back_to_system() -> void:
	_show_system_viewer()


## Handles request to go back to the galaxy viewer from system viewer.
func _on_back_to_galaxy() -> void:
	_show_galaxy_viewer()

	# Restore galaxy viewer state
	if _galaxy_viewer and _galaxy_viewer.has_saved_state():
		_galaxy_viewer.restore_state()


## Returns the current galaxy seed.
## @return: Galaxy seed value.
func get_galaxy_seed() -> int:
	return _galaxy_seed


## Returns the system cache (for testing).
## @return: SystemCache instance.
func get_system_cache() -> SystemCache:
	return _system_cache


## Returns the currently active viewer name.
## @return: "galaxy", "system", or "object".
func get_active_viewer() -> String:
	return _active_viewer


## Returns the current star seed being viewed.
## @return: Star seed or 0 if not viewing a system.
func get_current_star_seed() -> int:
	return _current_star_seed


## Returns the galaxy viewer instance (for testing).
## @return: GalaxyViewer instance.
func get_galaxy_viewer() -> GalaxyViewer:
	return _galaxy_viewer


## Returns the system viewer instance (for testing).
## @return: SystemViewer instance or null.
func get_system_viewer() -> SystemViewer:
	return _system_viewer
