#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxyViewerHome
{
    private const string GalaxyViewerScenePath = "res://src/app/galaxy_viewer/GalaxyViewer.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxyViewerHome::test_viewer_starts_at_home_by_default", TestViewerStartsAtHomeByDefault);
        runner.RunNativeTest("TestGalaxyViewerHome::test_starts_at_subsector_zoom_level", TestStartsAtSubsectorZoomLevel);
        runner.RunNativeTest("TestGalaxyViewerHome::test_starts_at_galaxy_when_disabled", TestStartsAtGalaxyWhenDisabled);
        runner.RunNativeTest("TestGalaxyViewerHome::test_navigate_to_home_works_from_galaxy_view", TestNavigateToHomeWorksFromGalaxyView);
        runner.RunNativeTest("TestGalaxyViewerHome::test_home_position_is_in_expected_quadrant", TestHomePositionIsInExpectedQuadrant);
        runner.RunNativeTest("TestGalaxyViewerHome::test_inspector_shows_info_after_home_init", TestInspectorShowsInfoAfterHomeInit);
        runner.RunNativeTest("TestGalaxyViewerHome::test_can_zoom_out_from_home", TestCanZoomOutFromHome);
        runner.RunNativeTest("TestGalaxyViewerHome::test_can_zoom_out_to_galaxy_view", TestCanZoomOutToGalaxyView);
        runner.RunNativeTest("TestGalaxyViewerHome::test_can_return_to_home_after_zooming_out", TestCanReturnToHomeAfterZoomingOut);
    }

    private static GalaxyViewer CreateViewer(bool startAtHome = true)
    {
        GalaxyViewer viewer = IntegrationTestUtils.InstantiateScene<GalaxyViewer>(GalaxyViewerScenePath);
        viewer.StartAtHome = startAtHome;
        viewer._Ready();
        return viewer;
    }

    private static void TestViewerStartsAtHomeByDefault()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertTrue(viewer.get_start_at_home(), "Viewer should start at home by default");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStartsAtSubsectorZoomLevel()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, viewer.get_zoom_level(), "Should start at subsector level");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStartsAtGalaxyWhenDisabled()
    {
        GalaxyViewer viewer = CreateViewer(startAtHome: false);
        try
        {
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, viewer.get_zoom_level(), "Should start at galaxy level");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestNavigateToHomeWorksFromGalaxyView()
    {
        GalaxyViewer viewer = CreateViewer(startAtHome: false);
        try
        {
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, viewer.get_zoom_level(), "Should start at galaxy");
            viewer.navigate_to_home();
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, viewer.get_zoom_level(), "Should navigate to home subsector");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHomePositionIsInExpectedQuadrant()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, viewer.get_zoom_level(), "Home init should enter subsector");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInspectorShowsInfoAfterHomeInit()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");
            DotNetNativeTestSuite.AssertFalse(panel!.HasStarSelected(), "No star should be selected initially");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCanZoomOutFromHome()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            InputEventKey zoomOut = new()
            {
                Keycode = Key.Bracketleft,
                Pressed = true,
            };
            viewer._handle_key_input(zoomOut);
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Sector, viewer.get_zoom_level(), "Should zoom out to sector");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCanZoomOutToGalaxyView()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            InputEventKey zoomOut = new()
            {
                Keycode = Key.Bracketleft,
                Pressed = true,
            };
            viewer._handle_key_input(zoomOut);
            viewer._handle_key_input(zoomOut);
            viewer._handle_key_input(zoomOut);
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, viewer.get_zoom_level(), "Should zoom out to galaxy");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCanReturnToHomeAfterZoomingOut()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            InputEventKey zoomOut = new()
            {
                Keycode = Key.Bracketleft,
                Pressed = true,
            };
            viewer._handle_key_input(zoomOut);
            viewer._handle_key_input(zoomOut);
            viewer._handle_key_input(zoomOut);
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, viewer.get_zoom_level(), "Should reach galaxy level");
            viewer.navigate_to_home();
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Subsector, viewer.get_zoom_level(), "Should return to home");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }
}
