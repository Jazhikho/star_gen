using StarGen.Domain.Rng;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Lookup table for size-category properties.
/// </summary>
public static class SizeTable
{
    /// <summary>
    /// Returns the mass range for a size category in Earth masses.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <returns>Inclusive (Min, Max) mass range in Earth masses.</returns>
    public static NumericRange GetMassRange(SizeCategoryArchetype.Category category)
    {
        (double min, double max) = GetMassRangeTuple(category);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetMassRangeTuple(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => (0.0001, 0.01),
            SizeCategoryArchetype.Category.SubTerrestrial => (0.01, 0.3),
            SizeCategoryArchetype.Category.Terrestrial => (0.3, 2.0),
            SizeCategoryArchetype.Category.SuperEarth => (2.0, 10.0),
            SizeCategoryArchetype.Category.MiniNeptune => (10.0, 25.0),
            SizeCategoryArchetype.Category.NeptuneClass => (25.0, 80.0),
            SizeCategoryArchetype.Category.GasGiant => (80.0, 4000.0),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the radius range for a size category in Earth radii.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <returns>Inclusive (Min, Max) radius range in Earth radii.</returns>
    public static NumericRange GetRadiusRange(SizeCategoryArchetype.Category category)
    {
        (double min, double max) = GetRadiusRangeTuple(category);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetRadiusRangeTuple(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => (0.03, 0.2),
            SizeCategoryArchetype.Category.SubTerrestrial => (0.2, 0.6),
            SizeCategoryArchetype.Category.Terrestrial => (0.6, 1.5),
            SizeCategoryArchetype.Category.SuperEarth => (1.2, 2.0),
            SizeCategoryArchetype.Category.MiniNeptune => (2.0, 4.0),
            SizeCategoryArchetype.Category.NeptuneClass => (3.5, 6.0),
            SizeCategoryArchetype.Category.GasGiant => (6.0, 15.0),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the density range for a size category in kg/m³.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <returns>Inclusive (Min, Max) density range in kg/m³.</returns>
    public static NumericRange GetDensityRange(SizeCategoryArchetype.Category category)
    {
        (double min, double max) = GetDensityRangeTuple(category);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetDensityRangeTuple(SizeCategoryArchetype.Category category)
    {
        return category switch
        {
            SizeCategoryArchetype.Category.Dwarf => (1500.0, 3500.0),
            SizeCategoryArchetype.Category.SubTerrestrial => (3000.0, 5500.0),
            SizeCategoryArchetype.Category.Terrestrial => (4000.0, 6500.0),
            SizeCategoryArchetype.Category.SuperEarth => (4500.0, 8000.0),
            SizeCategoryArchetype.Category.MiniNeptune => (1000.0, 3000.0),
            SizeCategoryArchetype.Category.NeptuneClass => (800.0, 2000.0),
            SizeCategoryArchetype.Category.GasGiant => (500.0, 1500.0),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Infers a size category from a mass value in Earth masses.
    /// </summary>
    /// <param name="massEarth">Planet mass in Earth masses.</param>
    /// <returns>Corresponding size category.</returns>
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
    /// GasGiant tier uses log-uniform sampling to avoid linear bias across the 80–4000 Earth-mass span.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random mass in Earth masses.</returns>
    public static double RandomMassEarth(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        (double min, double max) = GetMassRangeTuple(category);
        if (category == SizeCategoryArchetype.Category.GasGiant)
        {
            double logMin = System.Math.Log(min);
            double logMax = System.Math.Log(max);
            return System.Math.Exp(rng.RandfRange((float)logMin, (float)logMax));
        }

        return rng.RandfRange((float)min, (float)max);
    }

    /// <summary>
    /// Generates a random radius within the category in Earth radii.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random radius in Earth radii.</returns>
    public static double RandomRadiusEarth(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        (double min, double max) = GetRadiusRangeTuple(category);
        return rng.RandfRange((float)min, (float)max);
    }

    /// <summary>
    /// Generates a random density within the category in kg/m³.
    /// </summary>
    /// <param name="category">Target size category.</param>
    /// <param name="rng">Seeded random number generator.</param>
    /// <returns>Random density in kg/m³.</returns>
    public static double RandomDensity(SizeCategoryArchetype.Category category, SeededRng rng)
    {
        (double min, double max) = GetDensityRangeTuple(category);
        return rng.RandfRange((float)min, (float)max);
    }

    /// <summary>
    /// Calculates the radius of a sphere with the given mass and density.
    /// </summary>
    /// <param name="massKg">Mass in kilograms.</param>
    /// <param name="densityKgM3">Density in kg/m³.</param>
    /// <returns>Radius in meters.</returns>
    public static double RadiusFromMassDensity(double massKg, double densityKgM3)
    {
        if (densityKgM3 <= 0.0)
        {
            return 0.0;
        }

        double volume = massKg / densityKgM3;
        return System.Math.Pow(volume * 3.0 / (4.0 * System.Math.PI), 1.0 / 3.0);
    }

}
