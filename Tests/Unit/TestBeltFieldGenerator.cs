#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems.AsteroidFields;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for belt field generation.
/// </summary>
public static class TestBeltFieldGenerator
{
    private const double DefaultTolerance = 0.0001;

    /// <summary>
    /// Creates a test spec.
    /// </summary>
    private static BeltFieldSpec MakeSpec()
    {
        BeltFieldSpec spec = new BeltFieldSpec();
        spec.InnerRadiusAu = 2.0;
        spec.OuterRadiusAu = 3.2;
        spec.AsteroidCount = 120;
        spec.MaxInclinationDeg = 12.0;
        spec.MaxEccentricity = 0.2;
        spec.MinBodyRadiusKm = 0.5;
        spec.MaxBodyRadiusKm = 50.0;
        return spec;
    }

    /// <summary>
    /// Field generation should be deterministic for equal seeds.
    /// </summary>
    public static void TestGenerateFieldDeterministic()
    {
        BeltFieldSpec spec = MakeSpec();
        BeltFieldData dataA = BeltFieldGenerator.GenerateField(spec, new SeededRng(10101));
        BeltFieldData dataB = BeltFieldGenerator.GenerateField(spec, new SeededRng(10101));

        if (dataA.Asteroids.Count != dataB.Asteroids.Count)
        {
            throw new InvalidOperationException("Counts should match");
        }
        for (int i = 0; i < dataA.Asteroids.Count; i++)
        {
            BeltAsteroidData asteroidA = dataA.Asteroids[i];
            BeltAsteroidData asteroidB = dataB.Asteroids[i];
            if (System.Math.Abs(asteroidA.PositionAu.X - asteroidB.PositionAu.X) > DefaultTolerance)
            {
                throw new InvalidOperationException("X match");
            }
            if (System.Math.Abs(asteroidA.PositionAu.Y - asteroidB.PositionAu.Y) > DefaultTolerance)
            {
                throw new InvalidOperationException("Y match");
            }
            if (System.Math.Abs(asteroidA.PositionAu.Z - asteroidB.PositionAu.Z) > DefaultTolerance)
            {
                throw new InvalidOperationException("Z match");
            }
        }
    }

    /// <summary>
    /// Background asteroids should stay inside radial bounds.
    /// </summary>
    public static void TestGenerateFieldBackgroundWithinBounds()
    {
        BeltFieldSpec spec = MakeSpec();
        BeltFieldData data = BeltFieldGenerator.GenerateField(spec, new SeededRng(20202));

        foreach (BeltAsteroidData asteroid in data.GetBackgroundAsteroids())
        {
            if (asteroid.SemiMajorAxisAu < spec.InnerRadiusAu - 0.001)
            {
                throw new InvalidOperationException("SMA above inner bound");
            }
            if (asteroid.SemiMajorAxisAu > spec.OuterRadiusAu + 0.001)
            {
                throw new InvalidOperationException("SMA below outer bound");
            }
        }
    }
}
