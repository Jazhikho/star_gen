#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for Government data model.
/// </summary>
public static class TestGovernment
{
    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        Government gov = new();
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Tribal, gov.Regime, "Default Regime should be Tribal");
        DotNetNativeTestSuite.AssertFloatNear(0.0, gov.CoercionCentralization, 0.001, "Default CoercionCentralization should be 0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, gov.AdministrativeCapacity, 0.001, "Default AdministrativeCapacity should be 0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, gov.PoliticalInclusiveness, 0.001, "Default PoliticalInclusiveness should be 0");
        DotNetNativeTestSuite.AssertFloatNear(0.5, gov.Legitimacy, 0.001, "Default Legitimacy should be 0.5");
    }

    /// <summary>
    /// Tests create_native_default.
    /// </summary>
    public static void TestCreateNativeDefault()
    {
        SeededRng rng = new(12345);
        Government gov = Government.CreateNativeDefault(rng);

        Array<GovernmentType.Regime> nativeRegimes = GovernmentType.NativeStartingRegimes();
        DotNetNativeTestSuite.AssertTrue(nativeRegimes.Contains(gov.Regime), "Should have native starting regime");

        DotNetNativeTestSuite.AssertLessThan(gov.CoercionCentralization, 0.3, "CoercionCentralization should be < 0.3");
        DotNetNativeTestSuite.AssertLessThan(gov.AdministrativeCapacity, 0.2, "AdministrativeCapacity should be < 0.2");
    }

    /// <summary>
    /// Tests create_colony_default.
    /// </summary>
    public static void TestCreateColonyDefault()
    {
        SeededRng rng = new(12345);
        Government gov = Government.CreateColonyDefault(rng);

        DotNetNativeTestSuite.AssertGreaterThan(gov.AdministrativeCapacity, 0.2, "Colony should have higher administrative capacity");
    }

    /// <summary>
    /// Tests create_colony_default with colony type hint.
    /// </summary>
    public static void TestCreateColonyDefaultCorporate()
    {
        SeededRng rng = new(12345);
        Government gov = Government.CreateColonyDefault(rng, "corporate");

        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Corporate, gov.Regime, "Should be Corporate regime");
    }

    /// <summary>
    /// Tests create_colony_default with military hint.
    /// </summary>
    public static void TestCreateColonyDefaultMilitary()
    {
        SeededRng rng = new(12345);
        Government gov = Government.CreateColonyDefault(rng, "military");

        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.MilitaryJunta, gov.Regime, "Should be MilitaryJunta regime");
    }

    /// <summary>
    /// Tests is_stable for stable regime.
    /// </summary>
    public static void TestIsStableTrue()
    {
        Government gov = new();
        gov.Regime = GovernmentType.Regime.MassDemocracy;
        gov.Legitimacy = 0.7;

        DotNetNativeTestSuite.AssertTrue(gov.IsStable(), "Should be stable");
    }

    /// <summary>
    /// Tests is_stable for unstable regime.
    /// </summary>
    public static void TestIsStableFalseUnstableRegime()
    {
        Government gov = new();
        gov.Regime = GovernmentType.Regime.FailedState;
        gov.Legitimacy = 0.8;

        DotNetNativeTestSuite.AssertFalse(gov.IsStable(), "FailedState should not be stable");
    }

    /// <summary>
    /// Tests is_stable with low legitimacy.
    /// </summary>
    public static void TestIsStableFalseLowLegitimacy()
    {
        Government gov = new();
        gov.Regime = GovernmentType.Regime.MassDemocracy;
        gov.Legitimacy = 0.2;

        DotNetNativeTestSuite.AssertFalse(gov.IsStable(), "Low legitimacy should not be stable");
    }

    /// <summary>
    /// Tests is_regime_change_likely with very low legitimacy.
    /// </summary>
    public static void TestIsRegimeChangeLikelyLowLegitimacy()
    {
        Government gov = new();
        gov.Legitimacy = 0.1;

        DotNetNativeTestSuite.AssertTrue(gov.IsRegimeChangeLikely(), "Very low legitimacy should make regime change likely");
    }

    /// <summary>
    /// Tests get_summary.
    /// </summary>
    public static void TestGetSummary()
    {
        Government gov = new();
        gov.Regime = GovernmentType.Regime.MassDemocracy;
        gov.CoercionCentralization = 0.5;
        gov.AdministrativeCapacity = 0.6;
        gov.PoliticalInclusiveness = 0.8;
        gov.Legitimacy = 0.7;

        Godot.Collections.Dictionary summary = gov.GetSummary();

        DotNetNativeTestSuite.AssertEqual("Mass Democracy", summary["regime"].AsString(), "Regime string should match");
        DotNetNativeTestSuite.AssertFloatNear(0.5, summary["coercion"].AsDouble(), 0.001, "Coercion should match");
        DotNetNativeTestSuite.AssertTrue(summary["stable"].AsBool(), "Should be stable");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        Government original = new();
        original.Regime = GovernmentType.Regime.Constitutional;
        original.CoercionCentralization = 0.5;
        original.AdministrativeCapacity = 0.6;
        original.PoliticalInclusiveness = 0.7;
        original.Legitimacy = 0.8;
        original.RegimeEstablishedYear = -100;
        original.Name = "The Republic";

        Godot.Collections.Dictionary data = original.ToDictionary();
        Government restored = Government.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Regime, restored.Regime, "Regime should match");
        DotNetNativeTestSuite.AssertFloatNear(original.CoercionCentralization, restored.CoercionCentralization, 0.001, "CoercionCentralization should match");
        DotNetNativeTestSuite.AssertFloatNear(original.AdministrativeCapacity, restored.AdministrativeCapacity, 0.001, "AdministrativeCapacity should match");
        DotNetNativeTestSuite.AssertFloatNear(original.PoliticalInclusiveness, restored.PoliticalInclusiveness, 0.001, "PoliticalInclusiveness should match");
        DotNetNativeTestSuite.AssertFloatNear(original.Legitimacy, restored.Legitimacy, 0.001, "Legitimacy should match");
        DotNetNativeTestSuite.AssertEqual(original.RegimeEstablishedYear, restored.RegimeEstablishedYear, "RegimeEstablishedYear should match");
        DotNetNativeTestSuite.AssertEqual(original.Name, restored.Name, "Name should match");
    }

    /// <summary>
    /// Tests from_dict handles JSON string regime.
    /// </summary>
    public static void TestFromDictStringRegime()
    {
        Godot.Collections.Dictionary data = new()
        {
            { "regime", "5" },
            { "legitimacy", 0.6 },
        };
        Government gov = Government.FromDictionary(data);
        DotNetNativeTestSuite.AssertEqual((GovernmentType.Regime)5, gov.Regime, "Should parse regime from string");
    }

    /// <summary>
    /// Tests slider clamping in from_dict.
    /// </summary>
    public static void TestFromDictClampsSliders()
    {
        Godot.Collections.Dictionary data = new()
        {
            { "coercion_centralization", 2.0 },
            { "administrative_capacity", -0.5 },
            { "political_inclusiveness", 0.5 },
            { "legitimacy", 1.5 },
        };
        Government gov = Government.FromDictionary(data);

        DotNetNativeTestSuite.AssertFloatNear(1.0, gov.CoercionCentralization, 0.001, "CoercionCentralization should be clamped to 1.0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, gov.AdministrativeCapacity, 0.001, "AdministrativeCapacity should be clamped to 0.0");
        DotNetNativeTestSuite.AssertFloatNear(1.0, gov.Legitimacy, 0.001, "Legitimacy should be clamped to 1.0");
    }
}
