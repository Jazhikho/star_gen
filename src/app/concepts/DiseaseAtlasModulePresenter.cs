using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Disease;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the disease concept fold-in.
/// </summary>
public sealed class DiseaseAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the disease presenter.
    /// </summary>
    public DiseaseAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Disease,
            DisplayName = "Disease",
            Summary = "Model outbreak traits and population impacts from environment, density, and medical context.",
            AcceptedContext = "Planet environment, population density, or manual epidemiology profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static string BuildSummary(DiseaseConceptSnapshot snapshot, int population)
    {
        double peakShare = snapshot.PeakInfected / (double)population;
        return snapshot.PathogenType + " outbreak model projects a peak burden of "
            + (peakShare * 100.0).ToString("0.0", CultureInfo.InvariantCulture)
            + "% of the population around day "
            + snapshot.PeakDay.ToString(CultureInfo.InvariantCulture)
            + ", with " + snapshot.Symptoms[0].ToLowerInvariant() + " as a leading symptom signal.";
    }

    private static List<ConceptMetric> BuildMetrics(DiseaseConceptSnapshot snapshot, int population)
    {
        return new List<ConceptMetric>
        {
            BuildPercentMetric("Infectivity", snapshot.Infectivity),
            BuildPercentMetric("Severity", snapshot.Severity),
            BuildPercentMetric("Lethality", snapshot.Lethality),
            BuildPercentMetric("Peak infected", snapshot.PeakInfected / (double)population),
            BuildPercentMetric("Mortality", snapshot.TotalDeaths / (double)population),
        };
    }

    private static List<ConceptSection> BuildSections(DiseaseConceptSnapshot snapshot)
    {
        return new List<ConceptSection>
        {
            new ConceptSection
            {
                Title = "Pathogen profile",
                Items = new List<string>
                {
                    "Type: " + snapshot.PathogenType,
                    "Mutability: " + (snapshot.Mutability * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
                    "Resilience: " + (snapshot.Resilience * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
                },
            },
            new ConceptSection
            {
                Title = "Symptoms",
                Items = new List<string>(snapshot.Symptoms),
            },
            new ConceptSection
            {
                Title = "Epidemic model",
                Items = new List<string>
                {
                    "Total infected: " + snapshot.TotalInfected.ToString("N0", CultureInfo.InvariantCulture),
                    "Total deaths: " + snapshot.TotalDeaths.ToString("N0", CultureInfo.InvariantCulture),
                    "Peak infected: " + snapshot.PeakInfected.ToString("N0", CultureInfo.InvariantCulture),
                    "Peak day: " + snapshot.PeakDay.ToString(CultureInfo.InvariantCulture),
                },
            },
            new ConceptSection
            {
                Title = "Environmental drivers",
                Items = new List<string>(snapshot.EnvironmentalDrivers),
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
            DisplayText = (value * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%",
        };
    }
}
