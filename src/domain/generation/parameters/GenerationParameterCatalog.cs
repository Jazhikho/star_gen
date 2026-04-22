using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Shared parameter metadata for object, system, and galaxy generation editors.
/// </summary>
public static class GenerationParameterCatalog
{
    /// <summary>
    /// Returns generation-facing system parameter definitions.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemDefinitions()
    {
        return FilterByClassification(
            BuildSystemDefinitionSet(),
            GenerationParameterClassification.GenerationPrior,
            GenerationParameterClassification.GeneratorOverride);
    }

    /// <summary>
    /// Returns science-backed system priors only.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemScienceDefinitions()
    {
        return FilterByClassification(
            BuildSystemDefinitionSet(),
            GenerationParameterClassification.GenerationPrior);
    }

    /// <summary>
    /// Returns non-scientific system override seams.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemOverrideDefinitions()
    {
        return FilterByClassification(
            BuildSystemDefinitionSet(),
            GenerationParameterClassification.GeneratorOverride);
    }

    /// <summary>
    /// Returns system runtime controls that should not be treated as science parameters.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemRuntimeDefinitions()
    {
        return FilterByClassification(
            BuildSystemDefinitionSet(),
            GenerationParameterClassification.RuntimeOrchestrationControl);
    }

    /// <summary>
    /// Returns system presentation controls that should not be treated as generation parameters.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemPresentationDefinitions()
    {
        return FilterByClassification(
            BuildSystemDefinitionSet(),
            GenerationParameterClassification.PresentationReadoutControl);
    }

    /// <summary>
    /// Returns all system editor definitions, including runtime and presentation controls.
    /// </summary>
    public static List<GenerationParameterDefinition> GetSystemEditorDefinitions()
    {
        return BuildSystemDefinitionSet();
    }

    /// <summary>
    /// Returns generation-facing galaxy parameter definitions.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyDefinitions()
    {
        return FilterByClassification(
            BuildGalaxyDefinitionSet(),
            GenerationParameterClassification.GenerationPrior,
            GenerationParameterClassification.GeneratorOverride);
    }

    /// <summary>
    /// Returns science-backed galaxy priors only.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyScienceDefinitions()
    {
        return FilterByClassification(
            BuildGalaxyDefinitionSet(),
            GenerationParameterClassification.GenerationPrior);
    }

    /// <summary>
    /// Returns non-scientific galaxy override seams.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyOverrideDefinitions()
    {
        return FilterByClassification(
            BuildGalaxyDefinitionSet(),
            GenerationParameterClassification.GeneratorOverride);
    }

    /// <summary>
    /// Returns galaxy presentation controls that should not be treated as generation parameters.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyPresentationDefinitions()
    {
        return FilterByClassification(
            BuildGalaxyDefinitionSet(),
            GenerationParameterClassification.PresentationReadoutControl);
    }

    /// <summary>
    /// Returns all galaxy editor definitions, including presentation controls.
    /// </summary>
    public static List<GenerationParameterDefinition> GetGalaxyEditorDefinitions()
    {
        return BuildGalaxyDefinitionSet();
    }

    /// <summary>
    /// Finds a system-editor definition by identifier.
    /// </summary>
    public static GenerationParameterDefinition? FindSystemDefinition(string parameterId)
    {
        return FindDefinition(BuildSystemDefinitionSet(), parameterId);
    }

    /// <summary>
    /// Finds a galaxy-editor definition by identifier.
    /// </summary>
    public static GenerationParameterDefinition? FindGalaxyDefinition(string parameterId)
    {
        return FindDefinition(BuildGalaxyDefinitionSet(), parameterId);
    }

    private static List<GenerationParameterDefinition> BuildSystemDefinitionSet()
    {
        return new List<GenerationParameterDefinition>
        {
            new GenerationParameterDefinition("generation_seed", "Seed", string.Empty, GenerationParameterControlType.Number, "The seed is the master number that makes generation repeatable.\nKeep the same seed and the same settings if you want the same system again."),
            new GenerationParameterDefinition("star_count_min", "Min Stars", string.Empty, GenerationParameterControlType.Number, "This is the fewest stars the system is allowed to have.\nRaising it guarantees more multi-star systems."),
            new GenerationParameterDefinition("star_count_max", "Max Stars", string.Empty, GenerationParameterControlType.Number, "This is the most stars the system is allowed to have.\nHigher values allow bigger star groups up to 10, but those crowded systems stay rare and less stable."),
            new GenerationParameterDefinition("spectral_class_hints", "Spectral Hints", string.Empty, GenerationParameterControlType.Text, "Use letters like G, K, M, L, T, or Y to push the generator toward certain star types.\nM favors cool red dwarfs.\nL, T, and Y favor brown dwarfs.\nA or B favors hotter, brighter stars.", supportsTarget: true),
            new GenerationParameterDefinition("system_age_years", "System Age", "Gyr", GenerationParameterControlType.Number, "Set this if you want the stars to share one age target.\nOlder systems can now yield subgiants, giants, and white dwarfs when the masses fit.\nYounger systems lean hotter and more active.", supportsTarget: true),
            new GenerationParameterDefinition("system_metallicity", "System Metallicity", "Zsun", GenerationParameterControlType.Number, "Metallicity means how rich the system is in heavy elements such as iron, silicon, and oxygen.\nHigher values usually mean more raw material for dust and rocky worlds.", supportsTarget: true),
            new GenerationParameterDefinition("stellar_imf_form", "IMF Form", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_form")),
            new GenerationParameterDefinition("stellar_imf_variation_mode", "IMF Shift", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_imf_variation_mode")),
            new GenerationParameterDefinition("stellar_isochrone_model", "Star Model", string.Empty, GenerationParameterControlType.Choice, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_isochrone_model")),
            new GenerationParameterDefinition("stellar_multiplicity_scale", "Companions", string.Empty, GenerationParameterControlType.Number, StellarScienceReferenceCatalog.GetTooltipSummary("stellar_multiplicity_scale")),
            new GenerationParameterDefinition("planet_mass_radius_model", "Mass-Radius Model", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_mass_radius_model")),
            new GenerationParameterDefinition("planet_envelope_loss_model", "Envelope Loss", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_envelope_loss_model")),
            new GenerationParameterDefinition("planet_habitable_zone_model", "HZ Model", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_habitable_zone_model")),
            new GenerationParameterDefinition("planet_gas_giant_formation_model", "Giant Formation", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_gas_giant_formation_model")),
            new GenerationParameterDefinition("planet_metallicity_coupling_strength", "Metallicity Coupling", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_metallicity_coupling_strength")),
            new GenerationParameterDefinition("planet_rogue_planet_allowance", "Rogue Allowance", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_rogue_planet_allowance")),
            new GenerationParameterDefinition("planet_moon_formation_bias", "Moon Bias", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_moon_formation_bias")),
            new GenerationParameterDefinition("planet_minor_body_outer_system_bias", "Outer Small Bodies", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_minor_body_outer_system_bias")),
            new GenerationParameterDefinition("life_framework", "Life Framework", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("life_framework")),
            new GenerationParameterDefinition("abiogenesis_model", "Abiogenesis Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("abiogenesis_model")),
            new GenerationParameterDefinition("complex_life_model", "Complex Life Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("complex_life_model")),
            new GenerationParameterDefinition("civilization_model", "Civilization Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("civilization_model")),
            new GenerationParameterDefinition("environmental_window_weight", "Environmental Window Weight", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("environmental_window_weight")),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Generator override seam.\nSelects the downstream compatibility profile.\nRealistic keeps StarGen's default scientific and physical worldbuilding path.\nTraveller, Cepheus, Starfinder, and Starforged each apply different RPG-facing override pressure to mainworld selection, settlement likelihood, and system flavor without replacing the whole generator.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("force_life_on_supportable_worlds", "Force Life If Supportable", string.Empty, GenerationParameterControlType.Toggle, "Generator override seam.\nWhen enabled, worlds that pass the biology support gate keep native life instead of rolling it stochastically.\nUnsupported worlds still remain lifeless.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("mainworld_policy", "Mainworld Policy", string.Empty, GenerationParameterControlType.Choice, "Generator override seam.\nControls whether compatibility-oriented generation should ignore, prefer, or require a plausible mainworld candidate.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_temperate_slot_fill_multiplier", "Temperate World Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values fill more temperate slots with worlds.\nLower values leave more of those slots empty in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_harsh_slot_fill_multiplier", "Harsh World Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values preserve more harsh-slot worlds.\nLower values prune harsh worlds more aggressively in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_terrestrial_world_weight_multiplier", "Mainworld Class Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values favor rocky and super-Earth mainworld candidates over mini-Neptunes and giants.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_native_life_probability_multiplier", "Native Life Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values make supportable worlds more likely to keep native life in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_colony_probability_multiplier", "Settlement Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values make colonies and inhabited outposts more common in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("include_asteroid_belts", "Asteroid Belts", string.Empty, GenerationParameterControlType.Toggle, "Runtime control.\nEnables or skips the downstream asteroid-belt generation stage.", GenerationParameterClassification.RuntimeOrchestrationControl),
            new GenerationParameterDefinition("generate_population", "Generate Population", string.Empty, GenerationParameterControlType.Toggle, "Runtime control.\nEnables or skips the downstream population pipeline for planets and moons.", GenerationParameterClassification.RuntimeOrchestrationControl),
            new GenerationParameterDefinition("show_traveller_readouts", "Show UWP Code", string.Empty, GenerationParameterControlType.Toggle, "Presentation control.\nShows Universal World Profile code when the current flow has enough information to derive it.", GenerationParameterClassification.PresentationReadoutControl),
        };
    }

    private static List<GenerationParameterDefinition> BuildGalaxyDefinitionSet()
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
            new GenerationParameterDefinition("planet_mass_radius_model", "Mass-Radius Model", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_mass_radius_model")),
            new GenerationParameterDefinition("planet_envelope_loss_model", "Envelope Loss", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_envelope_loss_model")),
            new GenerationParameterDefinition("planet_habitable_zone_model", "HZ Model", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_habitable_zone_model")),
            new GenerationParameterDefinition("planet_gas_giant_formation_model", "Giant Formation", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_gas_giant_formation_model")),
            new GenerationParameterDefinition("planet_metallicity_coupling_strength", "Metallicity Coupling", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_metallicity_coupling_strength")),
            new GenerationParameterDefinition("planet_rogue_planet_allowance", "Rogue Allowance", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_rogue_planet_allowance")),
            new GenerationParameterDefinition("planet_moon_formation_bias", "Moon Bias", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_moon_formation_bias")),
            new GenerationParameterDefinition("planet_minor_body_outer_system_bias", "Outer Small Bodies", string.Empty, GenerationParameterControlType.Choice, PlanetaryScienceReferenceCatalog.GetTooltipSummary("planet_minor_body_outer_system_bias")),
            new GenerationParameterDefinition("life_framework", "Life Framework", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("life_framework")),
            new GenerationParameterDefinition("abiogenesis_model", "Abiogenesis Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("abiogenesis_model")),
            new GenerationParameterDefinition("complex_life_model", "Complex Life Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("complex_life_model")),
            new GenerationParameterDefinition("civilization_model", "Civilization Model", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("civilization_model")),
            new GenerationParameterDefinition("environmental_window_weight", "Environmental Window Weight", string.Empty, GenerationParameterControlType.Choice, LifeScienceReferenceCatalog.GetTooltipSummary("environmental_window_weight")),
            new GenerationParameterDefinition("ruleset_mode", "Ruleset", string.Empty, GenerationParameterControlType.Choice, "Generator override seam.\nSelects the downstream compatibility profile.\nRealistic keeps StarGen's default scientific and physical worldbuilding path.\nTraveller, Cepheus, Starfinder, and Starforged each apply different RPG-facing override pressure to mainworld selection, settlement likelihood, and region flavor without replacing the whole generator.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("force_life_on_supportable_worlds", "Force Life If Supportable", string.Empty, GenerationParameterControlType.Toggle, "Generator override seam.\nWhen enabled, worlds that pass the biology support gate keep native life instead of rolling it stochastically.\nUnsupported worlds still remain lifeless.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("mainworld_policy", "Mainworld Policy", string.Empty, GenerationParameterControlType.Choice, "Generator override seam.\nControls whether compatibility-oriented generation should ignore, prefer, or require plausible mainworld-ready systems.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_temperate_slot_fill_multiplier", "Temperate World Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values fill more temperate slots with worlds.\nLower values leave more of those slots empty in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_harsh_slot_fill_multiplier", "Harsh World Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values preserve more harsh-slot worlds.\nLower values prune harsh worlds more aggressively in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_terrestrial_world_weight_multiplier", "Mainworld Class Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values favor rocky and super-Earth mainworld candidates over mini-Neptunes and giants.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_native_life_probability_multiplier", "Native Life Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values make supportable worlds more likely to keep native life in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("compatibility_colony_probability_multiplier", "Settlement Bias", string.Empty, GenerationParameterControlType.Number, "Generator override seam.\nHigher values make colonies and inhabited outposts more common in compatibility-oriented flows.", GenerationParameterClassification.GeneratorOverride),
            new GenerationParameterDefinition("show_traveller_readouts", "Show UWP Code", string.Empty, GenerationParameterControlType.Toggle, "Presentation control.\nShows Universal World Profile code when the current flow has enough information to derive it.", GenerationParameterClassification.PresentationReadoutControl),
        };
    }

    private static List<GenerationParameterDefinition> FilterByClassification(
        IReadOnlyList<GenerationParameterDefinition> definitions,
        params GenerationParameterClassification[] allowed)
    {
        HashSet<GenerationParameterClassification> allowedSet = new(allowed);
        List<GenerationParameterDefinition> filtered = new();
        foreach (GenerationParameterDefinition definition in definitions)
        {
            if (allowedSet.Contains(definition.Classification))
            {
                filtered.Add(definition);
            }
        }

        return filtered;
    }

    private static GenerationParameterDefinition? FindDefinition(
        IReadOnlyList<GenerationParameterDefinition> definitions,
        string parameterId)
    {
        foreach (GenerationParameterDefinition definition in definitions)
        {
            if (definition.Id == parameterId)
            {
                return definition;
            }
        }

        return null;
    }
}
