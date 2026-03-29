using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Generates star celestial bodies from star specifications.
/// </summary>
public static class StarGenerator
{
    private const double HydrogenBurningLimitSolar = 0.075;
    private const double WhiteDwarfMinProgenitorMassSolar = 0.85;
    private const double WhiteDwarfMaxProgenitorMassSolar = 7.5;

    private enum StellarEvolutionStage
    {
        BrownDwarf,
        MainSequence,
        Subgiant,
        Giant,
        Supergiant,
        WhiteDwarf,
    }

    /// <summary>
    /// Generates a star body from a specification and RNG.
    /// </summary>
    public static CelestialBody Generate(StarSpec spec, SeededRng rng)
    {
        double metallicity = DetermineMetallicity(spec, rng);
        double sampledMassSolar = ResolveInitialMass(spec, rng);
        StarClass.SpectralClass hintedSpectralClass = DetermineSpectralClass(spec, sampledMassSolar, metallicity);
        int hintedSubclass = DetermineSubclass(spec, hintedSpectralClass, sampledMassSolar, rng);
        double progenitorMassSolar = CalculateMass(spec, hintedSpectralClass, hintedSubclass, metallicity, sampledMassSolar, rng);
        double ageYears = DetermineAge(spec, progenitorMassSolar, hintedSpectralClass, rng);
        bool hasHardMassOverride = spec.GetOverrideFloat("physical.mass_solar", -1.0) > 0.0;
        bool allowWhiteDwarf = !spec.HasSpectralClass() && !hasHardMassOverride;
        StellarEvolutionStage stage = ResolveEvolutionStage(progenitorMassSolar, ageYears, allowWhiteDwarf);

        double physicalMassSolar = progenitorMassSolar;
        if (stage == StellarEvolutionStage.WhiteDwarf)
        {
            physicalMassSolar = EstimateWhiteDwarfMassSolar(progenitorMassSolar);
        }

        StellarModelResult modelResult = ResolveStellarModel(
            spec,
            physicalMassSolar,
            ageYears,
            metallicity,
            stage,
            progenitorMassSolar,
            rng);
        double luminositySolar = modelResult.LuminositySolar;
        double radiusSolar = modelResult.RadiusSolar;
        double temperatureK = modelResult.TemperatureK;

        StarClass.SpectralClass spectralClass = hintedSpectralClass;
        int subclass = hintedSubclass;
        if (!spec.HasSpectralClass() && stage != StellarEvolutionStage.WhiteDwarf)
        {
            spectralClass = StarTable.ClassFromTemperature(temperatureK);
            if (!spec.HasSubclass())
            {
                subclass = DetermineSubclassFromTemperature(spectralClass, temperatureK);
            }
        }

        string stellarType = GetStellarType(stage);
        string luminosityClass = GetLuminosityClass(stage);
        string spectralString = BuildSpectralString(stage, spectralClass, subclass, temperatureK, luminosityClass);

        PhysicalProps physical = GeneratePhysicalProps(spec, physicalMassSolar, radiusSolar, spectralClass, stage, rng);
        StellarProps stellar = new(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            temperatureK,
            spectralString,
            stellarType,
            metallicity,
            ageYears);

        string bodyId = GenerateId(spec, rng);
        string bodyName = spec.NameHint;
        Provenance provenance = GeneratorUtils.CreateProvenance(spec.GenerationSeed, spec.ToDictionary());

        CelestialBody body = new(bodyId, bodyName, CelestialType.Type.Star, physical, provenance)
        {
            Stellar = stellar,
        };
        return body;
    }

    /// <summary>
    /// Picks spectral class from spec override or galaxy-conditioned weighted RNG.
    /// </summary>
    private static double ResolveInitialMass(StarSpec spec, SeededRng rng)
    {
        double overrideMass = spec.GetOverrideFloat("physical.mass_solar", -1.0);
        if (overrideMass > 0.0)
        {
            return overrideMass;
        }

        if (spec.HasSpectralClass())
        {
            return -1.0;
        }

        return StellarMassSampler.SampleMassSolar(spec.StellarProfile, spec.GalaxyContext, rng);
    }

    private static StarClass.SpectralClass DetermineSpectralClass(StarSpec spec, double initialMassSolar, double metallicity)
    {
        if (spec.HasSpectralClass())
        {
            return (StarClass.SpectralClass)spec.SpectralClass;
        }

        return DetermineSpectralClassFromMass(initialMassSolar, metallicity);
    }

    /// <summary>
    /// Picks subclass from spec override or random 0-9.
    /// </summary>
    private static int DetermineSubclass(StarSpec spec, StarClass.SpectralClass spectralClass, double initialMassSolar, SeededRng rng)
    {
        if (spec.HasSubclass())
        {
            return System.Math.Clamp(spec.Subclass, 0, 9);
        }

        if (!spec.HasSpectralClass())
        {
            return DetermineSubclassFromSampledMass(spectralClass, initialMassSolar);
        }

        return rng.RandiRange(0, 9);
    }

    /// <summary>
    /// Computes stellar mass in solar units from table and overrides.
    /// </summary>
    private static double CalculateMass(
        StarSpec spec,
        StarClass.SpectralClass spectralClass,
        int subclass,
        double metallicity,
        double initialMassSolar,
        SeededRng rng)
    {
        double overrideMass = spec.GetOverrideFloat("physical.mass_solar", -1.0);
        if (overrideMass > 0.0)
        {
            return overrideMass;
        }

        if (!spec.HasSpectralClass())
        {
            return System.Math.Clamp(initialMassSolar, 0.01, 150.0);
        }

        Dictionary massRange = StarTable.GetMassRange(spectralClass);
        double baseMass = StarTable.InterpolateBySubclass(spectralClass, subclass, massRange);
        double variation = rng.RandfRange(0.95f, 1.05f);
        if (spec.StellarProfile.ImfVariationMode == StellarImfVariationMode.MetallicityAgeModulated)
        {
            double metallicityBias = 1.0;
            if (metallicity < 0.7)
            {
                metallicityBias += 0.06;
            }
            else if (metallicity > 1.4)
            {
                metallicityBias -= 0.03;
            }

            variation *= metallicityBias;
        }
        return baseMass * variation;
    }

    private static StellarModelResult ResolveStellarModel(
        StarSpec spec,
        double massSolar,
        double ageYears,
        double metallicity,
        StellarEvolutionStage stage,
        double progenitorMassSolar,
        SeededRng rng)
    {
        StellarModelResult modelResult;
        if (stage == StellarEvolutionStage.BrownDwarf)
        {
            modelResult = ResolveBrownDwarfModel(spec.StellarProfile, massSolar, ageYears, metallicity);
        }
        else if (stage == StellarEvolutionStage.WhiteDwarf)
        {
            modelResult = ResolveWhiteDwarfModel(spec.StellarProfile, massSolar, ageYears, progenitorMassSolar);
        }
        else
        {
            modelResult = StellarIsochroneApproximator.Resolve(spec.StellarProfile, massSolar, ageYears, metallicity);
            modelResult = ApplyEvolutionStageAdjustments(modelResult, stage, massSolar, ageYears, metallicity);
        }

        double luminositySolar = modelResult.LuminositySolar;
        double radiusSolar = modelResult.RadiusSolar;
        double temperatureK = modelResult.TemperatureK;

        double overrideLuminosity = spec.GetOverrideFloat("stellar.luminosity_solar", -1.0);
        if (overrideLuminosity > 0.0)
        {
            luminositySolar = overrideLuminosity;
        }
        else
        {
            luminositySolar *= rng.RandfRange(0.94f, 1.06f);
        }

        double overrideRadius = spec.GetOverrideFloat("physical.radius_solar", -1.0);
        if (overrideRadius > 0.0)
        {
            radiusSolar = overrideRadius;
        }

        double overrideTemperature = spec.GetOverrideFloat("stellar.temperature_k", -1.0);
        if (overrideTemperature > 0.0)
        {
            temperatureK = overrideTemperature;
        }
        else
        {
            temperatureK *= rng.RandfRange(0.985f, 1.015f);
        }

        return new StellarModelResult(luminositySolar, radiusSolar, temperatureK, modelResult.AgeFraction, modelResult.LifetimeYears);
    }

    /// <summary>
    /// Picks age in years from spec or galaxy-conditioned randomization.
    /// </summary>
    private static double DetermineAge(
        StarSpec spec,
        double massSolar,
        StarClass.SpectralClass spectralClass,
        SeededRng rng)
    {
        if (spec.HasAge())
        {
            return spec.AgeYears;
        }

        const double MaxUniverseAgeYears = 13.5e9;
        double sampledLifetime = StellarIsochroneApproximator.EstimateMainSequenceLifetimeYears(massSolar);
        double maxAge = MaxUniverseAgeYears;
        if (massSolar < HydrogenBurningLimitSolar || spectralClass == StarClass.SpectralClass.L || spectralClass == StarClass.SpectralClass.T || spectralClass == StarClass.SpectralClass.Y)
        {
            maxAge = MaxUniverseAgeYears;
        }
        else if (massSolar > WhiteDwarfMaxProgenitorMassSolar)
        {
            maxAge = System.Math.Min(sampledLifetime * 0.92, MaxUniverseAgeYears);
        }
        else if (massSolar >= WhiteDwarfMinProgenitorMassSolar)
        {
            maxAge = System.Math.Min(sampledLifetime * 4.0, MaxUniverseAgeYears);
        }

        if (maxAge < 1.0e6)
        {
            maxAge = 1.0e6;
        }

        double contextAgeMeanYears = spec.GalaxyContext.AgeMeanGyr * 1.0e9;

        if (contextAgeMeanYears > 0.0)
        {
            double clampedMean = System.Math.Clamp(contextAgeMeanYears, 1.0e6, maxAge);
            double spreadYears = ResolveAgeSpreadYears(spec.GalaxyContext, clampedMean, maxAge);
            double sampledAge = clampedMean + (rng.Randfn(0.0f, 1.0f) * spreadYears);
            return System.Math.Clamp(sampledAge, 1.0e6, maxAge);
        }

        double minAge = 1.0e6;
        double raw = rng.Randf();
        double biased = System.Math.Pow(raw, 0.7);
        return minAge + ((maxAge - minAge) * biased);
    }

    /// <summary>
    /// Picks metallicity from spec or galaxy-conditioned log-normal randomization.
    /// </summary>
    private static double DetermineMetallicity(StarSpec spec, SeededRng rng)
    {
        if (spec.HasMetallicity())
        {
            return spec.Metallicity;
        }

        double baseline = spec.GalaxyContext.MetallicityPrior;
        if (baseline <= 0.0)
        {
            baseline = 1.0;
        }

        double logMetallicity = System.Math.Log(baseline) + rng.Randfn(0.0f, 0.12f);
        return System.Math.Clamp(System.Math.Exp(logMetallicity), 0.1, 3.0);
    }

    /// <summary>
    /// Builds physical properties from mass, radius, spectral class, and spec overrides.
    /// </summary>
    private static PhysicalProps GeneratePhysicalProps(
        StarSpec spec,
        double massSolar,
        double radiusSolar,
        StarClass.SpectralClass spectralClass,
        StellarEvolutionStage stage,
        SeededRng rng)
    {
        double massKg = spec.GetOverrideFloat("physical.mass_kg", massSolar * Units.SolarMassKg);
        double radiusM = spec.GetOverrideFloat("physical.radius_m", radiusSolar * Units.SolarRadiusMeters);

        (double rotMinDays, double rotMaxDays) = ResolveRotationRangeDays(spectralClass, stage);
        double rotationDays = rng.RandfRange((float)rotMinDays, (float)rotMaxDays);
        double rotationPeriodS = spec.GetOverrideFloat("physical.rotation_period_s", rotationDays * 24.0 * 3600.0);
        double axialTiltDeg = spec.GetOverrideFloat("physical.axial_tilt_deg", rng.RandfRange(0.0f, 30.0f));
        double oblateness = spec.GetOverrideFloat("physical.oblateness", rng.RandfRange(0.0f, 0.001f));
        double magneticMoment = spec.GetOverrideFloat("physical.magnetic_moment", rng.RandfRange(1.0e22f, 1.0e26f));
        double internalHeatWatts = spec.GetOverrideFloat("physical.internal_heat_watts", 0.0);

        return new PhysicalProps(
            massKg,
            radiusM,
            rotationPeriodS,
            axialTiltDeg,
            oblateness,
            magneticMoment,
            internalHeatWatts);
    }

    /// <summary>
    /// Produces a unique star id from spec override or RNG.
    /// </summary>
    private static string GenerateId(StarSpec spec, SeededRng rng)
    {
        Variant overrideId = spec.GetOverride("id", default);
        if (overrideId.VariantType == Variant.Type.String)
        {
            string id = (string)overrideId;
            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }
        }

        int randomPart = (int)(rng.Randi() % 1_000_000u);
        return GeneratorUtils.GenerateIdFromRandomPart("star", randomPart);
    }

    private static double ResolveAgeSpreadYears(GalaxyOriginContext galaxyContext, double meanYears, double maxAge)
    {
        double spreadYears = System.Math.Max(meanYears * 0.2, 0.15e9);
        if (galaxyContext.AgeCohort == GalaxyAgeCohort.Young)
        {
            spreadYears *= 0.7;
        }
        else if (galaxyContext.AgeCohort == GalaxyAgeCohort.Ancient)
        {
            spreadYears *= 0.5;
        }

        return System.Math.Min(spreadYears, maxAge * 0.35);
    }

    private static StarClass.SpectralClass DetermineSpectralClassFromMass(double massSolar, double metallicity)
    {
        if (massSolar < HydrogenBurningLimitSolar)
        {
            double substellarTemperature = ResolveBrownDwarfTemperatureK(massSolar, 1.0e9, 1.0);
            return StarTable.ClassFromTemperature(substellarTemperature);
        }

        double temperatureProxy = StarTable.TemperatureFromLuminosityRadius(
            StarTable.LuminosityFromMass(massSolar),
            StarTable.RadiusFromMass(massSolar));

        if (metallicity > 1.5)
        {
            temperatureProxy *= 0.985;
        }
        else if (metallicity < 0.6)
        {
            temperatureProxy *= 1.015;
        }

        return StarTable.ClassFromTemperature(temperatureProxy);
    }

    private static int DetermineSubclassFromSampledMass(StarClass.SpectralClass spectralClass, double sampledMass)
    {
        (double minMass, double maxMass) = StarTable.GetMassRangeTuple(spectralClass);
        if (maxMass <= minMass)
        {
            return 5;
        }

        double normalized = (maxMass - sampledMass) / (maxMass - minMass);
        double clamped = System.Math.Clamp(normalized, 0.0, 1.0);
        int subclass = (int)System.Math.Round(clamped * 9.0);
        return System.Math.Clamp(subclass, 0, 9);
    }

    private static int DetermineSubclassFromTemperature(StarClass.SpectralClass spectralClass, double temperatureK)
    {
        (double minTemperature, double maxTemperature) = StarTable.GetTemperatureRangeTuple(spectralClass);
        if (maxTemperature <= minTemperature)
        {
            return 5;
        }

        double normalized = (maxTemperature - temperatureK) / (maxTemperature - minTemperature);
        double clamped = System.Math.Clamp(normalized, 0.0, 1.0);
        int subclass = (int)System.Math.Round(clamped * 9.0);
        return System.Math.Clamp(subclass, 0, 9);
    }

    private static StellarEvolutionStage ResolveEvolutionStage(double massSolar, double ageYears, bool allowWhiteDwarf)
    {
        if (massSolar < HydrogenBurningLimitSolar)
        {
            return StellarEvolutionStage.BrownDwarf;
        }

        double lifetimeYears = StellarIsochroneApproximator.EstimateMainSequenceLifetimeYears(massSolar);
        double ageFraction = 0.0;
        if (lifetimeYears > 0.0)
        {
            ageFraction = ageYears / lifetimeYears;
        }

        if (allowWhiteDwarf
            && massSolar >= WhiteDwarfMinProgenitorMassSolar
            && massSolar <= WhiteDwarfMaxProgenitorMassSolar
            && ageFraction >= 1.25)
        {
            return StellarEvolutionStage.WhiteDwarf;
        }

        if (massSolar >= 8.0 && ageFraction >= 0.88)
        {
            return StellarEvolutionStage.Supergiant;
        }

        if (massSolar >= 0.85 && ageFraction >= 1.00)
        {
            return StellarEvolutionStage.Giant;
        }

        if (massSolar >= 0.85 && ageFraction >= 0.82)
        {
            return StellarEvolutionStage.Subgiant;
        }

        return StellarEvolutionStage.MainSequence;
    }

    private static StellarModelResult ApplyEvolutionStageAdjustments(
        StellarModelResult baseResult,
        StellarEvolutionStage stage,
        double massSolar,
        double ageYears,
        double metallicity)
    {
        if (stage == StellarEvolutionStage.MainSequence)
        {
            return baseResult;
        }

        double lifetimeYears = baseResult.LifetimeYears;
        double ageFraction = 0.0;
        if (lifetimeYears > 0.0)
        {
            ageFraction = ageYears / lifetimeYears;
        }

        double luminositySolar = baseResult.LuminositySolar;
        double radiusSolar = baseResult.RadiusSolar;
        double temperatureK = baseResult.TemperatureK;

        if (stage == StellarEvolutionStage.Subgiant)
        {
            double progress = System.Math.Clamp((ageFraction - 0.82) / 0.18, 0.0, 1.0);
            luminositySolar *= Lerp(1.6, 5.0, progress);
            radiusSolar *= Lerp(1.7, 3.8, progress);
            temperatureK *= Lerp(0.99, 0.92, progress);
        }
        else if (stage == StellarEvolutionStage.Giant)
        {
            double progress = System.Math.Clamp((ageFraction - 1.0) / 0.35, 0.0, 1.0);
            double maxLuminosity = 180.0;
            if (massSolar >= 2.5)
            {
                maxLuminosity = 1200.0;
            }

            luminositySolar *= Lerp(8.0, maxLuminosity, progress);
            radiusSolar *= Lerp(6.0, 55.0, progress);
            temperatureK *= Lerp(0.88, 0.62, progress);
        }
        else if (stage == StellarEvolutionStage.Supergiant)
        {
            double progress = System.Math.Clamp((ageFraction - 0.88) / 0.20, 0.0, 1.0);
            double temperatureFloor = 0.58;
            if (metallicity < 0.7)
            {
                temperatureFloor = 0.68;
            }

            luminositySolar *= Lerp(3000.0, 90000.0, progress);
            radiusSolar *= Lerp(30.0, 450.0, progress);
            temperatureK *= Lerp(1.02, temperatureFloor, progress);
        }

        double recomputedTemperature = StarTable.TemperatureFromLuminosityRadius(luminositySolar, radiusSolar);
        if (recomputedTemperature > 0.0)
        {
            temperatureK = recomputedTemperature;
        }

        return new StellarModelResult(luminositySolar, radiusSolar, temperatureK, ageFraction, lifetimeYears);
    }

    private static StellarModelResult ResolveBrownDwarfModel(
        StellarGenerationProfile profile,
        double massSolar,
        double ageYears,
        double metallicitySolar)
    {
        double radiusSolar = ResolveBrownDwarfRadiusSolar(massSolar);
        double temperatureK = ResolveBrownDwarfTemperatureK(massSolar, ageYears, metallicitySolar);

        if (profile.IsochroneModel == StellarIsochroneModel.Parsec)
        {
            temperatureK *= 0.97;
            radiusSolar *= 1.02;
        }
        else
        {
            temperatureK *= 1.01;
            radiusSolar *= 0.99;
        }

        double luminositySolar = System.Math.Pow(radiusSolar, 2.0) * System.Math.Pow(temperatureK / 5778.0, 4.0);
        return new StellarModelResult(luminositySolar, radiusSolar, temperatureK, 0.0, 1.0e12);
    }

    private static StellarModelResult ResolveWhiteDwarfModel(
        StellarGenerationProfile profile,
        double massSolar,
        double ageYears,
        double progenitorMassSolar)
    {
        double mainSequenceLifetime = StellarIsochroneApproximator.EstimateMainSequenceLifetimeYears(progenitorMassSolar);
        double coolingAgeYears = System.Math.Max(ageYears - mainSequenceLifetime, 1.0e7);
        double radiusSolar = ResolveWhiteDwarfRadiusSolar(massSolar);
        double temperatureK = ResolveWhiteDwarfTemperatureK(coolingAgeYears);

        if (profile.IsochroneModel == StellarIsochroneModel.Parsec)
        {
            temperatureK *= 0.98;
            radiusSolar *= 1.02;
        }
        else
        {
            temperatureK *= 1.02;
            radiusSolar *= 0.99;
        }

        double luminositySolar = System.Math.Pow(radiusSolar, 2.0) * System.Math.Pow(temperatureK / 5778.0, 4.0);
        double ageFraction = 1.0;
        if (mainSequenceLifetime > 0.0)
        {
            ageFraction = ageYears / mainSequenceLifetime;
        }

        return new StellarModelResult(luminositySolar, radiusSolar, temperatureK, ageFraction, mainSequenceLifetime);
    }

    private static double EstimateWhiteDwarfMassSolar(double progenitorMassSolar)
    {
        if (progenitorMassSolar <= 2.85)
        {
            return 0.53 + ((progenitorMassSolar - 0.85) * 0.08);
        }

        if (progenitorMassSolar <= 3.6)
        {
            return 0.69 + ((progenitorMassSolar - 2.85) * 0.19);
        }

        return 0.8325 + ((progenitorMassSolar - 3.6) * 0.11);
    }

    private static double ResolveBrownDwarfRadiusSolar(double massSolar)
    {
        double normalizedMass = System.Math.Clamp((massSolar - 0.01) / 0.065, 0.0, 1.0);
        double radiusSolar = 0.11 - (normalizedMass * 0.02);
        return System.Math.Clamp(radiusSolar, 0.08, 0.12);
    }

    private static double ResolveBrownDwarfTemperatureK(double massSolar, double ageYears, double metallicitySolar)
    {
        double normalizedMass = System.Math.Clamp((massSolar - 0.01) / 0.065, 0.0, 1.0);
        double ageGyr = System.Math.Max(ageYears / 1.0e9, 0.01);
        double baseTemperature = 350.0 + (normalizedMass * 2100.0);
        double coolingFactor = 1.0 / System.Math.Pow(1.0 + (ageGyr * 0.6), 0.35);
        double metallicityFactor = 1.0;
        if (metallicitySolar < 0.7)
        {
            metallicityFactor = 1.04;
        }
        else if (metallicitySolar > 1.4)
        {
            metallicityFactor = 0.97;
        }

        double temperatureK = baseTemperature * coolingFactor * metallicityFactor;
        return System.Math.Clamp(temperatureK, 250.0, 2400.0);
    }

    private static double ResolveWhiteDwarfRadiusSolar(double massSolar)
    {
        double normalizedMass = System.Math.Max(massSolar / 0.6, 0.2);
        double radiusSolar = 0.0125 / System.Math.Pow(normalizedMass, 0.35);
        return System.Math.Clamp(radiusSolar, 0.008, 0.02);
    }

    private static double ResolveWhiteDwarfTemperatureK(double coolingAgeYears)
    {
        double coolingGyr = System.Math.Max(coolingAgeYears / 1.0e9, 0.01);
        double temperatureK = 30000.0 / System.Math.Pow(1.0 + (coolingGyr * 3.0), 0.55);
        return System.Math.Clamp(temperatureK, 4000.0, 30000.0);
    }

    private static string GetStellarType(StellarEvolutionStage stage)
    {
        if (stage == StellarEvolutionStage.BrownDwarf)
        {
            return "brown_dwarf";
        }

        if (stage == StellarEvolutionStage.Subgiant)
        {
            return "subgiant";
        }

        if (stage == StellarEvolutionStage.Giant)
        {
            return "giant";
        }

        if (stage == StellarEvolutionStage.Supergiant)
        {
            return "supergiant";
        }

        if (stage == StellarEvolutionStage.WhiteDwarf)
        {
            return "white_dwarf";
        }

        return "main_sequence";
    }

    private static string GetLuminosityClass(StellarEvolutionStage stage)
    {
        if (stage == StellarEvolutionStage.Subgiant)
        {
            return "IV";
        }

        if (stage == StellarEvolutionStage.Giant)
        {
            return "III";
        }

        if (stage == StellarEvolutionStage.Supergiant)
        {
            return "I";
        }

        if (stage == StellarEvolutionStage.BrownDwarf || stage == StellarEvolutionStage.WhiteDwarf)
        {
            return string.Empty;
        }

        return "V";
    }

    private static string BuildSpectralString(
        StellarEvolutionStage stage,
        StarClass.SpectralClass spectralClass,
        int subclass,
        double temperatureK,
        string luminosityClass)
    {
        if (stage == StellarEvolutionStage.WhiteDwarf)
        {
            int coolingIndex = ResolveWhiteDwarfIndex(temperatureK);
            return $"DA{coolingIndex}";
        }

        return StarClass.BuildSpectralString(spectralClass, subclass, luminosityClass);
    }

    private static int ResolveWhiteDwarfIndex(double temperatureK)
    {
        double normalized = (30000.0 - temperatureK) / 26000.0;
        double clamped = System.Math.Clamp(normalized, 0.0, 1.0);
        int index = (int)System.Math.Round(clamped * 9.0);
        return System.Math.Clamp(index, 0, 9);
    }

    private static (double Min, double Max) ResolveRotationRangeDays(StarClass.SpectralClass spectralClass, StellarEvolutionStage stage)
    {
        if (stage == StellarEvolutionStage.WhiteDwarf)
        {
            return (0.04, 4.0);
        }

        if (stage == StellarEvolutionStage.BrownDwarf)
        {
            return (0.08, 0.60);
        }

        if (stage == StellarEvolutionStage.Subgiant)
        {
            return (10.0, 70.0);
        }

        if (stage == StellarEvolutionStage.Giant)
        {
            return (20.0, 250.0);
        }

        if (stage == StellarEvolutionStage.Supergiant)
        {
            return (30.0, 400.0);
        }

        return StarTable.GetRotationPeriodRangeDays(spectralClass);
    }

    private static double Lerp(double start, double end, double amount)
    {
        return start + ((end - start) * amount);
    }
}
