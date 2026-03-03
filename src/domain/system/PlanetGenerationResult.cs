using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.Domain.Systems;

/// <summary>
/// Result of planet generation for a solar system.
/// </summary>
public partial class PlanetGenerationResult : RefCounted
{
    /// <summary>
    /// Generated planets.
    /// </summary>
    public Array<CelestialBody> Planets = new();

    /// <summary>
    /// Updated slots including fill-state changes.
    /// </summary>
    public Array<OrbitSlot> Slots = new();

    /// <summary>
    /// Whether generation succeeded.
    /// </summary>
    public bool Success;

    /// <summary>
    /// Optional error message.
    /// </summary>
    public string ErrorMessage = string.Empty;
}
