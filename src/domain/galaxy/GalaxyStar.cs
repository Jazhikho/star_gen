using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Represents a star-system entry in the galaxy.
/// </summary>
public partial class GalaxyStar : RefCounted
{
    /// <summary>
    /// World-space position of this star in parsecs.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Deterministic seed for generating this star's system.
    /// </summary>
    public int StarSeed { get; set; }

    /// <summary>
    /// Galactic metallicity modifier retained for compatibility with existing downstream callers.
    /// </summary>
    public double Metallicity { get; set; } = 1.0;

    /// <summary>
    /// Legacy age-bias factor retained for compatibility with existing downstream callers.
    /// </summary>
    public double AgeBias { get; set; } = 1.0;

    /// <summary>
    /// Structured galaxy-origin context used by downstream star and system generation.
    /// </summary>
    public GalaxyOriginContext OriginContext { get; set; } = new GalaxyOriginContext();

    /// <summary>
    /// Parent sector quadrant coordinates.
    /// </summary>
    public Vector3I SectorQuadrant { get; set; } = Vector3I.Zero;

    /// <summary>
    /// Parent sector local coordinates.
    /// </summary>
    public Vector3I SectorLocal { get; set; } = Vector3I.Zero;

    /// <summary>
    /// Subsector local coordinates.
    /// </summary>
    public Vector3I SubsectorCoords { get; set; } = Vector3I.Zero;

    /// <summary>
    /// Creates a new galaxy-star entry.
    /// </summary>
    public GalaxyStar(Vector3 position, int seed)
    {
        Position = position;
        StarSeed = seed;
    }

    /// <summary>
    /// Creates a star with metallicity and galaxy-origin context derived from galactic position.
    /// </summary>
    public static GalaxyStar CreateWithDerivedProperties(Vector3 position, int seed, GalaxySpec galaxySpec)
    {
        GalaxyStar star = new GalaxyStar(position, seed);
        star.DerivePropertiesFromPosition(galaxySpec);
        return star;
    }

    /// <summary>
    /// Creates a detached copy of this star for snapshot-style callers.
    /// </summary>
    public GalaxyStar Clone()
    {
        return new GalaxyStar(Position, StarSeed)
        {
            Metallicity = Metallicity,
            AgeBias = AgeBias,
            OriginContext = OriginContext.Clone(),
            SectorQuadrant = SectorQuadrant,
            SectorLocal = SectorLocal,
            SubsectorCoords = SubsectorCoords,
        };
    }

    /// <summary>
    /// Derives metallicity and downstream galaxy context from galactic position.
    /// </summary>
    public void DerivePropertiesFromPosition(GalaxySpec galaxySpec)
    {
        OriginContext = GalaxyScientificFieldEvaluator.Evaluate(Position, galaxySpec);
        Metallicity = OriginContext.MetallicityPrior;
        AgeBias = OriginContext.AgeBias;
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
    /// Returns a concise diagnostic string.
    /// </summary>
    public override string ToString()
    {
        return $"GalaxyStar(seed={StarSeed}, pos={Position}, subtype={OriginContext.ResolvedSubtype}, [Fe/H]={Metallicity:0.###}, age_bias={AgeBias:0.###})";
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
            ["origin_context"] = OriginContext.ToDictionary(),
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

        GalaxyStar star = new GalaxyStar((Vector3)data["position"], DomainDictionaryUtils.GetInt(data, "star_seed", 0));
        star.Metallicity = DomainDictionaryUtils.GetDouble(data, "metallicity", 1.0);
        star.AgeBias = DomainDictionaryUtils.GetDouble(data, "age_bias", 1.0);
        star.SectorQuadrant = GetVector3I(data, "sector_quadrant", Vector3I.Zero);
        star.SectorLocal = GetVector3I(data, "sector_local", Vector3I.Zero);
        star.SubsectorCoords = GetVector3I(data, "subsector_coords", Vector3I.Zero);

        if (data.ContainsKey("origin_context") && data["origin_context"].VariantType == Variant.Type.Dictionary)
        {
            star.OriginContext = GalaxyOriginContext.FromDictionary((Dictionary)data["origin_context"]);
        }
        else
        {
            star.OriginContext = new GalaxyOriginContext
            {
                MetallicityPrior = star.Metallicity,
                AgeBias = star.AgeBias,
            };
        }

        return star;
    }

    /// <summary>
    /// Reads a Vector3I value from a dictionary.
    /// </summary>
    private static Vector3I GetVector3I(Dictionary data, string key, Vector3I fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Vector3I)
        {
            return (Vector3I)data[key];
        }

        return fallback;
    }
}
