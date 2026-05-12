#nullable enable annotations
#nullable disable warnings
using System;
using System.IO;
using System.Reflection;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// F2P tests for future galactic science behavior. These are expected to fail until the planned behavior exists.
/// </summary>
public static class TestGalacticScienceFutureBehavior
{
    /// <summary>
    /// F2P: galaxy specs should expose and serialize a behavior mode before diagnostics can drive generation.
    /// </summary>
    public static void TestF2PGalaxySpecSerializesDynamicsBehaviorMode()
    {
        Type? behaviorModeType = Type.GetType("StarGen.Domain.Galaxy.GalaxyDynamicsBehaviorMode");
        DotNetNativeTestSuite.AssertNotNull(behaviorModeType, "F2P expected GalaxyDynamicsBehaviorMode enum to exist before active dynamics behavior");

        PropertyInfo? behaviorModeProperty = typeof(GalaxySpec).GetProperty("DynamicsBehaviorMode");
        DotNetNativeTestSuite.AssertNotNull(behaviorModeProperty, "F2P expected GalaxySpec.DynamicsBehaviorMode to gate diagnostic behavior");

        GalaxySpec spec = GalaxySpec.CreateMilkyWay(18401);
        Dictionary data = spec.ToDictionary();
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("dynamics_behavior_mode"), "F2P expected GalaxySpec serialization to include dynamics_behavior_mode");
        DotNetNativeTestSuite.AssertEqual("diagnostics_only", data["dynamics_behavior_mode"].AsString(), "F2P expected default behavior mode to be diagnostics_only");
    }

    /// <summary>
    /// F2P: the behavior mode enum should define the documented activation stages.
    /// </summary>
    public static void TestF2PDynamicsBehaviorModeDefinesDocumentedStages()
    {
        Type behaviorModeType = GetRequiredBehaviorModeType();

        AssertEnumNameExists(behaviorModeType, "DiagnosticsOnly");
        AssertEnumNameExists(behaviorModeType, "AffectRegionContext");
        AssertEnumNameExists(behaviorModeType, "AffectPlacement");
    }

    /// <summary>
    /// F2P: origin context should carry local dynamics annotations in the first active-behavior stage.
    /// </summary>
    public static void TestF2PGalaxyOriginContextCarriesLocalDynamicsAnnotations()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(18402);
        GalaxyOriginContext context = GalaxyScientificFieldEvaluator.Evaluate(new Vector3(8200.0f, 0.0f, 0.0f), spec);
        Dictionary data = context.ToDictionary();

        AssertContextKey(data, "dynamics_behavior_mode");
        AssertContextKey(data, "local_total_mass_density_solar_per_pc3");
        AssertContextKey(data, "local_baryonic_mass_density_solar_per_pc3");
        AssertContextKey(data, "local_dark_matter_density_solar_per_pc3");
        AssertContextKey(data, "local_surface_density_solar_per_pc2");
        AssertContextKey(data, "bar_pattern_speed_km_s_per_kpc");
        AssertContextKey(data, "corotation_radius_pc");
        AssertContextKey(data, "analog_calibration_mode");
    }

    /// <summary>
    /// F2P: diagnostics-only behavior mode should leave placement unchanged when diagnostic readouts are present.
    /// </summary>
    public static void TestF2PDiagnosticsOnlyModeDoesNotMutatePlacement()
    {
        GalaxySpec baseline = GalaxySpec.CreateMilkyWay(18404);
        GalaxySpec diagnosticsOnly = GalaxySpec.FromDictionary(baseline.ToDictionary());
        SetBehaviorMode(diagnosticsOnly, "DiagnosticsOnly");

        GalaxySample baselineSample = DensitySampler.SampleGalaxy(baseline, 180, new SeededRng(18440));
        GalaxySample diagnosticsOnlySample = DensitySampler.SampleGalaxy(diagnosticsOnly, 180, new SeededRng(18440));

        AssertSampleEquals(baselineSample.BulgePoints, diagnosticsOnlySample.BulgePoints, "bulge");
        AssertSampleEquals(baselineSample.DiskPoints, diagnosticsOnlySample.DiskPoints, "disk");
    }

    /// <summary>
    /// F2P: region-context mode should enrich origin context without changing density-sampled placement.
    /// </summary>
    public static void TestF2PAffectRegionContextModeChangesOnlyOriginContext()
    {
        GalaxySpec baseline = GalaxySpec.CreateMilkyWay(18405);
        GalaxySpec regionContext = GalaxySpec.FromDictionary(baseline.ToDictionary());
        SetBehaviorMode(regionContext, "AffectRegionContext");

        GalaxySample baselineSample = DensitySampler.SampleGalaxy(baseline, 180, new SeededRng(18450));
        GalaxySample regionContextSample = DensitySampler.SampleGalaxy(regionContext, 180, new SeededRng(18450));
        AssertSampleEquals(baselineSample.BulgePoints, regionContextSample.BulgePoints, "bulge");
        AssertSampleEquals(baselineSample.DiskPoints, regionContextSample.DiskPoints, "disk");

        GalaxyOriginContext context = GalaxyScientificFieldEvaluator.Evaluate(new Vector3(8200.0f, 0.0f, 0.0f), regionContext);
        Dictionary data = context.ToDictionary();
        DotNetNativeTestSuite.AssertEqual("affect_region_context", data["dynamics_behavior_mode"].AsString(), "F2P expected active region-context mode to serialize on origin context");
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("local_total_mass_density_solar_per_pc3"), "F2P expected active region-context mode to enrich local dynamics context");
    }

    /// <summary>
    /// F2P: placement mode should be the only mode allowed to change density-sampled star positions.
    /// </summary>
    public static void TestF2PAffectPlacementModeChangesPlacementOnlyWhenExplicit()
    {
        GalaxySpec baseline = GalaxySpec.CreateMilkyWay(18406);
        GalaxySpec placement = GalaxySpec.FromDictionary(baseline.ToDictionary());
        SetBehaviorMode(placement, "AffectPlacement");

        GalaxySample baselineSample = DensitySampler.SampleGalaxy(baseline, 180, new SeededRng(18460));
        GalaxySample placementSample = DensitySampler.SampleGalaxy(placement, 180, new SeededRng(18460));

        DotNetNativeTestSuite.AssertTrue(
            SamplesDiffer(baselineSample.BulgePoints, placementSample.BulgePoints)
                || SamplesDiffer(baselineSample.DiskPoints, placementSample.DiskPoints),
            "F2P expected AffectPlacement mode to be the documented mode that changes density-sampled placement");
    }

    /// <summary>
    /// F2P: Studio should expose a read-only diagnostics panel and disabled behavior controls.
    /// </summary>
    public static void TestF2PGalaxyStudioExposesDiagnosticsAndDisabledDynamicsControls()
    {
        string screenSource = ReadRepoFile(Path.Combine("src", "app", "GalaxyGenerationScreen.Science.cs"));
        string sceneSource = ReadRepoFile(Path.Combine("src", "app", "GalaxyGenerationScreen.tscn"));
        string combined = screenSource + "\n" + sceneSource;

        AssertContains(combined, "Resolved Galactic Diagnostics");
        AssertContains(combined, "Diagnostics Only");
        AssertContains(combined, "Affect Region Context");
        AssertContains(combined, "Affect Placement");
        AssertContains(combined, "Analog Calibration");
        AssertContains(combined, "Bar Dynamics");
        AssertContains(combined, "comparison sources needed");
    }

    /// <summary>
    /// F2P: Studio help should distinguish structure, diagnostics, and behavior with separate source caveats.
    /// </summary>
    public static void TestF2PGalaxyStudioHelpExplainsDiagnosticsVsBehavior()
    {
        string screenSource = ReadRepoFile(Path.Combine("src", "app", "GalaxyGenerationScreen.Science.cs"));

        AssertContains(screenSource, "structural schema");
        AssertContains(screenSource, "diagnostic dynamics");
        AssertContains(screenSource, "active generation behavior");
        AssertContains(screenSource, "Bland-Hawthorn");
        AssertContains(screenSource, "Bovy");
        AssertContains(screenSource, "Khoperskov");
        AssertContains(screenSource, "Hunt");
        AssertContains(screenSource, "future comparison sources");
    }

    /// <summary>
    /// F2P: Viewer inspector should expose read-only science diagnostics from the expanded galaxy profile.
    /// </summary>
    public static void TestF2PGalaxyViewerInspectorExposesScienceDiagnosticsReadout()
    {
        string panelSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyInspectorPanel.cs"));
        string formatterSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyInspectorSelectionFormatter.cs"));
        string combined = panelSource + "\n" + formatterSource;

        AssertContains(combined, "Science Diagnostics");
        AssertContains(combined, "Mass Budget");
        AssertContains(combined, "Rotation");
        AssertContains(combined, "Dynamics");
        AssertContains(combined, "pattern speed");
        AssertContains(combined, "corotation");
        AssertContains(combined, "local density");
        AssertContains(combined, "analog calibration");
    }

    /// <summary>
    /// F2P: viewer diagnostics readouts should be pure readouts that do not mutate generation state.
    /// </summary>
    public static void TestF2PGalaxyViewerDiagnosticsReadoutDoesNotMutateGeneration()
    {
        string panelSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyInspectorPanel.cs"));
        string formatterSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyInspectorSelectionFormatter.cs"));
        string combined = panelSource + "\n" + formatterSource;

        AssertContains(combined, "BuildScienceDiagnostics");
        AssertContains(combined, "ReadOnly");
        DotNetNativeTestSuite.AssertFalse(combined.Contains("DynamicsBehaviorMode ="), "F2P expected viewer readout code not to mutate dynamics behavior mode");
        DotNetNativeTestSuite.AssertFalse(combined.Contains("DensitySampler.SampleGalaxy"), "F2P expected diagnostics readout code not to resample galaxy placement");
    }

    /// <summary>
    /// F2P: non-Milky-Way families should have comparison presets, not only caveat strings.
    /// </summary>
    public static void TestF2PNonMilkyWayFamiliesUseComparisonCalibrationPresets()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = GalaxySpec.GalaxyType.Elliptical;
        GalaxySpec spec = GalaxySpec.CreateFromConfig(config, 18403);

        DotNetNativeTestSuite.AssertTrue(
            spec.DynamicsDiagnostic.AnalogCalibrationMode != "family_proxy_pending_comparison",
            "F2P expected non-Milky-Way comparison calibration presets instead of proxy-only caveats");
        DotNetNativeTestSuite.AssertTrue(
            spec.DynamicsDiagnostic.NonMilkyWayComparisonStatus != "needs_non_milky_way_comparison_sources",
            "F2P expected non-Milky-Way comparison sources to be represented before active behavior");
    }

    /// <summary>
    /// F2P: future overlay/readout names should exist before diagnostics influence local-space workflows.
    /// </summary>
    public static void TestF2PGalaxyViewerDefinesDiagnosticsOverlaySurfaces()
    {
        string viewerSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyViewer.cs"));
        string rendererSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyRenderer.cs"));
        string localSpaceSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyViewer.LocalSpace.cs"));
        string combined = viewerSource + "\n" + rendererSource + "\n" + localSpaceSource;

        AssertContains(combined, "Bar/Corotation");
        AssertContains(combined, "GHZ");
        AssertContains(combined, "Hazard");
        AssertContains(combined, "Local Mass Density");
        AssertContains(combined, "diagnostic overlay");
    }

    /// <summary>
    /// F2P: local-space previews should show diagnostic annotations before route or population behavior consumes them.
    /// </summary>
    public static void TestF2PLocalSpacePreviewAnnotatesSelectedSystemsWithoutRoutePopulationEffects()
    {
        string localSpaceSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyViewer.LocalSpace.cs"));
        string jumpSource = ReadRepoFile(Path.Combine("src", "app", "galaxy_viewer", "GalaxyViewer.JumpRoutes.cs"));

        AssertContains(localSpaceSource, "local dynamics diagnostics");
        AssertContains(localSpaceSource, "diagnostic-only");
        DotNetNativeTestSuite.AssertFalse(jumpSource.Contains("LocalTotalMassDensitySolarPerPc3"), "F2P expected route behavior not to consume diagnostics before a reviewed behavior mode");
        DotNetNativeTestSuite.AssertFalse(jumpSource.Contains("DynamicsBehaviorMode"), "F2P expected route behavior not to consume dynamics mode before route behavior is explicitly approved");
    }

    private static void AssertContextKey(Dictionary data, string key)
    {
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey(key), $"F2P expected GalaxyOriginContext serialization to include {key}");
    }

    private static void AssertContains(string text, string fragment)
    {
        DotNetNativeTestSuite.AssertTrue(text.Contains(fragment), $"F2P expected implementation surface to contain '{fragment}'");
    }

    private static string ReadRepoFile(string path)
    {
        DotNetNativeTestSuite.AssertTrue(File.Exists(path), $"F2P expected file to exist: {path}");
        return File.ReadAllText(path);
    }

    private static Type GetRequiredBehaviorModeType()
    {
        Type? behaviorModeType = Type.GetType("StarGen.Domain.Galaxy.GalaxyDynamicsBehaviorMode");
        DotNetNativeTestSuite.AssertNotNull(behaviorModeType, "F2P expected GalaxyDynamicsBehaviorMode enum to exist");
        return behaviorModeType!;
    }

    private static void AssertEnumNameExists(Type enumType, string enumName)
    {
        DotNetNativeTestSuite.AssertTrue(Enum.IsDefined(enumType, enumName), $"F2P expected {enumType.Name}.{enumName} to exist");
    }

    private static void SetBehaviorMode(GalaxySpec spec, string enumName)
    {
        Type behaviorModeType = GetRequiredBehaviorModeType();
        object modeValue = Enum.Parse(behaviorModeType, enumName);
        PropertyInfo? behaviorModeProperty = typeof(GalaxySpec).GetProperty("DynamicsBehaviorMode");
        DotNetNativeTestSuite.AssertNotNull(behaviorModeProperty, "F2P expected GalaxySpec.DynamicsBehaviorMode to exist");
        behaviorModeProperty!.SetValue(spec, modeValue);
    }

    private static void AssertSampleEquals(Vector3[] expected, Vector3[] actual, string population)
    {
        DotNetNativeTestSuite.AssertEqual(expected.Length, actual.Length, $"F2P expected {population} sample count to remain unchanged");
        for (int index = 0; index < expected.Length; index += 1)
        {
            DotNetNativeTestSuite.AssertTrue(expected[index].IsEqualApprox(actual[index]), $"F2P expected {population} sample {index} to remain unchanged");
        }
    }

    private static bool SamplesDiffer(Vector3[] left, Vector3[] right)
    {
        if (left.Length != right.Length)
        {
            return true;
        }

        for (int index = 0; index < left.Length; index += 1)
        {
            if (!left[index].IsEqualApprox(right[index]))
            {
                return true;
            }
        }

        return false;
    }
}
