namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Defines how a generation parameter is exposed to the editor layer.
/// </summary>
public sealed class GenerationParameterDefinition
{
    /// <summary>
    /// Stable parameter identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Display label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Optional display units.
    /// </summary>
    public string Units { get; }

    /// <summary>
    /// Intended control type.
    /// </summary>
    public GenerationParameterControlType ControlType { get; }

    /// <summary>
    /// Explanation of the underlying assumption or target.
    /// </summary>
    public string AssumptionText { get; }

    /// <summary>
    /// Whether the parameter supports lock semantics.
    /// </summary>
    public bool SupportsLock { get; }

    /// <summary>
    /// Whether the parameter represents a target or bias instead of a hard value.
    /// </summary>
    public bool SupportsTarget { get; }

    /// <summary>
    /// Creates a parameter definition.
    /// </summary>
    public GenerationParameterDefinition(
        string id,
        string label,
        string units,
        GenerationParameterControlType controlType,
        string assumptionText,
        bool supportsLock = false,
        bool supportsTarget = false)
    {
        Id = id;
        Label = label;
        Units = units;
        ControlType = controlType;
        AssumptionText = assumptionText;
        SupportsLock = supportsLock;
        SupportsTarget = supportsTarget;
    }
}
