using Godot;
using Godot.Collections;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for moon generation.
/// </summary>
public partial class MoonSpec : BaseSpec
{
    /// <summary>
    /// Size category, or -1 for random.
    /// </summary>
    public int SizeCategory { get; set; }

    /// <summary>
    /// Whether this moon is captured.
    /// </summary>
    public bool IsCaptured { get; set; }

    /// <summary>
    /// Whether an atmosphere is required, or nil for auto.
    /// </summary>
    public Variant HasAtmosphere { get; set; }

    /// <summary>
    /// Whether a subsurface ocean is required, or nil for auto.
    /// </summary>
    public Variant HasSubsurfaceOcean { get; set; }

    /// <summary>
    /// Creates a new moon specification.
    /// </summary>
    public MoonSpec(
        int generationSeed = 0,
        int sizeCategory = -1,
        bool isCaptured = false,
        Variant hasAtmosphere = default,
        Variant hasSubsurfaceOcean = default,
        string nameHint = "",
        Dictionary? overrides = null)
        : base(generationSeed, nameHint, overrides)
    {
        SizeCategory = sizeCategory;
        IsCaptured = isCaptured;
        HasAtmosphere = hasAtmosphere;
        HasSubsurfaceOcean = hasSubsurfaceOcean;
    }

    /// <summary>
    /// Compatibility constructor accepting enum size category.
    /// </summary>
    public MoonSpec(
        int generationSeed,
        SizeCategoryArchetype.Category sizeCategory,
        bool isCaptured = false,
        Variant hasAtmosphere = default,
        Variant hasSubsurfaceOcean = default,
        string nameHint = "",
        Dictionary? overrides = null)
        : this(generationSeed, (int)sizeCategory, isCaptured, hasAtmosphere, hasSubsurfaceOcean, nameHint, overrides)
    {
    }

    /// <summary>
    /// Creates a fully random moon specification.
    /// </summary>
    public static MoonSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates a Luna-like moon specification.
    /// </summary>
    public static MoonSpec LunaLike(int generationSeed)
    {
        return new MoonSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.SubTerrestrial,
            false,
            false,
            false);
    }

    /// <summary>
    /// Creates a Europa-like moon specification.
    /// </summary>
    public static MoonSpec EuropaLike(int generationSeed)
    {
        return new MoonSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.SubTerrestrial,
            false,
            false,
            true);
    }

    /// <summary>
    /// Creates a Titan-like moon specification.
    /// </summary>
    public static MoonSpec TitanLike(int generationSeed)
    {
        return new MoonSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.SubTerrestrial,
            false,
            true,
            true);
    }

    /// <summary>
    /// Creates a captured-moon specification.
    /// </summary>
    public static MoonSpec Captured(int generationSeed)
    {
        return new MoonSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.Dwarf,
            true,
            false,
            false);
    }

    /// <summary>
    /// Returns whether a size category was specified.
    /// </summary>
    public bool HasSizeCategory() => SizeCategory >= 0;

    /// <summary>
    /// Returns whether an atmosphere preference was specified.
    /// </summary>
    public bool HasAtmospherePreference() => !HasAtmosphere.Equals(default(Variant));

    /// <summary>
    /// Returns whether an ocean preference was specified.
    /// </summary>
    public bool HasOceanPreference() => !HasSubsurfaceOcean.Equals(default(Variant));

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "moon";
        data["size_category"] = SizeCategory;
        data["is_captured"] = IsCaptured;
        data["has_atmosphere"] = HasAtmosphere;
        data["has_subsurface_ocean"] = HasSubsurfaceOcean;
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static MoonSpec FromDictionary(Dictionary data)
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

        int sizeCategory;
        if (data.ContainsKey("size_category"))
        {
            sizeCategory = (int)data["size_category"];
        }
        else
        {
            sizeCategory = -1;
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

        bool isCaptured = data.ContainsKey("is_captured") && (bool)data["is_captured"];
        return new MoonSpec(generationSeed, sizeCategory, isCaptured, GetVariant(data, "has_atmosphere"), GetVariant(data, "has_subsurface_ocean"), nameHint, overrides);
    }

    private static Variant GetVariant(Dictionary data, string key)
    {
        if (data.ContainsKey(key))
        {
            return data[key];
        }

        return default;
    }
}
