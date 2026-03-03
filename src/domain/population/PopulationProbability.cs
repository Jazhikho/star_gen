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
        if (profile.HabitabilityScore < MinHabitabilityForNatives)
        {
            return 0.0;
        }

        double probability = profile.HabitabilityScore * NativeProbabilityScale;
        if (profile.HasLiquidWater)
        {
            probability += LiquidWaterBonus;
        }

        if (profile.HasBreathableAtmosphere)
        {
            probability += BreathableAtmosphereBonus;
        }

        if (profile.IsMoon && profile.TidalHeatingFactor > 0.3)
        {
            probability += TidalHeatingBonus;
        }

        if (profile.IsTidallyLocked)
        {
            probability -= TidalLockingPenalty;
        }

        if (profile.RadiationLevel > 0.7)
        {
            probability -= 0.15;
        }

        return System.Math.Clamp(probability, 0.0, 0.95);
    }

    /// <summary>
    /// Calculates the probability that colonization is attempted.
    /// </summary>
    public static double CalculateColonyProbability(PlanetProfile profile, ColonySuitability suitability)
    {
        if (profile.HabitabilityScore < MinHabitabilityForColony)
        {
            return 0.0;
        }

        if (suitability.OverallScore < MinSuitabilityForColony)
        {
            return 0.0;
        }

        double scoreFactor = suitability.OverallScore / 50.0;
        double probability = ColonyBaseProbability * scoreFactor;
        return System.Math.Clamp(probability, 0.0, 0.90);
    }
}
