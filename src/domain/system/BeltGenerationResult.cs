using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.Domain.Systems;

/// <summary>
/// Result payload for asteroid-belt generation.
/// </summary>
public partial class BeltGenerationResult : Godot.RefCounted
{
    /// <summary>
    /// Generated belt definitions.
    /// </summary>
    public Array<AsteroidBelt> Belts = new();

    /// <summary>
    /// Generated major asteroids across all belts.
    /// </summary>
    public Array<CelestialBody> Asteroids = new();

    /// <summary>
    /// Mapping of belt identifiers to major asteroid identifiers.
    /// </summary>
    public Dictionary BeltAsteroidMap = new();

    /// <summary>
    /// Whether generation completed successfully.
    /// </summary>
    public bool Success;

    /// <summary>
    /// Error details when generation fails.
    /// </summary>
    public string ErrorMessage = string.Empty;
}
