using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Civilization;

/// <summary>
/// Folded-in civilisation summary used by the concept atlas.
/// </summary>
public sealed class CivilizationConceptSnapshot
{
    /// <summary>
    /// Display name of the polity.
    /// </summary>
    public string PolityName { get; set; } = string.Empty;

    /// <summary>
    /// Regime display name.
    /// </summary>
    public string RegimeName { get; set; } = string.Empty;

    /// <summary>
    /// Technology-era display name.
    /// </summary>
    public string TechEra { get; set; } = string.Empty;

    /// <summary>
    /// Stability estimate in the range [0, 1].
    /// </summary>
    public double Stability { get; set; }

    /// <summary>
    /// Centralization estimate in the range [0, 1].
    /// </summary>
    public double Centralization { get; set; }

    /// <summary>
    /// Inclusiveness estimate in the range [0, 1].
    /// </summary>
    public double Inclusiveness { get; set; }

    /// <summary>
    /// Innovation estimate in the range [0, 1].
    /// </summary>
    public double Innovation { get; set; }

    /// <summary>
    /// External-pressure estimate in the range [0, 1].
    /// </summary>
    public double ExternalPressure { get; set; }

    /// <summary>
    /// Narrative legitimacy frame.
    /// </summary>
    public string LegitimacyFrame { get; set; } = string.Empty;

    /// <summary>
    /// Economic pillars.
    /// </summary>
    public List<string> EconomySectors { get; set; } = new List<string>();

    /// <summary>
    /// Cultural-value descriptors.
    /// </summary>
    public List<string> CulturalValues { get; set; } = new List<string>();

    /// <summary>
    /// Historical milestone summaries.
    /// </summary>
    public List<string> HistoricalMilestones { get; set; } = new List<string>();

    /// <summary>
    /// External posture summaries.
    /// </summary>
    public List<string> ExternalPosture { get; set; } = new List<string>();
}
