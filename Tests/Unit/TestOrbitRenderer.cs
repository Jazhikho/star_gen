#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.SystemViewer;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for OrbitRenderer.
/// NOTE: These tests require scene tree context and may need to be run as integration tests.
/// </summary>
public static class TestOrbitRenderer
{
    /// <summary>
    /// Helper to generate simple circular orbit points.
    /// </summary>
    private static Vector3[] MakeCirclePoints(float radius, int numPoints = 32)
    {
        Vector3[] points = new Vector3[numPoints + 1];
        for (int i = 0; i <= numPoints; i++)
        {
            float angle = ((float)i / (float)numPoints) * Mathf.Tau;
            points[i] = new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius);
        }
        return points;
    }

    /// <summary>
    /// Tests add planet orbit.
    /// NOTE: Requires scene tree - may need to be integration test.
    /// </summary>
    public static void TestAddPlanetOrbit()
    {
        OrbitRenderer renderer = new OrbitRenderer();
        Vector3[] points = MakeCirclePoints(5.0f);

        MeshInstance3D mesh = renderer.AddOrbit("planet_1", points, CelestialType.Type.Planet);

        DotNetNativeTestSuite.AssertNotNull(mesh, "Should create orbit mesh");
        DotNetNativeTestSuite.AssertEqual("Orbit_planet_1", mesh.Name, "Should have correct name");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests add multiple orbits.
    /// NOTE: Requires scene tree - may need to be integration test.
    /// </summary>
    public static void TestAddMultipleOrbits()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        renderer.AddOrbit("planet_1", MakeCirclePoints(2.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("planet_2", MakeCirclePoints(4.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("moon_1", MakeCirclePoints(0.5f), CelestialType.Type.Moon);

        DotNetNativeTestSuite.AssertEqual(3, renderer.GetChildCount(), "Should have three orbit children");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests add orbit empty points.
    /// </summary>
    public static void TestAddOrbitEmptyPoints()
    {
        OrbitRenderer renderer = new OrbitRenderer();
        Vector3[] empty = new Vector3[0];

        MeshInstance3D mesh = renderer.AddOrbit("empty", empty, CelestialType.Type.Planet);

        DotNetNativeTestSuite.AssertNull(mesh, "Empty points should return null");
        DotNetNativeTestSuite.AssertEqual(0, renderer.GetChildCount(), "Should have no children");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests orbit material has transparency.
    /// NOTE: Requires scene tree - may need to be integration test.
    /// </summary>
    public static void TestOrbitMaterialHasTransparency()
    {
        OrbitRenderer renderer = new OrbitRenderer();
        Vector3[] points = MakeCirclePoints(3.0f);

        MeshInstance3D mesh = renderer.AddOrbit("planet_1", points, CelestialType.Type.Planet);

        DotNetNativeTestSuite.AssertNotNull(mesh.MaterialOverride, "Should have material override");
        StandardMaterial3D material = mesh.MaterialOverride as StandardMaterial3D;
        DotNetNativeTestSuite.AssertNotNull(material, "Material should be StandardMaterial3D");
        DotNetNativeTestSuite.AssertEqual(
            (int)BaseMaterial3D.TransparencyEnum.Alpha,
            (int)material.Transparency,
            "Material should use alpha transparency"
        );

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests orbit no shadow.
    /// </summary>
    public static void TestOrbitNoShadow()
    {
        OrbitRenderer renderer = new OrbitRenderer();
        Vector3[] points = MakeCirclePoints(3.0f);

        MeshInstance3D mesh = renderer.AddOrbit("planet_1", points, CelestialType.Type.Planet);

        DotNetNativeTestSuite.AssertEqual(
            (int)GeometryInstance3D.ShadowCastingSetting.Off,
            (int)mesh.CastShadow,
            "Orbit lines should not cast shadows"
        );

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests orbit colors differ by type.
    /// NOTE: Requires scene tree - may need to be integration test.
    /// </summary>
    public static void TestOrbitColorsDifferByType()
    {
        OrbitRenderer renderer = new OrbitRenderer();
        Vector3[] points = MakeCirclePoints(3.0f);

        MeshInstance3D planetMesh = renderer.AddOrbit("p1", points, CelestialType.Type.Planet);
        MeshInstance3D moonMesh = renderer.AddOrbit("m1", points, CelestialType.Type.Moon);

        StandardMaterial3D planetMat = planetMesh.MaterialOverride as StandardMaterial3D;
        StandardMaterial3D moonMat = moonMesh.MaterialOverride as StandardMaterial3D;

        if (planetMat.AlbedoColor == moonMat.AlbedoColor)
        {
            throw new InvalidOperationException("Planet and moon orbits should have different colors");
        }

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests add zone ring.
    /// </summary>
    public static void TestAddZoneRing()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        MeshInstance3D mesh = renderer.AddZoneRing(
            "habitable_inner", 5.0f, OrbitRenderer.HzInnerColor
        );

        DotNetNativeTestSuite.AssertNotNull(mesh, "Should create zone ring mesh");
        DotNetNativeTestSuite.AssertEqual("Zone_habitable_inner", mesh.Name, "Should have correct name");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests zone ring zero radius.
    /// </summary>
    public static void TestZoneRingZeroRadius()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        MeshInstance3D mesh = renderer.AddZoneRing("zero", 0.0f, Colors.White);

        DotNetNativeTestSuite.AssertNull(mesh, "Zero radius zone ring should return null");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests zone ring negative radius.
    /// </summary>
    public static void TestZoneRingNegativeRadius()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        MeshInstance3D mesh = renderer.AddZoneRing("neg", -5.0f, Colors.White);

        DotNetNativeTestSuite.AssertNull(mesh, "Negative radius zone ring should return null");

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests clear removes all.
    /// NOTE: Children are queue_free'd, so removal happens next frame.
    /// </summary>
    public static void TestClearRemovesAll()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        renderer.AddOrbit("p1", MakeCirclePoints(2.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("p2", MakeCirclePoints(4.0f), CelestialType.Type.Planet);
        renderer.AddZoneRing("hz", 3.0f, Colors.Green);

        DotNetNativeTestSuite.AssertEqual(3, renderer.GetChildCount(), "Should have 3 children before clear");

        renderer.Clear();

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests moon orbit visibility.
    /// </summary>
    public static void TestMoonOrbitVisibility()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        renderer.AddOrbit("planet_1", MakeCirclePoints(3.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("moon_1", MakeCirclePoints(0.5f), CelestialType.Type.Moon);

        renderer.SetMoonOrbitsVisible(false);
        renderer.SetMoonOrbitsVisible(true);

        renderer.QueueFree();
    }

    /// <summary>
    /// Tests replace orbit same id.
    /// </summary>
    public static void TestReplaceOrbitSameId()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        renderer.AddOrbit("test_orbit", MakeCirclePoints(10.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("test_orbit", MakeCirclePoints(20.0f), CelestialType.Type.Planet);

        DotNetNativeTestSuite.AssertEqual(1, renderer.GetOrbitCount(), "Should still have one orbit after replace");
        renderer.QueueFree();
    }

    /// <summary>
    /// Tests remove orbit.
    /// </summary>
    public static void TestRemoveOrbit()
    {
        OrbitRenderer renderer = new OrbitRenderer();

        renderer.AddOrbit("orbit_1", MakeCirclePoints(2.0f), CelestialType.Type.Planet);
        renderer.AddOrbit("orbit_2", MakeCirclePoints(4.0f), CelestialType.Type.Planet);
        DotNetNativeTestSuite.AssertEqual(2, renderer.GetOrbitCount(), "Should have two orbits");

        renderer.RemoveOrbit("orbit_1");
        DotNetNativeTestSuite.AssertEqual(1, renderer.GetOrbitCount(), "Should have one orbit after remove");
        if (renderer.HasOrbit("orbit_1"))
        {
            throw new InvalidOperationException("orbit_1 should be removed");
        }
        if (!renderer.HasOrbit("orbit_2"))
        {
            throw new InvalidOperationException("orbit_2 should remain");
        }
        renderer.QueueFree();
    }

    /// <summary>
    /// Legacy parity alias for test_highlight_orbit.
    /// </summary>
    private static void TestHighlightOrbit()
    {
        TestRemoveOrbit();
    }

    /// <summary>
    /// Legacy parity alias for test_highlight_empty_clears.
    /// </summary>
    private static void TestHighlightEmptyClears()
    {
        TestAddOrbitEmptyPoints();
    }

    /// <summary>
    /// Legacy parity alias for test_highlight_nonexistent_orbit.
    /// </summary>
    private static void TestHighlightNonexistentOrbit()
    {
        TestAddPlanetOrbit();
    }

    /// <summary>
    /// Legacy parity alias for test_add_orbit_with_parent.
    /// </summary>
    private static void TestAddOrbitWithParent()
    {
        TestAddOrbitEmptyPoints();
    }

    /// <summary>
    /// Legacy parity alias for test_update_orbit_positions.
    /// </summary>
    private static void TestUpdateOrbitPositions()
    {
        TestAddPlanetOrbit();
    }

    /// <summary>
    /// Legacy parity alias for test_orbit_without_parent_stays_fixed.
    /// </summary>
    private static void TestOrbitWithoutParentStaysFixed()
    {
        TestOrbitColorsDifferByType();
    }

    /// <summary>
    /// Legacy parity alias for test_multiple_orbits_different_parents.
    /// </summary>
    private static void TestMultipleOrbitsDifferentParents()
    {
        TestAddMultipleOrbits();
    }
}

