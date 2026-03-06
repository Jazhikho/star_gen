#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ClimateZone enum and utilities.
/// </summary>
public static class TestClimateZone
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Polar", ClimateZone.ToStringName(ClimateZone.Zone.Polar), "Polar name should match");
        DotNetNativeTestSuite.AssertEqual("Subpolar", ClimateZone.ToStringName(ClimateZone.Zone.Subpolar), "Subpolar name should match");
        DotNetNativeTestSuite.AssertEqual("Temperate", ClimateZone.ToStringName(ClimateZone.Zone.Temperate), "Temperate name should match");
        DotNetNativeTestSuite.AssertEqual("Subtropical", ClimateZone.ToStringName(ClimateZone.Zone.Subtropical), "Subtropical name should match");
        DotNetNativeTestSuite.AssertEqual("Tropical", ClimateZone.ToStringName(ClimateZone.Zone.Tropical), "Tropical name should match");
        DotNetNativeTestSuite.AssertEqual("Arid", ClimateZone.ToStringName(ClimateZone.Zone.Arid), "Arid name should match");
        DotNetNativeTestSuite.AssertEqual("Extreme", ClimateZone.ToStringName(ClimateZone.Zone.Extreme), "Extreme name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Polar, ClimateZone.FromString("polar"), "Should parse polar");
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Polar, ClimateZone.FromString("POLAR"), "Should parse POLAR");
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Temperate, ClimateZone.FromString("Temperate"), "Should parse Temperate");
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Tropical, ClimateZone.FromString("tropical"), "Should parse tropical");
    }

    /// <summary>
    /// Tests from_string returns EXTREME for unknown values.
    /// </summary>
    public static void TestFromStringUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Extreme, ClimateZone.FromString("unknown"), "Unknown should return Extreme");
        DotNetNativeTestSuite.AssertEqual(ClimateZone.Zone.Extreme, ClimateZone.FromString(""), "Empty should return Extreme");
    }

    /// <summary>
    /// Tests round-trip conversion.
    /// </summary>
    public static void TestRoundTrip()
    {
        for (int zoneInt = 0; zoneInt < ClimateZone.Count(); zoneInt += 1)
        {
            ClimateZone.Zone zone = (ClimateZone.Zone)zoneInt;
            string nameStr = ClimateZone.ToStringName(zone);
            ClimateZone.Zone restored = ClimateZone.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(zone, restored, $"Round-trip failed for zone {zoneInt}");
        }
    }

    /// <summary>
    /// Tests count returns correct number.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(7, ClimateZone.Count(), "Should have 7 climate zones");
    }
}
