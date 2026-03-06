#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for HabitabilityCategory enum and utilities.
/// </summary>
public static class TestHabitabilityCategory
{
    /// <summary>
    /// Tests from_score returns correct categories.
    /// </summary>
    public static void TestFromScore()
    {
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, HabitabilityCategory.FromScore(0), "Score 0 should be Impossible");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Hostile, HabitabilityCategory.FromScore(1), "Score 1 should be Hostile");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Hostile, HabitabilityCategory.FromScore(2), "Score 2 should be Hostile");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Harsh, HabitabilityCategory.FromScore(3), "Score 3 should be Harsh");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Harsh, HabitabilityCategory.FromScore(4), "Score 4 should be Harsh");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Marginal, HabitabilityCategory.FromScore(5), "Score 5 should be Marginal");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Marginal, HabitabilityCategory.FromScore(6), "Score 6 should be Marginal");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Challenging, HabitabilityCategory.FromScore(7), "Score 7 should be Challenging");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Comfortable, HabitabilityCategory.FromScore(8), "Score 8 should be Comfortable");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Comfortable, HabitabilityCategory.FromScore(9), "Score 9 should be Comfortable");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Ideal, HabitabilityCategory.FromScore(10), "Score 10 should be Ideal");
    }

    /// <summary>
    /// Tests from_score clamps out of range values.
    /// </summary>
    public static void TestFromScoreClamping()
    {
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, HabitabilityCategory.FromScore(-5), "Negative score should clamp to Impossible");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Ideal, HabitabilityCategory.FromScore(15), "High score should clamp to Ideal");
    }

    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Impossible", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Impossible), "Impossible name should match");
        DotNetNativeTestSuite.AssertEqual("Hostile", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Hostile), "Hostile name should match");
        DotNetNativeTestSuite.AssertEqual("Harsh", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Harsh), "Harsh name should match");
        DotNetNativeTestSuite.AssertEqual("Marginal", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Marginal), "Marginal name should match");
        DotNetNativeTestSuite.AssertEqual("Challenging", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Challenging), "Challenging name should match");
        DotNetNativeTestSuite.AssertEqual("Comfortable", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Comfortable), "Comfortable name should match");
        DotNetNativeTestSuite.AssertEqual("Ideal", HabitabilityCategory.ToStringName(HabitabilityCategory.Category.Ideal), "Ideal name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, HabitabilityCategory.FromString("impossible"), "Should parse impossible");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Hostile, HabitabilityCategory.FromString("HOSTILE"), "Should parse HOSTILE");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Ideal, HabitabilityCategory.FromString("Ideal"), "Should parse Ideal");
    }

    /// <summary>
    /// Tests from_string returns IMPOSSIBLE for unknown values.
    /// </summary>
    public static void TestFromStringUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, HabitabilityCategory.FromString("unknown"), "Unknown should return Impossible");
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, HabitabilityCategory.FromString(""), "Empty should return Impossible");
    }

    /// <summary>
    /// Tests allows_unassisted_survival returns expected values.
    /// </summary>
    public static void TestAllowsUnassistedSurvival()
    {
        DotNetNativeTestSuite.AssertFalse(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Impossible), "Impossible should not allow survival");
        DotNetNativeTestSuite.AssertFalse(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Hostile), "Hostile should not allow survival");
        DotNetNativeTestSuite.AssertFalse(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Harsh), "Harsh should not allow survival");
        DotNetNativeTestSuite.AssertTrue(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Marginal), "Marginal should allow survival");
        DotNetNativeTestSuite.AssertTrue(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Challenging), "Challenging should allow survival");
        DotNetNativeTestSuite.AssertTrue(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Comfortable), "Comfortable should allow survival");
        DotNetNativeTestSuite.AssertTrue(HabitabilityCategory.AllowsUnassistedSurvival(HabitabilityCategory.Category.Ideal), "Ideal should allow survival");
    }

    /// <summary>
    /// Tests get_description returns non-empty strings.
    /// </summary>
    public static void TestGetDescription()
    {
        for (int catInt = 0; catInt < HabitabilityCategory.Count(); catInt += 1)
        {
            HabitabilityCategory.Category cat = (HabitabilityCategory.Category)catInt;
            string desc = HabitabilityCategory.GetDescription(cat);
            DotNetNativeTestSuite.AssertTrue(desc.Length > 0, $"Description should not be empty for category {catInt}");
        }
    }

    /// <summary>
    /// Tests count returns correct number.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(7, HabitabilityCategory.Count(), "Should have 7 habitability categories");
    }
}
