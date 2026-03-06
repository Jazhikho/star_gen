#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for population generation wired into PlanetGenerator and MoonGenerator.
/// Verifies that population data is generated, deterministic, and properly attached.
/// </summary>
public static class TestPopulationIntegration
{
    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestPopulationIntegration::test_planet_generation_with_population",
            TestPlanetGenerationWithPopulation);
        runner.RunNativeTest(
            "TestPopulationIntegration::test_planet_generation_without_population",
            TestPlanetGenerationWithoutPopulation);
        runner.RunNativeTest(
            "TestPopulationIntegration::test_population_determinism",
            TestPopulationDeterminism);
        runner.RunNativeTest(
            "TestPopulationIntegration::test_moon_generation_with_population",
            TestMoonGenerationWithPopulation);
        runner.RunNativeTest(
            "TestPopulationIntegration::test_population_serialization_roundtrip",
            TestPopulationSerializationRoundtrip);
        runner.RunNativeTest(
            "TestPopulationIntegration::test_population_json_roundtrip",
            TestPopulationJsonRoundtrip);
    }

    /// <summary>
    /// Tests that planet generation with population enabled produces population data.
    /// </summary>
    private static void TestPlanetGenerationWithPopulation()
    {
        PlanetSpec spec = new(42, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();
        SeededRng rng = new(42);

        CelestialBody body = PlanetGenerator.Generate(spec, context, rng, true);

        DotNetNativeTestSuite.AssertNotNull(body, "Planet should be generated");
        DotNetNativeTestSuite.AssertTrue(body.HasPopulationData(), "Population data should be present when enabled");
        DotNetNativeTestSuite.AssertNotNull(body.PopulationData.Profile, "Profile should be generated");
        DotNetNativeTestSuite.AssertNotNull(body.PopulationData.Suitability, "Suitability should be generated");
    }

    /// <summary>
    /// Tests that planet generation without population flag does not produce population data.
    /// </summary>
    private static void TestPlanetGenerationWithoutPopulation()
    {
        PlanetSpec spec = new(42, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();
        SeededRng rng = new(42);

        CelestialBody body = PlanetGenerator.Generate(spec, context, rng, false);

        DotNetNativeTestSuite.AssertNotNull(body, "Planet should be generated");
        DotNetNativeTestSuite.AssertFalse(body.HasPopulationData(), "Population data should not be present when disabled");
    }

    /// <summary>
    /// Tests determinism: same seed produces same population data.
    /// </summary>
    private static void TestPopulationDeterminism()
    {
        PlanetSpec spec1 = new(100, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        PlanetSpec spec2 = new(100, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();

        SeededRng rng1 = new(100);
        SeededRng rng2 = new(100);

        CelestialBody body1 = PlanetGenerator.Generate(spec1, context, rng1, true);
        CelestialBody body2 = PlanetGenerator.Generate(spec2, context, rng2, true);

        DotNetNativeTestSuite.AssertNotNull(body1.PopulationData, "First body should have population data");
        DotNetNativeTestSuite.AssertNotNull(body2.PopulationData, "Second body should have population data");

        DotNetNativeTestSuite.AssertEqual(
            body1.PopulationData.Profile.HabitabilityScore,
            body2.PopulationData.Profile.HabitabilityScore,
            "Habitability scores should match");
        DotNetNativeTestSuite.AssertEqual(
            body1.PopulationData.Suitability.OverallScore,
            body2.PopulationData.Suitability.OverallScore,
            "Suitability scores should match");
        DotNetNativeTestSuite.AssertEqual(
            body1.PopulationData.NativePopulations.Count,
            body2.PopulationData.NativePopulations.Count,
            "Native population count should match");
        DotNetNativeTestSuite.AssertEqual(
            body1.PopulationData.Colonies.Count,
            body2.PopulationData.Colonies.Count,
            "Colony count should match");
    }

    /// <summary>
    /// Tests that moon generation with population enabled produces population data.
    /// </summary>
    private static void TestMoonGenerationWithPopulation()
    {
        MoonSpec spec = new(42, (int)SizeCategory.Category.SubTerrestrial);
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
        SeededRng rng = new(42);

        CelestialBody body = MoonGenerator.Generate(spec, context, rng, true);

        DotNetNativeTestSuite.AssertNotNull(body, "Moon should be generated");
        DotNetNativeTestSuite.AssertTrue(body.HasPopulationData(), "Population data should be present when enabled");
    }

    /// <summary>
    /// Tests serialization round-trip of population data.
    /// </summary>
    private static void TestPopulationSerializationRoundtrip()
    {
        PlanetSpec spec = new(55, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();
        SeededRng rng = new(55);

        CelestialBody body = PlanetGenerator.Generate(spec, context, rng, true);
        DotNetNativeTestSuite.AssertTrue(body.HasPopulationData(), "Original body should have population data");

        Godot.Collections.Dictionary data = CelestialSerializer.ToDictionary(body);
        DotNetNativeTestSuite.AssertTrue(data.ContainsKey("population_data"), "Serialized data should include population_data");

        CelestialBody restored = CelestialSerializer.FromDictionary(data);
        DotNetNativeTestSuite.AssertNotNull(restored, "Restored body should not be null");
        DotNetNativeTestSuite.AssertTrue(restored.HasPopulationData(), "Restored body should have population data");

        DotNetNativeTestSuite.AssertEqual(
            body.PopulationData.Profile.HabitabilityScore,
            restored.PopulationData.Profile.HabitabilityScore,
            "Habitability score should survive round-trip");
        DotNetNativeTestSuite.AssertEqual(
            body.PopulationData.Suitability.OverallScore,
            restored.PopulationData.Suitability.OverallScore,
            "Suitability score should survive round-trip");
    }

    /// <summary>
    /// Tests JSON serialization round-trip of population data.
    /// </summary>
    private static void TestPopulationJsonRoundtrip()
    {
        PlanetSpec spec = new(77, (int)SizeCategory.Category.SuperEarth, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();
        SeededRng rng = new(77);

        CelestialBody body = PlanetGenerator.Generate(spec, context, rng, true);
        DotNetNativeTestSuite.AssertTrue(body.HasPopulationData(), "Original body should have population data");

        string jsonStr = CelestialSerializer.ToJson(body);
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrEmpty(jsonStr), "JSON should not be empty");

        CelestialBody restored = CelestialSerializer.FromJson(jsonStr);
        DotNetNativeTestSuite.AssertNotNull(restored, "Restored body from JSON should not be null");
        DotNetNativeTestSuite.AssertTrue(restored.HasPopulationData(), "Restored body from JSON should have population data");
    }

    /// <summary>
    /// Asserts that two bodies have equal population data.
    /// </summary>
    private static void AssertPopulationDataEqual(CelestialBody a, CelestialBody b, string contextMsg)
    {
        DotNetNativeTestSuite.AssertTrue(a.HasPopulationData(), $"Body A should have population data ({contextMsg})");
        DotNetNativeTestSuite.AssertTrue(b.HasPopulationData(), $"Body B should have population data ({contextMsg})");

        if (!a.HasPopulationData() || !b.HasPopulationData())
        {
            return;
        }

        DotNetNativeTestSuite.AssertEqual(
            a.PopulationData.Profile.HabitabilityScore,
            b.PopulationData.Profile.HabitabilityScore,
            $"Habitability scores should match ({contextMsg})");
        DotNetNativeTestSuite.AssertFloatNear(
            a.PopulationData.Profile.AvgTemperatureK,
            b.PopulationData.Profile.AvgTemperatureK, 0.01,
            $"Temperatures should match ({contextMsg})");
    }
}
