## Integration tests for MainApp navigation between viewers.
extends TestCase

const _main_app_scene := preload("res://src/app/MainApp.tscn")
const _celestial_body := preload("res://src/domain/celestial/CelestialBody.gd")
const _celestial_type := preload("res://src/domain/celestial/CelestialType.gd")
const _physical_props := preload("res://src/domain/celestial/components/PhysicalProps.gd")
const _units := preload("res://src/domain/math/Units.gd")


func get_test_name() -> String:
	return "TestMainApp"


## Helper to create a test body.
func _create_test_body() -> CelestialBody:
	var body: CelestialBody = CelestialBody.new()
	body.id = "test_planet_01"
	body.name = "Test Planet"
	body.type = CelestialType.Type.PLANET
	body.physical = PhysicalProps.new()
	body.physical.mass_kg = Units.EARTH_MASS_KG
	body.physical.radius_m = Units.EARTH_RADIUS_METERS
	return body


## Tests that MainApp scene loads.
func test_scene_loads() -> void:
	var scene: PackedScene = load("res://src/app/MainApp.tscn") as PackedScene
	assert_not_null(scene, "MainApp scene should load")


## Tests that MainApp instantiates.
func test_instantiates() -> void:
	var app: Node = _main_app_scene.instantiate()
	assert_not_null(app, "MainApp should instantiate")
	assert_true(app.get_script() != null, "Should have script attached")
	app.free()


## Tests that ViewerContainer exists.
func test_has_viewer_container() -> void:
	var app: Node = _main_app_scene.instantiate()
	var container: Node = app.get_node_or_null("ViewerContainer")
	assert_not_null(container, "Should have ViewerContainer")
	app.free()


## Tests ObjectViewer has back_to_system_requested signal.
func test_object_viewer_has_back_signal() -> void:
	var viewer_scene: PackedScene = load("res://src/app/viewer/ObjectViewer.tscn") as PackedScene
	var viewer: Node = viewer_scene.instantiate()
	assert_true(viewer.has_signal("back_to_system_requested"), 
		"ObjectViewer should have back_to_system_requested signal")
	viewer.free()


## Tests ObjectViewer display_external_body method exists.
func test_object_viewer_has_display_external() -> void:
	var viewer_scene: PackedScene = load("res://src/app/viewer/ObjectViewer.tscn") as PackedScene
	var viewer: Node = viewer_scene.instantiate()
	assert_true(viewer.has_method("display_external_body"),
		"ObjectViewer should have display_external_body method")
	viewer.free()


## Tests SystemViewer has open_body_in_viewer signal.
func test_system_viewer_has_open_signal() -> void:
	var viewer_scene: PackedScene = load("res://src/app/system_viewer/SystemViewer.tscn") as PackedScene
	var viewer: Node = viewer_scene.instantiate()
	assert_true(viewer.has_signal("open_body_in_viewer"),
		"SystemViewer should have open_body_in_viewer signal")
	viewer.free()
