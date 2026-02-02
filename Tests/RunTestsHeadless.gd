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
	# Phase 2 tests - Stage 5
	preload("res://Tests/Unit/TestMoonGenerator.gd"),
	# Phase 2 tests - Stage 6
	preload("res://Tests/Unit/TestAsteroidGenerator.gd"),
	# Phase 2 tests - Stage 7
	preload("res://Tests/Unit/TestRingSystemGenerator.gd"),
	# Phase 2 tests - Stage 8
	preload("res://Tests/Unit/TestGoldenMasters.gd"),
	# Phase 3 tests - Stage 1
	preload("res://Tests/Integration/TestObjectViewer.gd"),
	# Phase 3 tests - Stage 4
	preload("res://Tests/Integration/TestSaveLoad.gd"),
	# Phase 3 tests - Stages 5 & 6
	preload("res://Tests/Unit/TestColorUtils.gd"),
	# System Viewer tests
	preload("res://Tests/Unit/TestSystemScaleManager.gd"),
	preload("res://Tests/Unit/TestOrbitRenderer.gd"),
	preload("res://Tests/Integration/TestSystemCameraController.gd"),
	preload("res://Tests/Unit/TestSystemBodyNode.gd"),
	preload("res://Tests/Unit/TestSystemInspectorPanel.gd"),
	# Phase 6 tests - Stage 1
	preload("res://Tests/Unit/TestHierarchyNode.gd"),
	preload("res://Tests/Unit/TestSystemHierarchy.gd"),
	preload("res://Tests/Unit/TestOrbitHost.gd"),
	preload("res://Tests/Unit/TestAsteroidBelt.gd"),
	preload("res://Tests/Unit/TestSolarSystem.gd"),
	# Phase 6 tests - Stage 2
	preload("res://Tests/Unit/TestOrbitalMechanics.gd"),
	# Phase 6 tests - Stage 3
	preload("res://Tests/Unit/TestSolarSystemSpec.gd"),
	preload("res://Tests/Unit/TestStellarConfigGenerator.gd"),
	# Phase 6 tests - Stage 4
	preload("res://Tests/Unit/TestOrbitSlot.gd"),
	preload("res://Tests/Unit/TestOrbitSlotGenerator.gd"),
	# Phase 6 tests - Stage 5
	preload("res://Tests/Unit/TestSystemPlanetGenerator.gd"),
	# Phase 6 tests - Stage 6
	preload("res://Tests/Unit/TestSystemMoonGenerator.gd"),
	# Phase 6 tests - Stage 7
	preload("res://Tests/Unit/TestSystemAsteroidGenerator.gd"),
	# Phase 6 tests - Stages 8 & 9
	preload("res://Tests/Unit/TestSystemValidator.gd"),
	preload("res://Tests/Unit/TestSystemSerializer.gd"),
	preload("res://Tests/Integration/TestSystemPersistence.gd"),
	preload("res://Tests/Unit/TestSystemGoldenMasters.gd"),
	# Phase 6 tests - System Viewer Integration
	preload("res://Tests/Integration/TestSystemViewer.gd"),
	# Phase 6 tests - Navigation
	preload("res://Tests/Integration/TestMainApp.gd"),
	# Galaxy tests
	preload("res://Tests/domain/galaxy/TestStableHash.gd"),
	preload("res://Tests/domain/galaxy/TestSpiralDensityModel.gd"),
	preload("res://Tests/domain/galaxy/TestDensitySampler.gd"),
	preload("res://Tests/domain/galaxy/TestGalaxyCoordinates.gd"),
	preload("res://Tests/domain/galaxy/TestZoomStateMachine.gd"),
	preload("res://Tests/domain/galaxy/TestRaycastUtils.gd"),
	preload("res://Tests/domain/galaxy/TestSeedDeriver.gd"),
	preload("res://Tests/domain/galaxy/TestQuadrantSelector.gd"),
	preload("res://Tests/domain/galaxy/TestGridCursor.gd"),
	preload("res://Tests/domain/galaxy/TestSubSectorGenerator.gd"),
	preload("res://Tests/domain/galaxy/TestStarPicker.gd"),
	preload("res://Tests/domain/galaxy/TestSubSectorNeighborhood.gd"),
]


func _init() -> void:
	print("")
	print("StarGen Test Suite (Headless)")
	print("==============================")
	print("")
	print("Running tests...")
	
	var runner: TestRunner = TestRunner.new()
	
	runner.run_all(_test_scripts)
	
	runner.print_summary()
	
	var exit_code: int = 0 if runner.get_fail_count() == 0 else 1
	quit(exit_code)
