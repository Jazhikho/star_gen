using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for star-spec helpers.
/// </summary>
[GlobalClass]
public partial class CSharpStarSpecBridge : RefCounted
{
    /// <summary>
    /// Creates a random star-spec payload.
    /// </summary>
    public Dictionary Random(int generationSeed) => StarSpec.Random(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a Sun-like star-spec payload.
    /// </summary>
    public Dictionary SunLike(int generationSeed) => StarSpec.SunLike(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a red-dwarf star-spec payload.
    /// </summary>
    public Dictionary RedDwarf(int generationSeed) => StarSpec.RedDwarf(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a hot-blue star-spec payload.
    /// </summary>
    public Dictionary HotBlue(int generationSeed) => StarSpec.HotBlue(generationSeed).ToDictionary();

    /// <summary>
    /// Normalizes an incoming star-spec payload.
    /// </summary>
    public Dictionary Normalize(Dictionary data) => StarSpec.FromDictionary(data).ToDictionary();
}
