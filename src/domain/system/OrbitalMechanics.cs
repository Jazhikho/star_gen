using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems;

/// <summary>
/// Orbital-mechanics calculations for solar-system generation and validation.
/// </summary>
public static class OrbitalMechanics
{
    /// <summary>
    /// Gravitational constant in m^3 kg^-1 s^-2.
    /// </summary>
    public const double G = 6.674e-11;

    /// <summary>
    /// Approximate stability factor for S-type orbits.
    /// </summary>
    public const double STypeCriticalRatio = 0.4;

    /// <summary>
    /// Approximate stability factor for P-type orbits.
    /// </summary>
    public const double PTypeCriticalRatio = 2.5;

    /// <summary>
    /// Calculates orbital period using Kepler's third law.
    /// </summary>
    public static double CalculateOrbitalPeriod(double semiMajorAxisM, double centralMassKg)
    {
        if (semiMajorAxisM <= 0.0 || centralMassKg <= 0.0)
        {
            return 0.0;
        }

        return 2.0 * System.Math.PI * System.Math.Sqrt(System.Math.Pow(semiMajorAxisM, 3.0) / (G * centralMassKg));
    }

    /// <summary>
    /// Calculates semi-major axis from orbital period.
    /// </summary>
    public static double CalculateSemiMajorAxis(double periodS, double centralMassKg)
    {
        if (periodS <= 0.0 || centralMassKg <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow((G * centralMassKg * System.Math.Pow(periodS, 2.0)) / (4.0 * System.Math.Pow(System.Math.PI, 2.0)), 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates orbital velocity for a circular orbit approximation.
    /// </summary>
    public static double CalculateOrbitalVelocity(double distanceM, double centralMassKg)
    {
        if (distanceM <= 0.0 || centralMassKg <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Sqrt(G * centralMassKg / distanceM);
    }

    /// <summary>
    /// Calculates escape velocity from a body's surface.
    /// </summary>
    public static double CalculateEscapeVelocity(double bodyMassKg, double bodyRadiusM)
    {
        if (bodyMassKg <= 0.0 || bodyRadiusM <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Sqrt(2.0 * G * bodyMassKg / bodyRadiusM);
    }

    /// <summary>
    /// Calculates mean motion in radians per second.
    /// </summary>
    public static double CalculateMeanMotion(double semiMajorAxisM, double centralMassKg)
    {
        if (semiMajorAxisM <= 0.0 || centralMassKg <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Sqrt(G * centralMassKg / System.Math.Pow(semiMajorAxisM, 3.0));
    }

    /// <summary>
    /// Calculates the Hill sphere radius for a body.
    /// </summary>
    public static double CalculateHillSphere(double bodyMassKg, double primaryMassKg, double semiMajorAxisM)
    {
        if (bodyMassKg <= 0.0 || primaryMassKg <= 0.0 || semiMajorAxisM <= 0.0)
        {
            return 0.0;
        }

        double massRatio = bodyMassKg / (3.0 * primaryMassKg);
        return semiMajorAxisM * System.Math.Pow(massRatio, 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates the fluid Roche limit.
    /// </summary>
    public static double CalculateRocheLimit(double primaryRadiusM, double primaryDensityKgM3, double satelliteDensityKgM3)
    {
        if (primaryRadiusM <= 0.0 || primaryDensityKgM3 <= 0.0 || satelliteDensityKgM3 <= 0.0)
        {
            return 0.0;
        }

        return 2.44 * primaryRadiusM * System.Math.Pow(primaryDensityKgM3 / satelliteDensityKgM3, 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates the Roche limit from mass and radius.
    /// </summary>
    public static double CalculateRocheLimitFromMass(double primaryMassKg, double primaryRadiusM, double satelliteDensityKgM3)
    {
        if (primaryRadiusM <= 0.0)
        {
            return 0.0;
        }

        double parentVolume = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(primaryRadiusM, 3.0);
        double parentDensity = primaryMassKg / parentVolume;
        return CalculateRocheLimit(primaryRadiusM, parentDensity, satelliteDensityKgM3);
    }

    /// <summary>
    /// Calculates the sphere of influence radius.
    /// </summary>
    public static double CalculateSphereOfInfluence(double bodyMassKg, double primaryMassKg, double semiMajorAxisM)
    {
        if (bodyMassKg <= 0.0 || primaryMassKg <= 0.0 || semiMajorAxisM <= 0.0)
        {
            return 0.0;
        }

        return semiMajorAxisM * System.Math.Pow(bodyMassKg / primaryMassKg, 2.0 / 5.0);
    }

    /// <summary>
    /// Calculates the barycenter distance from body A in a binary pair.
    /// </summary>
    public static double CalculateBarycenterFromA(double massAKg, double massBKg, double separationM)
    {
        if (massAKg <= 0.0 || massBKg <= 0.0 || separationM <= 0.0)
        {
            return 0.0;
        }

        return separationM * massBKg / (massAKg + massBKg);
    }

    /// <summary>
    /// Calculates the maximum stable S-type orbital distance.
    /// </summary>
    public static double CalculateStypeStabilityLimit(double binarySeparationM, double massRatio, double binaryEccentricity)
    {
        if (binarySeparationM <= 0.0)
        {
            return 0.0;
        }

        double mu = massRatio / (1.0 + massRatio);
        double eccentricity = System.Math.Clamp(binaryEccentricity, 0.0, 0.99);
        double coefficient =
            0.464
            - (0.380 * mu)
            - (0.631 * eccentricity)
            + (0.586 * mu * eccentricity)
            + (0.150 * eccentricity * eccentricity)
            - (0.198 * mu * eccentricity * eccentricity);
        return coefficient * binarySeparationM * 0.9;
    }

    /// <summary>
    /// Calculates the minimum stable P-type orbital distance.
    /// </summary>
    public static double CalculatePtypeStabilityLimit(double binarySeparationM, double massRatio, double binaryEccentricity)
    {
        if (binarySeparationM <= 0.0)
        {
            return 0.0;
        }

        double mu = massRatio / (1.0 + massRatio);
        double eccentricity = System.Math.Clamp(binaryEccentricity, 0.0, 0.99);
        double coefficient =
            1.60
            + (5.10 * eccentricity)
            - (2.22 * eccentricity * eccentricity)
            + (4.12 * mu)
            - (4.27 * eccentricity * mu)
            - (5.09 * mu * mu)
            + (4.61 * eccentricity * eccentricity * mu * mu);
        return coefficient * binarySeparationM * 1.1;
    }

    /// <summary>
    /// Calculates the Jacobi radius for a primary in the solar neighborhood.
    /// </summary>
    public static double CalculateJacobiRadiusM(double stellarMassKg)
    {
        if (stellarMassKg <= 0.0)
        {
            return 0.0;
        }

        double solarMasses = stellarMassKg / Units.SolarMassKg;
        double jacobiPc = 1.70 * System.Math.Pow(solarMasses / 2.0, 1.0 / 3.0);
        return jacobiPc * Units.ParsecMeters;
    }

    /// <summary>
    /// Calculates a formation-based outer disc limit.
    /// </summary>
    public static double CalculateFormationOuterLimitM(double stellarMassKg, double baseAuAt1Solar = 100.0)
    {
        if (stellarMassKg <= 0.0)
        {
            return 0.0;
        }

        double solarMasses = stellarMassKg / Units.SolarMassKg;
        double radiusAu = baseAuAt1Solar * System.Math.Pow(solarMasses, 0.6);
        return radiusAu * Units.AuMeters;
    }

    /// <summary>
    /// Calculates the recommended outer stability limit.
    /// </summary>
    public static double CalculateOuterStabilityLimitM(double stellarMassKg, double formationBaseAu = 100.0)
    {
        double formationM = CalculateFormationOuterLimitM(stellarMassKg, formationBaseAu);
        double jacobiM = CalculateJacobiRadiusM(stellarMassKg);
        return System.Math.Min(formationM, jacobiM);
    }

    /// <summary>
    /// Calculates binary orbital period.
    /// </summary>
    public static double CalculateBinaryPeriod(double separationM, double massAKg, double massBKg)
    {
        return CalculateOrbitalPeriod(separationM, massAKg + massBKg);
    }

    /// <summary>
    /// Calculates the inner edge of the habitable zone.
    /// </summary>
    public static double CalculateHabitableZoneInner(double luminosityWatts)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double solarLuminosity = luminosityWatts / StellarProps.SolarLuminosityWatts;
        return 0.95 * Units.AuMeters * System.Math.Sqrt(solarLuminosity);
    }

    /// <summary>
    /// Calculates the outer edge of the habitable zone.
    /// </summary>
    public static double CalculateHabitableZoneOuter(double luminosityWatts)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double solarLuminosity = luminosityWatts / StellarProps.SolarLuminosityWatts;
        return 1.37 * Units.AuMeters * System.Math.Sqrt(solarLuminosity);
    }

    /// <summary>
    /// Calculates the frost-line distance.
    /// </summary>
    public static double CalculateFrostLine(double luminosityWatts)
    {
        if (luminosityWatts <= 0.0)
        {
            return 0.0;
        }

        double solarLuminosity = luminosityWatts / StellarProps.SolarLuminosityWatts;
        return 2.7 * Units.AuMeters * System.Math.Sqrt(solarLuminosity);
    }

    /// <summary>
    /// Determines the orbit zone for a distance around a star.
    /// </summary>
    public static OrbitZone.Zone GetOrbitalZone(double distanceM, double luminosityWatts)
    {
        double habitableZoneInner = CalculateHabitableZoneInner(luminosityWatts);
        double frostLine = CalculateFrostLine(luminosityWatts);

        if (distanceM < habitableZoneInner)
        {
            return OrbitZone.Zone.Hot;
        }

        if (distanceM > frostLine)
        {
            return OrbitZone.Zone.Cold;
        }

        return OrbitZone.Zone.Temperate;
    }

    /// <summary>
    /// Calculates the next orbital distance at a target resonance ratio.
    /// </summary>
    public static double CalculateResonanceSpacing(double innerOrbitM, double ratio, double variation, SeededRng rng)
    {
        if (innerOrbitM <= 0.0 || ratio <= 1.0)
        {
            return innerOrbitM;
        }

        double baseDistance = innerOrbitM * System.Math.Pow(ratio, 2.0 / 3.0);
        if (variation <= 0.0)
        {
            return baseDistance;
        }

        double variationFactor = rng.RandfRange((float)(1.0 - variation), (float)(1.0 + variation));
        return baseDistance * variationFactor;
    }

    /// <summary>
    /// Returns common orbital resonance ratios.
    /// </summary>
    public static Array<double> GetCommonResonanceRatios()
    {
        return new Array<double> { 2.0, 1.5, 1.67, 1.4, 1.6, 1.25, 1.33 };
    }

    /// <summary>
    /// Converts a period ratio to a distance ratio.
    /// </summary>
    public static double PeriodRatioToDistanceRatio(double periodRatio)
    {
        if (periodRatio <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(periodRatio, 2.0 / 3.0);
    }

    /// <summary>
    /// Converts a distance ratio to a period ratio.
    /// </summary>
    public static double DistanceRatioToPeriodRatio(double distanceRatio)
    {
        if (distanceRatio <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(distanceRatio, 1.5);
    }

    /// <summary>
    /// Estimates minimum long-term-stable spacing between adjacent planets.
    /// </summary>
    public static double CalculateMinimumPlanetSpacing(double innerPlanetMassKg, double outerPlanetMassKg, double starMassKg, double innerOrbitM)
    {
        if (innerOrbitM <= 0.0 || starMassKg <= 0.0)
        {
            return 0.0;
        }

        double combinedMass = innerPlanetMassKg + outerPlanetMassKg;
        double hillRadius = innerOrbitM * System.Math.Pow(combinedMass / (3.0 * starMassKg), 1.0 / 3.0);
        return hillRadius * 10.0;
    }

    /// <summary>
    /// Estimates whether an orbit is stable against companion perturbations.
    /// </summary>
    public static bool IsOrbitStable(
        double orbitDistanceM,
        double hostMassKg,
        double hostPositionM,
        Array<double> companionMassesKg,
        Array<double> companionPositionsM)
    {
        if (orbitDistanceM <= 0.0 || hostMassKg <= 0.0)
        {
            return false;
        }

        if (companionMassesKg.Count == 0)
        {
            return true;
        }

        int count = System.Math.Min(companionMassesKg.Count, companionPositionsM.Count);
        for (int index = 0; index < count; index += 1)
        {
            double companionMass = companionMassesKg[index];
            double companionPosition = companionPositionsM[index];
            double separation = System.Math.Abs(companionPosition - hostPositionM);
            if (separation <= 0.0)
            {
                continue;
            }

            if (orbitDistanceM > separation * 0.5)
            {
                return false;
            }

            double hillRadius = CalculateHillSphere(hostMassKg, companionMass, separation);
            if (orbitDistanceM > hillRadius * 0.5)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates approximate perturbation strength from a companion.
    /// </summary>
    public static double CalculatePerturbationStrength(double orbitDistanceM, double companionDistanceM, double companionMassKg, double hostMassKg)
    {
        if (companionDistanceM <= 0.0 || hostMassKg <= 0.0)
        {
            return 0.0;
        }

        double massRatio = companionMassKg / hostMassKg;
        if (orbitDistanceM < companionDistanceM)
        {
            double distanceRatio = orbitDistanceM / companionDistanceM;
            return massRatio * System.Math.Pow(distanceRatio, 3.0);
        }

        double outerDistanceRatio = companionDistanceM / orbitDistanceM;
        return massRatio * System.Math.Pow(outerDistanceRatio, 2.0);
    }

    /// <summary>
    /// Returns whether two eccentric orbits overlap.
    /// </summary>
    public static bool DoOrbitsOverlap(double a1M, double e1, double a2M, double e2)
    {
        double peri1 = a1M * (1.0 - e1);
        double apo1 = a1M * (1.0 + e1);
        double peri2 = a2M * (1.0 - e2);
        double apo2 = a2M * (1.0 + e2);
        return !(apo1 < peri2 || apo2 < peri1);
    }

    /// <summary>
    /// Calculates the synodic period between two orbits.
    /// </summary>
    public static double CalculateSynodicPeriod(double period1S, double period2S)
    {
        if (period1S <= 0.0 || period2S <= 0.0)
        {
            return 0.0;
        }

        double difference = System.Math.Abs(period1S - period2S);
        double maxPeriod = System.Math.Max(period1S, period2S);
        if (difference < maxPeriod * 1.0e-10)
        {
            return 1.0e20;
        }

        return System.Math.Abs(period1S * period2S / difference);
    }
}
