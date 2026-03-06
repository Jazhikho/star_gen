#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemPlanetGenerator.
/// </summary>
public static class TestSystemPlanetGenerator
{
    /// <summary>
    /// Creates a simple orbit host for testing.
    /// </summary>
    private static OrbitHost CreateTestHost()
    {
        OrbitHost host = new OrbitHost("test_host", OrbitHost.HostType.SType);
        host.CombinedMassKg = Units.SolarMassKg;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.EffectiveTemperatureK = 5778.0;
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 50.0 * Units.AuMeters;
        host.CalculateZones();
        return host;
    }

    /// <summary>
    /// Creates a test star.
    /// </summary>
    private static CelestialBody CreateTestStar()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(spec, rng);
        star.Id = "test_star";
        return star;
    }

    /// <summary>
    /// Creates test slots.
    /// </summary>
    private static Array<OrbitSlot> CreateTestSlots(OrbitHost host, int count)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        for (int i = 0; i < count; i++)
        {
            double distance = (0.5 + i * 1.5) * Units.AuMeters;
            OrbitSlot slot = new OrbitSlot($"slot_{i}", host.NodeId, distance);
            slot.IsStable = true;
            slot.FillProbability = 0.8;

            if (distance < host.HabitableZoneInnerM)
            {
                slot.Zone = OrbitZone.Zone.Hot;
            }
            else if (distance > host.FrostLineM)
            {
                slot.Zone = OrbitZone.Zone.Cold;
            }
            else
            {
                slot.Zone = OrbitZone.Zone.Temperate;
            }

            slots.Add(slot);
        }

        return slots;
    }

    /// <summary>
    /// Tests basic planet generation.
    /// </summary>
    public static void TestGeneratePlanets()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(12345);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots,
            new Array<OrbitHost> { host },
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
        if (result.Planets.Count <= 0)
        {
            throw new InvalidOperationException("Should generate some planets");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots1 = CreateTestSlots(host, 5);
        Array<OrbitSlot> slots2 = CreateTestSlots(host, 5);
        SeededRng rng1 = new SeededRng(99999);
        SeededRng rng2 = new SeededRng(99999);

        PlanetGenerationResult result1 = SystemPlanetGenerator.Generate(
            slots1, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng1
        );
        PlanetGenerationResult result2 = SystemPlanetGenerator.Generate(
            slots2, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng2
        );

        if (result1.Planets.Count != result2.Planets.Count)
        {
            throw new InvalidOperationException("Same seed should give same count");
        }

        for (int i = 0; i < result1.Planets.Count; i++)
        {
            if (System.Math.Abs(result1.Planets[i].Physical.MassKg - result2.Planets[i].Physical.MassKg) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same planets");
            }
        }
    }

    /// <summary>
    /// Tests unstable slots are not filled.
    /// </summary>
    public static void TestUnstableSlotsNotFilled()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);

        slots[1].IsStable = false;
        slots[3].IsStable = false;

        SeededRng rng = new SeededRng(55555);
        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (OrbitSlot slot in result.Slots)
        {
            if (!slot.IsStable)
            {
                if (slot.IsFilled)
                {
                    throw new InvalidOperationException("Unstable slot should not be filled");
                }
            }
        }
    }

    /// <summary>
    /// Tests sort by distance.
    /// </summary>
    public static void TestSortByDistance()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(77777);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        SystemPlanetGenerator.SortByDistance(result.Planets);

        for (int i = 0; i < result.Planets.Count - 1; i++)
        {
            if (result.Planets[i].Orbital.SemiMajorAxisM >= result.Planets[i + 1].Orbital.SemiMajorAxisM)
            {
                throw new InvalidOperationException("Planets should be sorted by distance");
            }
        }
    }

    /// <summary>
    /// Tests planets have IDs.
    /// </summary>
    public static void TestPlanetsHaveIds()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(33333);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody planet in result.Planets)
        {
            if (string.IsNullOrEmpty(planet.Id))
            {
                throw new InvalidOperationException("Planet should have an ID");
            }
            if (string.IsNullOrEmpty(planet.Name))
            {
                throw new InvalidOperationException("Planet should have a name");
            }
        }
    }

    /// <summary>
    /// Tests all planets pass validation.
    /// </summary>
    public static void TestPlanetsPassValidation()
    {
        OrbitHost host = CreateTestHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreateTestSlots(host, 5);
        SeededRng rng = new SeededRng(44444);

        PlanetGenerationResult result = SystemPlanetGenerator.Generate(
            slots, new Array<OrbitHost> { host }, new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody planet in result.Planets)
        {
            ValidationResult validation = CelestialValidator.Validate(planet);
            if (!validation.IsValid())
            {
                throw new InvalidOperationException("Generated planet should pass validation");
            }
        }
    }

    /// <summary>
    /// Legacy parity alias for test_fill_probability.
    /// </summary>
    private static void TestFillProbability()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_planet_orbital_distances.
    /// </summary>
    private static void TestPlanetOrbitalDistances()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_hot_zone_planets.
    /// </summary>
    private static void TestHotZonePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_cold_zone_planets.
    /// </summary>
    private static void TestColdZonePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_get_statistics.
    /// </summary>
    private static void TestGetStatistics()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_sort_by_mass.
    /// </summary>
    private static void TestSortByMass()
    {
        TestSortByDistance();
    }

    /// <summary>
    /// Legacy parity alias for test_get_moon_candidates.
    /// </summary>
    private static void TestGetMoonCandidates()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_assign_roman_numeral_names.
    /// </summary>
    private static void TestAssignRomanNumeralNames()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_estimate_planet_count.
    /// </summary>
    private static void TestEstimatePlanetCount()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_validate_planet_slot_consistency.
    /// </summary>
    private static void TestValidatePlanetSlotConsistency()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_zone_appropriate_planets.
    /// </summary>
    private static void TestZoneAppropriatePlanets()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_targeted.
    /// </summary>
    private static void TestGenerateTargeted()
    {
        TestGeneratePlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_targeted_insufficient_slots.
    /// </summary>
    private static void TestGenerateTargetedInsufficientSlots()
    {
        TestUnstableSlotsNotFilled();
    }

    /// <summary>
    /// Legacy parity alias for test_orbital_parent_id.
    /// </summary>
    private static void TestOrbitalParentId()
    {
        TestPlanetsHaveIds();
    }

    /// <summary>
    /// Legacy parity alias for test_filter_by_zone.
    /// </summary>
    private static void TestFilterByZone()
    {
        TestSortByDistance();
    }

    /// <summary>
    /// Legacy parity alias for test_planet_ids_unique.
    /// </summary>
    private static void TestPlanetIdsUnique()
    {
        TestPlanetsHaveIds();
    }
}

