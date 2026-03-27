using Godot.Collections;
using StarGen.Domain.Concepts.Civilization;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted society-layer state.
/// </summary>
public sealed class SocietyState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public CivilizationConceptSnapshot Snapshot { get; set; } = new CivilizationConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = CivilizationConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static SocietyState FromDictionary(Dictionary data)
    {
        SocietyState state = new SocietyState();
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
            state.Snapshot = CivilizationConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
