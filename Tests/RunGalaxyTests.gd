## Minimal test runner scene script.
## Attach to a Node in a scene, run the scene, check the output console.
extends Node

const TestStableHashScript = preload("res://Tests/domain/galaxy/TestStableHash.gd")
const TestSpiralDensityModelScript = preload("res://Tests/domain/galaxy/TestSpiralDensityModel.gd")
const TestDensitySamplerScript = preload("res://Tests/domain/galaxy/TestDensitySampler.gd")


const TestGalaxyCoordinatesScript = preload("res://Tests/domain/galaxy/TestGalaxyCoordinates.gd")
const TestZoomStateMachineScript = preload("res://Tests/domain/galaxy/TestZoomStateMachine.gd")
const TestRaycastUtilsScript = preload("res://Tests/domain/galaxy/TestRaycastUtils.gd")
const TestSeedDeriverScript = preload("res://Tests/domain/galaxy/TestSeedDeriver.gd")
const TestQuadrantSelectorScript = preload("res://Tests/domain/galaxy/TestQuadrantSelector.gd")
const TestGridCursorScript = preload("res://Tests/domain/galaxy/TestGridCursor.gd")
const TestSubSectorGeneratorScript = preload("res://Tests/domain/galaxy/TestSubSectorGenerator.gd")
const TestStarPickerScript = preload("res://Tests/domain/galaxy/TestStarPicker.gd")
const TestSubSectorNeighborhoodScript = preload("res://Tests/domain/galaxy/TestSubSectorNeighborhood.gd")


func _ready() -> void:
	var suites: Array[TestCase] = [
		TestStableHashScript.new(),
		TestSpiralDensityModelScript.new(),
		TestDensitySamplerScript.new(),
		TestGalaxyCoordinatesScript.new(),
		TestZoomStateMachineScript.new(),
		TestRaycastUtilsScript.new(),
		TestSeedDeriverScript.new(),
		TestQuadrantSelectorScript.new(),
		TestGridCursorScript.new(),
		TestSubSectorGeneratorScript.new(),
		TestStarPickerScript.new(),
		TestSubSectorNeighborhoodScript.new(),
	]

	var total_pass: int = 0
	var total_fail: int = 0

	for suite in suites:
		var suite_name: String = suite.get_script().get_global_name()
		print("\n=== %s ===" % suite_name)
		suite.before_all()

		var methods: Array[String] = _get_test_methods(suite)
		for method_name in methods:
			suite._reset_failure_state()
			suite.before_each()

			var start: int = Time.get_ticks_usec()
			suite.call(method_name)
			var elapsed: float = float(Time.get_ticks_usec() - start) / 1000.0

			suite.after_each()

			if suite.has_failed():
				total_fail += 1
				print("  FAIL  %s (%.1f ms): %s" % [
					method_name, elapsed, suite.get_failure_message()
				])
			else:
				total_pass += 1
				print("  PASS  %s (%.1f ms)" % [method_name, elapsed])

		suite.after_all()

	print("\n--- Results: %d passed, %d failed ---" % [total_pass, total_fail])

	if total_fail > 0:
		printerr("TESTS FAILED")
	else:
		print("ALL TESTS PASSED")


## Collects all methods starting with "test_" on a test case.
## @param suite: The test case instance.
## @return: Sorted list of test method names.
func _get_test_methods(suite: TestCase) -> Array[String]:
	var methods: Array[String] = []
	for m in suite.get_method_list():
		var method_name: String = m["name"] as String
		if method_name.begins_with("test_"):
			methods.append(method_name)
	methods.sort()
	return methods
