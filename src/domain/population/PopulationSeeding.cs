namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic, order-independent seed derivation for population generation.
/// </summary>
public static class PopulationSeeding
{
    private const long HashPrimeA = 2654435761;
    private const long HashPrimeB = 2246822519;
    private const int PopulationSalt = 0x504F5055;

    /// <summary>
    /// Derives a deterministic population seed from body identifier and base seed.
    /// </summary>
    public static long GeneratePopulationSeed(string bodyId, long baseSeed)
    {
        long idHash = HashString(bodyId);
        long combined = idHash ^ baseSeed;
        combined = Mix(combined, PopulationSalt);
        return System.Math.Abs(combined);
    }

    /// <summary>
    /// Derives a native-population sub-seed.
    /// </summary>
    public static long GenerateNativeSeed(long populationSeed, int nativeIndex)
    {
        return System.Math.Abs(Mix(populationSeed, nativeIndex + 1));
    }

    /// <summary>
    /// Derives a colony sub-seed.
    /// </summary>
    public static long GenerateColonySeed(long populationSeed, int colonyIndex)
    {
        return System.Math.Abs(Mix(populationSeed, (colonyIndex + 1L) * 1000003L));
    }

    private static long HashString(string value)
    {
        long hash = 0x811C9DC5;
        foreach (char character in value)
        {
            hash ^= character;
            hash *= 0x01000193;
        }

        return hash;
    }

    private static long Mix(long a, long b)
    {
        long result = a ^ (b * HashPrimeA);
        result ^= result >> 16;
        result *= HashPrimeB;
        result ^= result >> 13;
        return result;
    }
}
