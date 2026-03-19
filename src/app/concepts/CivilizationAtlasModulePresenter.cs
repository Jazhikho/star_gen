using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Civilization;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the civilisation concept fold-in.
/// </summary>
public sealed class CivilizationAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the civilisation presenter.
    /// </summary>
    public CivilizationAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Civilization,
            DisplayName = "Civilisation",
            Summary = "Trace regime, economy, civic values, and historical trajectory as a population-facing worldbuilding layer.",
            AcceptedContext = "Population history, colony/native context, or manual social profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static string BuildSummary(CivilizationConceptSnapshot snapshot)
    {
        string economyLead = snapshot.EconomySectors.Count > 0 ? snapshot.EconomySectors[0].ToLowerInvariant() : "regional exchange";
        return snapshot.PolityName + " presents as a " + snapshot.RegimeName.ToLowerInvariant()
            + " in a " + snapshot.TechEra.ToLowerInvariant()
            + " horizon, organized around " + economyLead
            + " and legitimized through " + snapshot.LegitimacyFrame
            + ".";
    }

    private static List<ConceptMetric> BuildMetrics(CivilizationConceptSnapshot snapshot)
    {
        return new List<ConceptMetric>
        {
            BuildMetric("Stability", snapshot.Stability),
            BuildMetric("Centralization", snapshot.Centralization),
            BuildMetric("Inclusiveness", snapshot.Inclusiveness),
            BuildMetric("Innovation", snapshot.Innovation),
            BuildMetric("External pressure", snapshot.ExternalPressure),
        };
    }

    private static List<ConceptSection> BuildSections(CivilizationConceptSnapshot snapshot)
    {
        List<ConceptSection> sections = new List<ConceptSection>();
        sections.Add(new ConceptSection
        {
            Title = "Governance",
            Items = new List<string>
            {
                "Regime: " + snapshot.RegimeName,
                "Technology era: " + snapshot.TechEra,
                "Legitimacy frame: " + snapshot.LegitimacyFrame,
            },
        });
        sections.Add(new ConceptSection
        {
            Title = "Economy and values",
            Items = MergeLists(snapshot.EconomySectors, snapshot.CulturalValues),
        });
        sections.Add(new ConceptSection
        {
            Title = "Historical trajectory",
            Items = new List<string>(snapshot.HistoricalMilestones),
        });
        sections.Add(new ConceptSection
        {
            Title = "External posture",
            Items = new List<string>(snapshot.ExternalPosture),
        });
        return sections;
    }

    private static ConceptMetric BuildMetric(string label, double value)
    {
        return new ConceptMetric
        {
            Label = label,
            Value = value * 100.0,
            MaxValue = 100.0,
            DisplayText = (value * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
        };
    }

    private static List<string> MergeLists(List<string> left, List<string> right)
    {
        List<string> merged = new List<string>();
        merged.AddRange(left);
        merged.AddRange(right);
        return merged;
    }
}
