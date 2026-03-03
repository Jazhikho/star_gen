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
    public int SizeCategory;

    /// <summary>
    /// Whether this moon is captured.
    /// </summary>
    public bool IsCaptured;

    /// <summary>
    /// Whether an atmosphere is required, or nil for auto.
    /// </summary>
    public Variant HasAtmosphere;

    /// <summary>
    /// Whether a subsurface ocean is required, or nil for auto.
    /// </summary>
    public Variant HasSubsurfaceOcean;

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
        return new MoonSpec(
            data.ContainsKey("generation_seed") ? (int)data["generation_seed"] : 0,
            data.ContainsKey("size_category") ? (int)data["size_category"] : -1,
            data.ContainsKey("is_captured") && (bool)data["is_captured"],
            GetVariant(data, "has_atmosphere"),
            GetVariant(data, "has_subsurface_ocean"),
            data.ContainsKey("name_hint") ? (string)data["name_hint"] : string.Empty,
            data.ContainsKey("overrides") ? (Dictionary)data["overrides"] : null);
    }

    private static Variant GetVariant(Dictionary data, string key)
    {
        return data.ContainsKey(key) ? data[key] : default;
    }
}
