using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Base class for all generation specifications.
/// </summary>
public partial class BaseSpec : Godot.RefCounted
{
    /// <summary>
    /// The seed used for deterministic generation.
    /// </summary>
    public int GenerationSeed { get; set; }

    /// <summary>
    /// Optional name hint for the generated body.
    /// </summary>
    public string NameHint { get; set; }

    /// <summary>
    /// Field overrides that lock specific values during generation.
    /// </summary>
    public Dictionary Overrides { get; set; }

    /// <summary>
    /// Shared generation intent for ruleset/readout behavior.
    /// </summary>
    public GenerationUseCaseSettings UseCaseSettings { get; set; }

    /// <summary>
    /// Creates a new base specification.
    /// </summary>
    public BaseSpec(
        int generationSeed = 0,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
    {
        GenerationSeed = generationSeed;
        NameHint = nameHint;
        Overrides = CloneDictionary(overrides);
        UseCaseSettings = useCaseSettings?.Clone() ?? GenerationUseCaseSettings.CreateDefault();
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
        if (Overrides.ContainsKey(fieldPath))
        {
            return Overrides[fieldPath];
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets the override value as a floating-point value.
    /// Returns <paramref name="defaultValue"/> when the field is absent or the stored type is not numeric.
    /// </summary>
    /// <param name="fieldPath">Property path key.</param>
    /// <param name="defaultValue">Fallback when the key is missing or type is wrong.</param>
    /// <returns>Override double value, or <paramref name="defaultValue"/>.</returns>
    public double GetOverrideFloat(string fieldPath, double defaultValue)
    {
        if (!Overrides.ContainsKey(fieldPath))
        {
            return defaultValue;
        }

        Variant stored = Overrides[fieldPath];
        if (stored.VariantType == Variant.Type.Float)
        {
            return (double)stored;
        }

        if (stored.VariantType == Variant.Type.Int)
        {
            return (int)stored;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets the override value as an integer.
    /// Returns <paramref name="defaultValue"/> when the field is absent or the stored type is not numeric.
    /// </summary>
    /// <param name="fieldPath">Property path key.</param>
    /// <param name="defaultValue">Fallback when the key is missing or type is wrong.</param>
    /// <returns>Override integer value, or <paramref name="defaultValue"/>.</returns>
    public int GetOverrideInt(string fieldPath, int defaultValue)
    {
        if (!Overrides.ContainsKey(fieldPath))
        {
            return defaultValue;
        }

        Variant stored = Overrides[fieldPath];
        if (stored.VariantType == Variant.Type.Int)
        {
            return (int)stored;
        }

        if (stored.VariantType == Variant.Type.Float)
        {
            return (int)(double)stored;
        }

        return defaultValue;
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
            ["use_case_settings"] = UseCaseSettings.ToDictionary(),
        };
    }

    /// <summary>
    /// Populates the base fields from a dictionary.
    /// </summary>
    public void ApplyBaseFromDictionary(Dictionary data)
    {
        GenerationSeed = GetInt(data, "generation_seed", 0);
        NameHint = GetString(data, "name_hint", string.Empty);

        if (data.ContainsKey("overrides") && data["overrides"].VariantType == Variant.Type.Dictionary)
        {
            Overrides = CloneDictionary((Dictionary)data["overrides"]);
        }
        else
        {
            Overrides = new Dictionary();
        }

        if (data.ContainsKey("use_case_settings") && data["use_case_settings"].VariantType == Variant.Type.Dictionary)
        {
            UseCaseSettings = GenerationUseCaseSettings.FromDictionary((Dictionary)data["use_case_settings"]);
        }
        else
        {
            UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
        }
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (int)(double)value;
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.String)
        {
            return (string)data[key];
        }

        return fallback;
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
