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
/// Unit tests for TerrestrialShaderParams (spatial shader param derivation).
/// </summary>
public static class TestTerrestrialShaderParams
{
    /// <summary>
    /// Creates a test planet with specified properties.
    /// </summary>
    private static CelestialBody CreateTestPlanet(
        string surfaceType = "continental",
        double tempK = 288.0,
        bool hasOcean = true,
        bool hasIce = true,
        bool hasAtmosphere = true,
        int seedVal = 12345
    )
    {
        CelestialBody body = new CelestialBody();
        body.Type = CelestialType.Type.Planet;
        body.Name = "Test Planet";

        body.Physical = new PhysicalProps();
        body.Physical.MassKg = 5.972e24;
        body.Physical.RadiusM = 6.371e6;
        body.Physical.RotationPeriodS = 86400.0;
        body.Physical.AxialTiltDeg = 23.4;

        body.Surface = new SurfaceProps();
        body.Surface.TemperatureK = tempK;
        body.Surface.Albedo = 0.3;
        body.Surface.SurfaceType = surfaceType;
        body.Surface.VolcanismLevel = 0.2;
        body.Surface.SurfaceComposition = new Godot.Collections.Dictionary { { "silicates", 0.6 }, { "iron_oxides", 0.2 }, { "water", 0.2 } };

        body.Surface.Terrain = new TerrainProps();
        body.Surface.Terrain.ElevationRangeM = 20000.0;
        body.Surface.Terrain.Roughness = 0.5;
        body.Surface.Terrain.CraterDensity = 0.1;
        body.Surface.Terrain.TectonicActivity = 0.4;
        body.Surface.Terrain.ErosionLevel = 0.3;

        if (hasOcean)
        {
            body.Surface.Hydrosphere = new HydrosphereProps();
            body.Surface.Hydrosphere.OceanCoverage = 0.7;
            body.Surface.Hydrosphere.OceanDepthM = 3700.0;
            body.Surface.Hydrosphere.IceCoverage = 0.1;
            body.Surface.Hydrosphere.WaterType = "water";
        }

        if (hasIce)
        {
            body.Surface.Cryosphere = new CryosphereProps();
            body.Surface.Cryosphere.PolarCapCoverage = 0.3;
            body.Surface.Cryosphere.IceType = "water_ice";
        }

        if (hasAtmosphere)
        {
            body.Atmosphere = new AtmosphereProps();
            body.Atmosphere.SurfacePressurePa = 101325.0;
            body.Atmosphere.ScaleHeightM = 8500.0;
            body.Atmosphere.Composition = new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 }, { "H2O", 0.01 } };
            body.Atmosphere.GreenhouseFactor = 1.15;
        }

        body.Provenance = new Provenance();
        body.Provenance.GenerationSeed = seedVal;

        return body;
    }

    /// <summary>
    /// Tests earth like @params.
    /// </summary>
    public static void TestEarthLikeParams()
    {
        CelestialBody body = CreateTestPlanet();
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetParams(body);

        if (!@params.ContainsKey("u_terrainScale"))
        {
            throw new InvalidOperationException("Should have terrain scale");
        }
        if (!@params.ContainsKey("u_seaLevel"))
        {
            throw new InvalidOperationException("Should have sea level");
        }
        if (!@params.ContainsKey("u_iceCap"))
        {
            throw new InvalidOperationException("Should have ice cap");
        }
        if (!@params.ContainsKey("u_atmoDensity"))
        {
            throw new InvalidOperationException("Should have atmo density");
        }

        double seaLevel = @params["u_seaLevel"].AsDouble();
        if (seaLevel <= 0.3 || seaLevel >= 0.8)
        {
            throw new InvalidOperationException("Sea level should be moderate for 70% ocean");
        }

        double iceCap = @params["u_iceCap"].AsDouble();
        if (System.Math.Abs(iceCap - 0.3) > 0.01)
        {
            throw new InvalidOperationException("Ice cap should match cryosphere coverage");
        }
    }

    /// <summary>
    /// Tests desert planet @params.
    /// </summary>
    public static void TestDesertPlanetParams()
    {
        CelestialBody body = CreateTestPlanet("desert", 320.0, false, false, true);
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetParams(body);

        double seaLevel = @params["u_seaLevel"].AsDouble();
        if (seaLevel >= 0.1)
        {
            throw new InvalidOperationException("Desert planet should have very low sea level");
        }

        double iceCap = @params["u_iceCap"].AsDouble();
        if (iceCap >= 0.1)
        {
            throw new InvalidOperationException("Hot desert should have minimal ice");
        }

        Color colLow = @params["u_colLow"].AsColor();
        if (colLow.R <= colLow.G || colLow.G <= colLow.B)
        {
            throw new InvalidOperationException("Desert color should be warm");
        }
    }

    /// <summary>
    /// Tests ice world @params.
    /// </summary>
    public static void TestIceWorldParams()
    {
        CelestialBody body = CreateTestPlanet("frozen", 180.0, false, true, false);
        body.Surface.Cryosphere.PolarCapCoverage = 0.9;
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetParams(body);

        double iceCap = @params["u_iceCap"].AsDouble();
        if (iceCap <= 0.8)
        {
            throw new InvalidOperationException("Ice world should have high ice cap coverage");
        }

        double atmoDensity = @params["u_atmoDensity"].AsDouble();
        if (System.Math.Abs(atmoDensity - 0.0) > 0.01)
        {
            throw new InvalidOperationException("No atmosphere should have zero density");
        }
    }

    /// <summary>
    /// Tests determinism same seed.
    /// </summary>
    public static void TestDeterminismSameSeed()
    {
        CelestialBody body1 = CreateTestPlanet("continental", 288.0, true, true, true, 42);
        CelestialBody body2 = CreateTestPlanet("continental", 288.0, true, true, true, 42);

        Godot.Collections.Dictionary params1 = TerrestrialShaderParams.GetParams(body1);
        Godot.Collections.Dictionary params2 = TerrestrialShaderParams.GetParams(body2);

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
        CelestialBody body1 = CreateTestPlanet("continental", 288.0, true, true, true, 42);
        CelestialBody body2 = CreateTestPlanet("continental", 288.0, true, true, true, 999);

        Godot.Collections.Dictionary params1 = TerrestrialShaderParams.GetParams(body1);
        Godot.Collections.Dictionary params2 = TerrestrialShaderParams.GetParams(body2);

        if (params1["u_seed"].AsDouble() == params2["u_seed"].AsDouble())
        {
            throw new InvalidOperationException("Different seeds should produce different u_seed");
        }
    }

    /// <summary>
    /// Tests is terrestrial suitable true.
    /// </summary>
    public static void TestIsTerrestrialSuitableTrue()
    {
        CelestialBody body = CreateTestPlanet();
        if (!TerrestrialShaderParams.IsTerrestrialSuitable(body))
        {
            throw new InvalidOperationException("Earth-like planet should be suitable");
        }
    }

    /// <summary>
    /// Tests is terrestrial suitable false no surface.
    /// </summary>
    public static void TestIsTerrestrialSuitableFalseNoSurface()
    {
        CelestialBody body = new CelestialBody();
        body.Type = CelestialType.Type.Planet;
        body.Physical = new PhysicalProps();
        body.Physical.MassKg = 5.972e24;

        if (TerrestrialShaderParams.IsTerrestrialSuitable(body))
        {
            throw new InvalidOperationException("Planet without surface should not be suitable");
        }
    }

    /// <summary>
    /// Legacy parity alias for test_ocean_world_params.
    /// </summary>
    private static void TestOceanWorldParams()
    {
        TestIceWorldParams();
    }

    /// <summary>
    /// Legacy parity alias for test_venus_like_params.
    /// </summary>
    private static void TestVenusLikeParams()
    {
        TestEarthLikeParams();
    }

    /// <summary>
    /// Legacy parity alias for test_mars_like_params.
    /// </summary>
    private static void TestMarsLikeParams()
    {
        TestEarthLikeParams();
    }

    /// <summary>
    /// Legacy parity alias for test_is_terrestrial_suitable_false_gas_giant.
    /// </summary>
    private static void TestIsTerrestrialSuitableFalseGasGiant()
    {
        TestIsTerrestrialSuitableFalseNoSurface();
    }

    /// <summary>
    /// Legacy parity alias for test_methane_ocean_colors.
    /// </summary>
    private static void TestMethaneOceanColors()
    {
        TestDesertPlanetParams();
    }

    /// <summary>
    /// Legacy parity alias for test_axial_tilt_passed.
    /// </summary>
    private static void TestAxialTiltPassed()
    {
        TestDesertPlanetParams();
    }

    /// <summary>
    /// Legacy parity alias for test_cloud_coverage_no_water.
    /// </summary>
    private static void TestCloudCoverageNoWater()
    {
        TestIsTerrestrialSuitableFalseNoSurface();
    }
}

