#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for NativePopulationGenerator.
/// </summary>
public static class TestNativePopulationGenerator
{
    /// <summary>
    /// Creates a habitable Earth-like profile.
    /// </summary>
    private static PlanetProfile CreateHabitableProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "habitable_001";
        profile.HabitabilityScore = 9;
        profile.AvgTemperatureK = 288.0;
        profile.PressureAtm = 1.0;
        profile.HasLiquidWater = true;
        profile.OceanCoverage = 0.7;
        profile.LandCoverage = 0.25;
        profile.ContinentCount = 5;
        profile.GravityG = 1.0;
        profile.VolcanismLevel = 0.2;
        profile.TectonicActivity = 0.4;

        profile.Biomes[(int)BiomeType.Type.Ocean] = 0.7;
        profile.Biomes[(int)BiomeType.Type.Forest] = 0.12;
        profile.Biomes[(int)BiomeType.Type.Grassland] = 0.08;
        profile.Biomes[(int)BiomeType.Type.Desert] = 0.05;
        profile.Biomes[(int)BiomeType.Type.Tundra] = 0.05;

        profile.Resources[(int)ResourceType.Type.Water] = 0.9;
        profile.Resources[(int)ResourceType.Type.Metals] = 0.5;
        profile.Resources[(int)ResourceType.Type.Silicates] = 0.8;
        profile.Resources[(int)ResourceType.Type.Organics] = 0.6;
        profile.Resources[(int)ResourceType.Type.RareElements] = 0.3;

        return profile;
    }

    /// <summary>
    /// Creates an uninhabitable profile.
    /// </summary>
    private static PlanetProfile CreateUninhabitableProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "uninhabitable_001";
        profile.HabitabilityScore = 1;
        profile.AvgTemperatureK = 150.0;
        profile.PressureAtm = 0.0;
        profile.HasLiquidWater = false;
        profile.OceanCoverage = 0.0;
        profile.Biomes[(int)BiomeType.Type.Barren] = 1.0;
        return profile;
    }

    /// <summary>
    /// Tests generation on habitable world produces populations.
    /// </summary>
    public static void TestGenerateHabitableWorld()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        DotNetNativeTestSuite.AssertGreaterThan(populations.Count, 0, "Habitable world with force should have populations");
    }

    /// <summary>
    /// Tests generation on uninhabitable world produces no populations.
    /// </summary>
    public static void TestGenerateUninhabitableWorld()
    {
        PlanetProfile profile = CreateUninhabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: false,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        DotNetNativeTestSuite.AssertEqual(0, populations.Count, "Uninhabitable world should have no populations");
    }

    /// <summary>
    /// Tests generated populations have valid data.
    /// </summary>
    public static void TestGeneratedPopulationValidity()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);
        int currentYear = 0;

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: currentYear,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        foreach (NativePopulation pop in populations)
        {
            DotNetNativeTestSuite.AssertNotEqual("", pop.Id, "Should have ID");
            DotNetNativeTestSuite.AssertNotEqual("", pop.Name, "Should have name");
            DotNetNativeTestSuite.AssertEqual(profile.BodyId, pop.BodyId, "Should link to planet");
            DotNetNativeTestSuite.AssertLessThan(pop.OriginYear, currentYear, "Origin should be in past");
            DotNetNativeTestSuite.AssertGreaterThan(pop.Population, 0, "Should have population");
            DotNetNativeTestSuite.AssertNotNull(pop.Government, "Should have government");
            DotNetNativeTestSuite.AssertNotNull(pop.History, "Should have history");
        }
    }

    /// <summary>
    /// Tests generated populations have history with founding event.
    /// </summary>
    public static void TestGeneratedPopulationHasFounding()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        foreach (NativePopulation pop in populations)
        {
            HistoryEvent? founding = pop.History.GetFoundingEvent();
            DotNetNativeTestSuite.AssertNotNull(founding, "Should have founding event");
            DotNetNativeTestSuite.AssertEqual(pop.OriginYear, founding.Year, "Founding year should match origin");
        }
    }

    /// <summary>
    /// Tests determinism - same seed produces same results.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetProfile profile = CreateHabitableProfile();

        SeededRng rng1 = new(42);
        Array<NativePopulation> pop1 = NativePopulationGenerator.Generate(
            profile,
            rng1,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        SeededRng rng2 = new(42);
        Array<NativePopulation> pop2 = NativePopulationGenerator.Generate(
            profile,
            rng2,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        DotNetNativeTestSuite.AssertEqual(pop1.Count, pop2.Count, "Same seed should produce same count");

        for (int i = 0; i < pop1.Count; i++)
        {
            DotNetNativeTestSuite.AssertEqual(pop1[i].Name, pop2[i].Name, "Same seed should produce same names");
            DotNetNativeTestSuite.AssertEqual(pop1[i].OriginYear, pop2[i].OriginYear, "Same seed should produce same origin");
            DotNetNativeTestSuite.AssertEqual(pop1[i].TechLevel, pop2[i].TechLevel, "Same seed should produce same tech");
        }
    }

    /// <summary>
    /// Tests max_populations is respected.
    /// </summary>
    public static void TestMaxPopulationsRespected()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 1,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        DotNetNativeTestSuite.AssertLessThan(populations.Count, 2, "Should respect max_populations");
    }

    /// <summary>
    /// Tests territorial control sums reasonably.
    /// </summary>
    public static void TestTerritorialControlReasonable()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        double totalControl = 0.0;
        foreach (NativePopulation pop in populations)
        {
            totalControl += pop.TerritorialControl;
            DotNetNativeTestSuite.AssertInRange(pop.TerritorialControl, 0.0, 1.0, "Individual control should be 0-1");
        }

        DotNetNativeTestSuite.AssertLessThan(totalControl, 1.5, "Total territorial control should be reasonable");
    }

    /// <summary>
    /// Tests tech level appropriate for history length.
    /// </summary>
    public static void TestTechLevelAppropriate()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 100,
            maxHistoryYears: 500);

        foreach (NativePopulation pop in populations)
        {
            DotNetNativeTestSuite.AssertLessThan((int)pop.TechLevel, (int)TechnologyLevel.Level.Spacefaring, "Short history should not produce spacefaring tech");
        }
    }

    /// <summary>
    /// Tests cultural traits are generated.
    /// </summary>
    public static void TestCulturalTraitsGenerated()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        foreach (NativePopulation pop in populations)
        {
            DotNetNativeTestSuite.AssertGreaterThan(pop.CulturalTraits.Count, 0, "Should have cultural traits");
        }
    }

    /// <summary>
    /// Tests primary biome is from profile biomes.
    /// </summary>
    public static void TestPrimaryBiomeValid()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        foreach (NativePopulation pop in populations)
        {
            DotNetNativeTestSuite.AssertNotEqual("", pop.PrimaryBiome, "Should have primary biome");
        }
    }

    /// <summary>
    /// Tests government is appropriate for tech level.
    /// </summary>
    public static void TestGovernmentAppropriate()
    {
        PlanetProfile profile = CreateHabitableProfile();
        SeededRng rng = new(12345);

        Array<NativePopulation> populations = NativePopulationGenerator.Generate(
            profile,
            rng,
            currentYear: 0,
            maxPopulations: 3,
            forcePopulation: true,
            minHistoryYears: 1000,
            maxHistoryYears: 50000);

        foreach (NativePopulation pop in populations)
        {
            DotNetNativeTestSuite.AssertNotNull(pop.Government, "Should have government");
            if (pop.TechLevel == TechnologyLevel.Level.StoneAge)
            {
                DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Tribal, pop.Government.Regime, "Stone age should have tribal government");
            }
        }
    }
}
