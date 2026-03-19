using System.Collections.Generic;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Generic atlas result for a concept module.
/// </summary>
public sealed class ConceptRunResult
{
    /// <summary>
    /// Display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short subtitle.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Summary body.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Display metrics.
    /// </summary>
    public List<ConceptMetric> Metrics { get; set; } = new List<ConceptMetric>();

    /// <summary>
    /// Named content sections.
    /// </summary>
    public List<ConceptSection> Sections { get; set; } = new List<ConceptSection>();

    /// <summary>
    /// Provenance payload.
    /// </summary>
    public ConceptProvenance Provenance { get; set; } = new ConceptProvenance();
}
