using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for asteroid-spec helpers.
/// </summary>
[GlobalClass]
public partial class CSharpAsteroidSpecBridge : RefCounted
{
    /// <summary>
    /// Creates a random asteroid-spec payload.
    /// </summary>
    public Dictionary Random(int generationSeed) => AsteroidSpec.Random(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a carbonaceous payload.
    /// </summary>
    public Dictionary Carbonaceous(int generationSeed) => AsteroidSpec.Carbonaceous(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a metallic payload.
    /// </summary>
    public Dictionary Metallic(int generationSeed) => AsteroidSpec.Metallic(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a stony payload.
    /// </summary>
    public Dictionary Stony(int generationSeed) => AsteroidSpec.Stony(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Ceres-like payload.
    /// </summary>
    public Dictionary CeresLike(int generationSeed) => AsteroidSpec.CeresLike(generationSeed).ToDictionary();

    /// <summary>
    /// Normalizes an incoming asteroid-spec payload.
    /// </summary>
    public Dictionary Normalize(Dictionary data) => AsteroidSpec.FromDictionary(data).ToDictionary();
}
