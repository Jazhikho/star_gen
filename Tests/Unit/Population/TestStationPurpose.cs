#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationPurpose enum and utilities.
/// </summary>
public static class TestStationPurpose
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Utility", StationPurpose.ToStringName(StationPurpose.Purpose.Utility), "Utility name should match");
        DotNetNativeTestSuite.AssertEqual("Trade", StationPurpose.ToStringName(StationPurpose.Purpose.Trade), "Trade name should match");
        DotNetNativeTestSuite.AssertEqual("Military", StationPurpose.ToStringName(StationPurpose.Purpose.Military), "Military name should match");
        DotNetNativeTestSuite.AssertEqual("Science", StationPurpose.ToStringName(StationPurpose.Purpose.Science), "Science name should match");
        DotNetNativeTestSuite.AssertEqual("Mining", StationPurpose.ToStringName(StationPurpose.Purpose.Mining), "Mining name should match");
        DotNetNativeTestSuite.AssertEqual("Residential", StationPurpose.ToStringName(StationPurpose.Purpose.Residential), "Residential name should match");
        DotNetNativeTestSuite.AssertEqual("Administrative", StationPurpose.ToStringName(StationPurpose.Purpose.Administrative), "Administrative name should match");
        DotNetNativeTestSuite.AssertEqual("Industrial", StationPurpose.ToStringName(StationPurpose.Purpose.Industrial), "Industrial name should match");
        DotNetNativeTestSuite.AssertEqual("Medical", StationPurpose.ToStringName(StationPurpose.Purpose.Medical), "Medical name should match");
        DotNetNativeTestSuite.AssertEqual("Communications", StationPurpose.ToStringName(StationPurpose.Purpose.Communications), "Communications name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromStringParsesCorrectly()
    {
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Utility, StationPurpose.FromString("Utility"), "Should parse Utility");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Trade, StationPurpose.FromString("Trade"), "Should parse Trade");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Military, StationPurpose.FromString("Military"), "Should parse Military");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Science, StationPurpose.FromString("Science"), "Should parse Science");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Mining, StationPurpose.FromString("Mining"), "Should parse Mining");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Trade, StationPurpose.FromString("TRADE"), "Should parse TRADE");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Military, StationPurpose.FromString("military"), "Should parse military");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Utility, StationPurpose.FromString("unknown"), "Unknown should return Utility");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Utility, StationPurpose.FromString(""), "Empty should return Utility");
    }

    /// <summary>
    /// Tests typical_utility_purposes returns non-empty.
    /// </summary>
    public static void TestTypicalUtilityPurposesReturnsNonEmpty()
    {
        Array<StationPurpose.Purpose> purposes = StationPurpose.TypicalUtilityPurposes();
        DotNetNativeTestSuite.AssertTrue(purposes.Count > 0, "Should have utility purposes");
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Utility), "Should contain Utility");
    }

    /// <summary>
    /// Tests typical_outpost_purposes returns non-empty.
    /// </summary>
    public static void TestTypicalOutpostPurposesReturnsNonEmpty()
    {
        Array<StationPurpose.Purpose> purposes = StationPurpose.TypicalOutpostPurposes();
        DotNetNativeTestSuite.AssertTrue(purposes.Count > 0, "Should have outpost purposes");
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Military), "Should contain Military");
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Science), "Should contain Science");
    }

    /// <summary>
    /// Tests typical_settlement_purposes returns non-empty.
    /// </summary>
    public static void TestTypicalSettlementPurposesReturnsNonEmpty()
    {
        Array<StationPurpose.Purpose> purposes = StationPurpose.TypicalSettlementPurposes();
        DotNetNativeTestSuite.AssertTrue(purposes.Count > 0, "Should have settlement purposes");
        DotNetNativeTestSuite.AssertTrue(purposes.Contains(StationPurpose.Purpose.Residential), "Should contain Residential");
    }

    /// <summary>
    /// Tests is_small_station_purpose.
    /// </summary>
    public static void TestIsSmallStationPurpose()
    {
        DotNetNativeTestSuite.AssertTrue(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Utility), "Utility should be small");
        DotNetNativeTestSuite.AssertTrue(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Mining), "Mining should be small");
        DotNetNativeTestSuite.AssertTrue(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Science), "Science should be small");
        DotNetNativeTestSuite.AssertTrue(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Military), "Military should be small");
        DotNetNativeTestSuite.AssertFalse(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Residential), "Residential should not be small");
        DotNetNativeTestSuite.AssertFalse(StationPurpose.IsSmallStationPurpose(StationPurpose.Purpose.Administrative), "Administrative should not be small");
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(10, StationPurpose.Count(), "Should have 10 station purposes");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < StationPurpose.Count(); i += 1)
        {
            StationPurpose.Purpose purpose = (StationPurpose.Purpose)i;
            string nameStr = StationPurpose.ToStringName(purpose);
            StationPurpose.Purpose parsed = StationPurpose.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(purpose, parsed, $"Roundtrip failed for purpose {i}");
        }
    }
}
