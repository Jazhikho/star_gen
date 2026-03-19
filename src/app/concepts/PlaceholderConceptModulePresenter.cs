using System.Collections.Generic;
using StarGen.Domain.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Temporary presenter used until the concept-specific fold-in is complete.
/// </summary>
public sealed class PlaceholderConceptModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates a placeholder presenter.
    /// </summary>
    public PlaceholderConceptModulePresenter(ConceptModuleDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return new ConceptRunResult
        {
            Title = Descriptor.DisplayName,
            Subtitle = Descriptor.AcceptedContext,
            Summary = Descriptor.Summary,
            Metrics = new List<ConceptMetric>
            {
                new ConceptMetric { Label = "Context population", Value = request.Context.Population, MaxValue = 1000000000.0, DisplayText = request.Context.Population.ToString() },
                new ConceptMetric { Label = "Habitability", Value = request.Context.HabitabilityScore, MaxValue = 10.0, DisplayText = request.Context.HabitabilityScore + " / 10" },
                new ConceptMetric { Label = "Water availability", Value = request.Context.WaterAvailability, MaxValue = 1.0, DisplayText = request.Context.WaterAvailability.ToString("0.00") },
            },
            Sections = new List<ConceptSection>
            {
                new ConceptSection
                {
                    Title = "Context",
                    Items = new List<string>
                    {
                        "Source: " + request.Context.SourceLabel,
                        "Body: " + (string.IsNullOrEmpty(request.Context.BodyName) ? "Manual" : request.Context.BodyName),
                        "Biome: " + request.Context.DominantBiome,
                    },
                },
                new ConceptSection
                {
                    Title = "Status",
                    Items = new List<string>
                    {
                        "Atlas shell wired. Concept-specific deterministic fold-in follows in subsequent commits.",
                    },
                },
            },
            Provenance = new ConceptProvenance
            {
                ConceptId = Descriptor.Kind.ToString(),
                Seed = request.Context.Seed,
                GeneratorVersion = "atlas-shell-0.1.0",
                SourceContext = request.Context.SourceLabel,
            },
        };
    }
}
