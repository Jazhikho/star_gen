using StarGen.Domain.Galaxy;

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

        if (config.NumArms < 2 || config.NumArms > 6)
        {
            issues.AddError("num_arms", "Spiral arm count must be between 2 and 6.");
        }

        if (config.ArmPitchAngleDeg < 10.0 || config.ArmPitchAngleDeg > 30.0)
        {
            issues.AddError("arm_pitch_angle_deg", "Arm pitch must be between 10 and 30 degrees.");
        }

        if (config.ArmAmplitude < 0.3 || config.ArmAmplitude > 0.9)
        {
            issues.AddError("arm_amplitude", "Arm amplitude must be between 0.3 and 0.9.");
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

        if (config.Type == GalaxySpec.GalaxyType.Irregular && config.IrregularityScale >= 0.9)
        {
            issues.AddWarning("irregularity_scale", "Very strong irregularity is allowed, but it tends toward chaotic showcase structure.");
        }

        return issues;
    }
}
