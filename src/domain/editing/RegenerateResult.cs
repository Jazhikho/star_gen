using StarGen.Domain.Celestial;

namespace StarGen.Domain.Editing;

/// <summary>
/// Result of a celestial body regeneration attempt.
/// </summary>
public partial class RegenerateResult : Godot.RefCounted
{
    /// <summary>Whether regeneration succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>The regenerated body, or null on failure.</summary>
    public CelestialBody? Body { get; set; }

    /// <summary>Error message when Success is false.</summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Creates a successful result.</summary>
    public static RegenerateResult Ok(CelestialBody body)
    {
        RegenerateResult result = new RegenerateResult
        {
            Success = true,
            Body = body,
        };
        return result;
    }

    /// <summary>Creates a failure result.</summary>
    public static RegenerateResult Fail(string message)
    {
        RegenerateResult result = new RegenerateResult
        {
            Success = false,
            ErrorMessage = message,
        };
        return result;
    }
}
