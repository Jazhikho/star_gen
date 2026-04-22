using System.Collections.Generic;
using System.Text;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Source citation metadata used by life-model help surfaces and tests.
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
/// Canonical science notes and source registry for life-model controls.
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
            "life_framework",
            "This is the top-level life preset for the universe.\nIt decides the default stance for how easily life starts, how selective complex life is, and how hard civilizations are to get.\nEarth-Anchored Composite is a documented blend of the reviewed papers, not a one-paper default.",
            new[] { "lineweaverdavis2002", "spiegelturner2012", "forganrice2010", "mills2024", "balbi2023", "kopparapu2014" }),
        new LifeScienceParameterReference(
            "abiogenesis_model",
            "Abiogenesis means life starting from non-living chemistry.\nRapid Start makes simple life easier to appear once water and energy are present.\nConservative keeps early life possible, but treats Earth's fast start as weak evidence.",
            new[] { "lineweaverdavis2002", "spiegelturner2012" }),
        new LifeScienceParameterReference(
            "complex_life_model",
            "This sets how hard it is for simple life to become rich, complex ecosystems.\nEnvironmental Windows rewards long calm habitable periods.\nRare Earth Filters make complex life much pickier than microbes.",
            new[] { "mills2024", "forganrice2010", "lineweaverdavis2002", "spiegelturner2012" }),
        new LifeScienceParameterReference(
            "civilization_model",
            "This sets the extra bottlenecks between a sentient lineage and a technological civilization.\nRare Civilizations keeps advanced societies uncommon even when life exists.\nTechnosphere Oxygen Bottleneck makes oxygen-rich atmospheres more important at that late stage.",
            new[] { "forganrice2010", "balbi2023" }),
        new LifeScienceParameterReference(
            "environmental_window_weight",
            "This controls how strongly long stable habitable windows matter.\nHigher weights reward worlds with long calm climates, moderate radiation, and durable surface habitability more strongly.",
            new[] { "mills2024" }),
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
        if (reference == null)
        {
            return string.Empty;
        }

        return reference.TooltipSummary;
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

    public static string GetFrameworkLabel(GenerationUseCaseSettings.LifeFrameworkType framework)
    {
        return framework switch
        {
            GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite => "Earth-Anchored Composite",
            GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres => "Rapid Biospheres",
            GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows => "Environmental Windows",
            GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife => "Rare Complex Life",
            _ => "Earth-Anchored Composite",
        };
    }

    public static string GetAbiogenesisLabel(GenerationUseCaseSettings.AbiogenesisModelType model)
    {
        return model switch
        {
            GenerationUseCaseSettings.AbiogenesisModelType.RapidStart => "Rapid Start",
            GenerationUseCaseSettings.AbiogenesisModelType.Conservative => "Conservative",
            _ => "Follow Framework",
        };
    }

    public static string GetComplexLifeLabel(GenerationUseCaseSettings.ComplexLifeModelType model)
    {
        return model switch
        {
            GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite => "Earth-Anchored Composite",
            GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows => "Environmental Windows",
            GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters => "Rare Earth Filters",
            _ => "Follow Framework",
        };
    }

    public static string GetCivilizationLabel(GenerationUseCaseSettings.CivilizationModelType model)
    {
        return model switch
        {
            GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite => "Earth-Anchored Composite",
            GenerationUseCaseSettings.CivilizationModelType.RareCivilizations => "Rare Civilizations",
            GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck => "Technosphere Oxygen Bottleneck",
            _ => "Follow Framework",
        };
    }

    public static string GetEnvironmentalWindowWeightLabel(GenerationUseCaseSettings.EnvironmentalWindowWeightType weight)
    {
        return weight switch
        {
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Low => "Low",
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate => "Moderate",
            GenerationUseCaseSettings.EnvironmentalWindowWeightType.High => "High",
            _ => "Follow Framework",
        };
    }

    public static string BuildHelpPanelBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Life models[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]These controls do more than rename outcomes. They change how easily StarGen allows biospheres, complex life, sentient lineages, and technological civilizations to appear on generated worlds.[/color]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "Earth-Anchored Composite",
            "Use a documented Earth-anchored synthesis instead of pretending one paper answers everything.",
            "This is a composite preset built from the reviewed papers in the repo. It keeps simple life plausible on wet energy-rich worlds, makes complex life depend on long stable windows, keeps advanced life more selective than microbes, and adds an extra late bottleneck for technological civilizations.");
        builder.AppendLine("[color=#9cc4ff]What this composite assumes:[/color]");
        builder.AppendLine("- Simple life is plausible on suitable wet, energy-rich worlds, but not automatically common everywhere. [Lineweaver & Davis; Spiegel & Turner]");
        builder.AppendLine("- Complex life benefits from long stable habitable windows and favorable surface conditions. [Mills et al.]");
        builder.AppendLine("- Complex or intelligent life stays more selective than simple life. [Forgan & Rice]");
        builder.AppendLine("- Technological civilizations face extra atmospheric and oxygen bottlenecks beyond basic biosphere success. [Balbi et al.]");
        builder.AppendLine("- Planetary mass and stellar flux still gate whether a world sits in a viable environmental envelope at all. [Kopparapu et al.]");
        builder.AppendLine();

        AppendGuideSection(
            builder,
            "Rapid Biospheres",
            "Assume life starts fairly easily once water and energy are in place.",
            "More habitable worlds pick up biospheres. Complex life and civilizations are still filtered later, so this mostly raises living worlds rather than advanced ones.");

        AppendGuideSection(
            builder,
            "Environmental Windows",
            "Assume complex life depends strongly on long calm windows rather than pure luck alone.",
            "Worlds with long stable climates, moderate radiation, good surface diversity, and durable habitability get a bigger boost toward complex ecosystems and civilizations.");

        AppendGuideSection(
            builder,
            "Rare Complex Life",
            "Assume microbes may be common but complex life needs unusually favorable conditions.",
            "Biospheres still happen, but complex ecosystems and civilizations become much rarer because the later filters stay tight.");

        builder.AppendLine("[b][color=#f0c46a]What StarGen actually does with this[/color][/b]");
        builder.AppendLine("1. It separates biosphere support, abiogenesis, complex life, sentient lineages, and technological civilizations.");
        builder.AppendLine("2. It applies oxygen-rich atmospheric bottlenecks at the civilization stage instead of folding them into early life.");
        builder.AppendLine("3. It keeps Kopparapu-style habitability limits as environmental gating, not as a selectable life theory.");
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
