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
    /// <summary>
    /// Generates a star body from a specification and RNG.
    /// </summary>
    public static CelestialBody Generate(StarSpec spec, SeededRng rng)
    {
        double metallicity = DetermineMetallicity(spec, rng);
        double initialMassSolar = ResolveInitialMass(spec, rng);
        StarClass.SpectralClass spectralClass = DetermineSpectralClass(spec, initialMassSolar, metallicity);
        int subclass = DetermineSubclass(spec, spectralClass, initialMassSolar, rng);
        double massSolar = CalculateMass(spec, spectralClass, subclass, metallicity, initialMassSolar, rng);
        double ageYears = DetermineAge(spec, massSolar, spectralClass, rng);
        StellarModelResult modelResult = ResolveStellarModel(spec, massSolar, ageYears, metallicity, rng);
        double luminositySolar = modelResult.LuminositySolar;
        double radiusSolar = modelResult.RadiusSolar;
        double temperatureK = modelResult.TemperatureK;
        string spectralString = StarClass.BuildSpectralString(spectralClass, subclass, "V");

        PhysicalProps physical = GeneratePhysicalProps(spec, massSolar, radiusSolar, spectralClass, rng);
        StellarProps stellar = new(
            luminositySolar * StellarProps.SolarLuminosityWatts,
            temperatureK,
            spectralString,
            "main_sequence",
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
            return System.Math.Clamp(initialMassSolar, 0.08, 150.0);
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
        SeededRng rng)
    {
        StellarModelResult modelResult = StellarIsochroneApproximator.Resolve(spec.StellarProfile, massSolar, ageYears, metallicity);

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
        (double minLifetime, double maxLifetime) = StarTable.GetLifetimeRangeTuple(spectralClass);
        double maxAge = System.Math.Min(System.Math.Min(sampledLifetime * 0.92, maxLifetime * 0.92), MaxUniverseAgeYears);
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

        double minAge = System.Math.Max(1.0e6, minLifetime * 0.1);
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
        SeededRng rng)
    {
        double massKg = spec.GetOverrideFloat("physical.mass_kg", massSolar * Units.SolarMassKg);
        double radiusM = spec.GetOverrideFloat("physical.radius_m", radiusSolar * Units.SolarRadiusMeters);

        (double rotMinDays, double rotMaxDays) = StarTable.GetRotationPeriodRangeDays(spectralClass);
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
}
