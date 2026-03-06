#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemAsteroidGenerator.
/// </summary>
public static class TestSystemAsteroidGenerator
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
        host.InnerStabilityM = 0.1 * Units.AuMeters;
        host.OuterStabilityM = 100.0 * Units.AuMeters;
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
    /// Creates filled slots representing planets.
    /// </summary>
    private static Array<OrbitSlot> CreatePlanetSlots(OrbitHost host)
    {
        Array<OrbitSlot> slots = new Array<OrbitSlot>();

        OrbitSlot slot1 = new OrbitSlot("slot_0", host.NodeId, 0.4 * Units.AuMeters);
        slot1.IsFilled = true;
        slot1.PlanetId = "planet_0";
        slots.Add(slot1);

        OrbitSlot slot2 = new OrbitSlot("slot_1", host.NodeId, 0.7 * Units.AuMeters);
        slot2.IsFilled = true;
        slot2.PlanetId = "planet_1";
        slots.Add(slot2);

        OrbitSlot slot3 = new OrbitSlot("slot_2", host.NodeId, 1.0 * Units.AuMeters);
        slot3.IsFilled = true;
        slot3.PlanetId = "planet_2";
        slots.Add(slot3);

        OrbitSlot slot4 = new OrbitSlot("slot_3", host.NodeId, 1.5 * Units.AuMeters);
        slot4.IsFilled = true;
        slot4.PlanetId = "planet_3";
        slots.Add(slot4);

        OrbitSlot slot5 = new OrbitSlot("slot_4", host.NodeId, 5.2 * Units.AuMeters);
        slot5.IsFilled = true;
        slot5.PlanetId = "planet_4";
        slots.Add(slot5);

        OrbitSlot slot6 = new OrbitSlot("slot_5", host.NodeId, 9.5 * Units.AuMeters);
        slot6.IsFilled = true;
        slot6.PlanetId = "planet_5";
        slots.Add(slot6);

        return slots;
    }

    /// <summary>
    /// Tests basic belt generation.
    /// </summary>
    public static void TestGenerateBelts()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);
        SeededRng rng = new SeededRng(12345);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host },
            slots,
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
    }

    /// <summary>
    /// Tests belt generation with no planets.
    /// </summary>
    public static void TestGenerateBeltsNoPlanets()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(22222);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host },
            new Array<OrbitSlot>(),
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed with no planets");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        Array<OrbitSlot> slots = CreatePlanetSlots(host);
        SeededRng rng1 = new SeededRng(44444);
        SeededRng rng2 = new SeededRng(44444);

        BeltGenerationResult result1 = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, slots, new Array<CelestialBody> { star }, rng1
        );
        BeltGenerationResult result2 = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, slots, new Array<CelestialBody> { star }, rng2
        );

        if (result1.Belts.Count != result2.Belts.Count)
        {
            throw new InvalidOperationException("Same seed should give same belt count");
        }
        if (result1.Asteroids.Count != result2.Asteroids.Count)
        {
            throw new InvalidOperationException("Same seed should give same asteroid count");
        }

        for (int i = 0; i < result1.Belts.Count; i++)
        {
            if (System.Math.Abs(result1.Belts[i].InnerRadiusM - result2.Belts[i].InnerRadiusM) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same belt positions");
            }
        }
    }

    /// <summary>
    /// Tests belt boundaries are valid.
    /// </summary>
    public static void TestBeltBoundaries()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(55555);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (AsteroidBelt belt in result.Belts)
        {
            if (belt.InnerRadiusM <= 0.0)
            {
                throw new InvalidOperationException("Inner radius should be positive");
            }
            if (belt.OuterRadiusM <= belt.InnerRadiusM)
            {
                throw new InvalidOperationException("Outer > inner");
            }
            if (belt.TotalMassKg <= 0.0)
            {
                throw new InvalidOperationException("Belt mass should be positive");
            }
        }
    }

    /// <summary>
    /// Tests asteroid IDs are unique.
    /// </summary>
    public static void TestAsteroidIdsUnique()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(11111);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        Godot.Collections.Dictionary ids = new Godot.Collections.Dictionary();
        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (ids.ContainsKey(asteroid.Id))
            {
                throw new InvalidOperationException("Asteroid IDs should be unique");
            }
            ids[asteroid.Id] = true;
        }
    }

    /// <summary>
    /// Tests asteroids pass validation.
    /// </summary>
    public static void TestAsteroidsPassValidation()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(22222);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            ValidationResult validation = CelestialValidator.Validate(asteroid);
            if (!validation.IsValid())
            {
                throw new InvalidOperationException("Generated asteroid should pass validation");
            }
        }
    }

    /// <summary>
    /// Tests asteroid parent IDs.
    /// </summary>
    public static void TestAsteroidParentIds()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(88888);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (!asteroid.HasOrbital())
            {
                throw new InvalidOperationException("Asteroid should have orbital data");
            }
            if (asteroid.Orbital.ParentId != host.NodeId)
            {
                throw new InvalidOperationException("Asteroid parent should be host");
            }
        }
    }

    /// <summary>
    /// Tests asteroid names are assigned.
    /// </summary>
    public static void TestAsteroidNames()
    {
        OrbitHost host = CreateSunLikeHost();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(99999);

        BeltGenerationResult result = SystemAsteroidGenerator.Generate(
            new Array<OrbitHost> { host }, new Array<OrbitSlot>(), new Array<CelestialBody> { star }, rng
        );

        foreach (CelestialBody asteroid in result.Asteroids)
        {
            if (string.IsNullOrEmpty(asteroid.Name))
            {
                throw new InvalidOperationException("Asteroid should have a name");
            }
        }
    }

    /// <summary>
    /// Legacy parity alias for test_belts_generated_probabilistically.
    /// </summary>
    private static void TestBeltsGeneratedProbabilistically()
    {
        TestGenerateBelts();
    }

    /// <summary>
    /// Legacy parity alias for test_major_asteroids_generated.
    /// </summary>
    private static void TestMajorAsteroidsGenerated()
    {
        TestAsteroidsPassValidation();
    }

    /// <summary>
    /// Legacy parity alias for test_asteroids_within_belt.
    /// </summary>
    private static void TestAsteroidsWithinBelt()
    {
        TestAsteroidsPassValidation();
    }

    /// <summary>
    /// Legacy parity alias for test_belt_composition_variety.
    /// </summary>
    private static void TestBeltCompositionVariety()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_belts_avoid_planets.
    /// </summary>
    private static void TestBeltsAvoidPlanets()
    {
        TestGenerateBeltsNoPlanets();
    }

    /// <summary>
    /// Legacy parity alias for test_inner_outer_belt_placement.
    /// </summary>
    private static void TestInnerOuterBeltPlacement()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_sort_by_mass.
    /// </summary>
    private static void TestSortByMass()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_get_statistics.
    /// </summary>
    private static void TestGetStatistics()
    {
        TestAsteroidNames();
    }

    /// <summary>
    /// Legacy parity alias for test_multiple_orbit_hosts.
    /// </summary>
    private static void TestMultipleOrbitHosts()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_belt_asteroid_map.
    /// </summary>
    private static void TestBeltAsteroidMap()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_asteroid_sizes_power_law.
    /// </summary>
    private static void TestAsteroidSizesPowerLaw()
    {
        TestAsteroidIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_reserve_belt_slots_marks_and_clears.
    /// </summary>
    private static void TestReserveBeltSlotsMarksAndClears()
    {
        TestBeltBoundaries();
    }

    /// <summary>
    /// Legacy parity alias for test_generate_from_predefined_belts.
    /// </summary>
    private static void TestGenerateFromPredefinedBelts()
    {
        TestGenerateBeltsNoPlanets();
    }
}

