#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
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
        runner.RunNativeTest("TestStudioScienceUi::test_system_studio_supports_ten_star_cap_and_stellar_controls", TestSystemStudioSupportsTenStarCapAndStellarControls);
    }

    private static void TestGalaxyHelpPopupExistsAndToggles()
    {
        GalaxyGenerationScreen screen = IntegrationTestUtils.InstantiateScene<GalaxyGenerationScreen>("res://src/app/GalaxyGenerationScreen.tscn");
        screen._Ready();

        Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow/HelpButton");
        Window? helpDialog = screen.GetNodeOrNull<Window>("HelpDialog");
        RichTextLabel? helpText = screen.GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpDialogText");
        Button? closeButton = screen.GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/CloseButton");

        DotNetNativeTestSuite.AssertNotNull(helpButton, "Galaxy screen should expose a Help button");
        DotNetNativeTestSuite.AssertNotNull(helpDialog, "Galaxy screen should expose a Help popup window");
        DotNetNativeTestSuite.AssertNotNull(helpText, "Galaxy Help popup should contain scrollable text");
        DotNetNativeTestSuite.AssertNotNull(closeButton, "Galaxy Help popup should expose a Close button");
        DotNetNativeTestSuite.AssertTrue(helpText!.Text.Contains("Grand design"), "Galaxy Help text should explain spiral arm terms");

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
}
