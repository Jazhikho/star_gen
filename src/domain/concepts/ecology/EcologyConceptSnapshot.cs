using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Ecology;

/// <summary>
/// Folded-in ecology summary used by the concept atlas.
/// </summary>
public sealed class EcologyConceptSnapshot
{
    /// <summary>
    /// Total slot count.
    /// </summary>
    public int SlotCount { get; set; }

    /// <summary>
    /// Total connection count.
    /// </summary>
    public int ConnectionCount { get; set; }

    /// <summary>
    /// Primary productivity.
    /// </summary>
    public float Productivity { get; set; }

    /// <summary>
    /// Total biomass.
    /// </summary>
    public float Biomass { get; set; }

    /// <summary>
    /// Complexity score.
    /// </summary>
    public float Complexity { get; set; }

    /// <summary>
    /// Stability score.
    /// </summary>
    public float Stability { get; set; }

    /// <summary>
    /// Longest chain.
    /// </summary>
    public int MaxChainLength { get; set; }

    /// <summary>
    /// Counts by trophic level label.
    /// </summary>
    public Dictionary<string, int> LevelCounts { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Representative niche descriptions.
    /// </summary>
    public List<string> HighlightedNiches { get; set; } = new List<string>();
}
