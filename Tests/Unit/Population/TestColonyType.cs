#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ColonyType enum and helpers.
/// </summary>
public static class TestColonyType
{
    /// <summary>
    /// Tests to_string_name.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Settlement", ColonyType.ToStringName(ColonyType.Type.Settlement), "Settlement string should match");
        DotNetNativeTestSuite.AssertEqual("Corporate", ColonyType.ToStringName(ColonyType.Type.Corporate), "Corporate string should match");
        DotNetNativeTestSuite.AssertEqual("Military", ColonyType.ToStringName(ColonyType.Type.Military), "Military string should match");
        DotNetNativeTestSuite.AssertEqual("Scientific", ColonyType.ToStringName(ColonyType.Type.Scientific), "Scientific string should match");
    }

    /// <summary>
    /// Tests from_string.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(ColonyType.Type.Settlement, ColonyType.FromString("settlement"), "Should parse settlement");
        DotNetNativeTestSuite.AssertEqual(ColonyType.Type.Corporate, ColonyType.FromString("Corporate"), "Should parse Corporate");
        DotNetNativeTestSuite.AssertEqual(ColonyType.Type.Military, ColonyType.FromString("MILITARY"), "Should parse MILITARY");
        DotNetNativeTestSuite.AssertEqual(ColonyType.Type.Settlement, ColonyType.FromString("invalid"), "Invalid should return Settlement");
    }

    /// <summary>
    /// Tests typical_starting_regime.
    /// </summary>
    public static void TestTypicalStartingRegime()
    {
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Corporate, ColonyType.TypicalStartingRegime(ColonyType.Type.Corporate), "Corporate should have Corporate regime");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.MilitaryJunta, ColonyType.TypicalStartingRegime(ColonyType.Type.Military), "Military should have MilitaryJunta regime");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Technocracy, ColonyType.TypicalStartingRegime(ColonyType.Type.Scientific), "Scientific should have Technocracy regime");
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Theocracy, ColonyType.TypicalStartingRegime(ColonyType.Type.Religious), "Religious should have Theocracy regime");
    }

    /// <summary>
    /// Tests typical_initial_population.
    /// </summary>
    public static void TestTypicalInitialPopulation()
    {
        int settlementPop = ColonyType.TypicalInitialPopulation(ColonyType.Type.Settlement);
        int scientificPop = ColonyType.TypicalInitialPopulation(ColonyType.Type.Scientific);
        int refugeePop = ColonyType.TypicalInitialPopulation(ColonyType.Type.Refugee);

        DotNetNativeTestSuite.AssertGreaterThan(settlementPop, scientificPop, "Settlements should have more pop than scientific");
        DotNetNativeTestSuite.AssertGreaterThan(refugeePop, settlementPop, "Refugee colonies should have high pop");
    }

    /// <summary>
    /// Tests growth_rate_modifier.
    /// </summary>
    public static void TestGrowthRateModifier()
    {
        double settlementMod = ColonyType.GrowthRateModifier(ColonyType.Type.Settlement);
        double militaryMod = ColonyType.GrowthRateModifier(ColonyType.Type.Military);
        double religiousMod = ColonyType.GrowthRateModifier(ColonyType.Type.Religious);

        DotNetNativeTestSuite.AssertFloatNear(1.0, settlementMod, 0.01, "Settlement should have base growth");
        DotNetNativeTestSuite.AssertLessThan(militaryMod, settlementMod, "Military should have lower growth");
        DotNetNativeTestSuite.AssertGreaterThan(religiousMod, settlementMod, "Religious should have higher growth");
    }

    /// <summary>
    /// Tests tends_toward_native_conflict.
    /// </summary>
    public static void TestTendsTowardNativeConflict()
    {
        DotNetNativeTestSuite.AssertTrue(ColonyType.TendsTowardNativeConflict(ColonyType.Type.Corporate), "Corporate should tend toward conflict");
        DotNetNativeTestSuite.AssertTrue(ColonyType.TendsTowardNativeConflict(ColonyType.Type.Military), "Military should tend toward conflict");
        DotNetNativeTestSuite.AssertFalse(ColonyType.TendsTowardNativeConflict(ColonyType.Type.Scientific), "Scientific should not tend toward conflict");
        DotNetNativeTestSuite.AssertFalse(ColonyType.TendsTowardNativeConflict(ColonyType.Type.Religious), "Religious should not tend toward conflict");
    }

    /// <summary>
    /// Tests count.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(10, ColonyType.Count(), "Should have 10 colony types");
    }
}
