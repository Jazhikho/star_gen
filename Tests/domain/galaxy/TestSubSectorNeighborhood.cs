#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for SubSectorNeighborhood — 11x11x11 grid generation, shells, and boundary handling.
/// </summary>
public static class TestSubSectorNeighborhood
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

    public static void TestProduces1331SubsectorOrigins()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, _refDensity);
        DotNetNativeTestSuite.AssertEqual(1331, data.SubsectorOrigins.Length,
            "Should have exactly 1331 subsector origins for 11x11x11 grid");
    }

    public static void TestProduces1331ShellTags()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, _refDensity);
        DotNetNativeTestSuite.AssertEqual(1331, data.SubsectorShells.Length,
            "Should have 1331 shell tags matching origins");
    }

    public static void TestCenterOriginMatchesCameraSubsector()
    {
        BeforeEach();
        Vector3 cameraPos = new Vector3(505.0f, 505.0f, 505.0f);
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            cameraPos, 42, _model, _refDensity);
        Vector3 expectedOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(cameraPos);
        DotNetNativeTestSuite.AssertTrue(data.CenterOrigin.IsEqualApprox(expectedOrigin),
            "Center origin should match camera's subsector origin");
    }

    public static void TestShell0IsCenterOnly()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell0Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 0)
            {
                shell0Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(1, shell0Count, "Only 1 subsector should be in shell 0 (center)");
    }

    public static void TestShell1Has26Subsectors()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell1Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 1)
            {
                shell1Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(26, shell1Count, "Shell 1 should have 26 subsectors");
    }

    public static void TestShell2Has98Subsectors()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell2Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 2)
            {
                shell2Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(98, shell2Count, "Shell 2 should have 98 subsectors");
    }

    public static void TestShell3Has218Subsectors()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell3Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 3)
            {
                shell3Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(218, shell3Count, "Shell 3 should have 218 subsectors");
    }

    public static void TestShell4Has386Subsectors()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell4Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 4)
            {
                shell4Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(386, shell4Count, "Shell 4 should have 386 subsectors");
    }

    public static void TestShell5Has602Subsectors()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(505.0f, 505.0f, 505.0f), 42, _model, _refDensity);
        int shell5Count = 0;
        for (int i = 0; i < data.SubsectorShells.Length; i += 1)
        {
            if (data.SubsectorShells[i] == 5)
            {
                shell5Count += 1;
            }
        }
        DotNetNativeTestSuite.AssertEqual(602, shell5Count, "Shell 5 should have 602 subsectors");
    }

    public static void TestStarShellsMatchCount()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, _refDensity);
        DotNetNativeTestSuite.AssertEqual(data.StarPositions.Length, data.StarShells.Length,
            "star_shells must have same count as star_positions");
        DotNetNativeTestSuite.AssertEqual(data.StarSeeds.Length, data.StarShells.Length,
            "star_shells must have same count as star_seeds");
    }

    public static void TestStarShellsAreValidRange()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, _refDensity);
        for (int i = 0; i < data.StarShells.Length; i += 1)
        {
            DotNetNativeTestSuite.AssertInRange(data.StarShells[i], 0, 5,
                $"Star shell {i} must be in range [0, 5]");
        }
    }

    public static void TestDeterministic()
    {
        BeforeEach();
        Vector3 pos = new Vector3(300.0f, 50.0f, 300.0f);
        SubSectorNeighborhoodData dataA = SubSectorNeighborhood.Build(
            pos, 42, _model, _refDensity);
        SubSectorNeighborhoodData dataB = SubSectorNeighborhood.Build(
            pos, 42, _model, _refDensity);

        DotNetNativeTestSuite.AssertEqual(dataA.GetStarCount(), dataB.GetStarCount(),
            "Same inputs must produce same star count");

        for (int i = 0; i < Math.Min(10, dataA.GetStarCount()); i += 1)
        {
            DotNetNativeTestSuite.AssertTrue(
                dataA.StarPositions[i].IsEqualApprox(dataB.StarPositions[i]),
                $"Star position {i} must be identical");
            DotNetNativeTestSuite.AssertEqual(dataA.StarShells[i], dataB.StarShells[i],
                $"Star shell {i} must be identical");
        }
    }

    public static void TestDifferentPositionDifferentNeighborhood()
    {
        BeforeEach();
        SubSectorNeighborhoodData dataA = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, _refDensity);
        SubSectorNeighborhoodData dataB = SubSectorNeighborhood.Build(
            new Vector3(5000.0f, 500.0f, 5000.0f), 42, _model, _refDensity);

        DotNetNativeTestSuite.AssertFalse(
            dataA.CenterOrigin.IsEqualApprox(dataB.CenterOrigin),
            "Different positions should have different centers");
    }

    public static void TestOriginsForm11x11x11Grid()
    {
        BeforeEach();
        Vector3 cameraPos = new Vector3(505.0f, 505.0f, 505.0f);
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            cameraPos, 42, _model, _refDensity);

        float ssSize = (float)GalaxyCoordinates.SubsectorSizePc;
        Vector3 center = data.CenterOrigin;

        int expectedCount = 0;
        for (int dx = -5; dx <= 5; dx += 1)
        {
            for (int dy = -5; dy <= 5; dy += 1)
            {
                for (int dz = -5; dz <= 5; dz += 1)
                {
                    Vector3 expected = center + new Vector3(
                        dx * ssSize,
                        dy * ssSize,
                        dz * ssSize);
                    bool found = false;
                    foreach (Vector3 origin in data.SubsectorOrigins)
                    {
                        if (origin.IsEqualApprox(expected))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        expectedCount += 1;
                    }
                }
            }
        }

        DotNetNativeTestSuite.AssertEqual(1331, expectedCount, "All 1331 grid positions should be present");
    }

    public static void TestHandlesSectorBoundaryCrossing()
    {
        BeforeEach();
        Vector3 cameraPos = new Vector3(95.0f, 5.0f, 5.0f);
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            cameraPos, 42, _model, _refDensity);

        bool hasSector0 = false;
        bool hasSector1 = false;
        foreach (Vector3 origin in data.SubsectorOrigins)
        {
            Vector3 ssCenter = origin + Vector3.One * (float)GalaxyCoordinates.SubsectorSizePc * 0.5f;
            HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(ssCenter);
            if (hierarchy.SectorLocalCoords.X == 0)
            {
                hasSector0 = true;
            }
            else if (hierarchy.SectorLocalCoords.X == 1)
            {
                hasSector1 = true;
            }
        }

        DotNetNativeTestSuite.AssertTrue(hasSector0, "Should have subsectors in sector 0");
        DotNetNativeTestSuite.AssertTrue(hasSector1, "Should have subsectors crossing into sector 1");
    }

    public static void TestHandlesQuadrantBoundaryCrossing()
    {
        BeforeEach();
        Vector3 cameraPos = new Vector3(995.0f, 5.0f, 5.0f);
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            cameraPos, 42, _model, _refDensity);

        bool hasQuadrant0 = false;
        bool hasQuadrant1 = false;
        foreach (Vector3 origin in data.SubsectorOrigins)
        {
            Vector3 ssCenter = origin + Vector3.One * (float)GalaxyCoordinates.SubsectorSizePc * 0.5f;
            HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(ssCenter);
            if (hierarchy.QuadrantCoords.X == 0)
            {
                hasQuadrant0 = true;
            }
            else if (hierarchy.QuadrantCoords.X == 1)
            {
                hasQuadrant1 = true;
            }
        }

        DotNetNativeTestSuite.AssertTrue(hasQuadrant0, "Should have subsectors in quadrant 0");
        DotNetNativeTestSuite.AssertTrue(hasQuadrant1, "Should have subsectors crossing into quadrant 1");
    }

    public static void TestGetCenterOriginMatchesBuild()
    {
        BeforeEach();
        Vector3 cameraPos = new Vector3(505.0f, 55.0f, 305.0f);
        Vector3 center = SubSectorNeighborhood.GetCenterOrigin(cameraPos);
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            cameraPos, 42, _model, _refDensity);
        DotNetNativeTestSuite.AssertTrue(center.IsEqualApprox(data.CenterOrigin),
            "get_center_origin should match build center");
    }

    public static void TestProducesStarsNearGalacticCenter()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(5.0f, 5.0f, 5.0f), 42, _model, _refDensity);
        DotNetNativeTestSuite.AssertGreaterThan(data.GetStarCount(), 0,
            "Should produce stars near galactic center");
    }

    public static void TestZeroReferenceDensityProducesNoStars()
    {
        BeforeEach();
        SubSectorNeighborhoodData data = SubSectorNeighborhood.Build(
            new Vector3(500.0f, 500.0f, 500.0f), 42, _model, 0.0f);
        DotNetNativeTestSuite.AssertEqual(0, data.GetStarCount(),
            "Zero reference density should produce no stars");
    }

    /// <summary>
    /// Legacy parity alias for test_produces_343_subsector_origins.
    /// </summary>
    private static void TestProduces343SubsectorOrigins()
    {
        TestProduces1331SubsectorOrigins();
    }

    /// <summary>
    /// Legacy parity alias for test_produces_343_shell_tags.
    /// </summary>
    private static void TestProduces343ShellTags()
    {
        TestProduces1331ShellTags();
    }
}

