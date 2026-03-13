using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Specification for a single station's detailed component design.
/// </summary>
public sealed class DesignSpec
{
    public DesignTemplate Template = DesignTemplate.Custom;
    public int HullTons = 1000;
    public HullConfiguration Configuration = HullConfiguration.Sphere;
    public AutoPopulateFlags AutoFlags = AutoPopulateFlags.AllAuto;
    public double OfficerRatio = 0.2;

    /// <summary>
    /// Returns whether a specific section is set to auto-populate.
    /// </summary>
    public bool IsAuto(AutoPopulateFlags section)
    {
        return (AutoFlags & section) == section;
    }

    /// <summary>
    /// Returns whether all sections are auto-populated.
    /// </summary>
    public bool IsFullyAuto()
    {
        return AutoFlags == AutoPopulateFlags.AllAuto;
    }

    /// <summary>
    /// Serializes the design spec.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["template"] = (int)Template,
            ["hull_tons"] = HullTons,
            ["configuration"] = (int)Configuration,
            ["auto_flags"] = (int)AutoFlags,
            ["officer_ratio"] = OfficerRatio,
        };
    }

    /// <summary>
    /// Rebuilds a design spec from a dictionary.
    /// </summary>
    public static DesignSpec FromDictionary(Dictionary? data)
    {
        DesignSpec spec = new();
        if (data == null)
        {
            return spec;
        }

        spec.Template = (DesignTemplate)GetInt(data, "template", (int)spec.Template);
        spec.HullTons = GetInt(data, "hull_tons", spec.HullTons);
        spec.Configuration = (HullConfiguration)GetInt(data, "configuration", (int)spec.Configuration);
        spec.AutoFlags = (AutoPopulateFlags)GetInt(data, "auto_flags", (int)spec.AutoFlags);
        spec.OfficerRatio = GetDouble(data, "officer_ratio", spec.OfficerRatio);
        return spec;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType != Variant.Type.Int)
        {
            return fallback;
        }

        return (int)value;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Float)
        {
            return (double)value;
        }

        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        return fallback;
    }
}
