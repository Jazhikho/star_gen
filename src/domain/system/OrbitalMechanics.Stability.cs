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
    /// Default slot-spacing policy identifier for compact multi-planet architecture generation.
    /// </summary>
    public const string CompactArchitectureSpacingPolicyId = "stargen_architecture_mass_proxy_mutual_hill_v2";

    /// <summary>
    /// Source notes used by the default slot-spacing policy.
    /// </summary>
    public const string CompactArchitectureSpacingSourceIds = "Obertas2017;Rice2023;HeEtAl2020";

    /// <summary>
    /// Alternative diagnostic engine IDs for stability hardening.
    /// </summary>
    public const string StabilityAlternativeEngineIds = "mutual_hill_spacing_proxy;amd_screen_diagnostic;dynamical_packing_diagnostic;resonance_proximity_diagnostic";

    /// <summary>
    /// Source notes used by alternative diagnostic stability screens.
    /// </summary>
    public const string StabilityAlternativeSourceIds = "Petit2018;Petit2020;Laskar2017;Tamayo2020;FangMargot2013;ObertasTamayo2023";

    /// <summary>
    /// Minimum center-to-center spacing between adjacent planets as a multiple of mutual Hill radii.
    /// </summary>
    public const double MinimumAdjacentPlanetSpacingMutualHillRadii = 10.0;

    /// <summary>
    /// Minimum spacing for giant-dominated candidate architecture scaffolds.
    /// </summary>
    public const double MinimumGiantAdjacentSpacingMutualHillRadii = 6.0;

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
    /// Calculates the mutual Hill radius for an adjacent planet pair.
    /// </summary>
    public static double CalculateMutualHillRadius(
        double innerPlanetMassKg,
        double outerPlanetMassKg,
        double starMassKg,
        double innerOrbitM,
        double outerOrbitM)
    {
        if (innerPlanetMassKg <= 0.0 || outerPlanetMassKg <= 0.0 || starMassKg <= 0.0 || innerOrbitM <= 0.0 || outerOrbitM <= 0.0)
        {
            return 0.0;
        }

        double averageOrbitM = (innerOrbitM + outerOrbitM) * 0.5;
        double combinedMassRatio = (innerPlanetMassKg + outerPlanetMassKg) / (3.0 * starMassKg);
        return averageOrbitM * System.Math.Pow(combinedMassRatio, 1.0 / 3.0);
    }

    /// <summary>
    /// Calculates adjacent-planet separation in mutual Hill radii.
    /// </summary>
    public static double CalculateSeparationInMutualHillRadii(
        double innerPlanetMassKg,
        double outerPlanetMassKg,
        double starMassKg,
        double innerOrbitM,
        double outerOrbitM)
    {
        if (outerOrbitM <= innerOrbitM)
        {
            return 0.0;
        }

        double mutualHillRadius = CalculateMutualHillRadius(
            innerPlanetMassKg,
            outerPlanetMassKg,
            starMassKg,
            innerOrbitM,
            outerOrbitM);
        if (mutualHillRadius <= 0.0)
        {
            return 0.0;
        }

        return (outerOrbitM - innerOrbitM) / mutualHillRadius;
    }

    /// <summary>
    /// Calculates the outer orbit that yields a requested adjacent-pair mutual-Hill separation.
    /// </summary>
    public static double CalculateOuterOrbitForMutualHillSeparation(
        double innerPlanetMassKg,
        double outerPlanetMassKg,
        double starMassKg,
        double innerOrbitM,
        double separationMutualHillRadii)
    {
        if (innerPlanetMassKg <= 0.0 || outerPlanetMassKg <= 0.0 || starMassKg <= 0.0 || innerOrbitM <= 0.0 || separationMutualHillRadii <= 0.0)
        {
            return 0.0;
        }

        double combinedMassRatio = (innerPlanetMassKg + outerPlanetMassKg) / (3.0 * starMassKg);
        double massScale = System.Math.Pow(combinedMassRatio, 1.0 / 3.0);
        double scaledSeparation = separationMutualHillRadii * massScale;
        if (scaledSeparation >= 2.0)
        {
            return double.PositiveInfinity;
        }

        return innerOrbitM * ((2.0 + scaledSeparation) / (2.0 - scaledSeparation));
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
    /// Estimates minimum spacing between adjacent planets using the adjacent-pair mutual Hill radius.
    /// </summary>
    /// <param name="innerPlanetMassKg">Mass of the inner planet in kilograms.</param>
    /// <param name="outerPlanetMassKg">Mass of the outer planet in kilograms.</param>
    /// <param name="starMassKg">Mass of the host star in kilograms.</param>
    /// <param name="innerOrbitM">Semi-major axis of the inner planet in meters.</param>
    /// <returns>Minimum center-to-center separation in meters.</returns>
    public static double CalculateMinimumPlanetSpacing(double innerPlanetMassKg, double outerPlanetMassKg, double starMassKg, double innerOrbitM)
    {
        return CalculateMinimumPlanetSpacing(
            innerPlanetMassKg,
            outerPlanetMassKg,
            starMassKg,
            innerOrbitM,
            MinimumAdjacentPlanetSpacingMutualHillRadii);
    }

    /// <summary>
    /// Estimates minimum spacing between adjacent planets using a requested mutual-Hill threshold.
    /// </summary>
    /// <param name="innerPlanetMassKg">Mass of the inner planet in kilograms.</param>
    /// <param name="outerPlanetMassKg">Mass of the outer planet in kilograms.</param>
    /// <param name="starMassKg">Mass of the host star in kilograms.</param>
    /// <param name="innerOrbitM">Semi-major axis of the inner planet in meters.</param>
    /// <param name="separationMutualHillRadii">Required separation in mutual Hill radii.</param>
    /// <returns>Minimum center-to-center separation in meters.</returns>
    public static double CalculateMinimumPlanetSpacing(
        double innerPlanetMassKg,
        double outerPlanetMassKg,
        double starMassKg,
        double innerOrbitM,
        double separationMutualHillRadii)
    {
        if (innerOrbitM <= 0.0 || starMassKg <= 0.0)
        {
            return 0.0;
        }

        double outerOrbitM = CalculateOuterOrbitForMutualHillSeparation(
            innerPlanetMassKg,
            outerPlanetMassKg,
            starMassKg,
            innerOrbitM,
            separationMutualHillRadii);
        if (double.IsInfinity(outerOrbitM))
        {
            return double.PositiveInfinity;
        }

        return outerOrbitM - innerOrbitM;
    }

    /// <summary>
    /// Returns whether a period ratio lies in StarGen's current compact-architecture source band.
    /// </summary>
    public static bool IsCompactArchitecturePeriodRatio(double periodRatio)
    {
        if (periodRatio <= 0.0)
        {
            return false;
        }

        return periodRatio >= 1.25 && periodRatio <= 2.0;
    }

    /// <summary>
    /// Estimates proximity to simple low-order period-ratio resonances.
    /// </summary>
    public static double CalculateResonanceProximityScore(double periodRatio)
    {
        if (periodRatio <= 0.0)
        {
            return 0.0;
        }

        double[] candidateRatios =
        {
            1.25,
            1.3333333333333333,
            1.5,
            1.6666666666666667,
            2.0,
            2.5,
            3.0,
        };
        double bestScore = 0.0;
        foreach (double candidate in candidateRatios)
        {
            double fractionalDistance = System.Math.Abs(periodRatio - candidate) / candidate;
            double score = 1.0 - System.Math.Clamp(fractionalDistance / 0.04, 0.0, 1.0);
            bestScore = System.Math.Max(bestScore, score);
        }

        return bestScore;
    }

    /// <summary>
    /// Estimates AMD-style instability risk from spacing and eccentricity.
    /// </summary>
    public static double EstimateAmdInstabilityRisk(
        double separationMutualHillRadii,
        double eccentricity,
        double massProxyEarth)
    {
        if (separationMutualHillRadii <= 0.0)
        {
            return 0.0;
        }

        double spacingRisk = System.Math.Clamp((10.0 - separationMutualHillRadii) / 6.0, 0.0, 1.0);
        double eccentricityRisk = System.Math.Clamp(eccentricity / 0.18, 0.0, 1.0);
        double massRisk = System.Math.Clamp((massProxyEarth - 5.0) / 90.0, 0.0, 1.0);
        return System.Math.Clamp((spacingRisk * 0.55) + (eccentricityRisk * 0.30) + (massRisk * 0.15), 0.0, 1.0);
    }

    /// <summary>
    /// Estimates dynamical-packing risk from period ratio and mutual-Hill spacing.
    /// </summary>
    public static double EstimateDynamicalPackingRisk(
        double periodRatio,
        double separationMutualHillRadii,
        double resonanceProximityScore)
    {
        if (periodRatio <= 0.0 || separationMutualHillRadii <= 0.0)
        {
            return 0.0;
        }

        double periodRisk = System.Math.Clamp((1.60 - periodRatio) / 0.45, 0.0, 1.0);
        double spacingRisk = System.Math.Clamp((8.0 - separationMutualHillRadii) / 5.0, 0.0, 1.0);
        double resonanceRelief = resonanceProximityScore * 0.15;
        return System.Math.Clamp((periodRisk * 0.45) + (spacingRisk * 0.45) - resonanceRelief, 0.0, 1.0);
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
