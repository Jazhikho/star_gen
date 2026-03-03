using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Constants and helpers for the default solar-neighborhood home position.
/// </summary>
public static class HomePosition
{
    /// <summary>
    /// Approximate solar radial distance from the galactic center in parsecs.
    /// </summary>
    public const float SolarDistancePc = 8000.0f;

    /// <summary>
    /// Approximate solar height above the galactic plane in parsecs.
    /// </summary>
    public const float SolarHeightPc = 20.0f;

    /// <summary>
    /// Default angular position in the galactic disk.
    /// </summary>
    public const float SolarAngleRad = 0.0f;

    /// <summary>
    /// Returns the default home position in world-space parsecs.
    /// </summary>
    public static Vector3 GetDefaultPosition()
    {
        float x = SolarDistancePc * Mathf.Cos(SolarAngleRad);
        float z = SolarDistancePc * Mathf.Sin(SolarAngleRad);
        return new Vector3(x, SolarHeightPc, z);
    }

    /// <summary>
    /// Returns the quadrant containing the default home position.
    /// </summary>
    public static Vector3I GetHomeQuadrant()
    {
        return GalaxyCoordinates.ParsecToQuadrant(GetDefaultPosition());
    }

    /// <summary>
    /// Returns the full hierarchy coordinates for the default home position.
    /// </summary>
    public static HierarchyCoords GetHomeHierarchy()
    {
        return GalaxyCoordinates.ParsecToHierarchy(GetDefaultPosition());
    }

    /// <summary>
    /// Returns the world-space origin of the home sector.
    /// </summary>
    public static Vector3 GetHomeSectorOrigin()
    {
        HierarchyCoords hierarchy = GetHomeHierarchy();
        return GalaxyCoordinates.SectorWorldOrigin(hierarchy.QuadrantCoords, hierarchy.SectorLocalCoords);
    }

    /// <summary>
    /// Returns the world-space center of the home sector.
    /// </summary>
    public static Vector3 GetHomeSectorCenter()
    {
        return GetHomeSectorOrigin() + (Vector3.One * (float)(GalaxyCoordinates.SectorSizePc * 0.5));
    }

    /// <summary>
    /// Returns the world-space center of the home subsector.
    /// </summary>
    public static Vector3 GetHomeSubsectorCenter()
    {
        HierarchyCoords hierarchy = GetHomeHierarchy();
        Vector3 sectorOrigin = GalaxyCoordinates.SectorWorldOrigin(hierarchy.QuadrantCoords, hierarchy.SectorLocalCoords);
        Vector3 subsectorOrigin = sectorOrigin + new Vector3(
            (float)(hierarchy.SubsectorLocalCoords.X * GalaxyCoordinates.SubsectorSizePc),
            (float)(hierarchy.SubsectorLocalCoords.Y * GalaxyCoordinates.SubsectorSizePc),
            (float)(hierarchy.SubsectorLocalCoords.Z * GalaxyCoordinates.SubsectorSizePc));
        return subsectorOrigin + (Vector3.One * (float)(GalaxyCoordinates.SubsectorSizePc * 0.5));
    }

    /// <summary>
    /// Returns whether a position lies within the nominal galaxy bounds.
    /// </summary>
    public static bool IsWithinGalaxy(Vector3 position, GalaxySpec spec)
    {
        float radialDistance = Mathf.Sqrt((position.X * position.X) + (position.Z * position.Z));
        float height = Mathf.Abs(position.Y);
        return radialDistance <= spec.RadiusPc && height <= spec.HeightPc;
    }
}
