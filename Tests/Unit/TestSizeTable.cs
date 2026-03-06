#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SizeTable lookups.
/// </summary>
public static class TestSizeTable
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests mass range retrieval.
    /// </summary>
    public static void TestMassRanges()
    {
        (double Min, double Max) dwarfRange = SizeTable.GetMassRange(SizeCategory.Category.Dwarf);
        if (System.Math.Abs(dwarfRange.Min - 0.0001) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected dwarf min 0.0001, got {dwarfRange.Min}");
        }
        if (System.Math.Abs(dwarfRange.Max - 0.01) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected dwarf max 0.01, got {dwarfRange.Max}");
        }

        (double Min, double Max) giantRange = SizeTable.GetMassRange(SizeCategory.Category.GasGiant);
        if (System.Math.Abs(giantRange.Min - 80.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected giant min 80.0, got {giantRange.Min}");
        }
        if (System.Math.Abs(giantRange.Max - 4000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected giant max 4000.0, got {giantRange.Max}");
        }
    }

    /// <summary>
    /// Tests category detection from mass.
    /// </summary>
    public static void TestCategoryFromMass()
    {
        if (SizeTable.CategoryFromMass(0.001) != SizeCategory.Category.Dwarf)
        {
            throw new InvalidOperationException($"Expected Dwarf for 0.001, got {SizeTable.CategoryFromMass(0.001)}");
        }
        if (SizeTable.CategoryFromMass(0.1) != SizeCategory.Category.SubTerrestrial)
        {
            throw new InvalidOperationException($"Expected SubTerrestrial for 0.1, got {SizeTable.CategoryFromMass(0.1)}");
        }
        if (SizeTable.CategoryFromMass(1.0) != SizeCategory.Category.Terrestrial)
        {
            throw new InvalidOperationException($"Expected Terrestrial for 1.0, got {SizeTable.CategoryFromMass(1.0)}");
        }
        if (SizeTable.CategoryFromMass(5.0) != SizeCategory.Category.SuperEarth)
        {
            throw new InvalidOperationException($"Expected SuperEarth for 5.0, got {SizeTable.CategoryFromMass(5.0)}");
        }
        if (SizeTable.CategoryFromMass(15.0) != SizeCategory.Category.MiniNeptune)
        {
            throw new InvalidOperationException($"Expected MiniNeptune for 15.0, got {SizeTable.CategoryFromMass(15.0)}");
        }
        if (SizeTable.CategoryFromMass(50.0) != SizeCategory.Category.NeptuneClass)
        {
            throw new InvalidOperationException($"Expected NeptuneClass for 50.0, got {SizeTable.CategoryFromMass(50.0)}");
        }
        if (SizeTable.CategoryFromMass(500.0) != SizeCategory.Category.GasGiant)
        {
            throw new InvalidOperationException($"Expected GasGiant for 500.0, got {SizeTable.CategoryFromMass(500.0)}");
        }
    }

    /// <summary>
    /// Tests random mass generation stays in range.
    /// </summary>
    public static void TestRandomMassInRange()
    {
        SeededRng rng = new SeededRng(12345);

        foreach (SizeCategory.Category category in Enum.GetValues(typeof(SizeCategory.Category)))
        {
            (double Min, double Max) rangeData = SizeTable.GetMassRange(category);
            for (int i = 0; i < 10; i++)
            {
                double mass = SizeTable.RandomMassEarth(category, rng);
                double min = rangeData.Min;
                double max = rangeData.Max;
                if (mass < min || mass > max)
                {
                    throw new InvalidOperationException($"Expected mass in range [{min}, {max}], got {mass}");
                }
            }
        }
    }

    /// <summary>
    /// Tests radius from mass and density.
    /// </summary>
    public static void TestRadiusFromMassDensity()
    {
        double radius = SizeTable.RadiusFromMassDensity(5.972e24, 5515.0);
        if (radius < 6.0e6 || radius > 6.5e6)
        {
            throw new InvalidOperationException($"Expected radius in range [6.0e6, 6.5e6], got {radius}");
        }
    }

    /// <summary>
    /// Tests density ranges are realistic.
    /// </summary>
    public static void TestDensityRangesRealistic()
    {
        Godot.Collections.Dictionary rockyDensity = SizeTable.GetDensityRange(SizeCategory.Category.Terrestrial);
        Godot.Collections.Dictionary gasDensity = SizeTable.GetDensityRange(SizeCategory.Category.GasGiant);

        if (rockyDensity["min"].AsDouble() <= gasDensity["max"].AsDouble())
        {
            throw new InvalidOperationException($"Rocky bodies should have higher density than gas giants. Rocky min: {rockyDensity["min"]}, Gas max: {gasDensity["max"]}");
        }
    }
}
