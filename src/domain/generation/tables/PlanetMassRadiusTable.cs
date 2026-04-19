using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Result payload for a resolved planet mass-radius model application.
/// </summary>
public sealed class PlanetMassRadiusResolution
{
    /// <summary>
    /// Final resolved radius in Earth radii.
    /// </summary>
    public double RadiusEarth { get; }

    /// <summary>
    /// Final resolved bulk density in kg/m^3.
    /// </summary>
    public double DensityKgM3 { get; }

    /// <summary>
    /// Identifier for the model family that was actually applied.
    /// </summary>
    public string AppliedModelId { get; }

    /// <summary>
    /// Identifier for the mass-radius regime that was actually applied.
    /// </summary>
    public string AppliedRegimeId { get; }

    public PlanetMassRadiusResolution(double radiusEarth, double densityKgM3, string appliedModelId, string appliedRegimeId)
    {
        RadiusEarth = radiusEarth;
        DensityKgM3 = densityKgM3;
        AppliedModelId = appliedModelId;
        AppliedRegimeId = appliedRegimeId;
    }
}

/// <summary>
/// Resolves empirical planet radius and density from a selected mass-radius model.
/// </summary>
public static class PlanetMassRadiusTable
{
    private const double MinimumRadiusEarth = 0.03;
    private const double MaximumRadiusEarth = 15.0;

    /// <summary>
    /// Resolves a physical size from the selected mass-radius model and the current planet-facing biases.
    /// </summary>
    public static PlanetMassRadiusResolution Resolve(
        PlanetMassRadiusModel requestedModel,
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        double massEarth)
    {
        double baseRadiusEarth;
        string appliedModelId;
        string appliedRegimeId;

        if (requestedModel == PlanetMassRadiusModel.Otegi)
        {
            bool canUseOtegi = massEarth <= 120.0 && sizeCategory != SizeCategory.Category.GasGiant;
            if (canUseOtegi)
            {
                bool useRockyBranch = ShouldUseOtegiRockyBranch(spec, sizeCategory);
                if (useRockyBranch)
                {
                    baseRadiusEarth = ResolveOtegiRockyRadiusEarth(massEarth);
                    appliedModelId = "otegi_2020";
                    appliedRegimeId = "otegi_rocky";
                }
                else
                {
                    baseRadiusEarth = ResolveOtegiVolatileRadiusEarth(massEarth);
                    appliedModelId = "otegi_2020";
                    appliedRegimeId = "otegi_volatile";
                }
            }
            else
            {
                baseRadiusEarth = ResolveChenKippingRadiusEarth(massEarth, out appliedRegimeId);
                appliedModelId = "chen_kipping";
            }
        }
        else
        {
            baseRadiusEarth = ResolveChenKippingRadiusEarth(massEarth, out appliedRegimeId);
            appliedModelId = "chen_kipping";
        }

        double adjustedRadiusEarth = ApplyDirectPlanetBiasAdjustments(spec, sizeCategory, baseRadiusEarth);
        adjustedRadiusEarth = System.Math.Clamp(adjustedRadiusEarth, MinimumRadiusEarth, MaximumRadiusEarth);

        double massKg = massEarth * Units.EarthMassKg;
        double radiusMeters = adjustedRadiusEarth * Units.EarthRadiusMeters;
        double densityKgM3 = CalculateDensityKgM3(massKg, radiusMeters);
        (double minDensityKgM3, double maxDensityKgM3) = SizeTable.GetDensityRangeTuple(sizeCategory);
        if (densityKgM3 < minDensityKgM3)
        {
            densityKgM3 = minDensityKgM3;
            radiusMeters = SizeTable.RadiusFromMassDensity(massKg, densityKgM3);
            adjustedRadiusEarth = radiusMeters / Units.EarthRadiusMeters;
        }
        else if (densityKgM3 > maxDensityKgM3)
        {
            densityKgM3 = maxDensityKgM3;
            radiusMeters = SizeTable.RadiusFromMassDensity(massKg, densityKgM3);
            adjustedRadiusEarth = radiusMeters / Units.EarthRadiusMeters;
        }

        return new PlanetMassRadiusResolution(adjustedRadiusEarth, densityKgM3, appliedModelId, appliedRegimeId);
    }

    private static bool ShouldUseOtegiRockyBranch(PlanetSpec spec, SizeCategory.Category sizeCategory)
    {
        if (spec.ClassBias == PlanetClassBias.GasGiant || spec.ClassBias == PlanetClassBias.SubNeptune)
        {
            return false;
        }

        if (spec.ClassBias == PlanetClassBias.WaterRich)
        {
            return false;
        }

        if (spec.CompositionBias == PlanetCompositionBias.IcyWaterRich || spec.CompositionBias == PlanetCompositionBias.GasEnvelope)
        {
            return false;
        }

        if (spec.ClassBias == PlanetClassBias.Rocky || spec.ClassBias == PlanetClassBias.StrippedCore)
        {
            return true;
        }

        if (spec.CompositionBias == PlanetCompositionBias.Rocky)
        {
            return true;
        }

        if (sizeCategory == SizeCategory.Category.Dwarf
            || sizeCategory == SizeCategory.Category.SubTerrestrial
            || sizeCategory == SizeCategory.Category.Terrestrial
            || sizeCategory == SizeCategory.Category.SuperEarth)
        {
            return true;
        }

        return false;
    }

    private static double ResolveChenKippingRadiusEarth(double massEarth, out string regimeId)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 0.0001, 4000.0);
        if (clampedMassEarth < 2.04)
        {
            regimeId = "chen_kipping_terran";
            return System.Math.Pow(10.0, 0.00346) * System.Math.Pow(clampedMassEarth, 0.2790);
        }

        if (clampedMassEarth < 132.0)
        {
            regimeId = "chen_kipping_neptunian";
            return System.Math.Pow(10.0, -0.0925) * System.Math.Pow(clampedMassEarth, 0.5890);
        }

        regimeId = "chen_kipping_jovian";
        return System.Math.Pow(10.0, 1.25) * System.Math.Pow(clampedMassEarth, -0.0440);
    }

    private static double ResolveOtegiRockyRadiusEarth(double massEarth)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 0.1, 25.0);
        return System.Math.Pow(clampedMassEarth / 0.9, 1.0 / 3.45);
    }

    private static double ResolveOtegiVolatileRadiusEarth(double massEarth)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 1.0, 120.0);
        return System.Math.Pow(clampedMassEarth / 1.74, 1.0 / 1.58);
    }

    private static double ApplyDirectPlanetBiasAdjustments(PlanetSpec spec, SizeCategory.Category sizeCategory, double radiusEarth)
    {
        double adjustedRadiusEarth = radiusEarth;

        if (spec.CompositionBias == PlanetCompositionBias.Rocky)
        {
            adjustedRadiusEarth *= 0.95;
        }
        else if (spec.CompositionBias == PlanetCompositionBias.IcyWaterRich)
        {
            adjustedRadiusEarth *= 1.08;
        }
        else if (spec.CompositionBias == PlanetCompositionBias.GasEnvelope && sizeCategory != SizeCategory.Category.GasGiant)
        {
            adjustedRadiusEarth *= 1.12;
        }

        if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Thin)
        {
            adjustedRadiusEarth *= 0.94;
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Retained && sizeCategory != SizeCategory.Category.GasGiant)
        {
            adjustedRadiusEarth *= 1.08;
        }
        else if (spec.EnvelopeOverride == PlanetEnvelopeOverride.Stripped && sizeCategory != SizeCategory.Category.GasGiant)
        {
            adjustedRadiusEarth *= 0.90;
        }

        if (spec.VolatileRichness == PlanetVolatileRichness.Poor)
        {
            adjustedRadiusEarth *= 0.96;
        }
        else if (spec.VolatileRichness == PlanetVolatileRichness.Rich)
        {
            adjustedRadiusEarth *= 1.05;
        }

        if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Dry)
        {
            adjustedRadiusEarth *= 0.97;
        }
        else if (spec.HydrosphereTendency == PlanetHydrosphereTendency.Oceanic)
        {
            adjustedRadiusEarth *= 1.03;
        }

        return adjustedRadiusEarth;
    }

    private static double CalculateDensityKgM3(double massKg, double radiusMeters)
    {
        double volumeM3 = (4.0 / 3.0) * System.Math.PI * radiusMeters * radiusMeters * radiusMeters;
        if (volumeM3 <= 0.0)
        {
            throw new System.InvalidOperationException("PlanetMassRadiusTable.CalculateDensityKgM3 requires a positive radius.");
        }

        return massKg / volumeM3;
    }
}
