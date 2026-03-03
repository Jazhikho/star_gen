using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Base class for all generation specifications.
/// </summary>
public partial class BaseSpec : Godot.RefCounted
{
    /// <summary>
    /// The seed used for deterministic generation.
    /// </summary>
    public int GenerationSeed;

    /// <summary>
    /// Optional name hint for the generated body.
    /// </summary>
    public string NameHint;

    /// <summary>
    /// Field overrides that lock specific values during generation.
    /// </summary>
    public Dictionary Overrides;

    /// <summary>
    /// Creates a new base specification.
    /// </summary>
    public BaseSpec(
        int generationSeed = 0,
        string nameHint = "",
        Dictionary? overrides = null)
    {
        GenerationSeed = generationSeed;
        NameHint = nameHint;
        Overrides = CloneDictionary(overrides);
    }

    /// <summary>
    /// Returns whether a specific field has an override.
    /// </summary>
    public bool HasOverride(string fieldPath) => Overrides.ContainsKey(fieldPath);

    /// <summary>
    /// Gets the override value for a field, or a default if not overridden.
    /// </summary>
    public Variant GetOverride(string fieldPath, Variant defaultValue)
    {
        return Overrides.ContainsKey(fieldPath) ? Overrides[fieldPath] : defaultValue;
    }

    /// <summary>
    /// Gets the override value as a floating-point value.
    /// </summary>
    public double GetOverrideFloat(string fieldPath, double defaultValue)
    {
        return Overrides.ContainsKey(fieldPath) ? (double)Overrides[fieldPath] : defaultValue;
    }

    /// <summary>
    /// Gets the override value as an integer.
    /// </summary>
    public int GetOverrideInt(string fieldPath, int defaultValue)
    {
        return Overrides.ContainsKey(fieldPath) ? (int)Overrides[fieldPath] : defaultValue;
    }

    /// <summary>
    /// Sets an override value.
    /// </summary>
    public void SetOverride(string fieldPath, Variant value)
    {
        Overrides[fieldPath] = value;
    }

    /// <summary>
    /// Removes an override.
    /// </summary>
    public void RemoveOverride(string fieldPath)
    {
        Overrides.Remove(fieldPath);
    }

    /// <summary>
    /// Clears all overrides.
    /// </summary>
    public void ClearOverrides()
    {
        Overrides.Clear();
    }

    /// <summary>
    /// Converts the base fields to a dictionary.
    /// </summary>
    public Dictionary BaseToDictionary()
    {
        return new Dictionary
        {
            ["generation_seed"] = GenerationSeed,
            ["name_hint"] = NameHint,
            ["overrides"] = CloneDictionary(Overrides),
        };
    }

    /// <summary>
    /// Populates the base fields from a dictionary.
    /// </summary>
    public void ApplyBaseFromDictionary(Dictionary data)
    {
        GenerationSeed = data.ContainsKey("generation_seed") ? (int)data["generation_seed"] : 0;
        NameHint = data.ContainsKey("name_hint") ? (string)data["name_hint"] : string.Empty;
        Overrides = data.ContainsKey("overrides") ? CloneDictionary((Dictionary)data["overrides"]) : new Dictionary();
    }

    private static Dictionary CloneDictionary(Dictionary? source)
    {
        Dictionary clone = new();
        if (source == null)
        {
            return clone;
        }

        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
    }
}
