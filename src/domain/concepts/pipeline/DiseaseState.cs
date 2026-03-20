using Godot.Collections;
using StarGen.Domain.Concepts.Disease;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted disease-layer state.
/// </summary>
public sealed class DiseaseState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public DiseaseConceptSnapshot Snapshot { get; set; } = new DiseaseConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = DiseaseConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static DiseaseState FromDictionary(Dictionary data)
    {
        DiseaseState state = new DiseaseState();
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
            state.Snapshot = DiseaseConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
