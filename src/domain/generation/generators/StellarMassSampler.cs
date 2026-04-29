using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Samples stellar masses from a profile-driven initial mass function.
/// </summary>
public static class StellarMassSampler
{
    private const double MinimumMassSolar = 0.01;
    private const double MaximumMassSolar = 60.0;

    /// <summary>
    /// Samples a stellar mass in solar units from the configured IMF family.
    /// </summary>
    public static double SampleMassSolar(StellarGenerationProfile profile, GalaxyOriginContext context, SeededRng rng)
    {
        double baseMassSolar;
        if (profile.ImfForm == StellarImfForm.Chabrier)
        {
            baseMassSolar = SampleChabrierMassSolar(rng);
        }
        else
        {
            baseMassSolar = SampleKroupaMassSolar(rng);
        }

        baseMassSolar = ApplyUniversalContextBias(baseMassSolar, context);

        if (profile.ImfVariationMode == StellarImfVariationMode.MetallicityAgeModulated)
        {
            baseMassSolar = ApplyMetallicityAgeModulation(baseMassSolar, context);
        }

        return System.Math.Clamp(baseMassSolar, MinimumMassSolar, MaximumMassSolar);
    }

    private static double SampleKroupaMassSolar(SeededRng rng)
    {
        double substellarWeight = IntegratePowerLaw(0.01, 0.075, 0.7);
        double lowSegmentWeight = IntegratePowerLaw(0.075, 0.5, 1.3);
        double midSegmentWeight = IntegratePowerLaw(0.5, 1.0, 2.3);
        double highSegmentWeight = IntegratePowerLaw(1.0, MaximumMassSolar, 2.3);
        double totalWeight = substellarWeight + lowSegmentWeight + midSegmentWeight + highSegmentWeight;
        double roll = rng.Randf() * totalWeight;

        if (roll < substellarWeight)
        {
            return SamplePowerLaw(0.01, 0.075, 0.7, rng);
        }

        if (roll < substellarWeight + lowSegmentWeight)
        {
            return SamplePowerLaw(0.075, 0.5, 1.3, rng);
        }

        if (roll < substellarWeight + lowSegmentWeight + midSegmentWeight)
        {
            return SamplePowerLaw(0.5, 1.0, 2.3, rng);
        }

        return SamplePowerLaw(1.0, MaximumMassSolar, 2.3, rng);
    }

    private static double SampleChabrierMassSolar(SeededRng rng)
    {
        if (rng.Randf() < 0.93f)
        {
            double mu = System.Math.Log(0.16);
            double sigma = 0.58;
            double sample = System.Math.Exp(mu + (sigma * rng.Randfn(0.0f, 1.0f)));
            return System.Math.Clamp(sample, MinimumMassSolar, 1.0);
        }

        return SamplePowerLaw(1.0, MaximumMassSolar, 2.3, rng);
    }

    private static double ApplyUniversalContextBias(double massSolar, GalaxyOriginContext context)
    {
        // Li et al. (2023) argue that stellar mass-function shape varies with metallicity and
        // population age, so young and low-metallicity populations can skew toward higher
        // characteristic masses than older, metal-richer ones. Tuning: the additive shifts and
        // final clamp below are StarGen context weights, not directly fitted literature values.
        double massShift = 1.0;

        if (context.AgeCohort == GalaxyAgeCohort.Young)
        {
            massShift += 0.12;
        }
        else if (context.AgeCohort == GalaxyAgeCohort.Ancient)
        {
            massShift -= 0.10;
        }
        else if (context.AgeCohort == GalaxyAgeCohort.Old)
        {
            massShift -= 0.05;
        }

        if (context.RegionKind == GalaxyRegionKind.SpiralArm || context.RegionKind == GalaxyRegionKind.IrregularBody)
        {
            massShift += 0.08;
        }
        else if (context.RegionKind == GalaxyRegionKind.Halo || context.RegionKind == GalaxyRegionKind.DwarfEnvelope)
        {
            massShift -= 0.08;
        }

        massShift += (context.ClusterProbability - 0.1) * 0.12;
        return massSolar * System.Math.Clamp(massShift, 0.70, 1.35);
    }

    private static double ApplyMetallicityAgeModulation(double massSolar, GalaxyOriginContext context)
    {
        // Li et al. (2023) provide the framework for metallicity- and age-dependent IMF
        // variation. Tuning: the low-Z, high-Z, young, and ancient modulation coefficients
        // below are compact StarGen approximations for that trend rather than a direct IMF fit.
        double massShift = 1.0;

        if (context.MetallicityPrior < 0.7)
        {
            massShift += 0.08;
        }
        else if (context.MetallicityPrior > 1.4)
        {
            massShift -= 0.04;
        }

        if (context.AgeCohort == GalaxyAgeCohort.Young)
        {
            massShift += 0.10;
        }
        else if (context.AgeCohort == GalaxyAgeCohort.Ancient)
        {
            massShift -= 0.08;
        }

        massShift += (context.ClusterProbability - 0.1) * 0.08;
        return massSolar * System.Math.Clamp(massShift, 0.75, 1.35);
    }

    private static double SamplePowerLaw(double minimum, double maximum, double alpha, SeededRng rng)
    {
        double exponent = 1.0 - alpha;
        if (System.Math.Abs(exponent) < 0.000001)
        {
            double logMinimum = System.Math.Log(minimum);
            double logMaximum = System.Math.Log(maximum);
            double sample = logMinimum + ((logMaximum - logMinimum) * rng.Randf());
            return System.Math.Exp(sample);
        }

        double minimumTerm = System.Math.Pow(minimum, exponent);
        double maximumTerm = System.Math.Pow(maximum, exponent);
        double blended = minimumTerm + ((maximumTerm - minimumTerm) * rng.Randf());
        return System.Math.Pow(blended, 1.0 / exponent);
    }

    private static double IntegratePowerLaw(double minimum, double maximum, double alpha)
    {
        double exponent = 1.0 - alpha;
        if (System.Math.Abs(exponent) < 0.000001)
        {
            return System.Math.Log(maximum / minimum);
        }

        double maximumTerm = System.Math.Pow(maximum, exponent);
        double minimumTerm = System.Math.Pow(minimum, exponent);
        return (maximumTerm - minimumTerm) / exponent;
    }
}
