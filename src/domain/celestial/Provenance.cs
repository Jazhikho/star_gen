using Godot;
using Godot.Collections;
using StarGen.Domain.Constants;

namespace StarGen.Domain.Celestial;

/// <summary>
/// Tracks the origin and version of generated celestial data.
/// </summary>
public partial class Provenance : RefCounted
{
    /// <summary>
    /// The generation seed used to create this object.
    /// </summary>
    public long GenerationSeed;

    /// <summary>
    /// The generator version that created this object.
    /// </summary>
    public string GeneratorVersion;

    /// <summary>
    /// The schema version of the serialized format.
    /// </summary>
    public int SchemaVersion;

    /// <summary>
    /// Unix timestamp when the object was created.
    /// </summary>
    public long CreatedTimestamp;

    /// <summary>
    /// Optional snapshot of the spec used for generation.
    /// </summary>
    public Dictionary SpecSnapshot;

    /// <summary>
    /// Creates a new provenance instance.
    /// </summary>
    public Provenance(
        long generationSeed = 0,
        string generatorVersion = "",
        int schemaVersion = 0,
        long createdTimestamp = 0,
        Dictionary? specSnapshot = null)
    {
        GenerationSeed = generationSeed;
        GeneratorVersion = generatorVersion;
        SchemaVersion = schemaVersion;
        CreatedTimestamp = createdTimestamp;
        SpecSnapshot = CloneDictionary(specSnapshot);
    }

    /// <summary>
    /// Creates a provenance instance using the current version constants.
    /// </summary>
    public static Provenance CreateCurrent(long generationSeed, Dictionary? specSnapshot = null)
    {
        return new Provenance(
            generationSeed,
            Versions.GeneratorVersion,
            Versions.SchemaVersion,
            (long)Time.GetUnixTimeFromSystem(),
            specSnapshot);
    }

    /// <summary>
    /// Converts this provenance object to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["generation_seed"] = GenerationSeed,
            ["generator_version"] = GeneratorVersion,
            ["schema_version"] = SchemaVersion,
            ["created_timestamp"] = CreatedTimestamp,
            ["spec_snapshot"] = CloneDictionary(SpecSnapshot),
        };
    }

    /// <summary>
    /// Creates a provenance object from a dictionary.
    /// </summary>
    public static Provenance? FromDictionary(Dictionary data)
    {
        if (data.Count == 0)
        {
            return null;
        }

        return new Provenance(
            GetLong(data, "generation_seed", 0),
            GetString(data, "generator_version", ""),
            GetInt(data, "schema_version", 0),
            GetLong(data, "created_timestamp", 0),
            data.ContainsKey("spec_snapshot") ? (Dictionary)data["spec_snapshot"] : null);
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

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        return data.ContainsKey(key) ? (int)data[key] : fallback;
    }

    private static long GetLong(Dictionary data, string key, long fallback)
    {
        return data.ContainsKey(key) ? (long)data[key] : fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        return data.ContainsKey(key) ? (string)data[key] : fallback;
    }
}
