using System.Collections.Generic;
using Godot.Collections;

namespace StarGen.Domain.Concepts.Disease;

/// <summary>
/// Folded-in disease state used by the concept atlas and runtime pipeline.
/// </summary>
public sealed class DiseaseConceptSnapshot
{
    /// <summary>
    /// Pathogen category.
    /// </summary>
    public string PathogenType { get; set; } = string.Empty;

    /// <summary>
    /// Host-environment summary.
    /// </summary>
    public string HostEnvironment { get; set; } = string.Empty;

    /// <summary>
    /// Infectivity estimate in the range [0, 1].
    /// </summary>
    public double Infectivity { get; set; }

    /// <summary>
    /// Severity estimate in the range [0, 1].
    /// </summary>
    public double Severity { get; set; }

    /// <summary>
    /// Lethality estimate in the range [0, 1].
    /// </summary>
    public double Lethality { get; set; }

    /// <summary>
    /// Mutability estimate in the range [0, 1].
    /// </summary>
    public double Mutability { get; set; }

    /// <summary>
    /// Environmental resilience estimate in the range [0, 1].
    /// </summary>
    public double Resilience { get; set; }

    /// <summary>
    /// Incubation estimate in the range [0, 1].
    /// </summary>
    public double Incubation { get; set; }

    /// <summary>
    /// Airborne-transmission pressure in the range [0, 1].
    /// </summary>
    public double Airborne { get; set; }

    /// <summary>
    /// Immune-evasion pressure in the range [0, 1].
    /// </summary>
    public double ImmuneEvasion { get; set; }

    /// <summary>
    /// Simulated host population size.
    /// </summary>
    public int PopulationSize { get; set; }

    /// <summary>
    /// Symptoms associated with the outbreak.
    /// </summary>
    public List<string> Symptoms { get; set; } = new List<string>();

    /// <summary>
    /// Approximate total infected count.
    /// </summary>
    public int TotalInfected { get; set; }

    /// <summary>
    /// Approximate total death count.
    /// </summary>
    public int TotalDeaths { get; set; }

    /// <summary>
    /// Peak concurrent infected count.
    /// </summary>
    public int PeakInfected { get; set; }

    /// <summary>
    /// Day of peak infections.
    /// </summary>
    public int PeakDay { get; set; }

    /// <summary>
    /// Environmental driver notes.
    /// </summary>
    public List<string> EnvironmentalDrivers { get; set; } = new List<string>();

    /// <summary>
    /// Mutation-event summaries generated during the run.
    /// </summary>
    public List<string> MutationEvents { get; set; } = new List<string>();
}

/// <summary>
/// Serialization helpers for disease snapshots.
/// </summary>
public static class DiseaseConceptSnapshotSerialization
{
    public static Dictionary ToDictionary(DiseaseConceptSnapshot snapshot)
    {
        return new Dictionary
        {
            ["pathogen_type"] = snapshot.PathogenType,
            ["host_environment"] = snapshot.HostEnvironment,
            ["infectivity"] = snapshot.Infectivity,
            ["severity"] = snapshot.Severity,
            ["lethality"] = snapshot.Lethality,
            ["mutability"] = snapshot.Mutability,
            ["resilience"] = snapshot.Resilience,
            ["incubation"] = snapshot.Incubation,
            ["airborne"] = snapshot.Airborne,
            ["immune_evasion"] = snapshot.ImmuneEvasion,
            ["population_size"] = snapshot.PopulationSize,
            ["symptoms"] = ConceptSerializationUtils.ToArray(snapshot.Symptoms),
            ["total_infected"] = snapshot.TotalInfected,
            ["total_deaths"] = snapshot.TotalDeaths,
            ["peak_infected"] = snapshot.PeakInfected,
            ["peak_day"] = snapshot.PeakDay,
            ["environmental_drivers"] = ConceptSerializationUtils.ToArray(snapshot.EnvironmentalDrivers),
            ["mutation_events"] = ConceptSerializationUtils.ToArray(snapshot.MutationEvents),
        };
    }

    public static DiseaseConceptSnapshot FromDictionary(Dictionary data)
    {
        DiseaseConceptSnapshot snapshot = new DiseaseConceptSnapshot();
        snapshot.PathogenType = ConceptSerializationUtils.ReadString(data, "pathogen_type");
        snapshot.HostEnvironment = ConceptSerializationUtils.ReadString(data, "host_environment");
        snapshot.Infectivity = ConceptSerializationUtils.ReadDouble(data, "infectivity");
        snapshot.Severity = ConceptSerializationUtils.ReadDouble(data, "severity");
        snapshot.Lethality = ConceptSerializationUtils.ReadDouble(data, "lethality");
        snapshot.Mutability = ConceptSerializationUtils.ReadDouble(data, "mutability");
        snapshot.Resilience = ConceptSerializationUtils.ReadDouble(data, "resilience");
        snapshot.Incubation = ConceptSerializationUtils.ReadDouble(data, "incubation");
        snapshot.Airborne = ConceptSerializationUtils.ReadDouble(data, "airborne");
        snapshot.ImmuneEvasion = ConceptSerializationUtils.ReadDouble(data, "immune_evasion");
        snapshot.PopulationSize = ConceptSerializationUtils.ReadInt(data, "population_size");
        snapshot.Symptoms = ConceptSerializationUtils.ReadStringList(data, "symptoms");
        snapshot.TotalInfected = ConceptSerializationUtils.ReadInt(data, "total_infected");
        snapshot.TotalDeaths = ConceptSerializationUtils.ReadInt(data, "total_deaths");
        snapshot.PeakInfected = ConceptSerializationUtils.ReadInt(data, "peak_infected");
        snapshot.PeakDay = ConceptSerializationUtils.ReadInt(data, "peak_day");
        snapshot.EnvironmentalDrivers = ConceptSerializationUtils.ReadStringList(data, "environmental_drivers");
        snapshot.MutationEvents = ConceptSerializationUtils.ReadStringList(data, "mutation_events");
        return snapshot;
    }
}
