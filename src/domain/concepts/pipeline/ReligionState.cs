using Godot.Collections;
using StarGen.Domain.Concepts.Religion;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted religion-layer state.
/// </summary>
public sealed class ReligionState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public ReligionConceptSnapshot Snapshot { get; set; } = new ReligionConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = ReligionConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static ReligionState FromDictionary(Dictionary data)
    {
        ReligionState state = new ReligionState();
        state.Status = (ConceptRunStatus)ConceptSerializationUtils.ReadOptionalInt(data, "status", (int)ConceptRunStatus.NotApplicable);
        state.StatusReason = ConceptSerializationUtils.ReadOptionalString(data, "status_reason");
        Dictionary? provenance = ConceptSerializationUtils.ReadDictionary(data, "provenance");
        if (provenance != null)
        {
            state.Provenance = ConceptRunResultSerialization.FromProvenanceDictionary(provenance);
        }

        Dictionary? snapshot = ConceptSerializationUtils.ReadDictionary(data, "snapshot");
        if (snapshot != null)
        {
            state.Snapshot = ReligionConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
