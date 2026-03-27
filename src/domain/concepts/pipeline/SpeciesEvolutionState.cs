using Godot.Collections;
using StarGen.Domain.Concepts.Evolution;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted species and evolutionary-layer state.
/// </summary>
public sealed class SpeciesEvolutionState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public bool HasSentientCandidate { get; set; }

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public EvolutionConceptSnapshot Snapshot { get; set; } = new EvolutionConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["has_sentient_candidate"] = HasSentientCandidate,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = EvolutionConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static SpeciesEvolutionState FromDictionary(Dictionary data)
    {
        SpeciesEvolutionState state = new SpeciesEvolutionState();
        state.Status = (ConceptRunStatus)ConceptSerializationUtils.ReadOptionalInt(data, "status", (int)ConceptRunStatus.NotApplicable);
        state.StatusReason = ConceptSerializationUtils.ReadOptionalString(data, "status_reason");
        state.HasSentientCandidate = ConceptSerializationUtils.ReadBool(data, "has_sentient_candidate");
        Dictionary? provenance = ConceptSerializationUtils.ReadDictionary(data, "provenance");
        if (provenance != null)
        {
            state.Provenance = ConceptRunResultSerialization.FromProvenanceDictionary(provenance);
        }

        Dictionary? snapshot = ConceptSerializationUtils.ReadDictionary(data, "snapshot");
        if (snapshot != null)
        {
            state.Snapshot = EvolutionConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
