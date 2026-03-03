using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for ring-system-spec helpers.
/// </summary>
[GlobalClass]
public partial class CSharpRingSystemSpecBridge : RefCounted
{
    /// <summary>
    /// Creates a random ring-system payload.
    /// </summary>
    public Dictionary Random(int generationSeed) => RingSystemSpec.Random(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a trace-ring payload.
    /// </summary>
    public Dictionary Trace(int generationSeed) => RingSystemSpec.Trace(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a simple-ring payload.
    /// </summary>
    public Dictionary Simple(int generationSeed) => RingSystemSpec.Simple(generationSeed).ToDictionary();

    /// <summary>
    /// Creates a complex-ring payload.
    /// </summary>
    public Dictionary Complex(int generationSeed) => RingSystemSpec.Complex(generationSeed).ToDictionary();

    /// <summary>
    /// Creates an icy-ring payload.
    /// </summary>
    public Dictionary Icy(int generationSeed, int complexity = -1) => RingSystemSpec.Icy(generationSeed, complexity).ToDictionary();

    /// <summary>
    /// Creates a rocky-ring payload.
    /// </summary>
    public Dictionary Rocky(int generationSeed, int complexity = -1) => RingSystemSpec.Rocky(generationSeed, complexity).ToDictionary();

    /// <summary>
    /// Normalizes an incoming ring-system payload.
    /// </summary>
    public Dictionary Normalize(Dictionary data) => RingSystemSpec.FromDictionary(data).ToDictionary();
}
