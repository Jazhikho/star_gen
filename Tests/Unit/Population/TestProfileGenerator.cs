#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ProfileGenerator.
/// </summary>
public static class TestProfileGenerator
{
    /// <summary>
    /// Creates a minimal Earth-like planet for testing.
    /// </summary>
    private static CelestialBody CreateEarthLikeBody()
    {
        CelestialBody body = new(
            "test_earth",
            "Test Earth",
            CelestialType.Type.Planet
        );

        body.Physical = new PhysicalProps(
            5.972e24,
            6.371e6
        );
        body.Physical.RotationPeriodS = 86400.0;
        body.Physical.AxialTiltDeg = 23.4;
        body.Physical.MagneticMoment = 8.0e22;
        body.Physical.InternalHeatWatts = 4.7e13;

        body.Orbital = new OrbitalProps();
        body.Orbital.SemiMajorAxisM = 1.496e11;
        body.Orbital.Eccentricity = 0.017;

        body.Atmosphere = new AtmosphereProps(
            101325.0,
            8500.0,
            new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 }, { "Ar", 0.01 } },
            1.15
        );

        body.Surface = new SurfaceProps(
            288.0,
            0.3,
            "continental",
            0.3,
            new Godot.Collections.Dictionary { { "silicates", 0.6 }, { "iron_oxides", 0.2 }, { "carbonates", 0.1 }, { "water", 0.1 } }
        );

        body.Surface.Terrain = new TerrainProps(
            20000.0,
            0.5,
            0.2,
            0.5,
            0.4,
            "tectonic"
        );

        body.Surface.Hydrosphere = new HydrosphereProps(
            0.71,
            3800.0,
            0.03,
            35.0,
            "water"
        );

        body.Surface.Cryosphere = new CryosphereProps(
            0.03,
            100.0,
            false,
            0.0,
            0.0,
            "water_ice"
        );

        return body;
    }

    /// <summary>
    /// Creates a Mars-like planet for testing.
    /// </summary>
    private static CelestialBody CreateMarsLikeBody()
    {
        CelestialBody body = new(
            "test_mars",
            "Test Mars",
            CelestialType.Type.Planet
        );

        body.Physical = new PhysicalProps(
            6.39e23,
            3.389e6
        );
        body.Physical.RotationPeriodS = 88620.0;
        body.Physical.AxialTiltDeg = 25.2;
        body.Physical.MagneticMoment = 0.0;
        body.Physical.InternalHeatWatts = 1.0e12;

        body.Orbital = new OrbitalProps();
        body.Orbital.SemiMajorAxisM = 2.279e11;
        body.Orbital.Eccentricity = 0.093;

        body.Atmosphere = new AtmosphereProps(
            610.0,
            11000.0,
            new Godot.Collections.Dictionary { { "CO2", 0.95 }, { "N2", 0.03 }, { "Ar", 0.02 } },
            1.02
        );

        body.Surface = new SurfaceProps(
            210.0,
            0.25,
            "rocky_cold",
            0.1,
            new Godot.Collections.Dictionary { { "silicates", 0.5 }, { "iron_oxides", 0.35 }, { "sulfur_compounds", 0.15 } }
        );

        body.Surface.Terrain = new TerrainProps(
            30000.0,
            0.6,
            0.4,
            0.05,
            0.2,
            "volcanic"
        );

        body.Surface.Cryosphere = new CryosphereProps(
            0.15,
            500.0,
            false,
            0.0,
            0.0,
            "co2_ice"
        );

        return body;
    }

    /// <summary>
    /// Creates a Europa-like moon for testing.
    /// </summary>
    private static CelestialBody CreateEuropaLikeBody()
    {
        CelestialBody body = new(
            "test_europa",
            "Test Europa",
            CelestialType.Type.Moon
        );

        body.Physical = new PhysicalProps(
            4.8e22,
            1.561e6
        );
        body.Physical.RotationPeriodS = 306720.0;
        body.Physical.AxialTiltDeg = 0.1;
        body.Physical.MagneticMoment = 0.0;
        body.Physical.InternalHeatWatts = 1.0e12;

        body.Orbital = new OrbitalProps();
        body.Orbital.SemiMajorAxisM = 6.709e8;
        body.Orbital.Eccentricity = 0.009;

        body.Atmosphere = null;

        body.Surface = new SurfaceProps(
            102.0,
            0.67,
            "icy",
            0.0,
            new Godot.Collections.Dictionary { { "water_ice", 0.95 }, { "silicates", 0.05 } }
        );

        body.Surface.Terrain = new TerrainProps(
            200.0,
            0.3,
            0.1,
            0.0,
            0.1,
            "icy"
        );

        body.Surface.Cryosphere = new CryosphereProps(
            1.0,
            20000.0,
            true,
            100000.0,
            0.3,
            "water_ice"
        );

        return body;
    }

    /// <summary>
    /// Creates a Jupiter-like parent body.
    /// </summary>
    private static CelestialBody CreateJupiterParent()
    {
        CelestialBody body = new(
            "test_jupiter",
            "Test Jupiter",
            CelestialType.Type.Planet
        );

        body.Physical = new PhysicalProps(
            1.898e27,
            6.991e7
        );
        body.Physical.MagneticMoment = 1.5e20;
        body.Physical.RotationPeriodS = 35760.0;

        body.Orbital = new OrbitalProps();
        body.Orbital.SemiMajorAxisM = 7.785e11;

        return body;
    }

    /// <summary>
    /// Creates a sun-like context.
    /// </summary>
    private static ParentContext CreateSunContext()
    {
        return ParentContext.SunLike();
    }

    /// <summary>
    /// Tests profile generation for Earth-like planet.
    /// </summary>
    public static void TestGenerateEarthLike()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertEqual("test_earth", profile.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertFalse(profile.IsMoon, "Should not be a moon");
        DotNetNativeTestSuite.AssertTrue(profile.HasAtmosphere, "Should have atmosphere");
        DotNetNativeTestSuite.AssertTrue(profile.HasMagneticField, "Should have magnetic field");
        DotNetNativeTestSuite.AssertTrue(profile.HasLiquidWater, "Should have liquid water");
        DotNetNativeTestSuite.AssertTrue(profile.HasBreathableAtmosphere, "Should have breathable atmosphere");

        DotNetNativeTestSuite.AssertInRange(profile.HabitabilityScore, 8, 10, "Earth-like should score 8-10");

        DotNetNativeTestSuite.AssertFloatNear(1.0, profile.GravityG, 0.1, "Gravity should be ~1g");
        DotNetNativeTestSuite.AssertFloatNear(24.0, profile.DayLengthHours, 0.1, "Day should be ~24h");
        DotNetNativeTestSuite.AssertFloatNear(1.0, profile.PressureAtm, 0.01, "Pressure should be ~1 atm");
    }

    /// <summary>
    /// Tests profile generation for Mars-like planet.
    /// </summary>
    public static void TestGenerateMarsLike()
    {
        CelestialBody body = CreateMarsLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertEqual("test_mars", profile.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertTrue(profile.HasAtmosphere, "Should have thin atmosphere");
        DotNetNativeTestSuite.AssertFalse(profile.HasMagneticField, "Should not have magnetic field");
        DotNetNativeTestSuite.AssertFalse(profile.HasLiquidWater, "Should not have liquid water");
        DotNetNativeTestSuite.AssertFalse(profile.HasBreathableAtmosphere, "Should not have breathable atmosphere");

        DotNetNativeTestSuite.AssertInRange(profile.HabitabilityScore, 0, 3, "Mars-like should score 0-3");

        DotNetNativeTestSuite.AssertGreaterThan(profile.RadiationLevel, 0.5, "Mars should have high radiation");
    }

    /// <summary>
    /// Tests profile generation for Europa-like moon.
    /// </summary>
    public static void TestGenerateEuropaLike()
    {
        CelestialBody body = CreateEuropaLikeBody();
        CelestialBody parent = CreateJupiterParent();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context, parent);

        DotNetNativeTestSuite.AssertEqual("test_europa", profile.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertTrue(profile.IsMoon, "Should be a moon");
        DotNetNativeTestSuite.AssertFalse(profile.HasAtmosphere, "Should not have atmosphere");
        DotNetNativeTestSuite.AssertTrue(profile.HasLiquidWater, "Should have subsurface ocean");

        DotNetNativeTestSuite.AssertGreaterThan(profile.TidalHeatingFactor, 0.0, "Should have tidal heating");
        DotNetNativeTestSuite.AssertGreaterThan(profile.ParentRadiationExposure, 0.0, "Should have parent radiation");
    }

    /// <summary>
    /// Tests climate zones are generated.
    /// </summary>
    public static void TestClimateZonesGenerated()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertGreaterThan(profile.ClimateZones.Count, 0, "Should have climate zones");

        double total = 0.0;
        foreach (Dictionary zoneData in profile.ClimateZones)
        {
            total += (double)zoneData["coverage"];
        }
        DotNetNativeTestSuite.AssertFloatNear(1.0, total, 0.01, "Climate zone coverage should sum to 1.0");
    }

    /// <summary>
    /// Tests biomes are generated.
    /// </summary>
    public static void TestBiomesGenerated()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertGreaterThan(profile.Biomes.Count, 0, "Should have biomes");

        int oceanKey = (int)BiomeType.Type.Ocean;
        DotNetNativeTestSuite.AssertTrue(profile.Biomes.ContainsKey(oceanKey), "Should have ocean biome");
    }

    /// <summary>
    /// Tests resources are generated.
    /// </summary>
    public static void TestResourcesGenerated()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertGreaterThan(profile.Resources.Count, 0, "Should have resources");

        int waterKey = (int)ResourceType.Type.Water;
        DotNetNativeTestSuite.AssertTrue(profile.Resources.ContainsKey(waterKey), "Should have water resource");
    }

    /// <summary>
    /// Tests continent count is estimated.
    /// </summary>
    public static void TestContinentCount()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertGreaterThan(profile.ContinentCount, 0, "Should have continents");
    }

    /// <summary>
    /// Tests tidal locking detection.
    /// </summary>
    public static void TestTidalLockingDetection()
    {
        CelestialBody body = CreateEuropaLikeBody();
        CelestialBody parent = CreateJupiterParent();
        ParentContext context = CreateSunContext();

        double orbitalPeriod = body.Orbital.GetOrbitalPeriodS(parent.Physical.MassKg);
        body.Physical.RotationPeriodS = orbitalPeriod;

        PlanetProfile profile = ProfileGenerator.Generate(body, context, parent);

        DotNetNativeTestSuite.AssertTrue(profile.IsTidallyLocked, "Europa-like should be tidally locked");
    }

    /// <summary>
    /// Tests determinism - same input gives same output.
    /// </summary>
    public static void TestDeterminism()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile profile1 = ProfileGenerator.Generate(body, context);
        PlanetProfile profile2 = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertEqual(profile1.HabitabilityScore, profile2.HabitabilityScore, "HabitabilityScore should match");
        DotNetNativeTestSuite.AssertFloatNear(profile1.AvgTemperatureK, profile2.AvgTemperatureK, 0.001, "AvgTemperatureK should match");
        DotNetNativeTestSuite.AssertFloatNear(profile1.PressureAtm, profile2.PressureAtm, 0.001, "PressureAtm should match");
        DotNetNativeTestSuite.AssertFloatNear(profile1.RadiationLevel, profile2.RadiationLevel, 0.001, "RadiationLevel should match");
        DotNetNativeTestSuite.AssertEqual(profile1.ClimateZones.Count, profile2.ClimateZones.Count, "ClimateZones count should match");
    }

    /// <summary>
    /// Tests body without surface generates valid profile.
    /// </summary>
    public static void TestBodyWithoutSurface()
    {
        CelestialBody body = new(
            "test_gas_giant",
            "Test Gas Giant",
            CelestialType.Type.Planet
        );
        body.Physical = new PhysicalProps(1.898e27, 6.991e7);
        body.Physical.RotationPeriodS = 35760.0;
        body.Atmosphere = new AtmosphereProps(100000.0, 27000.0, new Dictionary { { "H2", 0.9 }, { "He", 0.1 } }, 1.0);

        ParentContext context = CreateSunContext();
        PlanetProfile profile = ProfileGenerator.Generate(body, context);

        DotNetNativeTestSuite.AssertInRange(profile.HabitabilityScore, 0, 2, "Gas giant should have very low habitability");
        DotNetNativeTestSuite.AssertFalse(profile.HasLiquidWater, "Gas giant should not have liquid water");
    }

    /// <summary>
    /// Tests serialization round-trip preserves generated profile.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetProfile original = ProfileGenerator.Generate(body, context);
        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.HabitabilityScore, restored.HabitabilityScore, "HabitabilityScore should match");
        DotNetNativeTestSuite.AssertFloatNear(original.AvgTemperatureK, restored.AvgTemperatureK, 0.001, "AvgTemperatureK should match");
        DotNetNativeTestSuite.AssertEqual(original.ClimateZones.Count, restored.ClimateZones.Count, "ClimateZones count should match");
        DotNetNativeTestSuite.AssertEqual(original.Biomes.Count, restored.Biomes.Count, "Biomes count should match");
        DotNetNativeTestSuite.AssertEqual(original.Resources.Count, restored.Resources.Count, "Resources count should match");
    }
}
