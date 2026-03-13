#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestWindowSettingsService
{
    private const string WindowSettingsPath = "user://window_settings.cfg";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestWindowSettingsService::test_save_and_load_round_trip", TestSaveAndLoadRoundTrip);
        runner.RunNativeTest("TestWindowSettingsService::test_parse_resolution_key", TestParseResolutionKey);
        runner.RunNativeTest("TestWindowSettingsService::test_apply_to_window_updates_windowed_size", TestApplyToWindowUpdatesWindowedSize);
        runner.RunNativeTest("TestWindowSettingsService::test_apply_to_window_updates_fullscreen_mode", TestApplyToWindowUpdatesFullscreenMode);
    }

    private static void TestSaveAndLoadRoundTrip()
    {
        string absolutePath = ProjectSettings.GlobalizePath(WindowSettingsPath);
        Cleanup(absolutePath);

        try
        {
            WindowSettingsService.WindowSettingsState expected =
                new WindowSettingsService.WindowSettingsState(false, new Vector2I(1920, 1080));
            WindowSettingsService.Save(expected);
            WindowSettingsService.WindowSettingsState loaded = WindowSettingsService.LoadOrCaptureCurrent();

            DotNetNativeTestSuite.AssertFalse(loaded.Fullscreen, "Saved settings should preserve fullscreen mode");
            DotNetNativeTestSuite.AssertEqual(expected.Resolution, loaded.Resolution, "Saved settings should preserve resolution");
        }
        finally
        {
            Cleanup(absolutePath);
        }
    }

    private static void TestParseResolutionKey()
    {
        bool parsed = WindowSettingsService.TryParseResolutionKey("2560x1440", out Vector2I resolution);

        DotNetNativeTestSuite.AssertTrue(parsed, "Resolution keys should parse");
        DotNetNativeTestSuite.AssertEqual(new Vector2I(2560, 1440), resolution, "Parsed resolution should match the key");
    }

    private static void TestApplyToWindowUpdatesWindowedSize()
    {
        Window window = new Window();
        try
        {
            WindowSettingsService.ApplyToWindow(
                window,
                new WindowSettingsService.WindowSettingsState(false, new Vector2I(1920, 1080)));

            DotNetNativeTestSuite.AssertEqual(Window.ModeEnum.Windowed, window.Mode, "Windowed settings should set windowed mode");
            DotNetNativeTestSuite.AssertEqual(new Vector2I(1920, 1080), window.Size, "Windowed settings should update the target size");
        }
        finally
        {
            window.Free();
        }
    }

    private static void TestApplyToWindowUpdatesFullscreenMode()
    {
        Window window = new Window();
        try
        {
            WindowSettingsService.ApplyToWindow(
                window,
                new WindowSettingsService.WindowSettingsState(true, new Vector2I(1600, 900)));

            DotNetNativeTestSuite.AssertEqual(Window.ModeEnum.Fullscreen, window.Mode, "Fullscreen settings should set fullscreen mode");
        }
        finally
        {
            window.Free();
        }
    }

    private static void Cleanup(string absolutePath)
    {
        if (FileAccess.FileExists(absolutePath))
        {
            DirAccess.RemoveAbsolute(absolutePath);
        }
    }
}
