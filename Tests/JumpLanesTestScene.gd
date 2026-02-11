## Test scene for running jump lanes tests with visual output.
extends Control


## Preload jump lanes domain so class_name types are registered before test scripts.
const _jump_lanes_deps = preload("res://Tests/JumpLanesDeps.gd")

## Test framework (TestResult is loaded via runner script).
const _test_runner_script: GDScript = preload("res://Tests/Framework/TestRunner.gd")

## Jump lane test scripts.
var _test_scripts: Array[GDScript] = [
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneSystem.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneConnection.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneRegion.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneResult.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneCalculator.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneClusterConnector.gd"),
]

## UI references.
@onready var status_label: Label = $VBoxContainer/Status
@onready var results_text: RichTextLabel = $VBoxContainer/Results


func _ready() -> void:
	_run_tests()


func _run_tests() -> void:
	results_text.clear()
	results_text.append_text("[b]Jump Lanes Test Suite[/b]\n")
	results_text.append_text("=".repeat(50) + "\n\n")

	var runner: TestRunner = _test_runner_script.new()
	runner.test_finished.connect(_on_test_finished)

	var _results: Array[TestResult] = await runner.run_all(_test_scripts, get_tree())

	_show_summary(runner)


func _on_test_finished(result: TestResult) -> void:
	if result.passed:
		results_text.append_text("[color=green]✓[/color] %s (%.1fms)\n" % [result.test_name, result.time_ms])
	else:
		results_text.append_text("[color=red]✗[/color] %s (%.1fms)\n" % [result.test_name, result.time_ms])
		results_text.append_text("  [color=red]%s[/color]\n" % result.message)


func _show_summary(runner: TestRunner) -> void:
	results_text.append_text("\n" + "=".repeat(50) + "\n")
	results_text.append_text("[b]Summary:[/b] ")

	var total: int = runner.get_total_count()
	var passed: int = runner.get_pass_count()
	var failed: int = runner.get_fail_count()

	if failed == 0:
		status_label.text = "All tests passed!"
		results_text.append_text("[color=green]%d/%d passed[/color]\n" % [passed, total])
	else:
		status_label.text = "%d test(s) failed" % failed
		results_text.append_text("[color=red]%d/%d passed, %d failed[/color]\n" % [passed, total, failed])
