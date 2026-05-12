#nullable enable annotations
#nullable disable warnings

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs the intentionally failing future galactic science behavior baseline.
    /// </summary>
    public static void RunGalacticFutureBehaviorHeadless(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_spec_serializes_dynamics_behavior_mode",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxySpecSerializesDynamicsBehaviorMode);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_origin_context_carries_local_dynamics_annotations",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyOriginContextCarriesLocalDynamicsAnnotations);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_studio_exposes_diagnostics_and_disabled_dynamics_controls",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyStudioExposesDiagnosticsAndDisabledDynamicsControls);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_viewer_inspector_exposes_science_diagnostics_readout",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyViewerInspectorExposesScienceDiagnosticsReadout);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_non_milky_way_families_use_comparison_calibration_presets",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PNonMilkyWayFamiliesUseComparisonCalibrationPresets);
        runner.RunNativeTest(
            "TestGalacticScienceFutureBehavior::test_f2p_galaxy_viewer_defines_diagnostics_overlay_surfaces",
            Tests.Unit.TestGalacticScienceFutureBehavior.TestF2PGalaxyViewerDefinesDiagnosticsOverlaySurfaces);
    }
}
