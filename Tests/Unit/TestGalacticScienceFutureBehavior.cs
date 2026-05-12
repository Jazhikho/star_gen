#nullable enable annotations
#nullable disable warnings
using System;
using System.IO;
using System.Reflection;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
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
}
