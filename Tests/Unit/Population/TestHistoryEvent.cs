#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for HistoryEvent data model.
/// </summary>
public static class TestHistoryEvent
{
    /// <summary>
    /// Tests basic event creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        HistoryEvent historyEvent = new();
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Founding, historyEvent.Type, "Default Type should be Founding");
        DotNetNativeTestSuite.AssertEqual(0, historyEvent.Year, "Default Year should be 0");
        DotNetNativeTestSuite.AssertEqual("", historyEvent.Title, "Default Title should be empty");
        DotNetNativeTestSuite.AssertEqual("", historyEvent.Description, "Default Description should be empty");
        DotNetNativeTestSuite.AssertFloatNear(0.0, historyEvent.Magnitude, 0.001, "Default Magnitude should be 0");
        DotNetNativeTestSuite.AssertEqual(0, historyEvent.PopulationDelta, "Default PopulationDelta should be 0");
        DotNetNativeTestSuite.AssertEqual("", historyEvent.RelatedPopulationId, "Default RelatedPopulationId should be empty");
    }

    /// <summary>
    /// Tests event creation with parameters.
    /// </summary>
    public static void TestCreationWithParams()
    {
        HistoryEvent historyEvent = new(
            HistoryEvent.EventType.War,
            -500,
            "The Great War",
            "A devastating conflict.",
            -0.7
        );
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.War, historyEvent.Type, "Type should be War");
        DotNetNativeTestSuite.AssertEqual(-500, historyEvent.Year, "Year should be -500");
        DotNetNativeTestSuite.AssertEqual("The Great War", historyEvent.Title, "Title should match");
        DotNetNativeTestSuite.AssertEqual("A devastating conflict.", historyEvent.Description, "Description should match");
        DotNetNativeTestSuite.AssertFloatNear(-0.7, historyEvent.Magnitude, 0.001, "Magnitude should be -0.7");
    }

    /// <summary>
    /// Tests magnitude clamping.
    /// </summary>
    public static void TestMagnitudeClamped()
    {
        HistoryEvent event1 = new(HistoryEvent.EventType.Founding, 0, "", "", 2.0);
        DotNetNativeTestSuite.AssertFloatNear(1.0, event1.Magnitude, 0.001, "Magnitude should be clamped to 1.0");

        HistoryEvent event2 = new(HistoryEvent.EventType.Founding, 0, "", "", -2.0);
        DotNetNativeTestSuite.AssertFloatNear(-1.0, event2.Magnitude, 0.001, "Magnitude should be clamped to -1.0");
    }

    /// <summary>
    /// Tests is_harmful method.
    /// </summary>
    public static void TestIsHarmful()
    {
        HistoryEvent historyEvent = new();

        historyEvent.Magnitude = -0.5;
        DotNetNativeTestSuite.AssertTrue(historyEvent.IsHarmful(), "Negative magnitude should be harmful");

        historyEvent.Magnitude = 0.0;
        DotNetNativeTestSuite.AssertFalse(historyEvent.IsHarmful(), "Zero magnitude should not be harmful");

        historyEvent.Magnitude = 0.5;
        DotNetNativeTestSuite.AssertFalse(historyEvent.IsHarmful(), "Positive magnitude should not be harmful");
    }

    /// <summary>
    /// Tests is_beneficial method.
    /// </summary>
    public static void TestIsBeneficial()
    {
        HistoryEvent historyEvent = new();

        historyEvent.Magnitude = 0.5;
        DotNetNativeTestSuite.AssertTrue(historyEvent.IsBeneficial(), "Positive magnitude should be beneficial");

        historyEvent.Magnitude = 0.0;
        DotNetNativeTestSuite.AssertFalse(historyEvent.IsBeneficial(), "Zero magnitude should not be beneficial");

        historyEvent.Magnitude = -0.5;
        DotNetNativeTestSuite.AssertFalse(historyEvent.IsBeneficial(), "Negative magnitude should not be beneficial");
    }

    /// <summary>
    /// Tests is_neutral method.
    /// </summary>
    public static void TestIsNeutral()
    {
        HistoryEvent historyEvent = new();

        historyEvent.Magnitude = 0.0;
        DotNetNativeTestSuite.AssertTrue(historyEvent.IsNeutral(), "Zero magnitude should be neutral");

        historyEvent.Magnitude = 0.1;
        DotNetNativeTestSuite.AssertFalse(historyEvent.IsNeutral(), "Non-zero magnitude should not be neutral");
    }

    /// <summary>
    /// Tests involves_other_population method.
    /// </summary>
    public static void TestInvolvesOtherPopulation()
    {
        HistoryEvent historyEvent = new();
        DotNetNativeTestSuite.AssertFalse(historyEvent.InvolvesOtherPopulation(), "Empty RelatedPopulationId should return false");

        historyEvent.RelatedPopulationId = "other_pop_001";
        DotNetNativeTestSuite.AssertTrue(historyEvent.InvolvesOtherPopulation(), "Non-empty RelatedPopulationId should return true");
    }

    /// <summary>
    /// Tests get_sort_key method.
    /// </summary>
    public static void TestGetSortKey()
    {
        HistoryEvent historyEvent = new();
        historyEvent.Year = -1000;
        DotNetNativeTestSuite.AssertEqual(-1000, historyEvent.GetSortKey(), "Sort key should match year");

        historyEvent.Year = 500;
        DotNetNativeTestSuite.AssertEqual(500, historyEvent.GetSortKey(), "Sort key should match year");
    }

    /// <summary>
    /// Tests type_to_string for all types.
    /// </summary>
    public static void TestTypeToString()
    {
        DotNetNativeTestSuite.AssertEqual("Founding", HistoryEvent.TypeToString(HistoryEvent.EventType.Founding), "Founding string should match");
        DotNetNativeTestSuite.AssertEqual("Natural Disaster", HistoryEvent.TypeToString(HistoryEvent.EventType.NaturalDisaster), "NaturalDisaster string should match");
        DotNetNativeTestSuite.AssertEqual("Plague", HistoryEvent.TypeToString(HistoryEvent.EventType.Plague), "Plague string should match");
        DotNetNativeTestSuite.AssertEqual("War", HistoryEvent.TypeToString(HistoryEvent.EventType.War), "War string should match");
        DotNetNativeTestSuite.AssertEqual("Golden Age", HistoryEvent.TypeToString(HistoryEvent.EventType.GoldenAge), "GoldenAge string should match");
        DotNetNativeTestSuite.AssertEqual("Technological Advancement", HistoryEvent.TypeToString(HistoryEvent.EventType.TechAdvancement), "TechAdvancement string should match");
    }

    /// <summary>
    /// Tests type_from_string.
    /// </summary>
    public static void TestTypeFromString()
    {
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Founding, HistoryEvent.TypeFromString("founding"), "Should parse founding");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.War, HistoryEvent.TypeFromString("War"), "Should parse War");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.GoldenAge, HistoryEvent.TypeFromString("GOLDEN_AGE"), "Should parse GOLDEN_AGE");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.NaturalDisaster, HistoryEvent.TypeFromString("natural_disaster"), "Should parse natural_disaster");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Founding, HistoryEvent.TypeFromString("invalid"), "Invalid should return Founding");
    }

    /// <summary>
    /// Tests is_typically_harmful.
    /// </summary>
    public static void TestIsTypicallyHarmful()
    {
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.NaturalDisaster), "NaturalDisaster should be harmful");
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.Plague), "Plague should be harmful");
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.War), "War should be harmful");
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.Collapse), "Collapse should be harmful");
        DotNetNativeTestSuite.AssertFalse(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.GoldenAge), "GoldenAge should not be harmful");
        DotNetNativeTestSuite.AssertFalse(HistoryEvent.IsTypicallyHarmful(HistoryEvent.EventType.Founding), "Founding should not be harmful");
    }

    /// <summary>
    /// Tests is_typically_beneficial.
    /// </summary>
    public static void TestIsTypicallyBeneficial()
    {
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyBeneficial(HistoryEvent.EventType.TechAdvancement), "TechAdvancement should be beneficial");
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyBeneficial(HistoryEvent.EventType.GoldenAge), "GoldenAge should be beneficial");
        DotNetNativeTestSuite.AssertTrue(HistoryEvent.IsTypicallyBeneficial(HistoryEvent.EventType.Expansion), "Expansion should be beneficial");
        DotNetNativeTestSuite.AssertFalse(HistoryEvent.IsTypicallyBeneficial(HistoryEvent.EventType.War), "War should not be beneficial");
        DotNetNativeTestSuite.AssertFalse(HistoryEvent.IsTypicallyBeneficial(HistoryEvent.EventType.Founding), "Founding should not be beneficial");
    }

    /// <summary>
    /// Tests type_count.
    /// </summary>
    public static void TestTypeCount()
    {
        DotNetNativeTestSuite.AssertEqual(20, HistoryEvent.TypeCount(), "Should have 20 event types");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        HistoryEvent original = new(
            HistoryEvent.EventType.War,
            -250,
            "The Border War",
            "A conflict over territory.",
            -0.6
        );
        original.PopulationDelta = -15000;
        original.RelatedPopulationId = "enemy_001";
        original.Metadata = new Godot.Collections.Dictionary { { "location", "northern_border" }, { "duration_years", 5 } };

        Godot.Collections.Dictionary data = original.ToDictionary();
        HistoryEvent restored = HistoryEvent.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Type, restored.Type, "Type should match");
        DotNetNativeTestSuite.AssertEqual(original.Year, restored.Year, "Year should match");
        DotNetNativeTestSuite.AssertEqual(original.Title, restored.Title, "Title should match");
        DotNetNativeTestSuite.AssertEqual(original.Description, restored.Description, "Description should match");
        DotNetNativeTestSuite.AssertFloatNear(original.Magnitude, restored.Magnitude, 0.001, "Magnitude should match");
        DotNetNativeTestSuite.AssertEqual(original.PopulationDelta, restored.PopulationDelta, "PopulationDelta should match");
        DotNetNativeTestSuite.AssertEqual(original.RelatedPopulationId, restored.RelatedPopulationId, "RelatedPopulationId should match");
        DotNetNativeTestSuite.AssertEqual(original.Metadata.Count, restored.Metadata.Count, "Metadata count should match");
    }

    /// <summary>
    /// Tests from_dict with string type value (JSON).
    /// </summary>
    public static void TestFromDictStringType()
    {
        Godot.Collections.Dictionary data = new()
        {
            { "event_type", "5" },
            { "year", 100 },
            { "title", "Test Event" },
        };
        HistoryEvent historyEvent = HistoryEvent.FromDictionary(data);
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.CivilWar, historyEvent.Type, "Type should be CivilWar (enum value 5)");
    }
}
