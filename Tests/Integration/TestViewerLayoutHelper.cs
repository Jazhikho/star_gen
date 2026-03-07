#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.Shared;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestViewerLayoutHelper
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestViewerLayoutHelper::test_compute_render_rect_excludes_panel_and_topbar", TestComputeRenderRectExcludesPanelAndTopbar);
        runner.RunNativeTest("TestViewerLayoutHelper::test_compute_center_offset_biases_visible_center", TestComputeCenterOffsetBiasesVisibleCenter);
    }

    private static void TestComputeRenderRectExcludesPanelAndTopbar()
    {
        SubViewport viewport = new();
        Control root = new();
        Panel topBar = new();
        Panel sidePanel = new();

        try
        {
            viewport.Size = new Vector2I(1200, 800);
            viewport.AddChild(root);

            topBar.Size = new Vector2(1200.0f, 60.0f);
            root.AddChild(topBar);

            sidePanel.Position = new Vector2(0.0f, 60.0f);
            sidePanel.Size = new Vector2(350.0f, 740.0f);
            root.AddChild(sidePanel);

            Rect2 renderRect = ViewerLayoutHelper.ComputeRenderRect(viewport, topBar, sidePanel);

            DotNetNativeTestSuite.AssertFloatNear(350.0, renderRect.Position.X, 0.001, "Render rect should start after the side panel");
            DotNetNativeTestSuite.AssertFloatNear(60.0, renderRect.Position.Y, 0.001, "Render rect should start below the top bar");
            DotNetNativeTestSuite.AssertFloatNear(850.0, renderRect.Size.X, 0.001, "Render rect width should exclude the side panel");
            DotNetNativeTestSuite.AssertFloatNear(740.0, renderRect.Size.Y, 0.001, "Render rect height should exclude the top bar");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }

    private static void TestComputeCenterOffsetBiasesVisibleCenter()
    {
        Rect2 renderRect = new(new Vector2(350.0f, 60.0f), new Vector2(850.0f, 740.0f));
        SubViewport viewport = new();

        try
        {
            viewport.Size = new Vector2I(1200, 800);
            Vector2 offset = ViewerLayoutHelper.ComputeNormalizedCenterOffset(viewport, renderRect);

            DotNetNativeTestSuite.AssertTrue(offset.X > 0.0f, "The visible center should shift right when a left inspector is present");
            DotNetNativeTestSuite.AssertTrue(offset.Y > 0.0f, "The visible center should shift down when a top bar is present");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewport);
        }
    }
}
