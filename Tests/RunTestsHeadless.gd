## Headless test runner script.
## Run with: godot --headless --script res://Tests/RunTestsHeadless.gd
extends SceneTree


## Array of test scripts to run. Add new test scripts here.
var _test_scripts: Array[GDScript] = [
	preload("res://Tests/Unit/TestSeededRng.gd"),
	preload("res://Tests/Unit/TestVersions.gd"),
	preload("res://Tests/Unit/TestMathUtils.gd"),
	preload("res://Tests/Unit/TestUnits.gd"),
	preload("res://Tests/Unit/TestValidation.gd"),
]


func _init() -> void:
	print("")
	print("StarGen Test Suite (Headless)")
	print("==============================")
	print("")
	
	var runner: TestRunner = TestRunner.new()
	
	runner.run_all(_test_scripts)
	runner.print_summary()
	
	var exit_code: int = 0 if runner.get_fail_count() == 0 else 1
	quit(exit_code)
