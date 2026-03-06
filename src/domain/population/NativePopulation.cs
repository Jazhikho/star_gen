using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Native population that emerged on a body.
/// </summary>
public partial class NativePopulation : RefCounted
{
    /// <summary>
    /// Population identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Population display name.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Source body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Year of emergence.
    /// </summary>
    public int OriginYear;

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
    public TechnologyLevel.Level TechLevel = TechnologyLevel.Level.StoneAge;

    /// <summary>
    /// Current government.
    /// </summary>
    public Government Government = new();

    /// <summary>
    /// Whether the population is extant.
    /// </summary>
    public bool IsExtant = true;

    /// <summary>
    /// Year of extinction if not extant.
    /// </summary>
    public int ExtinctionYear;

    /// <summary>
    /// Extinction cause if not extant.
    /// </summary>
    public string ExtinctionCause = string.Empty;

    /// <summary>
    /// Cultural traits.
    /// </summary>
    public Array<string> CulturalTraits = new();

    /// <summary>
    /// Primary biome string.
    /// </summary>
    public string PrimaryBiome = string.Empty;

    /// <summary>
    /// Fractional territorial control.
    /// </summary>
    public double TerritorialControl;

    /// <summary>
    /// Historical timeline.
    /// </summary>
    public PopulationHistory History = new();

    /// <summary>
    /// Extra metadata.
    /// </summary>
    public Dictionary Metadata = new();

    /// <summary>
    /// Returns the age of the population.
    /// </summary>
    public int GetAge(int currentYear)
    {
        if (IsExtant)
        {
            return currentYear - OriginYear;
        }

        return ExtinctionYear - OriginYear;
    }

    /// <summary>
    /// Returns the growth state string.
    /// </summary>
    public string GetGrowthState()
    {
        if (!IsExtant)
        {
            return "extinct";
        }

        if (Population >= PeakPopulation * 0.95)
        {
            return "growing";
        }

        if (Population >= PeakPopulation * 0.7)
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
    /// Returns whether the population can achieve spaceflight.
    /// </summary>
    public bool CanSpaceflight()
    {
        return TechnologyLevel.CanSpaceflight(TechLevel);
    }

    /// <summary>
    /// Returns whether the population can colonize other worlds.
    /// </summary>
    public bool CanColonize()
    {
        return TechnologyLevel.CanInterstellar(TechLevel);
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
        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["body_id"] = BodyId,
            ["population"] = Population,
            ["tech_level"] = TechnologyLevel.ToStringName(TechLevel),
            ["regime"] = GovernmentType.ToStringName(Government.Regime),
            ["is_extant"] = IsExtant,
            ["territorial_control"] = TerritorialControl,
        };
    }

    /// <summary>
    /// Records extinction.
    /// </summary>
    public void RecordExtinction(int year, string cause)
    {
        IsExtant = false;
        ExtinctionYear = year;
        ExtinctionCause = cause;
        Population = 0;
    }

    /// <summary>
    /// Converts this population to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<string> traits = new();
        foreach (string culturalTrait in CulturalTraits)
        {
            traits.Add(culturalTrait);
        }

        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["body_id"] = BodyId,
            ["origin_year"] = OriginYear,
            ["population"] = Population,
            ["peak_population"] = PeakPopulation,
            ["peak_population_year"] = PeakPopulationYear,
            ["tech_level"] = (int)TechLevel,
            ["is_extant"] = IsExtant,
            ["extinction_year"] = ExtinctionYear,
            ["extinction_cause"] = ExtinctionCause,
            ["cultural_traits"] = traits,
            ["primary_biome"] = PrimaryBiome,
            ["territorial_control"] = TerritorialControl,
            ["metadata"] = CloneDictionary(Metadata),
            ["government"] = Government.ToDictionary(),
            ["history"] = History.ToDictionary(),
        };
    }

    /// <summary>
    /// Alias for ToDictionary for test compatibility.
    /// </summary>
    public Dictionary ToDict() => ToDictionary();

    /// <summary>
    /// Creates a native population from a dictionary payload.
    /// </summary>
    public static NativePopulation FromDictionary(Dictionary data)
    {
        NativePopulation population = new()
        {
            Id = GetString(data, "id", string.Empty),
            Name = GetString(data, "name", string.Empty),
            BodyId = GetString(data, "body_id", string.Empty),
            OriginYear = GetInt(data, "origin_year", 0),
            Population = GetInt(data, "population", 0),
            PeakPopulation = GetInt(data, "peak_population", 0),
            PeakPopulationYear = GetInt(data, "peak_population_year", 0),
            TechLevel = (TechnologyLevel.Level)GetInt(data, "tech_level", 0),
            IsExtant = GetBool(data, "is_extant", true),
            ExtinctionYear = GetInt(data, "extinction_year", 0),
            ExtinctionCause = GetString(data, "extinction_cause", string.Empty),
            PrimaryBiome = GetString(data, "primary_biome", string.Empty),
            TerritorialControl = Clamp01(GetDouble(data, "territorial_control", 0.0)),
        };

        if (data.ContainsKey("government") && data["government"].VariantType == Variant.Type.Dictionary)
        {
            population.Government = Government.FromDictionary((Dictionary)data["government"]);
        }

        if (data.ContainsKey("history") && data["history"].VariantType == Variant.Type.Dictionary)
        {
            population.History = PopulationHistory.FromDictionary((Dictionary)data["history"]);
        }

        if (data.ContainsKey("metadata") && data["metadata"].VariantType == Variant.Type.Dictionary)
        {
            population.Metadata = CloneDictionary((Dictionary)data["metadata"]);
        }

        if (data.ContainsKey("cultural_traits") && data["cultural_traits"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["cultural_traits"])
            {
                if (value.VariantType == Variant.Type.String)
                {
                    population.CulturalTraits.Add((string)value);
                }
            }
        }

        return population;
    }

    /// <summary>
    /// Alias for FromDictionary for test compatibility.
    /// </summary>
    public static NativePopulation FromDict(Dictionary data) => FromDictionary(data);

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
