#nullable enable annotations
#nullable disable warnings

namespace StarGen.Tests.Framework;

/// <summary>
/// Dedicated native C# suite for probabilistic Solar-analog coverage.
/// </summary>
public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs only the Solar realization regression suite.
    /// </summary>
    public static void RunSolarRealizationHeadless(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_sun_like_system_probability_across_stellar_imf_forms",
            Tests.Unit.TestSolarSystemRealization.TestSunLikeSystemProbabilityAcrossStellarImfForms);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_sun_like_system_probability_across_stellar_imf_variation_modes",
            Tests.Unit.TestSolarSystemRealization.TestSunLikeSystemProbabilityAcrossStellarImfVariationModes);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_sun_like_system_probability_across_stellar_isochrone_models",
            Tests.Unit.TestSolarSystemRealization.TestSunLikeSystemProbabilityAcrossStellarIsochroneModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_sun_like_system_probability_across_multiplicity_scales",
            Tests.Unit.TestSolarSystemRealization.TestSunLikeSystemProbabilityAcrossMultiplicityScales);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_rocky_solar_analog_formation_across_planet_mass_radius_models",
            Tests.Unit.TestSolarSystemRealization.TestRockySolarAnalogFormationAcrossPlanetMassRadiusModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_rocky_solar_analog_formation_across_envelope_loss_models",
            Tests.Unit.TestSolarSystemRealization.TestRockySolarAnalogFormationAcrossEnvelopeLossModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_giant_solar_analog_formation_across_gas_giant_formation_models",
            Tests.Unit.TestSolarSystemRealization.TestGiantSolarAnalogFormationAcrossGasGiantFormationModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_giant_solar_analog_formation_across_metallicity_coupling_models",
            Tests.Unit.TestSolarSystemRealization.TestGiantSolarAnalogFormationAcrossMetallicityCouplingModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_giant_solar_analog_formation_across_rogue_allowance_models",
            Tests.Unit.TestSolarSystemRealization.TestGiantSolarAnalogFormationAcrossRogueAllowanceModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_giant_solar_analog_formation_across_outer_system_bias_models",
            Tests.Unit.TestSolarSystemRealization.TestGiantSolarAnalogFormationAcrossOuterSystemBiasModels);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_earth_moon_analogs_remain_possible_across_moon_formation_biases",
            Tests.Unit.TestSolarSystemRealization.TestEarthMoonAnalogsRemainPossibleAcrossMoonFormationBiases);
        runner.RunNativeTest(
            "TestSolarSystemRealization::test_gas_giant_moon_systems_respect_moon_formation_biases",
            Tests.Unit.TestSolarSystemRealization.TestGasGiantMoonSystemsRespectMoonFormationBiases);
    }
}
