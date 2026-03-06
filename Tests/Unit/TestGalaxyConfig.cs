#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for GalaxyConfig.
/// </summary>
public static class TestGalaxyConfig
{
    /// <summary>
    /// Tests create default returns valid config.
    /// </summary>
    public static void TestCreateDefaultReturnsValidConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();

        DotNetNativeTestSuite.AssertNotNull(config, "Should return config");
        if (!config.IsValid())
        {
            throw new InvalidOperationException("Default config should be valid");
        }
        DotNetNativeTestSuite.AssertEqual((int)GalaxySpec.GalaxyType.Spiral, (int)config.GalaxyType, "Default type should be spiral");
        DotNetNativeTestSuite.AssertEqual(4, config.NumArms, "Default arms should be 4");
    }

    /// <summary>
    /// Tests create milky way sets spiral params.
    /// </summary>
    public static void TestCreateMilkyWaySetsSpiralParams()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();

        DotNetNativeTestSuite.AssertEqual((int)GalaxySpec.GalaxyType.Spiral, (int)config.GalaxyType, "Should be spiral");
        DotNetNativeTestSuite.AssertEqual(4, config.NumArms, "Should have 4 arms");
        DotNetNativeTestSuite.AssertEqual(14.0, config.ArmPitchAngleDeg, "Pitch should match");
        DotNetNativeTestSuite.AssertEqual(0.65, config.ArmAmplitude, "Amplitude should match");
        DotNetNativeTestSuite.AssertEqual(15000.0, config.RadiusPc, "Radius should match");
    }

    /// <summary>
    /// Tests is valid rejects bad type.
    /// </summary>
    public static void TestIsValidRejectsBadType()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.GalaxyType = (GalaxySpec.GalaxyType)(-1);

        if (config.IsValid())
        {
            throw new InvalidOperationException("Invalid type should be rejected");
        }
    }

    /// <summary>
    /// Tests is valid rejects bad num arms.
    /// </summary>
    public static void TestIsValidRejectsBadNumArms()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.NumArms = 1;

        if (config.IsValid())
        {
            throw new InvalidOperationException("Too few arms should be rejected");
        }

        config.NumArms = 7;
        if (config.IsValid())
        {
            throw new InvalidOperationException("Too many arms should be rejected");
        }
    }

    /// <summary>
    /// Tests is valid rejects bad radius.
    /// </summary>
    public static void TestIsValidRejectsBadRadius()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.RadiusPc = 5000.0;

        if (config.IsValid())
        {
            throw new InvalidOperationException("Radius below range should be rejected");
        }

        config.RadiusPc = 30000.0;
        if (config.IsValid())
        {
            throw new InvalidOperationException("Radius above range should be rejected");
        }
    }

    /// <summary>
    /// Tests to dict round trip.
    /// </summary>
    public static void TestToDictRoundTrip()
    {
        GalaxyConfig original = GalaxyConfig.CreateMilkyWay();
        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxyConfig restored = GalaxyConfig.FromDictionary(dict);

        DotNetNativeTestSuite.AssertNotNull(restored, "Should deserialize");
        DotNetNativeTestSuite.AssertEqual((int)original.GalaxyType, (int)restored.GalaxyType, "Type should match");
        DotNetNativeTestSuite.AssertEqual(original.NumArms, restored.NumArms, "Arms should match");
        DotNetNativeTestSuite.AssertEqual(original.RadiusPc, restored.RadiusPc, "Radius should match");
        if (!restored.IsValid())
        {
            throw new InvalidOperationException("Restored config should be valid");
        }
    }

    /// <summary>
    /// Tests from dict empty returns null.
    /// </summary>
    public static void TestFromDictEmptyReturnsNull()
    {
        GalaxyConfig result = GalaxyConfig.FromDictionary(new Dictionary());

        DotNetNativeTestSuite.AssertNull(result, "Empty dict should return null");
    }

    /// <summary>
    /// Tests get type name spiral.
    /// </summary>
    public static void TestGetTypeNameSpiral()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.GalaxyType = GalaxySpec.GalaxyType.Spiral;

        DotNetNativeTestSuite.AssertEqual("Spiral", config.GetTypeName(), "Spiral type name should match");
    }

    /// <summary>
    /// Tests get type name elliptical.
    /// </summary>
    public static void TestGetTypeNameElliptical()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.GalaxyType = GalaxySpec.GalaxyType.Elliptical;

        DotNetNativeTestSuite.AssertEqual("Elliptical", config.GetTypeName(), "Elliptical type name should match");
    }

    /// <summary>
    /// Tests apply to spec.
    /// </summary>
    public static void TestApplyToSpec()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.NumArms = 5;
        config.RadiusPc = 20000.0;
        GalaxySpec spec = new GalaxySpec();
        spec.GalaxySeed = 12345;

        config.ApplyToSpec(spec);

        DotNetNativeTestSuite.AssertEqual(5, spec.NumArms, "Spec should have 5 arms");
        DotNetNativeTestSuite.AssertEqual(20000.0, spec.RadiusPc, "Spec radius should match");
    }
}
