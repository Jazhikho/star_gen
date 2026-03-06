#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for DensitySampler — determinism, distribution, and population split.
/// </summary>
public static class TestDensitySampler
{
    private static GalaxySpec _spec;

    private static void BeforeEach()
    {
        _spec = GalaxySpec.CreateMilkyWay(123);
    }

    public static void TestDeterminism()
    {
        BeforeEach();
        GalaxySample sample1 = DensitySampler.SampleGalaxy(_spec, 500, new SeededRng(999));
        GalaxySample sample2 = DensitySampler.SampleGalaxy(_spec, 500, new SeededRng(999));

        DotNetNativeTestSuite.AssertEqual(
            sample1.BulgePoints.Length, sample2.BulgePoints.Length,
            "Bulge counts must match across runs");
        DotNetNativeTestSuite.AssertEqual(
            sample1.DiskPoints.Length, sample2.DiskPoints.Length,
            "Disk counts must match across runs");

        for (int i = 0; i < Math.Min(10, sample1.BulgePoints.Length); i += 1)
        {
            DotNetNativeTestSuite.AssertTrue(
                sample1.BulgePoints[i].IsEqualApprox(sample2.BulgePoints[i]),
                $"Bulge point {i} must be identical");
        }
        for (int i = 0; i < Math.Min(10, sample1.DiskPoints.Length); i += 1)
        {
            DotNetNativeTestSuite.AssertTrue(
                sample1.DiskPoints[i].IsEqualApprox(sample2.DiskPoints[i]),
                $"Disk point {i} must be identical");
        }
    }

    public static void TestProducesRequestedCount()
    {
        BeforeEach();
        GalaxySample sample = DensitySampler.SampleGalaxy(_spec, 1000, new SeededRng(42));

        DotNetNativeTestSuite.AssertGreaterThan(sample.GetTotalCount(), 900,
            "Should produce close to requested count");
        DotNetNativeTestSuite.AssertLessThan(sample.GetTotalCount(), 1001,
            "Should not exceed requested count");
    }

    public static void TestHasBothPopulations()
    {
        BeforeEach();
        GalaxySample sample = DensitySampler.SampleGalaxy(_spec, 2000, new SeededRng(77));

        DotNetNativeTestSuite.AssertGreaterThan(sample.BulgePoints.Length, 0, "Should have bulge points");
        DotNetNativeTestSuite.AssertGreaterThan(sample.DiskPoints.Length, 0, "Should have disk points");
    }

    public static void TestBulgePointsNearCenter()
    {
        BeforeEach();
        GalaxySample sample = DensitySampler.SampleGalaxy(_spec, 2000, new SeededRng(55));

        float maxR = (float)_spec.BulgeRadiusPc * 4.0f;
        int outliers = 0;
        for (int i = 0; i < sample.BulgePoints.Length; i += 1)
        {
            Vector3 p = sample.BulgePoints[i];
            float r = Mathf.Sqrt(p.X * p.X + p.Z * p.Z);
            if (r > maxR)
            {
                outliers += 1;
            }
        }

        float outlierFrac = outliers / (float)sample.BulgePoints.Length;
        DotNetNativeTestSuite.AssertLessThan(outlierFrac, 0.01f, "Very few bulge stars should be far from center");
    }

    public static void TestDiskPointsWithinGalaxyBounds()
    {
        BeforeEach();
        GalaxySample sample = DensitySampler.SampleGalaxy(_spec, 2000, new SeededRng(66));

        for (int i = 0; i < sample.DiskPoints.Length; i += 1)
        {
            Vector3 p = sample.DiskPoints[i];
            float r = Mathf.Sqrt(p.X * p.X + p.Z * p.Z);
            DotNetNativeTestSuite.AssertLessThan(r, (float)_spec.RadiusPc + 1.0f,
                $"Disk point {i} radius must be within galaxy radius");
            DotNetNativeTestSuite.AssertLessThan(Mathf.Abs(p.Y), (float)_spec.HeightPc + 1.0f,
                $"Disk point {i} height must be within galaxy height");
        }
    }

    public static void TestDifferentSeedDifferentResult()
    {
        BeforeEach();
        GalaxySample sample1 = DensitySampler.SampleGalaxy(_spec, 500, new SeededRng(1));
        GalaxySample sample2 = DensitySampler.SampleGalaxy(_spec, 500, new SeededRng(2));

        bool anyDifferent = false;
        int checkCount = Math.Min(sample1.BulgePoints.Length, sample2.BulgePoints.Length);
        for (int i = 0; i < Math.Min(10, checkCount); i += 1)
        {
            if (!sample1.BulgePoints[i].IsEqualApprox(sample2.BulgePoints[i]))
            {
                anyDifferent = true;
                break;
            }
        }
        DotNetNativeTestSuite.AssertTrue(anyDifferent, "Different seeds should produce different galaxies");
    }

    public static void TestEllipticalGalaxyNoDisk()
    {
        GalaxySpec ellipticalSpec = new GalaxySpec
        {
            GalaxySeed = 100,
            Type = GalaxySpec.GalaxyType.Elliptical,
            BulgeIntensity = 1.0,
            BulgeRadiusPc = 2000.0,
            RadiusPc = 15000.0,
            Ellipticity = 0.4,
        };

        GalaxySample sample = DensitySampler.SampleGalaxy(ellipticalSpec, 1000, new SeededRng(100));

        DotNetNativeTestSuite.AssertGreaterThan(sample.BulgePoints.Length, 0, "Elliptical should have bulge points");
        DotNetNativeTestSuite.AssertEqual(sample.DiskPoints.Length, 0, "Elliptical should have no disk points");
    }

    public static void TestEllipticalGalaxyIs3dNotFlat()
    {
        GalaxySpec ellipticalSpec = new GalaxySpec
        {
            GalaxySeed = 300,
            Type = GalaxySpec.GalaxyType.Elliptical,
            BulgeIntensity = 1.0,
            BulgeRadiusPc = 2000.0,
            RadiusPc = 15000.0,
            Ellipticity = 0.3,
        };

        GalaxySample sample = DensitySampler.SampleGalaxy(ellipticalSpec, 2000, new SeededRng(300));

        float yAbsSum = 0.0f;
        for (int i = 0; i < sample.BulgePoints.Length; i += 1)
        {
            yAbsSum += Mathf.Abs(sample.BulgePoints[i].Y);
        }

        float avgYAbs = yAbsSum / sample.BulgePoints.Length;
        DotNetNativeTestSuite.AssertGreaterThan(avgYAbs, 500.0f,
            "Elliptical galaxy should have 3D distribution, not flat disk");
    }

    public static void TestIrregularGalaxyIs3dNotFlat()
    {
        GalaxySpec irregularSpec = new GalaxySpec
        {
            GalaxySeed = 400,
            Type = GalaxySpec.GalaxyType.Irregular,
            BulgeIntensity = 0.8,
            RadiusPc = 10000.0,
            IrregularityScale = 0.5,
        };

        GalaxySample sample = DensitySampler.SampleGalaxy(irregularSpec, 2000, new SeededRng(400));

        float yAbsSum = 0.0f;
        int total = 0;
        for (int i = 0; i < sample.BulgePoints.Length; i += 1)
        {
            yAbsSum += Mathf.Abs(sample.BulgePoints[i].Y);
            total += 1;
        }
        for (int i = 0; i < sample.DiskPoints.Length; i += 1)
        {
            yAbsSum += Mathf.Abs(sample.DiskPoints[i].Y);
            total += 1;
        }

        float avgYAbs = yAbsSum / total;
        DotNetNativeTestSuite.AssertGreaterThan(avgYAbs, 500.0f,
            "Irregular galaxy should have 3D distribution, not flat disk");
    }

    public static void TestIrregularGalaxyHasBothPopulations()
    {
        GalaxySpec irregularSpec = new GalaxySpec
        {
            GalaxySeed = 200,
            Type = GalaxySpec.GalaxyType.Irregular,
            BulgeIntensity = 0.5,
            BulgeRadiusPc = 1500.0,
            DiskScaleLengthPc = 3000.0,
            DiskScaleHeightPc = 400.0,
            RadiusPc = 12000.0,
            IrregularityScale = 0.6,
        };

        GalaxySample sample = DensitySampler.SampleGalaxy(irregularSpec, 1000, new SeededRng(200));

        DotNetNativeTestSuite.AssertGreaterThan(sample.BulgePoints.Length, 0, "Irregular should have bulge points");
        DotNetNativeTestSuite.AssertGreaterThan(sample.DiskPoints.Length, 0, "Irregular should have disk points");
    }
}
