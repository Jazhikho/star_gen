namespace StarGen.Domain.Concepts;

/// <summary>
/// Explicit concept generation outcome.
/// </summary>
public enum ConceptRunStatus
{
    Generated = 0,
    NotApplicable = 1,
    Failed = 2,
}
