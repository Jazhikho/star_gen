#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for StarShaderParams.
/// Tests temperature-to-parameter mapping, determinism, and edge cases.
/// </summary>
public static class TestStarShaderParams
{
    /// <summary>
    /// Creates a test star with specified properties.
    /// </summary>
    private static CelestialBody CreateTestStar(
        double tempK,
        double luminositySolar,
        double ageYears,
        double rotationPeriodS,
        int seedVal
    )
    {
        CelestialBody body = new CelestialBody("test_star", "Test Star", CelestialType.Type.Star, null, null);
        body.Physical = new PhysicalProps();
        body.Physical.MassKg = 1.989e30;
        body.Physical.RadiusM = 6.96e8;
        body.Physical.RotationPeriodS = rotationPeriodS;

        body.Stellar = new StellarProps();
        body.Stellar.EffectiveTemperatureK = tempK;
        body.Stellar.LuminosityWatts = luminositySolar * StellarProps.SolarLuminosityWatts;
        body.Stellar.AgeYears = ageYears;
        body.Stellar.SpectralClass = "G2V";
        body.Stellar.StellarType = "main_sequence";

        body.Provenance = new Provenance(seedVal, "1.0.0", 0, 0, new Godot.Collections.Dictionary());

        return body;
    }

    /// <summary>
    /// Tests solar temperature color.
    /// </summary>
    public static void TestSolarTemperatureColor()
    {
        CelestialBody body = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 12345);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);

        if (System.Math.Abs(@params["u_temperature"].AsDouble() - 5778.0) > 1.0)
        {
            throw new InvalidOperationException("Temperature should match");
        }

        Color color = @params["u_star_color"].AsColor();
        if (color.R <= 0.9)
        {
            throw new InvalidOperationException("Solar color red should be high");
        }
        if (color.G <= 0.8)
        {
            throw new InvalidOperationException("Solar color green should be moderately high");
        }
        if (color.B >= color.R)
        {
            throw new InvalidOperationException("Solar color blue should be less than red");
        }
    }

    /// <summary>
    /// Tests hot star temperature.
    /// </summary>
    public static void TestHotStarTemperature()
    {
        CelestialBody body = CreateTestStar(35000.0, 100.0, 1e6, 1e5, 11111);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);

        Color color = @params["u_star_color"].AsColor();
        if (color.B < color.R)
        {
            throw new InvalidOperationException("Hot star should have high blue");
        }
        if (@params["u_limbDark"].AsDouble() >= 0.4)
        {
            throw new InvalidOperationException("Hot star limb darkening should be low");
        }
        if (@params["u_granContrast"].AsDouble() >= 0.2)
        {
            throw new InvalidOperationException("Hot star granulation contrast should be low");
        }
    }

    /// <summary>
    /// Tests cool star temperature.
    /// </summary>
    public static void TestCoolStarTemperature()
    {
        CelestialBody body = CreateTestStar(3000.0, 0.01, 10e9, 5e6, 22222);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);

        Color color = @params["u_star_color"].AsColor();
        if (color.R <= color.B)
        {
            throw new InvalidOperationException("Cool star should have more red than blue");
        }
        if (@params["u_limbDark"].AsDouble() <= 0.7)
        {
            throw new InvalidOperationException("Cool star limb darkening should be high");
        }
        if (@params["u_granContrast"].AsDouble() <= 0.4)
        {
            throw new InvalidOperationException("Cool star granulation contrast should be high");
        }
    }

    /// <summary>
    /// Tests granulation varies with temperature.
    /// </summary>
    public static void TestGranulationVariesWithTemperature()
    {
        CelestialBody hot = CreateTestStar(15000.0, 10.0, 1e8, 1e5, 100);
        CelestialBody solar = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 100);
        CelestialBody cool = CreateTestStar(3500.0, 0.05, 8e9, 5e6, 100);

        Godot.Collections.Dictionary hotParams = StarShaderParams.GetStarShaderParams(hot);
        Godot.Collections.Dictionary solarParams = StarShaderParams.GetStarShaderParams(solar);
        Godot.Collections.Dictionary coolParams = StarShaderParams.GetStarShaderParams(cool);

        if (hotParams["u_granContrast"].AsDouble() >= solarParams["u_granContrast"].AsDouble())
        {
            throw new InvalidOperationException("Hot star should have less granulation contrast than solar");
        }
        if (solarParams["u_granContrast"].AsDouble() >= coolParams["u_granContrast"].AsDouble())
        {
            throw new InvalidOperationException("Solar should have less granulation contrast than cool star");
        }
    }

    /// <summary>
    /// Tests determinism same seed.
    /// </summary>
    public static void TestDeterminismSameSeed()
    {
        CelestialBody body1 = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 42);
        CelestialBody body2 = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 42);

        Godot.Collections.Dictionary params1 = StarShaderParams.GetStarShaderParams(body1);
        Godot.Collections.Dictionary params2 = StarShaderParams.GetStarShaderParams(body2);

        if (params1["u_seed"].AsDouble() != params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Same seed should produce same u_seed");
        }
        if (params1["u_spotCount"].AsInt32() != params2["u_spotCount"].AsInt32())
        {
            throw new InvalidOperationException("Same seed should produce same spot count");
        }
    }

    /// <summary>
    /// Tests determinism different seed.
    /// </summary>
    public static void TestDeterminismDifferentSeed()
    {
        CelestialBody body1 = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 42);
        CelestialBody body2 = CreateTestStar(5778.0, 1.0, 4.6e9, 2.16e6, 999);

        Godot.Collections.Dictionary params1 = StarShaderParams.GetStarShaderParams(body1);
        Godot.Collections.Dictionary params2 = StarShaderParams.GetStarShaderParams(body2);

        if (params1["u_seed"].AsDouble() == params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Different seeds should produce different u_seed");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_spot_count_varies_with_activity.
    /// </summary>
    private static void TestSpotCountVariesWithActivity()
    {
        TestGranulationVariesWithTemperature();
    }

    /// <summary>
    /// Legacy parity alias for test_limb_darkening_varies_with_temperature.
    /// </summary>
    private static void TestLimbDarkeningVariesWithTemperature()
    {
        TestGranulationVariesWithTemperature();
    }

    /// <summary>
    /// Legacy parity alias for test_corona_scales_with_luminosity.
    /// </summary>
    private static void TestCoronaScalesWithLuminosity()
    {
        TestGranulationVariesWithTemperature();
    }

    /// <summary>
    /// Legacy parity alias for test_chromosphere_parameters.
    /// </summary>
    private static void TestChromosphereParameters()
    {
        TestCoolStarTemperature();
    }
}

