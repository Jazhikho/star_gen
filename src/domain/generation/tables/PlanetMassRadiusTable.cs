using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

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

    /// <summary>
    /// Chen-Kipping Terran class probability for the supplied mass.
    /// </summary>
    public double TerranProbability { get; }

    /// <summary>
    /// Chen-Kipping Neptunian class probability for the supplied mass.
    /// </summary>
    public double NeptunianProbability { get; }

    /// <summary>
    /// Chen-Kipping Jovian class probability for the supplied mass.
    /// </summary>
    public double JovianProbability { get; }

    /// <summary>
    /// Log10 radius scatter sampled around the deterministic relation.
    /// </summary>
    public double RadiusScatterLog10 { get; }

    public PlanetMassRadiusResolution(
        double radiusEarth,
        double densityKgM3,
        string appliedModelId,
        string appliedRegimeId,
        double terranProbability = 0.0,
        double neptunianProbability = 0.0,
        double jovianProbability = 0.0,
        double radiusScatterLog10 = 0.0)
    {
        RadiusEarth = radiusEarth;
        DensityKgM3 = densityKgM3;
        AppliedModelId = appliedModelId;
        AppliedRegimeId = appliedRegimeId;
        TerranProbability = terranProbability;
        NeptunianProbability = neptunianProbability;
        JovianProbability = jovianProbability;
        RadiusScatterLog10 = radiusScatterLog10;
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
        return Resolve(requestedModel, spec, sizeCategory, massEarth, null);
    }

    /// <summary>
    /// Resolves a physical size from the selected mass-radius model and applies seeded probabilistic sampling when available.
    /// </summary>
    public static PlanetMassRadiusResolution Resolve(
        PlanetMassRadiusModel requestedModel,
        PlanetSpec spec,
        SizeCategory.Category sizeCategory,
        double massEarth,
        SeededRng? rng)
    {
        double baseRadiusEarth;
        string appliedModelId;
        string appliedRegimeId;
        double terranProbability = 0.0;
        double neptunianProbability = 0.0;
        double jovianProbability = 0.0;
        double radiusScatterLog10 = 0.0;

        if (requestedModel == PlanetMassRadiusModel.Otegi)
        {
            bool canUseOtegi = massEarth <= 120.0 && sizeCategory != SizeCategory.Category.GasGiant;
            if (canUseOtegi)
            {
                bool useRockyBranch = ShouldUseOtegiRockyBranch(spec, sizeCategory, massEarth);
                if (useRockyBranch)
                {
                    baseRadiusEarth = ResolveOtegiRockyRadiusEarth(massEarth, rng, out radiusScatterLog10);
                    appliedModelId = "otegi_2020";
                    appliedRegimeId = "otegi_rocky";
                }
                else
                {
                    baseRadiusEarth = ResolveOtegiVolatileRadiusEarth(massEarth, rng, out radiusScatterLog10);
                    appliedModelId = "otegi_2020";
                    appliedRegimeId = "otegi_volatile";
                }
            }
            else
            {
                baseRadiusEarth = ResolveChenKippingRadiusEarth(massEarth, out appliedRegimeId);
                appliedModelId = "chen_kipping";
                ChenKippingClassification classification = ResolveChenKippingClassification(massEarth);
                terranProbability = classification.TerranProbability;
                neptunianProbability = classification.NeptunianProbability;
                jovianProbability = classification.JovianProbability;
                radiusScatterLog10 = SampleChenKippingRadiusScatterLog10(classification, rng);
                baseRadiusEarth *= System.Math.Pow(10.0, radiusScatterLog10);
            }
        }
        else
        {
            baseRadiusEarth = ResolveChenKippingRadiusEarth(massEarth, out appliedRegimeId);
            appliedModelId = "chen_kipping";
            ChenKippingClassification classification = ResolveChenKippingClassification(massEarth);
            terranProbability = classification.TerranProbability;
            neptunianProbability = classification.NeptunianProbability;
            jovianProbability = classification.JovianProbability;
            radiusScatterLog10 = SampleChenKippingRadiusScatterLog10(classification, rng);
            baseRadiusEarth *= System.Math.Pow(10.0, radiusScatterLog10);
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

        return new PlanetMassRadiusResolution(
            adjustedRadiusEarth,
            densityKgM3,
            appliedModelId,
            appliedRegimeId,
            terranProbability,
            neptunianProbability,
            jovianProbability,
            radiusScatterLog10);
    }

    /// <summary>
    /// Returns Chen-Kipping class probabilities implied by the mass transitions.
    /// </summary>
    public static PlanetMassRadiusResolution ResolveChenKippingClassificationOnly(double massEarth)
    {
        ChenKippingClassification classification = ResolveChenKippingClassification(massEarth);
        return new PlanetMassRadiusResolution(
            0.0,
            0.0,
            "chen_kipping",
            classification.MostLikelyRegimeId,
            classification.TerranProbability,
            classification.NeptunianProbability,
            classification.JovianProbability);
    }

    private static bool ShouldUseOtegiRockyBranch(PlanetSpec spec, SizeCategory.Category sizeCategory, double massEarth)
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

        // Otegi et al. (2020) place the rocky population endpoint around 25 Earth masses.
        // Above that mass, even rocky-biased StarGen worlds should not keep using the
        // rocky relation as if it extended indefinitely.
        if (massEarth > 25.0)
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

    private static ChenKippingClassification ResolveChenKippingClassification(double massEarth)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 0.0001, 4000.0);

        // Chen and Kipping (2017) frame class assignment probabilistically around mass-radius
        // transitions. Tuning: the 2.04 Earth-mass transition width preserves the paper's
        // reported broad uncertainty; the 132 Earth-mass width is a conservative StarGen
        // smoothing envelope until the full posterior table is implemented.
        double terranToNeptunian = SmoothLogMassTransition(clampedMassEarth, 2.04, 0.14);
        double neptunianToJovian = SmoothLogMassTransition(clampedMassEarth, 132.0, 0.10);
        double terranProbability = 1.0 - terranToNeptunian;
        double neptunianProbability = terranToNeptunian * (1.0 - neptunianToJovian);
        double jovianProbability = neptunianToJovian;

        string mostLikelyRegimeId = "chen_kipping_terran";
        if (neptunianProbability >= terranProbability && neptunianProbability >= jovianProbability)
        {
            mostLikelyRegimeId = "chen_kipping_neptunian";
        }
        else if (jovianProbability >= terranProbability && jovianProbability >= neptunianProbability)
        {
            mostLikelyRegimeId = "chen_kipping_jovian";
        }

        return new ChenKippingClassification(
            terranProbability,
            neptunianProbability,
            jovianProbability,
            mostLikelyRegimeId);
    }

    private static double SmoothLogMassTransition(double massEarth, double boundaryMassEarth, double widthDex)
    {
        double safeMassEarth = System.Math.Max(massEarth, 0.0001);
        double logOffset = System.Math.Log10(safeMassEarth / boundaryMassEarth);
        double exponent = -logOffset / widthDex;
        return 1.0 / (1.0 + System.Math.Exp(exponent));
    }

    private static double SampleChenKippingRadiusScatterLog10(ChenKippingClassification classification, SeededRng? rng)
    {
        if (rng == null)
        {
            return 0.0;
        }

        // Chen and Kipping (2017) explicitly reject a deterministic mass-radius curve.
        // Tuning: these conservative scatter widths are StarGen sampling envelopes around
        // the published regime relation until the complete Forecaster posterior is wired in.
        double sigmaLog10 = (classification.TerranProbability * 0.035)
            + (classification.NeptunianProbability * 0.090)
            + (classification.JovianProbability * 0.045);
        double sampledScatter = rng.Randfn(0.0f, (float)sigmaLog10);
        double maxScatter = sigmaLog10 * 2.5;
        if (sampledScatter > maxScatter)
        {
            return maxScatter;
        }

        if (sampledScatter < -maxScatter)
        {
            return -maxScatter;
        }

        return sampledScatter;
    }

    private static double ResolveOtegiRockyRadiusEarth(double massEarth, SeededRng? rng, out double scatterLog10)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 0.1, 25.0);
        double centralRadiusEarth = System.Math.Pow(clampedMassEarth / 0.9, 1.0 / 3.45);
        if (rng == null)
        {
            scatterLog10 = 0.0;
            return centralRadiusEarth;
        }

        double coefficient = ClampSampledPositive(rng.Randfn(0.90f, 0.06f), 0.72, 1.08);
        double exponent = ClampSampledPositive(rng.Randfn(3.45f, 0.12f), 3.09, 3.81);
        double sampledRadiusEarth = System.Math.Pow(clampedMassEarth / coefficient, 1.0 / exponent);
        scatterLog10 = System.Math.Log10(sampledRadiusEarth / centralRadiusEarth);
        return sampledRadiusEarth;
    }

    private static double ResolveOtegiVolatileRadiusEarth(double massEarth, SeededRng? rng, out double scatterLog10)
    {
        double clampedMassEarth = System.Math.Clamp(massEarth, 1.0, 120.0);
        double centralRadiusEarth = System.Math.Pow(clampedMassEarth / 1.74, 1.0 / 1.58);
        if (rng == null)
        {
            scatterLog10 = 0.0;
            return centralRadiusEarth;
        }

        double coefficient = ClampSampledPositive(rng.Randfn(1.74f, 0.38f), 0.60, 2.88);
        double exponent = ClampSampledPositive(rng.Randfn(1.58f, 0.10f), 1.28, 1.88);
        double sampledRadiusEarth = System.Math.Pow(clampedMassEarth / coefficient, 1.0 / exponent);
        scatterLog10 = System.Math.Log10(sampledRadiusEarth / centralRadiusEarth);
        return sampledRadiusEarth;
    }

    private static double ClampSampledPositive(double value, double minimum, double maximum)
    {
        if (value < minimum)
        {
            return minimum;
        }

        if (value > maximum)
        {
            return maximum;
        }

        return value;
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

    private sealed class ChenKippingClassification
    {
        public double TerranProbability { get; }
        public double NeptunianProbability { get; }
        public double JovianProbability { get; }
        public string MostLikelyRegimeId { get; }

        public ChenKippingClassification(
            double terranProbability,
            double neptunianProbability,
            double jovianProbability,
            string mostLikelyRegimeId)
        {
            TerranProbability = terranProbability;
            NeptunianProbability = neptunianProbability;
            JovianProbability = jovianProbability;
            MostLikelyRegimeId = mostLikelyRegimeId;
        }
    }
}
