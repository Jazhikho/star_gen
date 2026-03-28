#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Constants;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for star generation with galaxy-origin context.
/// </summary>
public static class TestStarGenerator
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests that star generation remains deterministic for the same spec and RNG.
    /// </summary>
    public static void TestDeterminism()
    {
        StarSpec spec = StarSpec.Random(12345);
        SeededRng rng1 = new SeededRng(spec.GenerationSeed);
        SeededRng rng2 = new SeededRng(spec.GenerationSeed);

        CelestialBody star1 = StarGenerator.Generate(spec, rng1);
        CelestialBody star2 = StarGenerator.Generate(spec, rng2);

        if (star1.Id != star2.Id)
        {
            throw new InvalidOperationException("star ids should match for identical inputs");
        }

        if (System.Math.Abs(star1.Physical.MassKg - star2.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException("star masses should match for identical inputs");
        }
    }

    /// <summary>
    /// Tests that different seeds still diverge.
    /// </summary>
    public static void TestDifferentSeeds()
    {
        StarSpec spec1 = StarSpec.Random(11111);
        StarSpec spec2 = StarSpec.Random(22222);

        CelestialBody star1 = StarGenerator.Generate(spec1, new SeededRng(spec1.GenerationSeed));
        CelestialBody star2 = StarGenerator.Generate(spec2, new SeededRng(spec2.GenerationSeed));

        if (star1.Id == star2.Id)
        {
            throw new InvalidOperationException("different seeds should not produce identical star ids");
        }
    }

    /// <summary>
    /// Tests generated stars still pass validation.
    /// </summary>
    public static void TestValidationPasses()
    {
        StarSpec spec = StarSpec.Random(42);
        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
        ValidationResult result = CelestialValidator.Validate(star);

        if (!result.IsValid())
        {
            throw new InvalidOperationException("generated stars should pass validation");
        }
    }

    /// <summary>
    /// Tests the classic presets still work.
    /// </summary>
    public static void TestSunLikePreset()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));

        if (!star.Stellar.SpectralClass.StartsWith("G"))
        {
            throw new InvalidOperationException("sun-like preset should still generate a G star");
        }
    }

    /// <summary>
    /// Tests that galaxy metallicity priors feed through when no explicit metallicity is set.
    /// </summary>
    public static void TestGalaxyContextBiasesMetallicity()
    {
        StarSpec metalRichSpec = StarSpec.Random(9100);
        metalRichSpec.GalaxyContext = new GalaxyOriginContext
        {
            MetallicityPrior = 1.8,
            AgeMeanGyr = 4.0,
            AgeCohort = GalaxyAgeCohort.Mature,
        };

        StarSpec metalPoorSpec = StarSpec.Random(9200);
        metalPoorSpec.GalaxyContext = new GalaxyOriginContext
        {
            MetallicityPrior = 0.35,
            AgeMeanGyr = 4.0,
            AgeCohort = GalaxyAgeCohort.Mature,
        };

        CelestialBody metalRichStar = StarGenerator.Generate(metalRichSpec, new SeededRng(metalRichSpec.GenerationSeed));
        CelestialBody metalPoorStar = StarGenerator.Generate(metalPoorSpec, new SeededRng(metalPoorSpec.GenerationSeed));

        if (metalRichStar.Stellar.Metallicity <= metalPoorStar.Stellar.Metallicity)
        {
            throw new InvalidOperationException("higher galaxy metallicity priors should not produce lower stellar metallicity");
        }
    }

    /// <summary>
    /// Tests that galaxy age context centers the stellar age distribution.
    /// </summary>
    public static void TestGalaxyContextBiasesAge()
    {
        StarSpec oldSpec = new StarSpec(20001, (int)StarClass.SpectralClass.K);
        oldSpec.GalaxyContext = new GalaxyOriginContext
        {
            AgeMeanGyr = 9.5,
            AgeCohort = GalaxyAgeCohort.Ancient,
            MetallicityPrior = 1.0,
        };

        StarSpec youngSpec = new StarSpec(20002, (int)StarClass.SpectralClass.K);
        youngSpec.GalaxyContext = new GalaxyOriginContext
        {
            AgeMeanGyr = 2.0,
            AgeCohort = GalaxyAgeCohort.Young,
            MetallicityPrior = 1.0,
        };

        CelestialBody oldStar = StarGenerator.Generate(oldSpec, new SeededRng(oldSpec.GenerationSeed));
        CelestialBody youngStar = StarGenerator.Generate(youngSpec, new SeededRng(youngSpec.GenerationSeed));

        if (oldStar.Stellar.AgeYears <= youngStar.Stellar.AgeYears)
        {
            throw new InvalidOperationException("older galaxy contexts should not produce younger stars than younger contexts for the same spectral class");
        }
    }

    /// <summary>
    /// Tests that young star-forming regions favor hotter spectral classes more often than ancient regions.
    /// </summary>
    public static void TestYoungContextsProduceHotterSpectralMix()
    {
        int youngHotCount = CountHotStarsForContext(GalaxyAgeCohort.Young, GalaxyRegionKind.SpiralArm, 30000);
        int ancientHotCount = CountHotStarsForContext(GalaxyAgeCohort.Ancient, GalaxyRegionKind.Halo, 40000);

        if (youngHotCount <= ancientHotCount)
        {
            throw new InvalidOperationException("young star-forming contexts should produce more hot stars than ancient halo contexts");
        }
    }

    /// <summary>
    /// Tests that explicit overrides still win over galaxy context.
    /// </summary>
    public static void TestOverrides()
    {
        StarSpec spec = StarSpec.Random(12345);
        spec.GalaxyContext = new GalaxyOriginContext
        {
            MetallicityPrior = 0.2,
            AgeMeanGyr = 10.0,
            AgeCohort = GalaxyAgeCohort.Ancient,
        };
        spec.SetOverride("physical.mass_solar", 2.0);
        spec.SetOverride("stellar.luminosity_solar", 10.0);

        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double luminositySolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;

        if (System.Math.Abs(massSolar - 2.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("mass override should still win");
        }

        if (System.Math.Abs(luminositySolar - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("luminosity override should still win");
        }
    }

    /// <summary>
    /// Tests that basic star typing remains correct.
    /// </summary>
    public static void TestStarType()
    {
        StarSpec spec = StarSpec.Random(12345);
        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));

        if (star.Type != CelestialType.Type.Star)
        {
            throw new InvalidOperationException("generated body should still be a star");
        }

        if (!star.HasStellar())
        {
            throw new InvalidOperationException("generated body should still have stellar properties");
        }
    }

    private static int CountHotStarsForContext(GalaxyAgeCohort ageCohort, GalaxyRegionKind regionKind, int baseSeed)
    {
        int hotCount = 0;
        for (int index = 0; index < 128; index += 1)
        {
            int seed = baseSeed + index;
            StarSpec spec = StarSpec.Random(seed);
            spec.GalaxyContext = new GalaxyOriginContext
            {
                MetallicityPrior = 1.0,
                AgeMeanGyr = ageCohort == GalaxyAgeCohort.Young ? 1.5 : 10.0,
                AgeCohort = ageCohort,
                RegionKind = regionKind,
                ClusterProbability = ageCohort == GalaxyAgeCohort.Young ? 0.45 : 0.05,
            };

            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(seed));
            string spectralLetter = star.Stellar.SpectralClass.Substring(0, 1);
            if (spectralLetter == "O" || spectralLetter == "B" || spectralLetter == "A")
            {
                hotCount += 1;
            }
        }

        return hotCount;
    }
}
