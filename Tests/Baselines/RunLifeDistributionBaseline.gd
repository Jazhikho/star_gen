extends SceneTree

const BASELINE_SCENE_PATH := "res://Tests/Baselines/LifeDistributionBaselineRunner.tscn"

var _runner = null

func _initialize() -> void:
	print("")
	print("StarGen Life Distribution Baseline")
	print("=================================")
	print("")
	call_deferred("_start_runner")

func _start_runner() -> void:
	var scene := load(BASELINE_SCENE_PATH)
	if scene == null:
		push_error("Failed to load baseline scene: " + BASELINE_SCENE_PATH)
		quit(1)
		return

	var runner = scene.instantiate()
	if runner == null:
		push_error("Failed to instantiate baseline scene: " + BASELINE_SCENE_PATH)
		quit(1)
		return

	_runner = runner
	root.add_child(runner)
	if runner.has_signal("run_completed"):
		runner.connect("run_completed", Callable(self, "_on_run_completed"))
	elif runner.has_signal("RunCompleted"):
		runner.connect("RunCompleted", Callable(self, "_on_run_completed"))
	else:
		push_error("Baseline runner is missing RunCompleted/run_completed signal")
		quit(1)
		return

	runner.start_headless()

func _on_run_completed(exit_code: int) -> void:
	if _runner != null:
		_runner.queue_free()
		_runner = null

	for child in root.get_children():
		child.queue_free()

	await process_frame
	await process_frame
	await process_frame
	quit(exit_code)
