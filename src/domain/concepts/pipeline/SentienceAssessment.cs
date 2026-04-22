using Godot.Collections;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Persisted sentience-assessment state between species generation and society generation.
/// </summary>
public sealed class SentienceAssessment
{
    public ConceptRunStatus Status { get; set; } = ConceptRunStatus.NotApplicable;

    public string StatusReason { get; set; } = string.Empty;

    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();

    public bool HasSentientLife { get; set; }

    public bool HasTechnologicalCivilization { get; set; }

    public string CandidateSpeciesName { get; set; } = string.Empty;

    public double CognitionScore { get; set; }

    public double CommunicationScore { get; set; }

    public double ManipulationScore { get; set; }

    public double SocialComplexity { get; set; }

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["status"] = (int)Status,
            ["status_reason"] = StatusReason,
            ["provenance"] = ConceptRunResultSerialization.ToProvenanceDictionary(Provenance),
            ["has_sentient_life"] = HasSentientLife,
            ["has_technological_civilization"] = HasTechnologicalCivilization,
            ["candidate_species_name"] = CandidateSpeciesName,
            ["cognition_score"] = CognitionScore,
            ["communication_score"] = CommunicationScore,
            ["manipulation_score"] = ManipulationScore,
            ["social_complexity"] = SocialComplexity,
        };
    }

    public static SentienceAssessment FromDictionary(Dictionary data)
    {
        SentienceAssessment state = new SentienceAssessment();
        state.Status = (ConceptRunStatus)ConceptSerializationUtils.ReadOptionalInt(data, "status", (int)ConceptRunStatus.NotApplicable);
        state.StatusReason = ConceptSerializationUtils.ReadOptionalString(data, "status_reason");
        Dictionary? provenance = ConceptSerializationUtils.ReadDictionary(data, "provenance");
        if (provenance != null)
        {
            state.Provenance = ConceptRunResultSerialization.FromProvenanceDictionary(provenance);
        }

        state.HasSentientLife = ConceptSerializationUtils.ReadBool(data, "has_sentient_life");
        state.HasTechnologicalCivilization = ConceptSerializationUtils.ReadBool(data, "has_technological_civilization");
        state.CandidateSpeciesName = ConceptSerializationUtils.ReadString(data, "candidate_species_name");
        state.CognitionScore = ConceptSerializationUtils.ReadDouble(data, "cognition_score");
        state.CommunicationScore = ConceptSerializationUtils.ReadDouble(data, "communication_score");
        state.ManipulationScore = ConceptSerializationUtils.ReadDouble(data, "manipulation_score");
        state.SocialComplexity = ConceptSerializationUtils.ReadDouble(data, "social_complexity");
        return state;
    }
}
