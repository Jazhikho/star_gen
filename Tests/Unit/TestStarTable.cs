#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for StarTable lookups and calculations.
/// </summary>
public static class TestStarTable
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests mass range retrieval.
    /// </summary>
    public static void TestMassRanges()
    {
        (double Min, double Max) gRange = StarTable.GetMassRange(StarClass.SpectralClass.G);
        double min = gRange.Min;
        double max = gRange.Max;
        if (1.0 < min || 1.0 > max)
        {
            throw new InvalidOperationException($"Sun should fit in G range [{min}, {max}]");
        }

        (double Min, double Max) mRange = StarTable.GetMassRange(StarClass.SpectralClass.M);
        if (mRange.Max >= gRange.Min)
        {
            throw new InvalidOperationException("M stars smaller than G");
        }
    }

    /// <summary>
    /// Tests luminosity from mass relationship.
    /// </summary>
    public static void TestLuminosityFromMass()
    {
        double lSun = StarTable.LuminosityFromMass(1.0);
        if (System.Math.Abs(lSun - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity 1.0 for 1 solar mass, got {lSun}");
        }

        double l2 = StarTable.LuminosityFromMass(2.0);
        if (l2 < 10.0 || l2 > 12.0)
        {
            throw new InvalidOperationException($"Expected luminosity in range [10.0, 12.0] for 2 solar masses, got {l2}");
        }
    }

    /// <summary>
    /// Tests radius from mass relationship.
    /// </summary>
    public static void TestRadiusFromMass()
    {
        double rSun = StarTable.RadiusFromMass(1.0);
        if (System.Math.Abs(rSun - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected radius 1.0 for 1 solar mass, got {rSun}");
        }
    }

    /// <summary>
    /// Tests temperature calculation.
    /// </summary>
    public static void TestTemperatureFromLuminosityRadius()
    {
        double tSun = StarTable.TemperatureFromLuminosityRadius(1.0, 1.0);
        if (tSun < 5700.0 || tSun > 5850.0)
        {
            throw new InvalidOperationException($"Expected temperature in range [5700.0, 5850.0], got {tSun}");
        }
    }

    /// <summary>
    /// Tests class determination from temperature.
    /// </summary>
    public static void TestClassFromTemperature()
    {
        if (StarTable.ClassFromTemperature(5778.0) != StarClass.SpectralClass.G)
        {
            throw new InvalidOperationException($"Expected G for 5778 K, got {StarTable.ClassFromTemperature(5778.0)}");
        }
        if (StarTable.ClassFromTemperature(3500.0) != StarClass.SpectralClass.M)
        {
            throw new InvalidOperationException($"Expected M for 3500 K, got {StarTable.ClassFromTemperature(3500.0)}");
        }
        if (StarTable.ClassFromTemperature(10000.0) != StarClass.SpectralClass.A)
        {
            throw new InvalidOperationException($"Expected A for 10000 K, got {StarTable.ClassFromTemperature(10000.0)}");
        }
        if (StarTable.ClassFromTemperature(35000.0) != StarClass.SpectralClass.O)
        {
            throw new InvalidOperationException($"Expected O for 35000 K, got {StarTable.ClassFromTemperature(35000.0)}");
        }
    }

    /// <summary>
    /// Tests subclass interpolation.
    /// </summary>
    public static void TestSubclassInterpolation()
    {
        (double Min, double Max) tempRange = StarTable.GetTemperatureRange(StarClass.SpectralClass.G);

        double t0 = StarTable.InterpolateBySubclass(StarClass.SpectralClass.G, 0, tempRange);
        if (System.Math.Abs(t0 - tempRange.Max) > 1.0)
        {
            throw new InvalidOperationException($"Expected subclass 0 near max {tempRange.Max}, got {t0}");
        }

        double t9 = StarTable.InterpolateBySubclass(StarClass.SpectralClass.G, 9, tempRange);
        if (System.Math.Abs(t9 - tempRange.Min) > 1.0)
        {
            throw new InvalidOperationException($"Expected subclass 9 near min {tempRange.Min}, got {t9}");
        }
    }

    /// <summary>
    /// Tests lifetime ranges are physically reasonable.
    /// </summary>
    public static void TestLifetimeRanges()
    {
        (double Min, double Max) oLife = StarTable.GetLifetimeRange(StarClass.SpectralClass.O);
        (double Min, double Max) mLife = StarTable.GetLifetimeRange(StarClass.SpectralClass.M);

        if (oLife.Max >= mLife.Min)
        {
            throw new InvalidOperationException($"O stars should have shorter lifetimes than M stars. O max: {oLife.Max}, M min: {mLife.Min}");
        }
    }
}
