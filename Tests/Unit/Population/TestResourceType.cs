#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ResourceType enum and utilities.
/// </summary>
public static class TestResourceType
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Water", ResourceType.ToStringName(ResourceType.Type.Water), "Water name should match");
        DotNetNativeTestSuite.AssertEqual("Metals", ResourceType.ToStringName(ResourceType.Type.Metals), "Metals name should match");
        DotNetNativeTestSuite.AssertEqual("Rare Elements", ResourceType.ToStringName(ResourceType.Type.RareElements), "Rare Elements name should match");
        DotNetNativeTestSuite.AssertEqual("Organics", ResourceType.ToStringName(ResourceType.Type.Organics), "Organics name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Water, ResourceType.FromString("water"), "Should parse water");
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Water, ResourceType.FromString("WATER"), "Should parse WATER");
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.RareElements, ResourceType.FromString("Rare Elements"), "Should parse Rare Elements");
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.RareElements, ResourceType.FromString("rare_elements"), "Should parse rare_elements");
    }

    /// <summary>
    /// Tests from_string returns SILICATES for unknown values.
    /// </summary>
    public static void TestFromStringUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Silicates, ResourceType.FromString("unknown"), "Unknown should return Silicates");
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Silicates, ResourceType.FromString(""), "Empty should return Silicates");
    }

    /// <summary>
    /// Tests round-trip conversion.
    /// </summary>
    public static void TestRoundTrip()
    {
        for (int resourceInt = 0; resourceInt < ResourceType.Count(); resourceInt += 1)
        {
            ResourceType.Type resource = (ResourceType.Type)resourceInt;
            string nameStr = ResourceType.ToStringName(resource);
            ResourceType.Type restored = ResourceType.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(resource, restored, $"Round-trip failed for resource {resourceInt}");
        }
    }

    /// <summary>
    /// Tests count returns correct number.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(10, ResourceType.Count(), "Should have 10 resource types");
    }
}
