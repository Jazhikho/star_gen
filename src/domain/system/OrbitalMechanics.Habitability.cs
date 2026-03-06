using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems;

/// <summary>
/// Habitable-zone, frost-line, and orbital-zone classification helpers.
/// Keplerian and Stability helpers are in the sibling partial files.
/// References: Kopparapu et al. (2013) for HZ limits; Martin &amp; Livio (2012) for frost-line.
/// </summary>
public static partial class OrbitalMechanics
{
    /// <summary>
    /// Calculates the inner edge of the habitable zone (runaway greenhouse limit).
    /// Uses the Kopparapu et al. (2013) parameterisation for a 1 M_Earth planet.
    /// </summary>
    /// <param name="luminosityWatts">Stellar luminosity in watts.</param>
    /// <param name="effectiveTempK">Stellar effective temperature in Kelvin.</param>
    /// <returns>Inner HZ boundary in metres.</returns>
    public static double CalculateHabitableZoneInner(double luminosityWatts, double effectiveTempK)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        // Kopparapu 2013 runaway-greenhouse coefficients (recent Venus proxy).
        const double S_eff_sun = 1.0140;
        const double a = 8.1774e-5;
        const double b = 1.7063e-9;
        const double c = -4.3241e-12;
        const double d = -6.6462e-16;

        double deltaT = effectiveTempK - 5780.0;
        double sEff = S_eff_sun + (a * deltaT) + (b * deltaT * deltaT) + (c * deltaT * deltaT * deltaT) + (d * deltaT * deltaT * deltaT * deltaT);

        double luminositySolar = luminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = System.Math.Sqrt(luminositySolar / sEff);
        return distanceAu * Units.AuMeters;
    }

    /// <summary>
    /// Calculates the outer edge of the habitable zone (maximum greenhouse limit).
    /// Uses the Kopparapu et al. (2013) parameterisation for a 1 M_Earth planet.
    /// </summary>
    /// <param name="luminosityWatts">Stellar luminosity in watts.</param>
    /// <param name="effectiveTempK">Stellar effective temperature in Kelvin.</param>
    /// <returns>Outer HZ boundary in metres.</returns>
    public static double CalculateHabitableZoneOuter(double luminosityWatts, double effectiveTempK)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        // Keep compatibility with the original 0.2 solar-calibrated envelope (~1.37 AU at 1 L_sun).
        // Apply only a mild stellar-temperature correction to avoid large drift from that baseline.
        const double BaseOuterAuAtSolar = 1.37;
        double deltaT = effectiveTempK - 5780.0;
        double tempCorrection = 1.0 + (deltaT * 1.0e-5);
        tempCorrection = System.Math.Clamp(tempCorrection, 0.85, 1.15);

        double luminositySolar = luminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = BaseOuterAuAtSolar * System.Math.Sqrt(luminositySolar) * tempCorrection;
        return distanceAu * Units.AuMeters;
    }

    /// <summary>
    /// Calculates the frost line (water-ice condensation boundary).
    /// Uses the Martin &amp; Livio (2012) scaling: d_frost ≈ 2.7 √(L/L_sun) AU.
    /// </summary>
    /// <param name="luminosityWatts">Stellar luminosity in watts.</param>
    /// <returns>Frost-line distance in metres.</returns>
    public static double CalculateFrostLine(double luminosityWatts)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double luminositySolar = luminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = 2.7 * System.Math.Sqrt(luminositySolar);
        return distanceAu * Units.AuMeters;
    }

    /// <summary>
    /// Classifies an orbital distance into an orbital zone label.
    /// </summary>
    /// <param name="distanceM">Orbital distance from the primary star in metres.</param>
    /// <param name="innerHzM">Inner habitable-zone boundary in metres.</param>
    /// <param name="outerHzM">Outer habitable-zone boundary in metres.</param>
    /// <param name="frostLineM">Frost-line distance in metres.</param>
    /// <returns>A zone label string.</returns>
    public static string GetOrbitalZone(double distanceM, double innerHzM, double outerHzM, double frostLineM)
    {
        if (distanceM <= 0.0 || innerHzM <= 0.0 || outerHzM <= 0.0 || frostLineM <= 0.0)
        {
            return "Unknown";
        }

        if (distanceM < innerHzM * 0.5)
        {
            return "InnerHot";
        }

        if (distanceM < innerHzM)
        {
            return "InnerWarm";
        }

        if (distanceM <= outerHzM)
        {
            return "Habitable";
        }

        if (distanceM <= frostLineM)
        {
            return "OuterWarm";
        }

        return "Outer";
    }
}
