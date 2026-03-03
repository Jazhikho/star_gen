namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Asteroid compositional types based on spectral classification.
/// </summary>
public static class AsteroidType
{
    /// <summary>
    /// Supported asteroid composition types.
    /// </summary>
    public enum Type
    {
        CType,
        SType,
        MType,
    }

    /// <summary>
    /// Returns a human-readable type name.
    /// </summary>
    public static string ToStringName(Type asteroidType)
    {
        return asteroidType switch
        {
            Type.CType => "C-Type (Carbonaceous)",
            Type.SType => "S-Type (Silicaceous)",
            Type.MType => "M-Type (Metallic)",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Returns the short designation for a type.
    /// </summary>
    public static string ToShortName(Type asteroidType)
    {
        return asteroidType switch
        {
            Type.CType => "C",
            Type.SType => "S",
            Type.MType => "M",
            _ => "?",
        };
    }

    /// <summary>
    /// Parses a token into an asteroid type.
    /// </summary>
    public static bool TryParse(string typeName, out Type asteroidType)
    {
        string normalized = typeName
            .ToLowerInvariant()
            .Replace("-", "_")
            .Replace(" ", "_");

        switch (normalized)
        {
            case "c":
            case "c_type":
            case "carbonaceous":
                asteroidType = Type.CType;
                return true;
            case "s":
            case "s_type":
            case "silicaceous":
            case "stony":
                asteroidType = Type.SType;
                return true;
            case "m":
            case "m_type":
            case "metallic":
                asteroidType = Type.MType;
                return true;
            default:
                asteroidType = default;
                return false;
        }
    }

    /// <summary>
    /// Returns a representative albedo for a type.
    /// </summary>
    public static double GetTypicalAlbedo(Type asteroidType)
    {
        return asteroidType switch
        {
            Type.CType => 0.05,
            Type.SType => 0.20,
            Type.MType => 0.15,
            _ => 0.10,
        };
    }

    /// <summary>
    /// Returns the number of asteroid types.
    /// </summary>
    public static int Count() => 3;
}
