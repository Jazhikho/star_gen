## Test scene that runs all tests and displays results in the console.
## Run this scene directly or use --headless mode.
extends Node

## Preload Phase 1 domain/service scripts so class_name types are registered before test scripts compile.
const _phase1_deps = preload("res://Tests/Phase1Deps.gd")
## Preload population domain scripts so class_name types are registered before population test scripts.
const _population_deps = preload("res://Tests/PopulationDeps.gd")
## Preload jump lanes domain scripts so class_name types are registered before jump lanes test scripts.
const _jump_lanes_deps = preload("res://Tests/JumpLanesDeps.gd")

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
	# Phase 2 tests - Object generators
	preload("res://Tests/Unit/TestParentContext.gd"),
	preload("res://Tests/Unit/TestBaseSpec.gd"),
	preload("res://Tests/Unit/TestSizeTable.gd"),
	preload("res://Tests/Unit/TestStarTable.gd"),
	preload("res://Tests/Unit/TestStarGenerator.gd"),
	preload("res://Tests/Unit/TestPlanetGenerator.gd"),
	preload("res://Tests/Unit/TestMoonGenerator.gd"),
	preload("res://Tests/Unit/TestAsteroidGenerator.gd"),
	preload("res://Tests/Unit/TestRingSystemGenerator.gd"),
	preload("res://Tests/Unit/TestGoldenMasters.gd"),
	# Phase 3 tests - Object viewer
	preload("res://Tests/Integration/TestObjectViewer.gd"),
	preload("res://Tests/Integration/TestObjectViewerMoons.gd"),
	preload("res://Tests/Integration/TestSaveLoad.gd"),
	preload("res://Tests/Unit/TestColorUtils.gd"),
	preload("res://Tests/Unit/TestColorUtilsShaderParams.gd"),
	preload("res://Tests/Unit/TestStarShaderParams.gd"),
	preload("res://Tests/Unit/TestTerrestrialShaderParams.gd"),
	preload("res://Tests/Unit/TestGasGiantShaderParams.gd"),
	preload("res://Tests/Unit/TestRingShaderParams.gd"),
	preload("res://Tests/Unit/TestAtmosphereShaderParams.gd"),
	# System Viewer tests
	preload("res://Tests/Unit/TestSystemScaleManager.gd"),
	preload("res://Tests/Unit/TestOrbitRenderer.gd"),
	preload("res://Tests/Integration/TestSystemCameraController.gd"),
	preload("res://Tests/Unit/TestSystemBodyNode.gd"),
	preload("res://Tests/Unit/TestSystemInspectorPanel.gd"),
	preload("res://Tests/Unit/TestSystemDisplayLayout.gd"),
	# Phase 6 tests - Solar system
	preload("res://Tests/Unit/TestHierarchyNode.gd"),
	preload("res://Tests/Unit/TestSystemHierarchy.gd"),
	preload("res://Tests/Unit/TestOrbitHost.gd"),
	preload("res://Tests/Unit/TestAsteroidBelt.gd"),
	preload("res://Tests/Unit/TestSolarSystem.gd"),
	preload("res://Tests/Unit/TestOrbitalMechanics.gd"),
	preload("res://Tests/Unit/TestSolarSystemSpec.gd"),
	preload("res://Tests/Unit/TestStellarConfigGenerator.gd"),
	preload("res://Tests/Unit/TestOrbitSlot.gd"),
	preload("res://Tests/Unit/TestOrbitSlotGenerator.gd"),
	preload("res://Tests/Unit/TestSystemPlanetGenerator.gd"),
	preload("res://Tests/Unit/TestSystemMoonGenerator.gd"),
	preload("res://Tests/Unit/TestSystemAsteroidGenerator.gd"),
	preload("res://Tests/Unit/TestSystemValidator.gd"),
	preload("res://Tests/Unit/TestSystemSerializer.gd"),
	preload("res://Tests/Integration/TestSystemPersistence.gd"),
	preload("res://Tests/Unit/TestSystemGoldenMasters.gd"),
	preload("res://Tests/Integration/TestSystemViewer.gd"),
	preload("res://Tests/Integration/TestMainApp.gd"),
	preload("res://Tests/Integration/TestMainAppNavigation.gd"),
	preload("res://Tests/Unit/TestSystemCache.gd"),
	# Galaxy Viewer
	preload("res://Tests/Unit/TestGalaxyInspectorPanel.gd"),
	preload("res://Tests/Integration/TestGalaxyViewerUI.gd"),
	preload("res://Tests/Unit/TestHomePosition.gd"),
	preload("res://Tests/Integration/TestGalaxyViewerHome.gd"),
	preload("res://Tests/Integration/TestGalaxySystemTransition.gd"),
	preload("res://Tests/Unit/TestGalaxyConfig.gd"),
	preload("res://Tests/Unit/TestGalaxySaveData.gd"),
	preload("res://Tests/Integration/TestGalaxyPersistence.gd"),
	preload("res://Tests/Integration/TestGalaxyRandomization.gd"),
	preload("res://Tests/Integration/TestWelcomeScreen.gd"),
	preload("res://Tests/Integration/TestGalaxyStartup.gd"),
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
	# Population framework (NativePopulation first for class scope)
	preload("res://Tests/Unit/Population/TestNativePopulation.gd"),
	# Population framework (Stage 1: Planet Profile Model)
	preload("res://Tests/Unit/Population/TestClimateZone.gd"),
	preload("res://Tests/Unit/Population/TestBiomeType.gd"),
	preload("res://Tests/Unit/Population/TestResourceType.gd"),
	preload("res://Tests/Unit/Population/TestHabitabilityCategory.gd"),
	preload("res://Tests/Unit/Population/TestPlanetProfile.gd"),
	# Population framework (Stage 2: Profile Generator)
	preload("res://Tests/Unit/Population/TestProfileCalculations.gd"),
	preload("res://Tests/Unit/Population/TestProfileGenerator.gd"),
	# Population framework (Stage 2: Colony Suitability)
	preload("res://Tests/Unit/Population/TestColonySuitability.gd"),
	preload("res://Tests/Unit/Population/TestSuitabilityCalculator.gd"),
	# Population framework (Stage 3: History)
	preload("res://Tests/Unit/Population/TestHistoryEvent.gd"),
	preload("res://Tests/Unit/Population/TestPopulationHistory.gd"),
	preload("res://Tests/Unit/Population/TestHistoryGenerator.gd"),
	# Population framework (Government/Regime, TechnologyLevel, NativePopulation generator)
	preload("res://Tests/Unit/Population/TestGovernmentType.gd"),
	preload("res://Tests/Unit/Population/TestGovernment.gd"),
	preload("res://Tests/Unit/Population/TestTechnologyLevel.gd"),
	preload("res://Tests/Unit/Population/TestNativePopulationGenerator.gd"),
	# Population framework (Stage 5: Colony)
	preload("res://Tests/Unit/Population/TestColonyType.gd"),
	preload("res://Tests/Unit/Population/TestNativeRelation.gd"),
	preload("res://Tests/Unit/Population/TestColony.gd"),
	preload("res://Tests/Unit/Population/TestColonyGenerator.gd"),
	# Population framework (Stage 6: Integration boundary)
	preload("res://Tests/Unit/Population/TestPlanetPopulationData.gd"),
	preload("res://Tests/Unit/Population/TestPopulationGenerator.gd"),
	# Population integration (Phase 5: Population wiring)
	preload("res://Tests/Unit/Population/TestPopulationProbability.gd"),
	preload("res://Tests/Unit/Population/TestPopulationSeeding.gd"),
	preload("res://Tests/Integration/TestPopulationIntegration.gd"),
	preload("res://Tests/Integration/TestPopulationGoldenMasters.gd"),
	# Jump Lanes
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneSystem.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneConnection.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneRegion.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneResult.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneCalculator.gd"),
	preload("res://Tests/Unit/JumpLanes/TestJumpLaneClusterConnector.gd"),
	# Station framework (outposts, space stations)
	preload("res://Tests/Unit/Population/TestStationClass.gd"),
	preload("res://Tests/Unit/Population/TestStationType.gd"),
	preload("res://Tests/Unit/Population/TestStationPurpose.gd"),
	preload("res://Tests/Unit/Population/TestStationService.gd"),
	preload("res://Tests/Unit/Population/TestStationPlacementContext.gd"),
	preload("res://Tests/Unit/Population/TestOutpostAuthority.gd"),
	preload("res://Tests/Unit/Population/TestOutpost.gd"),
	preload("res://Tests/Unit/Population/TestSpaceStation.gd"),
	preload("res://Tests/Unit/Population/TestStationPlacementRules.gd"),
	preload("res://Tests/Unit/Population/TestStationSpec.gd"),
	preload("res://Tests/Unit/Population/TestStationGenerator.gd"),
]

## The test runner instance.
var _runner: TestRunner


func _ready() -> void:
	print("")
	print("StarGen Test Suite")
	print("==================")
	print("")
	print("Running tests...")
	
	_runner = TestRunner.new()
	_runner.test_finished.connect(_on_test_finished)
	
	await _runner.run_all(_test_scripts, get_tree())
	
	_runner.print_summary()
	
	var exit_code: int = 0 if _runner.get_fail_count() == 0 else 1
	
	# Give a moment for output to flush, then exit
	await get_tree().create_timer(0.1).timeout
	get_tree().quit(exit_code)


func _on_test_finished(_result: TestResult) -> void:
	# Failures are already printed by TestRunner during execution
	pass
