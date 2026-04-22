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
        ["hart2017"] = new GalaxyScienceSource(
            "hart2017",
            "Hart et al. (2017), spiral arm number and pitch-angle trends constrain arm-formation mechanisms.",
            "https://arxiv.org/abs/1708.04628"),
        ["lingard2021"] = new GalaxyScienceSource(
            "lingard2021",
            "Lingard et al. (2021), pitch-angle behavior is consistent with transient and recurrent spiral winding.",
            "https://arxiv.org/abs/2105.04500"),
        ["rodriguezpadilla2013"] = new GalaxyScienceSource(
            "rodriguezpadilla2013",
            "Rodriguez and Padilla (2013), intrinsic galaxy shapes and disc ellipticity from SDSS and Galaxy Zoo.",
            "https://arxiv.org/abs/1306.3264"),
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
            "This is the galaxy's big shape family.\nSpiral gives a disk and arms.\nElliptical is smoother and center-heavy.\nLenticular sits between disk and smooth.\nIrregular or dwarf is smaller and clumpier.",
            new[] { "park2007", "behroozi2019", "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "subtype_mode",
            "Subtype bias pushes the family toward smoother, center-heavy forms or looser, more disk-dominated forms.\nEarlier types usually mean a stronger middle.\nLater types usually mean more disk structure and star-forming gas.",
            new[] { "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "num_arms",
            "This sets how many main spiral lanes the disk can have.\nFewer arms make the pattern cleaner.\nMore arms spread star-forming structure around the disk.",
            new[] { "hart2017" }),
        new GalaxyScienceParameterReference(
            "arm_pitch_angle_deg",
            "This controls how tightly the arms wrap around the center.\nSmaller values make tightly wound arms.\nLarger values open the arms up and make the disk look looser.",
            new[] { "hart2017", "lingard2021" }),
        new GalaxyScienceParameterReference(
            "arm_amplitude",
            "This controls how strongly the arms stand out from the rest of the disk.\nHigher values pack more stars and star-forming activity into the arms instead of spreading them evenly.",
            new[] { "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "bar_mode",
            "A barred galaxy has a straight bar of stars through the center.\nPushing this upward makes that bar more likely, which can change how strongly the center and inner arms stand out.",
            new[] { "diazgarcia2016" }),
        new GalaxyScienceParameterReference(
            "arm_mechanism_preference",
            "Grand design means a few long, clean arms.\nMulti-armed means several strong arms.\nFlocculent means short, patchy arm pieces.\nThis changes how orderly or broken the spiral pattern looks.",
            new[] { "hart2017", "lingard2021" }),
        new GalaxyScienceParameterReference(
            "halo_mass_log10_solar",
            "Halo mass is the size of the invisible dark-matter halo holding the galaxy together.\nHigher values usually favor bigger, heavier galaxies.\nLower values trend toward smaller systems and dwarf-like behavior.",
            new[] { "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "environment_density_index",
            "Environment density means how crowded the galaxy's neighborhood is.\nHigher values act more like cluster environments, which favor smoother, gas-poorer galaxies.\nLower values act more like quieter field environments, which favor disks.",
            new[] { "park2007", "tanaka2004" }),
        new GalaxyScienceParameterReference(
            "bulge_intensity",
            "The bulge is the bright star-packed middle.\nRaising this makes the center more dominant.\nLowering it lets the disk matter more.",
            new[] { "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "bulge_radius_pc",
            "This sets how far the central bulge spreads.\nA larger bulge means the bright center influences more of the inner galaxy.",
            new[] { "oohama2009", "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "radius_pc",
            "This is the galaxy's rough outer size.\nLarger values spread stars over a wider region.\nSmaller values keep the whole galaxy more compact.",
            new[] { "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "disk_scale_length_pc",
            "This controls how quickly the disk thins out as you move away from the center.\nA larger value keeps the main disk important farther out.",
            new[] { "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "disk_scale_height_pc",
            "This controls disk thickness above and below the main plane.\nHigher values make the disk puffier.\nLower values keep it flatter and thinner.",
            new[] { "laurikainen2010" }),
        new GalaxyScienceParameterReference(
            "star_density_multiplier",
            "This is a generator crowding scale layered on top of the science-backed structure.\nRaising it makes the whole galaxy feel fuller.\nLowering it makes it feel emptier without changing the underlying morphology model.",
            new[] { "kennicutt1998" }),
        new GalaxyScienceParameterReference(
            "ellipticity",
            "Ellipticity means how stretched the galaxy looks instead of round.\nHigher values flatten it more.\nLower values keep it rounder.",
            new[] { "rodriguezpadilla2013" }),
        new GalaxyScienceParameterReference(
            "irregularity_scale",
            "This controls how lopsided and clumpy an irregular or dwarf galaxy looks.\nRaising it gives you a messier, less symmetric shape.",
            new[] { "behroozi2019" }),
        new GalaxyScienceParameterReference(
            "ghz_inner_radius_pc",
            "GHZ means galactic habitable zone.\nThis inner edge marks where the center starts to become less overwhelmingly crowded and dangerous.\nRaising it pushes the safer sweet spot farther out.",
            new[] { "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_outer_radius_pc",
            "This outer edge marks where the galaxy starts to run thin on heavy elements and long-term chemistry.\nRaising it lets the habitable sweet spot reach farther into the disk.",
            new[] { "forgan2017", "spitoni2017" }),
        new GalaxyScienceParameterReference(
            "ghz_transition_width_pc",
            "This softens the edge of the habitable zone.\nSmall values make the change sharper.\nLarger values make the zone fade in and out more gradually.",
            new[] { "forgan2017" }),
        new GalaxyScienceParameterReference(
            "metallicity_gradient_dex_per_kpc",
            "This says how fast heavy-element content drops from the center to the edge.\nMore negative values make the inner galaxy much richer in planet-building material.\nFlatter values spread those ingredients farther outward.",
            new[] { "apogee2024", "cmetall2024" }),
        new GalaxyScienceParameterReference(
            "star_formation_efficiency",
            "This controls how easily gas turns into new stars.\nHigher values mean more young star-forming patches and clustering.\nLower values make the galaxy feel calmer and more settled.",
            new[] { "kennicutt1998" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "park2007",
        "tanaka2004",
        "behroozi2019",
        "oohama2009",
        "laurikainen2010",
        "diazgarcia2016",
        "hart2017",
        "lingard2021",
        "rodriguezpadilla2013",
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
        builder.AppendLine("[b][color=#f0c46a]Galaxy controls[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]Use these settings to choose what kind of galaxy forms, where its stars cluster, and which parts of it are richer in the ingredients that build planets and complex chemistry.[/color]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "Galaxy Family",
            "This is the galaxy's big shape group.",
            "Changing it changes the entire starting point. Spiral gives you a disk with arms. Elliptical gives you a smoother ball-like shape with a strong center. Lenticular keeps a disk but loses obvious arms. Irregular or dwarf makes the galaxy smaller, messier, and clumpier.");

        AppendGuideSection(
            builder,
            "Subtype Bias",
            "Subtype bias nudges the chosen family toward earlier or later forms.",
            "Moving toward earlier types usually grows the central bulge and smooths things out. Moving toward later types usually gives the disk more say and makes ongoing star-forming structure more likely.");

        AppendGuideSection(
            builder,
            "Bar Bias and Arm Mode",
            "A bar is a straight lane of stars through the center. Arm mode is the style of the spiral pattern.",
            "Prefer barred if you want the inner galaxy to look more structured around a central bar. Use Grand design for a few long clear arms, Multi-armed for several strong arms, and Flocculent for broken, patchy arm pieces.");

        AppendGuideSection(
            builder,
            "Halo Mass",
            "The dark-matter halo is the invisible mass that helps hold the galaxy together.",
            "Raising halo mass generally supports larger, heavier galaxies. Lowering it makes smaller galaxies and dwarf-like outcomes more likely.");

        AppendGuideSection(
            builder,
            "Environment Density",
            "This is how crowded the galaxy's neighborhood is with other galaxies.",
            "Higher values behave more like a busy galaxy cluster. That tends to favor smoother, more gas-poor forms. Lower values behave more like a quieter neighborhood, which helps disk structure survive.");

        AppendGuideSection(
            builder,
            "GHZ",
            "GHZ means galactic habitable zone. It is not a magic safe ring. It is a soft best-compromise band between the crowded center and the thin outer edge.",
            "Moving the inner edge outward says the center stays too intense for longer. Moving the outer edge outward says useful chemistry stays available farther into the disk.");

        AppendGuideSection(
            builder,
            "Metallicity Gradient",
            "In astronomy, metals means every element heavier than hydrogen and helium. Those heavy elements are the raw material for dust, rock, and many planet-building ingredients.",
            "A steeper negative gradient makes the inner galaxy much richer in heavy elements than the edge. That means the center has more rocky-world ingredients, while the outer disk gets poorer faster. A flatter gradient spreads those ingredients farther out.");

        AppendGuideSection(
            builder,
            "Star Formation Efficiency",
            "This controls how easily gas turns into new stars.",
            "Higher values make young star-forming patches and cluster scaffolds more common. Lower values give a calmer galaxy with less fresh star birth.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. Your family choice locks in the broad kind of galaxy.");
        builder.AppendLine("2. Halo mass, environment, subtype bias, bar bias, and arm mode help shape the final structure inside that family.");
        builder.AppendLine("3. The generator builds fields for crowding, metallicity, GHZ weighting, and star-forming activity.");
        builder.AppendLine("4. Those fields then influence which stars and systems form in different parts of the galaxy.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Model limits[/color][/b]");
        builder.AppendLine("StarGen is generating believable starting conditions, not replaying billions of years of galaxy history.");
        builder.AppendLine("Some defaults are inspired by the Milky Way and nearby galaxies. They are useful starting points, not universal laws.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Sources[/color][/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            GalaxyScienceSource source = Sources[sourceId];
            builder.Append("- ");
            if (!string.IsNullOrWhiteSpace(source.Url))
            {
                builder.Append("[url=");
                builder.Append(source.Url);
                builder.Append("]");
                builder.Append(source.Citation);
                builder.Append("[/url]");
            }
            else
            {
                builder.Append(source.Citation);
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
        string barLabel = "No central bar";
        if (profile.IsBarred)
        {
            barLabel = $"Central bar {profile.BarStrength:0.00}";
        }

        string armLabel = "No main spiral arms";
        if (config.Type == GalaxySpec.GalaxyType.Spiral)
        {
            if (profile.ArmMechanism == GalaxyArmMechanism.GrandDesign)
            {
                armLabel = "Grand design spiral";
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
            $"Resolved galaxy: {config.GetTypeName()} | Subtype {subtypeLabel}\n" +
            $"Structure: {barLabel} | {armLabel} | Bulge share {profile.BulgeToTotal:0.00}\n" +
            $"Environment: Halo {profile.HaloMassLog10Solar:0.0} | Neighborhood {profile.EnvironmentDensityIndex:0.00}\n" +
            $"Chemistry: GHZ {profile.GhzInnerRadiusPc / 1000.0:0.0}-{profile.GhzOuterRadiusPc / 1000.0:0.0} kpc | Metal drop {profile.MetallicityGradientDexPerKpc:0.000} dex/kpc | Star birth {profile.StarFormationEfficiency:0.00}";
    }

    private static void AppendGuideSection(StringBuilder builder, string title, string meaning, string effect)
    {
        builder.Append("[b]");
        builder.Append(title);
        builder.AppendLine("[/b]");
        builder.Append("[color=#9cc4ff]What it means:[/color] ");
        builder.AppendLine(meaning);
        builder.Append("[color=#9cc4ff]What changing it does:[/color] ");
        builder.AppendLine(effect);
        builder.AppendLine();
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
