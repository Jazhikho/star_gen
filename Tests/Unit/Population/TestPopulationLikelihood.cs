#nullable enable annotations
#nullable disable warnings
using System;
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
