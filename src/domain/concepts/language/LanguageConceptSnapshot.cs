using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Language;

/// <summary>
/// Folded-in language summary used by the concept atlas.
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
    /// Whether the language uses cases.
    /// </summary>
    public bool HasCase { get; set; }

    /// <summary>
    /// Whether the language uses grammatical gender.
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
    /// Repair strategy summary.
    /// </summary>
    public string RepairStrategy { get; set; } = string.Empty;

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
    /// Gloss-to-form lexicon sample.
    /// </summary>
    public Dictionary<string, string> Lexicon { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Sample generated sentences.
    /// </summary>
    public List<string> ExampleSentences { get; set; } = new List<string>();
}
