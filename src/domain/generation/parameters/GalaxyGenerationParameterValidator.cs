using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Validates galaxy-generation inputs before regeneration runs.
/// </summary>
public static class GalaxyGenerationParameterValidator
{
    /// <summary>
    /// Validates a galaxy configuration and returns blocking errors and advisory warnings.
    /// </summary>
    public static GenerationParameterIssueSet Validate(int seedValue, GalaxyConfig config)
    {
        GenerationParameterIssueSet issues = new();
        if (seedValue <= 0)
        {
            issues.AddError("galaxy_seed", "Seed must be greater than zero.");
        }

        if (!config.IsValid())
        {
            if (config.HaloMassLog10Solar < 9.0 || config.HaloMassLog10Solar > 13.8)
            {
                issues.AddError("halo_mass_log10_solar", "Halo mass must stay between log10 9.0 and 13.8 solar masses.");
            }

            if (config.EnvironmentDensityIndex < 0.0 || config.EnvironmentDensityIndex > 1.0)
            {
                issues.AddError("environment_density_index", "Environment density must stay between 0.0 and 1.0.");
            }

            if (config.GhzInnerRadiusPc < 2000.0 || config.GhzInnerRadiusPc > 15000.0)
            {
                issues.AddError("ghz_inner_radius_pc", "GHZ inner radius must stay between 2000 and 15000 pc.");
            }

            if (config.GhzOuterRadiusPc <= config.GhzInnerRadiusPc || config.GhzOuterRadiusPc > 30000.0)
            {
                issues.AddError("ghz_outer_radius_pc", "GHZ outer radius must exceed the inner radius and stay at or below 30000 pc.");
            }

            if (config.GhzTransitionWidthPc < 500.0 || config.GhzTransitionWidthPc > 8000.0)
            {
                issues.AddError("ghz_transition_width_pc", "GHZ transition width must stay between 500 and 8000 pc.");
            }

            if (config.MetallicityGradientDexPerKpc > -0.005 || config.MetallicityGradientDexPerKpc < -0.12)
            {
                issues.AddError("metallicity_gradient_dex_per_kpc", "Metallicity gradient must stay between -0.120 and -0.005 dex per kpc.");
            }

            if (config.StarFormationEfficiency < 0.05 || config.StarFormationEfficiency > 0.65)
            {
                issues.AddError("star_formation_efficiency", "Star-formation efficiency must stay between 0.05 and 0.65.");
            }
        }

        if (config.NumArms < 2 || config.NumArms > 6)
        {
            if (config.Type == GalaxySpec.GalaxyType.Spiral)
            {
                issues.AddError("num_arms", "Spiral arm count must be between 2 and 6.");
            }
        }

        if (config.ArmPitchAngleDeg < 10.0 || config.ArmPitchAngleDeg > 30.0)
        {
            if (config.Type == GalaxySpec.GalaxyType.Spiral)
            {
                issues.AddError("arm_pitch_angle_deg", "Arm pitch must be between 10 and 30 degrees.");
            }
        }

        if (config.ArmAmplitude < 0.3 || config.ArmAmplitude > 0.9)
        {
            if (config.Type == GalaxySpec.GalaxyType.Spiral)
            {
                issues.AddError("arm_amplitude", "Arm amplitude must be between 0.3 and 0.9.");
            }
        }

        if (config.BulgeIntensity < 0.3 || config.BulgeIntensity > 1.2)
        {
            issues.AddError("bulge_intensity", "Bulge intensity must be between 0.3 and 1.2.");
        }

        if (config.BulgeRadiusPc < 1000.0 || config.BulgeRadiusPc > 2500.0)
        {
            issues.AddError("bulge_radius_pc", "Bulge radius must be between 1000 and 2500 pc.");
        }

        if (config.RadiusPc < 10000.0 || config.RadiusPc > 25000.0)
        {
            issues.AddError("radius_pc", "Galaxy radius must be between 10000 and 25000 pc.");
        }

        if (config.DiskScaleLengthPc < 2000.0 || config.DiskScaleLengthPc > 6000.0)
        {
            issues.AddError("disk_scale_length_pc", "Disk scale length must be between 2000 and 6000 pc.");
        }

        if (config.DiskScaleHeightPc < 200.0 || config.DiskScaleHeightPc > 500.0)
        {
            issues.AddError("disk_scale_height_pc", "Disk scale height must be between 200 and 500 pc.");
        }

        if (config.ThinDiskScaleLengthPc < 1200.0 || config.ThinDiskScaleLengthPc > 6000.0)
        {
            issues.AddError("thin_disk_scale_length_pc", "Thin-disk scale length must be between 1200 and 6000 pc.");
        }

        if (config.ThickDiskScaleLengthPc < 1000.0 || config.ThickDiskScaleLengthPc > 6000.0)
        {
            issues.AddError("thick_disk_scale_length_pc", "Thick-disk scale length must be between 1000 and 6000 pc.");
        }

        if (config.ThinDiskScaleHeightPc < 100.0 || config.ThinDiskScaleHeightPc > 600.0)
        {
            issues.AddError("thin_disk_scale_height_pc", "Thin-disk scale height must be between 100 and 600 pc.");
        }

        if (config.ThickDiskScaleHeightPc < 500.0 || config.ThickDiskScaleHeightPc > 2000.0)
        {
            issues.AddError("thick_disk_scale_height_pc", "Thick-disk scale height must be between 500 and 2000 pc.");
        }

        if (config.BarHalfLengthPc < 1500.0 || config.BarHalfLengthPc > 7000.0)
        {
            issues.AddError("bar_half_length_pc", "Bar half-length must be between 1500 and 7000 pc.");
        }

        if (config.SolarGalactocentricRadiusPc < 7000.0 || config.SolarGalactocentricRadiusPc > 9000.0)
        {
            issues.AddError("solar_galactocentric_radius_pc", "Solar-circle radius must be between 7000 and 9000 pc.");
        }

        if (config.CircularVelocityAtSolarRadiusKmS < 180.0 || config.CircularVelocityAtSolarRadiusKmS > 280.0)
        {
            issues.AddError("circular_velocity_at_solar_radius_km_s", "Solar-circle circular velocity must be between 180 and 280 km/s.");
        }

        if (config.StellarMassSolar < 1.0e9 || config.StellarMassSolar > 3.0e11)
        {
            issues.AddError("stellar_mass_solar", "Stellar mass scale must be between 1e9 and 3e11 solar masses.");
        }

        if (config.StarDensityMultiplier < 0.5 || config.StarDensityMultiplier > 2.0)
        {
            issues.AddError("star_density_multiplier", "Density multiplier must be between 0.5 and 2.0.");
        }

        if (config.Ellipticity < 0.0 || config.Ellipticity > 0.7)
        {
            issues.AddError("ellipticity", "Ellipticity must be between 0.0 and 0.7.");
        }

        if (config.IrregularityScale < 0.1 || config.IrregularityScale > 1.0)
        {
            issues.AddError("irregularity_scale", "Irregularity must be between 0.1 and 1.0.");
        }

        if (config.Type == GalaxySpec.GalaxyType.Spiral && config.NumArms >= 5)
        {
            issues.AddWarning("num_arms", "Five- and six-arm spirals are supported, but they are stylized compared with the Milky Way-like default.");
        }

        if (config.Type == GalaxySpec.GalaxyType.Spiral && config.ArmAmplitude >= 0.85)
        {
            issues.AddWarning("arm_amplitude", "Very strong arm contrast is allowed, but it bends realism toward a showcase presentation.");
        }

        if (config.Type == GalaxySpec.GalaxyType.Elliptical && config.DiskScaleHeightPc < 250.0)
        {
            issues.AddWarning("disk_scale_height_pc", "A thin disk height on an elliptical configuration is allowed, but it mixes morphology assumptions.");
        }

        if (config.Type == GalaxySpec.GalaxyType.Lenticular && config.SubtypeMode == GalaxySubtypeMode.LateType)
        {
            issues.AddWarning("subtype_mode", "Late-type lenticular mode maps to S0/a structure, which keeps a disk but not active spiral arms.");
        }

        if (config.Type == GalaxySpec.GalaxyType.Irregular && config.IrregularityScale >= 0.9)
        {
            issues.AddWarning("irregularity_scale", "Very strong irregularity is allowed, but it tends toward chaotic showcase structure.");
        }

        if (config.Type != GalaxySpec.GalaxyType.Spiral && config.ArmMechanismPreference != GalaxyArmMechanism.Auto)
        {
            issues.AddWarning("arm_mechanism_preference", "Arm mechanism only affects spiral galaxies; this choice is ignored for the current family.");
        }

        if (config.Type != GalaxySpec.GalaxyType.Spiral && config.NumArms > 0)
        {
            issues.AddWarning("num_arms", "Spiral arm count is stored for later family switches, but it does not affect the current family.");
        }

        if (config.Type != GalaxySpec.GalaxyType.Spiral && config.Type != GalaxySpec.GalaxyType.Lenticular && config.BarMode != GalaxyBarMode.Auto)
        {
            issues.AddWarning("bar_mode", "Bar mode only affects disk galaxies; this choice is ignored for the current family.");
        }

        if (!config.PlanetaryProfile.IsValid())
        {
            issues.AddError("planetary_profile", "The shared planetary-model settings must stay within the supported ranges.");
        }

        if (config.StarFormationEfficiency > 0.35)
        {
            issues.AddWarning("star_formation_efficiency", "High star-formation efficiency is allowed, but it shifts the generator toward unusually active star-forming regions.");
        }

        if (config.EnvironmentDensityIndex > 0.75 && config.Type == GalaxySpec.GalaxyType.Spiral && config.SubtypeMode == GalaxySubtypeMode.LateType)
        {
            issues.AddWarning("environment_density_index", "Dense environments tend to suppress late spiral structure; this combination intentionally leans against the default morphology-density trend.");
        }

        if (config.GhzOuterRadiusPc - config.GhzInnerRadiusPc < 2500.0)
        {
            issues.AddWarning("ghz_outer_radius_pc", "A very narrow galactic habitable zone is allowed, but it makes downstream habitability weighting unusually sharp.");
        }

        RpgCompatibilityProfile compatibilityProfile = config.UseCaseSettings.GetCompatibilityProfile();

        if (compatibilityProfile.IsActive && config.StarDensityMultiplier < 0.8)
        {
            issues.AddWarning("star_density_multiplier", $"{compatibilityProfile.Label} on a sparse galaxy can work, but it may produce fewer plausible mainworld candidates per region.");
        }

        if (config.UseCaseSettings.ShowTravellerReadouts && !compatibilityProfile.UsesUwpLikeReadouts)
        {
            issues.AddWarning("show_traveller_readouts", "UWP code is enabled while the active ruleset does not natively use UWP-like readouts; values shown will be derived mappings only.");
        }

        if (config.UseCaseSettings.MainworldPolicy == GenerationUseCaseSettings.MainworldPolicyType.Require
            && !compatibilityProfile.IsActive)
        {
            issues.AddWarning("mainworld_policy", "Requiring a mainworld is mainly intended for compatibility-oriented flows.");
        }

        return issues;
    }
}
