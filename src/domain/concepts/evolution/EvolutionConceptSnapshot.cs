using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Evolution;

/// <summary>
/// Folded-in evolution summary used by the concept atlas.
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
    /// Unlocked trait list.
    /// </summary>
    public List<string> Traits { get; set; } = new List<string>();

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
