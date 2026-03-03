using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for planet-spec helpers.
/// </summary>
[GlobalClass]
public partial class CSharpPlanetSpecBridge : RefCounted
{
    /// <summary>
    /// Creates a random planet-spec payload.
    /// </summary>
    public Dictionary Random(int generationSeed) => PlanetSpec.Random(generationSeed).ToDictionary();

    /// <summary>
    /// Creates an Earth-like planet-spec payload.
    /// </summary>
    public Dictionary EarthLike(int generationSeed) => PlanetSpec.EarthLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a hot-Jupiter payload.
    /// </summary>
    public Dictionary HotJupiter(int generationSeed) => PlanetSpec.HotJupiter(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a cold-giant payload.
    /// </summary>
    public Dictionary ColdGiant(int generationSeed) => PlanetSpec.ColdGiant(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Mars-like payload.
    /// </summary>
    public Dictionary MarsLike(int generationSeed) => PlanetSpec.MarsLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a dwarf-planet payload.
    /// </summary>
    public Dictionary DwarfPlanet(int generationSeed) => PlanetSpec.DwarfPlanet(generationSeed).ToDictionary();

    /// <summary>
    /// Creates an ice-giant payload.
    /// </summary>
    public Dictionary IceGiant(int generationSeed) => PlanetSpec.IceGiant(generationSeed).ToDictionary();

    /// <summary>
    /// Normalizes an incoming planet-spec payload.
    /// </summary>
    public Dictionary Normalize(Dictionary data) => PlanetSpec.FromDictionary(data).ToDictionary();
}
