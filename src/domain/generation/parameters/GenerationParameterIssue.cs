namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// One validation or realism issue attached to a parameter set.
/// </summary>
public sealed class GenerationParameterIssue
{
    /// <summary>
    /// Severity levels for pre-generation issues.
    /// </summary>
    public enum IssueSeverity
    {
        Warning,
        Error,
    }

    /// <summary>
    /// Associated parameter identifier.
    /// </summary>
    public string ParameterId { get; }

    /// <summary>
    /// Human-readable issue text.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    public IssueSeverity Severity { get; }

    /// <summary>
    /// Creates a parameter issue.
    /// </summary>
    public GenerationParameterIssue(string parameterId, string message, IssueSeverity severity)
    {
        ParameterId = parameterId;
        Message = message;
        Severity = severity;
    }
}
