#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PlanetProfile data model.
/// </summary>
public static class TestPlanetProfile
{
    /// <summary>
    /// Creates a minimal test profile.
    /// </summary>
    private static PlanetProfile CreateTestProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "test_planet_001";
        profile.HabitabilityScore = 7;
        profile.AvgTemperatureK = 288.0;
        profile.PressureAtm = 1.0;
        profile.OceanCoverage = 0.7;
        profile.LandCoverage = 0.25;
        profile.IceCoverage = 0.05;
        profile.ContinentCount = 5;
        profile.MaxElevationKm = 8.8;
        profile.DayLengthHours = 24.0;
        profile.AxialTiltDeg = 23.4;
        profile.GravityG = 1.0;
        profile.TectonicActivity = 0.5;
        profile.VolcanismLevel = 0.3;
        profile.WeatherSeverity = 0.4;
        profile.MagneticFieldStrength = 0.8;
        profile.RadiationLevel = 0.2;
        profile.Albedo = 0.3;
        profile.GreenhouseFactor = 1.15;
        profile.IsTidallyLocked = false;
        profile.HasAtmosphere = true;
        profile.HasMagneticField = true;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.IsMoon = false;

        profile.ClimateZones = new Array<Godot.Collections.Dictionary>
        {
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Polar }, { "coverage", 0.1 } },
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Temperate }, { "coverage", 0.5 } },
            new Godot.Collections.Dictionary { { "zone", (int)ClimateZone.Zone.Tropical }, { "coverage", 0.4 } },
        };

        profile.Biomes = new Godot.Collections.Dictionary
        {
            { (int)BiomeType.Type.Ocean, 0.7 },
            { (int)BiomeType.Type.Forest, 0.15 },
            { (int)BiomeType.Type.Grassland, 0.1 },
            { (int)BiomeType.Type.IceSheet, 0.05 },
        };

        profile.Resources = new Dictionary
        {
            { (int)ResourceType.Type.Water, 0.9 },
            { (int)ResourceType.Type.Metals, 0.4 },
            { (int)ResourceType.Type.Organics, 0.6 },
        };

        return profile;
    }

    /// <summary>
    /// Tests basic profile creation.
    /// </summary>
    public static void TestCreation()
    {
        PlanetProfile profile = new();
        DotNetNativeTestSuite.AssertEqual("", profile.BodyId, "Default body_id should be empty");
        DotNetNativeTestSuite.AssertEqual(0, profile.HabitabilityScore, "Default habitability_score should be 0");
        DotNetNativeTestSuite.AssertFloatNear(0.0, profile.AvgTemperatureK, 0.001, "Default avg_temperature_k should be 0");
    }

    /// <summary>
    /// Tests habitability category derivation.
    /// </summary>
    public static void TestGetHabitabilityCategory()
    {
        PlanetProfile profile = new();

        profile.HabitabilityScore = 0;
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Impossible, profile.GetHabitabilityCategory(), "Score 0 should be Impossible");

        profile.HabitabilityScore = 5;
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Marginal, profile.GetHabitabilityCategory(), "Score 5 should be Marginal");

        profile.HabitabilityScore = 10;
        DotNetNativeTestSuite.AssertEqual(HabitabilityCategory.Category.Ideal, profile.GetHabitabilityCategory(), "Score 10 should be Ideal");
    }

    /// <summary>
    /// Tests habitability category string.
    /// </summary>
    public static void TestGetHabitabilityCategoryString()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 7;
        DotNetNativeTestSuite.AssertEqual("Challenging", profile.GetHabitabilityCategoryString(), "Score 7 should be Challenging");
    }

    /// <summary>
    /// Tests temperature conversion.
    /// </summary>
    public static void TestGetTemperatureCelsius()
    {
        PlanetProfile profile = new();
        profile.AvgTemperatureK = 288.0;
        DotNetNativeTestSuite.AssertFloatNear(14.85, profile.GetTemperatureCelsius(), 0.01, "288K should be ~14.85°C");

        profile.AvgTemperatureK = 273.15;
        DotNetNativeTestSuite.AssertFloatNear(0.0, profile.GetTemperatureCelsius(), 0.01, "273.15K should be 0°C");
    }

    /// <summary>
    /// Tests habitable surface calculation.
    /// </summary>
    public static void TestGetHabitableSurface()
    {
        PlanetProfile profile = new();
        profile.LandCoverage = 0.3;
        profile.IceCoverage = 0.1;

        DotNetNativeTestSuite.AssertFloatNear(0.25, profile.GetHabitableSurface(), 0.001, "Habitable surface should be 0.25");
    }

    /// <summary>
    /// Tests habitable surface with no ice.
    /// </summary>
    public static void TestGetHabitableSurfaceNoIce()
    {
        PlanetProfile profile = new();
        profile.LandCoverage = 0.4;
        profile.IceCoverage = 0.0;

        DotNetNativeTestSuite.AssertFloatNear(0.4, profile.GetHabitableSurface(), 0.001, "Habitable surface should be 0.4");
    }

    /// <summary>
    /// Tests dominant biome detection.
    /// </summary>
    public static void TestGetDominantBiome()
    {
        PlanetProfile profile = CreateTestProfile();
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Ocean, profile.GetDominantBiome(), "Dominant biome should be Ocean");
    }

    /// <summary>
    /// Tests dominant biome with no biomes.
    /// </summary>
    public static void TestGetDominantBiomeEmpty()
    {
        PlanetProfile profile = new();
        DotNetNativeTestSuite.AssertEqual(BiomeType.Type.Barren, profile.GetDominantBiome(), "Empty biomes should return Barren");
    }

    /// <summary>
    /// Tests primary resource detection.
    /// </summary>
    public static void TestGetPrimaryResource()
    {
        PlanetProfile profile = CreateTestProfile();
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Water, profile.GetPrimaryResource(), "Primary resource should be Water");
    }

    /// <summary>
    /// Tests primary resource with no resources.
    /// </summary>
    public static void TestGetPrimaryResourceEmpty()
    {
        PlanetProfile profile = new();
        DotNetNativeTestSuite.AssertEqual(ResourceType.Type.Silicates, profile.GetPrimaryResource(), "Empty resources should return Silicates");
    }

    /// <summary>
    /// Tests can_support_native_life for habitable world.
    /// </summary>
    public static void TestCanSupportNativeLifeHabitable()
    {
        PlanetProfile profile = CreateTestProfile();
        DotNetNativeTestSuite.AssertTrue(profile.CanSupportNativeLife(), "Earth-like profile should support native life");
    }

    /// <summary>
    /// Tests can_support_native_life for barren world.
    /// </summary>
    public static void TestCanSupportNativeLifeBarren()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 1;
        profile.HasLiquidWater = false;
        profile.OceanCoverage = 0.0;
        profile.AvgTemperatureK = 150.0;
        profile.PressureAtm = 0.0;

        DotNetNativeTestSuite.AssertFalse(profile.CanSupportNativeLife(), "Barren world should not support native life");
    }

    /// <summary>
    /// Tests can_support_native_life requires minimum habitability.
    /// </summary>
    public static void TestCanSupportNativeLifeRequiresMinHab()
    {
        PlanetProfile profile = new();
        profile.HabitabilityScore = 2;
        profile.HasLiquidWater = true;
        profile.OceanCoverage = 0.5;
        profile.AvgTemperatureK = 290.0;
        profile.PressureAtm = 1.0;

        DotNetNativeTestSuite.AssertFalse(profile.CanSupportNativeLife(), "Score 2 should not support native life (minimum is 3)");
    }

    /// <summary>
    /// Tests is_colonizable for various scores.
    /// </summary>
    public static void TestIsColonizable()
    {
        PlanetProfile profile = new();

        profile.HabitabilityScore = 0;
        DotNetNativeTestSuite.AssertFalse(profile.IsColonizable(), "Score 0 should not be colonizable");

        profile.HabitabilityScore = 1;
        DotNetNativeTestSuite.AssertTrue(profile.IsColonizable(), "Score 1 should be colonizable");

        profile.HabitabilityScore = 5;
        DotNetNativeTestSuite.AssertTrue(profile.IsColonizable(), "Score 5 should be colonizable");

        profile.HabitabilityScore = 10;
        DotNetNativeTestSuite.AssertTrue(profile.IsColonizable(), "Score 10 should be colonizable");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        PlanetProfile original = CreateTestProfile();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.HabitabilityScore, restored.HabitabilityScore, "HabitabilityScore should match");
        DotNetNativeTestSuite.AssertFloatNear(original.AvgTemperatureK, restored.AvgTemperatureK, 0.001, "AvgTemperatureK should match");
        DotNetNativeTestSuite.AssertFloatNear(original.PressureAtm, restored.PressureAtm, 0.001, "PressureAtm should match");
        DotNetNativeTestSuite.AssertFloatNear(original.OceanCoverage, restored.OceanCoverage, 0.001, "OceanCoverage should match");
        DotNetNativeTestSuite.AssertFloatNear(original.LandCoverage, restored.LandCoverage, 0.001, "LandCoverage should match");
        DotNetNativeTestSuite.AssertFloatNear(original.IceCoverage, restored.IceCoverage, 0.001, "IceCoverage should match");
        DotNetNativeTestSuite.AssertEqual(original.ContinentCount, restored.ContinentCount, "ContinentCount should match");
        DotNetNativeTestSuite.AssertFloatNear(original.DayLengthHours, restored.DayLengthHours, 0.001, "DayLengthHours should match");
        DotNetNativeTestSuite.AssertFloatNear(original.GravityG, restored.GravityG, 0.001, "GravityG should match");
        DotNetNativeTestSuite.AssertEqual(original.IsTidallyLocked, restored.IsTidallyLocked, "IsTidallyLocked should match");
        DotNetNativeTestSuite.AssertEqual(original.HasAtmosphere, restored.HasAtmosphere, "HasAtmosphere should match");
        DotNetNativeTestSuite.AssertEqual(original.HasLiquidWater, restored.HasLiquidWater, "HasLiquidWater should match");
        DotNetNativeTestSuite.AssertEqual(original.IsMoon, restored.IsMoon, "IsMoon should match");
    }

    /// <summary>
    /// Tests climate zones serialization.
    /// </summary>
    public static void TestClimateZonesSerialization()
    {
        PlanetProfile original = CreateTestProfile();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.ClimateZones.Count, restored.ClimateZones.Count, "Climate zones count should match");
        for (int i = 0; i < original.ClimateZones.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(
                original.ClimateZones[i]["zone"],
                restored.ClimateZones[i]["zone"],
                $"Climate zone {i} should match"
            );
            DotNetNativeTestSuite.AssertFloatNear(
                (double)original.ClimateZones[i]["coverage"],
                (double)restored.ClimateZones[i]["coverage"],
                0.001,
                $"Climate zone {i} coverage should match"
            );
        }
    }

    /// <summary>
    /// Tests biomes serialization.
    /// </summary>
    public static void TestBiomesSerialization()
    {
        PlanetProfile original = CreateTestProfile();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Biomes.Count, restored.Biomes.Count, "Biomes count should match");
        foreach (Variant biomeKey in original.Biomes.Keys)
        {
            DotNetNativeTestSuite.AssertTrue(restored.Biomes.ContainsKey(biomeKey), $"Should contain biome {biomeKey}");
            DotNetNativeTestSuite.AssertFloatNear(
                (double)original.Biomes[biomeKey],
                (double)restored.Biomes[biomeKey],
                0.001,
                $"Biome {biomeKey} coverage should match"
            );
        }
    }

    /// <summary>
    /// Tests resources serialization.
    /// </summary>
    public static void TestResourcesSerialization()
    {
        PlanetProfile original = CreateTestProfile();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Resources.Count, restored.Resources.Count, "Resources count should match");
        foreach (Variant resourceKey in original.Resources.Keys)
        {
            DotNetNativeTestSuite.AssertTrue(restored.Resources.ContainsKey(resourceKey), $"Should contain resource {resourceKey}");
            DotNetNativeTestSuite.AssertFloatNear(
                (double)original.Resources[resourceKey],
                (double)restored.Resources[resourceKey],
                0.001,
                $"Resource {resourceKey} abundance should match"
            );
        }
    }

    /// <summary>
    /// Tests moon-specific fields serialization.
    /// </summary>
    public static void TestMoonFieldsSerialization()
    {
        PlanetProfile original = new();
        original.IsMoon = true;
        original.TidalHeatingFactor = 0.7;
        original.ParentRadiationExposure = 0.4;
        original.EclipseFactor = 0.2;

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(true, restored.IsMoon, "IsMoon should match");
        DotNetNativeTestSuite.AssertFloatNear(0.7, restored.TidalHeatingFactor, 0.001, "TidalHeatingFactor should match");
        DotNetNativeTestSuite.AssertFloatNear(0.4, restored.ParentRadiationExposure, 0.001, "ParentRadiationExposure should match");
        DotNetNativeTestSuite.AssertFloatNear(0.2, restored.EclipseFactor, 0.001, "EclipseFactor should match");
    }

    /// <summary>
    /// Tests empty profile serialization.
    /// </summary>
    public static void TestEmptyProfileSerialization()
    {
        PlanetProfile original = new();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual("", restored.BodyId, "BodyId should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.HabitabilityScore, "HabitabilityScore should be 0");
        DotNetNativeTestSuite.AssertEqual(0, restored.ClimateZones.Count, "ClimateZones should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.Biomes.Count, "Biomes should be empty");
        DotNetNativeTestSuite.AssertEqual(0, restored.Resources.Count, "Resources should be empty");
    }

    /// <summary>
    /// Tests from_dict handles JSON-style string keys for biomes and resources.
    /// </summary>
    public static void TestFromDictJsonStringKeys()
    {
        PlanetProfile original = CreateTestProfile();
        Godot.Collections.Dictionary data = original.ToDictionary();

        Godot.Collections.Dictionary jsonLikeBiomes = new();
        foreach (Variant key in ((Godot.Collections.Dictionary)data["biomes"]).Keys)
        {
            jsonLikeBiomes[key.ToString()] = ((Godot.Collections.Dictionary)data["biomes"])[key];
        }
        data["biomes"] = jsonLikeBiomes;

        Godot.Collections.Dictionary jsonLikeResources = new();
        foreach (Variant key in ((Godot.Collections.Dictionary)data["resources"]).Keys)
        {
            jsonLikeResources[key.ToString()] = ((Godot.Collections.Dictionary)data["resources"])[key];
        }
        data["resources"] = jsonLikeResources;

        PlanetProfile restored = PlanetProfile.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Biomes.Count, restored.Biomes.Count, "Biomes count should match");
        foreach (Variant biomeKey in original.Biomes.Keys)
        {
            int biomeType = (int)biomeKey;
            DotNetNativeTestSuite.AssertTrue(restored.Biomes.ContainsKey(biomeType), $"Should contain biome {biomeType}");
            DotNetNativeTestSuite.AssertFloatNear(
                (double)original.Biomes[biomeType],
                (double)restored.Biomes[biomeType],
                0.001,
                $"Biome {biomeType} should match"
            );
        }

        DotNetNativeTestSuite.AssertEqual(original.Resources.Count, restored.Resources.Count, "Resources count should match");
        foreach (Variant resourceKey in original.Resources.Keys)
        {
            int resourceType = (int)resourceKey;
            DotNetNativeTestSuite.AssertTrue(restored.Resources.ContainsKey(resourceType), $"Should contain resource {resourceType}");
            DotNetNativeTestSuite.AssertFloatNear(
                (double)original.Resources[resourceType],
                (double)restored.Resources[resourceType],
                0.001,
                $"Resource {resourceType} should match"
            );
        }
    }
}
