using Godot;
using Godot.Collections;
using RingComplexityArchetype = StarGen.Domain.Generation.Archetypes.RingComplexity;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for ring-system generation.
/// </summary>
public partial class RingSystemSpec : BaseSpec
{
    /// <summary>
    /// Complexity level, or -1 for random.
    /// </summary>
    public int Complexity { get; set; }

    /// <summary>
    /// Whether the rings are icy, or nil for auto.
    /// </summary>
    public Variant IsIcy { get; set; }

    /// <summary>
    /// Creates a new ring-system specification.
    /// </summary>
    public RingSystemSpec(
        int generationSeed = 0,
        int complexity = -1,
        Variant isIcy = default,
        string nameHint = "",
        Dictionary? overrides = null)
        : base(generationSeed, nameHint, overrides)
    {
        Complexity = complexity;
        IsIcy = isIcy;
    }

    /// <summary>
    /// Creates a fully random ring-system specification.
    /// </summary>
    public static RingSystemSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates a trace-ring specification.
    /// </summary>
    public static RingSystemSpec Trace(int generationSeed)
    {
        return new RingSystemSpec(generationSeed, (int)RingComplexityArchetype.Level.Trace);
    }

    /// <summary>
    /// Creates a simple-ring specification.
    /// </summary>
    public static RingSystemSpec Simple(int generationSeed)
    {
        return new RingSystemSpec(generationSeed, (int)RingComplexityArchetype.Level.Simple);
    }

    /// <summary>
    /// Creates a complex-ring specification.
    /// </summary>
    public static RingSystemSpec Complex(int generationSeed)
    {
        return new RingSystemSpec(generationSeed, (int)RingComplexityArchetype.Level.Complex);
    }

    /// <summary>
    /// Creates an icy-ring specification.
    /// </summary>
    public static RingSystemSpec Icy(int generationSeed, int complexity = -1)
    {
        return new RingSystemSpec(generationSeed, complexity, true);
    }

    /// <summary>
    /// Creates a rocky-ring specification.
    /// </summary>
    public static RingSystemSpec Rocky(int generationSeed, int complexity = -1)
    {
        return new RingSystemSpec(generationSeed, complexity, false);
    }

    /// <summary>
    /// Returns whether a complexity was specified.
    /// </summary>
    public bool HasComplexity() => Complexity >= 0;

    /// <summary>
    /// Returns whether a composition preference was specified.
    /// </summary>
    public bool HasCompositionPreference() => !IsIcy.Equals(default(Variant));

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "ring_system";
        data["complexity"] = Complexity;
        data["is_icy"] = IsIcy;
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static RingSystemSpec FromDictionary(Dictionary data)
    {
        int generationSeed;
        if (data.ContainsKey("generation_seed"))
        {
            generationSeed = (int)data["generation_seed"];
        }
        else
        {
            generationSeed = 0;
        }

        int complexity;
        if (data.ContainsKey("complexity"))
        {
            complexity = (int)data["complexity"];
        }
        else
        {
            complexity = -1;
        }

        Variant isIcy;
        if (data.ContainsKey("is_icy"))
        {
            isIcy = data["is_icy"];
        }
        else
        {
            isIcy = default;
        }
        string nameHint;
        if (data.ContainsKey("name_hint"))
        {
            nameHint = (string)data["name_hint"];
        }
        else
        {
            nameHint = string.Empty;
        }

        Dictionary? overrides = null;
        if (data.ContainsKey("overrides"))
        {
            overrides = (Dictionary)data["overrides"];
        }

        return new RingSystemSpec(generationSeed, complexity, isIcy, nameHint, overrides);
    }
}
