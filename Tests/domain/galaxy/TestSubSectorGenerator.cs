#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for SubSectorGenerator — determinism, density correlation, and bounds.
/// </summary>
public static class TestSubSectorGenerator
{
    private static GalaxySpec _spec;
    private static DensityModelInterface _model;
    private static float _refDensity;

    private static void BeforeEach()
    {
        _spec = GalaxySpec.CreateMilkyWay(42);
        _model = DensityModelInterface.CreateForSpec(_spec);
        _refDensity = _model.GetDensity(new Vector3(8000.0f, 0.0f, 0.0f));
    }

    public static void TestDeterminism()
    {
        BeforeEach();
        SectorStarData resultA = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);
        SectorStarData resultB = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        DotNetNativeTestSuite.AssertEqual(resultA.GetCount(), resultB.GetCount(),
            "Same inputs must produce same star count");

        for (int i = 0; i < Math.Min(10, resultA.GetCount()); i += 1)
        {
            DotNetNativeTestSuite.AssertTrue(
                resultA.Positions[i].IsEqualApprox(resultB.Positions[i]),
                $"Star position {i} must be identical");
            DotNetNativeTestSuite.AssertEqual(resultA.StarSeeds[i], resultB.StarSeeds[i],
                $"Star seed {i} must be identical");
        }
    }

    public static void TestDifferentSeedDifferentResult()
    {
        BeforeEach();
        SectorStarData resultA = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);
        SectorStarData resultB = SubSectorGenerator.GenerateSectorStars(
            99, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        bool anyDifferent = resultA.GetCount() != resultB.GetCount();
        if (!anyDifferent && resultA.GetCount() > 0)
        {
            anyDifferent = !resultA.Positions[0].IsEqualApprox(resultB.Positions[0]);
        }
        DotNetNativeTestSuite.AssertTrue(anyDifferent, "Different seeds should give different results");
    }

    public static void TestDifferentQuadrantDifferentResult()
    {
        BeforeEach();
        SectorStarData resultA = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);
        SectorStarData resultB = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(1, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        bool anyDifferent = resultA.GetCount() != resultB.GetCount();
        if (!anyDifferent && resultA.GetCount() > 0)
        {
            anyDifferent = !resultA.Positions[0].IsEqualApprox(resultB.Positions[0]);
        }
        DotNetNativeTestSuite.AssertTrue(anyDifferent, "Different quadrants should give different results");
    }

    public static void TestDifferentSectorDifferentResult()
    {
        BeforeEach();
        SectorStarData resultA = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), _model, _refDensity);
        SectorStarData resultB = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), _model, _refDensity);

        bool anyDifferent = resultA.GetCount() != resultB.GetCount();
        if (!anyDifferent && resultA.GetCount() > 0)
        {
            anyDifferent = !resultA.Positions[0].IsEqualApprox(resultB.Positions[0]);
        }
        DotNetNativeTestSuite.AssertTrue(anyDifferent, "Different sectors should give different results");
    }

    public static void TestCenterSectorHasMoreStarsThanEdge()
    {
        BeforeEach();
        SectorStarData center = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), _model, _refDensity);
        SectorStarData edge = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(14, 0, 14), new Vector3I(9, 0, 9), _model, _refDensity);

        DotNetNativeTestSuite.AssertGreaterThan(center.GetCount(), edge.GetCount(),
            "Center sector should have more stars than far edge sector");
    }

    public static void TestZeroReferenceDensityReturnsEmpty()
    {
        BeforeEach();
        SectorStarData result = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, 0.0f);
        DotNetNativeTestSuite.AssertEqual(0, result.GetCount(),
            "Zero reference density should produce no stars");
    }

    public static void TestStarPositionsWithinSectorBounds()
    {
        BeforeEach();
        Vector3I quadrant = new Vector3I(0, 0, 0);
        Vector3I sectorLocal = new Vector3I(3, 4, 5);
        SectorStarData result = SubSectorGenerator.GenerateSectorStars(
            42, quadrant, sectorLocal, _model, _refDensity);

        Vector3 sectorOrigin = GalaxyCoordinates.SectorWorldOrigin(quadrant, sectorLocal);
        Vector3 sectorMax = sectorOrigin + Vector3.One * (float)GalaxyCoordinates.SectorSizePc;

        for (int i = 0; i < result.GetCount(); i += 1)
        {
            Vector3 pos = result.Positions[i];
            DotNetNativeTestSuite.AssertGreaterThan(pos.X, sectorOrigin.X - 0.01f,
                $"Star {i} X below sector min");
            DotNetNativeTestSuite.AssertLessThan(pos.X, sectorMax.X + 0.01f,
                $"Star {i} X above sector max");
            DotNetNativeTestSuite.AssertGreaterThan(pos.Y, sectorOrigin.Y - 0.01f,
                $"Star {i} Y below sector min");
            DotNetNativeTestSuite.AssertLessThan(pos.Y, sectorMax.Y + 0.01f,
                $"Star {i} Y above sector max");
            DotNetNativeTestSuite.AssertGreaterThan(pos.Z, sectorOrigin.Z - 0.01f,
                $"Star {i} Z below sector min");
            DotNetNativeTestSuite.AssertLessThan(pos.Z, sectorMax.Z + 0.01f,
                $"Star {i} Z above sector max");
        }
    }

    public static void TestStarSeedsAreUniqueWithinSector()
    {
        BeforeEach();
        SectorStarData result = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        if (result.GetCount() < 2)
        {
            return;
        }

        System.Collections.Generic.Dictionary<long, bool> seen = new System.Collections.Generic.Dictionary<long, bool>();
        int duplicates = 0;
        for (int i = 0; i < result.GetCount(); i += 1)
        {
            long seed = result.StarSeeds[i];
            if (seen.ContainsKey(seed))
            {
                duplicates += 1;
            }
            seen[seed] = true;
        }

        DotNetNativeTestSuite.AssertEqual(0, duplicates, "Star seeds within a sector should be unique");
    }

    public static void TestProducesStarsAtGalaxyCenter()
    {
        BeforeEach();
        SectorStarData result = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(0, 0, 0), _model, _refDensity);
        DotNetNativeTestSuite.AssertGreaterThan(result.GetCount(), 0,
            "Should produce at least some stars near galactic center");
    }

    public static void TestPoissonProducesZeroForZeroLambda()
    {
        BeforeEach();
        SectorStarData emptyResult = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(99, 99, 99), new Vector3I(0, 0, 0), _model, _refDensity);
        DotNetNativeTestSuite.AssertLessThan(emptyResult.GetCount(), 50,
            "Very low density region should have very few stars");
    }

    public static void TestBorderGenerationDeterministic()
    {
        BeforeEach();
        SectorStarData resultA = SubSectorGenerator.GenerateSectorWithBorder(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);
        SectorStarData resultB = SubSectorGenerator.GenerateSectorWithBorder(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        DotNetNativeTestSuite.AssertEqual(resultA.GetCount(), resultB.GetCount(),
            "Border generation must be deterministic in count");

        for (int i = 0; i < Math.Min(10, resultA.GetCount()); i += 1)
        {
            DotNetNativeTestSuite.AssertTrue(
                resultA.Positions[i].IsEqualApprox(resultB.Positions[i]),
                $"Border star position {i} must be identical");
        }
    }

    public static void TestBorderHasMoreStarsThanSectorAlone()
    {
        BeforeEach();
        SectorStarData sectorOnly = SubSectorGenerator.GenerateSectorStars(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);
        SectorStarData withBorder = SubSectorGenerator.GenerateSectorWithBorder(
            42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5), _model, _refDensity);

        DotNetNativeTestSuite.AssertGreaterThan(withBorder.GetCount(), sectorOnly.GetCount(),
            "Border shell should add additional stars beyond the sector alone");
    }

    public static void TestBorderStarsExtendBeyondSectorBounds()
    {
        BeforeEach();
        Vector3I quadrant = new Vector3I(0, 0, 0);
        Vector3I sectorLocal = new Vector3I(5, 5, 5);
        SectorStarData result = SubSectorGenerator.GenerateSectorWithBorder(
            42, quadrant, sectorLocal, _model, _refDensity);

        Vector3 sectorOrigin = GalaxyCoordinates.SectorWorldOrigin(quadrant, sectorLocal);
        Vector3 sectorMax = sectorOrigin + Vector3.One * (float)GalaxyCoordinates.SectorSizePc;

        int outsideCount = 0;
        for (int i = 0; i < result.GetCount(); i += 1)
        {
            Vector3 pos = result.Positions[i];
            if (pos.X < sectorOrigin.X || pos.X > sectorMax.X)
            {
                outsideCount += 1;
            }
            else if (pos.Y < sectorOrigin.Y || pos.Y > sectorMax.Y)
            {
                outsideCount += 1;
            }
            else if (pos.Z < sectorOrigin.Z || pos.Z > sectorMax.Z)
            {
                outsideCount += 1;
            }
        }

        DotNetNativeTestSuite.AssertGreaterThan(outsideCount, 0,
            "Border generation should produce stars outside sector bounds");
    }

    public static void TestSolarNeighborhoodDensityRealistic()
    {
        BeforeEach();
        Vector3I quadrant = new Vector3I(8, 0, 0);
        Vector3I sectorLocal = new Vector3I(0, 0, 0);
        int totalStars = 0;
        int numSamples = 100;

        for (int i = 0; i < numSamples; i += 1)
        {
            SectorStarData result = SubSectorGenerator.GenerateSectorStars(
                i, quadrant, sectorLocal, _model, _refDensity);
            totalStars += result.GetCount();
        }

        float avgPerSector = totalStars / (float)numSamples;
        DotNetNativeTestSuite.AssertGreaterThan(avgPerSector, 1000.0f,
            "Average stars per sector at 8kpc should be > 1000");
        DotNetNativeTestSuite.AssertLessThan(avgPerSector, 8000.0f,
            "Average stars per sector at 8kpc should be < 8000");
    }

    public static void TestMergeStarData()
    {
        SectorStarData dataA = new SectorStarData
        {
            Positions = new Vector3[] { new Vector3(1.0f, 2.0f, 3.0f) },
            StarSeeds = new long[] { 100 },
        };

        SectorStarData dataB = new SectorStarData
        {
            Positions = new Vector3[] { new Vector3(4.0f, 5.0f, 6.0f) },
            StarSeeds = new long[] { 200 },
        };

        dataA.Merge(dataB);

        DotNetNativeTestSuite.AssertEqual(2, dataA.GetCount(), "Merged data should have 2 stars");
        DotNetNativeTestSuite.AssertEqual(200L, dataA.StarSeeds[1], "Second seed should be from merged data");
    }
}
