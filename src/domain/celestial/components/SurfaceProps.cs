using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Surface properties of a celestial body.
/// </summary>
public partial class SurfaceProps : RefCounted
{
    /// <summary>
    /// Surface temperature in Kelvin.
    /// </summary>
    public double TemperatureK;

    /// <summary>
    /// Bond albedo.
    /// </summary>
    public double Albedo;

    /// <summary>
    /// Surface type identifier.
    /// </summary>
    public string SurfaceType;

    /// <summary>
    /// Volcanism activity level.
    /// </summary>
    public double VolcanismLevel;

    /// <summary>
    /// Surface material composition.
    /// </summary>
    public Dictionary SurfaceComposition;

    /// <summary>
    /// Terrain properties.
    /// </summary>
    public TerrainProps? Terrain;

    /// <summary>
    /// Hydrosphere properties.
    /// </summary>
    public HydrosphereProps? Hydrosphere;

    /// <summary>
    /// Cryosphere properties.
    /// </summary>
    public CryosphereProps? Cryosphere;

    /// <summary>
    /// Creates a new surface-properties component.
    /// </summary>
    public SurfaceProps(
        double temperatureK = 0.0,
        double albedo = 0.0,
        string surfaceType = "",
        double volcanismLevel = 0.0,
        Dictionary? surfaceComposition = null)
    {
        TemperatureK = temperatureK;
        Albedo = albedo;
        SurfaceType = surfaceType;
        VolcanismLevel = volcanismLevel;
        SurfaceComposition = CloneDictionary(surfaceComposition);
        Terrain = null;
        Hydrosphere = null;
        Cryosphere = null;
    }

    /// <summary>
    /// Returns whether the surface has terrain data.
    /// </summary>
    public bool HasTerrain() => Terrain != null;

    /// <summary>
    /// Returns whether the surface has hydrosphere data.
    /// </summary>
    public bool HasHydrosphere() => Hydrosphere != null;

    /// <summary>
    /// Returns whether the surface has cryosphere data.
    /// </summary>
    public bool HasCryosphere() => Cryosphere != null;

    /// <summary>
    /// Returns whether the surface is volcanically active.
    /// </summary>
    public bool IsVolcanicallyActive() => VolcanismLevel > 0.1;

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new()
        {
            ["temperature_k"] = TemperatureK,
            ["albedo"] = Albedo,
            ["surface_type"] = SurfaceType,
            ["volcanism_level"] = VolcanismLevel,
            ["surface_composition"] = CloneDictionary(SurfaceComposition),
        };

        if (Terrain != null)
        {
            data["terrain"] = Terrain.ToDictionary();
        }

        if (Hydrosphere != null)
        {
            data["hydrosphere"] = Hydrosphere.ToDictionary();
        }

        if (Cryosphere != null)
        {
            data["cryosphere"] = Cryosphere.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a surface-properties component from a dictionary.
    /// </summary>
    public static SurfaceProps FromDictionary(Dictionary data)
    {
        Dictionary? surfaceComposition = null;
        if (data.ContainsKey("surface_composition"))
        {
            surfaceComposition = (Dictionary)data["surface_composition"];
        }

        SurfaceProps props = new(
            GetDouble(data, "temperature_k", 0.0),
            GetDouble(data, "albedo", 0.0),
            GetString(data, "surface_type", ""),
            GetDouble(data, "volcanism_level", 0.0),
            surfaceComposition);

        if (data.ContainsKey("terrain"))
        {
            props.Terrain = TerrainProps.FromDictionary((Dictionary)data["terrain"]);
        }

        if (data.ContainsKey("hydrosphere"))
        {
            props.Hydrosphere = HydrosphereProps.FromDictionary((Dictionary)data["hydrosphere"]);
        }

        if (data.ContainsKey("cryosphere"))
        {
            props.Cryosphere = CryosphereProps.FromDictionary((Dictionary)data["cryosphere"]);
        }

        return props;
    }

    private static Dictionary CloneDictionary(Dictionary? source)
    {
        Dictionary clone = new();
        if (source == null)
        {
            return clone;
        }

        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
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
