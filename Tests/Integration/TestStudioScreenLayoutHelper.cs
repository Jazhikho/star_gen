#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
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
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_galaxy_scene_uses_scene_owned_two_hundred_pixel_panel_minimums", TestGalaxySceneUsesSceneOwnedTwoHundredPixelPanelMinimums);
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_system_scene_uses_scene_owned_two_hundred_pixel_panel_minimums", TestSystemSceneUsesSceneOwnedTwoHundredPixelPanelMinimums);
        runner.RunNativeTest("TestStudioScreenLayoutHelper::test_object_scene_uses_scene_owned_two_hundred_pixel_panel_minimums", TestObjectSceneUsesSceneOwnedTwoHundredPixelPanelMinimums);
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
            settingsPanel.CustomMinimumSize = new Vector2(200.0f, 0.0f);
            studioRow.AddChild(settingsPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, null);

            DotNetNativeTestSuite.AssertTrue(studioRow.Vertical, "Compact studio layout should stack vertically");
            DotNetNativeTestSuite.AssertEqual(200.0f, settingsPanel.CustomMinimumSize.X, "Compact layout should preserve the scene-owned settings width");
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
            settingsPanel.CustomMinimumSize = new Vector2(200.0f, 0.0f);
            studioRow.AddChild(settingsPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, null);

            DotNetNativeTestSuite.AssertFalse(studioRow.Vertical, "Wide studio layout should stay horizontal");
            DotNetNativeTestSuite.AssertEqual(200.0f, settingsPanel.CustomMinimumSize.X, "Wide layout should preserve the scene-owned settings width");
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
            settingsPanel.CustomMinimumSize = new Vector2(200.0f, 0.0f);
            rulesPanel.CustomMinimumSize = new Vector2(200.0f, 0.0f);
            summaryPanel.CustomMinimumSize = new Vector2(200.0f, 0.0f);
            studioRow.AddChild(settingsPanel);
            studioRow.AddChild(rulesPanel);
            studioRow.AddChild(summaryPanel);

            StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(owner, studioRow, settingsPanel, rulesPanel, summaryPanel);

            DotNetNativeTestSuite.AssertTrue(studioRow.Vertical, "Three-panel studio layout should stack at compact widths");
            DotNetNativeTestSuite.AssertEqual(200.0f, settingsPanel.CustomMinimumSize.X, "Compact stacked layout should preserve the scene-owned settings width");
            DotNetNativeTestSuite.AssertEqual(200.0f, rulesPanel.CustomMinimumSize.X, "Compact stacked layout should preserve the scene-owned rules width");
            DotNetNativeTestSuite.AssertEqual(200.0f, summaryPanel.CustomMinimumSize.X, "Compact stacked layout should preserve the scene-owned summary width");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }

    private static void TestGalaxySceneUsesSceneOwnedTwoHundredPixelPanelMinimums()
    {
        GalaxyGenerationScreen screen = IntegrationTestUtils.InstantiateScene<GalaxyGenerationScreen>("res://src/app/GalaxyGenerationScreen.tscn");
        screen._Ready();

        try
        {
            AssertScenePanelMinimumWidths(screen, false);
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestSystemSceneUsesSceneOwnedTwoHundredPixelPanelMinimums()
    {
        SystemGenerationScreen screen = IntegrationTestUtils.InstantiateScene<SystemGenerationScreen>("res://src/app/SystemGenerationScreen.tscn");
        screen._Ready();

        try
        {
            AssertScenePanelMinimumWidths(screen, true);
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestObjectSceneUsesSceneOwnedTwoHundredPixelPanelMinimums()
    {
        ObjectGenerationScreen screen = IntegrationTestUtils.InstantiateScene<ObjectGenerationScreen>("res://src/app/ObjectGenerationScreen.tscn");
        screen._Ready();

        try
        {
            AssertScenePanelMinimumWidths(screen, true);
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void AssertScenePanelMinimumWidths(Control screen, bool expectRulesLegend)
    {
        Control? settingsPanel = screen.FindChild("SettingsPanel", true, false) as Control;
        Control? rulesPanel = screen.FindChild("RulesPanel", true, false) as Control;
        Control? summaryPanel = screen.FindChild("SummaryPanel", true, false) as Control;
        Label? summaryLabel = screen.FindChild("SummaryLabel", true, false) as Label;
        Label? assumptionsLabel = screen.FindChild("AssumptionsLabel", true, false) as Label;
        Label? permissivenessLegend = screen.FindChild("PermissivenessLegend", true, false) as Label;

        DotNetNativeTestSuite.AssertNotNull(settingsPanel, "Studio scene should expose the settings panel");
        DotNetNativeTestSuite.AssertNotNull(rulesPanel, "Studio scene should expose the rules panel");
        DotNetNativeTestSuite.AssertNotNull(summaryPanel, "Studio scene should expose the summary panel");
        DotNetNativeTestSuite.AssertNotNull(summaryLabel, "Studio scene should expose the summary label");
        DotNetNativeTestSuite.AssertNotNull(assumptionsLabel, "Studio scene should expose the assumptions label");
        DotNetNativeTestSuite.AssertEqual(200.0f, settingsPanel!.CustomMinimumSize.X, "Settings panel width should stay scene-owned at 200 px");
        DotNetNativeTestSuite.AssertEqual(200.0f, rulesPanel!.CustomMinimumSize.X, "Rules panel width should stay scene-owned at 200 px");
        DotNetNativeTestSuite.AssertEqual(200.0f, summaryPanel!.CustomMinimumSize.X, "Summary panel width should stay scene-owned at 200 px");
        DotNetNativeTestSuite.AssertEqual(0.0f, summaryLabel!.CustomMinimumSize.X, "Summary label should not force a wider panel than the scene allows");
        DotNetNativeTestSuite.AssertEqual(0.0f, assumptionsLabel!.CustomMinimumSize.X, "Assumptions label should not force a wider panel than the scene allows");

        if (expectRulesLegend)
        {
            DotNetNativeTestSuite.AssertNotNull(permissivenessLegend, "Studio scene should expose the permissiveness legend");
            DotNetNativeTestSuite.AssertEqual(0.0f, permissivenessLegend!.CustomMinimumSize.X, "Rules legend should not force a wider panel than the scene allows");
        }
    }
}
