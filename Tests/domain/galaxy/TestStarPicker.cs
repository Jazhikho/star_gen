#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for StarPicker — ray-based star selection.
/// </summary>
public static class TestStarPicker
{
    private static Vector3[] _positions;
    private static long[] _seeds;

    private static void BeforeEach()
    {
        _positions = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 10.0f),
            new Vector3(5.0f, 0.0f, 10.0f),
            new Vector3(0.0f, 5.0f, 10.0f),
            new Vector3(100.0f, 100.0f, 100.0f),
        };
        _seeds = new long[] { 111, 222, 333, 444 };
    }

    public static void TestPicksStarDirectlyOnRay()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 1.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Should pick star directly on the ray");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertEqual(0, pick.StarIndex, "Should pick star at index 0");
        DotNetNativeTestSuite.AssertEqual(111L, pick.StarSeed, "Should return correct seed");
        DotNetNativeTestSuite.AssertFloatNear(0.0, pick.LateralDistance, 0.01, "Lateral distance should be 0");
    }

    public static void TestPicksNearestWhenMultipleInRange()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 6.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Should pick a star");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertEqual(0, pick.StarIndex, "Should pick the closest star to the ray");
    }

    public static void TestReturnsNullWhenNoneInRange()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(50.0f, 50.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 1.0f);
        DotNetNativeTestSuite.AssertNull(result, "Should return null when no stars are near the ray");
    }

    public static void TestIgnoresStarsBehindRay()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(0.0f, 0.0f, 20.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 5.0f);
        DotNetNativeTestSuite.AssertNull(result, "Should not pick stars behind the ray origin");
    }

    public static void TestReturnsNullForEmptyArrays()
    {
        Vector3[] emptyPos = System.Array.Empty<Vector3>();
        long[] emptySeeds = System.Array.Empty<long>();
        StarPickResult? result = StarPicker.PickNearestToRay(
            Vector3.Zero, new Vector3(0.0f, 0.0f, 1.0f),
            emptyPos, emptySeeds, 5.0f);
        DotNetNativeTestSuite.AssertNull(result, "Should return null for empty star arrays");
    }

    public static void TestPickResultHasCorrectPosition()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(4.5f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 1.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Should pick nearby star");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertEqual(1, pick.StarIndex, "Should pick star at (5,0,10)");
        DotNetNativeTestSuite.AssertTrue(
            pick.WorldPosition.IsEqualApprox(new Vector3(5.0f, 0.0f, 10.0f)),
            "Position should match star position");
    }

    public static void TestRayDistanceIsPositive()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 1.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Should pick a star");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertFloatNear(10.0, pick.RayDistance, 0.01, "Ray distance should be 10");
    }

    public static void TestLateralDistanceCorrect()
    {
        BeforeEach();
        StarPickResult? result = StarPicker.PickNearestToRay(
            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f),
            _positions, _seeds, 6.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Should pick a star");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertFloatNear(0.0, pick.LateralDistance, 0.01, "Should be right on the ray");
    }

    public static void TestPicksFromDiagonalRay()
    {
        BeforeEach();
        Vector3 direction = new Vector3(5.0f, 0.0f, 10.0f).Normalized();
        StarPickResult? result = StarPicker.PickNearestToRay(
            Vector3.Zero, direction, _positions, _seeds, 1.0f);
        DotNetNativeTestSuite.AssertNotNull(result, "Diagonal ray should reach star at (5,0,10)");
        StarPickResult pick = result!;
        DotNetNativeTestSuite.AssertEqual(1, pick.StarIndex, "Should pick star aligned with diagonal ray");
    }
}
