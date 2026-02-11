## Headless test runner for jump lanes tests only.
## Run with: godot --headless --script res://Tests/JumpLanesTestRunner.gd
extends SceneTree


## Preload jump lanes domain so class_name types are registered before test scripts.
const _jump_lanes_deps = preload("res://Tests/JumpLanesDeps.gd")

## Test framework.
const _test_runner_script: GDScript = preload("res://Tests/Framework/TestRunner.gd")

## Jump lane test scripts (calculator test added at runtime so JumpLaneCalculator is registered first).
var _test_scripts: Array[GDScript] = [
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneSystem.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneConnection.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneRegion.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneResult.gd"),
]


func _init() -> void:
	_test_scripts.append(preload("res://Tests/Unit/JumpLanes/TestJumpLaneCalculator.gd"))
	_test_scripts.append(preload("res://Tests/Unit/JumpLanes/TestJumpLaneClusterConnector.gd"))

	print("")
	print("=".repeat(60))
	print("JUMP LANES TEST RUNNER")
	print("=".repeat(60))
	print("")

	var runner: TestRunner = _test_runner_script.new()
	runner.run_all(_test_scripts)
	runner.print_summary()

	var exit_code: int = 0 if runner.get_fail_count() == 0 else 1
	quit(exit_code)
