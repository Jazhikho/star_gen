using Godot.Collections;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems;

/// <summary>
/// Stability, formation-limit, and perturbation calculations.
/// Keplerian and Habitability helpers are in the sibling partial files.
/// </summary>
public static partial class OrbitalMechanics
{
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
    /// Based on Holman & Wiegert (1999) critical semi-major axis fit.
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
    /// Based on Holman & Wiegert (1999) critical semi-major axis fit.
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
    /// Uses the tidal truncation formula: r_J ≈ 1.70 × (M/2M_sun)^(1/3) pc
    /// (Binney &amp; Tremaine 2008 §8.2; Jiang &amp; Binney 2000).
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
    /// Scales as M^0.6 from the Andrews et al. (2010) disk-size/stellar-mass relation.
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
    /// Calculates the recommended outer stability limit (lesser of formation limit and Jacobi radius).
    /// </summary>
    public static double CalculateOuterStabilityLimitM(double stellarMassKg, double formationBaseAu = 100.0)
    {
        double formationM = CalculateFormationOuterLimitM(stellarMassKg, formationBaseAu);
        double jacobiM = CalculateJacobiRadiusM(stellarMassKg);
        return System.Math.Min(formationM, jacobiM);
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
}
