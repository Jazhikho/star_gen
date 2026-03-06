#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.Rendering;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestObjectViewer
{
    private const string ObjectViewerScenePath = "res://src/app/viewer/ObjectViewer.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestObjectViewer::test_viewer_scene_instantiates", TestViewerSceneInstantiates);
        runner.RunNativeTest("TestObjectViewer::test_viewer_runs_one_frame", TestViewerRunsOneFrame);
        runner.RunNativeTest("TestObjectViewer::test_viewer_displays_body", TestViewerDisplaysBody);
        runner.RunNativeTest("TestObjectViewer::test_viewer_handles_null_body", TestViewerHandlesNullBody);
        runner.RunNativeTest("TestObjectViewer::test_camera_controller_exists", TestCameraControllerExists);
        runner.RunNativeTest("TestObjectViewer::test_ui_elements_exist", TestUiElementsExist);
        runner.RunNativeTest("TestObjectViewer::test_status_messages", TestStatusMessages);
        runner.RunNativeTest("TestObjectViewer::test_generate_button_creates_objects", TestGenerateButtonCreatesObjects);
        runner.RunNativeTest("TestObjectViewer::test_object_scaling", TestObjectScaling);
        runner.RunNativeTest("TestObjectViewer::test_info_labels_update", TestInfoLabelsUpdate);
        runner.RunNativeTest("TestObjectViewer::test_inspector_shows_all_sections", TestInspectorShowsAllSections);
        runner.RunNativeTest("TestObjectViewer::test_collapsible_sections", TestCollapsibleSections);
        runner.RunNativeTest("TestObjectViewer::test_deterministic_generation", TestDeterministicGeneration);
        runner.RunNativeTest("TestObjectViewer::test_viewer_save_and_load_body", TestViewerSaveAndLoadBody);
    }

    private static ObjectViewer CreateViewer()
    {
        ObjectViewer viewer = IntegrationTestUtils.InstantiateScene<ObjectViewer>(ObjectViewerScenePath);
        viewer._Ready();
        InspectorPanel? inspectorPanel = viewer.inspector_panel as InspectorPanel;
        if (inspectorPanel != null)
        {
            inspectorPanel._Ready();
        }

        BodyRenderer? bodyRenderer = viewer.GetNodeOrNull<BodyRenderer>("BodyRenderer");
        if (bodyRenderer != null)
        {
            bodyRenderer._Ready();
        }

        return viewer;
    }

    private static void CleanupSaveFile(string path)
    {
        if (FileAccess.FileExists(path))
        {
            DirAccess.RemoveAbsolute(path);
        }
    }

    private static void TestViewerSceneInstantiates()
    {
        ObjectViewer viewer = IntegrationTestUtils.InstantiateScene<ObjectViewer>(ObjectViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer, "ObjectViewer should instantiate");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerRunsOneFrame()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer._Process(1.0 / 60.0);
            DotNetNativeTestSuite.AssertTrue(viewer.is_ready, "Viewer should be ready after _Ready");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerDisplaysBody()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            CelestialBody body = IntegrationTestUtils.CreateTestBody(type: CelestialType.Type.Planet);
            viewer.display_body(body);
            DotNetNativeTestSuite.AssertEqual(body, viewer.current_body, "Displayed body should become current body");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerHandlesNullBody()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.display_body(default);
            DotNetNativeTestSuite.AssertNull(viewer.current_body, "Null body should not set current body");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCameraControllerExists()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer.camera, "Viewer should have camera");
            viewer.camera!.FocusOnTarget();
            float distance = viewer.camera.GetDistance();
            DotNetNativeTestSuite.AssertTrue(distance >= 1.0f && distance <= 100.0f, $"Camera distance should be sensible: {distance}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestUiElementsExist()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer.status_label, "Status label should exist");
            DotNetNativeTestSuite.AssertNotNull(viewer.side_panel, "Side panel should exist");
            DotNetNativeTestSuite.AssertNotNull(viewer.panel_container, "Panel container should exist");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStatusMessages()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.set_status("Test status");
            DotNetNativeTestSuite.AssertEqual("Test status", viewer.status_label!.Text, "Status text should update");
            viewer.set_error("Test error", true);
            DotNetNativeTestSuite.AssertTrue(viewer.status_label.Text.Contains("Error"), "Error status should include prefix");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestGenerateButtonCreatesObjects()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Star, 12345);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Star, viewer.current_body!.Type, "Should generate star");

            viewer.generate_object(ObjectViewer.ObjectType.Planet, 23456);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Planet, viewer.current_body!.Type, "Should generate planet");

            viewer.generate_object(ObjectViewer.ObjectType.Moon, 34567);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Moon, viewer.current_body!.Type, "Should generate moon");

            viewer.generate_object(ObjectViewer.ObjectType.Asteroid, 45678);
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Asteroid, viewer.current_body!.Type, "Should generate asteroid");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestObjectScaling()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            MeshInstance3D? bodyMesh = viewer.GetNodeOrNull<MeshInstance3D>("BodyRenderer/BodyMesh");
            DotNetNativeTestSuite.AssertNotNull(bodyMesh, "Body mesh should exist");

            viewer.generate_object(ObjectViewer.ObjectType.Star, 11111);
            Vector3 starScale = bodyMesh!.Scale;
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 22222);
            Vector3 planetScale = bodyMesh.Scale;
            viewer.generate_object(ObjectViewer.ObjectType.Asteroid, 33333);
            Vector3 asteroidScale = bodyMesh.Scale;

            DotNetNativeTestSuite.AssertTrue(starScale.X > 0.0f && starScale.X < 10.0f, "Star scale should be reasonable");
            DotNetNativeTestSuite.AssertTrue(planetScale.X > 0.0f && planetScale.X < 10.0f, "Planet scale should be reasonable");
            DotNetNativeTestSuite.AssertTrue(asteroidScale.X > 0.0f && asteroidScale.X < 10.0f, "Asteroid scale should be reasonable");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInfoLabelsUpdate()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 12345);
            Node? inspector = viewer.inspector_panel;
            DotNetNativeTestSuite.AssertNotNull(inspector, "Inspector panel should exist");
            VBoxContainer? inspectorContainer = inspector!.GetNodeOrNull<VBoxContainer>("InspectorContainer");
            DotNetNativeTestSuite.AssertNotNull(inspectorContainer, "Inspector container should exist");
            DotNetNativeTestSuite.AssertTrue(inspectorContainer!.GetChildCount() > 0, "Inspector should render content");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInspectorShowsAllSections()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 55555);
            VBoxContainer? inspectorContainer = viewer.inspector_panel?.GetNodeOrNull<VBoxContainer>("InspectorContainer");
            DotNetNativeTestSuite.AssertNotNull(inspectorContainer, "Inspector container should exist");
            DotNetNativeTestSuite.AssertTrue(inspectorContainer!.GetChildCount() >= 3, "Inspector should include multiple sections");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestCollapsibleSections()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 12345);
            VBoxContainer? inspectorContainer = viewer.inspector_panel?.GetNodeOrNull<VBoxContainer>("InspectorContainer");
            DotNetNativeTestSuite.AssertNotNull(inspectorContainer, "Inspector container should exist");
            DotNetNativeTestSuite.AssertTrue(inspectorContainer!.GetChildCount() > 0, "Inspector should expose section content");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestDeterministicGeneration()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 99999);
            double firstMass = viewer.current_body!.Physical.MassKg;
            double firstRadius = viewer.current_body.Physical.RadiusM;

            viewer.generate_object(ObjectViewer.ObjectType.Planet, 99999);
            double secondMass = viewer.current_body!.Physical.MassKg;
            double secondRadius = viewer.current_body.Physical.RadiusM;

            DotNetNativeTestSuite.AssertEqual(firstMass, secondMass, "Mass should be deterministic");
            DotNetNativeTestSuite.AssertEqual(firstRadius, secondRadius, "Radius should be deterministic");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestViewerSaveAndLoadBody()
    {
        const string path = "user://object_viewer_save_test.sgt";

        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.generate_object(ObjectViewer.ObjectType.Star, 24680);

            Error saveError = viewer.SaveCurrentBodyToPath(path);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveError, "Viewer save should succeed");

            SaveDataLoadResult result = viewer.LoadBodyFromPath(path);
            DotNetNativeTestSuite.AssertTrue(result.Success, "Viewer load should succeed");
            DotNetNativeTestSuite.AssertNotNull(result.Body, "Loaded body should be present");
            DotNetNativeTestSuite.AssertEqual(CelestialType.Type.Star, viewer.current_body!.Type, "Viewer should display loaded star");
        }
        finally
        {
            CleanupSaveFile(path);
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }
}
