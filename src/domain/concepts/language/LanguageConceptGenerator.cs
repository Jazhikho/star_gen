using System.Collections.Generic;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Language;

/// <summary>
/// Deterministic conlang generator used by the concept atlas.
/// </summary>
public static class LanguageConceptGenerator
{
    private static readonly string[] ConsonantPool =
    {
        "m", "n", "p", "t", "k", "b", "d", "g", "s", "sh", "z", "h", "l", "r", "w", "y", "v", "f", "ts", "ch",
    };

    private static readonly string[] VowelPool =
    {
        "a", "e", "i", "o", "u", "ae", "ai", "ou", "ia",
    };

    private static readonly string[] SyllablePool =
    {
        "V", "CV", "VC", "CVC", "CCV", "CVCC",
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

    private static readonly string[] RepairStrategies =
    {
        "Restart the phrase from the beginning after a brief pause.",
        "Repeat the last word with rising intonation before correcting it.",
        "Insert a dedicated correction particle after the error.",
        "Use a filler syllable before replacing the mistaken word.",
    };

    private static readonly string[] LexiconGlosses =
    {
        "sun",
        "water",
        "person",
        "friend",
        "law",
        "trade",
        "fire",
        "star",
    };

    /// <summary>
    /// Generates a language snapshot from shared concept context.
    /// </summary>
    public static LanguageConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new(context.Seed ^ 0x51A6C3F);
        LanguageConceptSnapshot snapshot = new LanguageConceptSnapshot();
        snapshot.Consonants = PickDistinct(ConsonantPool, 10 + rng.RandiRange(0, 4), rng);
        snapshot.Vowels = PickDistinct(VowelPool, 4 + rng.RandiRange(0, 2), rng);
        snapshot.SyllablePatterns = PickDistinct(SyllablePool, 3 + rng.RandiRange(0, 1), rng);
        snapshot.WordOrder = Pick(WordOrders, rng);
        snapshot.MorphType = Pick(MorphTypes, rng);
        snapshot.HeadDirection = rng.Randf() < 0.5f ? "Head-initial" : "Head-final";
        snapshot.HasCase = rng.Randf() < 0.6f;
        snapshot.HasGender = rng.Randf() < 0.45f;
        snapshot.HasTones = rng.Randf() < 0.2f;
        snapshot.HasVowelHarmony = rng.Randf() < 0.3f;
        snapshot.RepairStrategy = Pick(RepairStrategies, rng);
        snapshot.Name = Capitalize(BuildWord(snapshot, rng, 2));
        snapshot.Lexicon = BuildLexicon(snapshot, rng);
        snapshot.ExampleSentences = BuildExamples(snapshot);
        return snapshot;
    }

    private static Dictionary<string, string> BuildLexicon(LanguageConceptSnapshot snapshot, SeededRng rng)
    {
        Dictionary<string, string> lexicon = new Dictionary<string, string>();
        foreach (string gloss in LexiconGlosses)
        {
            lexicon[gloss] = BuildWord(snapshot, rng, 1 + rng.RandiRange(0, 2));
        }

        return lexicon;
    }

    private static List<string> BuildExamples(LanguageConceptSnapshot snapshot)
    {
        List<string> examples = new List<string>();
        examples.Add("Greeting: " + BuildSentence(snapshot, "person", "friend", "trade"));
        examples.Add("Observation: " + BuildSentence(snapshot, "person", "water", "see"));
        examples.Add("Ceremony: " + BuildSentence(snapshot, "person", "sun", "praise"));
        return examples;
    }

    private static string BuildSentence(
        LanguageConceptSnapshot snapshot,
        string subjectGloss,
        string objectGloss,
        string verbGloss)
    {
        string subject = ResolveLexeme(snapshot, subjectGloss);
        string objectWord = ResolveLexeme(snapshot, objectGloss);
        string verb = ResolveLexeme(snapshot, verbGloss);
        if (snapshot.WordOrder == "SVO")
        {
            return subject + " " + verb + " " + objectWord;
        }

        if (snapshot.WordOrder == "VSO")
        {
            return verb + " " + subject + " " + objectWord;
        }

        if (snapshot.WordOrder == "VOS")
        {
            return verb + " " + objectWord + " " + subject;
        }

        if (snapshot.WordOrder == "OVS")
        {
            return objectWord + " " + verb + " " + subject;
        }

        if (snapshot.WordOrder == "OSV")
        {
            return objectWord + " " + subject + " " + verb;
        }

        return subject + " " + objectWord + " " + verb;
    }

    private static string ResolveLexeme(LanguageConceptSnapshot snapshot, string gloss)
    {
        if (snapshot.Lexicon.TryGetValue(gloss, out string? lexeme))
        {
            return lexeme;
        }

        return gloss;
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

        return word;
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

    private static string Pick(IReadOnlyList<string> values, SeededRng rng)
    {
        return values[rng.RandiRange(0, values.Count - 1)];
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
    }
}
