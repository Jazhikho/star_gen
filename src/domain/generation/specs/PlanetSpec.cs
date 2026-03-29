using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;
using OrbitZoneArchetype = StarGen.Domain.Generation.Archetypes.OrbitZone;
using RingComplexityArchetype = StarGen.Domain.Generation.Archetypes.RingComplexity;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Orbit-mode intent for a directly generated planet.
/// </summary>
public enum PlanetOrbitMode
{
    Auto = 0,
    Bound = 1,
    Rogue = 2,
}

/// <summary>
/// Direct composition bias for a single planet.
/// </summary>
public enum PlanetCompositionBias
{
    Auto = 0,
    Rocky = 1,
    IcyWaterRich = 2,
    GasEnvelope = 3,
}

/// <summary>
/// Direct atmosphere or envelope override for a single planet.
/// </summary>
public enum PlanetEnvelopeOverride
{
    Auto = 0,
    Thin = 1,
    Retained = 2,
    Stripped = 3,
}

/// <summary>
/// Direct volatile-richness tendency for a single planet.
/// </summary>
public enum PlanetVolatileRichness
{
    Auto = 0,
    Poor = 1,
    Moderate = 2,
    Rich = 3,
}

/// <summary>
/// Direct hydrosphere tendency for a single planet.
/// </summary>
public enum PlanetHydrosphereTendency
{
    Auto = 0,
    Dry = 1,
    Mixed = 2,
    Oceanic = 3,
}

/// <summary>
/// Direct class bias for a single planet.
/// </summary>
public enum PlanetClassBias
{
    Auto = 0,
    Rocky = 1,
    WaterRich = 2,
    SubNeptune = 3,
    GasGiant = 4,
    StrippedCore = 5,
}

/// <summary>
/// Specification for planet generation.
/// </summary>
public partial class PlanetSpec : BaseSpec
{
    /// <summary>
    /// Size category, or -1 for random.
    /// </summary>
    public int SizeCategory { get; set; }

    /// <summary>
    /// Orbit zone, or -1 for random.
    /// </summary>
    public int OrbitZone { get; set; }

    /// <summary>
    /// Whether an atmosphere is required, or nil for auto.
    /// </summary>
    public Variant HasAtmosphere { get; set; }

    /// <summary>
    /// Whether rings are required, or nil for auto.
    /// </summary>
    public Variant HasRings { get; set; }

    /// <summary>
    /// Ring complexity, or -1 for random.
    /// </summary>
    public int RingComplexity { get; set; }

    /// <summary>
    /// Whether the planet should stay bound to a star or behave like a rogue world.
    /// </summary>
    public PlanetOrbitMode OrbitMode { get; set; } = PlanetOrbitMode.Auto;

    /// <summary>
    /// Direct bias for what the planet is mostly made of.
    /// </summary>
    public PlanetCompositionBias CompositionBias { get; set; } = PlanetCompositionBias.Auto;

    /// <summary>
    /// Direct override for how much atmosphere or envelope the planet keeps.
    /// </summary>
    public PlanetEnvelopeOverride EnvelopeOverride { get; set; } = PlanetEnvelopeOverride.Auto;

    /// <summary>
    /// Direct volatile-richness tendency.
    /// </summary>
    public PlanetVolatileRichness VolatileRichness { get; set; } = PlanetVolatileRichness.Auto;

    /// <summary>
    /// Direct hydrosphere tendency.
    /// </summary>
    public PlanetHydrosphereTendency HydrosphereTendency { get; set; } = PlanetHydrosphereTendency.Auto;

    /// <summary>
    /// Direct broad class bias.
    /// </summary>
    public PlanetClassBias ClassBias { get; set; } = PlanetClassBias.Auto;

    /// <summary>
    /// Whether moon generation should be attempted for this single planet.
    /// </summary>
    public bool GenerateMoonBundle { get; set; }

    /// <summary>
    /// Requested target moon count for object-level generation.
    /// </summary>
    public int TargetMoonCount { get; set; }

    /// <summary>
    /// Whether captured moons should be favored for object-level generation.
    /// </summary>
    public bool PreferCapturedMoons { get; set; }

    /// <summary>
    /// Derived formation trace used for provenance and later inspection.
    /// </summary>
    public Dictionary FormationTrace { get; set; } = new();

    /// <summary>
    /// Creates a new planet specification.
    /// </summary>
    public PlanetSpec(
        int generationSeed = 0,
        int sizeCategory = -1,
        int orbitZone = -1,
        Variant hasAtmosphere = default,
        Variant hasRings = default,
        int ringComplexity = -1,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : base(generationSeed, nameHint, overrides, useCaseSettings)
    {
        SizeCategory = sizeCategory;
        OrbitZone = orbitZone;
        HasAtmosphere = hasAtmosphere;
        HasRings = hasRings;
        RingComplexity = ringComplexity;
    }

    /// <summary>
    /// Compatibility constructor accepting enum-typed categories.
    /// </summary>
    public PlanetSpec(
        int generationSeed,
        SizeCategoryArchetype.Category sizeCategory,
        OrbitZoneArchetype.Zone orbitZone,
        Variant hasAtmosphere = default,
        Variant hasRings = default,
        RingComplexityArchetype.Level ringComplexity = (RingComplexityArchetype.Level)(-1),
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : this(
            generationSeed,
            (int)sizeCategory,
            (int)orbitZone,
            hasAtmosphere,
            hasRings,
            (int)ringComplexity,
            nameHint,
            overrides,
            useCaseSettings)
    {
    }

    /// <summary>
    /// Compatibility constructor accepting enum categories and integer ring-complexity values.
    /// </summary>
    public PlanetSpec(
        int generationSeed,
        SizeCategoryArchetype.Category sizeCategory,
        OrbitZoneArchetype.Zone orbitZone,
        Variant hasAtmosphere,
        Variant hasRings,
        int ringComplexity,
        string nameHint = "",
        Dictionary? overrides = null,
        GenerationUseCaseSettings? useCaseSettings = null)
        : this(
            generationSeed,
            (int)sizeCategory,
            (int)orbitZone,
            hasAtmosphere,
            hasRings,
            ringComplexity,
            nameHint,
            overrides,
            useCaseSettings)
    {
    }

    /// <summary>
    /// Creates a fully random planet specification.
    /// </summary>
    public static PlanetSpec Random(int generationSeed) => new(generationSeed);

    /// <summary>
    /// Creates an Earth-like planet specification.
    /// </summary>
    public static PlanetSpec EarthLike(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.Terrestrial,
            (int)OrbitZoneArchetype.Zone.Temperate,
            true,
            false);
    }

    /// <summary>
    /// Creates a hot-Jupiter specification.
    /// </summary>
    public static PlanetSpec HotJupiter(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.GasGiant,
            (int)OrbitZoneArchetype.Zone.Hot,
            true,
            false);
    }

    /// <summary>
    /// Creates a cold-gas-giant specification.
    /// </summary>
    public static PlanetSpec ColdGiant(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.GasGiant,
            (int)OrbitZoneArchetype.Zone.Cold,
            true);
    }

    /// <summary>
    /// Creates a Mars-like planet specification.
    /// </summary>
    public static PlanetSpec MarsLike(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.SubTerrestrial,
            (int)OrbitZoneArchetype.Zone.Cold,
            true,
            false);
    }

    /// <summary>
    /// Creates a dwarf-planet specification.
    /// </summary>
    public static PlanetSpec DwarfPlanet(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.Dwarf,
            (int)OrbitZoneArchetype.Zone.Cold,
            false,
            false);
    }

    /// <summary>
    /// Creates an ice-giant specification.
    /// </summary>
    public static PlanetSpec IceGiant(int generationSeed)
    {
        return new PlanetSpec(
            generationSeed,
            (int)SizeCategoryArchetype.Category.NeptuneClass,
            (int)OrbitZoneArchetype.Zone.Cold,
            true,
            true,
            (int)RingComplexityArchetype.Level.Trace);
    }

    /// <summary>
    /// Returns whether a size category was specified.
    /// </summary>
    public bool HasSizeCategory() => SizeCategory >= 0;

    /// <summary>
    /// Returns whether an orbit zone was specified.
    /// </summary>
    public bool HasOrbitZone() => OrbitZone >= 0;

    /// <summary>
    /// Returns whether the atmosphere preference was specified.
    /// </summary>
    public bool HasAtmospherePreference() => !HasAtmosphere.Equals(default(Variant));

    /// <summary>
    /// Returns whether the ring preference was specified.
    /// </summary>
    public bool HasRingPreference() => !HasRings.Equals(default(Variant));

    /// <summary>
    /// Converts this specification to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = BaseToDictionary();
        data["spec_type"] = "planet";
        data["size_category"] = SizeCategory;
        data["orbit_zone"] = OrbitZone;
        data["has_atmosphere"] = HasAtmosphere;
        data["has_rings"] = HasRings;
        data["ring_complexity"] = RingComplexity;
        data["orbit_mode"] = (int)OrbitMode;
        data["composition_bias"] = (int)CompositionBias;
        data["envelope_override"] = (int)EnvelopeOverride;
        data["volatile_richness"] = (int)VolatileRichness;
        data["hydrosphere_tendency"] = (int)HydrosphereTendency;
        data["class_bias"] = (int)ClassBias;
        data["generate_moon_bundle"] = GenerateMoonBundle;
        data["target_moon_count"] = TargetMoonCount;
        data["prefer_captured_moons"] = PreferCapturedMoons;
        data["formation_trace"] = FormationTrace.Duplicate(true);
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static PlanetSpec FromDictionary(Dictionary data)
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

        int orbitZone;
        if (data.ContainsKey("orbit_zone"))
        {
            orbitZone = (int)data["orbit_zone"];
        }
        else
        {
            orbitZone = -1;
        }

        int ringComplexity;
        if (data.ContainsKey("ring_complexity"))
        {
            ringComplexity = (int)data["ring_complexity"];
        }
        else
        {
            ringComplexity = -1;
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

        PlanetSpec spec = new PlanetSpec(generationSeed, sizeCategory, orbitZone, GetVariant(data, "has_atmosphere"), GetVariant(data, "has_rings"), ringComplexity, nameHint, overrides);
        spec.ApplyBaseFromDictionary(data);
        spec.OrbitMode = ReadEnum(data, "orbit_mode", PlanetOrbitMode.Auto);
        spec.CompositionBias = ReadEnum(data, "composition_bias", PlanetCompositionBias.Auto);
        spec.EnvelopeOverride = ReadEnum(data, "envelope_override", PlanetEnvelopeOverride.Auto);
        spec.VolatileRichness = ReadEnum(data, "volatile_richness", PlanetVolatileRichness.Auto);
        spec.HydrosphereTendency = ReadEnum(data, "hydrosphere_tendency", PlanetHydrosphereTendency.Auto);
        spec.ClassBias = ReadEnum(data, "class_bias", PlanetClassBias.Auto);
        if (data.ContainsKey("generate_moon_bundle"))
        {
            spec.GenerateMoonBundle = (bool)data["generate_moon_bundle"];
        }

        if (data.ContainsKey("target_moon_count"))
        {
            spec.TargetMoonCount = (int)data["target_moon_count"];
        }

        if (data.ContainsKey("prefer_captured_moons"))
        {
            spec.PreferCapturedMoons = (bool)data["prefer_captured_moons"];
        }

        if (data.ContainsKey("formation_trace") && data["formation_trace"].VariantType == Variant.Type.Dictionary)
        {
            spec.FormationTrace = ((Dictionary)data["formation_trace"]).Duplicate(true);
        }

        return spec;
    }

    private static TEnum ReadEnum<TEnum>(Dictionary data, string key, TEnum fallback)
        where TEnum : struct, System.Enum
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        int value = (int)data[key];
        if (System.Enum.IsDefined(typeof(TEnum), value))
        {
            return (TEnum)System.Enum.ToObject(typeof(TEnum), value);
        }

        return fallback;
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
