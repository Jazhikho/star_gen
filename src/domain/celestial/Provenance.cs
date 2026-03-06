using System;
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
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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

        Dictionary? specSnapshot = null;
        if (data.ContainsKey("spec_snapshot"))
        {
            specSnapshot = (Dictionary)data["spec_snapshot"];
        }

        return new Provenance(
            GetLong(data, "generation_seed", 0),
            GetString(data, "generator_version", ""),
            GetInt(data, "schema_version", 0),
            GetLong(data, "created_timestamp", 0),
            specSnapshot);
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
        if (data.ContainsKey(key))
        {
            return (int)data[key];
        }

        return fallback;
    }

    private static long GetLong(Dictionary data, string key, long fallback)
    {
        if (data.ContainsKey(key))
        {
            return (long)data[key];
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }
}
