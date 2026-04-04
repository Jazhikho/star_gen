#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for biology-support evaluation under richer orbit, XUV, and tidal constraints.
/// </summary>
public static class TestBiologySupportEvaluator
{
    /// <summary>
    /// Tests that stronger XUV exposure materially penalizes biosphere chances.
    /// </summary>
    public static void TestHighXuvPenalizesBiosphereSupport()
    {
        PlanetEnvironmentProfile temperateWorld = CreateTemperateWaterWorld();
        PlanetEnvironmentProfile harshXuvWorld = CreateTemperateWaterWorld();
        harshXuvWorld.XuvExposure = 1.45;

        BiologySupportEvaluator.Assessment mildAssessment = BiologySupportEvaluator.Evaluate(temperateWorld);
        BiologySupportEvaluator.Assessment harshAssessment = BiologySupportEvaluator.Evaluate(harshXuvWorld);

        DotNetNativeTestSuite.AssertTrue(
            mildAssessment.AbiogenesisChance > harshAssessment.AbiogenesisChance,
            $"Higher XUV exposure should reduce abiogenesis chance | mild={mildAssessment.AbiogenesisChance:0.000} harsh={harshAssessment.AbiogenesisChance:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            mildAssessment.ComplexLifeChance > harshAssessment.ComplexLifeChance,
            $"Higher XUV exposure should reduce complex-life chance | mild={mildAssessment.ComplexLifeChance:0.000} harsh={harshAssessment.ComplexLifeChance:0.000}");
    }

    /// <summary>
    /// Tests that moderate tidal heating helps an icy moon more than almost no heating.
    /// </summary>
    public static void TestModerateTidalHeatingSupportsIcyMoonBiology()
    {
        PlanetEnvironmentProfile quietMoon = CreateIcyOceanMoon();
        quietMoon.TidalHeatingFactor = 0.01;
        PlanetEnvironmentProfile warmMoon = CreateIcyOceanMoon();
        warmMoon.TidalHeatingFactor = 0.22;

        BiologySupportEvaluator.Assessment quietAssessment = BiologySupportEvaluator.Evaluate(quietMoon);
        BiologySupportEvaluator.Assessment warmAssessment = BiologySupportEvaluator.Evaluate(warmMoon);

        DotNetNativeTestSuite.AssertTrue(
            warmAssessment.BiosphereSuitability > quietAssessment.BiosphereSuitability,
            $"Moderate tidal heating should improve icy-moon biosphere suitability | quiet={quietAssessment.BiosphereSuitability:0.000} warm={warmAssessment.BiosphereSuitability:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            warmAssessment.AbiogenesisChance > quietAssessment.AbiogenesisChance,
            $"Moderate tidal heating should raise abiogenesis odds for an icy ocean moon | quiet={quietAssessment.AbiogenesisChance:0.000} warm={warmAssessment.AbiogenesisChance:0.000}");
    }

    /// <summary>
    /// Tests that too much tidal heating hurts complex life on an icy moon.
    /// </summary>
    public static void TestExtremeTidalHeatingHurtsMoonComplexLife()
    {
        PlanetEnvironmentProfile moderateMoon = CreateIcyOceanMoon();
        moderateMoon.TidalHeatingFactor = 0.22;
        PlanetEnvironmentProfile overheatedMoon = CreateIcyOceanMoon();
        overheatedMoon.TidalHeatingFactor = 0.92;
        overheatedMoon.VolcanismLevel = 0.75;

        BiologySupportEvaluator.Assessment moderateAssessment = BiologySupportEvaluator.Evaluate(moderateMoon);
        BiologySupportEvaluator.Assessment overheatedAssessment = BiologySupportEvaluator.Evaluate(overheatedMoon);

        DotNetNativeTestSuite.AssertTrue(
            moderateAssessment.ComplexLifeChance > overheatedAssessment.ComplexLifeChance,
            $"Excess tidal heating should reduce complex-life odds | moderate={moderateAssessment.ComplexLifeChance:0.000} overheated={overheatedAssessment.ComplexLifeChance:0.000}");
    }

    private static PlanetEnvironmentProfile CreateTemperateWaterWorld()
    {
        return new PlanetEnvironmentProfile
        {
            BodyId = "temperate_world",
            BodyName = "Temperate World",
            BodyType = "Planet",
            HabitabilityScore = 8,
            AvgTemperatureK = 289.0,
            StellarAgeYears = 4.8e9,
            PressureAtm = 1.0,
            OceanCoverage = 0.65,
            LandCoverage = 0.30,
            IceCoverage = 0.05,
            GravityG = 1.0,
            TectonicActivity = 0.45,
            VolcanismLevel = 0.20,
            WeatherSeverity = 0.25,
            MagneticFieldStrength = 0.85,
            RadiationLevel = 0.12,
            StellarFluxEarth = 1.0,
            HabitableZoneInnerAu = 0.95,
            HabitableZoneOuterAu = 1.37,
            HabitableZoneAlignment = 1.0,
            XuvExposure = 0.22,
            HasAtmosphere = true,
            HasLiquidWater = true,
            HasBreathableAtmosphere = true,
            HasMagneticField = true,
            IsMoon = false,
        };
    }

    private static PlanetEnvironmentProfile CreateIcyOceanMoon()
    {
        return new PlanetEnvironmentProfile
        {
            BodyId = "icy_moon",
            BodyName = "Icy Moon",
            BodyType = "Moon",
            HabitabilityScore = 2,
            AvgTemperatureK = 185.0,
            StellarAgeYears = 5.0e9,
            PressureAtm = 0.0,
            OceanCoverage = 0.0,
            LandCoverage = 0.0,
            IceCoverage = 0.92,
            GravityG = 0.14,
            TectonicActivity = 0.10,
            VolcanismLevel = 0.12,
            WeatherSeverity = 0.0,
            MagneticFieldStrength = 0.0,
            RadiationLevel = 0.18,
            StellarFluxEarth = 0.20,
            HabitableZoneInnerAu = 0.95,
            HabitableZoneOuterAu = 1.37,
            HabitableZoneAlignment = 0.0,
            XuvExposure = 0.08,
            TidalHeatingFactor = 0.20,
            ParentRadiationExposure = 0.18,
            HasAtmosphere = false,
            HasLiquidWater = true,
            HasBreathableAtmosphere = false,
            HasMagneticField = false,
            IsMoon = true,
        };
    }
}
