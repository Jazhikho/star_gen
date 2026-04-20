#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestGenerationParameters
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestGenerationParameters::test_system_validator_blocks_zero_seed", TestSystemValidatorBlocksZeroSeed);
        runner.RunNativeTest("TestGenerationParameters::test_system_validator_warns_on_high_multiplicity", TestSystemValidatorWarnsOnHighMultiplicity);
        runner.RunNativeTest("TestGenerationParameters::test_galaxy_validator_blocks_out_of_range_radius", TestGalaxyValidatorBlocksOutOfRangeRadius);
        runner.RunNativeTest("TestGenerationParameters::test_galaxy_validator_warns_on_showcase_spiral", TestGalaxyValidatorWarnsOnShowcaseSpiral);
        runner.RunNativeTest("TestGenerationParameters::test_system_validator_warns_when_traveller_mode_disables_population", TestSystemValidatorWarnsWhenTravellerModeDisablesPopulation);
        runner.RunNativeTest("TestGenerationParameters::test_galaxy_validator_warns_when_default_ruleset_shows_traveller_readouts", TestGalaxyValidatorWarnsWhenDefaultRulesetShowsTravellerReadouts);
        runner.RunNativeTest("TestGenerationParameters::test_ruleset_profiles_apply_expected_defaults", TestRulesetProfilesApplyExpectedDefaults);
        runner.RunNativeTest("TestGenerationParameters::test_use_case_settings_round_trip_through_galaxy_config", TestUseCaseSettingsRoundTripThroughGalaxyConfig);
        runner.RunNativeTest("TestGenerationParameters::test_use_case_settings_round_trip_through_system_spec", TestUseCaseSettingsRoundTripThroughSystemSpec);
        runner.RunNativeTest("TestGenerationParameters::test_use_case_settings_round_trip_through_object_specs", TestUseCaseSettingsRoundTripThroughObjectSpecs);
    }

    private static void TestSystemValidatorBlocksZeroSeed()
    {
        SolarSystemSpec spec = new(0, 1, 1);
        GenerationParameterIssueSet issues = SystemGenerationParameterValidator.Validate(spec);
        DotNetNativeTestSuite.AssertTrue(issues.HasErrors(), "Zero-seed system specs should be blocked");
    }

    private static void TestSystemValidatorWarnsOnHighMultiplicity()
    {
        SolarSystemSpec spec = new(42, 4, 5);
        spec.SpectralClassHints.Add((int)StarClass.SpectralClass.G);
        GenerationParameterIssueSet issues = SystemGenerationParameterValidator.Validate(spec);
        DotNetNativeTestSuite.AssertFalse(issues.HasErrors(), "High-multiplicity systems should remain allowed");
        DotNetNativeTestSuite.AssertTrue(issues.Issues.Count > 0, "High-multiplicity systems should produce advisory warnings");
    }

    private static void TestGalaxyValidatorBlocksOutOfRangeRadius()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.RadiusPc = 8000.0;
        GenerationParameterIssueSet issues = GalaxyGenerationParameterValidator.Validate(42, config);
        DotNetNativeTestSuite.AssertTrue(issues.HasErrors(), "Out-of-range galaxy radius should be blocked");
    }

    private static void TestGalaxyValidatorWarnsOnShowcaseSpiral()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.Type = GalaxySpec.GalaxyType.Spiral;
        config.NumArms = 5;
        config.ArmAmplitude = 0.88;
        GenerationParameterIssueSet issues = GalaxyGenerationParameterValidator.Validate(42, config);
        DotNetNativeTestSuite.AssertFalse(issues.HasErrors(), "Showcase spiral values should remain allowed");
        DotNetNativeTestSuite.AssertTrue(issues.Issues.Count >= 2, "Showcase spiral values should produce advisory warnings");
    }

    private static void TestSystemValidatorWarnsWhenTravellerModeDisablesPopulation()
    {
        SolarSystemSpec spec = new(42, 1, 1);
        spec.UseCaseSettings.ApplyTravellerDefaults();
        spec.GeneratePopulation = false;

        GenerationParameterIssueSet issues = SystemGenerationParameterValidator.Validate(spec);
        DotNetNativeTestSuite.AssertFalse(issues.HasErrors(), "Traveller advisory conflicts should not block generation");
        DotNetNativeTestSuite.AssertTrue(ContainsIssue(issues, "generate_population"), "Traveller mode without population should raise an advisory warning");
    }

    private static void TestGalaxyValidatorWarnsWhenDefaultRulesetShowsTravellerReadouts()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.UseCaseSettings.ShowTravellerReadouts = true;

        GenerationParameterIssueSet issues = GalaxyGenerationParameterValidator.Validate(42, config);
        DotNetNativeTestSuite.AssertFalse(issues.HasErrors(), "Derived Traveller readouts should remain advisory in default mode");
        DotNetNativeTestSuite.AssertTrue(ContainsIssue(issues, "show_traveller_readouts"), "Default ruleset with Traveller readouts should raise an advisory warning");
    }

    private static void TestRulesetProfilesApplyExpectedDefaults()
    {
        AssertRulesetDefaults(
            GenerationUseCaseSettings.RulesetModeType.Traveller,
            GenerationUseCaseSettings.MainworldPolicyType.Require,
            true,
            GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres,
            0.58,
            1.35,
            0.85,
            1.25,
            1.15,
            1.35);
        AssertRulesetDefaults(
            GenerationUseCaseSettings.RulesetModeType.Cepheus,
            GenerationUseCaseSettings.MainworldPolicyType.Require,
            true,
            GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
            0.50,
            1.18,
            0.92,
            1.15,
            1.00,
            1.15);
        AssertRulesetDefaults(
            GenerationUseCaseSettings.RulesetModeType.Starfinder,
            GenerationUseCaseSettings.MainworldPolicyType.Prefer,
            false,
            GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows,
            0.60,
            1.25,
            1.05,
            1.18,
            1.08,
            1.45);
        AssertRulesetDefaults(
            GenerationUseCaseSettings.RulesetModeType.Starforged,
            GenerationUseCaseSettings.MainworldPolicyType.Prefer,
            false,
            GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite,
            0.45,
            0.95,
            1.15,
            0.95,
            0.92,
            0.85);
    }

    private static void TestUseCaseSettingsRoundTripThroughGalaxyConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.UseCaseSettings = CreateTravellerSettings();

        GalaxyConfig? rebuilt = GalaxyConfig.FromDictionary(config.ToDictionary());
        DotNetNativeTestSuite.AssertNotNull(rebuilt, "Galaxy config should round-trip from its dictionary payload");
        AssertUseCaseSettingsEqual(config.UseCaseSettings, rebuilt!.UseCaseSettings, "Galaxy config should preserve use-case settings");
    }

    private static void TestUseCaseSettingsRoundTripThroughSystemSpec()
    {
        SolarSystemSpec spec = new(4242, 2, 3);
        spec.UseCaseSettings = CreateTravellerSettings();

        SolarSystemSpec rebuilt = SolarSystemSpec.FromDictionary(spec.ToDictionary());
        AssertUseCaseSettingsEqual(spec.UseCaseSettings, rebuilt.UseCaseSettings, "System spec should preserve use-case settings");
    }

    private static void TestUseCaseSettingsRoundTripThroughObjectSpecs()
    {
        GenerationUseCaseSettings settings = CreateTravellerSettings();

        StarSpec starSpec = StarSpec.Random(9001);
        starSpec.UseCaseSettings = settings.Clone();
        StarSpec rebuiltStar = StarSpec.FromDictionary(starSpec.ToDictionary());
        AssertUseCaseSettingsEqual(settings, rebuiltStar.UseCaseSettings, "Star spec should preserve use-case settings");

        PlanetSpec planetSpec = PlanetSpec.Random(9002);
        planetSpec.UseCaseSettings = settings.Clone();
        PlanetSpec rebuiltPlanet = PlanetSpec.FromDictionary(planetSpec.ToDictionary());
        AssertUseCaseSettingsEqual(settings, rebuiltPlanet.UseCaseSettings, "Planet spec should preserve use-case settings");

        MoonSpec moonSpec = MoonSpec.Random(9003);
        moonSpec.UseCaseSettings = settings.Clone();
        MoonSpec rebuiltMoon = MoonSpec.FromDictionary(moonSpec.ToDictionary());
        AssertUseCaseSettingsEqual(settings, rebuiltMoon.UseCaseSettings, "Moon spec should preserve use-case settings");

        AsteroidSpec asteroidSpec = AsteroidSpec.Random(9004);
        asteroidSpec.UseCaseSettings = settings.Clone();
        AsteroidSpec rebuiltAsteroid = AsteroidSpec.FromDictionary(asteroidSpec.ToDictionary());
        AssertUseCaseSettingsEqual(settings, rebuiltAsteroid.UseCaseSettings, "Asteroid spec should preserve use-case settings");
    }

    private static bool ContainsIssue(GenerationParameterIssueSet issues, string parameterId)
    {
        foreach (GenerationParameterIssue issue in issues.Issues)
        {
            if (issue.ParameterId == parameterId)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertRulesetDefaults(
        GenerationUseCaseSettings.RulesetModeType rulesetMode,
        GenerationUseCaseSettings.MainworldPolicyType expectedMainworldPolicy,
        bool expectedUwpLikeReadouts,
        GenerationUseCaseSettings.LifeFrameworkType expectedLifeFramework,
        double expectedLifePermissiveness,
        double expectedTemperateMultiplier,
        double expectedHarshMultiplier,
        double expectedTerrestrialMultiplier,
        double expectedNativeLifeMultiplier,
        double expectedColonyMultiplier)
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.RulesetMode = rulesetMode;
        settings.ApplyRulesetDefaults();
        RpgCompatibilityProfile profile = settings.GetCompatibilityProfile();

        DotNetNativeTestSuite.AssertTrue(profile.IsActive, $"{rulesetMode} should resolve to an active compatibility profile");
        DotNetNativeTestSuite.AssertEqual(expectedMainworldPolicy, settings.MainworldPolicy, $"{rulesetMode} should apply its mainworld default");
        DotNetNativeTestSuite.AssertEqual(expectedUwpLikeReadouts, settings.ShowTravellerReadouts, $"{rulesetMode} should apply the expected UWP-readout default");
        DotNetNativeTestSuite.AssertEqual(expectedLifeFramework, settings.LifeFramework, $"{rulesetMode} should apply the expected life framework");
        DotNetNativeTestSuite.AssertFloatNear(expectedLifePermissiveness, settings.LifePermissiveness, 0.0001, $"{rulesetMode} should apply the expected life permissiveness");
        DotNetNativeTestSuite.AssertFloatNear(expectedTemperateMultiplier, profile.TemperateSlotFillMultiplier, 0.0001, $"{rulesetMode} should apply the expected temperate-world multiplier");
        DotNetNativeTestSuite.AssertFloatNear(expectedHarshMultiplier, profile.HarshSlotFillMultiplier, 0.0001, $"{rulesetMode} should apply the expected harsh-world multiplier");
        DotNetNativeTestSuite.AssertFloatNear(expectedTerrestrialMultiplier, profile.TerrestrialWorldWeightMultiplier, 0.0001, $"{rulesetMode} should apply the expected mainworld-class multiplier");
        DotNetNativeTestSuite.AssertFloatNear(expectedNativeLifeMultiplier, profile.NativeLifeProbabilityMultiplier, 0.0001, $"{rulesetMode} should apply the expected native-life multiplier");
        DotNetNativeTestSuite.AssertFloatNear(expectedColonyMultiplier, profile.ColonyProbabilityMultiplier, 0.0001, $"{rulesetMode} should apply the expected settlement multiplier");
    }

    private static GenerationUseCaseSettings CreateTravellerSettings()
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
        settings.ShowTravellerReadouts = true;
        settings.ForceLifeOnSupportableWorlds = true;
        settings.LifePermissiveness = 0.75;
        settings.LifeFramework = GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres;
        settings.AbiogenesisModel = GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework;
        settings.ComplexLifeModel = GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework;
        settings.CivilizationModel = GenerationUseCaseSettings.CivilizationModelType.FollowFramework;
        settings.EnvironmentalWindowWeight = GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework;
        settings.MainworldPolicy = GenerationUseCaseSettings.MainworldPolicyType.Require;
        settings.CompatibilityTemperateSlotFillMultiplier = 1.42;
        settings.CompatibilityHarshSlotFillMultiplier = 0.78;
        settings.CompatibilityTerrestrialWorldWeightMultiplier = 1.31;
        settings.CompatibilityNativeLifeProbabilityMultiplier = 1.17;
        settings.CompatibilityColonyProbabilityMultiplier = 1.44;
        return settings;
    }

    private static void AssertUseCaseSettingsEqual(GenerationUseCaseSettings expected, GenerationUseCaseSettings actual, string messagePrefix)
    {
        DotNetNativeTestSuite.AssertEqual(expected.RulesetMode, actual.RulesetMode, messagePrefix + ": ruleset mode should match");
        DotNetNativeTestSuite.AssertEqual(expected.ShowTravellerReadouts, actual.ShowTravellerReadouts, messagePrefix + ": readout visibility should match");
        DotNetNativeTestSuite.AssertEqual(expected.ForceLifeOnSupportableWorlds, actual.ForceLifeOnSupportableWorlds, messagePrefix + ": force-life override should match");
        DotNetNativeTestSuite.AssertEqual(expected.LifePermissiveness, actual.LifePermissiveness, messagePrefix + ": life permissiveness should match");
        DotNetNativeTestSuite.AssertEqual(expected.LifeFramework, actual.LifeFramework, messagePrefix + ": life framework should match");
        DotNetNativeTestSuite.AssertEqual(expected.AbiogenesisModel, actual.AbiogenesisModel, messagePrefix + ": abiogenesis model should match");
        DotNetNativeTestSuite.AssertEqual(expected.ComplexLifeModel, actual.ComplexLifeModel, messagePrefix + ": complex-life model should match");
        DotNetNativeTestSuite.AssertEqual(expected.CivilizationModel, actual.CivilizationModel, messagePrefix + ": civilization model should match");
        DotNetNativeTestSuite.AssertEqual(expected.EnvironmentalWindowWeight, actual.EnvironmentalWindowWeight, messagePrefix + ": environmental-window weight should match");
        DotNetNativeTestSuite.AssertEqual(expected.MainworldPolicy, actual.MainworldPolicy, messagePrefix + ": mainworld policy should match");
        DotNetNativeTestSuite.AssertEqual(expected.CompatibilityTemperateSlotFillMultiplier, actual.CompatibilityTemperateSlotFillMultiplier, messagePrefix + ": temperate-world multiplier should match");
        DotNetNativeTestSuite.AssertEqual(expected.CompatibilityHarshSlotFillMultiplier, actual.CompatibilityHarshSlotFillMultiplier, messagePrefix + ": harsh-world multiplier should match");
        DotNetNativeTestSuite.AssertEqual(expected.CompatibilityTerrestrialWorldWeightMultiplier, actual.CompatibilityTerrestrialWorldWeightMultiplier, messagePrefix + ": mainworld-class multiplier should match");
        DotNetNativeTestSuite.AssertEqual(expected.CompatibilityNativeLifeProbabilityMultiplier, actual.CompatibilityNativeLifeProbabilityMultiplier, messagePrefix + ": native-life multiplier should match");
        DotNetNativeTestSuite.AssertEqual(expected.CompatibilityColonyProbabilityMultiplier, actual.CompatibilityColonyProbabilityMultiplier, messagePrefix + ": settlement multiplier should match");
    }
}
