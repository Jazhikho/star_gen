#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationClass enum and utilities.
/// </summary>
public static class TestStationClass
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Utility", StationClass.ToStringName(StationClass.Class.U), "U name should be Utility");
        DotNetNativeTestSuite.AssertEqual("Outpost", StationClass.ToStringName(StationClass.Class.O), "O name should be Outpost");
        DotNetNativeTestSuite.AssertEqual("Base", StationClass.ToStringName(StationClass.Class.B), "B name should be Base");
        DotNetNativeTestSuite.AssertEqual("Anchor", StationClass.ToStringName(StationClass.Class.A), "A name should be Anchor");
        DotNetNativeTestSuite.AssertEqual("Super", StationClass.ToStringName(StationClass.Class.S), "S name should be Super");
    }

    /// <summary>
    /// Tests to_letter returns correct values.
    /// </summary>
    public static void TestToLetterReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("U", StationClass.ToLetter(StationClass.Class.U), "U letter should be U");
        DotNetNativeTestSuite.AssertEqual("O", StationClass.ToLetter(StationClass.Class.O), "O letter should be O");
        DotNetNativeTestSuite.AssertEqual("B", StationClass.ToLetter(StationClass.Class.B), "B letter should be B");
        DotNetNativeTestSuite.AssertEqual("A", StationClass.ToLetter(StationClass.Class.A), "A letter should be A");
        DotNetNativeTestSuite.AssertEqual("S", StationClass.ToLetter(StationClass.Class.S), "S letter should be S");
    }

    /// <summary>
    /// Tests from_string parses full names.
    /// </summary>
    public static void TestFromStringParsesFullNames()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.FromString("Utility"), "Should parse Utility");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.FromString("Outpost"), "Should parse Outpost");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.FromString("Base"), "Should parse Base");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, StationClass.FromString("Anchor"), "Should parse Anchor");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.S, StationClass.FromString("Super"), "Should parse Super");
    }

    /// <summary>
    /// Tests from_string parses letters.
    /// </summary>
    public static void TestFromStringParsesLetters()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.FromString("U"), "Should parse U");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.FromString("O"), "Should parse O");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.FromString("B"), "Should parse B");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, StationClass.FromString("A"), "Should parse A");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.S, StationClass.FromString("S"), "Should parse S");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.FromString("utility"), "Should parse utility");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.FromString("OUTPOST"), "Should parse OUTPOST");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.FromString("bAsE"), "Should parse bAsE");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.FromString("unknown"), "Unknown should return O");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.FromString(""), "Empty should return O");
    }

    /// <summary>
    /// Tests get_max_capacity returns correct values.
    /// </summary>
    public static void TestGetMaxCapacityReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual(10000, StationClass.GetMaxCapacity(StationClass.Class.U), "U max capacity should be 10000");
        DotNetNativeTestSuite.AssertEqual(10000, StationClass.GetMaxCapacity(StationClass.Class.O), "O max capacity should be 10000");
        DotNetNativeTestSuite.AssertEqual(100000, StationClass.GetMaxCapacity(StationClass.Class.B), "B max capacity should be 100000");
        DotNetNativeTestSuite.AssertEqual(1000000, StationClass.GetMaxCapacity(StationClass.Class.A), "A max capacity should be 1000000");
        DotNetNativeTestSuite.AssertEqual(-1, StationClass.GetMaxCapacity(StationClass.Class.S), "S max capacity should be -1 (unlimited)");
    }

    /// <summary>
    /// Tests get_min_capacity returns correct values.
    /// </summary>
    public static void TestGetMinCapacityReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual(0, StationClass.GetMinCapacity(StationClass.Class.U), "U min capacity should be 0");
        DotNetNativeTestSuite.AssertEqual(0, StationClass.GetMinCapacity(StationClass.Class.O), "O min capacity should be 0");
        DotNetNativeTestSuite.AssertEqual(0, StationClass.GetMinCapacity(StationClass.Class.B), "B min capacity should be 0");
        DotNetNativeTestSuite.AssertEqual(100000, StationClass.GetMinCapacity(StationClass.Class.A), "A min capacity should be 100000");
        DotNetNativeTestSuite.AssertEqual(1000000, StationClass.GetMinCapacity(StationClass.Class.S), "S min capacity should be 1000000");
    }

    /// <summary>
    /// Tests get_class_for_population small defaults to outpost.
    /// </summary>
    public static void TestGetClassForPopulationSmallDefaultsToOutpost()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.GetClassForPopulation(100), "100 should be O");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.GetClassForPopulation(5000), "5000 should be O");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, StationClass.GetClassForPopulation(10000), "10000 should be O");
    }

    /// <summary>
    /// Tests get_class_for_population small with utility flag.
    /// </summary>
    public static void TestGetClassForPopulationSmallWithUtilityFlag()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.GetClassForPopulation(100, true), "100 with utility should be U");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.GetClassForPopulation(5000, true), "5000 with utility should be U");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, StationClass.GetClassForPopulation(10000, true), "10000 with utility should be U");
    }

    /// <summary>
    /// Tests get_class_for_population base.
    /// </summary>
    public static void TestGetClassForPopulationBase()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.GetClassForPopulation(10001), "10001 should be B");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.GetClassForPopulation(50000), "50000 should be B");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, StationClass.GetClassForPopulation(100000), "100000 should be B");
    }

    /// <summary>
    /// Tests get_class_for_population anchor.
    /// </summary>
    public static void TestGetClassForPopulationAnchor()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, StationClass.GetClassForPopulation(100001), "100001 should be A");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, StationClass.GetClassForPopulation(500000), "500000 should be A");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, StationClass.GetClassForPopulation(1000000), "1000000 should be A");
    }

    /// <summary>
    /// Tests get_class_for_population super.
    /// </summary>
    public static void TestGetClassForPopulationSuper()
    {
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.S, StationClass.GetClassForPopulation(1000001), "1000001 should be S");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.S, StationClass.GetClassForPopulation(10000000), "10000000 should be S");
    }

    /// <summary>
    /// Tests uses_outpost_government.
    /// </summary>
    public static void TestUsesOutpostGovernment()
    {
        DotNetNativeTestSuite.AssertTrue(StationClass.UsesOutpostGovernment(StationClass.Class.U), "U should use outpost government");
        DotNetNativeTestSuite.AssertTrue(StationClass.UsesOutpostGovernment(StationClass.Class.O), "O should use outpost government");
        DotNetNativeTestSuite.AssertFalse(StationClass.UsesOutpostGovernment(StationClass.Class.B), "B should not use outpost government");
        DotNetNativeTestSuite.AssertFalse(StationClass.UsesOutpostGovernment(StationClass.Class.A), "A should not use outpost government");
        DotNetNativeTestSuite.AssertFalse(StationClass.UsesOutpostGovernment(StationClass.Class.S), "S should not use outpost government");
    }

    /// <summary>
    /// Tests uses_colony_government.
    /// </summary>
    public static void TestUsesColonyGovernment()
    {
        DotNetNativeTestSuite.AssertFalse(StationClass.UsesColonyGovernment(StationClass.Class.U), "U should not use colony government");
        DotNetNativeTestSuite.AssertFalse(StationClass.UsesColonyGovernment(StationClass.Class.O), "O should not use colony government");
        DotNetNativeTestSuite.AssertTrue(StationClass.UsesColonyGovernment(StationClass.Class.B), "B should use colony government");
        DotNetNativeTestSuite.AssertTrue(StationClass.UsesColonyGovernment(StationClass.Class.A), "A should use colony government");
        DotNetNativeTestSuite.AssertTrue(StationClass.UsesColonyGovernment(StationClass.Class.S), "S should use colony government");
    }

    /// <summary>
    /// Tests get_description returns non-empty.
    /// </summary>
    public static void TestGetDescriptionReturnsNonEmpty()
    {
        for (int i = 0; i < StationClass.Count(); i += 1)
        {
            StationClass.Class stationClass = (StationClass.Class)i;
            string description = StationClass.GetDescription(stationClass);
            DotNetNativeTestSuite.AssertTrue(description.Length > 0, $"Class {i} should have description");
        }
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(5, StationClass.Count(), "Should have 5 station classes");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < StationClass.Count(); i += 1)
        {
            StationClass.Class stationClass = (StationClass.Class)i;
            string nameStr = StationClass.ToStringName(stationClass);
            StationClass.Class parsed = StationClass.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(stationClass, parsed, $"Roundtrip failed for class {i}");
        }
    }
}
