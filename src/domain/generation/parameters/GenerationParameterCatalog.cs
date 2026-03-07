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
            new GenerationParameterDefinition("galaxy_type", "Galaxy Type", string.Empty, GenerationParameterControlType.Choice, "Selects which morphology rules are active."),
            new GenerationParameterDefinition("num_arms", "Spiral Arms", string.Empty, GenerationParameterControlType.Number, "Only applies to spiral galaxies."),
            new GenerationParameterDefinition("arm_pitch_angle_deg", "Arm Pitch", "deg", GenerationParameterControlType.Number, "Controls how tightly the spiral arms wind."),
            new GenerationParameterDefinition("arm_amplitude", "Arm Amplitude", string.Empty, GenerationParameterControlType.Number, "Biases star density toward arm structures."),
            new GenerationParameterDefinition("bulge_intensity", "Bulge Intensity", string.Empty, GenerationParameterControlType.Number, "Controls the central density concentration."),
            new GenerationParameterDefinition("bulge_radius_pc", "Bulge Radius", "pc", GenerationParameterControlType.Number, "Sets the central bulge size."),
            new GenerationParameterDefinition("radius_pc", "Radius", "pc", GenerationParameterControlType.Number, "Sets the disk radius and overall footprint."),
            new GenerationParameterDefinition("disk_scale_length_pc", "Disk Scale Length", "pc", GenerationParameterControlType.Number, "Controls radial density falloff in disk-like galaxies."),
            new GenerationParameterDefinition("disk_scale_height_pc", "Disk Scale Height", "pc", GenerationParameterControlType.Number, "Controls vertical thickness of the disk."),
            new GenerationParameterDefinition("star_density_multiplier", "Density Multiplier", "x", GenerationParameterControlType.Number, "Scales total sampled density without changing morphology."),
            new GenerationParameterDefinition("ellipticity", "Ellipticity", string.Empty, GenerationParameterControlType.Number, "Only applies to elliptical galaxies."),
            new GenerationParameterDefinition("irregularity_scale", "Irregularity", string.Empty, GenerationParameterControlType.Number, "Only applies to irregular galaxies."),
        };
    }
}
