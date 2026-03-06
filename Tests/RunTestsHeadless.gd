extends SceneTree

const CSHARP_TEST_SCENE_PATH := "res://Tests/TestSceneCSharp.tscn"
const SUPPORTED_GODOT_MAJOR := 4
const SUPPORTED_GODOT_MINOR := 6

var _csharp_harness = null

func _initialize() -> void:
	print("")
	print("StarGen Test Suite (Headless)")
	print("==============================")
	print("")
	print("Running tests...")
	print("Supported runtime: Godot .NET 4.6.x")
	print("Detected runtime: %s" % _get_runtime_label())

	if not _is_supported_runtime():
		push_error("Headless tests require Godot .NET 4.6.x. Detected %s." % _get_runtime_label())
		quit(1)
		return

	call_deferred("_start_csharp_harness")

func _start_csharp_harness() -> void:
	var scene := load(CSHARP_TEST_SCENE_PATH)
	if scene == null:
		push_error("Failed to load C# test harness scene: " + CSHARP_TEST_SCENE_PATH)
		quit(1)
		return

	var harness = scene.instantiate()
	if harness == null:
		push_error("Failed to instantiate C# test harness scene: " + CSHARP_TEST_SCENE_PATH)
		quit(1)
		return

	_csharp_harness = harness
	root.add_child(harness)
	if harness.has_signal("run_completed"):
		harness.connect("run_completed", Callable(self, "_on_csharp_headless_completed"))
	elif harness.has_signal("RunCompleted"):
		harness.connect("RunCompleted", Callable(self, "_on_csharp_headless_completed"))
	else:
		push_error("C# test harness is missing RunCompleted/run_completed signal")
		quit(1)
		return
	harness.start_headless()

func _on_csharp_headless_completed(exit_code: int) -> void:
	if _csharp_harness != null:
		_csharp_harness.queue_free()
		_csharp_harness = null

	for child in root.get_children():
		child.queue_free()

	await process_frame
	await process_frame
	await process_frame
	quit(exit_code)

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
