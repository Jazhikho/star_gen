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
        /// <summary>
        /// Whether the evaluated world can support biology.
        /// </summary>
        public bool IsSupported { get; init; }

        /// <summary>
        /// Failure reason when biology is not supported.
        /// </summary>
        public FailureReason Reason { get; init; }

        /// <summary>
        /// Best-fit chemistry for the current world model.
        /// </summary>
        public Biochemistry PreferredChemistry { get; init; }

        /// <summary>
        /// Best-fit chemistry suitability in the inclusive range [0, 1].
        /// </summary>
        public double BiosphereSuitability { get; init; }

        /// <summary>
        /// Probability that abiogenesis succeeds when the world is rolled.
        /// </summary>
        public double AbiogenesisChance { get; init; }

        /// <summary>
        /// Probability that the world can sustain rich, multicellular ecosystems.
        /// </summary>
        public double ComplexLifeChance { get; init; }

        /// <summary>
        /// Probability that a stable biosphere eventually produces a sentient lineage.
        /// </summary>
        public double SentienceChance { get; init; }

        /// <summary>
        /// Approximate fraction of the world engaged in the biosphere.
        /// </summary>
        public double BiosphereCoverage { get; init; }

        /// <summary>
        /// Whether the world can plausibly sustain complex ecosystems instead of only microbial life.
        /// </summary>
        public bool SupportsComplexLife { get; init; }
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
        double permissiveness = ResolveLifePermissiveness(useCaseSettings);
        Biochemistry bestChemistry = SelectBiochemistry(environment, permissiveness, out double chemistryScore);
        if (bestChemistry == Biochemistry.None)
        {
            return CreateFailure(DetermineFailureReason(environment, permissiveness));
        }

        double supportThreshold = Lerp(0.18, 0.04, permissiveness);
        if (chemistryScore < supportThreshold)
        {
            return CreateFailure(DetermineFailureReason(environment, permissiveness));
        }

        double abiogenesisChance = CalculateAbiogenesisChance(environment, chemistryScore, permissiveness);
        double complexLifeChance = CalculateComplexLifeChance(environment, chemistryScore);
        double sentienceChance = CalculateSentienceChance(environment, complexLifeChance);
        double biosphereCoverage = CalculateBiosphereCoverage(environment, bestChemistry, chemistryScore, permissiveness);
        bool supportsComplexLife = complexLifeChance >= 0.12;

        return new Assessment
        {
            IsSupported = abiogenesisChance > 0.0,
            Reason = FailureReason.None,
            PreferredChemistry = bestChemistry,
            BiosphereSuitability = chemistryScore,
            AbiogenesisChance = abiogenesisChance,
            ComplexLifeChance = complexLifeChance,
            SentienceChance = sentienceChance,
            BiosphereCoverage = biosphereCoverage,
            SupportsComplexLife = supportsComplexLife,
        };
    }

    private static Biochemistry SelectBiochemistry(
        PlanetEnvironmentProfile environment,
        double permissiveness,
        out double bestScore)
    {
        Biochemistry bestChemistry = Biochemistry.None;
        bestScore = 0.0;

        TryChemistry(
            Biochemistry.CarbonWater,
            ScoreCarbonWater(environment),
            0.0,
            permissiveness,
            ref bestChemistry,
            ref bestScore);
        TryChemistry(
            Biochemistry.CarbonAmmonia,
            ScoreCarbonAmmonia(environment),
            0.35,
            permissiveness,
            ref bestChemistry,
            ref bestScore);
        TryChemistry(
            Biochemistry.CarbonMethane,
            ScoreCarbonMethane(environment),
            0.55,
            permissiveness,
            ref bestChemistry,
            ref bestScore);
        TryChemistry(
            Biochemistry.SulfurChemistry,
            ScoreSulfurChemistry(environment),
            0.65,
            permissiveness,
            ref bestChemistry,
            ref bestScore);
        TryChemistry(
            Biochemistry.Exotic,
            ScoreExotic(environment),
            0.90,
            permissiveness,
            ref bestChemistry,
            ref bestScore);

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
        if (!hasWaterWindow && !(environment.IsMoon && environment.TidalHeatingFactor > 0.12 && environment.IceCoverage > 0.10))
        {
            return 0.0;
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
        double xuvScore = 1.0 - Clamp01(environment.XuvExposure * (environment.HasAtmosphere ? 0.25 : 0.50));
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

    private static double CalculateAbiogenesisChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore,
        double permissiveness)
    {
        double baseChance = Lerp(0.08, 0.96, permissiveness);
        double habitabilityExponent = Lerp(3.10, 0.55, permissiveness);
        double chance = baseChance * System.Math.Pow(chemistryScore, habitabilityExponent);

        if (environment.HasBreathableAtmosphere)
        {
            chance += Lerp(0.01, 0.03, permissiveness);
        }

        if (environment.HasLiquidWater && environment.PressureAtm >= 0.20)
        {
            chance += Lerp(0.00, 0.08, permissiveness);
        }

        if (environment.IsMoon && environment.TidalHeatingFactor > 0.20)
        {
            chance += Lerp(0.00, 0.06, permissiveness);
        }

        chance *= 0.90 + (0.10 * CalculateHabitableOrbitScore(environment));
        chance *= 1.0 - (Clamp01(environment.XuvExposure * 0.35) * Lerp(0.08, 0.03, permissiveness));

        if (environment.RadiationLevel > 0.75)
        {
            chance -= Lerp(0.12, 0.05, permissiveness);
        }

        if (environment.HabitabilityScore >= 8)
        {
            double primeWorldFloor = Lerp(0.05, 0.34, permissiveness);
            double xuvPenalty = 1.0 - (Clamp01(environment.XuvExposure * 0.22) * Lerp(0.14, 0.06, permissiveness));
            double orbitBonus = 0.92 + (0.08 * CalculateHabitableOrbitScore(environment));
            chance = System.Math.Max(chance, primeWorldFloor * xuvPenalty * orbitBonus);
        }

        return Clamp01(chance);
    }

    private static double CalculateComplexLifeChance(
        PlanetEnvironmentProfile environment,
        double chemistryScore)
    {
        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 0.9e9, 3.2e9, 0.78);
        double stabilityFactor = Clamp01(1.0
            - (environment.WeatherSeverity * 0.28)
            - (environment.VolcanismLevel * 0.18)
            - (environment.RadiationLevel * 0.24)
            - (environment.XuvExposure * 0.09));
        double diversityFactor = CalculateSurfaceDiversity(environment);
        double orbitFactor = 0.82 + (0.18 * CalculateHabitableOrbitScore(environment));
        double tidalFactor = CalculateTidalHabitabilityScore(environment);
        double chance = chemistryScore * ageFactor * stabilityFactor * diversityFactor * orbitFactor * tidalFactor;
        return Clamp01(chance);
    }

    private static double CalculateSentienceChance(
        PlanetEnvironmentProfile environment,
        double complexLifeChance)
    {
        if (complexLifeChance < 0.12)
        {
            return 0.0;
        }

        double ageFactor = CalculateAgeFactor(environment.StellarAgeYears, 2.2e9, 4.8e9, 0.66);
        double stabilityFactor = Clamp01(1.0
            - (environment.WeatherSeverity * 0.22)
            - (environment.VolcanismLevel * 0.10)
            - (environment.RadiationLevel * 0.18));
        double dryLandFactor = 0.35 + (Clamp01(environment.LandCoverage) * 0.55);
        double baselineChance = 0.008 + (0.028 * ageFactor * stabilityFactor * dryLandFactor);
        return Clamp01(baselineChance);
    }

    private static double CalculateBiosphereCoverage(
        PlanetEnvironmentProfile environment,
        Biochemistry chemistry,
        double chemistryScore,
        double permissiveness)
    {
        double coverage = chemistryScore * Lerp(0.25, 0.95, permissiveness);

        if (chemistry == Biochemistry.CarbonWater)
        {
            coverage *= Clamp01(0.30 + environment.OceanCoverage + environment.LandCoverage);
        }
        else if (chemistry == Biochemistry.CarbonAmmonia || chemistry == Biochemistry.CarbonMethane)
        {
            coverage *= Clamp01(0.18 + (environment.IceCoverage * 0.90));
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

            return environment.HasLiquidWater ? 0.88 : 0.55;
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

    private static double ResolveLifePermissiveness(GenerationUseCaseSettings? useCaseSettings)
    {
        if (useCaseSettings == null)
        {
            return GenerationUseCaseSettings.NeutralPermissiveness;
        }

        return useCaseSettings.LifePermissiveness;
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
            BiosphereCoverage = 0.0,
            SupportsComplexLife = false,
        };
    }
}
