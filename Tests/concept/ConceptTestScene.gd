## Scene-based runner for asteroid belt concept tests.
extends Node


## Preload concept domain and RNG so class_name types are registered.
const _concept_deps: GDScript = preload("res://Tests/concept/ConceptDeps.gd")

## Test scripts to execute. Add new concept test scripts here.
var _test_scripts: Array = [
	preload("res://Tests/concept/TestAsteroidBeltGeneratorConcept.gd"),
	preload("res://Tests/concept/TestOrbitalMathConcept.gd"),
]


## Runs all concept tests on scene ready, prints summary, and quits.
func _ready() -> void:
	print("=== Concept Test Scene ===")
	print("")
	var runner: TestRunner = TestRunner.new()
	var _results: Array[TestResult] = await runner.run_all(_test_scripts, get_tree())
	runner.print_summary()

	if runner.get_fail_count() > 0:
		get_tree().quit(1)
	else:
		get_tree().quit(0)
