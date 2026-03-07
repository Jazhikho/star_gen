#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxyViewerUI
{
    private const string GalaxyViewerScenePath = "res://src/app/galaxy_viewer/GalaxyViewer.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxyViewerUI::test_viewer_instantiates", TestViewerInstantiates);
        runner.RunNativeTest("TestGalaxyViewerUI::test_has_inspector_panel", TestHasInspectorPanel);
        runner.RunNativeTest("TestGalaxyViewerUI::test_starts_at_galaxy_zoom_level", TestStartsAtGalaxyZoomLevel);
        runner.RunNativeTest("TestGalaxyViewerUI::test_has_spec", TestHasSpec);
        runner.RunNativeTest("TestGalaxyViewerUI::test_status_updates", TestStatusUpdates);
        runner.RunNativeTest("TestGalaxyViewerUI::test_spec_matches_seed", TestSpecMatchesSeed);
        runner.RunNativeTest("TestGalaxyViewerUI::test_calculate_jump_routes_populates_result", TestCalculateJumpRoutesPopulatesResult);
        runner.RunNativeTest("TestGalaxyViewerUI::test_jump_route_visibility_toggle_updates_renderer", TestJumpRouteVisibilityToggleUpdatesRenderer);
        runner.RunNativeTest("TestGalaxyViewerUI::test_jump_route_progress_controls_exist", TestJumpRouteProgressControlsExist);
        runner.RunNativeTest("TestGalaxyViewerUI::test_recalculate_same_subsector_does_not_duplicate_systems", TestRecalculateSameSubsectorDoesNotDuplicateSystems);
        runner.RunNativeTest("TestGalaxyViewerUI::test_subsector_change_keeps_routes_and_recalculate_expands_region", TestSubsectorChangeKeepsRoutesAndRecalculateExpandsRegion);
        runner.RunNativeTest("TestGalaxyViewerUI::test_apply_valid_config_updates_spec", TestApplyValidConfigUpdatesSpec);
        runner.RunNativeTest("TestGalaxyViewerUI::test_invalid_config_is_blocked", TestInvalidConfigIsBlocked);
    }

    private static GalaxyViewer CreateViewer(bool startAtHome = true)
    {
        GalaxyViewer viewer = IntegrationTestUtils.InstantiateScene<GalaxyViewer>(GalaxyViewerScenePath);
        viewer.StartAtHome = startAtHome;
        viewer._Ready();

        GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
        if (panel != null)
        {
            panel._Ready();
        }

        return viewer;
    }

    private static void TestViewerInstantiates()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer, "Viewer should instantiate");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasInspectorPanel()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer.get_inspector_panel(), "Inspector panel should exist");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStartsAtGalaxyZoomLevel()
    {
        GalaxyViewer viewer = CreateViewer(startAtHome: false);
        try
        {
            DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.Galaxy, viewer.get_zoom_level(), "When home start disabled, viewer should start at galaxy");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasSpec()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxySpec? spec = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(spec, "Viewer should expose spec");
            DotNetNativeTestSuite.AssertEqual(42, spec!.GalaxySeed, "Default seed should be 42");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStatusUpdates()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            Label? statusLabel = viewer.GetNodeOrNull<Label>("UI/UIRoot/TopBar/MarginContainer/HBoxContainer/StatusLabel");
            DotNetNativeTestSuite.AssertNotNull(statusLabel, "Status label should exist");

            viewer.set_status("Test status message");
            DotNetNativeTestSuite.AssertEqual("Test status message", statusLabel!.Text, "Status updates should change the visible label text");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestSpecMatchesSeed()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxySpec? spec = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(spec, "Viewer should expose spec");
            DotNetNativeTestSuite.AssertEqual(GalaxySpec.GalaxyType.Spiral, spec!.Type, "Default type should be spiral");
            DotNetNativeTestSuite.AssertEqual(4, spec.NumArms, "Default arm count should be 4");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCalculateJumpRoutesPopulatesResult()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            SectorJumpLaneRenderer? renderer = viewer.GetNodeOrNull<SectorJumpLaneRenderer>("SectorJumpLaneRenderer");
            DotNetNativeTestSuite.AssertNotNull(renderer, "Jump-lane renderer should exist");

            viewer.CalculateJumpRoutesForCurrentSubsector();

            DotNetNativeTestSuite.AssertNotNull(viewer.GetJumpLaneRegion(), "Calculation should cache the current jump-lane region");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetJumpLaneResult(), "Calculation should cache the jump-lane result");
            DotNetNativeTestSuite.AssertGreaterThan(
                viewer.GetJumpLaneRegion()!.GetSystemCount(),
                0,
                "Calculated regions should include neighborhood stars");
            DotNetNativeTestSuite.AssertEqual(
                viewer.GetJumpLaneRegion()!.GetSystemCount(),
                viewer.GetJumpLaneResult()!.Systems.Count,
                "Jump-lane results should register every system in the region");
            DotNetNativeTestSuite.AssertTrue(
                renderer!.Visible,
                "Renderer should be visible after calculating routes while the toggle is enabled");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestJumpRouteVisibilityToggleUpdatesRenderer()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            SectorJumpLaneRenderer? renderer = viewer.GetNodeOrNull<SectorJumpLaneRenderer>("SectorJumpLaneRenderer");
            DotNetNativeTestSuite.AssertNotNull(renderer, "Jump-lane renderer should exist");
            CheckBox? showRoutesCheck = panel.GetNodeOrNull<CheckBox>("ShowRoutesCheck");
            DotNetNativeTestSuite.AssertNotNull(showRoutesCheck, "Show-routes checkbox should exist");

            viewer.CalculateJumpRoutesForCurrentSubsector();
            DotNetNativeTestSuite.AssertTrue(renderer!.Visible, "Renderer should start visible after calculation");

            showRoutesCheck!.ButtonPressed = false;
            panel.EmitSignal(GalaxyInspectorPanel.SignalName.JumpRoutesVisibilityToggled, false);
            DotNetNativeTestSuite.AssertFalse(renderer.Visible, "Renderer should hide when jump routes are toggled off");

            showRoutesCheck.ButtonPressed = true;
            panel.EmitSignal(GalaxyInspectorPanel.SignalName.JumpRoutesVisibilityToggled, true);
            DotNetNativeTestSuite.AssertTrue(renderer.Visible, "Renderer should show again when jump routes are toggled on");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestJumpRouteProgressControlsExist()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            Label? progressLabel = panel!.GetNodeOrNull<Label>("JumpRoutesProgressLabel");
            ProgressBar? progressBar = panel.GetNodeOrNull<ProgressBar>("JumpRoutesProgressBar");
            DotNetNativeTestSuite.AssertNotNull(progressLabel, "Jump-route progress label should exist");
            DotNetNativeTestSuite.AssertNotNull(progressBar, "Jump-route progress bar should exist");
            DotNetNativeTestSuite.AssertFalse(progressLabel!.Visible, "Progress label should start hidden");
            DotNetNativeTestSuite.AssertFalse(progressBar!.Visible, "Progress bar should start hidden");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestRecalculateSameSubsectorDoesNotDuplicateSystems()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            viewer.CalculateJumpRoutesForCurrentSubsector();
            int firstSystemCount = viewer.GetJumpLaneRegion()!.GetSystemCount();

            viewer.CalculateJumpRoutesForCurrentSubsector();
            int secondSystemCount = viewer.GetJumpLaneRegion()!.GetSystemCount();

            DotNetNativeTestSuite.AssertEqual(
                firstSystemCount,
                secondSystemCount,
                "Recalculating without moving should not duplicate cached jump-route systems");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestSubsectorChangeKeepsRoutesAndRecalculateExpandsRegion()
    {
        GalaxyViewer viewer = CreateViewer();
        try
        {
            viewer.CalculateJumpRoutesForCurrentSubsector();
            JumpLaneRegion? initialRegion = viewer.GetJumpLaneRegion();
            JumpLaneResult? initialResult = viewer.GetJumpLaneResult();
            DotNetNativeTestSuite.AssertNotNull(initialRegion, "Initial jump-route region should exist");
            DotNetNativeTestSuite.AssertNotNull(initialResult, "Initial jump-route result should exist");

            int initialSystemCount = initialRegion!.GetSystemCount();
            StarViewCamera? starCamera = viewer.GetStarCamera();
            DotNetNativeTestSuite.AssertNotNull(starCamera, "Star camera should exist");

            Vector3 shiftedPosition = starCamera!.GetCurrentPosition() + new Vector3((float)GalaxyCoordinates.SubsectorSizePc, 0.0f, 0.0f);
            starCamera.Configure(shiftedPosition);
            starCamera.EmitSignal(StarViewCamera.SignalName.SubsectorChanged, GalaxyCoordinates.GetSubsectorWorldOrigin(shiftedPosition));

            DotNetNativeTestSuite.AssertNotNull(viewer.GetJumpLaneResult(), "Routes should remain cached after moving to a new subsector");
            DotNetNativeTestSuite.AssertEqual(
                initialSystemCount,
                viewer.GetJumpLaneRegion()!.GetSystemCount(),
                "Moving alone should not discard or duplicate the accumulated jump-route region");

            viewer.CalculateJumpRoutesForCurrentSubsector();
            int expandedSystemCount = viewer.GetJumpLaneRegion()!.GetSystemCount();

            DotNetNativeTestSuite.AssertTrue(
                expandedSystemCount >= initialSystemCount,
                "Recalculating after moving should keep prior systems and add any newly visible ones");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestApplyValidConfigUpdatesSpec()
    {
        GalaxyViewer viewer = CreateViewer(startAtHome: false);
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            SpinBox? seedInput = viewer.GetNodeOrNull<SpinBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
            DotNetNativeTestSuite.AssertNotNull(seedInput, "Galaxy viewer should expose the seed input");
            seedInput!.Value = 424242;

            GalaxyConfig updatedConfig = GalaxyConfig.CreateDefault();
            updatedConfig.Type = GalaxySpec.GalaxyType.Elliptical;
            updatedConfig.BulgeIntensity = 1.1;
            updatedConfig.Ellipticity = 0.55;
            updatedConfig.RadiusPc = 18000.0;
            panel!.SetEditableConfig(updatedConfig);
            panel.EmitSignal(GalaxyInspectorPanel.SignalName.ApplyGalaxyConfigRequested);

            GalaxySpec? spec = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(spec, "Viewer should expose an updated spec");
            DotNetNativeTestSuite.AssertEqual(GalaxySpec.GalaxyType.Elliptical, spec!.Type, "Applying a valid config should update galaxy type");
            DotNetNativeTestSuite.AssertEqual(updatedConfig.RadiusPc, spec.RadiusPc, "Applying a valid config should update radius");
            DotNetNativeTestSuite.AssertEqual(424242, viewer.galaxy_seed, "Applying a valid config should update the seed");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInvalidConfigIsBlocked()
    {
        GalaxyViewer viewer = CreateViewer(startAtHome: false);
        try
        {
            GalaxyInspectorPanel? panel = viewer.get_inspector_panel() as GalaxyInspectorPanel;
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");
            SpinBox? seedInput = viewer.GetNodeOrNull<SpinBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
            DotNetNativeTestSuite.AssertNotNull(seedInput, "Galaxy viewer should expose the seed input");

            GalaxySpec? originalSpec = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(originalSpec, "Viewer should expose an initial spec");
            double originalRadius = originalSpec!.RadiusPc;

            seedInput!.Value = 0.0;
            GalaxyConfig invalidConfig = GalaxyConfig.CreateDefault();
            panel!.SetEditableConfig(invalidConfig);
            panel.EmitSignal(GalaxyInspectorPanel.SignalName.ApplyGalaxyConfigRequested);

            GalaxySpec? currentSpec = viewer.get_spec();
            DotNetNativeTestSuite.AssertNotNull(currentSpec, "Viewer should still expose a spec after invalid apply");
            DotNetNativeTestSuite.AssertEqual(originalRadius, currentSpec!.RadiusPc, "Invalid config should not replace the active spec");

            VBoxContainer? issuesContainer = panel.GetNodeOrNull<VBoxContainer>("ConfigEditorContainer/ConfigIssuesContainer");
            DotNetNativeTestSuite.AssertNotNull(issuesContainer, "Galaxy inspector should expose the config issue container");
            DotNetNativeTestSuite.AssertTrue(issuesContainer!.GetChildCount() > 0, "Invalid config should render issues");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }
}
