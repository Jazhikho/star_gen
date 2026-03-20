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
    /// Calculates the probability that native life emerged on a body using a user-facing permissiveness value.
    /// </summary>
    public static double CalculateNativeProbability(PlanetProfile profile, double lifePermissiveness)
    {
        double permissiveness = ClampPermissiveness(lifePermissiveness);
        if (!CanSupportBiologyAtAll(profile))
        {
            return 0.0;
        }

        double minimumHabitability = Lerp(7.0, 2.0, permissiveness);
        if (profile.HabitabilityScore < minimumHabitability)
        {
            return 0.0;
        }

        double normalizedHabitability = Normalize(profile.HabitabilityScore, minimumHabitability, 10.0);
        double probability = Lerp(0.04, 0.18, permissiveness);
        probability += normalizedHabitability * Lerp(0.18, 0.62, permissiveness);
        probability += LiquidWaterBonus;

        if (profile.HasBreathableAtmosphere)
        {
            probability += Lerp(BreathableAtmosphereBonus + 0.10, BreathableAtmosphereBonus, permissiveness);
        }
        else if (profile.HasAtmosphere)
        {
            probability -= Lerp(0.10, -0.02, permissiveness);
        }
        else
        {
            probability -= Lerp(0.22, 0.06, permissiveness);
        }

        if (profile.IsMoon && profile.TidalHeatingFactor > 0.3)
        {
            probability += Lerp(0.02, TidalHeatingBonus + 0.05, permissiveness);
        }

        if (profile.IsTidallyLocked)
        {
            probability -= Lerp(TidalLockingPenalty + 0.04, 0.04, permissiveness);
        }

        if (profile.RadiationLevel > 0.7)
        {
            probability -= Lerp(0.22, 0.10, permissiveness);
        }

        if (profile.AvgTemperatureK > 0.0)
        {
            double temperaturePenalty = GetDeviationPenalty(profile.AvgTemperatureK, 288.0, 18.0, 90.0);
            probability -= temperaturePenalty * Lerp(0.22, 0.08, permissiveness);
        }

        if (profile.GravityG > 0.0)
        {
            double gravityPenalty = GetDeviationPenalty(profile.GravityG, 1.0, 0.25, 1.25);
            probability -= gravityPenalty * Lerp(0.12, 0.04, permissiveness);
        }

        return System.Math.Clamp(probability, 0.0, 0.98);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted.
    /// </summary>
    public static double CalculateColonyProbability(PlanetProfile profile, ColonySuitability suitability)
    {
        return CalculateColonyProbability(profile, suitability, GenerationUseCaseSettings.NeutralPermissiveness);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted using a user-facing permissiveness value.
    /// </summary>
    public static double CalculateColonyProbability(
        PlanetProfile profile,
        ColonySuitability suitability,
        double populationPermissiveness)
    {
        double permissiveness = ClampPermissiveness(populationPermissiveness);
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
            probability += Lerp(0.10, 0.04, permissiveness);
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

        return System.Math.Clamp(probability, 0.0, 0.95);
    }

    private static bool CanSupportBiologyAtAll(PlanetProfile profile)
    {
        if (!profile.HasLiquidWater)
        {
            return false;
        }

        if (profile.HabitabilityScore < MinHabitabilityForNatives)
        {
            return false;
        }

        if (profile.RadiationLevel >= 0.95)
        {
            return false;
        }

        return true;
    }

    private static double GetDeviationPenalty(double value, double idealValue, double idealTolerance, double maxTolerance)
    {
        double deviation = System.Math.Abs(value - idealValue);
        if (deviation <= idealTolerance)
        {
            return 0.0;
        }

        if (deviation >= maxTolerance)
        {
            return 1.0;
        }

        return Normalize(deviation, idealTolerance, maxTolerance);
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
}
