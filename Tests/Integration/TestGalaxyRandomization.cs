#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxyRandomization
{
    private const string TestSavePath = "user://test_galaxy_randomization/test_save.sgg";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxyRandomization::test_startup_seed_is_positive", TestStartupSeedIsPositive);
        runner.RunNativeTest("TestGalaxyRandomization::test_startup_seed_is_bounded", TestStartupSeedIsBounded);
        runner.RunNativeTest("TestGalaxyRandomization::test_galaxy_viewer_receives_startup_seed", TestGalaxyViewerReceivesStartupSeed);
        runner.RunNativeTest("TestGalaxyRandomization::test_save_preserves_seed", TestSavePreservesSeed);
        runner.RunNativeTest("TestGalaxyRandomization::test_load_restores_seed_to_galaxy_viewer", TestLoadRestoresSeedToGalaxyViewer);
        runner.RunNativeTest("TestGalaxyRandomization::test_load_updates_mainapp_seed", TestLoadUpdatesMainappSeed);
        runner.RunNativeTest("TestGalaxyRandomization::test_determinism_same_seed_same_spec", TestDeterminismSameSeedSameSpec);
        runner.RunNativeTest("TestGalaxyRandomization::test_save_load_round_trip_file", TestSaveLoadRoundTripFile);
    }

    private static MainApp CreateStartedApp()
    {
        return IntegrationTestUtils.CreateMainAppReadyAndStarted();
    }

    private static void CleanupSaveFile()
    {
        if (FileAccess.FileExists(TestSavePath))
        {
            DirAccess.RemoveAbsolute(TestSavePath);
        }
    }

    private static void TestStartupSeedIsPositive()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertGreaterThan(app.get_galaxy_seed(), 0, "Seed should be positive");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestStartupSeedIsBounded()
    {
        MainApp app = CreateStartedApp();
        try
        {
            DotNetNativeTestSuite.AssertLessThan(app.get_galaxy_seed(), 1000000, "Seed should be bounded");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestGalaxyViewerReceivesStartupSeed()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            DotNetNativeTestSuite.AssertEqual(app.get_galaxy_seed(), viewer!.GalaxySeed, "Viewer seed should match app seed");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSavePreservesSeed()
    {
        MainApp app = CreateStartedApp();
        try
        {
            int originalSeed = app.get_galaxy_seed();
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            GalaxySaveData data = viewer!._saveLoad.CreateSaveData(viewer);
            DotNetNativeTestSuite.AssertEqual(originalSeed, data.GalaxySeed, "Save data should preserve seed");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestLoadRestoresSeedToGalaxyViewer()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            GalaxySaveData saveData = GalaxySaveData.Create(0);
            saveData.GalaxySeed = 12345;
            saveData.ZoomLevel = GalaxyCoordinates.ZoomLevel.Subsector;
            viewer!.apply_save_data(saveData);
            DotNetNativeTestSuite.AssertEqual(12345, viewer.GalaxySeed, "Viewer seed should update after load");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestLoadUpdatesMainappSeed()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            GalaxySaveData saveData = GalaxySaveData.Create(0);
            saveData.GalaxySeed = 54321;
            saveData.ZoomLevel = GalaxyCoordinates.ZoomLevel.Subsector;
            viewer!.apply_save_data(saveData);
            DotNetNativeTestSuite.AssertEqual(54321, app.get_galaxy_seed(), "MainApp seed should update via signal");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestDeterminismSameSeedSameSpec()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");
            GalaxySaveData saveData = GalaxySaveData.Create(0);
            saveData.GalaxySeed = 99999;
            saveData.ZoomLevel = GalaxyCoordinates.ZoomLevel.Galaxy;
            viewer!.apply_save_data(saveData);
            GalaxySpec? specOne = viewer.get_spec();
            viewer.apply_save_data(saveData);
            GalaxySpec? specTwo = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(specOne, "Spec one should exist");
            DotNetNativeTestSuite.AssertNotNull(specTwo, "Spec two should exist");
            DotNetNativeTestSuite.AssertEqual(specOne!.GalaxySeed, specTwo!.GalaxySeed, "Spec seed should match");
            DotNetNativeTestSuite.AssertEqual(specOne.RadiusPc, specTwo.RadiusPc, "Spec radius should match");
            DotNetNativeTestSuite.AssertEqual(specOne.NumArms, specTwo.NumArms, "Spec arm count should match");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestSaveLoadRoundTripFile()
    {
        CleanupSaveFile();
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer? viewer = app.get_galaxy_viewer();
            DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer should exist");

            GalaxySaveData originalData = GalaxySaveData.Create(0);
            originalData.GalaxySeed = 77777;
            originalData.ZoomLevel = GalaxyCoordinates.ZoomLevel.Sector;
            originalData.SelectedQuadrant = new Vector3I(1, 0, 2);
            viewer!.apply_save_data(originalData);
            GalaxySaveData saveData = viewer._saveLoad.CreateSaveData(viewer);

            string saveError = GalaxyPersistence.SaveBinary(TestSavePath, saveData);
            DotNetNativeTestSuite.AssertEqual(string.Empty, saveError, "Binary save should succeed");

            GalaxySaveData differentData = GalaxySaveData.Create(0);
            differentData.GalaxySeed = 11111;
            differentData.ZoomLevel = GalaxyCoordinates.ZoomLevel.Galaxy;
            viewer.apply_save_data(differentData);
            DotNetNativeTestSuite.AssertEqual(11111, viewer.GalaxySeed, "Seed should change before load");

            GalaxySaveData? loadedData = GalaxyPersistence.LoadBinary(TestSavePath);
            DotNetNativeTestSuite.AssertNotNull(loadedData, "Binary load should return data");
            viewer.apply_save_data(loadedData!);

            DotNetNativeTestSuite.AssertEqual(77777, viewer.GalaxySeed, "Viewer seed should restore from file");
            DotNetNativeTestSuite.AssertEqual(77777, app.get_galaxy_seed(), "MainApp seed should restore from file");
        }
        finally
        {
            CleanupSaveFile();
            IntegrationTestUtils.CleanupNode(app);
        }
    }
}
