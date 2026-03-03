using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Lookup table for orbital zone distances and properties.
/// </summary>
public static class OrbitTable
{
    /// <summary>
    /// Returns the orbital-distance range for a zone in meters.
    /// </summary>
    public static Dictionary GetDistanceRange(OrbitZone.Zone zone, double stellarLuminosityWatts)
    {
        double solarLuminosity = stellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
        if (solarLuminosity <= 0.0)
        {
            solarLuminosity = 1.0;
        }

        double squareRootLuminosity = System.Math.Sqrt(solarLuminosity);

        return zone switch
        {
            OrbitZone.Zone.Hot => BuildRange(0.01 * Units.AuMeters * squareRootLuminosity, 0.95 * Units.AuMeters * squareRootLuminosity),
            OrbitZone.Zone.Temperate => BuildRange(0.95 * Units.AuMeters * squareRootLuminosity, 2.7 * Units.AuMeters * squareRootLuminosity),
            OrbitZone.Zone.Cold => BuildRange(2.7 * Units.AuMeters * squareRootLuminosity, 50.0 * Units.AuMeters * squareRootLuminosity),
            _ => BuildRange(Units.AuMeters, Units.AuMeters),
        };
    }

    /// <summary>
    /// Generates a random orbital distance for a zone in meters.
    /// </summary>
    public static double RandomDistance(OrbitZone.Zone zone, double stellarLuminosityWatts, SeededRng rng)
    {
        Dictionary rangeData = GetDistanceRange(zone, stellarLuminosityWatts);
        double logMin = System.Math.Log((double)rangeData["min"]);
        double logMax = System.Math.Log((double)rangeData["max"]);
        double logValue = rng.RandfRange((float)logMin, (float)logMax);
        return System.Math.Exp(logValue);
    }

    /// <summary>
    /// Returns the eccentricity range for a zone.
    /// </summary>
    public static Dictionary GetEccentricityRange(OrbitZone.Zone zone)
    {
        return zone switch
        {
            OrbitZone.Zone.Hot => BuildRange(0.0, 0.1),
            OrbitZone.Zone.Temperate => BuildRange(0.0, 0.2),
            OrbitZone.Zone.Cold => BuildRange(0.0, 0.4),
            _ => BuildRange(0.0, 0.1),
        };
    }

    /// <summary>
    /// Generates a random eccentricity for a zone.
    /// </summary>
    public static double RandomEccentricity(OrbitZone.Zone zone, SeededRng rng)
    {
        Dictionary rangeData = GetEccentricityRange(zone);
        double raw = rng.RandfRange(0.0f, 1.0f);
        double biased = raw * raw;
        return ((double)rangeData["min"]) + (((double)rangeData["max"]) - ((double)rangeData["min"])) * biased;
    }

    /// <summary>
    /// Generates a random orbital inclination in degrees.
    /// </summary>
    public static double RandomInclination(SeededRng rng)
    {
        double raw = rng.RandfRange(0.0f, 1.0f);
        double biased = raw * raw;
        return biased * 10.0;
    }

    /// <summary>
    /// Estimates the tidal-locking timescale in years.
    /// </summary>
    public static double TidalLockingTimescaleYears(
        double orbitalDistanceM,
        double bodyMassKg,
        double bodyRadiusM,
        double stellarMassKg)
    {
        if (bodyRadiusM <= 0.0 || stellarMassKg <= 0.0)
        {
            return 1.0e20;
        }

        double orbitalDistanceAu = orbitalDistanceM / Units.AuMeters;
        double bodyMassEarth = bodyMassKg / Units.EarthMassKg;
        double bodyRadiusEarth = bodyRadiusM / Units.EarthRadiusMeters;
        double stellarMassSolar = stellarMassKg / Units.SolarMassKg;
        return 1.0e10
            * System.Math.Pow(orbitalDistanceAu, 6.0)
            * bodyMassEarth
            / (System.Math.Pow(stellarMassSolar, 2.0) * System.Math.Pow(bodyRadiusEarth, 3.0));
    }

    /// <summary>
    /// Returns whether the body should be tidally locked by the given system age.
    /// </summary>
    public static bool IsTidallyLocked(
        double orbitalDistanceM,
        double bodyMassKg,
        double bodyRadiusM,
        double stellarMassKg,
        double systemAgeYears)
    {
        return systemAgeYears > TidalLockingTimescaleYears(orbitalDistanceM, bodyMassKg, bodyRadiusM, stellarMassKg);
    }

    private static Dictionary BuildRange(double min, double max)
    {
        return new Dictionary
        {
            ["min"] = min,
            ["max"] = max,
        };
    }
}
