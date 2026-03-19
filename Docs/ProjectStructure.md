# StarGen Project Structure

Complete enumeration of the project file structure. Excludes `.uid` files, `.git/`, `.godot/` (generated).

## C# Migration — Complete

The GDScript-to-C# migration of all `src/` source files is complete. No `.gd` files remain under `src/`; all source logic, scenes, and app scripts are now in C#. All `.tscn` scene files reference their corresponding `.cs` scripts. The `SolarSystem.gd` GDScript bridge has been removed; `SystemViewer` now uses the C# `SolarSystem` type directly.

Partial-class splits (large files broken into focused parts):
- `SystemViewer.cs` / `SystemViewer.Setup.cs` / `SystemViewer.Rendering.cs` / `SystemViewer.Interaction.cs` / `SystemViewer.GdCompat.cs`
- `GalaxyViewer.cs` / `GalaxyViewer.Setup.cs` / `GalaxyViewer.Navigation.cs` / `GalaxyViewer.Selection.cs` / `GalaxyViewer.Accessors.cs`
- `MainApp.cs` / `MainApp.Navigation.cs`
- `ObjectViewer.cs` / `ObjectViewer.Display.cs` / `ObjectViewer.Parameters.cs` / `ObjectViewer.SaveLoad.cs`

UI layout baseline:
- The supported minimum app width is `640 px` (`480 px` height floor for windowed mode).
- Wrapped labels should have a panel-appropriate minimum width rather than relying on autowrap alone; large full-width headers can be broader, while sidebar/footer text should usually stay closer to the 180-320 px range.

Current public release target:
- `0.7.0.0`

Recent 0.7.0 additions:
- `VERSION.md`, `README.md`, `project.godot`, and `src/app/MainMenuScreen.cs` (Release 2 concept-integration milestone sync)
- `src/app/concepts/ConceptAtlasScreen.cs` (post-review atlas presentation polish: development note, scroll-safe sidebar behavior, and clearer showcase-facing framing)
- `src/app/system_viewer/SystemInspectorPanel.cs` / `src/app/system_viewer/SystemViewer*.cs` (post-review populated-world quick focus and standalone-atlas rollback)

Recent 0.4.0 additions:
- `Docs/Release-0.4.0-MVP.md`
- `src/app/shared/ViewerLayoutHelper.cs`
- `src/app/ObjectGenerationRequest.cs`
- `src/app/SystemGenerationScreen.cs`
- `src/app/SystemGenerationScreen.tscn`
- `src/app/ObjectGenerationScreen.cs`
- `src/app/ObjectGenerationScreen.EnhancedUi.cs`
- `src/app/ObjectGenerationScreen.Specs.cs`
- `src/app/ObjectGenerationScreen.tscn`
- `src/app/viewer/ObjectViewer.Menu.cs`
- `src/app/system_viewer/SystemViewer.Menu.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Menu.cs`
- `src/app/system_viewer/SystemViewer.Parameters.cs`
- `src/app/viewer/ObjectViewer.Parameters.cs`
- `src/domain/generation/GenerationUseCaseSettings.cs`
- `src/domain/system/TravellerMainworldSelector.cs`
- `src/domain/generation/parameters/` (shared parameter definitions and validators)
- `src/services/persistence/WindowSettingsService.cs`
- `Tests/Integration/TestGenerationParameters.cs`
- `Tests/Integration/TestViewerLayoutHelper.cs`
- `Tests/Integration/TestWindowSettingsService.cs`

Prototype files consolidated under `src/app/prototypes/`:
- `JumpLanesPrototype.cs`, `JumpLaneRenderer.cs`, `MockRegionGenerator.cs` (moved from `src/app/jumplanes_prototype/`)

Recent 0.4.2 additions:
- `src/domain/population/station_design/` (detailed station design calculators, mapping, serialization, and result types)
- `src/domain/population/station_design/classification/` (classification ids, contexts, evaluators, reports)
- `src/domain/population/station_design/presets/` (catalog, scaling rules, weapon allocation)
- `src/services/export/StationStatBlockExporter.cs`
- `Tests/Fixtures/StationDesign/`
- `Tests/Integration/TestStationDesignIntegration.cs`
- `Tests/Unit/Population/StationDesign/` (preset, calculator, serialization, regression, and classification coverage)

Recent 0.4.3 additions:
- `src/app/ObjectGenerationScreen.EnhancedUi.cs`
- `src/app/ObjectGenerationScreen.Specs.cs`
- `src/app/shared/StudioScreenLayoutHelper.cs`
- `src/domain/generation/traveller/` (Traveller world profile model, generator, and UWP helpers)
- `Tests/Integration/TestObjectGenerationStudio.cs`
- `Tests/Integration/TestStudioScreenLayoutHelper.cs`
- `Tests/Unit/TestTravellerWorldGenerator.cs`

Recent 0.5 release rollup highlights:
- `AI-Use-Statement.md`
- `AI-Provenance-Log.md`
- `src/app/StationStudioScreen.cs`
- `src/app/StationStudioScreen.tscn`
- `src/domain/population/station_design/`
- `src/services/export/StationStatBlockExporter.cs`

Recent 0.5.1 additions:
- `src/app/concepts/ConceptAtlasScreen.cs`
- `src/app/concepts/ConceptAtlasScreen.tscn`
- `src/app/concepts/ConceptAtlasModuleRegistry.cs`
- `src/domain/concepts/` (shared concept kinds, context, provenance, and atlas result types)
- `src/services/concepts/ConceptContextBuilder.cs`
- `Concepts/Additions.md` (active fold-in annotation for the selected concept prototypes)

Recent 0.5.2 additions:
- `src/app/concepts/EcologyAtlasModulePresenter.cs`
- `src/domain/concepts/ecology/EcologyConceptSnapshot.cs`
- `Tests/Framework/DotNetNativeTestSuite.Concepts.cs`

Recent 0.5.3 additions:
- `src/app/concepts/ReligionAtlasModulePresenter.cs`
- `src/domain/concepts/religion/ReligionConceptSnapshot.cs`
- `Concepts/ReligionGenerator/ReligionGenerator.cs`
- `Concepts/ReligionGenerator/ReligionParams.cs`
- `Concepts/ReligionGenerator/ReligionResult.cs`
- `Concepts/ReligionGenerator/ReligionRng.cs`

Recent 0.5.4 additions:
- `src/app/concepts/CivilizationAtlasModulePresenter.cs`
- `src/domain/concepts/civilization/CivilizationConceptGenerator.cs`
- `src/domain/concepts/civilization/CivilizationConceptSnapshot.cs`

Recent 0.5.5 additions:
- `src/app/concepts/LanguageAtlasModulePresenter.cs`
- `src/domain/concepts/language/LanguageConceptGenerator.cs`
- `src/domain/concepts/language/LanguageConceptSnapshot.cs`

Recent 0.5.6 additions:
- `src/app/concepts/DiseaseAtlasModulePresenter.cs`
- `src/domain/concepts/disease/DiseaseConceptGenerator.cs`
- `src/domain/concepts/disease/DiseaseConceptSnapshot.cs`

Recent 0.5.7 additions:
- `src/app/concepts/EvolutionAtlasModulePresenter.cs`
- `src/domain/concepts/evolution/EvolutionConceptGenerator.cs`
- `src/domain/concepts/evolution/EvolutionConceptSnapshot.cs`

Recent 0.5.8 additions:
- `src/app/MainApp.cs` / `src/app/MainApp.Navigation.cs` / `src/app/MainApp.GdCompat.cs` (context-aware Concept Atlas routing and return-to-origin navigation)
- `src/app/galaxy_viewer/GalaxyInspectorPanel.cs`
- `src/app/galaxy_viewer/GalaxyViewer.cs`
- `src/app/system_viewer/SystemInspectorPanel.cs`
- `src/app/system_viewer/SystemViewer.cs`
- `src/app/viewer/InspectorPanel.cs`
- `src/app/viewer/ObjectViewer.cs`
- `Tests/Integration/TestMainAppNavigation.cs`
- `Tests/Integration/TestGalaxyViewerUI.cs`
- `Tests/Integration/TestSystemViewer.cs`
- `Tests/Integration/TestObjectViewer.cs`

Recent 0.6.0 additions:
- `Docs/Roadmap.md` (Release 1 showcase status for the Concept Atlas effort)
- `Concepts/Additions.md` (selected-concept fold-in status updated to note live in-app replacements)
- `src/app/MainMenuScreen.cs` (showcase-facing release-note and help-text sync for the complete Release 1 atlas surface)

Recent 0.6.1 additions:
- `src/domain/concepts/ConceptResultStore.cs`
- `src/domain/concepts/ConceptRunResultSerialization.cs`
- `src/services/concepts/ConceptResultFactory.cs`
- `src/services/concepts/ConceptResultFactory.EcologyReligion.cs`
- `src/services/concepts/ConceptResultFactory.Society.cs`
- `src/services/concepts/ConceptWorldStateGenerator.cs`
- `src/app/system_viewer/SystemInspectorPanel.cs` / `src/app/viewer/InspectorPanel.cs` / `src/app/galaxy_viewer/GalaxyInspectorPanel.cs` (concept launch points and future integration scaffolding; automatic persisted-state hooks are currently disabled on the showcase branch)
- `Tests/Framework/DotNetNativeTestSuite.Concepts.cs`
- `Tests/Integration/TestSystemPersistence.cs`

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
- `src/domain/concepts/ConceptResultStore.cs`
- `src/domain/concepts/ConceptRunResultSerialization.cs`
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
- `src/domain/generation/GenerationUseCaseSettings.cs`
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
- `src/domain/generation/traveller/TravellerWorldProfile.cs`
- `src/domain/generation/traveller/TravellerWorldGenerator.cs`
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
- `src/domain/system/SolarSystem.Serialization.cs`
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
- `src/domain/system/TravellerMainworldSelector.cs`
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
- `src/services/concepts/ConceptContextBuilder.cs`
- `src/services/concepts/ConceptResultFactory.cs`
- `src/services/concepts/ConceptResultFactory.EcologyReligion.cs`
- `src/services/concepts/ConceptResultFactory.Society.cs`
- `src/services/concepts/ConceptWorldStateGenerator.cs`
- `src/services/persistence/CelestialPersistence.cs`
- `src/services/persistence/GalaxyPersistence.cs`
- `src/services/persistence/PersistenceUtils.cs`
- `src/services/persistence/SaveData.cs`
- `src/services/persistence/SaveData.Generators.cs`
- `src/services/persistence/SaveDataLoadResult.cs`
- `src/services/persistence/SystemPersistenceLoadResult.cs`
- `src/services/persistence/SystemPersistence.cs`
- `src/services/persistence/WindowSettingsService.cs`
- `src/app/galaxy_viewer/QuadrantSelector.cs`
- `src/app/galaxy_viewer/ZoomStateMachine.cs`
- `src/app/galaxy_viewer/GalaxyInspectorPanel.cs`
- `src/app/galaxy_viewer/GalaxyRenderer.cs`
- `src/app/galaxy_viewer/GalaxyViewer.cs`
- `src/app/galaxy_viewer/GalaxyViewer.Menu.cs`
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
- `src/app/system_viewer/SystemViewer.Menu.cs`
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
- `src/app/viewer/ObjectViewer.Menu.cs`
- `src/app/viewer/ObjectViewer.Parameters.cs`
- `src/app/viewer/ObjectViewer.SaveLoad.cs`
- `src/app/viewer/PropertyFormatter.cs`
- `src/app/viewer/EditDialog.cs`
- `src/app/shared/ViewerLayoutHelper.cs`
- `src/app/shared/StudioScreenLayoutHelper.cs`
- `src/app/concepts/ConceptAtlasScreen.cs`
- `src/app/concepts/ConceptAtlasModuleRegistry.cs`
- `src/app/concepts/CivilizationAtlasModulePresenter.cs`
- `src/app/concepts/DiseaseAtlasModulePresenter.cs`
- `src/app/concepts/EcologyAtlasModulePresenter.cs`
- `src/app/concepts/EvolutionAtlasModulePresenter.cs`
- `src/app/concepts/LanguageAtlasModulePresenter.cs`
- `src/app/concepts/ReligionAtlasModulePresenter.cs`
- `src/app/components/CollapsibleSection.cs`
- `src/app/MainApp.cs`
- `src/app/MainApp.Navigation.cs`
- `src/app/SplashScreen.cs`
- `src/app/MainMenuScreen.cs`
- `src/app/WelcomeScreen.cs`
- `src/app/ObjectGenerationRequest.cs`
- `src/app/SystemGenerationScreen.cs`
- `src/app/ObjectGenerationScreen.cs`
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

> **Note:** The trimmed tree below highlights the active layout after the station-design fold-in. The exhaustive file list above remains the authoritative enumeration.

```
star_gen/
|-- Concepts/
|   |-- Additions.md
|   |-- CivilisationEngine/
|   |-- EvoTechTree/
|   |-- DiseaseSimulator/
|   |-- ReligionGenerator/
|   `-- ConlangGenerator/
|-- Docs/
|   |-- ProjectStructure.md
|   |-- Roadmap.md
|   `-- TravellerWorldCreation.md
|-- src/
|   |-- app/
|   |   |-- galaxy_viewer/
|   |   |-- concepts/
|   |   |-- system_viewer/
|   |   |-- viewer/
|   |   `-- prototypes/
|   |-- domain/
|   |   |-- concepts/
|   |   |   |-- civilization/
|   |   |   |-- disease/
|   |   |   |-- ecology/
|   |   |   |-- evolution/
|   |   |   |-- language/
|   |   |   `-- religion/
|   |   |-- population/
|   |   |   |-- Outpost.cs
|   |   |   |-- SpaceStation.cs
|   |   |   |-- StationGenerator.cs
|   |   |   |-- StationPlacementRules.cs
|   |   |   `-- station_design/
|   |   |       |-- AccommodationKind.cs
|   |   |       |-- AutoPopulateFlags.cs
|   |   |       |-- BudgetFitter.cs
|   |   |       |-- ComponentCatalog.cs
|   |   |       |-- ComponentCounts.cs
|   |   |       |-- ComponentSelection.cs
|   |   |       |-- CrewCalculator.cs
|   |   |       |-- DesignBreakdowns.cs
|   |   |       |-- DesignCalculator.cs
|   |   |       |-- DesignMapping.cs
|   |   |       |-- DesignResult.cs
|   |   |       |-- DesignSpec.cs
|   |   |       |-- HullCalculator.cs
|   |   |       |-- PresetApplicator.cs
|   |   |       |-- classification/
|   |   |       |-- components/
|   |   |       `-- presets/
|   |   |-- generation/
|   |   |   `-- traveller/
|   |   |-- jumplanes/
|   |   `-- system/
|   `-- services/
|       |-- concepts/
|       |-- export/
|       |   `-- StationStatBlockExporter.cs
|       `-- persistence/
|-- Tests/
|   |-- Fixtures/
|   |   `-- StationDesign/
|   |-- Framework/
|   |-- Integration/
|   |   |-- TestObjectGenerationStudio.cs
|   |   |-- TestStudioScreenLayoutHelper.cs
|   |   `-- TestStationDesignIntegration.cs
|   |-- Unit/
|   |   |-- Population/
|   |   |   `-- StationDesign/
|   |   `-- TestTravellerWorldGenerator.cs
|-- VERSION.md
|-- README.md
`-- project.godot
``````

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
