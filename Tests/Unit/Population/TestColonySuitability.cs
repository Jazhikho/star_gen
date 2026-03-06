#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ColonySuitability data model.
/// </summary>
public static class TestColonySuitability
{
    /// <summary>
    /// Creates a test suitability with sample data.
    /// </summary>
    private static ColonySuitability CreateTestSuitability()
    {
        ColonySuitability suitability = new();
        suitability.BodyId = "test_planet_001";
        suitability.OverallScore = 72;
        suitability.CarryingCapacity = 500000000;
        suitability.BaseGrowthRate = 0.025;
        suitability.InfrastructureDifficulty = 1.2;

        suitability.FactorScores = new Dictionary
        {
            { (int)ColonySuitability.FactorType.Temperature, 85 },
            { (int)ColonySuitability.FactorType.Pressure, 90 },
            { (int)ColonySuitability.FactorType.Gravity, 95 },
            { (int)ColonySuitability.FactorType.Atmosphere, 100 },
            { (int)ColonySuitability.FactorType.Water, 88 },
            { (int)ColonySuitability.FactorType.Radiation, 75 },
            { (int)ColonySuitability.FactorType.Resources, 60 },
            { (int)ColonySuitability.FactorType.Terrain, 55 },
            { (int)ColonySuitability.FactorType.Weather, 45 },
            { (int)ColonySuitability.FactorType.DayLength, 92 },
        };

        suitability.LimitingFactors = new Array<int>
        {
            (int)ColonySuitability.FactorType.Weather
        };
        suitability.Advantages = new Array<int>
        {
            (int)ColonySuitability.FactorType.Atmosphere,
            (int)ColonySuitability.FactorType.Gravity,
            (int)ColonySuitability.FactorType.DayLength,
        };

        suitability.RequiresLifeSupport = false;
        suitability.RequiresPressureSuit = false;
        suitability.RequiresRadiationShielding = false;

        return suitability;
    }

    /// <summary>
    /// Tests basic suitability creation.
    /// </summary>
    public static void TestCreation()
    {
        ColonySuitability suitability = new();
        DotNetNativeTestSuite.AssertEqual("", suitability.BodyId, "Default BodyId should be empty");
        DotNetNativeTestSuite.AssertEqual(0, suitability.OverallScore, "Default OverallScore should be 0");
        DotNetNativeTestSuite.AssertEqual(0, suitability.CarryingCapacity, "Default CarryingCapacity should be 0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, suitability.BaseGrowthRate, 0.001, "Default BaseGrowthRate should be 0");
        DotNetNativeTestSuite.AssertFloatNear(1.0, suitability.InfrastructureDifficulty, 0.001, "Default InfrastructureDifficulty should be 1.0");
    }

    /// <summary>
    /// Tests category derivation for all score ranges.
    /// </summary>
    public static void TestGetCategoryRanges()
    {
        ColonySuitability suitability = new();

        suitability.OverallScore = 0;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Unsuitable, suitability.GetCategory(), "Score 0 should be Unsuitable");

        suitability.OverallScore = 9;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Unsuitable, suitability.GetCategory(), "Score 9 should be Unsuitable");

        suitability.OverallScore = 10;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Extreme, suitability.GetCategory(), "Score 10 should be Extreme");

        suitability.OverallScore = 29;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Extreme, suitability.GetCategory(), "Score 29 should be Extreme");

        suitability.OverallScore = 30;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Difficult, suitability.GetCategory(), "Score 30 should be Difficult");

        suitability.OverallScore = 49;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Difficult, suitability.GetCategory(), "Score 49 should be Difficult");

        suitability.OverallScore = 50;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Challenging, suitability.GetCategory(), "Score 50 should be Challenging");

        suitability.OverallScore = 69;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Challenging, suitability.GetCategory(), "Score 69 should be Challenging");

        suitability.OverallScore = 70;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Favorable, suitability.GetCategory(), "Score 70 should be Favorable");

        suitability.OverallScore = 89;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Favorable, suitability.GetCategory(), "Score 89 should be Favorable");

        suitability.OverallScore = 90;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Optimal, suitability.GetCategory(), "Score 90 should be Optimal");

        suitability.OverallScore = 100;
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Optimal, suitability.GetCategory(), "Score 100 should be Optimal");
    }

    /// <summary>
    /// Tests category string conversion.
    /// </summary>
    public static void TestGetCategoryString()
    {
        ColonySuitability suitability = new();
        suitability.OverallScore = 72;
        DotNetNativeTestSuite.AssertEqual("Favorable", suitability.GetCategoryString(), "Score 72 should be Favorable");
    }

    /// <summary>
    /// Tests category to string static method.
    /// </summary>
    public static void TestCategoryToString()
    {
        DotNetNativeTestSuite.AssertEqual("Unsuitable", ColonySuitability.CategoryToString(ColonySuitability.Category.Unsuitable), "Unsuitable string should match");
        DotNetNativeTestSuite.AssertEqual("Extreme", ColonySuitability.CategoryToString(ColonySuitability.Category.Extreme), "Extreme string should match");
        DotNetNativeTestSuite.AssertEqual("Difficult", ColonySuitability.CategoryToString(ColonySuitability.Category.Difficult), "Difficult string should match");
        DotNetNativeTestSuite.AssertEqual("Challenging", ColonySuitability.CategoryToString(ColonySuitability.Category.Challenging), "Challenging string should match");
        DotNetNativeTestSuite.AssertEqual("Favorable", ColonySuitability.CategoryToString(ColonySuitability.Category.Favorable), "Favorable string should match");
        DotNetNativeTestSuite.AssertEqual("Optimal", ColonySuitability.CategoryToString(ColonySuitability.Category.Optimal), "Optimal string should match");
    }

    /// <summary>
    /// Tests category from string static method.
    /// </summary>
    public static void TestCategoryFromString()
    {
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Unsuitable, ColonySuitability.CategoryFromString("unsuitable"), "Should parse unsuitable");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Extreme, ColonySuitability.CategoryFromString("Extreme"), "Should parse Extreme");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Difficult, ColonySuitability.CategoryFromString("DIFFICULT"), "Should parse DIFFICULT");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.Category.Unsuitable, ColonySuitability.CategoryFromString("invalid"), "Invalid should return Unsuitable");
    }

    /// <summary>
    /// Tests factor to string static method.
    /// </summary>
    public static void TestFactorToString()
    {
        DotNetNativeTestSuite.AssertEqual("Temperature", ColonySuitability.FactorToString(ColonySuitability.FactorType.Temperature), "Temperature string should match");
        DotNetNativeTestSuite.AssertEqual("Water", ColonySuitability.FactorToString(ColonySuitability.FactorType.Water), "Water string should match");
        DotNetNativeTestSuite.AssertEqual("Day Length", ColonySuitability.FactorToString(ColonySuitability.FactorType.DayLength), "Day Length string should match");
    }

    /// <summary>
    /// Tests factor from string static method.
    /// </summary>
    public static void TestFactorFromString()
    {
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorType.Temperature, ColonySuitability.FactorFromString("temperature"), "Should parse temperature");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorType.DayLength, ColonySuitability.FactorFromString("Day Length"), "Should parse Day Length");
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorType.DayLength, ColonySuitability.FactorFromString("day_length"), "Should parse day_length");
    }

    /// <summary>
    /// Tests get_factor_score method.
    /// </summary>
    public static void TestGetFactorScore()
    {
        ColonySuitability suitability = CreateTestSuitability();
        DotNetNativeTestSuite.AssertEqual(85, suitability.GetFactorScore(ColonySuitability.FactorType.Temperature), "Temperature score should be 85");
        DotNetNativeTestSuite.AssertEqual(45, suitability.GetFactorScore(ColonySuitability.FactorType.Weather), "Weather score should be 45");
    }

    /// <summary>
    /// Tests get_factor_score for missing factor.
    /// </summary>
    public static void TestGetFactorScoreMissing()
    {
        ColonySuitability suitability = new();
        DotNetNativeTestSuite.AssertEqual(0, suitability.GetFactorScore(ColonySuitability.FactorType.Temperature), "Missing factor should return 0");
    }

    /// <summary>
    /// Tests is_limiting_factor method.
    /// </summary>
    public static void TestIsLimitingFactor()
    {
        ColonySuitability suitability = CreateTestSuitability();
        DotNetNativeTestSuite.AssertTrue(suitability.IsLimitingFactor(ColonySuitability.FactorType.Weather), "Weather should be limiting factor");
        DotNetNativeTestSuite.AssertFalse(suitability.IsLimitingFactor(ColonySuitability.FactorType.Temperature), "Temperature should not be limiting factor");
    }

    /// <summary>
    /// Tests is_advantage method.
    /// </summary>
    public static void TestIsAdvantage()
    {
        ColonySuitability suitability = CreateTestSuitability();
        DotNetNativeTestSuite.AssertTrue(suitability.IsAdvantage(ColonySuitability.FactorType.Atmosphere), "Atmosphere should be advantage");
        DotNetNativeTestSuite.AssertFalse(suitability.IsAdvantage(ColonySuitability.FactorType.Weather), "Weather should not be advantage");
    }

    /// <summary>
    /// Tests get_worst_factor method.
    /// </summary>
    public static void TestGetWorstFactor()
    {
        ColonySuitability suitability = CreateTestSuitability();
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorType.Weather, suitability.GetWorstFactor(), "Worst factor should be Weather");
    }

    /// <summary>
    /// Tests get_best_factor method.
    /// </summary>
    public static void TestGetBestFactor()
    {
        ColonySuitability suitability = CreateTestSuitability();
        DotNetNativeTestSuite.AssertEqual(ColonySuitability.FactorType.Atmosphere, suitability.GetBestFactor(), "Best factor should be Atmosphere");
    }

    /// <summary>
    /// Tests is_colonizable method.
    /// </summary>
    public static void TestIsColonizable()
    {
        ColonySuitability suitability = new();

        suitability.OverallScore = 0;
        DotNetNativeTestSuite.AssertFalse(suitability.IsColonizable(), "Score 0 should not be colonizable");

        suitability.OverallScore = 9;
        DotNetNativeTestSuite.AssertFalse(suitability.IsColonizable(), "Score 9 should not be colonizable");

        suitability.OverallScore = 10;
        DotNetNativeTestSuite.AssertTrue(suitability.IsColonizable(), "Score 10 should be colonizable");

        suitability.OverallScore = 100;
        DotNetNativeTestSuite.AssertTrue(suitability.IsColonizable(), "Score 100 should be colonizable");
    }

    /// <summary>
    /// Tests get_summary returns all expected keys.
    /// </summary>
    public static void TestGetSummary()
    {
        ColonySuitability suitability = CreateTestSuitability();
        Godot.Collections.Dictionary summary = suitability.GetSummary();

        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("overall_score"), "Should have overall_score");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("category"), "Should have category");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("is_colonizable"), "Should have is_colonizable");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("carrying_capacity"), "Should have carrying_capacity");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("base_growth_rate"), "Should have base_growth_rate");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("infrastructure_difficulty"), "Should have infrastructure_difficulty");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("limiting_factors"), "Should have limiting_factors");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("advantages"), "Should have advantages");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("requires_life_support"), "Should have requires_life_support");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("requires_pressure_suit"), "Should have requires_pressure_suit");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("requires_radiation_shielding"), "Should have requires_radiation_shielding");

        DotNetNativeTestSuite.AssertEqual(72, summary["overall_score"].AsInt32(), "OverallScore should be 72");
        DotNetNativeTestSuite.AssertEqual("Favorable", summary["category"].AsString(), "Category should be Favorable");
        DotNetNativeTestSuite.AssertTrue(summary["is_colonizable"].AsBool(), "Should be colonizable");
        DotNetNativeTestSuite.AssertEqual(false, summary["requires_life_support"].AsBool(), "Should not require life support");
        DotNetNativeTestSuite.AssertEqual(false, summary["requires_pressure_suit"].AsBool(), "Should not require pressure suit");
        DotNetNativeTestSuite.AssertEqual(false, summary["requires_radiation_shielding"].AsBool(), "Should not require radiation shielding");
    }

    /// <summary>
    /// Tests category description helper.
    /// </summary>
    public static void TestGetCategoryDescription()
    {
        string desc = ColonySuitability.GetCategoryDescription(ColonySuitability.Category.Favorable);
        DotNetNativeTestSuite.AssertGreaterThan(desc.Length, 0, "Description should not be empty");
        DotNetNativeTestSuite.AssertTrue(desc.Contains("Good") || desc.Contains("moderate"), "Description should contain expected text");
    }

    /// <summary>
    /// Tests factor_count static method.
    /// </summary>
    public static void TestFactorCount()
    {
        DotNetNativeTestSuite.AssertEqual(10, ColonySuitability.FactorCount(), "Should have 10 factors");
    }

    /// <summary>
    /// Tests category_count static method.
    /// </summary>
    public static void TestCategoryCount()
    {
        DotNetNativeTestSuite.AssertEqual(6, ColonySuitability.CategoryCount(), "Should have 6 categories");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        ColonySuitability original = CreateTestSuitability();

        Godot.Collections.Dictionary data = original.ToDictionary();
        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.OverallScore, restored.OverallScore, "OverallScore should match");
        DotNetNativeTestSuite.AssertEqual(original.CarryingCapacity, restored.CarryingCapacity, "CarryingCapacity should match");
        DotNetNativeTestSuite.AssertFloatNear(original.BaseGrowthRate, restored.BaseGrowthRate, 0.0001, "BaseGrowthRate should match");
        DotNetNativeTestSuite.AssertFloatNear(original.InfrastructureDifficulty, restored.InfrastructureDifficulty, 0.0001, "InfrastructureDifficulty should match");
        DotNetNativeTestSuite.AssertEqual(original.RequiresLifeSupport, restored.RequiresLifeSupport, "RequiresLifeSupport should match");
        DotNetNativeTestSuite.AssertEqual(original.RequiresPressureSuit, restored.RequiresPressureSuit, "RequiresPressureSuit should match");
        DotNetNativeTestSuite.AssertEqual(original.RequiresRadiationShielding, restored.RequiresRadiationShielding, "RequiresRadiationShielding should match");
    }

    /// <summary>
    /// Tests factor_scores serialization.
    /// </summary>
    public static void TestFactorScoresSerialization()
    {
        ColonySuitability original = CreateTestSuitability();

        Godot.Collections.Dictionary data = original.ToDictionary();
        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.FactorScores.Count, restored.FactorScores.Count, "FactorScores count should match");
        foreach (Variant factorKey in original.FactorScores.Keys)
        {
            DotNetNativeTestSuite.AssertTrue(restored.FactorScores.ContainsKey(factorKey), $"Should contain factor {factorKey}");
            DotNetNativeTestSuite.AssertEqual(original.FactorScores[factorKey], restored.FactorScores[factorKey], $"Factor {factorKey} score should match");
        }
    }

    /// <summary>
    /// Tests limiting_factors serialization.
    /// </summary>
    public static void TestLimitingFactorsSerialization()
    {
        ColonySuitability original = CreateTestSuitability();

        Godot.Collections.Dictionary data = original.ToDictionary();
        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.LimitingFactors.Count, restored.LimitingFactors.Count, "LimitingFactors count should match");
        for (int i = 0; i < original.LimitingFactors.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(original.LimitingFactors[i], restored.LimitingFactors[i], $"LimitingFactor {i} should match");
        }
    }

    /// <summary>
    /// Tests advantages serialization.
    /// </summary>
    public static void TestAdvantagesSerialization()
    {
        ColonySuitability original = CreateTestSuitability();

        Godot.Collections.Dictionary data = original.ToDictionary();
        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Advantages.Count, restored.Advantages.Count, "Advantages count should match");
        for (int i = 0; i < original.Advantages.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(original.Advantages[i], restored.Advantages[i], $"Advantage {i} should match");
        }
    }

    /// <summary>
    /// Tests from_dict handles JSON-style string keys.
    /// </summary>
    public static void TestFromDictJsonStringKeys()
    {
        ColonySuitability original = CreateTestSuitability();
        Godot.Collections.Dictionary data = original.ToDictionary();

        Godot.Collections.Dictionary jsonLikeFactors = new();
        foreach (Variant key in ((Godot.Collections.Dictionary)data["factor_scores"]).Keys)
        {
            jsonLikeFactors[key.ToString()] = ((Godot.Collections.Dictionary)data["factor_scores"])[key];
        }
        data["factor_scores"] = jsonLikeFactors;

        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.FactorScores.Count, restored.FactorScores.Count, "FactorScores count should match");
        foreach (Variant factorKey in original.FactorScores.Keys)
        {
            DotNetNativeTestSuite.AssertTrue(restored.FactorScores.ContainsKey(factorKey), $"Should contain factor {factorKey}");
        }
    }

    /// <summary>
    /// Tests empty suitability serialization.
    /// </summary>
    public static void TestEmptySuitabilitySerialization()
    {
        ColonySuitability original = new();

        Godot.Collections.Dictionary data = original.ToDictionary();
        ColonySuitability restored = ColonySuitability.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual("", restored.BodyId, "BodyId should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.OverallScore, "OverallScore should be 0");
        DotNetNativeTestSuite.AssertEqual(0, restored.FactorScores.Count, "FactorScores should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.LimitingFactors.Count, "LimitingFactors should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.Advantages.Count, "Advantages should be empty");
    }
}
