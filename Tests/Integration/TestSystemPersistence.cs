#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for SystemPersistence.
/// </summary>
public static class TestSystemPersistence
{
    private const string TestJsonPath = "user://test_system.json";
    private const string TestBinaryPath = "user://test_system.sgs";

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestSystemPersistence::test_save_load_json",
            TestSaveLoadJson);
        runner.RunNativeTest(
            "TestSystemPersistence::test_save_load_binary",
            TestSaveLoadBinary);
        runner.RunNativeTest(
            "TestSystemPersistence::test_compression_reduces_size",
            TestCompressionReducesSize);
        runner.RunNativeTest(
            "TestSystemPersistence::test_load_nonexistent",
            TestLoadNonexistent);
        runner.RunNativeTest(
            "TestSystemPersistence::test_format_file_size",
            TestFormatFileSize);
        runner.RunNativeTest(
            "TestSystemPersistence::test_round_trip_full_data",
            TestRoundTripFullData);
        runner.RunNativeTest(
            "TestSystemPersistence::test_generated_system_without_auto_concepts_saves_compact_payload",
            TestGeneratedSystemWithoutAutoConceptsSavesCompactPayload);
    }

    /// <summary>
    /// Creates a test system.
    /// </summary>
    private static SolarSystem CreateTestSystem()
    {
        SolarSystem system = new("test_persist", "Persistence Test");

        StarSpec starSpec = StarSpec.SunLike(99999);
        SeededRng starRng = new(99999);
        CelestialBody star = StarGenerator.Generate(starSpec, starRng);
        star.Id = "star_1";
        system.AddBody(star);

        HierarchyNode starNode = HierarchyNode.CreateStar("node_star", "star_1");
        system.Hierarchy = new SystemHierarchy(starNode);

        OrbitHost host = new("node_star", OrbitHost.HostType.SType);
        host.CombinedMassKg = star.Physical.MassKg;
        system.AddOrbitHost(host);

        return system;
    }

    /// <summary>
    /// Cleans up test files after each test.
    /// </summary>
    private static void CleanupTestFiles()
    {
        if (FileAccess.FileExists(TestJsonPath))
        {
            DirAccess.RemoveAbsolute(TestJsonPath);
        }
        if (FileAccess.FileExists(TestBinaryPath))
        {
            DirAccess.RemoveAbsolute(TestBinaryPath);
        }
    }

    /// <summary>
    /// Tests save and load JSON.
    /// </summary>
    private static void TestSaveLoadJson()
    {
        try
        {
            SolarSystem original = CreateTestSystem();

            Error saveError = SystemPersistence.Save(original, TestJsonPath, false);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Save should succeed");

            SystemPersistenceLoadResult result = SystemPersistence.Load(TestJsonPath);

            DotNetNativeTestSuite.AssertTrue(result.Success, "Load should succeed");
            DotNetNativeTestSuite.AssertNotNull(result.System);
            DotNetNativeTestSuite.AssertEqual(original.Id, result.System.Id);
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests save and load compressed binary.
    /// </summary>
    private static void TestSaveLoadBinary()
    {
        try
        {
            SolarSystem original = CreateTestSystem();

            Error saveError = SystemPersistence.Save(original, TestBinaryPath, true);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Save should succeed");

            SystemPersistenceLoadResult result = SystemPersistence.Load(TestBinaryPath);

            DotNetNativeTestSuite.AssertTrue(result.Success, "Load should succeed");
            DotNetNativeTestSuite.AssertNotNull(result.System);
            DotNetNativeTestSuite.AssertEqual(original.Id, result.System.Id);
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests compression reduces file size.
    /// </summary>
    private static void TestCompressionReducesSize()
    {
        try
        {
            SolarSystem system = CreateTestSystem();

            SystemPersistence.Save(system, TestJsonPath, false);
            SystemPersistence.Save(system, TestBinaryPath, true);

            int jsonSize = (int)SystemPersistence.GetFileSize(TestJsonPath);
            int binarySize = (int)SystemPersistence.GetFileSize(TestBinaryPath);

            DotNetNativeTestSuite.AssertTrue(jsonSize > 0, "JSON file should have size");
            DotNetNativeTestSuite.AssertTrue(binarySize > 0, "Binary file should have size");
            DotNetNativeTestSuite.AssertTrue(binarySize < jsonSize, "Compressed should be smaller");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests load nonexistent file.
    /// </summary>
    private static void TestLoadNonexistent()
    {
        SystemPersistenceLoadResult result = SystemPersistence.Load("user://nonexistent.sgs");

        DotNetNativeTestSuite.AssertFalse(result.Success);
        DotNetNativeTestSuite.AssertTrue(result.ErrorMessage.Contains("not found"));
    }

    /// <summary>
    /// Tests format_file_size.
    /// </summary>
    private static void TestFormatFileSize()
    {
        DotNetNativeTestSuite.AssertEqual("500 B", SystemPersistence.FormatFileSize(500));
        DotNetNativeTestSuite.AssertEqual("1.5 KB", SystemPersistence.FormatFileSize(1500));
        DotNetNativeTestSuite.AssertEqual("1.4 MB", SystemPersistence.FormatFileSize(1500000));
    }

    /// <summary>
    /// Tests round-trip preserves all data.
    /// </summary>
    private static void TestRoundTripFullData()
    {
        try
        {
            SolarSystem original = CreateTestSystem();

            CelestialBody planet = new("planet_1", "Test Planet", CelestialType.Type.Planet);
            planet.Physical = new PhysicalProps(Units.EarthMassKg, Units.EarthRadiusMeters, 86400.0);
            planet.Orbital = new OrbitalProps(Units.AuMeters, 0.0, 0.0, 0.0, 0.0, 0.0, "node_star");
            original.AddBody(planet);

            AsteroidBelt belt = new("belt_1", "Test Belt");
            belt.InnerRadiusM = 2.0 * Units.AuMeters;
            belt.OuterRadiusM = 3.0 * Units.AuMeters;
            original.AddAsteroidBelt(belt);

            SystemPersistence.Save(original, TestBinaryPath, true);
            SystemPersistenceLoadResult result = SystemPersistence.Load(TestBinaryPath);

            DotNetNativeTestSuite.AssertTrue(result.Success);
            DotNetNativeTestSuite.AssertEqual(1, result.System.PlanetIds.Count);
            DotNetNativeTestSuite.AssertEqual(1, result.System.AsteroidBelts.Count);
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests generated systems stay on the compact save path when concept layers are not auto-persisted.
    /// </summary>
    private static void TestGeneratedSystemWithoutAutoConceptsSavesCompactPayload()
    {
        try
        {
            SolarSystemSpec spec = SolarSystemSpec.Binary(11223);
            spec.IncludeAsteroidBelts = true;
            spec.GeneratePopulation = true;
            spec.UseCaseSettings = CreateTravellerSettings();

            SolarSystem? generated = SystemFixtureGenerator.GenerateSystem(spec);
            DotNetNativeTestSuite.AssertNotNull(generated, "Generated system should exist");

            Error saveError = SystemPersistence.Save(generated, TestJsonPath, compress: false);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Compact JSON save should succeed");

            string json = FileAccess.GetFileAsString(TestJsonPath);
            Json parser = new();
            DotNetNativeTestSuite.AssertEqual(Error.Ok, parser.Parse(json), "Saved JSON should parse");

            Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)parser.Data;
            DotNetNativeTestSuite.AssertEqual((int)SystemPersistence.SaveMode.Compact, (int)data["save_mode"], "Generated systems without auto concept state should save compact payloads");
            DotNetNativeTestSuite.AssertFalse(data.ContainsKey("system"), "Compact payload should avoid the serialized system snapshot");
            DotNetNativeTestSuite.AssertTrue(data.ContainsKey("spec"), "Compact payload should preserve the regeneration spec");

            SystemPersistenceLoadResult result = SystemPersistence.Load(TestJsonPath);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Compact payload should load");
            DotNetNativeTestSuite.AssertNotNull(result.System, "Loaded system should exist");
            DotNetNativeTestSuite.AssertEqual(generated.StarIds.Count, result.System.StarIds.Count, "Loaded compact system should preserve star count");
            DotNetNativeTestSuite.AssertNotNull(result.System.Provenance, "Loaded compact system should preserve provenance");
            DotNetNativeTestSuite.AssertTrue(result.System.Provenance.SpecSnapshot.ContainsKey("use_case_settings"), "Compact system payload should keep use-case settings in the spec snapshot");
            DotNetNativeTestSuite.AssertFalse(result.System.HasConceptResults(), "Loaded system should remain free of auto-generated concept state");

            Godot.Collections.Dictionary settingsData = (Godot.Collections.Dictionary)result.System.Provenance.SpecSnapshot["use_case_settings"];
            GenerationUseCaseSettings settings = GenerationUseCaseSettings.FromDictionary(settingsData);
            DotNetNativeTestSuite.AssertEqual(GenerationUseCaseSettings.RulesetModeType.Traveller, settings.RulesetMode, "Compact system persistence should preserve ruleset mode");
            DotNetNativeTestSuite.AssertEqual(GenerationUseCaseSettings.MainworldPolicyType.Require, settings.MainworldPolicy, "Compact system persistence should preserve mainworld policy");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    private static GenerationUseCaseSettings CreateTravellerSettings()
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
        settings.ShowTravellerReadouts = true;
        settings.LifePermissiveness = 0.65;
        settings.PopulationPermissiveness = 0.8;
        settings.MainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require;
        return settings;
    }
}
