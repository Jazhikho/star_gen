#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationPlacementContext enum and utilities.
/// </summary>
public static class TestStationPlacementContext
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Bridge System", StationPlacementContext.ToStringName(StationPlacementContext.Context.BridgeSystem), "BridgeSystem name should match");
        DotNetNativeTestSuite.AssertEqual("Colony World", StationPlacementContext.ToStringName(StationPlacementContext.Context.ColonyWorld), "ColonyWorld name should match");
        DotNetNativeTestSuite.AssertEqual("Native World", StationPlacementContext.ToStringName(StationPlacementContext.Context.NativeWorld), "NativeWorld name should match");
        DotNetNativeTestSuite.AssertEqual("Resource System", StationPlacementContext.ToStringName(StationPlacementContext.Context.ResourceSystem), "ResourceSystem name should match");
        DotNetNativeTestSuite.AssertEqual("Strategic", StationPlacementContext.ToStringName(StationPlacementContext.Context.Strategic), "Strategic name should match");
        DotNetNativeTestSuite.AssertEqual("Scientific", StationPlacementContext.ToStringName(StationPlacementContext.Context.Scientific), "Scientific name should match");
        DotNetNativeTestSuite.AssertEqual("Other", StationPlacementContext.ToStringName(StationPlacementContext.Context.Other), "Other name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromStringParsesCorrectly()
    {
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, StationPlacementContext.FromString("Bridge System"), "Should parse Bridge System");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, StationPlacementContext.FromString("bridge_system"), "Should parse bridge_system");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, StationPlacementContext.FromString("bridge"), "Should parse bridge");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, StationPlacementContext.FromString("Colony World"), "Should parse Colony World");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, StationPlacementContext.FromString("colony"), "Should parse colony");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.NativeWorld, StationPlacementContext.FromString("Native World"), "Should parse Native World");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ResourceSystem, StationPlacementContext.FromString("Resource System"), "Should parse Resource System");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Strategic, StationPlacementContext.FromString("Strategic"), "Should parse Strategic");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Scientific, StationPlacementContext.FromString("Scientific"), "Should parse Scientific");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Scientific, StationPlacementContext.FromString("science"), "Should parse science");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, StationPlacementContext.FromString("BRIDGE_SYSTEM"), "Should parse BRIDGE_SYSTEM");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, StationPlacementContext.FromString("colony_world"), "Should parse colony_world");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Other, StationPlacementContext.FromString("unknown"), "Unknown should return Other");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.Other, StationPlacementContext.FromString(""), "Empty should return Other");
    }

    /// <summary>
    /// Tests favors_utility_stations.
    /// </summary>
    public static void TestFavorsUtilityStations()
    {
        DotNetNativeTestSuite.AssertTrue(StationPlacementContext.FavorsUtilityStations(StationPlacementContext.Context.BridgeSystem), "BridgeSystem should favor utility");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.FavorsUtilityStations(StationPlacementContext.Context.ColonyWorld), "ColonyWorld should not favor utility");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.FavorsUtilityStations(StationPlacementContext.Context.NativeWorld), "NativeWorld should not favor utility");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.FavorsUtilityStations(StationPlacementContext.Context.ResourceSystem), "ResourceSystem should not favor utility");
    }

    /// <summary>
    /// Tests can_support_large_stations.
    /// </summary>
    public static void TestCanSupportLargeStations()
    {
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.CanSupportLargeStations(StationPlacementContext.Context.BridgeSystem), "BridgeSystem should not support large");
        DotNetNativeTestSuite.AssertTrue(StationPlacementContext.CanSupportLargeStations(StationPlacementContext.Context.ColonyWorld), "ColonyWorld should support large");
        DotNetNativeTestSuite.AssertTrue(StationPlacementContext.CanSupportLargeStations(StationPlacementContext.Context.NativeWorld), "NativeWorld should support large");
        DotNetNativeTestSuite.AssertTrue(StationPlacementContext.CanSupportLargeStations(StationPlacementContext.Context.ResourceSystem), "ResourceSystem should support large");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.CanSupportLargeStations(StationPlacementContext.Context.Strategic), "Strategic should not support large");
    }

    /// <summary>
    /// Tests requires_spacefaring_natives.
    /// </summary>
    public static void TestRequiresSpacefaringNatives()
    {
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.RequiresSpacefaringNatives(StationPlacementContext.Context.BridgeSystem), "BridgeSystem should not require spacefaring natives");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.RequiresSpacefaringNatives(StationPlacementContext.Context.ColonyWorld), "ColonyWorld should not require spacefaring natives");
        DotNetNativeTestSuite.AssertTrue(StationPlacementContext.RequiresSpacefaringNatives(StationPlacementContext.Context.NativeWorld), "NativeWorld should require spacefaring natives");
        DotNetNativeTestSuite.AssertFalse(StationPlacementContext.RequiresSpacefaringNatives(StationPlacementContext.Context.ResourceSystem), "ResourceSystem should not require spacefaring natives");
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(7, StationPlacementContext.Count(), "Should have 7 placement contexts");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < StationPlacementContext.Count(); i += 1)
        {
            StationPlacementContext.Context context = (StationPlacementContext.Context)i;
            string nameStr = StationPlacementContext.ToStringName(context);
            StationPlacementContext.Context parsed = StationPlacementContext.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(context, parsed, $"Roundtrip failed for context {i}");
        }
    }
}
