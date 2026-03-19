using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Language;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Atlas presenter for the language concept fold-in.
/// </summary>
public sealed class LanguageAtlasModulePresenter : IConceptModulePresenter
{
    /// <summary>
    /// Creates the language presenter.
    /// </summary>
    public LanguageAtlasModulePresenter()
    {
        Descriptor = new ConceptModuleDescriptor
        {
            Kind = ConceptKind.Language,
            DisplayName = "Language",
            Summary = "Generate phonology, grammar, lexicon, and sample utterances for naming and cultural presentation.",
            AcceptedContext = "Civilisation context or manual language profile",
        };
    }

    /// <inheritdoc />
    public ConceptModuleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public ConceptRunResult Run(ConceptRunRequest request)
    {
        return ConceptResultFactory.Run(request);
    }

    private static string BuildSummary(LanguageConceptSnapshot snapshot)
    {
        string toneText = snapshot.HasTones ? "tonal" : "non-tonal";
        return snapshot.Name + " is a " + toneText + " " + snapshot.MorphType.ToLowerInvariant()
            + " language with " + snapshot.WordOrder + " order and " + snapshot.HeadDirection.ToLowerInvariant()
            + " phrase structure.";
    }

    private static List<ConceptMetric> BuildMetrics(LanguageConceptSnapshot snapshot)
    {
        List<ConceptMetric> metrics = new List<ConceptMetric>();
        metrics.Add(BuildCountMetric("Consonants", snapshot.Consonants.Count, 20.0));
        metrics.Add(BuildCountMetric("Vowels", snapshot.Vowels.Count, 10.0));
        metrics.Add(BuildCountMetric("Syllable patterns", snapshot.SyllablePatterns.Count, 6.0));
        metrics.Add(BuildBinaryMetric("Case", snapshot.HasCase));
        metrics.Add(BuildBinaryMetric("Gender", snapshot.HasGender));
        return metrics;
    }

    private static List<ConceptSection> BuildSections(LanguageConceptSnapshot snapshot)
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
                "Features: " + BuildFeatureSummary(snapshot),
            },
        });
        sections.Add(new ConceptSection
        {
            Title = "Lexicon sample",
            Items = BuildLexiconItems(snapshot),
        });
        sections.Add(new ConceptSection
        {
            Title = "Example utterances",
            Items = new List<string>(snapshot.ExampleSentences),
        });
        return sections;
    }

    private static List<string> BuildLexiconItems(LanguageConceptSnapshot snapshot)
    {
        List<string> items = new List<string>();
        foreach (KeyValuePair<string, string> entry in snapshot.Lexicon)
        {
            items.Add(entry.Key + " = " + entry.Value);
        }

        return items;
    }

    private static string BuildFeatureSummary(LanguageConceptSnapshot snapshot)
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
}
