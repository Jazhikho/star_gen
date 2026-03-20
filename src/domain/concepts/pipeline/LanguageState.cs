using Godot.Collections;
using StarGen.Domain.Concepts.Language;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted language-layer state.
/// </summary>
public sealed class LanguageState
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public LanguageConceptSnapshot Snapshot { get; set; } = new LanguageConceptSnapshot();

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["snapshot"] = LanguageConceptSnapshotSerialization.ToDictionary(Snapshot),
        };
    }

    public static LanguageState FromDictionary(Dictionary data)
    {
        LanguageState state = new LanguageState();
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
            state.Snapshot = LanguageConceptSnapshotSerialization.FromDictionary(snapshot);
        }

        return state;
    }
}
