using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Colony established on a body by an external civilization.
/// </summary>
public partial class Colony : RefCounted
{
    /// <summary>
    /// Colony identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Colony display name.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Source body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Colony type.
    /// </summary>
    public ColonyType.Type Type = ColonyType.Type.Settlement;

    /// <summary>
    /// Founding civilization identifier.
    /// </summary>
    public string FoundingCivilizationId = string.Empty;

    /// <summary>
    /// Founding civilization display name.
    /// </summary>
    public string FoundingCivilizationName = string.Empty;

    /// <summary>
    /// Founding year.
    /// </summary>
    public int FoundingYear;

    /// <summary>
    /// Current population.
    /// </summary>
    public int Population;

    /// <summary>
    /// Historical peak population.
    /// </summary>
    public int PeakPopulation;

    /// <summary>
    /// Year of peak population.
    /// </summary>
    public int PeakPopulationYear;

    /// <summary>
    /// Current technology level.
    /// </summary>
    public TechnologyLevel.Level TechLevel = TechnologyLevel.Level.Spacefaring;

    /// <summary>
    /// Government state.
    /// </summary>
    public Government Government = new();

    /// <summary>
    /// Whether the colony is active.
    /// </summary>
    public bool IsActive = true;

    /// <summary>
    /// Abandonment year if inactive.
    /// </summary>
    public int AbandonmentYear;

    /// <summary>
    /// Abandonment reason if inactive.
    /// </summary>
    public string AbandonmentReason = string.Empty;

    /// <summary>
    /// Whether the colony is independent.
    /// </summary>
    public bool IsIndependent;

    /// <summary>
    /// Independence year if independent.
    /// </summary>
    public int IndependenceYear;

    /// <summary>
    /// Native relations keyed by native-population id.
    /// </summary>
    public Dictionary NativeRelations = new();

    /// <summary>
    /// Fractional territorial control.
    /// </summary>
    public double TerritorialControl;

    /// <summary>
    /// Primary industry string.
    /// </summary>
    public string PrimaryIndustry = string.Empty;

    /// <summary>
    /// Self-sufficiency in the range [0, 1].
    /// </summary>
    public double SelfSufficiency;

    /// <summary>
    /// Historical timeline.
    /// </summary>
    public PopulationHistory History = new();

    /// <summary>
    /// Extra metadata.
    /// </summary>
    public Dictionary Metadata = new();

    /// <summary>
    /// Adds or updates a native relation.
    /// </summary>
    public void SetNativeRelation(NativeRelation relation)
    {
        NativeRelations[relation.NativePopulationId] = relation;
    }

    /// <summary>
    /// Returns all native relations.
    /// </summary>
    public Array<NativeRelation> GetAllNativeRelations()
    {
        Array<NativeRelation> result = new();
        foreach (Variant relation in NativeRelations.Values)
        {
            result.Add((NativeRelation)relation);
        }

        return result;
    }

    /// <summary>
    /// Returns whether any native relation is hostile.
    /// </summary>
    public bool HasHostileNativeRelations()
    {
        foreach (Variant relation in NativeRelations.Values)
        {
            if (((NativeRelation)relation).IsHostile())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Records colony abandonment.
    /// </summary>
    public void RecordAbandonment(int year, string reason)
    {
        IsActive = false;
        AbandonmentYear = year;
        AbandonmentReason = reason;
        Population = 0;
    }

    /// <summary>
    /// Records colony independence.
    /// </summary>
    public void RecordIndependence(int year)
    {
        IsIndependent = true;
        IndependenceYear = year;
    }

    /// <summary>
    /// Converts this colony to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary nativeRelationsData = new();
        foreach (Variant nativeId in NativeRelations.Keys)
        {
            nativeRelationsData[(string)nativeId] = ((NativeRelation)NativeRelations[nativeId]).ToDictionary();
        }

        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["body_id"] = BodyId,
            ["colony_type"] = (int)Type,
            ["founding_civilization_id"] = FoundingCivilizationId,
            ["founding_civilization_name"] = FoundingCivilizationName,
            ["founding_year"] = FoundingYear,
            ["population"] = Population,
            ["peak_population"] = PeakPopulation,
            ["peak_population_year"] = PeakPopulationYear,
            ["tech_level"] = (int)TechLevel,
            ["is_active"] = IsActive,
            ["abandonment_year"] = AbandonmentYear,
            ["abandonment_reason"] = AbandonmentReason,
            ["is_independent"] = IsIndependent,
            ["independence_year"] = IndependenceYear,
            ["native_relations"] = nativeRelationsData,
            ["territorial_control"] = TerritorialControl,
            ["primary_industry"] = PrimaryIndustry,
            ["self_sufficiency"] = SelfSufficiency,
            ["metadata"] = CloneDictionary(Metadata),
            ["government"] = Government.ToDictionary(),
            ["history"] = History.ToDictionary(),
        };
    }

    /// <summary>
    /// Creates a colony from a dictionary payload.
    /// </summary>
    public static Colony FromDictionary(Dictionary data)
    {
        Colony colony = new()
        {
            Id = GetString(data, "id", string.Empty),
            Name = GetString(data, "name", string.Empty),
            BodyId = GetString(data, "body_id", string.Empty),
            Type = (ColonyType.Type)GetInt(data, "colony_type", 0),
            FoundingCivilizationId = GetString(data, "founding_civilization_id", string.Empty),
            FoundingCivilizationName = GetString(data, "founding_civilization_name", string.Empty),
            FoundingYear = GetInt(data, "founding_year", 0),
            Population = GetInt(data, "population", 0),
            PeakPopulation = GetInt(data, "peak_population", 0),
            PeakPopulationYear = GetInt(data, "peak_population_year", 0),
            TechLevel = (TechnologyLevel.Level)GetInt(data, "tech_level", (int)TechnologyLevel.Level.Spacefaring),
            IsActive = GetBool(data, "is_active", true),
            AbandonmentYear = GetInt(data, "abandonment_year", 0),
            AbandonmentReason = GetString(data, "abandonment_reason", string.Empty),
            IsIndependent = GetBool(data, "is_independent", false),
            IndependenceYear = GetInt(data, "independence_year", 0),
            TerritorialControl = Clamp01(GetDouble(data, "territorial_control", 0.0)),
            PrimaryIndustry = GetString(data, "primary_industry", string.Empty),
            SelfSufficiency = Clamp01(GetDouble(data, "self_sufficiency", 0.0)),
        };

        if (data.ContainsKey("government") && data["government"].VariantType == Variant.Type.Dictionary)
        {
            colony.Government = Government.FromDictionary((Dictionary)data["government"]);
        }

        if (data.ContainsKey("history") && data["history"].VariantType == Variant.Type.Dictionary)
        {
            colony.History = PopulationHistory.FromDictionary((Dictionary)data["history"]);
        }

        if (data.ContainsKey("metadata") && data["metadata"].VariantType == Variant.Type.Dictionary)
        {
            colony.Metadata = CloneDictionary((Dictionary)data["metadata"]);
        }

        if (data.ContainsKey("native_relations") && data["native_relations"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary relationsData = (Dictionary)data["native_relations"];
            foreach (Variant nativeId in relationsData.Keys)
            {
                if (nativeId.VariantType != Variant.Type.String)
                {
                    continue;
                }

                Variant relationValue = relationsData[nativeId];
                if (relationValue.VariantType == Variant.Type.Dictionary)
                {
                    colony.NativeRelations[(string)nativeId] = NativeRelation.FromDictionary((Dictionary)relationValue);
                }
            }
        }

        return colony;
    }

    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => double.TryParse((string)value, out double parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool ? (bool)data[key] : fallback;
    }

    /// <summary>
    /// Clamps a value to the unit interval.
    /// </summary>
    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
