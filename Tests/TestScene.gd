## Test scene that runs all tests and displays results in the console.
## Run this scene directly or use --headless mode.
extends Node

## Preload Phase 1 domain/service scripts so class_name types are registered before test scripts compile.
const _phase1_deps = preload("res://Tests/Phase1Deps.gd")

## Array of test scripts to run. Add new test scripts here.
var _test_scripts: Array[GDScript] = [
	# Phase 0 tests
	preload("res://Tests/Unit/TestSeededRng.gd"),
	preload("res://Tests/Unit/TestVersions.gd"),
	preload("res://Tests/Unit/TestMathUtils.gd"),
	preload("res://Tests/Unit/TestUnits.gd"),
	preload("res://Tests/Unit/TestValidation.gd"),
	# Phase 1 tests
	preload("res://Tests/Unit/TestProvenance.gd"),
	preload("res://Tests/Unit/TestPhysicalProps.gd"),
	preload("res://Tests/Unit/TestOrbitalProps.gd"),
	preload("res://Tests/Unit/TestAtmosphereProps.gd"),
	preload("res://Tests/Unit/TestRingSystemProps.gd"),
	preload("res://Tests/Unit/TestStellarProps.gd"),
	preload("res://Tests/Unit/TestCelestialBody.gd"),
	preload("res://Tests/Unit/TestCelestialValidator.gd"),
	preload("res://Tests/Unit/TestCelestialSerializer.gd"),
	preload("res://Tests/Integration/TestCelestialPersistence.gd"),
	# Phase 2 tests - Stage 1
	preload("res://Tests/Unit/TestParentContext.gd"),
	preload("res://Tests/Unit/TestBaseSpec.gd"),
	preload("res://Tests/Unit/TestSizeTable.gd"),
	preload("res://Tests/Unit/TestStarTable.gd"),
	# Phase 2 tests - Stage 2
	preload("res://Tests/Unit/TestStarGenerator.gd"),
	# Phase 2 tests - Stage 3
	preload("res://Tests/Unit/TestPlanetGenerator.gd"),
	# Phase 2 tests - Stage 5
	preload("res://Tests/Unit/TestMoonGenerator.gd"),
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
