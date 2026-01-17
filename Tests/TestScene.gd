## Test scene that runs all tests and displays results in the console.
## Run this scene directly or use --headless mode.
extends Node


## Array of test scripts to run. Add new test scripts here.
var _test_scripts: Array[GDScript] = [
	preload("res://Tests/Unit/TestSeededRng.gd"),
	preload("res://Tests/Unit/TestVersions.gd"),
	preload("res://Tests/Unit/TestMathUtils.gd"),
	preload("res://Tests/Unit/TestUnits.gd"),
	preload("res://Tests/Unit/TestValidation.gd"),
]

## The test runner instance.
var _runner: TestRunner


func _ready() -> void:
	print("")
	print("StarGen Test Suite")
	print("==================")
	print("")
	
	_runner = TestRunner.new()
	_runner.test_started.connect(_on_test_started)
	_runner.test_finished.connect(_on_test_finished)
	
	_runner.run_all(_test_scripts)
	_runner.print_summary()
	
	var exit_code: int = 0 if _runner.get_fail_count() == 0 else 1
	
	# Give a moment for output to flush, then exit
	await get_tree().create_timer(0.1).timeout
	get_tree().quit(exit_code)


func _on_test_started(test_name: String) -> void:
	print("Running: %s" % test_name)


func _on_test_finished(result: TestResult) -> void:
	if not result.passed:
		push_error("FAILED: %s - %s" % [result.test_name, result.message])
