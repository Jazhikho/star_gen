#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for shader parameter derivation (StarShaderParams, TerrestrialShaderParams, GasGiantShaderParams, RingShaderParams).
/// </summary>
public static class TestColorUtilsShaderParams
{
    /// <summary>
    /// Creates a star with specified properties.
    /// </summary>
    private static CelestialBody CreateStar(double temperatureK, double luminosityWatts, double ageYears)
    {
        PhysicalProps physical = new PhysicalProps(
            Units.SolarMassKg,
            Units.SolarRadiusMeters,
            2.16e6,
            7.25,
            0.0,
            0.0,
            0.0
        );
        StellarProps stellar = new StellarProps(
            luminosityWatts,
            temperatureK,
            "G2V",
            "main_sequence",
            1.0,
            ageYears
        );
        CelestialBody body = new CelestialBody(
            "test_star",
            "Test Star",
            CelestialType.Type.Star,
            physical,
            null
        );
        body.Stellar = stellar;
        return body;
    }

    /// <summary>
    /// Creates a terrestrial planet with specified properties.
    /// </summary>
    private static CelestialBody CreateTerrestrialPlanet(
        double temperatureK,
        double pressureAtm,
        double oceanCoverage,
        double iceCoverage
    )
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.003,
            8.0e22,
            4.0e13
        );
        CelestialBody body = new CelestialBody(
            "test_planet",
            "Test Planet",
            CelestialType.Type.Planet,
            physical,
            new Provenance(12345, "1.0.0", 0, 0, new Dictionary())
        );
        SurfaceProps surface = new SurfaceProps();
        surface.TemperatureK = temperatureK;
        surface.Albedo = 0.3;
        surface.SurfaceType = "rocky";
        surface.VolcanismLevel = 0.2;
        TerrainProps terrain = new TerrainProps(8000.0, 0.5, 0.3, 0.4, 0.3, "varied");
        surface.Terrain = terrain;
        if (oceanCoverage > 0.0)
        {
            surface.Hydrosphere = new HydrosphereProps(oceanCoverage, 3700.0, 0.0, 35.0, "water");
        }
        if (iceCoverage > 0.0)
        {
            CryosphereProps cryo = new CryosphereProps();
            cryo.PolarCapCoverage = iceCoverage;
            surface.Cryosphere = cryo;
        }
        body.Surface = surface;
        if (pressureAtm > 0.0)
        {
            body.Atmosphere = new AtmosphereProps(
                pressureAtm * 101325.0,
                8500.0,
                new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 }, { "Ar", 0.01 } },
                1.0
            );
        }
        return body;
    }

    /// <summary>
    /// Creates a gas giant with specified properties.
    /// </summary>
    private static CelestialBody CreateGasGiant(double temperatureK, double oblateness, double rotationPeriodS)
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg * 318.0,
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
            new Provenance(54321, "1.0.0", 0, 0, new Godot.Collections.Dictionary())
        );
        SurfaceProps surface = new SurfaceProps();
        surface.TemperatureK = temperatureK;
        body.Surface = surface;
        return body;
    }

    /// <summary>
    /// Creates a ring system with specified properties.
    /// </summary>
    private static RingSystemProps CreateRingSystem(int bandCount, double innerRatio, double outerRatio)
    {
        RingSystemProps ringSystem = new RingSystemProps();
        ringSystem.InclinationDeg = 0.0;
        double planetRadius = 70000000.0;
        double totalWidth = (outerRatio - innerRatio) * planetRadius;
        double bandWidth = totalWidth / (double)bandCount * 0.7;
        for (int i = 0; i < bandCount; i++)
        {
            double bandInner = planetRadius * innerRatio + (totalWidth / (double)bandCount) * (double)i;
            double bandOuter = bandInner + bandWidth;
            RingBand band = new RingBand(
                bandInner,
                bandOuter,
                0.5,
                new Godot.Collections.Dictionary { { "water_ice", 0.7 }, { "silicates", 0.3 } },
                1.0,
                "Band " + i.ToString()
            );
            ringSystem.AddBand(band);
        }
        return ringSystem;
    }

    /// <summary>
    /// Tests star params sun like.
    /// </summary>
    public static void TestStarParamsSunLike()
    {
        CelestialBody body = CreateStar(5778.0, StellarProps.SolarLuminosityWatts, 4.6e9);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);
        if (!@params.ContainsKey("u_temperature"))
        {
            throw new InvalidOperationException("Should have temperature");
        }
        DotNetNativeTestSuite.AssertFloatNear(5778.0, @params["u_temperature"].AsDouble(), 1.0, "Temperature should be 5778K");
        if (!@params.ContainsKey("u_limbDark"))
        {
            throw new InvalidOperationException("Should have limb darkening");
        }
        DotNetNativeTestSuite.AssertFloatNear(0.6, @params["u_limbDark"].AsDouble(), 0.05, "G-type limb darkening ~0.6");
        if (!@params.ContainsKey("u_granScale"))
        {
            throw new InvalidOperationException("Should have granulation scale");
        }
        DotNetNativeTestSuite.AssertFloatNear(30.0, @params["u_granScale"].AsDouble(), 2.0, "G-type granulation scale ~30");
        if (!@params.ContainsKey("u_spotCount"))
        {
            throw new InvalidOperationException("Should have spot count");
        }
        double spotCount = @params["u_spotCount"].AsDouble();
        if (spotCount < 0.0 || spotCount > 10.0)
        {
            throw new InvalidOperationException("Reasonable spot count");
        }
    }

    /// <summary>
    /// Tests star params hot o type.
    /// </summary>
    public static void TestStarParamsHotOType()
    {
        CelestialBody body = CreateStar(35000.0, StellarProps.SolarLuminosityWatts * 100000.0, 1.0e6);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);
        DotNetNativeTestSuite.AssertFloatNear(0.2, @params["u_limbDark"].AsDouble(), 0.05, "O-type has minimal limb darkening");
        DotNetNativeTestSuite.AssertFloatNear(10.0, @params["u_granScale"].AsDouble(), 2.0, "O-type has small granulation");
        DotNetNativeTestSuite.AssertFloatNear(0.05, @params["u_granContrast"].AsDouble(), 0.02, "O-type has low granulation contrast");
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_spotCount"].AsDouble(), 0.01, "O-type has no spots");
    }

    /// <summary>
    /// Tests star params cool m type.
    /// </summary>
    public static void TestStarParamsCoolMType()
    {
        CelestialBody body = CreateStar(3000.0, StellarProps.SolarLuminosityWatts * 0.001, 8.0e9);
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);
        DotNetNativeTestSuite.AssertFloatNear(0.8, @params["u_limbDark"].AsDouble(), 0.05, "M-type has strong limb darkening");
        if (@params["u_granContrast"].AsDouble() <= 0.5)
        {
            throw new InvalidOperationException("M-type has high granulation contrast");
        }
        if (@params["u_spotCount"].AsDouble() <= 0.0)
        {
            throw new InvalidOperationException("M-type can have many spots");
        }
    }

    /// <summary>
    /// Tests star params young active.
    /// </summary>
    public static void TestStarParamsYoungActive()
    {
        CelestialBody body = CreateStar(5778.0, StellarProps.SolarLuminosityWatts, 0.5e9);
        body.Physical.RotationPeriodS = 86400.0 * 5.0;
        Godot.Collections.Dictionary @params = StarShaderParams.GetStarShaderParams(body);
        if (@params["u_spotCount"].AsDouble() <= 3.0)
        {
            throw new InvalidOperationException("Young fast rotator should have more spots");
        }
        if (@params["u_flareIntensity"].AsDouble() <= 0.15)
        {
            throw new InvalidOperationException("Young star should have higher flare intensity");
        }
    }

    /// <summary>
    /// Tests star params deterministic seed.
    /// </summary>
    public static void TestStarParamsDeterministicSeed()
    {
        CelestialBody body1 = CreateStar(5778.0, StellarProps.SolarLuminosityWatts, 4.6e9);
        body1.Provenance = new Provenance(12345, "1.0.0", 0, 0, new Godot.Collections.Dictionary());
        CelestialBody body2 = CreateStar(5778.0, StellarProps.SolarLuminosityWatts, 4.6e9);
        body2.Provenance = new Provenance(12345, "1.0.0", 0, 0, new Godot.Collections.Dictionary());
        Godot.Collections.Dictionary params1 = StarShaderParams.GetStarShaderParams(body1);
        Godot.Collections.Dictionary params2 = StarShaderParams.GetStarShaderParams(body2);
        DotNetNativeTestSuite.AssertFloatNear(params1["u_seed"].AsDouble(), params2["u_seed"].AsDouble(), 0.001, "Same seed should produce same u_seed");
    }

    /// <summary>
    /// Tests terrestrial params earth like.
    /// </summary>
    public static void TestTerrestrialParamsEarthLike()
    {
        CelestialBody body = CreateTerrestrialPlanet(288.0, 1.0, 0.7, 0.1);
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetTerrestrialShaderParams(body);
        if (!@params.ContainsKey("u_seaLevel"))
        {
            throw new InvalidOperationException("Should have sea level");
        }
        if (@params["u_seaLevel"].AsDouble() <= 0.3)
        {
            throw new InvalidOperationException("Should have significant sea level");
        }
        if (!@params.ContainsKey("u_iceCap"))
        {
            throw new InvalidOperationException("Should have ice cap");
        }
        DotNetNativeTestSuite.AssertFloatNear(0.1, @params["u_iceCap"].AsDouble(), 0.01, "Ice cap should match");
        if (!@params.ContainsKey("u_cloudCoverage"))
        {
            throw new InvalidOperationException("Should have cloud coverage");
        }
        if (@params["u_cloudCoverage"].AsDouble() <= 0.2)
        {
            throw new InvalidOperationException("Should have clouds with atmosphere");
        }
        if (!@params.ContainsKey("u_atmoDensity"))
        {
            throw new InvalidOperationException("Should have atmosphere density");
        }
        DotNetNativeTestSuite.AssertFloatNear(1.0, @params["u_atmoDensity"].AsDouble(), 0.1, "Earth-like atmosphere density");
    }

    /// <summary>
    /// Tests terrestrial params desert world.
    /// </summary>
    public static void TestTerrestrialParamsDesertWorld()
    {
        CelestialBody body = CreateTerrestrialPlanet(350.0, 0.8, 0.05, 0.0);
        body.Surface.SurfaceType = "desert";
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetTerrestrialShaderParams(body);
        if (@params["u_seaLevel"].AsDouble() >= 0.25)
        {
            throw new InvalidOperationException("Desert should have low sea level");
        }
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_iceCap"].AsDouble(), 0.01, "No ice on hot desert");
        Vector3 colMid = @params["u_colMid"].AsVector3();
        if (colMid.X <= colMid.Z)
        {
            throw new InvalidOperationException("Desert should have more red than blue");
        }
    }

    /// <summary>
    /// Tests terrestrial params ice world.
    /// </summary>
    public static void TestTerrestrialParamsIceWorld()
    {
        CelestialBody body = CreateTerrestrialPlanet(180.0, 0.5, 0.3, 0.8);
        body.Surface.SurfaceType = "icy";
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetTerrestrialShaderParams(body);
        DotNetNativeTestSuite.AssertFloatNear(0.8, @params["u_iceCap"].AsDouble(), 0.01, "Should have heavy ice coverage");
        Vector3 colMid = @params["u_colMid"].AsVector3();
        if (colMid.X <= 0.7 || colMid.Y <= 0.7)
        {
            throw new InvalidOperationException("Ice world should be bright");
        }
    }

    /// <summary>
    /// Tests terrestrial params no atmosphere.
    /// </summary>
    public static void TestTerrestrialParamsNoAtmosphere()
    {
        CelestialBody body = CreateTerrestrialPlanet(300.0, 0.0, 0.0, 0.0);
        body.Atmosphere = null;
        Godot.Collections.Dictionary @params = TerrestrialShaderParams.GetTerrestrialShaderParams(body);
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_atmoDensity"].AsDouble(), 0.01, "No atmosphere density");
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_cloudCoverage"].AsDouble(), 0.01, "No clouds without atmosphere");
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_scatterStrength"].AsDouble(), 0.01, "No scattering without atmosphere");
    }

    /// <summary>
    /// Tests gas giant params jupiter like.
    /// </summary>
    public static void TestGasGiantParamsJupiterLike()
    {
        CelestialBody body = CreateGasGiant(165.0, 0.065, 35730.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetGasGiantShaderParams(body);
        if (!@params.ContainsKey("u_gBandCount"))
        {
            throw new InvalidOperationException("Should have band count");
        }
        if (@params["u_gBandCount"].AsDouble() < 10.0)
        {
            throw new InvalidOperationException("Jupiter-like should have many bands");
        }
        DotNetNativeTestSuite.AssertFloatNear(0.0, @params["u_gOblateness"].AsDouble(), 0.001, "Rendered oblateness should be 0");
        Vector3 bandLight = @params["u_gColBandLight"].AsVector3();
        if (bandLight.X <= bandLight.Z)
        {
            throw new InvalidOperationException("Jupiter should have warm tones");
        }
    }

    /// <summary>
    /// Tests gas giant params ice giant.
    /// </summary>
    public static void TestGasGiantParamsIceGiant()
    {
        CelestialBody body = CreateGasGiant(72.0, 0.017, 57996.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetGasGiantShaderParams(body);
        Vector3 bandLight = @params["u_gColBandLight"].AsVector3();
        if (bandLight.Z <= bandLight.X)
        {
            throw new InvalidOperationException("Ice giant should have cool/blue tones");
        }
    }

    /// <summary>
    /// Tests gas giant params hot jupiter.
    /// </summary>
    public static void TestGasGiantParamsHotJupiter()
    {
        CelestialBody body = CreateGasGiant(1200.0, 0.03, 86400.0);
        Godot.Collections.Dictionary @params = GasGiantShaderParams.GetGasGiantShaderParams(body);
        Vector3 bandLight = @params["u_gColBandLight"].AsVector3();
        if (bandLight.X <= 0.8)
        {
            throw new InvalidOperationException("Hot Jupiter should have orange/red tones");
        }
    }

    /// <summary>
    /// Tests ring params simple ring.
    /// </summary>
    public static void TestRingParamsSimpleRing()
    {
        RingSystemProps ringSystem = CreateRingSystem(2, 1.5, 2.5);
        double planetRadius = 70000000.0;
        Godot.Collections.Dictionary @params = RingShaderParams.GetRingShaderParams(ringSystem, planetRadius);
        if (!@params.ContainsKey("u_ringType"))
        {
            throw new InvalidOperationException("Should have ring type");
        }
        if (@params["u_ringType"].AsInt32() < 1)
        {
            throw new InvalidOperationException("Should have rings enabled");
        }
        if (!@params.ContainsKey("u_ringInner"))
        {
            throw new InvalidOperationException("Should have inner radius");
        }
        if (!@params.ContainsKey("u_ringOuter"))
        {
            throw new InvalidOperationException("Should have outer radius");
        }
        if (@params["u_ringOuter"].AsDouble() <= @params["u_ringInner"].AsDouble())
        {
            throw new InvalidOperationException("Outer should be greater than inner");
        }
    }

    /// <summary>
    /// Tests ring params no rings.
    /// </summary>
    public static void TestRingParamsNoRings()
    {
        Godot.Collections.Dictionary @params = RingShaderParams.GetRingShaderParams(null, 70000000.0);
        DotNetNativeTestSuite.AssertEqual(0, @params["u_ringType"].AsInt32(), "No rings should be type 0");
    }

    /// <summary>
    /// Tests ring params complex rings.
    /// </summary>
    public static void TestRingParamsComplexRings()
    {
        RingSystemProps ringSystem = CreateRingSystem(7, 1.2, 3.0);
        double planetRadius = 60000000.0;
        Godot.Collections.Dictionary @params = RingShaderParams.GetRingShaderParams(ringSystem, planetRadius);
        DotNetNativeTestSuite.AssertEqual(3, @params["u_ringType"].AsInt32(), "7 bands should be complex (type 3)");
        DotNetNativeTestSuite.AssertEqual(7, @params["u_ringBands"].AsInt32(), "Should have 7 bands");
    }
}
