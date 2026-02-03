## Integration tests for the SystemViewer scene.
## Verifies scene loads, instantiates, and has correct node structure.
extends TestCase

const _system_viewer_scene: PackedScene = preload("res://src/app/system_viewer/SystemViewer.tscn")
const _system_viewer: GDScript = preload("res://src/app/system_viewer/SystemViewer.gd")
const _system_camera_controller: GDScript = preload("res://src/app/system_viewer/SystemCameraController.gd")
const _system_inspector_panel: GDScript = preload("res://src/app/system_viewer/SystemInspectorPanel.gd")


func get_test_name() -> String:
	return "TestSystemViewer"


## Verifies the scene resource loads successfully.
func test_scene_loads() -> void:
	var scene: PackedScene = load("res://src/app/system_viewer/SystemViewer.tscn") as PackedScene
	assert_not_null(scene, "SystemViewer scene should load")


## Verifies the scene instantiates as a Node3D.
func test_scene_instantiates() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()
	assert_not_null(viewer, "SystemViewer should instantiate")
	assert_true(viewer is Node3D, "Should be Node3D")
	viewer.free()


## Verifies the camera node exists and has the correct script.
func test_has_camera() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()
	var camera: Node = viewer.get_node_or_null("CameraRig/Camera3D")
	assert_not_null(camera, "Should have Camera3D under CameraRig")
	assert_true(camera is Camera3D, "Camera should be Camera3D type")
	assert_true(camera.get_script() != null, "Camera should have script attached")
	viewer.free()


## Verifies the 3D containers exist for bodies, orbits, and zones.
func test_has_3d_containers() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var bodies: Node = viewer.get_node_or_null("BodiesContainer")
	assert_not_null(bodies, "Should have BodiesContainer")
	assert_true(bodies is Node3D, "BodiesContainer should be Node3D")

	var orbits: Node = viewer.get_node_or_null("OrbitsContainer")
	assert_not_null(orbits, "Should have OrbitsContainer")
	assert_true(orbits is Node3D, "OrbitsContainer should be Node3D")

	var zones: Node = viewer.get_node_or_null("ZonesContainer")
	assert_not_null(zones, "Should have ZonesContainer")
	assert_true(zones is Node3D, "ZonesContainer should be Node3D")

	viewer.free()


## Verifies the UI structure exists with expected nodes.
func test_has_ui_structure() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var ui: Node = viewer.get_node_or_null("UI")
	assert_not_null(ui, "Should have UI node")
	assert_true(ui is Control, "UI should be Control")

	var top_bar: Node = viewer.get_node_or_null("UI/TopBar")
	assert_not_null(top_bar, "Should have TopBar")

	var status_label: Node = viewer.get_node_or_null("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel")
	assert_not_null(status_label, "Should have StatusLabel")

	var side_panel: Node = viewer.get_node_or_null("UI/SidePanel")
	assert_not_null(side_panel, "Should have SidePanel")

	viewer.free()


## Verifies the generation controls exist in the scene.
func test_has_generation_controls() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var base_path: String = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection"

	var star_count: Node = viewer.get_node_or_null(base_path + "/StarCountContainer/StarCountSpin")
	assert_not_null(star_count, "Should have StarCountSpin")
	assert_true(star_count is SpinBox, "StarCountSpin should be SpinBox")

	var seed_input: Node = viewer.get_node_or_null(base_path + "/SeedContainer/SeedInput")
	assert_not_null(seed_input, "Should have SeedInput")
	assert_true(seed_input is SpinBox, "SeedInput should be SpinBox")

	var generate_btn: Node = viewer.get_node_or_null(base_path + "/ButtonContainer/GenerateButton")
	assert_not_null(generate_btn, "Should have GenerateButton")
	assert_true(generate_btn is Button, "GenerateButton should be Button")

	var reroll_btn: Node = viewer.get_node_or_null(base_path + "/ButtonContainer/RerollButton")
	assert_not_null(reroll_btn, "Should have RerollButton")
	assert_true(reroll_btn is Button, "RerollButton should be Button")

	viewer.free()


## Verifies the view option controls exist.
func test_has_view_controls() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var view_path: String = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection"

	var orbits_check: Node = viewer.get_node_or_null(view_path + "/ShowOrbitsCheck")
	assert_not_null(orbits_check, "Should have ShowOrbitsCheck")
	assert_true(orbits_check is CheckBox, "ShowOrbitsCheck should be CheckBox")

	var zones_check: Node = viewer.get_node_or_null(view_path + "/ShowZonesCheck")
	assert_not_null(zones_check, "Should have ShowZonesCheck")
	assert_true(zones_check is CheckBox, "ShowZonesCheck should be CheckBox")

	viewer.free()


## Verifies the inspector panel exists with the correct script.
func test_has_inspector_panel() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var inspector: Node = viewer.get_node_or_null(
		"UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel"
	)
	assert_not_null(inspector, "Should have InspectorPanel")
	assert_true(inspector is VBoxContainer, "InspectorPanel should be VBoxContainer")
	assert_true(inspector.get_script() != null, "InspectorPanel should have script")

	viewer.free()


## Verifies the environment node exists.
func test_has_environment() -> void:
	var viewer: Node = _system_viewer_scene.instantiate()

	var world_env: Node = viewer.get_node_or_null("Environment/WorldEnvironment")
	assert_not_null(world_env, "Should have WorldEnvironment")
	assert_true(world_env is WorldEnvironment, "Should be WorldEnvironment type")

	viewer.free()


## Verifies SystemBodyNode scene loads and instantiates.
func test_body_node_scene_loads() -> void:
	var scene: PackedScene = load("res://src/app/system_viewer/SystemBodyNode.tscn") as PackedScene
	assert_not_null(scene, "SystemBodyNode scene should load")

	var node: Node = scene.instantiate()
	assert_not_null(node, "SystemBodyNode should instantiate")
	assert_true(node is Node3D, "Should be Node3D")
	node.free()
