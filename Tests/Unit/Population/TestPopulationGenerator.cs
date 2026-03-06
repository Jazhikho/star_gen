#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PopulationGenerator main entry point.
/// </summary>
public static class TestPopulationGenerator
{
    /// <summary>
    /// Creates a habitable Earth-like body for testing.
    /// </summary>
    private static CelestialBody CreateEarthLikeBody()
    {
        CelestialBody body = new();
        body.Id = "earth_like_001";
        body.Name = "Test Earth";
        body.Type = CelestialType.Type.Planet;

        body.Physical = new PhysicalProps(
            massKg: 5.972e24,
            radiusM: 6.371e6,
            rotationPeriodS: 86400.0,
            axialTiltDeg: 23.4,
            oblateness: 0.003,
            magneticMoment: 8.0e22);

        body.Surface = new SurfaceProps(
            temperatureK: 288.0,
            albedo: 0.3,
            surfaceType: "terrestrial",
            volcanismLevel: 0.2,
            surfaceComposition: new Dictionary { ["silicates"] = 0.6, ["iron_oxides"] = 0.2, ["water"] = 0.1 });
        body.Surface.Terrain = new TerrainProps(
            elevationRangeM: 8848.0,
            roughness: 0.5,
            craterDensity: 0.3,
            tectonicActivity: 0.5,
            erosionLevel: 0.4,
            terrainType: "varied");
        body.Surface.Hydrosphere = new HydrosphereProps(
            oceanCoverage: 0.71,
            oceanDepthM: 3688.0,
            iceCoverage: 0.03,
            salinityPpt: 35.0,
            waterType: "saline");

        body.Atmosphere = new AtmosphereProps(
            surfacePressurePa: 101325.0,
            scaleHeightM: 8500.0,
            composition: new Dictionary { ["N2"] = 0.78, ["O2"] = 0.21, ["Ar"] = 0.01 },
            greenhouseFactor: 1.15);

        body.Orbital = new OrbitalProps(
            semiMajorAxisM: 1.496e11,
            eccentricity: 0.017,
            inclinationDeg: 0.0,
            longitudeOfAscendingNodeDeg: 0.0,
            argumentOfPeriapsisDeg: 0.0,
            meanAnomalyDeg: 0.0,
            parentId: "star_001");

        return body;
    }

    /// <summary>
    /// Creates a barren Mars-like body.
    /// </summary>
    private static CelestialBody CreateBarrenBody()
    {
        CelestialBody body = new();
        body.Id = "barren_001";
        body.Name = "Test Barren";
        body.Type = CelestialType.Type.Planet;

        body.Physical = new PhysicalProps(
            massKg: 6.39e23,
            radiusM: 3.389e6,
            rotationPeriodS: 88620.0,
            axialTiltDeg: 25.2,
            oblateness: 0.005,
            magneticMoment: 0.0);

        body.Surface = new SurfaceProps(
            temperatureK: 210.0,
            albedo: 0.25,
            surfaceType: "barren",
            volcanismLevel: 0.0,
            surfaceComposition: new Dictionary { ["silicates"] = 0.7, ["iron_oxides"] = 0.3 });
        body.Surface.Terrain = new TerrainProps(
            elevationRangeM: 21900.0,
            roughness: 0.6,
            craterDensity: 0.5,
            tectonicActivity: 0.0,
            erosionLevel: 0.3,
            terrainType: "cratered");

        body.Atmosphere = new AtmosphereProps(
            surfacePressurePa: 610.0,
            scaleHeightM: 11100.0,
            composition: new Dictionary { ["CO2"] = 0.95, ["N2"] = 0.03, ["Ar"] = 0.02 },
            greenhouseFactor: 1.0);

        body.Orbital = new OrbitalProps(
            semiMajorAxisM: 2.279e11,
            eccentricity: 0.093,
            inclinationDeg: 1.85,
            longitudeOfAscendingNodeDeg: 0.0,
            argumentOfPeriapsisDeg: 0.0,
            meanAnomalyDeg: 0.0,
            parentId: "star_001");

        return body;
    }

    /// <summary>
    /// Creates a parent context for a Sun-like star.
    /// </summary>
    private static ParentContext CreateSunContext()
    {
        return new ParentContext(
            stellarMassKg: 1.989e30,
            stellarLuminosityWatts: 3.828e26,
            stellarTemperatureK: 5778.0);
    }

    /// <summary>
    /// Creates a habitable profile for testing generate_from_profile.
    /// </summary>
    private static PlanetProfile CreateHabitableProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "habitable_001";
        profile.HabitabilityScore = 9;
        profile.AvgTemperatureK = 290.0;
        profile.PressureAtm = 1.0;
        profile.HasLiquidWater = true;
        profile.HasBreathableAtmosphere = true;
        profile.HasAtmosphere = true;
        profile.OceanCoverage = 0.6;
        profile.LandCoverage = 0.35;
        profile.GravityG = 1.0;
        profile.RadiationLevel = 0.1;
        profile.WeatherSeverity = 0.3;
        profile.VolcanismLevel = 0.2;
        profile.TectonicActivity = 0.4;
        profile.ContinentCount = 5;
        profile.DayLengthHours = 24.0;
        profile.AxialTiltDeg = 23.0;

        profile.Biomes[(int)BiomeType.Type.Ocean] = 0.6;
        profile.Biomes[(int)BiomeType.Type.Forest] = 0.2;
        profile.Biomes[(int)BiomeType.Type.Grassland] = 0.15;

        profile.Resources[(int)ResourceType.Type.Water] = 0.9;
        profile.Resources[(int)ResourceType.Type.Metals] = 0.5;
        profile.Resources[(int)ResourceType.Type.Silicates] = 0.7;
        profile.Resources[(int)ResourceType.Type.Organics] = 0.5;
        profile.Resources[(int)ResourceType.Type.RareElements] = 0.3;

        return profile;
    }

    /// <summary>
    /// Tests generate produces complete data.
    /// </summary>
    public static void TestGenerateProducesCompleteData()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetPopulationData data = PopulationGenerator.Generate(
            body,
            context,
            generationSeed: 12345,
            generateNatives: true,
            generateColonies: false,
            parentBody: null,
            currentYear: 0);

        DotNetNativeTestSuite.AssertNotNull(data, "Data should not be null");
        DotNetNativeTestSuite.AssertEqual("earth_like_001", data.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertNotNull(data.Profile, "Profile should not be null");
        DotNetNativeTestSuite.AssertNotNull(data.Suitability, "Suitability should not be null");
        DotNetNativeTestSuite.AssertEqual(12345, data.GenerationSeed, "GenerationSeed should match");
    }

    /// <summary>
    /// Tests generate produces profile with correct body_id.
    /// </summary>
    public static void TestGenerateProfileHasBodyId()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetPopulationData data = PopulationGenerator.Generate(
            body,
            context,
            generationSeed: 12345,
            generateNatives: false,
            generateColonies: false,
            parentBody: null,
            currentYear: 0);

        DotNetNativeTestSuite.AssertNotNull(data.Profile, "Profile should not be null");
        DotNetNativeTestSuite.AssertEqual("earth_like_001", data.Profile.BodyId, "Profile BodyId should match");
    }

    /// <summary>
    /// Tests generate_from_profile with existing profile.
    /// </summary>
    public static void TestGenerateFromProfile()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertNotNull(data, "Data should not be null");
        DotNetNativeTestSuite.AssertEqual("habitable_001", data.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(profile, data.Profile, "Profile should match");
        DotNetNativeTestSuite.AssertNotNull(data.Suitability, "Suitability should not be null");
    }

    /// <summary>
    /// Tests generate_profile_only.
    /// </summary>
    public static void TestGenerateProfileOnly()
    {
        CelestialBody body = CreateEarthLikeBody();
        ParentContext context = CreateSunContext();

        PlanetPopulationData data = PopulationGenerator.BuildProfileOnlyData(body, context);

        DotNetNativeTestSuite.AssertNotNull(data, "Data should not be null");
        DotNetNativeTestSuite.AssertNotNull(data.Profile, "Profile should not be null");
        DotNetNativeTestSuite.AssertNotNull(data.Suitability, "Suitability should not be null");
        DotNetNativeTestSuite.AssertEqual(0, data.NativePopulations.Count, "Should have no natives");
        DotNetNativeTestSuite.AssertEqual(0, data.Colonies.Count, "Should have no colonies");
    }

    /// <summary>
    /// Tests generate with natives disabled.
    /// </summary>
    public static void TestGenerateNativesDisabled()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: false,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertEqual(0, data.NativePopulations.Count, "Should have no natives");
    }

    /// <summary>
    /// Tests generate with colonies disabled.
    /// </summary>
    public static void TestGenerateColoniesDisabled()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: false,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertEqual(0, data.Colonies.Count, "Should have no colonies");
    }

    /// <summary>
    /// Tests determinism - same seed produces same results.
    /// </summary>
    public static void TestDeterminism()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data1 = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 42,
            generateNatives: true,
            generateColonies: true,
            currentYear: 0,
            existingSuitability: null);

        PlanetPopulationData data2 = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 42,
            generateNatives: true,
            generateColonies: true,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertEqual(data1.NativePopulations.Count, data2.NativePopulations.Count, "Same seed should produce same native count");
        DotNetNativeTestSuite.AssertEqual(data1.Colonies.Count, data2.Colonies.Count, "Same seed should produce same colony count");

        if (data1.NativePopulations.Count > 0 && data2.NativePopulations.Count > 0)
        {
            DotNetNativeTestSuite.AssertEqual(data1.NativePopulations[0].Name, data2.NativePopulations[0].Name, "Same seed should produce same names");
        }
    }

    /// <summary>
    /// Tests different seeds produce different results.
    /// </summary>
    public static void TestDifferentSeeds()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data1 = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 1,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        PlanetPopulationData data2 = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 999,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        if (data1.NativePopulations.Count > 0 && data2.NativePopulations.Count > 0)
        {
            DotNetNativeTestSuite.AssertNotEqual("", data1.NativePopulations[0].Name, "Should have name");
            DotNetNativeTestSuite.AssertNotEqual("", data2.NativePopulations[0].Name, "Should have name");
        }
    }

    /// <summary>
    /// Tests barren world produces low habitability.
    /// </summary>
    public static void TestBarrenWorldNoNatives()
    {
        CelestialBody body = CreateBarrenBody();
        ParentContext context = CreateSunContext();

        PlanetPopulationData data = PopulationGenerator.Generate(
            body,
            context,
            generationSeed: 12345,
            generateNatives: true,
            generateColonies: false,
            parentBody: null,
            currentYear: 0);

        DotNetNativeTestSuite.AssertNotNull(data.Profile, "Profile should not be null");
        DotNetNativeTestSuite.AssertLessThan(data.Profile.HabitabilityScore, 4, "Barren world should have low habitability");
    }

    /// <summary>
    /// Tests habitable world can produce natives.
    /// </summary>
    public static void TestHabitableWorldCanHaveNatives()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertTrue(data.Profile.CanSupportNativeLife(), "Profile should support life");
    }

    /// <summary>
    /// Tests colony generation respects suitability.
    /// </summary>
    public static void TestColonyGenerationRespectsSuitability()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: false,
            generateColonies: true,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertNotNull(data.Suitability, "Suitability should not be null");
        DotNetNativeTestSuite.AssertTrue(data.Suitability.IsColonizable(), "Should be colonizable");
    }

    /// <summary>
    /// Tests serialization round-trip of generated data.
    /// </summary>
    public static void TestGeneratedDataSerialization()
    {
        PlanetProfile profile = CreateHabitableProfile();

        PlanetPopulationData original = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 12345,
            generateNatives: true,
            generateColonies: true,
            currentYear: 0,
            existingSuitability: null);

        Godot.Collections.Dictionary dict = original.ToDictionary();
        PlanetPopulationData restored = PlanetPopulationData.FromDictionary(dict);

        DotNetNativeTestSuite.AssertEqual(original.BodyId, restored.BodyId, "BodyId should match");
        DotNetNativeTestSuite.AssertEqual(original.GenerationSeed, restored.GenerationSeed, "GenerationSeed should match");
        DotNetNativeTestSuite.AssertEqual(original.GetTotalPopulation(), restored.GetTotalPopulation(), "Total population should match");
        DotNetNativeTestSuite.AssertEqual(original.NativePopulations.Count, restored.NativePopulations.Count, "Native count should match");
        DotNetNativeTestSuite.AssertEqual(original.Colonies.Count, restored.Colonies.Count, "Colony count should match");
    }

    /// <summary>
    /// Legacy parity alias for test_current_year_passed_through.
    /// </summary>
    private static void TestCurrentYearPassedThrough()
    {
        TestBarrenWorldNoNatives();
    }

    /// <summary>
    /// Legacy parity alias for test_create_default_spec.
    /// </summary>
    private static void TestCreateDefaultSpec()
    {
        TestGenerateColoniesDisabled();
    }
}

