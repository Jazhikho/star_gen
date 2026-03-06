namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic, order-independent seed derivation for population generation.
/// </summary>
public static class PopulationSeeding
{
    private const ulong FnvOffset64 = 14695981039346656037UL;
    private const ulong FnvPrime64 = 1099511628211UL;
    private const int PopulationSalt = 0x504F5055;
    private const int NativeSalt = 0x4E415456;
    private const int ColonySalt = 0x434F4C4E;

    /// <summary>
    /// Derives a deterministic population seed from body identifier and base seed.
    /// </summary>
    public static long GeneratePopulationSeed(string bodyId, long baseSeed)
    {
        ulong hash = FnvOffset64;
        hash = FnvHashBytes(hash, System.Text.Encoding.UTF8.GetBytes(bodyId));
        hash = FnvHashLong(hash, baseSeed);
        hash = FnvHashLong(hash, PopulationSalt);
        return NormalizeSeed(hash);
    }

    /// <summary>
    /// Derives a native-population sub-seed.
    /// </summary>
    public static long GenerateNativeSeed(long populationSeed, int nativeIndex)
    {
        ulong hash = FnvOffset64;
        hash = FnvHashLong(hash, populationSeed);
        hash = FnvHashLong(hash, nativeIndex + 1);
        hash = FnvHashLong(hash, NativeSalt);
        return NormalizeSeed(hash);
    }

    /// <summary>
    /// Derives a colony sub-seed.
    /// </summary>
    public static long GenerateColonySeed(long populationSeed, int colonyIndex)
    {
        ulong hash = FnvOffset64;
        hash = FnvHashLong(hash, populationSeed);
        hash = FnvHashLong(hash, colonyIndex + 1);
        hash = FnvHashLong(hash, ColonySalt);
        return NormalizeSeed(hash);
    }

    private static ulong FnvHashLong(ulong hash, long value)
    {
        unchecked
        {
            ulong unsigned = (ulong)value;
            for (int index = 0; index < 8; index += 1)
            {
                byte byteValue = (byte)((unsigned >> (index * 8)) & 0xFF);
                hash ^= byteValue;
                hash *= FnvPrime64;
            }
        }

        return hash;
    }

    private static ulong FnvHashBytes(ulong hash, byte[] bytes)
    {
        unchecked
        {
            foreach (byte value in bytes)
            {
                hash ^= value;
                hash *= FnvPrime64;
            }
        }

        return hash;
    }

    private static long NormalizeSeed(ulong value)
    {
        return unchecked((long)(value & 0x7FFF_FFFF_FFFF_FFFFUL));
    }
}
