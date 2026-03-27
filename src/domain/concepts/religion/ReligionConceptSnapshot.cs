using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Religion;

/// <summary>
/// Folded-in religion summary used by the concept atlas.
/// </summary>
public sealed class ReligionConceptSnapshot
{
    /// <summary>
    /// Primary deity framing.
    /// </summary>
    public string Deity { get; set; } = string.Empty;

    /// <summary>
    /// Cosmology summary.
    /// </summary>
    public string Cosmology { get; set; } = string.Empty;

    /// <summary>
    /// Authority summary.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Specialist summary.
    /// </summary>
    public string Specialist { get; set; } = string.Empty;

    /// <summary>
    /// Ritual sample.
    /// </summary>
    public List<string> Rituals { get; set; } = new List<string>();

    /// <summary>
    /// Ethical emphases.
    /// </summary>
    public List<string> Ethics { get; set; } = new List<string>();

    /// <summary>
    /// Religious landscape notes.
    /// </summary>
    public List<string> Landscape { get; set; } = new List<string>();
}

/// <summary>
/// Serialization helpers for religion snapshots.
/// </summary>
public static class ReligionConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(ReligionConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["deity"] = snapshot.Deity,
            ["cosmology"] = snapshot.Cosmology,
            ["authority"] = snapshot.Authority,
            ["specialist"] = snapshot.Specialist,
            ["rituals"] = ConceptSerializationUtils.ToArray(snapshot.Rituals),
            ["ethics"] = ConceptSerializationUtils.ToArray(snapshot.Ethics),
            ["landscape"] = ConceptSerializationUtils.ToArray(snapshot.Landscape),
        };
    }

    public static ReligionConceptSnapshot FromDictionary(Dictionary data)
    {
        ReligionConceptSnapshot snapshot = new ReligionConceptSnapshot();
        snapshot.Deity = ConceptSerializationUtils.ReadString(data, "deity");
        snapshot.Cosmology = ConceptSerializationUtils.ReadString(data, "cosmology");
        snapshot.Authority = ConceptSerializationUtils.ReadString(data, "authority");
        snapshot.Specialist = ConceptSerializationUtils.ReadString(data, "specialist");
        snapshot.Rituals = ConceptSerializationUtils.ReadStringList(data, "rituals");
        snapshot.Ethics = ConceptSerializationUtils.ReadStringList(data, "ethics");
        snapshot.Landscape = ConceptSerializationUtils.ReadStringList(data, "landscape");
        return snapshot;
    }
}
