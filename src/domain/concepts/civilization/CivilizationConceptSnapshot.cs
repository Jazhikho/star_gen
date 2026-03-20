using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Civilization;

/// <summary>
/// Folded-in civilisation state used by the concept atlas and runtime pipeline.
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
    /// Administrative-capacity estimate in the range [0, 1].
    /// </summary>
    public double AdministrativeCapacity { get; set; }

    /// <summary>
    /// Narrative legitimacy frame.
    /// </summary>
    public string LegitimacyFrame { get; set; } = string.Empty;

    /// <summary>
    /// Core terrain summary.
    /// </summary>
    public string CoreTerrain { get; set; } = string.Empty;

    /// <summary>
    /// Key economic sectors.
    /// </summary>
    public List<string> EconomySectors { get; set; } = new List<string>();

    /// <summary>
    /// Cultural-value descriptors.
    /// </summary>
    public List<string> CulturalValues { get; set; } = new List<string>();

    /// <summary>
    /// Key technologies or knowledge systems.
    /// </summary>
    public List<string> KeyTechnologies { get; set; } = new List<string>();

    /// <summary>
    /// Historical milestone summaries.
    /// </summary>
    public List<string> HistoricalMilestones { get; set; } = new List<string>();

    /// <summary>
    /// External posture summaries.
    /// </summary>
    public List<string> ExternalPosture { get; set; } = new List<string>();
}

/// <summary>
/// Serialization helpers for civilization snapshots.
/// </summary>
public static class CivilizationConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(CivilizationConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["polity_name"] = snapshot.PolityName,
            ["regime_name"] = snapshot.RegimeName,
            ["tech_era"] = snapshot.TechEra,
            ["stability"] = snapshot.Stability,
            ["centralization"] = snapshot.Centralization,
            ["inclusiveness"] = snapshot.Inclusiveness,
            ["innovation"] = snapshot.Innovation,
            ["external_pressure"] = snapshot.ExternalPressure,
            ["administrative_capacity"] = snapshot.AdministrativeCapacity,
            ["legitimacy_frame"] = snapshot.LegitimacyFrame,
            ["core_terrain"] = snapshot.CoreTerrain,
            ["economy_sectors"] = ConceptSerializationUtils.ToArray(snapshot.EconomySectors),
            ["cultural_values"] = ConceptSerializationUtils.ToArray(snapshot.CulturalValues),
            ["key_technologies"] = ConceptSerializationUtils.ToArray(snapshot.KeyTechnologies),
            ["historical_milestones"] = ConceptSerializationUtils.ToArray(snapshot.HistoricalMilestones),
            ["external_posture"] = ConceptSerializationUtils.ToArray(snapshot.ExternalPosture),
        };
    }

    public static CivilizationConceptSnapshot FromDictionary(Dictionary data)
    {
        CivilizationConceptSnapshot snapshot = new CivilizationConceptSnapshot();
        snapshot.PolityName = ConceptSerializationUtils.ReadString(data, "polity_name");
        snapshot.RegimeName = ConceptSerializationUtils.ReadString(data, "regime_name");
        snapshot.TechEra = ConceptSerializationUtils.ReadString(data, "tech_era");
        snapshot.Stability = ConceptSerializationUtils.ReadDouble(data, "stability");
        snapshot.Centralization = ConceptSerializationUtils.ReadDouble(data, "centralization");
        snapshot.Inclusiveness = ConceptSerializationUtils.ReadDouble(data, "inclusiveness");
        snapshot.Innovation = ConceptSerializationUtils.ReadDouble(data, "innovation");
        snapshot.ExternalPressure = ConceptSerializationUtils.ReadDouble(data, "external_pressure");
        snapshot.AdministrativeCapacity = ConceptSerializationUtils.ReadDouble(data, "administrative_capacity");
        snapshot.LegitimacyFrame = ConceptSerializationUtils.ReadString(data, "legitimacy_frame");
        snapshot.CoreTerrain = ConceptSerializationUtils.ReadString(data, "core_terrain");
        snapshot.EconomySectors = ConceptSerializationUtils.ReadStringList(data, "economy_sectors");
        snapshot.CulturalValues = ConceptSerializationUtils.ReadStringList(data, "cultural_values");
        snapshot.KeyTechnologies = ConceptSerializationUtils.ReadStringList(data, "key_technologies");
        snapshot.HistoricalMilestones = ConceptSerializationUtils.ReadStringList(data, "historical_milestones");
        snapshot.ExternalPosture = ConceptSerializationUtils.ReadStringList(data, "external_posture");
        return snapshot;
    }
}
