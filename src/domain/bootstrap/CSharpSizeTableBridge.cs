using Godot;
using Godot.Collections;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;
using StarGen.Domain.Generation.Tables;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for pure size-table helpers.
/// </summary>
[GlobalClass]
public partial class CSharpSizeTableBridge : RefCounted
{
    /// <summary>
    /// Returns the mass range for a size category.
    /// </summary>
    public Dictionary GetMassRange(int category) => SizeTable.GetMassRange((SizeCategoryArchetype.Category)category);

    /// <summary>
    /// Returns the radius range for a size category.
    /// </summary>
    public Dictionary GetRadiusRange(int category) => SizeTable.GetRadiusRange((SizeCategoryArchetype.Category)category);

    /// <summary>
    /// Returns the density range for a size category.
    /// </summary>
    public Dictionary GetDensityRange(int category) => SizeTable.GetDensityRange((SizeCategoryArchetype.Category)category);

    /// <summary>
    /// Infers a size category from a mass in Earth masses.
    /// </summary>
    public int CategoryFromMass(double massEarth) => (int)SizeTable.CategoryFromMass(massEarth);

    /// <summary>
    /// Calculates radius from mass and density.
    /// </summary>
    public double RadiusFromMassDensity(double massKg, double densityKgM3) => SizeTable.RadiusFromMassDensity(massKg, densityKgM3);
}
