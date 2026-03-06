#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for GovernmentType enum and helpers.
/// </summary>
public static class TestGovernmentType
{
    /// <summary>
    /// Tests to_string_name for all regimes.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Tribal", GovernmentType.ToStringName(GovernmentType.Regime.Tribal), "Tribal string should match");
        DotNetNativeTestSuite.AssertEqual("Mass Democracy", GovernmentType.ToStringName(GovernmentType.Regime.MassDemocracy), "MassDemocracy string should match");
        DotNetNativeTestSuite.AssertEqual("Military Junta", GovernmentType.ToStringName(GovernmentType.Regime.MilitaryJunta), "MilitaryJunta string should match");
        DotNetNativeTestSuite.AssertEqual("Corporate Governance", GovernmentType.ToStringName(GovernmentType.Regime.Corporate), "Corporate string should match");
    }

    /// <summary>
    /// Tests from_string conversion.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Tribal, GovernmentType.FromString("tribal"), "Should parse tribal");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.MassDemocracy, GovernmentType.FromString("Mass Democracy"), "Should parse Mass Democracy");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.MilitaryJunta, GovernmentType.FromString("military_junta"), "Should parse military_junta");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Tribal, GovernmentType.FromString("invalid"), "Invalid should return Tribal");
    }

    /// <summary>
    /// Tests native_starting_regimes.
    /// </summary>
    public static void TestNativeStartingRegimes()
    {
        Array<GovernmentType.Regime> regimes = GovernmentType.NativeStartingRegimes();
        DotNetNativeTestSuite.AssertTrue(regimes.Contains(GovernmentType.Regime.Tribal), "Should contain Tribal");
        DotNetNativeTestSuite.AssertTrue(regimes.Contains(GovernmentType.Regime.Chiefdom), "Should contain Chiefdom");
        DotNetNativeTestSuite.AssertFalse(regimes.Contains(GovernmentType.Regime.Corporate), "Should not contain Corporate");
    }

    /// <summary>
    /// Tests colony_starting_regimes.
    /// </summary>
    public static void TestColonyStartingRegimes()
    {
        Array<GovernmentType.Regime> regimes = GovernmentType.ColonyStartingRegimes();
        DotNetNativeTestSuite.AssertTrue(regimes.Contains(GovernmentType.Regime.Corporate), "Should contain Corporate");
        DotNetNativeTestSuite.AssertTrue(regimes.Contains(GovernmentType.Regime.MilitaryJunta), "Should contain MilitaryJunta");
        DotNetNativeTestSuite.AssertFalse(regimes.Contains(GovernmentType.Regime.Tribal), "Should not contain Tribal");
    }

    /// <summary>
    /// Tests baseline_transitions returns valid options.
    /// </summary>
    public static void TestBaselineTransitions()
    {
        Array<GovernmentType.Regime> tribalNext = GovernmentType.BaselineTransitions(GovernmentType.Regime.Tribal);
        DotNetNativeTestSuite.AssertTrue(tribalNext.Contains(GovernmentType.Regime.Chiefdom), "Tribal should transition to Chiefdom");

        Array<GovernmentType.Regime> democracyNext = GovernmentType.BaselineTransitions(GovernmentType.Regime.MassDemocracy);
        DotNetNativeTestSuite.AssertGreaterThan(democracyNext.Count, 0, "Democracy should have transitions");
    }

    /// <summary>
    /// Tests crisis_transitions returns valid options.
    /// </summary>
    public static void TestCrisisTransitions()
    {
        Array<GovernmentType.Regime> democracyCrisis = GovernmentType.CrisisTransitions(GovernmentType.Regime.MassDemocracy);
        DotNetNativeTestSuite.AssertTrue(democracyCrisis.Contains(GovernmentType.Regime.MilitaryJunta), "Democracy should transition to MilitaryJunta in crisis");
    }

    /// <summary>
    /// Tests is_authoritarian.
    /// </summary>
    public static void TestIsAuthoritarian()
    {
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsAuthoritarian(GovernmentType.Regime.AbsoluteMonarchy), "AbsoluteMonarchy should be authoritarian");
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsAuthoritarian(GovernmentType.Regime.MilitaryJunta), "MilitaryJunta should be authoritarian");
        DotNetNativeTestSuite.AssertFalse(GovernmentType.IsAuthoritarian(GovernmentType.Regime.MassDemocracy), "MassDemocracy should not be authoritarian");
        DotNetNativeTestSuite.AssertFalse(GovernmentType.IsAuthoritarian(GovernmentType.Regime.Tribal), "Tribal should not be authoritarian");
    }

    /// <summary>
    /// Tests is_participatory.
    /// </summary>
    public static void TestIsParticipatory()
    {
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsParticipatory(GovernmentType.Regime.MassDemocracy), "MassDemocracy should be participatory");
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsParticipatory(GovernmentType.Regime.Tribal), "Tribal should be participatory");
        DotNetNativeTestSuite.AssertFalse(GovernmentType.IsParticipatory(GovernmentType.Regime.AbsoluteMonarchy), "AbsoluteMonarchy should not be participatory");
    }

    /// <summary>
    /// Tests is_unstable.
    /// </summary>
    public static void TestIsUnstable()
    {
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsUnstable(GovernmentType.Regime.FailedState), "FailedState should be unstable");
        DotNetNativeTestSuite.AssertTrue(GovernmentType.IsUnstable(GovernmentType.Regime.MilitaryJunta), "MilitaryJunta should be unstable");
        DotNetNativeTestSuite.AssertFalse(GovernmentType.IsUnstable(GovernmentType.Regime.MassDemocracy), "MassDemocracy should not be unstable");
    }

    /// <summary>
    /// Tests count.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(18, GovernmentType.Count(), "Should have 18 regime types");
    }
}
