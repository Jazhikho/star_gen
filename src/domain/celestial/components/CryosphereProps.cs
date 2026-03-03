using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Cryosphere properties for bodies with significant ice features.
/// </summary>
public partial class CryosphereProps : RefCounted
{
    /// <summary>
    /// Fraction of surface covered by polar ice caps.
    /// </summary>
    public double PolarCapCoverage;

    /// <summary>
    /// Depth of permafrost layer in meters.
    /// </summary>
    public double PermafrostDepthM;

    /// <summary>
    /// Whether a subsurface liquid ocean exists.
    /// </summary>
    public bool HasSubsurfaceOcean;

    /// <summary>
    /// Subsurface ocean depth in meters.
    /// </summary>
    public double SubsurfaceOceanDepthM;

    /// <summary>
    /// Cryovolcanism activity level.
    /// </summary>
    public double CryovolcanismLevel;

    /// <summary>
    /// Ice composition type identifier.
    /// </summary>
    public string IceType;

    /// <summary>
    /// Creates a new cryosphere-properties component.
    /// </summary>
    public CryosphereProps(
        double polarCapCoverage = 0.0,
        double permafrostDepthM = 0.0,
        bool hasSubsurfaceOcean = false,
        double subsurfaceOceanDepthM = 0.0,
        double cryovolcanismLevel = 0.0,
        string iceType = "water_ice")
    {
        PolarCapCoverage = polarCapCoverage;
        PermafrostDepthM = permafrostDepthM;
        HasSubsurfaceOcean = hasSubsurfaceOcean;
        SubsurfaceOceanDepthM = subsurfaceOceanDepthM;
        CryovolcanismLevel = cryovolcanismLevel;
        IceType = iceType;
    }

    /// <summary>
    /// Returns whether the body has significant ice features.
    /// </summary>
    public bool HasSignificantIce() => PolarCapCoverage > 0.1 || PermafrostDepthM > 0.0;

    /// <summary>
    /// Returns whether cryovolcanism is active.
    /// </summary>
    public bool IsCryovolcanicallyActive() => CryovolcanismLevel > 0.1;

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["polar_cap_coverage"] = PolarCapCoverage,
            ["permafrost_depth_m"] = PermafrostDepthM,
            ["has_subsurface_ocean"] = HasSubsurfaceOcean,
            ["subsurface_ocean_depth_m"] = SubsurfaceOceanDepthM,
            ["cryovolcanism_level"] = CryovolcanismLevel,
            ["ice_type"] = IceType,
        };
    }

    /// <summary>
    /// Creates a cryosphere-properties component from a dictionary.
    /// </summary>
    public static CryosphereProps FromDictionary(Dictionary data)
    {
        return new CryosphereProps(
            GetDouble(data, "polar_cap_coverage", 0.0),
            GetDouble(data, "permafrost_depth_m", 0.0),
            GetBool(data, "has_subsurface_ocean", false),
            GetDouble(data, "subsurface_ocean_depth_m", 0.0),
            GetDouble(data, "cryovolcanism_level", 0.0),
            GetString(data, "ice_type", "water_ice"));
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) ? (bool)data[key] : fallback;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        return data.ContainsKey(key) ? (string)data[key] : fallback;
    }
}
