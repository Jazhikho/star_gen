using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Validation;

/// <summary>
/// Contains the results of validating a celestial body.
/// </summary>
public partial class ValidationResult : RefCounted
{
    /// <summary>
    /// List of validation issues found.
    /// </summary>
    public Array<ValidationError> Errors;

    /// <summary>
    /// Creates a new validation result.
    /// </summary>
    public ValidationResult()
    {
        Errors = new Array<ValidationError>();
    }

    /// <summary>
    /// Returns true if there are no error-severity issues.
    /// </summary>
    public bool IsValid()
    {
        foreach (ValidationError error in Errors)
        {
            if (error.Severity == ValidationError.SeverityLevel.Error)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if there are no errors or warnings.
    /// </summary>
    public bool IsClean() => Errors.Count == 0;

    /// <summary>
    /// Adds an error to the result.
    /// </summary>
    public void AddError(string field, string message)
    {
        Errors.Add(new ValidationError(field, message, ValidationError.SeverityLevel.Error));
    }

    /// <summary>
    /// Adds a warning to the result.
    /// </summary>
    public void AddWarning(string field, string message)
    {
        Errors.Add(new ValidationError(field, message, ValidationError.SeverityLevel.Warning));
    }

    /// <summary>
    /// Returns all error-severity issues.
    /// </summary>
    public Array<ValidationError> GetErrorsOnly()
    {
        Array<ValidationError> result = new();
        foreach (ValidationError error in Errors)
        {
            if (error.Severity == ValidationError.SeverityLevel.Error)
            {
                result.Add(error);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all warning-severity issues.
    /// </summary>
    public Array<ValidationError> GetWarningsOnly()
    {
        Array<ValidationError> result = new();
        foreach (ValidationError error in Errors)
        {
            if (error.Severity == ValidationError.SeverityLevel.Warning)
            {
                result.Add(error);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the count of errors.
    /// </summary>
    public int GetErrorCount() => GetErrorsOnly().Count;

    /// <summary>
    /// Returns the count of warnings.
    /// </summary>
    public int GetWarningCount() => GetWarningsOnly().Count;
}
