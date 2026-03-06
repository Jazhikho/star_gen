#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for GalaxyBodyOverrides.
/// </summary>
public static class TestGalaxyBodyOverrides
{
    /// <summary>
    /// Creates a test body with specified ID and name.
    /// </summary>
    private static CelestialBody MakeBody(string bodyId, string bodyName)
    {
        PhysicalProps phys = new PhysicalProps(5.972e24, 6.371e6);
        return new CelestialBody(bodyId, bodyName, CelestialType.Type.Planet, phys, null);
    }

    /// <summary>
    /// Tests empty state.
    /// </summary>
    public static void TestEmptyState()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        if (!o.IsEmpty())
        {
            throw new InvalidOperationException("Should be empty");
        }
        DotNetNativeTestSuite.AssertEqual(0, o.TotalCount(), "Total count should be 0");
        if (o.HasAnyFor(12345))
        {
            throw new InvalidOperationException("Should not have any for seed 12345");
        }
    }

    /// <summary>
    /// Tests set and get override.
    /// </summary>
    public static void TestSetAndGetOverride()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        CelestialBody body = MakeBody("planet_1", "Earth");
        o.SetOverride(1000, body);
        if (o.IsEmpty())
        {
            throw new InvalidOperationException("Should not be empty");
        }
        if (!o.HasAnyFor(1000))
        {
            throw new InvalidOperationException("Should have overrides for seed 1000");
        }
        DotNetNativeTestSuite.AssertEqual(1, o.TotalCount(), "Total count should be 1");
        Godot.Collections.Dictionary d = o.GetOverrideDict(1000, "planet_1");
        if (d.Count == 0)
        {
            throw new InvalidOperationException("Should have override dict");
        }
        DotNetNativeTestSuite.AssertEqual("planet_1", d.GetValueOrDefault("id", "").AsString(), "ID should match");
        CelestialBody restored = o.GetOverrideBody(1000, "planet_1");
        DotNetNativeTestSuite.AssertNotNull(restored, "Should restore body");
        DotNetNativeTestSuite.AssertEqual("planet_1", restored.Id, "Restored ID should match");
    }

    /// <summary>
    /// Tests get override missing returns empty.
    /// </summary>
    public static void TestGetOverrideMissingReturnsEmpty()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        if (o.GetOverrideDict(999, "x").Count != 0)
        {
            throw new InvalidOperationException("Missing override should return empty dict");
        }
        DotNetNativeTestSuite.AssertNull(o.GetOverrideBody(999, "x"), "Missing override should return null");
    }

    /// <summary>
    /// Tests clear override.
    /// </summary>
    public static void TestClearOverride()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        CelestialBody body = MakeBody("b1", "B1");
        o.SetOverride(1, body);
        if (!o.HasAnyFor(1))
        {
            throw new InvalidOperationException("Should have overrides for seed 1");
        }
        o.ClearOverride(1, "b1");
        if (o.HasAnyFor(1))
        {
            throw new InvalidOperationException("Should not have overrides after clear");
        }
        if (o.GetOverrideDict(1, "b1").Count != 0)
        {
            throw new InvalidOperationException("Override dict should be empty");
        }
        o.ClearOverride(1, "b1");
    }

    /// <summary>
    /// Tests multiple bodies same system.
    /// </summary>
    public static void TestMultipleBodiesSameSystem()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        o.SetOverride(50, MakeBody("p1", "P1"));
        o.SetOverride(50, MakeBody("p2", "P2"));
        DotNetNativeTestSuite.AssertEqual(2, o.TotalCount(), "Total count should be 2");
        Array<string> ids = o.GetOverriddenIds(50);
        DotNetNativeTestSuite.AssertEqual(2, ids.Count, "Should have 2 IDs");
        if (!ids.Contains("p1"))
        {
            throw new InvalidOperationException("Should contain p1");
        }
        if (!ids.Contains("p2"))
        {
            throw new InvalidOperationException("Should contain p2");
        }
    }

    /// <summary>
    /// Tests set override dict.
    /// </summary>
    public static void TestSetOverrideDict()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        Godot.Collections.Dictionary bodyDict = new Godot.Collections.Dictionary
        {
            { "id", "custom" },
            { "name", "Custom" },
            { "type", "planet" },
            { "physical", new Godot.Collections.Dictionary { { "mass_kg", 1e24 }, { "radius_m", 1e6 } } }
        };
        o.SetOverrideDict(200, "custom", bodyDict);
        if (!o.HasAnyFor(200))
        {
            throw new InvalidOperationException("Should have overrides for seed 200");
        }
        Godot.Collections.Dictionary d = o.GetOverrideDict(200, "custom");
        DotNetNativeTestSuite.AssertEqual("custom", d.GetValueOrDefault("id", "").AsString(), "ID should match");
    }

    /// <summary>
    /// Tests rejects null body.
    /// </summary>
    public static void TestRejectsNullBody()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        o.SetOverride(1, null);
        if (!o.IsEmpty())
        {
            throw new InvalidOperationException("Should remain empty after null body");
        }
    }

    /// <summary>
    /// Tests rejects empty body id.
    /// </summary>
    public static void TestRejectsEmptyBodyId()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        CelestialBody body = MakeBody("x", "X");
        body.Id = "";
        o.SetOverride(1, body);
        if (!o.IsEmpty())
        {
            throw new InvalidOperationException("Should remain empty after empty ID");
        }
    }

    /// <summary>
    /// Tests to dict from dict round trip.
    /// </summary>
    public static void TestToDictFromDictRoundTrip()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        o.SetOverride(42, MakeBody("a", "A"));
        o.SetOverride(42, MakeBody("b", "B"));
        Godot.Collections.Dictionary d = o.ToDictionary();
        if (!d.ContainsKey("42"))
        {
            throw new InvalidOperationException("Should have key 42");
        }
        GalaxyBodyOverrides restored = GalaxyBodyOverrides.FromDictionary(d);
        DotNetNativeTestSuite.AssertEqual(2, restored.TotalCount(), "Total count should be 2");
        if (!restored.HasAnyFor(42))
        {
            throw new InvalidOperationException("Should have overrides for seed 42");
        }
        DotNetNativeTestSuite.AssertEqual("A", restored.GetOverrideBody(42, "a").Name, "Name should match");
    }

    /// <summary>
    /// Tests apply to bodies replaces matching.
    /// </summary>
    public static void TestApplyToBodiesReplacesMatching()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        CelestialBody edited = MakeBody("p1", "Edited Planet");
        o.SetOverride(10, edited);
        CelestialBody original = MakeBody("p1", "Original");
        Array<CelestialBody> bodies = new Array<CelestialBody> { original };
        int replaced = o.ApplyToBodies(10, bodies);
        DotNetNativeTestSuite.AssertEqual(1, replaced, "Should replace 1 body");
        DotNetNativeTestSuite.AssertEqual("Edited Planet", bodies[0].Name, "Name should be updated");
    }

    /// <summary>
    /// Tests apply to bodies no op wrong seed.
    /// </summary>
    public static void TestApplyToBodiesNoOpWrongSeed()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        o.SetOverride(10, MakeBody("p1", "X"));
        CelestialBody body = MakeBody("p1", "Y");
        Array<CelestialBody> bodies = new Array<CelestialBody> { body };
        int replaced = o.ApplyToBodies(99, bodies);
        DotNetNativeTestSuite.AssertEqual(0, replaced, "Should replace 0 bodies");
        DotNetNativeTestSuite.AssertEqual("Y", bodies[0].Name, "Name should remain unchanged");
    }

    /// <summary>
    /// Tests apply to bodies handles nulls in array.
    /// </summary>
    public static void TestApplyToBodiesHandlesNullsInArray()
    {
        GalaxyBodyOverrides o = new GalaxyBodyOverrides();
        o.SetOverride(1, MakeBody("p1", "P1"));
        Array<CelestialBody> bodies = new Array<CelestialBody> { null, MakeBody("p1", "Old"), null };
        int replaced = o.ApplyToBodies(1, bodies);
        DotNetNativeTestSuite.AssertEqual(1, replaced, "Should replace 1 body");
        DotNetNativeTestSuite.AssertNull(bodies[0], "First element should remain null");
        DotNetNativeTestSuite.AssertNotNull(bodies[1], "Second element should not be null");
        DotNetNativeTestSuite.AssertNull(bodies[2], "Third element should remain null");
    }
}
