using Godot;
using StarGen.Domain.Celestial;

namespace StarGen.Services.Persistence;

/// <summary>
/// Result payload for compact body-save loads.
/// </summary>
public partial class SaveDataLoadResult : RefCounted
{
    /// <summary>
    /// Returns whether the load succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Loaded body, if successful.
    /// </summary>
    public CelestialBody? Body { get; set; }

    /// <summary>
    /// Error message when the load fails.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Creates a successful load-result payload.
    /// </summary>
    public static SaveDataLoadResult CreateSuccess(CelestialBody body)
    {
        SaveDataLoadResult result = new();
        result.Success = true;
        result.Body = body;
        return result;
    }

    /// <summary>
    /// Creates a failed load-result payload.
    /// </summary>
    public static SaveDataLoadResult CreateError(string message)
    {
        SaveDataLoadResult result = new();
        result.Success = false;
        result.ErrorMessage = message;
        return result;
    }
}
