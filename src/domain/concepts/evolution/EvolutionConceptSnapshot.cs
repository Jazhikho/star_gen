using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Evolution;

/// <summary>
/// Folded-in evolution state used by the concept atlas and runtime pipeline.
/// </summary>
public sealed class EvolutionConceptSnapshot
{
    /// <summary>
    /// Species display name.
    /// </summary>
    public string SpeciesName { get; set; } = string.Empty;

    /// <summary>
    /// Environment label used for adaptation.
    /// </summary>
    public string EnvironmentLabel { get; set; } = string.Empty;

    /// <summary>
    /// Evolutionary timespan in millions of years.
    /// </summary>
    public double EvolutionSpanMya { get; set; }

    /// <summary>
    /// Body-plan summary.
    /// </summary>
    public string BodyPlan { get; set; } = string.Empty;

    /// <summary>
    /// Locomotion summary.
    /// </summary>
    public string Locomotion { get; set; } = string.Empty;

    /// <summary>
    /// Diet summary.
    /// </summary>
    public string Diet { get; set; } = string.Empty;

    /// <summary>
    /// Senses summary.
    /// </summary>
    public string Senses { get; set; } = string.Empty;

    /// <summary>
    /// Sociality summary.
    /// </summary>
    public string Sociality { get; set; } = string.Empty;

    /// <summary>
    /// Reproduction summary.
    /// </summary>
    public string Reproduction { get; set; } = string.Empty;

    /// <summary>
    /// Communication summary.
    /// </summary>
    public string Communication { get; set; } = string.Empty;

    /// <summary>
    /// Metabolism summary.
    /// </summary>
    public string Metabolism { get; set; } = string.Empty;

    /// <summary>
    /// Integument summary.
    /// </summary>
    public string Integument { get; set; } = string.Empty;

    /// <summary>
    /// Unlocked trait list.
    /// </summary>
    public List<string> Traits { get; set; } = new List<string>();

    /// <summary>
    /// Internal node unlock list.
    /// </summary>
    public List<string> UnlockedNodes { get; set; } = new List<string>();

    /// <summary>
    /// Lineage milestones.
    /// </summary>
    public List<string> LineageMilestones { get; set; } = new List<string>();

    /// <summary>
    /// Adaptation-fit estimate in the range [0, 1].
    /// </summary>
    public double AdaptationFit { get; set; }

    /// <summary>
    /// Defensive sophistication estimate in the range [0, 1].
    /// </summary>
    public double DefenseScore { get; set; }

    /// <summary>
    /// Mobility estimate in the range [0, 1].
    /// </summary>
    public double MobilityScore { get; set; }

    /// <summary>
    /// Cognitive complexity estimate in the range [0, 1].
    /// </summary>
    public double CognitionScore { get; set; }
}

/// <summary>
/// Serialization helpers for evolution snapshots.
/// </summary>
public static class EvolutionConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(EvolutionConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["species_name"] = snapshot.SpeciesName,
            ["environment_label"] = snapshot.EnvironmentLabel,
            ["evolution_span_mya"] = snapshot.EvolutionSpanMya,
            ["body_plan"] = snapshot.BodyPlan,
            ["locomotion"] = snapshot.Locomotion,
            ["diet"] = snapshot.Diet,
            ["senses"] = snapshot.Senses,
            ["sociality"] = snapshot.Sociality,
            ["reproduction"] = snapshot.Reproduction,
            ["communication"] = snapshot.Communication,
            ["metabolism"] = snapshot.Metabolism,
            ["integument"] = snapshot.Integument,
            ["traits"] = ConceptSerializationUtils.ToArray(snapshot.Traits),
            ["unlocked_nodes"] = ConceptSerializationUtils.ToArray(snapshot.UnlockedNodes),
            ["lineage_milestones"] = ConceptSerializationUtils.ToArray(snapshot.LineageMilestones),
            ["adaptation_fit"] = snapshot.AdaptationFit,
            ["defense_score"] = snapshot.DefenseScore,
            ["mobility_score"] = snapshot.MobilityScore,
            ["cognition_score"] = snapshot.CognitionScore,
        };
    }

    public static EvolutionConceptSnapshot FromDictionary(Dictionary data)
    {
        EvolutionConceptSnapshot snapshot = new EvolutionConceptSnapshot();
        snapshot.SpeciesName = ConceptSerializationUtils.ReadString(data, "species_name");
        snapshot.EnvironmentLabel = ConceptSerializationUtils.ReadString(data, "environment_label");
        snapshot.EvolutionSpanMya = ConceptSerializationUtils.ReadDouble(data, "evolution_span_mya");
        snapshot.BodyPlan = ConceptSerializationUtils.ReadString(data, "body_plan");
        snapshot.Locomotion = ConceptSerializationUtils.ReadString(data, "locomotion");
        snapshot.Diet = ConceptSerializationUtils.ReadString(data, "diet");
        snapshot.Senses = ConceptSerializationUtils.ReadString(data, "senses");
        snapshot.Sociality = ConceptSerializationUtils.ReadString(data, "sociality");
        snapshot.Reproduction = ConceptSerializationUtils.ReadString(data, "reproduction");
        snapshot.Communication = ConceptSerializationUtils.ReadString(data, "communication");
        snapshot.Metabolism = ConceptSerializationUtils.ReadString(data, "metabolism");
        snapshot.Integument = ConceptSerializationUtils.ReadString(data, "integument");
        snapshot.Traits = ConceptSerializationUtils.ReadStringList(data, "traits");
        snapshot.UnlockedNodes = ConceptSerializationUtils.ReadStringList(data, "unlocked_nodes");
        snapshot.LineageMilestones = ConceptSerializationUtils.ReadStringList(data, "lineage_milestones");
        snapshot.AdaptationFit = ConceptSerializationUtils.ReadDouble(data, "adaptation_fit");
        snapshot.DefenseScore = ConceptSerializationUtils.ReadDouble(data, "defense_score");
        snapshot.MobilityScore = ConceptSerializationUtils.ReadDouble(data, "mobility_score");
        snapshot.CognitionScore = ConceptSerializationUtils.ReadDouble(data, "cognition_score");
        return snapshot;
    }
}
