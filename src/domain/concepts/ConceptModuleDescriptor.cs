namespace StarGen.Domain.Concepts;

/// <summary>
/// Static metadata for a concept module.
/// </summary>
public sealed class ConceptModuleDescriptor
{
    /// <summary>
    /// Concept kind identifier.
    /// </summary>
    public ConceptKind Kind { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Short summary for the atlas.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Description of supported inputs.
    /// </summary>
    public string AcceptedContext { get; set; } = string.Empty;
}
