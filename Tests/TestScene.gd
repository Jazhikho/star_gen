extends Node

const CSHARP_TEST_SCENE_PATH := "res://Tests/TestSceneCSharp.tscn"
const SUPPORTED_GODOT_MAJOR := 4
const SUPPORTED_GODOT_MINOR := 6

var _csharp_harness = null
var _overlay_root = null
var _status_label = null
var _current_test_label = null
var _counts_label = null
var _completed_count := 0
var _passed_count := 0
var _failed_count := 0

func _ready() -> void:
	_build_overlay()
	print("")
	print("StarGen Test Suite")
	print("==================")
	print("")
	print("Running tests...")
	print("Supported runtime: Godot .NET 4.6.x")
	print("Detected runtime: %s" % _get_runtime_label())

	if not _is_supported_runtime():
		push_error("Interactive tests require Godot .NET 4.6.x. Detected %s." % _get_runtime_label())
		_set_status_text("Unsupported Godot runtime: " + _get_runtime_label())
		_set_current_test_text("Use Godot .NET 4.6.x to run interactive tests.")
		_schedule_quit(1)
		return

	_set_status_text("Launching interactive C# harness")
	_set_current_test_text("Waiting for first test...")
	_update_counts_text()
	call_deferred("_run_csharp_harness")

func _run_csharp_harness() -> void:
	var scene := load(CSHARP_TEST_SCENE_PATH)
	if scene == null:
		push_error("Failed to load C# test harness scene: " + CSHARP_TEST_SCENE_PATH)
		_schedule_quit(1)
		return

	var harness = scene.instantiate()
	if harness == null:
		push_error("Failed to instantiate C# test harness scene: " + CSHARP_TEST_SCENE_PATH)
		_schedule_quit(1)
		return

	_csharp_harness = harness
	add_child(harness)

	if harness.has_signal("run_completed"):
		harness.connect("run_completed", Callable(self, "_on_csharp_run_completed"))
	elif harness.has_signal("RunCompleted"):
		harness.connect("RunCompleted", Callable(self, "_on_csharp_run_completed"))
	else:
		push_error("C# test harness is missing RunCompleted/run_completed signal")
		_set_status_text("Harness missing completion signal")
		_schedule_quit(1)
		return

	if harness.has_signal("test_started"):
		harness.connect("test_started", Callable(self, "_on_test_started"))
	elif harness.has_signal("TestStarted"):
		harness.connect("TestStarted", Callable(self, "_on_test_started"))

	if harness.has_signal("test_finished"):
		harness.connect("test_finished", Callable(self, "_on_test_finished"))
	elif harness.has_signal("TestFinished"):
		harness.connect("TestFinished", Callable(self, "_on_test_finished"))

	if harness.has_method("start_interactive"):
		_set_status_text("Running interactive suites")
		harness.call_deferred("start_interactive")
		return

	push_error("C# test harness is missing start_interactive()")
	_set_status_text("Harness missing start_interactive()")
	_schedule_quit(1)

func _schedule_quit(exit_code: int) -> void:
	var timer := get_tree().create_timer(2.0)
	timer.timeout.connect(func() -> void:
		get_tree().quit(exit_code)
	)

func _on_csharp_run_completed(exit_code: int) -> void:
	if exit_code == 0:
		_set_status_text("Run completed successfully")
	else:
		_set_status_text("Run completed with failures")

	if _csharp_harness != null:
		_csharp_harness.queue_free()
		_csharp_harness = null

	var timer := get_tree().create_timer(2.0)
	await timer.timeout
	get_tree().quit(exit_code)

func _on_test_started(test_name: String) -> void:
	_set_current_test_text(test_name)
	_set_status_text("Running tests")

func _on_test_finished(test_name: String, passed: bool, _time_ms: float, _message: String) -> void:
	_completed_count += 1
	if passed:
		_passed_count += 1
	else:
		_failed_count += 1
	_set_current_test_text(test_name)
	_update_counts_text()

func _build_overlay() -> void:
	_overlay_root = CanvasLayer.new()
	_overlay_root.layer = 50
	add_child(_overlay_root)

	var panel := PanelContainer.new()
	panel.name = "TestOverlay"
	panel.offset_left = 16.0
	panel.offset_top = 16.0
	panel.offset_right = 720.0
	panel.offset_bottom = 140.0
	_overlay_root.add_child(panel)

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 12)
	margin.add_theme_constant_override("margin_top", 10)
	margin.add_theme_constant_override("margin_right", 12)
	margin.add_theme_constant_override("margin_bottom", 10)
	panel.add_child(margin)

	var layout := VBoxContainer.new()
	layout.add_theme_constant_override("separation", 6)
	margin.add_child(layout)

	var title := Label.new()
	title.text = "Interactive Test Runner"
	title.add_theme_font_size_override("font_size", 18)
	layout.add_child(title)

	_status_label = Label.new()
	_status_label.text = "Starting..."
	_status_label.add_theme_font_size_override("font_size", 13)
	layout.add_child(_status_label)

	_counts_label = Label.new()
	_counts_label.text = "Completed: 0  Passed: 0  Failed: 0"
	_counts_label.add_theme_font_size_override("font_size", 13)
	layout.add_child(_counts_label)

	_current_test_label = Label.new()
	_current_test_label.text = "Waiting for first test..."
	_current_test_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_current_test_label.add_theme_font_size_override("font_size", 12)
	layout.add_child(_current_test_label)

func _set_status_text(text: String) -> void:
	if _status_label != null:
		_status_label.text = text

func _set_current_test_text(text: String) -> void:
	if _current_test_label != null:
		_current_test_label.text = "Current: " + text

func _update_counts_text() -> void:
	if _counts_label != null:
		_counts_label.text = "Completed: %d  Passed: %d  Failed: %d" % [_completed_count, _passed_count, _failed_count]

func _is_supported_runtime() -> bool:
	var version_info: Dictionary = Engine.get_version_info()
	return int(version_info.get("major", 0)) == SUPPORTED_GODOT_MAJOR and int(version_info.get("minor", 0)) == SUPPORTED_GODOT_MINOR

func _get_runtime_label() -> String:
	var version_info: Dictionary = Engine.get_version_info()
	var major := int(version_info.get("major", 0))
	var minor := int(version_info.get("minor", 0))
	var patch := int(version_info.get("patch", 0))
	var status := String(version_info.get("status", "unknown"))
	return "%d.%d.%d %s" % [major, minor, patch, status]
