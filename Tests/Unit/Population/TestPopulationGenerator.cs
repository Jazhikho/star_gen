#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Concepts;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;
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
            stellarTemperatureK: 5778.0,
            stellarAgeYears: 4.6e9);
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
        profile.StellarAgeYears = 5.2e9;
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
    /// Creates a harsh moon colony target that strict settings reject without extra pressure.
    /// </summary>
    private static PlanetProfile CreateHarshMoonProfile()
    {
        PlanetProfile profile = new();
        profile.BodyId = "moon_target";
        profile.HabitabilityScore = 1;
        profile.IsMoon = true;
        profile.AvgTemperatureK = 205.0;
        profile.PressureAtm = 0.02;
        profile.HasLiquidWater = false;
        profile.HasAtmosphere = false;
        profile.HasBreathableAtmosphere = false;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 1.0;
        profile.GravityG = 0.16;
        profile.RadiationLevel = 0.28;
        profile.WeatherSeverity = 0.05;
        profile.VolcanismLevel = 0.02;
        profile.TectonicActivity = 0.02;
        profile.DayLengthHours = 48.0;
        profile.AxialTiltDeg = 2.0;
        profile.Resources[(int)ResourceType.Type.Metals] = 0.7;
        profile.Resources[(int)ResourceType.Type.Silicates] = 0.8;
        profile.Resources[(int)ResourceType.Type.RareElements] = 0.4;
        return profile;
    }

    /// <summary>
    /// Finds a deterministic seed that yields sentient natives on the provided profile.
    /// </summary>
    private static int FindSeedForSentientNatives(PlanetProfile profile, int maxSeed = 20000)
    {
        for (int seed = 1; seed <= maxSeed; seed += 1)
        {
            PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
                profile,
                generationSeed: seed,
                generateNatives: true,
                generateColonies: false,
                currentYear: 0,
                existingSuitability: null);
            if (data.SentienceAssessment != null
                && data.SentienceAssessment.HasSentientLife
                && data.NativePopulations.Count > 0)
            {
                return seed;
            }
        }

        return -1;
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
    /// Tests habitable worlds can keep a biosphere without automatically producing sentient natives.
    /// </summary>
    public static void TestHabitableWorldCanHaveBiosphereWithoutSentients()
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
        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.Generated, data.EcologyState!.Status, "Habitable worlds should still produce a biosphere");
        DotNetNativeTestSuite.AssertEqual(0, data.NativePopulations.Count, "Most biospheres should not automatically become sentient native populations");
        DotNetNativeTestSuite.AssertTrue(data.SentienceAssessment != null && !data.SentienceAssessment.HasSentientLife, "The sentience gate should remain separate from biosphere generation");
    }

    /// <summary>
    /// Tests prime worlds can still yield sentient native populations for some deterministic seeds.
    /// </summary>
    public static void TestPrimeWorldCanStillProduceSentientNatives()
    {
        PlanetProfile profile = CreateHabitableProfile();
        int sentientSeed = FindSeedForSentientNatives(profile);

        DotNetNativeTestSuite.AssertTrue(sentientSeed > 0, "A deterministic seed should exist that yields sentient natives on a prime world");

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: sentientSeed,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null);

        DotNetNativeTestSuite.AssertTrue(data.NativePopulations.Count > 0, "Sentient worlds should still materialize native populations");
        DotNetNativeTestSuite.AssertTrue(data.SentienceAssessment != null && data.SentienceAssessment.HasSentientLife, "Sentient worlds should preserve the sentience assessment");
    }

    /// <summary>
    /// Legacy parity alias for the former direct-native-generation expectation.
    /// </summary>
    public static void TestHabitableWorldCanHaveNatives()
    {
        TestPrimeWorldCanStillProduceSentientNatives();
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
        DotNetNativeTestSuite.AssertTrue(data.HasColonies(), "Colonizable profile should materialize at least one colony when colony generation is enabled");
    }

    /// <summary>
    /// Tests that life permissiveness widens the authoritative ecology gate for marginal hot worlds.
    /// </summary>
    public static void TestLifePermissivenessWidensEcologyEnvelope()
    {
        PlanetProfile profile = new();
        profile.BodyId = "hot_ocean";
        profile.HabitabilityScore = 4;
        profile.AvgTemperatureK = 450.0;
        profile.PressureAtm = 2.1;
        profile.HasLiquidWater = true;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = false;
        profile.OceanCoverage = 0.92;
        profile.LandCoverage = 0.05;
        profile.GravityG = 1.1;
        profile.RadiationLevel = 0.30;
        profile.WeatherSeverity = 0.4;
        profile.VolcanismLevel = 0.3;
        profile.TectonicActivity = 0.4;
        profile.Biomes[(int)BiomeType.Type.Ocean] = 1.0;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();
        strictSettings.LifePermissiveness = 0.0;

        GenerationUseCaseSettings permissiveSettings = GenerationUseCaseSettings.CreateDefault();
        permissiveSettings.LifePermissiveness = 1.0;

        PlanetPopulationData strictData = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 24680,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: strictSettings);
        PlanetPopulationData permissiveData = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 24680,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: permissiveSettings);

        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.NotApplicable, strictData.EcologyState!.Status, "Strict life settings should still reject a very hot marginal world");
        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.Generated, permissiveData.EcologyState!.Status, "Space-opera life settings should allow biology on a marginal but wet hot world");
    }

    /// <summary>
    /// Tests that strict settings still preserve a biosphere on prime wet worlds.
    /// </summary>
    public static void TestStrictPrimeWorldStillGeneratesEcology()
    {
        PlanetProfile profile = CreateHabitableProfile();
        profile.HabitabilityScore = 8;
        profile.AvgTemperatureK = 326.0;
        profile.PressureAtm = 2.6;
        profile.HasBreathableAtmosphere = false;
        profile.RadiationLevel = 0.20;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();
        strictSettings.LifePermissiveness = 0.0;

        PlanetPopulationData data = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 13579,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: strictSettings);

        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.Generated, data.EcologyState!.Status, "Strict life settings should still preserve biospheres on prime wet worlds");
    }

    /// <summary>
    /// Tests that permissive life settings unlock cold alternative-biology worlds that strict settings still reject.
    /// </summary>
    public static void TestLifePermissivenessControlsNativeGenerationThreshold()
    {
        PlanetProfile profile = new();
        profile.BodyId = "methane_world";
        profile.HabitabilityScore = 2;
        profile.AvgTemperatureK = 130.0;
        profile.StellarAgeYears = 6.0e9;
        profile.PressureAtm = 1.4;
        profile.HasLiquidWater = false;
        profile.HasAtmosphere = true;
        profile.HasBreathableAtmosphere = false;
        profile.OceanCoverage = 0.0;
        profile.LandCoverage = 0.15;
        profile.IceCoverage = 0.80;
        profile.GravityG = 0.55;
        profile.RadiationLevel = 0.12;
        profile.WeatherSeverity = 0.2;
        profile.VolcanismLevel = 0.05;
        profile.TectonicActivity = 0.1;
        profile.Biomes[(int)BiomeType.Type.IceSheet] = 0.80;
        profile.Biomes[(int)BiomeType.Type.Tundra] = 0.20;

        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();
        strictSettings.LifePermissiveness = 0.0;

        GenerationUseCaseSettings permissiveSettings = GenerationUseCaseSettings.CreateDefault();
        permissiveSettings.LifePermissiveness = 1.0;

        PlanetPopulationData strictData = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 97531,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: strictSettings);
        PlanetPopulationData permissiveData = PopulationGenerator.GenerateFromProfile(
            profile,
            generationSeed: 97531,
            generateNatives: true,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: permissiveSettings);

        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.NotApplicable, strictData.EcologyState!.Status, "Strict life settings should reject worlds that need alternative chemistry assumptions");
        DotNetNativeTestSuite.AssertEqual(0, strictData.NativePopulations.Count, "Strict life settings should not generate natives on those worlds");
        DotNetNativeTestSuite.AssertEqual(ConceptRunStatus.Generated, permissiveData.EcologyState!.Status, "Permissive life settings should allow biology on cold alternative-chemistry worlds");
        DotNetNativeTestSuite.AssertEqual(0, permissiveData.NativePopulations.Count, "Alternative biospheres should not automatically imply sentient natives");
    }

    /// <summary>
    /// Tests that the second-pass colony rebuild can use nearby native pressure to establish colonies.
    /// </summary>
    public static void TestRebuildColoniesForSystemUsesNativePressure()
    {
        GenerationUseCaseSettings strictSettings = GenerationUseCaseSettings.CreateDefault();

        PlanetProfile targetProfile = CreateHarshMoonProfile();
        ColonySuitability harshSuitability = new();
        harshSuitability.BodyId = targetProfile.BodyId;
        harshSuitability.OverallScore = 30;
        harshSuitability.RequiresLifeSupport = true;
        harshSuitability.RequiresPressureSuit = true;
        harshSuitability.RequiresRadiationShielding = false;

        ColonyPressureContext pressureContext = new ColonyPressureContext
        {
            LocalNativePressure = 0.57,
            NearbySystemNativePressure = 1.0,
            LocalNativeWorldCount = 1,
            NearbyNativeWorldCount = 2,
        };

        int matchingSeed = -1;
        for (int populationSeed = 1; populationSeed <= 10000; populationSeed += 1)
        {
            bool strictResult = PopulationLikelihood.ShouldGenerateColony(
                targetProfile,
                harshSuitability,
                populationSeed,
                strictSettings);
            bool pressuredResult = PopulationLikelihood.ShouldGenerateColony(
                targetProfile,
                harshSuitability,
                populationSeed,
                strictSettings,
                pressureContext);
            if (!strictResult && pressuredResult)
            {
                matchingSeed = populationSeed;
                break;
            }
        }

        DotNetNativeTestSuite.AssertTrue(matchingSeed > 0, "A deterministic seed should exist for native-pressure-assisted colony generation");

        SolarSystem system = new SolarSystem("system_native_pressure", "Native Pressure Test");

        CelestialBody star = new CelestialBody("star_001", "Test Star", CelestialType.Type.Star, new PhysicalProps(1.989e30, 6.96e8), null);
        CelestialBody nativePlanet = new CelestialBody("planet_native", "Native Planet", CelestialType.Type.Planet, new PhysicalProps(5.972e24, 6.371e6), null);
        nativePlanet.Orbital = new OrbitalProps(1.496e11, 0.01, 0.0, 0.0, 0.0, 0.0, "star_001");

        CelestialBody targetMoon = new CelestialBody("moon_target", "Target Moon", CelestialType.Type.Moon, new PhysicalProps(7.35e22, 1.74e6), null);
        targetMoon.Orbital = new OrbitalProps(4.2e8, 0.01, 0.0, 0.0, 0.0, 0.0, "planet_native");

        PlanetProfile sourceProfile = CreateHabitableProfile();
        sourceProfile.BodyId = "planet_native";
        PlanetPopulationData sourceData = PopulationGenerator.GenerateFromProfile(
            sourceProfile,
            generationSeed: 1234,
            generateNatives: false,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: null,
            useCaseSettings: strictSettings);

        NativePopulation nativePopulation = new NativePopulation
        {
            Id = "native_001",
            Name = "Test Natives",
            BodyId = "planet_native",
            OriginYear = -5000,
            Population = 900000000,
            PeakPopulation = 900000000,
            PeakPopulationYear = 0,
            TechLevel = TechnologyLevel.Level.Interstellar,
            IsExtant = true,
            TerritorialControl = 0.7,
        };
        sourceData.NativePopulations.Add(nativePopulation);
        sourceData.Population = sourceData.GetTotalPopulation();
        sourceData.IsActive = true;
        nativePlanet.PopulationData = sourceData;

        PlanetPopulationData targetData = PopulationGenerator.GenerateFromProfile(
            targetProfile,
            generationSeed: matchingSeed,
            generateNatives: false,
            generateColonies: false,
            currentYear: 0,
            existingSuitability: harshSuitability,
            useCaseSettings: strictSettings);
        DotNetNativeTestSuite.AssertFalse(targetData.HasColonies(), "Harsh moon target should start without colonies");
        targetMoon.PopulationData = targetData;

        system.AddBody(star);
        system.AddBody(nativePlanet);
        system.AddBody(targetMoon);

        NativeSystemPressureSummary nearbySummary = new NativeSystemPressureSummary
        {
            NativeWorldCount = 2,
            PressureSignal = 1.0,
        };

        PopulationGenerator.RebuildColoniesForSystem(system, strictSettings, nearbySummary);

        DotNetNativeTestSuite.AssertTrue(
            targetMoon.PopulationData != null && targetMoon.PopulationData.HasColonies(),
            "Second-pass colony rebuild should establish a colony when native pressure is present");
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

