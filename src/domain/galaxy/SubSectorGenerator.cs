using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Generates deterministic star-system positions within sectors and subsectors.
/// </summary>
public static class SubSectorGenerator
{
    /// <summary>
    /// Average real-world star-system density in the solar neighborhood.
    /// </summary>
    public const float SolarNeighborhoodDensity = 0.004f;

    /// <summary>
    /// Volume of one subsector in cubic parsecs.
    /// </summary>
    public const float SubsectorVolumePc3 = 1000.0f;

    /// <summary>
    /// Expected systems per subsector at solar-neighborhood density.
    /// </summary>
    public const float ExpectedSystemsAtSolar = SolarNeighborhoodDensity * SubsectorVolumePc3;

    /// <summary>
    /// Number of subsectors along each sector edge.
    /// </summary>
    public const int SubsectorsPerEdge = 10;

    /// <summary>
    /// One-subsector border shell for neighborhood sampling.
    /// </summary>
    public const int BorderExtent = 1;

    /// <summary>
    /// Generates stars for a single sector.
    /// </summary>
    public static SectorStarData GenerateSectorStars(
        long galaxySeed,
        Vector3I quadrantCoords,
        Vector3I sectorLocalCoords,
        DensityModelInterface densityModel,
        float referenceDensity)
    {
        SectorStarData result = new();
        if (referenceDensity <= 0.0f)
        {
            return result;
        }

        List<Vector3> positions = new();
        List<long> starSeeds = new();
        Vector3 sectorOrigin = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorLocalCoords);
        long sectorSeed = SeedDeriver.DeriveSectorSeedFull(galaxySeed, quadrantCoords, sectorLocalCoords);

        for (int x = 0; x < SubsectorsPerEdge; x += 1)
        {
            for (int y = 0; y < SubsectorsPerEdge; y += 1)
            {
                for (int z = 0; z < SubsectorsPerEdge; z += 1)
                {
                    Vector3I subsectorLocal = new(x, y, z);
                    Vector3 subsectorOrigin = GalaxyCoordinates.SubsectorOffsetToWorld(sectorOrigin, subsectorLocal);
                    GenerateSubsectorStars(
                        sectorSeed,
                        subsectorLocal,
                        subsectorOrigin,
                        densityModel,
                        referenceDensity,
                        positions,
                        starSeeds);
                }
            }
        }

        result.Positions = positions.ToArray();
        result.StarSeeds = starSeeds.ToArray();
        return result;
    }

    /// <summary>
    /// Generates stars for a sector plus a one-subsector border shell.
    /// </summary>
    public static SectorStarData GenerateSectorWithBorder(
        long galaxySeed,
        Vector3I quadrantCoords,
        Vector3I sectorLocalCoords,
        DensityModelInterface densityModel,
        float referenceDensity)
    {
        SectorStarData result = new();
        if (referenceDensity <= 0.0f)
        {
            return result;
        }

        List<Vector3> positions = new();
        List<long> starSeeds = new();
        Vector3 sectorOrigin = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorLocalCoords);
        int minIndex = -BorderExtent;
        int maxIndex = SubsectorsPerEdge + BorderExtent;

        for (int x = minIndex; x < maxIndex; x += 1)
        {
            for (int y = minIndex; y < maxIndex; y += 1)
            {
                for (int z = minIndex; z < maxIndex; z += 1)
                {
                    Vector3I subsectorOffset = new(x, y, z);
                    Vector3 worldOrigin = GalaxyCoordinates.SubsectorOffsetToWorld(sectorOrigin, subsectorOffset);
                    Vector3 center = worldOrigin + (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5));
                    HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(center);
                    long sectorSeed = SeedDeriver.DeriveSectorSeedFull(
                        galaxySeed,
                        hierarchy.QuadrantCoords,
                        hierarchy.SectorLocalCoords);

                    GenerateSubsectorStars(
                        sectorSeed,
                        hierarchy.SubsectorLocalCoords,
                        worldOrigin,
                        densityModel,
                        referenceDensity,
                        positions,
                        starSeeds);
                }
            }
        }

        result.Positions = positions.ToArray();
        result.StarSeeds = starSeeds.ToArray();
        return result;
    }

    /// <summary>
    /// Generates stars for a single subsector identified by world origin.
    /// </summary>
    public static SectorStarData GenerateSingleSubsector(
        long galaxySeed,
        Vector3 worldOrigin,
        DensityModelInterface densityModel,
        float referenceDensity)
    {
        SectorStarData result = new();
        if (referenceDensity <= 0.0f)
        {
            return result;
        }

        List<Vector3> positions = new();
        List<long> starSeeds = new();
        Vector3 center = worldOrigin + (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5));
        HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(center);
        long sectorSeed = SeedDeriver.DeriveSectorSeedFull(galaxySeed, hierarchy.QuadrantCoords, hierarchy.SectorLocalCoords);
        GenerateSubsectorStars(
            sectorSeed,
            hierarchy.SubsectorLocalCoords,
            worldOrigin,
            densityModel,
            referenceDensity,
            positions,
            starSeeds);
        result.Positions = positions.ToArray();
        result.StarSeeds = starSeeds.ToArray();
        return result;
    }

    /// <summary>
    /// Generates star positions and star seeds for a single subsector.
    /// </summary>
    private static void GenerateSubsectorStars(
        long sectorSeed,
        Vector3I subsectorLocalCoords,
        Vector3 subsectorOrigin,
        DensityModelInterface densityModel,
        float referenceDensity,
        List<Vector3> outPositions,
        List<long> outSeeds)
    {
        long subsectorSeed = SeedDeriver.DeriveSubsectorSeed(sectorSeed, subsectorLocalCoords);
        RandomNumberGenerator rng = new()
        {
            Seed = unchecked((ulong)subsectorSeed),
        };

        Vector3 center = subsectorOrigin + (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5));
        float localDensity = densityModel.GetDensity(center);
        float densityRatio = Mathf.Clamp(localDensity / referenceDensity, 0.0f, 10.0f);
        float expectedStars = densityRatio * ExpectedSystemsAtSolar;
        int starCount = SamplePoisson(expectedStars, rng);

        for (int index = 0; index < starCount; index += 1)
        {
            long starSeed = SeedDeriver.DeriveStarSeed(subsectorSeed, index);
            Vector3 position = subsectorOrigin + new Vector3(
                rng.Randf() * (float)GalaxyCoordinates.SubsectorSizePc,
                rng.Randf() * (float)GalaxyCoordinates.SubsectorSizePc,
                rng.Randf() * (float)GalaxyCoordinates.SubsectorSizePc);
            outPositions.Add(position);
            outSeeds.Add(starSeed);
        }
    }

    /// <summary>
    /// Samples a Poisson-distributed count using inverse-transform sampling.
    /// </summary>
    private static int SamplePoisson(float lambdaValue, RandomNumberGenerator rng)
    {
        if (lambdaValue <= 0.0f)
        {
            return 0;
        }

        float threshold = Mathf.Exp(-lambdaValue);
        int k = 0;
        float product = 1.0f;

        while (true)
        {
            k += 1;
            product *= rng.Randf();
            if (product <= threshold)
            {
                break;
            }
        }

        return k - 1;
    }
}
