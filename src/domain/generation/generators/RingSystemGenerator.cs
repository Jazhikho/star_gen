using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates ring systems for planets.
/// </summary>
public static class RingSystemGenerator
{
    private static readonly float[] ComplexityWeights = new float[] { 40.0f, 35.0f, 25.0f };
    private static readonly double[] ResonanceFractions = new double[] { 0.48, 0.63, 0.76, 0.87 };

    private const double IceLineAu = 2.7;
    private const double IcyParticleDensity = 900.0;
    private const double RockyParticleDensity = 2500.0;
    private const double SaturnRingMassKg = 1.5e19;
    private const double MinGapFraction = 0.02;
    private const double MaxGapFraction = 0.15;

    /// <summary>
    /// Returns whether a planet should have rings.
    /// </summary>
    public static bool ShouldHaveRings(
        PhysicalProps planetPhysical,
        ParentContext context,
        SeededRng rng)
    {
        _ = context;
        double massEarth = planetPhysical.MassKg / Units.EarthMassKg;

        double probability;
        if (massEarth > 50.0)
        {
            probability = 0.7;
        }
        else if (massEarth > 10.0)
        {
            probability = 0.4;
        }
        else if (massEarth > 5.0)
        {
            probability = 0.15;
        }
        else if (massEarth > 2.0)
        {
            probability = 0.05;
        }
        else
        {
            probability = 0.01;
        }

        return rng.Randf() < probability;
    }

    /// <summary>
    /// Generates a ring-system component for the supplied planet.
    /// </summary>
    public static RingSystemProps? Generate(
        RingSystemSpec? spec,
        PhysicalProps planetPhysical,
        ParentContext context,
        SeededRng rng)
    {
        RingSystemSpec ringSpec = spec ?? RingSystemSpec.Random(unchecked((int)rng.Randi()));
        RingComplexity.Level complexity = DetermineComplexity(ringSpec, rng);
        bool isIcy = DetermineComposition(ringSpec, context);
        (double innerLimitM, double outerLimitM) = CalculateRingLimits(planetPhysical, context);

        if (innerLimitM >= outerLimitM)
        {
            return null;
        }

        Array<RingBand> bands = GenerateBands(
            complexity,
            innerLimitM,
            outerLimitM,
            isIcy,
            rng);

        if (bands.Count == 0)
        {
            return null;
        }

        double totalMassKg = CalculateTotalMass(bands, isIcy, rng);
        double inclinationDeg = ringSpec.GetOverrideFloat(
            "inclination_deg",
            rng.RandfRange(0.0f, 2.0f));

        return new RingSystemProps(bands, totalMassKg, inclinationDeg);
    }

    /// <summary>Picks complexity level from spec or weighted RNG.</summary>
    private static RingComplexity.Level DetermineComplexity(RingSystemSpec spec, SeededRng rng)
    {
        if (spec.HasComplexity())
        {
            return (RingComplexity.Level)spec.Complexity;
        }

        RingComplexity.Level[] levels = new RingComplexity.Level[]
        {
            RingComplexity.Level.Trace,
            RingComplexity.Level.Simple,
            RingComplexity.Level.Complex,
        };

        RingComplexity.Level? selected = rng.WeightedChoice(levels, ComplexityWeights);
        return selected ?? RingComplexity.Level.Trace;
    }

    /// <summary>Returns true if rings should be icy (beyond ice line) or from spec.</summary>
    private static bool DetermineComposition(RingSystemSpec spec, ParentContext context)
    {
        if (spec.HasCompositionPreference())
        {
            return (bool)spec.IsIcy;
        }

        double distanceAu = context.OrbitalDistanceFromStarM / Units.AuMeters;
        double luminositySolar = context.StellarLuminosityWatts / 3.828e26;
        double adjustedIceLine = IceLineAu * System.Math.Sqrt(luminositySolar);
        return distanceAu > adjustedIceLine;
    }

    /// <summary>Computes inner and outer ring limits from planet radius and Roche/Hill.</summary>
    private static (double InnerLimitM, double OuterLimitM) CalculateRingLimits(
        PhysicalProps planetPhysical,
        ParentContext context)
    {
        double planetRadiusM = planetPhysical.RadiusM;
        double innerLimitM = planetRadiusM * 1.1;
        double planetDensity = planetPhysical.GetDensityKgM3();
        double rocheLimitM = 2.44
            * planetRadiusM
            * System.Math.Pow(planetDensity / IcyParticleDensity, 1.0 / 3.0);
        double outerLimitM = rocheLimitM * 2.5;

        if (context.HasParentBody())
        {
            double hillRadius = context.GetHillSphereRadiusM();
            outerLimitM = System.Math.Min(outerLimitM, hillRadius * 0.3);
        }

        innerLimitM = System.Math.Max(innerLimitM, rocheLimitM * 0.5);
        return (innerLimitM, outerLimitM);
    }

    /// <summary>Generates ring bands for the given complexity and limits.</summary>
    private static Array<RingBand> GenerateBands(
        RingComplexity.Level complexity,
        double innerLimitM,
        double outerLimitM,
        bool isIcy,
        SeededRng rng)
    {
        Godot.Collections.Dictionary<string, int> countRange = RingComplexity.GetBandCountRange(complexity);
        int bandCount = rng.RandiRange(countRange["min"], countRange["max"]);
        Godot.Collections.Dictionary<string, double> depthRange = RingComplexity.GetOpticalDepthRange(complexity);

        if (bandCount == 1)
        {
            return new Array<RingBand>
            {
                CreateBand(innerLimitM, outerLimitM, depthRange, isIcy, "Main", rng),
            };
        }

        return GenerateMultiBandSystem(
            bandCount,
            innerLimitM,
            outerLimitM,
            depthRange,
            isIcy,
            rng);
    }

    /// <summary>Builds multiple bands with resonance-style gaps.</summary>
    private static Array<RingBand> GenerateMultiBandSystem(
        int bandCount,
        double innerLimitM,
        double outerLimitM,
        Godot.Collections.Dictionary<string, double> depthRange,
        bool isIcy,
        SeededRng rng)
    {
        Array<RingBand> bands = new();
        double totalWidth = outerLimitM - innerLimitM;
        List<double> gapPositions = new();

        foreach (double resonanceFraction in ResonanceFractions)
        {
            if (gapPositions.Count >= bandCount - 1)
            {
                break;
            }

            double jitteredFraction = resonanceFraction + rng.RandfRange(-0.05f, 0.05f);
            jitteredFraction = System.Math.Clamp(jitteredFraction, 0.1, 0.9);
            double gapPosition = innerLimitM + (totalWidth * jitteredFraction);

            bool tooClose = false;
            foreach (double existing in gapPositions)
            {
                if (System.Math.Abs(gapPosition - existing) < totalWidth * 0.1)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                gapPositions.Add(gapPosition);
            }
        }

        while (gapPositions.Count < bandCount - 1)
        {
            double gapPosition = rng.RandfRange(
                (float)(innerLimitM + (totalWidth * 0.1)),
                (float)(outerLimitM - (totalWidth * 0.1)));

            bool tooClose = false;
            foreach (double existing in gapPositions)
            {
                if (System.Math.Abs(gapPosition - existing) < totalWidth * 0.08)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                gapPositions.Add(gapPosition);
            }
        }

        gapPositions.Sort();
        string[] bandNames = new string[] { "A", "B", "C", "D", "E", "F", "G" };
        double currentInner = innerLimitM;

        for (int index = 0; index < bandCount; index += 1)
        {
            double bandOuter;
            if (index < gapPositions.Count)
            {
                double gapWidth = totalWidth * rng.RandfRange(
                    (float)MinGapFraction,
                    (float)MaxGapFraction);
                bandOuter = gapPositions[index] - (gapWidth * 0.5);
            }
            else
            {
                bandOuter = outerLimitM;
            }

            if (bandOuter > currentInner + 1000.0)
            {
                string bandName;
                if (index < bandNames.Length)
                {
                    bandName = bandNames[index];
                }
                else
                {
                    bandName = $"Band_{index}";
                }
                bands.Add(CreateBand(
                    currentInner,
                    bandOuter,
                    depthRange,
                    isIcy,
                    bandName,
                    rng));
            }

            if (index < gapPositions.Count)
            {
                double gapWidth = totalWidth * rng.RandfRange(
                    (float)MinGapFraction,
                    (float)MaxGapFraction);
                currentInner = gapPositions[index] + (gapWidth * 0.5);
            }
            else
            {
                currentInner = bandOuter;
            }
        }

        return bands;
    }

    /// <summary>Creates a single ring band with optical depth and composition.</summary>
    private static RingBand CreateBand(
        double innerM,
        double outerM,
        Godot.Collections.Dictionary<string, double> depthRange,
        bool isIcy,
        string bandName,
        SeededRng rng)
    {
        double opticalDepth = rng.RandfRange(
            (float)depthRange["min"],
            (float)depthRange["max"]);
        Dictionary composition = GenerateComposition(isIcy, rng);
        double logSizeMin = System.Math.Log(0.001);
        double logSizeMax = System.Math.Log(10.0);
        double particleSizeM = System.Math.Exp(rng.RandfRange(
            (float)logSizeMin,
            (float)logSizeMax));

        return new RingBand(
            innerM,
            outerM,
            opticalDepth,
            composition,
            particleSizeM,
            bandName);
    }

    /// <summary>Generates normalized composition dict for icy or rocky rings.</summary>
    private static Dictionary GenerateComposition(bool isIcy, SeededRng rng)
    {
        Dictionary composition = new();

        if (isIcy)
        {
            composition["water_ice"] = rng.RandfRange(0.85f, 0.98f);
            composition["silicates"] = rng.RandfRange(0.01f, 0.10f);
            composition["carbon_compounds"] = rng.RandfRange(0.001f, 0.03f);
        }
        else
        {
            composition["silicates"] = rng.RandfRange(0.60f, 0.80f);
            composition["iron_oxides"] = rng.RandfRange(0.10f, 0.25f);
            composition["carbon_compounds"] = rng.RandfRange(0.05f, 0.15f);
        }

        double total = 0.0;
        foreach (Godot.Variant fraction in composition.Values)
        {
            total += (double)fraction;
        }

        if (total > 0.0)
        {
            foreach (Godot.Variant material in composition.Keys)
            {
                composition[material] = (double)composition[material] / total;
            }
        }

        return composition;
    }

    /// <summary>Estimates total ring mass from band areas and optical depths.</summary>
    private static double CalculateTotalMass(
        Array<RingBand> bands,
        bool isIcy,
        SeededRng rng)
    {
        double totalAreaM2 = 0.0;
        double averageOpticalDepth = 0.0;

        foreach (RingBand band in bands)
        {
            double area = System.Math.PI
                * ((band.OuterRadiusM * band.OuterRadiusM) - (band.InnerRadiusM * band.InnerRadiusM));
            totalAreaM2 += area;
            averageOpticalDepth += band.OpticalDepth;
        }

        if (bands.Count > 0)
        {
            averageOpticalDepth /= bands.Count;
        }

        double areaRatio = totalAreaM2 / 1.5e17;
        double depthRatio = averageOpticalDepth / 0.5;
        double baseMass = SaturnRingMassKg * areaRatio * depthRatio;

        if (!isIcy)
        {
            baseMass *= RockyParticleDensity / IcyParticleDensity;
        }

        double variation = rng.RandfRange(0.5f, 2.0f);
        return baseMass * variation;
    }
}
