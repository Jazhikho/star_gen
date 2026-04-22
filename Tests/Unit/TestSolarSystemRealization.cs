#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

namespace StarGen.Tests.Unit;

/// <summary>
/// Probabilistic Solar-analog coverage for the scientific model settings.
/// The suite asks whether the scientific assumption families still leave room
/// for Sun-like stellar scaffolds, Solar-like planet analogs, and expected
/// moon-formation channels under Solar-neighborhood galactic context.
/// </summary>
public static class TestSolarSystemRealization
{
    private const int GalaxySeedBase = 1_240_001;
    private const int StarSeedBase = 8_640_221;
    private const int SystemSeedBase = 9_510_001;
    private const int MoonSeedBase = 9_990_001;
    private const int SunLikeSystemSampleCount = 256;
    private const int SolarAnalogSystemSampleCount = 128;
    private const int MoonSampleCount = 96;
    private const double SunLikeSystemFractionMin = 0.002;
    private const double SunLikeSystemFractionMax = 0.20;
    private const double EarthMoonAnalogFractionMin = 0.01;
    private const double EarthMoonAnalogFractionMax = 0.45;

    private static readonly SolarPlanetExpectation[] RockySolarAnalogs =
    {
        new SolarPlanetExpectation("Mercury", 0.387, 0.18, 0.02, 0.20, 0.20, 0.65, AtmosphereExpectation.Airless, false, 0.005, 0.80),
        new SolarPlanetExpectation("Venus", 0.723, 0.22, 0.30, 2.00, 0.60, 1.50, AtmosphereExpectation.Required, false, 0.005, 0.80),
        new SolarPlanetExpectation("Earth", 1.000, 0.22, 0.30, 2.00, 0.60, 1.50, AtmosphereExpectation.Required, true, 0.005, 0.80),
        new SolarPlanetExpectation("Mars", 1.524, 0.70, 0.02, 0.35, 0.20, 0.75, AtmosphereExpectation.Any, false, 0.005, 0.80),
    };

    private static readonly SolarPlanetExpectation[] GiantSolarAnalogs =
    {
        new SolarPlanetExpectation("Jupiter", 5.203, 1.50, 80.0, 4000.0, 6.0, 15.0, AtmosphereExpectation.Required, false, 0.005, 0.80),
        new SolarPlanetExpectation("Saturn", 9.537, 2.60, 80.0, 4000.0, 6.0, 15.0, AtmosphereExpectation.Required, false, 0.005, 0.80),
        new SolarPlanetExpectation("Uranus", 19.19, 6.00, 10.0, 25.0, 2.0, 4.5, AtmosphereExpectation.Required, false, 0.005, 0.80),
        new SolarPlanetExpectation("Neptune", 30.07, 8.00, 10.0, 25.0, 2.0, 4.5, AtmosphereExpectation.Required, false, 0.005, 0.80),
    };

    /// <summary>
    /// Ensures IMF-family variants still produce Sun-like stellar scaffolds at a plausible frequency.
    /// </summary>
    public static void TestSunLikeSystemProbabilityAcrossStellarImfForms()
    {
        AssertSunLikeSystemProbabilityAcrossVariants(
            "stellar IMF forms",
            new StellarImfForm[]
            {
                StellarImfForm.Kroupa,
                StellarImfForm.Chabrier,
            },
            delegate(GalaxyConfig config, StellarImfForm variant)
            {
                config.StellarProfile.ImfForm = variant;
            });
    }

    /// <summary>
    /// Ensures IMF-variation variants still produce Sun-like stellar scaffolds at a plausible frequency.
    /// </summary>
    public static void TestSunLikeSystemProbabilityAcrossStellarImfVariationModes()
    {
        AssertSunLikeSystemProbabilityAcrossVariants(
            "stellar IMF variation modes",
            new StellarImfVariationMode[]
            {
                StellarImfVariationMode.Canonical,
                StellarImfVariationMode.MetallicityAgeModulated,
            },
            delegate(GalaxyConfig config, StellarImfVariationMode variant)
            {
                config.StellarProfile.ImfVariationMode = variant;
            });
    }

    /// <summary>
    /// Ensures isochrone-model variants still produce Sun-like stellar scaffolds at a plausible frequency.
    /// </summary>
    public static void TestSunLikeSystemProbabilityAcrossStellarIsochroneModels()
    {
        AssertSunLikeSystemProbabilityAcrossVariants(
            "stellar isochrone models",
            new StellarIsochroneModel[]
            {
                StellarIsochroneModel.Mist,
                StellarIsochroneModel.Parsec,
            },
            delegate(GalaxyConfig config, StellarIsochroneModel variant)
            {
                config.StellarProfile.IsochroneModel = variant;
            });
    }

    /// <summary>
    /// Ensures multiplicity-scale variants still leave room for Sun-like single-star systems.
    /// </summary>
    public static void TestSunLikeSystemProbabilityAcrossMultiplicityScales()
    {
        AssertSunLikeSystemProbabilityAcrossVariants(
            "multiplicity scales",
            new double[] { 0.45, 1.0, 1.75 },
            delegate(GalaxyConfig config, double variant)
            {
                config.StellarProfile.MultiplicityScale = variant;
            });
    }

    /// <summary>
    /// Ensures rocky inner Solar analogs remain possible across mass-radius models.
    /// </summary>
    public static void TestRockySolarAnalogFormationAcrossPlanetMassRadiusModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "planet mass-radius models",
            RockySolarAnalogs,
            new PlanetMassRadiusModel[]
            {
                PlanetMassRadiusModel.ChenKipping,
                PlanetMassRadiusModel.Otegi,
            },
            delegate(GalaxyConfig config, PlanetMassRadiusModel variant)
            {
                config.PlanetaryProfile.MassRadiusModel = variant;
            });
    }

    /// <summary>
    /// Ensures rocky inner Solar analogs remain possible across envelope-loss models.
    /// </summary>
    public static void TestRockySolarAnalogFormationAcrossEnvelopeLossModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "envelope-loss models",
            RockySolarAnalogs,
            new PlanetEnvelopeLossModel[]
            {
                PlanetEnvelopeLossModel.Auto,
                PlanetEnvelopeLossModel.Photoevaporation,
                PlanetEnvelopeLossModel.CorePowered,
            },
            delegate(GalaxyConfig config, PlanetEnvelopeLossModel variant)
            {
                config.PlanetaryProfile.EnvelopeLossModel = variant;
            });
    }

    /// <summary>
    /// Ensures giant and ice-giant Solar analogs remain possible across gas-giant formation models.
    /// </summary>
    public static void TestGiantSolarAnalogFormationAcrossGasGiantFormationModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "gas-giant formation models",
            GiantSolarAnalogs,
            new GasGiantFormationModel[]
            {
                GasGiantFormationModel.CoreAccretion,
                GasGiantFormationModel.PebbleAssisted,
                GasGiantFormationModel.Mixed,
            },
            delegate(GalaxyConfig config, GasGiantFormationModel variant)
            {
                config.PlanetaryProfile.GasGiantFormationModel = variant;
            });
    }

    /// <summary>
    /// Ensures giant and ice-giant Solar analogs remain possible across metallicity-coupling strengths.
    /// </summary>
    public static void TestGiantSolarAnalogFormationAcrossMetallicityCouplingModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "metallicity coupling strengths",
            GiantSolarAnalogs,
            new PlanetMetallicityCouplingStrength[]
            {
                PlanetMetallicityCouplingStrength.Weak,
                PlanetMetallicityCouplingStrength.ObservedDefault,
                PlanetMetallicityCouplingStrength.Strong,
            },
            delegate(GalaxyConfig config, PlanetMetallicityCouplingStrength variant)
            {
                config.PlanetaryProfile.MetallicityCouplingStrength = variant;
            });
    }

    /// <summary>
    /// Ensures giant and ice-giant Solar analogs remain possible across rogue-planet allowance models.
    /// </summary>
    public static void TestGiantSolarAnalogFormationAcrossRogueAllowanceModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "rogue-planet allowance models",
            GiantSolarAnalogs,
            new PlanetRoguePlanetAllowance[]
            {
                PlanetRoguePlanetAllowance.Off,
                PlanetRoguePlanetAllowance.Rare,
                PlanetRoguePlanetAllowance.Standard,
            },
            delegate(GalaxyConfig config, PlanetRoguePlanetAllowance variant)
            {
                config.PlanetaryProfile.RoguePlanetAllowance = variant;
            });
    }

    /// <summary>
    /// Ensures outer Solar analogs remain possible across outer-system small-body bias models.
    /// </summary>
    public static void TestGiantSolarAnalogFormationAcrossOuterSystemBiasModels()
    {
        AssertPlanetAnalogsPossibleAcrossVariants(
            "outer-system small-body bias models",
            GiantSolarAnalogs,
            new PlanetMinorBodyOuterSystemBias[]
            {
                PlanetMinorBodyOuterSystemBias.AsteroidLeaning,
                PlanetMinorBodyOuterSystemBias.Balanced,
                PlanetMinorBodyOuterSystemBias.CometLeaning,
            },
            delegate(GalaxyConfig config, PlanetMinorBodyOuterSystemBias variant)
            {
                config.PlanetaryProfile.MinorBodyOuterSystemBias = variant;
            });
    }

    /// <summary>
    /// Ensures Earth-Moon-like outcomes remain possible across moon-formation biases.
    /// </summary>
    public static void TestEarthMoonAnalogsRemainPossibleAcrossMoonFormationBiases()
    {
        System.Collections.Generic.Dictionary<PlanetMoonFormationBias, TerrestrialMoonStats> statsByBias = new System.Collections.Generic.Dictionary<PlanetMoonFormationBias, TerrestrialMoonStats>();
        PlanetMoonFormationBias[] variants =
        {
            PlanetMoonFormationBias.RegularDiskFavored,
            PlanetMoonFormationBias.CapturedRich,
            PlanetMoonFormationBias.Mixed,
        };

        foreach (PlanetMoonFormationBias variant in variants)
        {
            GalaxyConfig config = CreateBaselineGalaxyConfig();
            config.PlanetaryProfile.MoonFormationBias = variant;
            TerrestrialMoonStats stats = MeasureTerrestrialMoonStats(config, MoonSampleCount);
            statsByBias[variant] = stats;

            if (stats.EarthMoonAnalogCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Moon formation bias {variant} did not produce any Earth-Moon analogs across {stats.Samples} Earth-like samples.");
            }

            double fraction = (double)stats.EarthMoonAnalogCount / (double)stats.Samples;
            if (fraction < EarthMoonAnalogFractionMin || fraction > EarthMoonAnalogFractionMax)
            {
                throw new InvalidOperationException(
                    $"Moon formation bias {variant} produced Earth-Moon analogs at fraction {fraction:0.000}, outside the broad expected band [{EarthMoonAnalogFractionMin:0.000}, {EarthMoonAnalogFractionMax:0.000}]. Captured moons: {stats.CapturedMoonCount}. Systems with any moon: {stats.SystemsWithAnyMoons}.");
            }
        }

        if (statsByBias[PlanetMoonFormationBias.RegularDiskFavored].EarthMoonAnalogCount
            < statsByBias[PlanetMoonFormationBias.CapturedRich].EarthMoonAnalogCount)
        {
            throw new InvalidOperationException(
                "Regular-disk-favored moon bias should not produce fewer Earth-Moon analogs than captured-rich bias across the same Earth-like sample.");
        }
    }

    /// <summary>
    /// Ensures gas-giant moon systems remain possible and respond to moon-formation bias.
    /// </summary>
    public static void TestGasGiantMoonSystemsRespectMoonFormationBiases()
    {
        System.Collections.Generic.Dictionary<PlanetMoonFormationBias, GasGiantMoonStats> statsByBias = new System.Collections.Generic.Dictionary<PlanetMoonFormationBias, GasGiantMoonStats>();
        PlanetMoonFormationBias[] variants =
        {
            PlanetMoonFormationBias.RegularDiskFavored,
            PlanetMoonFormationBias.CapturedRich,
            PlanetMoonFormationBias.Mixed,
        };

        foreach (PlanetMoonFormationBias variant in variants)
        {
            GalaxyConfig config = CreateBaselineGalaxyConfig();
            config.PlanetaryProfile.MoonFormationBias = variant;
            GasGiantMoonStats stats = MeasureGasGiantMoonStats(config, MoonSampleCount);
            statsByBias[variant] = stats;

            if (stats.MoonRichSystemCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Moon formation bias {variant} did not produce any moon-rich gas-giant systems across {stats.Samples} Jupiter-like samples.");
            }

            if (stats.TotalMoonCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Moon formation bias {variant} produced no gas-giant moons across {stats.Samples} Jupiter-like samples.");
            }
        }

        if (statsByBias[PlanetMoonFormationBias.CapturedRich].CapturedMoonCount
            <= statsByBias[PlanetMoonFormationBias.RegularDiskFavored].CapturedMoonCount)
        {
            throw new InvalidOperationException(
                "Captured-rich moon bias should produce more captured moons than regular-disk-favored bias across the same Jupiter-like sample.");
        }
    }

    private static void AssertSunLikeSystemProbabilityAcrossVariants<TVariant>(
        string familyLabel,
        TVariant[] variants,
        Action<GalaxyConfig, TVariant> applyVariant)
    {
        for (int variantIndex = 0; variantIndex < variants.Length; variantIndex += 1)
        {
            TVariant variant = variants[variantIndex];
            GalaxyConfig config = CreateBaselineGalaxyConfig();
            applyVariant(config, variant);

            int generatedSystems = 0;
            int sunLikeSystems = 0;
            string closestSummary = "no systems generated";
            double bestDistanceScore = double.MaxValue;

            for (int sampleIndex = 0; sampleIndex < SunLikeSystemSampleCount; sampleIndex += 1)
            {
                int galaxySeed = GalaxySeedBase + (variantIndex * 5000) + sampleIndex;
                int starSeed = StarSeedBase + (variantIndex * 5000) + sampleIndex;
                SolarSystem? system = GenerateGalaxyDrivenStellarScaffold(config, galaxySeed, starSeed);
                if (system == null)
                {
                    throw new InvalidOperationException(
                        $"Sun-like probability sampling failed during stellar scaffold generation for {familyLabel} variant {variant} at sample {sampleIndex}.");
                }

                generatedSystems += 1;

                if (IsSunLikeSingleStarSystem(system))
                {
                    sunLikeSystems += 1;
                    continue;
                }

                SunLikeCandidateSummary candidate = FindClosestSunLikeCandidate(system);
                if (candidate.DistanceScore < bestDistanceScore)
                {
                    bestDistanceScore = candidate.DistanceScore;
                    closestSummary = candidate.Summary;
                }
            }

            if (generatedSystems != SunLikeSystemSampleCount)
            {
                throw new InvalidOperationException(
                    $"Sun-like probability sampling for {familyLabel} variant {variant} only generated {generatedSystems} stellar scaffolds out of {SunLikeSystemSampleCount} requested samples.");
            }

            double fraction = (double)sunLikeSystems / (double)generatedSystems;
            if (fraction < SunLikeSystemFractionMin || fraction > SunLikeSystemFractionMax)
            {
                throw new InvalidOperationException(
                    $"Sun-like stellar scaffolds for {familyLabel} variant {variant} occurred at fraction {fraction:0.000}, outside the broad expected band [{SunLikeSystemFractionMin:0.000}, {SunLikeSystemFractionMax:0.000}]. Closest miss: {closestSummary}.");
            }
        }
    }

    private static void AssertPlanetAnalogsPossibleAcrossVariants<TVariant>(
        string familyLabel,
        SolarPlanetExpectation[] expectations,
        TVariant[] variants,
        Action<GalaxyConfig, TVariant> applyVariant)
    {
        for (int variantIndex = 0; variantIndex < variants.Length; variantIndex += 1)
        {
            TVariant variant = variants[variantIndex];
            GalaxyConfig config = CreateBaselineGalaxyConfig();
            applyVariant(config, variant);

            System.Collections.Generic.Dictionary<string, AnalogOccurrenceStats> statsByAnalog = CreateAnalogStats(expectations);

            for (int sampleIndex = 0; sampleIndex < SolarAnalogSystemSampleCount; sampleIndex += 1)
            {
                int galaxySeed = GalaxySeedBase + (variantIndex * 7000) + sampleIndex;
                int starSeed = StarSeedBase + (variantIndex * 7000) + sampleIndex;
                int systemSeed = SystemSeedBase + (variantIndex * 7000) + sampleIndex;
                SolarSystemSpec spec = CreateSunLikeSpecFromGalaxyContext(config, galaxySeed, starSeed, systemSeed);
                SolarSystem? system = SystemFixtureGenerator.GenerateSystem(spec, false);
                if (system == null)
                {
                    throw new InvalidOperationException(
                        $"Planet analog sampling failed during system generation for {familyLabel} variant {variant} at sample {sampleIndex}.");
                }

                foreach (SolarPlanetExpectation expectation in expectations)
                {
                    AnalogOccurrenceStats stats = statsByAnalog[expectation.Name];
                    stats.SystemsSampled += 1;

                    if (TryFindMatchingPlanet(system, expectation, out CelestialBody? _))
                    {
                        stats.MatchCount += 1;
                        continue;
                    }

                    ClosestPlanetCandidateSummary closest = FindClosestPlanetCandidate(system, expectation);
                    if (closest.DistanceScore < stats.BestDistanceScore)
                    {
                        stats.BestDistanceScore = closest.DistanceScore;
                        stats.ClosestMissSummary = closest.Summary;
                    }
                }
            }

            foreach (SolarPlanetExpectation expectation in expectations)
            {
                AnalogOccurrenceStats stats = statsByAnalog[expectation.Name];
                if (stats.SystemsSampled != SolarAnalogSystemSampleCount)
                {
                    throw new InvalidOperationException(
                        $"Planet analog sampling for {familyLabel} variant {variant} only evaluated {stats.SystemsSampled} systems for {expectation.Name}.");
                }

                double fraction = (double)stats.MatchCount / (double)stats.SystemsSampled;
                if (fraction < expectation.MinOccurrenceFraction || fraction > expectation.MaxOccurrenceFraction)
                {
                    throw new InvalidOperationException(
                        $"Planet analog {expectation.Name} under {familyLabel} variant {variant} occurred at fraction {fraction:0.000}, outside the broad expected band [{expectation.MinOccurrenceFraction:0.000}, {expectation.MaxOccurrenceFraction:0.000}]. Closest miss: {stats.ClosestMissSummary}.");
                }
            }
        }
    }

    private static GalaxyConfig CreateBaselineGalaxyConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.UseCaseSettings = GenerationUseCaseSettings.CreateDefault();
        return config;
    }

    private static SolarSystem? GenerateGalaxyDrivenStellarScaffold(GalaxyConfig config, int galaxySeed, int starSeed)
    {
        GalaxySpec galaxySpec = GalaxySpec.CreateFromConfig(config, galaxySeed);
        Vector3 homePosition = HomePosition.GetDefaultPosition();
        GalaxyStar derivedStar = GalaxyStar.CreateWithDerivedProperties(homePosition, starSeed, galaxySpec);

        SolarSystemSpec spec = SolarSystemSpec.RandomSmall(starSeed);
        spec.SystemMetallicity = derivedStar.Metallicity;
        spec.SystemAgeYears = derivedStar.OriginContext.AgeMeanGyr * 1.0e9;
        spec.GalaxyContext = derivedStar.OriginContext.Clone();
        spec.StellarProfile = config.StellarProfile.Clone();
        spec.PlanetaryProfile = config.PlanetaryProfile.Clone();
        spec.IncludeAsteroidBelts = false;
        spec.GeneratePopulation = false;

        return StellarConfigGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
    }

    private static SolarSystemSpec CreateSunLikeSpecFromGalaxyContext(GalaxyConfig config, int galaxySeed, int starSeed, int systemSeed)
    {
        GalaxySpec galaxySpec = GalaxySpec.CreateFromConfig(config, galaxySeed);
        Vector3 homePosition = HomePosition.GetDefaultPosition();
        GalaxyStar derivedStar = GalaxyStar.CreateWithDerivedProperties(homePosition, starSeed, galaxySpec);

        SolarSystemSpec spec = SolarSystemSpec.SunLike(systemSeed);
        spec.NameHint = "solar_statistical_probe";
        spec.SystemMetallicity = derivedStar.Metallicity;
        spec.SystemAgeYears = derivedStar.OriginContext.AgeMeanGyr * 1.0e9;
        spec.GalaxyContext = derivedStar.OriginContext.Clone();
        spec.StellarProfile = config.StellarProfile.Clone();
        spec.PlanetaryProfile = config.PlanetaryProfile.Clone();
        spec.IncludeAsteroidBelts = false;
        spec.GeneratePopulation = false;
        return spec;
    }

    private static bool IsSunLikeSingleStarSystem(SolarSystem system)
    {
        if (system.GetStarCount() != 1)
        {
            return false;
        }

        Array<CelestialBody> stars = system.GetStars();
        if (stars.Count != 1)
        {
            return false;
        }

        return IsSunLikeStar(stars[0]);
    }

    private static bool IsSunLikeStar(CelestialBody star)
    {
        if (!star.HasStellar() || star.Stellar == null)
        {
            return false;
        }

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double radiusSolar = star.Physical.RadiusM / Units.SolarRadiusMeters;
        double luminositySolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;
        double ageGyr = star.Stellar.AgeYears / 1.0e9;
        double temperatureK = star.Stellar.EffectiveTemperatureK;

        if (massSolar < 0.80 || massSolar > 1.20)
        {
            return false;
        }

        if (radiusSolar < 0.75 || radiusSolar > 1.35)
        {
            return false;
        }

        if (luminositySolar < 0.35 || luminositySolar > 2.20)
        {
            return false;
        }

        if (ageGyr < 1.0 || ageGyr > 10.0)
        {
            return false;
        }

        if (temperatureK < 5000.0 || temperatureK > 6500.0)
        {
            return false;
        }

        string spectralClass = star.Stellar.SpectralClass ?? string.Empty;
        if (string.IsNullOrWhiteSpace(spectralClass))
        {
            return false;
        }

        char first = char.ToUpperInvariant(spectralClass[0]);
        if (first != 'F' && first != 'G' && first != 'K')
        {
            return false;
        }

        return true;
    }

    private static SunLikeCandidateSummary FindClosestSunLikeCandidate(SolarSystem system)
    {
        Array<CelestialBody> stars = system.GetStars();
        if (stars.Count == 0)
        {
            return new SunLikeCandidateSummary("no stars generated", double.MaxValue);
        }

        double bestScore = double.MaxValue;
        string bestSummary = string.Empty;

        foreach (CelestialBody star in stars)
        {
            double score = ScoreSunLikeDistance(star);
            if (score < bestScore)
            {
                bestScore = score;
                bestSummary = DescribeStarCandidate(system, star);
            }
        }

        return new SunLikeCandidateSummary(bestSummary, bestScore);
    }

    private static double ScoreSunLikeDistance(CelestialBody star)
    {
        if (!star.HasStellar() || star.Stellar == null)
        {
            return double.MaxValue / 4.0;
        }

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double radiusSolar = star.Physical.RadiusM / Units.SolarRadiusMeters;
        double luminositySolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;
        double temperatureK = star.Stellar.EffectiveTemperatureK;

        double score = 0.0;
        score += Math.Abs(massSolar - 1.0) * 3.0;
        score += Math.Abs(radiusSolar - 1.0) * 2.0;
        score += Math.Abs(luminositySolar - 1.0);
        score += Math.Abs(temperatureK - 5778.0) / 3000.0;
        return score;
    }

    private static string DescribeStarCandidate(SolarSystem system, CelestialBody star)
    {
        if (!star.HasStellar() || star.Stellar == null)
        {
            return $"system with {system.GetStarCount()} stars where one candidate lacked stellar metadata";
        }

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double radiusSolar = star.Physical.RadiusM / Units.SolarRadiusMeters;
        double luminositySolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;
        double ageGyr = star.Stellar.AgeYears / 1.0e9;
        return $"system had {system.GetStarCount()} stars; closest primary candidate was spectral {star.Stellar.SpectralClass}, mass {massSolar:0.00} Msun, radius {radiusSolar:0.00} Rsun, luminosity {luminositySolar:0.00} Lsun, age {ageGyr:0.00} Gyr";
    }

    private static System.Collections.Generic.Dictionary<string, AnalogOccurrenceStats> CreateAnalogStats(SolarPlanetExpectation[] expectations)
    {
        System.Collections.Generic.Dictionary<string, AnalogOccurrenceStats> statsByAnalog = new System.Collections.Generic.Dictionary<string, AnalogOccurrenceStats>(StringComparer.Ordinal);
        foreach (SolarPlanetExpectation expectation in expectations)
        {
            statsByAnalog[expectation.Name] = new AnalogOccurrenceStats();
        }

        return statsByAnalog;
    }

    private static bool TryFindMatchingPlanet(SolarSystem system, SolarPlanetExpectation expectation, out CelestialBody? matchingPlanet)
    {
        matchingPlanet = null;
        Array<CelestialBody> planets = system.GetPlanets();
        foreach (CelestialBody planet in planets)
        {
            if (PlanetMatchesExpectation(planet, expectation))
            {
                matchingPlanet = planet;
                return true;
            }
        }

        return false;
    }

    private static bool PlanetMatchesExpectation(CelestialBody planet, SolarPlanetExpectation expectation)
    {
        if (!planet.HasOrbital())
        {
            return false;
        }

        double orbitAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
        if (Math.Abs(orbitAu - expectation.TargetSemiMajorAxisAu) > expectation.OrbitToleranceAu)
        {
            return false;
        }

        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        if (massEarth < expectation.MinMassEarth || massEarth > expectation.MaxMassEarth)
        {
            return false;
        }

        double radiusEarth = planet.Physical.RadiusM / Units.EarthRadiusMeters;
        if (radiusEarth < expectation.MinRadiusEarth || radiusEarth > expectation.MaxRadiusEarth)
        {
            return false;
        }

        if (expectation.AtmosphereRequirement == AtmosphereExpectation.Required)
        {
            if (!planet.HasAtmosphere() || planet.Atmosphere == null || planet.Atmosphere.SurfacePressurePa < 100.0)
            {
                return false;
            }
        }

        if (expectation.AtmosphereRequirement == AtmosphereExpectation.Airless)
        {
            if (planet.HasAtmosphere() && planet.Atmosphere != null && planet.Atmosphere.SurfacePressurePa >= 100.0)
            {
                return false;
            }
        }

        if (expectation.RequiresHydrosphere)
        {
            if (!planet.HasSurface() || planet.Surface == null || !planet.Surface.HasHydrosphere() || planet.Surface.Hydrosphere == null)
            {
                return false;
            }

            if (planet.Surface.Hydrosphere.GetLiquidCoverage() <= 0.05)
            {
                return false;
            }
        }

        return true;
    }

    private static ClosestPlanetCandidateSummary FindClosestPlanetCandidate(SolarSystem system, SolarPlanetExpectation expectation)
    {
        Array<CelestialBody> planets = system.GetPlanets();
        if (planets.Count == 0)
        {
            return new ClosestPlanetCandidateSummary("system generated no planets", double.MaxValue);
        }

        double bestScore = double.MaxValue;
        string bestSummary = string.Empty;

        foreach (CelestialBody planet in planets)
        {
            double score = ScorePlanetDistance(planet, expectation);
            if (score < bestScore)
            {
                bestScore = score;
                bestSummary = DescribePlanetCandidate(planet, expectation);
            }
        }

        return new ClosestPlanetCandidateSummary(bestSummary, bestScore);
    }

    private static double ScorePlanetDistance(CelestialBody planet, SolarPlanetExpectation expectation)
    {
        if (!planet.HasOrbital())
        {
            return double.MaxValue / 8.0;
        }

        double orbitAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        double radiusEarth = planet.Physical.RadiusM / Units.EarthRadiusMeters;

        double orbitScore = Math.Abs(orbitAu - expectation.TargetSemiMajorAxisAu) / Math.Max(expectation.OrbitToleranceAu, 0.01);
        double massScore = ScoreRangePenalty(massEarth, expectation.MinMassEarth, expectation.MaxMassEarth);
        double radiusScore = ScoreRangePenalty(radiusEarth, expectation.MinRadiusEarth, expectation.MaxRadiusEarth);
        double atmosphereScore = 0.0;
        double hydrosphereScore = 0.0;

        if (expectation.AtmosphereRequirement == AtmosphereExpectation.Required)
        {
            if (!planet.HasAtmosphere() || planet.Atmosphere == null || planet.Atmosphere.SurfacePressurePa < 100.0)
            {
                atmosphereScore = 2.0;
            }
        }

        if (expectation.AtmosphereRequirement == AtmosphereExpectation.Airless)
        {
            if (planet.HasAtmosphere() && planet.Atmosphere != null && planet.Atmosphere.SurfacePressurePa >= 100.0)
            {
                atmosphereScore = 2.0;
            }
        }

        if (expectation.RequiresHydrosphere)
        {
            if (!planet.HasSurface() || planet.Surface == null || !planet.Surface.HasHydrosphere() || planet.Surface.Hydrosphere == null)
            {
                hydrosphereScore = 2.0;
            }
            else if (planet.Surface.Hydrosphere.GetLiquidCoverage() <= 0.05)
            {
                hydrosphereScore = 1.0;
            }
        }

        return orbitScore + massScore + radiusScore + atmosphereScore + hydrosphereScore;
    }

    private static double ScoreRangePenalty(double value, double min, double max)
    {
        if (value < min)
        {
            return (min - value) / Math.Max(min, 0.01);
        }

        if (value > max)
        {
            return (value - max) / Math.Max(max, 0.01);
        }

        return 0.0;
    }

    private static string DescribePlanetCandidate(CelestialBody planet, SolarPlanetExpectation expectation)
    {
        if (!planet.HasOrbital())
        {
            return $"{planet.Name} lacked orbital data while searching for {expectation.Name}";
        }

        double orbitAu = planet.Orbital!.SemiMajorAxisM / Units.AuMeters;
        double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
        double radiusEarth = planet.Physical.RadiusM / Units.EarthRadiusMeters;
        string atmosphereSummary = "none";
        if (planet.HasAtmosphere() && planet.Atmosphere != null && planet.Atmosphere.SurfacePressurePa >= 100.0)
        {
            atmosphereSummary = $"{planet.Atmosphere.SurfacePressurePa / 101325.0:0.00} atm";
        }

        string hydrosphereSummary = "none";
        if (planet.HasSurface() && planet.Surface != null && planet.Surface.HasHydrosphere() && planet.Surface.Hydrosphere != null)
        {
            hydrosphereSummary = $"{planet.Surface.Hydrosphere.GetLiquidCoverage():0.00} liquid coverage";
        }

        return $"{planet.Name} at {orbitAu:0.000} AU, mass {massEarth:0.000} Earth, radius {radiusEarth:0.000} Earth, atmosphere {atmosphereSummary}, hydrosphere {hydrosphereSummary}";
    }

    private static TerrestrialMoonStats MeasureTerrestrialMoonStats(GalaxyConfig config, int sampleCount)
    {
        TerrestrialMoonStats stats = new TerrestrialMoonStats();

        for (int index = 0; index < sampleCount; index += 1)
        {
            int sampleSeed = MoonSeedBase + index;
            CelestialBody planet = CreateEarthLikePlanet();
            CelestialBody star = CreateSunLikeTestStar();
            SolarSystemSpec spec = SolarSystemSpec.SingleStar(sampleSeed);
            spec.PlanetaryProfile = config.PlanetaryProfile.Clone();
            spec.StellarProfile = config.StellarProfile.Clone();
            spec.IncludeAsteroidBelts = false;
            spec.GeneratePopulation = false;

            MoonGenerationResult result = SystemMoonGenerator.Generate(
                new Array<CelestialBody> { planet },
                new Array<OrbitHost>(),
                new Array<CelestialBody> { star },
                new SeededRng(sampleSeed),
                false,
                null,
                spec);

            stats.Samples += 1;
            if (result.Moons.Count > 0)
            {
                stats.SystemsWithAnyMoons += 1;
            }

            stats.CapturedMoonCount += CountCapturedMoons(result.Moons);
            if (IsEarthMoonAnalog(planet, star, result.Moons))
            {
                stats.EarthMoonAnalogCount += 1;
            }
        }

        return stats;
    }

    private static GasGiantMoonStats MeasureGasGiantMoonStats(GalaxyConfig config, int sampleCount)
    {
        GasGiantMoonStats stats = new GasGiantMoonStats();

        for (int index = 0; index < sampleCount; index += 1)
        {
            int sampleSeed = MoonSeedBase + 5000 + index;
            CelestialBody planet = CreateJupiterLikePlanet();
            CelestialBody star = CreateSunLikeTestStar();
            SolarSystemSpec spec = SolarSystemSpec.SingleStar(sampleSeed);
            spec.PlanetaryProfile = config.PlanetaryProfile.Clone();
            spec.StellarProfile = config.StellarProfile.Clone();
            spec.IncludeAsteroidBelts = false;
            spec.GeneratePopulation = false;

            MoonGenerationResult result = SystemMoonGenerator.Generate(
                new Array<CelestialBody> { planet },
                new Array<OrbitHost>(),
                new Array<CelestialBody> { star },
                new SeededRng(sampleSeed),
                false,
                null,
                spec);

            int captured = CountCapturedMoons(result.Moons);
            stats.Samples += 1;
            stats.TotalMoonCount += result.Moons.Count;
            stats.CapturedMoonCount += captured;
            stats.RegularMoonCount += result.Moons.Count - captured;
            if (result.Moons.Count >= 4)
            {
                stats.MoonRichSystemCount += 1;
            }
        }

        return stats;
    }

    private static CelestialBody CreateSunLikeTestStar()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(spec, rng);
        star.Id = "test_star";
        star.Name = "Sun";
        return star;
    }

    private static CelestialBody CreateEarthLikePlanet()
    {
        CelestialBody planet = new CelestialBody("earth_like_1", "Earth Analog", CelestialType.Type.Planet);
        planet.Physical = new PhysicalProps(
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            86400.0,
            23.5,
            0.003,
            1.0e15,
            1.0e13);
        planet.Orbital = new OrbitalProps(
            Units.AuMeters,
            0.017,
            0.0,
            0.0,
            0.0,
            0.0,
            "test_star");
        return planet;
    }

    private static CelestialBody CreateJupiterLikePlanet()
    {
        CelestialBody planet = new CelestialBody("jupiter_like_1", "Jupiter Analog", CelestialType.Type.Planet);
        planet.Physical = new PhysicalProps(
            Units.JupiterMassKg,
            Units.JupiterRadiusMeters,
            35730.0,
            3.0,
            0.06,
            1.0e20,
            1.0e17);
        planet.Orbital = new OrbitalProps(
            5.2 * Units.AuMeters,
            0.05,
            1.3,
            0.0,
            0.0,
            0.0,
            "test_star");
        return planet;
    }

    private static int CountCapturedMoons(Array<CelestialBody> moons)
    {
        int count = 0;
        foreach (CelestialBody moon in moons)
        {
            if (moon.Name.Contains("(captured)", StringComparison.Ordinal))
            {
                count += 1;
            }
        }

        return count;
    }

    private static bool IsEarthMoonAnalog(CelestialBody planet, CelestialBody star, Array<CelestialBody> moons)
    {
        if (moons.Count != 1)
        {
            return false;
        }

        CelestialBody moon = moons[0];
        if (moon.Name.Contains("(captured)", StringComparison.Ordinal))
        {
            return false;
        }

        double moonMassRatio = moon.Physical.MassKg / planet.Physical.MassKg;
        if (moonMassRatio < 0.005 || moonMassRatio > 0.03)
        {
            return false;
        }

        if (!moon.HasOrbital())
        {
            return false;
        }

        double hillRadius = OrbitalMechanics.CalculateHillSphere(
            planet.Physical.MassKg,
            star.Physical.MassKg,
            planet.Orbital!.SemiMajorAxisM);
        double orbitalDistance = moon.Orbital!.SemiMajorAxisM;
        if (orbitalDistance <= planet.Physical.RadiusM * 10.0)
        {
            return false;
        }

        if (orbitalDistance >= hillRadius * 0.49)
        {
            return false;
        }

        return true;
    }

    private sealed class SolarPlanetExpectation
    {
        public string Name;
        public double TargetSemiMajorAxisAu;
        public double OrbitToleranceAu;
        public double MinMassEarth;
        public double MaxMassEarth;
        public double MinRadiusEarth;
        public double MaxRadiusEarth;
        public AtmosphereExpectation AtmosphereRequirement;
        public bool RequiresHydrosphere;
        public double MinOccurrenceFraction;
        public double MaxOccurrenceFraction;

        public SolarPlanetExpectation(
            string name,
            double targetSemiMajorAxisAu,
            double orbitToleranceAu,
            double minMassEarth,
            double maxMassEarth,
            double minRadiusEarth,
            double maxRadiusEarth,
            AtmosphereExpectation atmosphereRequirement,
            bool requiresHydrosphere,
            double minOccurrenceFraction,
            double maxOccurrenceFraction)
        {
            Name = name;
            TargetSemiMajorAxisAu = targetSemiMajorAxisAu;
            OrbitToleranceAu = orbitToleranceAu;
            MinMassEarth = minMassEarth;
            MaxMassEarth = maxMassEarth;
            MinRadiusEarth = minRadiusEarth;
            MaxRadiusEarth = maxRadiusEarth;
            AtmosphereRequirement = atmosphereRequirement;
            RequiresHydrosphere = requiresHydrosphere;
            MinOccurrenceFraction = minOccurrenceFraction;
            MaxOccurrenceFraction = maxOccurrenceFraction;
        }
    }

    private enum AtmosphereExpectation
    {
        Any,
        Required,
        Airless,
    }

    private sealed class AnalogOccurrenceStats
    {
        public int SystemsSampled;
        public int MatchCount;
        public string ClosestMissSummary = "no candidate recorded";
        public double BestDistanceScore = double.MaxValue;
    }

    private sealed class TerrestrialMoonStats
    {
        public int Samples;
        public int SystemsWithAnyMoons;
        public int EarthMoonAnalogCount;
        public int CapturedMoonCount;
    }

    private sealed class GasGiantMoonStats
    {
        public int Samples;
        public int MoonRichSystemCount;
        public int TotalMoonCount;
        public int CapturedMoonCount;
        public int RegularMoonCount;
    }

    private sealed class SunLikeCandidateSummary
    {
        public string Summary;
        public double DistanceScore;

        public SunLikeCandidateSummary(string summary, double distanceScore)
        {
            Summary = summary;
            DistanceScore = distanceScore;
        }
    }

    private sealed class ClosestPlanetCandidateSummary
    {
        public string Summary;
        public double DistanceScore;

        public ClosestPlanetCandidateSummary(string summary, double distanceScore)
        {
            Summary = summary;
            DistanceScore = distanceScore;
        }
    }
}
