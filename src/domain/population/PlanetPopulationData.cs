using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Full population payload for a body.
/// </summary>
public partial class PlanetPopulationData : RefCounted
{
    /// <summary>
    /// Source body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Derived profile.
    /// </summary>
    public PlanetProfile? Profile;

    /// <summary>
    /// Colony suitability assessment.
    /// </summary>
    public ColonySuitability? Suitability;

    /// <summary>
    /// Native populations.
    /// </summary>
    public Array<NativePopulation> NativePopulations = new();

    /// <summary>
    /// Colonies.
    /// </summary>
    public Array<Colony> Colonies = new();

    /// <summary>
    /// Deterministic generation seed.
    /// </summary>
    public int GenerationSeed;

    /// <summary>
    /// Timestamp placeholder.
    /// </summary>
    public int GeneratedTimestamp;

    /// <summary>
    /// Native relations keyed by native population id (for body-level relation tracking).
    /// </summary>
    public Dictionary NativeRelations = new();

    /// <summary>
    /// Whether the body-level population state is active (used by RecordAbandonment).
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
    /// Whether the body achieved independence (used by RecordIndependence).
    /// </summary>
    public bool IsIndependent;

    /// <summary>
    /// Independence year if applicable.
    /// </summary>
    public int IndependenceYear;

    /// <summary>
    /// Cached or aggregate population count (used by RecordAbandonment).
    /// </summary>
    public int Population;

    /// <summary>
    /// Returns the total extant native plus active colony population.
    /// </summary>
    public int GetTotalPopulation()
    {
        return GetNativePopulation() + GetColonyPopulation();
    }

    /// <summary>
    /// Returns the total extant native population.
    /// </summary>
    public int GetNativePopulation()
    {
        int total = 0;
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant)
            {
                total += nativePopulation.Population;
            }
        }

        return total;
    }

    /// <summary>
    /// Returns the total active colony population.
    /// </summary>
    public int GetColonyPopulation()
    {
        int total = 0;
        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive)
            {
                total += colony.Population;
            }
        }

        return total;
    }

    /// <summary>
    /// Returns whether the body currently has any active population.
    /// </summary>
    public bool IsInhabited()
    {
        return GetTotalPopulation() > 0;
    }

    /// <summary>
    /// Returns whether the body has any native populations.
    /// </summary>
    public bool HasNatives()
    {
        return NativePopulations.Count > 0;
    }

    /// <summary>
    /// Returns whether the body has any extant native populations.
    /// </summary>
    public bool HasExtantNatives()
    {
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns whether the body has any colonies.
    /// </summary>
    public bool HasColonies()
    {
        return Colonies.Count > 0;
    }

    /// <summary>
    /// Returns whether the body has any active colonies.
    /// </summary>
    public bool HasActiveColonies()
    {
        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the count of extant native populations.
    /// </summary>
    public int GetExtantNativeCount()
    {
        int count = 0;
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns the count of active colonies.
    /// </summary>
    public int GetActiveColonyCount()
    {
        int count = 0;
        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns all extant native populations.
    /// </summary>
    public Array<NativePopulation> GetExtantNatives()
    {
        Array<NativePopulation> result = new();
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant)
            {
                result.Add(nativePopulation);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all active colonies.
    /// </summary>
    public Array<Colony> GetActiveColonies()
    {
        Array<Colony> result = new();
        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive)
            {
                result.Add(colony);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the dominant population (native or colony with highest population).
    /// </summary>
    public Variant GetDominantPopulation()
    {
        int maxPopulation = 0;
        Variant dominant = default;

        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant && nativePopulation.Population > maxPopulation)
            {
                maxPopulation = nativePopulation.Population;
                dominant = Variant.From(nativePopulation);
            }
        }

        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive && colony.Population > maxPopulation)
            {
                maxPopulation = colony.Population;
                dominant = Variant.From(colony);
            }
        }

        return dominant;
    }

    /// <summary>
    /// Returns the name of the dominant population.
    /// </summary>
    public string GetDominantPopulationName()
    {
        Variant dominant = GetDominantPopulation();
        if (dominant.VariantType == Variant.Type.Nil)
        {
            return "Uninhabited";
        }

        if (dominant.Obj is NativePopulation nativePop)
        {
            return $"Native {nativePop.Id}";
        }

        if (dominant.Obj is Colony colony)
        {
            return colony.Name;
        }

        return "Unknown";
    }

    /// <summary>
    /// Returns whether there is native-colony conflict.
    /// </summary>
    public bool HasNativeColonyConflict()
    {
        foreach (Colony colony in Colonies)
        {
            if (colony.HasHostileNativeRelations())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the political situation string.
    /// </summary>
    public string GetPoliticalSituation()
    {
        bool hasExtantNatives = HasExtantNatives();
        bool hasActiveColonies = HasActiveColonies();

        if (!hasExtantNatives && !hasActiveColonies)
        {
            return "uninhabited";
        }

        if (hasExtantNatives && !hasActiveColonies)
        {
            return "native_only";
        }

        if (!hasExtantNatives && hasActiveColonies)
        {
            return "colony_only";
        }

        if (HasNativeColonyConflict())
        {
            return "conflict";
        }

        return "coexisting";
    }

    /// <summary>
    /// Returns the highest technology level present.
    /// </summary>
    public TechnologyLevel.Level GetHighestTechLevel()
    {
        TechnologyLevel.Level highest = TechnologyLevel.Level.StoneAge;

        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.IsExtant && (int)nativePopulation.TechLevel > (int)highest)
            {
                highest = nativePopulation.TechLevel;
            }
        }

        foreach (Colony colony in Colonies)
        {
            if (colony.IsActive && (int)colony.TechLevel > (int)highest)
            {
                highest = colony.TechLevel;
            }
        }

        return highest;
    }

    /// <summary>
    /// Returns a native population by ID.
    /// </summary>
    public NativePopulation? GetNativeById(string nativeId)
    {
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            if (nativePopulation.Id == nativeId)
            {
                return nativePopulation;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a colony by ID.
    /// </summary>
    public Colony? GetColonyById(string colonyId)
    {
        foreach (Colony colony in Colonies)
        {
            if (colony.Id == colonyId)
            {
                return colony;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a summary dictionary.
    /// </summary>
    public Dictionary GetSummary()
    {
        Dictionary summary = new()
        {
            ["body_id"] = BodyId,
            ["total_population"] = GetTotalPopulation(),
            ["extant_native_count"] = GetExtantNativeCount(),
            ["active_colony_count"] = GetActiveColonyCount(),
            ["political_situation"] = GetPoliticalSituation(),
        };

        if (Profile != null)
        {
            summary["habitability_score"] = Profile.HabitabilityScore;
        }

        if (Suitability != null)
        {
            summary["suitability_score"] = Suitability.OverallScore;
        }

        return summary;
    }

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
    /// Converts this payload to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> nativeData = new();
        foreach (NativePopulation nativePopulation in NativePopulations)
        {
            nativeData.Add(nativePopulation.ToDictionary());
        }

        Array<Dictionary> colonyData = new();
        foreach (Colony colony in Colonies)
        {
            colonyData.Add(colony.ToDictionary());
        }

        Dictionary data = new()
        {
            ["body_id"] = BodyId,
            ["generation_seed"] = GenerationSeed,
            ["generated_timestamp"] = GeneratedTimestamp,
            ["native_populations"] = nativeData,
            ["colonies"] = colonyData,
        };

        if (Profile != null)
        {
            data["profile"] = Profile.ToDictionary();
        }

        if (Suitability != null)
        {
            data["suitability"] = Suitability.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Alias for ToDictionary for test compatibility.
    /// </summary>
    public Dictionary ToDict() => ToDictionary();

    /// <summary>
    /// Creates a population payload from a dictionary payload.
    /// </summary>
    public static PlanetPopulationData FromDictionary(Dictionary data)
    {
        PlanetPopulationData populationData = new()
        {
            BodyId = GetString(data, "body_id", string.Empty),
            GenerationSeed = GetInt(data, "generation_seed", 0),
            GeneratedTimestamp = GetInt(data, "generated_timestamp", 0),
        };

        if (data.ContainsKey("profile") && data["profile"].VariantType == Variant.Type.Dictionary)
        {
            populationData.Profile = PlanetProfile.FromDictionary((Dictionary)data["profile"]);
        }

        if (data.ContainsKey("suitability") && data["suitability"].VariantType == Variant.Type.Dictionary)
        {
            populationData.Suitability = ColonySuitability.FromDictionary((Dictionary)data["suitability"]);
        }

        if (data.ContainsKey("native_populations") && data["native_populations"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["native_populations"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    populationData.NativePopulations.Add(NativePopulation.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("colonies") && data["colonies"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["colonies"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    populationData.Colonies.Add(Colony.FromDictionary((Dictionary)value));
                }
            }
        }

        return populationData;
    }

    /// <summary>
    /// Alias for FromDictionary for test compatibility.
    /// </summary>
    public static PlanetPopulationData FromDict(Dictionary data) => FromDictionary(data);

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

    private static int TryParseInt(string s, int fallback)
    {
        if (int.TryParse(s, out int parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
