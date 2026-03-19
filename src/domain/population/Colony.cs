using Godot;
using Godot.Collections;
using StarGen.Domain.Concepts;

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
    /// Persisted concept results for this colony.
    /// </summary>
    public ConceptResultStore ConceptResults = new();

    /// <summary>
    /// Returns the age of the colony.
    /// </summary>
    public int GetAge(int currentYear)
    {
        if (IsActive)
        {
            return currentYear - FoundingYear;
        }

        return AbandonmentYear - FoundingYear;
    }

    /// <summary>
    /// Returns the growth state string.
    /// </summary>
    public string GetGrowthState()
    {
        if (!IsActive)
        {
            return "abandoned";
        }

        if (Population >= PeakPopulation * 0.95)
        {
            return "growing";
        }

        if (Population >= PeakPopulation * 0.6)
        {
            return "stable";
        }

        return "declining";
    }

    /// <summary>
    /// Returns the current regime.
    /// </summary>
    public GovernmentType.Regime GetRegime()
    {
        return Government.Regime;
    }

    /// <summary>
    /// Returns whether the government is stable.
    /// </summary>
    public bool IsPoliticallyStable()
    {
        return Government.IsStable();
    }

    /// <summary>
    /// Updates the peak population if current is higher.
    /// </summary>
    public void UpdatePeakPopulation(int year)
    {
        if (Population > PeakPopulation)
        {
            PeakPopulation = Population;
            PeakPopulationYear = year;
        }
    }

    /// <summary>
    /// Returns a summary dictionary.
    /// </summary>
    public Dictionary GetSummary()
    {
        Dictionary data = new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["body_id"] = BodyId,
            ["colony_type"] = ColonyType.ToStringName(Type),
            ["population"] = Population,
            ["tech_level"] = TechnologyLevel.ToStringName(TechLevel),
            ["regime"] = GovernmentType.ToStringName(Government.Regime),
            ["is_active"] = IsActive,
            ["is_independent"] = IsIndependent,
            ["territorial_control"] = TerritorialControl,
        };

        return data;
    }

    /// <summary>
    /// Adds or updates a native relation.
    /// </summary>
    public void SetNativeRelation(NativeRelation relation)
    {
        NativeRelations[relation.NativePopulationId] = relation;
    }

    /// <summary>
    /// Returns whether any native relations exist.
    /// </summary>
    public bool HasNativeRelations()
    {
        return NativeRelations.Count > 0;
    }

    /// <summary>
    /// Returns a specific native relation by ID.
    /// </summary>
    public NativeRelation? GetNativeRelation(string nativeId)
    {
        if (NativeRelations.ContainsKey(nativeId))
        {
            return (NativeRelation)NativeRelations[nativeId];
        }

        return null;
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
    /// Returns the overall native status string.
    /// </summary>
    public string GetOverallNativeStatus()
    {
        if (NativeRelations.Count == 0)
        {
            return "none";
        }

        bool hasHostile = false;
        bool hasPeaceful = false;

        foreach (Variant relation in NativeRelations.Values)
        {
            NativeRelation nativeRelation = (NativeRelation)relation;
            if (nativeRelation.IsHostile())
            {
                hasHostile = true;
            }
            else if (nativeRelation.IsPositive())
            {
                hasPeaceful = true;
            }
        }

        if (hasHostile && hasPeaceful)
        {
            return "mixed";
        }

        if (hasHostile)
        {
            return "hostile";
        }

        if (hasPeaceful)
        {
            return "peaceful";
        }

        return "neutral";
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

        Dictionary data = new Dictionary
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

        if (!ConceptResults.IsEmpty())
        {
            data["concept_results"] = ConceptResults.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Alias for ToDictionary for test compatibility.
    /// </summary>
    public Dictionary ToDict() => ToDictionary();

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

        if (data.ContainsKey("concept_results") && data["concept_results"].VariantType == Variant.Type.Dictionary)
        {
            colony.ConceptResults = ConceptResultStore.FromDictionary((Dictionary)data["concept_results"]);
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

    /// <summary>
    /// Alias for FromDictionary for test compatibility.
    /// </summary>
    public static Colony FromDict(Dictionary data) => FromDictionary(data);

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
            Variant.Type.String => TryParseInt((string)value, fallback),
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
            Variant.Type.String => TryParseDouble((string)value, fallback),
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
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool)
        {
            return (bool)data[key];
        }

        return fallback;
    }

    /// <summary>
    /// Clamps a value to the unit interval.
    /// </summary>
    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }

    private static int TryParseInt(string s, int fallback)
    {
        if (int.TryParse(s, out int parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static double TryParseDouble(string s, double fallback)
    {
        if (double.TryParse(s, out double parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
