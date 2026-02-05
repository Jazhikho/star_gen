## Prototype scene for visualizing jump lane calculations.
extends Node3D


## Region type options for the dropdown.
enum RegionType {SIMPLE, RANDOM, CLUSTERED}

## Preload domain so class_name types are registered when scene runs.
const _jump_lanes_deps = preload("res://Tests/JumpLanesDeps.gd")

## UI references.
@onready var region_option: OptionButton = $UI/Panel/VBox/RegionOption
@onready var seed_input: SpinBox = $UI/Panel/VBox/SeedContainer/SeedInput
@onready var generate_button: Button = $UI/Panel/VBox/GenerateButton
@onready var stats_label: Label = $UI/Panel/VBox/StatsLabel
@onready var legend_label: Label = $UI/Panel/VBox/LegendLabel

## 3D components.
@onready var renderer: JumpLaneRenderer = $JumpLaneRenderer
@onready var camera_pivot: Node3D = $CameraPivot
@onready var camera: Camera3D = $CameraPivot/Camera3D

## Calculator instance.
var _calculator: JumpLaneCalculator

## Current region and result.
var _current_region: JumpLaneRegion
var _current_result: JumpLaneResult

## Camera control state.
var _camera_distance: float = 40.0
var _camera_rotation: Vector2 = Vector2(-0.5, 0.0)
var _is_dragging: bool = false
var _last_mouse_pos: Vector2 = Vector2.ZERO


func _ready() -> void:
	_calculator = JumpLaneCalculator.new()
	_setup_ui()
	_setup_legend()
	_update_camera()
	_generate_and_display()


func _setup_ui() -> void:
	region_option.add_item("Simple (Hand-crafted)", RegionType.SIMPLE)
	region_option.add_item("Random", RegionType.RANDOM)
	region_option.add_item("Clustered", RegionType.CLUSTERED)
	region_option.selected = 0

	seed_input.value = 12345
	seed_input.min_value = 0
	seed_input.max_value = 999999

	generate_button.pressed.connect(_on_generate_pressed)


func _setup_legend() -> void:
	legend_label.text = "Legend:\n"
	legend_label.text += "● Blue: Populated system\n"
	legend_label.text += "● Cyan: Bridge system\n"
	legend_label.text += "● Red: Orphan (no connections)\n"
	legend_label.text += "● Gray: Unpopulated\n"
	legend_label.text += "— Green: Direct (≤5 pc)\n"
	legend_label.text += "— Yellow: Bridged\n"
	legend_label.text += "— Orange: Direct (7 pc)"


func _on_generate_pressed() -> void:
	_generate_and_display()


func _generate_and_display() -> void:
	var region_type: int = region_option.get_selected_id()
	var seed_value: int = int(seed_input.value)

	_current_region = _create_region(region_type, seed_value)
	_current_result = _calculator.calculate(_current_region)

	renderer.render(_current_region, _current_result)
	_update_stats()


func _create_region(region_type: int, seed_value: int) -> JumpLaneRegion:
	match region_type:
		RegionType.SIMPLE:
			return MockRegionGenerator.create_simple_region(seed_value)
		RegionType.RANDOM:
			return MockRegionGenerator.create_random_region(seed_value)
		RegionType.CLUSTERED:
			return MockRegionGenerator.create_clustered_region(seed_value)
	return MockRegionGenerator.create_simple_region(seed_value)


func _update_stats() -> void:
	if _current_result == null:
		stats_label.text = "No data"
		return

	var counts: Dictionary = _current_result.get_connection_counts()
	stats_label.text = "Systems: %d\n" % _current_region.get_system_count()
	stats_label.text += "Populated: %d\n" % _current_region.get_populated_count()
	stats_label.text += "Connections: %d\n" % _current_result.get_total_connections()
	stats_label.text += "  Green: %d\n" % counts[JumpLaneConnection.ConnectionType.GREEN]
	stats_label.text += "  Yellow: %d\n" % counts[JumpLaneConnection.ConnectionType.YELLOW]
	stats_label.text += "  Orange: %d\n" % counts[JumpLaneConnection.ConnectionType.ORANGE]
	stats_label.text += "Orphans: %d" % _current_result.get_total_orphans()


func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		_handle_mouse_button(event as InputEventMouseButton)
	elif event is InputEventMouseMotion:
		_handle_mouse_motion(event as InputEventMouseMotion)


func _handle_mouse_button(event: InputEventMouseButton) -> void:
	if event.button_index == MOUSE_BUTTON_RIGHT:
		_is_dragging = event.pressed
		_last_mouse_pos = event.position
	elif event.button_index == MOUSE_BUTTON_WHEEL_UP:
		_camera_distance = maxf(_camera_distance - 3.0, 10.0)
		_update_camera()
	elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
		_camera_distance = minf(_camera_distance + 3.0, 100.0)
		_update_camera()


func _handle_mouse_motion(event: InputEventMouseMotion) -> void:
	if _is_dragging:
		var delta: Vector2 = event.position - _last_mouse_pos
		_last_mouse_pos = event.position
		_camera_rotation.x -= delta.y * 0.01
		_camera_rotation.y -= delta.x * 0.01
		_camera_rotation.x = clampf(_camera_rotation.x, -PI / 2 + 0.1, PI / 2 - 0.1)
		_update_camera()


func _update_camera() -> void:
	camera_pivot.rotation = Vector3(_camera_rotation.x, _camera_rotation.y, 0)
	camera.position = Vector3(0, 0, _camera_distance)
