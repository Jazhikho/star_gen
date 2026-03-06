#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for OrbitSlot.
/// </summary>
public static class TestOrbitSlot
{
    private const double DefaultTolerance = 0.01;

    /// <summary>
    /// Tests basic construction.
    /// </summary>
    public static void TestConstruction()
    {
        OrbitSlot slot = new OrbitSlot("slot_1", "host_1", 1.5e11);

        if (slot.Id != "slot_1")
        {
            throw new InvalidOperationException("Expected id slot_1");
        }
        if (slot.OrbitHostId != "host_1")
        {
            throw new InvalidOperationException("Expected orbit_host_id host_1");
        }
        if (slot.SemiMajorAxisM != 1.5e11)
        {
            throw new InvalidOperationException("Expected semi_major_axis_m 1.5e11");
        }
        if (System.Math.Abs(slot.SuggestedEccentricity - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("Expected default suggested_eccentricity 0.0");
        }
        if (!slot.IsStable)
        {
            throw new InvalidOperationException("Expected default is_stable true");
        }
        if (slot.IsFilled)
        {
            throw new InvalidOperationException("Expected default is_filled false");
        }
    }

    /// <summary>
    /// Tests get semi major axis au.
    /// </summary>
    public static void TestGetSemiMajorAxisAu()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", Units.AuMeters * 2.5);

        if (System.Math.Abs(slot.GetSemiMajorAxisAu() - 2.5) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 2.5 AU, got {slot.GetSemiMajorAxisAu()}");
        }
    }

    /// <summary>
    /// Tests zone string conversion.
    /// </summary>
    public static void TestGetZoneString()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", 1e11);

        slot.Zone = OrbitZone.Zone.Hot;
        if (slot.GetZoneString() != "Hot")
        {
            throw new InvalidOperationException("Expected Hot");
        }

        slot.Zone = OrbitZone.Zone.Temperate;
        if (slot.GetZoneString() != "Temperate")
        {
            throw new InvalidOperationException("Expected Temperate");
        }

        slot.Zone = OrbitZone.Zone.Cold;
        if (slot.GetZoneString() != "Cold")
        {
            throw new InvalidOperationException("Expected Cold");
        }
    }

    /// <summary>
    /// Tests fill with planet.
    /// </summary>
    public static void TestFillWithPlanet()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", 1e11);

        if (slot.IsFilled)
        {
            throw new InvalidOperationException("Should not be filled initially");
        }
        if (slot.PlanetId != "")
        {
            throw new InvalidOperationException("Planet ID should be empty");
        }

        slot.FillWithPlanet("planet_42");

        if (!slot.IsFilled)
        {
            throw new InvalidOperationException("Should be filled after fill_with_planet");
        }
        if (slot.PlanetId != "planet_42")
        {
            throw new InvalidOperationException("Expected planet_id planet_42");
        }
    }

    /// <summary>
    /// Tests clear planet.
    /// </summary>
    public static void TestClearPlanet()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", 1e11);
        slot.FillWithPlanet("planet_42");

        slot.ClearPlanet();

        if (slot.IsFilled)
        {
            throw new InvalidOperationException("Should not be filled after clear");
        }
        if (slot.PlanetId != "")
        {
            throw new InvalidOperationException("Planet ID should be empty after clear");
        }
    }

    /// <summary>
    /// Tests is available.
    /// </summary>
    public static void TestIsAvailable()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", 1e11);

        slot.IsStable = true;
        slot.IsFilled = false;
        if (!slot.IsAvailable())
        {
            throw new InvalidOperationException("Stable and unfilled should be available");
        }

        slot.IsStable = false;
        if (slot.IsAvailable())
        {
            throw new InvalidOperationException("Unstable should not be available");
        }

        slot.IsStable = true;
        slot.FillWithPlanet("p1");
        if (slot.IsAvailable())
        {
            throw new InvalidOperationException("Filled should not be available");
        }
    }

    /// <summary>
    /// Tests suggested eccentricity can be set.
    /// </summary>
    public static void TestSuggestedEccentricity()
    {
        OrbitSlot slot = new OrbitSlot("s1", "h1", 1e11);

        slot.SuggestedEccentricity = 0.15;
        if (System.Math.Abs(slot.SuggestedEccentricity - 0.15) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.15, got {slot.SuggestedEccentricity}");
        }
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestRoundTrip()
    {
        OrbitSlot original = new OrbitSlot("slot_test", "host_test", 2.5e11);
        original.SuggestedEccentricity = 0.12;
        original.Zone = OrbitZone.Zone.Cold;
        original.IsStable = true;
        original.FillProbability = 0.75;
        original.FillWithPlanet("planet_99");

        Godot.Collections.Dictionary data = original.ToDictionary();
        OrbitSlot restored = OrbitSlot.FromDictionary(data);

        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
        if (restored.OrbitHostId != original.OrbitHostId)
        {
            throw new InvalidOperationException("Orbit host ID should match");
        }
        if (System.Math.Abs(restored.SemiMajorAxisM - original.SemiMajorAxisM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Semi-major axis should match");
        }
        if (System.Math.Abs(restored.SuggestedEccentricity - original.SuggestedEccentricity) > DefaultTolerance)
        {
            throw new InvalidOperationException("Suggested eccentricity should match");
        }
        if (restored.Zone != original.Zone)
        {
            throw new InvalidOperationException("Zone should match");
        }
        if (restored.IsStable != original.IsStable)
        {
            throw new InvalidOperationException("Is stable should match");
        }
        if (System.Math.Abs(restored.FillProbability - original.FillProbability) > DefaultTolerance)
        {
            throw new InvalidOperationException("Fill probability should match");
        }
        if (restored.IsFilled != original.IsFilled)
        {
            throw new InvalidOperationException("Is filled should match");
        }
        if (restored.PlanetId != original.PlanetId)
        {
            throw new InvalidOperationException("Planet ID should match");
        }
    }
}
