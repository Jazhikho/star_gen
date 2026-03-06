#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for RaycastUtils — ray-AABB intersection correctness.
/// </summary>
public static class TestRaycastUtils
{
    private static readonly Vector3 UnitMin = new Vector3(-1.0f, -1.0f, -1.0f);
    private static readonly Vector3 UnitMax = new Vector3(1.0f, 1.0f, 1.0f);

    public static void TestRayHitsAabbFromFront()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 0.0f, 1.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(4.0, dist, 0.001, "Should hit front face at distance 4");
    }

    public static void TestRayHitsAabbFromSide()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(4.0, dist, 0.001, "Should hit left face at distance 4");
    }

    public static void TestRayHitsAabbFromAbove()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(4.0, dist, 0.001, "Should hit top face at distance 4");
    }

    public static void TestRayMissesAabbPointingAway()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(RaycastUtils.NoHit, dist, 0.001, "Should miss when pointing away");
    }

    public static void TestRayMissesAabbBehind()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(0.0f, 0.0f, 5.0f), new Vector3(0.0f, 0.0f, 1.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(RaycastUtils.NoHit, dist, 0.001,
            "Should miss when AABB is behind ray");
    }

    public static void TestRayOriginInsideAabb()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(0.0, dist, 0.001, "Should return 0 when ray starts inside AABB");
    }

    public static void TestRayParallelToSlabInside()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(4.0, dist, 0.001, "Parallel ray inside slab should hit");
    }

    public static void TestRayParallelToSlabOutside()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, 5.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(RaycastUtils.NoHit, dist, 0.001,
            "Parallel ray outside slab should miss");
    }

    public static void TestDiagonalRayHits()
    {
        Vector3 direction = new Vector3(1.0f, 1.0f, 1.0f).Normalized();
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(-5.0f, -5.0f, -5.0f), direction,
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertGreaterThan(dist, 0.0f, "Diagonal ray toward AABB should hit");
    }

    public static void TestOffsetAabb()
    {
        Vector3 aabbMin = new Vector3(10.0f, 10.0f, 10.0f);
        Vector3 aabbMax = new Vector3(12.0f, 12.0f, 12.0f);
        Vector3 target = new Vector3(11.0f, 11.0f, 11.0f);
        Vector3 origin = new Vector3(11.0f, 11.0f, 0.0f);
        Vector3 direction = (target - origin).Normalized();

        float dist = RaycastUtils.RayIntersectsAabb(origin, direction, aabbMin, aabbMax);
        DotNetNativeTestSuite.AssertFloatNear(10.0, dist, 0.001, "Should hit offset AABB at distance 10");
    }

    public static void TestNearMiss()
    {
        float dist = RaycastUtils.RayIntersectsAabb(
            new Vector3(1.1f, 1.1f, -5.0f), new Vector3(0.0f, 0.0f, 1.0f),
            UnitMin, UnitMax);
        DotNetNativeTestSuite.AssertFloatNear(RaycastUtils.NoHit, dist, 0.001,
            "Ray just outside corner should miss");
    }
}
