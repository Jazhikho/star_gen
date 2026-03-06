#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for ProfileCalculations pure functions.
/// </summary>
public static class TestProfileCalculations
{
    /// <summary>
    /// Tests habitability score for Earth-like conditions.
    /// </summary>
    public static void TestHabitabilityScoreEarthLike()
    {
        int score = ProfileCalculations.CalculateHabitabilityScore(
            288.0,
            1.0,
            1.0,
            true,
            true,
            0.1,
            0.7
        );
        DotNetNativeTestSuite.AssertInRange(score, 8, 10, "Earth-like should score 8-10");
    }

    /// <summary>
    /// Tests habitability score for Mars-like conditions.
    /// </summary>
    public static void TestHabitabilityScoreMarsLike()
    {
        int score = ProfileCalculations.CalculateHabitabilityScore(
            210.0,
            0.006,
            0.38,
            false,
            false,
            0.5,
            0.0
        );
        DotNetNativeTestSuite.AssertInRange(score, 0, 3, "Mars-like should score 0-3");
    }

    /// <summary>
    /// Tests habitability score for Venus-like conditions.
    /// </summary>
    public static void TestHabitabilityScoreVenusLike()
    {
        int score = ProfileCalculations.CalculateHabitabilityScore(
            737.0,
            92.0,
            0.9,
            false,
            false,
            0.2,
            0.0
        );
        DotNetNativeTestSuite.AssertInRange(score, 0, 2, "Venus-like should score 0-2");
    }

    /// <summary>
    /// Tests habitability score clamping.
    /// </summary>
    public static void TestHabitabilityScoreClamping()
    {
        int score = ProfileCalculations.CalculateHabitabilityScore(
            293.0, 1.0, 1.0, true, true, 0.0, 0.5
        );
        DotNetNativeTestSuite.AssertInRange(score, 0, 10, "Score should be clamped to 0-10");
    }

    /// <summary>
    /// Tests weather severity with no atmosphere.
    /// </summary>
    public static void TestWeatherSeverityNoAtmosphere()
    {
        double severity = ProfileCalculations.CalculateWeatherSeverity(0.0, 86400.0, false);
        DotNetNativeTestSuite.AssertFloatNear(0.0, severity, 0.001, "No atmosphere should mean no weather");
    }

    /// <summary>
    /// Tests weather severity with thick atmosphere.
    /// </summary>
    public static void TestWeatherSeverityThickAtmosphere()
    {
        double severity = ProfileCalculations.CalculateWeatherSeverity(2.0, 86400.0, true);
        DotNetNativeTestSuite.AssertGreaterThan(severity, 0.3, "Thick atmosphere should increase weather severity");
    }

    /// <summary>
    /// Tests weather severity with fast rotation.
    /// </summary>
    public static void TestWeatherSeverityFastRotation()
    {
        double normal = ProfileCalculations.CalculateWeatherSeverity(1.0, 86400.0, true);
        double fast = ProfileCalculations.CalculateWeatherSeverity(1.0, 21600.0, true);
        DotNetNativeTestSuite.AssertGreaterThan(fast, normal, "Fast rotation should increase weather severity");
    }

    /// <summary>
    /// Tests magnetic field strength calculation.
    /// </summary>
    public static void TestMagneticStrength()
    {
        double earthStrength = ProfileCalculations.CalculateMagneticStrength(8.0e22);
        DotNetNativeTestSuite.AssertFloatNear(1.0, earthStrength, 0.01, "Earth magnetic moment should give ~1.0");

        double zeroStrength = ProfileCalculations.CalculateMagneticStrength(0.0);
        DotNetNativeTestSuite.AssertFloatNear(0.0, zeroStrength, 0.001, "Zero moment should give 0.0");

        double halfStrength = ProfileCalculations.CalculateMagneticStrength(4.0e22);
        DotNetNativeTestSuite.AssertFloatNear(0.5, halfStrength, 0.01, "Half Earth moment should give ~0.5");
    }

    /// <summary>
    /// Tests radiation level with full protection.
    /// </summary>
    public static void TestRadiationLevelProtected()
    {
        double radiation = ProfileCalculations.CalculateRadiationLevel(
            8.0e22,
            1.0,
            true
        );
        DotNetNativeTestSuite.AssertLessThan(radiation, 0.3, "Protected body should have low radiation");
    }

    /// <summary>
    /// Tests radiation level with no protection.
    /// </summary>
    public static void TestRadiationLevelUnprotected()
    {
        double radiation = ProfileCalculations.CalculateRadiationLevel(
            0.0,
            0.0,
            false
        );
        DotNetNativeTestSuite.AssertFloatNear(1.0, radiation, 0.01, "Unprotected body should have max radiation");
    }

    /// <summary>
    /// Tests continent count estimation.
    /// </summary>
    public static void TestContinentCountNoLand()
    {
        int count = ProfileCalculations.EstimateContinentCount(0.5, 0.0, true);
        DotNetNativeTestSuite.AssertEqual(0, count, "No land should mean no continents");
    }

    /// <summary>
    /// Tests continent count estimation with moderate land.
    /// </summary>
    public static void TestContinentCountModerateLand()
    {
        int count = ProfileCalculations.EstimateContinentCount(0.5, 0.3, true);
        DotNetNativeTestSuite.AssertInRange(count, 2, 6, "30% land with tectonics should have 2-6 continents");
    }

    /// <summary>
    /// Tests continent count increases with tectonics.
    /// </summary>
    public static void TestContinentCountTectonicEffect()
    {
        int lowTec = ProfileCalculations.EstimateContinentCount(0.1, 0.5, true);
        int highTec = ProfileCalculations.EstimateContinentCount(1.0, 0.5, true);
        DotNetNativeTestSuite.AssertGreaterThan(highTec, lowTec, "Higher tectonics should mean more continents");
    }

    /// <summary>
    /// Tests climate zones for Earth-like world.
    /// </summary>
    public static void TestClimateZonesEarthLike()
    {
        Array<Dictionary> zones = ProfileCalculations.CalculateClimateZones(
            23.5,
            288.0,
            true
        );

        DotNetNativeTestSuite.AssertGreaterThan(zones.Count, 1, "Should have multiple climate zones");

        double total = 0.0;
        foreach (Dictionary zoneData in zones)
        {
            total += (double)zoneData["coverage"];
        }
        DotNetNativeTestSuite.AssertFloatNear(1.0, total, 0.01, "Zone coverage should sum to 1.0");
    }

    /// <summary>
    /// Tests climate zones for frozen world.
    /// </summary>
    public static void TestClimateZonesFrozen()
    {
        Array<Dictionary> zones = ProfileCalculations.CalculateClimateZones(
            23.5,
            150.0,
            true
        );

        DotNetNativeTestSuite.AssertEqual(1, zones.Count, "Frozen world should have single zone");
        DotNetNativeTestSuite.AssertEqual((int)ClimateZone.Zone.Polar, zones[0]["zone"].AsInt32(), "Should be polar zone");
    }

    /// <summary>
    /// Tests climate zones for airless world.
    /// </summary>
    public static void TestClimateZonesAirless()
    {
        Array<Dictionary> zones = ProfileCalculations.CalculateClimateZones(
            0.0,
            300.0,
            false
        );

        DotNetNativeTestSuite.AssertEqual(1, zones.Count, "Airless world should have single zone");
        DotNetNativeTestSuite.AssertEqual((int)ClimateZone.Zone.Extreme, zones[0]["zone"].AsInt32(), "Should be extreme zone");
    }

    /// <summary>
    /// Tests biome calculation produces ocean biome.
    /// </summary>
    public static void TestBiomesOcean()
    {
        Array<Godot.Collections.Dictionary> zones = new()
        {
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Temperate }, { "coverage", 1.0 } }
        };
        Godot.Collections.Dictionary biomes = ProfileCalculations.CalculateBiomes(
            zones, 0.7, 0.0, 0.0, true, true
        );

        int oceanKey = (int)BiomeType.Type.Ocean;
        DotNetNativeTestSuite.AssertTrue(biomes.ContainsKey(oceanKey), "Should have ocean biome");
        DotNetNativeTestSuite.AssertFloatNear(0.7, (double)biomes[oceanKey], 0.01, "Ocean should be 70%");
    }

    /// <summary>
    /// Tests biome calculation for desert world.
    /// </summary>
    public static void TestBiomesDesert()
    {
        Array<Godot.Collections.Dictionary> zones = new()
        {
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Arid }, { "coverage", 1.0 } }
        };
        Godot.Collections.Dictionary biomes = ProfileCalculations.CalculateBiomes(
            zones, 0.0, 0.0, 0.0, false, true
        );

        int desertKey = (int)BiomeType.Type.Desert;
        DotNetNativeTestSuite.AssertTrue(biomes.ContainsKey(desertKey), "Should have desert biome");
    }

    /// <summary>
    /// Tests biome calculation for volcanic world.
    /// </summary>
    public static void TestBiomesVolcanic()
    {
        Array<Godot.Collections.Dictionary> zones = new()
        {
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Temperate }, { "coverage", 1.0 } }
        };
        Godot.Collections.Dictionary biomes = ProfileCalculations.CalculateBiomes(
            zones, 0.0, 0.0, 0.8, false, true
        );

        int volcanicKey = (int)BiomeType.Type.Volcanic;
        DotNetNativeTestSuite.AssertTrue(biomes.ContainsKey(volcanicKey), "High volcanism should create volcanic biome");
    }

    /// <summary>
    /// Tests resource calculation water abundance.
    /// </summary>
    public static void TestResourcesWater()
    {
        Godot.Collections.Dictionary biomes = new Godot.Collections.Dictionary
        {
            { (int)BiomeType.Type.Ocean, 0.5 }
        };
        Godot.Collections.Dictionary resources = ProfileCalculations.CalculateResources(
            new Godot.Collections.Dictionary(), biomes, 0.0, true, 0.5
        );

        int waterKey = (int)ResourceType.Type.Water;
        DotNetNativeTestSuite.AssertTrue(resources.ContainsKey(waterKey), "Should have water resource");
        DotNetNativeTestSuite.AssertGreaterThan((double)resources[waterKey], 0.5, "Should have significant water");
    }

    /// <summary>
    /// Tests resource calculation metals from composition.
    /// </summary>
    public static void TestResourcesMetals()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary { { "iron_oxides", 0.3 } };
        Godot.Collections.Dictionary resources = ProfileCalculations.CalculateResources(
            composition, new Godot.Collections.Dictionary(), 0.0, false, 0.0
        );

        int metalsKey = (int)ResourceType.Type.Metals;
        DotNetNativeTestSuite.AssertTrue(resources.ContainsKey(metalsKey), "Should have metals resource");
    }

    /// <summary>
    /// Tests resource calculation rare elements from volcanism.
    /// </summary>
    public static void TestResourcesRareElements()
    {
        Godot.Collections.Dictionary resources = ProfileCalculations.CalculateResources(
            new Godot.Collections.Dictionary(), new Godot.Collections.Dictionary(), 0.7, false, 0.0
        );

        int rareKey = (int)ResourceType.Type.RareElements;
        DotNetNativeTestSuite.AssertTrue(resources.ContainsKey(rareKey), "High volcanism should produce rare elements");
    }

    /// <summary>
    /// Tests breathability check positive case.
    /// </summary>
    public static void TestBreathabilityEarthLike()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary
        {
            { "N2", 0.78 },
            { "O2", 0.21 },
            { "Ar", 0.01 }
        };
        bool breathable = ProfileCalculations.CheckBreathability(composition, 1.0);
        DotNetNativeTestSuite.AssertTrue(breathable, "Earth-like atmosphere should be breathable");
    }

    /// <summary>
    /// Tests breathability check low oxygen.
    /// </summary>
    public static void TestBreathabilityLowOxygen()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary
        {
            { "N2", 0.95 },
            { "O2", 0.05 }
        };
        bool breathable = ProfileCalculations.CheckBreathability(composition, 1.0);
        DotNetNativeTestSuite.AssertFalse(breathable, "Low oxygen should not be breathable");
    }

    /// <summary>
    /// Tests breathability check toxic gas.
    /// </summary>
    public static void TestBreathabilityToxic()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary
        {
            { "N2", 0.60 },
            { "O2", 0.20 },
            { "H2S", 0.05 }
        };
        bool breathable = ProfileCalculations.CheckBreathability(composition, 1.0);
        DotNetNativeTestSuite.AssertFalse(breathable, "Toxic gas should make atmosphere unbreathable");
    }

    /// <summary>
    /// Tests breathability check low pressure.
    /// </summary>
    public static void TestBreathabilityLowPressure()
    {
        Godot.Collections.Dictionary composition = new Godot.Collections.Dictionary
        {
            { "N2", 0.78 },
            { "O2", 0.21 }
        };
        bool breathable = ProfileCalculations.CheckBreathability(composition, 0.1);
        DotNetNativeTestSuite.AssertFalse(breathable, "Low pressure should not be breathable");
    }

    /// <summary>
    /// Tests tidal heating calculation.
    /// </summary>
    public static void TestTidalHeatingIoLike()
    {
        double heating = ProfileCalculations.CalculateTidalHeating(
            1.898e27,
            4.217e8,
            1.821e6,
            0.0041
        );
        DotNetNativeTestSuite.AssertInRange(heating, 0.5, 1.0, "Io-like moon should have high tidal heating");
    }

    /// <summary>
    /// Tests tidal heating with no eccentricity.
    /// </summary>
    public static void TestTidalHeatingCircularOrbit()
    {
        double heating = ProfileCalculations.CalculateTidalHeating(
            1.898e27,
            4.217e8,
            1.821e6,
            0.0
        );
        DotNetNativeTestSuite.AssertFloatNear(0.0, heating, 0.01, "Circular orbit should have minimal tidal heating");
    }

    /// <summary>
    /// Tests parent radiation for Io-like moon.
    /// </summary>
    public static void TestParentRadiationIoLike()
    {
        double radiation = ProfileCalculations.CalculateParentRadiation(
            1.898e27,
            1.5e20,
            4.217e8
        );
        DotNetNativeTestSuite.AssertGreaterThan(radiation, 0.2, "Io-like moon should have significant parent radiation");
    }

    /// <summary>
    /// Tests parent radiation for small parent.
    /// </summary>
    public static void TestParentRadiationSmallParent()
    {
        double radiation = ProfileCalculations.CalculateParentRadiation(
            5.972e24,
            8.0e22,
            3.844e8
        );
        DotNetNativeTestSuite.AssertFloatNear(0.0, radiation, 0.01, "Small parent should have no significant radiation");
    }

    /// <summary>
    /// Tests eclipse factor calculation.
    /// </summary>
    public static void TestEclipseFactor()
    {
        double factor = ProfileCalculations.CalculateEclipseFactor(
            6.991e7,
            4.217e8,
            1.5e5,
            3.74e8
        );
        DotNetNativeTestSuite.AssertGreaterThan(factor, 0.0, "Close moon should have eclipse factor");
    }

    /// <summary>
    /// Tests eclipse factor with zero values.
    /// </summary>
    public static void TestEclipseFactorInvalid()
    {
        double factor = ProfileCalculations.CalculateEclipseFactor(0.0, 0.0, 0.0, 0.0);
        DotNetNativeTestSuite.AssertFloatNear(0.0, factor, 0.001, "Invalid inputs should return 0");
    }
}
