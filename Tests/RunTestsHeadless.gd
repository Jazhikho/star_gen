## Headless test runner script.
## Run with: godot --headless --script res://Tests/RunTestsHeadless.gd
extends SceneTree

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
var _test_scripts: Array[GDScript] = _test_registry.get_headless_test_scripts()
## The active C# harness instance for .NET runs.
var _csharp_harness: Node = null


func _init() -> void:
	print("")
	print("StarGen Test Suite (Headless)")
	print("==============================")
	print("")
	print("Running tests...")

	if ClassDB.class_exists("CSharpScript"):
		call_deferred("_run_csharp_harness")
		return

	var runner: TestRunner = TestRunner.new()

	runner.run_all(_test_scripts)

	runner.print_summary()

	var exit_code: int = 0 if runner.get_fail_count() == 0 else 1
	quit(exit_code)


## Runs the parallel C# harness when the .NET runtime is available.
func _run_csharp_harness() -> void:
	var scene: PackedScene = ResourceLoader.load(
		_csharp_test_scene_path,
		"PackedScene",
		ResourceLoader.CACHE_MODE_IGNORE
	) as PackedScene
	if scene == null:
		push_error("Failed to load C# test harness scene: %s" % _csharp_test_scene_path)
		quit(1)
		return

	_csharp_harness = scene.instantiate()
	if _csharp_harness == null:
		push_error("Failed to instantiate C# test harness scene: %s" % _csharp_test_scene_path)
		quit(1)
		return

	root.add_child(_csharp_harness)
	if _csharp_harness.has_signal("RunCompleted"):
		_csharp_harness.connect("RunCompleted", Callable(self, "_on_csharp_headless_completed"))
	else:
		_cleanup_csharp_harness()
		push_error("C# test harness does not expose a completion signal")
		quit(1)
		return
	_csharp_harness.call("start_run", _test_scripts, self)


## Removes the C# harness from the tree and frees it.
func _cleanup_csharp_harness() -> void:
	if _csharp_harness == null:
		return
	var parent: Node = _csharp_harness.get_parent()
	if parent != null:
		parent.remove_child(_csharp_harness)
	_csharp_harness.queue_free()
	_csharp_harness = null


## Quits the headless process after the C# harness completes.
func _on_csharp_headless_completed(exit_code: int) -> void:
	_cleanup_csharp_harness()
	await process_frame
	quit(exit_code)
