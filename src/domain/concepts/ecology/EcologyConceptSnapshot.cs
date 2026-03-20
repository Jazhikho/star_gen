using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Ecology;

/// <summary>
/// Folded-in ecology summary used by the concept atlas.
/// </summary>
public sealed class EcologyConceptSnapshot
{
    /// <summary>
    /// Total slot count.
    /// </summary>
    public int SlotCount { get; set; }

    /// <summary>
    /// Total connection count.
    /// </summary>
    public int ConnectionCount { get; set; }

    /// <summary>
    /// Primary productivity.
    /// </summary>
    public float Productivity { get; set; }

    /// <summary>
    /// Total biomass.
    /// </summary>
    public float Biomass { get; set; }

    /// <summary>
    /// Complexity score.
    /// </summary>
    public float Complexity { get; set; }

    /// <summary>
    /// Stability score.
    /// </summary>
    public float Stability { get; set; }

    /// <summary>
    /// Longest chain.
    /// </summary>
    public int MaxChainLength { get; set; }

    /// <summary>
    /// Counts by trophic level label.
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> LevelCounts { get; set; } = new System.Collections.Generic.Dictionary<string, int>();

    /// <summary>
    /// Representative niche descriptions.
    /// </summary>
    public List<string> HighlightedNiches { get; set; } = new List<string>();
}

/// <summary>
/// Serialization helpers for ecology snapshots.
/// </summary>
public static class EcologyConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(EcologyConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["slot_count"] = snapshot.SlotCount,
            ["connection_count"] = snapshot.ConnectionCount,
            ["productivity"] = snapshot.Productivity,
            ["biomass"] = snapshot.Biomass,
            ["complexity"] = snapshot.Complexity,
            ["stability"] = snapshot.Stability,
            ["max_chain_length"] = snapshot.MaxChainLength,
            ["level_counts"] = ConceptSerializationUtils.ToDictionary(snapshot.LevelCounts),
            ["highlighted_niches"] = ConceptSerializationUtils.ToArray(snapshot.HighlightedNiches),
        };
    }

    public static EcologyConceptSnapshot FromDictionary(Dictionary data)
    {
        EcologyConceptSnapshot snapshot = new EcologyConceptSnapshot();
        snapshot.SlotCount = ConceptSerializationUtils.ReadInt(data, "slot_count");
        snapshot.ConnectionCount = ConceptSerializationUtils.ReadInt(data, "connection_count");
        snapshot.Productivity = (float)ConceptSerializationUtils.ReadDouble(data, "productivity");
        snapshot.Biomass = (float)ConceptSerializationUtils.ReadDouble(data, "biomass");
        snapshot.Complexity = (float)ConceptSerializationUtils.ReadDouble(data, "complexity");
        snapshot.Stability = (float)ConceptSerializationUtils.ReadDouble(data, "stability");
        snapshot.MaxChainLength = ConceptSerializationUtils.ReadInt(data, "max_chain_length");
        snapshot.LevelCounts = new System.Collections.Generic.Dictionary<string, int>(ConceptSerializationUtils.ReadIntDictionary(data, "level_counts"));
        snapshot.HighlightedNiches = ConceptSerializationUtils.ReadStringList(data, "highlighted_niches");
        return snapshot;
    }
}
