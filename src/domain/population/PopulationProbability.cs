using StarGen.Domain.Generation;

namespace StarGen.Domain.Population;

/// <summary>
/// Pure probability helpers for native-life and colony generation.
/// </summary>
public static class PopulationProbability
{
    /// <summary>
    /// Minimum habitability score for native life to be possible.
    /// </summary>
    public const int MinHabitabilityForNatives = 3;

    /// <summary>
    /// Minimum habitability score for any colonization attempt.
    /// </summary>
    public const int MinHabitabilityForColony = 1;

    /// <summary>
    /// Base scaling factor for native-life emergence probability.
    /// </summary>
    public const double NativeProbabilityScale = 0.06;

    /// <summary>
    /// Base probability for a colony attempt before suitability scaling.
    /// </summary>
    public const double ColonyBaseProbability = 0.30;

    /// <summary>
    /// Maximum allowed native-life probability after all modifiers.
    /// </summary>
    public const double MaxNativeProbability = 0.98;

    /// <summary>
    /// Maximum allowed colony-attempt probability after all modifiers.
    /// </summary>
    public const double MaxColonyProbability = 0.97;

    private const double LiquidWaterBonus = 0.15;
    private const double BreathableAtmosphereBonus = 0.10;
    private const double TidalLockingPenalty = 0.10;
    private const double TidalHeatingBonus = 0.05;
    private const int MinSuitabilityForColony = 10;

    /// <summary>
    /// Calculates the probability that native life emerged on a body.
    /// </summary>
    public static double CalculateNativeProbability(PlanetProfile profile)
    {
        return CalculateNativeProbability(profile, GenerationUseCaseSettings.NeutralPermissiveness);
    }

    /// <summary>
    /// Calculates the probability that native life emerged on a body using the active use-case settings.
    /// </summary>
    public static double CalculateNativeProbability(PlanetProfile profile, GenerationUseCaseSettings? useCaseSettings)
    {
        BiologySupportEvaluator.Assessment assessment = BiologySupportEvaluator.Evaluate(profile, useCaseSettings);
        if (!assessment.IsSupported)
        {
            return 0.0;
        }

        double permissiveness = ResolveLifePermissiveness(useCaseSettings);
        RpgCompatibilityProfile compatibilityProfile = ResolveCompatibilityProfile(useCaseSettings);
        double probability = assessment.AbiogenesisChance;

        if (profile.IsTidallyLocked)
        {
            probability -= Lerp(TidalLockingPenalty + 0.04, 0.04, permissiveness);
        }

        if (profile.HasLiquidWater)
        {
            probability += LiquidWaterBonus * Lerp(0.70, 0.40, permissiveness);
        }

        if (profile.HasBreathableAtmosphere)
        {
            probability += BreathableAtmosphereBonus * Lerp(0.80, 0.30, permissiveness);
        }

        if (profile.IsMoon && profile.TidalHeatingFactor > 0.30)
        {
            probability += Lerp(0.01, TidalHeatingBonus + 0.04, permissiveness);
        }

        probability *= compatibilityProfile.NativeLifeProbabilityMultiplier;
        return System.Math.Clamp(probability, 0.0, MaxNativeProbability);
    }

    /// <summary>
    /// Calculates the probability that native life emerged on a body using a user-facing permissiveness value.
    /// </summary>
    public static double CalculateNativeProbability(PlanetProfile profile, double lifePermissiveness)
    {
        double permissiveness = ClampPermissiveness(lifePermissiveness);
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.LifePermissiveness = permissiveness;
        return CalculateNativeProbability(profile, settings);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted.
    /// </summary>
    public static double CalculateColonyProbability(PlanetProfile profile, ColonySuitability suitability)
    {
        return CalculateColonyProbability(profile, suitability, GenerationUseCaseSettings.NeutralPermissiveness, null);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted using a user-facing permissiveness value.
    /// </summary>
    public static double CalculateColonyProbability(
        PlanetProfile profile,
        ColonySuitability suitability,
        double populationPermissiveness,
        ColonyPressureContext? pressureContext = null)
    {
        double permissiveness = ResolveEffectivePermissiveness(populationPermissiveness, pressureContext);
        if (suitability.OverallScore < MinSuitabilityForColony)
        {
            return 0.0;
        }

        double minimumSuitability = Lerp(35.0, 5.0, permissiveness);
        if (suitability.OverallScore < minimumSuitability)
        {
            return 0.0;
        }

        double normalizedSuitability = Normalize(suitability.OverallScore, minimumSuitability, 100.0);
        double probability = Lerp(0.05, ColonyBaseProbability + 0.10, permissiveness);
        probability += normalizedSuitability * Lerp(0.18, 0.48, permissiveness);

        if (profile.HabitabilityScore >= 6)
        {
            probability += Lerp(0.10, 0.06, permissiveness);
        }

        if (profile.HabitabilityScore >= 8)
        {
            probability += Lerp(0.0, 0.10, permissiveness);
        }
        else if (profile.HabitabilityScore < MinHabitabilityForColony)
        {
            probability -= Lerp(0.12, 0.0, permissiveness);
        }

        if (suitability.RequiresLifeSupport)
        {
            probability -= Lerp(0.18, 0.05, permissiveness);
        }

        if (suitability.RequiresPressureSuit)
        {
            probability -= Lerp(0.10, 0.03, permissiveness);
        }

        if (suitability.RequiresRadiationShielding)
        {
            probability -= Lerp(0.12, 0.04, permissiveness);
        }

        if (profile.IsMoon)
        {
            probability += Lerp(0.0, 0.08, permissiveness);
        }

        if (suitability.OverallScore >= 35)
        {
            probability = System.Math.Max(probability, Lerp(0.04, 0.22, permissiveness));
        }

        if (suitability.OverallScore >= 50)
        {
            probability = System.Math.Max(probability, Lerp(0.08, 0.40, permissiveness));
        }

        if (pressureContext != null)
        {
            probability += pressureContext.LocalNativePressure * Lerp(0.08, 0.24, permissiveness);
            probability += pressureContext.NearbySystemNativePressure * Lerp(0.04, 0.16, permissiveness);

            if (pressureContext.LocalNativeWorldCount > 0 && profile.IsMoon)
            {
                probability += Lerp(0.01, 0.06, permissiveness);
            }

            if (pressureContext.GetCombinedNativeWorldCount() >= 2)
            {
                probability += Lerp(0.01, 0.05, permissiveness);
            }
        }

        return System.Math.Clamp(probability, 0.0, MaxColonyProbability);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted using active use-case settings.
    /// </summary>
    public static double CalculateColonyProbability(
        PlanetProfile profile,
        ColonySuitability suitability,
        GenerationUseCaseSettings? settings,
        ColonyPressureContext? pressureContext = null)
    {
        double permissiveness = ResolveLifePermissiveness(settings);
        double probability = CalculateColonyProbability(profile, suitability, permissiveness, pressureContext);
        RpgCompatibilityProfile compatibilityProfile = ResolveCompatibilityProfile(settings);
        probability *= compatibilityProfile.ColonyProbabilityMultiplier;
        return System.Math.Clamp(probability, 0.0, MaxColonyProbability);
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

    private static double ClampPermissiveness(double permissiveness)
    {
        return System.Math.Clamp(permissiveness, 0.0, 1.0);
    }

    private static double ResolveLifePermissiveness(GenerationUseCaseSettings? settings)
    {
        if (settings == null)
        {
            return GenerationUseCaseSettings.NeutralPermissiveness;
        }

        if (!settings.HasLifePermissivenessOverride())
        {
            return GenerationUseCaseSettings.GetRecommendedLifePermissiveness(settings.LifePotentialModel);
        }

        return ClampPermissiveness(settings.LifePermissiveness);
    }

    private static RpgCompatibilityProfile ResolveCompatibilityProfile(GenerationUseCaseSettings? settings)
    {
        if (settings == null)
        {
            return RpgCompatibilityProfile.Resolve(GenerationUseCaseSettings.RulesetModeType.Default);
        }

        return settings.GetCompatibilityProfile();
    }

    private static double ResolveEffectivePermissiveness(double basePermissiveness, ColonyPressureContext? pressureContext)
    {
        double permissiveness = ClampPermissiveness(basePermissiveness);
        if (pressureContext == null)
        {
            return permissiveness;
        }

        double combinedPressure = pressureContext.GetCombinedPressure();
        double pressureLift = (1.0 - permissiveness) * combinedPressure * 0.55;
        return ClampPermissiveness(permissiveness + pressureLift);
    }
}
