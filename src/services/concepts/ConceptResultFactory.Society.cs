using System;
using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Civilization;
using StarGen.Domain.Concepts.Disease;
using StarGen.Domain.Concepts.Evolution;
using StarGen.Domain.Concepts.Language;
using StarGen.Domain.Concepts.Pipeline;

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
        if (context.SocietyState != null)
        {
            if (context.SocietyState.Status == ConceptRunStatus.NotApplicable)
            {
                return BuildNotApplicableResult(
                    ConceptKind.Civilization,
                    context.Seed,
                    "Civilisation Sandbox",
                    context.SourceLabel,
                    context.SocietyState.StatusReason,
                    context.SocietyState.Provenance.GeneratorVersion);
            }

            if (context.SocietyState.Status == ConceptRunStatus.Failed)
            {
                return BuildFailedResult(
                    ConceptKind.Civilization,
                    context.Seed,
                    "Civilisation Sandbox",
                    context.SourceLabel,
                    context.SocietyState.StatusReason,
                    context.SocietyState.Provenance.GeneratorVersion);
            }

            return BuildCivilizationResultFromState(context.SocietyState);
        }

        if (context.Population <= 0)
        {
            return BuildNotApplicableResult(
                ConceptKind.Civilization,
                context.Seed,
                "Civilisation Sandbox",
                context.SourceLabel,
                "Civilisation requires an extant sentient population.",
                CivilizationGeneratorVersion);
        }

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

    private static ConceptRunResult BuildCivilizationResultFromState(SocietyState state)
    {
        CivilizationConceptSnapshot snapshot = state.Snapshot;
        string title = snapshot.PolityName;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Civilisation Sandbox";
        }

        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.Generated;
        result.Title = title;
        result.Subtitle = snapshot.TechEra + " | " + snapshot.RegimeName;
        result.Summary = BuildCivilizationSummary(snapshot);
        result.Metrics = BuildCivilizationMetrics(snapshot);
        result.Sections = BuildCivilizationSections(snapshot);
        result.Provenance = state.Provenance;
        return result;
    }

    private static string BuildCivilizationSummary(CivilizationConceptSnapshot snapshot)
    {
        string economyLead = "regional exchange";
        if (snapshot.EconomySectors.Count > 0)
        {
            economyLead = snapshot.EconomySectors[0].ToLowerInvariant();
        }

        return snapshot.PolityName + " presents as a " + snapshot.RegimeName.ToLowerInvariant()
            + " in a " + snapshot.TechEra.ToLowerInvariant()
            + " horizon, organized around " + economyLead
            + " with a " + snapshot.CoreTerrain
            + " core and legitimacy anchored in " + snapshot.LegitimacyFrame
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
            BuildPercentMetric("Administrative capacity", snapshot.AdministrativeCapacity),
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
                "Core terrain: " + snapshot.CoreTerrain,
                "Legitimacy frame: " + snapshot.LegitimacyFrame,
                "Key systems: " + string.Join(", ", snapshot.KeyTechnologies),
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
        if (context.LanguageState != null)
        {
            if (context.LanguageState.Status == ConceptRunStatus.NotApplicable)
            {
                return BuildNotApplicableResult(
                    ConceptKind.Language,
                    context.Seed,
                    "Language Sandbox",
                    context.SourceLabel,
                    context.LanguageState.StatusReason,
                    context.LanguageState.Provenance.GeneratorVersion);
            }

            if (context.LanguageState.Status == ConceptRunStatus.Failed)
            {
                return BuildFailedResult(
                    ConceptKind.Language,
                    context.Seed,
                    "Language Sandbox",
                    context.SourceLabel,
                    context.LanguageState.StatusReason,
                    context.LanguageState.Provenance.GeneratorVersion);
            }

            return BuildLanguageResultFromState(context.LanguageState);
        }

        if (context.Population <= 0)
        {
            return BuildNotApplicableResult(
                ConceptKind.Language,
                context.Seed,
                "Language Sandbox",
                context.SourceLabel,
                "Language requires an extant sentient population or a manual society context.",
                LanguageGeneratorVersion);
        }

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

    private static ConceptRunResult BuildLanguageResultFromState(LanguageState state)
    {
        LanguageConceptSnapshot snapshot = state.Snapshot;
        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.Generated;
        result.Title = snapshot.Name;
        result.Subtitle = snapshot.WordOrder + " | " + snapshot.MorphType;
        result.Summary = BuildLanguageSummary(snapshot);
        result.Metrics = BuildLanguageMetrics(snapshot);
        result.Sections = BuildLanguageSections(snapshot);
        result.Provenance = state.Provenance;
        return result;
    }

    private static string BuildLanguageSummary(LanguageConceptSnapshot snapshot)
    {
        string toneText = "non-tonal";
        if (snapshot.HasTones)
        {
            toneText = "tonal";
        }

        return snapshot.Name + " is a " + toneText + " " + snapshot.MorphType.ToLowerInvariant()
            + " language with " + snapshot.WordOrder + " order and " + snapshot.HeadDirection.ToLowerInvariant()
            + " phrase structure, favoring " + snapshot.AffixPreference + " morphology.";
    }

    private static List<ConceptMetric> BuildLanguageMetrics(LanguageConceptSnapshot snapshot)
    {
        List<ConceptMetric> metrics = new List<ConceptMetric>();
        metrics.Add(BuildCountMetric("Consonants", snapshot.Consonants.Count, 20.0));
        metrics.Add(BuildCountMetric("Vowels", snapshot.Vowels.Count, 10.0));
        metrics.Add(BuildCountMetric("Syllable patterns", snapshot.SyllablePatterns.Count, 6.0));
        metrics.Add(BuildBinaryMetric("Case", snapshot.HasCase));
        metrics.Add(BuildBinaryMetric("Gender", snapshot.HasGender));
        metrics.Add(BuildBinaryMetric("Aspect", snapshot.HasAspect));
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
                "Affix preference: " + snapshot.AffixPreference,
                "Repair strategy: " + snapshot.RepairStrategy,
                "Repair particle: " + snapshot.RepairParticle,
                "Filler particle: " + snapshot.FillerParticle,
                "Features: " + BuildLanguageFeatureSummary(snapshot) + BuildOptionalLanguageInventory(snapshot),
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
        sections.Add(new ConceptSection
        {
            Title = "Morphology inventory",
            Items = BuildMorphologyInventory(snapshot),
        });
        return sections;
    }

    private static List<string> BuildLanguageLexiconItems(LanguageConceptSnapshot snapshot)
    {
        List<string> items = new List<string>();
        List<string> keys = new List<string>(snapshot.Lexicon.Keys);
        keys.Sort(StringComparer.Ordinal);
        foreach (string key in keys)
        {
            items.Add(key + " = " + snapshot.Lexicon[key]);
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

        if (snapshot.HasAspect)
        {
            features.Add("aspect");
        }

        if (features.Count == 0)
        {
            return "analytic inventory";
        }

        return string.Join(", ", features);
    }

    private static string BuildOptionalLanguageInventory(LanguageConceptSnapshot snapshot)
    {
        List<string> notes = new List<string>();
        if (snapshot.GenderClasses.Count > 0)
        {
            notes.Add("classes: " + string.Join(", ", snapshot.GenderClasses));
        }

        if (snapshot.Tones.Count > 0)
        {
            notes.Add("tones: " + string.Join(", ", snapshot.Tones));
        }

        if (notes.Count == 0)
        {
            return string.Empty;
        }

        return " | " + string.Join(" | ", notes);
    }

    private static List<string> BuildMorphologyInventory(LanguageConceptSnapshot snapshot)
    {
        List<string> items = new List<string>();
        AppendAffixItems(items, "Tense", snapshot.TenseAffixes);
        AppendAffixItems(items, "Noun", snapshot.NounAffixes);
        AppendAffixItems(items, "Verb", snapshot.VerbAffixes);
        AppendAffixItems(items, "Case", snapshot.CaseAffixes);
        AppendAffixItems(items, "Gender", snapshot.GenderAffixes);
        AppendAffixItems(items, "Adposition", snapshot.Adpositions);
        return items;
    }

    private static void AppendAffixItems(List<string> items, string label, Dictionary<string, string> values)
    {
        List<string> keys = new List<string>(values.Keys);
        keys.Sort(StringComparer.Ordinal);
        foreach (string key in keys)
        {
            items.Add(label + " - " + key + ": " + values[key]);
        }
    }

    private static ConceptRunResult BuildDiseaseResult(ConceptContextSnapshot context)
    {
        if (context.DiseaseState != null)
        {
            if (context.DiseaseState.Status == ConceptRunStatus.NotApplicable)
            {
                return BuildNotApplicableResult(
                    ConceptKind.Disease,
                    context.Seed,
                    "Disease Sandbox",
                    context.SourceLabel,
                    context.DiseaseState.StatusReason,
                    context.DiseaseState.Provenance.GeneratorVersion);
            }

            if (context.DiseaseState.Status == ConceptRunStatus.Failed)
            {
                return BuildFailedResult(
                    ConceptKind.Disease,
                    context.Seed,
                    "Disease Sandbox",
                    context.SourceLabel,
                    context.DiseaseState.StatusReason,
                    context.DiseaseState.Provenance.GeneratorVersion);
            }

            return BuildDiseaseResultFromState(context, context.DiseaseState);
        }

        if (context.Population <= 0 && !SupportsManualEcology(context))
        {
            return BuildNotApplicableResult(
                ConceptKind.Disease,
                context.Seed,
                "Disease Sandbox",
                context.SourceLabel,
                "Disease requires a biological host ecology or an inhabited population.",
                DiseaseGeneratorVersion);
        }

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

    private static ConceptRunResult BuildDiseaseResultFromState(ConceptContextSnapshot context, DiseaseState state)
    {
        DiseaseConceptSnapshot snapshot = state.Snapshot;
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

        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.Generated;
        result.Title = title;
        result.Subtitle = snapshot.PathogenType;
        result.Summary = BuildDiseaseSummary(snapshot, population);
        result.Metrics = BuildDiseaseMetrics(snapshot, population);
        result.Sections = BuildDiseaseSections(snapshot);
        result.Provenance = state.Provenance;
        return result;
    }

    private static string BuildDiseaseSummary(DiseaseConceptSnapshot snapshot, int population)
    {
        double peakShare = snapshot.PeakInfected / (double)population;
        return snapshot.PathogenType + " outbreak model projects a peak burden of "
            + (peakShare * 100.0).ToString("0.0", CultureInfo.InvariantCulture)
            + "% of the population around day "
            + snapshot.PeakDay.ToString(CultureInfo.InvariantCulture)
            + " in " + snapshot.HostEnvironment.ToLowerInvariant()
            + ", with " + snapshot.Symptoms[0].ToLowerInvariant() + " as a leading symptom signal.";
    }

    private static List<ConceptMetric> BuildDiseaseMetrics(DiseaseConceptSnapshot snapshot, int population)
    {
        return new List<ConceptMetric>
        {
            BuildPercentMetric("Infectivity", snapshot.Infectivity),
            BuildPercentMetric("Severity", snapshot.Severity),
            BuildPercentMetric("Lethality", snapshot.Lethality),
            BuildPercentMetric("Incubation", snapshot.Incubation),
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
                    "Host environment: " + snapshot.HostEnvironment,
                    "Mutability: " + (snapshot.Mutability * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
                    "Resilience: " + (snapshot.Resilience * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
                    "Airborne pressure: " + (snapshot.Airborne * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
                    "Immune evasion: " + (snapshot.ImmuneEvasion * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%",
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
                Title = "Mutation history",
                Items = new List<string>(snapshot.MutationEvents),
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
        if (context.SpeciesEvolution != null)
        {
            if (context.SpeciesEvolution.Status == ConceptRunStatus.NotApplicable)
            {
                return BuildNotApplicableResult(
                    ConceptKind.Evolution,
                    context.Seed,
                    "Evolution Sandbox",
                    context.SourceLabel,
                    context.SpeciesEvolution.StatusReason,
                    context.SpeciesEvolution.Provenance.GeneratorVersion);
            }

            if (context.SpeciesEvolution.Status == ConceptRunStatus.Failed)
            {
                return BuildFailedResult(
                    ConceptKind.Evolution,
                    context.Seed,
                    "Evolution Sandbox",
                    context.SourceLabel,
                    context.SpeciesEvolution.StatusReason,
                    context.SpeciesEvolution.Provenance.GeneratorVersion);
            }

            return BuildEvolutionResultFromState(context.SpeciesEvolution);
        }

        if (!SupportsManualEcology(context))
        {
            return BuildNotApplicableResult(
                ConceptKind.Evolution,
                context.Seed,
                "Evolution Sandbox",
                context.SourceLabel,
                "Evolution requires a life-supporting ecological context.",
                EvolutionGeneratorVersion);
        }

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

    private static ConceptRunResult BuildEvolutionResultFromState(SpeciesEvolutionState state)
    {
        EvolutionConceptSnapshot snapshot = state.Snapshot;
        ConceptRunResult result = new ConceptRunResult();
        result.Status = ConceptRunStatus.Generated;
        result.Title = snapshot.SpeciesName;
        result.Subtitle = snapshot.EnvironmentLabel;
        result.Summary = BuildEvolutionSummary(snapshot);
        result.Metrics = BuildEvolutionMetrics(snapshot);
        result.Sections = BuildEvolutionSections(snapshot);
        result.Provenance = state.Provenance;
        return result;
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
                Value = snapshot.UnlockedNodes.Count,
                MaxValue = 24.0,
                DisplayText = snapshot.UnlockedNodes.Count.ToString(CultureInfo.InvariantCulture),
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
                    "Reproduction: " + snapshot.Reproduction,
                    "Communication: " + snapshot.Communication,
                    "Metabolism: " + snapshot.Metabolism,
                    "Integument: " + snapshot.Integument,
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
        double numericValue = 0.0;
        string displayText = "No";
        if (value)
        {
            numericValue = 1.0;
            displayText = "Yes";
        }

        return new ConceptMetric
        {
            Label = label,
            Value = numericValue,
            MaxValue = 1.0,
            DisplayText = displayText,
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
