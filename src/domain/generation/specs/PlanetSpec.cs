using Godot;
using Godot.Collections;
using OrbitZoneArchetype = StarGen.Domain.Generation.Archetypes.OrbitZone;
using RingComplexityArchetype = StarGen.Domain.Generation.Archetypes.RingComplexity;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;

namespace StarGen.Domain.Generation.Specs;

/// <summary>
/// Specification for planet generation.
/// </summary>
public partial class PlanetSpec : BaseSpec
{
    /// <summary>
    /// Size category, or -1 for random.
    /// </summary>
    public int SizeCategory;

    /// <summary>
    /// Orbit zone, or -1 for random.
    /// </summary>
    public int OrbitZone;

    /// <summary>
    /// Whether an atmosphere is required, or nil for auto.
    /// </summary>
    public Variant HasAtmosphere;

    /// <summary>
    /// Whether rings are required, or nil for auto.
    /// </summary>
    public Variant HasRings;

    /// <summary>
    /// Ring complexity, or -1 for random.
    /// </summary>
    public int RingComplexity;

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
        Dictionary? overrides = null)
        : base(generationSeed, nameHint, overrides)
    {
        SizeCategory = sizeCategory;
        OrbitZone = orbitZone;
        HasAtmosphere = hasAtmosphere;
        HasRings = hasRings;
        RingComplexity = ringComplexity;
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
        return data;
    }

    /// <summary>
    /// Rebuilds a specification from a dictionary payload.
    /// </summary>
    public static PlanetSpec FromDictionary(Dictionary data)
    {
        return new PlanetSpec(
            data.ContainsKey("generation_seed") ? (int)data["generation_seed"] : 0,
            data.ContainsKey("size_category") ? (int)data["size_category"] : -1,
            data.ContainsKey("orbit_zone") ? (int)data["orbit_zone"] : -1,
            GetVariant(data, "has_atmosphere"),
            GetVariant(data, "has_rings"),
            data.ContainsKey("ring_complexity") ? (int)data["ring_complexity"] : -1,
            data.ContainsKey("name_hint") ? (string)data["name_hint"] : string.Empty,
            data.ContainsKey("overrides") ? (Dictionary)data["overrides"] : null);
    }

    private static Variant GetVariant(Dictionary data, string key)
    {
        return data.ContainsKey(key) ? data[key] : default;
    }
}
