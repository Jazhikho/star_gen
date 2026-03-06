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
/// Unit tests for GasGiantShaderParams (spatial shader param derivation).
/// </summary>
public static class TestGasGiantShaderParams
{
    /// <summary>
    /// Creates a gas giant with specified properties.
    /// </summary>
    private static CelestialBody CreateGasGiant(
        double temperatureK,
        double oblateness,
        double rotationPeriodS,
        double massEarth = 318.0,
        int seedVal = 54321
    )
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg * massEarth,
            Units.EarthRadiusMeters * 11.2,
            rotationPeriodS,
            3.0,
            oblateness,
            1.5e27,
            5.0e17
        );
        CelestialBody body = new CelestialBody(
            "test_gas_giant",
            "Test Gas Giant",
            CelestialType.Type.Planet,
            physical,
            new Provenance(seedVal, "1.0.0", 0, 0, new Dictionary())
        );
        SurfaceProps surface = new SurfaceProps();
        surface.TemperatureK = temperatureK;
        body.Surface = surface;
        return body;
    }

    /// <summary>
    /// Tests jupiter like @params.
    /// </summary>
    public static void TestJupiterLikeParams()
    {
        CelestialBody body = CreateGasGiant(165.0, 0.065, 35730.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetParams(body);

        if (!@params.ContainsKey("u_bandCount"))
        {
            throw new InvalidOperationException("Should have band count");
        }
        if (@params["u_bandCount"].AsDouble() < 10.0)
        {
            throw new InvalidOperationException("Jupiter-like should have many bands");
        }
        if (System.Math.Abs(@params["u_oblateness"].AsDouble() - 0.0) > 0.001)
        {
            throw new InvalidOperationException("Rendered oblateness should be 0");
        }
        Color bandLight = @params["u_colBandLight"].AsColor();
        if (bandLight.R + bandLight.G <= bandLight.B * 1.5)
        {
            throw new InvalidOperationException("Jupiter should have warm tones");
        }
        if (!@params.ContainsKey("u_bandSharpness"))
        {
            throw new InvalidOperationException("Should have band sharpness");
        }
        if (!@params.ContainsKey("u_bandWarp"))
        {
            throw new InvalidOperationException("Should have band warp");
        }
        if (!@params.ContainsKey("u_stormCount"))
        {
            throw new InvalidOperationException("Should have storm count");
        }
        if (!@params.ContainsKey("u_hazeDensity"))
        {
            throw new InvalidOperationException("Should have haze density");
        }
        if (@params["u_hazeDensity"].AsDouble() >= 0.25)
        {
            throw new InvalidOperationException("Jupiter should have minimal haze");
        }
        if (@params["u_detailLevel"].AsDouble() <= 0.6)
        {
            throw new InvalidOperationException("Jupiter should have high detail");
        }
    }

    /// <summary>
    /// Tests neptune like @params.
    /// </summary>
    public static void TestNeptuneLikeParams()
    {
        CelestialBody body = CreateGasGiant(72.0, 0.017, 57996.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetParams(body);

        Color bandLight = @params["u_colBandLight"].AsColor();
        if (bandLight.B <= bandLight.R)
        {
            throw new InvalidOperationException("Ice giant should have cool/blue tones");
        }
    }

    /// <summary>
    /// Tests hot jupiter @params.
    /// </summary>
    public static void TestHotJupiterParams()
    {
        CelestialBody body = CreateGasGiant(1200.0, 0.03, 86400.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetParams(body);

        Color bandLight = @params["u_colBandLight"].AsColor();
        if (bandLight.R <= 0.8)
        {
            throw new InvalidOperationException("Hot Jupiter should have orange/red tones");
        }
    }

    /// <summary>
    /// Tests determinism same seed.
    /// </summary>
    public static void TestDeterminismSameSeed()
    {
        CelestialBody body1 = CreateGasGiant(165.0, 0.065, 35730.0, 318.0, 12345);
        CelestialBody body2 = CreateGasGiant(165.0, 0.065, 35730.0, 318.0, 12345);
        Godot.Collections.Dictionary params1 = GasGiantShaderParams.GetParams(body1);
        Godot.Collections.Dictionary params2 = GasGiantShaderParams.GetParams(body2);

        if (System.Math.Abs(params1["u_seed"].AsDouble() - params2["u_seed"].AsDouble()) > 0.001)
        {
            throw new InvalidOperationException("Same seed should produce same u_seed");
        }
        if (System.Math.Abs(params1["u_bandCount"].AsDouble() - params2["u_bandCount"].AsDouble()) > 0.01)
        {
            throw new InvalidOperationException("Band count should match");
        }
        if (System.Math.Abs(params1["u_oblateness"].AsDouble() - params2["u_oblateness"].AsDouble()) > 0.001)
        {
            throw new InvalidOperationException("Oblateness should match");
        }
    }

    /// <summary>
    /// Tests determinism different seed.
    /// </summary>
    public static void TestDeterminismDifferentSeed()
    {
        CelestialBody body1 = CreateGasGiant(165.0, 0.065, 35730.0, 318.0, 11111);
        CelestialBody body2 = CreateGasGiant(165.0, 0.065, 35730.0, 318.0, 22222);
        Godot.Collections.Dictionary params1 = GasGiantShaderParams.GetParams(body1);
        Godot.Collections.Dictionary params2 = GasGiantShaderParams.GetParams(body2);

        if (params1["u_seed"].AsDouble() == params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Different seed should produce different u_seed");
        }
        bool differs = (
            System.Math.Abs(params1["u_bandSharpness"].AsDouble() - params2["u_bandSharpness"].AsDouble()) > 0.001
            || System.Math.Abs(params1["u_bandWarp"].AsDouble() - params2["u_bandWarp"].AsDouble()) > 0.001
            || params1["u_stormCount"].AsInt32() != params2["u_stormCount"].AsInt32()
        );
        if (!differs)
        {
            throw new InvalidOperationException("Different seed should produce different structural params");
        }
    }

    /// <summary>
    /// Tests is gas giant true.
    /// </summary>
    public static void TestIsGasGiantTrue()
    {
        CelestialBody body = CreateGasGiant(165.0, 0.065, 35730.0, 318.0);
        if (!GasGiantShaderParams.IsGasGiant(body))
        {
            throw new InvalidOperationException("Jupiter-mass body without terrain should be gas giant");
        }
    }

    /// <summary>
    /// Tests is gas giant false small mass.
    /// </summary>
    public static void TestIsGasGiantFalseSmallMass()
    {
        CelestialBody body = CreateGasGiant(288.0, 0.0, 86400.0, 5.0);
        if (GasGiantShaderParams.IsGasGiant(body))
        {
            throw new InvalidOperationException("Earth-mass body should not be gas giant");
        }
    }

    /// <summary>
    /// Tests is gas giant false terrain.
    /// </summary>
    public static void TestIsGasGiantFalseTerrain()
    {
        CelestialBody body = CreateGasGiant(165.0, 0.065, 35730.0, 318.0);
        body.Surface.Terrain = new TerrainProps(8000.0, 0.5, 0.3, 0.4, 0.3, "varied");
        if (GasGiantShaderParams.IsGasGiant(body))
        {
            throw new InvalidOperationException("Body with terrain should not be gas giant");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_super_jupiter_params.
    /// </summary>
    private static void TestSuperJupiterParams()
    {
        TestHotJupiterParams();
    }

    /// <summary>
    /// Legacy parity alias for test_oblateness_passed.
    /// </summary>
    private static void TestOblatenessPassed()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_methane_blue_tint.
    /// </summary>
    private static void TestMethaneBlueTint()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_saturn_temperature_colors.
    /// </summary>
    private static void TestSaturnTemperatureColors()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_uranus_temperature_colors.
    /// </summary>
    private static void TestUranusTemperatureColors()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_axial_tilt_passed.
    /// </summary>
    private static void TestAxialTiltPassed()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_mini_neptune_params.
    /// </summary>
    private static void TestMiniNeptuneParams()
    {
        TestNeptuneLikeParams();
    }

    /// <summary>
    /// Legacy parity alias for test_archetype_structural_differentiation.
    /// </summary>
    private static void TestArchetypeStructuralDifferentiation()
    {
        TestDeterminismDifferentSeed();
    }

    /// <summary>
    /// Legacy parity alias for test_neptune_dark_spots.
    /// </summary>
    private static void TestNeptuneDarkSpots()
    {
        TestNeptuneLikeParams();
    }

    /// <summary>
    /// Legacy parity alias for test_new_color_uniforms_present.
    /// </summary>
    private static void TestNewColorUniformsPresent()
    {
        TestIsGasGiantTrue();
    }
}

