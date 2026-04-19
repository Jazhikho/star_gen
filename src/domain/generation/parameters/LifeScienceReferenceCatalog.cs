using System.Collections.Generic;
using System.Text;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by life-potential help surfaces and tests.
/// </summary>
public sealed class LifeScienceSource
{
    public string Id { get; }
    public string Citation { get; }
    public string Url { get; }

    public LifeScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }
}

/// <summary>
/// Parameter-specific life-science help text and source linkage.
/// </summary>
public sealed class LifeScienceParameterReference
{
    public string ParameterId { get; }
    public string TooltipSummary { get; }
    public IReadOnlyList<string> SourceIds { get; }

    public LifeScienceParameterReference(string parameterId, string tooltipSummary, IReadOnlyList<string> sourceIds)
    {
        ParameterId = parameterId;
        TooltipSummary = tooltipSummary;
        SourceIds = sourceIds;
    }
}

/// <summary>
/// Canonical science notes and source registry for life-potential controls.
/// </summary>
public static class LifeScienceReferenceCatalog
{
    private static readonly Dictionary<string, LifeScienceSource> Sources = new()
    {
        ["kopparapu2014"] = new LifeScienceSource(
            "kopparapu2014",
            "Kopparapu et al. (2014), habitable-zone limits vary with planetary mass and flux.",
            "https://arxiv.org/abs/1404.5292"),
        ["lineweaverdavis2002"] = new LifeScienceSource(
            "lineweaverdavis2002",
            "Lineweaver and Davis (2002), rapid terrestrial biogenesis may favor common simple life on Earth-like worlds.",
            "https://arxiv.org/abs/astro-ph/0205014"),
        ["spiegelturner2012"] = new LifeScienceSource(
            "spiegelturner2012",
            "Spiegel and Turner (2012), early life on Earth does not by itself prove abiogenesis is easy everywhere.",
            "https://arxiv.org/abs/1107.3835"),
        ["forganrice2010"] = new LifeScienceSource(
            "forganrice2010",
            "Forgan and Rice (2010), Rare Earth constraints can allow microbes while keeping intelligent life uncommon.",
            "https://arxiv.org/abs/1001.1680"),
        ["mills2024"] = new LifeScienceSource(
            "mills2024",
            "Mills et al. (2024), intelligent life may depend on long environmental windows rather than a chain of intrinsically improbable hard steps.",
            "https://arxiv.org/abs/2408.10293"),
        ["balbi2023"] = new LifeScienceSource(
            "balbi2023",
            "Balbi et al. (2023), oxygen-rich conditions may be an important bottleneck for technological civilizations.",
            "https://arxiv.org/abs/2308.01160"),
    };

    private static readonly List<LifeScienceParameterReference> ParameterReferences = new()
    {
        new LifeScienceParameterReference(
            "life_potential_model",
            "This chooses how strict StarGen is about biospheres and civilizations.\nEarth History keeps life plausible on good worlds but makes complex life and civilizations depend on long stable conditions.\nRapid Biospheres makes simple life easier to start.\nEnvironmental Windows boosts worlds with long calm habitable periods.\nRare Complex Life keeps simple life possible but makes complex life and civilizations much rarer.",
            new[] { "kopparapu2014", "lineweaverdavis2002", "spiegelturner2012", "forganrice2010", "mills2024", "balbi2023" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "kopparapu2014",
        "lineweaverdavis2002",
        "spiegelturner2012",
        "forganrice2010",
        "mills2024",
        "balbi2023",
    };

    public static string GetTooltipSummary(string parameterId)
    {
        LifeScienceParameterReference? reference = FindParameterReference(parameterId);
        return reference?.TooltipSummary ?? string.Empty;
    }

    public static IReadOnlyList<string> GetParameterSourceIds(string parameterId)
    {
        LifeScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return System.Array.Empty<string>();
        }

        return reference.SourceIds;
    }

    public static LifeScienceSource? GetSource(string sourceId)
    {
        if (Sources.ContainsKey(sourceId))
        {
            return Sources[sourceId];
        }

        return null;
    }

    public static string GetModelLabel(GenerationUseCaseSettings.LifePotentialModelType model)
    {
        return model switch
        {
            GenerationUseCaseSettings.LifePotentialModelType.EarthHistory => "Earth History",
            GenerationUseCaseSettings.LifePotentialModelType.RapidBiospheres => "Rapid Biospheres",
            GenerationUseCaseSettings.LifePotentialModelType.EnvironmentalWindows => "Environmental Windows",
            GenerationUseCaseSettings.LifePotentialModelType.RareComplexLife => "Rare Complex Life",
            _ => "Earth History",
        };
    }

    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Life potential models[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]This setting is not a flavor slider. It changes how easily worlds get biospheres, how hard it is for complex life to survive, and how often StarGen allows native civilizations to emerge.[/color]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "Earth History",
            "Use Earth as the rough baseline.",
            "Simple life can appear on good worlds, but complex life and civilizations still need long-lived water, moderate radiation, and stable surface conditions. This is the realistic default.");

        AppendGuideSection(
            builder,
            "Rapid Biospheres",
            "Assume life starts fairly easily once water and energy are in place.",
            "More habitable worlds pick up biospheres. Complex life and civilizations are still filtered later, so this raises living worlds more than it raises advanced civilizations.");

        AppendGuideSection(
            builder,
            "Environmental Windows",
            "Assume complex life depends on long calm windows rather than pure luck.",
            "Worlds with long stable climates, moderate radiation, good surface diversity, and breathable atmospheres get a bigger boost. This raises the odds of complex ecosystems and native civilizations on especially stable worlds.");

        AppendGuideSection(
            builder,
            "Rare Complex Life",
            "Assume microbes may be common but complex life needs unusually Earth-like conditions.",
            "This model keeps tight limits on radiation, climate stability, and long-lived surface habitability. Biospheres still happen, but advanced life and civilizations become much rarer.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. It changes the biology-support threshold, not just the text label.");
        builder.AppendLine("2. It changes abiogenesis, complex-life, and sentience or civilization chances separately.");
        builder.AppendLine("3. It changes which worlds count as good long-term homes for native civilizations.");
        builder.AppendLine();

        builder.AppendLine("[b][color=#f0c46a]Sources[/color][/b]");
        foreach (string sourceId in PanelSourceIds)
        {
            LifeScienceSource source = Sources[sourceId];
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

    private static LifeScienceParameterReference? FindParameterReference(string parameterId)
    {
        foreach (LifeScienceParameterReference reference in ParameterReferences)
        {
            if (reference.ParameterId == parameterId)
            {
                return reference;
            }
        }

        return null;
    }
}
