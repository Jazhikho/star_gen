#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for QuadrantSelector — ray-based click picking and selection state.
/// </summary>
public static class TestQuadrantSelector
{
    private static QuadrantSelector _selector;

    private static void BeforeEach()
    {
        _selector = new QuadrantSelector();
    }

    public static void TestPicksQuadrantWhenRayHits()
    {
        BeforeEach();
        Vector3 target = new Vector3(500.0f, 500.0f, 500.0f);
        Vector3 rayOrigin = new Vector3(500.0f, 500.0f, -5000.0f);
        Vector3 rayDirection = (target - rayOrigin).Normalized();

        Array<Vector3I> coords = new Array<Vector3I> { new Vector3I(0, 0, 0) };
        Variant result = _selector.PickFromRay(rayOrigin, rayDirection, coords);

        DotNetNativeTestSuite.AssertTrue(result.VariantType != Variant.Type.Nil, "Should pick the quadrant the ray hits");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, 0), result.As<Vector3I>(), "Should pick quadrant (0,0,0)");
    }

    public static void TestReturnsNullWhenRayMissesAll()
    {
        BeforeEach();
        Vector3 rayOrigin = new Vector3(500.0f, -5000.0f, 500.0f);
        Vector3 rayDirection = new Vector3(1.0f, 0.0f, 0.0f);

        Array<Vector3I> coords = new Array<Vector3I> { new Vector3I(0, 0, 0) };
        Variant result = _selector.PickFromRay(rayOrigin, rayDirection, coords);

        DotNetNativeTestSuite.AssertTrue(result.VariantType == Variant.Type.Nil, "Should return null when ray misses all quadrants");
    }

    public static void TestReturnsNullForEmptyCoordsList()
    {
        BeforeEach();
        Vector3 rayOrigin = new Vector3(0.0f, 0.0f, -5000.0f);
        Vector3 rayDirection = new Vector3(0.0f, 0.0f, 1.0f);

        Array<Vector3I> coords = new Array<Vector3I>();
        Variant result = _selector.PickFromRay(rayOrigin, rayDirection, coords);

        DotNetNativeTestSuite.AssertTrue(result.VariantType == Variant.Type.Nil, "Should return null for empty occupied coords");
    }

    public static void TestPicksNearestWhenRayHitsMultiple()
    {
        BeforeEach();
        Vector3 rayOrigin = new Vector3(500.0f, 500.0f, -5000.0f);
        Vector3 rayDirection = new Vector3(0.0f, 0.0f, 1.0f);

        Array<Vector3I> coords = new Array<Vector3I>
        {
            new Vector3I(0, 0, 0),
            new Vector3I(0, 0, 2),
            new Vector3I(0, 0, -3),
        };
        Variant result = _selector.PickFromRay(rayOrigin, rayDirection, coords);

        DotNetNativeTestSuite.AssertTrue(result.VariantType != Variant.Type.Nil, "Should pick the nearest quadrant");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(0, 0, -3), result.As<Vector3I>(),
            "Should pick the closest quadrant along the ray");
    }

    public static void TestPicksCorrectQuadrantAtNegativeCoords()
    {
        BeforeEach();
        Vector3 target = new Vector3(-1500.0f, -500.0f, -500.0f);
        Vector3 rayOrigin = new Vector3(-1500.0f, -500.0f, -5000.0f);
        Vector3 rayDirection = (target - rayOrigin).Normalized();

        Array<Vector3I> coords = new Array<Vector3I>
        {
            new Vector3I(0, 0, 0),
            new Vector3I(-2, -1, -1),
            new Vector3I(3, 0, 2),
        };
        Variant result = _selector.PickFromRay(rayOrigin, rayDirection, coords);

        DotNetNativeTestSuite.AssertTrue(result.VariantType != Variant.Type.Nil, "Should pick negative-coord quadrant");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(-2, -1, -1), result.As<Vector3I>(),
            "Should pick the quadrant at negative coordinates");
    }

    public static void TestInitialSelectionIsNull()
    {
        BeforeEach();
        DotNetNativeTestSuite.AssertTrue(_selector.SelectedCoords.VariantType == Variant.Type.Nil, "Should start with no selection");
        DotNetNativeTestSuite.AssertFalse(_selector.HasSelection(), "has_selection should be false initially");
    }

    public static void TestSetSelection()
    {
        BeforeEach();
        _selector.SetSelection(Variant.CreateFrom(new Vector3I(1, 2, 3)));
        DotNetNativeTestSuite.AssertTrue(_selector.HasSelection(), "has_selection should be true after set");
        DotNetNativeTestSuite.AssertEqual(
            new Vector3I(1, 2, 3), _selector.SelectedCoords.As<Vector3I>(),
            "selected_coords should match what was set");
    }

    public static void TestClearSelection()
    {
        BeforeEach();
        _selector.SetSelection(Variant.CreateFrom(new Vector3I(5, 5, 5)));
        _selector.ClearSelection();
        DotNetNativeTestSuite.AssertTrue(_selector.SelectedCoords.VariantType == Variant.Type.Nil, "Should be null after clear");
        DotNetNativeTestSuite.AssertFalse(_selector.HasSelection(), "has_selection should be false after clear");
    }

    public static void TestSetSelectionToNull()
    {
        BeforeEach();
        _selector.SetSelection(Variant.CreateFrom(new Vector3I(1, 1, 1)));
        _selector.SetSelection(default);
        DotNetNativeTestSuite.AssertTrue(_selector.SelectedCoords.VariantType == Variant.Type.Nil, "Setting null should clear selection");
        DotNetNativeTestSuite.AssertFalse(_selector.HasSelection(), "has_selection should be false after null set");
    }
}
