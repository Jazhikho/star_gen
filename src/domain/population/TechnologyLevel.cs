namespace StarGen.Domain.Population;

/// <summary>
/// Technology level classification for populations.
/// </summary>
public static class TechnologyLevel
{
    /// <summary>
    /// Technology eras and levels.
    /// </summary>
    public enum Level
    {
        StoneAge,
        BronzeAge,
        IronAge,
        Classical,
        Medieval,
        Renaissance,
        Industrial,
        Atomic,
        Information,
        Spacefaring,
        Interstellar,
        Advanced,
    }

    /// <summary>
    /// Converts a level to a display string.
    /// </summary>
    public static string ToStringName(Level level)
    {
        return level switch
        {
            Level.StoneAge => "Stone Age",
            Level.BronzeAge => "Bronze Age",
            Level.IronAge => "Iron Age",
            Level.Classical => "Classical",
            Level.Medieval => "Medieval",
            Level.Renaissance => "Renaissance",
            Level.Industrial => "Industrial",
            Level.Atomic => "Atomic Age",
            Level.Information => "Information Age",
            Level.Spacefaring => "Spacefaring",
            Level.Interstellar => "Interstellar",
            Level.Advanced => "Advanced",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a level from a string.
    /// </summary>
    public static Level FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_");
        return normalized switch
        {
            "stone_age" => Level.StoneAge,
            "bronze_age" => Level.BronzeAge,
            "iron_age" => Level.IronAge,
            "classical" => Level.Classical,
            "medieval" => Level.Medieval,
            "renaissance" => Level.Renaissance,
            "industrial" => Level.Industrial,
            "atomic" => Level.Atomic,
            "atomic_age" => Level.Atomic,
            "information" => Level.Information,
            "information_age" => Level.Information,
            "spacefaring" => Level.Spacefaring,
            "interstellar" => Level.Interstellar,
            "advanced" => Level.Advanced,
            _ => Level.StoneAge,
        };
    }

    /// <summary>
    /// Returns the next level or the current level at the maximum.
    /// </summary>
    public static Level NextLevel(Level level)
    {
        int nextValue = (int)level + 1;
        if (nextValue >= Count())
        {
            return level;
        }

        return (Level)nextValue;
    }

    /// <summary>
    /// Returns the previous level or the current level at the minimum.
    /// </summary>
    public static Level PreviousLevel(Level level)
    {
        int previousValue = (int)level - 1;
        if (previousValue < 0)
        {
            return level;
        }

        return (Level)previousValue;
    }

    /// <summary>
    /// Returns whether a level can achieve spaceflight.
    /// </summary>
    public static bool CanSpaceflight(Level level)
    {
        return level >= Level.Spacefaring;
    }

    /// <summary>
    /// Returns whether a level can achieve interstellar travel.
    /// </summary>
    public static bool CanInterstellar(Level level)
    {
        return level >= Level.Interstellar;
    }

    /// <summary>
    /// Returns the approximate years required to reach a level.
    /// </summary>
    public static int TypicalYearsToReach(Level level)
    {
        return level switch
        {
            Level.StoneAge => 0,
            Level.BronzeAge => 50000,
            Level.IronAge => 55000,
            Level.Classical => 57000,
            Level.Medieval => 58000,
            Level.Renaissance => 59000,
            Level.Industrial => 59500,
            Level.Atomic => 59600,
            Level.Information => 59650,
            Level.Spacefaring => 59700,
            Level.Interstellar => 60000,
            Level.Advanced => 65000,
            _ => 0,
        };
    }

    /// <summary>
    /// Returns the number of defined levels.
    /// </summary>
    public static int Count()
    {
        return 12;
    }
}
