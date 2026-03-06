using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Colony-suitability assessment for a planet.
/// </summary>
public partial class ColonySuitability : RefCounted
{
    /// <summary>
    /// Suitability categories derived from overall score.
    /// </summary>
    public enum Category
    {
        Unsuitable,
        Extreme,
        Difficult,
        Challenging,
        Favorable,
        Optimal,
    }

    /// <summary>
    /// Contributing factor types.
    /// </summary>
    public enum FactorType
    {
        Temperature,
        Pressure,
        Gravity,
        Atmosphere,
        Water,
        Radiation,
        Resources,
        Terrain,
        Weather,
        DayLength,
    }

    /// <summary>
    /// Overall 0-100 suitability score.
    /// </summary>
    public int OverallScore;

    /// <summary>
    /// Individual factor scores keyed by factor integer.
    /// </summary>
    public Dictionary FactorScores = new();

    /// <summary>
    /// Estimated carrying capacity.
    /// </summary>
    public int CarryingCapacity;

    /// <summary>
    /// Base annual population growth rate.
    /// </summary>
    public double BaseGrowthRate;

    /// <summary>
    /// Infrastructure difficulty modifier.
    /// </summary>
    public double InfrastructureDifficulty = 1.0;

    /// <summary>
    /// Primary limiting factors, worst first.
    /// </summary>
    public Array<int> LimitingFactors = new();

    /// <summary>
    /// Primary advantages, best first.
    /// </summary>
    public Array<int> Advantages = new();

    /// <summary>
    /// Source profile body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Whether life support is required.
    /// </summary>
    public bool RequiresLifeSupport = true;

    /// <summary>
    /// Whether pressure suits are required.
    /// </summary>
    public bool RequiresPressureSuit = true;

    /// <summary>
    /// Whether radiation shielding is required.
    /// </summary>
    public bool RequiresRadiationShielding;

    /// <summary>
    /// Returns the derived category from the overall score.
    /// </summary>
    public Category GetCategory()
    {
        return OverallScore switch
        {
            < 10 => Category.Unsuitable,
            < 30 => Category.Extreme,
            < 50 => Category.Difficult,
            < 70 => Category.Challenging,
            < 90 => Category.Favorable,
            _ => Category.Optimal,
        };
    }

    /// <summary>
    /// Returns the category as a display string.
    /// </summary>
    public string GetCategoryString() => CategoryToString(GetCategory());

    /// <summary>
    /// Converts a category to a display string.
    /// </summary>
    public static string CategoryToString(Category category)
    {
        return category switch
        {
            Category.Unsuitable => "Unsuitable",
            Category.Extreme => "Extreme",
            Category.Difficult => "Difficult",
            Category.Challenging => "Challenging",
            Category.Favorable => "Favorable",
            Category.Optimal => "Optimal",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a category from a string.
    /// </summary>
    public static Category CategoryFromString(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "unsuitable" => Category.Unsuitable,
            "extreme" => Category.Extreme,
            "difficult" => Category.Difficult,
            "challenging" => Category.Challenging,
            "favorable" => Category.Favorable,
            "optimal" => Category.Optimal,
            _ => Category.Unsuitable,
        };
    }

    /// <summary>
    /// Converts a factor type to a display string.
    /// </summary>
    public static string FactorToString(FactorType factor)
    {
        return factor switch
        {
            FactorType.Temperature => "Temperature",
            FactorType.Pressure => "Pressure",
            FactorType.Gravity => "Gravity",
            FactorType.Atmosphere => "Atmosphere",
            FactorType.Water => "Water",
            FactorType.Radiation => "Radiation",
            FactorType.Resources => "Resources",
            FactorType.Terrain => "Terrain",
            FactorType.Weather => "Weather",
            FactorType.DayLength => "Day Length",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a factor type from a string.
    /// </summary>
    public static FactorType FactorFromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_");
        return normalized switch
        {
            "temperature" => FactorType.Temperature,
            "pressure" => FactorType.Pressure,
            "gravity" => FactorType.Gravity,
            "atmosphere" => FactorType.Atmosphere,
            "water" => FactorType.Water,
            "radiation" => FactorType.Radiation,
            "resources" => FactorType.Resources,
            "terrain" => FactorType.Terrain,
            "weather" => FactorType.Weather,
            "day_length" => FactorType.DayLength,
            _ => FactorType.Temperature,
        };
    }

    /// <summary>
    /// Returns a short description of a category.
    /// </summary>
    public static string GetCategoryDescription(Category category)
    {
        return category switch
        {
            Category.Unsuitable => "Cannot support a colony under any realistic conditions",
            Category.Extreme => "Requires massive investment and accepts high ongoing risk",
            Category.Difficult => "Significant challenges require substantial resources to overcome",
            Category.Challenging => "Notable challenges but viable with proper preparation",
            Category.Favorable => "Good conditions with moderate infrastructure investment",
            Category.Optimal => "Excellent conditions requiring minimal adaptation",
            _ => "Unknown suitability",
        };
    }

    /// <summary>
    /// Returns the score for a factor.
    /// </summary>
    public int GetFactorScore(FactorType factor)
    {
        int key = (int)factor;
        if (FactorScores.ContainsKey(key))
        {
            return (int)FactorScores[key];
        }

        return 0;
    }

    /// <summary>
    /// Returns whether a factor is limiting.
    /// </summary>
    public bool IsLimitingFactor(FactorType factor) => GetFactorScore(factor) < 50;

    /// <summary>
    /// Returns whether a factor is an advantage.
    /// </summary>
    public bool IsAdvantage(FactorType factor) => GetFactorScore(factor) >= 70;

    /// <summary>
    /// Returns the worst factor.
    /// </summary>
    public FactorType GetWorstFactor()
    {
        if (LimitingFactors.Count > 0)
        {
            return (FactorType)LimitingFactors[0];
        }

        FactorType worst = FactorType.Temperature;
        int worstScore = 101;
        foreach (Variant key in FactorScores.Keys)
        {
            int score = (int)FactorScores[key];
            if (score < worstScore)
            {
                worstScore = score;
                worst = (FactorType)(int)key;
            }
        }

        return worst;
    }

    /// <summary>
    /// Returns the best factor.
    /// </summary>
    public FactorType GetBestFactor()
    {
        if (Advantages.Count > 0)
        {
            return (FactorType)Advantages[0];
        }

        FactorType best = FactorType.Temperature;
        int bestScore = -1;
        foreach (Variant key in FactorScores.Keys)
        {
            int score = (int)FactorScores[key];
            if (score > bestScore)
            {
                bestScore = score;
                best = (FactorType)(int)key;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns whether colonization is possible.
    /// </summary>
    public bool IsColonizable() => OverallScore >= 10;

    /// <summary>
    /// Returns a summary dictionary for display.
    /// </summary>
    public Dictionary GetSummary()
    {
        Array<string> limitingNames = new();
        foreach (int factor in LimitingFactors)
        {
            limitingNames.Add(FactorToString((FactorType)factor));
        }

        Array<string> advantageNames = new();
        foreach (int factor in Advantages)
        {
            advantageNames.Add(FactorToString((FactorType)factor));
        }

        return new Dictionary
        {
            ["overall_score"] = OverallScore,
            ["category"] = GetCategoryString(),
            ["is_colonizable"] = IsColonizable(),
            ["carrying_capacity"] = CarryingCapacity,
            ["base_growth_rate"] = BaseGrowthRate,
            ["infrastructure_difficulty"] = InfrastructureDifficulty,
            ["limiting_factors"] = limitingNames,
            ["advantages"] = advantageNames,
            ["requires_life_support"] = RequiresLifeSupport,
            ["requires_pressure_suit"] = RequiresPressureSuit,
            ["requires_radiation_shielding"] = RequiresRadiationShielding,
        };
    }

    /// <summary>
    /// Returns the number of factor types.
    /// </summary>
    public static int FactorCount() => 10;

    /// <summary>
    /// Returns the number of category types.
    /// </summary>
    public static int CategoryCount() => 6;

    /// <summary>
    /// Converts the suitability to a dictionary.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary factorScoresData = new();
        foreach (Variant key in FactorScores.Keys)
        {
            factorScoresData[(int)key] = (int)FactorScores[key];
        }

        Array<int> limitingFactorsData = new();
        foreach (int factor in LimitingFactors)
        {
            limitingFactorsData.Add(factor);
        }

        Array<int> advantagesData = new();
        foreach (int factor in Advantages)
        {
            advantagesData.Add(factor);
        }

        return new Dictionary
        {
            ["body_id"] = BodyId,
            ["overall_score"] = OverallScore,
            ["factor_scores"] = factorScoresData,
            ["carrying_capacity"] = CarryingCapacity,
            ["base_growth_rate"] = BaseGrowthRate,
            ["infrastructure_difficulty"] = InfrastructureDifficulty,
            ["limiting_factors"] = limitingFactorsData,
            ["advantages"] = advantagesData,
            ["requires_life_support"] = RequiresLifeSupport,
            ["requires_pressure_suit"] = RequiresPressureSuit,
            ["requires_radiation_shielding"] = RequiresRadiationShielding,
        };
    }

    /// <summary>
    /// Creates a suitability object from a dictionary payload.
    /// </summary>
    public static ColonySuitability FromDictionary(Dictionary data)
    {
        ColonySuitability suitability = new()
        {
            BodyId = GetString(data, "body_id", string.Empty),
            OverallScore = GetInt(data, "overall_score", 0),
            CarryingCapacity = GetInt(data, "carrying_capacity", 0),
            BaseGrowthRate = GetDouble(data, "base_growth_rate", 0.0),
            InfrastructureDifficulty = GetDouble(data, "infrastructure_difficulty", 1.0),
            RequiresLifeSupport = GetBool(data, "requires_life_support", true),
            RequiresPressureSuit = GetBool(data, "requires_pressure_suit", true),
            RequiresRadiationShielding = GetBool(data, "requires_radiation_shielding", false),
        };

        if (data.ContainsKey("factor_scores"))
        {
            Dictionary factorScoresData = (Dictionary)data["factor_scores"];
            foreach (Variant key in factorScoresData.Keys)
            {
                suitability.FactorScores[KeyToInt(key)] = (int)factorScoresData[key];
            }
        }

        if (data.ContainsKey("limiting_factors"))
        {
            foreach (Variant factor in (Array)data["limiting_factors"])
            {
                suitability.LimitingFactors.Add(KeyToInt(factor));
            }
        }

        if (data.ContainsKey("advantages"))
        {
            foreach (Variant factor in (Array)data["advantages"])
            {
                suitability.Advantages.Add(KeyToInt(factor));
            }
        }

        return suitability;
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key))
        {
            return (bool)data[key];
        }

        return fallback;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (data.ContainsKey(key))
        {
            return (int)data[key];
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }

    private static int KeyToInt(Variant value)
    {
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.String => int.Parse((string)value),
            _ => 0,
        };
    }
}
