namespace StarGen.Domain.Concepts;

/// <summary>
/// Request payload for running a concept module.
/// </summary>
public sealed class ConceptRunRequest
{
    /// <summary>
    /// Concept module to run.
    /// </summary>
    public ConceptKind Kind { get; set; }

    /// <summary>
    /// Context snapshot passed to the module.
    /// </summary>
    public ConceptContextSnapshot Context { get; set; } = new ConceptContextSnapshot();
}
