using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.Domain.Systems;

/// <summary>
/// Result payload for system-wide moon generation.
/// </summary>
public partial class MoonGenerationResult : Godot.RefCounted
{
    /// <summary>
    /// Generated moons across the full system.
    /// </summary>
    public Array<CelestialBody> Moons = new();

    /// <summary>
    /// Mapping of planet identifiers to generated moon identifiers.
    /// </summary>
    public Dictionary PlanetMoonMap = new();

    /// <summary>
    /// Whether generation completed successfully.
    /// </summary>
    public bool Success;

    /// <summary>
    /// Error details when generation fails.
    /// </summary>
    public string ErrorMessage = string.Empty;
}
