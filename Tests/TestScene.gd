## Test scene that runs all tests and displays results in the console.
## Run this scene directly or use --headless mode.
extends Node

## Preload Phase 1 domain/service scripts so class_name types are registered before test scripts compile.
const _phase1_deps = preload("res://Tests/Phase1Deps.gd")
## Preload population domain scripts so class_name types are registered before population test scripts.
const _population_deps = preload("res://Tests/PopulationDeps.gd")
## Preload jump lanes domain scripts so class_name types are registered before jump lanes test scripts.
const _jump_lanes_deps = preload("res://Tests/JumpLanesDeps.gd")
## Shared registry of all test scripts.
const _test_registry: GDScript = preload("res://Tests/TestRegistry.gd")
## Parallel C# harness scene for .NET-capable runs.
const _csharp_test_scene_path: String = "res://Tests/TestSceneCSharp.tscn"

## Array of test scripts to run. Add new test scripts in TestRegistry.gd.
var _test_scripts: Array[GDScript] = _test_registry.get_scene_test_scripts()

## The test runner instance.
var _runner: TestRunner
## The active C# harness instance for .NET runs.
var _csharp_harness: Node = null


func _ready() -> void:
	print("")
	print("StarGen Test Suite")
	print("==================")
	print("")
	print("Running tests...")
	_run_csharp_harness()


func _on_test_finished(_result: TestResult) -> void:
	# Failures are already printed by TestRunner during execution
	pass


## Runs the parallel C# harness when the .NET runtime is available.
func _run_csharp_harness() -> void:
	var scene: PackedScene = ResourceLoader.load(
		_csharp_test_scene_path,
		"PackedScene",
		ResourceLoader.CACHE_MODE_IGNORE
	) as PackedScene
	if scene == null:
		push_error("Failed to load C# test harness scene: %s" % _csharp_test_scene_path)
		await get_tree().create_timer(5.0).timeout
		get_tree().quit(1)
		return

	_csharp_harness = scene.instantiate()
	if _csharp_harness == null:
		push_error("Failed to instantiate C# test harness scene: %s" % _csharp_test_scene_path)
		await get_tree().create_timer(5.0).timeout
		get_tree().quit(1)
		return

	add_child(_csharp_harness)
	if _csharp_harness.has_signal("RunCompleted"):
		_csharp_harness.connect("RunCompleted", Callable(self, "_on_csharp_run_completed"))
	else:
		_cleanup_csharp_harness()
		push_error("C# test harness does not expose a completion signal")
		await get_tree().create_timer(5.0).timeout
		get_tree().quit(1)
		return
	_csharp_harness.call("start_run", _test_scripts, get_tree())


## Removes the C# harness from the tree and frees it.
func _cleanup_csharp_harness() -> void:
	if _csharp_harness == null:
		return
	var parent: Node = _csharp_harness.get_parent()
	if parent != null:
		parent.remove_child(_csharp_harness)
	_csharp_harness.queue_free()
	_csharp_harness = null


## Quits the scene after the C# harness completes.
func _on_csharp_run_completed(exit_code: int) -> void:
	_cleanup_csharp_harness()
	await get_tree().process_frame
	await get_tree().create_timer(5.0).timeout
	get_tree().quit(exit_code)
