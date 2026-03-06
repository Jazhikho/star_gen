using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;

namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Orbital zones relative to the habitable zone and frost line.
/// </summary>
public static class OrbitZone
{

    /// <summary>
    /// Orbit zone enumeration.
    /// </summary>
    public enum Zone
    {
        Hot,
        Temperate,
        Cold,
    }

    /// <summary>
    /// Returns a human-readable zone name.
    /// </summary>
    public static string ToStringName(Zone zone)
    {
        return zone switch
        {
            Zone.Hot => "Hot",
            Zone.Temperate => "Temperate",
            Zone.Cold => "Cold",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a token into an orbit zone.
    /// </summary>
    public static bool TryParse(string zoneName, out Zone zone)
    {
        switch (zoneName.ToLowerInvariant())
        {
            case "hot":
                zone = Zone.Hot;
                return true;
            case "temperate":
                zone = Zone.Temperate;
                return true;
            case "cold":
                zone = Zone.Cold;
                return true;
            default:
                zone = default;
                return false;
        }
    }

    /// <summary>
    /// Determines the orbit zone from orbital distance and stellar luminosity.
    /// </summary>
    public static Zone FromOrbitalDistance(double orbitalDistanceMeters, double stellarLuminosityWatts)
    {
        if (stellarLuminosityWatts <= 0.0 || orbitalDistanceMeters <= 0.0)
        {
            return Zone.Temperate;
        }

        double solarLuminosityRatio = stellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
        double squareRootLuminosity = System.Math.Sqrt(solarLuminosityRatio);
        double habitableZoneInnerMeters = 0.95 * StarGen.Domain.Math.Units.AuMeters * squareRootLuminosity;
        double frostLineMeters = 2.7 * StarGen.Domain.Math.Units.AuMeters * squareRootLuminosity;

        if (orbitalDistanceMeters < habitableZoneInnerMeters)
        {
            return Zone.Hot;
        }

        if (orbitalDistanceMeters > frostLineMeters)
        {
            return Zone.Cold;
        }

        return Zone.Temperate;
    }

    /// <summary>
    /// Returns the number of orbit zones.
    /// </summary>
    public static int Count() => 3;
}
