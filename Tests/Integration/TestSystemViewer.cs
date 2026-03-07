#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.SystemViewer;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestSystemViewer
{
    private const string SystemViewerScenePath = "res://src/app/system_viewer/SystemViewer.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestSystemViewer::test_scene_loads", TestSceneLoads);
        runner.RunNativeTest("TestSystemViewer::test_scene_instantiates", TestSceneInstantiates);
        runner.RunNativeTest("TestSystemViewer::test_has_camera", TestHasCamera);
        runner.RunNativeTest("TestSystemViewer::test_has_3d_containers", TestHas3dContainers);
        runner.RunNativeTest("TestSystemViewer::test_has_ui_structure", TestHasUiStructure);
        runner.RunNativeTest("TestSystemViewer::test_has_generation_controls", TestHasGenerationControls);
        runner.RunNativeTest("TestSystemViewer::test_has_view_controls", TestHasViewControls);
        runner.RunNativeTest("TestSystemViewer::test_has_inspector_panel", TestHasInspectorPanel);
        runner.RunNativeTest("TestSystemViewer::test_has_save_load_section", TestHasSaveLoadSection);
        runner.RunNativeTest("TestSystemViewer::test_has_environment", TestHasEnvironment);
        runner.RunNativeTest("TestSystemViewer::test_body_node_scene_loads", TestBodyNodeSceneLoads);
        runner.RunNativeTest("TestSystemViewer::test_belt_renderer_script_loads", TestBeltRendererScriptLoads);
        runner.RunNativeTest("TestSystemViewer::test_has_parameter_editor_controls", TestHasParameterEditorControls);
        runner.RunNativeTest("TestSystemViewer::test_invalid_spec_blocks_generation", TestInvalidSpecBlocksGeneration);
    }

    private static SystemViewer CreateViewer()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        viewer._Ready();
        return viewer;
    }

    private static void TestSceneLoads()
    {
        IntegrationTestUtils.LoadPackedScene(SystemViewerScenePath);
    }

    private static void TestSceneInstantiates()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer, "SystemViewer should instantiate");
            DotNetNativeTestSuite.AssertTrue(viewer is Node3D, "SystemViewer should be Node3D");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasCamera()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            Node? camera = viewer.GetNodeOrNull<Node>("CameraRig/Camera3D");
            DotNetNativeTestSuite.AssertNotNull(camera, "SystemViewer should have camera");
            DotNetNativeTestSuite.AssertTrue(camera is Camera3D, "Camera node should be Camera3D");
            DotNetNativeTestSuite.AssertNotNull(camera!.GetScript(), "Camera should have script attached");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHas3dContainers()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertTrue(viewer.GetNodeOrNull<Node3D>("BodiesContainer") != null, "Should have BodiesContainer");
            DotNetNativeTestSuite.AssertTrue(viewer.GetNodeOrNull<Node3D>("OrbitsContainer") != null, "Should have OrbitsContainer");
            DotNetNativeTestSuite.AssertTrue(viewer.GetNodeOrNull<Node3D>("ZonesContainer") != null, "Should have ZonesContainer");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasUiStructure()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Control>("UI"), "Should have UI root");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Node>("UI/TopBar"), "Should have top bar");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Label>("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel"), "Should have status label");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Node>("UI/SidePanel"), "Should have side panel");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasGenerationControls()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/StarCountContainer/StarCountSpin"), "Should have star-count control");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/SeedContainer/SeedInput"), "Should have seed input");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Button>($"{basePath}/ButtonContainer/GenerateButton"), "Should have generate button");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Button>($"{basePath}/ButtonContainer/RerollButton"), "Should have reroll button");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasViewControls()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<CheckBox>($"{basePath}/ShowOrbitsCheck"), "Should have orbit toggle");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<CheckBox>($"{basePath}/ShowZonesCheck"), "Should have zone toggle");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasInspectorPanel()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            Node? inspector = viewer.GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
            DotNetNativeTestSuite.AssertNotNull(inspector, "Should have inspector panel");
            DotNetNativeTestSuite.AssertTrue(inspector is VBoxContainer, "Inspector should be VBoxContainer");
            DotNetNativeTestSuite.AssertNotNull(inspector!.GetScript(), "Inspector should have script");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasSaveLoadSection()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Button>($"{basePath}/SaveButton"), "Should have save button");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Button>($"{basePath}/LoadButton"), "Should have load button");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestHasEnvironment()
    {
        SystemViewer viewer = IntegrationTestUtils.InstantiateScene<SystemViewer>(SystemViewerScenePath);
        try
        {
            WorldEnvironment? environment = viewer.GetNodeOrNull<WorldEnvironment>("Environment/WorldEnvironment");
            DotNetNativeTestSuite.AssertNotNull(environment, "Should have world environment");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestBodyNodeSceneLoads()
    {
        PackedScene scene = IntegrationTestUtils.LoadPackedScene("res://src/app/system_viewer/SystemBodyNode.tscn");
        Node? node = scene.Instantiate();
        DotNetNativeTestSuite.AssertNotNull(node, "SystemBodyNode should instantiate");
        DotNetNativeTestSuite.AssertTrue(node is Node3D, "SystemBodyNode should be Node3D");
        IntegrationTestUtils.CleanupNode(node);
    }

    private static void TestBeltRendererScriptLoads()
    {
        BeltRenderer renderer = new();
        DotNetNativeTestSuite.AssertNotNull(renderer, "BeltRenderer type should instantiate");
        IntegrationTestUtils.CleanupNode(renderer);
    }

    private static void TestHasParameterEditorControls()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/StarCountContainer/StarCountSpin"), "Should expose min-star editor");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<LineEdit>($"{basePath}/SpectralHintsContainer/SpectralHintsInput"), "Should expose spectral-hint editor");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/SystemAgeContainer/SystemAgeInput"), "Should expose age editor");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/SystemMetallicityContainer/SystemMetallicityInput"), "Should expose metallicity editor");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInvalidSpecBlocksGeneration()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            SolarSystem? previousSystem = viewer.GetCurrentSystem();
            SolarSystemSpec invalidSpec = new(0, 1, 1);
            viewer.GenerateSystem(invalidSpec);

            Label? statusLabel = viewer.GetNodeOrNull<Label>("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel");
            DotNetNativeTestSuite.AssertNotNull(statusLabel, "Status label should exist");
            DotNetNativeTestSuite.AssertTrue(statusLabel!.Text.StartsWith("Error"), "Blocking validation errors should surface as an error status");
            DotNetNativeTestSuite.AssertEqual(previousSystem, viewer.GetCurrentSystem(), "Blocking validation errors should leave the previous system intact");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

}
