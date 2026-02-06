## Test scene that runs all tests and displays results in the console.
## Run this scene directly or use --headless mode.
extends Node

## Preload Phase 1 domain/service scripts so class_name types are registered before test scripts compile.
const _phase1_deps = preload("res://Tests/Phase1Deps.gd")
## Preload population domain scripts so class_name types are registered before population test scripts.
const _population_deps = preload("res://Tests/PopulationDeps.gd")

## Array of test scripts to run. Add new test scripts here.
## On population branch: only population tests run; others commented out until merge.
var _test_scripts: Array[GDScript] = [
	# --- COMMENTED OUT (unrelated to population branch) ---
	# # Phase 0 tests
	# preload("res://Tests/Unit/TestSeededRng.gd"),
	# preload("res://Tests/Unit/TestVersions.gd"),
	# preload("res://Tests/Unit/TestMathUtils.gd"),
	# preload("res://Tests/Unit/TestUnits.gd"),
	# preload("res://Tests/Unit/TestValidation.gd"),
	# # Phase 1 tests
	# preload("res://Tests/Unit/TestProvenance.gd"),
	# preload("res://Tests/Unit/TestPhysicalProps.gd"),
	# preload("res://Tests/Unit/TestOrbitalProps.gd"),
	# preload("res://Tests/Unit/TestAtmosphereProps.gd"),
	# preload("res://Tests/Unit/TestRingSystemProps.gd"),
	# preload("res://Tests/Unit/TestStellarProps.gd"),
	# preload("res://Tests/Unit/TestCelestialBody.gd"),
	# preload("res://Tests/Unit/TestCelestialValidator.gd"),
	# preload("res://Tests/Unit/TestCelestialSerializer.gd"),
	# preload("res://Tests/Integration/TestCelestialPersistence.gd"),
	# # Phase 2 tests - Stage 1
	# preload("res://Tests/Unit/TestParentContext.gd"),
	# preload("res://Tests/Unit/TestBaseSpec.gd"),
	# preload("res://Tests/Unit/TestSizeTable.gd"),
	# preload("res://Tests/Unit/TestStarTable.gd"),
	# # Phase 2 tests - Stage 2
	# preload("res://Tests/Unit/TestStarGenerator.gd"),
	# # Phase 2 tests - Stage 3
	# preload("res://Tests/Unit/TestPlanetGenerator.gd"),
	# # Phase 2 tests - Stage 5
	# preload("res://Tests/Unit/TestMoonGenerator.gd"),
	# # Phase 2 tests - Stage 6
	# preload("res://Tests/Unit/TestAsteroidGenerator.gd"),
	# # Phase 2 tests - Stage 7
	# preload("res://Tests/Unit/TestRingSystemGenerator.gd"),
	# # Phase 2 tests - Stage 8
	# preload("res://Tests/Unit/TestGoldenMasters.gd"),
	# # Phase 3 tests - Stage 1
	# preload("res://Tests/Integration/TestObjectViewer.gd"),
	# # Phase 3 tests - Stage 4
	# preload("res://Tests/Integration/TestSaveLoad.gd"),
	# # Phase 3 tests - Stages 5 & 6
	# preload("res://Tests/Unit/TestColorUtils.gd"),
	# # System Viewer tests
	# preload("res://Tests/Unit/TestSystemScaleManager.gd"),
	# preload("res://Tests/Unit/TestOrbitRenderer.gd"),
	# preload("res://Tests/Integration/TestSystemCameraController.gd"),
	# preload("res://Tests/Unit/TestSystemBodyNode.gd"),
	# preload("res://Tests/Unit/TestSystemInspectorPanel.gd"),
	# # Phase 6 tests - Stage 1
	# preload("res://Tests/Unit/TestHierarchyNode.gd"),
	# preload("res://Tests/Unit/TestSystemHierarchy.gd"),
	# preload("res://Tests/Unit/TestOrbitHost.gd"),
	# preload("res://Tests/Unit/TestAsteroidBelt.gd"),
	# preload("res://Tests/Unit/TestSolarSystem.gd"),
	# # Phase 6 tests - Stage 2
	# preload("res://Tests/Unit/TestOrbitalMechanics.gd"),
	# # Phase 6 tests - Stage 3
	# preload("res://Tests/Unit/TestSolarSystemSpec.gd"),
	# preload("res://Tests/Unit/TestStellarConfigGenerator.gd"),
	# # Phase 6 tests - Stage 4
	# preload("res://Tests/Unit/TestOrbitSlot.gd"),
	# preload("res://Tests/Unit/TestOrbitSlotGenerator.gd"),
	# # Phase 6 tests - Stage 5
	# preload("res://Tests/Unit/TestSystemPlanetGenerator.gd"),
	# # Phase 6 tests - Stage 6
	# preload("res://Tests/Unit/TestSystemMoonGenerator.gd"),
	# # Phase 6 tests - Stage 7
	# preload("res://Tests/Unit/TestSystemAsteroidGenerator.gd"),
	# # Phase 6 tests - Stages 8 & 9
	# preload("res://Tests/Unit/TestSystemValidator.gd"),
	# preload("res://Tests/Unit/TestSystemSerializer.gd"),
	# preload("res://Tests/Integration/TestSystemPersistence.gd"),
	# preload("res://Tests/Unit/TestSystemGoldenMasters.gd"),
	# # Phase 6 tests - System Viewer Integration
	# preload("res://Tests/Integration/TestSystemViewer.gd"),
	# # Phase 6 tests - Navigation
	# preload("res://Tests/Integration/TestMainApp.gd"),
	# preload("res://Tests/Integration/TestMainAppNavigation.gd"),
	# # Phase 7 - System cache
	# preload("res://Tests/Unit/TestSystemCache.gd"),
	# # Galaxy Viewer UI
	# preload("res://Tests/Unit/TestGalaxyInspectorPanel.gd"),
	# preload("res://Tests/Integration/TestGalaxyViewerUI.gd"),
	# # Galaxy Viewer Home (Stage 3)
	# preload("res://Tests/Unit/TestHomePosition.gd"),
	# preload("res://Tests/Integration/TestGalaxyViewerHome.gd"),
	# # Galaxy → System transitions (Stage 4)
	# preload("res://Tests/Integration/TestGalaxySystemTransition.gd"),
	# # Galaxy persistence (Stage 5)
	# preload("res://Tests/Unit/TestGalaxyConfig.gd"),
	# preload("res://Tests/Unit/TestGalaxySaveData.gd"),
	# preload("res://Tests/Integration/TestGalaxyPersistence.gd"),
	# preload("res://Tests/Integration/TestGalaxyRandomization.gd"),
	# preload("res://Tests/Integration/TestWelcomeScreen.gd"),
	# preload("res://Tests/Integration/TestGalaxyStartup.gd"),
	# # Galaxy tests
	# preload("res://Tests/domain/galaxy/TestStableHash.gd"),
	# preload("res://Tests/domain/galaxy/TestSpiralDensityModel.gd"),
	# preload("res://Tests/domain/galaxy/TestDensitySampler.gd"),
	# preload("res://Tests/domain/galaxy/TestGalaxyCoordinates.gd"),
	# preload("res://Tests/domain/galaxy/TestZoomStateMachine.gd"),
	# preload("res://Tests/domain/galaxy/TestRaycastUtils.gd"),
	# preload("res://Tests/domain/galaxy/TestSeedDeriver.gd"),
	# preload("res://Tests/domain/galaxy/TestQuadrantSelector.gd"),
	# preload("res://Tests/domain/galaxy/TestGridCursor.gd"),
	# preload("res://Tests/domain/galaxy/TestSubSectorGenerator.gd"),
	# preload("res://Tests/domain/galaxy/TestStarPicker.gd"),
	# preload("res://Tests/domain/galaxy/TestSubSectorNeighborhood.gd"),
	# --- Population branch: run only population tests ---
	# Population framework (Stage 4: NativePopulation - load early so NativePopulation class is in scope)
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
	# Station framework (outposts-and-spacestations branch)
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
