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
    private const string StudioRootPath = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox";
    private const string SeedSpinPath = StudioRootPath + "/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/SeedContainer/SeedSpin";
    private const string StartButtonPath = StudioRootPath + "/StudioRow/SummaryPanel/MarginContainer/SummaryVBox/Buttons/StartButton";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_galaxy_generation_screen_instantiates", TestGalaxyGenerationScreenInstantiates);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_get_current_config_returns_valid_config", TestGetCurrentConfigReturnsValidConfig);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_set_seeded_rng_accepts_rng", TestSetSeededRngAcceptsRng);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_set_current_config_round_trips", TestSetCurrentConfigRoundTrips);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_start_blocks_when_validation_errors_exist", TestStartBlocksWhenValidationErrorsExist);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_exposes_three_column_studio_layout", TestGalaxyGenerationScreenExposesThreeColumnStudioLayout);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_active_profile_summary_is_concise_and_uses_updated_labels", TestActiveProfileSummaryIsConciseAndUsesUpdatedLabels);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_main_panel_matches_main_menu_horizontal_inset", TestMainPanelMatchesMainMenuHorizontalInset);
        runner.RunNativeTest("TestGalaxyGenerationScreen::test_tooltips_use_updated_wording", TestTooltipsUseUpdatedWording);
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
            SpinBox? seedSpin = screen.FindChild("SeedSpin", recursive: true, owned: false) as SpinBox;
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

            SpinBox? seedSpin = screen.FindChild("SeedSpin", recursive: true, owned: false) as SpinBox;
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
            Control? rulesPanel = screen.FindChild("RulesPanel", recursive: true, owned: false) as Control;
            Control? summaryPanel = screen.FindChild("SummaryPanel", recursive: true, owned: false) as Control;
            Label? summaryLabel = screen.FindChild("SummaryLabel", recursive: true, owned: false) as Label;
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

    private static void TestActiveProfileSummaryIsConciseAndUsesUpdatedLabels()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            Label? summaryLabel = screen.FindChild("SummaryLabel", recursive: true, owned: false) as Label;
            Label? assumptionsLabel = screen.FindChild("AssumptionsLabel", recursive: true, owned: false) as Label;

            DotNetNativeTestSuite.AssertNotNull(summaryLabel, "Galaxy studio should expose the active profile summary label");
            DotNetNativeTestSuite.AssertTrue(summaryLabel!.Text.Contains("Ruleset Realistic"), "Summary should use the Realistic ruleset label");
            DotNetNativeTestSuite.AssertFalse(summaryLabel.Text.Contains("Expansion Pressure"), "Summary should not expose colonization controls on the generation surface");
            DotNetNativeTestSuite.AssertFalse(summaryLabel.Text.Contains("worldbuilding permissiveness scale"), "Summary should not include explanation paragraphs");
            DotNetNativeTestSuite.AssertNotNull(assumptionsLabel, "Galaxy studio should still have the assumptions label node");
            DotNetNativeTestSuite.AssertEqual("", assumptionsLabel!.Text, "Assumptions label should not carry explanation paragraphs");
            DotNetNativeTestSuite.AssertFalse(assumptionsLabel.Visible, "Assumptions label should be hidden in the concise summary layout");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestMainPanelMatchesMainMenuHorizontalInset()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            MarginContainer? mainPanelMargin = screen.GetNodeOrNull<MarginContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer");

            DotNetNativeTestSuite.AssertNotNull(mainPanelMargin, "Galaxy studio should expose the main-panel margin container");
            DotNetNativeTestSuite.AssertEqual(18, mainPanelMargin!.GetThemeConstant("margin_left"), "Galaxy studio should keep the same left inset as the top and bottom shell spacing");
            DotNetNativeTestSuite.AssertEqual(18, mainPanelMargin.GetThemeConstant("margin_right"), "Galaxy studio should keep the same right inset as the top and bottom shell spacing");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }

    private static void TestTooltipsUseUpdatedWording()
    {
        GalaxyGenerationScreen screen = CreateGalaxyGenerationScreen();
        try
        {
            Label? lifeValue = screen.FindChild("LifePermissivenessValue", recursive: true, owned: false) as Label;
            Control? populationRow = screen.FindChild("PopulationRow", recursive: true, owned: false) as Control;
            OptionButton? rulesetOption = screen.FindChild("RulesetModeOption", recursive: true, owned: false) as OptionButton;
            Button? advancedInfo = screen.FindChild("AdvancedAssumptionsInfoButton", recursive: true, owned: false) as Button;

            DotNetNativeTestSuite.AssertNotNull(lifeValue, "Life value label should exist");
            DotNetNativeTestSuite.AssertNotNull(populationRow, "Population row should still exist in the scene for compatibility");
            DotNetNativeTestSuite.AssertNotNull(rulesetOption, "Ruleset selector should exist");
            DotNetNativeTestSuite.AssertNotNull(advancedInfo, "Advanced assumptions info button should exist");

            DotNetNativeTestSuite.AssertTrue(lifeValue!.TooltipText.Contains("native life"), "Life tooltip should explain life potential");
            DotNetNativeTestSuite.AssertFalse(populationRow!.Visible, "Colonization controls should be hidden from the generation screen");
            DotNetNativeTestSuite.AssertTrue(rulesetOption!.TooltipText.Contains("Realistic"), "Ruleset tooltip should use the Realistic label");
            DotNetNativeTestSuite.AssertFalse(advancedInfo!.TooltipText.Contains("Expansion Pressure"), "Advanced assumptions tooltip should stop describing colonization as generation");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(screen);
        }
    }
}
