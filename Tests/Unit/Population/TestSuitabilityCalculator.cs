#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for SuitabilityCalculator pure functions.
/// </summary>
public static class TestSuitabilityCalculator
{
    /// <summary>
    /// Creates an Earth-like profile for testing.
    /// </summary>
    private static PlanetProfile CreateEarthLikeProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "earth_like_001";
        profile.HabitabilityScore = 10;
        profile.AvgTemperatureK = 288.0;
        profile.PressureAtm = 1.0;
        profile.OceanCoverage = 0.71;
        profile.LandCoverage = 0.29;
        profile.IceCoverage = 0.03;
        profile.ContinentCount = 7;
        profile.MaxElevationKm = 8.8;
        profile.DayLengthHours = 24.0;
        profile.AxialTiltDeg = 23.4;
        profile.GravityG = 1.0;
        profile.TectonicActivity = 0.5;
        profile.VolcanismLevel = 0.2;
        profile.WeatherSeverity = 0.3;
        profile.MagneticFieldStrength = 1.0;
        profile.RadiationLevel = 0.1;
        profile.Albedo = 0.3;
        profile.GreenhouseFactor = 1.15;
        profile.IsTidallyLocked = false;
        profile.HasAtmosphere = true;
        profile.HasMagneticField = true;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.IsMoon = false;

        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Water, 0.9 },
            { (int)ResourceType.Type.Metals, 0.5 },
            { (int)ResourceType.Type.Silicates, 0.8 },
            { (int)ResourceType.Type.Organics, 0.7 },
            { (int)ResourceType.Type.RareElements, 0.3 },
        };

        profile.Biomes = new Dictionary
        {
            { (int)BiomeType.Type.Ocean, 0.71 },
            { (int)BiomeType.Type.Forest, 0.12 },
            { (int)BiomeType.Type.Grassland, 0.08 },
            { (int)BiomeType.Type.Desert, 0.05 },
        };

        return profile;
    }

    /// <summary>
    /// Creates a Mars-like profile for testing.
    /// </summary>
    private static PlanetProfile CreateMarsLikeProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "mars_like_001";
        profile.HabitabilityScore = 2;
        profile.AvgTemperatureK = 210.0;
        profile.PressureAtm = 0.006;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 0.95;
        profile.IceCoverage = 0.05;
        profile.ContinentCount = 0;
        profile.MaxElevationKm = 21.9;
        profile.DayLengthHours = 24.6;
        profile.AxialTiltDeg = 25.2;
        profile.GravityG = 0.38;
        profile.TectonicActivity = 0.0;
        profile.VolcanismLevel = 0.0;
        profile.WeatherSeverity = 0.4;
        profile.MagneticFieldStrength = 0.0;
        profile.RadiationLevel = 0.6;
        profile.Albedo = 0.25;
        profile.GreenhouseFactor = 1.0;
        profile.IsTidallyLocked = false;
        profile.HasAtmosphere = true;
        profile.HasMagneticField = false;
        profile.HasLiquidWater = false;
        profile.HasBreathableAtmosphere = false;
        profile.IsMoon = false;

        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Water, 0.2 },
            { (int)ResourceType.Type.Metals, 0.4 },
            { (int)ResourceType.Type.Silicates, 0.9 },
        };

        profile.Biomes = new Dictionary
        {
            { (int)BiomeType.Type.Barren, 0.95 },
            { (int)BiomeType.Type.IceSheet, 0.05 },
        };

        return profile;
    }

    /// <summary>
    /// Creates an airless moon profile for testing.
    /// </summary>
    private static PlanetProfile CreateAirlessMoonProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "moon_like_001";
        profile.HabitabilityScore = 1;
        profile.AvgTemperatureK = 250.0;
        profile.PressureAtm = 0.0;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 1.0;
        profile.IceCoverage = 0.0;
        profile.ContinentCount = 0;
        profile.MaxElevationKm = 10.0;
        profile.DayLengthHours = 708.0;
        profile.AxialTiltDeg = 1.5;
        profile.GravityG = 0.16;
        profile.TectonicActivity = 0.0;
        profile.VolcanismLevel = 0.0;
        profile.WeatherSeverity = 0.0;
        profile.MagneticFieldStrength = 0.0;
        profile.RadiationLevel = 0.9;
        profile.Albedo = 0.12;
        profile.GreenhouseFactor = 1.0;
        profile.IsTidallyLocked = true;
        profile.HasAtmosphere = false;
        profile.HasMagneticField = false;
        profile.HasLiquidWater = false;
        profile.HasBreathableAtmosphere = false;
        profile.IsMoon = true;

        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Silicates, 0.8 },
            { (int)ResourceType.Type.Metals, 0.3 },
        };

        profile.Biomes = new Dictionary
        {
            { (int)BiomeType.Type.Barren, 1.0 },
        };

        return profile;
    }

    /// <summary>
    /// Creates a Venus-like profile for testing.
    /// </summary>
    private static PlanetProfile CreateVenusLikeProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "venus_like_001";
        profile.HabitabilityScore = 0;
        profile.AvgTemperatureK = 737.0;
        profile.PressureAtm = 92.0;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 1.0;
        profile.IceCoverage = 0.0;
        profile.ContinentCount = 2;
        profile.MaxElevationKm = 11.0;
        profile.DayLengthHours = 2802.0;
        profile.AxialTiltDeg = 177.0;
        profile.GravityG = 0.91;
        profile.TectonicActivity = 0.3;
        profile.VolcanismLevel = 0.8;
        profile.WeatherSeverity = 0.9;
        profile.MagneticFieldStrength = 0.0;
        profile.RadiationLevel = 0.3;
        profile.Albedo = 0.77;
        profile.GreenhouseFactor = 2.5;
        profile.IsTidallyLocked = false;
        profile.HasAtmosphere = true;
        profile.HasMagneticField = false;
        profile.HasLiquidWater = false;
        profile.HasBreathableAtmosphere = false;
        profile.IsMoon = false;

        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Silicates, 0.7 },
            { (int)ResourceType.Type.Volatiles, 0.9 },
        };

        profile.Biomes = new Dictionary
        {
            { (int)BiomeType.Type.Volcanic, 0.3 },
            { (int)BiomeType.Type.Barren, 0.7 },
        };

        return profile;
    }

    /// <summary>
    /// Tests Earth-like planet scores high.
    /// </summary>
    public static void TestCalculateEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertInRange(suitability.OverallScore, 80, 100, "Earth-like should score 80-100");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Optimal, suitability.GetCategory(), "Should be Optimal category");
        DotNetNativeTestSuite.AssertTrue(suitability.IsColonizable(), "Should be colonizable");
    }

    /// <summary>
    /// Tests Mars-like planet scores moderately.
    /// </summary>
    public static void TestCalculateMarsLike()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertInRange(suitability.OverallScore, 20, 50, "Mars-like should score 20-50");
        DotNetNativeTestSuite.AssertTrue(suitability.IsColonizable(), "Should be colonizable");
    }

    /// <summary>
    /// Tests airless moon scores low but colonizable.
    /// </summary>
    public static void TestCalculateAirlessMoon()
    {
        PlanetProfile profile = CreateAirlessMoonProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertInRange(suitability.OverallScore, 10, 35, "Airless moon should score 10-35");
        DotNetNativeTestSuite.AssertTrue(suitability.IsColonizable(), "Should be colonizable");
    }

    /// <summary>
    /// Tests Venus-like planet is unsuitable.
    /// </summary>
    public static void TestCalculateVenusLike()
    {
        PlanetProfile profile = CreateVenusLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertLessThan(suitability.OverallScore, 15, "Venus-like should score < 15");
    }

    /// <summary>
    /// Tests body_id is preserved.
    /// </summary>
    public static void TestBodyIdPreserved()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertEqual("earth_like_001", suitability.BodyId, "BodyId should be preserved");
    }

    /// <summary>
    /// Tests all factor scores are populated.
    /// </summary>
    public static void TestAllFactorsPopulated()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorCount(), suitability.FactorScores.Count, "All factors should be populated");

        for (int i = 0; i < ColonySuitability.FactorCount(); i += 1)
        {
            ColonySuitability.FactorType factor = (ColonySuitability.FactorType)i;
            DotNetNativeTestSuite.AssertTrue(
                suitability.FactorScores.ContainsKey((int)factor),
                $"Missing factor: {ColonySuitability.FactorToString(factor)}"
            );
        }
    }

    /// <summary>
    /// Tests factor scores are in valid range.
    /// </summary>
    public static void TestFactorScoresInRange()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        foreach (Variant factorKey in suitability.FactorScores.Keys)
        {
            int score = suitability.FactorScores[factorKey].AsInt32();
            DotNetNativeTestSuite.AssertInRange(score, 0, 100, "Factor score should be in range 0-100");
        }
    }

    /// <summary>
    /// Tests temperature factor ideal range.
    /// </summary>
    public static void TestTemperatureFactorIdeal()
    {
        PlanetProfile profile = new();
        profile.AvgTemperatureK = 288.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int tempScore = suitability.GetFactorScore(ColonySuitability.FactorType.Temperature);

        DotNetNativeTestSuite.AssertEqual(100, tempScore, "Ideal temperature should score 100");
    }

    /// <summary>
    /// Tests temperature factor freezing.
    /// </summary>
    public static void TestTemperatureFactorCold()
    {
        PlanetProfile profile = new();
        profile.AvgTemperatureK = 220.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int tempScore = suitability.GetFactorScore(ColonySuitability.FactorType.Temperature);

        DotNetNativeTestSuite.AssertInRange(tempScore, 20, 50, "Cold temperature should score 20-50");
    }

    /// <summary>
    /// Tests temperature factor too extreme.
    /// </summary>
    public static void TestTemperatureFactorExtreme()
    {
        PlanetProfile profile = new();
        profile.AvgTemperatureK = 150.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int tempScore = suitability.GetFactorScore(ColonySuitability.FactorType.Temperature);

        DotNetNativeTestSuite.AssertEqual(0, tempScore, "Extreme temperature should score 0");
    }

    /// <summary>
    /// Tests gravity factor ideal range.
    /// </summary>
    public static void TestGravityFactorIdeal()
    {
        PlanetProfile profile = new();
        profile.GravityG = 1.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int gravityScore = suitability.GetFactorScore(ColonySuitability.FactorType.Gravity);

        DotNetNativeTestSuite.AssertEqual(100, gravityScore, "Earth gravity should score 100");
    }

    /// <summary>
    /// Tests gravity factor too low.
    /// </summary>
    public static void TestGravityFactorLow()
    {
        PlanetProfile profile = new();
        profile.GravityG = 0.16;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int gravityScore = suitability.GetFactorScore(ColonySuitability.FactorType.Gravity);

        DotNetNativeTestSuite.AssertInRange(gravityScore, 5, 30, "Low gravity should score 5-30");
    }

    /// <summary>
    /// Tests gravity factor too high.
    /// </summary>
    public static void TestGravityFactorTooHigh()
    {
        PlanetProfile profile = new();
        profile.GravityG = 4.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int gravityScore = suitability.GetFactorScore(ColonySuitability.FactorType.Gravity);

        DotNetNativeTestSuite.AssertEqual(0, gravityScore, "Extreme gravity should score 0");
    }

    /// <summary>
    /// Tests atmosphere factor breathable.
    /// </summary>
    public static void TestAtmosphereFactorBreathable()
    {
        PlanetProfile profile = new();
        profile.HasBreathableAtmosphere = true;
        profile.HasAtmosphere = true;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int atmoScore = suitability.GetFactorScore(ColonySuitability.FactorType.Atmosphere);

        DotNetNativeTestSuite.AssertEqual(100, atmoScore, "Breathable atmosphere should score 100");
    }

    /// <summary>
    /// Tests atmosphere factor none.
    /// </summary>
    public static void TestAtmosphereFactorNone()
    {
        PlanetProfile profile = new();
        profile.HasBreathableAtmosphere = false;
        profile.HasAtmosphere = false;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int atmoScore = suitability.GetFactorScore(ColonySuitability.FactorType.Atmosphere);

        DotNetNativeTestSuite.AssertEqual(30, atmoScore, "No atmosphere should score 30");
    }

    /// <summary>
    /// Tests water factor with ocean.
    /// </summary>
    public static void TestWaterFactorOcean()
    {
        PlanetProfile profile = new();
        profile.HasLiquidWater = true;
        profile.OceanCoverage = 0.7;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int waterScore = suitability.GetFactorScore(ColonySuitability.FactorType.Water);

        DotNetNativeTestSuite.AssertInRange(waterScore, 85, 100, "Ocean world should score 85-100 for water");
    }

    /// <summary>
    /// Tests water factor with ice only.
    /// </summary>
    public static void TestWaterFactorIce()
    {
        PlanetProfile profile = new();
        profile.HasLiquidWater = false;
        profile.IceCoverage = 0.3;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int waterScore = suitability.GetFactorScore(ColonySuitability.FactorType.Water);

        DotNetNativeTestSuite.AssertInRange(waterScore, 40, 70, "Ice world should score 40-70 for water");
    }

    /// <summary>
    /// Tests water factor with no water.
    /// </summary>
    public static void TestWaterFactorNone()
    {
        PlanetProfile profile = new();
        profile.HasLiquidWater = false;
        profile.IceCoverage = 0.0;
        profile.Resources = new Dictionary();

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int waterScore = suitability.GetFactorScore(ColonySuitability.FactorType.Water);

        DotNetNativeTestSuite.AssertEqual(5, waterScore, "No water should score 5");
    }

    /// <summary>
    /// Tests radiation factor protected.
    /// </summary>
    public static void TestRadiationFactorProtected()
    {
        PlanetProfile profile = new();
        profile.RadiationLevel = 0.1;
        profile.HasMagneticField = true;
        profile.HasAtmosphere = true;
        profile.PressureAtm = 1.0;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int radScore = suitability.GetFactorScore(ColonySuitability.FactorType.Radiation);

        DotNetNativeTestSuite.AssertInRange(radScore, 80, 100, "Protected world should score 80-100 for radiation");
    }

    /// <summary>
    /// Tests radiation factor exposed.
    /// </summary>
    public static void TestRadiationFactorExposed()
    {
        PlanetProfile profile = new();
        profile.RadiationLevel = 0.9;
        profile.HasMagneticField = false;
        profile.HasAtmosphere = false;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int radScore = suitability.GetFactorScore(ColonySuitability.FactorType.Radiation);

        DotNetNativeTestSuite.AssertInRange(radScore, 5, 25, "Exposed world should score 5-25 for radiation");
    }

    /// <summary>
    /// Tests day length factor ideal.
    /// </summary>
    public static void TestDayLengthFactorIdeal()
    {
        PlanetProfile profile = new();
        profile.DayLengthHours = 24.0;
        profile.IsTidallyLocked = false;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int dayScore = suitability.GetFactorScore(ColonySuitability.FactorType.DayLength);

        DotNetNativeTestSuite.AssertEqual(100, dayScore, "24-hour day should score 100");
    }

    /// <summary>
    /// Tests day length factor tidally locked.
    /// </summary>
    public static void TestDayLengthFactorTidallyLocked()
    {
        PlanetProfile profile = new();
        profile.IsTidallyLocked = true;

        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);
        int dayScore = suitability.GetFactorScore(ColonySuitability.FactorType.DayLength);

        DotNetNativeTestSuite.AssertEqual(40, dayScore, "Tidally locked should score 40");
    }

    /// <summary>
    /// Tests limiting factors are identified.
    /// </summary>
    public static void TestLimitingFactorsIdentified()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.LimitingFactors.Count, 0, "Should have limiting factors");

        foreach (ColonySuitability.FactorType factor in suitability.LimitingFactors)
        {
            int score = suitability.GetFactorScore(factor);
            DotNetNativeTestSuite.AssertLessThan(score, 50, "Limiting factor should have score < 50");
        }
    }

    /// <summary>
    /// Tests advantages are identified.
    /// </summary>
    public static void TestAdvantagesIdentified()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.Advantages.Count, 0, "Should have advantages");

        foreach (ColonySuitability.FactorType factor in suitability.Advantages)
        {
            int score = suitability.GetFactorScore(factor);
            DotNetNativeTestSuite.AssertGreaterThan(score, 69, "Advantage should have score >= 70");
        }
    }

    /// <summary>
    /// Tests limiting factors are sorted by severity.
    /// </summary>
    public static void TestLimitingFactorsSorted()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        if (suitability.LimitingFactors.Count >= 2)
        {
            int prevScore = suitability.GetFactorScore((ColonySuitability.FactorType)suitability.LimitingFactors[0]);
            for (int i = 1; i < suitability.LimitingFactors.Count; i += 1)
            {
                int currScore = suitability.GetFactorScore((ColonySuitability.FactorType)suitability.LimitingFactors[i]);
                DotNetNativeTestSuite.AssertTrue(currScore >= prevScore, "Limiting factors should be sorted worst first");
                prevScore = currScore;
            }
        }
    }

    /// <summary>
    /// Tests advantages are sorted by strength.
    /// </summary>
    public static void TestAdvantagesSorted()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        if (suitability.Advantages.Count >= 2)
        {
            int prevScore = suitability.GetFactorScore((ColonySuitability.FactorType)suitability.Advantages[0]);
            for (int i = 1; i < suitability.Advantages.Count; i += 1)
            {
                int currScore = suitability.GetFactorScore((ColonySuitability.FactorType)suitability.Advantages[i]);
                DotNetNativeTestSuite.AssertTrue(currScore <= prevScore, "Advantages should be sorted best first");
                prevScore = currScore;
            }
        }
    }

    /// <summary>
    /// Tests carrying capacity for Earth-like.
    /// </summary>
    public static void TestCarryingCapacityEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.CarryingCapacity, 1000000000, "Earth-like should support > 1 billion");
    }

    /// <summary>
    /// Tests carrying capacity for hostile world.
    /// </summary>
    public static void TestCarryingCapacityHostile()
    {
        PlanetProfile profile = CreateAirlessMoonProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        if (suitability.IsColonizable())
        {
            DotNetNativeTestSuite.AssertGreaterThan(suitability.CarryingCapacity, 0, "Colonizable world should have capacity > 0");
            DotNetNativeTestSuite.AssertLessThan(suitability.CarryingCapacity, 1000000000, "Hostile world should have less capacity than Earth-like");
        }
    }

    /// <summary>
    /// Tests carrying capacity zero for unsuitable.
    /// </summary>
    public static void TestCarryingCapacityUnsuitable()
    {
        PlanetProfile profile = CreateVenusLikeProfile();
        profile.AvgTemperatureK = 1000.0;
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        if (!suitability.IsColonizable())
        {
            DotNetNativeTestSuite.AssertEqual(0, suitability.CarryingCapacity, "Unsuitable world should have 0 capacity");
        }
    }

    /// <summary>
    /// Tests growth rate for Earth-like.
    /// </summary>
    public static void TestGrowthRateEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.BaseGrowthRate, 0.02, "Earth-like should have > 2% growth");
        DotNetNativeTestSuite.AssertLessThan(suitability.BaseGrowthRate, 0.04, "Growth rate should be < 4%");
    }

    /// <summary>
    /// Tests growth rate for hostile world.
    /// </summary>
    public static void TestGrowthRateHostile()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.BaseGrowthRate, 0.0, "Colonizable world should have > 0 growth");
        DotNetNativeTestSuite.AssertLessThan(suitability.BaseGrowthRate, 0.02, "Hostile world should have < 2% growth");
    }

    /// <summary>
    /// Tests infrastructure difficulty for Earth-like.
    /// </summary>
    public static void TestInfrastructureDifficultyEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertFloatNear(1.0, suitability.InfrastructureDifficulty, 0.3, "Earth-like should have ~1.0 infrastructure difficulty");
    }

    /// <summary>
    /// Tests infrastructure difficulty for hostile world.
    /// </summary>
    public static void TestInfrastructureDifficultyHostile()
    {
        PlanetProfile profile = CreateAirlessMoonProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertGreaterThan(suitability.InfrastructureDifficulty, 1.5, "Hostile world should have > 1.5 infrastructure difficulty");
    }

    /// <summary>
    /// Tests population projection logistic growth.
    /// </summary>
    public static void TestProjectPopulationGrowth()
    {
        int pop = SuitabilityCalculator.ProjectPopulation(1000, 50, 0.03, 1000000);

        DotNetNativeTestSuite.AssertGreaterThan(pop, 1000, "Population should grow over time");
        DotNetNativeTestSuite.AssertLessThan(pop, 1000000, "Population should not exceed capacity");
    }

    /// <summary>
    /// Tests population projection approaches capacity.
    /// </summary>
    public static void TestProjectPopulationApproachesCapacity()
    {
        int capacity = 10000;
        int pop = SuitabilityCalculator.ProjectPopulation(1000, 500, 0.05, capacity);

        DotNetNativeTestSuite.AssertGreaterThan(pop, (int)(capacity * 0.9), "Population should approach capacity");
    }

    /// <summary>
    /// Tests population projection with zero growth.
    /// </summary>
    public static void TestProjectPopulationZeroGrowth()
    {
        int pop = SuitabilityCalculator.ProjectPopulation(1000, 100, 0.0, 10000);

        DotNetNativeTestSuite.AssertEqual(1000, pop, "Zero growth should maintain initial population");
    }

    /// <summary>
    /// Tests population projection at capacity.
    /// </summary>
    public static void TestProjectPopulationAtCapacity()
    {
        int pop = SuitabilityCalculator.ProjectPopulation(10000, 50, 0.03, 10000);

        DotNetNativeTestSuite.AssertEqual(10000, pop, "Population at capacity should stay at capacity");
    }

    /// <summary>
    /// Tests population projection with zero years.
    /// </summary>
    public static void TestProjectPopulationZeroYears()
    {
        int pop = SuitabilityCalculator.ProjectPopulation(5000, 0, 0.03, 100000);

        DotNetNativeTestSuite.AssertEqual(5000, pop, "Zero years should return initial population");
    }

    /// <summary>
    /// Tests equipment requirements for Earth-like world.
    /// </summary>
    public static void TestEquipmentRequirementsEarthLike()
    {
        PlanetProfile profile = CreateEarthLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertFalse(suitability.RequiresLifeSupport, "Earth-like should not require life support");
        DotNetNativeTestSuite.AssertFalse(suitability.RequiresPressureSuit, "Earth-like should not require pressure suit");
        DotNetNativeTestSuite.AssertFalse(suitability.RequiresRadiationShielding, "Earth-like should not require radiation shielding");
    }

    /// <summary>
    /// Tests equipment requirements for Mars-like world.
    /// </summary>
    public static void TestEquipmentRequirementsMarsLike()
    {
        PlanetProfile profile = CreateMarsLikeProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertTrue(suitability.RequiresLifeSupport, "Mars-like should require life support");
        DotNetNativeTestSuite.AssertTrue(suitability.RequiresPressureSuit, "Mars-like should require pressure suit");
        DotNetNativeTestSuite.AssertTrue(suitability.RequiresRadiationShielding, "Mars-like should require radiation shielding");
    }

    /// <summary>
    /// Tests equipment requirements for airless moon.
    /// </summary>
    public static void TestEquipmentRequirementsAirless()
    {
        PlanetProfile profile = CreateAirlessMoonProfile();
        ColonySuitability suitability = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertTrue(suitability.RequiresLifeSupport, "Airless moon should require life support");
        DotNetNativeTestSuite.AssertTrue(suitability.RequiresPressureSuit, "Airless moon should require pressure suit");
        DotNetNativeTestSuite.AssertTrue(suitability.RequiresRadiationShielding, "Airless moon should require radiation shielding");
    }

    /// <summary>
    /// Tests determinism - same inputs give same results.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetProfile profile = CreateEarthLikeProfile();

        ColonySuitability result1 = SuitabilityCalculator.Calculate(profile);
        ColonySuitability result2 = SuitabilityCalculator.Calculate(profile);

        DotNetNativeTestSuite.AssertEqual(result1.OverallScore, result2.OverallScore, "OverallScore should be deterministic");
        DotNetNativeTestSuite.AssertEqual(result1.CarryingCapacity, result2.CarryingCapacity, "CarryingCapacity should be deterministic");
        DotNetNativeTestSuite.AssertFloatNear(result1.BaseGrowthRate, result2.BaseGrowthRate, 0.0001, "BaseGrowthRate should be deterministic");
    }
}
