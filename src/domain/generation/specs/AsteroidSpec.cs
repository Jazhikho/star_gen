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
    /// Preferred orbit band, or -1 for random.
    /// </summary>
    public int OrbitBand { get; set; }

    /// <summary>
    /// Preferred density profile, or -1 for random.
    /// </summary>
    public int DensityProfile { get; set; }

    /// <summary>
    /// Preferred albedo profile, or -1 for random.
    /// </summary>
    public int AlbedoProfile { get; set; }

    /// <summary>
    /// Creates a new asteroid specification.
    /// </summary>
    public AsteroidSpec(
        int generationSeed = 0,
        int asteroidType = -1,
        bool isLarge = false,
        int orbitBand = -1,
        int densityProfile = -1,
        int albedoProfile = -1,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : base(generationSeed, nameHint, overrides, useCaseSettings)
    {
        AsteroidType = asteroidType;
        IsLarge = isLarge;
        OrbitBand = orbitBand;
        DensityProfile = densityProfile;
        AlbedoProfile = albedoProfile;
    }

    /// <summary>
    /// Compatibility constructor accepting enum asteroid type.
    /// </summary>
    public AsteroidSpec(
        int generationSeed,
        AsteroidTypeArchetype.Type asteroidType,
        bool isLarge = false,
        int orbitBand = -1,
        int densityProfile = -1,
        int albedoProfile = -1,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : this(generationSeed, (int)asteroidType, isLarge, orbitBand, densityProfile, albedoProfile, nameHint, overrides, useCaseSettings)
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
    /// Creates a dark-red primitive asteroid specification.
    /// </summary>
    public static AsteroidSpec DarkRedPrimitive(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.DType, false, 1, 0, 0);
    }

    /// <summary>
    /// Creates a basaltic asteroid specification.
    /// </summary>
    public static AsteroidSpec Basaltic(int generationSeed)
    {
        return new AsteroidSpec(generationSeed, (int)AsteroidTypeArchetype.Type.VType, false, 0, 2, 2);
    }

    /// <summary>
    /// Returns whether an asteroid type was specified.
    /// </summary>
    public bool HasAsteroidType() => AsteroidType >= 0;

    /// <summary>
    /// Returns whether an orbit band was specified.
    /// </summary>
    public bool HasOrbitBand() => OrbitBand >= 0;

    /// <summary>
    /// Returns whether a density profile was specified.
    /// </summary>
    public bool HasDensityProfile() => DensityProfile >= 0;

    /// <summary>
    /// Returns whether an albedo profile was specified.
    /// </summary>
    public bool HasAlbedoProfile() => AlbedoProfile >= 0;

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "asteroid";
        data["asteroid_type"] = AsteroidType;
        data["is_large"] = IsLarge;
        data["orbit_band"] = OrbitBand;
        data["density_profile"] = DensityProfile;
        data["albedo_profile"] = AlbedoProfile;
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
        int orbitBand = data.ContainsKey("orbit_band") ? (int)data["orbit_band"] : -1;
        int densityProfile = data.ContainsKey("density_profile") ? (int)data["density_profile"] : -1;
        int albedoProfile = data.ContainsKey("albedo_profile") ? (int)data["albedo_profile"] : -1;
        AsteroidSpec spec = new AsteroidSpec(generationSeed, asteroidType, isLarge, orbitBand, densityProfile, albedoProfile, nameHint, overrides);
        spec.ApplyBaseFromDictionary(data);
        return spec;
    }
}
