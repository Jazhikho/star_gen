#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.Viewer;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Non-visual scene integration tests for studio help and stellar controls.
/// </summary>
public static class TestStudioScienceUi
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestStudioScienceUi::test_galaxy_help_popup_exists_and_toggles", TestGalaxyHelpPopupExistsAndToggles);
        runner.RunNativeTest("TestStudioScienceUi::test_object_studio_filters_presets_and_traveller_rules_by_context", TestObjectStudioFiltersPresetsAndTravellerRulesByContext);
        runner.RunNativeTest("TestStudioScienceUi::test_system_studio_supports_ten_star_cap_and_stellar_controls", TestSystemStudioSupportsTenStarCapAndStellarControls);
        runner.RunNativeTest("TestStudioScienceUi::test_system_help_popup_exists_and_toggles", TestSystemHelpPopupExistsAndToggles);
    }

    private static void TestGalaxyHelpPopupExistsAndToggles()
    {
        GalaxyGenerationScreen screen = IntegrationTestUtils.InstantiateScene<GalaxyGenerationScreen>("res://src/app/GalaxyGenerationScreen.tscn");
        screen._Ready();

        Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow/HelpButton");
        Window? helpDialog = screen.GetNodeOrNull<Window>("HelpDialog");
        RichTextLabel? helpText = screen.GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        Button? closeButton = screen.GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "Galaxy screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "Galaxy screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "Galaxy Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "Galaxy Help popup should expose a Close button");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "Help popup should open when the Help button is pressed");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "Help popup should close when the Close button is pressed");

        IntegrationTestUtils.CleanupNode(screen);
    }

    private static void TestSystemStudioSupportsTenStarCapAndStellarControls()
    {
        SystemGenerationScreen screen = IntegrationTestUtils.InstantiateScene<SystemGenerationScreen>("res://src/app/SystemGenerationScreen.tscn");
        screen._Ready();

        SpinBox? starCountMinInput = screen.GetNodeOrNull<SpinBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarCountMinRow/StarCountMinInput");
        SpinBox? starCountMaxInput = screen.GetNodeOrNull<SpinBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarCountMaxRow/StarCountMaxInput");
        OptionButton? imfFormOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/ImfFormRow/ImfFormOption");
        OptionButton? isochroneOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/IsochroneRow/IsochroneOption");
        HSlider? multiplicityInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityInput");

        DotNetNativeTestSuite.AssertNotNull(starCountMinInput, "System studio should expose a minimum star-count input");
        DotNetNativeTestSuite.AssertNotNull(starCountMaxInput, "System studio should expose a maximum star-count input");
        DotNetNativeTestSuite.AssertEqual(10.0, starCountMinInput!.MaxValue, "Minimum star-count input should allow up to 10");
        DotNetNativeTestSuite.AssertEqual(10.0, starCountMaxInput!.MaxValue, "Maximum star-count input should allow up to 10");
        DotNetNativeTestSuite.AssertNotNull(imfFormOption, "System studio should expose an IMF selector");
        DotNetNativeTestSuite.AssertNotNull(isochroneOption, "System studio should expose an isochrone selector");
        DotNetNativeTestSuite.AssertNotNull(multiplicityInput, "System studio should expose a multiplicity slider");

        starCountMaxInput.Value = 10.0;
        SolarSystemSpec spec = screen.GetCurrentSpec();
        DotNetNativeTestSuite.AssertEqual(10, spec.StarCountMax, "System studio should build specs that allow up to 10 stars");

        IntegrationTestUtils.CleanupNode(screen);
    }

    private static void TestObjectStudioFiltersPresetsAndTravellerRulesByContext()
    {
        ObjectGenerationScreen screen = IntegrationTestUtils.InstantiateScene<ObjectGenerationScreen>("res://src/app/ObjectGenerationScreen.tscn");
        screen._Ready();

        OptionButton? typeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TypeRow/TypeOption");
        OptionButton? presetOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PresetRow/PresetOption");
        OptionButton? rulesetOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/RulesetRow/RulesetModeOption");
        VBoxContainer? travellerRulesSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/TravellerSection");
        VBoxContainer? strayTravellerParameterSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TravellerSection");
        VBoxContainer? planetSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection");
        VBoxContainer? starSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection");
        VBoxContainer? asteroidSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/AsteroidSection");
        VBoxContainer? cometSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/CometSection");
        HBoxContainer? showTravellerReadoutsRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ShowTravellerReadoutsRow");
        HBoxContainer? planetGenerateMoonRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetGenerateMoonRow");
        HBoxContainer? moonCapturedRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/MoonCapturedRow");
        OptionButton? starSubclassOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection/StarSubclassRow/StarSubclassOption");
        HBoxContainer? starMetallicityRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection/StarMetallicityRow");
        HBoxContainer? starAgeRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection/StarAgeGyrRow");
        OptionButton? asteroidOrbitBandOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/AsteroidSection/AsteroidOrbitBandRow/AsteroidOrbitBandOption");
        OptionButton? asteroidDensityOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/AsteroidSection/AsteroidDensityProfileRow/AsteroidDensityProfileOption");
        OptionButton? cometFamilyOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/CometSection/CometFamilyRow/CometFamilyOption");
        CheckBox? planetGenerateMoonCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetGenerateMoonRow/PlanetGenerateMoonCheck");

        DotNetNativeTestSuite.AssertNotNull(typeOption, "Object studio should expose a type selector");
        DotNetNativeTestSuite.AssertNotNull(presetOption, "Object studio should expose a preset selector");
        DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Object studio should expose a ruleset selector");
        DotNetNativeTestSuite.AssertNotNull(travellerRulesSection, "Traveller controls should live in the Generation Rules panel");
        DotNetNativeTestSuite.AssertNull(strayTravellerParameterSection, "Traveller controls should not remain in the Parameters panel");
        DotNetNativeTestSuite.AssertNotNull(planetSection, "Object studio should expose a planet section");
        DotNetNativeTestSuite.AssertNotNull(starSection, "Object studio should expose a star section");
        DotNetNativeTestSuite.AssertNotNull(asteroidSection, "Object studio should expose an asteroid section");
        DotNetNativeTestSuite.AssertNotNull(cometSection, "Object studio should expose a comet section");
        DotNetNativeTestSuite.AssertNotNull(showTravellerReadoutsRow, "Traveller readout control should exist in the rules panel");
        DotNetNativeTestSuite.AssertNotNull(planetGenerateMoonRow, "Planet controls should include a moon checkbox");
        DotNetNativeTestSuite.AssertNotNull(moonCapturedRow, "Planet controls should include a captured moon row");
        DotNetNativeTestSuite.AssertNotNull(starSubclassOption, "Star controls should expose a subclass selector");
        DotNetNativeTestSuite.AssertNull(starMetallicityRow, "Object studio should not expose star metallicity editing");
        DotNetNativeTestSuite.AssertNull(starAgeRow, "Object studio should not expose star age editing");
        DotNetNativeTestSuite.AssertNotNull(asteroidOrbitBandOption, "Asteroid controls should expose an orbit-band selector");
        DotNetNativeTestSuite.AssertNotNull(asteroidDensityOption, "Asteroid controls should expose a density selector");
        DotNetNativeTestSuite.AssertNotNull(cometFamilyOption, "Comet controls should expose a family selector");

        DotNetNativeTestSuite.AssertTrue(planetSection!.Visible, "Planet controls should be visible for the default planet type");
        DotNetNativeTestSuite.AssertFalse(starSection!.Visible, "Star controls should be hidden for the default planet type");
        DotNetNativeTestSuite.AssertFalse(asteroidSection!.Visible, "Asteroid controls should be hidden for the default planet type");
        DotNetNativeTestSuite.AssertFalse(cometSection!.Visible, "Comet controls should be hidden for the default planet type");
        DotNetNativeTestSuite.AssertFalse(travellerRulesSection!.Visible, "Traveller rules should stay hidden until Traveller generation is selected");
        DotNetNativeTestSuite.AssertFalse(showTravellerReadoutsRow!.Visible, "Traveller readouts should stay hidden until Traveller generation is selected");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption!, "Earth-like"), "Planet presets should be available for planets");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(presetOption, "Sun-like"), "Star presets should not appear while planet type is selected");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(typeOption!, "Moon"), "Moon should not appear as a top-level object type");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(typeOption, "Comet"), "Comet should appear as a top-level object type");
        DotNetNativeTestSuite.AssertFalse(moonCapturedRow!.Visible, "Captured moon row should stay hidden until moon generation is enabled");

        planetGenerateMoonCheck!.ButtonPressed = true;
        planetGenerateMoonCheck.EmitSignal(CheckBox.SignalName.Toggled, true);
        DotNetNativeTestSuite.AssertTrue(moonCapturedRow.Visible, "Captured moon row should appear when moon generation is enabled");
        planetGenerateMoonCheck.ButtonPressed = false;
        planetGenerateMoonCheck.EmitSignal(CheckBox.SignalName.Toggled, false);
        DotNetNativeTestSuite.AssertFalse(moonCapturedRow.Visible, "Captured moon row should hide when moon generation is disabled");

        SelectOptionById(typeOption!, (int)ObjectViewer.ObjectType.Star);
        DotNetNativeTestSuite.AssertFalse(planetSection.Visible, "Planet controls should hide when the user switches to stars");
        DotNetNativeTestSuite.AssertTrue(starSection.Visible, "Star controls should appear when the user switches to stars");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Sun-like"), "Star presets should appear when star type is selected");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(presetOption, "Earth-like"), "Planet presets should disappear when star type is selected");
        DotNetNativeTestSuite.AssertFalse(travellerRulesSection.Visible, "Traveller world-profile controls should stay hidden for non-planet types");

        SelectOptionById(typeOption, (int)ObjectViewer.ObjectType.Asteroid);
        DotNetNativeTestSuite.AssertTrue(asteroidSection.Visible, "Asteroid controls should appear when asteroid type is selected");
        DotNetNativeTestSuite.AssertFalse(planetSection.Visible, "Planet controls should hide when asteroid type is selected");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Dark Red"), "Expanded asteroid presets should appear for asteroids");

        SelectOptionById(typeOption, (int)ObjectViewer.ObjectType.Comet);
        DotNetNativeTestSuite.AssertTrue(cometSection.Visible, "Comet controls should appear when comet type is selected");
        DotNetNativeTestSuite.AssertFalse(asteroidSection.Visible, "Asteroid controls should hide when comet type is selected");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Jupiter-family"), "Comet presets should appear when comet type is selected");

        SelectOptionById(typeOption, (int)ObjectViewer.ObjectType.Planet);
        SelectOptionById(rulesetOption!, (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        DotNetNativeTestSuite.AssertTrue(planetSection.Visible, "Planet controls should reappear when switching back to planets");
        DotNetNativeTestSuite.AssertTrue(travellerRulesSection.Visible, "Traveller world-profile controls should appear for Traveller planet generation");
        DotNetNativeTestSuite.AssertTrue(showTravellerReadoutsRow.Visible, "Traveller readout control should appear for Traveller generation");

        SelectOptionById(rulesetOption, (int)GenerationUseCaseSettings.RulesetModeType.Default);
        DotNetNativeTestSuite.AssertFalse(travellerRulesSection.Visible, "Traveller world-profile controls should hide when leaving Traveller generation");
        DotNetNativeTestSuite.AssertFalse(showTravellerReadoutsRow.Visible, "Traveller readout control should hide when leaving Traveller generation");

        IntegrationTestUtils.CleanupNode(screen);
    }

    private static void TestSystemHelpPopupExistsAndToggles()
    {
        SystemGenerationScreen screen = IntegrationTestUtils.InstantiateScene<SystemGenerationScreen>("res://src/app/SystemGenerationScreen.tscn");
        screen._Ready();

        Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow/HelpButton");
        Window? helpDialog = screen.GetNodeOrNull<Window>("HelpDialog");
        RichTextLabel? helpText = screen.GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        Button? closeButton = screen.GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "System screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "System screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "System Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "System Help popup should expose a Close button");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "System Help popup should open when the Help button is pressed");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "System Help popup should close when the Close button is pressed");

        IntegrationTestUtils.CleanupNode(screen);
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

        throw new System.InvalidOperationException($"OptionButton '{optionButton.Name}' is missing id {id}.");
    }

    private static bool OptionContainsText(OptionButton optionButton, string expectedText)
    {
        for (int index = 0; index < optionButton.ItemCount; index++)
        {
            if (optionButton.GetItemText(index) == expectedText)
            {
                return true;
            }
        }

        return false;
    }
}
