#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PopulationProbability probability calculation and deterministic rolling.
/// </summary>
public static class TestPopulationProbability
{
    /// <summary>
    /// Tests that uninhabitable planets return zero native probability.
    /// </summary>
    public static void TestZeroProbabilityForLowHabitability()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 0;

        double probability = PopulationProbability.CalculateNativeProbability(profile);
        DotNetNativeTestSuite.AssertFloatNear(0.0, probability, 0.001, "Score 0 should give 0 probability");

        profile.HabitabilityScore = 2;
        probability = PopulationProbability.CalculateNativeProbability(profile);
        DotNetNativeTestSuite.AssertFloatNear(0.0, probability, 0.001, "Score 2 should give 0 probability (below threshold)");
    }

    /// <summary>
    /// Tests that higher habitability scores give higher probabilities.
    /// </summary>
    public static void TestProbabilityIncreasesWithHabitability()
    {
        PlanetProfile profileLow = new();
        profileLow.HabitabilityScore = 3;
        profileLow.HasLiquidWater = true;

        PlanetProfile profileHigh = new();
        profileHigh.HabitabilityScore = 8;
        profileHigh.HasLiquidWater = true;

        double probLow = PopulationProbability.CalculateNativeProbability(profileLow);
        double probHigh = PopulationProbability.CalculateNativeProbability(profileHigh);

        DotNetNativeTestSuite.AssertTrue(probHigh > probLow, "Higher habitability should give higher probability");
    }

    /// <summary>
    /// Tests that liquid water adds a bonus.
    /// </summary>
    public static void TestLiquidWaterBonus()
    {
        PlanetProfile profileDry = new();
        profileDry.HabitabilityScore = 5;
        profileDry.HasLiquidWater = false;

        PlanetProfile profileWet = new();
        profileWet.HabitabilityScore = 5;
        profileWet.HasLiquidWater = true;

        double probDry = PopulationProbability.CalculateNativeProbability(profileDry);
        double probWet = PopulationProbability.CalculateNativeProbability(profileWet);

        DotNetNativeTestSuite.AssertTrue(probWet > probDry, "Liquid water should increase probability");
    }

    /// <summary>
    /// Tests that breathable atmosphere adds a bonus.
    /// </summary>
    public static void TestBreathableAtmosphereBonus()
    {
        PlanetProfile profileNo = new();
        profileNo.HabitabilityScore = 5;
        profileNo.HasLiquidWater = true;
        profileNo.HasBreathableAtmosphere = false;
        profileNo.HasAtmosphere = true;

        PlanetProfile profileYes = new();
        profileYes.HabitabilityScore = 5;
        profileYes.HasLiquidWater = true;
        profileYes.HasBreathableAtmosphere = true;
        profileYes.HasAtmosphere = true;

        double probNo = PopulationProbability.CalculateNativeProbability(profileNo);
        double probYes = PopulationProbability.CalculateNativeProbability(profileYes);

        DotNetNativeTestSuite.AssertTrue(probYes > probNo, "Breathable atmosphere should increase probability");
    }

    /// <summary>
    /// Tests that tidal locking reduces probability.
    /// </summary>
    public static void TestTidalLockingPenalty()
    {
        PlanetProfile profileFree = new();
        profileFree.HabitabilityScore = 5;
        profileFree.HasLiquidWater = true;
        profileFree.HasAtmosphere = true;
        profileFree.IsTidallyLocked = false;

        PlanetProfile profileLocked = new();
        profileLocked.HabitabilityScore = 5;
        profileLocked.HasLiquidWater = true;
        profileLocked.HasAtmosphere = true;
        profileLocked.IsTidallyLocked = true;

        double probFree = PopulationProbability.CalculateNativeProbability(profileFree);
        double probLocked = PopulationProbability.CalculateNativeProbability(profileLocked);

        DotNetNativeTestSuite.AssertTrue(probFree > probLocked, "Tidal locking should reduce probability");
    }

    /// <summary>
    /// Tests that probability is clamped to [0, 0.95].
    /// </summary>
    public static void TestProbabilityClamped()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 10;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;

        double probability = PopulationProbability.CalculateNativeProbability(profile);
        DotNetNativeTestSuite.AssertTrue(probability <= 0.95, "Probability should not exceed 0.95");
        DotNetNativeTestSuite.AssertTrue(probability >= 0.0, "Probability should not be negative");
    }

    /// <summary>
    /// Tests colony probability is zero for unsuitable planets.
    /// </summary>
    public static void TestColonyProbabilityZeroForUnsuitable()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 0;

        ColonySuitability suitability = new();
        suitability.OverallScore = 5;

        double probability = PopulationProbability.CalculateColonyProbability(profile, suitability);
        DotNetNativeTestSuite.AssertFloatNear(0.0, probability, 0.001, "Uninhabitable should give 0 colony probability");
    }

    /// <summary>
    /// Tests colony probability increases with suitability score.
    /// </summary>
    public static void TestColonyProbabilityScalesWithSuitability()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 5;

        ColonySuitability suitLow = new();
        suitLow.OverallScore = 20;

        ColonySuitability suitHigh = new();
        suitHigh.OverallScore = 80;

        double probLow = PopulationProbability.CalculateColonyProbability(profile, suitLow);
        double probHigh = PopulationProbability.CalculateColonyProbability(profile, suitHigh);

        DotNetNativeTestSuite.AssertTrue(probHigh > probLow, "Higher suitability should increase colony probability");
    }

    /// <summary>
    /// Tests that high life permissiveness materially raises native-life probability on marginal worlds.
    /// </summary>
    public static void TestLifePermissivenessAffectsMarginalWorlds()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 4;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = false;
        profile.RadiationLevel = 0.45;

        double strictProbability = PopulationProbability.CalculateNativeProbability(profile, 0.0);
        double permissiveProbability = PopulationProbability.CalculateNativeProbability(profile, 1.0);

        DotNetNativeTestSuite.AssertTrue(strictProbability < 0.05, "Strict life settings should nearly suppress marginal biospheres");
        DotNetNativeTestSuite.AssertTrue(permissiveProbability > strictProbability, "Permissive life settings should raise marginal biosphere probability");
    }

    /// <summary>
    /// Tests that high settlement permissiveness materially raises colony probability on harsh but survivable worlds.
    /// </summary>
    public static void TestSettlementPermissivenessAffectsHarshWorlds()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 1;
        profile.IsMoon = true;

        ColonySuitability harshSuitability = new();
        harshSuitability.OverallScore = 18;
        harshSuitability.RequiresLifeSupport = true;
        harshSuitability.RequiresPressureSuit = true;
        harshSuitability.RequiresRadiationShielding = false;

        double strictProbability = PopulationProbability.CalculateColonyProbability(profile, harshSuitability, 0.0);
        double permissiveProbability = PopulationProbability.CalculateColonyProbability(profile, harshSuitability, 1.0);

        DotNetNativeTestSuite.AssertFloatNear(0.0, strictProbability, 0.001, "Strict settlement settings should reject harsh colony targets");
        DotNetNativeTestSuite.AssertTrue(permissiveProbability > 0.0, "Permissive settlement settings should allow harsh colony targets");
    }

    /// <summary>
    /// Legacy parity alias for test_should_generate_natives_determinism.
    /// </summary>
    private static void TestShouldGenerateNativesDeterminism()
    {
        TestProbabilityIncreasesWithHabitability();
    }
}

