using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Machine-readable source and tuning metadata for generator coefficients that materially affect science output.
/// </summary>
public sealed class ScienceTuningEntry
{
    public ScienceTuningEntry(
        string id,
        string label,
        string valueOrRange,
        string units,
        IReadOnlyList<string> sourceIds,
        string supportedClaim,
        string consumingGenerator,
        string publicControlId,
        string implementationStatus,
        bool requiresHumanAudit)
    {
        Id = id;
        Label = label;
        ValueOrRange = valueOrRange;
        Units = units;
        SourceIds = sourceIds;
        SupportedClaim = supportedClaim;
        ConsumingGenerator = consumingGenerator;
        PublicControlId = publicControlId;
        ImplementationStatus = implementationStatus;
        RequiresHumanAudit = requiresHumanAudit;
    }

    public string Id { get; }

    public string Label { get; }

    public string ValueOrRange { get; }

    public string Units { get; }

    public IReadOnlyList<string> SourceIds { get; }

    public string SupportedClaim { get; }

    public string ConsumingGenerator { get; }

    public string PublicControlId { get; }

    public string ImplementationStatus { get; }

    public bool RequiresHumanAudit { get; }
}

/// <summary>
/// Canonical registry for source-backed generator constants and StarGen synthesis parameters.
/// </summary>
public static class ScienceTuningRegistry
{
    private static readonly Dictionary<string, ScienceTuningEntry> Entries = BuildEntries();

    public static IReadOnlyCollection<ScienceTuningEntry> GetEntries()
    {
        return Entries.Values;
    }

    public static ScienceTuningEntry? GetEntry(string id)
    {
        if (Entries.TryGetValue(id, out ScienceTuningEntry? entry))
        {
            return entry;
        }

        return null;
    }

    public static bool HasEntry(string id)
    {
        return Entries.ContainsKey(id);
    }

    public static IReadOnlyList<ScienceTuningEntry> GetEntriesForPublicControl(string publicControlId)
    {
        List<ScienceTuningEntry> matches = new();
        foreach (ScienceTuningEntry entry in Entries.Values)
        {
            if (entry.PublicControlId == publicControlId)
            {
                matches.Add(entry);
            }
        }

        return matches;
    }

    private static Dictionary<string, ScienceTuningEntry> BuildEntries()
    {
        Dictionary<string, ScienceTuningEntry> entries = new();

        AddCatalogEntry(entries, "galaxy.galaxy_type", "Galaxy family", "enum", "model", "galaxy_type", "GalaxyRealismProfileBuilder", "Galaxy morphology changes disk, bulge, arm, and environment-dependent system context.", false);
        AddCatalogEntry(entries, "galaxy.subtype_mode", "Galaxy subtype bias", "enum", "model", "subtype_mode", "GalaxyRealismProfileBuilder", "Subtype bias changes bulge/disk balance and downstream structural context.", false);
        AddCatalogEntry(entries, "galaxy.num_arms", "Spiral arm count", "integer control", "arms", "num_arms", "SpiralDensityModel", "Arm count changes spiral structure and regional arm/interarm context.", false);
        AddCatalogEntry(entries, "galaxy.arm_pitch_angle_deg", "Spiral pitch angle", "continuous control", "deg", "arm_pitch_angle_deg", "SpiralDensityModel", "Pitch angle changes how tightly spiral arms wind through the disk.", false);
        AddCatalogEntry(entries, "galaxy.arm_amplitude", "Spiral arm amplitude", "continuous control", "scalar", "arm_amplitude", "SpiralDensityModel", "Arm amplitude changes how strongly star-forming structure concentrates in arms.", false);
        AddCatalogEntry(entries, "galaxy.bar_mode", "Bar mode", "enum", "model", "bar_mode", "GalaxyRealismProfileBuilder", "Bar bias changes central structure and inner-disk morphology.", false);
        AddCatalogEntry(entries, "galaxy.arm_mechanism_preference", "Spiral arm mechanism preference", "enum", "model", "arm_mechanism_preference", "GalaxyRealismProfileBuilder", "Arm mechanism changes whether arms are orderly, multi-armed, or patchy.", false);
        AddCatalogEntry(entries, "galaxy.halo_mass_log10_solar", "Halo mass", "continuous control", "log10 solar masses", "halo_mass_log10_solar", "GalaxyRealismProfileBuilder", "Halo mass changes galaxy scale and stellar-density context.", false);
        AddCatalogEntry(entries, "galaxy.environment_density_index", "Environment density", "continuous control", "scalar", "environment_density_index", "GalaxyRealismProfileBuilder", "Environment density shifts morphology toward field-like or cluster-like outcomes.", false);
        AddCatalogEntry(entries, "galaxy.bulge_intensity", "Bulge intensity", "continuous control", "scalar", "bulge_intensity", "GalaxyRealismProfileBuilder", "Bulge intensity changes central dominance and inner-galaxy context.", false);
        AddCatalogEntry(entries, "galaxy.bulge_radius_pc", "Bulge radius", "continuous control", "pc", "bulge_radius_pc", "GalaxyRealismProfileBuilder", "Bulge radius changes how far central structure influences the galaxy.", false);
        AddCatalogEntry(entries, "galaxy.radius_pc", "Galaxy radius", "continuous control", "pc", "radius_pc", "GalaxyRealismProfileBuilder", "Galaxy radius changes the physical scale over which stars and systems are distributed.", false);
        AddCatalogEntry(entries, "galaxy.disk_scale_length_pc", "Disk scale length", "continuous control", "pc", "disk_scale_length_pc", "GalaxyRealismProfileBuilder", "Disk scale length changes radial disk falloff.", false);
        AddCatalogEntry(entries, "galaxy.disk_scale_height_pc", "Disk scale height", "continuous control", "pc", "disk_scale_height_pc", "GalaxyRealismProfileBuilder", "Disk scale height changes vertical thickness.", false);
        AddCatalogEntry(entries, "galaxy.star_density_multiplier", "Star density multiplier", "continuous control", "scalar", "star_density_multiplier", "GalaxyScientificFieldEvaluator", "Star density multiplier changes crowding without replacing the morphology model.", false);
        AddCatalogEntry(entries, "galaxy.ellipticity", "Ellipticity", "continuous control", "scalar", "ellipticity", "GalaxyRealismProfileBuilder", "Ellipticity changes flattening and projected shape.", false);
        AddCatalogEntry(entries, "galaxy.irregularity_scale", "Irregularity scale", "continuous control", "scalar", "irregularity_scale", "GalaxyRealismProfileBuilder", "Irregularity scale changes clumpiness and lopsidedness.", false);
        AddCatalogEntry(entries, "galaxy.ghz_inner_radius_pc", "GHZ inner radius", "continuous control", "pc", "ghz_inner_radius_pc", "GalaxyScientificFieldEvaluator", "GHZ inner radius changes galactic habitability weighting near the center.", false);
        AddCatalogEntry(entries, "galaxy.ghz_outer_radius_pc", "GHZ outer radius", "continuous control", "pc", "ghz_outer_radius_pc", "GalaxyScientificFieldEvaluator", "GHZ outer radius changes how far habitable-zone weighting extends into the disk.", false);
        AddCatalogEntry(entries, "galaxy.ghz_transition_width_pc", "GHZ transition width", "continuous control", "pc", "ghz_transition_width_pc", "GalaxyScientificFieldEvaluator", "GHZ transition width changes whether habitability context fades sharply or gradually.", false);
        AddCatalogEntry(entries, "galaxy.metallicity_gradient_dex_per_kpc", "Metallicity gradient", "continuous control", "dex/kpc", "metallicity_gradient_dex_per_kpc", "GalaxyScientificFieldEvaluator", "Metallicity gradient changes regional planet-building material context.", false);
        AddCatalogEntry(entries, "galaxy.star_formation_efficiency", "Star formation efficiency", "continuous control", "scalar", "star_formation_efficiency", "GalaxyScientificFieldEvaluator", "Star formation efficiency changes young-star and cluster-context weighting.", false);

        AddCatalogEntry(entries, "stellar.imf_form", "IMF form", "enum", "model", "stellar_imf_form", "StellarMassSampler", "IMF form changes the stellar and brown-dwarf mass mix.", false);
        AddCatalogEntry(entries, "stellar.imf_variation_mode", "IMF variation mode", "enum", "model", "stellar_imf_variation_mode", "StellarMassSampler", "IMF variation allows metallicity and age to nudge the mass mix.", false);
        AddCatalogEntry(entries, "stellar.isochrone_model", "Stellar isochrone model", "enum", "model", "stellar_isochrone_model", "StellarIsochroneApproximator", "Isochrone model changes mass-age-metallicity to luminosity, temperature, radius, and stage mapping.", false);
        AddCatalogEntry(entries, "stellar.multiplicity_scale", "Stellar multiplicity scale", "continuous control", "scalar", "stellar_multiplicity_scale", "StellarConfigGenerator", "Multiplicity scale changes companion frequency and hierarchy construction.", false);

        AddCatalogEntry(entries, "planet.mass_radius_model", "Planet mass-radius model", "enum", "model", "planet_mass_radius_model", "ProfileGenerator", "Mass-radius model changes radius, density, gravity, and composition ambiguity.", false);
        AddCatalogEntry(entries, "planet.envelope_loss_model", "Envelope-loss model", "enum", "model", "planet_envelope_loss_model", "SystemPlanetGenerator", "Envelope-loss model changes stripped-world, mini-Neptune, and atmosphere-retention outcomes.", false);
        AddCatalogEntry(entries, "planet.habitable_zone_model", "Habitable-zone model", "enum", "model", "planet_habitable_zone_model", "OrbitHost", "HZ model changes temperate orbit weighting and environmental scoring.", false);
        AddCatalogEntry(entries, "planet.gas_giant_formation_model", "Gas-giant formation model", "enum", "model", "planet_gas_giant_formation_model", "PlanetarySystemState", "Gas-giant model changes giant-world weighting from core-accretion or pebble-assisted assumptions.", false);
        AddCatalogEntry(entries, "planet.metallicity_coupling_strength", "Metallicity coupling strength", "enum", "model", "planet_metallicity_coupling_strength", "PlanetarySystemState", "Metallicity coupling changes solid and giant-planet weighting.", false);
        AddCatalogEntry(entries, "planet.rogue_planet_allowance", "Rogue planet allowance", "enum", "model", "planet_rogue_planet_allowance", "PlanetarySystemState", "Rogue allowance changes ejection/scattering pressure and free-floating-world context.", false);
        AddCatalogEntry(entries, "planet.moon_formation_bias", "Moon formation bias", "enum", "model", "planet_moon_formation_bias", "SystemMoonGenerator", "Moon bias changes regular, captured, and ice-giant moon architecture.", false);
        AddCatalogEntry(entries, "planet.minor_body_outer_system_bias", "Outer minor-body bias", "enum", "model", "planet_minor_body_outer_system_bias", "SystemAsteroidGenerator", "Outer minor-body bias changes asteroid and small-body placement/composition context.", false);
        AddCatalogEntry(entries, "planet.comet_nucleus_model", "Comet nucleus model", "enum", "model", "comet_nucleus_model", "CometGenerator", "Comet nucleus model changes physical radius priors and keeps compatibility behavior explicit.", false);
        AddCatalogEntry(entries, "planet.comet_activity_model", "Comet activity model", "enum", "model", "comet_activity_model", "CometGenerator", "Comet activity model changes active, dormant, and extinct state odds.", false);
        AddCatalogEntry(entries, "planet.comet_size_scale", "Comet size scale", "continuous control", "scalar", "comet_size_scale", "CometGenerator", "Comet size scale provides continuous uncertainty around source-shaped radius priors.", false);
        AddCatalogEntry(entries, "planet.minor_body_population_slope", "Minor-body population slope", "continuous control", "slope", "minor_body_population_slope", "CometGenerator", "Minor-body slope changes the relative mix of small and large minor bodies.", false);
        AddCatalogEntry(entries, "planet.disk_radius_scale", "Disk radius scale", "continuous control", "scalar", "planet_disk_radius_scale", "PlanetarySystemState", "Disk radius scale changes gas budget and giant-formation context.", true);
        AddCatalogEntry(entries, "planet.dust_to_gas_scale", "Dust-to-gas scale", "continuous control", "scalar", "planet_dust_to_gas_scale", "PlanetarySystemState", "Dust-to-gas scale changes solid budget and core-growth context.", true);
        AddCatalogEntry(entries, "planet.fragmentation_velocity_model", "Fragmentation velocity model", "enum", "model", "planet_fragmentation_velocity_model", "PlanetarySystemState", "Fragmentation model changes pebble-assisted growth weighting.", true);
        AddCatalogEntry(entries, "planet.giant_origin_band", "Giant planet origin band", "enum", "model", "planet_giant_origin_band_model", "PlanetarySystemState", "Giant-origin band changes where gas giants are favored relative to the snow line.", true);

        AddCatalogEntry(entries, "life.life_framework", "Life framework", "enum", "model", "life_framework", "LifePotentialModeling", "Life framework changes abiogenesis, complex life, and civilization defaults together.", true);
        AddCatalogEntry(entries, "life.abiogenesis_model", "Abiogenesis model", "enum", "model", "abiogenesis_model", "BiologySupportEvaluator", "Abiogenesis model changes simple-life emergence odds after support scoring.", true);
        AddCatalogEntry(entries, "life.complex_life_model", "Complex-life model", "enum", "model", "complex_life_model", "BiologySupportEvaluator", "Complex-life model changes how hard rich ecosystems are after microbial life.", true);
        AddCatalogEntry(entries, "life.civilization_model", "Civilization model", "enum", "model", "civilization_model", "PopulationLikelihood", "Civilization model changes late sentience and technology bottlenecks.", true);
        AddCatalogEntry(entries, "life.environmental_window_weight", "Environmental window weight", "enum", "model", "environmental_window_weight", "BiologySupportEvaluator", "Environmental-window weight changes the value of long stable habitable intervals.", true);
        AddCatalogEntry(entries, "life.subsurface_habitability_model", "Subsurface habitability model", "enum", "model", "subsurface_habitability_model", "BiologySupportEvaluator", "Subsurface model changes whether protected oceans also require dark-biosphere energy proxies.", false);
        AddCatalogEntry(entries, "life.dark_biosphere_energy", "Dark-biosphere energy scale", "continuous control", "scalar", "dark_biosphere_energy_scale", "BiologySupportEvaluator", "Dark-biosphere energy scale changes subsurface chemical-energy support.", false);

        AddCatalogEntry(entries, "sentient.social_scale_model", "Sentient-world social-scale model", "enum", "model", "sentient_social_scale_model", "SentientWorldProfileBuilder", "Social-scale model changes how population, groups, and administration combine.", true);
        AddCatalogEntry(entries, "sentient.tech_diffusion_model", "Technology diffusion model", "enum", "model", "sentient_technology_diffusion_model", "SentientWorldProfileBuilder", "Technology diffusion model changes adoption capacity separately from highest available technology.", true);
        AddCatalogEntry(entries, "sentient.economic_complexity_model", "Economic-complexity model", "enum", "model", "sentient_economic_complexity_model", "SentientWorldProfileBuilder", "Economic-complexity model changes capability-breadth and binding-constraint proxies.", true);
        AddCatalogEntry(entries, "sentient.legitimacy_model", "Legitimacy model", "enum", "model", "sentient_legitimacy_model", "SentientWorldProfileBuilder", "Legitimacy model changes internal/external norm and legal-reach proxies.", true);

        AddDirectEntry(entries, "comet.jfc_bauer_radius_m", "Bauer-style Jupiter-family comet radius prior", "median 600; model spread 0.45-1.85", "m", new[] { "baueretal2017" }, "Jupiter-family comet nuclei should be centered around sub-kilometer to kilometer-scale radii rather than the older broad range.", "CometGenerator", "comet_nucleus_model", "implemented", false);
        AddDirectEntry(entries, "comet.legacy_radius_m", "Compatibility wide comet radius prior", "small 1000-12000; large 7000-30000", "m", new[] { "baueretal2017" }, "The previous broad range remains available only as an explicit stylized compatibility model.", "CometGenerator", "comet_nucleus_model", "compatibility tuning", false);
        AddDirectEntry(entries, "comet.activity_fraction", "Comet activity thresholds", "survey 0.52/0.84; active-rich 0.70/0.90; dormant-rich 0.38/0.88", "probability", new[] { "baueretal2017" }, "Comet activity state should be an explicit activity model rather than an undocumented roll.", "CometGenerator", "comet_activity_model", "implemented as StarGen tuning around source activity framing", false);
        AddDirectEntry(entries, "galaxy.bar_strength_coefficients", "Galaxy bar-strength coefficients", "0.35 baseline plus 0.60 bulge-coupled lift", "scalar", new[] { "diazgarcia2016" }, "Bar-strength numerics are StarGen tuning layered on a source-backed barred-disk framework.", "GalaxyRealismProfileBuilder", "bar_mode", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "galaxy.sersic_band_coefficients", "Elliptical Sersic bands", "intermediate 3.2-4.4; giant 4.0-5.5", "Sersic index", new[] { "oohama2009", "laurikainen2010" }, "Elliptical and bulge concentration bands are StarGen tuning inside broad source-backed structural families.", "GalaxyRealismProfileBuilder", "bulge_intensity", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "galaxy.spiral_density_coefficients", "Spiral density contrast coefficients", "arm/interarm and pitch-angle weighting family", "scalar", new[] { "hart2017", "lingard2021", "kennicutt1998" }, "Spiral density coefficients are deterministic StarGen tuning around source-backed arm structure and star-formation context.", "SpiralDensityModel", "arm_amplitude", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "stellar.lifetime_exponent_coefficients", "Stellar lifetime approximation exponents", "piecewise 2.1, 2.5, 2.9", "power-law exponent", new[] { "hurley2000", "choi2016", "bressan2012" }, "Compact lifetime exponents are StarGen tuning around analytic and grid-based stellar-evolution frameworks.", "StellarIsochroneApproximator", "stellar_isochrone_model", "implemented as StarGen approximation; human verification pending", false);
        AddDirectEntry(entries, "stellar.imf_sampling_coefficients", "IMF sampling coefficients", "canonical and metallicity/age modulation weights", "scalar", new[] { "kroupa2001", "chabrier2003", "li2023", "kirkpatrick2024" }, "IMF sampling coefficients are StarGen deterministic approximations around the selected IMF family.", "StellarMassSampler", "stellar_imf_form", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "stellar.companion_architecture_coefficients", "Companion architecture coefficients", "period, hierarchy, and low-mass companion weighting family", "scalar", new[] { "duchene2013", "raghavan2010", "tokovinin2021", "moedistefano2017" }, "Companion architecture coefficients are deterministic tuning around observed multiplicity and hierarchy patterns.", "StellarConfigGenerator", "stellar_multiplicity_scale", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.formation_state_coefficients", "Planetary formation-state coefficients", "solid/gas/migration/scattering composite family", "scalar", new[] { "pascucci2016", "ribas2015", "mordasini2007", "lambrechtsjohansen2012", "fernandes2019", "izidoro2017", "tanakatakeuchiward2002" }, "Formation-state coefficients translate source-backed formation pressures into deterministic top-down generation state.", "PlanetarySystemState", "planet_gas_giant_formation_model", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.class_zone_weight_tables", "Planet class zone weight tables", "inner/habitable/outer class-weight dictionaries plus host demographic scalars", "relative weight", new[] { "petigura2013", "bryson2021", "bergstenetal2023", "mentcharbonneau2023", "cuietal2026", "gillisetal2026", "fulton2017", "fernandes2019", "kopparapu2013", "kopparapu2014" }, "Planet class zone weights are StarGen deterministic priors shaped by occurrence, HZ, host demographic, and giant-planet source context.", "SystemPlanetGenerator", "planet_mass_radius_model", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet_occurrence_demographic_scalars", "Planet occurrence demographic scalars", "host-regime close-in/HZ/sub-Neptune/hot-giant scalar family", "relative weight", new[] { "petigura2013", "bryson2021", "bergstenetal2023", "mentcharbonneau2023", "cuietal2026", "gillisetal2026" }, "Occurrence demographic scalars translate reviewed host-type occurrence directions into deterministic generation weights while full period-radius occurrence tables remain follow-up.", "PlanetarySystemState", "planet_mass_radius_model", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.envelope_loss_coefficients", "Envelope-loss class modifiers", "flux and orbit threshold modifiers", "relative weight", new[] { "fulton2017", "owenwu2017", "ginzburg2018" }, "Envelope-loss modifiers are StarGen tuning around photoevaporation and core-powered mass-loss model families.", "SystemPlanetGenerator", "planet_envelope_loss_model", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.volatile_delivery_coefficients", "Volatile delivery coefficients", "snow-line, giant-scattering, and inner-delivery modifiers", "scalar", new[] { "raymondizidoro2017", "pascucci2016", "fernandes2019", "izidoro2017", "tanakatakeuchiward2002" }, "Volatile-delivery coefficients are deterministic tuning around migration, snow-line, and giant-scattering source context.", "SystemPlanetGenerator", "planet_minor_body_outer_system_bias", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.slot_scoring_coefficients", "Planet slot scoring coefficients", "HZ alignment, harsh-slot, and temperate-slot scoring weights", "relative score", new[] { "kasting1993", "kopparapu2013", "kopparapu2014" }, "Slot scoring coefficients are StarGen tuning that make HZ model choices materially affect planet placement.", "SystemPlanetGenerator", "planet_habitable_zone_model", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "planet.moon_architecture_coefficients", "Moon architecture coefficients", "host-mass and regular/captured weighting family", "relative weight", new[] { "ronnet2020", "sasaki2010", "szulagyi2018", "jewitthaghighipour2007" }, "Moon architecture coefficients are deterministic tuning around competing regular and captured moon-formation models.", "SystemMoonGenerator", "planet_moon_formation_bias", "implemented as StarGen tuning; human verification pending", false);
        AddDirectEntry(entries, "life.support_score_coefficients", "Life support scoring coefficients", "habitability, radiation, solvent, biosphere, and complexity composite weights", "scalar", new[] { "lineweaverdavis2002", "spiegelturner2012", "forganrice2010", "mills2024", "balbi2023", "kopparapu2014" }, "Life support scoring coefficients are StarGen synthesis tuning around the selected life framework.", "BiologySupportEvaluator", "life_framework", "implemented as StarGen tuning; human audit required", true);
        AddDirectEntry(entries, "life.dark_biosphere_proxy_coefficients", "Dark-biosphere proxy coefficients", "rock-fluid, radiolytic, and serpentinization proxy weights", "scalar", new[] { "escuderoetal2023", "hellerbarnes2013" }, "Dark-biosphere proxy coefficients document the deterministic subsurface energy approximation.", "BiologySupportEvaluator", "subsurface_habitability_model", "implemented as StarGen proxy; human audit required", true);
        AddDirectEntry(entries, "life.oxygen_bottleneck_coefficients", "Civilization oxygen-bottleneck coefficients", "oxygen and abiotic-O2 risk modifiers", "scalar", new[] { "balbi2023" }, "Oxygen-bottleneck coefficients are StarGen tuning around late technosphere support, not an accepted universal law.", "BiologySupportEvaluator", "civilization_model", "implemented as StarGen tuning; human audit required", true);

        return entries;
    }

    private static void AddCatalogEntry(
        Dictionary<string, ScienceTuningEntry> entries,
        string id,
        string label,
        string valueOrRange,
        string units,
        string publicControlId,
        string consumingGenerator,
        string supportedClaim,
        bool requiresHumanAudit)
    {
        AddDirectEntry(
            entries,
            id,
            label,
            valueOrRange,
            units,
            GetCatalogSourceIds(publicControlId),
            supportedClaim,
            consumingGenerator,
            publicControlId,
            "implemented; coefficients remain StarGen tuning unless a source-specific entry narrows them",
            requiresHumanAudit);
    }

    private static void AddDirectEntry(
        Dictionary<string, ScienceTuningEntry> entries,
        string id,
        string label,
        string valueOrRange,
        string units,
        IReadOnlyList<string> sourceIds,
        string supportedClaim,
        string consumingGenerator,
        string publicControlId,
        string implementationStatus,
        bool requiresHumanAudit)
    {
        entries[id] = new ScienceTuningEntry(
            id,
            label,
            valueOrRange,
            units,
            sourceIds,
            supportedClaim,
            consumingGenerator,
            publicControlId,
            implementationStatus,
            requiresHumanAudit);
    }

    private static IReadOnlyList<string> GetCatalogSourceIds(string publicControlId)
    {
        IReadOnlyList<string> sourceIds = GalaxyScienceReferenceCatalog.GetParameterSourceIds(publicControlId);
        if (sourceIds.Count > 0)
        {
            return sourceIds;
        }

        sourceIds = StellarScienceReferenceCatalog.GetParameterSourceIds(publicControlId);
        if (sourceIds.Count > 0)
        {
            return sourceIds;
        }

        sourceIds = PlanetaryScienceReferenceCatalog.GetParameterSourceIds(publicControlId);
        if (sourceIds.Count > 0)
        {
            return sourceIds;
        }

        sourceIds = LifeScienceReferenceCatalog.GetParameterSourceIds(publicControlId);
        if (sourceIds.Count > 0)
        {
            return sourceIds;
        }

        sourceIds = SentientScienceReferenceCatalog.GetParameterSourceIds(publicControlId);
        if (sourceIds.Count > 0)
        {
            return sourceIds;
        }

        return System.Array.Empty<string>();
    }
}
