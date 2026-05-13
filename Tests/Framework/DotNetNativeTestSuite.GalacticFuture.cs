#nullable enable annotations
#nullable disable warnings

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs the explicit future-to-present science behavior baseline.
    /// </summary>
    public static void RunGalacticFutureBehaviorHeadless(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_spec_serializes_dynamics_behavior_mode",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxySpecSerializesDynamicsBehaviorMode);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_dynamics_behavior_mode_defines_documented_stages",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PDynamicsBehaviorModeDefinesDocumentedStages);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_origin_context_carries_local_dynamics_annotations",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyOriginContextCarriesLocalDynamicsAnnotations);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_diagnostics_only_mode_does_not_mutate_placement",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PDiagnosticsOnlyModeDoesNotMutatePlacement);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_affect_region_context_mode_changes_only_origin_context",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PAffectRegionContextModeChangesOnlyOriginContext);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_affect_placement_mode_changes_placement_only_when_explicit",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PAffectPlacementModeChangesPlacementOnlyWhenExplicit);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_studio_exposes_diagnostics_and_disabled_dynamics_controls",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyStudioExposesDiagnosticsAndDisabledDynamicsControls);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_studio_help_explains_diagnostics_vs_behavior",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyStudioHelpExplainsDiagnosticsVsBehavior);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_viewer_inspector_exposes_science_diagnostics_readout",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyViewerInspectorExposesScienceDiagnosticsReadout);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_viewer_diagnostics_readout_does_not_mutate_generation",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyViewerDiagnosticsReadoutDoesNotMutateGeneration);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_non_milky_way_families_use_comparison_calibration_presets",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PNonMilkyWayFamiliesUseComparisonCalibrationPresets);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_viewer_defines_diagnostics_overlay_surfaces",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyViewerDefinesDiagnosticsOverlaySurfaces);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_local_space_preview_annotates_selected_systems_without_route_population_effects",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PLocalSpacePreviewAnnotatesSelectedSystemsWithoutRoutePopulationEffects);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_completion_plan_covers_remaining_hardening_families",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2PCompletionPlanCoversRemainingHardeningFamilies);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_engine_catalog_defines_alternative_families",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2PEngineCatalogDefinesAlternativeFamilies);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_planet_generation_records_mass_corrected_hz_diagnostics",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2PPlanetGenerationRecordsMassCorrectedHzDiagnostics);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_orbit_slots_serialize_alternative_stability_diagnostics",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2POrbitSlotsSerializeAlternativeStabilityDiagnostics);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_planet_formation_trace_copies_stability_diagnostics",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2PPlanetFormationTraceCopiesStabilityDiagnostics);
        runner.RunNativeTest(
            "TestScienceHardeningFutureBehavior::test_f2p_moon_trace_marks_terrestrial_impact_channel_sources",
            Tests.Unit.TestScienceHardeningFutureBehavior.TestF2PMoonTraceMarksTerrestrialImpactChannelSources);
    }
}
