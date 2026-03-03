using Godot.Collections;
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
    public int AsteroidType;

    /// <summary>
    /// Whether this is a large asteroid.
    /// </summary>
    public bool IsLarge;

    /// <summary>
    /// Creates a new asteroid specification.
    /// </summary>
    public AsteroidSpec(
        int generationSeed = 0,
        int asteroidType = -1,
        bool isLarge = false,
        string nameHint = "",
        Dictionary? overrides = null)
        : base(generationSeed, nameHint, overrides)
    {
        AsteroidType = asteroidType;
        IsLarge = isLarge;
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
        return new AsteroidSpec(
            data.ContainsKey("generation_seed") ? (int)data["generation_seed"] : 0,
            data.ContainsKey("asteroid_type") ? (int)data["asteroid_type"] : -1,
            data.ContainsKey("is_large") && (bool)data["is_large"],
            data.ContainsKey("name_hint") ? (string)data["name_hint"] : string.Empty,
            data.ContainsKey("overrides") ? (Dictionary)data["overrides"] : null);
    }
}
