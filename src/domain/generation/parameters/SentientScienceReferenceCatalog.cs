using System.Collections.Generic;
using System.Text;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by sentient-world science help surfaces and tests.
/// </summary>
public sealed class SentientScienceSource
{
    public SentientScienceSource(string id, string citation, string url)
    {
        Id = id;
        Citation = citation;
        Url = url;
    }

    public string Id { get; }

    public string Citation { get; }

    public string Url { get; }
}

/// <summary>
/// Parameter-specific sentient-world help text and source linkage.
/// </summary>
public sealed class SentientScienceParameterReference
{
    public SentientScienceParameterReference(string parameterId, string tooltipSummary, IReadOnlyList<string> sourceIds)
    {
        ParameterId = parameterId;
        TooltipSummary = tooltipSummary;
        SourceIds = sourceIds;
    }

    public string ParameterId { get; }

    public string TooltipSummary { get; }

    public IReadOnlyList<string> SourceIds { get; }
}

/// <summary>
/// Canonical science notes and source registry for sentient-world generation controls.
/// </summary>
public static class SentientScienceReferenceCatalog
{
    private static readonly Dictionary<string, SentientScienceSource> Sources = new()
    {
        ["hamiltonetal2020"] = new SentientScienceSource(
            "hamiltonetal2020",
            "Hamilton et al. (2020), population scale matters for social organization but is not sufficient by itself.",
            "Sources/Texts/HamiltonEtAl2020.txt"),
        ["bettencourtetal2007"] = new SentientScienceSource(
            "bettencourtetal2007",
            "Bettencourt et al. (2007), urban output scales nonlinearly with population and infrastructure.",
            "Sources/Texts/BettencourtEtAl2007.txt"),
        ["arvidssonetal2023"] = new SentientScienceSource(
            "arvidssonetal2023",
            "Arvidsson et al. (2023), urban scaling benefits are strongly shaped by within-city inequality.",
            "Sources/Texts/ArvidssonEtAl2023.txt"),
        ["knez2023"] = new SentientScienceSource(
            "knez2023",
            "Knez (2023), technology access and adoption density can diverge from the highest technology present.",
            "Sources/Texts/Knez2023.txt"),
        ["stokey2020"] = new SentientScienceSource(
            "stokey2020",
            "Stokey (2020), diffusion across users and places can matter as much as invention for long-run technology effects.",
            "Sources/Texts/Stokey2020.txt"),
        ["chacuaetal2024"] = new SentientScienceSource(
            "chacuaetal2024",
            "Chacua et al. (2024), economic complexity depends on capability portfolios and relatedness.",
            "Sources/Texts/ChacuaEtAl2024.txt"),
        ["vankleefetal2023"] = new SentientScienceSource(
            "vankleefetal2023",
            "Van Kleef et al. (2023), legitimacy depends on dominance, prestige, and norm alignment.",
            "Sources/Texts/VanKleefEtAl2023.txt"),
        ["comin2013"] = new SentientScienceSource(
            "comin2013",
            "Comin and Hobijn (2013), technology diffusion varies across technologies, places, and time.",
            "Sources/Texts/Comin2013.txt"),
        ["cominmestieri2013"] = new SentientScienceSource(
            "cominmestieri2013",
            "Comin and Mestieri (2013), technology adoption lags shape long-run development differences.",
            "Sources/Texts/CominMestieri2013.txt"),
        ["chowdhury2022"] = new SentientScienceSource(
            "chowdhury2022",
            "Chowdhury (2022), low state capacity can substitute regulation for harder enforcement functions.",
            "Sources/Texts/Chowdhury2022.txt"),
        ["ballandetal2022"] = new SentientScienceSource(
            "ballandetal2022",
            "Balland et al. (2022), productive capabilities and relatedness constrain economic development paths.",
            "Sources/Texts/BallandEtAl2022.txt"),
    };

    private static readonly List<SentientScienceParameterReference> ParameterReferences = new()
    {
        new SentientScienceParameterReference(
            "sentient_social_scale_model",
            "This selects how population turns into institutional scale.\nPopulation Composite keeps the current StarGen blend.\nPopulation Hierarchy Aware adds administrative capacity and group structure while keeping population important.\nHuman audit is required before release claims.",
            new[] { "hamiltonetal2020", "bettencourtetal2007" }),
        new SentientScienceParameterReference(
            "sentient_technology_diffusion_model",
            "This selects how StarGen separates highest available technology from median access, adoption capacity, and lag pressure.\nAccess-Cost Density Proxy adds density and implementation-cost pressure.\nHuman audit is required before release claims.",
            new[] { "knez2023", "stokey2020", "comin2013", "cominmestieri2013" }),
        new SentientScienceParameterReference(
            "sentient_economic_complexity_model",
            "This selects how StarGen estimates capability breadth and invention pressure.\nCapability Portfolio Proxy adds resource diversity, cultural accumulation, access inequality, and binding constraints to trade and surplus.\nHuman audit is required before release claims.",
            new[] { "chacuaetal2024", "ballandetal2022", "bettencourtetal2007", "arvidssonetal2023" }),
        new SentientScienceParameterReference(
            "sentient_legitimacy_model",
            "This selects how StarGen treats legitimacy.\nInternal/External Norm Proxy separates local acceptance from outside recognition instead of using one capacity score.\nHuman audit is required before release claims.",
            new[] { "vankleefetal2023", "chowdhury2022" }),
    };

    private static readonly List<string> PanelSourceIds = new()
    {
        "hamiltonetal2020",
        "bettencourtetal2007",
        "arvidssonetal2023",
        "knez2023",
        "stokey2020",
        "comin2013",
        "cominmestieri2013",
        "chowdhury2022",
        "chacuaetal2024",
        "ballandetal2022",
        "vankleefetal2023",
    };

    public static string GetTooltipSummary(string parameterId)
    {
        SentientScienceParameterReference? reference = FindParameterReference(parameterId);
        return reference?.TooltipSummary ?? string.Empty;
    }

    public static IReadOnlyList<SentientScienceParameterReference> GetParameterReferences()
    {
        return ParameterReferences;
    }

    public static IReadOnlyList<string> GetParameterSourceIds(string parameterId)
    {
        SentientScienceParameterReference? reference = FindParameterReference(parameterId);
        if (reference == null)
        {
            return System.Array.Empty<string>();
        }

        return reference.SourceIds;
    }

    public static IReadOnlyList<string> GetHelpPanelSourceIds()
    {
        return PanelSourceIds;
    }

    public static SentientScienceSource? GetSource(string sourceId)
    {
        if (Sources.TryGetValue(sourceId, out SentientScienceSource? source))
        {
            return source;
        }

        return null;
    }

    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new();
        builder.AppendLine("[b][color=#f0c46a]Sentient-world science controls[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]These controls shape neutral sentient-world profile proxies. They are not final authority for culture, governance, economics, or release claims; human audit remains required.[/color]");
        builder.AppendLine();
        foreach (SentientScienceParameterReference reference in ParameterReferences)
        {
            builder.Append("[b]");
            builder.Append(reference.ParameterId);
            builder.AppendLine("[/b]");
            builder.AppendLine(reference.TooltipSummary);
            builder.Append("[color=#9cc4ff]Sources:[/color] ");
            builder.AppendLine(string.Join(", ", reference.SourceIds));
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static SentientScienceParameterReference? FindParameterReference(string parameterId)
    {
        foreach (SentientScienceParameterReference reference in ParameterReferences)
        {
            if (reference.ParameterId == parameterId)
            {
                return reference;
            }
        }

        return null;
    }
}
