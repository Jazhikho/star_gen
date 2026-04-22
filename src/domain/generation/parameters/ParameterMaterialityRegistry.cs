using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Machine-readable parameter-to-effect map used by tests and audit docs.
/// </summary>
public static class ParameterMaterialityRegistry
{
    /// <summary>
    /// Describes where a parameter is surfaced and how it affects downstream output.
    /// </summary>
    public sealed class Entry
    {
        public Entry(
            string id,
            GenerationParameterClassification classification,
            IReadOnlyList<string> surfacedIn,
            IReadOnlyList<string> serializedThrough,
            IReadOnlyList<string> derivedThrough,
            IReadOnlyList<string> consumers,
            IReadOnlyList<string> outputs)
        {
            Id = id;
            Classification = classification;
            SurfacedIn = surfacedIn;
            SerializedThrough = serializedThrough;
            DerivedThrough = derivedThrough;
            Consumers = consumers;
            Outputs = outputs;
        }

        public string Id { get; }

        public GenerationParameterClassification Classification { get; }

        public IReadOnlyList<string> SurfacedIn { get; }

        public IReadOnlyList<string> SerializedThrough { get; }

        public IReadOnlyList<string> DerivedThrough { get; }

        public IReadOnlyList<string> Consumers { get; }

        public IReadOnlyList<string> Outputs { get; }
    }

    /// <summary>
    /// Returns the materiality entry for a surfaced parameter.
    /// </summary>
    public static Entry? GetEntry(string parameterId)
    {
        return parameterId switch
        {
            "galaxy_seed" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen seed control" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxySpec.GalaxySeed", "GalaxyRealismProfileBuilder.Build(...)" },
                new[] { "Galaxy.Generate(...)", "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "repeatable galaxy structure", "repeatable regional stellar context" }),
            "generation_seed" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "SystemGenerationScreen seed control" },
                new[] { "SolarSystemSpec" },
                new[] { "system seed family", "planetary population seeds" },
                new[] { "StellarConfigGenerator.Generate(...)", "SystemPlanetGenerator.Generate(...)", "SystemMoonGenerator.Generate(...)" },
                new[] { "repeatable stellar layout", "repeatable planet and moon outcomes" }),
            "galaxy_type" or "subtype_mode" or "bar_mode" or "arm_mechanism_preference" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen type section" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxyRealismProfileBuilder.Build(...)", "GalaxySpec.ApplyToSpec(...)" },
                new[] { "Galaxy.Generate(...)", "GalaxyDensityModelFactory.Create(...)" },
                new[] { "resolved galaxy family", "disk-vs-bulge structure", "arm/bar behavior" }),
            "num_arms" or "arm_pitch_angle_deg" or "arm_amplitude" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen structure section" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxyRealismProfileBuilder.Build(...)", "GalaxySpec arm fields" },
                new[] { "Galaxy.Generate(...)", "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "spiral arm geometry", "arm/interarm regional context" }),
            "halo_mass_log10_solar" or "environment_density_index" or "star_formation_efficiency" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen scientific priors section" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxyRealismProfileBuilder.Build(...)", "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "Galaxy.Generate(...)", "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "stellar density field", "metallicity prior", "habitability and multiplicity context" }),
            "bulge_intensity" or "bulge_radius_pc" or "radius_pc" or "disk_scale_length_pc" or "disk_scale_height_pc" or "star_density_multiplier" or "ellipticity" or "irregularity_scale" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen structure/size sections" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxyRealismProfileBuilder.Build(...)", "GalaxySpec size and shape fields" },
                new[] { "Galaxy.Generate(...)", "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "global morphology", "local density field", "sampled region classification" }),
            "ghz_inner_radius_pc" or "ghz_outer_radius_pc" or "ghz_transition_width_pc" or "metallicity_gradient_dex_per_kpc" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen size section" },
                new[] { "GalaxyConfig" },
                new[] { "GalaxyScientificFieldEvaluator.Evaluate(...)" },
                new[] { "GalaxyScientificFieldEvaluator.Evaluate(...)", "GalaxySystemGenerator.CreateSpecFromStar(...)" },
                new[] { "regional metallicity prior", "galactic habitability weighting", "system metallicity context" }),
            "star_count_min" or "star_count_max" or "spectral_class_hints" or "system_age_years" or "system_metallicity" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "SystemGenerationScreen system controls" },
                new[] { "SolarSystemSpec" },
                new[] { "StellarConfigGenerator inputs", "PlanetarySystemState.Build(...)" },
                new[] { "StellarConfigGenerator.Generate(...)", "SystemPlanetGenerator.Generate(...)" },
                new[] { "stellar scaffold", "orbital architecture context", "planet class weighting" }),
            "stellar_imf_form" or "stellar_imf_variation_mode" or "stellar_isochrone_model" or "stellar_multiplicity_scale" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen stellar section", "SystemGenerationScreen stellar priors" },
                new[] { "StellarGenerationProfile" },
                new[] { "StellarProfile on GalaxyConfig/SolarSystemSpec" },
                new[] { "StellarMassSampler", "StarGenerator", "StellarIsochroneApproximator", "StellarConfigGenerator" },
                new[] { "stellar mass mix", "stellar evolution outputs", "multiplicity and companion layout" }),
            "planet_mass_radius_model" or "planet_envelope_loss_model" or "planet_habitable_zone_model" or "planet_gas_giant_formation_model" or "planet_metallicity_coupling_strength" or "planet_rogue_planet_allowance" or "planet_moon_formation_bias" or "planet_minor_body_outer_system_bias" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen planetary section", "SystemGenerationScreen planetary priors" },
                new[] { "PlanetaryGenerationProfile" },
                new[] { "PlanetarySystemState.Build(...)", "OrbitHost.CalculateZones(...)" },
                new[] { "SystemPlanetGenerator", "SystemMoonGenerator", "SystemAsteroidGenerator", "ProfileGenerator" },
                new[] { "planet class mix", "moon architecture", "small-body composition", "environment and habitability context" }),
            "life_framework" or "abiogenesis_model" or "complex_life_model" or "civilization_model" or "environmental_window_weight" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GenerationPrior,
                new[] { "GalaxyGenerationScreen life section", "SystemGenerationScreen life section", "ObjectGenerationScreen life section" },
                new[] { "GenerationUseCaseSettings" },
                new[] { "LifePotentialModeling", "BiologySupportEvaluator", "PopulationProbability" },
                new[] { "BiologySupportEvaluator", "PopulationLikelihood", "PopulationGenerator", "SentientWorldProfileBuilder" },
                new[] { "native-life gating", "sentience and civilization odds", "population and settlement outcomes" }),
            "ruleset_mode" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GeneratorOverride,
                new[] { "GalaxyGenerationScreen rules panel", "SystemGenerationScreen rules panel", "ObjectGenerationScreen overrides panel" },
                new[] { "GenerationUseCaseSettings" },
                new[] { "RpgCompatibilityProfile.Resolve(...)" },
                new[] { "GalaxySystemGenerator.CreateSpecFromStar(...)", "SystemPlanetGenerator", "PopulationProbability", "InspectorPanel" },
                new[] { "mainworld pressure", "settlement bias", "RPG-facing readout defaults" }),
            "force_life_on_supportable_worlds" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GeneratorOverride,
                new[] { "GalaxyGenerationScreen rules panel", "SystemGenerationScreen rules panel", "ObjectGenerationScreen life overrides" },
                new[] { "GenerationUseCaseSettings" },
                new[] { "PopulationLikelihood.ShouldGenerateNatives(...)" },
                new[] { "PopulationLikelihood.ShouldGenerateNatives(...)" },
                new[] { "native-life retention on supportable worlds" }),
            "mainworld_policy" or "compatibility_temperate_slot_fill_multiplier" or "compatibility_harsh_slot_fill_multiplier" or "compatibility_terrestrial_world_weight_multiplier" or "compatibility_native_life_probability_multiplier" or "compatibility_colony_probability_multiplier" => CreateEntry(
                parameterId,
                GenerationParameterClassification.GeneratorOverride,
                new[] { "GalaxyGenerationScreen rules overrides", "SystemGenerationScreen rules overrides" },
                new[] { "GenerationUseCaseSettings", "RpgCompatibilityProfile clone" },
                new[] { "GetCompatibilityProfile()", "compatibility multiplier resolution" },
                new[] { "SystemPlanetGenerator slot and class weighting", "PopulationProbability", "TravellerSystemGenerator.ApplyTravellerMainworld(...)" },
                new[] { "mainworld availability", "native-life pressure", "colony pressure", "world-class bias" }),
            "include_asteroid_belts" => CreateEntry(
                parameterId,
                GenerationParameterClassification.RuntimeOrchestrationControl,
                new[] { "SystemGenerationScreen system controls" },
                new[] { "SolarSystemSpec.IncludeAsteroidBelts" },
                new[] { "SystemFixtureGenerator.GenerateSystem(...)" },
                new[] { "SystemAsteroidGenerator.Generate(...)" },
                new[] { "presence or absence of asteroid belts" }),
            "generate_population" => CreateEntry(
                parameterId,
                GenerationParameterClassification.RuntimeOrchestrationControl,
                new[] { "SolarSystemSpec runtime flag", "compatibility-driven system generation" },
                new[] { "SolarSystemSpec.GeneratePopulation" },
                new[] { "GalaxySystemGenerator.CreateSpecFromStar(...)" },
                new[] { "PopulationGenerator.Generate(...)" },
                new[] { "presence or absence of colonies, natives, and sentient-world profiles" }),
            "show_traveller_readouts" => CreateEntry(
                parameterId,
                GenerationParameterClassification.PresentationReadoutControl,
                new[] { "GalaxyGenerationScreen rules panel", "SystemGenerationScreen rules panel", "ObjectGenerationScreen overrides panel", "viewer inspector gating" },
                new[] { "GenerationUseCaseSettings" },
                new[] { "InspectorPanel.ShouldShowTravellerSection(...)" },
                new[] { "InspectorPanel.PopulateTravellerSection(...)", "viewer summaries" },
                new[] { "UWP visibility only" }),
            _ => null,
        };
    }

    private static Entry CreateEntry(
        string id,
        GenerationParameterClassification classification,
        IReadOnlyList<string> surfacedIn,
        IReadOnlyList<string> serializedThrough,
        IReadOnlyList<string> derivedThrough,
        IReadOnlyList<string> consumers,
        IReadOnlyList<string> outputs)
    {
        return new Entry(id, classification, surfacedIn, serializedThrough, derivedThrough, consumers, outputs);
    }
}
