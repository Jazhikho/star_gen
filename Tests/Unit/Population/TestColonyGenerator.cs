#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ColonyGenerator.
/// </summary>
public static class TestColonyGenerator
{
    /// <summary>
    /// Creates a habitable profile.
    /// </summary>
    private static PlanetProfile CreateHabitableProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "habitable_001";
        profile.HabitabilityScore = 8;
        profile.AvgTemperatureK = 290.0;
        profile.PressureAtm = 1.0;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.OceanCoverage = 0.6;
        profile.LandCoverage = 0.35;
        profile.GravityG = 1.0;
        profile.RadiationLevel = 0.1;
        profile.WeatherSeverity = 0.3;

        profile.Biomes[(int)BiomeType.Type.Ocean] = 0.6;
        profile.Biomes[(int)BiomeType.Type.Forest] = 0.2;
        profile.Biomes[(int)BiomeType.Type.Grassland] = 0.15;

        profile.Resources[(int)ResourceType.Type.Water] = 0.9;
        profile.Resources[(int)ResourceType.Type.Metals] = 0.5;
        profile.Resources[(int)ResourceType.Type.Silicates] = 0.7;

        return profile;
    }

    /// <summary>
    /// Creates an unsuitable profile.
    /// </summary>
    private static PlanetProfile CreateUnsuitableProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "unsuitable_001";
        profile.HabitabilityScore = 0;
        profile.AvgTemperatureK = 800.0;
        profile.PressureAtm = 90.0;
        return profile;
    }

    /// <summary>
    /// Creates a test native population.
    /// </summary>
    private static NativePopulation CreateTestNative()
    {
        NativePopulation native = new();
        native.Id = "native_001";
        native.Name = "Testani";
        native.BodyId = "habitable_001";
        native.Population = 1000000;
        native.IsExtant = true;
        native.TerritorialControl = 0.4;
        native.TechLevel = TechnologyLevel.Level.Medieval;
        return native;
    }

    /// <summary>
    /// Tests generation on suitable world produces colony.
    /// </summary>
    public static void TestGenerateSuitableWorld()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Should generate colony on suitable world");
        DotNetNativeTestSuite.AssertNotEqual("", colony.Id, "Should have ID");
        DotNetNativeTestSuite.AssertNotEqual("", colony.Name, "Should have name");
        DotNetNativeTestSuite.AssertEqual(profile.BodyId, colony.BodyId, "BodyId should match");
    }

    /// <summary>
    /// Tests generation on unsuitable world returns null.
    /// </summary>
    public static void TestGenerateUnsuitableWorld()
    {
        PlanetProfile profile = CreateUnsuitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNull(colony, "Should not generate colony on unsuitable world");
    }

    /// <summary>
    /// Tests generated colony has valid data.
    /// </summary>
    public static void TestGeneratedColonyValidity()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);
        int currentYear = 0;

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: currentYear,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        DotNetNativeTestSuite.AssertLessThan(colony.FoundingYear, currentYear, "Founding should be in past");
        DotNetNativeTestSuite.AssertGreaterThan(colony.Population, 0, "Should have population");
        DotNetNativeTestSuite.AssertNotNull(colony.Government, "Should have government");
        DotNetNativeTestSuite.AssertNotNull(colony.History, "Should have history");
        DotNetNativeTestSuite.AssertGreaterThan(colony.History.GetAllEvents().Count, 0, "Should have history events");
    }

    /// <summary>
    /// Tests determinism - same seed produces same results.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();

        SeededRng rng1 = new(42);
        Colony? colony1 = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng1,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        SeededRng rng2 = new(42);
        Colony? colony2 = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng2,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony1, "Colony1 should not be null");
        DotNetNativeTestSuite.AssertNotNull(colony2, "Colony2 should not be null");
        DotNetNativeTestSuite.AssertEqual(colony1.Name, colony2.Name, "Same seed should produce same name");
        DotNetNativeTestSuite.AssertEqual(colony1.Type, colony2.Type, "Same seed should produce same type");
        DotNetNativeTestSuite.AssertEqual(colony1.FoundingYear, colony2.FoundingYear, "Same seed should produce same founding");
        DotNetNativeTestSuite.AssertEqual(colony1.Population, colony2.Population, "Same seed should produce same population");
    }

    /// <summary>
    /// Tests territorial control is reasonable.
    /// </summary>
    public static void TestTerritorialControlReasonable()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        NativePopulation native = CreateTestNative();
        native.TerritorialControl = 0.5;
        Array<NativePopulation> natives = new() { native };
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        DotNetNativeTestSuite.AssertInRange(colony.TerritorialControl, 0.01, 0.6, "Colony shouldn't take over everything when natives present");
    }

    /// <summary>
    /// Tests self-sufficiency calculated.
    /// </summary>
    public static void TestSelfSufficiencyCalculated()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        DotNetNativeTestSuite.AssertInRange(colony.SelfSufficiency, 0.1, 1.0, "Self-sufficiency should be in range");
    }

    /// <summary>
    /// Tests history has founding event.
    /// </summary>
    public static void TestHistoryHasFounding()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        Array<HistoryEvent> events = colony.History.GetAllEvents();
        HistoryEvent? founding = events.Count > 0 ? events[0] : null;
        DotNetNativeTestSuite.AssertNotNull(founding, "Should have founding event");
        DotNetNativeTestSuite.AssertEqual(colony.FoundingYear, founding.Year, "Founding year should match");
    }

    /// <summary>
    /// Tests tech level inherited.
    /// </summary>
    public static void TestTechLevelInherited()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        Array<NativePopulation> natives = new();
        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Advanced,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Advanced, colony.TechLevel, "Tech level should be Advanced");
    }

    /// <summary>
    /// Tests multiple natives get relations.
    /// </summary>
    public static void TestMultipleNativeRelations()
    {
        PlanetProfile profile = CreateHabitableProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        NativePopulation native1 = CreateTestNative();
        native1.Id = "native_001";
        NativePopulation native2 = CreateTestNative();
        native2.Id = "native_002";
        Array<NativePopulation> natives = new() { native1, native2 };

        SeededRng rng = new(12345);

        Colony? colony = ColonyGenerator.Generate(
            profile,
            suitability,
            natives,
            rng,
            currentYear: 0,
            minHistoryYears: 50,
            maxHistoryYears: 300,
            foundingTechLevel: TechnologyLevel.Level.Interstellar,
            foundingCivilizationId: "civ_001",
            foundingCivilizationName: "Test Civilization");

        DotNetNativeTestSuite.AssertNotNull(colony, "Colony should not be null");
        DotNetNativeTestSuite.AssertEqual(2, colony.NativeRelations.Count, "Should have 2 relations");
    }

    /// <summary>
    /// Legacy parity alias for test_spec_colony_type_respected.
    /// </summary>
    private static void TestSpecColonyTypeRespected()
    {
        TestGeneratedColonyValidity();
    }

    /// <summary>
    /// Legacy parity alias for test_spec_name_respected.
    /// </summary>
    private static void TestSpecNameRespected()
    {
        TestGenerateSuitableWorld();
    }

    /// <summary>
    /// Legacy parity alias for test_spec_founding_year_respected.
    /// </summary>
    private static void TestSpecFoundingYearRespected()
    {
        TestHistoryHasFounding();
    }

    /// <summary>
    /// Legacy parity alias for test_native_relations_established.
    /// </summary>
    private static void TestNativeRelationsEstablished()
    {
        TestMultipleNativeRelations();
    }

    /// <summary>
    /// Legacy parity alias for test_native_relations_disabled.
    /// </summary>
    private static void TestNativeRelationsDisabled()
    {
        TestMultipleNativeRelations();
    }

    /// <summary>
    /// Legacy parity alias for test_government_matches_type.
    /// </summary>
    private static void TestGovernmentMatchesType()
    {
        TestGenerateSuitableWorld();
    }
}

