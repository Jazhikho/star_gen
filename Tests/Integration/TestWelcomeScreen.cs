#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestWelcomeScreen
{
    private const string WelcomeScenePath = "res://src/app/WelcomeScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestWelcomeScreen::test_welcome_screen_instantiates", TestWelcomeScreenInstantiates);
        runner.RunNativeTest("TestWelcomeScreen::test_get_current_config_returns_valid_config", TestGetCurrentConfigReturnsValidConfig);
        runner.RunNativeTest("TestWelcomeScreen::test_set_seeded_rng_accepts_rng", TestSetSeededRngAcceptsRng);
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
            SpinBox? seedSpin = welcome.GetNodeOrNull<SpinBox>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/SeedSpin");
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
}
