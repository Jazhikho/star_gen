using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Shared parameter metadata for object, system, and galaxy generation editors.
/// </summary>
public static class GenerationParameterCatalog
{
    /// <summary>
    /// Returns system-generation parameter definitions.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemDefinitions()
    {
        return new List<GenerationParameterDefinition>
        {
            new GenerationParameterDefinition("generation_seed", "Seed", string.Empty, GenerationParameterControlType.Number, "Master deterministic input for the whole system."),
            new GenerationParameterDefinition("star_count_min", "Min Stars", string.Empty, GenerationParameterControlType.Number, "Sets the lower bound for stellar multiplicity."),
            new GenerationParameterDefinition("star_count_max", "Max Stars", string.Empty, GenerationParameterControlType.Number, "Sets the upper bound for stellar multiplicity."),
            new GenerationParameterDefinition("spectral_class_hints", "Spectral Hints", string.Empty, GenerationParameterControlType.Text, "Ordered star-class targets; extra stars still use generator defaults.", supportsTarget: true),
            new GenerationParameterDefinition("system_age_years", "System Age", "Gyr", GenerationParameterControlType.Number, "When set, all stars share an age target instead of rolling independently.", supportsTarget: true),
            new GenerationParameterDefinition("system_metallicity", "System Metallicity", "Zsun", GenerationParameterControlType.Number, "When set, all stars share a metallicity target instead of rolling independently.", supportsTarget: true),
            new GenerationParameterDefinition("include_asteroid_belts", "Asteroid Belts", string.Empty, GenerationParameterControlType.Toggle, "Controls whether the belt stage participates in generation."),
            new GenerationParameterDefinition("generate_population", "Generate Population", string.Empty, GenerationParameterControlType.Toggle, "Enables the downstream population pipeline for planets and moons."),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Selects the downstream generation pipeline. Realistic keeps StarGen's default scientific and physical worldbuilding path, while Traveller keeps normal galaxy structure but uses Traveller world-generation rules for supported mainworld outputs."),
            new GenerationParameterDefinition("show_traveller_readouts", "Traveller Readouts", string.Empty, GenerationParameterControlType.Toggle, "Shows derived Traveller/UWP-oriented readouts when the current flow has enough information."),
            new GenerationParameterDefinition("life_permissiveness", "Life Potential", string.Empty, GenerationParameterControlType.Number, "Controls native-life permissiveness. Low values require near-Earthlike conditions and penalize hostile factors hard; high values allow biospheres on marginal but still biologically plausible worlds."),
            new GenerationParameterDefinition("mainworld_policy", "Mainworld Policy", string.Empty, GenerationParameterControlType.Choice, "Controls whether Traveller-oriented flows should ignore, prefer, or require a plausible mainworld candidate."),
        };
    }

    /// <summary>
    /// Returns galaxy-generation parameter definitions.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyDefinitions()
    {
        return new List<GenerationParameterDefinition>
        {
            new GenerationParameterDefinition("galaxy_seed", "Seed", string.Empty, GenerationParameterControlType.Number, "Master deterministic input for galaxy sampling."),
            new GenerationParameterDefinition("galaxy_type", "Galaxy Type", string.Empty, GenerationParameterControlType.Choice, "Selects the density model. Spiral uses a disk plus arm modulation, elliptical uses a smooth triaxial spheroid, and irregular uses an off-center clumpy field."),
            new GenerationParameterDefinition("num_arms", "Spiral Arms", string.Empty, GenerationParameterControlType.Number, "Only applies to spiral galaxies. Changes the number of logarithmic arm ridges that concentrate stars."),
            new GenerationParameterDefinition("arm_pitch_angle_deg", "Arm Pitch", "deg", GenerationParameterControlType.Number, "Only applies to spiral galaxies. Lower values wind the arms tighter; higher values open them out."),
            new GenerationParameterDefinition("arm_amplitude", "Arm Amplitude", string.Empty, GenerationParameterControlType.Number, "Only applies to spiral galaxies. Raises or lowers the contrast between arm and inter-arm star density."),
            new GenerationParameterDefinition("bulge_intensity", "Bulge Intensity", string.Empty, GenerationParameterControlType.Number, "Controls how strongly stars pile into the central bulge or core region."),
            new GenerationParameterDefinition("bulge_radius_pc", "Bulge Radius", "pc", GenerationParameterControlType.Number, "Sets the size of the central bulge used by spiral and elliptical density models."),
            new GenerationParameterDefinition("radius_pc", "Radius", "pc", GenerationParameterControlType.Number, "Sets the outer scale of the galaxy footprint before local density falloff is applied."),
            new GenerationParameterDefinition("disk_scale_length_pc", "Disk Scale Length", "pc", GenerationParameterControlType.Number, "For disk-like galaxies, controls how quickly stellar density drops with radial distance from the center."),
            new GenerationParameterDefinition("disk_scale_height_pc", "Disk Scale Height", "pc", GenerationParameterControlType.Number, "For disk-like galaxies, controls how thick the stellar disk is above and below the plane."),
            new GenerationParameterDefinition("star_density_multiplier", "Density Multiplier", "x", GenerationParameterControlType.Number, "Scales total sampled star density while keeping the selected morphology shape intact."),
            new GenerationParameterDefinition("ellipticity", "Ellipticity", string.Empty, GenerationParameterControlType.Number, "Only applies to elliptical galaxies. Changes how flattened the ellipsoid is relative to a rounder spheroid."),
            new GenerationParameterDefinition("irregularity_scale", "Irregularity", string.Empty, GenerationParameterControlType.Number, "Only applies to irregular galaxies. Changes how asymmetric, offset, and clumpy the density field becomes."),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Selects the downstream generation pipeline. Realistic keeps StarGen's default scientific and physical worldbuilding path, while Traveller keeps normal galaxy structure but uses Traveller world-generation rules for supported mainworld outputs."),
            new GenerationParameterDefinition("show_traveller_readouts", "Traveller Readouts", string.Empty, GenerationParameterControlType.Toggle, "Shows derived Traveller/UWP-oriented readouts when the current flow has enough information."),
            new GenerationParameterDefinition("life_permissiveness", "Life Potential", string.Empty, GenerationParameterControlType.Number, "Controls native-life permissiveness. Low values require near-Earthlike conditions and penalize hostile factors hard; high values allow biospheres on marginal but still biologically plausible worlds."),
            new GenerationParameterDefinition("mainworld_policy", "Mainworld Policy", string.Empty, GenerationParameterControlType.Choice, "Controls whether Traveller-oriented flows should ignore, prefer, or require plausible mainworld-ready systems."),
        };
    }
}
