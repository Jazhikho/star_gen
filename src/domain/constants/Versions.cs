namespace StarGen.Domain.Constants;

/// <summary>
/// Version constants for tracking generator and schema compatibility.
/// </summary>
public static class Versions
{
    /// <summary>
    /// Current generator version. Increment when generation output changes.
    /// </summary>
    public const string GeneratorVersion = "0.1.0";

    /// <summary>
    /// Current schema version. Increment when serialization has breaking changes.
    /// </summary>
    public const int SchemaVersion = 1;
}
