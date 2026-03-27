#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PopulationLikelihood: likelihood estimation and seed-based population checks.
/// </summary>
public static class TestPopulationLikelihood
{
    /// <summary>
    /// Tests that estimate_native_likelihood matches PopulationProbability.calculate_native_probability.
    /// </summary>
    public static void TestEstimateNativeLikelihoodMatchesProbability()
    {
        PlanetProfile profile = new();
        profile.BodyId = "test";
        profile.HabitabilityScore = 7;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = false;
        profile.IsTidallyLocked = false;
        profile.RadiationLevel = 0.3;
        profile.IsMoon = false;
        profile.TidalHeatingFactor = 0.0;

        double likelihood = PopulationLikelihood.EstimateNativeLikelihood(profile);
        double prob = PopulationProbability.CalculateNativeProbability(profile);
        DotNetNativeTestSuite.AssertFloatNear(prob, likelihood, 0.0001, "Likelihood should match probability calculation");
    }

    /// <summary>
    /// Tests that derive_roll_value is deterministic for same seed + salt.
    /// </summary>
    public static void TestDeriveRollValueDeterministic()
    {
        long seedVal = 12345;
        int salt = PopulationLikelihood.NativeRollSalt;

        double roll1 = PopulationLikelihood.DeriveRollValue(seedVal, salt);
        double roll2 = PopulationLikelihood.DeriveRollValue(seedVal, salt);
        DotNetNativeTestSuite.AssertFloatNear(roll1, roll2, 0.0, "Same seed+salt must yield same roll");
    }

    /// <summary>
    /// Tests that derive_roll_value returns value in [0, 1).
    /// </summary>
    public static void TestDeriveRollValueInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            long seedVal = i * 7777;
            double roll = PopulationLikelihood.DeriveRollValue(seedVal, PopulationLikelihood.ColonyRollSalt);
            DotNetNativeTestSuite.AssertTrue(roll >= 0.0, $"Roll must be >= 0 for seed {seedVal}");
            DotNetNativeTestSuite.AssertTrue(roll < 1.0, $"Roll must be < 1 for seed {seedVal}");
        }
    }

    /// <summary>
    /// Tests that native and colony use different derived values (different salts).
    /// </summary>
    public static void TestNativeAndColonyRollsDiffer()
    {
        long seedVal = 99999;
        double nativeRoll = PopulationLikelihood.DeriveRollValue(seedVal, PopulationLikelihood.NativeRollSalt);
        double colonyRoll = PopulationLikelihood.DeriveRollValue(seedVal, PopulationLikelihood.ColonyRollSalt);
        DotNetNativeTestSuite.AssertNotEqual(nativeRoll, colonyRoll, "Different salts must yield different rolls");
    }

    /// <summary>
    /// Tests should_generate_natives is deterministic for same profile + seed.
    /// </summary>
    public static void TestShouldGenerateNativesDeterministic()
    {
        PlanetProfile profile = new();
        profile.BodyId = "body1";
        profile.HabitabilityScore = 8;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.IsTidallyLocked = false;
        profile.RadiationLevel = 0.2;
        profile.IsMoon = false;
        profile.TidalHeatingFactor = 0.0;

        long popSeed = 123456;
        bool result1 = PopulationLikelihood.ShouldGenerateNatives(profile, popSeed);
        bool result2 = PopulationLikelihood.ShouldGenerateNatives(profile, popSeed);
        DotNetNativeTestSuite.AssertEqual(result1, result2, "Same profile+seed must yield same natives decision");
    }

    /// <summary>
    /// Tests should_generate_natives returns false when habitability too low.
    /// </summary>
    public static void TestShouldGenerateNativesZeroWhenUninhabitable()
    {
        PlanetProfile profile = new();
        profile.BodyId = "hostile";
        profile.HabitabilityScore = 1;
        profile.HasLiquidWater = false;
        profile.HasBreathableAtmosphere = false;
        profile.IsTidallyLocked = true;
        profile.RadiationLevel = 1.0;

        bool result = PopulationLikelihood.ShouldGenerateNatives(profile, 99999);
        DotNetNativeTestSuite.AssertFalse(result, "Very low habitability should never produce natives");
    }

    /// <summary>
    /// Tests that life permissiveness changes deterministic native-generation outcomes for the same seed.
    /// </summary>
    public static void TestShouldGenerateNativesRespectsLifePermissiveness()
    {
        PlanetProfile profile = new();
        profile.BodyId = "marginal";
        profile.HabitabilityScore = 4;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = false;
        profile.IsTidallyLocked = false;
        profile.RadiationLevel = 0.35;
        profile.AvgTemperatureK = 450.0;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();
        strictSettings.LifePermissiveness = 0.0;

        GenerationUseCaseSettings permissiveSettings = GenerationUseCaseSettings.CreateDefault();
        permissiveSettings.LifePermissiveness = 1.0;

        long matchingSeed = -1;
        for (long populationSeed = 1; populationSeed <= 10000; populationSeed += 1)
        {
            bool strictResult = PopulationLikelihood.ShouldGenerateNatives(profile, populationSeed, strictSettings);
            bool permissiveResult = PopulationLikelihood.ShouldGenerateNatives(profile, populationSeed, permissiveSettings);
            if (!strictResult && permissiveResult)
            {
                matchingSeed = populationSeed;
                break;
            }
        }

        DotNetNativeTestSuite.AssertTrue(matchingSeed > 0, "A deterministic seed should exist where permissive life settings admit the same world that strict settings reject");
    }

    /// <summary>
    /// Tests that prime wet worlds always get a biosphere even under strict settings.
    /// </summary>
    public static void TestShouldGenerateNativesGuaranteesPrimeWorldsAtStrict()
    {
        PlanetProfile profile = new();
        profile.BodyId = "prime";
        profile.HabitabilityScore = 8;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = true;
        profile.RadiationLevel = 0.2;
        profile.IsTidallyLocked = false;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();
        strictSettings.LifePermissiveness = 0.0;

        bool result = PopulationLikelihood.ShouldGenerateNatives(profile, 1, strictSettings);
        DotNetNativeTestSuite.AssertTrue(result, "Strict life settings should still guarantee a biosphere on prime wet worlds");
    }

    /// <summary>
    /// Tests that the same deterministic colony roll can fail without native pressure and pass with it.
    /// </summary>
    public static void TestShouldGenerateColonyRespectsNativePressure()
    {
        PlanetProfile profile = new();
        profile.BodyId = "harsh_moon";
        profile.HabitabilityScore = 1;
        profile.IsMoon = true;

        ColonySuitability harshSuitability = new();
        harshSuitability.OverallScore = 20;
        harshSuitability.RequiresLifeSupport = true;
        harshSuitability.RequiresPressureSuit = true;
        harshSuitability.RequiresRadiationShielding = false;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();

        ColonyPressureContext pressureContext = new ColonyPressureContext
        {
            LocalNativePressure = 1.0,
            NearbySystemNativePressure = 0.8,
            LocalNativeWorldCount = 1,
            NearbyNativeWorldCount = 2,
        };

        long matchingSeed = -1;
        for (long populationSeed = 1; populationSeed <= 10000; populationSeed += 1)
        {
            bool strictResult = PopulationLikelihood.ShouldGenerateColony(
                profile,
                harshSuitability,
                populationSeed,
                strictSettings);
            bool pressuredResult = PopulationLikelihood.ShouldGenerateColony(
                profile,
                harshSuitability,
                populationSeed,
                strictSettings,
                pressureContext);
            if (!strictResult && pressuredResult)
            {
                matchingSeed = populationSeed;
                break;
            }
        }

        DotNetNativeTestSuite.AssertTrue(
            matchingSeed > 0,
            "A deterministic seed should exist where native pressure admits the same harsh target that the baseline colony check rejects");
    }

    /// <summary>
    /// Tests Override enum values.
    /// </summary>
    public static void TestOverrideEnumValues()
    {
        DotNetNativeTestSuite.AssertEqual(0, (int)PopulationLikelihood.Override.Auto, "AUTO should be 0");
        DotNetNativeTestSuite.AssertEqual(1, (int)PopulationLikelihood.Override.None, "NONE should be 1");
        DotNetNativeTestSuite.AssertEqual(2, (int)PopulationLikelihood.Override.ForceNatives, "FORCE_NATIVES should be 2");
        DotNetNativeTestSuite.AssertEqual(3, (int)PopulationLikelihood.Override.ForceColony, "FORCE_COLONY should be 3");
    }
}
