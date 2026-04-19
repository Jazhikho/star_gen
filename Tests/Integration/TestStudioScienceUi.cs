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
        Button? typeSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TypeSection/TypeHeaderRow/TypeSourcesButton");
        Button? scienceSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/ScienceSection/ScienceHeaderRow/ScienceSourcesButton");
        Button? stellarSourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarHeaderRow/StellarSourcesButton");
        Button? planetarySourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryHeaderRow/PlanetarySourcesButton");
        OptionButton? gasGiantFormationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationOption");
        OptionButton? moonBiasOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/MoonFormationBiasRow/MoonFormationBiasOption");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "Galaxy screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "Galaxy screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "Galaxy Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "Galaxy Help popup should expose a Close button");
        DotNetNativeTestSuite.AssertNotNull(typeSourcesButton, "Galaxy Type heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(scienceSourcesButton, "Scientific Priors heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(stellarSourcesButton, "Stellar heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(planetarySourcesButton, "Planetary heading should expose a sources tooltip button");
        DotNetNativeTestSuite.AssertNotNull(gasGiantFormationOption, "Galaxy studio should expose aggregate planetary gas-giant controls");
        DotNetNativeTestSuite.AssertNotNull(moonBiasOption, "Galaxy studio should expose aggregate moon-formation controls");

        DotNetNativeTestSuite.AssertTrue(typeSourcesButton!.TooltipText.Contains("Sources for Galaxy Type"), "Galaxy Type sources tooltip should identify the section");
        DotNetNativeTestSuite.AssertTrue(typeSourcesButton.TooltipText.Contains("Park et al. (2007)"), "Galaxy Type sources tooltip should list galaxy-type references");
        DotNetNativeTestSuite.AssertTrue(scienceSourcesButton!.TooltipText.Contains("Kennicutt (1998)"), "Scientific Priors sources tooltip should list science references");
        DotNetNativeTestSuite.AssertTrue(stellarSourcesButton!.TooltipText.Contains("Kroupa (2001)"), "Stellar sources tooltip should list stellar references");
        DotNetNativeTestSuite.AssertTrue(planetarySourcesButton!.TooltipText.Contains("Chen and Kipping (2017)"), "Planetary sources tooltip should list planetary references");

        helpButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertTrue(helpDialog!.Visible, "Help popup should open when the Help button is pressed");

        closeButton!.EmitSignal(Button.SignalName.Pressed);
        DotNetNativeTestSuite.AssertFalse(helpDialog.Visible, "Help popup should close when the Close button is pressed");

        SelectOptionById(gasGiantFormationOption!, (int)GasGiantFormationModel.PebbleAssisted);
        SelectOptionById(moonBiasOption!, (int)PlanetMoonFormationBias.CapturedRich);
        GalaxyConfig config = screen.GetCurrentConfig();
        DotNetNativeTestSuite.AssertEqual((int)GasGiantFormationModel.PebbleAssisted, (int)config.PlanetaryProfile.GasGiantFormationModel, "Galaxy studio should write the selected gas-giant model into the config");
        DotNetNativeTestSuite.AssertEqual((int)PlanetMoonFormationBias.CapturedRich, (int)config.PlanetaryProfile.MoonFormationBias, "Galaxy studio should write the selected moon-formation bias into the config");

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
        OptionButton? envelopeLossOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/EnvelopeLossRow/EnvelopeLossOption");
        OptionButton? gasGiantFormationOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationOption");
        OptionButton? rogueAllowanceOption = screen.GetNodeOrNull<OptionButton>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox/RogueAllowanceRow/RogueAllowanceOption");

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

        starCountMaxInput.Value = 10.0;
        SelectOptionById(envelopeLossOption!, (int)PlanetEnvelopeLossModel.CorePowered);
        SelectOptionById(gasGiantFormationOption!, (int)GasGiantFormationModel.PebbleAssisted);
        SelectOptionById(rogueAllowanceOption!, (int)PlanetRoguePlanetAllowance.Standard);
        SolarSystemSpec spec = screen.GetCurrentSpec();
        DotNetNativeTestSuite.AssertEqual(10, spec.StarCountMax, "System studio should build specs that allow up to 10 stars");
        DotNetNativeTestSuite.AssertEqual((int)PlanetEnvelopeLossModel.CorePowered, (int)spec.PlanetaryProfile.EnvelopeLossModel, "System studio should write the selected envelope-loss model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)GasGiantFormationModel.PebbleAssisted, (int)spec.PlanetaryProfile.GasGiantFormationModel, "System studio should write the selected gas-giant model into the system spec");
        DotNetNativeTestSuite.AssertEqual((int)PlanetRoguePlanetAllowance.Standard, (int)spec.PlanetaryProfile.RoguePlanetAllowance, "System studio should write the selected rogue allowance into the system spec");

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
        VBoxContainer? strayAggregatePlanetarySection = screen.GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection");
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
        DotNetNativeTestSuite.AssertNull(strayAggregatePlanetarySection, "Object studio should not expose aggregate planetary formation controls");
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
        DotNetNativeTestSuite.AssertFalse(moonTargetCountRow!.Visible, "Moon target-count row should stay hidden until moon generation is enabled");
        DotNetNativeTestSuite.AssertFalse(moonCapturedRow!.Visible, "Captured moon row should stay hidden until moon generation is enabled");
        DotNetNativeTestSuite.AssertTrue(planetOrbitModeRow!.Visible, "Direct planet orbit-mode controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetClassBiasRow!.Visible, "Direct class-bias controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetCompositionBiasRow!.Visible, "Direct composition-bias controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetEnvelopeOverrideRow!.Visible, "Direct envelope controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetVolatileRichnessRow!.Visible, "Direct volatile-richness controls should be visible for planets");
        DotNetNativeTestSuite.AssertTrue(planetHydrosphereTendencyRow!.Visible, "Direct hydrosphere controls should be visible for planets");

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
