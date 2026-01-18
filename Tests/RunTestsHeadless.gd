## Headless test runner script.
## Run with: godot --headless --script res://Tests/RunTestsHeadless.gd
extends SceneTree

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
