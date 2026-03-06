namespace StarGen.Domain.Population;

/// <summary>
/// Station size classification based on population capacity.
/// </summary>
public static class StationClass
{
    /// <summary>
    /// Station size classes.
    /// </summary>
    public enum Class
    {
        U,
        O,
        B,
        A,
        S,
    }

    /// <summary>
    /// Maximum utility-station population.
    /// </summary>
    public const int UtilityMax = 10000;

    /// <summary>
    /// Maximum outpost population.
    /// </summary>
    public const int OutpostMax = 10000;

    /// <summary>
    /// Maximum base population.
    /// </summary>
    public const int BaseMax = 100000;

    /// <summary>
    /// Maximum anchor population.
    /// </summary>
    public const int AnchorMax = 1000000;

    /// <summary>
    /// Converts a class to a display string.
    /// </summary>
    public static string ToStringName(Class stationClass)
    {
        return stationClass switch
        {
            Class.U => "Utility",
            Class.O => "Outpost",
            Class.B => "Base",
            Class.A => "Anchor",
            Class.S => "Super",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Converts a class to its letter.
    /// </summary>
    public static string ToLetter(Class stationClass)
    {
        return stationClass switch
        {
            Class.U => "U",
            Class.O => "O",
            Class.B => "B",
            Class.A => "A",
            Class.S => "S",
            _ => "?",
        };
    }

    /// <summary>
    /// Parses a station class from a string.
    /// </summary>
    public static Class FromString(string name)
    {
        return name.ToLowerInvariant().Trim() switch
        {
            "u" => Class.U,
            "utility" => Class.U,
            "o" => Class.O,
            "outpost" => Class.O,
            "b" => Class.B,
            "base" => Class.B,
            "a" => Class.A,
            "anchor" => Class.A,
            "s" => Class.S,
            "super" => Class.S,
            _ => Class.O,
        };
    }

    /// <summary>
    /// Returns the maximum population for a class, or -1 if unlimited.
    /// </summary>
    public static int GetMaxCapacity(Class stationClass)
    {
        return stationClass switch
        {
            Class.U => UtilityMax,
            Class.O => OutpostMax,
            Class.B => BaseMax,
            Class.A => AnchorMax,
            Class.S => -1,
            _ => OutpostMax,
        };
    }

    /// <summary>
    /// Returns the minimum population threshold for a class.
    /// </summary>
    public static int GetMinCapacity(Class stationClass)
    {
        return stationClass switch
        {
            Class.A => BaseMax,
            Class.S => AnchorMax,
            _ => 0,
        };
    }

    /// <summary>
    /// Returns the appropriate class for a population.
    /// </summary>
    public static Class GetClassForPopulation(int population, bool isUtility = false)
    {
        if (population <= OutpostMax)
        {
            if (isUtility)
            {
                return Class.U;
            }

            return Class.O;
        }

        if (population <= BaseMax)
        {
            return Class.B;
        }

        if (population <= AnchorMax)
        {
            return Class.A;
        }

        return Class.S;
    }

    /// <summary>
    /// Returns whether a class uses outpost governance.
    /// </summary>
    public static bool UsesOutpostGovernment(Class stationClass)
    {
        return stationClass == Class.U || stationClass == Class.O;
    }

    /// <summary>
    /// Returns whether a class uses colony governance.
    /// </summary>
    public static bool UsesColonyGovernment(Class stationClass)
    {
        return stationClass == Class.B || stationClass == Class.A || stationClass == Class.S;
    }

    /// <summary>
    /// Returns a short description of the class.
    /// </summary>
    public static string GetDescription(Class stationClass)
    {
        return stationClass switch
        {
            Class.U => "Utility station providing basic services for passing ships",
            Class.O => "Small outpost for specific purposes like mining or research",
            Class.B => "Permanent base with established population",
            Class.A => "Major regional hub and population center",
            Class.S => "Megastructure supporting city-scale or larger population",
            _ => "Unknown station class",
        };
    }

    /// <summary>
    /// Returns the number of station classes.
    /// </summary>
    public static int Count()
    {
        return 5;
    }
}
