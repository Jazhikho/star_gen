#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for Colony data model.
/// </summary>
public static class TestColony
{
    /// <summary>
    /// Creates a test colony.
    /// </summary>
    private static Colony CreateTestColony()
    {
        Colony colony = new();
        colony.Id = "test_colony_001";
        colony.Name = "New Terra";
        colony.BodyId = "planet_001";
        colony.Type = ColonyType.Type.Settlement;
        colony.FoundingCivilizationId = "civ_001";
        colony.FoundingCivilizationName = "Human Federation";
        colony.FoundingYear = -200;
        colony.Population = 500000;
        colony.PeakPopulation = 600000;
        colony.PeakPopulationYear = -50;
        colony.TechLevel = TechnologyLevel.Level.Interstellar;
        colony.IsActive = true;
        colony.TerritorialControl = 0.3;
        colony.PrimaryIndustry = "mixed economy";
        colony.SelfSufficiency = 0.7;

        colony.Government.Regime = GovernmentType.Regime.Constitutional;
        colony.Government.Legitimacy = 0.8;

        colony.History.AddNewEvent(
            HistoryEvent.EventType.Founding,
            -200,
            "Colony Founded",
            "The colony was established"
        );

        return colony;
    }

    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        Colony colony = new();
        DotNetNativeTestSuite.AssertEqual("", colony.Id, "Default ID should be empty");
        DotNetNativeTestSuite.AssertEqual(ColonyType.Type.Settlement, colony.Type, "Default type should be Settlement");
        DotNetNativeTestSuite.AssertEqual(0, colony.Population, "Default population should be 0");
        DotNetNativeTestSuite.AssertTrue(colony.IsActive, "Default should be active");
        DotNetNativeTestSuite.AssertFalse(colony.IsIndependent, "Default should not be independent");
        DotNetNativeTestSuite.AssertNotNull(colony.Government, "Government should not be null");
        DotNetNativeTestSuite.AssertNotNull(colony.History, "History should not be null");
    }

    /// <summary>
    /// Tests get_age for active colony.
    /// </summary>
    public static void TestGetAgeActive()
    {
        Colony colony = CreateTestColony();
        colony.FoundingYear = -200;

        int age = colony.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(200, age, "Age should be 200 years");
    }

    /// <summary>
    /// Tests get_age for abandoned colony.
    /// </summary>
    public static void TestGetAgeAbandoned()
    {
        Colony colony = CreateTestColony();
        colony.FoundingYear = -200;
        colony.IsActive = false;
        colony.AbandonmentYear = -50;

        int age = colony.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(150, age, "Age should be 150 years");
    }

    /// <summary>
    /// Tests get_growth_state.
    /// </summary>
    public static void TestGetGrowthState()
    {
        Colony colony = CreateTestColony();

        colony.Population = 600000;
        colony.PeakPopulation = 600000;
        DotNetNativeTestSuite.AssertEqual("growing", colony.GetGrowthState(), "Should be growing");

        colony.Population = 400000;
        DotNetNativeTestSuite.AssertEqual("stable", colony.GetGrowthState(), "Should be stable");

        colony.Population = 200000;
        DotNetNativeTestSuite.AssertEqual("declining", colony.GetGrowthState(), "Should be declining");

        colony.IsActive = false;
        DotNetNativeTestSuite.AssertEqual("abandoned", colony.GetGrowthState(), "Should be abandoned");
    }

    /// <summary>
    /// Tests get_regime.
    /// </summary>
    public static void TestGetRegime()
    {
        Colony colony = CreateTestColony();
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Constitutional, colony.GetRegime(), "Regime should match");
    }

    /// <summary>
    /// Tests is_politically_stable.
    /// </summary>
    public static void TestIsPoliticallyStable()
    {
        Colony colony = CreateTestColony();
        colony.Government.Legitimacy = 0.8;
        DotNetNativeTestSuite.AssertTrue(colony.IsPoliticallyStable(), "Should be stable with high legitimacy");

        colony.Government.Legitimacy = 0.1;
        DotNetNativeTestSuite.AssertFalse(colony.IsPoliticallyStable(), "Should not be stable with low legitimacy");
    }

    /// <summary>
    /// Tests native relation management.
    /// </summary>
    public static void TestNativeRelations()
    {
        Colony colony = CreateTestColony();
        DotNetNativeTestSuite.AssertFalse(colony.HasNativeRelations(), "Should have no relations initially");

        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -150, 20);
        colony.SetNativeRelation(relation);

        DotNetNativeTestSuite.AssertTrue(colony.HasNativeRelations(), "Should have relations");
        DotNetNativeTestSuite.AssertNotNull(colony.GetNativeRelation("native_001"), "Should find native_001");
        DotNetNativeTestSuite.AssertNull(colony.GetNativeRelation("nonexistent"), "Should not find nonexistent");
    }

    /// <summary>
    /// Tests get_all_native_relations.
    /// </summary>
    public static void TestGetAllNativeRelations()
    {
        Colony colony = CreateTestColony();

        NativeRelation rel1 = NativeRelation.CreateFirstContact("native_001", -150, 20);
        NativeRelation rel2 = NativeRelation.CreateFirstContact("native_002", -100, -30);
        colony.SetNativeRelation(rel1);
        colony.SetNativeRelation(rel2);

        Array<NativeRelation> allRelations = colony.GetAllNativeRelations();
        DotNetNativeTestSuite.AssertEqual(2, allRelations.Count, "Should have 2 relations");
    }

    /// <summary>
    /// Tests has_hostile_native_relations.
    /// </summary>
    public static void TestHasHostileNativeRelations()
    {
        Colony colony = CreateTestColony();

        NativeRelation peaceful = NativeRelation.CreateFirstContact("native_001", -150, 50);
        peaceful.CurrentStatus = NativeRelation.Status.Peaceful;
        colony.SetNativeRelation(peaceful);

        DotNetNativeTestSuite.AssertFalse(colony.HasHostileNativeRelations(), "Should not have hostile relations");

        NativeRelation hostile = NativeRelation.CreateFirstContact("native_002", -100, -80);
        hostile.CurrentStatus = NativeRelation.Status.Hostile;
        colony.SetNativeRelation(hostile);

        DotNetNativeTestSuite.AssertTrue(colony.HasHostileNativeRelations(), "Should have hostile relations");
    }

    /// <summary>
    /// Tests get_overall_native_status.
    /// </summary>
    public static void TestGetOverallNativeStatus()
    {
        Colony colony = CreateTestColony();
        DotNetNativeTestSuite.AssertEqual("none", colony.GetOverallNativeStatus(), "Should be none initially");

        NativeRelation peaceful = NativeRelation.CreateFirstContact("native_001", -150, 50);
        peaceful.CurrentStatus = NativeRelation.Status.Peaceful;
        colony.SetNativeRelation(peaceful);
        DotNetNativeTestSuite.AssertEqual("peaceful", colony.GetOverallNativeStatus(), "Should be peaceful");

        NativeRelation hostile = NativeRelation.CreateFirstContact("native_002", -100, -80);
        hostile.CurrentStatus = NativeRelation.Status.Hostile;
        colony.SetNativeRelation(hostile);
        DotNetNativeTestSuite.AssertEqual("mixed", colony.GetOverallNativeStatus(), "Should be mixed");
    }

    /// <summary>
    /// Tests record_abandonment.
    /// </summary>
    public static void TestRecordAbandonment()
    {
        Colony colony = CreateTestColony();
        DotNetNativeTestSuite.AssertTrue(colony.IsActive, "Should start active");

        colony.RecordAbandonment(-50, "resource depletion");

        DotNetNativeTestSuite.AssertFalse(colony.IsActive, "Should be inactive");
        DotNetNativeTestSuite.AssertEqual(-50, colony.AbandonmentYear, "Abandonment year should match");
        DotNetNativeTestSuite.AssertEqual("resource depletion", colony.AbandonmentReason, "Abandonment reason should match");
        DotNetNativeTestSuite.AssertEqual(0, colony.Population, "Population should be 0");
    }

    /// <summary>
    /// Tests record_independence.
    /// </summary>
    public static void TestRecordIndependence()
    {
        Colony colony = CreateTestColony();
        DotNetNativeTestSuite.AssertFalse(colony.IsIndependent, "Should start dependent");

        colony.RecordIndependence(-30);

        DotNetNativeTestSuite.AssertTrue(colony.IsIndependent, "Should be independent");
        DotNetNativeTestSuite.AssertEqual(-30, colony.IndependenceYear, "Independence year should match");
    }

    /// <summary>
    /// Tests update_peak_population.
    /// </summary>
    public static void TestUpdatePeakPopulation()
    {
        Colony colony = CreateTestColony();
        colony.Population = 700000;
        colony.PeakPopulation = 600000;

        colony.UpdatePeakPopulation(-10);

        DotNetNativeTestSuite.AssertEqual(700000, colony.PeakPopulation, "Peak should update");
        DotNetNativeTestSuite.AssertEqual(-10, colony.PeakPopulationYear, "Peak year should update");
    }

    /// <summary>
    /// Tests get_summary.
    /// </summary>
    public static void TestGetSummary()
    {
        Colony colony = CreateTestColony();
        Godot.Collections.Dictionary summary = colony.GetSummary();

        DotNetNativeTestSuite.AssertEqual("test_colony_001", summary["id"].AsString(), "ID should match");
        DotNetNativeTestSuite.AssertEqual("New Terra", summary["name"].AsString(), "Name should match");
        DotNetNativeTestSuite.AssertEqual("Settlement", summary["colony_type"].AsString(), "Colony type should match");
        DotNetNativeTestSuite.AssertEqual(500000, summary["population"].AsInt32(), "Population should match");
        DotNetNativeTestSuite.AssertTrue(summary["is_active"].AsBool(), "Should be active");
        DotNetNativeTestSuite.AssertFalse(summary["is_independent"].AsBool(), "Should not be independent");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        Colony original = CreateTestColony();

        NativeRelation rel = NativeRelation.CreateFirstContact("native_001", -150, 30);
        original.SetNativeRelation(rel);

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Id, restored.Id, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.Name, restored.Name, "Name should match");
        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.Type, restored.Type, "Type should match");
        DotNetNativeTestSuite.AssertEqual(original.FoundingCivilizationId, restored.FoundingCivilizationId, "FoundingCivilizationId should match");
        DotNetNativeTestSuite.AssertEqual(original.FoundingYear, restored.FoundingYear, "FoundingYear should match");
        DotNetNativeTestSuite.AssertEqual(original.Population, restored.Population, "Population should match");
        DotNetNativeTestSuite.AssertEqual(original.TechLevel, restored.TechLevel, "TechLevel should match");
        DotNetNativeTestSuite.AssertEqual(original.IsActive, restored.IsActive, "IsActive should match");
        DotNetNativeTestSuite.AssertFloatNear(original.TerritorialControl, restored.TerritorialControl, 0.001, "TerritorialControl should match");
        DotNetNativeTestSuite.AssertFloatNear(original.SelfSufficiency, restored.SelfSufficiency, 0.001, "SelfSufficiency should match");
    }

    /// <summary>
    /// Tests government serialization.
    /// </summary>
    public static void TestGovernmentSerialization()
    {
        Colony original = CreateTestColony();

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Government.Regime, restored.Government.Regime, "Regime should match");
        DotNetNativeTestSuite.AssertFloatNear(original.Government.Legitimacy, restored.Government.Legitimacy, 0.001, "Legitimacy should match");
    }

    /// <summary>
    /// Tests history serialization.
    /// </summary>
    public static void TestHistorySerialization()
    {
        Colony original = CreateTestColony();

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.History.GetAllEvents().Count, restored.History.GetAllEvents().Count, "History size should match");
    }

    /// <summary>
    /// Tests native_relations serialization.
    /// </summary>
    public static void TestNativeRelationsSerialization()
    {
        Colony original = CreateTestColony();

        NativeRelation rel1 = NativeRelation.CreateFirstContact("native_001", -150, 30);
        NativeRelation rel2 = NativeRelation.CreateFirstContact("native_002", -100, -20);
        original.SetNativeRelation(rel1);
        original.SetNativeRelation(rel2);

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(2, restored.NativeRelations.Count, "Should have 2 relations");
        DotNetNativeTestSuite.AssertNotNull(restored.GetNativeRelation("native_001"), "Should have native_001");
        DotNetNativeTestSuite.AssertNotNull(restored.GetNativeRelation("native_002"), "Should have native_002");
    }

    /// <summary>
    /// Tests abandoned colony serialization.
    /// </summary>
    public static void TestAbandonedColonySerialization()
    {
        Colony original = CreateTestColony();
        original.RecordAbandonment(-50, "hostile natives");

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertFalse(restored.IsActive, "Should be inactive");
        DotNetNativeTestSuite.AssertEqual(-50, restored.AbandonmentYear, "Abandonment year should match");
        DotNetNativeTestSuite.AssertEqual("hostile natives", restored.AbandonmentReason, "Abandonment reason should match");
    }

    /// <summary>
    /// Tests independent colony serialization.
    /// </summary>
    public static void TestIndependentColonySerialization()
    {
        Colony original = CreateTestColony();
        original.RecordIndependence(-30);

        Godot.Collections.Dictionary data = original.ToDictionary();
        Colony restored = Colony.FromDictionary(data);

        DotNetNativeTestSuite.AssertTrue(restored.IsIndependent, "Should be independent");
        DotNetNativeTestSuite.AssertEqual(-30, restored.IndependenceYear, "Independence year should match");
    }
}
