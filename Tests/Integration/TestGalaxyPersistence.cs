#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Galaxy;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for GalaxyPersistence.
/// </summary>
public static class TestGalaxyPersistence
{
    private const string TestJsonPath = "user://test_galaxy_save/test_galaxy.json";
    private const string TestBinaryPath = "user://test_galaxy_save/test_galaxy.sgg";
    private const string TestDir = "user://test_galaxy_save/";

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_save_json_and_load_json_round_trip",
            TestSaveJsonAndLoadJsonRoundTrip);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_save_binary_and_load_binary_round_trip",
            TestSaveBinaryAndLoadBinaryRoundTrip);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_load_auto_json",
            TestLoadAutoJson);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_load_auto_binary",
            TestLoadAutoBinary);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_load_json_nonexistent_returns_null",
            TestLoadJsonNonexistentReturnsNull);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_save_json_null_data_returns_error",
            TestSaveJsonNullDataReturnsError);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_save_json_invalid_data_returns_error",
            TestSaveJsonInvalidDataReturnsError);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_save_binary_null_data_returns_error",
            TestSaveBinaryNullDataReturnsError);
        runner.RunNativeTest(
            "TestGalaxyPersistence::test_get_file_filter",
            TestGetFileFilter);
    }

    /// <summary>
    /// Ensures test directory exists before each test.
    /// </summary>
    private static void SetupTestDirectory()
    {
        DirAccess.MakeDirRecursiveAbsolute(TestDir);
    }

    /// <summary>
    /// Creates valid GalaxySaveData for tests.
    /// </summary>
    private static GalaxySaveData CreateTestData()
    {
        GalaxySaveData data = GalaxySaveData.Create(0);
        data.GalaxySeed = 12345;
        data.ZoomLevel = GalaxyCoordinates.ZoomLevel.Sector;
        data.SelectedQuadrant = new Vector3I(1, 0, 2);
        data.SelectedSector = new Vector3I(5, 3, 7);
        data.CameraPosition = new Vector3(100.0f, 50.0f, 200.0f);
        data.CameraRotation = new Vector3(0.1f, 0.5f, 0.0f);
        data.HasStarSelection = true;
        data.SelectedStarSeed = 99999;
        data.SelectedStarPosition = new Vector3(105.0f, 52.0f, 210.0f);
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.UseCaseSettings = CreateTravellerSettings();
        data.SetConfig(config);
        return data;
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
    /// Tests save_json and load_json round-trip.
    /// </summary>
    private static void TestSaveJsonAndLoadJsonRoundTrip()
    {
        SetupTestDirectory();
        try
        {
            GalaxySaveData original = CreateTestData();

            string errMsg = GalaxyPersistence.SaveJson(TestJsonPath, original);
            DotNetNativeTestSuite.AssertEqual("", errMsg, "Save should succeed");

            GalaxySaveData loaded = GalaxyPersistence.LoadJson(TestJsonPath);
            DotNetNativeTestSuite.AssertNotNull(loaded, "Load should return data");

            DotNetNativeTestSuite.AssertEqual(original.GalaxySeed, loaded.GalaxySeed, "Seed should match");
            DotNetNativeTestSuite.AssertEqual(original.ZoomLevel, loaded.ZoomLevel, "Zoom level should match");
            DotNetNativeTestSuite.AssertEqual(original.SelectedQuadrant, loaded.SelectedQuadrant, "Quadrant should match");
            DotNetNativeTestSuite.AssertEqual(original.SelectedSector, loaded.SelectedSector, "Sector should match");
            DotNetNativeTestSuite.AssertTrue(loaded.CameraPosition.IsEqualApprox(original.CameraPosition),
                "Camera position should match");
            DotNetNativeTestSuite.AssertTrue(loaded.CameraRotation.IsEqualApprox(original.CameraRotation),
                "Camera rotation should match");
            DotNetNativeTestSuite.AssertEqual(original.HasStarSelection, loaded.HasStarSelection, "Star selection flag should match");
            DotNetNativeTestSuite.AssertEqual(original.SelectedStarSeed, loaded.SelectedStarSeed, "Star seed should match");
            DotNetNativeTestSuite.AssertTrue(loaded.SelectedStarPosition.IsEqualApprox(original.SelectedStarPosition),
                "Star position should match");
            DotNetNativeTestSuite.AssertNotNull(loaded.GetConfig(), "Galaxy config should round-trip");
            DotNetNativeTestSuite.AssertEqual(
                GenerationUseCaseSettings.RulesetModeType.Traveller,
                loaded.GetConfig()!.UseCaseSettings.RulesetMode,
                "Galaxy save should preserve the active ruleset mode");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests save_binary and load_binary round-trip.
    /// </summary>
    private static void TestSaveBinaryAndLoadBinaryRoundTrip()
    {
        SetupTestDirectory();
        try
        {
            GalaxySaveData original = CreateTestData();

            string errMsg = GalaxyPersistence.SaveBinary(TestBinaryPath, original);
            DotNetNativeTestSuite.AssertEqual("", errMsg, "Save should succeed");

            GalaxySaveData loaded = GalaxyPersistence.LoadBinary(TestBinaryPath);
            DotNetNativeTestSuite.AssertNotNull(loaded, "Load should return data");

            DotNetNativeTestSuite.AssertEqual(original.GalaxySeed, loaded.GalaxySeed, "Seed should match");
            DotNetNativeTestSuite.AssertEqual(original.ZoomLevel, loaded.ZoomLevel, "Zoom level should match");
            DotNetNativeTestSuite.AssertTrue(loaded.CameraPosition.IsEqualApprox(original.CameraPosition),
                "Camera position should match");
            DotNetNativeTestSuite.AssertEqual(original.SelectedStarSeed, loaded.SelectedStarSeed, "Star seed should match");
            DotNetNativeTestSuite.AssertNotNull(loaded.GetConfig(), "Galaxy config should round-trip through binary persistence");
            DotNetNativeTestSuite.AssertTrue(
                loaded.GetConfig()!.UseCaseSettings.ShowTravellerReadouts,
                "Binary galaxy persistence should preserve Traveller readout visibility");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests load_auto with JSON extension.
    /// </summary>
    private static void TestLoadAutoJson()
    {
        SetupTestDirectory();
        try
        {
            GalaxySaveData original = CreateTestData();
            GalaxyPersistence.SaveJson(TestJsonPath, original);

            GalaxySaveData loaded = GalaxyPersistence.LoadAuto(TestJsonPath);
            DotNetNativeTestSuite.AssertNotNull(loaded, "Load auto should return data for .json");
            DotNetNativeTestSuite.AssertEqual(original.GalaxySeed, loaded.GalaxySeed, "Seed should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests load_auto with binary extension.
    /// </summary>
    private static void TestLoadAutoBinary()
    {
        SetupTestDirectory();
        try
        {
            GalaxySaveData original = CreateTestData();
            GalaxyPersistence.SaveBinary(TestBinaryPath, original);

            GalaxySaveData loaded = GalaxyPersistence.LoadAuto(TestBinaryPath);
            DotNetNativeTestSuite.AssertNotNull(loaded, "Load auto should return data for .sgg");
            DotNetNativeTestSuite.AssertEqual(original.GalaxySeed, loaded.GalaxySeed, "Seed should match");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests load_json from non-existent file returns null.
    /// </summary>
    private static void TestLoadJsonNonexistentReturnsNull()
    {
        GalaxySaveData loaded = GalaxyPersistence.LoadJson("user://does_not_exist_galaxy.json");
        DotNetNativeTestSuite.AssertNull(loaded, "Load should return null for missing file");
    }

    /// <summary>
    /// Tests save_json with null data returns error.
    /// </summary>
    private static void TestSaveJsonNullDataReturnsError()
    {
        SetupTestDirectory();
        try
        {
            string errMsg = GalaxyPersistence.SaveJson(TestJsonPath, null);
            DotNetNativeTestSuite.AssertTrue(errMsg.Length > 0, "Should return error for null data");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests save_json with invalid data returns error.
    /// </summary>
    private static void TestSaveJsonInvalidDataReturnsError()
    {
        SetupTestDirectory();
        try
        {
            GalaxySaveData invalid = new();
            invalid.GalaxySeed = 0;

            string errMsg = GalaxyPersistence.SaveJson(TestJsonPath, invalid);
            DotNetNativeTestSuite.AssertTrue(errMsg.Length > 0, "Should return error for invalid data");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests save_binary with null data returns error.
    /// </summary>
    private static void TestSaveBinaryNullDataReturnsError()
    {
        SetupTestDirectory();
        try
        {
            string errMsg = GalaxyPersistence.SaveBinary(TestBinaryPath, null);
            DotNetNativeTestSuite.AssertTrue(errMsg.Length > 0, "Should return error for null data");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    /// <summary>
    /// Tests get_file_filter returns expected format.
    /// </summary>
    private static void TestGetFileFilter()
    {
        string filterStr = GalaxyPersistence.GetFileFilter();
        DotNetNativeTestSuite.AssertTrue(filterStr.Contains("sgg"), "Filter should mention sgg");
        DotNetNativeTestSuite.AssertTrue(filterStr.Contains("json"), "Filter should mention json");
    }

    private static GenerationUseCaseSettings CreateTravellerSettings()
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
        settings.ShowTravellerReadouts = true;
        settings.LifePermissiveness = 0.7;
        settings.PopulationPermissiveness = 0.8;
        settings.MainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require;
        return settings;
    }
}
