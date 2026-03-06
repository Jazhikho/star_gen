#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for OutpostAuthority enum and utilities.
/// </summary>
public static class TestOutpostAuthority
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Corporate", OutpostAuthority.ToStringName(OutpostAuthority.Type.Corporate), "Corporate name should match");
        DotNetNativeTestSuite.AssertEqual("Military", OutpostAuthority.ToStringName(OutpostAuthority.Type.Military), "Military name should match");
        DotNetNativeTestSuite.AssertEqual("Independent", OutpostAuthority.ToStringName(OutpostAuthority.Type.Independent), "Independent name should match");
        DotNetNativeTestSuite.AssertEqual("Franchise", OutpostAuthority.ToStringName(OutpostAuthority.Type.Franchise), "Franchise name should match");
        DotNetNativeTestSuite.AssertEqual("Cooperative", OutpostAuthority.ToStringName(OutpostAuthority.Type.Cooperative), "Cooperative name should match");
        DotNetNativeTestSuite.AssertEqual("Automated", OutpostAuthority.ToStringName(OutpostAuthority.Type.Automated), "Automated name should match");
        DotNetNativeTestSuite.AssertEqual("Government", OutpostAuthority.ToStringName(OutpostAuthority.Type.Government), "Government name should match");
        DotNetNativeTestSuite.AssertEqual("Religious", OutpostAuthority.ToStringName(OutpostAuthority.Type.Religious), "Religious name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromStringParsesCorrectly()
    {
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Corporate, OutpostAuthority.FromString("Corporate"), "Should parse Corporate");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Military, OutpostAuthority.FromString("Military"), "Should parse Military");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Independent, OutpostAuthority.FromString("Independent"), "Should parse Independent");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Franchise, OutpostAuthority.FromString("Franchise"), "Should parse Franchise");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Cooperative, OutpostAuthority.FromString("Cooperative"), "Should parse Cooperative");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Automated, OutpostAuthority.FromString("Automated"), "Should parse Automated");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Government, OutpostAuthority.FromString("Government"), "Should parse Government");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Religious, OutpostAuthority.FromString("Religious"), "Should parse Religious");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Corporate, OutpostAuthority.FromString("CORPORATE"), "Should parse CORPORATE");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Military, OutpostAuthority.FromString("military"), "Should parse military");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Independent, OutpostAuthority.FromString("unknown"), "Unknown should return Independent");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Independent, OutpostAuthority.FromString(""), "Empty should return Independent");
    }

    /// <summary>
    /// Tests typical_commander_title returns non-empty.
    /// </summary>
    public static void TestTypicalCommanderTitleReturnsNonEmpty()
    {
        for (int i = 0; i < OutpostAuthority.Count(); i += 1)
        {
            OutpostAuthority.Type authority = (OutpostAuthority.Type)i;
            string title = OutpostAuthority.TypicalCommanderTitle(authority);
            DotNetNativeTestSuite.AssertTrue(title.Length > 0, $"Authority {i} should have commander title");
        }
    }

    /// <summary>
    /// Tests typical_commander_title varies by type.
    /// </summary>
    public static void TestTypicalCommanderTitleVariesByType()
    {
        string corporateTitle = OutpostAuthority.TypicalCommanderTitle(OutpostAuthority.Type.Corporate);
        string militaryTitle = OutpostAuthority.TypicalCommanderTitle(OutpostAuthority.Type.Military);
        DotNetNativeTestSuite.AssertNotEqual(corporateTitle, militaryTitle, "Corporate and Military titles should differ");
    }

    /// <summary>
    /// Tests has_parent_organization.
    /// </summary>
    public static void TestHasParentOrganization()
    {
        DotNetNativeTestSuite.AssertTrue(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Corporate), "Corporate should have parent");
        DotNetNativeTestSuite.AssertTrue(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Military), "Military should have parent");
        DotNetNativeTestSuite.AssertTrue(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Franchise), "Franchise should have parent");
        DotNetNativeTestSuite.AssertTrue(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Government), "Government should have parent");
        DotNetNativeTestSuite.AssertTrue(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Religious), "Religious should have parent");
        DotNetNativeTestSuite.AssertFalse(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Independent), "Independent should not have parent");
        DotNetNativeTestSuite.AssertFalse(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Cooperative), "Cooperative should not have parent");
        DotNetNativeTestSuite.AssertFalse(OutpostAuthority.HasParentOrganization(OutpostAuthority.Type.Automated), "Automated should not have parent");
    }

    /// <summary>
    /// Tests typical_for_utility returns non-empty.
    /// </summary>
    public static void TestTypicalForUtilityReturnsNonEmpty()
    {
        Array<OutpostAuthority.Type> types = OutpostAuthority.TypicalForUtility();
        DotNetNativeTestSuite.AssertTrue(types.Count > 0, "Should have utility authorities");
        DotNetNativeTestSuite.AssertTrue(types.Contains(OutpostAuthority.Type.Corporate), "Should contain Corporate");
        DotNetNativeTestSuite.AssertTrue(types.Contains(OutpostAuthority.Type.Franchise), "Should contain Franchise");
    }

    /// <summary>
    /// Tests typical_for_outpost returns non-empty.
    /// </summary>
    public static void TestTypicalForOutpostReturnsNonEmpty()
    {
        Array<OutpostAuthority.Type> types = OutpostAuthority.TypicalForOutpost();
        DotNetNativeTestSuite.AssertTrue(types.Count > 0, "Should have outpost authorities");
        DotNetNativeTestSuite.AssertTrue(types.Contains(OutpostAuthority.Type.Military), "Should contain Military");
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(8, OutpostAuthority.Count(), "Should have 8 outpost authorities");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < OutpostAuthority.Count(); i += 1)
        {
            OutpostAuthority.Type authority = (OutpostAuthority.Type)i;
            string nameStr = OutpostAuthority.ToStringName(authority);
            OutpostAuthority.Type parsed = OutpostAuthority.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(authority, parsed, $"Roundtrip failed for authority {i}");
        }
    }
}
