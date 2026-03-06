#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for BiomeType enum and utilities.
/// </summary>
public static class TestBiomeType
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Ocean", BiomeType.ToStringName(BiomeType.Type.Ocean), "Ocean name should match");
        DotNetNativeTestSuite.AssertEqual("Ice Sheet", BiomeType.ToStringName(BiomeType.Type.IceSheet), "Ice Sheet name should match");
        DotNetNativeTestSuite.AssertEqual("Forest", BiomeType.ToStringName(BiomeType.Type.Forest), "Forest name should match");
        DotNetNativeTestSuite.AssertEqual("Desert", BiomeType.ToStringName(BiomeType.Type.Desert), "Desert name should match");
        DotNetNativeTestSuite.AssertEqual("Volcanic", BiomeType.ToStringName(BiomeType.Type.Volcanic), "Volcanic name should match");
        DotNetNativeTestSuite.AssertEqual("Barren", BiomeType.ToStringName(BiomeType.Type.Barren), "Barren name should match");
        DotNetNativeTestSuite.AssertEqual("Gas Giant", BiomeType.ToStringName((BiomeType.Type)BiomeType.GasGiantBiomeKey), "Gas Giant name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Ocean, BiomeType.FromString("ocean"), "Should parse ocean");
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Ocean, BiomeType.FromString("OCEAN"), "Should parse OCEAN");
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.IceSheet, BiomeType.FromString("Ice Sheet"), "Should parse Ice Sheet");
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.IceSheet, BiomeType.FromString("ice_sheet"), "Should parse ice_sheet");
    }

    /// <summary>
    /// Tests from_string returns BARREN for unknown values.
    /// </summary>
    public static void TestFromStringUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Barren, BiomeType.FromString("unknown"), "Unknown should return Barren");
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Barren, BiomeType.FromString(""), "Empty should return Barren");
    }

    /// <summary>
    /// Tests can_support_life returns expected values.
    /// </summary>
    public static void TestCanSupportLife()
    {
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Ocean), "Ocean should support life");
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Forest), "Forest should support life");
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Jungle), "Jungle should support life");
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Grassland), "Grassland should support life");
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Desert), "Desert should support life");
        DotNetNativeTestSuite.AssertTrue(BiomeType.CanSupportLife(BiomeType.Type.Tundra), "Tundra should support life");

        DotNetNativeTestSuite.AssertFalse(BiomeType.CanSupportLife(BiomeType.Type.Barren), "Barren should not support life");
        DotNetNativeTestSuite.AssertFalse(BiomeType.CanSupportLife(BiomeType.Type.Volcanic), "Volcanic should not support life");
        DotNetNativeTestSuite.AssertFalse(BiomeType.CanSupportLife(BiomeType.Type.IceSheet), "Ice Sheet should not support life");
        DotNetNativeTestSuite.AssertFalse(BiomeType.CanSupportLife((BiomeType.Type)BiomeType.GasGiantBiomeKey), "Gas Giant should not support life");
    }

    /// <summary>
    /// Tests round-trip conversion.
    /// </summary>
    public static void TestRoundTrip()
    {
        for (int biomeInt = 0; biomeInt < BiomeType.Count(); biomeInt += 1)
        {
            BiomeType.Type biome = (BiomeType.Type)biomeInt;
            string nameStr = BiomeType.ToStringName(biome);
            BiomeType.Type restored = BiomeType.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(biome, restored, $"Round-trip failed for biome {biomeInt}");
        }
    }

    /// <summary>
    /// Tests count returns correct number.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(15, BiomeType.Count(), "Should have 15 biome types");
    }
}
