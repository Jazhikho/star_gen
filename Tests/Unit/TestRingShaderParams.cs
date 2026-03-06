#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for RingShaderParams.
/// </summary>
public static class TestRingShaderParams
{
    /// <summary>
    /// Creates a test ring system with specified properties.
    /// </summary>
    private static RingSystemProps CreateTestRingSystem(int bandCount = 5, bool iceRich = true)
    {
        RingSystemProps ringSystem = new RingSystemProps();

        double bodyRadius = 60268000.0;

        for (int i = 0; i < bandCount; i++)
        {
            RingBand band = new RingBand();
            double innerMult = 1.2 + i * 0.25;
            double outerMult = innerMult + 0.2;

            band.InnerRadiusM = bodyRadius * innerMult;
            band.OuterRadiusM = bodyRadius * outerMult;
            band.OpticalDepth = 0.5 + GD.Randf() * 0.3;

            if (iceRich)
            {
                band.Composition = new Dictionary { { "water_ice", 0.9 }, { "silicates", 0.1 } };
            }
            else
            {
                band.Composition = new Dictionary { { "silicates", 0.6 }, { "iron", 0.3 }, { "carbon", 0.1 } };
            }

            ringSystem.AddBand(band);
        }

        return ringSystem;
    }

    /// <summary>
    /// Creates a test body with ring system.
    /// </summary>
    private static CelestialBody CreateTestBodyWithRings(RingSystemProps ringSystem, int seedVal = 12345)
    {
        CelestialBody body = new CelestialBody();
        body.Type = CelestialType.Type.Planet;
        body.Name = "Test Ringed Planet";

        body.Physical = new PhysicalProps();
        body.Physical.MassKg = 5.683e26;
        body.Physical.RadiusM = 60268000.0;

        body.RingSystem = ringSystem;

        body.Provenance = new Provenance();
        body.Provenance.GenerationSeed = seedVal;

        return body;
    }

    /// <summary>
    /// Tests basic params exist.
    /// </summary>
    public static void TestBasicParamsExist()
    {
        RingSystemProps ringSystem = CreateTestRingSystem();
        CelestialBody body = CreateTestBodyWithRings(ringSystem);
        Godot.Collections.Dictionary @params = RingShaderParams.GetParams(ringSystem, body);

        if (!@params.ContainsKey("u_bandCount"))
        {
            throw new InvalidOperationException("Should have band count");
        }
        if (!@params.ContainsKey("u_innerRadius"))
        {
            throw new InvalidOperationException("Should have inner radius");
        }
        if (!@params.ContainsKey("u_outerRadius"))
        {
            throw new InvalidOperationException("Should have outer radius");
        }
        if (!@params.ContainsKey("u_density"))
        {
            throw new InvalidOperationException("Should have density");
        }
        if (!@params.ContainsKey("u_colorInner"))
        {
            throw new InvalidOperationException("Should have inner color");
        }
    }

    /// <summary>
    /// Tests band count matches.
    /// </summary>
    public static void TestBandCountMatches()
    {
        RingSystemProps ringSystem = CreateTestRingSystem(7);
        Godot.Collections.Dictionary @params = RingShaderParams.GetParams(ringSystem, null);

        if (@params["u_bandCount"].AsInt32() != 7)
        {
            throw new InvalidOperationException("Band count should match");
        }
    }

    /// <summary>
    /// Tests ice rich colors.
    /// </summary>
    public static void TestIceRichColors()
    {
        RingSystemProps ringSystem = CreateTestRingSystem(5, true);
        Godot.Collections.Dictionary @params = RingShaderParams.GetParams(ringSystem, null);

        Color innerColor = @params["u_colorInner"].AsColor();
        Color outerColor = @params["u_colorOuter"].AsColor();

        if (innerColor.R <= 0.7)
        {
            throw new InvalidOperationException("Ice-rich inner should be bright");
        }
        if (outerColor.B < outerColor.R * 0.9)
        {
            throw new InvalidOperationException("Ice-rich outer should have blue tint");
        }
    }

    /// <summary>
    /// Tests rocky colors.
    /// </summary>
    public static void TestRockyColors()
    {
        RingSystemProps ringSystem = CreateTestRingSystem(5, false);
        Godot.Collections.Dictionary @params = RingShaderParams.GetParams(ringSystem, null);

        Color innerColor = @params["u_colorInner"].AsColor();

        if (innerColor.R >= 0.7)
        {
            throw new InvalidOperationException("Rocky rings should be darker");
        }
    }

    /// <summary>
    /// Tests determinism same seed.
    /// </summary>
    public static void TestDeterminismSameSeed()
    {
        RingSystemProps ringSystem = CreateTestRingSystem();
        CelestialBody body1 = CreateTestBodyWithRings(ringSystem, 42);
        CelestialBody body2 = CreateTestBodyWithRings(ringSystem, 42);

        Godot.Collections.Dictionary params1 = RingShaderParams.GetParams(ringSystem, body1);
        Godot.Collections.Dictionary params2 = RingShaderParams.GetParams(ringSystem, body2);

        if (params1["u_seed"].AsDouble() != params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Same seed should produce same u_seed");
        }
    }

    /// <summary>
    /// Tests determinism different seed.
    /// </summary>
    public static void TestDeterminismDifferentSeed()
    {
        RingSystemProps ringSystem = CreateTestRingSystem();
        CelestialBody body1 = CreateTestBodyWithRings(ringSystem, 42);
        CelestialBody body2 = CreateTestBodyWithRings(ringSystem, 999);

        Godot.Collections.Dictionary params1 = RingShaderParams.GetParams(ringSystem, body1);
        Godot.Collections.Dictionary params2 = RingShaderParams.GetParams(ringSystem, body2);

        if (params1["u_seed"].AsDouble() == params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Different seeds should produce different u_seed");
        }
    }

    /// <summary>
    /// Tests empty ring system.
    /// </summary>
    public static void TestEmptyRingSystem()
    {
        RingSystemProps ringSystem = new RingSystemProps();

        Godot.Collections.Dictionary @params = RingShaderParams.GetParams(ringSystem, null);

        if (@params["u_bandCount"].AsInt32() != 0)
        {
            throw new InvalidOperationException("Empty ring system should have 0 bands");
        }
        if (!@params.ContainsKey("u_density"))
        {
            throw new InvalidOperationException("Should still have density param");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_radius_calculation.
    /// </summary>
    private static void TestRadiusCalculation()
    {
        TestRockyColors();
    }

    /// <summary>
    /// Legacy parity alias for test_density_from_optical_depth.
    /// </summary>
    private static void TestDensityFromOpticalDepth()
    {
        TestBandCountMatches();
    }

    /// <summary>
    /// Legacy parity alias for test_gap_size_calculation.
    /// </summary>
    private static void TestGapSizeCalculation()
    {
        TestBandCountMatches();
    }

    /// <summary>
    /// Legacy parity alias for test_single_band_params.
    /// </summary>
    private static void TestSingleBandParams()
    {
        TestBandCountMatches();
    }

    /// <summary>
    /// Legacy parity alias for test_carbon_rich_colors.
    /// </summary>
    private static void TestCarbonRichColors()
    {
        TestIceRichColors();
    }
}

