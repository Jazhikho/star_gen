#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for GridCursor — directional navigation through occupied grids.
/// </summary>
public static class TestGridCursor
{
    private static Vector3I[] _occupied;

    private static void BeforeEach()
    {
        _occupied = new Vector3I[]
        {
            new Vector3I(0, 0, 0),
            new Vector3I(1, 0, 0),
            new Vector3I(2, 0, 0),
            new Vector3I(0, 1, 0),
            new Vector3I(0, 0, 1),
            new Vector3I(-1, 0, 0),
            new Vector3I(0, -1, 0),
            new Vector3I(3, 2, 1),
        };
    }

    public static void TestFindNearestPositiveX()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find a coord in +X direction");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), result!.Value,
            "Should find the nearest in +X");
    }

    public static void TestFindNearestNegativeX()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(-1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find a coord in -X direction");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(-1, 0, 0), result!.Value,
            "Should find the nearest in -X");
    }

    public static void TestFindNearestPositiveY()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(0, 1, 0), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find a coord in +Y direction");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 1, 0), result!.Value,
            "Should find the nearest in +Y");
    }

    public static void TestFindNearestPositiveZ()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(0, 0, 1), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find a coord in +Z direction");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 1), result!.Value,
            "Should find the nearest in +Z");
    }

    public static void TestReturnsNullWhenNothingInDirection()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(3, 2, 1), new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertNull(result, "Should return null when no coords in +X from (3,2,1)");
    }

    public static void TestReturnsNullForEmptyList()
    {
        Vector3I[] empty = System.Array.Empty<Vector3I>();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), empty);
        DotNetNativeTestSuite.AssertNull(result, "Should return null for empty occupied list");
    }

    public static void TestSkipsCurrentPosition()
    {
        Vector3I[] single = new Vector3I[] { new Vector3I(0, 0, 0) };
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), single);
        DotNetNativeTestSuite.AssertNull(result, "Should not return current position");
    }

    public static void TestFindsClosestAmongMultipleCandidates()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), result!.Value,
            "Should pick closest candidate in +X direction");
    }

    public static void TestFindsDiagonalCandidateInDirection()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearestInDirection(
            new Vector3I(2, 0, 0), new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find diagonal candidate in +X");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(3, 2, 1), result!.Value,
            "Should find (3,2,1) as next in +X from (2,0,0)");
    }

    public static void TestFindNearestToPosition()
    {
        BeforeEach();
        Vector3I? result = GridCursor.FindNearest(new Vector3I(1, 1, 0), _occupied);
        DotNetNativeTestSuite.AssertNotNull(result, "Should find nearest overall");
        Vector3I coords = result!.Value;
        Vector3I delta = coords - new Vector3I(1, 1, 0);
        float distSq = delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z;
        DotNetNativeTestSuite.AssertFloatNear(1.0, distSq, 0.01,
            "Nearest should be at distance 1 from (1,1,0)");
    }

    public static void TestFindNearestEmptyList()
    {
        Vector3I[] empty = System.Array.Empty<Vector3I>();
        Vector3I? result = GridCursor.FindNearest(new Vector3I(0, 0, 0), empty);
        DotNetNativeTestSuite.AssertNull(result, "Should return null for empty list");
    }

    public static void TestMoveUpdatesPosition()
    {
        GridCursor cursor = new GridCursor
        {
            Position = new Vector3I(0, 0, 0),
        };
        BeforeEach();
        Vector3I? result = cursor.MoveInDirection(new Vector3I(1, 0, 0), _occupied);

        DotNetNativeTestSuite.AssertNotNull(result, "Move should succeed");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), cursor.Position,
            "Cursor position should update after move");
    }

    public static void TestMoveReturnsNullWhenBlocked()
    {
        GridCursor cursor = new GridCursor
        {
            Position = new Vector3I(3, 2, 1),
        };
        BeforeEach();
        Vector3I? result = cursor.MoveInDirection(new Vector3I(1, 0, 0), _occupied);

        DotNetNativeTestSuite.AssertNull(result, "Move should return null when no target in direction");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(3, 2, 1), cursor.Position,
            "Cursor position should not change on failed move");
    }

    public static void TestSnapToNearest()
    {
        GridCursor cursor = new GridCursor
        {
            Position = new Vector3I(5, 5, 5),
        };
        BeforeEach();
        Vector3I? result = cursor.SnapToNearest(_occupied);

        DotNetNativeTestSuite.AssertNotNull(result, "Snap should find a target");
        DotNetNativeTestSuite.AssertEqual(cursor.Position, result!.Value,
            "Cursor position should match snap result");
    }

    public static void TestSequentialMoves()
    {
        GridCursor cursor = new GridCursor
        {
            Position = new Vector3I(0, 0, 0),
        };
        BeforeEach();

        cursor.MoveInDirection(new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), cursor.Position, "First move +X");

        cursor.MoveInDirection(new Vector3I(1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertEqual(new Vector3I(2, 0, 0), cursor.Position, "Second move +X");

        cursor.MoveInDirection(new Vector3I(-1, 0, 0), _occupied);
        DotNetNativeTestSuite.AssertEqual(new Vector3I(1, 0, 0), cursor.Position, "Move back -X");
    }
}
