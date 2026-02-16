## Discovers and runs tests, collecting and reporting results.
class_name TestRunner
extends RefCounted


## Signal emitted when a test starts.
signal test_started(test_name: String)

## Signal emitted when a test finishes.
signal test_finished(result: TestResult)

## Signal emitted when all tests are complete.
signal all_tests_finished(results: Array[TestResult])


## All test results from the last run.
var _results: Array[TestResult] = []

## Total number of tests run.
var _total_count: int = 0

## Number of tests that passed.
var _pass_count: int = 0

## Number of tests that failed.
var _fail_count: int = 0

## Progress indicator buffer for dots.
var _progress_buffer: String = ""


## Runs all tests in the provided test case scripts.
## When scene_tree is provided (e.g. from TestScene), runs async and processes
## frames after before_each for scripts that need it (e.g. TestMainAppNavigation).
## @param test_scripts: Array of TestCase scripts (GDScript resources).
## @param scene_tree: Optional SceneTree; when set, run_all is async and processes frames for deferred setup.
## @return: Array of all TestResults.
func run_all(test_scripts: Array, scene_tree: SceneTree = null) -> Array[TestResult]:
	_results.clear()
	_total_count = 0
	_pass_count = 0
	_fail_count = 0
	_progress_buffer = ""
	
	if scene_tree != null:
		await _run_all_async(test_scripts, scene_tree)
	else:
		for script in test_scripts:
			_run_test_script(script)
	
	# Flush any remaining progress dots
	if _progress_buffer.length() > 0:
		print(_progress_buffer)
		_progress_buffer = ""
	
	all_tests_finished.emit(_results)
	return _results


## Async run path: processes frames after before_each for TestMainAppNavigation so deferred add_child runs.
func _run_all_async(test_scripts: Array, scene_tree: SceneTree) -> void:
	for script in test_scripts:
		await _run_test_script_async(script, scene_tree)


## Runs all test methods in a single test script.
## @param script: The TestCase script to run.
func _run_test_script(script: GDScript) -> void:
	var test_instance: TestCase = script.new() as TestCase
	if test_instance == null:
		push_error("Script does not extend TestCase: %s" % script.resource_path)
		return
	
	var test_methods: Array[String] = _get_test_methods(test_instance)
	
	if test_methods.is_empty():
		return
	
	test_instance.before_all()
	
	for method_name in test_methods:
		_run_single_test(test_instance, method_name, script.resource_path)
	
	test_instance.after_all()


## Async: runs one test script and awaits frame processing for scripts that use deferred add_child.
func _run_test_script_async(script: GDScript, scene_tree: SceneTree) -> void:
	var test_instance: TestCase = script.new() as TestCase
	if test_instance == null:
		push_error("Script does not extend TestCase: %s" % script.resource_path)
		return
	
	test_instance.runner_scene_tree = scene_tree
	
	var test_methods: Array[String] = _get_test_methods(test_instance)
	
	if test_methods.is_empty():
		return
	
	test_instance.before_all()
	
	for method_name in test_methods:
		await _run_single_test_async(test_instance, method_name, script.resource_path, scene_tree)
	
	test_instance.after_all()


## Gets all methods starting with "test_" from a test instance.
## @param instance: The TestCase instance to inspect.
## @return: Array of test method names.
func _get_test_methods(instance: TestCase) -> Array[String]:
	var methods: Array[String] = []
	
	for method_info in instance.get_method_list():
		var method_name: String = method_info["name"]
		if method_name.begins_with("test_"):
			methods.append(method_name)
	
	return methods


## Runs a single test method and records the result.
## @param instance: The TestCase instance.
## @param method_name: The name of the test method to run.
## @param script_path: The path to the script (for reporting).
func _run_single_test(instance: TestCase, method_name: String, script_path: String) -> void:
	var full_name: String = "%s::%s" % [script_path.get_file().get_basename(), method_name]
	
	test_started.emit(full_name)
	print("Running: %s" % full_name)
	
	instance._reset_failure_state()
	instance.before_each()
	
	var start_time: int = Time.get_ticks_msec()
	instance.call(method_name)
	var end_time: int = Time.get_ticks_msec()
	
	instance.after_each()
	
	_record_result(full_name, instance, start_time, end_time)


## Async: runs a single test and awaits frames after before_each for TestMainAppNavigation (deferred add_child).
func _run_single_test_async(instance: TestCase, method_name: String, script_path: String, scene_tree: SceneTree) -> void:
	var full_name: String = "%s::%s" % [script_path.get_file().get_basename(), method_name]
	
	test_started.emit(full_name)
	print("Running: %s" % full_name)
	
	instance._reset_failure_state()
	instance.before_each()
	
	# Let deferred add_child run so the viewer/app is in the tree before the test runs.
	if "TestMainAppNavigation" in script_path or "TestGalaxyViewerUI" in script_path or "TestGalaxyRandomization" in script_path or "TestWelcomeScreen" in script_path or "TestGalaxyStartup" in script_path:
		await scene_tree.process_frame
		await scene_tree.process_frame
		await scene_tree.process_frame
	
	var start_time: int = Time.get_ticks_msec()
	var callable: Callable = Callable(instance, method_name)
	await callable.call()
	var end_time: int = Time.get_ticks_msec()
	
	instance.after_each()
	
	_record_result(full_name, instance, start_time, end_time)


## Records a test result and updates counts/buffer.
func _record_result(full_name: String, instance: TestCase, start_time: int, end_time: int) -> void:
	var time_ms: float = float(end_time - start_time)
	var passed: bool = not instance.has_failed()
	var message: String = instance.get_failure_message()
	
	var result: TestResult = TestResult.new(full_name, passed, message, time_ms)
	_results.append(result)
	
	_total_count += 1
	if passed:
		_pass_count += 1
		# Add dot to progress buffer
		_progress_buffer += "."
		# Flush every 50 dots to avoid huge lines
		if _progress_buffer.length() >= 50:
			print(_progress_buffer)
			_progress_buffer = ""
	else:
		_fail_count += 1
		# Flush progress buffer first
		if _progress_buffer.length() > 0:
			print(_progress_buffer)
			_progress_buffer = ""
		# Print failure details immediately
		print("[FAIL] %s (%.1fms)" % [full_name, time_ms])
		if message:
			print("       -> %s" % message)
	
	test_finished.emit(result)


## Returns the total number of tests run.
## @return: Total test count.
func get_total_count() -> int:
	return _total_count


## Returns the number of tests that passed.
## @return: Pass count.
func get_pass_count() -> int:
	return _pass_count


## Returns the number of tests that failed.
## @return: Fail count.
func get_fail_count() -> int:
	return _fail_count


## Prints a summary of test results to the console.
func print_summary() -> void:
	print("")
	print("")
	print("=".repeat(60))
	print("TEST SUMMARY")
	print("=".repeat(60))
	
	# Only show failed tests in detail (passing tests already shown as dots)
	if _fail_count > 0:
		print("")
		print("FAILED TESTS:")
		print("-".repeat(60))
		for result in _results:
			if not result.passed:
				print("[FAIL] %s (%.1fms)" % [result.test_name, result.time_ms])
				if result.message:
					print("       -> %s" % result.message)
		print("")
	
	# Summary statistics
	print("-".repeat(60))
	print("Total: %d | Passed: %d | Failed: %d" % [_total_count, _pass_count, _fail_count])
	print("=".repeat(60))
	if _fail_count > 0:
		print("SOME TESTS FAILED")
	else:
		print("ALL TESTS PASSED")
	print("")
	print("(Report complete.)")
