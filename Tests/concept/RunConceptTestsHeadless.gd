## Headless runner for asteroid belt concept tests.
## Run with: godot --headless -s Tests/concept/RunConceptTestsHeadless.gd
extends SceneTree


## Preload concept domain and RNG so class_name types are registered before test scripts.
const _concept_deps: GDScript = preload("res://Tests/concept/ConceptDeps.gd")
## Preload test framework.
const _test_case: GDScript = preload("res://Tests/Framework/TestCase.gd")
const _test_runner: GDScript = preload("res://Tests/Framework/TestRunner.gd")
const _test_result: GDScript = preload("res://Tests/Framework/TestResult.gd")

## Test scripts to execute. Must match ConceptTestScene._test_scripts.
var _test_scripts: Array = [
	preload("res://Tests/concept/TestAsteroidBeltGeneratorConcept.gd"),
	preload("res://Tests/concept/TestOrbitalMathConcept.gd"),
]


## Runs all concept tests via a child node so we can await run_all() (coroutine).
func _init() -> void:
	print("=== Concept Tests (Headless) ===")
	print("")
	var runner_node: Node = Node.new()
	runner_node.set_script(preload("res://Tests/concept/ConceptHeadlessRunnerNode.gd"))
	runner_node.set_meta("test_scripts", _test_scripts)
	root.add_child(runner_node)
