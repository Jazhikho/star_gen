using Godot;
using StarGen.Domain.Math;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Orbital-math helpers for asteroid-belt sampling and rendering.
/// </summary>
public static class BeltOrbitalMath
{
    /// <summary>
    /// One astronomical unit in meters.
    /// </summary>
    public const double AuMeters = Units.AuMeters;

    /// <summary>
    /// Converts six orbital elements to a 3D position vector.
    /// </summary>
    public static Vector3 OrbitalElementsToPosition(
        double semiMajorAxis,
        double eccentricity,
        double inclination,
        double longitudeAscendingNode,
        double argumentPeriapsis,
        double trueAnomaly)
    {
        double radius = semiMajorAxis * (1.0 - (eccentricity * eccentricity)) / (1.0 + (eccentricity * System.Math.Cos(trueAnomaly)));
        double xPf = radius * System.Math.Cos(trueAnomaly);
        double yPf = radius * System.Math.Sin(trueAnomaly);

        double cosO = System.Math.Cos(longitudeAscendingNode);
        double sinO = System.Math.Sin(longitudeAscendingNode);
        double cosW = System.Math.Cos(argumentPeriapsis);
        double sinW = System.Math.Sin(argumentPeriapsis);
        double cosI = System.Math.Cos(inclination);
        double sinI = System.Math.Sin(inclination);

        double px = (cosO * cosW) - (sinO * sinW * cosI);
        double py = (sinO * cosW) + (cosO * sinW * cosI);
        double pz = sinW * sinI;
        double qx = (-cosO * sinW) - (sinO * cosW * cosI);
        double qy = (-sinO * sinW) + (cosO * cosW * cosI);
        double qz = cosW * sinI;

        return new Vector3(
            (float)((xPf * px) + (yPf * qx)),
            (float)((xPf * pz) + (yPf * qz)),
            (float)((xPf * py) + (yPf * qy)));
    }

    /// <summary>
    /// Converts mean anomaly to true anomaly.
    /// </summary>
    public static double MeanAnomalyToTrueAnomaly(double meanAnomalyRad, double eccentricity)
    {
        double eccentricAnomaly = SolveKeplerEquation(meanAnomalyRad, eccentricity);
        return EccentricToTrueAnomaly(eccentricAnomaly, eccentricity);
    }

    /// <summary>
    /// Converts eccentric anomaly to true anomaly.
    /// </summary>
    public static double EccentricToTrueAnomaly(double eccentricAnomalyRad, double eccentricity)
    {
        if (eccentricity < 1.0e-10)
        {
            return eccentricAnomalyRad;
        }

        double halfAnomaly = eccentricAnomalyRad * 0.5;
        return 2.0 * System.Math.Atan2(
            System.Math.Sqrt(1.0 + eccentricity) * System.Math.Sin(halfAnomaly),
            System.Math.Sqrt(1.0 - eccentricity) * System.Math.Cos(halfAnomaly));
    }

    /// <summary>
    /// Solves Kepler's equation via Newton-Raphson.
    /// </summary>
    public static double SolveKeplerEquation(
        double meanAnomalyRad,
        double eccentricity,
        double tolerance = 1.0e-10,
        int maxIterations = 50)
    {
        if (eccentricity < 1.0e-10)
        {
            return meanAnomalyRad;
        }

        double eccentricAnomaly = meanAnomalyRad;
        for (int iteration = 0; iteration < maxIterations; iteration += 1)
        {
            double delta = eccentricAnomaly - (eccentricity * System.Math.Sin(eccentricAnomaly)) - meanAnomalyRad;
            double derivative = 1.0 - (eccentricity * System.Math.Cos(eccentricAnomaly));
            if (System.Math.Abs(derivative) < 1.0e-12)
            {
                break;
            }

            eccentricAnomaly -= delta / derivative;
            if (System.Math.Abs(delta) < tolerance)
            {
                break;
            }
        }

        return eccentricAnomaly;
    }
}
