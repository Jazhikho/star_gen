#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationType enum and utilities.
/// </summary>
public static class TestStationType
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Orbital", StationType.ToStringName(StationType.Type.Orbital), "Orbital name should match");
        DotNetNativeTestSuite.AssertEqual("Deep Space", StationType.ToStringName(StationType.Type.DeepSpace), "Deep Space name should match");
        DotNetNativeTestSuite.AssertEqual("Lagrange Point", StationType.ToStringName(StationType.Type.Lagrange), "Lagrange name should match");
        DotNetNativeTestSuite.AssertEqual("Asteroid Belt", StationType.ToStringName(StationType.Type.AsteroidBelt), "Asteroid Belt name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromStringParsesCorrectly()
    {
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, StationType.FromString("Orbital"), "Should parse Orbital");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.DeepSpace, StationType.FromString("Deep Space"), "Should parse Deep Space");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.DeepSpace, StationType.FromString("deep_space"), "Should parse deep_space");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Lagrange, StationType.FromString("Lagrange"), "Should parse Lagrange");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Lagrange, StationType.FromString("Lagrange Point"), "Should parse Lagrange Point");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.AsteroidBelt, StationType.FromString("Asteroid Belt"), "Should parse Asteroid Belt");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.AsteroidBelt, StationType.FromString("belt"), "Should parse belt");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, StationType.FromString("ORBITAL"), "Should parse ORBITAL");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, StationType.FromString("orbital"), "Should parse orbital");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, StationType.FromString("unknown"), "Unknown should return Orbital");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, StationType.FromString(""), "Empty should return Orbital");
    }

    /// <summary>
    /// Tests is_body_associated.
    /// </summary>
    public static void TestIsBodyAssociated()
    {
        DotNetNativeTestSuite.AssertTrue(StationType.IsBodyAssociated(StationType.Type.Orbital), "Orbital should be body associated");
        DotNetNativeTestSuite.AssertFalse(StationType.IsBodyAssociated(StationType.Type.DeepSpace), "Deep Space should not be body associated");
        DotNetNativeTestSuite.AssertFalse(StationType.IsBodyAssociated(StationType.Type.Lagrange), "Lagrange should not be body associated");
        DotNetNativeTestSuite.AssertFalse(StationType.IsBodyAssociated(StationType.Type.AsteroidBelt), "Asteroid Belt should not be body associated");
    }

    /// <summary>
    /// Tests is_free_floating.
    /// </summary>
    public static void TestIsFreeFloating()
    {
        DotNetNativeTestSuite.AssertFalse(StationType.IsFreeFloating(StationType.Type.Orbital), "Orbital should not be free floating");
        DotNetNativeTestSuite.AssertTrue(StationType.IsFreeFloating(StationType.Type.DeepSpace), "Deep Space should be free floating");
        DotNetNativeTestSuite.AssertTrue(StationType.IsFreeFloating(StationType.Type.Lagrange), "Lagrange should be free floating");
        DotNetNativeTestSuite.AssertTrue(StationType.IsFreeFloating(StationType.Type.AsteroidBelt), "Asteroid Belt should be free floating");
    }

    /// <summary>
    /// Tests is_body_associated and is_free_floating are mutually exclusive.
    /// </summary>
    public static void TestIsBodyAssociatedAndFreeFloatingAreMutuallyExclusive()
    {
        for (int i = 0; i < StationType.Count(); i += 1)
        {
            StationType.Type stationType = (StationType.Type)i;
            bool isBody = StationType.IsBodyAssociated(stationType);
            bool isFree = StationType.IsFreeFloating(stationType);
            DotNetNativeTestSuite.AssertTrue(isBody != isFree, $"Type {i} should be either body-associated or free-floating");
        }
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(4, StationType.Count(), "Should have 4 station types");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < StationType.Count(); i += 1)
        {
            StationType.Type stationType = (StationType.Type)i;
            string nameStr = StationType.ToStringName(stationType);
            StationType.Type parsed = StationType.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(stationType, parsed, $"Roundtrip failed for type {i}");
        }
    }
}
