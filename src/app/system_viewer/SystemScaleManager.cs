using Godot;
using StarGen.Domain.Celestial;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Manages scale transforms for solar-system visualization.
/// </summary>
public partial class SystemScaleManager : RefCounted
{
    /// <summary>
    /// Minimum body display radius in viewport units.
    /// </summary>
    public const float MinBodyDisplayRadius = 0.05f;

    /// <summary>
    /// Maximum body display radius in viewport units.
    /// </summary>
    public const float MaxBodyDisplayRadius = 0.5f;

    /// <summary>
    /// Star display radius multiplier.
    /// </summary>
    public const float StarDisplayMultiplier = 3.0f;

    /// <summary>
    /// Planet display radius multiplier.
    /// </summary>
    public const float PlanetDisplayMultiplier = 8.0f;

    /// <summary>
    /// Moon display radius multiplier.
    /// </summary>
    public const float MoonDisplayMultiplier = 12.0f;

    /// <summary>
    /// Asteroid display radius multiplier.
    /// </summary>
    public const float AsteroidDisplayMultiplier = 20.0f;

    /// <summary>
    /// Orbit line width in viewport units.
    /// </summary>
    public const float OrbitLineWidth = 0.01f;

    /// <summary>
    /// Distance scale in meters per viewport unit.
    /// </summary>
    public double DistanceScaleMPerUnit;

    /// <summary>
    /// Creates a scale manager with the supplied distance scale.
    /// </summary>
    public SystemScaleManager(double distanceScaleMPerUnit = StarGen.Domain.Math.Units.AuMeters)
    {
        DistanceScaleMPerUnit = System.Math.Max(distanceScaleMPerUnit, 1.0);
    }

    /// <summary>
    /// Converts a distance in meters to viewport units.
    /// </summary>
    public double DistanceToUnits(double distanceM)
    {
        return distanceM / DistanceScaleMPerUnit;
    }

    /// <summary>
    /// Converts a viewport-unit distance to meters.
    /// </summary>
    public double UnitsToDistance(double units)
    {
        return units * DistanceScaleMPerUnit;
    }

    /// <summary>
    /// Calculates the exaggerated display radius for a body.
    /// </summary>
    public double GetBodyDisplayRadius(CelestialBody? body)
    {
        if (body == null)
        {
            return MinBodyDisplayRadius;
        }

        double baseRadius = DistanceToUnits(body.Physical.RadiusM);
        double displayRadius = baseRadius * GetTypeMultiplier(body.Type);
        return Mathf.Clamp((float)displayRadius, MinBodyDisplayRadius, MaxBodyDisplayRadius);
    }

    /// <summary>
    /// Calculates a body position from orbital elements.
    /// </summary>
    public Vector3 GetOrbitalPosition(
        double semiMajorAxisM,
        double eccentricity,
        double inclinationDeg,
        double longitudeAscendingNodeDeg,
        double argumentPeriapsisDeg,
        double meanAnomalyDeg)
    {
        double meanAnomalyRad = Mathf.DegToRad((float)meanAnomalyDeg);
        double eccentricAnomaly = SolveKepler(meanAnomalyRad, eccentricity);
        double trueAnomaly = EccentricToTrueAnomaly(eccentricAnomaly, eccentricity);

        double radiusM = semiMajorAxisM * (1.0 - eccentricity * System.Math.Cos(eccentricAnomaly));
        double radiusUnits = DistanceToUnits(radiusM);

        double xOrbital = radiusUnits * System.Math.Cos(trueAnomaly);
        double zOrbital = radiusUnits * System.Math.Sin(trueAnomaly);

        double omega = Mathf.DegToRad((float)argumentPeriapsisDeg);
        double bigOmega = Mathf.DegToRad((float)longitudeAscendingNodeDeg);
        double inclination = Mathf.DegToRad((float)inclinationDeg);

        double xRot = xOrbital * System.Math.Cos(omega) - zOrbital * System.Math.Sin(omega);
        double zRot = xOrbital * System.Math.Sin(omega) + zOrbital * System.Math.Cos(omega);

        double yInclined = zRot * System.Math.Sin(inclination);
        double zInclined = zRot * System.Math.Cos(inclination);

        double xFinal = xRot * System.Math.Cos(bigOmega) - zInclined * System.Math.Sin(bigOmega);
        double zFinal = xRot * System.Math.Sin(bigOmega) + zInclined * System.Math.Cos(bigOmega);

        return new Vector3((float)xFinal, (float)yInclined, (float)zFinal);
    }

    /// <summary>
    /// Calculates a body position from its orbital component.
    /// </summary>
    public Vector3 GetBodyOrbitalPosition(CelestialBody? body)
    {
        if (body == null || !body.HasOrbital() || body.Orbital == null)
        {
            return Vector3.Zero;
        }

        return GetOrbitalPosition(
            body.Orbital.SemiMajorAxisM,
            body.Orbital.Eccentricity,
            body.Orbital.InclinationDeg,
            body.Orbital.LongitudeOfAscendingNodeDeg,
            body.Orbital.ArgumentOfPeriapsisDeg,
            body.Orbital.MeanAnomalyDeg);
    }

    /// <summary>
    /// Generates an orbital ellipse as viewport-space points.
    /// </summary>
    public Vector3[] GenerateOrbitPoints(
        double semiMajorAxisM,
        double eccentricity,
        double inclinationDeg,
        double longitudeAscendingNodeDeg,
        double argumentPeriapsisDeg,
        int numPoints = 128)
    {
        if (semiMajorAxisM <= 0.0)
        {
            return System.Array.Empty<Vector3>();
        }

        Vector3[] points = new Vector3[numPoints + 1];
        double omega = Mathf.DegToRad((float)argumentPeriapsisDeg);
        double bigOmega = Mathf.DegToRad((float)longitudeAscendingNodeDeg);
        double inclination = Mathf.DegToRad((float)inclinationDeg);
        double aUnits = DistanceToUnits(semiMajorAxisM);
        double bUnits = aUnits * System.Math.Sqrt(1.0 - (eccentricity * eccentricity));
        double centerOffset = aUnits * eccentricity;

        for (int index = 0; index <= numPoints; index++)
        {
            double angle = ((double)index / numPoints) * Mathf.Tau;
            double xOrbital = aUnits * System.Math.Cos(angle) - centerOffset;
            double zOrbital = bUnits * System.Math.Sin(angle);

            double xRot = xOrbital * System.Math.Cos(omega) - zOrbital * System.Math.Sin(omega);
            double zRot = xOrbital * System.Math.Sin(omega) + zOrbital * System.Math.Cos(omega);

            double yInclined = zRot * System.Math.Sin(inclination);
            double zInclined = zRot * System.Math.Cos(inclination);

            double xFinal = xRot * System.Math.Cos(bigOmega) - zInclined * System.Math.Sin(bigOmega);
            double zFinal = xRot * System.Math.Sin(bigOmega) + zInclined * System.Math.Cos(bigOmega);

            points[index] = new Vector3((float)xFinal, (float)yInclined, (float)zFinal);
        }

        return points;
    }

    /// <summary>
    /// Solves Kepler's equation for eccentric anomaly.
    /// </summary>
    private static double SolveKepler(double meanAnomalyRad, double eccentricity)
    {
        if (eccentricity < 1e-10)
        {
            return meanAnomalyRad;
        }

        double eccentricAnomaly;
        if (eccentricity > 0.8)
        {
            eccentricAnomaly = Mathf.Pi;
        }
        else
        {
            eccentricAnomaly = meanAnomalyRad;
        }
        for (int iteration = 0; iteration < 20; iteration++)
        {
            double delta = eccentricAnomaly - eccentricity * System.Math.Sin(eccentricAnomaly) - meanAnomalyRad;
            double derivative = 1.0 - eccentricity * System.Math.Cos(eccentricAnomaly);
            if (System.Math.Abs(derivative) < 1e-15)
            {
                break;
            }

            double correction = delta / derivative;
            eccentricAnomaly -= correction;
            if (System.Math.Abs(correction) < 1e-12)
            {
                break;
            }
        }

        return eccentricAnomaly;
    }

    /// <summary>
    /// Converts eccentric anomaly to true anomaly.
    /// </summary>
    private static double EccentricToTrueAnomaly(double eccentricAnomaly, double eccentricity)
    {
        if (eccentricity < 1e-10)
        {
            return eccentricAnomaly;
        }

        double halfE = eccentricAnomaly / 2.0;
        double numerator = System.Math.Sqrt(1.0 + eccentricity) * System.Math.Sin(halfE);
        double denominator = System.Math.Sqrt(1.0 - eccentricity) * System.Math.Cos(halfE);
        if (System.Math.Abs(denominator) < 1e-15)
        {
            return eccentricAnomaly;
        }

        return 2.0 * System.Math.Atan2(numerator, denominator);
    }

    /// <summary>
    /// Returns the display multiplier for a body type.
    /// </summary>
    private static double GetTypeMultiplier(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => StarDisplayMultiplier,
            CelestialType.Type.Planet => PlanetDisplayMultiplier,
            CelestialType.Type.Moon => MoonDisplayMultiplier,
            CelestialType.Type.Asteroid => AsteroidDisplayMultiplier,
            _ => PlanetDisplayMultiplier,
        };
    }
}
