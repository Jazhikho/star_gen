#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Population;
using StarGen.Domain.Generation;
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

    /// <summary>
    /// Tests that rapid abiogenesis raises biosphere odds without directly boosting civilization-stage odds.
    /// </summary>
    public static void TestRapidStartRaisesAbiogenesisWithoutDirectCivilizationBoost()
    {
        PlanetEnvironmentProfile world = CreatePrimeSentientWorld();
        GenerationUseCaseSettings rapidSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.RapidStart,
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);
        GenerationUseCaseSettings conservativeSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative,
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);

        BiologySupportEvaluator.Assessment rapidAssessment = BiologySupportEvaluator.Evaluate(world, rapidSettings);
        BiologySupportEvaluator.Assessment conservativeAssessment = BiologySupportEvaluator.Evaluate(world, conservativeSettings);

        DotNetNativeTestSuite.AssertTrue(rapidAssessment.AbiogenesisChance > conservativeAssessment.AbiogenesisChance, "Rapid Start should raise abiogenesis chance");
        DotNetNativeTestSuite.AssertEqual(rapidAssessment.SentienceChance, conservativeAssessment.SentienceChance, "Changing only abiogenesis should not directly alter sentience chance");
        DotNetNativeTestSuite.AssertEqual(rapidAssessment.CivilizationChance, conservativeAssessment.CivilizationChance, "Changing only abiogenesis should not directly alter civilization chance");
    }

    /// <summary>
    /// Tests that the technosphere bottleneck suppresses civilization later than sentience.
    /// </summary>
    public static void TestTechnosphereBottleneckSuppressesCivilizationLaterThanSentience()
    {
        PlanetEnvironmentProfile world = CreateTemperateWaterWorld();
        GenerationUseCaseSettings compositeSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative,
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);
        GenerationUseCaseSettings bottleneckSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative,
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);

        BiologySupportEvaluator.Assessment compositeAssessment = BiologySupportEvaluator.Evaluate(world, compositeSettings);
        BiologySupportEvaluator.Assessment bottleneckAssessment = BiologySupportEvaluator.Evaluate(world, bottleneckSettings);

        DotNetNativeTestSuite.AssertEqual(compositeAssessment.SentienceChance, bottleneckAssessment.SentienceChance, "Civilization-stage bottlenecks should not directly change sentience chance");
        DotNetNativeTestSuite.AssertTrue(compositeAssessment.CivilizationChance > bottleneckAssessment.CivilizationChance, "Technosphere bottlenecks should suppress civilization chance");
    }

    /// <summary>
    /// Tests that different life stages can now resolve separately for the same class of world.
    /// </summary>
    public static void TestPopulationSummaryDistinguishesSentienceFromCivilization()
    {
        PlanetEnvironmentProfile world = CreatePrimeSentientWorld();
        world.BodyId = "stage_split_world";
        world.BodyName = "Stage Split World";

        bool foundSentientWithoutCivilization = false;
        bool foundTechnologicalCivilization = false;
        GenerationUseCaseSettings stageSplitSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.RapidStart,
            GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows,
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.High);

        BiologySupportEvaluator.Assessment assessment = BiologySupportEvaluator.Evaluate(world, stageSplitSettings);
        DotNetNativeTestSuite.AssertTrue(assessment.SentienceChance > 0.0, "Prime sentient world should retain a non-zero sentience chance");
        DotNetNativeTestSuite.AssertTrue(assessment.CivilizationChance > 0.0, "Prime sentient world should retain a non-zero civilization chance");

        for (int index = 0; index < 131072; index += 1)
        {
            int seed = 60000 + index;
            double sentienceRoll = PopulationLikelihood.DeriveRollValue(seed, 0x53454E54);
            double civilizationRoll = PopulationLikelihood.DeriveRollValue(seed, 0x43495649);
            bool hasSentientLife = sentienceRoll < assessment.SentienceChance;
            bool hasTechnologicalCivilization = hasSentientLife && civilizationRoll < assessment.CivilizationChance;

            if (hasSentientLife && !hasTechnologicalCivilization)
            {
                foundSentientWithoutCivilization = true;
            }

            if (hasTechnologicalCivilization)
            {
                foundTechnologicalCivilization = true;
            }

            if (foundSentientWithoutCivilization && foundTechnologicalCivilization)
            {
                break;
            }
        }

        DotNetNativeTestSuite.AssertTrue(foundSentientWithoutCivilization, "A sentient but non-technological outcome should exist for some deterministic seed");
        DotNetNativeTestSuite.AssertTrue(foundTechnologicalCivilization, "A technological-civilization outcome should still exist for some deterministic seed");
    }

    /// <summary>
    /// Tests that mixed land-ocean worlds expose better nutrient accessibility than ocean-dominated worlds.
    /// </summary>
    public static void TestMixedLandOceanWorldImprovesNutrientAccessibility()
    {
        PlanetEnvironmentProfile mixedWorld = CreatePrimeSentientWorld();
        PlanetEnvironmentProfile oceanWorld = CreatePrimeSentientWorld();
        oceanWorld.OceanCoverage = 0.97;
        oceanWorld.LandCoverage = 0.02;
        oceanWorld.ContinentCount = 1;

        BiologySupportEvaluator.Assessment mixedAssessment = BiologySupportEvaluator.Evaluate(mixedWorld);
        BiologySupportEvaluator.Assessment oceanAssessment = BiologySupportEvaluator.Evaluate(oceanWorld);

        DotNetNativeTestSuite.AssertTrue(
            mixedAssessment.NutrientAccessibility > oceanAssessment.NutrientAccessibility,
            $"Mixed land-ocean worlds should improve nutrient accessibility | mixed={mixedAssessment.NutrientAccessibility:0.000} ocean={oceanAssessment.NutrientAccessibility:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            mixedAssessment.OxygenationChance > oceanAssessment.OxygenationChance,
            $"Mixed land-ocean worlds should improve oxygenation potential | mixed={mixedAssessment.OxygenationChance:0.000} ocean={oceanAssessment.OxygenationChance:0.000}");
    }

    /// <summary>
    /// Tests that icy ocean moons favor protected biospheres over exposed surface ones.
    /// </summary>
    public static void TestIcyMoonFavorsProtectedBiosphere()
    {
        PlanetEnvironmentProfile icyMoon = CreateIcyOceanMoon();
        icyMoon.TidalHeatingFactor = 0.24;

        BiologySupportEvaluator.Assessment assessment = BiologySupportEvaluator.Evaluate(icyMoon);

        DotNetNativeTestSuite.AssertTrue(
            assessment.ProtectedBiosphereChance > assessment.SurfaceBiosphereChance,
            $"Icy ocean moons should favor protected biospheres | protected={assessment.ProtectedBiosphereChance:0.000} surface={assessment.SurfaceBiosphereChance:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            assessment.BiosignatureDetectabilityChance < assessment.ProtectedBiosphereChance,
            $"Protected biospheres should be harder to detect remotely | detectability={assessment.BiosignatureDetectabilityChance:0.000} protected={assessment.ProtectedBiosphereChance:0.000}");
    }

    /// <summary>
    /// Tests that the dark-biosphere model makes subsurface chemical energy a material setting.
    /// </summary>
    public static void TestDarkBiosphereEnergyScaleChangesSubsurfaceSupport()
    {
        PlanetEnvironmentProfile icyMoon = CreateIcyOceanMoon();
        icyMoon.TidalHeatingFactor = 0.32;
        icyMoon.GravityG = 0.20;
        icyMoon.HabitableZoneAlignment = 0.18;
        GenerationUseCaseSettings lowEnergy = GenerationUseCaseSettings.CreateDefault();
        lowEnergy.SubsurfaceHabitabilityModel = GenerationUseCaseSettings.SubsurfaceHabitabilityModelType.DarkBiosphereEnergyLimited;
        lowEnergy.DarkBiosphereEnergyScale = 0.35;
        GenerationUseCaseSettings highEnergy = GenerationUseCaseSettings.CreateDefault();
        highEnergy.SubsurfaceHabitabilityModel = GenerationUseCaseSettings.SubsurfaceHabitabilityModelType.DarkBiosphereEnergyLimited;
        highEnergy.DarkBiosphereEnergyScale = 1.75;

        BiologySupportEvaluator.Assessment lowAssessment = BiologySupportEvaluator.Evaluate(icyMoon, lowEnergy);
        BiologySupportEvaluator.Assessment highAssessment = BiologySupportEvaluator.Evaluate(icyMoon, highEnergy);

        DotNetNativeTestSuite.AssertTrue(
            highAssessment.BiosphereSuitability > lowAssessment.BiosphereSuitability,
            $"Dark-biosphere energy scale should change subsurface support | low={lowAssessment.BiosphereSuitability:0.000} high={highAssessment.BiosphereSuitability:0.000}");
    }

    /// <summary>
    /// Tests that strong XUV and weak protection increase desiccation risk and suppress abiogenesis.
    /// </summary>
    public static void TestHighXuvRaisesDesiccationRiskAndSuppressesAbiogenesis()
    {
        PlanetEnvironmentProfile mildWorld = CreateTemperateWaterWorld();
        PlanetEnvironmentProfile harshWorld = CreateTemperateWaterWorld();
        harshWorld.XuvExposure = 1.40;
        harshWorld.PressureAtm = 0.15;
        harshWorld.MagneticFieldStrength = 0.02;
        harshWorld.HasMagneticField = false;

        BiologySupportEvaluator.Assessment mildAssessment = BiologySupportEvaluator.Evaluate(mildWorld);
        BiologySupportEvaluator.Assessment harshAssessment = BiologySupportEvaluator.Evaluate(harshWorld);

        DotNetNativeTestSuite.AssertTrue(
            harshAssessment.EarlyDesiccationRisk > mildAssessment.EarlyDesiccationRisk,
            $"Harsh XUV worlds should have higher desiccation risk | mild={mildAssessment.EarlyDesiccationRisk:0.000} harsh={harshAssessment.EarlyDesiccationRisk:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            mildAssessment.AbiogenesisChance > harshAssessment.AbiogenesisChance,
            $"Harsh XUV worlds should have lower abiogenesis chance | mild={mildAssessment.AbiogenesisChance:0.000} harsh={harshAssessment.AbiogenesisChance:0.000}");
    }

    /// <summary>
    /// Tests that abiogenesis, oxygenation, and detectability remain separate diagnostics.
    /// </summary>
    public static void TestDetectabilityRemainsSeparateFromLifeExistence()
    {
        PlanetEnvironmentProfile world = CreatePrimeSentientWorld();
        world.HasBreathableAtmosphere = false;
        world.PressureAtm = 0.55;
        world.OceanCoverage = 0.88;
        world.LandCoverage = 0.08;
        world.ContinentCount = 1;
        world.XuvExposure = 0.38;

        BiologySupportEvaluator.Assessment assessment = BiologySupportEvaluator.Evaluate(world);

        DotNetNativeTestSuite.AssertTrue(
            assessment.AbiogenesisChance > assessment.BiosignatureDetectabilityChance,
            $"Life existence should exceed biosignature detectability on weakly oxygenated worlds | abiogenesis={assessment.AbiogenesisChance:0.000} detectability={assessment.BiosignatureDetectabilityChance:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            assessment.OxygenationChance < assessment.SurfaceBiosphereChance,
            $"Oxygenation should remain a later bottleneck than surface biosphere support | oxygenation={assessment.OxygenationChance:0.000} surface={assessment.SurfaceBiosphereChance:0.000}");
    }

    /// <summary>
    /// Tests that breathable atmospheres with strong abiotic oxygen risk do not fully support civilization odds.
    /// </summary>
    public static void TestAbioticOxygenRiskDiscountsCivilizationSupport()
    {
        PlanetEnvironmentProfile biologicalOxygenWorld = CreatePrimeSentientWorld();
        PlanetEnvironmentProfile abioticRiskWorld = CreatePrimeSentientWorld();
        abioticRiskWorld.XuvExposure = 1.35;
        abioticRiskWorld.PressureAtm = 0.55;
        abioticRiskWorld.OceanCoverage = 0.05;
        abioticRiskWorld.LandCoverage = 0.92;
        abioticRiskWorld.IceCoverage = 0.0;
        abioticRiskWorld.MagneticFieldStrength = 0.02;
        abioticRiskWorld.HasMagneticField = false;

        GenerationUseCaseSettings bottleneckSettings = CreateLifeSettings(
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative,
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite,
            GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck,
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);

        BiologySupportEvaluator.Assessment biologicalAssessment = BiologySupportEvaluator.Evaluate(biologicalOxygenWorld, bottleneckSettings);
        BiologySupportEvaluator.Assessment abioticRiskAssessment = BiologySupportEvaluator.Evaluate(abioticRiskWorld, bottleneckSettings);

        DotNetNativeTestSuite.AssertTrue(
            abioticRiskAssessment.AbioticOxygenFalsePositiveRisk > biologicalAssessment.AbioticOxygenFalsePositiveRisk,
            $"Abiotic-risk world should expose higher oxygen false-positive risk | biological={biologicalAssessment.AbioticOxygenFalsePositiveRisk:0.000} abiotic={abioticRiskAssessment.AbioticOxygenFalsePositiveRisk:0.000}");
        DotNetNativeTestSuite.AssertTrue(
            biologicalAssessment.CivilizationChance > abioticRiskAssessment.CivilizationChance,
            $"Abiotic oxygen risk should discount civilization support even when the atmosphere is flagged breathable | biological={biologicalAssessment.CivilizationChance:0.000} abiotic={abioticRiskAssessment.CivilizationChance:0.000}");
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
            ContinentCount = 4,
            DayLengthHours = 24.0,
            AxialTiltDeg = 23.5,
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
            IsTidallyLocked = false,
            ResourceRichness = 0.62,
            ResourceDiversity = 0.58,
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
            ContinentCount = 0,
            DayLengthHours = 84.0,
            AxialTiltDeg = 2.0,
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
            IsTidallyLocked = true,
            ResourceRichness = 0.34,
            ResourceDiversity = 0.25,
        };
    }

    private static PlanetEnvironmentProfile CreatePrimeSentientWorld()
    {
        return new PlanetEnvironmentProfile
        {
            BodyId = "prime_sentient_world",
            BodyName = "Prime Sentient World",
            BodyType = "Planet",
            HabitabilityScore = 10,
            AvgTemperatureK = 288.0,
            StellarAgeYears = 6.4e9,
            PressureAtm = 1.05,
            OceanCoverage = 0.42,
            LandCoverage = 0.54,
            IceCoverage = 0.04,
            ContinentCount = 5,
            DayLengthHours = 23.0,
            AxialTiltDeg = 19.0,
            GravityG = 1.0,
            TectonicActivity = 0.30,
            VolcanismLevel = 0.08,
            WeatherSeverity = 0.12,
            MagneticFieldStrength = 0.90,
            RadiationLevel = 0.04,
            StellarFluxEarth = 1.0,
            HabitableZoneInnerAu = 0.94,
            HabitableZoneOuterAu = 1.38,
            HabitableZoneAlignment = 1.0,
            XuvExposure = 0.06,
            HasAtmosphere = true,
            HasLiquidWater = true,
            HasBreathableAtmosphere = true,
            HasMagneticField = true,
            IsMoon = false,
            IsTidallyLocked = false,
            ResourceRichness = 0.68,
            ResourceDiversity = 0.64,
        };
    }

    private static GenerationUseCaseSettings CreateLifeSettings(
        GenerationUseCaseSettings.AbiogenesisModelType abiogenesisModel,
        GenerationUseCaseSettings.ComplexLifeModelType complexLifeModel,
        GenerationUseCaseSettings.CivilizationModelType civilizationModel,
        GenerationUseCaseSettings.EnvironmentalWindowWeightType environmentalWindowWeight)
    {
        return new GenerationUseCaseSettings
        {
            LifeFramework = GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
            AbiogenesisModel = abiogenesisModel,
            ComplexLifeModel = complexLifeModel,
            CivilizationModel = civilizationModel,
            EnvironmentalWindowWeight = environmentalWindowWeight,
            LifePermissiveness = GenerationUseCaseSettings.GetRecommendedLifePermissiveness(GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite),
        };
    }
}
