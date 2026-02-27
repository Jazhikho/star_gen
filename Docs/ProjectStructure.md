# StarGen Project Structure

Complete enumeration of the project file structure. Excludes `.uid` files, `.git/`, and `.godot/` (generated).

```
star_gen/
├── .editorconfig
├── .gitattributes
├── .gitignore
├── claude.md                       # Architecture and working agreement
├── icon.svg
├── icon.svg.import
├── LICENSE
├── project.godot
├── README.md
│
├── Concepts/                       # Visual concept demos (reference)
│   ├── Additions.md
│   ├── planetgenerator.html
│   ├── stargenerator.html
│   ├── CivilisationReference/      # Tech tree & regime chart (React)
│   │   ├── index.html
│   │   ├── App.jsx
│   │   ├── LayoutHelpers.js
│   │   ├── RegimeChartTab.jsx
│   │   ├── TechTreeTab.jsx
│   │   └── data/
│   │       ├── Levels.js
│   │       ├── Regimes.js
│   │       └── Techs.js
│   │
│   ├── HistoryGenerator/           # Culture sim, regime transitions, map (React)
│   │   ├── index.html
│   │   ├── HistoryGenerator.jsx
│   │   ├── data/
│   │   │   ├── Constants.js
│   │   │   ├── Regimes.js
│   │   │   └── Transitions.js
│   │   └── logic/
│   │       ├── NameGen.js
│   │       ├── Simulation.js
│   │       └── Utils.js
│   │
│   ├── EvoTechTree/                # Biology as evolutionary tech tree; lineage, evo simulator (React)
│   │   ├── index.html
│   │   ├── App.jsx                 # Data, NODES, ENVS, evolveStep, genSpecies
│   │   └── AppUI.jsx               # SpeciesDetail, DictModal, NodeCard, EvoMode, App + mount
│   │
│   └── Integration/                # Civilisation Engine: shared data, Tech Tree, Regime Chart, History sim
│       ├── index.html
│       ├── IntegrationApp.jsx
│       ├── LayoutHelpers.js
│       ├── data/
│       │   └── SharedData.js
│       └── logic/
│           └── Simulation.js
│
├── Docs/
│   ├── CelestialBodyProperties.md
│   ├── GDD.md
│   ├── ProjectStructure.md        # This file
│   ├── RegimeChangeModel.md
│   └── Roadmap.md
│
├── src/
│   ├── app/
│   │   ├── MainApp.gd
│   │   ├── MainApp.tscn
│   │   ├── WelcomeScreen.gd
│   │   ├── WelcomeScreen.tscn
│   │   │
│   │   ├── components/
│   │   │   ├── CollapsibleSection.gd
│   │   │   └── CollapsibleSection.tscn
│   │   │
    │   │   ├── galaxy_viewer/
    │   │   │   ├── GalaxyInspectorPanel.gd
    │   │   │   ├── GalaxyRenderer.gd
    │   │   │   ├── SectorJumpLaneRenderer.gd
    │   │   │   ├── GalaxyViewer.gd
│   │   │   ├── GalaxyViewer.tscn
│   │   │   ├── GalaxyViewerDeps.gd
│   │   │   ├── GalaxyViewerSaveLoad.gd
│   │   │   ├── NavigationCompass.gd
│   │   │   ├── NeighborhoodRenderer.gd
│   │   │   ├── OrbitCamera.gd
│   │   │   ├── QuadrantRenderer.gd
│   │   │   ├── QuadrantSelector.gd
│   │   │   ├── SectorRenderer.gd
│   │   │   ├── SelectionIndicator.gd
│   │   │   ├── StarViewCamera.gd
│   │   │   ├── SubSectorRenderer.gd
│   │   │   ├── ZoomStateMachine.gd
│   │   │   └── shaders/
│   │   │       ├── quadrant_cell.gdshader
│   │   │       ├── sector_cell.gdshader
│   │   │       ├── selection_ring.gdshader
│   │   │       ├── star_billboard.gdshader
│   │   │       ├── star_sector_view.gdshader
│   │   │       └── subsector_wire.gdshader
│   │   │
│   │   ├── jumplanes_prototype/
│   │   │   ├── JumpLaneRenderer.gd
│   │   │   ├── JumpLanesPrototype.gd
│   │   │   ├── JumpLanesPrototype.tscn
│   │   │   └── MockRegionGenerator.gd
│   │   │
│   │   ├── prototypes/
│   │   │   ├── StationGeneratorPrototype.gd
│   │   │   └── StationGeneratorPrototype.tscn
│   │   │
│   │   ├── rendering/
│   │   │   ├── AtmosphereShaderParams.gd
│   │   │   ├── BodyRenderer.gd
│   │   │   ├── BodyRenderer.tscn
│   │   │   ├── ColorUtils.gd
│   │   │   ├── GasGiantShaderParams.gd
│   │   │   ├── MaterialFactory.gd
│   │   │   ├── RingShaderParams.gd
│   │   │   ├── ShaderParamHelpers.gd
│   │   │   ├── StarShaderParams.gd
│   │   │   ├── TerrestrialShaderParams.gd
│   │   │   ├── shaders/
│   │   │   │   ├── atmosphere_rim.gdshader
│   │   │   │   ├── noise_lib.gdshaderinc
│   │   │   │   ├── planet_gas_giant_surface.gdshader
│   │   │   │   ├── planet_terrestrial_surface.gdshader
│   │   │   │   ├── ring_system.gdshader
│   │   │   │   ├── star_atmosphere.gdshader
│   │   │   │   ├── star_surface.gdshader
│   │   │   └── textures/
│   │   │       └── noise.tres
│   │   │
│   │   ├── system_viewer/
│   │   │   ├── OrbitRenderer.gd
│   │   │   ├── SystemBodyNode.gd
│   │   │   ├── SystemBodyNode.tscn
│   │   │   ├── SystemCameraController.gd
│   │   │   ├── SystemDisplayLayout.gd
│   │   │   ├── SystemInspectorPanel.gd
│   │   │   ├── SystemScaleManager.gd
│   │   │   ├── SystemViewer.gd
│   │   │   ├── SystemViewerSaveLoad.gd
│   │   │   └── SystemViewer.tscn
│   │   │
│   │   ├── themes/
│   │   │   └── DarkTheme.tres
│   │   │
│   │   └── viewer/
│   │       ├── CameraController.gd
│   │       ├── ObjectViewerMoonSystem.gd
│   │       ├── EditDialog.gd              # Phase 4 deferred
│   │       ├── EditDialog.tscn
│   │       ├── InspectorPanel.gd
│   │       ├── ObjectViewer.gd
│   │       ├── ObjectViewer.tscn
│   │       └── PropertyFormatter.gd
│   │
│   ├── domain/
│   │   ├── celestial/
│   │   │   ├── CelestialBody.gd
│   │   │   ├── CelestialType.gd
│   │   │   ├── Provenance.gd
│   │   │   ├── components/
│   │   │   │   ├── AtmosphereProps.gd
│   │   │   │   ├── CryosphereProps.gd
│   │   │   │   ├── HydrosphereProps.gd
│   │   │   │   ├── OrbitalProps.gd
│   │   │   │   ├── PhysicalProps.gd
│   │   │   │   ├── RingBand.gd
│   │   │   │   ├── RingSystemProps.gd
│   │   │   │   ├── StellarProps.gd
│   │   │   │   ├── SurfaceProps.gd
│   │   │   │   └── TerrainProps.gd
│   │   │   ├── serialization/
│   │   │   │   └── CelestialSerializer.gd
│   │   │   └── validation/
│   │   │       ├── CelestialValidator.gd
│   │   │       ├── ValidationError.gd
│   │   │       └── ValidationResult.gd
│   │   │
│   │   ├── constants/
│   │   │   └── Versions.gd
│   │   │
│   │   ├── galaxy/
│   │   │   ├── DensityModelInterface.gd
│   │   │   ├── DensitySampler.gd
│   │   │   ├── EllipticalDensityModel.gd
│   │   │   ├── Galaxy.gd
│   │   │   ├── GalaxyConfig.gd
│   │   │   ├── GalaxyCoordinates.gd
│   │   │   ├── GalaxySample.gd
│   │   │   ├── GalaxySaveData.gd
│   │   │   ├── GalaxySpec.gd
│   │   │   ├── GalaxyStar.gd
│   │   │   ├── GalaxySystemGenerator.gd
│   │   │   ├── GridCursor.gd
│   │   │   ├── HomePosition.gd
│   │   │   ├── IrregularDensityModel.gd
│   │   │   ├── RaycastUtils.gd
│   │   │   ├── SeedDeriver.gd
│   │   │   ├── Sector.gd
│   │   │   ├── SpiralDensityModel.gd
│   │   │   ├── StableHash.gd
│   │   │   ├── StarPicker.gd
│   │   │   ├── StarSystemPreview.gd
│   │   │   ├── SubSectorGenerator.gd
│   │   │   └── SubSectorNeighborhood.gd
│   │   │
│   │   ├── generation/
│   │   │   ├── GenerationRealismProfile.gd
│   │   │   ├── ParentContext.gd
│   │   │   ├── archetypes/
│   │   │   │   ├── AsteroidType.gd
│   │   │   │   ├── OrbitZone.gd
│   │   │   │   ├── RingComplexity.gd
│   │   │   │   ├── SizeCategory.gd
│   │   │   │   └── StarClass.gd
│   │   │   ├── fixtures/
│   │   │   │   └── FixtureGenerator.gd
│   │   │   ├── generators/
│   │   │   │   ├── AsteroidGenerator.gd
│   │   │   │   ├── GeneratorUtils.gd
│   │   │   │   ├── MoonGenerator.gd
│   │   │   │   ├── PlanetGenerator.gd
│   │   │   │   ├── RingSystemGenerator.gd
│   │   │   │   ├── StarGenerator.gd
│   │   │   │   ├── moon/
│   │   │   │   │   ├── MoonAtmosphereGenerator.gd
│   │   │   │   │   ├── MoonPhysicalGenerator.gd
│   │   │   │   │   └── MoonSurfaceGenerator.gd
│   │   │   │   └── planet/
│   │   │   │       ├── PlanetAtmosphereGenerator.gd
│   │   │   │       ├── PlanetPhysicalGenerator.gd
│   │   │   │       └── PlanetSurfaceGenerator.gd
│   │   │   ├── specs/
│   │   │   │   ├── AsteroidSpec.gd
│   │   │   │   ├── BaseSpec.gd
│   │   │   │   ├── MoonSpec.gd
│   │   │   │   ├── PlanetSpec.gd
│   │   │   │   ├── RingSystemSpec.gd
│   │   │   │   └── StarSpec.gd
│   │   │   ├── tables/
│   │   │   │   ├── OrbitTable.gd
│   │   │   │   ├── SizeTable.gd
│   │   │   │   └── StarTable.gd
│   │   │   └── utils/
│   │   │       └── AtmosphereUtils.gd
│   │   │
│   │   ├── jumplanes/
│   │   │   ├── JumpLaneCalculator.gd
│   │   │   ├── JumpLaneClusterConnector.gd
│   │   │   ├── JumpLaneConnection.gd
│   │   │   ├── JumpLaneRegion.gd
│   │   │   ├── JumpLaneResult.gd
│   │   │   └── JumpLaneSystem.gd
│   │   │
│   │   ├── math/
│   │   │   ├── MathUtils.gd
│   │   │   └── Units.gd
│   │   │
│   │   ├── population/
│   │   │   ├── BiomeType.gd
│   │   │   ├── ClimateZone.gd
│   │   │   ├── Colony.gd
│   │   │   ├── ColonyGenerator.gd
│   │   │   ├── ColonySuitability.gd
│   │   │   ├── ColonyType.gd
│   │   │   ├── Government.gd
│   │   │   ├── GovernmentType.gd
│   │   │   ├── HabitabilityCategory.gd
│   │   │   ├── HistoryEvent.gd
│   │   │   ├── HistoryGenerator.gd
│   │   │   ├── NativePopulation.gd
│   │   │   ├── NativePopulationGenerator.gd
│   │   │   ├── NativeRelation.gd
│   │   │   ├── Outpost.gd
│   │   │   ├── OutpostAuthority.gd
│   │   │   ├── PlanetPopulationData.gd
│   │   │   ├── PlanetProfile.gd
│   │   │   ├── PopulationGenerator.gd
│   │   │   ├── PopulationHistory.gd
│   │   │   ├── PopulationLikelihood.gd
│   │   │   ├── PopulationProbability.gd
│   │   │   ├── PopulationSeeding.gd
│   │   │   ├── ProfileCalculations.gd
│   │   │   ├── ProfileGenerator.gd
│   │   │   ├── ResourceType.gd
│   │   │   ├── SpaceStation.gd
│   │   │   ├── StationClass.gd
│   │   │   ├── StationGenerator.gd
│   │   │   ├── StationPlacementContext.gd
│   │   │   ├── StationPlacementRules.gd
│   │   │   ├── StationPurpose.gd
│   │   │   ├── StationService.gd
│   │   │   ├── StationSpec.gd
│   │   │   ├── StationType.gd
│   │   │   ├── SuitabilityCalculator.gd
│   │   │   └── TechnologyLevel.gd
│   │   │
│   │   ├── rng/
│   │   │   └── SeededRng.gd
│   │   │
│   │   ├── system/
│   │   │   ├── AsteroidBelt.gd
│   │   │   ├── HierarchyNode.gd
│   │   │   ├── OrbitalMechanics.gd
│   │   │   ├── OrbitHost.gd
│   │   │   ├── OrbitSlot.gd
│   │   │   ├── OrbitSlotGenerator.gd
│   │   │   ├── SolarSystem.gd
│   │   │   ├── SolarSystemSpec.gd
│   │   │   ├── StellarConfigGenerator.gd
│   │   │   ├── SystemAsteroidGenerator.gd
│   │   │   ├── SystemCache.gd
│   │   │   ├── SystemHierarchy.gd
│   │   │   ├── SystemMoonGenerator.gd
│   │   │   ├── SystemPlanetGenerator.gd
│   │   │   ├── SystemSerializer.gd
│   │   │   ├── SystemValidator.gd
│   │   │   └── fixtures/
│   │   │       └── SystemFixtureGenerator.gd
│   │   │
│   │   └── validation/
│   │       └── Validation.gd
│   │
│   └── services/
│       └── persistence/
│           ├── CelestialPersistence.gd
│           ├── GalaxyPersistence.gd
│           ├── SaveData.gd
│           └── SystemPersistence.gd
│
└── Tests/
    ├── Framework/
    │   ├── TestCase.gd
    │   ├── TestResult.gd
    │   └── TestRunner.gd
    │
    ├── domain/
    │   └── galaxy/
    │       ├── TestDensitySampler.gd
    │       ├── TestGalaxyCoordinates.gd
    │       ├── TestGridCursor.gd
    │       ├── TestQuadrantSelector.gd
    │       ├── TestRaycastUtils.gd
    │       ├── TestSeedDeriver.gd
    │       ├── TestSpiralDensityModel.gd
    │       ├── TestStableHash.gd
    │       ├── TestStarPicker.gd
    │       ├── TestSubSectorGenerator.gd
    │       ├── TestSubSectorNeighborhood.gd
    │       └── TestZoomStateMachine.gd
    │
    ├── Integration/
    │   ├── TestCelestialPersistence.gd
    │   ├── TestGalaxyPersistence.gd
    │   ├── TestGalaxyRandomization.gd
    │   ├── TestGalaxyStartup.gd
    │   ├── TestGalaxySystemTransition.gd
    │   ├── TestStarSystemPreviewIntegration.gd
    │   ├── TestGalaxyViewerHome.gd
    │   ├── TestGalaxyViewerUI.gd
    │   ├── TestMainApp.gd
    │   ├── TestMainAppNavigation.gd
    │   ├── TestObjectViewer.gd
    │   ├── TestObjectViewerMoons.gd
    │   ├── TestPopulationGoldenMasters.gd
    │   ├── TestPopulationIntegration.gd
    │   ├── TestSaveLoad.gd
    │   ├── TestSystemCameraController.gd
    │   ├── TestSystemPersistence.gd
    │   ├── TestSystemViewer.gd
    │   ├── TestSystemViewerSaveLoad.gd
    │   └── TestWelcomeScreen.gd
    │
    ├── Unit/
    │   ├── JumpLanes/
    │   │   ├── TestJumpLaneCalculator.gd
    │   │   ├── TestJumpLaneClusterConnector.gd
    │   │   ├── TestJumpLaneConnection.gd
    │   │   ├── TestJumpLaneRegion.gd
    │   │   ├── TestJumpLaneResult.gd
    │   │   └── TestJumpLaneSystem.gd
    │   │
    │   ├── Population/
    │   │   ├── TestBiomeType.gd
    │   │   ├── TestClimateZone.gd
    │   │   ├── TestColony.gd
    │   │   ├── TestColonyGenerator.gd
    │   │   ├── TestColonySuitability.gd
    │   │   ├── TestColonyType.gd
    │   │   ├── TestGovernment.gd
    │   │   ├── TestGovernmentType.gd
    │   │   ├── TestHabitabilityCategory.gd
    │   │   ├── TestHistoryEvent.gd
    │   │   ├── TestHistoryGenerator.gd
    │   │   ├── TestNativePopulation.gd
    │   │   ├── TestNativePopulationGenerator.gd
    │   │   ├── TestNativeRelation.gd
    │   │   ├── TestOutpost.gd
    │   │   ├── TestOutpostAuthority.gd
    │   │   ├── TestPlanetPopulationData.gd
    │   │   ├── TestPlanetProfile.gd
    │   │   ├── TestPopulationGenerator.gd
    │   │   ├── TestPopulationHistory.gd
    │   │   ├── TestPopulationLikelihood.gd
    │   │   ├── TestPopulationProbability.gd
    │   │   ├── TestPopulationSeeding.gd
    │   │   ├── TestProfileCalculations.gd
    │   │   ├── TestProfileGenerator.gd
    │   │   ├── TestResourceType.gd
    │   │   ├── TestSpaceStation.gd
    │   │   ├── TestStationClass.gd
    │   │   ├── TestStationGenerator.gd
    │   │   ├── TestStationPlacementContext.gd
    │   │   ├── TestStationPlacementRules.gd
    │   │   ├── TestStationPurpose.gd
    │   │   ├── TestStationService.gd
    │   │   ├── TestStationSpec.gd
    │   │   ├── TestStationType.gd
    │   │   ├── TestSuitabilityCalculator.gd
    │   │   └── TestTechnologyLevel.gd
    │   │
    │   ├── TestAsteroidBelt.gd
    │   ├── TestAsteroidGenerator.gd
    │   ├── TestAtmosphereProps.gd
    │   ├── TestAtmosphereShaderParams.gd
    │   ├── TestBaseSpec.gd
    │   ├── TestCelestialBody.gd
    │   ├── TestCelestialSerializer.gd
    │   ├── TestCelestialValidator.gd
    │   ├── TestColorUtils.gd
    │   ├── TestColorUtilsShaderParams.gd
    │   ├── TestGalaxy.gd
    │   ├── TestGalaxyConfig.gd
│   ├── TestGalaxyInspectorPanel.gd
│   ├── TestGalaxySaveData.gd
│   ├── TestGenerationRealismProfile.gd
    │   ├── TestGalaxyStar.gd
    │   ├── TestGalaxySystemGenerator.gd
    │   ├── TestSector.gd
    │   ├── TestStarSystemPreview.gd
    │   ├── TestGasGiantShaderParams.gd
    │   ├── TestGoldenMasters.gd
    │   ├── TestHierarchyNode.gd
    │   ├── TestHomePosition.gd
    │   ├── TestMathUtils.gd
    │   ├── TestMoonGenerator.gd
    │   ├── TestOrbitalMechanics.gd
    │   ├── TestOrbitalProps.gd
    │   ├── TestOrbitHost.gd
    │   ├── TestOrbitRenderer.gd
    │   ├── TestOrbitSlot.gd
    │   ├── TestOrbitSlotGenerator.gd
    │   ├── TestParentContext.gd
    │   ├── TestPhysicalProps.gd
    │   ├── TestPlanetGenerator.gd
    │   ├── TestProvenance.gd
    │   ├── TestRingShaderParams.gd
    │   ├── TestRingSystemGenerator.gd
    │   ├── TestRingSystemProps.gd
    │   ├── TestSeededRng.gd
    │   ├── TestSizeTable.gd
    │   ├── TestSolarSystem.gd
    │   ├── TestSolarSystemSpec.gd
    │   ├── TestStarGenerator.gd
│   ├── TestStarGeneratorDistributions.gd
    │   ├── TestStarShaderParams.gd
    │   ├── TestStarTable.gd
    │   ├── TestStellarConfigGenerator.gd
    │   ├── TestStellarProps.gd
    │   ├── TestSystemAsteroidGenerator.gd
    │   ├── TestSystemBodyNode.gd
    │   ├── TestSystemCache.gd
    │   ├── TestSystemDisplayLayout.gd
    │   ├── TestSystemGoldenMasters.gd
│   ├── TestSystemPlanetDistributions.gd
    │   ├── TestSystemHierarchy.gd
    │   ├── TestSystemInspectorPanel.gd
    │   ├── TestSolarSystemPopulation.gd
    │   ├── TestSystemMoonGenerator.gd
    │   ├── TestSystemPlanetGenerator.gd
    │   ├── TestSystemScaleManager.gd
    │   ├── TestSystemSerializer.gd
    │   ├── TestSystemValidator.gd
    │   ├── TestTerrestrialShaderParams.gd
    │   ├── TestUnits.gd
    │   ├── TestValidation.gd
    │   └── TestVersions.gd
    │
    ├── JumpLanesDeps.gd
    ├── JumpLanesTestRunner.gd
    ├── JumpLanesTestScene.gd
    ├── JumpLanesTestScene.tscn
    ├── Phase1Deps.gd
    ├── PopulationDeps.gd
    ├── RunGalaxyTests.gd
    ├── RunTestsHeadless.gd
    ├── TestScene.gd
    └── TestScene.tscn
```

## Layer summary

| Layer   | Path                    | Purpose                                      |
|---------|-------------------------|----------------------------------------------|
| Domain  | `src/domain/`           | Pure logic; no Nodes, SceneTree, or file I/O |
| Services| `src/services/`         | Persistence, I/O, caching                     |
| App     | `src/app/`              | Scenes, UI, input, rendering                  |
