#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.Viewer;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Specs;
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
        runner.RunNativeTest("TestStudioScienceUi::test_checkbox_theme_uses_compact_white_box_icons", TestCheckboxThemeUsesCompactWhiteBoxIcons);
        runner.RunNativeTest("TestStudioScienceUi::test_galaxy_help_popup_exists_and_toggles", TestGalaxyHelpPopupExistsAndToggles);
        runner.RunNativeTest("TestStudioScienceUi::test_object_studio_filters_presets_and_traveller_rules_by_context", TestObjectStudioFiltersPresetsAndTravellerRulesByContext);
        runner.RunNativeTest("TestStudioScienceUi::test_object_help_popup_and_planet_life_controls", TestObjectHelpPopupAndPlanetLifeControls);
        runner.RunNativeTest("TestStudioScienceUi::test_system_studio_supports_ten_star_cap_and_science_controls", TestSystemStudioSupportsTenStarCapAndScienceControls);
        runner.RunNativeTest("TestStudioScienceUi::test_system_help_popup_exists_and_toggles", TestSystemHelpPopupExistsAndToggles);
    }

    private static void TestCheckboxThemeUsesCompactWhiteBoxIcons()
    {
        Theme? theme = ResourceLoader.Load<Theme>("res://src/app/themes/DarkTheme.tres");
        DotNetNativeTestSuite.AssertNotNull(theme, "DarkTheme should load for checkbox-style validation");

        StyleBoxFlat? checkBoxNormal = theme!.GetStylebox("normal", "CheckBox") as StyleBoxFlat;
        StyleBoxFlat? checkButtonNormal = theme.GetStylebox("normal", "CheckButton") as StyleBoxFlat;
        DotNetNativeTestSuite.AssertNotNull(checkBoxNormal, "CheckBox should use a flat theme style");
        DotNetNativeTestSuite.AssertNotNull(checkButtonNormal, "CheckButton should use a flat theme style");
        DotNetNativeTestSuite.AssertFalse(checkBoxNormal!.DrawCenter, "CheckBox should not draw a button-like filled background");
        DotNetNativeTestSuite.AssertFalse(checkButtonNormal!.DrawCenter, "CheckButton should not draw a button-like filled background");
        Texture2D? checkBoxUnchecked = theme.GetIcon("unchecked", "CheckBox");
        Texture2D? checkBoxChecked = theme.GetIcon("checked", "CheckBox");
        Texture2D? checkButtonUnchecked = theme.GetIcon("unchecked", "CheckButton");
        Texture2D? checkButtonChecked = theme.GetIcon("checked", "CheckButton");
        DotNetNativeTestSuite.AssertNotNull(checkBoxUnchecked, "CheckBox should expose a visible unchecked icon");
        DotNetNativeTestSuite.AssertNotNull(checkBoxChecked, "CheckBox should expose a visible checked icon");
        DotNetNativeTestSuite.AssertNotNull(checkButtonUnchecked, "CheckButton should expose a visible unchecked icon");
        DotNetNativeTestSuite.AssertNotNull(checkButtonChecked, "CheckButton should expose a visible checked icon");
        DotNetNativeTestSuite.AssertEqual(8, theme.GetConstant("h_separation", "CheckBox"), "CheckBox should keep compact text spacing");
        DotNetNativeTestSuite.AssertEqual(8, theme.GetConstant("h_separation", "CheckButton"), "CheckButton should keep compact text spacing");
    }

    private static void TestGalaxyHelpPopupExistsAndToggles()
    {
        GalaxyGenerationScreen screen = IntegrationTestUtils.InstantiateScene<GalaxyGenerationScreen>("res://src/app/GalaxyGenerationScreen.tscn");
        screen._Ready();

        Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow/HelpButton");
        Window? helpDialog = screen.GetNodeOrNull<Window>("HelpDialog");
        RichTextLabel? helpText = screen.GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        Button? closeButton = screen.GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
        Button? typeSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TypeSection/TypeHeaderRow/TypeSourcesButton");
        Button? scienceSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/ScienceSection/ScienceHeaderRow/ScienceSourcesButton");
        Button? stellarSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarHeaderRow/StellarSourcesButton");
        Button? planetarySourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryHeaderRow/PlanetarySourcesButton");
        Button? lifeSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeHeaderRow/LifeSourcesButton");
        OptionButton? lifeFrameworkOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkOption");
        OptionButton? rulesetModeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/RulesetRow/RulesetModeOption");
        OptionButton? abiogenesisOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelOption");
        OptionButton? complexLifeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelOption");
        OptionButton? civilizationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelOption");
        OptionButton? windowWeightOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
        Label? settingsTitle = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/SettingsTitle");
        Label? rulesTitle = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/RulesTitle");
        CheckBox? showUwpCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/ShowTravellerReadoutsCheck");
        CheckBox? forceLifeCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/ForceLifeOnSupportableWorldsCheck");
        HBoxContainer? mainworldPolicyRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/MainworldPolicyRow");
        OptionButton? mainworldPolicyOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/MainworldPolicyRow/MainworldPolicyOption");
        HBoxContainer? temperateWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/TemperateWorldBiasRow");
        HSlider? temperateWorldBiasSlider = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/TemperateWorldBiasRow/TemperateWorldBiasSlider");
        HBoxContainer? harshWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/HarshWorldBiasRow");
        HSlider? harshWorldBiasSlider = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/HarshWorldBiasRow/HarshWorldBiasSlider");
        HBoxContainer? terrestrialWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/TerrestrialWorldBiasRow");
        HSlider? terrestrialWorldBiasSlider = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/TerrestrialWorldBiasRow/TerrestrialWorldBiasSlider");
        HBoxContainer? nativeLifeBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/NativeLifeBiasRow");
        HSlider? nativeLifeBiasSlider = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/NativeLifeBiasRow/NativeLifeBiasSlider");
        HBoxContainer? settlementBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/PopulationRow");
        HSlider? settlementBiasSlider = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/UseCaseSection/PopulationRow/PopulationPermissivenessInput");
        OptionButton? gasGiantFormationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationOption");
        OptionButton? moonBiasOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/MoonFormationBiasRow/MoonFormationBiasOption");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "Galaxy screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "Galaxy screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "Galaxy Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "Galaxy Help popup should expose a Close button");
        DotNetNativeTestSuite.AssertNotNull(typeSourcesButton, "Galaxy Type heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(scienceSourcesButton, "Scientific Priors heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(lifeSourcesButton, "Life Models heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(stellarSourcesButton, "Stellar heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(planetarySourcesButton, "Planetary heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(lifeFrameworkOption, "Galaxy screen should expose the life-framework selector");
        DotNetNativeTestSuite.AssertNotNull(abiogenesisOption, "Galaxy screen should expose the abiogenesis selector");
        DotNetNativeTestSuite.AssertNotNull(complexLifeOption, "Galaxy screen should expose the complex-life selector");
        DotNetNativeTestSuite.AssertNotNull(civilizationOption, "Galaxy screen should expose the civilization selector");
        DotNetNativeTestSuite.AssertNotNull(windowWeightOption, "Galaxy screen should expose the environmental-window weighting selector");
        DotNetNativeTestSuite.AssertNotNull(settingsTitle, "Galaxy screen should expose the settings title label");
        DotNetNativeTestSuite.AssertNotNull(rulesTitle, "Galaxy screen should expose the rules title label");
        DotNetNativeTestSuite.AssertNotNull(showUwpCheck, "Galaxy screen should expose the UWP code checkbox");
        DotNetNativeTestSuite.AssertNotNull(forceLifeCheck, "Galaxy screen should expose the force-life override checkbox");
        DotNetNativeTestSuite.AssertNotNull(rulesetModeOption, "Galaxy screen should expose the ruleset selector");
        DotNetNativeTestSuite.AssertNotNull(mainworldPolicyRow, "Galaxy screen should define the Space Opera mainworld override row");
        DotNetNativeTestSuite.AssertNotNull(temperateWorldBiasRow, "Galaxy screen should define the Space Opera temperate-world override row");
        DotNetNativeTestSuite.AssertNotNull(harshWorldBiasRow, "Galaxy screen should define the Space Opera harsh-world override row");
        DotNetNativeTestSuite.AssertNotNull(terrestrialWorldBiasRow, "Galaxy screen should define the Space Opera mainworld-class override row");
        DotNetNativeTestSuite.AssertNotNull(nativeLifeBiasRow, "Galaxy screen should define the Space Opera native-life override row");
        DotNetNativeTestSuite.AssertNotNull(settlementBiasRow, "Galaxy screen should define the Space Opera settlement override row");
        DotNetNativeTestSuite.AssertNotNull(gasGiantFormationOption, "Galaxy studio should expose aggregate planetary gas-giant controls");
        DotNetNativeTestSuite.AssertNotNull(moonBiasOption, "Galaxy studio should expose aggregate moon-formation controls");

        DotNetNativeTestSuite.AssertTrue(typeSourcesButton!.TooltipText.Contains("Sources for Galaxy Type"), "Galaxy Type sources tooltip should identify the section");
        DotNetNativeTestSuite.AssertTrue(typeSourcesButton.TooltipText.Contains("Park et al. (2007)"), "Galaxy Type sources tooltip should list galaxy-type references");
        DotNetNativeTestSuite.AssertTrue(scienceSourcesButton!.TooltipText.Contains("Kennicutt (1998)"), "Scientific Priors sources tooltip should list science references");
        DotNetNativeTestSuite.AssertTrue(lifeSourcesButton!.TooltipText.Contains("Lineweaver and Davis (2002)"), "Life sources tooltip should list life-model references");
        DotNetNativeTestSuite.AssertTrue(stellarSourcesButton!.TooltipText.Contains("Kroupa (2001)"), "Stellar sources tooltip should list stellar references");
        DotNetNativeTestSuite.AssertTrue(planetarySourcesButton!.TooltipText.Contains("Chen and Kipping (2017)"), "Planetary sources tooltip should list planetary references");
        DotNetNativeTestSuite.AssertEqual("Scientific Assumptions", settingsTitle!.Text, "Galaxy screen should label the left column as scientific assumptions");
        DotNetNativeTestSuite.AssertEqual("Generation Overrides", rulesTitle!.Text, "Galaxy screen should rename the center column to Generation Overrides");
        DotNetNativeTestSuite.AssertEqual("Show UWP Code", showUwpCheck!.Text, "Galaxy screen should rename Traveller readouts to Show UWP Code");
        DotNetNativeTestSuite.AssertTrue(forceLifeCheck!.TooltipText.Contains("Generation override, not a scientific model."), "Galaxy screen should explain that force life is an override");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(lifeFrameworkOption!, "Earth-Anchored Composite"), "Galaxy screen should expose the Earth-Anchored Composite life framework");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(lifeFrameworkOption, "Rapid Biospheres"), "Galaxy screen should expose the Rapid Biospheres life framework");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption!, "Cepheus"), "Galaxy screen should expose the Cepheus compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption, "Starfinder"), "Galaxy screen should expose the Starfinder compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption, "Starforged"), "Galaxy screen should expose the Starforged compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(abiogenesisOption!, "Rapid Start"), "Galaxy screen should expose the Rapid Start abiogenesis model");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(complexLifeOption!, "Rare Earth Filters"), "Galaxy screen should expose the Rare Earth Filters complex-life model");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(civilizationOption!, "Technosphere Oxygen Bottleneck"), "Galaxy screen should expose the technosphere bottleneck civilization model");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(windowWeightOption!, "High"), "Galaxy screen should expose environmental-window weighting levels");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(lifeFrameworkOption, "Earth History"), "Galaxy screen should not expose the old Earth History label");
        DotNetNativeTestSuite.AssertFalse(mainworldPolicyRow!.Visible, "Galaxy screen should hide Space Opera override rows until Space Opera is selected");
        DotNetNativeTestSuite.AssertFalse(temperateWorldBiasRow!.Visible, "Galaxy screen should hide Space Opera temperate-world bias until Space Opera is selected");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "Help popup should open when the Help button is pressed");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.X <= 700, "Galaxy Help popup should stay narrow enough for smaller windows");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.Y <= 520, "Galaxy Help popup should stay short enough for smaller windows");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "Help popup should close when the Close button is pressed");

        SelectOptionById(rulesetModeOption!, (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        DotNetNativeTestSuite.AssertTrue(mainworldPolicyRow.Visible, "Galaxy screen should show Space Opera override rows when Space Opera is selected");
        DotNetNativeTestSuite.AssertTrue(settlementBiasRow!.Visible, "Galaxy screen should show settlement override rows when Space Opera is selected");
        SelectOptionById(lifeFrameworkOption!, (int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows);
        SelectOptionById(abiogenesisOption!, (int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative);
        SelectOptionById(complexLifeOption!, (int)GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows);
        SelectOptionById(civilizationOption!, (int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck);
        SelectOptionById(windowWeightOption!, (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High);
        SelectOptionById(gasGiantFormationOption!, (int)GasGiantFormationModel.PebbleAssisted);
        SelectOptionById(moonBiasOption!, (int)PlanetMoonFormationBias.CapturedRich);
        forceLifeCheck.ButtonPressed = true;
        forceLifeCheck.EmitSignal(CheckBox.SignalName.Toggled, true);
        SelectOptionById(mainworldPolicyOption!, (int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        temperateWorldBiasSlider!.Value = 1.60;
        harshWorldBiasSlider!.Value = 0.70;
        terrestrialWorldBiasSlider!.Value = 1.40;
        nativeLifeBiasSlider!.Value = 1.25;
        settlementBiasSlider!.Value = 1.55;
        GalaxyConfig config = screen.GetCurrentConfig();
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows, (int)config.UseCaseSettings.LifeFramework, "Galaxy studio should write the selected life framework into the config");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative, (int)config.UseCaseSettings.AbiogenesisModel, "Galaxy studio should write the selected abiogenesis model into the config");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows, (int)config.UseCaseSettings.ComplexLifeModel, "Galaxy studio should write the selected complex-life model into the config");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck, (int)config.UseCaseSettings.CivilizationModel, "Galaxy studio should write the selected civilization model into the config");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High, (int)config.UseCaseSettings.EnvironmentalWindowWeight, "Galaxy studio should write the selected window weight into the config");
        DotNetNativeTestSuite.AssertTrue(config.UseCaseSettings.ForceLifeOnSupportableWorlds, "Galaxy studio should write the force-life override into the config");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.MainworldPolicyType.Require, (int)config.UseCaseSettings.MainworldPolicy, "Galaxy studio should write the selected mainworld override into the config");
        DotNetNativeTestSuite.AssertFloatNear(1.60, config.UseCaseSettings.CompatibilityTemperateSlotFillMultiplier, 0.001, "Galaxy studio should write the selected temperate-world multiplier into the config");
        DotNetNativeTestSuite.AssertFloatNear(0.70, config.UseCaseSettings.CompatibilityHarshSlotFillMultiplier, 0.001, "Galaxy studio should write the selected harsh-world multiplier into the config");
        DotNetNativeTestSuite.AssertFloatNear(1.40, config.UseCaseSettings.CompatibilityTerrestrialWorldWeightMultiplier, 0.001, "Galaxy studio should write the selected mainworld-class multiplier into the config");
        DotNetNativeTestSuite.AssertFloatNear(1.25, config.UseCaseSettings.CompatibilityNativeLifeProbabilityMultiplier, 0.001, "Galaxy studio should write the selected native-life multiplier into the config");
        DotNetNativeTestSuite.AssertFloatNear(1.55, config.UseCaseSettings.CompatibilityColonyProbabilityMultiplier, 0.001, "Galaxy studio should write the selected settlement multiplier into the config");
        DotNetNativeTestSuite.AssertEqual((int)GasGiantFormationModel.PebbleAssisted, (int)config.PlanetaryProfile.GasGiantFormationModel, "Galaxy studio should write the selected gas-giant model into the config");
        DotNetNativeTestSuite.AssertEqual((int)PlanetMoonFormationBias.CapturedRich, (int)config.PlanetaryProfile.MoonFormationBias, "Galaxy studio should write the selected moon-formation bias into the config");

        IntegrationTestUtils.CleanupNode(screen);
    }

    private static void TestSystemStudioSupportsTenStarCapAndScienceControls()
    {
        SystemGenerationScreen screen = IntegrationTestUtils.InstantiateScene<SystemGenerationScreen>("res://src/app/SystemGenerationScreen.tscn");
        screen._Ready();

        SpinBox? starCountMinInput = screen.GetNodeOrNull<SpinBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarCountMinRow/StarCountMinInput");
        SpinBox? starCountMaxInput = screen.GetNodeOrNull<SpinBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarCountMaxRow/StarCountMaxInput");
        OptionButton? imfFormOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/ImfFormRow/ImfFormOption");
        OptionButton? isochroneOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/IsochroneRow/IsochroneOption");
        HSlider? multiplicityInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityInput");
        OptionButton? envelopeLossOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/EnvelopeLossRow/EnvelopeLossOption");
        OptionButton? gasGiantFormationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationOption");
        OptionButton? rogueAllowanceOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/RogueAllowanceRow/RogueAllowanceOption");
        OptionButton? lifeFrameworkOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkOption");
        OptionButton? abiogenesisOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelOption");
        OptionButton? complexLifeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelOption");
        OptionButton? civilizationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelOption");
        OptionButton? windowWeightOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
        OptionButton? rulesetModeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/RulesetModeRow/RulesetModeOption");
        Label? rulesTitle = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/RulesTitle");
        CheckBox? showUwpCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ShowTravellerReadoutsCheck");
        CheckBox? forceLifeCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ForceLifeOnSupportableWorldsCheck");
        HBoxContainer? temperateWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/TemperateWorldBiasRow");
        HSlider? temperateWorldBiasInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/TemperateWorldBiasRow/TemperateWorldBiasInput");
        HBoxContainer? harshWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/HarshWorldBiasRow");
        HSlider? harshWorldBiasInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/HarshWorldBiasRow/HarshWorldBiasInput");
        HBoxContainer? terrestrialWorldBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/TerrestrialWorldBiasRow");
        HSlider? terrestrialWorldBiasInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/TerrestrialWorldBiasRow/TerrestrialWorldBiasInput");
        HBoxContainer? nativeLifeBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/NativeLifeBiasRow");
        HSlider? nativeLifeBiasInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/NativeLifeBiasRow/NativeLifeBiasInput");
        HBoxContainer? populationPermissivenessRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/PopulationPermissivenessRow");
        HSlider? populationPermissivenessInput = screen.GetNodeOrNull<HSlider>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/PopulationPermissivenessRow/PopulationPermissivenessInput");
        HBoxContainer? mainworldPolicyRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/MainworldPolicyRow");
        OptionButton? mainworldPolicyOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/MainworldPolicyRow/MainworldPolicyOption");

        DotNetNativeTestSuite.AssertNotNull(starCountMinInput, "System studio should expose a minimum star-count input");
        DotNetNativeTestSuite.AssertNotNull(starCountMaxInput, "System studio should expose a maximum star-count input");
        DotNetNativeTestSuite.AssertEqual(10.0, starCountMinInput!.MaxValue, "Minimum star-count input should allow up to 10");
        DotNetNativeTestSuite.AssertEqual(10.0, starCountMaxInput!.MaxValue, "Maximum star-count input should allow up to 10");
        DotNetNativeTestSuite.AssertNotNull(imfFormOption, "System studio should expose an IMF selector");
        DotNetNativeTestSuite.AssertNotNull(isochroneOption, "System studio should expose an isochrone selector");
        DotNetNativeTestSuite.AssertNotNull(multiplicityInput, "System studio should expose a multiplicity slider");
        DotNetNativeTestSuite.AssertNotNull(envelopeLossOption, "System studio should expose aggregate planetary envelope-loss controls");
        DotNetNativeTestSuite.AssertNotNull(gasGiantFormationOption, "System studio should expose aggregate gas-giant formation controls");
        DotNetNativeTestSuite.AssertNotNull(rogueAllowanceOption, "System studio should expose aggregate rogue-planet controls");
        DotNetNativeTestSuite.AssertNotNull(lifeFrameworkOption, "System studio should expose the audited life-framework selector");
        DotNetNativeTestSuite.AssertNotNull(abiogenesisOption, "System studio should expose the abiogenesis selector");
        DotNetNativeTestSuite.AssertNotNull(complexLifeOption, "System studio should expose the complex-life selector");
        DotNetNativeTestSuite.AssertNotNull(civilizationOption, "System studio should expose the civilization selector");
        DotNetNativeTestSuite.AssertNotNull(windowWeightOption, "System studio should expose the environmental-window selector");
        DotNetNativeTestSuite.AssertNotNull(rulesetModeOption, "System studio should expose the ruleset selector");
        DotNetNativeTestSuite.AssertNotNull(rulesTitle, "System studio should expose the rules title");
        DotNetNativeTestSuite.AssertNotNull(showUwpCheck, "System studio should expose the Show UWP Code checkbox");
        DotNetNativeTestSuite.AssertNotNull(forceLifeCheck, "System studio should expose the force-life override checkbox");
        DotNetNativeTestSuite.AssertNotNull(temperateWorldBiasRow, "System studio should define the Space Opera temperate-world override row");
        DotNetNativeTestSuite.AssertNotNull(harshWorldBiasRow, "System studio should define the Space Opera harsh-world override row");
        DotNetNativeTestSuite.AssertNotNull(terrestrialWorldBiasRow, "System studio should define the Space Opera mainworld-class override row");
        DotNetNativeTestSuite.AssertNotNull(nativeLifeBiasRow, "System studio should define the Space Opera native-life override row");
        DotNetNativeTestSuite.AssertNotNull(populationPermissivenessRow, "System studio should define the Space Opera settlement override row");
        DotNetNativeTestSuite.AssertNotNull(mainworldPolicyRow, "System studio should define the Space Opera mainworld-policy row");
        DotNetNativeTestSuite.AssertEqual("Generation Overrides", rulesTitle!.Text, "System studio should name the center column Generation Overrides");
        DotNetNativeTestSuite.AssertEqual("Show UWP Code", showUwpCheck!.Text, "System studio should use the Show UWP Code label");
        DotNetNativeTestSuite.AssertTrue(forceLifeCheck!.TooltipText.Contains("Generation override, not a scientific model."), "System studio should explain that force life is an override");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(lifeFrameworkOption!, "Earth-Anchored Composite"), "System studio should expose the Earth-Anchored Composite life framework");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(lifeFrameworkOption, "Earth History"), "System studio should not expose the old Earth History label");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption!, "Cepheus"), "System studio should expose the Cepheus compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption, "Starfinder"), "System studio should expose the Starfinder compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetModeOption, "Starforged"), "System studio should expose the Starforged compatibility profile");
        DotNetNativeTestSuite.AssertFalse(temperateWorldBiasRow!.Visible, "System studio should hide Space Opera override rows until Space Opera is selected");
        DotNetNativeTestSuite.AssertFalse(mainworldPolicyRow!.Visible, "System studio should hide Space Opera mainworld-policy rows until Space Opera is selected");

        starCountMaxInput.Value = 10.0;
        SelectOptionById(envelopeLossOption!, (int)PlanetEnvelopeLossModel.CorePowered);
        SelectOptionById(gasGiantFormationOption!, (int)GasGiantFormationModel.PebbleAssisted);
        SelectOptionById(rogueAllowanceOption!, (int)PlanetRoguePlanetAllowance.Standard);
        SelectOptionById(rulesetModeOption!, (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        DotNetNativeTestSuite.AssertTrue(temperateWorldBiasRow.Visible, "System studio should show Space Opera override rows when Space Opera is selected");
        DotNetNativeTestSuite.AssertTrue(mainworldPolicyRow.Visible, "System studio should show Space Opera mainworld-policy rows when Space Opera is selected");
        SelectOptionById(lifeFrameworkOption!, (int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows);
        SelectOptionById(abiogenesisOption!, (int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative);
        SelectOptionById(complexLifeOption!, (int)GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows);
        SelectOptionById(civilizationOption!, (int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck);
        SelectOptionById(windowWeightOption!, (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High);
        SelectOptionById(mainworldPolicyOption!, (int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        temperateWorldBiasInput!.Value = 1.58;
        harshWorldBiasInput!.Value = 0.72;
        terrestrialWorldBiasInput!.Value = 1.36;
        nativeLifeBiasInput!.Value = 1.18;
        populationPermissivenessInput!.Value = 1.48;
        forceLifeCheck.ButtonPressed = true;
        forceLifeCheck.EmitSignal(CheckBox.SignalName.Toggled, true);
        SolarSystemSpec spec = screen.GetCurrentSpec();
        DotNetNativeTestSuite.AssertEqual(10, spec.StarCountMax, "System studio should build specs that allow up to 10 stars");
        DotNetNativeTestSuite.AssertEqual((int)PlanetEnvelopeLossModel.CorePowered, (int)spec.PlanetaryProfile.EnvelopeLossModel, "System studio should write the selected envelope-loss model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GasGiantFormationModel.PebbleAssisted, (int)spec.PlanetaryProfile.GasGiantFormationModel, "System studio should write the selected gas-giant model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetRoguePlanetAllowance.Standard, (int)spec.PlanetaryProfile.RoguePlanetAllowance, "System studio should write the selected rogue allowance into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows, (int)spec.UseCaseSettings.LifeFramework, "System studio should write the selected life framework into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative, (int)spec.UseCaseSettings.AbiogenesisModel, "System studio should write the selected abiogenesis model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows, (int)spec.UseCaseSettings.ComplexLifeModel, "System studio should write the selected complex-life model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck, (int)spec.UseCaseSettings.CivilizationModel, "System studio should write the selected civilization model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High, (int)spec.UseCaseSettings.EnvironmentalWindowWeight, "System studio should write the selected environmental-window weight into the system spec");
        DotNetNativeTestSuite.AssertTrue(spec.UseCaseSettings.ForceLifeOnSupportableWorlds, "System studio should write the force-life override into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.MainworldPolicyType.Require, (int)spec.UseCaseSettings.MainworldPolicy, "System studio should write the selected mainworld override into the system spec");
        DotNetNativeTestSuite.AssertFloatNear(1.58, spec.UseCaseSettings.CompatibilityTemperateSlotFillMultiplier, 0.001, "System studio should write the selected temperate-world multiplier into the system spec");
        DotNetNativeTestSuite.AssertFloatNear(0.72, spec.UseCaseSettings.CompatibilityHarshSlotFillMultiplier, 0.001, "System studio should write the selected harsh-world multiplier into the system spec");
        DotNetNativeTestSuite.AssertFloatNear(1.36, spec.UseCaseSettings.CompatibilityTerrestrialWorldWeightMultiplier, 0.001, "System studio should write the selected mainworld-class multiplier into the system spec");
        DotNetNativeTestSuite.AssertFloatNear(1.18, spec.UseCaseSettings.CompatibilityNativeLifeProbabilityMultiplier, 0.001, "System studio should write the selected native-life multiplier into the system spec");
        DotNetNativeTestSuite.AssertFloatNear(1.48, spec.UseCaseSettings.CompatibilityColonyProbabilityMultiplier, 0.001, "System studio should write the selected settlement multiplier into the system spec");

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
        Label? rulesTitle = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/RulesTitle");
        HBoxContainer? showTravellerReadoutsRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ShowTravellerReadoutsRow");
        Label? showTravellerReadoutsLabel = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ShowTravellerReadoutsRow/ShowTravellerReadoutsRowLabel");
        CheckBox? showTravellerReadoutsCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/ShowTravellerReadoutsRow/ShowTravellerReadoutsCheck");
        HBoxContainer? planetGenerateMoonRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetGenerateMoonRow");
        HBoxContainer? moonTargetCountRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/MoonTargetCountRow");
        HBoxContainer? moonCapturedRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/MoonCapturedRow");
        OptionButton? moonTargetCountOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/MoonTargetCountRow/MoonTargetCountOption");
        HBoxContainer? planetOrbitModeRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetOrbitModeRow");
        HBoxContainer? planetClassBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetClassBiasRow");
        HBoxContainer? planetCompositionBiasRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetCompositionBiasRow");
        HBoxContainer? planetEnvelopeOverrideRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetEnvelopeOverrideRow");
        HBoxContainer? planetVolatileRichnessRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetVolatileRichnessRow");
        HBoxContainer? planetHydrosphereTendencyRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetHydrosphereTendencyRow");
        OptionButton? planetOrbitModeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetOrbitModeRow/PlanetOrbitModeOption");
        OptionButton? planetClassBiasOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetClassBiasRow/PlanetClassBiasOption");
        OptionButton? planetCompositionBiasOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetCompositionBiasRow/PlanetCompositionBiasOption");
        OptionButton? planetEnvelopeOverrideOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetEnvelopeOverrideRow/PlanetEnvelopeOverrideOption");
        OptionButton? planetVolatileRichnessOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetVolatileRichnessRow/PlanetVolatileRichnessOption");
        OptionButton? planetHydrosphereTendencyOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetHydrosphereTendencyRow/PlanetHydrosphereTendencyOption");
        VBoxContainer? strayAggregatePlanetarySection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection");
        HBoxContainer? strayLifePermissivenessRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/LifePermissivenessRow");
        HBoxContainer? strayPopulationPermissivenessRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent/PopulationPermissivenessRow");
        OptionButton? starSubclassOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection/StarSubclassRow/StarSubclassOption");
        OptionButton? starSpectralClassOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection/StarSpectralClassRow/StarSpectralClassOption");
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
        DotNetNativeTestSuite.AssertNotNull(rulesTitle, "Object studio should expose the rules-panel title");
        DotNetNativeTestSuite.AssertNotNull(showTravellerReadoutsRow, "Traveller readout control should exist in the rules panel");
        DotNetNativeTestSuite.AssertNotNull(showTravellerReadoutsLabel, "Object studio should expose the Show UWP Code row label");
        DotNetNativeTestSuite.AssertNotNull(showTravellerReadoutsCheck, "Object studio should expose the Show UWP Code checkbox");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetOption!, "Cepheus"), "Object studio should expose the Cepheus compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetOption, "Starfinder"), "Object studio should expose the Starfinder compatibility profile");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(rulesetOption, "Starforged"), "Object studio should expose the Starforged compatibility profile");
        DotNetNativeTestSuite.AssertNotNull(planetGenerateMoonRow, "Planet controls should include a moon checkbox");
        DotNetNativeTestSuite.AssertNotNull(moonTargetCountRow, "Planet controls should include a moon target-count row");
        DotNetNativeTestSuite.AssertNotNull(moonCapturedRow, "Planet controls should include a captured moon row");
        DotNetNativeTestSuite.AssertNotNull(moonTargetCountOption, "Planet controls should expose a moon target-count selector");
        DotNetNativeTestSuite.AssertNotNull(planetOrbitModeRow, "Object studio should expose a direct orbit-mode row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetClassBiasRow, "Object studio should expose a direct class-bias row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetCompositionBiasRow, "Object studio should expose a direct composition-bias row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetEnvelopeOverrideRow, "Object studio should expose a direct envelope row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetVolatileRichnessRow, "Object studio should expose a direct volatile-richness row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetHydrosphereTendencyRow, "Object studio should expose a direct hydrosphere row for planets");
        DotNetNativeTestSuite.AssertNotNull(planetOrbitModeOption, "Object studio should expose a direct orbit-mode selector for planets");
        DotNetNativeTestSuite.AssertNotNull(planetClassBiasOption, "Object studio should expose a direct class-bias selector for planets");
        DotNetNativeTestSuite.AssertNotNull(planetCompositionBiasOption, "Object studio should expose a direct composition-bias selector for planets");
        DotNetNativeTestSuite.AssertNotNull(planetEnvelopeOverrideOption, "Object studio should expose a direct envelope selector for planets");
        DotNetNativeTestSuite.AssertNotNull(planetVolatileRichnessOption, "Object studio should expose a direct volatile-richness selector for planets");
        DotNetNativeTestSuite.AssertNotNull(planetHydrosphereTendencyOption, "Object studio should expose a direct hydrosphere selector for planets");
        DotNetNativeTestSuite.AssertNull(strayAggregatePlanetarySection, "Object studio should not expose aggregate planetary formation controls");
        DotNetNativeTestSuite.AssertNull(strayLifePermissivenessRow, "Object studio should not expose aggregate life sliders");
        DotNetNativeTestSuite.AssertNull(strayPopulationPermissivenessRow, "Object studio should not expose aggregate population sliders");
        DotNetNativeTestSuite.AssertNotNull(starSubclassOption, "Star controls should expose a subclass selector");
        DotNetNativeTestSuite.AssertNotNull(starSpectralClassOption, "Star controls should expose a spectral-class selector");
        DotNetNativeTestSuite.AssertNull(starMetallicityRow, "Object studio should not expose star metallicity editing");
        DotNetNativeTestSuite.AssertNull(starAgeRow, "Object studio should not expose star age editing");
        DotNetNativeTestSuite.AssertNotNull(asteroidOrbitBandOption, "Asteroid controls should expose an orbit-band selector");
        DotNetNativeTestSuite.AssertNotNull(asteroidDensityOption, "Asteroid controls should expose a density selector");
        DotNetNativeTestSuite.AssertNotNull(cometFamilyOption, "Comet controls should expose a family selector");
        DotNetNativeTestSuite.AssertEqual("Generation Overrides", rulesTitle!.Text, "Object studio should rename the center column to Generation Overrides");
        DotNetNativeTestSuite.AssertEqual("Show UWP Code", showTravellerReadoutsLabel!.Text, "Object studio should rename Traveller readouts to Show UWP Code");

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
        DotNetNativeTestSuite.AssertFalse(moonTargetCountRow!.Visible, "Moon target-count row should stay hidden until moon generation is enabled");
        DotNetNativeTestSuite.AssertFalse(moonCapturedRow!.Visible, "Captured moon row should stay hidden until moon generation is enabled");
        DotNetNativeTestSuite.AssertTrue(planetOrbitModeRow!.Visible, "Direct planet orbit-mode controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetClassBiasRow!.Visible, "Direct class-bias controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetCompositionBiasRow!.Visible, "Direct composition-bias controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetEnvelopeOverrideRow!.Visible, "Direct envelope controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetVolatileRichnessRow!.Visible, "Direct volatile-richness controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetHydrosphereTendencyRow!.Visible, "Direct hydrosphere controls should be visible for planets");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(planetOrbitModeOption!.TooltipText), "Planet orbit-mode selector should explain its direct effect");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(planetClassBiasOption!.TooltipText), "Planet class-bias selector should explain its direct effect");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(planetCompositionBiasOption!.TooltipText), "Planet composition-bias selector should explain its direct effect");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(planetEnvelopeOverrideOption!.TooltipText), "Planet envelope selector should explain its direct effect");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(moonTargetCountOption!.TooltipText), "Moon target-count selector should explain its direct effect");

        SelectOptionById(planetOrbitModeOption!, (int)PlanetOrbitMode.Rogue);
        HBoxContainer? planetOrbitZoneRow = screen.GetNodeOrNull<HBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetOrbitZoneRow");
        DotNetNativeTestSuite.AssertNotNull(planetOrbitZoneRow, "Object studio should still define an orbit-zone row");
        DotNetNativeTestSuite.AssertFalse(planetOrbitZoneRow!.Visible, "Orbit-zone controls should hide for rogue planets because they no longer orbit a star");
        SelectOptionById(planetOrbitModeOption, (int)PlanetOrbitMode.Bound);

        planetGenerateMoonCheck!.ButtonPressed = true;
        planetGenerateMoonCheck.EmitSignal(CheckBox.SignalName.Toggled, true);
        DotNetNativeTestSuite.AssertTrue(moonTargetCountRow.Visible, "Moon target-count row should appear when moon generation is enabled");
        DotNetNativeTestSuite.AssertTrue(moonCapturedRow.Visible, "Captured moon row should appear when moon generation is enabled");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(moonTargetCountOption!, "12"), "Moon target-count selector should expose a higher target range");
        SelectOptionById(planetClassBiasOption!, (int)PlanetClassBias.StrippedCore);
        SelectOptionById(planetCompositionBiasOption!, (int)PlanetCompositionBias.IcyWaterRich);
        SelectOptionById(planetEnvelopeOverrideOption!, (int)PlanetEnvelopeOverride.Stripped);
        SelectOptionById(planetVolatileRichnessOption!, (int)PlanetVolatileRichness.Rich);
        SelectOptionById(planetHydrosphereTendencyOption!, (int)PlanetHydrosphereTendency.Oceanic);
        SelectOptionById(moonTargetCountOption!, 5);
        moonCapturedRow.GetNode<CheckBox>("MoonCapturedCheck").ButtonPressed = true;
        ObjectGenerationRequest planetRequest = screen.GetCurrentRequest();
        PlanetSpec planetSpec = PlanetSpec.FromDictionary(planetRequest.SpecData);
        DotNetNativeTestSuite.AssertEqual((int)PlanetOrbitMode.Bound, (int)planetSpec.OrbitMode, "Object studio should write direct orbit mode into the planet spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetClassBias.StrippedCore, (int)planetSpec.ClassBias, "Object studio should write direct class bias into the planet spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetCompositionBias.IcyWaterRich, (int)planetSpec.CompositionBias, "Object studio should write direct composition bias into the planet spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetEnvelopeOverride.Stripped, (int)planetSpec.EnvelopeOverride, "Object studio should write direct envelope overrides into the planet spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetVolatileRichness.Rich, (int)planetSpec.VolatileRichness, "Object studio should write direct volatile-richness overrides into the planet spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetHydrosphereTendency.Oceanic, (int)planetSpec.HydrosphereTendency, "Object studio should write direct hydrosphere overrides into the planet spec");
        DotNetNativeTestSuite.AssertTrue(planetSpec.GenerateMoonBundle, "Object studio should write direct moon generation into the planet spec");
        DotNetNativeTestSuite.AssertEqual(5, planetSpec.TargetMoonCount, "Object studio should write the requested moon target count into the planet spec");
        DotNetNativeTestSuite.AssertTrue(planetSpec.PreferCapturedMoons, "Object studio should write the captured-moon preference into the planet spec");

        planetGenerateMoonCheck.ButtonPressed = false;
        planetGenerateMoonCheck.EmitSignal(CheckBox.SignalName.Toggled, false);
        DotNetNativeTestSuite.AssertFalse(moonTargetCountRow.Visible, "Moon target-count row should hide when moon generation is disabled");
        DotNetNativeTestSuite.AssertFalse(moonCapturedRow.Visible, "Captured moon row should hide when moon generation is disabled");

        SelectOptionById(typeOption!, (int)ObjectViewer.ObjectType.Star);
        DotNetNativeTestSuite.AssertFalse(planetSection.Visible, "Planet controls should hide when the user switches to stars");
        DotNetNativeTestSuite.AssertTrue(starSection.Visible, "Star controls should appear when the user switches to stars");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Sun-like"), "Star presets should appear when star type is selected");
        DotNetNativeTestSuite.AssertFalse(OptionContainsText(presetOption, "Earth-like"), "Planet presets should disappear when star type is selected");
        DotNetNativeTestSuite.AssertFalse(travellerRulesSection.Visible, "Traveller world-profile controls should stay hidden for non-planet types");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(starSpectralClassOption!.TooltipText), "Star spectral-class selector should explain its direct effect");

        SelectOptionById(typeOption, (int)ObjectViewer.ObjectType.Asteroid);
        DotNetNativeTestSuite.AssertTrue(asteroidSection.Visible, "Asteroid controls should appear when asteroid type is selected");
        DotNetNativeTestSuite.AssertFalse(planetSection.Visible, "Planet controls should hide when asteroid type is selected");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Dark Red"), "Expanded asteroid presets should appear for asteroids");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(asteroidOrbitBandOption!.TooltipText), "Asteroid orbit-band selector should explain its direct effect");

        SelectOptionById(typeOption, (int)ObjectViewer.ObjectType.Comet);
        DotNetNativeTestSuite.AssertTrue(cometSection.Visible, "Comet controls should appear when comet type is selected");
        DotNetNativeTestSuite.AssertFalse(asteroidSection.Visible, "Asteroid controls should hide when comet type is selected");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(presetOption, "Jupiter-family"), "Comet presets should appear when comet type is selected");
        DotNetNativeTestSuite.AssertFalse(string.IsNullOrWhiteSpace(cometFamilyOption!.TooltipText), "Comet family selector should explain its direct effect");

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
        Label? settingsTitle = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/SettingsTitle");
        Button? systemSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/SystemHeaderRow/SystemSourcesButton");
        Button? stellarSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarHeaderRow/StellarSourcesButton");
        Button? planetarySourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryHeaderRow/PlanetarySourcesButton");
        Button? lifeSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeHeaderRow/LifeSourcesButton");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "System screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "System screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "System Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "System Help popup should expose a Close button");
        DotNetNativeTestSuite.AssertNotNull(settingsTitle, "System screen should expose the scientific assumptions title");
        DotNetNativeTestSuite.AssertNotNull(systemSourcesButton, "System screen should expose a system-controls sources button");
        DotNetNativeTestSuite.AssertNotNull(stellarSourcesButton, "System screen should expose a stellar sources button");
        DotNetNativeTestSuite.AssertNotNull(planetarySourcesButton, "System screen should expose a planetary sources button");
        DotNetNativeTestSuite.AssertNotNull(lifeSourcesButton, "System screen should expose a life sources button");
        DotNetNativeTestSuite.AssertEqual("Scientific Assumptions", settingsTitle!.Text, "System screen should label the left column as scientific assumptions");
        DotNetNativeTestSuite.AssertTrue(systemSourcesButton!.TooltipText.Contains("deterministic generator controls"), "System controls sources tooltip should explain the mixed deterministic/system-target section");
        DotNetNativeTestSuite.AssertTrue(stellarSourcesButton!.TooltipText.Contains("Kroupa (2001)"), "System stellar sources tooltip should list stellar references");
        DotNetNativeTestSuite.AssertTrue(planetarySourcesButton!.TooltipText.Contains("Chen and Kipping (2017)"), "System planetary sources tooltip should list planetary references");
        DotNetNativeTestSuite.AssertTrue(lifeSourcesButton!.TooltipText.Contains("Lineweaver and Davis (2002)"), "System life sources tooltip should list life references");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "System Help popup should open when the Help button is pressed");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.X <= 700, "System Help popup should stay narrow enough for smaller windows");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.Y <= 520, "System Help popup should stay short enough for smaller windows");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Position.X >= 0, "System Help popup should stay on-screen horizontally");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Position.Y >= 0, "System Help popup should stay on-screen vertically");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "System Help popup should close when the Close button is pressed");

        IntegrationTestUtils.CleanupNode(screen);
    }

    private static void TestObjectHelpPopupAndPlanetLifeControls()
    {
        ObjectGenerationScreen screen = IntegrationTestUtils.InstantiateScene<ObjectGenerationScreen>("res://src/app/ObjectGenerationScreen.tscn");
        screen._Ready();

        Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow/HelpButton");
        Window? helpDialog = screen.GetNodeOrNull<Window>("HelpDialog");
        RichTextLabel? helpText = screen.GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        Button? closeButton = screen.GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
        VBoxContainer? lifeSection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection");
        OptionButton? typeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TypeRow/TypeOption");
        OptionButton? lifeFrameworkOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkOption");
        OptionButton? abiogenesisOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelOption");
        OptionButton? complexLifeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelOption");
        OptionButton? civilizationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelOption");
        OptionButton? windowWeightOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
        CheckBox? forceLifeCheck = screen.GetNodeOrNull<CheckBox>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox/ForceLifeOnSupportableWorldsRow/ForceLifeOnSupportableWorldsCheck");
        OptionButton? atmosphereOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetAtmosphereRow/PlanetAtmosphereOption");
        OptionButton? envelopeOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetEnvelopeOverrideRow/PlanetEnvelopeOverrideOption");
        OptionButton? hydrosphereOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetHydrosphereTendencyRow/PlanetHydrosphereTendencyOption");
        OptionButton? pressureOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection/PlanetSurfacePressureRow/PlanetSurfacePressureOption");
        VBoxContainer? issuesContainer = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/SummaryScroll/SummaryContent/IssuesContainer");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "Object studio should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "Object studio should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "Object studio Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "Object studio Help popup should expose a Close button");
        DotNetNativeTestSuite.AssertNotNull(lifeSection, "Object studio should expose a planet life section");
        DotNetNativeTestSuite.AssertNotNull(lifeFrameworkOption, "Object studio should expose a life framework selector");
        DotNetNativeTestSuite.AssertNotNull(abiogenesisOption, "Object studio should expose an abiogenesis selector");
        DotNetNativeTestSuite.AssertNotNull(complexLifeOption, "Object studio should expose a complex-life selector");
        DotNetNativeTestSuite.AssertNotNull(civilizationOption, "Object studio should expose a civilization selector");
        DotNetNativeTestSuite.AssertNotNull(windowWeightOption, "Object studio should expose an environmental-window selector");
        DotNetNativeTestSuite.AssertNotNull(forceLifeCheck, "Object studio should expose a direct force-life override");
        DotNetNativeTestSuite.AssertNotNull(atmosphereOption, "Object studio should expose a direct atmosphere selector");
        DotNetNativeTestSuite.AssertNotNull(envelopeOption, "Object studio should expose a direct envelope selector");
        DotNetNativeTestSuite.AssertNotNull(hydrosphereOption, "Object studio should expose a direct hydrosphere selector");
        DotNetNativeTestSuite.AssertNotNull(pressureOption, "Object studio should expose a direct surface-pressure selector");
        DotNetNativeTestSuite.AssertNotNull(issuesContainer, "Object studio should expose an issues container");

        DotNetNativeTestSuite.AssertTrue(lifeSection!.Visible, "Object studio life settings should be visible for planet authoring");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(lifeFrameworkOption!, "Earth-Anchored Composite"), "Object studio should expose Earth-Anchored Composite");
        DotNetNativeTestSuite.AssertTrue(OptionContainsText(civilizationOption!, "Technosphere Oxygen Bottleneck"), "Object studio should expose the technosphere bottleneck option");
        DotNetNativeTestSuite.AssertTrue(forceLifeCheck!.TooltipText.Contains("Generation override, not a scientific model."), "Object studio should explain that force life is an override");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "Object studio Help popup should open when the Help button is pressed");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.X <= 700, "Object studio Help popup should stay narrow enough for smaller windows");
        DotNetNativeTestSuite.AssertTrue(helpDialog.Size.Y <= 520, "Object studio Help popup should stay short enough for smaller windows");
        DotNetNativeTestSuite.AssertTrue(helpText!.Text.Contains("Object Studio"), "Object studio help should explain the direct-authoring surface");
        DotNetNativeTestSuite.AssertTrue(helpText.Text.Contains("Wordsworth"), "Object studio help should include the conflict-note science sources");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "Object studio Help popup should close when the Close button is pressed");

        SelectOptionById(lifeFrameworkOption!, (int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows);
        SelectOptionById(abiogenesisOption!, (int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative);
        SelectOptionById(complexLifeOption!, (int)GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters);
        SelectOptionById(civilizationOption!, (int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck);
        SelectOptionById(windowWeightOption!, (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High);
        forceLifeCheck.ButtonPressed = true;
        forceLifeCheck.EmitSignal(CheckBox.SignalName.Toggled, true);

        ObjectGenerationRequest request = screen.GetCurrentRequest();
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows, (int)request.UseCaseSettings.LifeFramework, "Object studio should write the selected life framework into the request");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative, (int)request.UseCaseSettings.AbiogenesisModel, "Object studio should write the selected abiogenesis model into the request");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters, (int)request.UseCaseSettings.ComplexLifeModel, "Object studio should write the selected complex-life model into the request");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck, (int)request.UseCaseSettings.CivilizationModel, "Object studio should write the selected civilization model into the request");
        DotNetNativeTestSuite.AssertEqual((int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High, (int)request.UseCaseSettings.EnvironmentalWindowWeight, "Object studio should write the selected environmental-window weight into the request");
        DotNetNativeTestSuite.AssertTrue(request.UseCaseSettings.ForceLifeOnSupportableWorlds, "Object studio should write the direct force-life override into the request");

        SelectOptionById(atmosphereOption!, 0);
        SelectOptionById(envelopeOption!, (int)PlanetEnvelopeOverride.Stripped);
        SelectOptionById(hydrosphereOption!, (int)PlanetHydrosphereTendency.Oceanic);
        SelectOptionById(pressureOption!, 2);

        DotNetNativeTestSuite.AssertTrue(ContainerHasLabelText(issuesContainer!, "Airless planets"), "Object studio should flag the airless-versus-ocean conflict");
        DotNetNativeTestSuite.AssertTrue(ContainerHasLabelText(issuesContainer, "Technosphere Oxygen Bottleneck"), "Object studio should flag the late civilization bottleneck conflict");

        Label? airlessConflict = FindLabelByTextFragment(issuesContainer, "Airless planets");
        Label? bottleneckConflict = FindLabelByTextFragment(issuesContainer, "Technosphere Oxygen Bottleneck");
        DotNetNativeTestSuite.AssertNotNull(airlessConflict, "Object studio should create a label for the atmosphere conflict");
        DotNetNativeTestSuite.AssertNotNull(bottleneckConflict, "Object studio should create a label for the civilization conflict");
        DotNetNativeTestSuite.AssertTrue(airlessConflict!.TooltipText.Contains("Wordsworth and Kreidberg (2022)"), "Atmosphere conflict note should cite Wordsworth and Kreidberg (2022)");
        DotNetNativeTestSuite.AssertTrue(bottleneckConflict!.TooltipText.Contains("Balbi and Frank (2023)"), "Civilization conflict note should cite Balbi and Frank (2023)");

        SelectOptionById(typeOption!, (int)ObjectViewer.ObjectType.Star);
        DotNetNativeTestSuite.AssertFalse(lifeSection.Visible, "Object studio life settings should hide when the user switches away from planets");

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

    private static bool ContainerHasLabelText(VBoxContainer container, string fragment)
    {
        return FindLabelByTextFragment(container, fragment) != null;
    }

    private static Label? FindLabelByTextFragment(VBoxContainer container, string fragment)
    {
        foreach (Node child in container.GetChildren())
        {
            if (child is Label label && label.Text.Contains(fragment))
            {
                return label;
            }
        }

        return null;
    }
}
