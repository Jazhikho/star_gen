using Godot;
using StarGen.Domain.Systems;

namespace StarGen.Services.Persistence;

/// <summary>
/// Result payload for solar-system load operations.
/// </summary>
public partial class SystemPersistenceLoadResult : RefCounted
{
    /// <summary>
    /// Loaded solar system, if successful.
    /// </summary>
    public SolarSystem? System;

    /// <summary>
    /// Returns whether the load succeeded.
    /// </summary>
    public bool Success;

    /// <summary>
    /// Error message when the load fails.
    /// </summary>
    public string ErrorMessage = string.Empty;
}
