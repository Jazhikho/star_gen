#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

namespace StarGen.Tests.Unit;

/// <summary>
/// Golden master regression tests for solar system generation.
/// Ensures generated systems remain deterministic across code changes.
/// </summary>
public static class TestSystemGoldenMasters
{
    private const double DefaultTolerance = 0.001;

    /// <summary>
    /// Gets all fixtures for testing.
    /// </summary>
    private static Array<Godot.Collections.Dictionary> GetFixtures()
    {
        return SystemFixtureGenerator.GenerateAllFixtures();
    }

    /// <summary>
    /// Finds a fixture by name.
    /// </summary>
    private static Godot.Collections.Dictionary FindFixture(Array<Godot.Collections.Dictionary> fixtures, string fixtureName)
    {
        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture.ContainsKey("name") && fixture["name"].AsString() == fixtureName)
            {
                return fixture;
            }
        }
        return new Godot.Collections.Dictionary();
    }

    /// <summary>
    /// Tests that fixtures are generated.
    /// </summary>
    public static void TestFixturesGenerated()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();

        if (fixtures.Count <= 0)
        {
            throw new InvalidOperationException("Should generate fixtures");
        }
    }

    /// <summary>
    /// Tests single star Sun-like system.
    /// </summary>
    public static void TestFixtureSingleSunLike()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_single_sun_like");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests single star red dwarf system.
    /// </summary>
    public static void TestFixtureSingleRedDwarf()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_single_red_dwarf");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests binary equal mass system.
    /// </summary>
    public static void TestFixtureBinaryEqual()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_binary_equal");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests binary unequal mass system.
    /// </summary>
    public static void TestFixtureBinaryUnequal()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_binary_unequal");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests triple hierarchical system.
    /// </summary>
    public static void TestFixtureTripleHierarchical()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_triple_hierarchical");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests quadruple system.
    /// </summary>
    public static void TestFixtureQuadruple()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_quadruple");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests maximum stars system.
    /// </summary>
    public static void TestFixtureMaxStars()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();
        Godot.Collections.Dictionary fixture = FindFixture(fixtures, "system_max_stars");

        if (fixture.Count == 0)
        {
            throw new InvalidOperationException("Fixture should exist");
        }

        VerifyFixtureRegenerates(fixture);
    }

    /// <summary>
    /// Tests all fixtures pass validation.
    /// </summary>
    public static void TestAllFixturesValid()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            Godot.Collections.Dictionary systemData = fixture.ContainsKey("system") ? (Godot.Collections.Dictionary)fixture["system"] : new Godot.Collections.Dictionary();
            SolarSystem system = SystemSerializer.FromDictionary(systemData);

            if (system == null)
            {
                throw new InvalidOperationException($"Fixture {fixture["name"]} should deserialize");
            }

            ValidationResult result = SystemValidator.Validate(system);
            if (!result.IsValid())
            {
                throw new InvalidOperationException($"Fixture {fixture["name"]} should be valid");
            }
        }
    }

    /// <summary>
    /// Tests fixture star counts match spec.
    /// </summary>
    public static void TestFixtureStarCounts()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            Godot.Collections.Dictionary specData = fixture.ContainsKey("spec") ? (Godot.Collections.Dictionary)fixture["spec"] : new Godot.Collections.Dictionary();
            Godot.Collections.Dictionary systemData = fixture.ContainsKey("system") ? (Godot.Collections.Dictionary)fixture["system"] : new Godot.Collections.Dictionary();

            SolarSystem system = SystemSerializer.FromDictionary(systemData);
            if (system == null)
            {
                continue;
            }

            int minStars = specData.ContainsKey("star_count_min") ? specData["star_count_min"].AsInt32() : 1;
            int maxStars = specData.ContainsKey("star_count_max") ? specData["star_count_max"].AsInt32() : 1;
            int actualStars = system.GetStarCount();

            if (actualStars < minStars || actualStars > maxStars)
            {
                throw new InvalidOperationException($"Fixture {fixture["name"]} star count should match spec");
            }
        }
    }

    /// <summary>
    /// Tests fixture serialization round-trip.
    /// </summary>
    public static void TestFixtureSerializationRoundtrip()
    {
        Array<Godot.Collections.Dictionary> fixtures = GetFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            Godot.Collections.Dictionary originalData = fixture.ContainsKey("system") ? (Godot.Collections.Dictionary)fixture["system"] : new Godot.Collections.Dictionary();
            SolarSystem system = SystemSerializer.FromDictionary(originalData);

            if (system == null)
            {
                continue;
            }

            Godot.Collections.Dictionary reserialized = SystemSerializer.ToDictionary(system);
            SolarSystem restored = SystemSerializer.FromDictionary(reserialized);

            if (restored == null)
            {
                throw new InvalidOperationException($"Fixture {fixture["name"]} should round-trip");
            }
            if (restored.Id != system.Id)
            {
                throw new InvalidOperationException("ID should match");
            }
            if (restored.GetStarCount() != system.GetStarCount())
            {
                throw new InvalidOperationException("Star count should match");
            }
            if (restored.GetPlanetCount() != system.GetPlanetCount())
            {
                throw new InvalidOperationException("Planet count should match");
            }
        }
    }

    /// <summary>
    /// Verifies a fixture regenerates identically.
    /// </summary>
    private static void VerifyFixtureRegenerates(Godot.Collections.Dictionary fixture)
    {
        Godot.Collections.Dictionary specData = fixture.ContainsKey("spec") ? (Godot.Collections.Dictionary)fixture["spec"] : new Godot.Collections.Dictionary();
        Godot.Collections.Dictionary originalSystemData = fixture.ContainsKey("system") ? (Godot.Collections.Dictionary)fixture["system"] : new Godot.Collections.Dictionary();

        SolarSystemSpec spec = SolarSystemSpec.FromDictionary(specData);

        SolarSystem regenerated = SystemFixtureGenerator.GenerateSystem(spec);
        if (regenerated == null)
        {
            throw new InvalidOperationException("Should regenerate system");
        }

        SolarSystem original = SystemSerializer.FromDictionary(originalSystemData);
        if (original == null)
        {
            throw new InvalidOperationException("Original should deserialize");
        }

        if (regenerated.GetStarCount() != original.GetStarCount())
        {
            throw new InvalidOperationException("Star count should match");
        }

        if (regenerated.GetPlanetCount() != original.GetPlanetCount())
        {
            throw new InvalidOperationException("Planet count should match");
        }

        if (regenerated.GetMoonCount() != original.GetMoonCount())
        {
            throw new InvalidOperationException("Moon count should match");
        }

        Array<CelestialBody> origStars = original.GetStars();
        Array<CelestialBody> regenStars = regenerated.GetStars();

        int minCount = System.Math.Min(origStars.Count, regenStars.Count);
        for (int i = 0; i < minCount; i++)
        {
            double tolerance = origStars[i].Physical.MassKg * 0.001;
            if (System.Math.Abs(origStars[i].Physical.MassKg - regenStars[i].Physical.MassKg) > tolerance)
            {
                throw new InvalidOperationException($"Star {i} mass should match");
            }
        }
    }
}
