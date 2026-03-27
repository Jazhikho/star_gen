using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Language;

/// <summary>
/// Folded-in language state used by the concept atlas and runtime pipeline.
/// </summary>
public sealed class LanguageConceptSnapshot
{
    /// <summary>
    /// Display name of the language.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dominant word order.
    /// </summary>
    public string WordOrder { get; set; } = string.Empty;

    /// <summary>
    /// Morphological type.
    /// </summary>
    public string MorphType { get; set; } = string.Empty;

    /// <summary>
    /// Head-direction summary.
    /// </summary>
    public string HeadDirection { get; set; } = string.Empty;

    /// <summary>
    /// Preferred affix placement.
    /// </summary>
    public string AffixPreference { get; set; } = string.Empty;

    /// <summary>
    /// Whether the language uses cases.
    /// </summary>
    public bool HasCase { get; set; }

    /// <summary>
    /// Whether the language uses grammatical gender or noun classes.
    /// </summary>
    public bool HasGender { get; set; }

    /// <summary>
    /// Whether the language uses tones.
    /// </summary>
    public bool HasTones { get; set; }

    /// <summary>
    /// Whether the language uses vowel harmony.
    /// </summary>
    public bool HasVowelHarmony { get; set; }

    /// <summary>
    /// Whether the language marks aspect overtly.
    /// </summary>
    public bool HasAspect { get; set; }

    /// <summary>
    /// Repair strategy summary.
    /// </summary>
    public string RepairStrategy { get; set; } = string.Empty;

    /// <summary>
    /// Dedicated correction particle if present.
    /// </summary>
    public string RepairParticle { get; set; } = string.Empty;

    /// <summary>
    /// Dedicated filler particle if present.
    /// </summary>
    public string FillerParticle { get; set; } = string.Empty;

    /// <summary>
    /// Consonant inventory.
    /// </summary>
    public List<string> Consonants { get; set; } = new List<string>();

    /// <summary>
    /// Vowel inventory.
    /// </summary>
    public List<string> Vowels { get; set; } = new List<string>();

    /// <summary>
    /// Syllable patterns.
    /// </summary>
    public List<string> SyllablePatterns { get; set; } = new List<string>();

    /// <summary>
    /// Gender classes or noun classes when present.
    /// </summary>
    public List<string> GenderClasses { get; set; } = new List<string>();

    /// <summary>
    /// Tone inventory when tonal.
    /// </summary>
    public List<string> Tones { get; set; } = new List<string>();

    /// <summary>
    /// Tense and aspect marking inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> TenseAffixes { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Noun-affix inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> NounAffixes { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Verb-affix inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> VerbAffixes { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Case-affix inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> CaseAffixes { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Gender-affix inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> GenderAffixes { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Adposition inventory.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> Adpositions { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Gloss-to-form lexicon sample.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> Lexicon { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

    /// <summary>
    /// Sample generated sentences.
    /// </summary>
    public List<string> ExampleSentences { get; set; } = new List<string>();
}

/// <summary>
/// Serialization helpers for language snapshots.
/// </summary>
public static class LanguageConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(LanguageConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["name"] = snapshot.Name,
            ["word_order"] = snapshot.WordOrder,
            ["morph_type"] = snapshot.MorphType,
            ["head_direction"] = snapshot.HeadDirection,
            ["affix_preference"] = snapshot.AffixPreference,
            ["has_case"] = snapshot.HasCase,
            ["has_gender"] = snapshot.HasGender,
            ["has_tones"] = snapshot.HasTones,
            ["has_vowel_harmony"] = snapshot.HasVowelHarmony,
            ["has_aspect"] = snapshot.HasAspect,
            ["repair_strategy"] = snapshot.RepairStrategy,
            ["repair_particle"] = snapshot.RepairParticle,
            ["filler_particle"] = snapshot.FillerParticle,
            ["consonants"] = ConceptSerializationUtils.ToArray(snapshot.Consonants),
            ["vowels"] = ConceptSerializationUtils.ToArray(snapshot.Vowels),
            ["syllable_patterns"] = ConceptSerializationUtils.ToArray(snapshot.SyllablePatterns),
            ["gender_classes"] = ConceptSerializationUtils.ToArray(snapshot.GenderClasses),
            ["tones"] = ConceptSerializationUtils.ToArray(snapshot.Tones),
            ["tense_affixes"] = ConceptSerializationUtils.ToDictionary(snapshot.TenseAffixes),
            ["noun_affixes"] = ConceptSerializationUtils.ToDictionary(snapshot.NounAffixes),
            ["verb_affixes"] = ConceptSerializationUtils.ToDictionary(snapshot.VerbAffixes),
            ["case_affixes"] = ConceptSerializationUtils.ToDictionary(snapshot.CaseAffixes),
            ["gender_affixes"] = ConceptSerializationUtils.ToDictionary(snapshot.GenderAffixes),
            ["adpositions"] = ConceptSerializationUtils.ToDictionary(snapshot.Adpositions),
            ["lexicon"] = ConceptSerializationUtils.ToDictionary(snapshot.Lexicon),
            ["example_sentences"] = ConceptSerializationUtils.ToArray(snapshot.ExampleSentences),
        };
    }

    public static LanguageConceptSnapshot FromDictionary(Dictionary data)
    {
        LanguageConceptSnapshot snapshot = new LanguageConceptSnapshot();
        snapshot.Name = ConceptSerializationUtils.ReadString(data, "name");
        snapshot.WordOrder = ConceptSerializationUtils.ReadString(data, "word_order");
        snapshot.MorphType = ConceptSerializationUtils.ReadString(data, "morph_type");
        snapshot.HeadDirection = ConceptSerializationUtils.ReadString(data, "head_direction");
        snapshot.AffixPreference = ConceptSerializationUtils.ReadString(data, "affix_preference");
        snapshot.HasCase = ConceptSerializationUtils.ReadBool(data, "has_case");
        snapshot.HasGender = ConceptSerializationUtils.ReadBool(data, "has_gender");
        snapshot.HasTones = ConceptSerializationUtils.ReadBool(data, "has_tones");
        snapshot.HasVowelHarmony = ConceptSerializationUtils.ReadBool(data, "has_vowel_harmony");
        snapshot.HasAspect = ConceptSerializationUtils.ReadBool(data, "has_aspect");
        snapshot.RepairStrategy = ConceptSerializationUtils.ReadString(data, "repair_strategy");
        snapshot.RepairParticle = ConceptSerializationUtils.ReadString(data, "repair_particle");
        snapshot.FillerParticle = ConceptSerializationUtils.ReadString(data, "filler_particle");
        snapshot.Consonants = ConceptSerializationUtils.ReadStringList(data, "consonants");
        snapshot.Vowels = ConceptSerializationUtils.ReadStringList(data, "vowels");
        snapshot.SyllablePatterns = ConceptSerializationUtils.ReadStringList(data, "syllable_patterns");
        snapshot.GenderClasses = ConceptSerializationUtils.ReadStringList(data, "gender_classes");
        snapshot.Tones = ConceptSerializationUtils.ReadStringList(data, "tones");
        snapshot.TenseAffixes = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "tense_affixes"));
        snapshot.NounAffixes = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "noun_affixes"));
        snapshot.VerbAffixes = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "verb_affixes"));
        snapshot.CaseAffixes = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "case_affixes"));
        snapshot.GenderAffixes = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "gender_affixes"));
        snapshot.Adpositions = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "adpositions"));
        snapshot.Lexicon = new System.Collections.Generic.Dictionary<string, string>(ConceptSerializationUtils.ReadStringDictionary(data, "lexicon"));
        snapshot.ExampleSentences = ConceptSerializationUtils.ReadStringList(data, "example_sentences");
        return snapshot;
    }
}
