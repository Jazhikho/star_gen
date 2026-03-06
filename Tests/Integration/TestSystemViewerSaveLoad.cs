#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.SystemViewer;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static partial class TestSystemViewerSaveLoad
{
    private const string SystemViewerScenePath = "res://src/app/system_viewer/SystemViewer.tscn";
    private const string TestBinaryPath = "user://test_viewer_system.sgs";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_save_load_class_exists", TestSaveLoadClassExists);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_viewer_has_save_load_ui", TestViewerHasSaveLoadUi);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_viewer_has_get_current_system", TestViewerHasGetCurrentSystem);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_update_seed_display_updates_seed_input", TestUpdateSeedDisplayUpdatesSeedInput);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_escape_key_requests_return_to_galaxy", TestEscapeKeyRequestsReturnToGalaxy);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_viewer_tooltips_are_configured", TestViewerTooltipsAreConfigured);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_programmatic_save", TestProgrammaticSave);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_programmatic_load", TestProgrammaticLoad);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_round_trip_preserves_data", TestRoundTripPreservesData);
        runner.RunNativeTest("TestSystemViewerSaveLoad::test_load_nonexistent_file", TestLoadNonexistentFile);
    }

    private sealed partial class MockViewer : Node, ISystemViewerSaveLoadHost
    {
        internal SolarSystem? System;
        internal string StatusMessage = string.Empty;

        public SolarSystem? GetCurrentSystem()
        {
            return System;
        }

        public void SetStatus(string message)
        {
            StatusMessage = message;
        }

        public void SetError(string message)
        {
            StatusMessage = "Error: " + message;
        }

        public void UpdateSeedDisplay(int _seedValue)
        {
        }

        public void DisplaySystem(SolarSystem system)
        {
            System = system;
        }
    }

    private static void CleanupTestFiles()
    {
        if (FileAccess.FileExists(TestBinaryPath))
        {
            DirAccess.RemoveAbsolute(TestBinaryPath);
        }
    }

    private static void TestSaveLoadClassExists()
    {
        SystemViewerSaveLoad saveLoad = new();
        DotNetNativeTestSuite.AssertNotNull(saveLoad, "SystemViewerSaveLoad should instantiate");
    }

    private static void TestViewerHasSaveLoadUi()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(
                viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton"),
                "Viewer should have SaveButton");
            DotNetNativeTestSuite.AssertNotNull(
                viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton"),
                "Viewer should have LoadButton");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerHasGetCurrentSystem()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer, "Viewer should instantiate");
            DotNetNativeTestSuite.AssertTrue(viewer.GetCurrentSystem() == null, "Viewer should not expose a current system before setup runs");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestUpdateSeedDisplayUpdatesSeedInput()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            viewer._Ready();
            SpinBox? seedInput = viewer.GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
            DotNetNativeTestSuite.AssertNotNull(seedInput, "Viewer should have a seed input");

            viewer.UpdateSeedDisplay(24680);
            DotNetNativeTestSuite.AssertEqual(24680, (int)seedInput!.Value, "UpdateSeedDisplay should update the visible seed input");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestEscapeKeyRequestsReturnToGalaxy()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            bool requestedReturn = false;
            viewer.Connect(SystemViewer.SignalName.BackToGalaxyRequested, Callable.From(() => requestedReturn = true));

            InputEventKey escapePressed = new()
            {
                Pressed = true,
                Keycode = Key.Escape,
            };

            viewer._UnhandledKeyInput(escapePressed);
            DotNetNativeTestSuite.AssertTrue(requestedReturn, "Escape should request a return to the galaxy viewer");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerTooltipsAreConfigured()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            viewer._Ready();
            Button? saveButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton");
            Button? loadButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton");
            DotNetNativeTestSuite.AssertNotNull(saveButton, "Viewer should have a save button");
            DotNetNativeTestSuite.AssertNotNull(loadButton, "Viewer should have a load button");
            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrEmpty(saveButton!.TooltipText), "Save button tooltip should be configured");
            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrEmpty(loadButton!.TooltipText), "Load button tooltip should be configured");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestProgrammaticSave()
    {
        CleanupTestFiles();
        try
        {
            SystemViewerSaveLoad saveLoad = new();
            MockViewer viewer = new();
            viewer.System = SystemFixtureGenerator.GenerateSystem(new SolarSystemSpec(12345, 1, 1));
            Error error = saveLoad.SaveToPath(viewer, TestBinaryPath, true);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, error, "Programmatic save should succeed");
            DotNetNativeTestSuite.AssertTrue(FileAccess.FileExists(TestBinaryPath), "Save file should exist");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    private static void TestProgrammaticLoad()
    {
        CleanupTestFiles();
        try
        {
            SystemViewerSaveLoad saveLoad = new();
            MockViewer viewer = new();
            viewer.System = SystemFixtureGenerator.GenerateSystem(new SolarSystemSpec(54321, 1, 1));
            saveLoad.SaveToPath(viewer, TestBinaryPath, true);
            SystemPersistenceLoadResult result = saveLoad.LoadFromPath(TestBinaryPath);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Programmatic load should succeed");
            DotNetNativeTestSuite.AssertNotNull(result.System, "Loaded system should be present");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    private static void TestRoundTripPreservesData()
    {
        CleanupTestFiles();
        try
        {
            SystemViewerSaveLoad saveLoad = new();
            MockViewer viewer = new();
            SolarSystem original = SystemFixtureGenerator.GenerateSystem(new SolarSystemSpec(99999, 2, 2));
            viewer.System = original;
            saveLoad.SaveToPath(viewer, TestBinaryPath, true);
            SystemPersistenceLoadResult result = saveLoad.LoadFromPath(TestBinaryPath);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Load should succeed");
            DotNetNativeTestSuite.AssertNotNull(result.System, "Loaded system should be present");
            DotNetNativeTestSuite.AssertEqual(original.Id, result.System!.Id, "System ID should round-trip");
            DotNetNativeTestSuite.AssertEqual(original.StarIds.Count, result.System.StarIds.Count, "Star count should round-trip");
            DotNetNativeTestSuite.AssertEqual(original.PlanetIds.Count, result.System.PlanetIds.Count, "Planet count should round-trip");
        }
        finally
        {
            CleanupTestFiles();
        }
    }

    private static void TestLoadNonexistentFile()
    {
        SystemViewerSaveLoad saveLoad = new();
        SystemPersistenceLoadResult result = saveLoad.LoadFromPath("user://nonexistent_system.sgs");
        DotNetNativeTestSuite.AssertFalse(result.Success, "Load should fail for missing file");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrEmpty(result.ErrorMessage), "Missing-file load should include an error");
    }
}
