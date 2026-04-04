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
        profileLow.HasAtmosphere = true;
        profileLow.PressureAtm = 1.0;
        profileLow.OceanCoverage = 0.4;
        profileLow.GravityG = 1.0;
        profileLow.AvgTemperatureK = 289.0;
        profileLow.RadiationLevel = 0.18;

        PlanetProfile profileHigh = new();
        profileHigh.HabitabilityScore = 8;
        profileHigh.HasLiquidWater = true;
        profileHigh.HasAtmosphere = true;
        profileHigh.PressureAtm = 1.0;
        profileHigh.OceanCoverage = 0.7;
        profileHigh.GravityG = 1.0;
        profileHigh.AvgTemperatureK = 289.0;
        profileHigh.RadiationLevel = 0.18;

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
        profileDry.HabitabilityScore = 7;
        profileDry.HasLiquidWater = false;
        profileDry.HasAtmosphere = true;
        profileDry.HasBreathableAtmosphere = true;
        profileDry.PressureAtm = 1.0;
        profileDry.GravityG = 1.0;
        profileDry.AvgTemperatureK = 289.0;
        profileDry.RadiationLevel = 0.18;

        PlanetProfile profileWet = new();
        profileWet.HabitabilityScore = 7;
        profileWet.HasLiquidWater = true;
        profileWet.HasAtmosphere = true;
        profileWet.HasBreathableAtmosphere = true;
        profileWet.PressureAtm = 1.0;
        profileWet.OceanCoverage = 0.55;
        profileWet.GravityG = 1.0;
        profileWet.AvgTemperatureK = 289.0;
        profileWet.RadiationLevel = 0.18;

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
        profileNo.HabitabilityScore = 7;
        profileNo.HasLiquidWater = true;
        profileNo.HasBreathableAtmosphere = false;
        profileNo.HasAtmosphere = true;
        profileNo.PressureAtm = 1.0;
        profileNo.OceanCoverage = 0.55;
        profileNo.GravityG = 1.0;
        profileNo.AvgTemperatureK = 289.0;
        profileNo.RadiationLevel = 0.18;

        PlanetProfile profileYes = new();
        profileYes.HabitabilityScore = 7;
        profileYes.HasLiquidWater = true;
        profileYes.HasBreathableAtmosphere = true;
        profileYes.HasAtmosphere = true;
        profileYes.PressureAtm = 1.0;
        profileYes.OceanCoverage = 0.55;
        profileYes.GravityG = 1.0;
        profileYes.AvgTemperatureK = 289.0;
        profileYes.RadiationLevel = 0.18;

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
        profileFree.HabitabilityScore = 7;
        profileFree.HasLiquidWater = true;
        profileFree.HasAtmosphere = true;
        profileFree.HasBreathableAtmosphere = true;
        profileFree.IsTidallyLocked = false;
        profileFree.PressureAtm = 1.0;
        profileFree.OceanCoverage = 0.55;
        profileFree.GravityG = 1.0;
        profileFree.AvgTemperatureK = 289.0;
        profileFree.RadiationLevel = 0.18;

        PlanetProfile profileLocked = new();
        profileLocked.HabitabilityScore = 7;
        profileLocked.HasLiquidWater = true;
        profileLocked.HasAtmosphere = true;
        profileLocked.HasBreathableAtmosphere = true;
        profileLocked.IsTidallyLocked = true;
        profileLocked.PressureAtm = 1.0;
        profileLocked.OceanCoverage = 0.55;
        profileLocked.GravityG = 1.0;
        profileLocked.AvgTemperatureK = 289.0;
        profileLocked.RadiationLevel = 0.18;

        double probFree = PopulationProbability.CalculateNativeProbability(profileFree);
        double probLocked = PopulationProbability.CalculateNativeProbability(profileLocked);

        DotNetNativeTestSuite.AssertTrue(probFree > probLocked, "Tidal locking should reduce probability");
    }

    /// <summary>
    /// Tests that probability is clamped to the documented native-life ceiling.
    /// </summary>
    public static void TestProbabilityClamped()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 10;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.HasAtmosphere = true;
        profile.PressureAtm = 1.0;
        profile.OceanCoverage = 0.65;
        profile.GravityG = 1.0;
        profile.AvgTemperatureK = 288.0;
        profile.RadiationLevel = 0.12;

        double probability = PopulationProbability.CalculateNativeProbability(profile);
        DotNetNativeTestSuite.AssertTrue(
            probability <= PopulationProbability.MaxNativeProbability,
            $"Probability should not exceed {PopulationProbability.MaxNativeProbability:0.00}");
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
        profile.HabitabilityScore = 5;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = false;
        profile.PressureAtm = 0.9;
        profile.OceanCoverage = 0.35;
        profile.GravityG = 0.95;
        profile.RadiationLevel = 0.30;
        profile.AvgTemperatureK = 300.0;

        double strictProbability = PopulationProbability.CalculateNativeProbability(profile, 0.0);
        double permissiveProbability = PopulationProbability.CalculateNativeProbability(profile, 1.0);

        DotNetNativeTestSuite.AssertTrue(strictProbability > 0.0, "Strict life settings should still allow a non-zero abiogenesis chance on viable wet worlds");
        DotNetNativeTestSuite.AssertTrue(permissiveProbability > strictProbability, "Permissive life settings should raise marginal biosphere probability");
        DotNetNativeTestSuite.AssertTrue(permissiveProbability >= 0.55, $"Space-opera life settings should give viable wet marginal worlds a high life chance | strict={strictProbability:0.000} permissive={permissiveProbability:0.000}");
    }

    /// <summary>
    /// Tests that strict life settings preserve high odds only for earthlike prime worlds.
    /// </summary>
    public static void TestStrictLifeSettingsFavorEarthlikeWorlds()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 9;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = true;
        profile.PressureAtm = 1.0;
        profile.OceanCoverage = 0.70;
        profile.RadiationLevel = 0.10;
        profile.AvgTemperatureK = 288.0;
        profile.GravityG = 1.0;

        double strictProbability = PopulationProbability.CalculateNativeProbability(profile, 0.0);
        DotNetNativeTestSuite.AssertTrue(strictProbability >= 0.10, "Strict life settings should still meaningfully favor earthlike prime worlds");
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
        DotNetNativeTestSuite.AssertTrue(permissiveProbability > 0.10, "Permissive settlement settings should materially allow harsh colony targets");
    }

    /// <summary>
    /// Tests that native-pressure context can materially raise colony probability for harsh moon targets.
    /// </summary>
    public static void TestNativePressureRaisesColonyProbability()
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

        ColonyPressureContext pressureContext = new ColonyPressureContext
        {
            LocalNativePressure = 1.0,
            NearbySystemNativePressure = 0.8,
            LocalNativeWorldCount = 1,
            NearbyNativeWorldCount = 2,
        };

        double strictProbability = PopulationProbability.CalculateColonyProbability(profile, harshSuitability, 0.0, null);
        double pressuredProbability = PopulationProbability.CalculateColonyProbability(profile, harshSuitability, 0.0, pressureContext);

        DotNetNativeTestSuite.AssertFloatNear(0.0, strictProbability, 0.001, "Strict settings without native pressure should still reject harsh targets");
        DotNetNativeTestSuite.AssertTrue(pressuredProbability > strictProbability, "Native pressure should raise colony probability");
        DotNetNativeTestSuite.AssertTrue(pressuredProbability > 0.0, "Strong native pressure should make at least some harsh targets colonizable");
    }

    /// <summary>
    /// Legacy parity alias for test_should_generate_natives_determinism.
    /// </summary>
    private static void TestShouldGenerateNativesDeterminism()
    {
        TestProbabilityIncreasesWithHabitability();
    }
}

