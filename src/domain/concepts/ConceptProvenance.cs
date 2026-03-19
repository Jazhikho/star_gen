namespace StarGen.Domain.Concepts;

/// <summary>
/// Provenance payload for concept-atlas runs.
/// </summary>
public sealed class ConceptProvenance
{
    /// <summary>
    /// Concept identifier.
    /// </summary>
    public string ConceptId { get; set; } = string.Empty;

    /// <summary>
    /// Deterministic seed used for generation.
    /// </summary>
    public int Seed { get; set; }

    /// <summary>
    /// Generator version string.
    /// </summary>
    public string GeneratorVersion { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable source context summary.
    /// </summary>
    public string SourceContext { get; set; } = string.Empty;
}
