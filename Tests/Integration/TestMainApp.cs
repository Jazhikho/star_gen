#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.SystemViewer;
using StarGen.App.Viewer;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestMainApp
{
    private const string MainAppScenePath = "res://src/app/MainApp.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestMainApp::test_scene_loads", TestSceneLoads);
        runner.RunNativeTest("TestMainApp::test_instantiates", TestInstantiates);
        runner.RunNativeTest("TestMainApp::test_has_viewer_container", TestHasViewerContainer);
        runner.RunNativeTest("TestMainApp::test_object_viewer_return_menu_emits_signal", TestObjectViewerReturnMenuEmitsSignal);
        runner.RunNativeTest("TestMainApp::test_object_viewer_main_menu_return_emits_signal", TestObjectViewerMainMenuReturnEmitsSignal);
        runner.RunNativeTest("TestMainApp::test_object_viewer_display_external_body_sets_state", TestObjectViewerDisplayExternalBodySetsState);
    }

    private static void TestSceneLoads()
    {
        IntegrationTestUtils.LoadPackedScene(MainAppScenePath);
    }

    private static void TestInstantiates()
    {
        MainApp app = IntegrationTestUtils.InstantiateScene<MainApp>(MainAppScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(app.GetScript(), "MainApp should have script attached");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static void TestHasViewerContainer()
    {
        MainApp app = IntegrationTestUtils.InstantiateScene<MainApp>(MainAppScenePath);
        try
        {
            Node? container = app.GetNodeOrNull<Node>("ViewerContainer");
            DotNetNativeTestSuite.AssertNotNull(container, "MainApp should include ViewerContainer");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(app);
        }
    }

    private static ObjectViewer CreateObjectViewer()
    {
        ObjectViewer viewer = IntegrationTestUtils.InstantiateScene<ObjectViewer>("res://src/app/viewer/ObjectViewer.tscn");
        viewer._Ready();
        return viewer;
    }

    private static void TestObjectViewerReturnMenuEmitsSignal()
    {
        ObjectViewer viewer = CreateObjectViewer();
        try
        {
            bool requestedBack = false;
            viewer.Connect(ObjectViewer.SignalName.BackToSystemRequested, Callable.From(() => requestedBack = true));

            viewer.DisplayExternalBody(IntegrationTestUtils.CreateTestBody(type: StarGen.Domain.Celestial.CelestialType.Type.Planet), [], 123);
            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            DotNetNativeTestSuite.AssertNotNull(menuRow, "Object viewer should expose a top menu row");

            PopupMenu? fileMenu = null;
            foreach (Node child in menuRow!.GetChildren())
            {
                if (child is MenuButton typedButton && typedButton.Text == "File")
                {
                    fileMenu = typedButton.GetPopup();
                    break;
                }
            }

            DotNetNativeTestSuite.AssertNotNull(fileMenu, "Displaying an external body should expose a file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            fileMenu.EmitSignal(PopupMenu.SignalName.IdPressed, 3L);
            DotNetNativeTestSuite.AssertTrue(requestedBack, "Selecting the return menu item should emit BackToSystemRequested");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestObjectViewerMainMenuReturnEmitsSignal()
    {
        ObjectViewer viewer = CreateObjectViewer();
        try
        {
            bool requestedMainMenu = false;
            viewer.Connect(ObjectViewer.SignalName.BackToMainMenuRequested, Callable.From(() => requestedMainMenu = true));
            viewer.SetBackNavigationVisibility(true, "Return to Main Menu", "Return to the main menu", true);

            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            DotNetNativeTestSuite.AssertNotNull(menuRow, "Object viewer should expose a top menu row");

            PopupMenu? fileMenu = null;
            foreach (Node child in menuRow!.GetChildren())
            {
                if (child is MenuButton typedButton && typedButton.Text == "File")
                {
                    fileMenu = typedButton.GetPopup();
                    break;
                }
            }

            DotNetNativeTestSuite.AssertNotNull(fileMenu, "Main-menu return should be available from the file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            fileMenu.EmitSignal(PopupMenu.SignalName.IdPressed, 3L);
            DotNetNativeTestSuite.AssertTrue(requestedMainMenu, "Selecting the main-menu return should emit BackToMainMenuRequested");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestObjectViewerDisplayExternalBodySetsState()
    {
        ObjectViewer viewer = CreateObjectViewer();
        try
        {
            StarGen.Domain.Celestial.CelestialBody body = IntegrationTestUtils.CreateTestBody(type: StarGen.Domain.Celestial.CelestialType.Type.Planet);
            viewer.DisplayExternalBody(body, [], 98765);

            DotNetNativeTestSuite.AssertEqual(body, viewer.current_body, "DisplayExternalBody should set the current body");
            DotNetNativeTestSuite.AssertTrue(
                viewer.status_label != null && viewer.status_label.Text.Contains(body.Name),
                "DisplayExternalBody should update the visible status with the body name");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }
}
