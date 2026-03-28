using System.Collections.Generic;
using System.Text;
using StarGen.Domain.Galaxy;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by the galaxy-generation studio and tests.
/// </summary>
public sealed class GalaxyScienceSource
{
    /// <summary>
    /// Stable source identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Short human-readable citation.
    /// </summary>
    public string Citation { get; }

    /// <summary>
    /// Optional source URL for user-facing reference panels.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Creates a source entry.
    /// </summary>
    public GalaxyScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }
}

/// <summary>
/// Parameter-specific science notes used for tooltips and testable citation linkage.
/// </summary>
public sealed class GalaxyScienceParameterReference
{
    /// <summary>
    /// Stable parameter identifier.
    /// </summary>
    public string ParameterId { get; }

    /// <summary>
    /// Compact user-facing tooltip text.
    /// </summary>
    public string TooltipSummary { get; }

    /// <summary>
    /// Source identifiers backing the summary text.
    /// </summary>
    public IReadOnlyList<string> SourceIds { get; }

    /// <summary>
    /// Creates a parameter reference entry.
    /// </summary>
    public GalaxyScienceParameterReference(string parameterId, string tooltipSummary, IReadOnlyList<string> sourceIds)
    {
        ParameterId = parameterId;
        TooltipSummary = tooltipSummary;
        SourceIds = sourceIds;
    }
}

/// <summary>
/// Canonical science notes and source registry for the galaxy generator.
/// </summary>
public static class GalaxyScienceReferenceCatalog
{
    private static readonly Dictionary<string, GalaxyScienceSource> Sources = new()
    {
        ["generator-paper"] = new GalaxyScienceSource(
            "generator-paper",
            "StarGen galactic_formation.md: internal design paper for the scientifically grounded galaxy generator.",
            "Docs/galactic_formation.md"),
        ["park2007"] = new GalaxyScienceSource(
            "park2007",
            "Park et al. (2007), morphology-density relations from SDSS environment measures.",
            "https://arxiv.org/abs/astro-ph/0611610"),
        ["tanaka2004"] = new GalaxyScienceSource(
            "tanaka2004",
            "Tanaka et al. (2004), local-density environment trends in SDSS galaxy populations.",
            "https://arxiv.org/abs/astro-ph/0411132"),
        ["behroozi2019"] = new GalaxyScienceSource(
            "behroozi2019",
            "Behroozi et al. (2019), UniverseMachine stellar-to-halo mass priors.",
            "https://arxiv.org/abs/1806.07893"),
        ["oohama2009"] = new GalaxyScienceSource(
            "oohama2009",
            "Oohama et al. (2009), lenticular bulge-disc scaling and structure.",
            "https://arxiv.org/abs/0908.4312"),
        ["laurikainen2010"] = new GalaxyScienceSource(
            "laurikainen2010",
            "Laurikainen et al. (2010), S0 and S0/a bulge-disc decomposition trends.",
            "https://academic.oup.com/mnras/article/405/2/1089/1181902"),
        ["diazgarcia2016"] = new GalaxyScienceSource(
            "diazgarcia2016",
            "Diaz-Garcia et al. (2016), nearby barred-disk fractions and bar-strength caveats.",
            "https://www.oulu.fi/astronomy/S4G_BARFORCE/AA_astroph_paper_bars_2015_diazgarcia_simon.pdf"),
        ["kennicutt1998"] = new GalaxyScienceSource(
            "kennicutt1998",
            "Kennicutt (1998), star-formation surface-density scaling used for cluster scaffolding.",
            "https://arxiv.org/abs/astro-ph/9712213"),
        ["forgan2017"] = new GalaxyScienceSource(
            "forgan2017",
            "Forgan et al. (2017), galactic habitable-zone modeling and soft habitability weighting.",
            "https://arxiv.org/abs/1511.01786"),
        ["spitoni2017"] = new GalaxyScienceSource(
            "spitoni2017",
            "Spitoni et al. (2017), Milky Way chemical evolution and habitable-zone context.",
            "https://www.aanda.org/articles/aa/full_html/2017/09/aa30545-17/aa30545-17.html"),
        ["apogee2024"] = new GalaxyScienceSource(
            "apogee2024",
            "APOGEE-based metallicity gradient study (2024) for low-redshift radial metallicity defaults.",
            "https://arxiv.org/abs/2405.18120"),
        ["cmetall2024"] = new GalaxyScienceSource(
            "cmetall2024",
            "C-MetaLL (2024), metallicity and gradient behavior across nearby galaxies.",
            "https://www.aanda.org/articles/aa/full_html/2024/10/aa50376-24/aa50376-24.html"),
    };

    private static readonly List<GalaxyScienceParameterReference> ParameterReferences = new()
    {
        new GalaxyScienceParameterReference(
            "galaxy_type",
            "Locks the top-level family while allowing subtype variation inside the selected family instead of treating morphology as a pure cosmetic preset.",
            new[] { "generator-paper", "park2007", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "subtype_mode",
            "Biases the selected family toward earlier or later observed subtypes rather than forcing one exact catalogue label.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "num_arms",
            "Sets the visible spiral-arm count after family and subtype resolve the broader disk structure.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "arm_pitch_angle_deg",
            "Controls logarithmic-arm winding for spiral galaxies, with tighter or looser arms inside scientifically plausible disk morphologies.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "arm_amplitude",
            "Controls arm-to-interarm contrast, which the generator interprets as a morphology and star-formation bias rather than a decorative overlay.",
            new[] { "generator-paper", "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "bar_mode",
            "Chooses whether disk galaxies should prefer barred or unbarred realizations using nearby-galaxy barred-disk priors.",
            new[] { "generator-paper", "diazgarcia2016" }),
        new GalaxyScienceParameterReference(
            "arm_mechanism_preference",
            "Chooses grand-design, multi-armed, or flocculent spiral behavior when the selected family supports spiral structure.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "halo_mass_log10_solar",
            "Approximate halo mass steers subtype, scale, and age priors using low-redshift stellar-to-halo relations instead of a hand-authored shape preset.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "environment_density_index",
            "Local environment density biases morphology and hazard assumptions using observed morphology-density trends.",
            new[] { "generator-paper", "park2007", "tanaka2004" }),
        new GalaxyScienceParameterReference(
            "bulge_intensity",
            "Sets the prominence of the central spheroid once the scientific profile chooses the family and subtype regime.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "bulge_radius_pc",
            "Controls the size of the central bulge or spheroid that the resolved morphology uses for density evaluation.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "radius_pc",
            "Sets the outer footprint of the galaxy while halo mass and subtype determine the internally resolved scientific profile.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "disk_scale_length_pc",
            "Controls radial disk falloff for spiral and lenticular families after the subtype profile selects the broader size regime.",
            new[] { "generator-paper", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "disk_scale_height_pc",
            "Controls vertical disk thickness for disk-bearing families and influences local-density and hazard fields downstream.",
            new[] { "generator-paper", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "star_density_multiplier",
            "Scales total density without replacing the morphology-conditioned scientific profile that sets where stars prefer to form.",
            new[] { "generator-paper", "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "ellipticity",
            "Controls how flattened elliptical spheroids appear while the scientific profile governs the Sérsic-like structure underneath.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "irregularity_scale",
            "Controls clumpiness and asymmetry for irregular and dwarf families, which already carry lower-mass and less ordered priors.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "ghz_inner_radius_pc",
            "Sets where the soft galactic habitable-zone weighting begins to rise above the central high-hazard region.",
            new[] { "generator-paper", "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_outer_radius_pc",
            "Sets where the soft galactic habitable-zone weighting begins to decline again in the outer low-metallicity disk.",
            new[] { "generator-paper", "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_transition_width_pc",
            "Controls how softly the galactic habitable-zone weighting ramps in and out instead of acting like a hard cutoff.",
            new[] { "generator-paper", "forgan2017" }),
        new GalaxyScienceParameterReference(
            "metallicity_gradient_dex_per_kpc",
            "Controls the radial metallicity trend used for downstream stellar and system priors using low-redshift observational defaults.",
            new[] { "generator-paper", "apogee2024", "cmetall2024" }),
        new GalaxyScienceParameterReference(
            "star_formation_efficiency",
            "Controls the Kennicutt-Schmidt-inspired star-formation scaffold that seeds cluster probability and young-region bias downstream.",
            new[] { "generator-paper", "kennicutt1998" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "generator-paper",
        "park2007",
        "tanaka2004",
        "behroozi2019",
        "oohama2009",
        "laurikainen2010",
        "diazgarcia2016",
        "kennicutt1998",
        "forgan2017",
        "spitoni2017",
        "apogee2024",
        "cmetall2024",
    };

    /// <summary>
    /// Returns the tooltip summary for a galaxy-generation parameter.
    /// </summary>
    public static string GetTooltipSummary(string parameterId)
    {
        GalaxyScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return string.Empty;
        }

        return reference.TooltipSummary;
    }

    /// <summary>
    /// Returns the source identifiers backing a parameter tooltip.
    /// </summary>
    public static IReadOnlyList<string> GetParameterSourceIds(string parameterId)
    {
        GalaxyScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return System.Array.Empty<string>();
        }

        return reference.SourceIds;
    }

    /// <summary>
    /// Returns the canonical science references for galaxy parameters.
    /// </summary>
    public static IReadOnlyList<GalaxyScienceParameterReference> GetParameterReferences()
    {
        return ParameterReferences;
    }

    /// <summary>
    /// Returns the canonical source registry used by the studio.
    /// </summary>
    public static IReadOnlyList<GalaxyScienceSource> GetSources()
    {
        List<GalaxyScienceSource> orderedSources = new();
        foreach (string sourceId in PanelSourceIds)
        {
            orderedSources.Add(Sources[sourceId]);
        }

        return orderedSources;
    }

    /// <summary>
    /// Returns the source identifiers surfaced in the studio science panel.
    /// </summary>
    public static IReadOnlyList<string> GetSciencePanelSourceIds()
    {
        return PanelSourceIds;
    }

    /// <summary>
    /// Returns the source entry for the supplied identifier, or null when unknown.
    /// </summary>
    public static GalaxyScienceSource? GetSource(string sourceId)
    {
        if (Sources.ContainsKey(sourceId))
        {
            return Sources[sourceId];
        }

        return null;
    }

    /// <summary>
    /// Builds the rich-text content displayed by the galaxy studio science panel.
    /// </summary>
    public static string BuildSciencePanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b]Galaxy science model[/b]");
        builder.AppendLine("StarGen treats galaxy generation as deterministic initial-condition generation, not a time-step simulation.");
        builder.AppendLine();
        builder.AppendLine("[b]How the model works[/b]");
        builder.AppendLine("1. The user locks a top-level family: spiral, elliptical, lenticular, or irregular/dwarf.");
        builder.AppendLine("2. Halo mass and environment density bias the resolved subtype and broad structure inside that family.");
        builder.AppendLine("3. The resolved profile derives Sérsic behavior, bar likelihood, arm mechanism, metallicity gradient, galactic habitable-zone weighting, and star-formation scaffolding.");
        builder.AppendLine("4. Those galaxy fields propagate downstream into stellar metallicity, age, hazard, and cluster-bias context.");
        builder.AppendLine();
        builder.AppendLine("[b]Important modeling limits[/b]");
        builder.AppendLine("Clustered star formation is scaffolded through region and efficiency metadata. The generator does not claim to run a resolved galaxy-evolution simulation.");
        builder.AppendLine("Milky-Way-derived gradients and habitable-zone defaults are treated as documented priors, not universal truths.");
        builder.AppendLine();
        builder.AppendLine("[b]Sources[/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            GalaxyScienceSource source = Sources[sourceId];
            builder.Append("[i]");
            builder.Append(source.Citation);
            builder.Append("[/i]");
            if (!string.IsNullOrWhiteSpace(source.Url))
            {
                builder.Append(" ");
                builder.Append(source.Url);
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Builds a compact summary of the resolved scientific profile for the active configuration.
    /// </summary>
    public static string BuildProfileSummary(GalaxyConfig config, GalaxyRealismProfile profile)
    {
        string subtypeLabel = GalaxyRealismProfileBuilder.GetSubtypeLabel(profile.ResolvedSubtype);
        string barLabel = "unbarred";
        if (profile.IsBarred)
        {
            barLabel = $"barred ({profile.BarStrength:0.00})";
        }

        string armLabel = "n/a";
        if (config.Type == GalaxySpec.GalaxyType.Spiral)
        {
            armLabel = profile.ArmMechanism.ToString();
        }

        return
            $"Resolved {config.GetTypeName()} profile: {subtypeLabel} | Halo log M {profile.HaloMassLog10Solar:0.0} | Environment {profile.EnvironmentDensityIndex:0.00}\n" +
            $"Morphology: {barLabel} | Arm mode {armLabel} | Sérsic n {profile.SersicIndex:0.0} | B/T {profile.BulgeToTotal:0.00}\n" +
            $"Galaxy fields: GHZ {profile.GhzInnerRadiusPc / 1000.0:0.0}-{profile.GhzOuterRadiusPc / 1000.0:0.0} kpc | [M/H] gradient {profile.MetallicityGradientDexPerKpc:0.000} dex/kpc | SFE {profile.StarFormationEfficiency:0.00}";
    }

    private static GalaxyScienceParameterReference? FindParameterReference(string parameterId)
    {
        foreach (GalaxyScienceParameterReference reference in ParameterReferences)
        {
            if (reference.ParameterId == parameterId)
            {
                return reference;
            }
        }

        return null;
    }
}
