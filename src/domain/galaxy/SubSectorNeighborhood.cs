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
        return Build(
            cameraPosition,
            galaxySeed,
            densityModel,
            referenceDensity,
            new Vector3I(Extent, Extent, Extent));
    }

    /// <summary>
    /// Builds the full neighborhood around a camera position for custom subsector extents.
    /// </summary>
    public static SubSectorNeighborhoodData Build(
        Vector3 cameraPosition,
        long galaxySeed,
        DensityModelInterface densityModel,
        float referenceDensity,
        Vector3I extent)
    {
        Vector3 centerOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(cameraPosition);
        float subsectorSize = (float)GalaxyCoordinates.SubsectorSizePc;
        List<Vector3> starPositions = new();
        List<long> starSeeds = new();
        List<int> starShells = new();
        Vector3I clampedExtent = new(
            System.Math.Max(extent.X, 0),
            System.Math.Max(extent.Y, 0),
            System.Math.Max(extent.Z, 0));
        List<Vector3> subsectorOrigins = new(GetTotalSubsectors(clampedExtent));
        List<int> subsectorShells = new(GetTotalSubsectors(clampedExtent));

        for (int dx = -clampedExtent.X; dx <= clampedExtent.X; dx += 1)
        {
            for (int dy = -clampedExtent.Y; dy <= clampedExtent.Y; dy += 1)
            {
                for (int dz = -clampedExtent.Z; dz <= clampedExtent.Z; dz += 1)
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
            Extent = clampedExtent,
            StarPositions = starPositions.ToArray(),
            StarSeeds = starSeeds.ToArray(),
            StarShells = starShells.ToArray(),
            SubsectorOrigins = subsectorOrigins.ToArray(),
            SubsectorShells = subsectorShells.ToArray(),
            CenterOrigin = centerOrigin,
        };
    }

    /// <summary>
    /// Returns the total number of subsectors covered by the supplied extents.
    /// </summary>
    public static int GetTotalSubsectors(Vector3I extent)
    {
        int width = (System.Math.Max(extent.X, 0) * 2) + 1;
        int height = (System.Math.Max(extent.Y, 0) * 2) + 1;
        int depth = (System.Math.Max(extent.Z, 0) * 2) + 1;
        return width * height * depth;
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
