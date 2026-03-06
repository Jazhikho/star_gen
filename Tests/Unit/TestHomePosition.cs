#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for HomePosition utilities.
/// </summary>
public static class TestHomePosition
{
    /// <summary>
    /// Tests default position distance from center.
    /// </summary>
    public static void TestDefaultPositionDistanceFromCenter()
    {
        Vector3 pos = HomePosition.GetDefaultPosition();
        double radialDistance = Math.Sqrt(pos.X * pos.X + pos.Z * pos.Z);

        DotNetNativeTestSuite.AssertFloatNear(HomePosition.SolarDistancePc, radialDistance, 1.0,
            "Home position should be ~8000 pc from center");
    }

    /// <summary>
    /// Tests default position height.
    /// </summary>
    public static void TestDefaultPositionHeight()
    {
        Vector3 pos = HomePosition.GetDefaultPosition();

        DotNetNativeTestSuite.AssertFloatNear(HomePosition.SolarHeightPc, pos.Y, 0.1,
            "Home position should be ~20 pc above disk plane");
    }

    /// <summary>
    /// Tests home quadrant is valid.
    /// </summary>
    public static void TestHomeQuadrantIsValid()
    {
        Vector3I quadrant = HomePosition.GetHomeQuadrant();

        if (quadrant.X <= 5)
        {
            throw new InvalidOperationException("Quadrant X should be positive and significant");
        }
        if (quadrant.X >= 10)
        {
            throw new InvalidOperationException("Quadrant X should be less than 10");
        }
    }

    /// <summary>
    /// Tests home hierarchy is consistent.
    /// </summary>
    public static void TestHomeHierarchyIsConsistent()
    {
        GalaxyCoordinates.HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();
        Vector3I quadrant = HomePosition.GetHomeQuadrant();

        DotNetNativeTestSuite.AssertEqual(quadrant, hierarchy.QuadrantCoords,
            "Hierarchy quadrant should match direct quadrant calculation");
    }

    /// <summary>
    /// Tests home sector coords in range.
    /// </summary>
    public static void TestHomeSectorCoordsInRange()
    {
        GalaxyCoordinates.HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();

        if (hierarchy.SectorLocalCoords.X < 0)
        {
            throw new InvalidOperationException("Sector X >= 0");
        }
        if (hierarchy.SectorLocalCoords.X >= 10)
        {
            throw new InvalidOperationException("Sector X < 10");
        }
        if (hierarchy.SectorLocalCoords.Y < 0)
        {
            throw new InvalidOperationException("Sector Y >= 0");
        }
        if (hierarchy.SectorLocalCoords.Y >= 10)
        {
            throw new InvalidOperationException("Sector Y < 10");
        }
        if (hierarchy.SectorLocalCoords.Z < 0)
        {
            throw new InvalidOperationException("Sector Z >= 0");
        }
        if (hierarchy.SectorLocalCoords.Z >= 10)
        {
            throw new InvalidOperationException("Sector Z < 10");
        }
    }

    /// <summary>
    /// Tests home subsector coords in range.
    /// </summary>
    public static void TestHomeSubsectorCoordsInRange()
    {
        GalaxyCoordinates.HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();

        if (hierarchy.SubsectorLocalCoords.X < 0)
        {
            throw new InvalidOperationException("Subsector X >= 0");
        }
        if (hierarchy.SubsectorLocalCoords.X >= 10)
        {
            throw new InvalidOperationException("Subsector X < 10");
        }
        if (hierarchy.SubsectorLocalCoords.Y < 0)
        {
            throw new InvalidOperationException("Subsector Y >= 0");
        }
        if (hierarchy.SubsectorLocalCoords.Y >= 10)
        {
            throw new InvalidOperationException("Subsector Y < 10");
        }
        if (hierarchy.SubsectorLocalCoords.Z < 0)
        {
            throw new InvalidOperationException("Subsector Z >= 0");
        }
        if (hierarchy.SubsectorLocalCoords.Z >= 10)
        {
            throw new InvalidOperationException("Subsector Z < 10");
        }
    }

    /// <summary>
    /// Tests home sector origin is within galaxy.
    /// </summary>
    public static void TestHomeSectorOriginIsWithinGalaxy()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        Vector3 origin = HomePosition.GetHomeSectorOrigin();

        if (!HomePosition.IsWithinGalaxy(origin, spec))
        {
            throw new InvalidOperationException("Home sector origin should be within galaxy bounds");
        }
    }

    /// <summary>
    /// Tests home sector center is near default position.
    /// </summary>
    public static void TestHomeSectorCenterIsNearDefaultPosition()
    {
        Vector3 center = HomePosition.GetHomeSectorCenter();
        Vector3 defaultPos = HomePosition.GetDefaultPosition();

        float distance = center.DistanceTo(defaultPos);
        float maxDistance = (float)(GalaxyCoordinates.SectorSizePc * 1.5);

        if (distance >= maxDistance)
        {
            throw new InvalidOperationException("Sector center should be near default position");
        }
    }

    /// <summary>
    /// Tests home subsector center is near default position.
    /// </summary>
    public static void TestHomeSubsectorCenterIsNearDefaultPosition()
    {
        Vector3 center = HomePosition.GetHomeSubsectorCenter();
        Vector3 defaultPos = HomePosition.GetDefaultPosition();

        float distance = center.DistanceTo(defaultPos);
        float maxDistance = (float)(GalaxyCoordinates.SubsectorSizePc * 2.0);

        if (distance >= maxDistance)
        {
            throw new InvalidOperationException("Subsector center should be near default position");
        }
    }

    /// <summary>
    /// Tests is within galaxy accepts valid position.
    /// </summary>
    public static void TestIsWithinGalaxyAcceptsValidPosition()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        Vector3 validPos = new Vector3(5000.0f, 100.0f, 3000.0f);

        if (!HomePosition.IsWithinGalaxy(validPos, spec))
        {
            throw new InvalidOperationException("Position within bounds should be valid");
        }
    }

    /// <summary>
    /// Tests is within galaxy rejects too far.
    /// </summary>
    public static void TestIsWithinGalaxyRejectsTooFar()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        Vector3 farPos = new Vector3(20000.0f, 0.0f, 0.0f);

        if (HomePosition.IsWithinGalaxy(farPos, spec))
        {
            throw new InvalidOperationException("Position beyond radius should be invalid");
        }
    }

    /// <summary>
    /// Tests is within galaxy rejects too high.
    /// </summary>
    public static void TestIsWithinGalaxyRejectsTooHigh()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        Vector3 highPos = new Vector3(5000.0f, 2000.0f, 0.0f);

        if (HomePosition.IsWithinGalaxy(highPos, spec))
        {
            throw new InvalidOperationException("Position beyond height should be invalid");
        }
    }

    /// <summary>
    /// Tests default position is deterministic.
    /// </summary>
    public static void TestDefaultPositionIsDeterministic()
    {
        Vector3 pos1 = HomePosition.GetDefaultPosition();
        Vector3 pos2 = HomePosition.GetDefaultPosition();

        if (!pos1.IsEqualApprox(pos2))
        {
            throw new InvalidOperationException("Default position should be deterministic");
        }
    }
}
