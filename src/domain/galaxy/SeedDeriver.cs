using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Deterministic seed derivation for the galaxy hierarchy.
/// </summary>
public static class SeedDeriver
{
    /// <summary>
    /// Derives a quadrant seed from the galaxy seed and quadrant coordinates.
    /// </summary>
    public static long DeriveQuadrantSeed(long galaxySeed, Vector3I quadrantCoords)
    {
        return StableHash.DeriveSeed(galaxySeed, quadrantCoords);
    }

    /// <summary>
    /// Derives a sector seed from a quadrant seed and sector-local coordinates.
    /// </summary>
    public static long DeriveSectorSeed(long quadrantSeed, Vector3I sectorLocalCoords)
    {
        return StableHash.DeriveSeed(quadrantSeed, sectorLocalCoords);
    }

    /// <summary>
    /// Derives a subsector seed from a sector seed and subsector-local coordinates.
    /// </summary>
    public static long DeriveSubsectorSeed(long sectorSeed, Vector3I subsectorLocalCoords)
    {
        return StableHash.DeriveSeed(sectorSeed, subsectorLocalCoords);
    }

    /// <summary>
    /// Derives a star seed from a subsector seed and star index.
    /// </summary>
    public static long DeriveStarSeed(long subsectorSeed, long starIndex)
    {
        return StableHash.DeriveSeedIndexed(subsectorSeed, starIndex);
    }

    /// <summary>
    /// Derives a sector seed directly from the galaxy seed and hierarchy coordinates.
    /// </summary>
    public static long DeriveSectorSeedFull(long galaxySeed, Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        long quadrantSeed = DeriveQuadrantSeed(galaxySeed, quadrantCoords);
        return DeriveSectorSeed(quadrantSeed, sectorLocalCoords);
    }

    /// <summary>
    /// Derives a subsector seed directly from the galaxy seed and hierarchy coordinates.
    /// </summary>
    public static long DeriveSubsectorSeedFull(
        long galaxySeed,
        Vector3I quadrantCoords,
        Vector3I sectorLocalCoords,
        Vector3I subsectorLocalCoords)
    {
        long sectorSeed = DeriveSectorSeedFull(galaxySeed, quadrantCoords, sectorLocalCoords);
        return DeriveSubsectorSeed(sectorSeed, subsectorLocalCoords);
    }
}
