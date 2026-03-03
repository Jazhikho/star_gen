namespace StarGen.Domain.Population;

/// <summary>
/// Biome classifications for planetary surfaces.
/// </summary>
public static class BiomeType
{
    /// <summary>
    /// Supported biome types.
    /// </summary>
    public enum Type
    {
        Ocean,
        IceSheet,
        Tundra,
        Taiga,
        Forest,
        Grassland,
        Savanna,
        Jungle,
        Desert,
        Wetland,
        Mountain,
        Volcanic,
        Barren,
        Subsurface,
        GasGiant,
    }

    /// <summary>
    /// Integer key for the gas-giant biome.
    /// </summary>
    public const int GasGiantBiomeKey = (int)Type.GasGiant;

    /// <summary>
    /// Converts a biome to a display string.
    /// </summary>
    public static string ToStringName(Type biome)
    {
        return biome switch
        {
            Type.Ocean => "Ocean",
            Type.IceSheet => "Ice Sheet",
            Type.Tundra => "Tundra",
            Type.Taiga => "Taiga",
            Type.Forest => "Forest",
            Type.Grassland => "Grassland",
            Type.Savanna => "Savanna",
            Type.Jungle => "Jungle",
            Type.Desert => "Desert",
            Type.Wetland => "Wetland",
            Type.Mountain => "Mountain",
            Type.Volcanic => "Volcanic",
            Type.Barren => "Barren",
            Type.Subsurface => "Subsurface",
            Type.GasGiant => "Gas Giant",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a biome from a string.
    /// </summary>
    public static Type FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_");
        return normalized switch
        {
            "ocean" => Type.Ocean,
            "ice_sheet" => Type.IceSheet,
            "tundra" => Type.Tundra,
            "taiga" => Type.Taiga,
            "forest" => Type.Forest,
            "grassland" => Type.Grassland,
            "savanna" => Type.Savanna,
            "jungle" => Type.Jungle,
            "desert" => Type.Desert,
            "wetland" => Type.Wetland,
            "mountain" => Type.Mountain,
            "volcanic" => Type.Volcanic,
            "barren" => Type.Barren,
            "subsurface" => Type.Subsurface,
            "gas_giant" => Type.GasGiant,
            _ => Type.Barren,
        };
    }

    /// <summary>
    /// Returns whether the biome can support surface life.
    /// </summary>
    public static bool CanSupportLife(Type biome)
    {
        return biome is not Type.Barren
            and not Type.Volcanic
            and not Type.IceSheet
            and not Type.GasGiant;
    }

    /// <summary>
    /// Returns the number of biome types.
    /// </summary>
    public static int Count() => 15;
}
