#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestMainMenuScreen
{
    private const string ScenePath = "res://src/app/MainMenuScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestMainMenuScreen::test_mode_buttons_emit_navigation_signals", TestModeButtonsEmitNavigationSignals);
        runner.RunNativeTest("TestMainMenuScreen::test_utility_buttons_open_fallback_dialogs", TestUtilityButtonsOpenFallbackDialogs);
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

    private static void TestUtilityButtonsOpenFallbackDialogs()
    {
        MainMenuScreen screen = IntegrationTestUtils.InstantiateScene<MainMenuScreen>(ScenePath);
        try
        {
            screen._Ready();

            Button? helpButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/HelpButton");
            Button? optionsButton = screen.GetNodeOrNull<Button>("MarginContainer/ScrollContainer/Layout/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/OptionsButton");

            DotNetNativeTestSuite.AssertNotNull(helpButton, "Help button should exist");
            DotNetNativeTestSuite.AssertNotNull(optionsButton, "Options button should exist");

            helpButton!.EmitSignal(BaseButton.SignalName.Pressed);
            Window? infoDialog = screen.GetNodeOrNull<Window>("InfoDialog");
            DotNetNativeTestSuite.AssertNotNull(infoDialog, "Help fallback should create an info dialog");
            DotNetNativeTestSuite.AssertTrue(infoDialog!.Visible, "Help fallback dialog should be visible");

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
}
