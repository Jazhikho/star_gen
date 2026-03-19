using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Civilization;
using StarGen.Domain.Concepts.Disease;
using StarGen.Domain.Concepts.Evolution;
using StarGen.Domain.Concepts.Language;

namespace StarGen.Services.Concepts;

/// <summary>
/// Civilization, language, disease, and evolution concept-result builders.
/// </summary>
public static partial class ConceptResultFactory
{
    private const string CivilizationGeneratorVersion = "civilization-atlas-v1";
    private const string LanguageGeneratorVersion = "language-atlas-v1";
    private const string DiseaseGeneratorVersion = "disease-atlas-v1";
    private const string EvolutionGeneratorVersion = "evolution-atlas-v1";

    private static ConceptRunResult BuildCivilizationResult(ConceptContextSnapshot context)
    {
        CivilizationConceptSnapshot snapshot = CivilizationConceptGenerator.Generate(context);
        string title = snapshot.PolityName;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Civilisation Sandbox";
        }

        return new ConceptRunResult
        {
            Title = title,
            Subtitle = snapshot.TechEra + " | " + snapshot.RegimeName,
            Summary = BuildCivilizationSummary(snapshot),
            Metrics = BuildCivilizationMetrics(snapshot),
            Sections = BuildCivilizationSections(snapshot),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Civilization.ToString(),
                Seed = context.Seed,
                GeneratorVersion = CivilizationGeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static string BuildCivilizationSummary(CivilizationConceptSnapshot snapshot)
    {
        string economyLead = snapshot.EconomySectors.Count > 0 ? snapshot.EconomySectors[0].ToLowerInvariant() : "regional exchange";
        return snapshot.PolityName + " presents as a " + snapshot.RegimeName.ToLowerInvariant()
            + " in a " + snapshot.TechEra.ToLowerInvariant()
            + " horizon, organized around " + economyLead
            + " and legitimized through " + snapshot.LegitimacyFrame
            + ".";
    }

    private static List<ConceptMetric> BuildCivilizationMetrics(CivilizationConceptSnapshot snapshot)
    {
        return new List<ConceptMetric>
        {
            BuildPercentMetric("Stability", snapshot.Stability),
            BuildPercentMetric("Centralization", snapshot.Centralization),
            BuildPercentMetric("Inclusiveness", snapshot.Inclusiveness),
            BuildPercentMetric("Innovation", snapshot.Innovation),
            BuildPercentMetric("External pressure", snapshot.ExternalPressure),
        };
    }

    private static List<ConceptSection> BuildCivilizationSections(CivilizationConceptSnapshot snapshot)
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

    private static ConceptRunResult BuildLanguageResult(ConceptContextSnapshot context)
    {
        LanguageConceptSnapshot snapshot = LanguageConceptGenerator.Generate(context);
        return new ConceptRunResult
        {
            Title = snapshot.Name,
            Subtitle = snapshot.WordOrder + " | " + snapshot.MorphType,
            Summary = BuildLanguageSummary(snapshot),
            Metrics = BuildLanguageMetrics(snapshot),
            Sections = BuildLanguageSections(snapshot),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Language.ToString(),
                Seed = context.Seed,
                GeneratorVersion = LanguageGeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static string BuildLanguageSummary(LanguageConceptSnapshot snapshot)
    {
        string toneText = snapshot.HasTones ? "tonal" : "non-tonal";
        return snapshot.Name + " is a " + toneText + " " + snapshot.MorphType.ToLowerInvariant()
            + " language with " + snapshot.WordOrder + " order and " + snapshot.HeadDirection.ToLowerInvariant()
            + " phrase structure.";
    }

    private static List<ConceptMetric> BuildLanguageMetrics(LanguageConceptSnapshot snapshot)
    {
        List<ConceptMetric> metrics = new List<ConceptMetric>();
        metrics.Add(BuildCountMetric("Consonants", snapshot.Consonants.Count, 20.0));
        metrics.Add(BuildCountMetric("Vowels", snapshot.Vowels.Count, 10.0));
        metrics.Add(BuildCountMetric("Syllable patterns", snapshot.SyllablePatterns.Count, 6.0));
        metrics.Add(BuildBinaryMetric("Case", snapshot.HasCase));
        metrics.Add(BuildBinaryMetric("Gender", snapshot.HasGender));
        return metrics;
    }

    private static List<ConceptSection> BuildLanguageSections(LanguageConceptSnapshot snapshot)
    {
        List<ConceptSection> sections = new List<ConceptSection>();
        sections.Add(new ConceptSection
        {
            Title = "Phonology",
            Items = new List<string>
            {
                "Consonants: " + string.Join(", ", snapshot.Consonants),
                "Vowels: " + string.Join(", ", snapshot.Vowels),
                "Syllable shapes: " + string.Join(", ", snapshot.SyllablePatterns),
            },
        });
        sections.Add(new ConceptSection
        {
            Title = "Grammar",
            Items = new List<string>
            {
                "Word order: " + snapshot.WordOrder,
                "Morphology: " + snapshot.MorphType,
                "Head direction: " + snapshot.HeadDirection,
                "Repair strategy: " + snapshot.RepairStrategy,
                "Features: " + BuildLanguageFeatureSummary(snapshot),
            },
        });
        sections.Add(new ConceptSection
        {
            Title = "Lexicon sample",
            Items = BuildLanguageLexiconItems(snapshot),
        });
        sections.Add(new ConceptSection
        {
            Title = "Example utterances",
            Items = new List<string>(snapshot.ExampleSentences),
        });
        return sections;
    }

    private static List<string> BuildLanguageLexiconItems(LanguageConceptSnapshot snapshot)
    {
        List<string> items = new List<string>();
        foreach (KeyValuePair<string, string> entry in snapshot.Lexicon)
        {
            items.Add(entry.Key + " = " + entry.Value);
        }

        return items;
    }

    private static string BuildLanguageFeatureSummary(LanguageConceptSnapshot snapshot)
    {
        List<string> features = new List<string>();
        if (snapshot.HasCase)
        {
            features.Add("case");
        }

        if (snapshot.HasGender)
        {
            features.Add("gender");
        }

        if (snapshot.HasTones)
        {
            features.Add("tone");
        }

        if (snapshot.HasVowelHarmony)
        {
            features.Add("vowel harmony");
        }

        if (features.Count == 0)
        {
            return "analytic inventory";
        }

        return string.Join(", ", features);
    }

    private static ConceptRunResult BuildDiseaseResult(ConceptContextSnapshot context)
    {
        DiseaseConceptSnapshot snapshot = DiseaseConceptGenerator.Generate(context);
        int population = context.Population;
        if (population <= 0)
        {
            population = 1000000;
        }

        string title = context.BodyName;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Disease Sandbox";
        }
        else
        {
            title += " Outbreak Model";
        }

        return new ConceptRunResult
        {
            Title = title,
            Subtitle = snapshot.PathogenType,
            Summary = BuildDiseaseSummary(snapshot, population),
            Metrics = BuildDiseaseMetrics(snapshot, population),
            Sections = BuildDiseaseSections(snapshot),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Disease.ToString(),
                Seed = context.Seed,
                GeneratorVersion = DiseaseGeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static string BuildDiseaseSummary(DiseaseConceptSnapshot snapshot, int population)
    {
        double peakShare = snapshot.PeakInfected / (double)population;
        return snapshot.PathogenType + " outbreak model projects a peak burden of "
            + (peakShare * 100.0).ToString("0.0", CultureInfo.InvariantCulture)
            + "% of the population around day "
            + snapshot.PeakDay.ToString(CultureInfo.InvariantCulture)
            + ", with " + snapshot.Symptoms[0].ToLowerInvariant() + " as a leading symptom signal.";
    }

    private static List<ConceptMetric> BuildDiseaseMetrics(DiseaseConceptSnapshot snapshot, int population)
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

    private static List<ConceptSection> BuildDiseaseSections(DiseaseConceptSnapshot snapshot)
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

    private static ConceptRunResult BuildEvolutionResult(ConceptContextSnapshot context)
    {
        EvolutionConceptSnapshot snapshot = EvolutionConceptGenerator.Generate(context);
        return new ConceptRunResult
        {
            Title = snapshot.SpeciesName,
            Subtitle = snapshot.EnvironmentLabel,
            Summary = BuildEvolutionSummary(snapshot),
            Metrics = BuildEvolutionMetrics(snapshot),
            Sections = BuildEvolutionSections(snapshot),
            Provenance = new ConceptProvenance
            {
                ConceptId = ConceptKind.Evolution.ToString(),
                Seed = context.Seed,
                GeneratorVersion = EvolutionGeneratorVersion,
                SourceContext = context.SourceLabel,
            },
        };
    }

    private static string BuildEvolutionSummary(EvolutionConceptSnapshot snapshot)
    {
        return snapshot.SpeciesName + " emerges from a "
            + snapshot.EnvironmentLabel.ToLowerInvariant()
            + " lineage, combining " + snapshot.BodyPlan.ToLowerInvariant()
            + " with " + snapshot.Locomotion.ToLowerInvariant()
            + " over " + snapshot.EvolutionSpanMya.ToString("0.0", CultureInfo.InvariantCulture)
            + " million years of adaptation.";
    }

    private static List<ConceptMetric> BuildEvolutionMetrics(EvolutionConceptSnapshot snapshot)
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

    private static List<ConceptSection> BuildEvolutionSections(EvolutionConceptSnapshot snapshot)
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
            DisplayText = (value * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%",
        };
    }

    private static ConceptMetric BuildCountMetric(string label, int value, double maxValue)
    {
        return new ConceptMetric
        {
            Label = label,
            Value = value,
            MaxValue = maxValue,
            DisplayText = value.ToString(CultureInfo.InvariantCulture),
        };
    }

    private static ConceptMetric BuildBinaryMetric(string label, bool value)
    {
        return new ConceptMetric
        {
            Label = label,
            Value = value ? 1.0 : 0.0,
            MaxValue = 1.0,
            DisplayText = value ? "Yes" : "No",
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
