#nullable enable annotations
#nullable disable warnings
using Godot;
using System.Reflection;
using StarGen.App;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestMainMenuScreen
{
    private const string ScenePath = "res://src/app/MainMenuScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestMainMenuScreen::test_mode_buttons_emit_navigation_signals", TestModeButtonsEmitNavigationSignals);
        runner.RunNativeTest("TestMainMenuScreen::test_concept_atlas_button_emits_signal", TestConceptAtlasButtonEmitsSignal);
        runner.RunNativeTest("TestMainMenuScreen::test_utility_buttons_open_fallback_dialogs", TestUtilityButtonsOpenFallbackDialogs);
        runner.RunNativeTest("TestMainMenuScreen::test_help_credits_and_sources_copy_are_user_facing", TestHelpCreditsAndSourcesCopyAreUserFacing);
        runner.RunNativeTest("TestMainMenuScreen::test_main_menu_uses_user_facing_version_label", TestMainMenuUsesUserFacingVersionLabel);
    }

    private static void TestModeButtonsEmitNavigationSignals()
    {
        MainMenuScreen screen = IntegrationTestUtils.InstantiateScene<MainMenuScreen>(ScenePath);
        try
        {
            screen._Ready();

            bool galaxyRequested = false;
            bool systemRequested = false;
            bool objectRequested = false;
            bool stationRequested = false;

            screen.Connect(MainMenuScreen.SignalName.galaxy_generation_requested, Callable.From(() => galaxyRequested = true));
            screen.Connect(MainMenuScreen.SignalName.system_generation_requested, Callable.From(() => systemRequested = true));
            screen.Connect(MainMenuScreen.SignalName.object_generation_requested, Callable.From(() => objectRequested = true));
            screen.Connect(MainMenuScreen.SignalName.station_generation_requested, Callable.From(() => stationRequested = true));

            Button? galaxyButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardGalaxy/MarginContainer/VBoxContainer/GalaxyButton");
            Button? systemButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardSystem/MarginContainer/VBoxContainer/SystemButton");
            Button? objectButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardObject/MarginContainer/VBoxContainer/ObjectButton");
            Button? stationButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardStation/MarginContainer/VBoxContainer/StationButton");

            DotNetNativeTestSuite.AssertNotNull(galaxyButton, "Galaxy button should exist");
            DotNetNativeTestSuite.AssertNotNull(systemButton, "System button should exist");
            DotNetNativeTestSuite.AssertNotNull(objectButton, "Object button should exist");
            DotNetNativeTestSuite.AssertNotNull(stationButton, "Station button should exist");

            galaxyButton!.EmitSignal(BaseButton.SignalName.Pressed);
            systemButton!.EmitSignal(BaseButton.SignalName.Pressed);
            objectButton!.EmitSignal(BaseButton.SignalName.Pressed);
            stationButton!.EmitSignal(BaseButton.SignalName.Pressed);

            DotNetNativeTestSuite.AssertTrue(galaxyRequested, "Galaxy button should emit galaxy_generation_requested");
            DotNetNativeTestSuite.AssertTrue(systemRequested, "System button should emit system_generation_requested");
            DotNetNativeTestSuite.AssertTrue(objectRequested, "Object button should emit object_generation_requested");
            DotNetNativeTestSuite.AssertTrue(stationRequested, "Station button should emit station_generation_requested");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestConceptAtlasButtonEmitsSignal()
    {
        MainMenuScreen screen = IntegrationTestUtils.InstantiateScene<MainMenuScreen>(ScenePath);
        try
        {
            screen._Ready();

            bool atlasRequested = false;
            screen.Connect(MainMenuScreen.SignalName.concept_atlas_requested, Callable.From(() => atlasRequested = true));

            Button? atlasButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardConceptAtlas/MarginContainer/VBoxContainer/ConceptAtlasButton");
            DotNetNativeTestSuite.AssertNotNull(atlasButton, "Concept Atlas button should exist");

            atlasButton!.EmitSignal(BaseButton.SignalName.Pressed);
            DotNetNativeTestSuite.AssertTrue(atlasRequested, "Concept Atlas button should emit concept_atlas_requested");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestUtilityButtonsOpenFallbackDialogs()
    {
        MainMenuScreen screen = IntegrationTestUtils.InstantiateScene<MainMenuScreen>(ScenePath);
        try
        {
            screen._Ready();

            Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/HelpButton");
            Button? sourcesButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/SourcesButton");
            Button? optionsButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/OptionsButton");

            DotNetNativeTestSuite.AssertNotNull(helpButton, "Help button should exist");
            DotNetNativeTestSuite.AssertNotNull(sourcesButton, "Sources button should exist");
            DotNetNativeTestSuite.AssertNotNull(optionsButton, "Options button should exist");

            helpButton!.EmitSignal(BaseButton.SignalName.Pressed);
            Window? infoDialog = screen.GetNodeOrNull<Window>("InfoDialog");
            DotNetNativeTestSuite.AssertNotNull(infoDialog, "Help fallback should create an info dialog");
            DotNetNativeTestSuite.AssertTrue(infoDialog!.Visible, "Help fallback dialog should be visible");

            sourcesButton!.EmitSignal(BaseButton.SignalName.Pressed);
            DotNetNativeTestSuite.AssertTrue(infoDialog.Visible, "Sources fallback dialog should reuse the info dialog");

            optionsButton!.EmitSignal(BaseButton.SignalName.Pressed);
            Window? optionsDialog = screen.GetNodeOrNull<Window>("OptionsDialog");
            DotNetNativeTestSuite.AssertNotNull(optionsDialog, "Options fallback should create an options dialog");
            DotNetNativeTestSuite.AssertTrue(optionsDialog!.Visible, "Options fallback dialog should be visible");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestHelpCreditsAndSourcesCopyAreUserFacing()
    {
        MethodInfo? helpMethod = typeof(MainMenuScreen).GetMethod("BuildHelpFallbackText", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo? creditsMethod = typeof(MainMenuScreen).GetMethod("BuildCreditsFallbackText", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo? sourcesMethod = typeof(MainMenuScreen).GetMethod("BuildSourcesFallbackText", BindingFlags.NonPublic | BindingFlags.Static);

        DotNetNativeTestSuite.AssertNotNull(helpMethod, "Help fallback builder should exist");
        DotNetNativeTestSuite.AssertNotNull(creditsMethod, "Credits fallback builder should exist");
        DotNetNativeTestSuite.AssertNotNull(sourcesMethod, "Sources fallback builder should exist");

        string? helpText = helpMethod!.Invoke(null, null) as string;
        string? creditsText = creditsMethod!.Invoke(null, null) as string;
        string? sourcesText = sourcesMethod!.Invoke(null, null) as string;

        DotNetNativeTestSuite.AssertNotNull(helpText, "Help fallback text should exist");
        DotNetNativeTestSuite.AssertNotNull(creditsText, "Credits fallback text should exist");
        DotNetNativeTestSuite.AssertNotNull(sourcesText, "Sources fallback text should exist");
        DotNetNativeTestSuite.AssertTrue(helpText!.Contains("Station Studio"), "Help text should mention the Station Studio");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("Sources"), "Help text should mention the Sources button");
        DotNetNativeTestSuite.AssertTrue(creditsText!.Contains("AI assistance"), "Credits text should mention AI assistance");
        DotNetNativeTestSuite.AssertTrue(creditsText.Contains("Cursor"), "Credits text should mention Cursor");
        DotNetNativeTestSuite.AssertTrue(creditsText.Contains("Freepik via Flaticon"), "Credits should contain direct icon attribution");
        DotNetNativeTestSuite.AssertFalse(creditsText.Contains("Assets.md"), "Credits should not refer users to repository documentation");
        DotNetNativeTestSuite.AssertFalse(helpText.Contains("repo"), "Help text should avoid repository-facing language");
        DotNetNativeTestSuite.AssertTrue(sourcesText!.Contains("Bland-Hawthorn"), "Sources text should mention the current astronomy references");
    }

    private static void TestMainMenuUsesUserFacingVersionLabel()
    {
        MainMenuScreen screen = IntegrationTestUtils.InstantiateScene<MainMenuScreen>(ScenePath);
        try
        {
            screen._Ready();

            Label? versionLabel = screen.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/TopRow/VersionLabel");
            DotNetNativeTestSuite.AssertNotNull(versionLabel, "Version label should exist");
            DotNetNativeTestSuite.AssertTrue(versionLabel!.Text.Contains("0.8.0.0"), "Main menu should show the user-facing upcoming release label");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }
}
