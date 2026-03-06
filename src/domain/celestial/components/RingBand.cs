using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// A single band within a ring system.
/// </summary>
public partial class RingBand : RefCounted
{
    /// <summary>
    /// Inner radius of this band in meters.
    /// </summary>
    public double InnerRadiusM;

    /// <summary>
    /// Outer radius of this band in meters.
    /// </summary>
    public double OuterRadiusM;

    /// <summary>
    /// Optical depth.
    /// </summary>
    public double OpticalDepth;

    /// <summary>
    /// Composition as material name to mass fraction.
    /// </summary>
    public Dictionary Composition;

    /// <summary>
    /// Median particle size in meters.
    /// </summary>
    public double ParticleSizeM;

    /// <summary>
    /// Optional name of the band.
    /// </summary>
    public string Name;

    /// <summary>
    /// Creates a new ring-band component.
    /// </summary>
    public RingBand(
        double innerRadiusM = 0.0,
        double outerRadiusM = 0.0,
        double opticalDepth = 0.0,
        Dictionary? composition = null,
        double particleSizeM = 1.0,
        string name = "")
    {
        InnerRadiusM = innerRadiusM;
        OuterRadiusM = outerRadiusM;
        OpticalDepth = opticalDepth;
        Composition = CloneDictionary(composition);
        ParticleSizeM = particleSizeM;
        Name = name;
    }

    /// <summary>
    /// Calculates the width of this band in meters.
    /// </summary>
    public double GetWidthM() => OuterRadiusM - InnerRadiusM;

    /// <summary>
    /// Returns the dominant material in the composition.
    /// </summary>
    public string GetDominantMaterial()
    {
        double maxFraction = 0.0;
        string dominant = string.Empty;

        foreach (Variant material in Composition.Keys)
        {
            double fraction = (double)Composition[material];
            if (fraction > maxFraction)
            {
                maxFraction = fraction;
                dominant = (string)material;
            }
        }

        return dominant;
    }

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["inner_radius_m"] = InnerRadiusM,
            ["outer_radius_m"] = OuterRadiusM,
            ["optical_depth"] = OpticalDepth,
            ["composition"] = CloneDictionary(Composition),
            ["particle_size_m"] = ParticleSizeM,
            ["name"] = Name,
        };
    }

    /// <summary>
    /// Creates a ring-band component from a dictionary.
    /// </summary>
    public static RingBand FromDictionary(Dictionary data)
    {
        Dictionary? composition = null;
        if (data.ContainsKey("composition"))
        {
            composition = (Dictionary)data["composition"];
        }

        return new RingBand(
            GetDouble(data, "inner_radius_m", 0.0),
            GetDouble(data, "outer_radius_m", 0.0),
            GetDouble(data, "optical_depth", 0.0),
            composition,
            GetDouble(data, "particle_size_m", 1.0),
            GetString(data, "name", ""));
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
