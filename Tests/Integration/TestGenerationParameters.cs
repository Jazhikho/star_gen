#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Parameters;
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
}
