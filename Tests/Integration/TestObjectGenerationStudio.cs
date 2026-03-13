#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Domain.Math;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestObjectGenerationStudio
{
    private const string ScenePath = "res://src/app/ObjectGenerationScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestObjectGenerationStudio::test_scene_instantiates", TestSceneInstantiates);
        runner.RunNativeTest("TestObjectGenerationStudio::test_seed_hidden_by_default", TestSeedHiddenByDefault);
        runner.RunNativeTest("TestObjectGenerationStudio::test_traveller_planet_request_builds_profile", TestTravellerPlanetRequestBuildsProfile);
        runner.RunNativeTest("TestObjectGenerationStudio::test_advanced_overrides_persist_to_request", TestAdvancedOverridesPersistToRequest);
        runner.RunNativeTest("TestObjectGenerationStudio::test_optional_feature_labels_use_auto_yes_no", TestOptionalFeatureLabelsUseAutoYesNo);
        runner.RunNativeTest("TestObjectGenerationStudio::test_traveller_planet_request_resolves_orbit_zone", TestTravellerPlanetRequestResolvesOrbitZone);
    }

    private static ObjectGenerationScreen CreateScreen()
    {
        ObjectGenerationScreen screen = IntegrationTestUtils.InstantiateScene<ObjectGenerationScreen>(ScenePath);
        screen._Ready();
        return screen;
    }

    private static void TestSceneInstantiates()
    {
        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(screen, "Object generation studio should instantiate");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestSeedHiddenByDefault()
    {
        string preferencesPath = ProjectSettings.GlobalizePath("user://studio_ui_preferences.cfg");
        Cleanup(preferencesPath);

        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            HBoxContainer? seedRow = screen.FindChild("SeedRow", true, false) as HBoxContainer;
            DotNetNativeTestSuite.AssertNotNull(seedRow, "Seed row should exist");
            DotNetNativeTestSuite.AssertFalse(seedRow!.Visible, "Seed row should be hidden by default");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
            Cleanup(preferencesPath);
        }
    }

    private static void TestTravellerPlanetRequestBuildsProfile()
    {
        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            OptionButton? rulesetOption = screen.FindChild("RulesetModeOption", true, false) as OptionButton;
            HBoxContainer? travellerRow = screen.FindChild("UseTravellerWorldProfileRow", true, false) as HBoxContainer;
            HBoxContainer? sizeCodeRow = screen.FindChild("TravellerSizeCodeRow", true, false) as HBoxContainer;
            CheckBox? travellerToggle = travellerRow?.GetChild(1) as CheckBox;
            OptionButton? sizeCodeOption = sizeCodeRow?.GetChild(1) as OptionButton;

            DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Ruleset selector should exist");
            DotNetNativeTestSuite.AssertNotNull(travellerToggle, "Traveller generation toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(sizeCodeOption, "Traveller size-code selector should exist");

            rulesetOption!.Select(1);
            rulesetOption.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
            travellerToggle!.ButtonPressed = true;
            travellerToggle.EmitSignal(CheckBox.SignalName.Toggled, true);
            SelectOptionById(sizeCodeOption!, 8);

            ObjectGenerationRequest request = screen.GetCurrentRequest();
            DotNetNativeTestSuite.AssertTrue(request.SpecData.Count > 0, "Planet studio should emit explicit spec data");
            DotNetNativeTestSuite.AssertTrue(request.TravellerWorldProfileData.Count > 0, "Traveller mode should emit a Traveller world profile");
            DotNetNativeTestSuite.AssertEqual(8, (int)request.TravellerWorldProfileData["size_code"], "Selected Traveller size code should persist");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestAdvancedOverridesPersistToRequest()
    {
        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            CheckBox? advancedToggle = screen.FindChild("ShowAdvancedControlsCheck", true, false) as CheckBox;
            CheckBox? massToggle = screen.FindChild("MassOverrideToggle", true, false) as CheckBox;
            SpinBox? massInput = screen.FindChild("MassOverrideInput", true, false) as SpinBox;

            DotNetNativeTestSuite.AssertNotNull(advancedToggle, "Advanced-controls toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(massToggle, "Mass override toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(massInput, "Mass override input should exist");

            advancedToggle!.ButtonPressed = true;
            advancedToggle.EmitSignal(CheckBox.SignalName.Toggled, true);
            massToggle!.ButtonPressed = true;
            massToggle.EmitSignal(CheckBox.SignalName.Toggled, true);
            massInput!.Value = 1.0;

            ObjectGenerationRequest request = screen.GetCurrentRequest();
            Godot.Collections.Dictionary overrides = (Godot.Collections.Dictionary)request.SpecData["overrides"];
            double storedMass = (double)overrides["physical.mass_kg"];
            double difference = System.Math.Abs(storedMass - Units.EarthMassKg);
            DotNetNativeTestSuite.AssertTrue(difference < 1.0e21, $"Planet mass override should be stored in base units. Difference was {difference}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestOptionalFeatureLabelsUseAutoYesNo()
    {
        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            HBoxContainer? atmosphereRow = screen.FindChild("PlanetAtmosphereRow", true, false) as HBoxContainer;
            DotNetNativeTestSuite.AssertNotNull(atmosphereRow, "Planet atmosphere row should exist");
            OptionButton? atmosphereOption = atmosphereRow!.GetChild(1) as OptionButton;
            DotNetNativeTestSuite.AssertNotNull(atmosphereOption, "Planet atmosphere option should exist");

            DotNetNativeTestSuite.AssertEqual("Auto", atmosphereOption!.GetItemText(0), "Tri-state options should expose Auto");
            DotNetNativeTestSuite.AssertEqual("Yes", atmosphereOption.GetItemText(1), "Tri-state options should expose Yes");
            DotNetNativeTestSuite.AssertEqual("No", atmosphereOption.GetItemText(2), "Tri-state options should expose No");
            DotNetNativeTestSuite.AssertTrue(atmosphereOption.TooltipText.Contains("Auto lets seeded generation decide"), "Tri-state tooltip should explain Auto behavior");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestTravellerPlanetRequestResolvesOrbitZone()
    {
        ObjectGenerationScreen screen = CreateScreen();
        try
        {
            OptionButton? rulesetOption = screen.FindChild("RulesetModeOption", true, false) as OptionButton;
            HBoxContainer? travellerRow = screen.FindChild("UseTravellerWorldProfileRow", true, false) as HBoxContainer;
            HBoxContainer? orbitRow = screen.FindChild("PlanetOrbitZoneRow", true, false) as HBoxContainer;
            CheckBox? travellerToggle = travellerRow?.GetChild(1) as CheckBox;
            OptionButton? orbitOption = orbitRow?.GetChild(1) as OptionButton;

            DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Ruleset selector should exist");
            DotNetNativeTestSuite.AssertNotNull(travellerToggle, "Traveller generation toggle should exist");
            DotNetNativeTestSuite.AssertNotNull(orbitOption, "Orbit-zone selector should exist");

            rulesetOption!.Select(1);
            rulesetOption.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
            travellerToggle!.ButtonPressed = true;
            travellerToggle.EmitSignal(CheckBox.SignalName.Toggled, true);
            ObjectGenerationRequest request = screen.GetCurrentRequest();
            DotNetNativeTestSuite.AssertTrue(orbitOption!.GetSelectedId() >= 0, "Traveller mode may promote the orbit-zone picker to a concrete preset value");
            DotNetNativeTestSuite.AssertTrue(request.SpecData.Count > 0, "Traveller request should emit explicit spec data");
            DotNetNativeTestSuite.AssertTrue((int)request.SpecData["orbit_zone"] >= 0, "Traveller generation should resolve a concrete orbit zone before launch");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void SelectOptionById(OptionButton optionButton, int id)
    {
        for (int index = 0; index < optionButton.ItemCount; index++)
        {
            if (optionButton.GetItemId(index) == id)
            {
                optionButton.Select(index);
                optionButton.EmitSignal(OptionButton.SignalName.ItemSelected, (long)index);
                return;
            }
        }

        throw new System.InvalidOperationException($"No option id {id} found");
    }

    private static void Cleanup(string absolutePath)
    {
        if (FileAccess.FileExists(absolutePath))
        {
            DirAccess.RemoveAbsolute(absolutePath);
        }
    }
}
