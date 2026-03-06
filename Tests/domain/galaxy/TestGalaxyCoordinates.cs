#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for GalaxyCoordinates — grid conversions and bounds.
/// </summary>
public static class TestGalaxyCoordinates
{
    public static void TestOriginMapsToQuadrantZero()
    {
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(new Vector3(0.0f, 0.0f, 0.0f));
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 0), coords, "Origin should be in quadrant (0,0,0)");
    }

    public static void TestPositivePositionCorrectQuadrant()
    {
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(new Vector3(500.0f, 200.0f, 999.9f));
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 0), coords, "Position inside first quadrant");
    }

    public static void TestPositionAtBoundaryRollsToNext()
    {
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(new Vector3(1000.0f, 0.0f, 0.0f));
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), coords, "Exactly at 1000 should be quadrant 1");
    }

    public static void TestNegativePositionCorrectQuadrant()
    {
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(new Vector3(-1.0f, 0.0f, 0.0f));
        DotNetNativeTestSuite.AssertEqual(new Vector3I(-1, 0, 0), coords, "Just below zero should be quadrant -1");
    }

    public static void TestNegativePositionDeep()
    {
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(new Vector3(-1500.0f, -500.0f, -2001.0f));
        DotNetNativeTestSuite.AssertEqual(new Vector3I(-2, -1, -3), coords, "Deep negative position");
    }

    public static void TestQuadrantCenterPositive()
    {
        Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(new Vector3I(0, 0, 0));
        DotNetNativeTestSuite.AssertFloatNear(500.0, center.X, 0.01, "Center X of quadrant (0,0,0)");
        DotNetNativeTestSuite.AssertFloatNear(500.0, center.Y, 0.01, "Center Y of quadrant (0,0,0)");
        DotNetNativeTestSuite.AssertFloatNear(500.0, center.Z, 0.01, "Center Z of quadrant (0,0,0)");
    }

    public static void TestQuadrantCenterNegative()
    {
        Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(new Vector3I(-1, -1, -1));
        DotNetNativeTestSuite.AssertFloatNear(-500.0, center.X, 0.01, "Center X of quadrant (-1,-1,-1)");
        DotNetNativeTestSuite.AssertFloatNear(-500.0, center.Y, 0.01, "Center Y of quadrant (-1,-1,-1)");
        DotNetNativeTestSuite.AssertFloatNear(-500.0, center.Z, 0.01, "Center Z of quadrant (-1,-1,-1)");
    }

    public static void TestRoundTripPositive()
    {
        Vector3 original = new Vector3(3456.0f, 789.0f, 1234.0f);
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(original);
        Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(coords);

        Vector3I recoords = GalaxyCoordinates.ParsecToQuadrant(center);
        DotNetNativeTestSuite.AssertEqual(coords, recoords, "Round-trip: center maps back to same quadrant");
    }

    public static void TestRoundTripNegative()
    {
        Vector3 original = new Vector3(-7890.0f, -456.0f, -123.0f);
        Vector3I coords = GalaxyCoordinates.ParsecToQuadrant(original);
        Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(coords);

        Vector3I recoords = GalaxyCoordinates.ParsecToQuadrant(center);
        DotNetNativeTestSuite.AssertEqual(coords, recoords, "Round-trip negative: center maps back to same quadrant");
    }

    public static void TestCenterIsWithinCell()
    {
        Vector3I coords = new Vector3I(3, -2, 7);
        Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(coords);

        float cellMinX = coords.X * (float)GalaxyCoordinates.QuadrantSizePc;
        float cellMaxX = cellMinX + (float)GalaxyCoordinates.QuadrantSizePc;
        DotNetNativeTestSuite.AssertGreaterThan(center.X, cellMinX, "Center X above cell minimum");
        DotNetNativeTestSuite.AssertLessThan(center.X, cellMaxX, "Center X below cell maximum");
    }

    public static void TestGridBoundsCoverGalaxy()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        Vector3I gridMin = GalaxyCoordinates.GetQuadrantGridMin(spec);
        Vector3I gridMax = GalaxyCoordinates.GetQuadrantGridMax(spec);

        Vector3 minParsec = GalaxyCoordinates.QuadrantToParsecCenter(gridMin);
        Vector3 maxParsec = GalaxyCoordinates.QuadrantToParsecCenter(gridMax);

        DotNetNativeTestSuite.AssertLessThan(
            minParsec.X, (float)(-spec.RadiusPc + GalaxyCoordinates.QuadrantSizePc),
            "Grid min X should cover negative radius");
        DotNetNativeTestSuite.AssertGreaterThan(
            maxParsec.X, (float)(spec.RadiusPc - GalaxyCoordinates.QuadrantSizePc),
            "Grid max X should cover positive radius");
    }

    public static void TestGridBoundsSymmetric()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        Vector3I gridMin = GalaxyCoordinates.GetQuadrantGridMin(spec);
        Vector3I gridMax = GalaxyCoordinates.GetQuadrantGridMax(spec);

        DotNetNativeTestSuite.AssertEqual(gridMin.X, gridMin.Z, "Min X and Z should be equal for round galaxy");
        DotNetNativeTestSuite.AssertEqual(gridMax.X, gridMax.Z, "Max X and Z should be equal for round galaxy");
    }

    public static void TestIsPositionInQuadrantTrue()
    {
        Vector3 pos = new Vector3(500.0f, 500.0f, 500.0f);
        bool result = GalaxyCoordinates.IsPositionInQuadrant(pos, new Vector3I(0, 0, 0));
        DotNetNativeTestSuite.AssertTrue(result, "Position at center of quadrant 0 should be in quadrant 0");
    }

    public static void TestIsPositionInQuadrantFalse()
    {
        Vector3 pos = new Vector3(1500.0f, 500.0f, 500.0f);
        bool result = GalaxyCoordinates.IsPositionInQuadrant(pos, new Vector3I(0, 0, 0));
        DotNetNativeTestSuite.AssertFalse(result, "Position in quadrant 1 should not be in quadrant 0");
    }

    public static void TestEffectiveHalfHeightUsesBulgeWhenLarger()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        float effective = (float)GalaxyCoordinates.GetEffectiveHalfHeight(spec);
        double expected = spec.BulgeHeightPc * GalaxyCoordinates.BulgeSigmaCoverage;
        float expectedFloat = (float)expected;

        DotNetNativeTestSuite.AssertFloatNear(expectedFloat, effective, 0.01,
            "Effective half-height should use bulge 3-sigma when larger than height_pc");
    }

    public static void TestEffectiveHalfHeightUsesHeightWhenLarger()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        spec.HeightPc = 5000.0;
        spec.BulgeHeightPc = 100.0;
        float effective = (float)GalaxyCoordinates.GetEffectiveHalfHeight(spec);

        DotNetNativeTestSuite.AssertFloatNear(5000.0f, effective, 0.01,
            "Effective half-height should use height_pc when larger than bulge extent");
    }

    public static void TestEffectiveRadiusUsesDiskWhenLarger()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        float effective = (float)GalaxyCoordinates.GetEffectiveRadius(spec);
        float expectedRadius = (float)spec.RadiusPc;

        DotNetNativeTestSuite.AssertFloatNear(expectedRadius, effective, 0.01,
            "Effective radius should use disk radius when larger than bulge extent");
    }

    public static void TestEffectiveRadiusUsesBulgeWhenLarger()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        spec.RadiusPc = 1000.0f;
        spec.BulgeRadiusPc = 5000.0f;
        float effective = (float)GalaxyCoordinates.GetEffectiveRadius(spec);
        double expectedCalc = 5000.0 * GalaxyCoordinates.BulgeSigmaCoverage;
        float expectedFloat = (float)expectedCalc;

        DotNetNativeTestSuite.AssertFloatNear(expectedFloat, effective, 0.01,
            "Effective radius should use bulge 3-sigma when larger than disk radius");
    }

    public static void TestGridBoundsCoverBulgeVertically()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        Vector3I gridMin = GalaxyCoordinates.GetQuadrantGridMin(spec);
        Vector3I gridMax = GalaxyCoordinates.GetQuadrantGridMax(spec);

        float bulgeExtent = (float)(spec.BulgeHeightPc * GalaxyCoordinates.BulgeSigmaCoverage);
        Vector3 minCenter = GalaxyCoordinates.QuadrantToParsecCenter(gridMin);
        Vector3 maxCenter = GalaxyCoordinates.QuadrantToParsecCenter(gridMax);

        DotNetNativeTestSuite.AssertLessThan(
            minCenter.Y, -bulgeExtent + (float)GalaxyCoordinates.QuadrantSizePc,
            "Grid min should cover negative bulge extent");
        DotNetNativeTestSuite.AssertGreaterThan(
            maxCenter.Y, bulgeExtent - (float)GalaxyCoordinates.QuadrantSizePc,
            "Grid max should cover positive bulge extent");
    }

    public static void TestGridHasSymmetricYLayers()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(1);
        Vector3I gridMin = GalaxyCoordinates.GetQuadrantGridMin(spec);
        Vector3I gridMax = GalaxyCoordinates.GetQuadrantGridMax(spec);

        int yLayers = gridMax.Y - gridMin.Y + 1;
        DotNetNativeTestSuite.AssertGreaterThan(yLayers, 3,
            "Should have more than 3 y-layers to show structure above and below plane");
    }
}
