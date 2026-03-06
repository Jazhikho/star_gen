using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Terrain properties for solid-surface celestial bodies.
/// </summary>
public partial class TerrainProps : RefCounted
{
    /// <summary>
    /// Maximum elevation range in meters.
    /// </summary>
    public double ElevationRangeM;

    /// <summary>
    /// Surface roughness.
    /// </summary>
    public double Roughness;

    /// <summary>
    /// Crater density.
    /// </summary>
    public double CraterDensity;

    /// <summary>
    /// Tectonic activity level.
    /// </summary>
    public double TectonicActivity;

    /// <summary>
    /// Erosion level.
    /// </summary>
    public double ErosionLevel;

    /// <summary>
    /// Terrain type classification.
    /// </summary>
    public string TerrainType;

    /// <summary>
    /// Creates a new terrain-properties component.
    /// </summary>
    public TerrainProps(
        double elevationRangeM = 0.0,
        double roughness = 0.5,
        double craterDensity = 0.0,
        double tectonicActivity = 0.0,
        double erosionLevel = 0.0,
        string terrainType = "")
    {
        ElevationRangeM = elevationRangeM;
        Roughness = roughness;
        CraterDensity = craterDensity;
        TectonicActivity = tectonicActivity;
        ErosionLevel = erosionLevel;
        TerrainType = terrainType;
    }

    /// <summary>
    /// Returns whether the surface shows signs of geological activity.
    /// </summary>
    public bool IsGeologicallyActive() => TectonicActivity > 0.1;

    /// <summary>
    /// Returns whether the surface is heavily cratered.
    /// </summary>
    public bool IsHeavilyCratered() => CraterDensity > 0.5;

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["elevation_range_m"] = ElevationRangeM,
            ["roughness"] = Roughness,
            ["crater_density"] = CraterDensity,
            ["tectonic_activity"] = TectonicActivity,
            ["erosion_level"] = ErosionLevel,
            ["terrain_type"] = TerrainType,
        };
    }

    /// <summary>
    /// Creates a terrain-properties component from a dictionary.
    /// </summary>
    public static TerrainProps FromDictionary(Dictionary data)
    {
        return new TerrainProps(
            GetDouble(data, "elevation_range_m", 0.0),
            GetDouble(data, "roughness", 0.5),
            GetDouble(data, "crater_density", 0.0),
            GetDouble(data, "tectonic_activity", 0.0),
            GetDouble(data, "erosion_level", 0.0),
            GetString(data, "terrain_type", ""));
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }
}
