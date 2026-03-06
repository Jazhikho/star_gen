using Godot.Collections;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems;

/// <summary>
/// Orbital-mechanics calculations for solar-system generation and validation.
/// Keplerian and resonance calculations. Stability in OrbitalMechanics.Stability.cs.
/// Habitable-zone and frost-line helpers in OrbitalMechanics.Habitability.cs.
/// </summary>
public static partial class OrbitalMechanics
{

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

        return 2.0 * System.Math.PI * System.Math.Sqrt(System.Math.Pow(semiMajorAxisM, 3.0) / (Units.G * centralMassKg));
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

        return System.Math.Pow((Units.G * centralMassKg * System.Math.Pow(periodS, 2.0)) / (4.0 * System.Math.Pow(System.Math.PI, 2.0)), 1.0 / 3.0);
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

        return System.Math.Sqrt(Units.G * centralMassKg / distanceM);
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

        return System.Math.Sqrt(2.0 * Units.G * bodyMassKg / bodyRadiusM);
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

        return System.Math.Sqrt(Units.G * centralMassKg / System.Math.Pow(semiMajorAxisM, 3.0));
    }

    /// <summary>
    /// Calculates binary orbital period.
    /// </summary>
    public static double CalculateBinaryPeriod(double separationM, double massAKg, double massBKg)
    {
        return CalculateOrbitalPeriod(separationM, massAKg + massBKg);
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
