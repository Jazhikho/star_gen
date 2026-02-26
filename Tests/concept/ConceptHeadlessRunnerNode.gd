## Child node used by RunConceptTestsHeadless to run tests in _ready() so we can await run_all().
## Test scripts are passed via meta "test_scripts".
extends Node


## Runs tests with await, then prints summary and quits the tree.
func _ready() -> void:
	var test_scripts: Array = get_meta("test_scripts")
	var runner: TestRunner = TestRunner.new()
	var _results: Array[TestResult] = await runner.run_all(test_scripts, null)
	runner.print_summary()

	if runner.get_fail_count() > 0:
		get_tree().quit(1)
	else:
		get_tree().quit(0)
