using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Neutral technology-domain access record for sentient worldbuilding and RPG adapters.
/// </summary>
public partial class TechnologyDomainAccessRecord : RefCounted
{
    /// <summary>
    /// Neutral technology domain label.
    /// </summary>
    public string Domain = string.Empty;

    /// <summary>
    /// Overall neutral core level for this domain in the inclusive range [0, 24].
    /// </summary>
    public int CoreTechLevel;

    /// <summary>
    /// Elite or institutionally available neutral core level for this domain.
    /// </summary>
    public int EliteCoreTechLevel;

    /// <summary>
    /// Median resident-access neutral core level for this domain.
    /// </summary>
    public int MedianCoreTechLevel;

    /// <summary>
    /// Domain-specific adoption capacity in the inclusive range [0, 1].
    /// </summary>
    public double AdoptionCapacity;

    /// <summary>
    /// Domain-specific adoption lag in the inclusive range [0, 1].
    /// </summary>
    public double LagPressure;

    /// <summary>
    /// Domain-specific access inequality in the inclusive range [0, 1].
    /// </summary>
    public double AccessInequality;

    /// <summary>
    /// Compact neutral explanation of why the domain sits where it does.
    /// </summary>
    public string SourceSignal = string.Empty;

    /// <summary>
    /// Converts this domain access record to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["domain"] = Domain,
            ["core_tech_level"] = TechnologyLevel.ClampCoreLevel(CoreTechLevel),
            ["elite_core_tech_level"] = TechnologyLevel.ClampCoreLevel(EliteCoreTechLevel),
            ["median_core_tech_level"] = TechnologyLevel.ClampCoreLevel(MedianCoreTechLevel),
            ["adoption_capacity"] = Clamp01(AdoptionCapacity),
            ["lag_pressure"] = Clamp01(LagPressure),
            ["access_inequality"] = Clamp01(AccessInequality),
            ["source_signal"] = SourceSignal,
        };
    }

    /// <summary>
    /// Creates a technology-domain access record from a dictionary payload.
    /// </summary>
    public static TechnologyDomainAccessRecord FromDictionary(Dictionary data)
    {
        TechnologyDomainAccessRecord record = new();
        record.Domain = GetString(data, "domain", string.Empty);
        record.CoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(data, "core_tech_level", 0));
        record.EliteCoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(data, "elite_core_tech_level", record.CoreTechLevel));
        record.MedianCoreTechLevel = TechnologyLevel.ClampCoreLevel(GetInt(data, "median_core_tech_level", record.CoreTechLevel));
        record.AdoptionCapacity = Clamp01(GetDouble(data, "adoption_capacity", 0.0));
        record.LagPressure = Clamp01(GetDouble(data, "lag_pressure", 0.0));
        record.AccessInequality = Clamp01(GetDouble(data, "access_inequality", 0.0));
        record.SourceSignal = GetString(data, "source_signal", string.Empty);
        return record;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (int)(double)value;
        }

        if (value.VariantType == Variant.Type.String)
        {
            if (int.TryParse((string)value, out int parsed))
            {
                return parsed;
            }
        }

        return fallback;
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

        if (value.VariantType == Variant.Type.String)
        {
            if (double.TryParse((string)value, out double parsed))
            {
                return parsed;
            }
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
