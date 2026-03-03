using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Hydrosphere properties for bodies with liquid water.
/// </summary>
public partial class HydrosphereProps : RefCounted
{
    /// <summary>
    /// Fraction of surface covered by liquid water.
    /// </summary>
    public double OceanCoverage;

    /// <summary>
    /// Average ocean depth in meters.
    /// </summary>
    public double OceanDepthM;

    /// <summary>
    /// Fraction of water surface covered by ice.
    /// </summary>
    public double IceCoverage;

    /// <summary>
    /// Ocean salinity in parts per thousand.
    /// </summary>
    public double SalinityPpt;

    /// <summary>
    /// Water composition type identifier.
    /// </summary>
    public string WaterType;

    /// <summary>
    /// Creates a new hydrosphere-properties component.
    /// </summary>
    public HydrosphereProps(
        double oceanCoverage = 0.0,
        double oceanDepthM = 0.0,
        double iceCoverage = 0.0,
        double salinityPpt = 0.0,
        string waterType = "water")
    {
        OceanCoverage = oceanCoverage;
        OceanDepthM = oceanDepthM;
        IceCoverage = iceCoverage;
        SalinityPpt = salinityPpt;
        WaterType = waterType;
    }

    /// <summary>
    /// Returns the fraction of surface that is liquid and not ice-covered.
    /// </summary>
    public double GetLiquidCoverage() => OceanCoverage * (1.0 - IceCoverage);

    /// <summary>
    /// Returns whether this body qualifies as an ocean world.
    /// </summary>
    public bool IsOceanWorld() => OceanCoverage > 0.9;

    /// <summary>
    /// Returns whether the oceans are mostly frozen.
    /// </summary>
    public bool IsFrozen() => IceCoverage > 0.8;

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["ocean_coverage"] = OceanCoverage,
            ["ocean_depth_m"] = OceanDepthM,
            ["ice_coverage"] = IceCoverage,
            ["salinity_ppt"] = SalinityPpt,
            ["water_type"] = WaterType,
        };
    }

    /// <summary>
    /// Creates a hydrosphere-properties component from a dictionary.
    /// </summary>
    public static HydrosphereProps FromDictionary(Dictionary data)
    {
        return new HydrosphereProps(
            GetDouble(data, "ocean_coverage", 0.0),
            GetDouble(data, "ocean_depth_m", 0.0),
            GetDouble(data, "ice_coverage", 0.0),
            GetDouble(data, "salinity_ppt", 0.0),
            GetString(data, "water_type", "water"));
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
