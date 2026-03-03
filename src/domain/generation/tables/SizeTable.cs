using Godot.Collections;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Lookup table for size-category properties.
/// </summary>
public static class SizeTable
{
    /// <summary>
    /// Returns the mass range for a size category in Earth masses.
    /// </summary>
    public static Dictionary GetMassRange(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => BuildRange(0.0001, 0.01),
            SizeCategoryArchetype.Category.SubTerrestrial => BuildRange(0.01, 0.3),
            SizeCategoryArchetype.Category.Terrestrial => BuildRange(0.3, 2.0),
            SizeCategoryArchetype.Category.SuperEarth => BuildRange(2.0, 10.0),
            SizeCategoryArchetype.Category.MiniNeptune => BuildRange(10.0, 25.0),
            SizeCategoryArchetype.Category.NeptuneClass => BuildRange(25.0, 80.0),
            SizeCategoryArchetype.Category.GasGiant => BuildRange(80.0, 4000.0),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the radius range for a size category in Earth radii.
    /// </summary>
    public static Dictionary GetRadiusRange(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => BuildRange(0.03, 0.2),
            SizeCategoryArchetype.Category.SubTerrestrial => BuildRange(0.2, 0.6),
            SizeCategoryArchetype.Category.Terrestrial => BuildRange(0.6, 1.5),
            SizeCategoryArchetype.Category.SuperEarth => BuildRange(1.2, 2.0),
            SizeCategoryArchetype.Category.MiniNeptune => BuildRange(2.0, 4.0),
            SizeCategoryArchetype.Category.NeptuneClass => BuildRange(3.5, 6.0),
            SizeCategoryArchetype.Category.GasGiant => BuildRange(6.0, 15.0),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the density range for a size category in kg/m^3.
    /// </summary>
    public static Dictionary GetDensityRange(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => BuildRange(1500.0, 3500.0),
            SizeCategoryArchetype.Category.SubTerrestrial => BuildRange(3000.0, 5500.0),
            SizeCategoryArchetype.Category.Terrestrial => BuildRange(4000.0, 6500.0),
            SizeCategoryArchetype.Category.SuperEarth => BuildRange(4500.0, 8000.0),
            SizeCategoryArchetype.Category.MiniNeptune => BuildRange(1000.0, 3000.0),
            SizeCategoryArchetype.Category.NeptuneClass => BuildRange(800.0, 2000.0),
            SizeCategoryArchetype.Category.GasGiant => BuildRange(500.0, 1500.0),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Infers a size category from a mass value in Earth masses.
    /// </summary>
    public static SizeCategoryArchetype.Category CategoryFromMass(double massEarth)
    {
        return massEarth switch
        {
            < 0.01 => SizeCategoryArchetype.Category.Dwarf,
            < 0.3 => SizeCategoryArchetype.Category.SubTerrestrial,
            < 2.0 => SizeCategoryArchetype.Category.Terrestrial,
            < 10.0 => SizeCategoryArchetype.Category.SuperEarth,
            < 25.0 => SizeCategoryArchetype.Category.MiniNeptune,
            < 80.0 => SizeCategoryArchetype.Category.NeptuneClass,
            _ => SizeCategoryArchetype.Category.GasGiant,
        };
    }

    /// <summary>
    /// Generates a random mass within the category in Earth masses.
    /// </summary>
    public static double RandomMassEarth(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        Dictionary rangeData = GetMassRange(category);
        return rng.RandfRange((float)(double)rangeData["min"], (float)(double)rangeData["max"]);
    }

    /// <summary>
    /// Generates a random radius within the category in Earth radii.
    /// </summary>
    public static double RandomRadiusEarth(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        Dictionary rangeData = GetRadiusRange(category);
        return rng.RandfRange((float)(double)rangeData["min"], (float)(double)rangeData["max"]);
    }

    /// <summary>
    /// Generates a random density within the category in kg/m^3.
    /// </summary>
    public static double RandomDensity(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        Dictionary rangeData = GetDensityRange(category);
        return rng.RandfRange((float)(double)rangeData["min"], (float)(double)rangeData["max"]);
    }

    /// <summary>
    /// Calculates radius from mass and density.
    /// </summary>
    public static double RadiusFromMassDensity(double massKg, double densityKgM3)
    {
        if (densityKgM3 <= 0.0)
        {
            return 0.0;
        }

        double volume = massKg / densityKgM3;
        return System.Math.Pow(volume * 3.0 / (4.0 * System.Math.PI), 1.0 / 3.0);
    }

    private static Dictionary BuildRange(double min, double max)
    {
        return new Dictionary
        {
            ["min"] = min,
            ["max"] = max,
        };
    }
}
