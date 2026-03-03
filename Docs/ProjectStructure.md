# StarGen Project Structure

Complete enumeration of the project file structure. Excludes `.uid` files, `.git/`, `.godot/` (generated).

## Incremental C# Migration Note

The repo now contains side-by-side `.gd` and `.cs` sources during the incremental C# migration. Existing scenes still reference the `.gd` scripts until each slice is explicitly migrated.

Current C# bootstrap additions:
- `StarGen.sln`
- `StarGen.csproj`
- `src/domain/bootstrap/CSharpSmokeTest.cs`
- `src/domain/bootstrap/CSharp*Bridge.cs` (remaining transitional bridge helpers; the generator/helper subset is now removed where no longer needed)
- `src/domain/constants/Versions.cs`
- `src/domain/math/Units.cs`
- `src/domain/math/MathUtils.cs`
- `src/domain/validation/Validation.cs`
- `src/domain/rng/SeededRng.cs`
- `src/domain/celestial/CelestialType.cs`
- `src/domain/celestial/CelestialBody.cs`
- `src/domain/celestial/Provenance.cs`
- `src/domain/celestial/validation/ValidationError.cs`
- `src/domain/celestial/validation/ValidationResult.cs`
- `src/domain/celestial/validation/CelestialValidator.cs`
- `src/domain/celestial/serialization/CelestialSerializer.cs`
- `src/domain/celestial/serialization/SerializedPopulationData.cs`
- `src/domain/celestial/components/TerrainProps.cs`
- `src/domain/celestial/components/HydrosphereProps.cs`
- `src/domain/celestial/components/CryosphereProps.cs`
- `src/domain/celestial/components/SurfaceProps.cs`
- `src/domain/celestial/components/RingBand.cs`
- `src/domain/celestial/components/RingSystemProps.cs`
- `src/domain/celestial/components/PhysicalProps.cs`
- `src/domain/celestial/components/OrbitalProps.cs`
- `src/domain/celestial/components/AtmosphereProps.cs`
- `src/domain/celestial/components/StellarProps.cs`
- `src/domain/generation/ParentContext.cs`
- `src/domain/generation/GenerationRealismProfile.cs`
- `src/domain/generation/specs/BaseSpec.cs`
- `src/domain/generation/specs/StarSpec.cs`
- `src/domain/generation/specs/PlanetSpec.cs`
- `src/domain/generation/specs/MoonSpec.cs`
- `src/domain/generation/specs/AsteroidSpec.cs`
- `src/domain/generation/specs/RingSystemSpec.cs`
- `src/domain/generation/tables/SizeTable.cs`
- `src/domain/generation/tables/StarTable.cs`
- `src/domain/generation/tables/OrbitTable.cs`
- `src/domain/generation/utils/AtmosphereUtils.cs`
- `src/domain/generation/generators/GeneratorUtils.cs`
- `src/domain/generation/generators/StarGenerator.cs`
- `src/domain/generation/generators/PlanetGenerator.cs`
- `src/domain/generation/generators/MoonGenerator.cs`
- `src/domain/generation/generators/AsteroidGenerator.cs`
- `src/domain/generation/generators/RingSystemGenerator.cs`
- `src/domain/generation/archetypes/SizeCategory.cs`
- `src/domain/generation/archetypes/StarClass.cs`
- `src/domain/generation/archetypes/AsteroidType.cs`
- `src/domain/generation/archetypes/RingComplexity.cs`
- `src/domain/generation/archetypes/TravellerSizeCode.cs`
- `src/domain/generation/archetypes/OrbitZone.cs`
- `src/domain/population/HabitabilityCategory.cs`
- `src/domain/population/ClimateZone.cs`
- `src/domain/population/BiomeType.cs`
- `src/domain/population/ResourceType.cs`
- `src/domain/population/TechnologyLevel.cs`
- `src/domain/population/GovernmentType.cs`
- `src/domain/population/Government.cs`
- `src/domain/population/ColonyType.cs`
- `src/domain/population/NativeRelation.cs`
- `src/domain/population/HistoryEvent.cs`
- `src/domain/population/PopulationHistory.cs`
- `src/domain/population/NativePopulation.cs`
- `src/domain/population/Colony.cs`
- `src/domain/population/PlanetPopulationData.cs`
- `src/domain/population/PlanetProfile.cs`
- `src/domain/population/ColonySuitability.cs`
- `src/domain/population/PopulationSeeding.cs`
- `src/domain/population/PopulationProbability.cs`
- `src/domain/population/PopulationLikelihood.cs`
- `src/domain/population/SuitabilityCalculator.cs`
- `src/domain/population/ProfileCalculations.cs`
- `src/domain/population/ProfileGenerator.cs`
- `src/domain/population/HistoryGenerator.cs`
- `src/domain/population/NativePopulationGenerator.cs`
- `src/domain/population/ColonyGenerator.cs`
- `src/domain/population/PopulationGenerator.cs`
- `src/domain/population/OutpostAuthority.cs`
- `src/domain/population/StationClass.cs`
- `src/domain/population/StationType.cs`
- `src/domain/population/StationPurpose.cs`
- `src/domain/population/StationService.cs`
- `src/domain/population/StationPlacementContext.cs`
- `src/domain/population/StationPlacementRecommendation.cs`
- `src/domain/population/StationSystemContext.cs`
- `src/domain/population/StationPlacementRules.cs`
- `src/domain/population/StationSpec.cs`
- `src/domain/population/Outpost.cs`
- `src/domain/population/SpaceStation.cs`
- `src/domain/population/StationGenerationResult.cs`
- `src/domain/population/StationGenerator.cs`
- `src/domain/jumplanes/JumpLaneConnection.cs`
- `src/domain/jumplanes/JumpLaneSystem.cs`
- `src/domain/jumplanes/JumpLaneRegion.cs`
- `src/domain/jumplanes/JumpLaneResult.cs`
- `src/domain/jumplanes/JumpLaneClusterConnector.cs`
- `src/domain/jumplanes/JumpLaneCalculator.cs`
- `src/domain/system/HierarchyNode.cs`
- `src/domain/system/SystemHierarchy.cs`
- `src/domain/system/OrbitHost.cs`
- `src/domain/system/OrbitSlot.cs`
- `src/domain/system/AsteroidBelt.cs`
- `src/domain/system/SolarSystemSpec.cs`
- `src/domain/system/SolarSystem.cs`
- `src/domain/system/SystemSerializer.cs`
- `src/domain/system/SystemCache.cs`
- `src/domain/system/OrbitalMechanics.cs`
- `src/domain/system/SystemValidator.cs`
- `src/domain/system/OrbitSlotGenerationResult.cs`
- `src/domain/system/MoonGenerationResult.cs`
- `src/domain/system/BeltGenerationResult.cs`
- `src/domain/system/BeltReservationResult.cs`
- `src/domain/system/PlanetGenerationResult.cs`
- `src/domain/system/OrbitSlotGenerator.cs`
- `src/domain/system/StellarConfigGenerator.cs`
- `src/domain/system/SystemPlanetGenerator.cs`
- `src/domain/system/SystemMoonGenerator.cs`
- `src/domain/system/SystemAsteroidGenerator.cs`
- `src/domain/system/fixtures/SystemFixtureGenerator.cs`
- `src/domain/system/asteroid_belt/BeltFieldSpec.cs`
- `src/domain/system/asteroid_belt/BeltFieldData.cs`
- `src/domain/system/asteroid_belt/BeltAsteroidData.cs`
- `src/domain/system/asteroid_belt/BeltMajorAsteroidInput.cs`
- `src/domain/system/asteroid_belt/BeltOrbitalMath.cs`
- `src/domain/system/asteroid_belt/BeltFieldGenerator.cs`
- `src/domain/galaxy/GalaxySpec.cs`
- `src/domain/galaxy/GalaxyStar.cs`
- `src/domain/galaxy/GalaxyBodyOverrides.cs`
- `src/domain/galaxy/GalaxySystemGenerator.cs`
- `src/domain/galaxy/StableHash.cs`
- `src/domain/galaxy/SeedDeriver.cs`
- `src/domain/galaxy/HierarchyCoords.cs`
- `src/domain/galaxy/GalaxyCoordinates.cs`
- `src/domain/galaxy/GalaxyConfig.cs`
- `src/domain/galaxy/GalaxySample.cs`
- `src/domain/galaxy/DensityModelInterface.cs`
- `src/domain/galaxy/SpiralDensityModel.cs`
- `src/domain/galaxy/EllipticalDensityModel.cs`
- `src/domain/galaxy/IrregularDensityModel.cs`
- `src/domain/galaxy/DensitySampler.cs`
- `src/domain/galaxy/StarPickResult.cs`
- `src/domain/galaxy/StarPicker.cs`
- `src/domain/galaxy/SectorStarData.cs`
- `src/domain/galaxy/SubSectorGenerator.cs`
- `src/domain/galaxy/HomePosition.cs`
- `src/domain/galaxy/GridCursor.cs`
- `src/domain/galaxy/StarSystemPreviewData.cs`
- `src/domain/galaxy/StarSystemPreview.cs`
- `src/domain/galaxy/SubSectorNeighborhoodData.cs`
- `src/domain/galaxy/SubSectorNeighborhood.cs`
- `src/domain/galaxy/GalaxySaveData.cs`
- `src/domain/galaxy/RaycastUtils.cs`
- `src/domain/galaxy/Sector.cs`
- `src/domain/galaxy/Galaxy.cs`
- `src/services/persistence/CelestialPersistence.cs`
- `src/services/persistence/GalaxyPersistence.cs`
- `src/services/persistence/SaveDataLoadResult.cs`
- `src/services/persistence/SaveData.cs`
- `src/services/persistence/SystemPersistenceLoadResult.cs`
- `src/services/persistence/SystemPersistence.cs`
- `src/app/galaxy_viewer/QuadrantSelector.cs`
- `src/app/galaxy_viewer/ZoomStateMachine.cs`
- `src/app/galaxy_viewer/GalaxyInspectorPanel.cs`
- `src/app/galaxy_viewer/GalaxyRenderer.cs`
- `src/app/galaxy_viewer/GalaxyViewer.cs`
- `src/app/galaxy_viewer/GalaxyViewerCSharp.tscn`
- `src/app/galaxy_viewer/GalaxyViewerSaveLoad.cs`
- `src/app/galaxy_viewer/NeighborhoodRenderer.cs`
- `src/app/galaxy_viewer/NavigationCompass.cs`
- `src/app/galaxy_viewer/OrbitCamera.cs`
- `src/app/galaxy_viewer/QuadrantRenderer.cs`
- `src/app/galaxy_viewer/SectorJumpLaneRenderer.cs`
- `src/app/galaxy_viewer/SectorRenderer.cs`
- `src/app/galaxy_viewer/SelectionIndicator.cs`
- `src/app/galaxy_viewer/StarViewCamera.cs`
- `src/app/galaxy_viewer/SubSectorRenderer.cs`
- `src/app/rendering/ColorUtils.cs`
- `src/app/rendering/ShaderParamHelpers.cs`
- `src/app/rendering/StarShaderParams.cs`
- `src/app/rendering/AtmosphereShaderParams.cs`
- `src/app/rendering/GasGiantShaderParamProfiles.cs`
- `src/app/rendering/GasGiantShaderParams.cs`
- `src/app/rendering/MaterialFactory.cs`
- `src/app/rendering/BodyRenderer.cs`
- `src/app/rendering/RingShaderParams.cs`
- `src/app/rendering/TerrestrialShaderParamProfiles.cs`
- `src/app/rendering/TerrestrialShaderParams.cs`
- `src/app/system_viewer/SystemScaleManager.cs`
- `src/app/system_viewer/BodyLayout.cs`
- `src/app/system_viewer/BeltLayout.cs`
- `src/app/system_viewer/NodeExtent.cs`
- `src/app/system_viewer/SystemLayout.cs`
- `src/app/system_viewer/SystemDisplayLayout.cs`
- `src/app/system_viewer/SystemBodyNode.cs`
- `src/app/system_viewer/SystemCameraController.cs`
- `src/app/system_viewer/OrbitRenderer.cs`
- `src/app/system_viewer/BeltRenderer.cs`
- `src/app/system_viewer/SystemBodyNodeCSharp.tscn`
- `src/app/system_viewer/SystemInspectorPanel.cs`
- `src/app/system_viewer/SystemViewer.cs`
- `src/app/system_viewer/SystemViewerSaveLoad.cs`
- `src/app/system_viewer/SystemViewerCSharp.tscn`
- `src/app/viewer/CameraController.cs`
- `src/app/viewer/InspectorPanel.cs`
- `src/app/viewer/ObjectViewerMoonSystem.cs`
- `src/app/viewer/ObjectViewer.cs`
- `src/app/viewer/ObjectViewerCSharp.tscn`
- `src/app/viewer/PropertyFormatter.cs`
- `Tests/Framework/DotNetTestResult.cs`
- `Tests/Framework/DotNetTestRunner.cs`
- `Tests/Framework/DotNetNativeTestSuite.cs`
- `Tests/TestRegistry.gd`
- `Tests/TestSceneCSharp.cs`
- `Tests/TestSceneCSharp.tscn`

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
├── Concepts/                       # Visual concept demos (reference); prototypes folded into main are removed
│   ├── Additions.md
│   ├── CivilisationEngine/         # Civ + history: Tech Tree, Regime Chart, culture sim, regime transitions (React)
│   │   ├── index.html
│   │   ├── IntegrationApp.jsx
│   │   ├── LayoutHelpers.js
│   │   ├── data/
│   │   │   └── SharedData.js
│   │   └── logic/
│   │       └── Simulation.js
│   │
│   ├── EvoTechTree/                # Biology as evolutionary tech tree; lineage, evo simulator (React)
│   │   ├── index.html
│   │   ├── App.jsx                 # Data, NODES, ENVS, evolveStep, genSpecies
│   │   └── AppUI.jsx               # SpeciesDetail, DictModal, NodeCard, EvoMode, App + mount
│   │
│   ├── SpaceStationBuilder/        # Traveller station builder: extended ship rules, classification, export
│   │   ├── index.html
│   │   └── StationBuilder.jsx
│   │
│   └── DiseaseSimulator/           # Concept 9: pathogen evolution sim — SEIRDV, symptoms, comorbidities (deterministic RNG)
│       ├── index.html
│       └── DiseaseSimulator.jsx
│
├── Docs/
│   ├── CelestialBodyProperties.md
│   ├── GDD.md
│   ├── ProjectStructure.md        # This file
│   ├── RegimeChangeModel.md
│   ├── Roadmap.md
│   └── TravellerWorldCreation.md
│
├── Sources/                       # Scientific literature and bibliography
│   ├── AnnotatedBibliography.md
│   ├── SourceReviewProcedure.md   # How to add/review sources; abridgement rules; consistency check
│   ├── ToReview.md
│   └── Texts/                     # Full-text copies (AuthorYear.txt)
│       └── .gitkeep
│
├── src/
│   ├── app/
│   │   ├── MainApp.cs
│   │   ├── MainApp.gd
│   │   ├── MainApp.tscn
│   │   ├── WelcomeScreen.cs
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
│   │   │   ├── BeltRenderer.gd
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
│   │   ├── editing/
│   │   │   ├── ConstraintSet.gd
│   │   │   ├── EditRegenerator.gd
│   │   │   ├── EditSpecBuilder.gd
│   │   │   ├── PropertyConstraint.gd
│   │   │   ├── PropertyConstraintSolver.gd
│   │   │   └── TravellerConstraintBuilder.gd
│   │   │
│   │   ├── galaxy/
│   │   │   ├── DensityModelInterface.gd
│   │   │   ├── DensitySampler.gd
│   │   │   ├── EllipticalDensityModel.gd
│   │   │   ├── Galaxy.gd
│   │   │   ├── GalaxyConfig.gd
│   │   │   ├── GalaxyCoordinates.gd
│   │   │   ├── GalaxySample.gd
│   │   │   ├── GalaxyBodyOverrides.gd
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
│   │   │   │   ├── StarClass.gd
│   │   │   │   └── TravellerSizeCode.gd
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
│   │   │   ├── asteroid_belt/
│   │   │   │   ├── BeltAsteroidData.gd
│   │   │   │   ├── BeltFieldData.gd
│   │   │   │   ├── BeltFieldGenerator.gd
│   │   │   │   ├── BeltFieldSpec.gd
│   │   │   │   ├── BeltMajorAsteroidInput.gd
│   │   │   │   └── BeltOrbitalMath.gd
│   │   │   │
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
    │   ├── DotNetTestResult.cs
    │   ├── DotNetTestRunner.cs
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
    │   ├── TestBeltFieldGenerator.gd
    │   ├── TestBeltOrbitalMath.gd
    │   ├── TestCelestialBody.gd
    │   ├── TestCelestialSerializer.gd
    │   ├── TestCelestialValidator.gd
    │   ├── TestColorUtils.gd
    │   ├── TestConstraintSet.gd
    │   ├── TestEditRegenerator.gd
    │   ├── TestEditSpecBuilder.gd
    │   ├── TestPropertyConstraint.gd
    │   ├── TestPropertyConstraintSolver.gd
    │   ├── TestTravellerConstraintBuilder.gd
    │   ├── TestColorUtilsShaderParams.gd
    │   ├── TestGalaxy.gd
    │   ├── TestGalaxyConfig.gd
    │   ├── TestGalaxyInspectorPanel.gd
    │   ├── TestGalaxySaveData.gd
        │   ├── TestGalaxyBodyOverrides.gd
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
    │   ├── TestTravellerSizeCode.gd
    │   ├── TestUnits.gd
    │   ├── TestValidation.gd
    │   └── TestVersions.gd
    │
    ├── GenerationStatsHarness.gd
    ├── JumpLanesDeps.gd
    ├── JumpLanesTestRunner.gd
    ├── JumpLanesTestScene.gd
    ├── JumpLanesTestScene.tscn
    ├── Phase1Deps.gd
    ├── PopulationDeps.gd
    ├── RunTestsHeadless.gd
    ├── ScientificBenchmarks.gd
    ├── TestScene.gd
    ├── TestScene.tscn
    ├── TestSceneCSharp.cs
    └── TestSceneCSharp.tscn
```

## Layer summary

| Layer   | Path                    | Purpose                                      |
|---------|-------------------------|----------------------------------------------|
| Domain  | `src/domain/`           | Pure logic; no Nodes, SceneTree, or file I/O |
| Services| `src/services/`         | Persistence, I/O, caching                     |
| App     | `src/app/`              | Scenes, UI, input, rendering                  |
