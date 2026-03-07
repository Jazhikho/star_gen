using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Collects pre-generation validation issues.
/// </summary>
public sealed class GenerationParameterIssueSet
{
    /// <summary>
    /// Ordered issue list.
    /// </summary>
    public List<GenerationParameterIssue> Issues { get; } = new();

    /// <summary>
    /// Returns true when any error-level issue exists.
    /// </summary>
    public bool HasErrors()
    {
        foreach (GenerationParameterIssue issue in Issues)
        {
            if (issue.Severity == GenerationParameterIssue.IssueSeverity.Error)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds an error issue.
    /// </summary>
    public void AddError(string parameterId, string message)
    {
        Issues.Add(new GenerationParameterIssue(parameterId, message, GenerationParameterIssue.IssueSeverity.Error));
    }

    /// <summary>
    /// Adds a warning issue.
    /// </summary>
    public void AddWarning(string parameterId, string message)
    {
        Issues.Add(new GenerationParameterIssue(parameterId, message, GenerationParameterIssue.IssueSeverity.Warning));
    }
}
