namespace StarGen.Domain.Population;

/// <summary>
/// Resource types available for exploitation on a planet.
/// </summary>
public static class ResourceType
{
    /// <summary>
    /// Supported resource types.
    /// </summary>
    public enum Type
    {
        Water,
        Silicates,
        Metals,
        RareElements,
        Radioactives,
        Hydrocarbons,
        Organics,
        Volatiles,
        Crystals,
        Exotics,
    }

    /// <summary>
    /// Converts a resource type to a display string.
    /// </summary>
    public static string ToStringName(Type resource)
    {
        return resource switch
        {
            Type.Water => "Water",
            Type.Silicates => "Silicates",
            Type.Metals => "Metals",
            Type.RareElements => "Rare Elements",
            Type.Radioactives => "Radioactives",
            Type.Hydrocarbons => "Hydrocarbons",
            Type.Organics => "Organics",
            Type.Volatiles => "Volatiles",
            Type.Crystals => "Crystals",
            Type.Exotics => "Exotics",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a resource type from a string.
    /// </summary>
    public static Type FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_");
        return normalized switch
        {
            "water" => Type.Water,
            "silicates" => Type.Silicates,
            "metals" => Type.Metals,
            "rare_elements" => Type.RareElements,
            "radioactives" => Type.Radioactives,
            "hydrocarbons" => Type.Hydrocarbons,
            "organics" => Type.Organics,
            "volatiles" => Type.Volatiles,
            "crystals" => Type.Crystals,
            "exotics" => Type.Exotics,
            _ => Type.Silicates,
        };
    }

    /// <summary>
    /// Returns the number of resource types.
    /// </summary>
    public static int Count() => 10;
}
