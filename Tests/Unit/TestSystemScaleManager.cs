#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.App.SystemViewer;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for SystemScaleManager.
/// Tests distance conversion, body sizing, orbital position calculation, and Kepler solving.
/// </summary>
public static class TestSystemScaleManager
{
    /// <summary>
    /// Helper to create a body with physical properties.
    /// </summary>
    private static CelestialBody MakeBody(
        CelestialType.Type type,
        double radiusM,
        double massKg = 1.0e24
    )
    {
        PhysicalProps physical = new PhysicalProps(
            massKg,
            radiusM,
            86400.0,
            0.0,
            0.0,
            0.0,
            0.0
        );
        return new CelestialBody(
            "test_body",
            "Test Body",
            type,
            physical,
            null
        );
    }

    /// <summary>
    /// Helper to create a body with orbital properties.
    /// </summary>
    private static CelestialBody MakeOrbitingBody(
        double semiMajorAxisM,
        double eccentricity = 0.0,
        double meanAnomalyDeg = 0.0,
        double inclinationDeg = 0.0
    )
    {
        PhysicalProps physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0, 0.0, 0.0, 0.0, 0.0
        );
        OrbitalProps orbital = new OrbitalProps(
            semiMajorAxisM,
            eccentricity,
            inclinationDeg,
            0.0, 0.0,
            meanAnomalyDeg,
            "star_1"
        );
        CelestialBody body = new CelestialBody(
            "test_orbiter",
            "Test Orbiter",
            CelestialType.Type.Planet,
            physical,
            null
        );
        body.Orbital = orbital;
        return body;
    }

    /// <summary>
    /// Tests default scale is 1 au.
    /// </summary>
    public static void TestDefaultScaleIs1Au()
    {
        SystemScaleManager manager = new SystemScaleManager();
        DotNetNativeTestSuite.AssertFloatNear(Units.AuMeters, manager.DistanceScaleMPerUnit, 1.0,
            "Default scale should be 1 AU per unit");
    }

    /// <summary>
    /// Tests distance to units 1 au.
    /// </summary>
    public static void TestDistanceToUnits1Au()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double result = manager.DistanceToUnits(Units.AuMeters);
        DotNetNativeTestSuite.AssertFloatNear(1.0, result, 0.001,
            "1 AU should convert to 1.0 viewport units");
    }

    /// <summary>
    /// Tests distance to units 5 au.
    /// </summary>
    public static void TestDistanceToUnits5Au()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double result = manager.DistanceToUnits(5.0 * Units.AuMeters);
        DotNetNativeTestSuite.AssertFloatNear(5.0, result, 0.001,
            "5 AU should convert to 5.0 viewport units");
    }

    /// <summary>
    /// Tests units to distance round trip.
    /// </summary>
    public static void TestUnitsToDistanceRoundTrip()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double originalM = 3.5 * Units.AuMeters;
        double units = manager.DistanceToUnits(originalM);
        double backToM = manager.UnitsToDistance(units);
        DotNetNativeTestSuite.AssertFloatNear(originalM, backToM, 1.0,
            "Round trip conversion should preserve distance");
    }

    /// <summary>
    /// Tests custom scale.
    /// </summary>
    public static void TestCustomScale()
    {
        double customScale = Units.AuMeters * 0.1;
        SystemScaleManager manager = new SystemScaleManager(customScale);
        double result = manager.DistanceToUnits(Units.AuMeters);
        DotNetNativeTestSuite.AssertFloatNear(10.0, result, 0.001,
            "1 AU at 0.1 AU/unit scale should be 10 units");
    }

    /// <summary>
    /// Tests distance zero.
    /// </summary>
    public static void TestDistanceZero()
    {
        SystemScaleManager manager = new SystemScaleManager();
        DotNetNativeTestSuite.AssertFloatNear(0.0, manager.DistanceToUnits(0.0), 0.001,
            "Zero distance should convert to zero units");
    }

    /// <summary>
    /// Tests negative scale clamped.
    /// </summary>
    public static void TestNegativeScaleClamped()
    {
        SystemScaleManager manager = new SystemScaleManager(-100.0);
        if (manager.DistanceScaleMPerUnit < 1.0)
        {
            throw new InvalidOperationException("Negative scale should be clamped to at least 1.0");
        }
    }

    /// <summary>
    /// Tests star display radius in bounds.
    /// </summary>
    public static void TestStarDisplayRadiusInBounds()
    {
        SystemScaleManager manager = new SystemScaleManager();
        CelestialBody star = MakeBody(
            CelestialType.Type.Star,
            Units.SolarRadiusMeters,
            Units.SolarMassKg
        );
        double radius = manager.GetBodyDisplayRadius(star);
        if (radius < SystemScaleManager.MinBodyDisplayRadius)
        {
            throw new InvalidOperationException("Star display radius should be >= minimum");
        }
        if (radius > SystemScaleManager.MaxBodyDisplayRadius)
        {
            throw new InvalidOperationException("Star display radius should be <= maximum");
        }
    }

    /// <summary>
    /// Tests planet display radius in bounds.
    /// </summary>
    public static void TestPlanetDisplayRadiusInBounds()
    {
        SystemScaleManager manager = new SystemScaleManager();
        CelestialBody planet = MakeBody(
            CelestialType.Type.Planet,
            Units.EarthRadiusMeters,
            Units.EarthMassKg
        );
        double radius = manager.GetBodyDisplayRadius(planet);
        if (radius < SystemScaleManager.MinBodyDisplayRadius)
        {
            throw new InvalidOperationException("Planet display radius should be >= minimum");
        }
        if (radius > SystemScaleManager.MaxBodyDisplayRadius)
        {
            throw new InvalidOperationException("Planet display radius should be <= maximum");
        }
    }

    /// <summary>
    /// Tests null body returns minimum.
    /// </summary>
    public static void TestNullBodyReturnsMinimum()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double radius = manager.GetBodyDisplayRadius(null);
        DotNetNativeTestSuite.AssertFloatNear(SystemScaleManager.MinBodyDisplayRadius, radius, 0.001,
            "Null body should return minimum display radius");
    }

    /// <summary>
    /// Tests star larger than planet same radius.
    /// </summary>
    public static void TestStarLargerThanPlanetSameRadius()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double testRadius = Units.EarthRadiusMeters * 10.0;

        CelestialBody star = MakeBody(CelestialType.Type.Star, testRadius);
        CelestialBody planet = MakeBody(CelestialType.Type.Planet, testRadius);

        double starDisplay = manager.GetBodyDisplayRadius(star);
        double planetDisplay = manager.GetBodyDisplayRadius(planet);

        if (starDisplay < planetDisplay)
        {
            throw new InvalidOperationException("Star should be displayed >= planet of same physical size");
        }
    }

    /// <summary>
    /// Tests larger body larger display.
    /// </summary>
    public static void TestLargerBodyLargerDisplay()
    {
        SystemScaleManager manager = new SystemScaleManager();
        CelestialBody smallPlanet = MakeBody(
            CelestialType.Type.Planet,
            Units.EarthRadiusMeters
        );
        CelestialBody largePlanet = MakeBody(
            CelestialType.Type.Planet,
            Units.EarthRadiusMeters * 10.0
        );

        double smallDisplay = manager.GetBodyDisplayRadius(smallPlanet);
        double largeDisplay = manager.GetBodyDisplayRadius(largePlanet);

        if (largeDisplay < smallDisplay)
        {
            throw new InvalidOperationException("Larger body should have >= display radius");
        }
    }

    /// <summary>
    /// Tests circular orbit zero anomaly.
    /// </summary>
    public static void TestCircularOrbitZeroAnomaly()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters,
            0.0,
            0.0,
            0.0,
            0.0,
            0.0
        );

        double distance = pos.Length();
        DotNetNativeTestSuite.AssertFloatNear(1.0, distance, 0.01,
            "Circular orbit at 1 AU should place body at distance 1.0 units");
    }

    /// <summary>
    /// Tests circular orbit 180 anomaly.
    /// </summary>
    public static void TestCircularOrbit180Anomaly()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters,
            0.0,
            0.0, 0.0, 0.0,
            180.0
        );

        double distance = pos.Length();
        DotNetNativeTestSuite.AssertFloatNear(1.0, distance, 0.01,
            "Circular orbit at 180 deg should still be at distance 1.0 units");
    }

    /// <summary>
    /// Tests circular orbit 90 anomaly.
    /// </summary>
    public static void TestCircularOrbit90Anomaly()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters,
            0.0,
            0.0, 0.0, 0.0,
            90.0
        );

        double distance = pos.Length();
        DotNetNativeTestSuite.AssertFloatNear(1.0, distance, 0.01,
            "Circular orbit at 90 deg should be at distance 1.0 units");
    }

    /// <summary>
    /// Tests eccentric orbit periapsis vs apoapsis.
    /// </summary>
    public static void TestEccentricOrbitPeriapsisVsApoapsis()
    {
        SystemScaleManager manager = new SystemScaleManager();
        double ecc = 0.5;

        Vector3 posPeri = manager.GetOrbitalPosition(
            Units.AuMeters, ecc, 0.0, 0.0, 0.0, 0.0
        );

        Vector3 posApo = manager.GetOrbitalPosition(
            Units.AuMeters, ecc, 0.0, 0.0, 0.0, 180.0
        );

        double distPeri = posPeri.Length();
        double distApo = posApo.Length();

        if (distPeri >= distApo)
        {
            throw new InvalidOperationException($"Periapsis should be closer than apoapsis (peri={distPeri:F3}, apo={distApo:F3})");
        }
        DotNetNativeTestSuite.AssertFloatNear(0.5, distPeri, 0.05,
            "Periapsis distance should be ~0.5 AU units");
        DotNetNativeTestSuite.AssertFloatNear(1.5, distApo, 0.05,
            "Apoapsis distance should be ~1.5 AU units");
    }

    /// <summary>
    /// Tests inclined orbit has y component.
    /// </summary>
    public static void TestInclinedOrbitHasYComponent()
    {
        SystemScaleManager manager = new SystemScaleManager();

        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters, 0.0,
            45.0,
            0.0, 0.0,
            90.0
        );

        if (Math.Abs(pos.Y) <= 0.1)
        {
            throw new InvalidOperationException($"Inclined orbit should have significant Y component (got {pos.Y:F3})");
        }
    }

    /// <summary>
    /// Tests flat orbit no y component.
    /// </summary>
    public static void TestFlatOrbitNoYComponent()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters, 0.2,
            0.0,
            0.0, 0.0,
            45.0
        );

        DotNetNativeTestSuite.AssertFloatNear(0.0, pos.Y, 0.001,
            "Flat orbit should have no Y component");
    }

    /// <summary>
    /// Tests body orbital position null.
    /// </summary>
    public static void TestBodyOrbitalPositionNull()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetBodyOrbitalPosition(null);
        DotNetNativeTestSuite.AssertFloatNear(0.0, pos.Length(), 0.001,
            "Null body should return origin");
    }

    /// <summary>
    /// Tests body orbital position no orbital.
    /// </summary>
    public static void TestBodyOrbitalPositionNoOrbital()
    {
        SystemScaleManager manager = new SystemScaleManager();
        CelestialBody body = MakeBody(CelestialType.Type.Planet, Units.EarthRadiusMeters);
        Vector3 pos = manager.GetBodyOrbitalPosition(body);
        DotNetNativeTestSuite.AssertFloatNear(0.0, pos.Length(), 0.001,
            "Body without orbital props should return origin");
    }

    /// <summary>
    /// Tests body orbital position uses props.
    /// </summary>
    public static void TestBodyOrbitalPositionUsesProps()
    {
        SystemScaleManager manager = new SystemScaleManager();
        CelestialBody body = MakeOrbitingBody(Units.AuMeters, 0.0, 0.0);
        Vector3 pos = manager.GetBodyOrbitalPosition(body);

        double distance = pos.Length();
        DotNetNativeTestSuite.AssertFloatNear(1.0, distance, 0.05,
            "Body at 1 AU circular orbit should be at ~1.0 units from origin");
    }

    /// <summary>
    /// Tests orbit points count.
    /// </summary>
    public static void TestOrbitPointsCount()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.0, 0.0, 0.0, 0.0, 64
        );
        DotNetNativeTestSuite.AssertEqual(65, points.Length,
            "Should generate num_points + 1 points for closed loop");
    }

    /// <summary>
    /// Tests orbit points closed loop.
    /// </summary>
    public static void TestOrbitPointsClosedLoop()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.0, 0.0, 0.0, 0.0, 64
        );

        Vector3 first = points[0];
        Vector3 last = points[points.Length - 1];
        double gap = first.DistanceTo(last);

        if (gap >= 0.01)
        {
            throw new InvalidOperationException($"First and last orbit points should be nearly identical (gap={gap:F5})");
        }
    }

    /// <summary>
    /// Tests circular orbit points equidistant.
    /// </summary>
    public static void TestCircularOrbitPointsEquidistant()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.0, 0.0, 0.0, 0.0, 32
        );

        double expectedDist = 1.0;
        for (int i = 0; i < points.Length; i++)
        {
            double dist = points[i].Length();
            DotNetNativeTestSuite.AssertFloatNear(expectedDist, dist, 0.05,
                $"Circular orbit point {i} should be at distance 1.0 (got {dist:F3})");
        }
    }

    /// <summary>
    /// Tests eccentric orbit points vary distance.
    /// </summary>
    public static void TestEccentricOrbitPointsVaryDistance()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.5, 0.0, 0.0, 0.0, 64
        );

        double minDist = double.PositiveInfinity;
        double maxDist = 0.0;
        foreach (Vector3 point in points)
        {
            double dist = point.Length();
            minDist = Math.Min(minDist, dist);
            maxDist = Math.Max(maxDist, dist);
        }

        if (minDist >= 0.7)
        {
            throw new InvalidOperationException($"Eccentric orbit min distance should be < 0.7 (got {minDist:F3})");
        }
        if (maxDist <= 1.3)
        {
            throw new InvalidOperationException($"Eccentric orbit max distance should be > 1.3 (got {maxDist:F3})");
        }
    }

    /// <summary>
    /// Tests orbit points zero sma.
    /// </summary>
    public static void TestOrbitPointsZeroSma()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            0.0, 0.0, 0.0, 0.0, 0.0, 32
        );
        DotNetNativeTestSuite.AssertEqual(0, points.Length,
            "Zero semi-major axis should return empty points array");
    }

    /// <summary>
    /// Tests flat orbit points no y.
    /// </summary>
    public static void TestFlatOrbitPointsNoY()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.3, 0.0, 0.0, 0.0, 32
        );

        for (int i = 0; i < points.Length; i++)
        {
            DotNetNativeTestSuite.AssertFloatNear(0.0, points[i].Y, 0.001,
                $"Flat orbit point {i} should have zero Y");
        }
    }

    /// <summary>
    /// Tests inclined orbit points have y.
    /// </summary>
    public static void TestInclinedOrbitPointsHaveY()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3[] points = manager.GenerateOrbitPoints(
            Units.AuMeters, 0.0, 30.0, 0.0, 0.0, 32
        );

        bool hasNonzeroY = false;
        foreach (Vector3 point in points)
        {
            if (Math.Abs(point.Y) > 0.01)
            {
                hasNonzeroY = true;
                break;
            }
        }

        if (!hasNonzeroY)
        {
            throw new InvalidOperationException("Inclined orbit should have points with non-zero Y");
        }
    }

    /// <summary>
    /// Tests kepler circular.
    /// </summary>
    public static void TestKeplerCircular()
    {
        SystemScaleManager manager = new SystemScaleManager();
        Vector3 pos = manager.GetOrbitalPosition(
            Units.AuMeters, 0.0, 0.0, 0.0, 0.0, 90.0
        );

        double distance = pos.Length();
        DotNetNativeTestSuite.AssertFloatNear(1.0, distance, 0.01,
            "Circular orbit at 90 deg should be at distance 1.0");
    }

    /// <summary>
    /// Tests kepler high eccentricity no nan.
    /// </summary>
    public static void TestKeplerHighEccentricityNoNan()
    {
        SystemScaleManager manager = new SystemScaleManager();

        double[] eccentricities = { 0.9, 0.95, 0.99 };
        double[] anomalies = { 0.0, 45.0, 90.0, 135.0, 180.0, 270.0 };

        foreach (double ecc in eccentricities)
        {
            foreach (double anomaly in anomalies)
            {
                Vector3 pos = manager.GetOrbitalPosition(
                    Units.AuMeters, ecc, 0.0, 0.0, 0.0, anomaly
                );
                if (double.IsNaN(pos.X) || double.IsNaN(pos.Y) || double.IsNaN(pos.Z))
                {
                    throw new InvalidOperationException($"Position should not be NaN for e={ecc:F2}, M={anomaly:F1}");
                }
                if (double.IsInfinity(pos.X) || double.IsInfinity(pos.Y) || double.IsInfinity(pos.Z))
                {
                    throw new InvalidOperationException($"Position should not be INF for e={ecc:F2}, M={anomaly:F1}");
                }
            }
        }
    }

    /// <summary>
    /// Tests orbit symmetry.
    /// </summary>
    public static void TestOrbitSymmetry()
    {
        SystemScaleManager manager = new SystemScaleManager();

        Vector3 pos0 = manager.GetOrbitalPosition(
            Units.AuMeters, 0.3, 0.0, 0.0, 0.0, 0.0
        );
        Vector3 pos180 = manager.GetOrbitalPosition(
            Units.AuMeters, 0.3, 0.0, 0.0, 0.0, 180.0
        );

        double dot = pos0.Normalized().Dot(pos180.Normalized());
        if (dot >= 0.0)
        {
            throw new InvalidOperationException($"0 and 180 degree positions should be on opposite sides (dot={dot:F3})");
        }
    }

    /// <summary>
    /// Tests circular orbit positions cancel.
    /// </summary>
    public static void TestCircularOrbitPositionsCancel()
    {
        SystemScaleManager manager = new SystemScaleManager();

        Vector3 sum = Vector3.Zero;
        int numSamples = 36;
        for (int i = 0; i < numSamples; i++)
        {
            double anomaly = (double)i / (double)numSamples * 360.0;
            Vector3 pos = manager.GetOrbitalPosition(
                Units.AuMeters, 0.0, 0.0, 0.0, 0.0, anomaly
            );
            sum += pos;
        }

        Vector3 avg = sum / (float)numSamples;
        if (avg.Length() >= 0.1)
        {
            throw new InvalidOperationException($"Average position of circular orbit should be near origin (got {avg.Length():F3})");
        }
    }
}
