using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Simplified system context for station-placement evaluation.
/// </summary>
public partial class StationSystemContext : RefCounted
{
    /// <summary>
    /// System identifier.
    /// </summary>
    public string SystemId = string.Empty;

    /// <summary>
    /// Whether the system is a bridge system.
    /// </summary>
    public bool IsBridgeSystem;

    /// <summary>
    /// Number of habitable planets.
    /// </summary>
    public int HabitablePlanetCount;

    /// <summary>
    /// Number of worlds with native populations.
    /// </summary>
    public int NativeWorldCount;

    /// <summary>
    /// Number of worlds with colonies.
    /// </summary>
    public int ColonyWorldCount;

    /// <summary>
    /// Highest native tech level, or null when none exists.
    /// </summary>
    public TechnologyLevel.Level? HighestNativeTech;

    /// <summary>
    /// Whether any natives are spacefaring.
    /// </summary>
    public bool HasSpacefaringNatives;

    /// <summary>
    /// Total resource-richness score in the range [0, 1].
    /// </summary>
    public double ResourceRichness;

    /// <summary>
    /// Number of asteroid belts.
    /// </summary>
    public int AsteroidBeltCount;

    /// <summary>
    /// Planet ids in the system.
    /// </summary>
    public Array<string> PlanetIds = new();

    /// <summary>
    /// Planet ids with native populations.
    /// </summary>
    public Array<string> NativePlanetIds = new();

    /// <summary>
    /// Planet ids with colonies.
    /// </summary>
    public Array<string> ColonyPlanetIds = new();

    /// <summary>
    /// Resource-rich body ids.
    /// </summary>
    public Array<string> ResourceBodyIds = new();
}
