#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestStarSystemPreviewIntegration
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestStarSystemPreviewIntegration::test_no_preview_before_star_click", TestNoPreviewBeforeStarClick);
        runner.RunNativeTest("TestStarSystemPreviewIntegration::test_preview_generated_after_simulate_star_select", TestPreviewGeneratedAfterSimulateStarSelect);
        runner.RunNativeTest("TestStarSystemPreviewIntegration::test_preview_cleared_on_empty_click", TestPreviewClearedOnEmptyClick);
        runner.RunNativeTest("TestStarSystemPreviewIntegration::test_open_system_requested_emits_with_correct_seed", TestOpenSystemRequestedEmitsWithCorrectSeed);
    }

    private static MainApp CreateStartedApp()
    {
        return IntegrationTestUtils.CreateMainAppReadyAndStarted();
    }

    private static GalaxyViewer GetViewer(MainApp app)
    {
        GalaxyViewer? viewer = app.get_galaxy_viewer();
        DotNetNativeTestSuite.AssertNotNull(viewer, "Galaxy viewer must exist");
        return viewer!;
    }

    private static void TestNoPreviewBeforeStarClick()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer viewer = GetViewer(app);
            DotNetNativeTestSuite.AssertNull(viewer.get_star_preview(), "Preview should not exist before selection");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestPreviewGeneratedAfterSimulateStarSelect()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer viewer = GetViewer(app);
            DotNetNativeTestSuite.AssertNotNull(viewer.get_zoom_machine(), "Zoom machine should be ready");
            DotNetNativeTestSuite.AssertNotNull(viewer.get_spec(), "Galaxy spec should be ready");

            int fakeSeed = 99991;
            Vector3 fakePos = new(8000.0f, 0.0f, 0.0f);
            viewer.simulate_star_selected(fakeSeed, fakePos);

            StarSystemPreviewData? preview = viewer.get_star_preview();
            DotNetNativeTestSuite.AssertNotNull(preview, "Preview should exist after selection");
            DotNetNativeTestSuite.AssertEqual(fakeSeed, preview!.StarSeed, "Preview seed should match");
            DotNetNativeTestSuite.AssertTrue(preview.StarCount >= 1, "Preview should include at least one star");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestPreviewClearedOnEmptyClick()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer viewer = GetViewer(app);
            viewer.simulate_star_selected(99992, new Vector3(8000.0f, 0.0f, 0.0f));
            DotNetNativeTestSuite.AssertNotNull(viewer.get_star_preview(), "Preview should exist after select");
            viewer.simulate_star_deselected();
            DotNetNativeTestSuite.AssertNull(viewer.get_star_preview(), "Preview should clear after deselect");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestOpenSystemRequestedEmitsWithCorrectSeed()
    {
        MainApp app = CreateStartedApp();
        try
        {
            GalaxyViewer viewer = GetViewer(app);
            int fakeSeed = 99993;
            Vector3 fakePos = new(8000.0f, 0.0f, 0.0f);
            viewer.simulate_star_selected(fakeSeed, fakePos);

            int receivedSeed = 0;
            Vector3 receivedPos = Vector3.Zero;
            viewer.OpenSystemRequested += (seed, position) =>
            {
                receivedSeed = seed;
                receivedPos = position;
            };

            ZoomStateMachine? zoomMachine = viewer.get_zoom_machine();
            DotNetNativeTestSuite.AssertNotNull(zoomMachine, "Zoom machine should be initialized");
            zoomMachine!.SetLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
            viewer.simulate_open_selected_system();

            DotNetNativeTestSuite.AssertEqual(fakeSeed, receivedSeed, "Open request seed should match");
            DotNetNativeTestSuite.AssertEqual(fakePos, receivedPos, "Open request position should match");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }
}
