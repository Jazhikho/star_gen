#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.SystemViewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Population;
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
        runner.RunNativeTest("TestSystemViewer::test_top_menu_exists", TestTopMenuExists);
        runner.RunNativeTest("TestSystemViewer::test_has_generation_controls", TestHasGenerationControls);
        runner.RunNativeTest("TestSystemViewer::test_has_view_controls", TestHasViewControls);
        runner.RunNativeTest("TestSystemViewer::test_has_inspector_panel", TestHasInspectorPanel);
        runner.RunNativeTest("TestSystemViewer::test_has_save_load_section", TestHasSaveLoadSection);
        runner.RunNativeTest("TestSystemViewer::test_has_environment", TestHasEnvironment);
        runner.RunNativeTest("TestSystemViewer::test_body_node_scene_loads", TestBodyNodeSceneLoads);
        runner.RunNativeTest("TestSystemViewer::test_belt_renderer_script_loads", TestBeltRendererScriptLoads);
        runner.RunNativeTest("TestSystemViewer::test_has_parameter_editor_controls", TestHasParameterEditorControls);
        runner.RunNativeTest("TestSystemViewer::test_invalid_spec_blocks_generation", TestInvalidSpecBlocksGeneration);
        runner.RunNativeTest("TestSystemViewer::test_standalone_startup_waits_for_generate", TestStandaloneStartupWaitsForGenerate);
        runner.RunNativeTest("TestSystemViewer::test_traveller_controls_exist", TestTravellerControlsExist);
        runner.RunNativeTest("TestSystemViewer::test_traveller_ruleset_applies_defaults", TestTravellerRulesetAppliesDefaults);
        runner.RunNativeTest("TestSystemViewer::test_inspector_concept_atlas_button_emits_viewer_signal", TestInspectorConceptAtlasButtonEmitsViewerSignal);
        runner.RunNativeTest("TestSystemViewer::test_populated_world_button_focuses_body", TestPopulatedWorldButtonFocusesBody);
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
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Label>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel"), "Should have status label");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<Node>("UI/SidePanel"), "Should have side panel");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestTopMenuExists()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            DotNetNativeTestSuite.AssertNotNull(menuRow, "System viewer should expose a top menu row");
            DotNetNativeTestSuite.AssertGreaterThan(menuRow!.GetChildCount(), 3, "System viewer should expose standard top-level menus");
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

            Label? statusLabel = viewer.GetNodeOrNull<Label>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel");
            DotNetNativeTestSuite.AssertNotNull(statusLabel, "Status label should exist");
            DotNetNativeTestSuite.AssertTrue(statusLabel!.Text.StartsWith("Error"), "Blocking validation errors should surface as an error status");
            DotNetNativeTestSuite.AssertEqual(previousSystem, viewer.GetCurrentSystem(), "Blocking validation errors should leave the previous system intact");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStandaloneStartupWaitsForGenerate()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            Label? statusLabel = viewer.GetNodeOrNull<Label>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel");
            Button? saveButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton");
            Button? loadButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton");
            Label? emptyStateLabel = viewer.GetNodeOrNull<Label>("UI/EmptyStateLabel");

            DotNetNativeTestSuite.AssertNull(viewer.GetCurrentSystem(), "System viewer should not auto-generate on startup");
            DotNetNativeTestSuite.AssertNotNull(statusLabel, "Status label should exist");
            DotNetNativeTestSuite.AssertEqual("Set parameters, then click Generate", statusLabel!.Text, "Standalone startup should prompt for generation");
            DotNetNativeTestSuite.AssertNotNull(saveButton, "Save button should exist");
            DotNetNativeTestSuite.AssertTrue(saveButton!.Disabled, "Save should stay disabled without a generated system");
            DotNetNativeTestSuite.AssertNotNull(loadButton, "Load button should exist");
            DotNetNativeTestSuite.AssertFalse(loadButton!.Disabled, "Load should stay enabled before generation");
            DotNetNativeTestSuite.AssertNotNull(emptyStateLabel, "Empty-state label should exist");
            DotNetNativeTestSuite.AssertTrue(emptyStateLabel!.Visible, "Empty-state label should be visible before generation");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestTravellerControlsExist()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<OptionButton>($"{basePath}/RulesetModeContainer/RulesetModeOption"), "System viewer should expose a ruleset selector");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<CheckBox>($"{basePath}/ShowTravellerReadoutsCheck"), "System viewer should expose a Traveller readout toggle");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<HSlider>($"{basePath}/LifePermissivenessContainer/LifePermissivenessInput"), "System viewer should expose a life-potential control");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<HSlider>($"{basePath}/PopulationPermissivenessContainer/PopulationPermissivenessInput"), "System viewer should expose a settlement-density control");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<OptionButton>($"{basePath}/MainworldPolicyContainer/MainworldPolicyOption"), "System viewer should expose a mainworld-policy selector");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestTravellerRulesetAppliesDefaults()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection";
            OptionButton? rulesetOption = viewer.GetNodeOrNull<OptionButton>($"{basePath}/RulesetModeContainer/RulesetModeOption");
            CheckBox? readoutsCheck = viewer.GetNodeOrNull<CheckBox>($"{basePath}/ShowTravellerReadoutsCheck");
            CheckBox? populationCheck = viewer.GetNodeOrNull<CheckBox>($"{basePath}/GeneratePopulationCheck");
            OptionButton? mainworldOption = viewer.GetNodeOrNull<OptionButton>($"{basePath}/MainworldPolicyContainer/MainworldPolicyOption");

            DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Ruleset selector should exist");
            DotNetNativeTestSuite.AssertNotNull(readoutsCheck, "Traveller readout toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(populationCheck, "Population toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(mainworldOption, "Mainworld selector should exist");

            rulesetOption!.Select(1);
            rulesetOption.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);

            DotNetNativeTestSuite.AssertTrue(readoutsCheck!.ButtonPressed, "Traveller mode should enable Traveller readouts by default");
            DotNetNativeTestSuite.AssertTrue(populationCheck!.ButtonPressed, "Traveller mode should enable population generation by default");
            DotNetNativeTestSuite.AssertEqual(2, mainworldOption!.Selected, "Traveller mode should require a mainworld by default");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInspectorConceptAtlasButtonEmitsViewerSignal()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            SystemInspectorPanel? panel = viewer.GetNodeOrNull<SystemInspectorPanel>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            CelestialBody body = IntegrationTestUtils.CreateTestBody(name: "Kepler", type: CelestialType.Type.Planet);
            bool signaled = false;
            string emittedBodyId = string.Empty;
            viewer.Connect(
                SystemViewer.SignalName.OpenConceptAtlasRequested,
                Callable.From<GodotObject>(emittedBody =>
                {
                    signaled = true;
                    if (emittedBody is CelestialBody typedBody)
                    {
                        emittedBodyId = typedBody.Id;
                    }
                }));

            panel!.DisplaySelectedBody(body);
            Button? button = FindButtonByText(panel, "Open Concept Atlas");
            DotNetNativeTestSuite.AssertNotNull(button, "System inspector should expose a concept-atlas button for selected bodies");

            button!.EmitSignal(Button.SignalName.Pressed);

            DotNetNativeTestSuite.AssertTrue(signaled, "Concept-atlas button should bubble through the system viewer signal");
            DotNetNativeTestSuite.AssertEqual(body.Id, emittedBodyId, "Viewer signal should preserve the selected body");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestPopulatedWorldButtonFocusesBody()
    {
        SystemViewer viewer = CreateViewer();
        try
        {
            SolarSystem system = new("focus_test", "Focus Test");
            CelestialBody inhabitedBody = IntegrationTestUtils.CreateTestBody(name: "Haven", type: CelestialType.Type.Planet);
            PlanetPopulationData populationData = new()
            {
                BodyId = inhabitedBody.Id,
            };
            NativePopulation nativePopulation = new()
            {
                Id = "native_focus",
                Name = "Haveners",
                BodyId = inhabitedBody.Id,
                Population = 1250000,
                PeakPopulation = 1250000,
            };
            populationData.NativePopulations.Add(nativePopulation);
            inhabitedBody.PopulationData = populationData;
            system.AddBody(inhabitedBody);

            viewer.DisplaySystem(system);

            SystemInspectorPanel? panel = viewer.GetNodeOrNull<SystemInspectorPanel>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
            DotNetNativeTestSuite.AssertNotNull(panel, "Inspector panel should exist");

            Button? button = FindButtonByText(panel!, "Focus Haven (" + StarGen.App.Viewer.PropertyFormatter.FormatPopulation(populationData.GetTotalPopulation()) + ")");
            DotNetNativeTestSuite.AssertNotNull(button, "System overview should expose a populated-world focus button");

            button!.EmitSignal(Button.SignalName.Pressed);

            DotNetNativeTestSuite.AssertTrue(GetSelectedBodyId(viewer) == inhabitedBody.Id, "Focus button should select the populated world");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static Button? FindButtonByText(Node root, string text)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Button typedButton && typedButton.Text == text)
            {
                return typedButton;
            }

            Button? nested = FindButtonByText(child, text);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static string GetSelectedBodyId(SystemViewer viewer)
    {
        System.Reflection.FieldInfo? field = typeof(SystemViewer).GetField("_selectedBodyId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field == null)
        {
            return string.Empty;
        }

        object? value = field.GetValue(viewer);
        if (value is string selectedBodyId)
        {
            return selectedBodyId;
        }

        return string.Empty;
    }

}
