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
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_three_panel_layout_stacks_at_compact_widths", TestThreePanelLayoutStacksAtCompactWidths);
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
            DotNetNativeTestSuite.AssertEqual(0.0f, settingsPanel.CustomMinimumSize.X, "Compact layout should let the settings panel fill the available width");
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

    private static void TestThreePanelLayoutStacksAtCompactWidths()
    {
        SubViewport viewport = new();
        Control owner = new();
        BoxContainer studioRow = new();
        PanelContainer settingsPanel = new();
        PanelContainer rulesPanel = new();
        PanelContainer summaryPanel = new();

        try
        {
            viewport.Size = new Vector2I(960, 720);
            viewport.AddChild(owner);
            owner.Size = new Vector2(960.0f, 720.0f);
            owner.AddChild(studioRow);
            studioRow.AddChild(settingsPanel);
            studioRow.AddChild(rulesPanel);
            studioRow.AddChild(summaryPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, rulesPanel, summaryPanel);

            DotNetNativeTestSuite.AssertTrue(studioRow.Vertical, "Three-panel studio layout should stack at compact widths");
            DotNetNativeTestSuite.AssertEqual(0.0f, settingsPanel.CustomMinimumSize.X, "Compact stacked layout should clear fixed widths");
            DotNetNativeTestSuite.AssertEqual(0.0f, rulesPanel.CustomMinimumSize.X, "Compact stacked layout should clear fixed widths");
            DotNetNativeTestSuite.AssertEqual(0.0f, summaryPanel.CustomMinimumSize.X, "Compact stacked layout should clear fixed widths");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }
}
