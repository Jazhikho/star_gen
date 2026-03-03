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
}
