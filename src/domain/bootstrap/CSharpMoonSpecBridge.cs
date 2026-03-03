using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for moon-spec helpers.
/// </summary>
[GlobalClass]
public partial class CSharpMoonSpecBridge : RefCounted
{
    /// <summary>
    /// Creates a random moon-spec payload.
    /// </summary>
    public Dictionary Random(int generationSeed) => MoonSpec.Random(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Luna-like moon-spec payload.
    /// </summary>
    public Dictionary LunaLike(int generationSeed) => MoonSpec.LunaLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Europa-like moon-spec payload.
    /// </summary>
    public Dictionary EuropaLike(int generationSeed) => MoonSpec.EuropaLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Titan-like moon-spec payload.
    /// </summary>
    public Dictionary TitanLike(int generationSeed) => MoonSpec.TitanLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a captured-moon payload.
    /// </summary>
    public Dictionary Captured(int generationSeed) => MoonSpec.Captured(generationSeed).ToDictionary();

    /// <summary>
    /// Normalizes an incoming moon-spec payload.
    /// </summary>
    public Dictionary Normalize(Dictionary data) => MoonSpec.FromDictionary(data).ToDictionary();
}
