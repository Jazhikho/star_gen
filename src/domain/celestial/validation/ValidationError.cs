using Godot;

namespace StarGen.Domain.Celestial.Validation;

/// <summary>
/// Represents a single validation error.
/// </summary>
public partial class ValidationError : RefCounted
{
    /// <summary>
    /// Severity levels for validation errors.
    /// </summary>
    public enum SeverityLevel
    {
        Warning,
        Error,
    }

    /// <summary>
    /// The field or path that has the error.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Severity of the error.
    /// </summary>
    public SeverityLevel Severity { get; set; }

    /// <summary>
    /// Creates a new validation error.
    /// </summary>
    public ValidationError(
        string field = "",
        string message = "",
        SeverityLevel severity = SeverityLevel.Error)
    {
        Field = field;
        Message = message;
        Severity = severity;
    }

    /// <summary>
    /// Returns a formatted string representation of the error.
    /// </summary>
    public string FormatError()
    {
        string severityText;
        if (Severity == SeverityLevel.Error)
        {
            severityText = "ERROR";
        }
        else
        {
            severityText = "WARNING";
        }

        return $"[{severityText}] {Field}: {Message}";
    }
}
