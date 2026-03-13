#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.Rendering;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation.Archetypes;
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
        runner.RunNativeTest("TestObjectViewer::test_top_menu_exists", TestTopMenuExists);
        runner.RunNativeTest("TestObjectViewer::test_status_messages", TestStatusMessages);
        runner.RunNativeTest("TestObjectViewer::test_generate_button_creates_objects", TestGenerateButtonCreatesObjects);
        runner.RunNativeTest("TestObjectViewer::test_object_scaling", TestObjectScaling);
        runner.RunNativeTest("TestObjectViewer::test_info_labels_update", TestInfoLabelsUpdate);
        runner.RunNativeTest("TestObjectViewer::test_inspector_shows_all_sections", TestInspectorShowsAllSections);
        runner.RunNativeTest("TestObjectViewer::test_collapsible_sections", TestCollapsibleSections);
        runner.RunNativeTest("TestObjectViewer::test_deterministic_generation", TestDeterministicGeneration);
        runner.RunNativeTest("TestObjectViewer::test_viewer_save_and_load_body", TestViewerSaveAndLoadBody);
        runner.RunNativeTest("TestObjectViewer::test_preset_controls_exist", TestPresetControlsExist);
        runner.RunNativeTest("TestObjectViewer::test_earth_like_preset_updates_spec_snapshot", TestEarthLikePresetUpdatesSpecSnapshot);
        runner.RunNativeTest("TestObjectViewer::test_standalone_return_stays_menu_scoped", TestStandaloneReturnStaysMenuScoped);
        runner.RunNativeTest("TestObjectViewer::test_standalone_generator_waits_for_explicit_generation", TestStandaloneGeneratorWaitsForExplicitGeneration);
        runner.RunNativeTest("TestObjectViewer::test_use_case_controls_exist", TestUseCaseControlsExist);
        runner.RunNativeTest("TestObjectViewer::test_traveller_readout_visibility_tracks_settings", TestTravellerReadoutVisibilityTracksSettings);
        runner.RunNativeTest("TestObjectViewer::test_inspector_shows_uwp_for_planets", TestInspectorShowsUwpForPlanets);
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

    private static void TestTopMenuExists()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
            DotNetNativeTestSuite.AssertNotNull(menuRow, "Object viewer should expose a top menu row");
            DotNetNativeTestSuite.AssertGreaterThan(menuRow!.GetChildCount(), 3, "Object viewer should expose standard top-level menus");
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

    private static void TestPresetControlsExist()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            OptionButton? presetOption = viewer.GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PresetContainer/PresetOption");
            Label? assumptionsLabel = viewer.GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PresetAssumptionsLabel");
            DotNetNativeTestSuite.AssertNotNull(presetOption, "Object viewer should expose a preset selector");
            DotNetNativeTestSuite.AssertNotNull(assumptionsLabel, "Object viewer should expose preset assumption text");
            DotNetNativeTestSuite.AssertTrue(presetOption!.ItemCount > 0, "Preset selector should contain entries");
            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrEmpty(assumptionsLabel!.Text), "Preset assumption text should be populated");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestEarthLikePresetUpdatesSpecSnapshot()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            OptionButton? typeOption = viewer.GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/TypeContainer/TypeOption");
            OptionButton? presetOption = viewer.GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PresetContainer/PresetOption");
            Button? generateButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton");

            DotNetNativeTestSuite.AssertNotNull(typeOption, "Type selector should exist");
            DotNetNativeTestSuite.AssertNotNull(presetOption, "Preset selector should exist");
            DotNetNativeTestSuite.AssertNotNull(generateButton, "Generate button should exist");

            typeOption!.Select((int)ObjectViewer.ObjectType.Planet);
            typeOption.EmitSignal(OptionButton.SignalName.ItemSelected, (long)ObjectViewer.ObjectType.Planet);
            presetOption!.Select(1);
            presetOption.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
            generateButton!.EmitSignal(Button.SignalName.Pressed);

            DotNetNativeTestSuite.AssertNotNull(viewer.current_body, "Preset generation should produce a body");
            Godot.Collections.Dictionary snapshot = viewer.current_body!.Provenance.SpecSnapshot;
            DotNetNativeTestSuite.AssertEqual("planet", (string)snapshot["spec_type"], "Preset generation should preserve the planet spec snapshot");
            DotNetNativeTestSuite.AssertEqual((int)SizeCategory.Category.Terrestrial, (int)snapshot["size_category"], "Earth-like preset should lock terrestrial size target");
            DotNetNativeTestSuite.AssertEqual((int)OrbitZone.Zone.Temperate, (int)snapshot["orbit_zone"], "Earth-like preset should lock temperate orbit target");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStandaloneReturnStaysMenuScoped()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.PrepareStandaloneGenerator(12345, ObjectViewer.ObjectType.Planet);
            Button? generateButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton");
            DotNetNativeTestSuite.AssertNotNull(generateButton, "Generate button should exist");

            Button? backButton = FindBackButton(viewer);
            DotNetNativeTestSuite.AssertNull(backButton, "Standalone mode should not show a duplicate header back button");

            PopupMenu? fileMenu = GetFileMenu(viewer);
            DotNetNativeTestSuite.AssertNotNull(fileMenu, "Standalone mode should expose a file menu");
            fileMenu!.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "Standalone mode should route return through the file menu");

            generateButton!.EmitSignal(Button.SignalName.Pressed);

            fileMenu.EmitSignal(PopupMenu.SignalName.AboutToPopup);
            DotNetNativeTestSuite.AssertTrue(PopupContainsText(fileMenu, "Return to Main Menu"), "Regeneration should preserve menu-scoped return navigation");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestStandaloneGeneratorWaitsForExplicitGeneration()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            viewer.PrepareStandaloneGenerator(12345, ObjectViewer.ObjectType.Planet);

            Button? saveButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/SaveButton");
            Button? loadButton = viewer.GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/LoadButton");
            Label? emptyStateLabel = viewer.GetNodeOrNull<Label>("UI/EmptyStateLabel");

            DotNetNativeTestSuite.AssertNull(viewer.current_body, "Standalone object generation should not auto-generate");
            DotNetNativeTestSuite.AssertEqual("Set parameters, then click Generate", viewer.status_label!.Text, "Standalone startup should prompt for generation");
            DotNetNativeTestSuite.AssertNotNull(saveButton, "Save button should exist");
            DotNetNativeTestSuite.AssertTrue(saveButton!.Disabled, "Save should stay disabled until a body exists");
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

    private static void TestUseCaseControlsExist()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            string basePath = "UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection";
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<OptionButton>($"{basePath}/RulesetContainer/RulesetModeOption"), "Object viewer should expose a ruleset selector");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<CheckBox>($"{basePath}/ShowTravellerReadoutsCheck"), "Object viewer should expose a Traveller readout toggle");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/LifePermissivenessContainer/LifePermissivenessInput"), "Object viewer should expose a life-bias control");
            DotNetNativeTestSuite.AssertNotNull(viewer.GetNodeOrNull<SpinBox>($"{basePath}/PopulationPermissivenessContainer/PopulationPermissivenessInput"), "Object viewer should expose a population-bias control");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestTravellerReadoutVisibilityTracksSettings()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            VBoxContainer? inspectorContainer = viewer.inspector_panel?.GetNodeOrNull<VBoxContainer>("InspectorContainer");
            OptionButton? rulesetOption = viewer.GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/RulesetContainer/RulesetModeOption");

            DotNetNativeTestSuite.AssertNotNull(inspectorContainer, "Inspector container should exist");
            DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Ruleset selector should exist");

            viewer.generate_object(ObjectViewer.ObjectType.Planet, 54321);
            DotNetNativeTestSuite.AssertFalse(InspectorContainsText(inspectorContainer!, "Size Code"), "Default object generation should hide Traveller readouts");

            rulesetOption!.Select(1);
            rulesetOption.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
            viewer.generate_object(ObjectViewer.ObjectType.Planet, 54321);

            DotNetNativeTestSuite.AssertTrue(InspectorContainsText(inspectorContainer!, "Traveller"), "Traveller mode should show the Traveller inspector section");
            DotNetNativeTestSuite.AssertTrue(InspectorContainsText(inspectorContainer, "Size Code"), "Traveller mode should show the Traveller size-code readout");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static void TestInspectorShowsUwpForPlanets()
    {
        ObjectViewer viewer = CreateViewer();
        try
        {
            VBoxContainer? inspectorContainer = viewer.inspector_panel?.GetNodeOrNull<VBoxContainer>("InspectorContainer");
            DotNetNativeTestSuite.AssertNotNull(inspectorContainer, "Inspector container should exist");

            viewer.generate_object(ObjectViewer.ObjectType.Planet, 13579);

            DotNetNativeTestSuite.AssertTrue(InspectorContainsText(inspectorContainer!, "World Profile"), "Planet inspector should show a world-profile section");
            DotNetNativeTestSuite.AssertTrue(InspectorContainsText(inspectorContainer, "UWP"), "Planet inspector should surface UWP near the top of the readout");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(viewer);
        }
    }

    private static bool InspectorContainsText(Control root, string text)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Label typedLabel && typedLabel.Text.Contains(text))
            {
                return true;
            }

            if (child is Control typedControl && InspectorContainsText(typedControl, text))
            {
                return true;
            }
        }

        return false;
    }

    private static Button? FindBackButton(ObjectViewer viewer)
    {
        HBoxContainer? headerRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow");
        if (headerRow == null)
        {
            return null;
        }

        foreach (Node child in headerRow.GetChildren())
        {
            if (child is Button typedButton && typedButton.Text.Contains("Back"))
            {
                return typedButton;
            }
        }

        return null;
    }

    private static PopupMenu? GetFileMenu(ObjectViewer viewer)
    {
        HBoxContainer? menuRow = viewer.GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
        if (menuRow == null)
        {
            return null;
        }

        foreach (Node child in menuRow.GetChildren())
        {
            if (child is MenuButton typedButton && typedButton.Text == "File")
            {
                return typedButton.GetPopup();
            }
        }

        return null;
    }

    private static bool PopupContainsText(PopupMenu popupMenu, string text)
    {
        for (int index = 0; index < popupMenu.ItemCount; index += 1)
        {
            if (popupMenu.GetItemText(index).Contains(text))
            {
                return true;
            }
        }

        return false;
    }
}
