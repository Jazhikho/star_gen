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
        ["generator-paper"] = new StellarScienceSource(
            "generator-paper",
            "StarGen galactic_formation.md: internal design paper for stellar population and multiplicity modeling.",
            "Docs/galactic_formation.md"),
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
            "IMF means initial mass function. It is the rule for how many small, medium, and large stars form. Kroupa and Chabrier are two research-backed ways to model that rule.",
            new[] { "generator-paper", "kroupa2001", "chabrier2003" }),
        new StellarScienceParameterReference(
            "stellar_imf_variation_mode",
            "Canonical keeps the same mass rule everywhere. Metallicity and age modulated lets older or more metal-poor regions shift that rule a little.",
            new[] { "generator-paper", "li2023" }),
        new StellarScienceParameterReference(
            "stellar_isochrone_model",
            "A stellar model turns mass, age, and chemistry into star size, heat, and brightness. MIST and PARSEC are two well-known model families.",
            new[] { "generator-paper", "choi2016", "bressan2012" }),
        new StellarScienceParameterReference(
            "stellar_multiplicity_scale",
            "Multiplicity means how often a star has companions. Higher values make binaries and larger star groups more common. Lower values make lone stars more common.",
            new[] { "generator-paper", "duchene2013", "tokovinin2021", "raghavan2010" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "generator-paper",
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
    /// Builds the plain-language stellar-help content displayed in the galaxy help popup.
    /// </summary>
    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b]Stellar science[/b]");
        builder.AppendLine("After StarGen places a system in a galaxy, it still has to decide what kinds of stars form there.");
        builder.AppendLine();
        builder.AppendLine("[b]Key ideas[/b]");
        builder.AppendLine("IMF means initial mass function. It is the rule for how many low-mass, medium-mass, and high-mass stars form.");
        builder.AppendLine("Isochrone model means the star-evolution chart used to turn mass, age, and chemistry into brightness, size, and temperature.");
        builder.AppendLine("Multiplicity means whether a star is alone or part of a binary, triple, or larger group.");
        builder.AppendLine();
        builder.AppendLine("[b]How StarGen uses this[/b]");
        builder.AppendLine("1. The stellar profile picks an IMF family.");
        builder.AppendLine("2. Galaxy age and metallicity can leave the IMF alone or shift it a little.");
        builder.AppendLine("3. A stellar model then turns mass, age, and chemistry into the star's visible properties.");
        builder.AppendLine("4. Multiplicity settings decide how often systems get companion stars.");
        builder.AppendLine();
        builder.AppendLine("[b]Modeling limits[/b]");
        builder.AppendLine("StarGen uses deterministic approximations tuned to these research models. It does not ship the full raw MIST or PARSEC tables in this branch.");
        builder.AppendLine();
        builder.AppendLine("[b]Sources[/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            StellarScienceSource source = Sources[sourceId];
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
