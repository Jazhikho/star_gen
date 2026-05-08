namespace StarGen.Domain.Population;

/// <summary>
/// Technology level classification for populations.
/// </summary>
public static class TechnologyLevel
{
    /// <summary>
    /// Minimum neutral core technology level.
    /// </summary>
    public const int MinCoreTechLevel = 0;

    /// <summary>
    /// Maximum neutral core technology level.
    /// </summary>
    public const int MaxCoreTechLevel = 24;

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
    /// Clamps a neutral core technology level to the supported range.
    /// </summary>
    public static int ClampCoreLevel(int coreLevel)
    {
        return System.Math.Clamp(coreLevel, MinCoreTechLevel, MaxCoreTechLevel);
    }

    /// <summary>
    /// Maps a neutral 0-24 core technology level to the legacy era label.
    /// </summary>
    public static Level CoreLevelToEra(int coreLevel)
    {
        int clamped = ClampCoreLevel(coreLevel);
        if (clamped <= 1)
        {
            return Level.StoneAge;
        }

        if (clamped == 2)
        {
            return Level.BronzeAge;
        }

        if (clamped == 3)
        {
            return Level.IronAge;
        }

        if (clamped == 4)
        {
            return Level.Classical;
        }

        if (clamped == 5)
        {
            return Level.Medieval;
        }

        if (clamped == 6)
        {
            return Level.Renaissance;
        }

        if (clamped == 7)
        {
            return Level.Industrial;
        }

        if (clamped == 8)
        {
            return Level.Atomic;
        }

        if (clamped == 9)
        {
            return Level.Information;
        }

        if (clamped <= 11)
        {
            return Level.Spacefaring;
        }

        if (clamped <= 15)
        {
            return Level.Interstellar;
        }

        return Level.Advanced;
    }

    /// <summary>
    /// Maps a legacy era to a representative neutral core technology level.
    /// </summary>
    public static int EraToRepresentativeCoreLevel(Level level)
    {
        return level switch
        {
            Level.StoneAge => 1,
            Level.BronzeAge => 2,
            Level.IronAge => 3,
            Level.Classical => 4,
            Level.Medieval => 5,
            Level.Renaissance => 6,
            Level.Industrial => 7,
            Level.Atomic => 8,
            Level.Information => 9,
            Level.Spacefaring => 10,
            Level.Interstellar => 12,
            Level.Advanced => 18,
            _ => 0,
        };
    }

    /// <summary>
    /// Maps a neutral core technology level to a Traveller/Cepheus tech level code.
    /// </summary>
    public static int CoreLevelToTravellerTechLevel(int coreLevel)
    {
        int clamped = ClampCoreLevel(coreLevel);
        if (clamped == 0)
        {
            return 0;
        }

        if (clamped == 1 || clamped == 2)
        {
            return 1;
        }

        if (clamped == 3)
        {
            return 2;
        }

        if (clamped == 4)
        {
            return 3;
        }

        if (clamped == 5)
        {
            return 4;
        }

        if (clamped == 6)
        {
            return 5;
        }

        if (clamped == 7 || clamped == 8)
        {
            return 6;
        }

        if (clamped == 9)
        {
            return 7;
        }

        if (clamped == 10)
        {
            return 8;
        }

        if (clamped == 11)
        {
            return 9;
        }

        if (clamped == 12 || clamped == 13)
        {
            return 10;
        }

        if (clamped == 14)
        {
            return 11;
        }

        if (clamped == 15 || clamped == 16)
        {
            return 12;
        }

        if (clamped == 17 || clamped == 18)
        {
            return 13;
        }

        if (clamped == 19 || clamped == 20)
        {
            return 14;
        }

        return 15;
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
