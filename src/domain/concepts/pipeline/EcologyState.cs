using Godot.Collections;
using StarGen.Domain.Concepts.Ecology;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted ecology-layer state.
/// </summary>
public sealed class EcologyState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public EcologyConceptSnapshot Snapshot { get; set; } = new EcologyConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = EcologyConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static EcologyState FromDictionary(Dictionary data)
    {
        EcologyState state = new EcologyState();
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
            state.Snapshot = EcologyConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
