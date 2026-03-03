namespace StarGen.Domain.Population;

/// <summary>
/// Station location-type classification.
/// </summary>
public static class StationType
{
    /// <summary>
    /// Station location types.
    /// </summary>
    public enum Type
    {
        Orbital,
        DeepSpace,
        Lagrange,
        AsteroidBelt,
    }

    /// <summary>
    /// Converts a station type to a display string.
    /// </summary>
    public static string ToStringName(Type stationType)
    {
        return stationType switch
        {
            Type.Orbital => "Orbital",
            Type.DeepSpace => "Deep Space",
            Type.Lagrange => "Lagrange Point",
            Type.AsteroidBelt => "Asteroid Belt",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a station type from a string.
    /// </summary>
    public static Type FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_").Trim();
        return normalized switch
        {
            "orbital" => Type.Orbital,
            "deep_space" => Type.DeepSpace,
            "lagrange" => Type.Lagrange,
            "lagrange_point" => Type.Lagrange,
            "asteroid_belt" => Type.AsteroidBelt,
            "belt" => Type.AsteroidBelt,
            _ => Type.Orbital,
        };
    }

    /// <summary>
    /// Returns whether the type is associated with a specific body.
    /// </summary>
    public static bool IsBodyAssociated(Type stationType)
    {
        return stationType == Type.Orbital;
    }

    /// <summary>
    /// Returns whether the type is free-floating in system space.
    /// </summary>
    public static bool IsFreeFloating(Type stationType)
    {
        return stationType == Type.DeepSpace
            || stationType == Type.Lagrange
            || stationType == Type.AsteroidBelt;
    }

    /// <summary>
    /// Returns the number of station types.
    /// </summary>
    public static int Count()
    {
        return 4;
    }
}
