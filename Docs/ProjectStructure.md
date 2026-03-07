# StarGen Project Structure

Complete enumeration of the project file structure. Excludes `.uid` files, `.git/`, `.godot/` (generated).

## C# Migration — Complete

The GDScript-to-C# migration of all `src/` source files is complete. No `.gd` files remain under `src/`; all source logic, scenes, and app scripts are now in C#. All `.tscn` scene files reference their corresponding `.cs` scripts. The `SolarSystem.gd` GDScript bridge has been removed; `SystemViewer` now uses the C# `SolarSystem` type directly.

Partial-class splits (large files broken into focused parts):
- `SystemViewer.cs` / `SystemViewer.Setup.cs` / `SystemViewer.Rendering.cs` / `SystemViewer.Interaction.cs` / `SystemViewer.GdCompat.cs`
- `GalaxyViewer.cs` / `GalaxyViewer.Setup.cs` / `GalaxyViewer.Navigation.cs` / `GalaxyViewer.Selection.cs` / `GalaxyViewer.Accessors.cs`
- `MainApp.cs` / `MainApp.Navigation.cs`
- `ObjectViewer.cs` / `ObjectViewer.Display.cs` / `ObjectViewer.SaveLoad.cs`

Recent 0.4.0 additions:
- `Docs/Release-0.4.0-MVP.md`
- `src/app/shared/ViewerLayoutHelper.cs`
- `src/app/system_viewer/SystemViewer.Parameters.cs`
- `src/domain/generation/parameters/` (shared parameter definitions and validators)
- `Tests/Integration/TestGenerationParameters.cs`
- `Tests/Integration/TestViewerLayoutHelper.cs`

Prototype files consolidated under `src/app/prototypes/`:
- `JumpLanesPrototype.cs`, `JumpLaneRenderer.cs`, `MockRegionGenerator.cs` (moved from `src/app/jumplanes_prototype/`)
- `StationGeneratorPrototype.cs`

C# source files:
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
- `src/domain/generation/fixtures/FixtureGenerator.cs`
- `src/domain/generation/parameters/GenerationParameterControlType.cs`
- `src/domain/generation/parameters/GenerationParameterDefinition.cs`
- `src/domain/generation/parameters/GenerationParameterIssue.cs`
- `src/domain/generation/parameters/GenerationParameterIssueSet.cs`
- `src/domain/generation/parameters/GenerationParameterCatalog.cs`
- `src/domain/generation/parameters/SystemGenerationParameterValidator.cs`
- `src/domain/generation/parameters/GalaxyGenerationParameterValidator.cs`
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
- `src/domain/editing/PropertyConstraint.cs`
- `src/domain/editing/ConstraintSet.cs`
- `src/domain/editing/EditSpecBuilder.cs`
- `src/domain/editing/PropertyConstraintSolver.cs`
- `src/domain/editing/TravellerConstraintBuilder.cs`
- `src/domain/editing/EditRegenerator.cs`
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
- `src/services/persistence/PersistenceUtils.cs`
- `src/services/persistence/SaveData.cs`
- `src/services/persistence/SaveData.Generators.cs`
- `src/services/persistence/SaveDataLoadResult.cs`
- `src/services/persistence/SystemPersistenceLoadResult.cs`
- `src/services/persistence/SystemPersistence.cs`
- `src/app/galaxy_viewer/QuadrantSelector.cs`
- `src/app/galaxy_viewer/ZoomStateMachine.cs`
- `src/app/galaxy_viewer/GalaxyInspectorPanel.cs`
- `src/app/galaxy_viewer/GalaxyRenderer.cs`
- `src/app/galaxy_viewer/GalaxyViewer.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Setup.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Navigation.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Selection.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Accessors.cs`
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
- `src/app/system_viewer/SystemViewer.Parameters.cs`
- `src/app/system_viewer/SystemViewer.Setup.cs`
- `src/app/system_viewer/SystemViewer.Rendering.cs`
- `src/app/system_viewer/SystemViewer.Interaction.cs`
- `src/app/system_viewer/SystemViewer.GdCompat.cs`
- `src/app/system_viewer/SystemViewerSaveLoad.cs`
- `src/app/viewer/CameraController.cs`
- `src/app/viewer/InspectorPanel.cs`
- `src/app/viewer/ObjectViewerMoonSystem.cs`
- `src/app/viewer/ObjectViewer.cs`
- `src/app/viewer/ObjectViewer.Display.cs`
- `src/app/viewer/ObjectViewer.SaveLoad.cs`
- `src/app/viewer/PropertyFormatter.cs`
- `src/app/viewer/EditDialog.cs`
- `src/app/shared/ViewerLayoutHelper.cs`
- `src/app/components/CollapsibleSection.cs`
- `src/app/MainApp.cs`
- `src/app/MainApp.Navigation.cs`
- `src/app/SplashScreen.cs`
- `src/app/MainMenuScreen.cs`
- `src/app/WelcomeScreen.cs`
- `src/app/prototypes/JumpLanesPrototype.cs`
- `src/app/prototypes/JumpLaneRenderer.cs`
- `src/app/prototypes/MockRegionGenerator.cs`
- `src/app/prototypes/StationGeneratorPrototype.cs`
- `Tests/Framework/DotNetTestResult.cs`
- `Tests/Framework/DotNetTestRunner.cs`
- `Tests/Framework/DotNetNativeTestSuite.cs`
- `Tests/TestRegistry.cs`
- `Tests/TestRegistry.gd`
- `Tests/TestSceneCSharp.cs`
- `Tests/TestSceneCSharp.tscn`

> **Note:** The tree below reflects the current state after the C# migration. All `.gd` source files have been removed from `src/`. Under `Tests/`, unit and integration tests are C# (`Test*.cs` in Unit/, Integration/, domain/, Quality/); only `RunTestsHeadless.gd` and `TestScene.gd` remain as GDScript launchers that boot the .NET harness. `TestRegistry.cs` is the maintained suite manifest.

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
│   │
│   └── ReligionGenerator/          # Concept 16: procedural religion — deity, cosmology, ritual, landscape (deterministic seeded C#)
│       ├── ReligionGenerator.csproj
│       ├── ReligionParams.cs
│       ├── ReligionResult.cs
│       ├── ReligionRng.cs
│       ├── ReligionGenerator.cs
│       └── README.md
│   │
│   └── ConlangGenerator/            # Concept 18: conlang — phonology, grammar, concept lexicon, inflection (Mulberry32 seeded React)
│       ├── index.html
│       └── ConlangGenerator.jsx
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
│   │   ├── MainApp.Navigation.cs
│   │   ├── MainApp.tscn
│   │   ├── SplashScreen.cs
│   │   ├── SplashScreen.tscn
│   │   ├── MainMenuScreen.cs
│   │   ├── MainMenuScreen.tscn
│   │   ├── WelcomeScreen.cs
│   │   ├── WelcomeScreen.tscn
│   │   │
│   │   ├── components/
│   │   │   ├── CollapsibleSection.cs
│   │   │   └── CollapsibleSection.tscn
│   │   │
│   │   ├── galaxy_viewer/
│   │   │   ├── GalaxyInspectorPanel.cs
│   │   │   ├── GalaxyRenderer.cs
│   │   │   ├── GalaxyViewer.cs
│   │   │   ├── GalaxyViewer.Setup.cs
│   │   │   ├── GalaxyViewer.Navigation.cs
│   │   │   ├── GalaxyViewer.Selection.cs
│   │   │   ├── GalaxyViewer.Accessors.cs
│   │   │   ├── GalaxyViewer.tscn
│   │   │   ├── GalaxyViewerCSharp.tscn
│   │   │   ├── GalaxyViewerSaveLoad.cs
│   │   │   ├── NavigationCompass.cs
│   │   │   ├── NeighborhoodRenderer.cs
│   │   │   ├── OrbitCamera.cs
│   │   │   ├── QuadrantRenderer.cs
│   │   │   ├── QuadrantSelector.cs
│   │   │   ├── SectorJumpLaneRenderer.cs
│   │   │   ├── SectorRenderer.cs
│   │   │   ├── SelectionIndicator.cs
│   │   │   ├── StarViewCamera.cs
│   │   │   ├── SubSectorRenderer.cs
│   │   │   ├── ZoomStateMachine.cs
│   │   │   └── shaders/
│   │   │       ├── quadrant_cell.gdshader
│   │   │       ├── sector_cell.gdshader
│   │   │       ├── selection_ring.gdshader
│   │   │       ├── star_billboard.gdshader
│   │   │       ├── star_sector_view.gdshader
│   │   │       └── subsector_wire.gdshader
│   │   │
│   │   ├── prototypes/
│   │   │   ├── JumpLaneRenderer.cs
│   │   │   ├── JumpLanesPrototype.cs
│   │   │   ├── JumpLanesPrototype.tscn
│   │   │   ├── MockRegionGenerator.cs
│   │   │   ├── StationGeneratorPrototype.cs
│   │   │   └── StationGeneratorPrototype.tscn
│   │   │
│   │   ├── rendering/
│   │   │   ├── AtmosphereShaderParams.cs
│   │   │   ├── BodyRenderer.cs
│   │   │   ├── BodyRenderer.tscn
│   │   │   ├── ColorUtils.cs
│   │   │   ├── GasGiantShaderParams.cs
│   │   │   ├── GasGiantShaderParamProfiles.cs
│   │   │   ├── MaterialFactory.cs
│   │   │   ├── RingShaderParams.cs
│   │   │   ├── ShaderParamHelpers.cs
│   │   │   ├── StarShaderParams.cs
│   │   │   ├── TerrestrialShaderParams.cs
│   │   │   ├── TerrestrialShaderParamProfiles.cs
│   │   │   ├── shaders/
│   │   │   │   ├── atmosphere_rim.gdshader
│   │   │   │   ├── noise_lib.gdshaderinc
│   │   │   │   ├── planet_gas_giant_surface.gdshader
│   │   │   │   ├── planet_terrestrial_surface.gdshader
│   │   │   │   ├── ring_system.gdshader
│   │   │   │   ├── star_atmosphere.gdshader
│   │   │   │   └── star_surface.gdshader
│   │   │   └── textures/
│   │   │       └── noise.tres
│   │   │
│   │   ├── system_viewer/
│   │   │   ├── BeltRenderer.cs
│   │   │   ├── BeltLayout.cs
│   │   │   ├── BodyLayout.cs
│   │   │   ├── NodeExtent.cs
│   │   │   ├── OrbitRenderer.cs
│   │   │   ├── SystemBodyNode.cs
│   │   │   ├── SystemBodyNode.tscn
│   │   │   ├── SystemBodyNodeCSharp.tscn
│   │   │   ├── SystemCameraController.cs
│   │   │   ├── SystemDisplayLayout.cs
│   │   │   ├── SystemInspectorPanel.cs
│   │   │   ├── SystemLayout.cs
│   │   │   ├── SystemScaleManager.cs
│   │   │   ├── SystemViewer.cs
│   │   │   ├── SystemViewer.Setup.cs
│   │   │   ├── SystemViewer.Rendering.cs
│   │   │   ├── SystemViewer.Interaction.cs
│   │   │   ├── SystemViewer.GdCompat.cs
│   │   │   ├── SystemViewerSaveLoad.cs
│   │   │   └── SystemViewer.tscn
│   │   │
│   │   ├── themes/
│   │   │   └── DarkTheme.tres
│   │   │
│   │   └── viewer/
│   │       ├── CameraController.cs
│   │       ├── EditDialog.cs
│   │       ├── EditDialog.tscn
│   │       ├── InspectorPanel.cs
│   │       ├── ObjectViewer.cs
│   │       ├── ObjectViewer.Display.cs
│   │       ├── ObjectViewer.SaveLoad.cs
│   │       ├── ObjectViewer.tscn
│   │       ├── ObjectViewerMoonSystem.cs
│   │       └── PropertyFormatter.cs
│   │
│   ├── domain/
│   │   ├── bootstrap/
│   │   │   ├── CSharpCelestialTypeBridge.cs
│   │   │   ├── CSharpOrbitTableBridge.cs
│   │   │   ├── CSharpSizeTableBridge.cs
│   │   │   ├── CSharpSmokeTest.cs
│   │   │   └── CSharpStarTableBridge.cs
│   │   │
│   │   ├── celestial/
│   │   │   ├── CelestialBody.cs
│   │   │   ├── CelestialType.cs
│   │   │   ├── Provenance.cs
│   │   │   ├── components/
│   │   │   │   ├── AtmosphereProps.cs
│   │   │   │   ├── CryosphereProps.cs
│   │   │   │   ├── HydrosphereProps.cs
│   │   │   │   ├── OrbitalProps.cs
│   │   │   │   ├── PhysicalProps.cs
│   │   │   │   ├── RingBand.cs
│   │   │   │   ├── RingSystemProps.cs
│   │   │   │   ├── StellarProps.cs
│   │   │   │   ├── SurfaceProps.cs
│   │   │   │   └── TerrainProps.cs
│   │   │   ├── serialization/
│   │   │   │   ├── CelestialSerializer.cs
│   │   │   │   └── SerializedPopulationData.cs
│   │   │   └── validation/
│   │   │       ├── CelestialValidator.cs
│   │   │       ├── ValidationError.cs
│   │   │       └── ValidationResult.cs
│   │   │
│   │   ├── constants/
│   │   │   └── Versions.cs
│   │   │
│   │   ├── editing/
│   │   │   ├── ConstraintSet.cs
│   │   │   ├── EditRegenerator.cs
│   │   │   ├── EditSpecBuilder.cs
│   │   │   ├── PropertyConstraint.cs
│   │   │   ├── PropertyConstraintSolver.cs
│   │   │   ├── RegenerateResult.cs
│   │   │   └── TravellerConstraintBuilder.cs
│   │   │
│   │   ├── galaxy/
│   │   │   ├── DensityModelInterface.cs
│   │   │   ├── DensitySampler.cs
│   │   │   ├── EllipticalDensityModel.cs
│   │   │   ├── Galaxy.cs
│   │   │   ├── GalaxyBodyOverrides.cs
│   │   │   ├── GalaxyConfig.cs
│   │   │   ├── GalaxyCoordinates.cs
│   │   │   ├── GalaxySample.cs
│   │   │   ├── GalaxySaveData.cs
│   │   │   ├── GalaxySpec.cs
│   │   │   ├── GalaxyStar.cs
│   │   │   ├── GalaxySystemGenerator.cs
│   │   │   ├── GridCursor.cs
│   │   │   ├── HierarchyCoords.cs
│   │   │   ├── HomePosition.cs
│   │   │   ├── IrregularDensityModel.cs
│   │   │   ├── RaycastUtils.cs
│   │   │   ├── Sector.cs
│   │   │   ├── SectorStarData.cs
│   │   │   ├── SeedDeriver.cs
│   │   │   ├── SpiralDensityModel.cs
│   │   │   ├── StableHash.cs
│   │   │   ├── StarPickResult.cs
│   │   │   ├── StarPicker.cs
│   │   │   ├── StarSystemPreview.cs
│   │   │   ├── StarSystemPreviewData.cs
│   │   │   ├── SubSectorGenerator.cs
│   │   │   ├── SubSectorNeighborhood.cs
│   │   │   └── SubSectorNeighborhoodData.cs
│   │   │
│   │   ├── generation/
│   │   │   ├── GenerationRealismProfile.cs
│   │   │   ├── ParentContext.cs
│   │   │   ├── archetypes/
│   │   │   │   ├── AsteroidType.cs
│   │   │   │   ├── OrbitZone.cs
│   │   │   │   ├── RingComplexity.cs
│   │   │   │   ├── SizeCategory.cs
│   │   │   │   ├── StarClass.cs
│   │   │   │   └── TravellerSizeCode.cs
│   │   │   ├── fixtures/
│   │   │   │   └── FixtureGenerator.cs
│   │   │   ├── generators/
│   │   │   │   ├── AsteroidGenerator.cs
│   │   │   │   ├── GeneratorUtils.cs
│   │   │   │   ├── MoonGenerator.cs
│   │   │   │   ├── PlanetGenerator.cs
│   │   │   │   ├── RingSystemGenerator.cs
│   │   │   │   └── StarGenerator.cs
│   │   │   ├── specs/
│   │   │   │   ├── AsteroidSpec.cs
│   │   │   │   ├── BaseSpec.cs
│   │   │   │   ├── MoonSpec.cs
│   │   │   │   ├── PlanetSpec.cs
│   │   │   │   ├── RingSystemSpec.cs
│   │   │   │   └── StarSpec.cs
│   │   │   ├── tables/
│   │   │   │   ├── OrbitTable.cs
│   │   │   │   ├── SizeTable.cs
│   │   │   │   └── StarTable.cs
│   │   │   └── utils/
│   │   │       └── AtmosphereUtils.cs
│   │   │
│   │   ├── jumplanes/
│   │   │   ├── JumpLaneCalculator.cs
│   │   │   ├── JumpLaneClusterConnector.cs
│   │   │   ├── JumpLaneConnection.cs
│   │   │   ├── JumpLaneRegion.cs
│   │   │   ├── JumpLaneResult.cs
│   │   │   └── JumpLaneSystem.cs
│   │   │
│   │   ├── math/
│   │   │   ├── MathUtils.cs
│   │   │   └── Units.cs
│   │   │
│   │   ├── population/
│   │   │   ├── BiomeType.cs
│   │   │   ├── ClimateZone.cs
│   │   │   ├── Colony.cs
│   │   │   ├── ColonyGenerator.cs
│   │   │   ├── ColonySuitability.cs
│   │   │   ├── ColonyType.cs
│   │   │   ├── Government.cs
│   │   │   ├── GovernmentType.cs
│   │   │   ├── HabitabilityCategory.cs
│   │   │   ├── HistoryEvent.cs
│   │   │   ├── HistoryGenerator.cs
│   │   │   ├── NativePopulation.cs
│   │   │   ├── NativePopulationGenerator.cs
│   │   │   ├── NativeRelation.cs
│   │   │   ├── Outpost.cs
│   │   │   ├── OutpostAuthority.cs
│   │   │   ├── PlanetPopulationData.cs
│   │   │   ├── PlanetProfile.cs
│   │   │   ├── PopulationGenerator.cs
│   │   │   ├── PopulationHistory.cs
│   │   │   ├── PopulationLikelihood.cs
│   │   │   ├── PopulationProbability.cs
│   │   │   ├── PopulationSeeding.cs
│   │   │   ├── ProfileCalculations.cs
│   │   │   ├── ProfileGenerator.cs
│   │   │   ├── ResourceType.cs
│   │   │   ├── SpaceStation.cs
│   │   │   ├── StationClass.cs
│   │   │   ├── StationGenerationResult.cs
│   │   │   ├── StationGenerator.cs
│   │   │   ├── StationPlacementContext.cs
│   │   │   ├── StationPlacementRecommendation.cs
│   │   │   ├── StationPlacementRules.cs
│   │   │   ├── StationPurpose.cs
│   │   │   ├── StationService.cs
│   │   │   ├── StationSpec.cs
│   │   │   ├── StationSystemContext.cs
│   │   │   ├── StationType.cs
│   │   │   ├── SuitabilityCalculator.cs
│   │   │   └── TechnologyLevel.cs
│   │   │
│   │   ├── rng/
│   │   │   └── SeededRng.cs
│   │   │
│   │   ├── system/
│   │   │   ├── AsteroidBelt.cs
│   │   │   ├── BeltGenerationResult.cs
│   │   │   ├── BeltReservationResult.cs
│   │   │   ├── HierarchyNode.cs
│   │   │   ├── MoonGenerationResult.cs
│   │   │   ├── OrbitalMechanics.cs
│   │   │   ├── OrbitHost.cs
│   │   │   ├── OrbitSlot.cs
│   │   │   ├── OrbitSlotGenerationResult.cs
│   │   │   ├── OrbitSlotGenerator.cs
│   │   │   ├── PlanetGenerationResult.cs
│   │   │   ├── SolarSystem.cs
│   │   │   ├── SolarSystemSpec.cs
│   │   │   ├── StellarConfigGenerator.cs
│   │   │   ├── SystemAsteroidGenerator.cs
│   │   │   ├── SystemCache.cs
│   │   │   ├── SystemHierarchy.cs
│   │   │   ├── SystemMoonGenerator.cs
│   │   │   ├── SystemPlanetGenerator.cs
│   │   │   ├── SystemSerializer.cs
│   │   │   ├── SystemValidator.cs
│   │   │   ├── asteroid_belt/
│   │   │   │   ├── BeltAsteroidData.cs
│   │   │   │   ├── BeltFieldData.cs
│   │   │   │   ├── BeltFieldGenerator.cs
│   │   │   │   ├── BeltFieldSpec.cs
│   │   │   │   ├── BeltMajorAsteroidInput.cs
│   │   │   │   └── BeltOrbitalMath.cs
│   │   │   └── fixtures/
│   │   │       └── SystemFixtureGenerator.cs
│   │   │
│   │   └── validation/
│   │       └── Validation.cs
│   │
│   └── services/
│       └── persistence/
│           ├── CelestialPersistence.cs
│           ├── GalaxyPersistence.cs
│           ├── PersistenceUtils.cs
│           ├── SaveData.cs
│           ├── SaveData.Generators.cs
│           ├── SaveDataLoadResult.cs
│           ├── SystemPersistence.cs
│           └── SystemPersistenceLoadResult.cs
│
└── Tests/
    ├── Framework/
    │   ├── DotNetNativeTestSuite.cs        # base class for C# test suites
    │   ├── DotNetNativeTestSuite.App.cs
    │   ├── DotNetNativeTestSuite.Galaxy.cs
    │   ├── DotNetNativeTestSuite.Generation.cs
    │   ├── DotNetNativeTestSuite.Helpers.cs
    │   ├── DotNetNativeTestSuite.Integration.cs
    │   ├── DotNetNativeTestSuite.JumpLanes.cs
    │   ├── DotNetNativeTestSuite.Population.cs
    │   ├── DotNetNativeTestSuite.System.cs
    │   ├── DotNetTestResult.cs
    │   ├── DotNetTestRunner.cs
    │   └── GodotDictionaryCompatExtensions.cs
    │
    ├── Integration/
    │   ├── IntegrationTestUtils.cs
    │   ├── TestCelestialPersistence.cs
    │   ├── TestGalaxyPersistence.cs
    │   ├── TestGalaxyRandomization.cs
    │   ├── TestGalaxyStartup.cs
    │   ├── TestGalaxySystemTransition.cs
    │   ├── TestGalaxyViewerHome.cs
    │   ├── TestGalaxyViewerUI.cs
    │   ├── TestMainApp.cs
    │   ├── TestMainAppNavigation.cs
    │   ├── TestObjectViewer.cs
    │   ├── TestObjectViewerMoons.cs
    │   ├── TestPopulationGoldenMasters.cs
    │   ├── TestPopulationIntegration.cs
    │   ├── TestSaveLoad.cs
    │   ├── TestStarSystemPreviewIntegration.cs
    │   ├── TestSystemCameraController.cs
    │   ├── TestSystemPersistence.cs
    │   ├── TestSystemViewer.cs
    │   ├── TestSystemViewerSaveLoad.cs
    │   └── TestWelcomeScreen.cs
    │
    ├── Quality/
    │   └── TestSuiteIntegrity.cs
    │
    ├── Unit/
    │   ├── JumpLanes/
    │   │   ├── TestJumpLaneCalculator.cs
    │   │   ├── TestJumpLaneClusterConnector.cs
    │   │   ├── TestJumpLaneConnection.cs
    │   │   ├── TestJumpLaneRegion.cs
    │   │   ├── TestJumpLaneResult.cs
    │   │   └── TestJumpLaneSystem.cs
    │   │
    │   ├── Population/
    │   │   └── (Test*.cs for population types)
    │   │
    │   └── (Test*.cs for domain, generation, system, app helpers)
    │
    ├── domain/
    │   └── galaxy/
    │       └── (Test*.cs for galaxy domain)
    │
    ├── GlobalUsings.cs
    ├── GenerationStatsHarness.cs
    ├── JumpLanesTestScene.cs
    ├── JumpLanesTestScene.tscn
    ├── RunTestsHeadless.cs
    ├── RunTestsHeadless.gd                 # launcher only; boots C# harness
    ├── ScientificBenchmarks.cs
    ├── TestRegistry.cs                     # maintained suite manifest
    ├── TestRegistry.gd                    # reference copy
    ├── TestScene.cs
    ├── TestScene.gd                       # launcher only; boots C# harness
    ├── TestScene.tscn
    └── TestSceneCSharp.tscn
```

## Save system (in-app)

Save/load is driven by the app layer; persistence services handle format and I/O. **File extension is significant:** it selects format (compressed binary vs JSON) on both save and load.

| Scope | Service | Extensions | App entry points |
|-------|---------|------------|-------------------|
| Single body (star/planet/moon/asteroid) | `SaveData` | `.sgt` star, `.sgp` planet/moon, `.sga` asteroid, `.sgb` legacy body, `.json` | ObjectViewer Save/Load; EditDialog "Save As…" |
| Solar system | `SystemPersistence` | `.sgs` compressed, `.json` | SystemViewer Save/Load (Ctrl+S) |
| Galaxy + viewer state | `GalaxyPersistence` | `.sgg` binary, `.json` | GalaxyViewer Save/Load |

Body and system saves support **Compact** (seed + spec, regenerate on load) or **Full** (full serialization). Edited bodies use Full via `SaveData.SaveEditedBody`. See **Save format and compatibility** in Docs/Roadmap.md for ZSTD/binary roadmap.

## Layer summary

| Layer   | Path                    | Purpose                                      |
|---------|-------------------------|----------------------------------------------|
| Domain  | `src/domain/`           | Pure logic; no Nodes, SceneTree, or file I/O |
| Services| `src/services/`         | Persistence, I/O, caching                     |
| App     | `src/app/`              | Scenes, UI, input, rendering                  |
