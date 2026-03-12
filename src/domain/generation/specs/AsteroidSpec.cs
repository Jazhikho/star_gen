using Godot.Collections;
using StarGen.Domain.Generation;
using AsteroidTypeArchetype = StarGen.Domain.Generation.Archetypes.AsteroidType;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for asteroid generation.
/// </summary>
public partial class AsteroidSpec : BaseSpec
{
    /// <summary>
    /// Target asteroid type, or -1 for random.
    /// </summary>
    public int AsteroidType { get; set; }

    /// <summary>
    /// Whether this is a large asteroid.
    /// </summary>
    public bool IsLarge { get; set; }

    /// <summary>
    /// Creates a new asteroid specification.
    /// </summary>
    public AsteroidSpec(
        int generationSeed = 0,
        int asteroidType = -1,
        bool isLarge = false,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : base(generationSeed, nameHint, overrides, useCaseSettings)
    {
        AsteroidType = asteroidType;
        IsLarge = isLarge;
    }

    /// <summary>
    /// Compatibility constructor accepting enum asteroid type.
    /// </summary>
    public AsteroidSpec(
        int generationSeed,
        AsteroidTypeArchetype.Type asteroidType,
        bool isLarge = false,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : this(generationSeed, (int)asteroidType, isLarge, nameHint, overrides, useCaseSettings)
    {
    }

    /// <summary>
    /// Creates a fully random asteroid specification.
    /// </summary>
    public static AsteroidSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates a carbonaceous asteroid specification.
    /// </summary>
    public static AsteroidSpec Carbonaceous(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.CType, false);
    }

    /// <summary>
    /// Creates a metallic asteroid specification.
    /// </summary>
    public static AsteroidSpec Metallic(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.MType, false);
    }

    /// <summary>
    /// Creates a stony asteroid specification.
    /// </summary>
    public static AsteroidSpec Stony(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.SType, false);
    }

    /// <summary>
    /// Creates a Ceres-like asteroid specification.
    /// </summary>
    public static AsteroidSpec CeresLike(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.CType, true);
    }

    /// <summary>
    /// Returns whether an asteroid type was specified.
    /// </summary>
    public bool HasAsteroidType() => AsteroidType >= 0;

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "asteroid";
        data["asteroid_type"] = AsteroidType;
        data["is_large"] = IsLarge;
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static AsteroidSpec FromDictionary(Dictionary data)
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

        int asteroidType;
        if (data.ContainsKey("asteroid_type"))
        {
            asteroidType = (int)data["asteroid_type"];
        }
        else
        {
            asteroidType = -1;
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

        bool isLarge = data.ContainsKey("is_large") && (bool)data["is_large"];
        AsteroidSpec spec = new AsteroidSpec(generationSeed, asteroidType, isLarge, nameHint, overrides);
        spec.ApplyBaseFromDictionary(data);
        return spec;
    }
}
