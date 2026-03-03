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
    /// Returns whether the government is stable.
    /// </summary>
    public bool IsPoliticallyStable()
    {
        return Government.IsStable();
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
