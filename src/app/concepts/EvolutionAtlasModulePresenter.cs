using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Evolution;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the evolution concept fold-in.
/// </summary>
public sealed class EvolutionAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the evolution presenter.
    /// </summary>
    public EvolutionAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Evolution,
            DisplayName = "Evolution",
            Summary = "Generate trait-line progression and species-facing outcomes from environment and ecological pressure.",
            AcceptedContext = "Ecology/environment context or manual adaptive-pressure profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static string BuildSummary(EvolutionConceptSnapshot snapshot)
    {
        return snapshot.SpeciesName + " emerges from a "
            + snapshot.EnvironmentLabel.ToLowerInvariant()
            + " lineage, combining " + snapshot.BodyPlan.ToLowerInvariant()
            + " with " + snapshot.Locomotion.ToLowerInvariant()
            + " over " + snapshot.EvolutionSpanMya.ToString("0.0", CultureInfo.InvariantCulture)
            + " million years of adaptation.";
    }

    private static List<ConceptMetric> BuildMetrics(EvolutionConceptSnapshot snapshot)
    {
        return new List<ConceptMetric>
        {
            BuildPercentMetric("Adaptation fit", snapshot.AdaptationFit),
            BuildPercentMetric("Defense", snapshot.DefenseScore),
            BuildPercentMetric("Mobility", snapshot.MobilityScore),
            BuildPercentMetric("Cognition", snapshot.CognitionScore),
            new ConceptMetric
            {
                Label = "Traits",
                Value = snapshot.Traits.Count,
                MaxValue = 10.0,
                DisplayText = snapshot.Traits.Count.ToString(CultureInfo.InvariantCulture),
            },
        };
    }

    private static List<ConceptSection> BuildSections(EvolutionConceptSnapshot snapshot)
    {
        return new List<ConceptSection>
        {
            new ConceptSection
            {
                Title = "Species profile",
                Items = new List<string>
                {
                    "Body plan: " + snapshot.BodyPlan,
                    "Locomotion: " + snapshot.Locomotion,
                    "Diet: " + snapshot.Diet,
                    "Senses: " + snapshot.Senses,
                    "Sociality: " + snapshot.Sociality,
                },
            },
            new ConceptSection
            {
                Title = "Unlocked traits",
                Items = new List<string>(snapshot.Traits),
            },
            new ConceptSection
            {
                Title = "Lineage milestones",
                Items = new List<string>(snapshot.LineageMilestones),
            },
        };
    }

    private static ConceptMetric BuildPercentMetric(string label, double value)
    {
        return new ConceptMetric
        {
            Label = label,
            Value = value * 100.0,
            MaxValue = 100.0,
            DisplayText = (value * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
        };
    }
}
