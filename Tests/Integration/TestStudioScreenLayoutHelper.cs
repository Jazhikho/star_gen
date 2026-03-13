#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.Shared;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestStudioScreenLayoutHelper
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_compact_layout_stacks_without_summary_panel", TestCompactLayoutStacksWithoutSummaryPanel);
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_wide_layout_keeps_settings_panel_expand_fill_without_summary_panel", TestWideLayoutKeepsSettingsPanelExpandFillWithoutSummaryPanel);
    }

    private static void TestCompactLayoutStacksWithoutSummaryPanel()
    {
        SubViewport viewport = new();
        Control owner = new();
        BoxContainer studioRow = new();
        PanelContainer settingsPanel = new();

        try
        {
            viewport.Size = new Vector2I(960, 720);
            viewport.AddChild(owner);
            owner.Size = new Vector2(960.0f, 720.0f);
            owner.AddChild(studioRow);
            studioRow.AddChild(settingsPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, null);

            DotNetNativeTestSuite.AssertTrue(studioRow.Vertical, "Compact studio layout should stack vertically");
            DotNetNativeTestSuite.AssertEqual(0.0f, settingsPanel.CustomMinimumSize.Y, "Compact layout should not force extra vertical height on the settings panel");
            DotNetNativeTestSuite.AssertEqual((int)Control.SizeFlags.ExpandFill, (int)settingsPanel.SizeFlagsVertical, "Compact layout should keep the settings panel vertically expandable");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }

    private static void TestWideLayoutKeepsSettingsPanelExpandFillWithoutSummaryPanel()
    {
        SubViewport viewport = new();
        Control owner = new();
        BoxContainer studioRow = new();
        PanelContainer settingsPanel = new();

        try
        {
            viewport.Size = new Vector2I(1440, 900);
            viewport.AddChild(owner);
            owner.Size = new Vector2(1440.0f, 900.0f);
            owner.AddChild(studioRow);
            studioRow.AddChild(settingsPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, null);

            DotNetNativeTestSuite.AssertFalse(studioRow.Vertical, "Wide studio layout should stay horizontal");
            DotNetNativeTestSuite.AssertEqual((int)Control.SizeFlags.ExpandFill, (int)settingsPanel.SizeFlagsHorizontal, "Wide layout should keep the settings panel expandable");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }
}
