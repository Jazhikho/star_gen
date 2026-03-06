#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Fixtures;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Rng;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Golden master regression tests for all generators.
/// Verifies that generators produce consistent output for known seeds.
/// </summary>
public static class TestGoldenMasters
{
    private static void AssertBodiesDeterministic(CelestialBody first, CelestialBody second, string message)
    {
        Godot.Collections.Dictionary firstData = CelestialSerializer.ToDict(first);
        Godot.Collections.Dictionary secondData = CelestialSerializer.ToDict(second);
        NormalizeTransientFields(firstData);
        NormalizeTransientFields(secondData);

        string json1 = Json.Stringify(firstData);
        string json2 = Json.Stringify(secondData);
        if (json1 != json2)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void NormalizeTransientFields(Variant value)
    {
        if (value.VariantType == Variant.Type.Dictionary)
        {
            Godot.Collections.Dictionary dictionary = value.AsGodotDictionary();
            dictionary.Remove("created_timestamp");
            foreach (Variant key in dictionary.Keys)
            {
                NormalizeTransientFields(dictionary[key]);
            }

            return;
        }

        if (value.VariantType == Variant.Type.Array)
        {
            Godot.Collections.Array array = value.AsGodotArray();
            foreach (Variant item in array)
            {
                NormalizeTransientFields(item);
            }
        }
    }

    /// <summary>
    /// Tests that all fixtures can be generated without errors.
    /// </summary>
    public static void TestAllFixturesGenerate()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        if (fixtures.Count < 28)
        {
            throw new InvalidOperationException("Should generate at least 28 fixtures (7 per type)");
        }

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (!fixture.ContainsKey("name"))
            {
                throw new InvalidOperationException("Fixture should have name");
            }
            if (!fixture.ContainsKey("body"))
            {
                throw new InvalidOperationException("Fixture should have body");
            }

            Variant bodyVariant = fixture["body"];
            if (bodyVariant.VariantType != Variant.Type.Dictionary)
            {
                throw new InvalidOperationException($"Body should not be null for: {fixture["name"]}");
            }
        }
    }

    /// <summary>
    /// Tests star generation determinism.
    /// </summary>
    public static void TestStarDeterminism()
    {
        int seedVal = 99999;
        StarSpec spec1 = StarSpec.Random(seedVal);
        StarSpec spec2 = StarSpec.Random(seedVal);

        SeededRng rng1 = new SeededRng(seedVal);
        SeededRng rng2 = new SeededRng(seedVal);

        CelestialBody body1 = StarGenerator.Generate(spec1, rng1);
        CelestialBody body2 = StarGenerator.Generate(spec2, rng2);
        AssertBodiesDeterministic(body1, body2, "Star generation should be deterministic");
    }

    /// <summary>
    /// Tests planet generation determinism.
    /// </summary>
    public static void TestPlanetDeterminism()
    {
        int seedVal = 88888;
        PlanetSpec spec1 = PlanetSpec.Random(seedVal);
        PlanetSpec spec2 = PlanetSpec.Random(seedVal);
        ParentContext context = ParentContext.SunLike();

        SeededRng rng1 = new SeededRng(seedVal);
        SeededRng rng2 = new SeededRng(seedVal);

        CelestialBody body1 = PlanetGenerator.Generate(spec1, context, rng1);
        CelestialBody body2 = PlanetGenerator.Generate(spec2, context, rng2);
        AssertBodiesDeterministic(body1, body2, "Planet generation should be deterministic");
    }

    /// <summary>
    /// Tests moon generation determinism.
    /// </summary>
    public static void TestMoonDeterminism()
    {
        int seedVal = 77777;
        MoonSpec spec1 = MoonSpec.Random(seedVal);
        MoonSpec spec2 = MoonSpec.Random(seedVal);

        ParentContext context = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            5.2 * Units.AuMeters,
            1.898e27,
            6.9911e7,
            5.0e8
        );

        SeededRng rng1 = new SeededRng(seedVal);
        SeededRng rng2 = new SeededRng(seedVal);

        CelestialBody body1 = MoonGenerator.Generate(spec1, context, rng1);
        CelestialBody body2 = MoonGenerator.Generate(spec2, context, rng2);
        AssertBodiesDeterministic(body1, body2, "Moon generation should be deterministic");
    }

    /// <summary>
    /// Tests asteroid generation determinism.
    /// </summary>
    public static void TestAsteroidDeterminism()
    {
        int seedVal = 66666;
        AsteroidSpec spec1 = AsteroidSpec.Random(seedVal);
        AsteroidSpec spec2 = AsteroidSpec.Random(seedVal);
        ParentContext context = ParentContext.SunLike(2.7 * Units.AuMeters);

        SeededRng rng1 = new SeededRng(seedVal);
        SeededRng rng2 = new SeededRng(seedVal);

        CelestialBody body1 = AsteroidGenerator.Generate(spec1, context, rng1);
        CelestialBody body2 = AsteroidGenerator.Generate(spec2, context, rng2);
        AssertBodiesDeterministic(body1, body2, "Asteroid generation should be deterministic");
    }

    /// <summary>
    /// Tests that fixtures export to valid JSON.
    /// </summary>
    public static void TestFixturesExportToJson()
    {
        Godot.Collections.Dictionary jsonExports = FixtureGenerator.ExportAllToJson(true);

        if (jsonExports.Count < 28)
        {
            throw new InvalidOperationException("Should export at least 28 fixtures");
        }

        foreach (string fixtureName in jsonExports.Keys)
        {
            string jsonStr = jsonExports[fixtureName].AsString();
            if (string.IsNullOrEmpty(jsonStr))
            {
                throw new InvalidOperationException($"JSON should not be empty for: {fixtureName}");
            }

            Godot.Json json = new Godot.Json();
            Godot.Error error = json.Parse(jsonStr);
            if (error != Godot.Error.Ok)
            {
                throw new InvalidOperationException($"Should be valid JSON for: {fixtureName}");
            }
        }
    }

    /// <summary>
    /// Tests range validation for star properties.
    /// </summary>
    public static void TestStarRangeValidation()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture["type"].AsString() != "star")
            {
                continue;
            }

            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();
            Godot.Collections.Dictionary physical = bodyData["physical"].AsGodotDictionary();

            if (physical["mass_kg"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Star mass should be positive");
            }
            if (physical["radius_m"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Star radius should be positive");
            }

            if (bodyData.ContainsKey("stellar"))
            {
                Godot.Collections.Dictionary stellar = bodyData["stellar"].AsGodotDictionary();
                if (stellar.Get("luminosity_watts", 0.0).AsDouble() <= 0.0)
                {
                    throw new InvalidOperationException("Star luminosity should be positive");
                }
                if (stellar.Get("effective_temperature_k", 0.0).AsDouble() <= 0.0)
                {
                    throw new InvalidOperationException("Star temperature should be positive");
                }
                if (stellar.Get("age_years", 0.0).AsDouble() <= 0.0)
                {
                    throw new InvalidOperationException("Star age should be positive");
                }
            }
        }
    }

    /// <summary>
    /// Tests range validation for planet properties.
    /// </summary>
    public static void TestPlanetRangeValidation()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture["type"].AsString() != "planet")
            {
                continue;
            }

            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();
            Godot.Collections.Dictionary physical = bodyData["physical"].AsGodotDictionary();

            if (physical["mass_kg"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Planet mass should be positive");
            }
            if (physical["radius_m"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Planet radius should be positive");
            }

            if (bodyData.ContainsKey("orbital"))
            {
                Godot.Collections.Dictionary orbital = bodyData["orbital"].AsGodotDictionary();
                if (orbital["semi_major_axis_m"].AsDouble() <= 0.0)
                {
                    throw new InvalidOperationException("Orbital distance should be positive");
                }
                if (orbital["eccentricity"].AsDouble() < 0.0)
                {
                    throw new InvalidOperationException("Eccentricity should be non-negative");
                }
                if (orbital["eccentricity"].AsDouble() >= 1.0)
                {
                    throw new InvalidOperationException("Eccentricity should be < 1");
                }
            }
        }
    }

    /// <summary>
    /// Tests range validation for moon properties.
    /// </summary>
    public static void TestMoonRangeValidation()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture["type"].AsString() != "moon")
            {
                continue;
            }

            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();
            Godot.Collections.Dictionary physical = bodyData["physical"].AsGodotDictionary();

            if (physical["mass_kg"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Moon mass should be positive");
            }
            if (physical["radius_m"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Moon radius should be positive");
            }

            if (bodyData.ContainsKey("orbital"))
            {
                Godot.Collections.Dictionary orbital = bodyData["orbital"].AsGodotDictionary();
                if (orbital["semi_major_axis_m"].AsDouble() <= 0.0)
                {
                    throw new InvalidOperationException("Orbital distance should be positive");
                }
            }
        }
    }

    /// <summary>
    /// Tests range validation for asteroid properties.
    /// </summary>
    public static void TestAsteroidRangeValidation()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture["type"].AsString() != "asteroid")
            {
                continue;
            }

            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();
            Godot.Collections.Dictionary physical = bodyData["physical"].AsGodotDictionary();

            if (physical["mass_kg"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Asteroid mass should be positive");
            }
            if (physical["radius_m"].AsDouble() <= 0.0)
            {
                throw new InvalidOperationException("Asteroid radius should be positive");
            }
        }
    }

    /// <summary>
    /// Tests physics relationships for stars.
    /// </summary>
    public static void TestStellarPhysicsRelationships()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            if (fixture["type"].AsString() != "star")
            {
                continue;
            }

            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();
            Godot.Collections.Dictionary physical = bodyData["physical"].AsGodotDictionary();

            if (!bodyData.ContainsKey("stellar"))
            {
                continue;
            }

            Godot.Collections.Dictionary stellar = bodyData["stellar"].AsGodotDictionary();

            double radiusM = physical["radius_m"].AsDouble();
            double luminosityW = stellar.Get("luminosity_watts", 0.0).AsDouble();
            double temperatureK = stellar.Get("effective_temperature_k", 0.0).AsDouble();

            if (luminosityW <= 0.0 || temperatureK <= 0.0)
            {
                continue;
            }

            double stefanBoltzmann = 5.67e-8;
            double expectedLuminosity = 4.0 * System.Math.PI * radiusM * radiusM * stefanBoltzmann * System.Math.Pow(temperatureK, 4.0);

            double ratio = luminosityW / expectedLuminosity;
            if (ratio <= 0.3 || ratio >= 3.0)
            {
                throw new InvalidOperationException($"Stellar luminosity should roughly match Stefan-Boltzmann (ratio: {ratio})");
            }
        }
    }

    /// <summary>
    /// Tests that serialization round-trips correctly.
    /// </summary>
    public static void TestSerializationRoundtrip()
    {
        Array<Godot.Collections.Dictionary> fixtures = FixtureGenerator.GenerateAllFixtures();

        foreach (Godot.Collections.Dictionary fixture in fixtures)
        {
            Godot.Collections.Dictionary bodyData = fixture["body"].AsGodotDictionary();

            CelestialBody body = CelestialSerializer.FromDictionary(bodyData);
            if (body == null)
            {
                throw new InvalidOperationException($"Should deserialize: {fixture["name"]}");
            }

            Godot.Collections.Dictionary reSerialized = CelestialSerializer.ToDict(body);

            if (bodyData["id"].AsString() != reSerialized["id"].AsString())
            {
                throw new InvalidOperationException("ID should match after roundtrip");
            }
            if (bodyData["type"].AsInt32() != reSerialized["type"].AsInt32())
            {
                throw new InvalidOperationException("Type should match after roundtrip");
            }
        }
    }
}
