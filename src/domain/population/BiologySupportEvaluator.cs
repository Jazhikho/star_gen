using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Shared biology-support rules used by both native-life likelihood and the ecology pipeline.
/// </summary>
public static class BiologySupportEvaluator
{
    /// <summary>
    /// Preferred biochemical baseline for the evaluated world.
    /// </summary>
    public enum Biochemistry
    {
        None = 0,
        CarbonWater = 1,
        CarbonAmmonia = 2,
        CarbonMethane = 3,
        SulfurChemistry = 4,
        Exotic = 5,
    }

    /// <summary>
    /// Deterministic support-evaluation result used for diagnostics and downstream gating.
    /// </summary>
    public sealed class Assessment
    {
        public bool IsSupported { get; init; }

        public FailureReason Reason { get; init; }

        public Biochemistry PreferredChemistry { get; init; }

        public double BiosphereSuitability { get; init; }

        public double AbiogenesisChance { get; init; }

        public double ComplexLifeChance { get; init; }

        public double SentienceChance { get; init; }

        public double CivilizationChance { get; init; }

        public double BiosphereCoverage { get; init; }

        public bool SupportsComplexLife { get; init; }

        public double NutrientAccessibility { get; init; }

        public double SurfaceBiosphereChance { get; init; }

        public double ProtectedBiosphereChance { get; init; }

        public double OxygenationChance { get; init; }

        public double BiosignatureDetectabilityChance { get; init; }

        public double EarlyDesiccationRisk { get; init; }

        public double PrebioticUvAdequacy { get; init; }

        public double AbioticOxygenFalsePositiveRisk { get; init; }
    }

    /// <summary>
    /// Canonical reasons a world fails the biology-support gate.
    /// </summary>
    public enum FailureReason
    {
        None = 0,
        NoLiquidWater = 1,
        NoViableChemistry = 2,
        LowHabitability = 3,
        HighRadiation = 4,
        TooCold = 5,
        TooHot = 6,
    }

    private readonly struct LifeOpportunityState
    {
        public LifeOpportunityState(
            double environmentalWindow,
            double nutrientAccessibility,
            double prebioticUvAdequacy,
            double earlyDesiccationRisk,
            double protectedBiosphereSupport,
            double surfaceBiosphereSupport,
            double oxygenationPotential,
            double abioticOxygenFalsePositiveRisk)
        {
            EnvironmentalWindow = environmentalWindow;
            NutrientAccessibility = nutrientAccessibility;
            PrebioticUvAdequacy = prebioticUvAdequacy;
            EarlyDesiccationRisk = earlyDesiccationRisk;
            ProtectedBiosphereSupport = protectedBiosphereSupport;
            SurfaceBiosphereSupport = surfaceBiosphereSupport;
            OxygenationPotential = oxygenationPotential;
            AbioticOxygenFalsePositiveRisk = abioticOxygenFalsePositiveRisk;
        }

        public double EnvironmentalWindow { get; }

        public double NutrientAccessibility { get; }

        public double PrebioticUvAdequacy { get; }

        public double EarlyDesiccationRisk { get; }

        public double ProtectedBiosphereSupport { get; }

        public double SurfaceBiosphereSupport { get; }

        public double OxygenationPotential { get; }

        public double AbioticOxygenFalsePositiveRisk { get; }
    }

    /// <summary>
    /// Returns whether a planet profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static bool SupportsBiology(
        PlanetProfile profile,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        Assessment assessment = Evaluate(profile, useCaseSettings);
        return assessment.IsSupported;
    }

    /// <summary>
    /// Evaluates whether a planet profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static Assessment Evaluate(
        PlanetProfile profile,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        PlanetEnvironmentProfile environment = PlanetEnvironmentProfile.FromPlanetProfile(
            profile,
            0,
            profile.BodyId,
            "Planet");
        return Evaluate(environment, useCaseSettings);
    }

    /// <summary>
    /// Returns whether an environment profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static bool SupportsBiology(
        PlanetEnvironmentProfile environment,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        Assessment assessment = Evaluate(environment, useCaseSettings);
        return assessment.IsSupported;
    }

    /// <summary>
    /// Evaluates whether an environment profile can support a native biosphere under the provided assumptions.
    /// </summary>
    public static Assessment Evaluate(
        PlanetEnvironmentProfile environment,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        LifePotentialModeling.Tuning lifeTuning = LifePotentialModeling.Resolve(environment, useCaseSettings);
        double permissiveness = lifeTuning.Permissiveness;
        Biochemistry bestChemistry = SelectBiochemistry(environment, permissiveness, out double chemistryScore);
        if (bestChemistry == Biochemistry.None)
        {
            return CreateFailure(DetermineFailureReason(environment, permissiveness));
        }

        double supportThreshold = Lerp(0.18, 0.04, permissiveness) + lifeTuning.SupportThresholdOffset;
        if (chemistryScore < supportThreshold)
        {
            return CreateFailure(DetermineFailureReason(environment, permissiveness));
        }

        LifeOpportunityState opportunityState = BuildLifeOpportunityState(environment, chemistryScore, permissiveness);
        double abiogenesisChance = CalculateAbiogenesisChance(
            environment,
            chemistryScore,
            permissiveness,
            lifeTuning,
            opportunityState);
        double surfaceBiosphereChance = CalculateSurfaceBiosphereChance(
            environment,
            chemistryScore,
            lifeTuning,
            opportunityState);
        double protectedBiosphereChance = CalculateProtectedBiosphereChance(
            environment,
            chemistryScore,
            permissiveness,
            opportunityState);
        double oxygenationChance = CalculateOxygenationChance(
            environment,
            surfaceBiosphereChance,
            opportunityState);
        double complexLifeChance = CalculateComplexLifeChance(
            environment,
            chemistryScore,
            surfaceBiosphereChance,
            protectedBiosphereChance,
            oxygenationChance,
            lifeTuning,
            opportunityState);
        double sentienceChance = CalculateSentienceChance(
            environment,
            complexLifeChance,
            oxygenationChance,
            lifeTuning,
            opportunityState);
        double civilizationChance = CalculateCivilizationChance(
            environment,
            sentienceChance,
            oxygenationChance,
            lifeTuning,
            opportunityState);
        double biosignatureDetectabilityChance = CalculateBiosignatureDetectabilityChance(
            environment,
            abiogenesisChance,
            surfaceBiosphereChance,
            oxygenationChance,
            opportunityState);
        double biosphereCoverage = CalculateBiosphereCoverage(
            environment,
            bestChemistry,
            chemistryScore,
            permissiveness,
            surfaceBiosphereChance,
            protectedBiosphereChance);

        bool supportsComplexLife = complexLifeChance >= 0.05;

        return new Assessment
        {
            IsSupported = abiogenesisChance > 0.0,
            Reason = FailureReason.None,
            PreferredChemistry = bestChemistry,
            BiosphereSuitability = chemistryScore,
            AbiogenesisChance = abiogenesisChance,
            ComplexLifeChance = complexLifeChance,
            SentienceChance = sentienceChance,
            CivilizationChance = civilizationChance,
            BiosphereCoverage = biosphereCoverage,
            SupportsComplexLife = supportsComplexLife,
            NutrientAccessibility = opportunityState.NutrientAccessibility,
            SurfaceBiosphereChance = surfaceBiosphereChance,
            ProtectedBiosphereChance = protectedBiosphereChance,
            OxygenationChance = oxygenationChance,
            BiosignatureDetectabilityChance = biosignatureDetectabilityChance,
            EarlyDesiccationRisk = opportunityState.EarlyDesiccationRisk,
            PrebioticUvAdequacy = opportunityState.PrebioticUvAdequacy,
            AbioticOxygenFalsePositiveRisk = opportunityState.AbioticOxygenFalsePositiveRisk,
        };
    }

    private static Biochemistry SelectBiochemistry(
        PlanetEnvironmentProfile environment,
        double permissiveness,
        out double bestScore)
    {
        Biochemistry bestChemistry = Biochemistry.None;
        bestScore = 0.0;

        TryChemistry(Biochemistry.CarbonWater, ScoreCarbonWater(environment), 0.0, permissiveness, ref bestChemistry, ref bestScore);
        TryChemistry(Biochemistry.CarbonAmmonia, ScoreCarbonAmmonia(environment), 0.35, permissiveness, ref bestChemistry, ref bestScore);
        TryChemistry(Biochemistry.CarbonMethane, ScoreCarbonMethane(environment), 0.55, permissiveness, ref bestChemistry, ref bestScore);
        TryChemistry(Biochemistry.SulfurChemistry, ScoreSulfurChemistry(environment), 0.65, permissiveness, ref bestChemistry, ref bestScore);
        TryChemistry(Biochemistry.Exotic, ScoreExotic(environment), 0.90, permissiveness, ref bestChemistry, ref bestScore);

        return bestChemistry;
    }

    private static void TryChemistry(
        Biochemistry chemistry,
        double rawScore,
        double unlockAt,
        double permissiveness,
        ref Biochemistry bestChemistry,
        ref double bestScore)
    {
        if (permissiveness < unlockAt)
        {
            return;
        }

        if (rawScore <= 0.0)
        {
            return;
        }

        double score = rawScore;
        if (unlockAt > 0.0)
        {
            double ramp = Normalize(permissiveness, unlockAt, System.Math.Min(1.0, unlockAt + 0.20));
            score *= ramp;
        }

        if (score > bestScore)
        {
            bestChemistry = chemistry;
            bestScore = score;
        }
    }

    private static double ScoreCarbonWater(PlanetEnvironmentProfile environment)
    {
        bool hasSubsurfaceOcean = environment.IsMoon && environment.HasLiquidWater && environment.IceCoverage > 0.55;
        bool hasWaterWindow = environment.HasLiquidWater || environment.OceanCoverage > 0.02;
        if (!hasWaterWindow)
        {
            bool tidalSubsurfaceWindow = environment.IsMoon
                && environment.TidalHeatingFactor > 0.12
                && environment.IceCoverage > 0.10;
            if (!tidalSubsurfaceWindow)
            {
                return 0.0;
            }
        }

        double temperatureScore = Trap(environment.AvgTemperatureK, 215.0, 245.0, 315.0, 350.0);
        if (hasSubsurfaceOcean)
        {
            double subsurfaceTemperatureScore = 0.45 + (System.Math.Min(environment.TidalHeatingFactor, 0.35) * 1.55);
            temperatureScore = System.Math.Max(temperatureScore, Clamp01(subsurfaceTemperatureScore));
        }

        double pressureScore = Trap(environment.PressureAtm, 0.01, 0.08, 4.0, 14.0);
        if (hasSubsurfaceOcean && !environment.HasAtmosphere)
        {
            pressureScore = 0.82;
        }

        double gravityScore = Trap(environment.GravityG, 0.20, 0.45, 1.80, 2.80);
        if (hasSubsurfaceOcean && environment.GravityG > 0.05)
        {
            gravityScore = System.Math.Max(gravityScore, 0.70);
        }

        double radiationScore = 1.0 - Clamp01(environment.RadiationLevel * 1.10);
        double xuvExposureMultiplier = 0.50;
        if (environment.HasAtmosphere)
        {
            xuvExposureMultiplier = 0.25;
        }

        double xuvScore = 1.0 - Clamp01(environment.XuvExposure * xuvExposureMultiplier);
        double stabilityScore = Clamp01(1.0 - (environment.WeatherSeverity * 0.18) - (environment.VolcanismLevel * 0.12));
        double orbitScore = CalculateHabitableOrbitScore(environment);
        double tidalScore = CalculateTidalHabitabilityScore(environment);
        double magneticBonus = 0.85;
        if (environment.HasMagneticField || environment.MagneticFieldStrength >= 0.10)
        {
            magneticBonus = 1.0;
        }
        else if (hasSubsurfaceOcean)
        {
            magneticBonus = 0.95;
        }

        double solventScore = Clamp01(environment.OceanCoverage + (environment.IceCoverage * 0.30) + 0.20);
        if (environment.IsMoon && environment.TidalHeatingFactor > 0.20)
        {
            solventScore = System.Math.Max(solventScore, 0.45 + (environment.TidalHeatingFactor * 0.35));
        }

        if (hasSubsurfaceOcean)
        {
            solventScore = System.Math.Max(solventScore, 0.72);
        }

        return temperatureScore
            * pressureScore
            * gravityScore
            * radiationScore
            * xuvScore
            * stabilityScore
            * orbitScore
            * tidalScore
            * magneticBonus
            * Clamp01(solventScore);
    }

    private static double ScoreCarbonAmmonia(PlanetEnvironmentProfile environment)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double coldVolatileScore = Clamp01((environment.IceCoverage * 0.80) + 0.20);
        double temperatureScore = Trap(environment.AvgTemperatureK, 155.0, 175.0, 225.0, 255.0);
        double pressureScore = Trap(environment.PressureAtm, 0.20, 0.60, 8.0, 24.0);
        double gravityScore = Trap(environment.GravityG, 0.15, 0.35, 2.10, 3.10);
        double radiationScore = 1.0 - Clamp01(environment.RadiationLevel * 0.95);
        double xuvScore = 1.0 - Clamp01(environment.XuvExposure * 0.35);
        return temperatureScore * pressureScore * gravityScore * radiationScore * xuvScore * coldVolatileScore * 0.82;
    }

    private static double ScoreCarbonMethane(PlanetEnvironmentProfile environment)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double volatileScore = Clamp01((environment.IceCoverage * 0.90) + 0.10);
        double temperatureScore = Trap(environment.AvgTemperatureK, 75.0, 90.0, 155.0, 185.0);
        double pressureScore = Trap(environment.PressureAtm, 0.30, 0.80, 6.0, 18.0);
        double gravityScore = Trap(environment.GravityG, 0.08, 0.20, 1.60, 2.30);
        double radiationScore = 1.0 - Clamp01(environment.RadiationLevel);
        double xuvScore = 1.0 - Clamp01(environment.XuvExposure * 0.30);
        return temperatureScore * pressureScore * gravityScore * radiationScore * xuvScore * volatileScore * 0.70;
    }

    private static double ScoreSulfurChemistry(PlanetEnvironmentProfile environment)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double volcanismScore = Clamp01((environment.VolcanismLevel * 0.85) + 0.15);
        double temperatureScore = Trap(environment.AvgTemperatureK, 290.0, 330.0, 430.0, 520.0);
        double pressureScore = Trap(environment.PressureAtm, 0.20, 0.90, 12.0, 55.0);
        double radiationScore = 1.0 - Clamp01(environment.RadiationLevel * 1.15);
        double xuvScore = 1.0 - Clamp01(environment.XuvExposure * 0.25);
        return temperatureScore * pressureScore * volcanismScore * radiationScore * xuvScore * 0.62;
    }

    private static double ScoreExotic(PlanetEnvironmentProfile environment)
    {
        double temperatureBand = Trap(environment.AvgTemperatureK, 60.0, 90.0, 500.0, 620.0);
        if (temperatureBand <= 0.0)
        {
            return 0.0;
        }

        double anchorScore = 0.0;
        if (environment.HasLiquidWater)
        {
            anchorScore += 0.25;
        }

        if (environment.IceCoverage > 0.10)
        {
            anchorScore += 0.20;
        }

        if (environment.HasAtmosphere)
        {
            anchorScore += 0.20;
        }

        if (environment.IsMoon && environment.TidalHeatingFactor > 0.10)
        {
            anchorScore += 0.20;
        }

        if (environment.VolcanismLevel > 0.20)
        {
            anchorScore += 0.15;
        }

        return temperatureBand * Clamp01(anchorScore) * 0.35;
    }

    private static LifeOpportunityState BuildLifeOpportunityState(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double permissiveness)
    {
        double environmentalWindow = CalculateEnvironmentalWindowScore(environment);
        double nutrientAccessibility = CalculateNutrientAccessibility(environment);
        double prebioticUvAdequacy = CalculatePrebioticUvAdequacy(environment);
        double earlyDesiccationRisk = CalculateEarlyDesiccationRisk(environment);
        double protectedBiosphereSupport = CalculateProtectedBiosphereSupport(environment, chemistryScore);
        double surfaceBiosphereSupport = CalculateSurfaceBiosphereSupport(
            environment,
            chemistryScore,
            environmentalWindow,
            nutrientAccessibility);
        double oxygenationPotential = CalculateOxygenationPotential(
            environment,
            environmentalWindow,
            nutrientAccessibility);
        double abioticOxygenFalsePositiveRisk = CalculateAbioticOxygenFalsePositiveRisk(
            environment,
            earlyDesiccationRisk,
            permissiveness);

        return new LifeOpportunityState(
            environmentalWindow,
            nutrientAccessibility,
            prebioticUvAdequacy,
            earlyDesiccationRisk,
            protectedBiosphereSupport,
            surfaceBiosphereSupport,
            oxygenationPotential,
            abioticOxygenFalsePositiveRisk);
    }

    private static double CalculateAbiogenesisChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double permissiveness,
        LifePotentialModeling.Tuning lifeTuning,
        LifeOpportunityState opportunityState)
    {
        double baseChance = Lerp(0.08, 0.96, permissiveness);
        double habitabilityExponent = Lerp(3.10, 0.55, permissiveness);
        double chance = baseChance * System.Math.Pow(chemistryScore, habitabilityExponent);

        chance *= 0.65 + (0.35 * opportunityState.PrebioticUvAdequacy);
        chance *= 0.70 + (0.30 * opportunityState.EnvironmentalWindow);
        chance *= 1.0 - (opportunityState.EarlyDesiccationRisk * Lerp(0.40, 0.16, permissiveness));

        if (environment.HasLiquidWater && environment.PressureAtm >= 0.20)
        {
            chance += Lerp(0.00, 0.08, permissiveness);
        }

        if (environment.IsMoon && environment.TidalHeatingFactor > 0.20)
        {
            chance += Lerp(0.00, 0.05, permissiveness);
        }

        if (environment.HabitabilityScore >= 8)
        {
            double primeWorldFloor = Lerp(0.05, 0.34, permissiveness);
            double floorWindowMultiplier = 1.0 - (opportunityState.EarlyDesiccationRisk * 0.20);
            chance = System.Math.Max(chance, primeWorldFloor * floorWindowMultiplier);
        }

        chance *= lifeTuning.AbiogenesisMultiplier;
        return Clamp01(chance);
    }

    private static double CalculateSurfaceBiosphereChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        LifePotentialModeling.Tuning lifeTuning,
        LifeOpportunityState opportunityState)
    {
        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 0.8e9, 3.0e9, 0.72);
        double chance = chemistryScore
            * ageFactor
            * opportunityState.SurfaceBiosphereSupport
            * (1.0 - (opportunityState.EarlyDesiccationRisk * 0.50));

        if (LifePotentialModeling.RewardsStableWindows(lifeTuning))
        {
            chance *= lifeTuning.EnvironmentalWindowMultiplier;
        }

        return Clamp01(chance);
    }

    private static double CalculateProtectedBiosphereChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double permissiveness,
        LifeOpportunityState opportunityState)
    {
        double chance = chemistryScore * opportunityState.ProtectedBiosphereSupport;
        if (environment.IsMoon)
        {
            chance *= 1.08;
        }

        chance *= 0.90 + (0.10 * permissiveness);
        return Clamp01(chance);
    }

    private static double CalculateComplexLifeChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double surfaceBiosphereChance,
        double protectedBiosphereChance,
        double oxygenationChance,
        LifePotentialModeling.Tuning lifeTuning,
        LifeOpportunityState opportunityState)
    {
        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 0.9e9, 3.2e9, 0.78);
        double complexLifeBase = chemistryScore * ageFactor;
        complexLifeBase *= 0.78 + (0.22 * opportunityState.EnvironmentalWindow);
        complexLifeBase *= 0.58 + (0.42 * opportunityState.NutrientAccessibility);
        double shorelineFactor = 0.45 + (0.55 * Clamp01(System.Math.Min(environment.OceanCoverage, environment.LandCoverage) * 2.0));

        double surfaceTrack = complexLifeBase * surfaceBiosphereChance * shorelineFactor * (0.45 + (0.55 * oxygenationChance));
        double protectedTrack = complexLifeBase * protectedBiosphereChance * 0.18;

        if (LifePotentialModeling.RequiresBreathableAtmosphereForComplexLife(lifeTuning))
        {
            if (!environment.HasBreathableAtmosphere)
            {
                if (environment.HasAtmosphere)
                {
                    surfaceTrack *= 0.30;
                }
                else
                {
                    surfaceTrack *= 0.10;
                }
            }
        }

        double chance = System.Math.Max(surfaceTrack, protectedTrack);
        if (environment.HabitabilityScore >= 9
            && environment.HasLiquidWater)
        {
            chance = System.Math.Max(chance, 0.08);
        }

        chance *= lifeTuning.ComplexLifeMultiplier;
        return Clamp01(chance);
    }

    private static double CalculateSentienceChance(
        PlanetEnvironmentProfile environment,
        double complexLifeChance,
        double oxygenationChance,
        LifePotentialModeling.Tuning lifeTuning,
        LifeOpportunityState opportunityState)
    {
        if (complexLifeChance < 0.05)
        {
            return 0.0;
        }

        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 2.2e9, 4.8e9, 0.66);
        double dryLandFactor = 0.22 + (Clamp01(environment.LandCoverage) * 0.58);
        double climaticVariability = CalculateClimaticVariability(environment);
        double sentienceChance = complexLifeChance
            * (0.050 + (0.120 * ageFactor))
            * dryLandFactor
            * (0.65 + (0.35 * opportunityState.NutrientAccessibility))
            * (0.60 + (0.40 * oxygenationChance))
            * (0.72 + (0.28 * climaticVariability));

        if (LifePotentialModeling.RewardsStableWindows(lifeTuning))
        {
            sentienceChance *= 0.84 + (0.32 * opportunityState.EnvironmentalWindow);
        }

        if (environment.HabitabilityScore >= 9
            && environment.HasLiquidWater)
        {
            sentienceChance = System.Math.Max(sentienceChance, 0.001);
        }

        sentienceChance *= lifeTuning.SentienceMultiplier;
        return Clamp01(sentienceChance);
    }

    private static double CalculateCivilizationChance(
        PlanetEnvironmentProfile environment,
        double sentienceChance,
        double oxygenationChance,
        LifePotentialModeling.Tuning lifeTuning,
        LifeOpportunityState opportunityState)
    {
        if (sentienceChance <= 0.0)
        {
            return 0.0;
        }

        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 3.0e9, 6.2e9, 0.60);
        double landFactor = 0.20 + (Clamp01(environment.LandCoverage) * 0.60);
        double resourceFactor = 0.55 + (0.20 * environment.ResourceDiversity) + (0.25 * environment.ResourceRichness);
        double oxygenFactor = 1.0;
        if (lifeTuning.RequiresOxygenRichAtmosphereForCivilization && !environment.HasBreathableAtmosphere)
        {
            if (environment.HasAtmosphere)
            {
                oxygenFactor = 0.12 + (oxygenationChance * 0.25);
            }
            else
            {
                oxygenFactor = 0.04;
            }
        }
        else if (!environment.HasBreathableAtmosphere)
        {
            oxygenFactor = 0.30 + (oxygenationChance * 0.30);
        }

        double civilizationChance = sentienceChance
            * (0.55 + (0.30 * ageFactor))
            * landFactor
            * resourceFactor
            * oxygenFactor
            * (0.75 + (0.25 * opportunityState.EnvironmentalWindow));
        civilizationChance *= lifeTuning.EnvironmentalWindowMultiplier;
        civilizationChance *= lifeTuning.CivilizationMultiplier;
        return Clamp01(civilizationChance);
    }

    private static double CalculateOxygenationChance(
        PlanetEnvironmentProfile environment,
        double surfaceBiosphereChance,
        LifeOpportunityState opportunityState)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 1.8e9, 4.0e9, 0.50);
        double chance = surfaceBiosphereChance
            * ageFactor
            * opportunityState.OxygenationPotential
            * (1.0 - (opportunityState.AbioticOxygenFalsePositiveRisk * 0.20));
        return Clamp01(chance);
    }

    private static double CalculateBiosignatureDetectabilityChance(
        PlanetEnvironmentProfile environment,
        double abiogenesisChance,
        double surfaceBiosphereChance,
        double oxygenationChance,
        LifeOpportunityState opportunityState)
    {
        double atmosphericAccess = 0.18;
        if (environment.HasAtmosphere)
        {
            atmosphericAccess = 1.0;
        }

        double detectability = abiogenesisChance
            * atmosphericAccess
            * (0.25 + (0.55 * surfaceBiosphereChance) + (0.20 * oxygenationChance))
            * (1.0 - (opportunityState.AbioticOxygenFalsePositiveRisk * 0.55));
        return Clamp01(detectability);
    }

    private static double CalculateBiosphereCoverage(
        PlanetEnvironmentProfile environment,
        Biochemistry chemistry,
        double chemistryScore,
        double permissiveness,
        double surfaceBiosphereChance,
        double protectedBiosphereChance)
    {
        double coverage = chemistryScore * Lerp(0.25, 0.95, permissiveness);

        if (chemistry == Biochemistry.CarbonWater)
        {
            coverage *= Clamp01(0.20 + (surfaceBiosphereChance * 0.75) + (protectedBiosphereChance * 0.25));
            coverage *= Clamp01(0.20 + environment.OceanCoverage + environment.LandCoverage);
        }
        else if (chemistry == Biochemistry.CarbonAmmonia || chemistry == Biochemistry.CarbonMethane)
        {
            coverage *= Clamp01(0.12 + (environment.IceCoverage * 0.75) + (protectedBiosphereChance * 0.45));
        }
        else if (chemistry == Biochemistry.SulfurChemistry)
        {
            coverage *= Clamp01(0.12 + (environment.VolcanismLevel * 0.60));
        }
        else
        {
            coverage *= 0.35;
        }

        return Clamp01(coverage);
    }

    private static double CalculateNutrientAccessibility(PlanetEnvironmentProfile environment)
    {
        double shorelineMix = System.Math.Min(environment.OceanCoverage, environment.LandCoverage) * 2.0;
        double continentFactor = Clamp01(environment.ContinentCount / 5.0);
        double tectonicFactor = Clamp01((environment.TectonicActivity * 0.65) + (environment.VolcanismLevel * 0.20));
        double nutrientScore = 0.18
            + (shorelineMix * 0.32)
            + (continentFactor * 0.16)
            + (tectonicFactor * 0.14)
            + (environment.ResourceDiversity * 0.10)
            + (environment.ResourceRichness * 0.10);

        if (environment.IsMoon && environment.IceCoverage > 0.70 && environment.TidalHeatingFactor > 0.15)
        {
            nutrientScore = System.Math.Max(nutrientScore, 0.34 + (environment.TidalHeatingFactor * 0.28));
        }

        if (environment.OceanCoverage > 0.92 && environment.LandCoverage < 0.05)
        {
            nutrientScore *= 0.72;
        }

        return Clamp01(nutrientScore);
    }

    private static double CalculatePrebioticUvAdequacy(PlanetEnvironmentProfile environment)
    {
        double uvScore = Trap(environment.XuvExposure, 0.02, 0.06, 0.25, 0.60);
        double atmosphericModeration = 1.0;
        if (!environment.HasAtmosphere)
        {
            atmosphericModeration = 0.85;
        }
        else if (environment.PressureAtm > 2.0)
        {
            atmosphericModeration = 0.88;
        }

        double rotationScore = CalculateRotationHabitabilityScore(environment);
        return Clamp01((uvScore * atmosphericModeration * 0.75) + (rotationScore * 0.25));
    }

    private static double CalculateEarlyDesiccationRisk(PlanetEnvironmentProfile environment)
    {
        double risk = 0.0;
        risk += Clamp01(environment.XuvExposure * 0.55);
        risk += Clamp01(System.Math.Max(0.0, environment.StellarFluxEarth - 1.10) / 1.40) * 0.20;
        risk += Clamp01(0.25 - environment.PressureAtm) * 0.35;
        risk += Clamp01(0.12 - environment.MagneticFieldStrength) * 0.20;
        if (environment.IsTidallyLocked && environment.HasAtmosphere && environment.PressureAtm < 0.40)
        {
            risk += 0.10;
        }

        return Clamp01(risk);
    }

    private static double CalculateProtectedBiosphereSupport(
        PlanetEnvironmentProfile environment,
        double chemistryScore)
    {
        if (!environment.IsMoon && environment.IceCoverage < 0.50)
        {
            return Clamp01(chemistryScore * 0.18);
        }

        double support = chemistryScore
            * (0.30 + (environment.IceCoverage * 0.35))
            * (0.35 + (Clamp01(environment.TidalHeatingFactor / 0.35) * 0.45))
            * (0.70 + (environment.ParentRadiationExposure * 0.10));

        if (environment.HasLiquidWater)
        {
            support += 0.12;
        }

        return Clamp01(support);
    }

    private static double CalculateSurfaceBiosphereSupport(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double environmentalWindow,
        double nutrientAccessibility)
    {
        double rotationScore = CalculateRotationHabitabilityScore(environment);
        double support = chemistryScore
            * environmentalWindow
            * (0.55 + (0.45 * nutrientAccessibility))
            * (0.72 + (0.28 * rotationScore));

        if (environment.HasLiquidWater)
        {
            support *= 1.08;
        }

        if (environment.IsMoon)
        {
            support *= 0.82;
        }

        return Clamp01(support);
    }

    private static double CalculateOxygenationPotential(
        PlanetEnvironmentProfile environment,
        double environmentalWindow,
        double nutrientAccessibility)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double landFactor = Clamp01(environment.LandCoverage);
        double tectonicFactor = Clamp01((environment.TectonicActivity * 0.70) + (environment.VolcanismLevel * 0.20));
        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 1.5e9, 4.5e9, 0.45);
        double oxygenation = environmentalWindow
            * nutrientAccessibility
            * (0.30 + (landFactor * 0.30) + (tectonicFactor * 0.20) + (ageFactor * 0.20));

        if (!environment.HasBreathableAtmosphere)
        {
            oxygenation *= 0.72;
        }

        return Clamp01(oxygenation);
    }

    private static double CalculateAbioticOxygenFalsePositiveRisk(
        PlanetEnvironmentProfile environment,
        double earlyDesiccationRisk,
        double permissiveness)
    {
        if (!environment.HasAtmosphere)
        {
            return 0.0;
        }

        double dryBias = 1.0 - Clamp01(environment.OceanCoverage + (environment.IceCoverage * 0.35));
        double thinAirBias = Clamp01(0.45 - environment.PressureAtm);
        double risk = (environment.XuvExposure * 0.45)
            + (earlyDesiccationRisk * 0.30)
            + (dryBias * 0.20)
            + (thinAirBias * 0.15);
        risk *= 0.85 + (0.15 * permissiveness);
        return Clamp01(risk);
    }

    private static double CalculateSurfaceDiversity(PlanetEnvironmentProfile environment)
    {
        double waterLandMix = System.Math.Min(environment.OceanCoverage, environment.LandCoverage) * 2.0;
        double biomeDiversity = 0.0;
        if (environment.BiomeCoverage.Count > 0)
        {
            biomeDiversity = Clamp01(environment.BiomeCoverage.Count / 6.0);
        }

        double factor = 0.35 + (Clamp01(waterLandMix) * 0.35) + (biomeDiversity * 0.30);
        if (environment.IceCoverage > 0.60 && environment.LandCoverage < 0.10)
        {
            factor *= 0.70;
        }

        return Clamp01(factor);
    }

    private static double CalculateHabitableOrbitScore(PlanetEnvironmentProfile environment)
    {
        if (environment.HabitableZoneAlignment > 0.0)
        {
            return Clamp01(0.55 + (environment.HabitableZoneAlignment * 0.45));
        }

        if (environment.IsMoon && environment.TidalHeatingFactor > 0.12 && environment.IceCoverage > 0.10)
        {
            return Clamp01(0.58 + (System.Math.Min(environment.TidalHeatingFactor, 0.45) * 0.55));
        }

        if (environment.StellarFluxEarth > 0.0)
        {
            double fluxScore = Trap(environment.StellarFluxEarth, 0.15, 0.35, 1.80, 2.80);
            return Clamp01(0.40 + (fluxScore * 0.60));
        }

        if (environment.HasLiquidWater)
        {
            return 0.82;
        }

        if (environment.HasAtmosphere)
        {
            return 0.70;
        }

        return 0.55;
    }

    private static double CalculateTidalHabitabilityScore(PlanetEnvironmentProfile environment)
    {
        if (!environment.IsMoon)
        {
            return 1.0;
        }

        if (environment.TidalHeatingFactor <= 0.02)
        {
            if (environment.HasLiquidWater && environment.IceCoverage > 0.50)
            {
                return 0.72;
            }

            if (environment.HasLiquidWater)
            {
                return 0.88;
            }

            return 0.55;
        }

        if (environment.TidalHeatingFactor <= 0.30)
        {
            double ramp = Normalize(environment.TidalHeatingFactor, 0.02, 0.30);
            return Clamp01(0.75 + (ramp * 0.25));
        }

        if (environment.TidalHeatingFactor <= 0.65)
        {
            double decline = Normalize(environment.TidalHeatingFactor, 0.30, 0.65);
            return Clamp01(1.0 - (decline * 0.35));
        }

        double overload = Normalize(environment.TidalHeatingFactor, 0.65, 1.10);
        return Clamp01(0.65 - (overload * 0.40));
    }

    private static double CalculateClimaticVariability(PlanetEnvironmentProfile environment)
    {
        double tiltScore = Trap(environment.AxialTiltDeg, 2.0, 8.0, 30.0, 55.0);
        double weatherPenalty = 1.0 - Clamp01(environment.WeatherSeverity * 0.55);
        double variability = (tiltScore * 0.55) + (weatherPenalty * 0.45);
        if (environment.IsTidallyLocked)
        {
            variability *= 0.82;
        }

        return Clamp01(variability);
    }

    private static double CalculateRotationHabitabilityScore(PlanetEnvironmentProfile environment)
    {
        if (environment.IsTidallyLocked)
        {
            if (environment.HasAtmosphere && environment.PressureAtm >= 0.50)
            {
                return 0.68;
            }

            return 0.34;
        }

        if (environment.DayLengthHours <= 0.0)
        {
            return 0.72;
        }

        return Trap(environment.DayLengthHours, 6.0, 12.0, 72.0, 200.0);
    }

    private static double CalculateAgeFactor(
        double stellarAgeYears,
        double minYears,
        double idealYears,
        double fallbackWhenUnknown)
    {
        if (stellarAgeYears <= 0.0)
        {
            return Clamp01(fallbackWhenUnknown);
        }

        return Normalize(stellarAgeYears, minYears, idealYears);
    }

    private static FailureReason DetermineFailureReason(PlanetEnvironmentProfile environment, double permissiveness)
    {
        double minimumTemperature = 70.0;
        if (permissiveness < 0.65)
        {
            minimumTemperature = 150.0;
        }

        if (permissiveness < 0.35)
        {
            minimumTemperature = 205.0;
        }

        double maximumTemperature = 620.0;
        if (permissiveness < 0.90)
        {
            maximumTemperature = 520.0;
        }

        if (permissiveness < 0.65)
        {
            maximumTemperature = 420.0;
        }

        if (environment.AvgTemperatureK > 0.0 && environment.AvgTemperatureK < minimumTemperature)
        {
            return FailureReason.TooCold;
        }

        if (environment.AvgTemperatureK > maximumTemperature)
        {
            return FailureReason.TooHot;
        }

        double radiationLimit = Lerp(0.90, 0.995, permissiveness);
        if (environment.RadiationLevel >= radiationLimit)
        {
            return FailureReason.HighRadiation;
        }

        bool chemistryAnchorsPresent = environment.HasLiquidWater
            || environment.OceanCoverage > 0.02
            || environment.IceCoverage > 0.10
            || environment.HasAtmosphere
            || (environment.IsMoon && environment.TidalHeatingFactor > 0.10)
            || environment.VolcanismLevel > 0.20;
        if (!chemistryAnchorsPresent)
        {
            return FailureReason.NoLiquidWater;
        }

        return FailureReason.LowHabitability;
    }

    private static double CalculateEnvironmentalWindowScore(PlanetEnvironmentProfile environment)
    {
        double score = 0.0;
        score += Clamp01(environment.HabitableZoneAlignment) * 0.20;
        score += Clamp01(1.0 - environment.RadiationLevel) * 0.14;
        score += Clamp01(1.0 - environment.XuvExposure) * 0.12;
        score += Clamp01(1.0 - (environment.WeatherSeverity * 0.75)) * 0.10;
        score += Clamp01(1.0 - (environment.VolcanismLevel * 0.65)) * 0.08;
        double breathableAtmosphereScore = 0.0;
        if (environment.HasBreathableAtmosphere)
        {
            breathableAtmosphereScore = 1.0;
        }

        score += Clamp01(breathableAtmosphereScore) * 0.08;
        score += Clamp01(environment.OceanCoverage + environment.LandCoverage) * 0.08;
        score += Clamp01(environment.StellarAgeYears / 6.0e9) * 0.08;
        score += CalculateSurfaceDiversity(environment) * 0.07;
        score += CalculateRotationHabitabilityScore(environment) * 0.05;
        return Clamp01(score);
    }

    private static double Trap(double x, double a, double b, double c, double d)
    {
        if (x <= a || x >= d)
        {
            return 0.0;
        }

        if (x >= b && x <= c)
        {
            return 1.0;
        }

        if (x < b)
        {
            return Normalize(x, a, b);
        }

        return Normalize(d - x, 0.0, d - c);
    }

    private static double Normalize(double value, double minValue, double maxValue)
    {
        if (value <= minValue)
        {
            return 0.0;
        }

        if (value >= maxValue)
        {
            return 1.0;
        }

        return (value - minValue) / (maxValue - minValue);
    }

    private static double Lerp(double minValue, double maxValue, double factor)
    {
        return minValue + ((maxValue - minValue) * factor);
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }

    private static Assessment CreateFailure(FailureReason reason)
    {
        return new Assessment
        {
            IsSupported = false,
            Reason = reason,
            PreferredChemistry = Biochemistry.None,
            BiosphereSuitability = 0.0,
            AbiogenesisChance = 0.0,
            ComplexLifeChance = 0.0,
            SentienceChance = 0.0,
            CivilizationChance = 0.0,
            BiosphereCoverage = 0.0,
            SupportsComplexLife = false,
            NutrientAccessibility = 0.0,
            SurfaceBiosphereChance = 0.0,
            ProtectedBiosphereChance = 0.0,
            OxygenationChance = 0.0,
            BiosignatureDetectabilityChance = 0.0,
            EarlyDesiccationRisk = 0.0,
            PrebioticUvAdequacy = 0.0,
            AbioticOxygenFalsePositiveRisk = 0.0,
        };
    }
}
