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
            "Galaxy family means the big shape group. Spiral, elliptical, lenticular, and irregular galaxies form stars in different ways, so this choice changes more than looks.",
            new[] { "generator-paper", "park2007", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "subtype_mode",
            "Subtype bias nudges the family toward earlier or later forms. Earlier usually means bigger bulges and smoother structure. Later usually means smaller bulges and looser structure.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "num_arms",
            "This sets how many main spiral arms are visible when the galaxy family supports arms.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "arm_pitch_angle_deg",
            "Pitch angle tells you how tightly the arms wind. Small values mean tighter arms. Large values mean looser arms.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "arm_amplitude",
            "Arm amplitude means how much denser the arms are than the spaces between them. Higher values make the arms stand out more strongly.",
            new[] { "generator-paper", "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "bar_mode",
            "A barred galaxy has a straight, bright bar of stars through the center. This setting lets the generator prefer barred or unbarred disk galaxies.",
            new[] { "generator-paper", "diazgarcia2016" }),
        new GalaxyScienceParameterReference(
            "arm_mechanism_preference",
            "Grand design means a galaxy has a few clear main arms. Multi-armed means several strong arms. Flocculent means patchy, broken, fluffy-looking arms instead of long clean ones.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "halo_mass_log10_solar",
            "Halo mass is the size of the galaxy's dark-matter halo. It helps set how large the galaxy is and which internal forms are more likely.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "environment_density_index",
            "Environment density means how crowded the galaxy's neighborhood is. A lonely field galaxy and a galaxy inside a crowded cluster do not evolve the same way.",
            new[] { "generator-paper", "park2007", "tanaka2004" }),
        new GalaxyScienceParameterReference(
            "bulge_intensity",
            "The bulge is the bright central star-rich region. This controls how dominant that central region is compared with the rest of the galaxy.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "bulge_radius_pc",
            "This sets the size of the central bulge in parsecs. A parsec is a standard astronomy distance unit.",
            new[] { "generator-paper", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "radius_pc",
            "This sets the rough outer size of the galaxy in parsecs.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "disk_scale_length_pc",
            "Disk scale length tells the generator how quickly the disk thins out as you move away from the center.",
            new[] { "generator-paper", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "disk_scale_height_pc",
            "Disk scale height tells the generator how thick the disk is above and below the galaxy's main plane.",
            new[] { "generator-paper", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "star_density_multiplier",
            "This is a global density knob. It raises or lowers how crowded the whole galaxy feels without replacing the science-based structure underneath.",
            new[] { "generator-paper", "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "ellipticity",
            "Ellipticity tells you how stretched an elliptical galaxy looks. Low values are rounder. High values are flatter.",
            new[] { "generator-paper" }),
        new GalaxyScienceParameterReference(
            "irregularity_scale",
            "Irregularity controls how lopsided and clumpy an irregular or dwarf galaxy looks.",
            new[] { "generator-paper", "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "ghz_inner_radius_pc",
            "GHZ means galactic habitable zone. This inner edge marks where the galaxy starts to become safer and more chemistry-friendly than the crowded center.",
            new[] { "generator-paper", "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_outer_radius_pc",
            "This outer edge marks where the galactic habitable zone starts to fade in the thin outer disk, where heavy elements are often less common.",
            new[] { "generator-paper", "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_transition_width_pc",
            "This controls how softly the galactic habitable zone fades in and out instead of switching on like a hard wall.",
            new[] { "generator-paper", "forgan2017" }),
        new GalaxyScienceParameterReference(
            "metallicity_gradient_dex_per_kpc",
            "A metallicity gradient describes how metal content changes with distance from the center. In astronomy, metals means elements heavier than hydrogen and helium.",
            new[] { "generator-paper", "apogee2024", "cmetall2024" }),
        new GalaxyScienceParameterReference(
            "star_formation_efficiency",
            "Star formation efficiency means how easily gas turns into stars. Higher values make young star-forming regions and cluster scaffolds more common.",
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
    /// Builds the rich-text content displayed by the galaxy help popup.
    /// </summary>
    public static string BuildSciencePanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b]Galaxy science[/b]");
        builder.AppendLine("StarGen builds a galaxy as a set of starting conditions. It does not run a full galaxy history simulation.");
        builder.AppendLine();
        builder.AppendLine("[b]Important terms[/b]");
        builder.AppendLine("Halo mass: the size of the dark-matter halo around the galaxy.");
        builder.AppendLine("Environment density: how crowded the galaxy's neighborhood is.");
        builder.AppendLine("Barred galaxy: a galaxy with a straight bar of stars through the center.");
        builder.AppendLine("Grand design spiral: a galaxy with a few clear, long main arms.");
        builder.AppendLine("Multi-armed spiral: a galaxy with several strong arms.");
        builder.AppendLine("Flocculent spiral: a galaxy with patchy, broken arm fragments.");
        builder.AppendLine("Metallicity gradient: how metal content changes from the center to the edge.");
        builder.AppendLine("GHZ: galactic habitable zone, a soft band where the balance of chemistry and hazards is more favorable.");
        builder.AppendLine("Star formation efficiency: how easily gas turns into new stars.");
        builder.AppendLine();
        builder.AppendLine("[b]How StarGen uses this[/b]");
        builder.AppendLine("1. You choose the top-level galaxy family.");
        builder.AppendLine("2. Halo mass and environment density help choose the internal form of that family.");
        builder.AppendLine("3. The generator then builds the galaxy's bulge, disk, arms, metallicity pattern, GHZ weighting, and star-forming regions.");
        builder.AppendLine("4. Those galaxy-level fields are passed down into stellar and system generation.");
        builder.AppendLine();
        builder.AppendLine("[b]Limits[/b]");
        builder.AppendLine("Clustered star formation is represented as deterministic scaffolding, not as a full moving cluster simulation.");
        builder.AppendLine("Some defaults are based on Milky Way and nearby-galaxy research. They are good starting points, not universal truths.");
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
        string barLabel = "Unbarred";
        if (profile.IsBarred)
        {
            barLabel = $"Barred ({profile.BarStrength:0.00})";
        }

        string armLabel = "No main spiral arms";
        if (config.Type == GalaxySpec.GalaxyType.Spiral)
        {
            if (profile.ArmMechanism == GalaxyArmMechanism.GrandDesign)
            {
                armLabel = "Grand design arms";
            }
            else if (profile.ArmMechanism == GalaxyArmMechanism.MultiArmed)
            {
                armLabel = "Multi-armed spiral";
            }
            else if (profile.ArmMechanism == GalaxyArmMechanism.Flocculent)
            {
                armLabel = "Flocculent spiral";
            }
            else
            {
                armLabel = "Automatic arm mode";
            }
        }

        return
            $"Resolved galaxy: {config.GetTypeName()} | Subtype {subtypeLabel} | Halo {profile.HaloMassLog10Solar:0.0} | Environment {profile.EnvironmentDensityIndex:0.00}\n" +
            $"Structure: {barLabel} | {armLabel} | Sersic n {profile.SersicIndex:0.0} | Bulge fraction {profile.BulgeToTotal:0.00}\n" +
            $"Fields: GHZ {profile.GhzInnerRadiusPc / 1000.0:0.0}-{profile.GhzOuterRadiusPc / 1000.0:0.0} kpc | Metallicity gradient {profile.MetallicityGradientDexPerKpc:0.000} dex/kpc | SFE {profile.StarFormationEfficiency:0.00}";
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
