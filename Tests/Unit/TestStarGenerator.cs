#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Constants;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
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

    /// <summary>
    /// Tests that different IMF families produce measurably different mass tendencies.
    /// </summary>
    public static void TestImfFamiliesShiftAverageMass()
    {
        double kroupaAverage = CalculateAverageMassSolar(StellarImfForm.Kroupa, StellarImfVariationMode.Canonical, 41000);
        double chabrierAverage = CalculateAverageMassSolar(StellarImfForm.Chabrier, StellarImfVariationMode.Canonical, 42000);
        double kroupaSubstellarFraction = CalculateSubstellarFraction(StellarImfForm.Kroupa, StellarImfVariationMode.Canonical, 41000);
        double chabrierSubstellarFraction = CalculateSubstellarFraction(StellarImfForm.Chabrier, StellarImfVariationMode.Canonical, 42000);

        if (System.Math.Abs(kroupaAverage - chabrierAverage) < 0.002
            && System.Math.Abs(kroupaSubstellarFraction - chabrierSubstellarFraction) < 0.02)
        {
            throw new InvalidOperationException("different IMF families should not collapse to the same average mass");
        }
    }

    /// <summary>
    /// Tests that metallicity and age modulation changes the sampled stellar mass.
    /// </summary>
    public static void TestImfVariationModeChangesMassSampling()
    {
        StarSpec canonicalSpec = StarSpec.Random(43000);
        canonicalSpec.GalaxyContext = new GalaxyOriginContext
        {
            MetallicityPrior = 0.4,
            AgeMeanGyr = 1.5,
            AgeCohort = GalaxyAgeCohort.Young,
            ClusterProbability = 0.45,
        };
        canonicalSpec.StellarProfile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Kroupa,
            ImfVariationMode = StellarImfVariationMode.Canonical,
            IsochroneModel = StellarIsochroneModel.Mist,
            MultiplicityScale = 1.0,
        };

        StarSpec modulatedSpec = StarSpec.Random(43000);
        modulatedSpec.GalaxyContext = canonicalSpec.GalaxyContext.Clone();
        modulatedSpec.StellarProfile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Kroupa,
            ImfVariationMode = StellarImfVariationMode.MetallicityAgeModulated,
            IsochroneModel = StellarIsochroneModel.Mist,
            MultiplicityScale = 1.0,
        };

        CelestialBody canonicalStar = StarGenerator.Generate(canonicalSpec, new SeededRng(canonicalSpec.GenerationSeed));
        CelestialBody modulatedStar = StarGenerator.Generate(modulatedSpec, new SeededRng(modulatedSpec.GenerationSeed));

        if (System.Math.Abs(canonicalStar.Physical.MassKg - modulatedStar.Physical.MassKg) < 1.0)
        {
            throw new InvalidOperationException("IMF variation mode should change the resulting stellar mass in a biased environment");
        }
    }

    /// <summary>
    /// Tests that isochrone-model choices produce different resolved stellar properties.
    /// </summary>
    public static void TestIsochroneModelChangesResolvedProperties()
    {
        StarSpec mistSpec = StarSpec.Random(44000);
        mistSpec.SetOverride("physical.mass_solar", 1.1);
        mistSpec.StellarProfile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Kroupa,
            ImfVariationMode = StellarImfVariationMode.Canonical,
            IsochroneModel = StellarIsochroneModel.Mist,
            MultiplicityScale = 1.0,
        };

        StarSpec parsecSpec = StarSpec.Random(44000);
        parsecSpec.SetOverride("physical.mass_solar", 1.1);
        parsecSpec.StellarProfile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Kroupa,
            ImfVariationMode = StellarImfVariationMode.Canonical,
            IsochroneModel = StellarIsochroneModel.Parsec,
            MultiplicityScale = 1.0,
        };

        CelestialBody mistStar = StarGenerator.Generate(mistSpec, new SeededRng(mistSpec.GenerationSeed));
        CelestialBody parsecStar = StarGenerator.Generate(parsecSpec, new SeededRng(parsecSpec.GenerationSeed));

        if (System.Math.Abs(mistStar.Stellar.LuminosityWatts - parsecStar.Stellar.LuminosityWatts) < 1.0)
        {
            throw new InvalidOperationException("different isochrone models should produce different stellar-property outputs");
        }
    }

    /// <summary>
    /// Tests that hard spectral hints still override the profile-driven sampler.
    /// </summary>
    public static void TestSpectralHintsOverrideProfile()
    {
        StarSpec spec = new StarSpec(45000, (int)StarClass.SpectralClass.G);
        spec.StellarProfile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Chabrier,
            ImfVariationMode = StellarImfVariationMode.MetallicityAgeModulated,
            IsochroneModel = StellarIsochroneModel.Parsec,
            MultiplicityScale = 1.4,
        };

        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
        if (!star.Stellar.SpectralClass.StartsWith("G"))
        {
            throw new InvalidOperationException("explicit spectral hints should still win over the profile-driven sampler");
        }
    }

    /// <summary>
    /// Tests brown-dwarf spectral hints produce substellar outputs.
    /// </summary>
    public static void TestBrownDwarfHintsGenerateBrownDwarfs()
    {
        StarSpec spec = new StarSpec(46000, (int)StarClass.SpectralClass.L, 5, 1.0, 3.5e9);
        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
        double massSolar = star.Physical.MassKg / Units.SolarMassKg;

        if (star.Stellar.StellarType != "brown_dwarf")
        {
            throw new InvalidOperationException($"Expected a brown dwarf, got '{star.Stellar.StellarType}'");
        }
        if (!star.Stellar.SpectralClass.StartsWith("L"))
        {
            throw new InvalidOperationException($"Expected an L-dwarf label, got '{star.Stellar.SpectralClass}'");
        }
        if (massSolar >= 0.08)
        {
            throw new InvalidOperationException($"Brown dwarfs should stay below the hydrogen-burning limit, got {massSolar} solar masses");
        }
    }

    /// <summary>
    /// Tests older intermediate-mass stars can leave the main sequence without being forced into white dwarfs.
    /// </summary>
    public static void TestOldIntermediateMassStarsBecomeGiants()
    {
        StarSpec spec = new StarSpec(46010, (int)StarClass.SpectralClass.F, 5, 1.0, 2.6e9);
        spec.SetOverride("physical.mass_solar", 2.0);
        CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));

        if (star.Stellar.StellarType != "giant")
        {
            throw new InvalidOperationException($"Expected an evolved giant, got '{star.Stellar.StellarType}'");
        }
        if (star.Stellar.GetLuminosityClass() != "III")
        {
            throw new InvalidOperationException($"Expected luminosity class III, got '{star.Stellar.GetLuminosityClass()}'");
        }
    }

    /// <summary>
    /// Tests ancient mixed populations can produce white dwarfs when the sampled progenitor has aged past the main sequence.
    /// </summary>
    public static void TestAncientPopulationsProduceWhiteDwarfs()
    {
        int whiteDwarfCount = 0;
        for (int index = 0; index < 512; index += 1)
        {
            int seed = 47000 + index;
            StarSpec spec = StarSpec.Random(seed);
            spec.GalaxyContext = new GalaxyOriginContext
            {
                MetallicityPrior = 1.0,
                AgeMeanGyr = 10.5,
                AgeCohort = GalaxyAgeCohort.Ancient,
                ClusterProbability = 0.05,
            };

            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(seed));
            if (star.Stellar.StellarType == "white_dwarf")
            {
                whiteDwarfCount += 1;
                if (!star.Stellar.SpectralClass.StartsWith("D"))
                {
                    throw new InvalidOperationException($"White dwarfs should use D-class labels, got '{star.Stellar.SpectralClass}'");
                }
            }
        }

        if (whiteDwarfCount <= 0)
        {
            throw new InvalidOperationException("Ancient field populations should occasionally produce white dwarfs");
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

    private static double CalculateAverageMassSolar(StellarImfForm imfForm, StellarImfVariationMode variationMode, int baseSeed)
    {
        double totalMassSolar = 0.0;
        for (int index = 0; index < 512; index += 1)
        {
            int seed = baseSeed + index;
            StarSpec spec = StarSpec.Random(seed);
            spec.StellarProfile = new StellarGenerationProfile
            {
                ImfForm = imfForm,
                ImfVariationMode = variationMode,
                IsochroneModel = StellarIsochroneModel.Mist,
                MultiplicityScale = 1.0,
            };

            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(seed));
            totalMassSolar += star.Physical.MassKg / Units.SolarMassKg;
        }

        return totalMassSolar / 512.0;
    }

    private static double CalculateSubstellarFraction(StellarImfForm imfForm, StellarImfVariationMode variationMode, int baseSeed)
    {
        int substellarCount = 0;
        for (int index = 0; index < 512; index += 1)
        {
            int seed = baseSeed + index;
            StarSpec spec = StarSpec.Random(seed);
            spec.StellarProfile = new StellarGenerationProfile
            {
                ImfForm = imfForm,
                ImfVariationMode = variationMode,
                IsochroneModel = StellarIsochroneModel.Mist,
                MultiplicityScale = 1.0,
            };

            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(seed));
            if (star.Stellar.StellarType == "brown_dwarf")
            {
                substellarCount += 1;
            }
        }

        return (double)substellarCount / 512.0;
    }
}
