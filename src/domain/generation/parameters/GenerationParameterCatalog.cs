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
            new GenerationParameterDefinition("generation_seed", "Seed", string.Empty, GenerationParameterControlType.Number, "The seed is the master number that makes generation repeatable.\nKeep the same seed and the same settings if you want the same system again."),
            new GenerationParameterDefinition("star_count_min", "Min Stars", string.Empty, GenerationParameterControlType.Number, "This is the fewest stars the system is allowed to have.\nRaising it guarantees more multi-star systems."),
            new GenerationParameterDefinition("star_count_max", "Max Stars", string.Empty, GenerationParameterControlType.Number, "This is the most stars the system is allowed to have.\nHigher values allow bigger star groups up to 10, but those crowded systems stay rare and less stable."),
            new GenerationParameterDefinition("spectral_class_hints", "Spectral Hints", string.Empty, GenerationParameterControlType.Text, "Use letters like G, K, or M to push the generator toward certain star types.\nMore M hints favor cool red dwarfs.\nMore A or B hints favor hotter, brighter stars.", supportsTarget: true),
            new GenerationParameterDefinition("system_age_years", "System Age", "Gyr", GenerationParameterControlType.Number, "Set this if you want the stars to share one age target.\nOlder systems tend to feel more evolved.\nYounger systems lean hotter and more active.", supportsTarget: true),
            new GenerationParameterDefinition("system_metallicity", "System Metallicity", "Zsun", GenerationParameterControlType.Number, "Metallicity means how rich the system is in heavy elements such as iron, silicon, and oxygen.\nHigher values usually mean more raw material for dust and rocky worlds.", supportsTarget: true),
            new GenerationParameterDefinition("stellar_imf_form", "IMF Form", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_form")),
            new GenerationParameterDefinition("stellar_imf_variation_mode", "IMF Shift", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_variation_mode")),
            new GenerationParameterDefinition("stellar_isochrone_model", "Star Model", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_isochrone_model")),
            new GenerationParameterDefinition("stellar_multiplicity_scale", "Companions", string.Empty, GenerationParameterControlType.Number, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_multiplicity_scale")),
            new GenerationParameterDefinition("include_asteroid_belts", "Asteroid Belts", string.Empty, GenerationParameterControlType.Toggle, "Controls whether the belt stage participates in generation."),
            new GenerationParameterDefinition("generate_population", "Generate Population", string.Empty, GenerationParameterControlType.Toggle, "Enables the downstream population pipeline for planets and moons."),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Selects the downstream generation pipeline.\nRealistic keeps StarGen's default scientific and physical worldbuilding path.\nTraveller keeps normal galaxy structure but uses Traveller world-generation rules for supported mainworld outputs."),
            new GenerationParameterDefinition("show_traveller_readouts", "Traveller Readouts", string.Empty, GenerationParameterControlType.Toggle, "Shows derived Traveller or UWP-oriented readouts when the current flow has enough information."),
            new GenerationParameterDefinition("life_permissiveness", "Life Potential", string.Empty, GenerationParameterControlType.Number, "Controls native-life permissiveness.\nLow values require near-Earthlike conditions and penalize hostile factors hard.\nHigh values allow biospheres on marginal but still biologically plausible worlds."),
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
            new GenerationParameterDefinition("galaxy_seed", "Seed", string.Empty, GenerationParameterControlType.Number, "The seed is the master number that makes generation repeatable.\nThe same seed and the same settings give you the same galaxy again."),
            new GenerationParameterDefinition("galaxy_type", "Galaxy Family", string.Empty, GenerationParameterControlType.Choice, GalaxyScienceReferenceCatalog.GetTooltipSummary("galaxy_type")),
            new GenerationParameterDefinition("subtype_mode", "Subtype Bias", string.Empty, GenerationParameterControlType.Choice, GalaxyScienceReferenceCatalog.GetTooltipSummary("subtype_mode")),
            new GenerationParameterDefinition("num_arms", "Spiral Arms", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("num_arms")),
            new GenerationParameterDefinition("arm_pitch_angle_deg", "Arm Pitch", "deg", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("arm_pitch_angle_deg")),
            new GenerationParameterDefinition("arm_amplitude", "Arm Amplitude", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("arm_amplitude")),
            new GenerationParameterDefinition("bar_mode", "Bar Bias", string.Empty, GenerationParameterControlType.Choice, GalaxyScienceReferenceCatalog.GetTooltipSummary("bar_mode")),
            new GenerationParameterDefinition("arm_mechanism_preference", "Arm Mode", string.Empty, GenerationParameterControlType.Choice, GalaxyScienceReferenceCatalog.GetTooltipSummary("arm_mechanism_preference")),
            new GenerationParameterDefinition("halo_mass_log10_solar", "Halo Mass", "log10(Msun)", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("halo_mass_log10_solar")),
            new GenerationParameterDefinition("environment_density_index", "Environment", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("environment_density_index")),
            new GenerationParameterDefinition("bulge_intensity", "Bulge Intensity", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("bulge_intensity")),
            new GenerationParameterDefinition("bulge_radius_pc", "Bulge Radius", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("bulge_radius_pc")),
            new GenerationParameterDefinition("radius_pc", "Radius", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("radius_pc")),
            new GenerationParameterDefinition("disk_scale_length_pc", "Disk Scale Length", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("disk_scale_length_pc")),
            new GenerationParameterDefinition("disk_scale_height_pc", "Disk Scale Height", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("disk_scale_height_pc")),
            new GenerationParameterDefinition("star_density_multiplier", "Density Multiplier", "x", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("star_density_multiplier")),
            new GenerationParameterDefinition("ellipticity", "Ellipticity", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("ellipticity")),
            new GenerationParameterDefinition("irregularity_scale", "Irregularity", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("irregularity_scale")),
            new GenerationParameterDefinition("ghz_inner_radius_pc", "GHZ Inner", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("ghz_inner_radius_pc")),
            new GenerationParameterDefinition("ghz_outer_radius_pc", "GHZ Outer", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("ghz_outer_radius_pc")),
            new GenerationParameterDefinition("ghz_transition_width_pc", "GHZ Width", "pc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("ghz_transition_width_pc")),
            new GenerationParameterDefinition("metallicity_gradient_dex_per_kpc", "Metallicity Gradient", "dex/kpc", GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("metallicity_gradient_dex_per_kpc")),
            new GenerationParameterDefinition("star_formation_efficiency", "Star Formation Efficiency", string.Empty, GenerationParameterControlType.Number, GalaxyScienceReferenceCatalog.GetTooltipSummary("star_formation_efficiency")),
            new GenerationParameterDefinition("stellar_imf_form", "IMF Form", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_form")),
            new GenerationParameterDefinition("stellar_imf_variation_mode", "IMF Shift", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_variation_mode")),
            new GenerationParameterDefinition("stellar_isochrone_model", "Star Model", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_isochrone_model")),
            new GenerationParameterDefinition("stellar_multiplicity_scale", "Companions", string.Empty, GenerationParameterControlType.Number, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_multiplicity_scale")),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Selects the downstream generation pipeline.\nRealistic keeps StarGen's default scientific and physical worldbuilding path.\nTraveller keeps normal galaxy structure but uses Traveller world-generation rules for supported mainworld outputs."),
            new GenerationParameterDefinition("show_traveller_readouts", "Traveller Readouts", string.Empty, GenerationParameterControlType.Toggle, "Shows derived Traveller or UWP-oriented readouts when the current flow has enough information."),
            new GenerationParameterDefinition("life_permissiveness", "Life Potential", string.Empty, GenerationParameterControlType.Number, "Controls native-life permissiveness.\nLow values require near-Earthlike conditions and penalize hostile factors hard.\nHigh values allow biospheres on marginal but still biologically plausible worlds."),
            new GenerationParameterDefinition("mainworld_policy", "Mainworld Policy", string.Empty, GenerationParameterControlType.Choice, "Controls whether Traveller-oriented flows should ignore, prefer, or require plausible mainworld-ready systems."),
        };
    }
}
