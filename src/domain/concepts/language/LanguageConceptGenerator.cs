using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Language;

/// <summary>
/// Deterministic conlang generator used by the concept atlas and runtime concept pipeline.
/// </summary>
public static class LanguageConceptGenerator
{
    private sealed class PhonemeOption
    {
        public string Symbol { get; }

        public int Weight { get; }

        public PhonemeOption(string symbol, int weight)
        {
            Symbol = symbol;
            Weight = weight;
        }
    }

    private sealed class AffixForm
    {
        public string Form { get; }

        public string Placement { get; }

        public AffixForm(string form, string placement)
        {
            Form = form;
            Placement = placement;
        }
    }

    private static readonly List<PhonemeOption> ConsonantPool = new List<PhonemeOption>
    {
        new PhonemeOption("m", 9),
        new PhonemeOption("n", 9),
        new PhonemeOption("p", 8),
        new PhonemeOption("t", 8),
        new PhonemeOption("k", 8),
        new PhonemeOption("b", 6),
        new PhonemeOption("d", 6),
        new PhonemeOption("g", 6),
        new PhonemeOption("f", 5),
        new PhonemeOption("v", 4),
        new PhonemeOption("s", 8),
        new PhonemeOption("z", 4),
        new PhonemeOption("sh", 5),
        new PhonemeOption("zh", 2),
        new PhonemeOption("x", 2),
        new PhonemeOption("h", 4),
        new PhonemeOption("th", 2),
        new PhonemeOption("dh", 2),
        new PhonemeOption("ts", 3),
        new PhonemeOption("ch", 4),
        new PhonemeOption("j", 3),
        new PhonemeOption("l", 8),
        new PhonemeOption("r", 8),
        new PhonemeOption("w", 6),
        new PhonemeOption("y", 6),
        new PhonemeOption("ng", 3),
        new PhonemeOption("q", 1),
        new PhonemeOption("'", 1),
    };

    private static readonly List<PhonemeOption> VowelPool = new List<PhonemeOption>
    {
        new PhonemeOption("a", 10),
        new PhonemeOption("e", 9),
        new PhonemeOption("i", 9),
        new PhonemeOption("o", 8),
        new PhonemeOption("u", 8),
        new PhonemeOption("ae", 3),
        new PhonemeOption("ai", 4),
        new PhonemeOption("au", 3),
        new PhonemeOption("ea", 3),
        new PhonemeOption("ei", 3),
        new PhonemeOption("ia", 4),
        new PhonemeOption("io", 3),
        new PhonemeOption("oa", 3),
        new PhonemeOption("oi", 2),
        new PhonemeOption("ou", 3),
        new PhonemeOption("uu", 1),
        new PhonemeOption("y", 1),
    };

    private static readonly string[] SyllablePatterns =
    {
        "V",
        "CV",
        "VC",
        "CVC",
        "CCV",
        "CVCC",
        "CCVC",
        "VCC",
    };

    private static readonly string[] WordOrders =
    {
        "SOV",
        "SVO",
        "VSO",
        "VOS",
        "OVS",
        "OSV",
    };

    private static readonly string[] MorphTypes =
    {
        "Agglutinative",
        "Fusional",
        "Isolating",
        "Polysynthetic",
    };

    private static readonly string[] GenderOptions =
    {
        "animate",
        "inanimate",
        "celestial",
        "earthly",
        "masculine",
        "feminine",
        "neuter",
        "sacred",
        "profane",
        "human",
        "nonhuman",
    };

    private static readonly string[] ToneInventory =
    {
        "high",
        "low",
        "rising",
        "falling",
        "mid",
    };

    private static readonly string[] RepairStrategies =
    {
        "Repeat the last word with rising intonation before correcting it.",
        "Insert a filler particle before the corrected word.",
        "Restart the full clause after a brief pause.",
        "Use a correction particle after the mistaken word.",
        "Pause, negate the previous form, then continue with the corrected word.",
    };

    private static readonly string[] AdpositionRoles =
    {
        "of",
        "in",
        "at",
        "to",
        "from",
        "with",
        "by",
        "on",
        "under",
        "over",
        "before",
        "after",
        "between",
        "through",
        "against",
        "without",
        "about",
        "because",
        "if",
        "and",
        "or",
        "not",
        "than",
    };

    private static readonly string[] Glosses =
    {
        "sun",
        "moon",
        "star",
        "water",
        "fire",
        "earth",
        "wind",
        "tree",
        "stone",
        "person",
        "friend",
        "enemy",
        "leader",
        "warrior",
        "healer",
        "merchant",
        "spirit",
        "law",
        "ritual",
        "trade",
        "story",
        "road",
        "home",
        "gift",
        "debt",
        "truth",
        "good",
        "fear",
        "hope",
        "go",
        "come",
        "see",
        "hear",
        "speak",
        "eat",
        "drink",
        "sleep",
        "fight",
        "build",
        "remember",
    };

    /// <summary>
    /// Generates a language snapshot from shared concept context.
    /// </summary>
    public static LanguageConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new SeededRng(context.Seed ^ 0x51A6C3F);
        LanguageConceptSnapshot snapshot = new LanguageConceptSnapshot();
        snapshot.Consonants = PickWeightedDistinct(ConsonantPool, ResolveConsonantCount(context, rng), rng);
        snapshot.Vowels = PickWeightedDistinct(VowelPool, ResolveVowelCount(context, rng), rng);
        snapshot.SyllablePatterns = PickDistinct(SyllablePatterns, ResolveSyllablePatternCount(context, rng), rng);
        snapshot.WordOrder = ResolveWordOrder(context, rng);
        snapshot.MorphType = ResolveMorphType(context, rng);
        snapshot.HeadDirection = ResolveHeadDirection(snapshot.WordOrder, rng);
        snapshot.HasCase = ResolveHasCase(snapshot.MorphType, rng);
        snapshot.HasGender = ResolveHasGender(context, snapshot.MorphType, rng);
        snapshot.HasTones = ResolveHasTones(context, rng);
        snapshot.HasVowelHarmony = ResolveHasVowelHarmony(snapshot, rng);
        snapshot.HasAspect = ResolveHasAspect(snapshot.MorphType, rng);
        snapshot.AffixPreference = ResolveAffixPreference(snapshot.MorphType, rng);
        snapshot.RepairStrategy = Pick(RepairStrategies, rng);
        snapshot.Name = Capitalize(BuildWord(snapshot, rng, 2));
        snapshot.RepairParticle = BuildParticle(snapshot, rng);
        snapshot.FillerParticle = BuildParticle(snapshot, rng);

        if (snapshot.HasGender)
        {
            snapshot.GenderClasses = PickDistinct(GenderOptions, ResolveGenderClassCount(snapshot.MorphType, rng), rng);
        }

        if (snapshot.HasTones)
        {
            snapshot.Tones = PickDistinct(ToneInventory, ResolveToneCount(snapshot.MorphType, rng), rng);
        }

        BuildAffixInventories(snapshot, rng);
        snapshot.Adpositions = BuildAdpositions(snapshot, rng);
        snapshot.Lexicon = BuildLexicon(snapshot, rng);
        snapshot.ExampleSentences = BuildExampleSentences(snapshot, rng);
        return snapshot;
    }

    private static int ResolveConsonantCount(ConceptContextSnapshot context, SeededRng rng)
    {
        int baseCount = 10;
        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            baseCount += 2;
        }

        if (context.Population > 5000000)
        {
            baseCount += 2;
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            baseCount += 1;
        }

        return baseCount + rng.RandiRange(0, 5);
    }

    private static int ResolveVowelCount(ConceptContextSnapshot context, SeededRng rng)
    {
        int baseCount = 4;
        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            baseCount += 1;
        }

        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            baseCount += 1;
        }

        return baseCount + rng.RandiRange(0, 3);
    }

    private static int ResolveSyllablePatternCount(ConceptContextSnapshot context, SeededRng rng)
    {
        int baseCount = 3;
        if (context.Population > 10000000)
        {
            baseCount += 1;
        }

        if (context.RadiationLevel > 0.4)
        {
            baseCount += 1;
        }

        return baseCount + rng.RandiRange(0, 2);
    }

    private static string ResolveWordOrder(ConceptContextSnapshot context, SeededRng rng)
    {
        List<string> weightedOrders = new List<string>();
        string preferredOrder = "SOV";
        if (context.Regime.HasValue && GovernmentType.IsParticipatory(context.Regime.Value))
        {
            preferredOrder = "SVO";
        }

        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            preferredOrder = "SVO";
        }

        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            preferredOrder = "VSO";
        }

        AddWeightedOrders(weightedOrders, preferredOrder, 4);
        foreach (string order in WordOrders)
        {
            AddWeightedOrders(weightedOrders, order, 1);
        }

        return Pick(weightedOrders, rng);
    }

    private static string ResolveMorphType(ConceptContextSnapshot context, SeededRng rng)
    {
        List<string> weightedMorphTypes = new List<string>();
        string preferred = "Agglutinative";
        if (context.Population > 10000000)
        {
            preferred = "Fusional";
        }

        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            preferred = "Isolating";
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            preferred = "Polysynthetic";
        }

        AddWeightedOrders(weightedMorphTypes, preferred, 4);
        foreach (string morphType in MorphTypes)
        {
            AddWeightedOrders(weightedMorphTypes, morphType, 1);
        }

        return Pick(weightedMorphTypes, rng);
    }

    private static string ResolveHeadDirection(string wordOrder, SeededRng rng)
    {
        if (wordOrder == "SOV" || wordOrder == "OSV")
        {
            return "Head-final";
        }

        if (wordOrder == "SVO" || wordOrder == "VSO")
        {
            return "Head-initial";
        }

        if (rng.Randf() < 0.5f)
        {
            return "Head-initial";
        }

        return "Head-final";
    }

    private static bool ResolveHasCase(string morphType, SeededRng rng)
    {
        float threshold = 0.55f;
        if (morphType == "Isolating")
        {
            threshold = 0.18f;
        }
        else if (morphType == "Polysynthetic")
        {
            threshold = 0.78f;
        }

        return rng.Randf() < threshold;
    }

    private static bool ResolveHasGender(ConceptContextSnapshot context, string morphType, SeededRng rng)
    {
        float threshold = 0.35f;
        if (morphType == "Fusional")
        {
            threshold = 0.55f;
        }
        else if (morphType == "Polysynthetic")
        {
            threshold = 0.48f;
        }

        if (context.Population > 20000000)
        {
            threshold += 0.05f;
        }

        return rng.Randf() < threshold;
    }

    private static bool ResolveHasTones(ConceptContextSnapshot context, SeededRng rng)
    {
        float threshold = 0.18f;
        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            threshold += 0.08f;
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            threshold += 0.04f;
        }

        return rng.Randf() < threshold;
    }

    private static bool ResolveHasVowelHarmony(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        float threshold = 0.22f;
        if (snapshot.MorphType == "Agglutinative")
        {
            threshold = 0.46f;
        }

        return rng.Randf() < threshold;
    }

    private static bool ResolveHasAspect(string morphType, SeededRng rng)
    {
        float threshold = 0.42f;
        if (morphType == "Agglutinative" || morphType == "Polysynthetic")
        {
            threshold = 0.72f;
        }

        return rng.Randf() < threshold;
    }

    private static string ResolveAffixPreference(string morphType, SeededRng rng)
    {
        if (morphType == "Isolating")
        {
            return "light suffixing";
        }

        if (morphType == "Agglutinative")
        {
            if (rng.Randf() < 0.65f)
            {
                return "suffixing";
            }

            return "prefixing";
        }

        if (morphType == "Polysynthetic")
        {
            return "mixed";
        }

        if (rng.Randf() < 0.5f)
        {
            return "suffixing";
        }

        return "mixed";
    }

    private static int ResolveGenderClassCount(string morphType, SeededRng rng)
    {
        int count = 2;
        if (morphType == "Polysynthetic")
        {
            count = 4;
        }
        else if (morphType == "Fusional")
        {
            count = 3;
        }

        return count + rng.RandiRange(0, 1);
    }

    private static int ResolveToneCount(string morphType, SeededRng rng)
    {
        int count = 2;
        if (morphType == "Isolating")
        {
            count = 3;
        }

        return count + rng.RandiRange(0, 1);
    }

    private static void BuildAffixInventories(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        Dictionary<string, AffixForm> tenseAffixes = new Dictionary<string, AffixForm>();
        tenseAffixes["past"] = CreateAffix(snapshot, rng);
        tenseAffixes["future"] = CreateAffix(snapshot, rng);
        tenseAffixes["present"] = new AffixForm("unmarked", "stem");
        if (snapshot.HasAspect)
        {
            tenseAffixes["perfective"] = CreateAffix(snapshot, rng);
            tenseAffixes["imperfective"] = CreateAffix(snapshot, rng);
        }

        Dictionary<string, AffixForm> nounAffixes = new Dictionary<string, AffixForm>();
        nounAffixes["plural"] = CreateAffix(snapshot, rng);
        nounAffixes["diminutive"] = CreateAffix(snapshot, rng);
        nounAffixes["augmentative"] = CreateAffix(snapshot, rng);

        Dictionary<string, AffixForm> verbAffixes = new Dictionary<string, AffixForm>();
        verbAffixes["negation"] = CreateAffix(snapshot, rng);
        verbAffixes["causative"] = CreateAffix(snapshot, rng);
        verbAffixes["passive"] = CreateAffix(snapshot, rng);
        verbAffixes["question"] = CreateAffix(snapshot, rng);

        Dictionary<string, AffixForm> caseAffixes = new Dictionary<string, AffixForm>();
        if (snapshot.HasCase)
        {
            caseAffixes["nominative"] = new AffixForm("unmarked", "stem");
            caseAffixes["accusative"] = CreateAffix(snapshot, rng);
            caseAffixes["genitive"] = CreateAffix(snapshot, rng);
            caseAffixes["dative"] = CreateAffix(snapshot, rng);
            caseAffixes["locative"] = CreateAffix(snapshot, rng);
        }

        Dictionary<string, AffixForm> genderAffixes = new Dictionary<string, AffixForm>();
        if (snapshot.HasGender)
        {
            foreach (string genderClass in snapshot.GenderClasses)
            {
                genderAffixes[genderClass] = CreateAffix(snapshot, rng);
            }
        }

        snapshot.TenseAffixes = FormatAffixDictionary(tenseAffixes);
        snapshot.NounAffixes = FormatAffixDictionary(nounAffixes);
        snapshot.VerbAffixes = FormatAffixDictionary(verbAffixes);
        snapshot.CaseAffixes = FormatAffixDictionary(caseAffixes);
        snapshot.GenderAffixes = FormatAffixDictionary(genderAffixes);
    }

    private static Dictionary<string, string> BuildAdpositions(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        Dictionary<string, string> adpositions = new Dictionary<string, string>();
        foreach (string role in AdpositionRoles)
        {
            adpositions[role] = BuildWord(snapshot, rng, 1);
        }

        return adpositions;
    }

    private static Dictionary<string, string> BuildLexicon(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        Dictionary<string, string> lexicon = new Dictionary<string, string>();
        foreach (string gloss in Glosses)
        {
            int syllables = 1;
            if (rng.Randf() >= 0.3f)
            {
                syllables = 2;
            }

            if (rng.Randf() >= 0.8f)
            {
                syllables = 3;
            }

            lexicon[gloss] = BuildWord(snapshot, rng, syllables);
        }

        return lexicon;
    }

    private static List<string> BuildExampleSentences(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        List<string> examples = new List<string>();
        examples.Add("Greeting: " + BuildSentence(snapshot, "person", "friend", "speak", "present", false, false, null, null));
        examples.Add("Trade: " + BuildSentence(snapshot, "merchant", "gift", "trade", "present", false, false, "with", "friend"));
        examples.Add("Ritual: " + BuildSentence(snapshot, "leader", "spirit", "remember", "past", false, false, "before", "ritual"));
        examples.Add("Warning: " + BuildSentence(snapshot, "warrior", "enemy", "see", "future", true, false, "against", "enemy"));
        if (rng.Randf() < 0.5f)
        {
            examples.Add("Question: " + BuildSentence(snapshot, "healer", "water", "come", "present", false, true, "from", "home"));
        }

        return examples;
    }

    private static string BuildSentence(
        LanguageConceptSnapshot snapshot,
        string subjectGloss,
        string objectGloss,
        string verbGloss,
        string tense,
        bool negated,
        bool question,
        string? adpositionRole,
        string? adpositionNoun)
    {
        string subject = InflectNoun(snapshot, ResolveLexeme(snapshot, subjectGloss), "nominative", false);
        string objectWord = InflectNoun(snapshot, ResolveLexeme(snapshot, objectGloss), "accusative", false);
        string verb = InflectVerb(snapshot, ResolveLexeme(snapshot, verbGloss), tense, negated, question);
        List<string> ordered = new List<string>();

        if (snapshot.WordOrder == "SVO")
        {
            ordered.Add(subject);
            ordered.Add(verb);
            ordered.Add(objectWord);
        }
        else if (snapshot.WordOrder == "VSO")
        {
            ordered.Add(verb);
            ordered.Add(subject);
            ordered.Add(objectWord);
        }
        else if (snapshot.WordOrder == "VOS")
        {
            ordered.Add(verb);
            ordered.Add(objectWord);
            ordered.Add(subject);
        }
        else if (snapshot.WordOrder == "OVS")
        {
            ordered.Add(objectWord);
            ordered.Add(verb);
            ordered.Add(subject);
        }
        else if (snapshot.WordOrder == "OSV")
        {
            ordered.Add(objectWord);
            ordered.Add(subject);
            ordered.Add(verb);
        }
        else
        {
            ordered.Add(subject);
            ordered.Add(objectWord);
            ordered.Add(verb);
        }

        if (!string.IsNullOrEmpty(adpositionRole) && !string.IsNullOrEmpty(adpositionNoun))
        {
            string adposition = ResolveAdposition(snapshot, adpositionRole);
            string noun = InflectNoun(snapshot, ResolveLexeme(snapshot, adpositionNoun), "locative", false);
            if (snapshot.HeadDirection == "Head-initial")
            {
                ordered.Add(adposition);
                ordered.Add(noun);
            }
            else
            {
                ordered.Add(noun);
                ordered.Add(adposition);
            }
        }

        return string.Join(" ", ordered);
    }

    private static string InflectNoun(LanguageConceptSnapshot snapshot, string noun, string caseName, bool plural)
    {
        string inflected = noun;
        if (plural && snapshot.NounAffixes.ContainsKey("plural"))
        {
            inflected = ApplyAffix(inflected, snapshot.NounAffixes["plural"]);
        }

        if (snapshot.HasCase && snapshot.CaseAffixes.ContainsKey(caseName))
        {
            inflected = ApplyAffix(inflected, snapshot.CaseAffixes[caseName]);
        }

        return inflected;
    }

    private static string InflectVerb(
        LanguageConceptSnapshot snapshot,
        string verb,
        string tense,
        bool negated,
        bool question)
    {
        string inflected = verb;
        if (snapshot.TenseAffixes.ContainsKey(tense))
        {
            inflected = ApplyAffix(inflected, snapshot.TenseAffixes[tense]);
        }

        if (negated && snapshot.VerbAffixes.ContainsKey("negation"))
        {
            inflected = ApplyAffix(inflected, snapshot.VerbAffixes["negation"]);
        }

        if (question && snapshot.VerbAffixes.ContainsKey("question"))
        {
            inflected = ApplyAffix(inflected, snapshot.VerbAffixes["question"]);
        }

        return inflected;
    }

    private static string ResolveLexeme(LanguageConceptSnapshot snapshot, string gloss)
    {
        if (snapshot.Lexicon.TryGetValue(gloss, out string? lexeme))
        {
            return lexeme;
        }

        return gloss;
    }

    private static string ResolveAdposition(LanguageConceptSnapshot snapshot, string role)
    {
        if (snapshot.Adpositions.TryGetValue(role, out string? form))
        {
            return form;
        }

        return role;
    }

    private static string ApplyAffix(string baseWord, string formattedAffix)
    {
        if (formattedAffix == "unmarked")
        {
            return baseWord;
        }

        string placement = "suffix";
        string form = formattedAffix;
        int markerIndex = formattedAffix.LastIndexOf(" (", StringComparison.Ordinal);
        if (markerIndex > 0 && formattedAffix.EndsWith(")", StringComparison.Ordinal))
        {
            form = formattedAffix.Substring(0, markerIndex);
            placement = formattedAffix.Substring(markerIndex + 2, formattedAffix.Length - markerIndex - 3);
        }

        if (placement == "prefix")
        {
            return form + baseWord;
        }

        if (placement == "stem")
        {
            return baseWord;
        }

        return baseWord + form;
    }

    private static AffixForm CreateAffix(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        string placement = "suffix";
        if (snapshot.AffixPreference == "prefixing")
        {
            placement = "prefix";
        }
        else if (snapshot.AffixPreference == "mixed")
        {
            if (rng.Randf() < 0.5f)
            {
                placement = "prefix";
            }
        }
        else if (snapshot.AffixPreference == "light suffixing" && rng.Randf() < 0.2f)
        {
            placement = "prefix";
        }

        return new AffixForm(BuildParticle(snapshot, rng), placement);
    }

    private static Dictionary<string, string> FormatAffixDictionary(Dictionary<string, AffixForm> affixes)
    {
        Dictionary<string, string> formatted = new Dictionary<string, string>();
        foreach (KeyValuePair<string, AffixForm> entry in affixes)
        {
            if (entry.Value.Placement == "stem")
            {
                formatted[entry.Key] = "unmarked";
            }
            else
            {
                formatted[entry.Key] = entry.Value.Form + " (" + entry.Value.Placement + ")";
            }
        }

        return formatted;
    }

    private static string BuildParticle(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        return BuildWord(snapshot, rng, 1);
    }

    private static string BuildWord(LanguageConceptSnapshot snapshot, SeededRng rng, int syllableCount)
    {
        string word = string.Empty;
        for (int index = 0; index < syllableCount; index += 1)
        {
            string pattern = Pick(snapshot.SyllablePatterns, rng);
            foreach (char symbol in pattern)
            {
                if (symbol == 'C')
                {
                    word += Pick(snapshot.Consonants, rng);
                }
                else
                {
                    word += Pick(snapshot.Vowels, rng);
                }
            }
        }

        if (snapshot.HasVowelHarmony)
        {
            word = ApplyVowelHarmony(word, snapshot.Vowels);
        }

        return word;
    }

    private static string ApplyVowelHarmony(string word, List<string> vowels)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }

        string frontVowel = "e";
        string backVowel = "a";
        foreach (string vowel in vowels)
        {
            if (vowel.Contains("e", StringComparison.Ordinal) || vowel.Contains("i", StringComparison.Ordinal))
            {
                frontVowel = vowel;
                break;
            }
        }

        foreach (string vowel in vowels)
        {
            if (vowel.Contains("a", StringComparison.Ordinal) || vowel.Contains("o", StringComparison.Ordinal) || vowel.Contains("u", StringComparison.Ordinal))
            {
                backVowel = vowel;
                break;
            }
        }

        bool useFront = word.Contains("e", StringComparison.Ordinal) || word.Contains("i", StringComparison.Ordinal);
        if (useFront)
        {
            return word.Replace("a", frontVowel, StringComparison.Ordinal);
        }

        return word.Replace("e", backVowel, StringComparison.Ordinal);
    }

    private static List<string> PickWeightedDistinct(List<PhonemeOption> pool, int count, SeededRng rng)
    {
        List<string> values = new List<string>();
        while (values.Count < count && values.Count < pool.Count)
        {
            string candidate = PickWeighted(pool, rng);
            if (!values.Contains(candidate))
            {
                values.Add(candidate);
            }
        }

        values.Sort(StringComparer.Ordinal);
        return values;
    }

    private static string PickWeighted(List<PhonemeOption> options, SeededRng rng)
    {
        int totalWeight = 0;
        foreach (PhonemeOption option in options)
        {
            totalWeight += option.Weight;
        }

        int roll = rng.RandiRange(1, totalWeight);
        int cumulative = 0;
        foreach (PhonemeOption option in options)
        {
            cumulative += option.Weight;
            if (roll <= cumulative)
            {
                return option.Symbol;
            }
        }

        throw new InvalidOperationException("Weighted phoneme selection failed.");
    }

    private static List<string> PickDistinct(IReadOnlyList<string> pool, int count, SeededRng rng)
    {
        List<string> values = new List<string>();
        while (values.Count < count && values.Count < pool.Count)
        {
            string candidate = Pick(pool, rng);
            if (!values.Contains(candidate))
            {
                values.Add(candidate);
            }
        }

        return values;
    }

    private static T Pick<T>(IReadOnlyList<T> values, SeededRng rng)
    {
        int index = rng.RandiRange(0, values.Count - 1);
        return values[index];
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string first = value.Substring(0, 1).ToUpperInvariant();
        string remainder = value.Substring(1);
        return first + remainder;
    }

    private static void AddWeightedOrders(List<string> collection, string value, int weight)
    {
        for (int index = 0; index < weight; index += 1)
        {
            collection.Add(value);
        }
    }
}
