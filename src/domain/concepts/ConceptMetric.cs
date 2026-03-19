namespace StarGen.Domain.Concepts;

/// <summary>
/// Single numeric metric shown in the concept atlas.
/// </summary>
public sealed class ConceptMetric
{
    /// <summary>
    /// Metric label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Current value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Maximum display value.
    /// </summary>
    public double MaxValue { get; set; } = 1.0;

    /// <summary>
    /// Optional formatted display string.
    /// </summary>
    public string DisplayText { get; set; } = string.Empty;
}
