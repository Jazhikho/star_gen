#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Systems.AsteroidFields;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for belt orbital math helpers.
/// </summary>
public static class TestBeltOrbitalMath
{
    private const double DefaultTolerance = 0.001;

    /// <summary>
    /// Circular orbit should preserve radius in XZ plane.
    /// </summary>
    public static void TestOrbitalElementsCircularFlatRadius()
    {
        double a = 3.0;
        Vector3 pos = BeltOrbitalMath.OrbitalElementsToPosition(a, 0.0, 0.0, 0.0, 0.0, System.Math.PI / 3.0);
        double horizontalRadius = System.Math.Sqrt(pos.X * pos.X + pos.Z * pos.Z);
        if (System.Math.Abs(horizontalRadius - a) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Circular orbit keeps constant radius. Expected {a}, got {horizontalRadius}");
        }
        if (System.Math.Abs(pos.Y - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Zero inclination keeps Y at zero. Expected 0.0, got {pos.Y}");
        }
    }

    /// <summary>
    /// Mean anomaly should equal true anomaly when eccentricity is zero.
    /// </summary>
    public static void TestMeanToTrueAnomalyForCircularOrbit()
    {
        double meanAnomaly = 1.2;
        double trueAnomaly = BeltOrbitalMath.MeanAnomalyToTrueAnomaly(meanAnomaly, 0.0);
        if (System.Math.Abs(trueAnomaly - meanAnomaly) > 0.0001)
        {
            throw new InvalidOperationException($"Circular orbit anomaly identity. Expected {meanAnomaly}, got {trueAnomaly}");
        }
    }
}
