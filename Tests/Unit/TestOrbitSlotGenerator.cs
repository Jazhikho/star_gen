#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for OrbitSlotGenerator.
/// </summary>
public static class TestOrbitSlotGenerator
{
    /// <summary>
    /// Creates a Sun-like orbit host for testing.
    /// </summary>
    private static OrbitHost CreateSunLikeHost()
    {
        OrbitHost host = new OrbitHost("host_sol", OrbitHost.HostType.SType);
        host.CombinedMassKg = Units.SolarMassKg;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.EffectiveTemperatureK = 5778.0;
        host.InnerStabilityM = Units.SolarRadiusMeters * 3.0;
        host.OuterStabilityM = 100.0 * Units.AuMeters;
        host.CalculateZones();
        return host;
    }

    /// <summary>
    /// Creates a close binary orbit host for testing.
    /// </summary>
    private static OrbitHost CreateCloseBinaryHost()
    {
        OrbitHost host = new OrbitHost("host_binary", OrbitHost.HostType.PType);
        host.CombinedMassKg = Units.SolarMassKg * 2.0;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts * 2.0;
        host.EffectiveTemperatureK = 5778.0;
        host.InnerStabilityM = 3.0 * Units.AuMeters;
        host.OuterStabilityM = 50.0 * Units.AuMeters;
        host.CalculateZones();
        return host;
    }

    /// <summary>
    /// Tests slot generation for single star.
    /// </summary>
    public static void TestGenerateForHostSingleStar()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(12345);

        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
        if (result.Slots.Count <= 0)
        {
            throw new InvalidOperationException("Should generate some slots");
        }
        if (result.OrbitHostId != "host_sol")
        {
            throw new InvalidOperationException("Expected orbit_host_id host_sol");
        }
    }

    /// <summary>
    /// Tests slots are within stability zone.
    /// </summary>
    public static void TestSlotsWithinStabilityZone()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(22222);

        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );

        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.SemiMajorAxisM <= host.InnerStabilityM)
            {
                throw new InvalidOperationException("Slot should be outside inner limit");
            }
            if (slot.SemiMajorAxisM >= host.OuterStabilityM)
            {
                throw new InvalidOperationException("Slot should be inside outer limit");
            }
        }
    }

    /// <summary>
    /// Tests slots are in increasing distance order.
    /// </summary>
    public static void TestSlotsIncreasingDistance()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(33333);

        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );

        double prevDistance = 0.0;
        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.SemiMajorAxisM <= prevDistance)
            {
                throw new InvalidOperationException("Slots should be in increasing distance order");
            }
            prevDistance = slot.SemiMajorAxisM;
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng1 = new SeededRng(66666);
        SeededRng rng2 = new SeededRng(66666);

        OrbitSlotGenerationResult result1 = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng1
        );

        OrbitSlotGenerationResult result2 = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng2
        );

        if (result1.Slots.Count != result2.Slots.Count)
        {
            throw new InvalidOperationException("Same seed should give same slot count");
        }

        for (int i = 0; i < result1.Slots.Count; i++)
        {
            if (System.Math.Abs(result1.Slots[i].SemiMajorAxisM - result2.Slots[i].SemiMajorAxisM) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same distances");
            }
        }
    }

    /// <summary>
    /// Tests filter stable.
    /// </summary>
    public static void TestFilterStable()
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot stableSlot = new OrbitSlot("s1", "h1", 1e11);
        stableSlot.IsStable = true;
        slots.Add(stableSlot);

        OrbitSlot unstableSlot = new OrbitSlot("s2", "h1", 2e11);
        unstableSlot.IsStable = false;
        slots.Add(unstableSlot);

        Array<OrbitSlot> filtered = OrbitSlotGenerator.FilterStable(slots);

        if (filtered.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 stable slot");
        }
        if (filtered[0].Id != "s1")
        {
            throw new InvalidOperationException("Expected s1");
        }
    }

    /// <summary>
    /// Tests filter available.
    /// </summary>
    public static void TestFilterAvailable()
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot available = new OrbitSlot("s1", "h1", 1e11);
        available.IsStable = true;
        available.IsFilled = false;
        slots.Add(available);

        OrbitSlot filled = new OrbitSlot("s2", "h1", 2e11);
        filled.IsStable = true;
        filled.FillWithPlanet("p1");
        slots.Add(filled);

        OrbitSlot unstable = new OrbitSlot("s3", "h1", 3e11);
        unstable.IsStable = false;
        slots.Add(unstable);

        Array<OrbitSlot> filtered = OrbitSlotGenerator.FilterAvailable(slots);

        if (filtered.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 available slot");
        }
        if (filtered[0].Id != "s1")
        {
            throw new InvalidOperationException("Expected s1");
        }
    }

    /// <summary>
    /// Tests filter by zone.
    /// </summary>
    public static void TestFilterByZone()
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot hotSlot = new OrbitSlot("s1", "h1", 1e10);
        hotSlot.Zone = OrbitZone.Zone.Hot;
        slots.Add(hotSlot);

        OrbitSlot temperateSlot = new OrbitSlot("s2", "h1", 1e11);
        temperateSlot.Zone = OrbitZone.Zone.Temperate;
        slots.Add(temperateSlot);

        OrbitSlot coldSlot = new OrbitSlot("s3", "h1", 1e12);
        coldSlot.Zone = OrbitZone.Zone.Cold;
        slots.Add(coldSlot);

        Array<OrbitSlot> temperateSlots = OrbitSlotGenerator.FilterByZone(slots, OrbitZone.Zone.Temperate);

        if (temperateSlots.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 temperate slot");
        }
        if (temperateSlots[0].Id != "s2")
        {
            throw new InvalidOperationException("Expected s2");
        }
    }

    /// <summary>
    /// Tests sort by distance.
    /// </summary>
    public static void TestSortByDistance()
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();
        slots.Add(new OrbitSlot("s3", "h1", 3e11));
        slots.Add(new OrbitSlot("s1", "h1", 1e11));
        slots.Add(new OrbitSlot("s2", "h1", 2e11));

        OrbitSlotGenerator.SortByDistance(slots);

        if (slots[0].Id != "s1")
        {
            throw new InvalidOperationException("Expected s1 first");
        }
        if (slots[1].Id != "s2")
        {
            throw new InvalidOperationException("Expected s2 second");
        }
        if (slots[2].Id != "s3")
        {
            throw new InvalidOperationException("Expected s3 third");
        }
    }

    /// <summary>
    /// Tests sort by probability.
    /// </summary>
    public static void TestSortByProbability()
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot low = new OrbitSlot("low", "h1", 1e11);
        low.FillProbability = 0.2;
        slots.Add(low);

        OrbitSlot high = new OrbitSlot("high", "h1", 2e11);
        high.FillProbability = 0.9;
        slots.Add(high);

        OrbitSlot mid = new OrbitSlot("mid", "h1", 3e11);
        mid.FillProbability = 0.5;
        slots.Add(mid);

        OrbitSlotGenerator.SortByProbability(slots);

        if (slots[0].Id != "high")
        {
            throw new InvalidOperationException("Expected high first");
        }
        if (slots[1].Id != "mid")
        {
            throw new InvalidOperationException("Expected mid second");
        }
        if (slots[2].Id != "low")
        {
            throw new InvalidOperationException("Expected low third");
        }
    }

    /// <summary>Tests zones are classified correctly.</summary>
    public static void TestZoneClassification()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(44444);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.SemiMajorAxisM < host.HabitableZoneInnerM)
            {
                if (slot.Zone != OrbitZone.Zone.Hot)
                {
                    throw new InvalidOperationException("Inner slots should be HOT");
                }
            }
            else if (slot.SemiMajorAxisM > host.FrostLineM)
            {
                if (slot.Zone != OrbitZone.Zone.Cold)
                {
                    throw new InvalidOperationException("Outer slots should be COLD");
                }
            }
            else
            {
                if (slot.Zone != OrbitZone.Zone.Temperate)
                {
                    throw new InvalidOperationException("Middle slots should be TEMPERATE");
                }
            }
        }
    }

    /// <summary>Tests fill probability decreases outward.</summary>
    public static void TestFillProbabilityDecreases()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(55555);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count < 2)
        {
            return;
        }
        double firstProb = result.Slots[0].FillProbability;
        double lastProb = result.Slots[result.Slots.Count - 1].FillProbability;
        if (firstProb <= lastProb)
        {
            throw new InvalidOperationException("Inner slots should have higher fill probability");
        }
    }

    /// <summary>Tests suggested eccentricity increases with distance.</summary>
    public static void TestEccentricityIncreasesWithDistance()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(33333);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count < 5)
        {
            return;
        }
        double avgInner = 0.0;
        double avgOuter = 0.0;
        for (int i = 0; i < 3; i++)
        {
            avgInner += result.Slots[i].SuggestedEccentricity;
            avgOuter += result.Slots[result.Slots.Count - 1 - i].SuggestedEccentricity;
        }
        avgInner /= 3.0;
        avgOuter /= 3.0;
        if (avgOuter <= avgInner * 0.5)
        {
            throw new InvalidOperationException("Outer orbits should tend toward higher eccentricity");
        }
    }

    /// <summary>Tests minimum spacing between slots.</summary>
    public static void TestMinimumSpacing()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(55555);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        for (int i = 0; i < result.Slots.Count - 1; i++)
        {
            double spacing = result.Slots[i + 1].SemiMajorAxisM - result.Slots[i].SemiMajorAxisM;
            double minSpacing = result.Slots[i].SemiMajorAxisM * OrbitSlotGenerator.MinSpacingFactor;
            if (spacing < minSpacing * 0.99)
            {
                throw new InvalidOperationException("Spacing should meet minimum requirement");
            }
        }
    }

    /// <summary>Tests slots respect star radius safety margin.</summary>
    public static void TestStarRadiusSafety()
    {
        OrbitHost host = CreateSunLikeHost();
        host.InnerStabilityM = 0.001 * Units.AuMeters;
        double starRadius = Units.SolarRadiusMeters;
        SeededRng rng = new SeededRng(66666);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            starRadius,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count > 0)
        {
            double minSafeDistance = starRadius * OrbitSlotGenerator.StarRadiusSafetyMargin;
            if (result.Slots[0].SemiMajorAxisM < minSafeDistance)
            {
                throw new InvalidOperationException("First slot should respect star radius safety margin");
            }
        }
    }

    /// <summary>Tests generate_for_hosts with multiple hosts.</summary>
    public static void TestGenerateForHosts()
    {
        OrbitHost host1 = CreateSunLikeHost();
        OrbitHost host2 = CreateCloseBinaryHost();
        Array<OrbitHost> hosts = new Array<OrbitHost> { host1, host2 };
        Array<CelestialBody> stars = new Array<CelestialBody>();
        SystemHierarchy hierarchy = new SystemHierarchy();
        SeededRng rng = new SeededRng(77777);
        Godot.Collections.Dictionary<string, Array<OrbitSlot>> allSlots = OrbitSlotGenerator.GenerateAllSlots(hosts, stars, hierarchy, rng);
        if (allSlots.Count <= 0)
        {
            throw new InvalidOperationException("Should generate slots for multiple hosts");
        }
        if (!allSlots.ContainsKey("host_sol") || !allSlots.ContainsKey("host_binary"))
        {
            throw new InvalidOperationException("Should have slots for both hosts");
        }
    }

    /// <summary>Tests stability checking.</summary>
    public static void TestCheckStability()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(88888);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        Array<double> companionMasses = new Array<double> { Units.SolarMassKg };
        Array<double> companionDistances = new Array<double> { 10.0 * Units.AuMeters };
        OrbitSlotGenerator.CheckStability(result.Slots, host, companionMasses, companionDistances, 0.0);
        bool foundStable = false;
        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.IsStable)
            {
                foundStable = true;
                break;
            }
        }
        if (!foundStable)
        {
            throw new InvalidOperationException("Some slots should remain stable");
        }
    }

    /// <summary>Tests get_statistics.</summary>
    public static void TestGetStatistics()
    {
        OrbitHost host = CreateSunLikeHost();
        SeededRng rng = new SeededRng(99999);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        Godot.Collections.Dictionary stats = OrbitSlotGenerator.GetStatistics(result.Slots);
        if (stats["total"].AsInt32() != result.Slots.Count)
        {
            throw new InvalidOperationException("stats total should match slot count");
        }
        if (stats["min_distance_au"].AsDouble() <= 0.0)
        {
            throw new InvalidOperationException("min_distance_au should be positive");
        }
        if (stats["max_distance_au"].AsDouble() <= stats["min_distance_au"].AsDouble())
        {
            throw new InvalidOperationException("max should be greater than min");
        }
        int zoneSum = stats["hot"].AsInt32() + stats["temperate"].AsInt32() + stats["cold"].AsInt32();
        if (zoneSum != stats["total"].AsInt32())
        {
            throw new InvalidOperationException("Zone counts should sum to total");
        }
    }

    /// <summary>Tests maximum slot limit.</summary>
    public static void TestMaxSlotsLimit()
    {
        OrbitHost host = CreateSunLikeHost();
        host.OuterStabilityM = 10000.0 * Units.AuMeters;
        SeededRng rng = new SeededRng(11111);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count >= 21)
        {
            throw new InvalidOperationException("Should not exceed max slots");
        }
    }

    /// <summary>Tests P-type host generation.</summary>
    public static void TestPtypeHostGeneration()
    {
        OrbitHost host = CreateCloseBinaryHost();
        SeededRng rng = new SeededRng(12121);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters * 2.0,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (!result.Success)
        {
            throw new InvalidOperationException("P-type generation should succeed");
        }
        if (result.Slots.Count <= 0)
        {
            throw new InvalidOperationException("Should generate slots");
        }
        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.SemiMajorAxisM <= host.InnerStabilityM)
            {
                throw new InvalidOperationException("P-type slots should be outside inner limit");
            }
        }
    }

    /// <summary>Tests invalid host handling.</summary>
    public static void TestInvalidHost()
    {
        OrbitHost host = new OrbitHost("invalid", OrbitHost.HostType.SType);
        SeededRng rng = new SeededRng(13131);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Success)
        {
            throw new InvalidOperationException("Should fail for invalid host");
        }
        if (result.Slots.Count != 0)
        {
            throw new InvalidOperationException("Should have no slots");
        }
    }

    /// <summary>Tests narrow stability zone.</summary>
    public static void TestNarrowStabilityZone()
    {
        OrbitHost host = new OrbitHost("narrow", OrbitHost.HostType.SType);
        host.CombinedMassKg = Units.SolarMassKg;
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.InnerStabilityM = 1.0 * Units.AuMeters;
        host.OuterStabilityM = 1.5 * Units.AuMeters;
        host.CalculateZones();
        SeededRng rng = new SeededRng(77777);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count <= 0)
        {
            throw new InvalidOperationException("Should generate slots even in narrow zone");
        }
        foreach (OrbitSlot slot in result.Slots)
        {
            if (slot.SemiMajorAxisM < 1.0 * Units.AuMeters || slot.SemiMajorAxisM > 1.5 * Units.AuMeters)
            {
                throw new InvalidOperationException("Slots should be within narrow zone");
            }
        }
    }

    /// <summary>Tests wide zone generates many slots.</summary>
    public static void TestWideZoneManySlots()
    {
        OrbitHost host = CreateSunLikeHost();
        host.OuterStabilityM = 100.0 * Units.AuMeters;
        SeededRng rng = new SeededRng(99999);
        OrbitSlotGenerationResult result = OrbitSlotGenerator.GenerateForHost(
            host,
            Units.SolarRadiusMeters,
            new Array<double>(),
            new Array<double>(),
            rng
        );
        if (result.Slots.Count <= 5)
        {
            throw new InvalidOperationException("Wide zone should generate multiple slots");
        }
    }
}
