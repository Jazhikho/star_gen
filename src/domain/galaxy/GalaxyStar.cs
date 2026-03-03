using Godot;
using Godot.Collections;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Represents a star-system entry in the galaxy.
/// </summary>
public partial class GalaxyStar : RefCounted
{
    /// <summary>
    /// World-space position of this star in parsecs.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Deterministic seed for generating this star's system.
    /// </summary>
    public int StarSeed;

    /// <summary>
    /// Galactic metallicity modifier.
    /// </summary>
    public double Metallicity = 1.0;

    /// <summary>
    /// Age bias factor.
    /// </summary>
    public double AgeBias = 1.0;

    /// <summary>
    /// Parent sector quadrant coordinates.
    /// </summary>
    public Vector3I SectorQuadrant = Vector3I.Zero;

    /// <summary>
    /// Parent sector local coordinates.
    /// </summary>
    public Vector3I SectorLocal = Vector3I.Zero;

    /// <summary>
    /// Subsector local coordinates.
    /// </summary>
    public Vector3I SubsectorCoords = Vector3I.Zero;

    /// <summary>
    /// Creates a new galaxy-star entry.
    /// </summary>
    public GalaxyStar(Vector3 position, int seed)
    {
        Position = position;
        StarSeed = seed;
    }

    /// <summary>
    /// Creates a star with metallicity and age derived from galactic position.
    /// </summary>
    public static GalaxyStar CreateWithDerivedProperties(Vector3 position, int seed, GalaxySpec galaxySpec)
    {
        GalaxyStar star = new(position, seed);
        star.DerivePropertiesFromPosition(galaxySpec);
        return star;
    }

    /// <summary>
    /// Derives metallicity and age bias from galactic position.
    /// </summary>
    public void DerivePropertiesFromPosition(GalaxySpec galaxySpec)
    {
        double radialDistance = System.Math.Sqrt((Position.X * Position.X) + (Position.Z * Position.Z));
        double height = System.Math.Abs(Position.Y);
        double normalizedRadius = galaxySpec.DiskScaleLengthPc > 0.0 ? radialDistance / galaxySpec.DiskScaleLengthPc : 0.0;
        Metallicity = CalculateMetallicity(normalizedRadius, height, galaxySpec);
        AgeBias = CalculateAgeBias(normalizedRadius, height, galaxySpec);
    }

    /// <summary>
    /// Returns the distance from galactic center in parsecs.
    /// </summary>
    public double GetDistanceFromCenter()
    {
        return Position.Length();
    }

    /// <summary>
    /// Returns the radial distance in the galactic plane.
    /// </summary>
    public double GetRadialDistance()
    {
        return System.Math.Sqrt((Position.X * Position.X) + (Position.Z * Position.Z));
    }

    /// <summary>
    /// Returns the height above or below the galactic plane.
    /// </summary>
    public double GetHeight()
    {
        return Position.Y;
    }

    /// <summary>
    /// Converts the star to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["position"] = Position,
            ["star_seed"] = StarSeed,
            ["metallicity"] = Metallicity,
            ["age_bias"] = AgeBias,
            ["sector_quadrant"] = SectorQuadrant,
            ["sector_local"] = SectorLocal,
            ["subsector_coords"] = SubsectorCoords,
        };
    }

    /// <summary>
    /// Rebuilds a star from a dictionary payload.
    /// </summary>
    public static GalaxyStar? FromDictionary(Dictionary data)
    {
        if (!data.ContainsKey("position") || data["position"].VariantType != Variant.Type.Vector3)
        {
            return null;
        }

        GalaxyStar star = new((Vector3)data["position"], GetInt(data, "star_seed", 0))
        {
            Metallicity = GetDouble(data, "metallicity", 1.0),
            AgeBias = GetDouble(data, "age_bias", 1.0),
            SectorQuadrant = GetVector3I(data, "sector_quadrant", Vector3I.Zero),
            SectorLocal = GetVector3I(data, "sector_local", Vector3I.Zero),
            SubsectorCoords = GetVector3I(data, "subsector_coords", Vector3I.Zero),
        };
        return star;
    }

    /// <summary>
    /// Calculates metallicity based on galactic position.
    /// </summary>
    private double CalculateMetallicity(double normalizedRadius, double height, GalaxySpec galaxySpec)
    {
        double radialFactor = System.Math.Exp(-0.3 * normalizedRadius) + 0.3;
        double normalizedHeight = galaxySpec.BulgeHeightPc > 0.0 ? height / galaxySpec.BulgeHeightPc : 0.0;
        double verticalFactor = System.Math.Exp(-0.5 * normalizedHeight);
        double rawMetallicity = radialFactor * verticalFactor * 1.2;
        return System.Math.Clamp(rawMetallicity, 0.1, 3.0);
    }

    /// <summary>
    /// Calculates age bias based on galactic position.
    /// </summary>
    private double CalculateAgeBias(double normalizedRadius, double height, GalaxySpec galaxySpec)
    {
        double bulgeRadiusSq = galaxySpec.BulgeRadiusPc * galaxySpec.BulgeRadiusPc;
        double bulgeHeightSq = galaxySpec.BulgeHeightPc * galaxySpec.BulgeHeightPc;
        double bulgeDistance = bulgeRadiusSq > 0.0 && bulgeHeightSq > 0.0
            ? System.Math.Sqrt(
                (((Position.X * Position.X) + (Position.Z * Position.Z)) / bulgeRadiusSq) +
                ((Position.Y * Position.Y) / bulgeHeightSq))
            : 0.0;
        double bulgeFactor = System.Math.Exp(-bulgeDistance) * 0.5;
        double normalizedHeight = galaxySpec.BulgeHeightPc > 0.0 ? height / galaxySpec.BulgeHeightPc : 0.0;
        double haloFactor = 0.3 * (1.0 - System.Math.Exp(-normalizedHeight));
        double diskFactor = -0.2 * (1.0 - System.Math.Exp(-normalizedRadius * 0.5));
        return System.Math.Clamp(1.0 + bulgeFactor + haloFactor + diskFactor, 0.5, 2.0);
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a Vector3I value from a dictionary.
    /// </summary>
    private static Vector3I GetVector3I(Dictionary data, string key, Vector3I fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Vector3I ? (Vector3I)data[key] : fallback;
    }
}
