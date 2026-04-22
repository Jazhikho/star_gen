using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
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
        return CalculateHabitableZoneInner(
            luminosityWatts,
            effectiveTempK,
            PlanetHabitableZoneModel.Kopparapu2013Conservative);
    }

    /// <summary>
    /// Calculates the inner edge of the habitable zone for a selected academic model family.
    /// </summary>
    public static double CalculateHabitableZoneInner(
        double luminosityWatts,
        double effectiveTempK,
        PlanetHabitableZoneModel habitableZoneModel)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        if (habitableZoneModel == PlanetHabitableZoneModel.Kasting1993Conservative)
        {
            return CalculateKasting1993Boundary(luminosityWatts, 0.95);
        }

        if (habitableZoneModel == PlanetHabitableZoneModel.Kopparapu2013Optimistic)
        {
            return CalculateKopparapuBoundary(
                luminosityWatts,
                effectiveTempK,
                1.7763,
                1.4335e-4,
                3.3954e-9,
                -7.6364e-12,
                -1.1950e-15);
        }

        return CalculateKopparapuBoundary(
            luminosityWatts,
            effectiveTempK,
            1.0140,
            8.1774e-5,
            1.7063e-9,
            -4.3241e-12,
            -6.6462e-16);
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
        return CalculateHabitableZoneOuter(
            luminosityWatts,
            effectiveTempK,
            PlanetHabitableZoneModel.Kopparapu2013Conservative);
    }

    /// <summary>
    /// Calculates the outer edge of the habitable zone for a selected academic model family.
    /// </summary>
    public static double CalculateHabitableZoneOuter(
        double luminosityWatts,
        double effectiveTempK,
        PlanetHabitableZoneModel habitableZoneModel)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        if (habitableZoneModel == PlanetHabitableZoneModel.Kasting1993Conservative)
        {
            return CalculateKasting1993Boundary(luminosityWatts, 1.37);
        }

        if (habitableZoneModel == PlanetHabitableZoneModel.Kopparapu2013Optimistic)
        {
            return CalculateKopparapuBoundary(
                luminosityWatts,
                effectiveTempK,
                0.3207,
                5.4471e-5,
                1.5275e-9,
                -2.1709e-12,
                -3.8282e-16);
        }

        return CalculateKopparapuBoundary(
            luminosityWatts,
            effectiveTempK,
            0.3438,
            5.8942e-5,
            1.6558e-9,
            -3.0045e-12,
            -5.2983e-16);
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

    private static double CalculateKasting1993Boundary(double luminosityWatts, double solarDistanceAu)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double luminositySolar = luminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = solarDistanceAu * System.Math.Sqrt(luminositySolar);
        return distanceAu * Units.AuMeters;
    }

    private static double CalculateKopparapuBoundary(
        double luminosityWatts,
        double effectiveTempK,
        double solarFluxAt5780K,
        double coefficientA,
        double coefficientB,
        double coefficientC,
        double coefficientD)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double temperatureForModelK = effectiveTempK;
        if (temperatureForModelK <= 0.0)
        {
            temperatureForModelK = 5780.0;
        }

        double clampedTemperatureK = System.Math.Clamp(temperatureForModelK, 2600.0, 7200.0);
        double deltaT = clampedTemperatureK - 5780.0;
        double deltaTSquared = deltaT * deltaT;
        double deltaTCubed = deltaTSquared * deltaT;
        double deltaTToFourth = deltaTCubed * deltaT;
        double stellarFlux = solarFluxAt5780K
            + (coefficientA * deltaT)
            + (coefficientB * deltaTSquared)
            + (coefficientC * deltaTCubed)
            + (coefficientD * deltaTToFourth);
        stellarFlux = System.Math.Max(stellarFlux, 0.01);

        double luminositySolar = luminosityWatts / StellarProps.SolarLuminosityWatts;
        double distanceAu = System.Math.Sqrt(luminositySolar / stellarFlux);
        return distanceAu * Units.AuMeters;
    }
}
