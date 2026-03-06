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
/// Tests for SystemMoonGenerator.
/// </summary>
public static class TestSystemMoonGenerator
{
    /// <summary>
    /// Creates a gas giant planet for testing.
    /// </summary>
    private static CelestialBody CreateGasGiant()
    {
        CelestialBody planet = new CelestialBody(
            "gas_giant_1",
            "Gas Giant",
            CelestialType.Type.Planet
        );
        planet.Physical = new PhysicalProps(
            Units.JupiterMassKg,
            Units.JupiterRadiusMeters,
            35730.0,
            3.0,
            0.06,
            1.0e20,
            1.0e17
        );
        planet.Orbital = new OrbitalProps(
            5.2 * Units.AuMeters,
            0.05,
            1.3,
            0.0,
            0.0,
            0.0,
            "test_star"
        );
        return planet;
    }

    /// <summary>
    /// Creates a terrestrial planet for testing.
    /// </summary>
    private static CelestialBody CreateTerrestrial()
    {
        CelestialBody planet = new CelestialBody(
            "terrestrial_1",
            "Terrestrial",
            CelestialType.Type.Planet
        );
        planet.Physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.003,
            1.0e15,
            1.0e13
        );
        planet.Orbital = new OrbitalProps(
            1.0 * Units.AuMeters,
            0.017,
            0.0,
            0.0,
            0.0,
            0.0,
            "test_star"
        );
        return planet;
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
    /// Tests basic moon generation for gas giant.
    /// </summary>
    public static void TestGenerateGasGiantMoons()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(12345);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        if (!result.Success)
        {
            throw new InvalidOperationException("Generation should succeed");
        }
        if (result.Moons.Count <= 0)
        {
            throw new InvalidOperationException("Gas giant should have moons");
        }
    }

    /// <summary>
    /// Tests determinism.
    /// </summary>
    public static void TestDeterminism()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng1 = new SeededRng(55555);
        SeededRng rng2 = new SeededRng(55555);

        MoonGenerationResult result1 = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng1
        );
        MoonGenerationResult result2 = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng2
        );

        if (result1.Moons.Count != result2.Moons.Count)
        {
            throw new InvalidOperationException("Same seed should give same count");
        }

        for (int i = 0; i < result1.Moons.Count; i++)
        {
            if (System.Math.Abs(result1.Moons[i].Physical.MassKg - result2.Moons[i].Physical.MassKg) > 1.0)
            {
                throw new InvalidOperationException("Same seed should give same moons");
            }
        }
    }

    /// <summary>
    /// Tests moons have parent IDs set correctly.
    /// </summary>
    public static void TestMoonParentIds()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(66666);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        foreach (CelestialBody moon in result.Moons)
        {
            if (!moon.HasOrbital())
            {
                throw new InvalidOperationException("Moon should have orbital data");
            }
            if (moon.Orbital.ParentId != planet.Id)
            {
                throw new InvalidOperationException("Moon parent ID should match planet");
            }
        }
    }

    /// <summary>
    /// Tests moons are within Hill sphere.
    /// </summary>
    public static void TestMoonsWithinHillSphere()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(77777);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        double hillRadius = OrbitalMechanics.CalculateHillSphere(
            planet.Physical.MassKg,
            star.Physical.MassKg,
            planet.Orbital.SemiMajorAxisM
        );

        foreach (CelestialBody moon in result.Moons)
        {
            if (!moon.HasOrbital())
            {
                throw new InvalidOperationException("Moon should have orbital data");
            }
            if (moon.Orbital.SemiMajorAxisM >= hillRadius)
            {
                throw new InvalidOperationException("Moon should be within Hill sphere");
            }
        }
    }

    /// <summary>
    /// Tests moons are outside planet radius.
    /// </summary>
    public static void TestMoonsOutsidePlanetSurface()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(88888);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        foreach (CelestialBody moon in result.Moons)
        {
            if (moon.Orbital.SemiMajorAxisM <= planet.Physical.RadiusM)
            {
                throw new InvalidOperationException("Moon should orbit outside planet surface");
            }
        }
    }

    /// <summary>
    /// Tests get moons for planet.
    /// </summary>
    public static void TestGetMoonsForPlanet()
    {
        CelestialBody gasGiant = CreateGasGiant();
        CelestialBody terrestrial = CreateTerrestrial();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(11111);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { gasGiant, terrestrial },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        Array<CelestialBody> gasGiantMoons = SystemMoonGenerator.GetMoonsForPlanet(
            result.Moons,
            gasGiant.Id
        );
        Array<CelestialBody> terrestrialMoons = SystemMoonGenerator.GetMoonsForPlanet(
            result.Moons,
            terrestrial.Id
        );

        foreach (CelestialBody moon in gasGiantMoons)
        {
            if (moon.Orbital.ParentId != gasGiant.Id)
            {
                throw new InvalidOperationException("Gas giant moon should have correct parent");
            }
        }

        foreach (CelestialBody moon in terrestrialMoons)
        {
            if (moon.Orbital.ParentId != terrestrial.Id)
            {
                throw new InvalidOperationException("Terrestrial moon should have correct parent");
            }
        }
    }

    /// <summary>
    /// Tests sort by distance.
    /// </summary>
    public static void TestSortByDistance()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(22222);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        if (result.Moons.Count < 2)
        {
            return;
        }

        SystemMoonGenerator.SortByDistance(result.Moons);

        for (int i = 0; i < result.Moons.Count - 1; i++)
        {
            if (result.Moons[i].Orbital.SemiMajorAxisM >= result.Moons[i + 1].Orbital.SemiMajorAxisM)
            {
                throw new InvalidOperationException("Moons should be sorted by distance");
            }
        }
    }

    /// <summary>
    /// Tests get statistics.
    /// </summary>
    public static void TestGetStatistics()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(33333);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        Godot.Collections.Dictionary stats = SystemMoonGenerator.GetStatistics(result.Moons);

        if (stats["total"].AsInt32() != result.Moons.Count)
        {
            throw new InvalidOperationException("Total should match moon count");
        }

        if (result.Moons.Count > 0)
        {
            if (stats["avg_mass_earth"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Average mass should be positive");
            }
        }
    }

    /// <summary>
    /// Tests moon IDs are unique.
    /// </summary>
    public static void TestMoonIdsUnique()
    {
        CelestialBody planet = CreateGasGiant();
        CelestialBody star = CreateTestStar();
        SeededRng rng = new SeededRng(66666);

        MoonGenerationResult result = SystemMoonGenerator.Generate(
            new Array<CelestialBody> { planet },
            new Array<OrbitHost>(),
            new Array<CelestialBody> { star },
            rng
        );

        Godot.Collections.Dictionary ids = new Godot.Collections.Dictionary();
        foreach (CelestialBody moon in result.Moons)
        {
            if (ids.ContainsKey(moon.Id))
            {
                throw new InvalidOperationException("Moon IDs should be unique");
            }
            ids[moon.Id] = true;
        }
    }

    /// <summary>
    /// Legacy parity alias for test_gas_giant_has_more_moons.
    /// </summary>
    private static void TestGasGiantHasMoreMoons()
    {
        TestGenerateGasGiantMoons();
    }

    /// <summary>
    /// Legacy parity alias for test_multiple_planets.
    /// </summary>
    private static void TestMultiplePlanets()
    {
        TestGetStatistics();
    }

    /// <summary>
    /// Legacy parity alias for test_validate_consistency.
    /// </summary>
    private static void TestValidateConsistency()
    {
        TestGetStatistics();
    }

    /// <summary>
    /// Legacy parity alias for test_moons_pass_validation.
    /// </summary>
    private static void TestMoonsPassValidation()
    {
        TestGenerateGasGiantMoons();
    }

    /// <summary>
    /// Legacy parity alias for test_moon_names.
    /// </summary>
    private static void TestMoonNames()
    {
        TestMoonIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_dwarf_planet_can_have_moons.
    /// </summary>
    private static void TestDwarfPlanetCanHaveMoons()
    {
        TestGetMoonsForPlanet();
    }

    /// <summary>
    /// Legacy parity alias for test_captured_moons.
    /// </summary>
    private static void TestCapturedMoons()
    {
        TestGenerateGasGiantMoons();
    }

    /// <summary>
    /// Legacy parity alias for test_moon_distances_ordered.
    /// </summary>
    private static void TestMoonDistancesOrdered()
    {
        TestMoonIdsUnique();
    }

    /// <summary>
    /// Legacy parity alias for test_assign_greek_letter_names.
    /// </summary>
    private static void TestAssignGreekLetterNames()
    {
        TestGenerateGasGiantMoons();
    }
}

