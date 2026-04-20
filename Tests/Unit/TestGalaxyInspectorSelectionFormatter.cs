#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.GalaxyViewer;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests galaxy-inspector selection formatting derived from world-space positions.
/// </summary>
public static class TestGalaxyInspectorSelectionFormatter
{
	public static void TestBuildUsesCurrentQuadrantAndSubsectorGrid()
	{
		Vector3 activePosition = new(8123.4f, 27.8f, -45.6f);

		var summary = GalaxyInspectorSelectionFormatter.Build(activePosition);

		DotNetNativeTestSuite.AssertEqual(new Vector3I(8, 0, -1), summary.Quadrant, "quadrant should come from the active overview position");
		DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 9), summary.Sector, "sector should resolve from the active position within the quadrant");
		DotNetNativeTestSuite.AssertEqual(new Vector3I(3, 7, 4), summary.LocalGrid, "local coordinates should reflect the active position within its subsector grid");
	}

	public static void TestBuildComputesHomeRelativePolarReadout()
	{
		Vector3 activePosition = new(0.0f, 100.0f, 100.0f);

		var summary = GalaxyInspectorSelectionFormatter.Build(activePosition);

		DotNetNativeTestSuite.AssertFloatNear(90.0, summary.AzimuthDegrees, 0.01, "azimuth should measure disk angle from the Earth-home reference direction");
		DotNetNativeTestSuite.AssertFloatNear(45.0, summary.InclinationDegrees, 0.01, "inclination should measure angle above the galactic center plane");
		DotNetNativeTestSuite.AssertFloatNear(100.0, summary.DistanceFromCorePc, 0.01, "distance from core should measure planar radial distance");
	}

	public static void TestFormatDistanceFromCoreUsesPcAndKpcThresholds()
	{
		DotNetNativeTestSuite.AssertEqual("850.0 pc", GalaxyInspectorSelectionFormatter.FormatDistanceFromCore(850.0), "sub-kiloparsec distances should stay in parsecs");
		DotNetNativeTestSuite.AssertEqual("8.50 kpc", GalaxyInspectorSelectionFormatter.FormatDistanceFromCore(8500.0), "kiloparsec-scale distances should switch to kiloparsecs");
	}

	public static void TestFormatHierarchicalCoordinatesUsesQuadrantSectorAndLocalTriples()
	{
		Vector3 activePosition = new(8123.4f, 27.8f, -45.6f);
		var summary = GalaxyInspectorSelectionFormatter.Build(activePosition);

		string formatted = GalaxyInspectorSelectionFormatter.FormatHierarchicalCoordinates(summary);

		DotNetNativeTestSuite.AssertEqual("Q(8, 0, -1) S(1, 0, 9) L(3, 7, 4)", formatted, "hierarchical coordinates should include quadrant, sector, and local triples");
	}
}
