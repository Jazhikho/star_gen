#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Golden master tests for population generation determinism.
/// Verifies that the same seed always produces the same population data
/// across regeneration, including profile, suitability, natives, and colonies.
/// </summary>
public static class TestPopulationGoldenMasters
{
    private static readonly int[] GoldenSeeds = [42, 100, 255, 999, 12345];

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestPopulationGoldenMasters::test_planet_population_determinism_across_regenerations",
            TestPlanetPopulationDeterminismAcrossRegenerations);
        runner.RunNativeTest(
            "TestPopulationGoldenMasters::test_population_dict_roundtrip_determinism",
            TestPopulationDictRoundtripDeterminism);
        runner.RunNativeTest(
            "TestPopulationGoldenMasters::test_population_json_roundtrip_determinism",
            TestPopulationJsonRoundtripDeterminism);
        runner.RunNativeTest(
            "TestPopulationGoldenMasters::test_population_seeding_order_independence",
            TestPopulationSeedingOrderIndependence);
        runner.RunNativeTest(
            "TestPopulationGoldenMasters::test_population_does_not_affect_core_generation",
            TestPopulationDoesNotAffectCoreGeneration);
    }

    /// <summary>
    /// Tests that multiple regenerations of the same planet produce identical population data.
    /// </summary>
    private static void TestPlanetPopulationDeterminismAcrossRegenerations()
    {
        foreach (int seedVal in GoldenSeeds)
        {
            PlanetSpec spec1 = new(seedVal, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
            PlanetSpec spec2 = new(seedVal, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
            ParentContext context = ParentContext.SunLike();

            SeededRng rng1 = new(seedVal);
            SeededRng rng2 = new(seedVal);

            CelestialBody body1 = PlanetGenerator.Generate(spec1, context, rng1, true);
            CelestialBody body2 = PlanetGenerator.Generate(spec2, context, rng2, true);

            AssertPopulationDataEqual(body1, body2, $"seed {seedVal}");
        }
    }

    /// <summary>
    /// Tests that population data survives dict serialization round-trip.
    /// </summary>
    private static void TestPopulationDictRoundtripDeterminism()
    {
        foreach (int seedVal in GoldenSeeds)
        {
            PlanetSpec spec = new(seedVal, (int)SizeCategory.Category.SuperEarth, (int)OrbitZone.Zone.Temperate);
            ParentContext context = ParentContext.SunLike();
            SeededRng rng = new(seedVal);

            CelestialBody original = PlanetGenerator.Generate(spec, context, rng, true);

            Godot.Collections.Dictionary data = CelestialSerializer.ToDictionary(original);
            CelestialBody restored = CelestialSerializer.FromDictionary(data);

            AssertPopulationDataEqual(original, restored, $"dict roundtrip seed {seedVal}");
        }
    }

    /// <summary>
    /// Tests that population data survives JSON serialization round-trip.
    /// </summary>
    private static void TestPopulationJsonRoundtripDeterminism()
    {
        foreach (int seedVal in GoldenSeeds)
        {
            PlanetSpec spec = new(seedVal, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
            ParentContext context = ParentContext.SunLike();
            SeededRng rng = new(seedVal);

            CelestialBody original = PlanetGenerator.Generate(spec, context, rng, true);

            string jsonStr = CelestialSerializer.ToJson(original);
            CelestialBody restored = CelestialSerializer.FromJson(jsonStr);

            AssertPopulationDataEqual(original, restored, $"json roundtrip seed {seedVal}");
        }
    }

    /// <summary>
    /// Tests population seeding is order-independent.
    /// </summary>
    private static void TestPopulationSeedingOrderIndependence()
    {
        int baseSeed = 42000;
        string[] bodyIds = ["planet_01", "planet_02", "planet_03", "moon_01_01", "moon_02_01"];

        System.Collections.Generic.Dictionary<string, int> forwardSeeds = new();
        foreach (string id in bodyIds)
        {
            forwardSeeds[id] = (int)PopulationSeeding.GeneratePopulationSeed(id, baseSeed);
        }

        System.Collections.Generic.Dictionary<string, int> reverseSeeds = new();
        string[] reversedIds = new string[bodyIds.Length];
        System.Array.Copy(bodyIds, reversedIds, bodyIds.Length);
        System.Array.Reverse(reversedIds);
        foreach (string id in reversedIds)
        {
            reverseSeeds[id] = (int)PopulationSeeding.GeneratePopulationSeed(id, baseSeed);
        }

        foreach (string id in bodyIds)
        {
            DotNetNativeTestSuite.AssertEqual(
                forwardSeeds[id], reverseSeeds[id],
                $"Seed for '{id}' should be same regardless of generation order"
            );
        }
    }

    /// <summary>
    /// Tests that enabling population does not change the core body generation.
    /// </summary>
    private static void TestPopulationDoesNotAffectCoreGeneration()
    {
        int seedVal = 42;
        PlanetSpec specNoPop = new(seedVal, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        PlanetSpec specPop = new(seedVal, (int)SizeCategory.Category.Terrestrial, (int)OrbitZone.Zone.Temperate);
        ParentContext context = ParentContext.SunLike();

        SeededRng rngNoPop = new(seedVal);
        SeededRng rngPop = new(seedVal);

        CelestialBody bodyNoPop = PlanetGenerator.Generate(specNoPop, context, rngNoPop, false);
        CelestialBody bodyPop = PlanetGenerator.Generate(specPop, context, rngPop, true);

        DotNetNativeTestSuite.AssertEqual(bodyNoPop.Id, bodyPop.Id, "IDs should match");
        DotNetNativeTestSuite.AssertFloatNear(bodyNoPop.Physical.MassKg, bodyPop.Physical.MassKg, 1.0, "Mass should match");
        DotNetNativeTestSuite.AssertFloatNear(bodyNoPop.Physical.RadiusM, bodyPop.Physical.RadiusM, 1.0, "Radius should match");
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

        PlanetPopulationData pa = a.PopulationData;
        PlanetPopulationData pb = b.PopulationData;

        if (pa.Profile != null && pb.Profile != null)
        {
            DotNetNativeTestSuite.AssertEqual(
                pa.Profile.HabitabilityScore, pb.Profile.HabitabilityScore,
                $"Habitability scores should match ({contextMsg})");
            DotNetNativeTestSuite.AssertFloatNear(
                pa.Profile.AvgTemperatureK, pb.Profile.AvgTemperatureK, 0.01,
                $"Temperatures should match ({contextMsg})");
            DotNetNativeTestSuite.AssertFloatNear(
                pa.Profile.OceanCoverage, pb.Profile.OceanCoverage, 0.001,
                $"Ocean coverage should match ({contextMsg})");
        }

        if (pa.Suitability != null && pb.Suitability != null)
        {
            DotNetNativeTestSuite.AssertEqual(
                pa.Suitability.OverallScore, pb.Suitability.OverallScore,
                $"Suitability scores should match ({contextMsg})");
        }

        DotNetNativeTestSuite.AssertEqual(
            pa.NativePopulations.Count, pb.NativePopulations.Count,
            $"Native population counts should match ({contextMsg})");
        for (int i = 0; i < System.Math.Min(pa.NativePopulations.Count, pb.NativePopulations.Count); i++)
        {
            DotNetNativeTestSuite.AssertEqual(
                pa.NativePopulations[i].Population, pb.NativePopulations[i].Population,
                $"Native population {i} count should match ({contextMsg})");
        }

        DotNetNativeTestSuite.AssertEqual(
            pa.Colonies.Count, pb.Colonies.Count,
            $"Colony counts should match ({contextMsg})");
        for (int i = 0; i < System.Math.Min(pa.Colonies.Count, pb.Colonies.Count); i++)
        {
            DotNetNativeTestSuite.AssertEqual(
                pa.Colonies[i].Population, pb.Colonies[i].Population,
                $"Colony {i} population should match ({contextMsg})");
        }
    }
}
