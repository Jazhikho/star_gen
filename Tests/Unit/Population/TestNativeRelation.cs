#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for NativeRelation data model.
/// </summary>
public static class TestNativeRelation
{
    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        NativeRelation relation = new();
        DotNetNativeTestSuite.AssertEqual("", relation.NativePopulationId, "Default ID should be empty");
        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Unknown, relation.CurrentStatus, "Default status should be Unknown");
        DotNetNativeTestSuite.AssertEqual(0, relation.RelationScore, "Default score should be 0");
        DotNetNativeTestSuite.AssertFalse(relation.HasTreaty, "Default should have no treaty");
    }

    /// <summary>
    /// Tests create_first_contact.
    /// </summary>
    public static void TestCreateFirstContact()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 20);

        DotNetNativeTestSuite.AssertEqual("native_001", relation.NativePopulationId, "ID should match");
        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.FirstContact, relation.CurrentStatus, "Status should be FirstContact");
        DotNetNativeTestSuite.AssertEqual(-100, relation.FirstContactYear, "First contact year should match");
        DotNetNativeTestSuite.AssertEqual(20, relation.RelationScore, "Relation score should match");
        DotNetNativeTestSuite.AssertGreaterThan(relation.RelationshipEvents.Count, 0, "Should have relationship events");
    }

    /// <summary>
    /// Tests create_first_contact clamps disposition.
    /// </summary>
    public static void TestCreateFirstContactClamps()
    {
        NativeRelation relation1 = NativeRelation.CreateFirstContact("native_001", 0, 200);
        DotNetNativeTestSuite.AssertEqual(100, relation1.RelationScore, "Should clamp to 100");

        NativeRelation relation2 = NativeRelation.CreateFirstContact("native_002", 0, -200);
        DotNetNativeTestSuite.AssertEqual(-100, relation2.RelationScore, "Should clamp to -100");
    }

    /// <summary>
    /// Tests update_status to peaceful.
    /// </summary>
    public static void TestUpdateStatusPeaceful()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 30);
        relation.UpdateStatus();

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Peaceful, relation.CurrentStatus, "Status should be Peaceful");
    }

    /// <summary>
    /// Tests update_status to trading.
    /// </summary>
    public static void TestUpdateStatusTrading()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 50);
        relation.TradeLevel = 0.5;
        relation.UpdateStatus();

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Trading, relation.CurrentStatus, "Status should be Trading");
    }

    /// <summary>
    /// Tests update_status to hostile.
    /// </summary>
    public static void TestUpdateStatusHostile()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, -30);
        relation.ConflictIntensity = 0.7;
        relation.UpdateStatus();

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Hostile, relation.CurrentStatus, "Status should be Hostile");
    }

    /// <summary>
    /// Tests update_status to subjugated.
    /// </summary>
    public static void TestUpdateStatusSubjugated()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 0);
        relation.TerritoryTaken = 0.9;
        relation.UpdateStatus();

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Subjugated, relation.CurrentStatus, "Status should be Subjugated");
    }

    /// <summary>
    /// Tests update_status to integrated.
    /// </summary>
    public static void TestUpdateStatusIntegrated()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 70);
        relation.CulturalExchange = 0.8;
        relation.UpdateStatus();

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Integrated, relation.CurrentStatus, "Status should be Integrated");
    }

    /// <summary>
    /// Tests record_extinction.
    /// </summary>
    public static void TestRecordExtinction()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 0);
        relation.RecordExtinction(-50, "plague");

        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Extinct, relation.CurrentStatus, "Status should be Extinct");
        DotNetNativeTestSuite.AssertGreaterThan(relation.RelationshipEvents.Count, 1, "Should have multiple events");
    }

    /// <summary>
    /// Tests record_treaty.
    /// </summary>
    public static void TestRecordTreaty()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 20);
        relation.RecordTreaty(-50, "Peace treaty");

        DotNetNativeTestSuite.AssertTrue(relation.HasTreaty, "Should have treaty");
        DotNetNativeTestSuite.AssertEqual(-50, relation.TreatyYear, "Treaty year should match");
        DotNetNativeTestSuite.AssertEqual(40, relation.RelationScore, "Relation score should increase by 20");
    }

    /// <summary>
    /// Tests record_conflict.
    /// </summary>
    public static void TestRecordConflict()
    {
        NativeRelation relation = NativeRelation.CreateFirstContact("native_001", -100, 20);
        relation.HasTreaty = true;
        relation.RecordConflict(-50, "Border war", 0.6);

        DotNetNativeTestSuite.AssertFalse(relation.HasTreaty, "Treaty should be broken");
        DotNetNativeTestSuite.AssertFloatNear(0.6, relation.ConflictIntensity, 0.01, "Conflict intensity should match");
        DotNetNativeTestSuite.AssertLessThan(relation.RelationScore, 20, "Relation score should decrease");
    }

    /// <summary>
    /// Tests is_positive.
    /// </summary>
    public static void TestIsPositive()
    {
        NativeRelation relation = new();

        relation.RelationScore = 10;
        DotNetNativeTestSuite.AssertTrue(relation.IsPositive(), "Score 10 should be positive");

        relation.RelationScore = 0;
        DotNetNativeTestSuite.AssertFalse(relation.IsPositive(), "Score 0 should not be positive");

        relation.RelationScore = -10;
        DotNetNativeTestSuite.AssertFalse(relation.IsPositive(), "Score -10 should not be positive");
    }

    /// <summary>
    /// Tests is_hostile.
    /// </summary>
    public static void TestIsHostile()
    {
        NativeRelation relation = new();

        relation.CurrentStatus = NativeRelation.Status.Hostile;
        DotNetNativeTestSuite.AssertTrue(relation.IsHostile(), "Hostile status should be hostile");

        relation.CurrentStatus = NativeRelation.Status.Tense;
        relation.RelationScore = -50;
        DotNetNativeTestSuite.AssertTrue(relation.IsHostile(), "Tense with low score should be hostile");

        relation.CurrentStatus = NativeRelation.Status.Tense;
        relation.RelationScore = -10;
        DotNetNativeTestSuite.AssertFalse(relation.IsHostile(), "Tense with moderate score should not be hostile");
    }

    /// <summary>
    /// Tests has_active_trade.
    /// </summary>
    public static void TestHasActiveTrade()
    {
        NativeRelation relation = new();

        relation.CurrentStatus = NativeRelation.Status.Trading;
        relation.TradeLevel = 0.5;
        DotNetNativeTestSuite.AssertTrue(relation.HasActiveTrade(), "Trading status with trade level should have active trade");

        relation.CurrentStatus = NativeRelation.Status.Peaceful;
        DotNetNativeTestSuite.AssertFalse(relation.HasActiveTrade(), "Peaceful status should not have active trade");
    }

    /// <summary>
    /// Tests status_to_string.
    /// </summary>
    public static void TestStatusToString()
    {
        DotNetNativeTestSuite.AssertEqual("Unknown", NativeRelation.StatusToString(NativeRelation.Status.Unknown), "Unknown string should match");
        DotNetNativeTestSuite.AssertEqual("First Contact", NativeRelation.StatusToString(NativeRelation.Status.FirstContact), "FirstContact string should match");
        DotNetNativeTestSuite.AssertEqual("Hostile", NativeRelation.StatusToString(NativeRelation.Status.Hostile), "Hostile string should match");
    }

    /// <summary>
    /// Tests status_from_string.
    /// </summary>
    public static void TestStatusFromString()
    {
        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Unknown, NativeRelation.StatusFromString("unknown"), "Should parse unknown");
        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.FirstContact, NativeRelation.StatusFromString("First Contact"), "Should parse First Contact");
        DotNetNativeTestSuite.AssertEqual(NativeRelation.Status.Hostile, NativeRelation.StatusFromString("hostile"), "Should parse hostile");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        NativeRelation original = NativeRelation.CreateFirstContact("native_001", -100, 30);
        original.CurrentStatus = NativeRelation.Status.Trading;
        original.TradeLevel = 0.5;
        original.CulturalExchange = 0.3;
        original.TerritoryTaken = 0.1;
        original.HasTreaty = true;
        original.TreatyYear = -50;

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativeRelation restored = NativeRelation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.NativePopulationId, restored.NativePopulationId, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.CurrentStatus, restored.CurrentStatus, "Status should match");
        DotNetNativeTestSuite.AssertEqual(original.FirstContactYear, restored.FirstContactYear, "First contact year should match");
        DotNetNativeTestSuite.AssertEqual(original.RelationScore, restored.RelationScore, "Relation score should match");
        DotNetNativeTestSuite.AssertEqual(original.HasTreaty, restored.HasTreaty, "Treaty flag should match");
        DotNetNativeTestSuite.AssertFloatNear(original.TradeLevel, restored.TradeLevel, 0.001, "Trade level should match");
        DotNetNativeTestSuite.AssertFloatNear(original.CulturalExchange, restored.CulturalExchange, 0.001, "Cultural exchange should match");
    }

    /// <summary>
    /// Tests relationship_events serialization.
    /// </summary>
    public static void TestRelationshipEventsSerialization()
    {
        NativeRelation original = NativeRelation.CreateFirstContact("native_001", -100, 0);
        original.RecordTreaty(-50, "Peace");

        Godot.Collections.Dictionary data = original.ToDictionary();
        NativeRelation restored = NativeRelation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.RelationshipEvents.Count, restored.RelationshipEvents.Count, "Events count should match");
    }
}
