using System.Collections.Generic;
using System.Text;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by stellar-generation help surfaces and tests.
/// </summary>
public sealed class StellarScienceSource
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
    /// Optional user-facing source URL.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Creates a source entry.
    /// </summary>
    public StellarScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }
}

/// <summary>
/// Parameter-specific stellar-science help text and source linkage.
/// </summary>
public sealed class StellarScienceParameterReference
{
    /// <summary>
    /// Stable parameter identifier.
    /// </summary>
    public string ParameterId { get; }

    /// <summary>
    /// Plain-language tooltip text for non-expert users.
    /// </summary>
    public string TooltipSummary { get; }

    /// <summary>
    /// Backing source identifiers.
    /// </summary>
    public IReadOnlyList<string> SourceIds { get; }

    /// <summary>
    /// Creates a parameter reference entry.
    /// </summary>
    public StellarScienceParameterReference(string parameterId, string tooltipSummary, IReadOnlyList<string> sourceIds)
    {
        ParameterId = parameterId;
        TooltipSummary = tooltipSummary;
        SourceIds = sourceIds;
    }
}

/// <summary>
/// Canonical science notes and source registry for stellar-generation controls.
/// </summary>
public static class StellarScienceReferenceCatalog
{
    private static readonly Dictionary<string, StellarScienceSource> Sources = new()
    {
        ["kroupa2001"] = new StellarScienceSource(
            "kroupa2001",
            "Kroupa (2001), broken-power-law initial mass function.",
            "https://arxiv.org/abs/astro-ph/0009005"),
        ["chabrier2003"] = new StellarScienceSource(
            "chabrier2003",
            "Chabrier (2003), log-normal low-mass initial mass function.",
            "https://arxiv.org/abs/astro-ph/0304382"),
        ["li2023"] = new StellarScienceSource(
            "li2023",
            "Li et al. (2023), IMF shifts with metallicity and age in resolved stellar populations.",
            "https://arxiv.org/abs/2205.02375"),
        ["choi2016"] = new StellarScienceSource(
            "choi2016",
            "Choi et al. (2016), MIST stellar tracks and isochrones.",
            "https://arxiv.org/abs/1604.08592"),
        ["bressan2012"] = new StellarScienceSource(
            "bressan2012",
            "Bressan et al. (2012), PARSEC stellar tracks and isochrones.",
            "https://arxiv.org/abs/1208.4498"),
        ["duchene2013"] = new StellarScienceSource(
            "duchene2013",
            "Duchene and Kraus (2013), review of stellar multiplicity by primary mass.",
            "https://arxiv.org/abs/1303.3028"),
        ["raghavan2010"] = new StellarScienceSource(
            "raghavan2010",
            "Raghavan et al. (2010), binary period distribution for solar-type stars.",
            "https://arxiv.org/abs/1007.0414"),
        ["tokovinin2014"] = new StellarScienceSource(
            "tokovinin2014",
            "Tokovinin (2014), hierarchical architectures in triples and quadruples.",
            "https://arxiv.org/abs/1401.6827"),
        ["tokovinin2021"] = new StellarScienceSource(
            "tokovinin2021",
            "Tokovinin (2021), architecture of hierarchical stellar systems.",
            "https://arxiv.org/abs/2109.09118"),
    };

    private static readonly List<StellarScienceParameterReference> ParameterReferences = new()
    {
        new StellarScienceParameterReference(
            "stellar_imf_form",
            "IMF means initial mass function: the rule for how many small, medium, and large stars form.\nKroupa and Chabrier mostly change the balance of red dwarfs versus larger stars.",
            new[] { "kroupa2001", "chabrier2003" }),
        new StellarScienceParameterReference(
            "stellar_imf_variation_mode",
            "Canonical keeps that mass mix the same everywhere.\nMetallicity and age modulated lets old or metal-poor regions shift the mix a little, so some places favor different star sizes.",
            new[] { "li2023" }),
        new StellarScienceParameterReference(
            "stellar_isochrone_model",
            "This is the star-evolution chart used to turn mass, age, and chemistry into brightness, heat, and size.\nSwitching models changes the exact numbers more than the big picture.",
            new[] { "choi2016", "bressan2012" }),
        new StellarScienceParameterReference(
            "stellar_multiplicity_scale",
            "Multiplicity means whether stars form alone or with companions.\nRaise this for more binaries and triples.\nLower it for more lone stars.",
            new[] { "duchene2013", "tokovinin2021", "raghavan2010" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "kroupa2001",
        "chabrier2003",
        "li2023",
        "choi2016",
        "bressan2012",
        "duchene2013",
        "raghavan2010",
        "tokovinin2014",
        "tokovinin2021",
    };

    /// <summary>
    /// Returns the tooltip summary for a stellar-generation parameter.
    /// </summary>
    public static string GetTooltipSummary(string parameterId)
    {
        StellarScienceParameterReference? reference = FindParameterReference(parameterId);
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
        StellarScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return System.Array.Empty<string>();
        }

        return reference.SourceIds;
    }

    /// <summary>
    /// Returns the canonical parameter-reference list.
    /// </summary>
    public static IReadOnlyList<StellarScienceParameterReference> GetParameterReferences()
    {
        return ParameterReferences;
    }

    /// <summary>
    /// Returns the ordered source registry used by the help panel.
    /// </summary>
    public static IReadOnlyList<StellarScienceSource> GetSources()
    {
        List<StellarScienceSource> orderedSources = new();
        foreach (string sourceId in PanelSourceIds)
        {
            orderedSources.Add(Sources[sourceId]);
        }

        return orderedSources;
    }

    /// <summary>
    /// Returns the source identifiers surfaced in the help panel.
    /// </summary>
    public static IReadOnlyList<string> GetHelpPanelSourceIds()
    {
        return PanelSourceIds;
    }

    /// <summary>
    /// Returns the source entry for the supplied identifier.
    /// </summary>
    public static StellarScienceSource? GetSource(string sourceId)
    {
        if (Sources.ContainsKey(sourceId))
        {
            return Sources[sourceId];
        }

        return null;
    }

    /// <summary>
    /// Builds the plain-language stellar-help content displayed in the help popups.
    /// </summary>
    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Star controls[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]These settings decide what kinds of stars are common, how many companion stars show up, and how star mass and age turn into brightness, size, and heat.[/color]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "IMF Form",
            "IMF means initial mass function. It is the rule for how many tiny stars, Sun-like stars, and massive stars are born.",
            "Switching IMF form changes the balance of common low-mass stars versus rarer high-mass stars. In practice, that changes how often you see long-lived red dwarfs compared with brighter, shorter-lived stars.");

        AppendGuideSection(
            builder,
            "IMF Shift",
            "This decides whether every region uses one fixed mass rule or whether old and metal-poor regions can tilt that rule a little.",
            "Use canonical if you want the same mass mix everywhere. Use metallicity and age modulated if you want different parts of a galaxy to nudge star sizes in different directions.");

        AppendGuideSection(
            builder,
            "Star Model",
            "A star model is the chart that turns mass, age, and chemistry into temperature, brightness, and radius.",
            "Switching between MIST and PARSEC usually changes the exact numbers, not the broad type of system. It matters most when you care about the fine details of star properties.");

        AppendGuideSection(
            builder,
            "Companions",
            "Multiplicity means how often stars are alone versus paired up in binaries, triples, or larger groups.",
            "Raising this value makes multi-star systems more common. Lowering it makes single-star systems more common.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. It chooses a mass distribution for the stars that can form.");
        builder.AppendLine("2. It optionally lets age and metallicity nudge that distribution.");
        builder.AppendLine("3. It turns each star's mass, age, and chemistry into visible properties.");
        builder.AppendLine("4. It decides how often stars get companion stars and builds stable hierarchies up to the app limit.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Model limits[/color][/b]");
        builder.AppendLine("StarGen uses deterministic approximations shaped by these research models.");
        builder.AppendLine("It does not ship the full raw MIST or PARSEC tables in this branch.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Sources[/color][/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            StellarScienceSource source = Sources[sourceId];
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

    private static StellarScienceParameterReference? FindParameterReference(string parameterId)
    {
        foreach (StellarScienceParameterReference reference in ParameterReferences)
        {
            if (reference.ParameterId == parameterId)
            {
                return reference;
            }
        }

        return null;
    }
}
