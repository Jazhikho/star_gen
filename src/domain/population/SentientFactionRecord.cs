using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Deterministic neutral faction record derived from sentient-world population structure.
/// </summary>
public partial class SentientFactionRecord : RefCounted
{
    /// <summary>
    /// Stable faction identifier scoped to the source world profile.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Compact display name for the faction.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Neutral faction type.
    /// </summary>
    public string Type = string.Empty;

    /// <summary>
    /// Share of total faction influence in the inclusive range [0, 1].
    /// </summary>
    public double InfluenceShare;

    /// <summary>
    /// Relationship to the dominant regime.
    /// </summary>
    public string RegimeAlignment = string.Empty;

    /// <summary>
    /// Current faction tension pressure in the inclusive range [0, 1].
    /// </summary>
    public double TensionLevel;

    /// <summary>
    /// Source native-population or colony identifier, when the faction maps to a population.
    /// </summary>
    public string SourcePopulationId = string.Empty;

    /// <summary>
    /// Primary neutral issue represented by the faction.
    /// </summary>
    public string PrimaryIssue = string.Empty;

    /// <summary>
    /// Converts this faction to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["type"] = Type,
            ["influence_share"] = InfluenceShare,
            ["regime_alignment"] = RegimeAlignment,
            ["tension_level"] = TensionLevel,
            ["source_population_id"] = SourcePopulationId,
            ["primary_issue"] = PrimaryIssue,
        };
    }

    /// <summary>
    /// Creates a faction record from a dictionary payload.
    /// </summary>
    public static SentientFactionRecord FromDictionary(Dictionary data)
    {
        SentientFactionRecord record = new();
        record.Id = GetString(data, "id", string.Empty);
        record.Name = GetString(data, "name", string.Empty);
        record.Type = GetString(data, "type", string.Empty);
        record.InfluenceShare = Clamp01(GetDouble(data, "influence_share", 0.0));
        record.RegimeAlignment = GetString(data, "regime_alignment", string.Empty);
        record.TensionLevel = Clamp01(GetDouble(data, "tension_level", 0.0));
        record.SourcePopulationId = GetString(data, "source_population_id", string.Empty);
        record.PrimaryIssue = GetString(data, "primary_issue", string.Empty);
        return record;
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
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
}
