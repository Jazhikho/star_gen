using Godot.Collections;

namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Ring system complexity levels.
/// </summary>
public static class RingComplexity
{
    /// <summary>
    /// Complexity level enumeration.
    /// </summary>
    public enum Level
    {
        Trace,
        Simple,
        Complex,
    }

    /// <summary>
    /// Returns a human-readable level name.
    /// </summary>
    public static string ToStringName(Level level)
    {
        return level switch
        {
            Level.Trace => "Trace",
            Level.Simple => "Simple",
            Level.Complex => "Complex",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a token into a ring complexity level.
    /// </summary>
    public static bool TryParse(string levelName, out Level level)
    {
        switch (levelName.ToLowerInvariant())
        {
            case "trace":
                level = Level.Trace;
                return true;
            case "simple":
                level = Level.Simple;
                return true;
            case "complex":
                level = Level.Complex;
                return true;
            default:
                level = default;
                return false;
        }
    }

    /// <summary>
    /// Returns the expected band-count range for the level.
    /// </summary>
    public static Dictionary<string, int> GetBandCountRange(Level level)
    {
        return level switch
        {
            Level.Trace => new Dictionary<string, int> { ["min"] = 1, ["max"] = 1 },
            Level.Simple => new Dictionary<string, int> { ["min"] = 2, ["max"] = 3 },
            Level.Complex => new Dictionary<string, int> { ["min"] = 4, ["max"] = 7 },
            _ => new Dictionary<string, int> { ["min"] = 1, ["max"] = 1 },
        };
    }

    /// <summary>
    /// Returns the representative optical-depth range for the level.
    /// </summary>
    public static Dictionary<string, double> GetOpticalDepthRange(Level level)
    {
        return level switch
        {
            Level.Trace => new Dictionary<string, double> { ["min"] = 0.01, ["max"] = 0.1 },
            Level.Simple => new Dictionary<string, double> { ["min"] = 0.1, ["max"] = 0.5 },
            Level.Complex => new Dictionary<string, double> { ["min"] = 0.3, ["max"] = 2.0 },
            _ => new Dictionary<string, double> { ["min"] = 0.01, ["max"] = 0.1 },
        };
    }

    /// <summary>
    /// Returns the number of ring complexity levels.
    /// </summary>
    public static int Count() => 3;
}
