using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Atmospheric properties of a celestial body.
/// </summary>
public partial class AtmosphereProps : RefCounted
{
    /// <summary>
    /// Surface pressure in Pascals.
    /// </summary>
    public double SurfacePressurePa;

    /// <summary>
    /// Atmospheric scale height in meters.
    /// </summary>
    public double ScaleHeightM;

    /// <summary>
    /// Composition as gas name to fraction.
    /// </summary>
    public Dictionary Composition;

    /// <summary>
    /// Greenhouse warming factor.
    /// </summary>
    public double GreenhouseFactor;

    /// <summary>
    /// Creates a new atmosphere-properties component.
    /// </summary>
    public AtmosphereProps(
        double surfacePressurePa = 0.0,
        double scaleHeightM = 0.0,
        Dictionary? composition = null,
        double greenhouseFactor = 1.0)
    {
        SurfacePressurePa = surfacePressurePa;
        ScaleHeightM = scaleHeightM;
        Composition = CloneDictionary(composition);
        GreenhouseFactor = greenhouseFactor;
    }

    /// <summary>
    /// Calculates the sum of all composition fractions.
    /// </summary>
    public double GetCompositionSum()
    {
        double total = 0.0;
        foreach (Variant fraction in Composition.Values)
        {
            total += (double)fraction;
        }

        return total;
    }

    /// <summary>
    /// Returns the dominant gas in the atmosphere.
    /// </summary>
    public string GetDominantGas()
    {
        double maxFraction = 0.0;
        string dominant = string.Empty;

        foreach (Variant gas in Composition.Keys)
        {
            double fraction = (double)Composition[gas];
            if (fraction > maxFraction)
            {
                maxFraction = fraction;
                dominant = (string)gas;
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
            ["surface_pressure_pa"] = SurfacePressurePa,
            ["scale_height_m"] = ScaleHeightM,
            ["composition"] = CloneDictionary(Composition),
            ["greenhouse_factor"] = GreenhouseFactor,
        };
    }

    /// <summary>
    /// Creates an atmosphere-properties component from a dictionary.
    /// </summary>
    public static AtmosphereProps FromDictionary(Dictionary data)
    {
        return new AtmosphereProps(
            GetDouble(data, "surface_pressure_pa", 0.0),
            GetDouble(data, "scale_height_m", 0.0),
            data.ContainsKey("composition") ? (Dictionary)data["composition"] : null,
            GetDouble(data, "greenhouse_factor", 1.0));
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
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }
}
