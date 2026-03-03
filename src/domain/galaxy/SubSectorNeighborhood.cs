using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Builds a pure-data subsector neighborhood around a camera position.
/// </summary>
public static class SubSectorNeighborhood
{
    /// <summary>
    /// Extent of the neighborhood from the center in each direction.
    /// </summary>
    public const int Extent = 5;

    /// <summary>
    /// Total subsectors in the neighborhood.
    /// </summary>
    public const int TotalSubsectors = 1331;

    /// <summary>
    /// Builds the full neighborhood around a camera position.
    /// </summary>
    public static SubSectorNeighborhoodData Build(
        Vector3 cameraPosition,
        long galaxySeed,
        DensityModelInterface densityModel,
        float referenceDensity)
    {
        Vector3 centerOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(cameraPosition);
        float subsectorSize = (float)GalaxyCoordinates.SubsectorSizePc;
        List<Vector3> starPositions = new();
        List<long> starSeeds = new();
        List<int> starShells = new();
        List<Vector3> subsectorOrigins = new(TotalSubsectors);
        List<int> subsectorShells = new(TotalSubsectors);

        for (int dx = -Extent; dx <= Extent; dx += 1)
        {
            for (int dy = -Extent; dy <= Extent; dy += 1)
            {
                for (int dz = -Extent; dz <= Extent; dz += 1)
                {
                    int shell = GetChebyshevDistance(dx, dy, dz);
                    Vector3 offsetOrigin = centerOrigin + new Vector3(dx * subsectorSize, dy * subsectorSize, dz * subsectorSize);
                    subsectorOrigins.Add(offsetOrigin);
                    subsectorShells.Add(shell);

                    SectorStarData subsectorData = SubSectorGenerator.GenerateSingleSubsector(
                        galaxySeed,
                        offsetOrigin,
                        densityModel,
                        referenceDensity);

                    for (int index = 0; index < subsectorData.GetCount(); index += 1)
                    {
                        starPositions.Add(subsectorData.Positions[index]);
                        starSeeds.Add(subsectorData.StarSeeds[index]);
                        starShells.Add(shell);
                    }
                }
            }
        }

        return new SubSectorNeighborhoodData
        {
            StarPositions = starPositions.ToArray(),
            StarSeeds = starSeeds.ToArray(),
            StarShells = starShells.ToArray(),
            SubsectorOrigins = subsectorOrigins.ToArray(),
            SubsectorShells = subsectorShells.ToArray(),
            CenterOrigin = centerOrigin,
        };
    }

    /// <summary>
    /// Returns the current center subsector origin for the supplied camera position.
    /// </summary>
    public static Vector3 GetCenterOrigin(Vector3 cameraPosition)
    {
        return GalaxyCoordinates.GetSubsectorWorldOrigin(cameraPosition);
    }

    /// <summary>
    /// Computes the Chebyshev shell distance from the origin.
    /// </summary>
    private static int GetChebyshevDistance(int dx, int dy, int dz)
    {
        return System.Math.Max(System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)), System.Math.Abs(dz));
    }
}
