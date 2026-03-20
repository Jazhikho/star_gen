#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGalaxyGenerationScreen
{
    private const string GalaxyGenerationScenePath = "res://src/app/GalaxyGenerationScreen.tscn";
    private const string SeedSpinPath = "MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/SeedContainer/SeedSpin";
    private const string StartButtonPath = "MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/Buttons/StartButton";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_galaxy_generation_screen_instantiates", TestGalaxyGenerationScreenInstantiates);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_get_current_config_returns_valid_config", TestGetCurrentConfigReturnsValidConfig);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_set_seeded_rng_accepts_rng", TestSetSeededRngAcceptsRng);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_set_current_config_round_trips", TestSetCurrentConfigRoundTrips);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_start_blocks_when_validation_errors_exist", TestStartBlocksWhenValidationErrorsExist);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_exposes_three_column_studio_layout", TestGalaxyGenerationScreenExposesThreeColumnStudioLayout);
    }

    private static GalaxyGenerationScreen CreateGalaxyGenerationScreen()
    {
        GalaxyGenerationScreen screen = IntegrationTestUtils.InstantiateScene<GalaxyGenerationScreen>(GalaxyGenerationScenePath);
        screen._Ready();
        return screen;
    }

    private static void TestGalaxyGenerationScreenInstantiates()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            DotNetNativeTestSuite.AssertNotNull(screen, "Galaxy generation screen should instantiate");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestGetCurrentConfigReturnsValidConfig()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            StarGen.Domain.Galaxy.GalaxyConfig config = screen.get_current_config();
            DotNetNativeTestSuite.AssertNotNull(config, "Config should be returned");
            DotNetNativeTestSuite.AssertTrue(config.IsValid(), "Config should be valid");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestSetSeededRngAcceptsRng()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            SpinBox? seedSpin = screen.GetNodeOrNull<SpinBox>(SeedSpinPath);
            DotNetNativeTestSuite.AssertNotNull(seedSpin, "Galaxy generation screen should expose the seed spin box");

            SeededRng expectedRng = new(42);
            int expectedSeed = expectedRng.RandiRange(1, 999999);

            SeededRng rng = new(42);
            screen.SetSeededRng(rng);
            DotNetNativeTestSuite.AssertEqual(expectedSeed, (int)seedSpin!.Value, "Setting a seeded RNG should refresh the seed field deterministically");

            screen.set_seeded_rng(default);
            DotNetNativeTestSuite.AssertEqual(12345, (int)seedSpin.Value, "Clearing the seeded RNG should fall back to the default deterministic seed");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestSetCurrentConfigRoundTrips()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            GalaxyConfig config = GalaxyConfig.CreateDefault();
            config.Type = GalaxySpec.GalaxyType.Elliptical;
            config.BulgeIntensity = 1.1;
            config.Ellipticity = 0.55;
            config.RadiusPc = 18000.0;
            config.StarDensityMultiplier = 1.4;

            screen.SetCurrentConfig(config);
            GalaxyConfig roundTrip = screen.GetCurrentConfig();

            DotNetNativeTestSuite.AssertEqual(config.Type, roundTrip.Type, "Galaxy type should round-trip through the galaxy studio");
            DotNetNativeTestSuite.AssertEqual(config.BulgeIntensity, roundTrip.BulgeIntensity, "Bulge intensity should round-trip through the galaxy studio");
            DotNetNativeTestSuite.AssertEqual(config.Ellipticity, roundTrip.Ellipticity, "Ellipticity should round-trip through the galaxy studio");
            DotNetNativeTestSuite.AssertEqual(config.RadiusPc, roundTrip.RadiusPc, "Radius should round-trip through the galaxy studio");
            DotNetNativeTestSuite.AssertEqual(config.StarDensityMultiplier, roundTrip.StarDensityMultiplier, "Density should round-trip through the galaxy studio");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestStartBlocksWhenValidationErrorsExist()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            bool started = false;
            screen.Connect("start_new_galaxy", Callable.From<GalaxyConfig, int>((_config, _seed) => started = true));

            SpinBox? seedSpin = screen.GetNodeOrNull<SpinBox>(SeedSpinPath);
            DotNetNativeTestSuite.AssertNotNull(seedSpin, "Galaxy generation screen should expose the seed spin box");
            seedSpin!.Value = 0.0;

            Button? startButton = screen.GetNodeOrNull<Button>(StartButtonPath);
            DotNetNativeTestSuite.AssertNotNull(startButton, "Galaxy generation screen should expose the start button");
            startButton!.EmitSignal(Button.SignalName.Pressed);

            DotNetNativeTestSuite.AssertFalse(started, "Blocking validation errors should stop startup emission");
            DotNetNativeTestSuite.AssertTrue(screen.GetCurrentIssues().HasErrors(), "Blocking validation errors should be surfaced in the galaxy studio");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestGalaxyGenerationScreenExposesThreeColumnStudioLayout()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            Control? rulesPanel = screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel");
            Control? summaryPanel = screen.GetNodeOrNull<Control>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel");
            Label? summaryLabel = screen.GetNodeOrNull<Label>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/SummaryScroll/SummaryContent/SummaryLabel");
            Button? startButton = screen.GetNodeOrNull<Button>(StartButtonPath);
            Button? infoButton = screen.FindChild("AdvancedAssumptionsInfoButton", recursive: true, owned: false) as Button;
            OptionButton? mainworldOption = screen.FindChild("MainworldPolicyOption", recursive: true, owned: false) as OptionButton;

            DotNetNativeTestSuite.AssertNotNull(rulesPanel, "Galaxy studio should expose a dedicated generation-rules panel");
            DotNetNativeTestSuite.AssertNotNull(summaryPanel, "Galaxy studio should expose a separate summary panel");
            DotNetNativeTestSuite.AssertNotNull(summaryLabel, "Summary panel should include the active-profile summary label");
            DotNetNativeTestSuite.AssertNotNull(startButton, "Galaxy studio should expose the generate button");
            DotNetNativeTestSuite.AssertNotNull(infoButton, "Advanced assumptions should expose an info button");
            DotNetNativeTestSuite.AssertEqual("Generate Galaxy", startButton!.Text, "Galaxy studio start button should use the user-facing generation label");
            DotNetNativeTestSuite.AssertNull(mainworldOption, "Galaxy studio should not expose the misleading mainworld control");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }
}
