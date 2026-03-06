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
    /// Distances scale with the square root of stellar luminosity (stellar habitable-zone scaling).
    /// </summary>
    /// <param name="zone">Orbital zone.</param>
    /// <param name="stellarLuminosityWatts">Stellar luminosity in watts.</param>
    /// <returns>Inclusive (Min, Max) distance range in meters.</returns>
    public static NumericRange GetDistanceRange(OrbitZone.Zone zone, double stellarLuminosityWatts)
    {
        (double min, double max) = GetDistanceRangeTuple(zone, stellarLuminosityWatts);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetDistanceRangeTuple(OrbitZone.Zone zone, double stellarLuminosityWatts)
    {
        double solarLuminosity = stellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
        if (solarLuminosity <= 0.0)
        {
            solarLuminosity = 1.0;
        }

        double sqrtLum = System.Math.Sqrt(solarLuminosity);

        return zone switch
        {
            OrbitZone.Zone.Hot => (0.01 * Units.AuMeters * sqrtLum, 0.95 * Units.AuMeters * sqrtLum),
            OrbitZone.Zone.Temperate => (0.95 * Units.AuMeters * sqrtLum, 2.7 * Units.AuMeters * sqrtLum),
            OrbitZone.Zone.Cold => (2.7 * Units.AuMeters * sqrtLum, 50.0 * Units.AuMeters * sqrtLum),
            _ => (Units.AuMeters, Units.AuMeters),
        };
    }

    /// <summary>
    /// Generates a random orbital distance for a zone in meters using log-uniform sampling.
    /// Log-uniform sampling avoids over-populating the outer edge of wide zones.
    /// </summary>
    /// <param name="zone">Orbital zone.</param>
    /// <param name="stellarLuminosityWatts">Stellar luminosity in watts.</param>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random orbital distance in meters.</returns>
    public static double RandomDistance(OrbitZone.Zone zone, double stellarLuminosityWatts, SeededRng rng)
    {
        (double min, double max) = GetDistanceRangeTuple(zone, stellarLuminosityWatts);
        double logMin = System.Math.Log(min);
        double logMax = System.Math.Log(max);
        double logValue = rng.RandfRange((float)logMin, (float)logMax);
        return System.Math.Exp(logValue);
    }

    /// <summary>
    /// Returns the eccentricity range for a zone.
    /// Outer zones allow higher eccentricities due to reduced tidal damping.
    /// </summary>
    /// <param name="zone">Orbital zone.</param>
    /// <returns>Inclusive (Min, Max) eccentricity range.</returns>
    public static NumericRange GetEccentricityRange(OrbitZone.Zone zone)
    {
        (double min, double max) = GetEccentricityRangeTuple(zone);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetEccentricityRangeTuple(OrbitZone.Zone zone)
    {
        return zone switch
        {
            OrbitZone.Zone.Hot => (0.0, 0.1),
            OrbitZone.Zone.Temperate => (0.0, 0.2),
            OrbitZone.Zone.Cold => (0.0, 0.4),
            _ => (0.0, 0.1),
        };
    }

    /// <summary>
    /// Generates a random eccentricity for a zone.
    /// Uses a quadratic bias toward low eccentricities, matching observed distributions.
    /// </summary>
    /// <param name="zone">Orbital zone.</param>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random eccentricity in [min, max].</returns>
    public static double RandomEccentricity(OrbitZone.Zone zone, SeededRng rng)
    {
        (double min, double max) = GetEccentricityRangeTuple(zone);
        double raw = rng.RandfRange(0.0f, 1.0f);
        double biased = raw * raw;
        return min + ((max - min) * biased);
    }

    /// <summary>
    /// Generates a random orbital inclination in degrees.
    /// Uses a quadratic bias toward low inclinations, consistent with disk formation models.
    /// </summary>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random inclination in degrees.</returns>
    public static double RandomInclination(SeededRng rng)
    {
        double raw = rng.RandfRange(0.0f, 1.0f);
        double biased = raw * raw;
        return biased * 10.0;
    }

    /// <summary>
    /// Estimates the tidal-locking timescale in years using the simplified scaling relation
    /// t ∝ (a^6 * M_body) / (M_star^2 * R_body^3).
    /// </summary>
    /// <param name="orbitalDistanceM">Orbital semi-major axis in meters.</param>
    /// <param name="bodyMassKg">Body mass in kg.</param>
    /// <param name="bodyRadiusM">Body radius in meters.</param>
    /// <param name="stellarMassKg">Host star mass in kg.</param>
    /// <returns>Tidal-locking timescale in years.</returns>
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
    /// Returns whether the body should be tidally locked given the system age.
    /// </summary>
    /// <param name="orbitalDistanceM">Orbital semi-major axis in meters.</param>
    /// <param name="bodyMassKg">Body mass in kg.</param>
    /// <param name="bodyRadiusM">Body radius in meters.</param>
    /// <param name="stellarMassKg">Host star mass in kg.</param>
    /// <param name="systemAgeYears">System age in years.</param>
    /// <returns>True when the tidal-locking timescale is less than the system age.</returns>
    public static bool IsTidallyLocked(
        double orbitalDistanceM,
        double bodyMassKg,
        double bodyRadiusM,
        double stellarMassKg,
        double systemAgeYears)
    {
        return systemAgeYears > TidalLockingTimescaleYears(orbitalDistanceM, bodyMassKg, bodyRadiusM, stellarMassKg);
    }

}
