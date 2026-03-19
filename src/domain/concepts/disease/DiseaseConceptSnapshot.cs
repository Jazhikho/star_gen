using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Disease;

/// <summary>
/// Folded-in disease summary used by the concept atlas.
/// </summary>
public sealed class DiseaseConceptSnapshot
{
    /// <summary>
    /// Pathogen category.
    /// </summary>
    public string PathogenType { get; set; } = string.Empty;

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
    /// Resilience estimate in the range [0, 1].
    /// </summary>
    public double Resilience { get; set; }

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
}
