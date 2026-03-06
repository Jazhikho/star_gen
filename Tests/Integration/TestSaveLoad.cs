#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for the save/load system.
/// </summary>
public static class TestSaveLoad
{
    private const string TestDir = "user://test_saves/";

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestSaveLoad::test_save_and_load_star_compressed",
            TestSaveAndLoadStarCompressed);
        runner.RunNativeTest(
            "TestSaveLoad::test_save_and_load_planet_json",
            TestSaveAndLoadPlanetJson);
        runner.RunNativeTest(
            "TestSaveLoad::test_save_and_load_moon",
            TestSaveAndLoadMoon);
        runner.RunNativeTest(
            "TestSaveLoad::test_save_and_load_asteroid",
            TestSaveAndLoadAsteroid);
        runner.RunNativeTest(
            "TestSaveLoad::test_load_invalid_file_fails_gracefully",
            TestLoadInvalidFileFailsGracefully);
        runner.RunNativeTest(
            "TestSaveLoad::test_load_invalid_json_fails_gracefully",
            TestLoadInvalidJsonFailsGracefully);
        runner.RunNativeTest(
            "TestSaveLoad::test_load_wrong_format_fails_gracefully",
            TestLoadWrongFormatFailsGracefully);
        runner.RunNativeTest(
            "TestSaveLoad::test_compressed_file_is_smaller",
            TestCompressedFileIsSmaller);
        runner.RunNativeTest(
            "TestSaveLoad::test_full_save_mode",
            TestFullSaveMode);
        runner.RunNativeTest(
            "TestSaveLoad::test_roundtrip_preserves_provenance",
            TestRoundtripPreservesProvenance);
        runner.RunNativeTest(
            "TestSaveLoad::test_save_null_body_fails",
            TestSaveNullBodyFails);
        runner.RunNativeTest(
            "TestSaveLoad::test_file_size_formatting",
            TestFileSizeFormatting);
    }

    /// <summary>
    /// Ensures test directory exists.
    /// </summary>
    private static void SetupTestDirectory()
    {
        DirAccess.MakeDirRecursiveAbsolute(TestDir);
    }

    /// <summary>
    /// Cleans up test files after each test.
    /// </summary>
    private static void CleanupTestFiles()
    {
        DirAccess dir = DirAccess.Open(TestDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir())
                {
                    dir.Remove(fileName);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }

    /// <summary>
    /// Creates a test star.
    /// </summary>
    private static CelestialBody CreateTestStar(int seedVal)
    {
        StarSpec spec = StarSpec.Random(seedVal);
        SeededRng rng = new(seedVal);
        return StarGenerator.Generate(spec, rng);
    }

    /// <summary>
    /// Creates a test planet.
    /// </summary>
    private static CelestialBody CreateTestPlanet(int seedVal)
    {
        PlanetSpec spec = PlanetSpec.Random(seedVal);
        ParentContext context = ParentContext.SunLike();
        SeededRng rng = new(seedVal);
        return PlanetGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Creates a test moon.
    /// </summary>
    private static CelestialBody CreateTestMoon(int seedVal)
    {
        MoonSpec spec = MoonSpec.Random(seedVal);
        ParentContext context = ParentContext.ForMoon(
            Units.SolarMassKg,
            3.828e26,
            5778.0,
            4.6e9,
            5.2 * Units.AuMeters,
            1.898e27,
            6.9911e7,
            5.0e8
        );
        SeededRng rng = new(seedVal);
        return MoonGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Creates a test asteroid.
    /// </summary>
    private static CelestialBody CreateTestAsteroid(int seedVal)
    {
        AsteroidSpec spec = AsteroidSpec.Random(seedVal);
        ParentContext context = ParentContext.SunLike(2.7 * Units.AuMeters);
        SeededRng rng = new(seedVal);
        return AsteroidGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Tests saving and loading a star (compressed).
    /// </summary>
    private static void TestSaveAndLoadStarCompressed()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestStar(12345);
            string path = TestDir + "test_star.sgb";

            Error saveError = SaveData.SaveBody(original, path, SaveData.SaveMode.Compact, true);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Should save without error");

            SaveDataLoadResult result = SaveData.LoadBody(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully: " + result.ErrorMessage);
            DotNetNativeTestSuite.AssertNotNull(result.Body, "Should have loaded body");

            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Star, result.Body.Type, "Should be a star");
            DotNetNativeTestSuite.AssertEqual(original.Physical.MassKg, result.Body.Physical.MassKg, "Mass should match");
            DotNetNativeTestSuite.AssertEqual(original.Physical.RadiusM, result.Body.Physical.RadiusM, "Radius should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests saving and loading a planet (JSON).
    /// </summary>
    private static void TestSaveAndLoadPlanetJson()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestPlanet(23456);
            string path = TestDir + "test_planet.json";

            Error saveError = SaveData.SaveBody(original, path, SaveData.SaveMode.Compact, false);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Should save without error");

            SaveDataLoadResult result = SaveData.LoadBody(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully: " + result.ErrorMessage);
            DotNetNativeTestSuite.AssertNotNull(result.Body, "Should have loaded body");

            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Planet, result.Body.Type, "Should be a planet");
            DotNetNativeTestSuite.AssertEqual(original.Physical.MassKg, result.Body.Physical.MassKg, "Mass should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests saving and loading a moon.
    /// </summary>
    private static void TestSaveAndLoadMoon()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestMoon(34567);
            string path = TestDir + "test_moon.sgb";

            Error saveError = SaveData.SaveBody(original, path);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Should save without error");

            SaveDataLoadResult result = SaveData.LoadBody(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully: " + result.ErrorMessage);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Moon, result.Body.Type, "Should be a moon");
            DotNetNativeTestSuite.AssertEqual(original.Physical.MassKg, result.Body.Physical.MassKg, "Mass should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests saving and loading an asteroid.
    /// </summary>
    private static void TestSaveAndLoadAsteroid()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestAsteroid(45678);
            string path = TestDir + "test_asteroid.sgb";

            Error saveError = SaveData.SaveBody(original, path);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Should save without error");

            SaveDataLoadResult result = SaveData.LoadBody(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully: " + result.ErrorMessage);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Asteroid, result.Body.Type, "Should be an asteroid");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests loading invalid file fails gracefully.
    /// </summary>
    private static void TestLoadInvalidFileFailsGracefully()
    {
        SaveDataLoadResult result = SaveData.LoadBody(TestDir + "nonexistent.sgb");

        DotNetNativeTestSuite.AssertFalse(result.Success, "Should fail to load nonexistent file");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrEmpty(result.ErrorMessage), "Should have error message");
        DotNetNativeTestSuite.AssertNull(result.Body, "Should not have body");
    }

    /// <summary>
    /// Tests loading invalid JSON fails gracefully.
    /// </summary>
    private static void TestLoadInvalidJsonFailsGracefully()
    {
        SetupTestDirectory();
        try
        {
            string path = TestDir + "invalid.json";
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreString("{ this is not valid json }");
            file.Close();

            SaveDataLoadResult result = SaveData.LoadBody(path);

            DotNetNativeTestSuite.AssertFalse(result.Success, "Should fail to load invalid JSON");
            DotNetNativeTestSuite.AssertTrue(result.ErrorMessage.Contains("Invalid JSON"), "Error should mention invalid JSON");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests loading wrong format fails gracefully.
    /// </summary>
    private static void TestLoadWrongFormatFailsGracefully()
    {
        SetupTestDirectory();
        try
        {
            string path = TestDir + "wrong_format.sgb";
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreString("NOT A STARGEN FILE");
            file.Close();

            SaveDataLoadResult result = SaveData.LoadBody(path);

            DotNetNativeTestSuite.AssertFalse(result.Success, "Should fail to load wrong format");
            DotNetNativeTestSuite.AssertTrue(result.ErrorMessage.Contains("Invalid file format"), "Error should mention format");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests compressed file is smaller than JSON.
    /// </summary>
    private static void TestCompressedFileIsSmaller()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody body = CreateTestPlanet(56789);
            string jsonPath = TestDir + "size_test.json";
            string compressedPath = TestDir + "size_test.sgb";

            SaveData.SaveBody(body, jsonPath, SaveData.SaveMode.Compact, false);
            SaveData.SaveBody(body, compressedPath, SaveData.SaveMode.Compact, true);

            int jsonSize = (int)SaveData.GetFileSize(jsonPath);
            int compressedSize = (int)SaveData.GetFileSize(compressedPath);

            DotNetNativeTestSuite.AssertTrue(compressedSize < jsonSize,
                $"Compressed ({compressedSize} bytes) should be smaller than JSON ({jsonSize} bytes)");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests full save mode.
    /// </summary>
    private static void TestFullSaveMode()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestPlanet(67890);
            string path = TestDir + "test_full.sgb";

            Error saveError = SaveData.SaveBody(original, path, SaveData.SaveMode.Full, true);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Should save without error");

            SaveDataLoadResult result = SaveData.LoadBody(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully");
            DotNetNativeTestSuite.AssertEqual(original.Type, result.Body.Type, "Type should match");
            DotNetNativeTestSuite.AssertEqual(original.Id, result.Body.Id, "ID should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests roundtrip preserves provenance.
    /// </summary>
    private static void TestRoundtripPreservesProvenance()
    {
        SetupTestDirectory();
        try
        {
            CelestialBody original = CreateTestStar(78901);
            string path = TestDir + "test_provenance.sgb";

            SaveData.SaveBody(original, path);
            SaveDataLoadResult result = SaveData.LoadBody(path);

            DotNetNativeTestSuite.AssertTrue(result.Success, "Should load successfully");
            DotNetNativeTestSuite.AssertNotNull(result.Body.Provenance, "Should have provenance");
            DotNetNativeTestSuite.AssertEqual(original.Provenance.GenerationSeed, result.Body.Provenance.GenerationSeed,
                "Seed should be preserved");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests saving null body fails.
    /// </summary>
    private static void TestSaveNullBodyFails()
    {
        SetupTestDirectory();
        try
        {
            string path = TestDir + "test_null.sgb";
            Error error = SaveData.SaveBody(null, path);

            DotNetNativeTestSuite.AssertTrue(error != Error.Ok, "Should fail to save null body");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests file size formatting.
    /// </summary>
    private static void TestFileSizeFormatting()
    {
        string bytesStr = SaveData.FormatFileSize(512);
        DotNetNativeTestSuite.AssertTrue(bytesStr.Contains("512"), "Should format bytes correctly");

        string kbStr = SaveData.FormatFileSize(2048);
        DotNetNativeTestSuite.AssertTrue(kbStr.Contains("KB"), "Should format KB correctly");

        string mbStr = SaveData.FormatFileSize(2 * 1024 * 1024);
        DotNetNativeTestSuite.AssertTrue(mbStr.Contains("MB"), "Should format MB correctly");
    }
}
