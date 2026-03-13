#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestWelcomeScreen
{
    private const string WelcomeScenePath = "res://src/app/WelcomeScreen.tscn";
    private const string SeedSpinPath = "MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/SeedContainer/SeedSpin";
    private const string StartButtonPath = "MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/FooterVBox/Buttons/StartButton";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestWelcomeScreen::test_welcome_screen_instantiates", TestWelcomeScreenInstantiates);
        runner.RunNativeTest("TestWelcomeScreen::test_get_current_config_returns_valid_config", TestGetCurrentConfigReturnsValidConfig);
        runner.RunNativeTest("TestWelcomeScreen::test_set_seeded_rng_accepts_rng", TestSetSeededRngAcceptsRng);
        runner.RunNativeTest("TestWelcomeScreen::test_set_current_config_round_trips", TestSetCurrentConfigRoundTrips);
        runner.RunNativeTest("TestWelcomeScreen::test_start_blocks_when_validation_errors_exist", TestStartBlocksWhenValidationErrorsExist);
    }

    private static WelcomeScreen CreateWelcomeScreen()
    {
        WelcomeScreen welcome = IntegrationTestUtils.InstantiateScene<WelcomeScreen>(WelcomeScenePath);
        welcome._Ready();
        return welcome;
    }

    private static void TestWelcomeScreenInstantiates()
    {
        WelcomeScreen welcome = CreateWelcomeScreen();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(welcome, "Welcome screen should instantiate");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(welcome);
        }
    }

    private static void TestGetCurrentConfigReturnsValidConfig()
    {
        WelcomeScreen welcome = CreateWelcomeScreen();
        try
        {
            StarGen.Domain.Galaxy.GalaxyConfig config = welcome.get_current_config();
            DotNetNativeTestSuite.AssertNotNull(config, "Config should be returned");
            DotNetNativeTestSuite.AssertTrue(config.IsValid(), "Config should be valid");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(welcome);
        }
    }

    private static void TestSetSeededRngAcceptsRng()
    {
        WelcomeScreen welcome = CreateWelcomeScreen();
        try
        {
            SpinBox? seedSpin = welcome.GetNodeOrNull<SpinBox>(SeedSpinPath);
            DotNetNativeTestSuite.AssertNotNull(seedSpin, "Welcome screen should expose the seed spin box");

            SeededRng expectedRng = new(42);
            int expectedSeed = expectedRng.RandiRange(1, 999999);

            SeededRng rng = new(42);
            welcome.SetSeededRng(rng);
            DotNetNativeTestSuite.AssertEqual(expectedSeed, (int)seedSpin!.Value, "Setting a seeded RNG should refresh the seed field deterministically");

            welcome.set_seeded_rng(default);
            DotNetNativeTestSuite.AssertEqual(12345, (int)seedSpin.Value, "Clearing the seeded RNG should fall back to the default deterministic seed");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(welcome);
        }
    }

    private static void TestSetCurrentConfigRoundTrips()
    {
        WelcomeScreen welcome = CreateWelcomeScreen();
        try
        {
            GalaxyConfig config = GalaxyConfig.CreateDefault();
            config.Type = GalaxySpec.GalaxyType.Elliptical;
            config.BulgeIntensity = 1.1;
            config.Ellipticity = 0.55;
            config.RadiusPc = 18000.0;
            config.StarDensityMultiplier = 1.4;

            welcome.SetCurrentConfig(config);
            GalaxyConfig roundTrip = welcome.GetCurrentConfig();

            DotNetNativeTestSuite.AssertEqual(config.Type, roundTrip.Type, "Galaxy type should round-trip through the welcome editor");
            DotNetNativeTestSuite.AssertEqual(config.BulgeIntensity, roundTrip.BulgeIntensity, "Bulge intensity should round-trip through the welcome editor");
            DotNetNativeTestSuite.AssertEqual(config.Ellipticity, roundTrip.Ellipticity, "Ellipticity should round-trip through the welcome editor");
            DotNetNativeTestSuite.AssertEqual(config.RadiusPc, roundTrip.RadiusPc, "Radius should round-trip through the welcome editor");
            DotNetNativeTestSuite.AssertEqual(config.StarDensityMultiplier, roundTrip.StarDensityMultiplier, "Density should round-trip through the welcome editor");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(welcome);
        }
    }

    private static void TestStartBlocksWhenValidationErrorsExist()
    {
        WelcomeScreen welcome = CreateWelcomeScreen();
        try
        {
            bool started = false;
            welcome.Connect("start_new_galaxy", Callable.From<GalaxyConfig, int>((_config, _seed) => started = true));

            SpinBox? seedSpin = welcome.GetNodeOrNull<SpinBox>(SeedSpinPath);
            DotNetNativeTestSuite.AssertNotNull(seedSpin, "Welcome screen should expose the seed spin box");
            seedSpin!.Value = 0.0;

            Button? startButton = welcome.GetNodeOrNull<Button>(StartButtonPath);
            DotNetNativeTestSuite.AssertNotNull(startButton, "Welcome screen should expose the start button");
            startButton!.EmitSignal(Button.SignalName.Pressed);

            DotNetNativeTestSuite.AssertFalse(started, "Blocking validation errors should stop startup emission");
            DotNetNativeTestSuite.AssertTrue(welcome.GetCurrentIssues().HasErrors(), "Blocking validation errors should be surfaced in the welcome screen");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(welcome);
        }
    }
}
