namespace StarGen.Domain.Celestial;

/// <summary>
/// Enumeration helpers for celestial body types.
/// </summary>
public static class CelestialType
{
    /// <summary>
    /// Supported celestial body types.
    /// </summary>
    public enum Type
    {
        Star,
        Planet,
        Moon,
        Asteroid,
    }

    /// <summary>
    /// Returns a human-readable type name.
    /// </summary>
    public static string TypeToString(Type type)
    {
        return type switch
        {
            Type.Star => "Star",
            Type.Planet => "Planet",
            Type.Moon => "Moon",
            Type.Asteroid => "Asteroid",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a string to a celestial type.
    /// </summary>
    public static bool TryParse(string typeName, out Type type)
    {
        switch (typeName.ToLowerInvariant())
        {
            case "star":
                type = Type.Star;
                return true;
            case "planet":
                type = Type.Planet;
                return true;
            case "moon":
                type = Type.Moon;
                return true;
            case "asteroid":
                type = Type.Asteroid;
                return true;
            default:
                type = default;
                return false;
        }
    }
}
