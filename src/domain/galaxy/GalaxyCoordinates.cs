using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Conversions between parsec-space positions and hierarchical grid coordinates.
/// </summary>
public static partial class GalaxyCoordinates
{
    /// <summary>
    /// Hierarchical zoom levels for the galaxy viewer.
    /// </summary>
    public enum ZoomLevel
    {
        Galaxy,
        Quadrant,
        Sector,
        Subsector,
        SubSector = Subsector,
    }

    /// <summary>
    /// Legacy nested hierarchy-coordinates type alias.
    /// </summary>
    public partial class HierarchyCoords : global::StarGen.Domain.Galaxy.HierarchyCoords
    {
    }

    /// <summary>
    /// Size of one quadrant edge in parsecs.
    /// </summary>
    public const double QuadrantSizePc = 1000.0;

    /// <summary>
    /// Size of one sector edge in parsecs.
    /// </summary>
    public const double SectorSizePc = 100.0;

    /// <summary>
    /// Size of one subsector edge in parsecs.
    /// </summary>
    public const double SubsectorSizePc = 10.0;

    /// <summary>
    /// Number of standard deviations to include for bulge extent.
    /// </summary>
    public const double BulgeSigmaCoverage = 3.0;

    /// <summary>
    /// Converts a parsec-space position to quadrant grid coordinates.
    /// </summary>
    public static Vector3I ParsecToQuadrant(Vector3 position)
    {
        return new Vector3I(
            FloorToInt(position.X / QuadrantSizePc),
            FloorToInt(position.Y / QuadrantSizePc),
            FloorToInt(position.Z / QuadrantSizePc));
    }

    /// <summary>
    /// Converts quadrant grid coordinates to the parsec-space center of that cell.
    /// </summary>
    public static Vector3 QuadrantToParsecCenter(Vector3I coords)
    {
        double half = QuadrantSizePc * 0.5;
        return new Vector3(
            (float)((coords.X * QuadrantSizePc) + half),
            (float)((coords.Y * QuadrantSizePc) + half),
            (float)((coords.Z * QuadrantSizePc) + half));
    }

    /// <summary>
    /// Computes the effective radial extent of the galaxy in parsecs.
    /// </summary>
    public static double GetEffectiveRadius(GalaxySpec spec)
    {
        double bulgeRadialExtent = spec.BulgeRadiusPc * BulgeSigmaCoverage;
        return System.Math.Max(spec.RadiusPc, bulgeRadialExtent);
    }

    /// <summary>
    /// Computes the effective half-height of the galaxy in parsecs.
    /// </summary>
    public static double GetEffectiveHalfHeight(GalaxySpec spec)
    {
        switch (spec.Type)
        {
            case GalaxySpec.GalaxyType.Spiral:
            {
                double bulgeVerticalExtent = spec.BulgeHeightPc * BulgeSigmaCoverage;
                return System.Math.Max(spec.HeightPc, bulgeVerticalExtent);
            }
            case GalaxySpec.GalaxyType.Elliptical:
            {
                double axisRatio = System.Math.Max(1.0 - spec.Ellipticity, 0.3);
                double sigmaMajor = spec.RadiusPc * 0.35;
                double sigmaMinor = sigmaMajor * axisRatio;
                return sigmaMinor * BulgeSigmaCoverage;
            }
            case GalaxySpec.GalaxyType.Irregular:
                return spec.RadiusPc * 0.8;
            default:
            {
                double bulgeVerticalExtent = spec.BulgeHeightPc * BulgeSigmaCoverage;
                return System.Math.Max(spec.HeightPc, bulgeVerticalExtent);
            }
        }
    }

    /// <summary>
    /// Returns the minimum quadrant grid coordinates that cover the galaxy bounds.
    /// </summary>
    public static Vector3I GetQuadrantGridMin(GalaxySpec spec)
    {
        double effectiveRadius = GetEffectiveRadius(spec);
        double effectiveHalfHeight = GetEffectiveHalfHeight(spec);
        return ParsecToQuadrant(new Vector3(
            (float)(-effectiveRadius),
            (float)(-effectiveHalfHeight),
            (float)(-effectiveRadius)));
    }

    /// <summary>
    /// Returns the maximum quadrant grid coordinates that cover the galaxy bounds.
    /// </summary>
    public static Vector3I GetQuadrantGridMax(GalaxySpec spec)
    {
        double effectiveRadius = GetEffectiveRadius(spec);
        double effectiveHalfHeight = GetEffectiveHalfHeight(spec);
        return ParsecToQuadrant(new Vector3(
            (float)effectiveRadius,
            (float)effectiveHalfHeight,
            (float)effectiveRadius));
    }

    /// <summary>
    /// Returns whether a parsec position lies in a given quadrant.
    /// </summary>
    public static bool IsPositionInQuadrant(Vector3 position, Vector3I quadrantCoords)
    {
        return ParsecToQuadrant(position) == quadrantCoords;
    }

    /// <summary>
    /// Converts a world-space parsec position to full hierarchy coordinates.
    /// </summary>
    public static HierarchyCoords ParsecToHierarchy(Vector3 position)
    {
        HierarchyCoords result = new HierarchyCoords
        {
            QuadrantCoords = ParsecToQuadrant(position),
        };

        Vector3 quadrantOrigin = new(
            (float)(result.QuadrantCoords.X * QuadrantSizePc),
            (float)(result.QuadrantCoords.Y * QuadrantSizePc),
            (float)(result.QuadrantCoords.Z * QuadrantSizePc));
        Vector3 localInQuadrant = position - quadrantOrigin;

        result.SectorLocalCoords = new Vector3I(
            Clamp(FloorToInt(localInQuadrant.X / SectorSizePc), 0, 9),
            Clamp(FloorToInt(localInQuadrant.Y / SectorSizePc), 0, 9),
            Clamp(FloorToInt(localInQuadrant.Z / SectorSizePc), 0, 9));

        Vector3 sectorOrigin = new(
            (float)(result.SectorLocalCoords.X * SectorSizePc),
            (float)(result.SectorLocalCoords.Y * SectorSizePc),
            (float)(result.SectorLocalCoords.Z * SectorSizePc));
        Vector3 localInSector = localInQuadrant - sectorOrigin;

        result.SubsectorLocalCoords = new Vector3I(
            Clamp(FloorToInt(localInSector.X / SubsectorSizePc), 0, 9),
            Clamp(FloorToInt(localInSector.Y / SubsectorSizePc), 0, 9),
            Clamp(FloorToInt(localInSector.Z / SubsectorSizePc), 0, 9));

        return result;
    }

    /// <summary>
    /// Computes the world-space origin of a subsector from a reference sector origin.
    /// </summary>
    public static Vector3 SubsectorOffsetToWorld(Vector3 refSectorOrigin, Vector3I subsectorOffset)
    {
        return refSectorOrigin + new Vector3(
            (float)(subsectorOffset.X * SubsectorSizePc),
            (float)(subsectorOffset.Y * SubsectorSizePc),
            (float)(subsectorOffset.Z * SubsectorSizePc));
    }

    /// <summary>
    /// Computes the world-space origin of a sector.
    /// </summary>
    public static Vector3 SectorWorldOrigin(Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        return new Vector3(
            (float)((quadrantCoords.X * QuadrantSizePc) + (sectorLocalCoords.X * SectorSizePc)),
            (float)((quadrantCoords.Y * QuadrantSizePc) + (sectorLocalCoords.Y * SectorSizePc)),
            (float)((quadrantCoords.Z * QuadrantSizePc) + (sectorLocalCoords.Z * SectorSizePc)));
    }

    /// <summary>
    /// Computes the world-space origin of the subsector containing a position.
    /// </summary>
    public static Vector3 GetSubsectorWorldOrigin(Vector3 position)
    {
        HierarchyCoords hierarchy = ParsecToHierarchy(position);
        return SectorWorldOrigin(hierarchy.QuadrantCoords, hierarchy.SectorLocalCoords) + new Vector3(
            (float)(hierarchy.SubsectorLocalCoords.X * SubsectorSizePc),
            (float)(hierarchy.SubsectorLocalCoords.Y * SubsectorSizePc),
            (float)(hierarchy.SubsectorLocalCoords.Z * SubsectorSizePc));
    }

    /// <summary>
    /// Computes the world-space center of the subsector containing a position.
    /// </summary>
    public static Vector3 GetSubsectorWorldCenter(Vector3 position)
    {
        return GetSubsectorWorldOrigin(position) + (Vector3.One * (float)(SubsectorSizePc * 0.5));
    }

    /// <summary>
    /// Floors a double and converts it to an integer.
    /// </summary>
    private static int FloorToInt(double value)
    {
        return (int)System.Math.Floor(value);
    }

    /// <summary>
    /// Clamps an integer between bounds.
    /// </summary>
    private static int Clamp(int value, int min, int max)
    {
        return System.Math.Max(min, System.Math.Min(max, value));
    }
}
