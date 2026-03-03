namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic likelihood checks based on derived seed rolls.
/// </summary>
public static class PopulationLikelihood
{
    /// <summary>
    /// Population-generation override modes.
    /// </summary>
    public enum Override
    {
        Auto,
        None,
        ForceNatives,
        ForceColony,
    }

    /// <summary>
    /// Salt used for native roll derivation.
    /// </summary>
    public const int NativeRollSalt = 0x4E415449;

    /// <summary>
    /// Salt used for colony roll derivation.
    /// </summary>
    public const int ColonyRollSalt = 0x434F4C4F;

    private const double RollDenominator = 2147483648.0;

    /// <summary>
    /// Estimates the native-life likelihood.
    /// </summary>
    public static double EstimateNativeLikelihood(PlanetProfile profile)
    {
        return PopulationProbability.CalculateNativeProbability(profile);
    }

    /// <summary>
    /// Estimates the colony likelihood.
    /// </summary>
    public static double EstimateColonyLikelihood(PlanetProfile profile, ColonySuitability suitability)
    {
        return PopulationProbability.CalculateColonyProbability(profile, suitability);
    }

    /// <summary>
    /// Derives a deterministic roll in the range [0, 1).
    /// </summary>
    public static double DeriveRollValue(long populationSeed, int salt)
    {
        long mixed = MixSeed(populationSeed, salt);
        long positive = System.Math.Abs(mixed);
        double normalized = (positive % 0x7FFFFFFF) / RollDenominator;
        return System.Math.Clamp(normalized, 0.0, 0.9999999);
    }

    /// <summary>
    /// Returns whether natives should be generated.
    /// </summary>
    public static bool ShouldGenerateNatives(PlanetProfile profile, long populationSeed)
    {
        double likelihood = EstimateNativeLikelihood(profile);
        if (likelihood <= 0.0)
        {
            return false;
        }

        double roll = DeriveRollValue(populationSeed, NativeRollSalt);
        return roll < likelihood;
    }

    /// <summary>
    /// Returns whether a colony should be generated.
    /// </summary>
    public static bool ShouldGenerateColony(
        PlanetProfile profile,
        ColonySuitability suitability,
        long populationSeed)
    {
        double likelihood = EstimateColonyLikelihood(profile, suitability);
        if (likelihood <= 0.0)
        {
            return false;
        }

        double roll = DeriveRollValue(populationSeed, ColonyRollSalt);
        return roll < likelihood;
    }

    private static long MixSeed(long seedValue, long saltValue)
    {
        const long primeA = 2654435761;
        const long primeB = 2246822519;

        long hash = (seedValue ^ saltValue) * primeA;
        hash ^= hash >> 16;
        hash *= primeB;
        hash ^= hash >> 13;
        return hash;
    }
}
