using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
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
    private static readonly float[] SpectralWeights =
    {
        0.00003f,
        0.13f,
        0.6f,
        3.0f,
        7.6f,
        12.1f,
        76.45f,
    };

    private static readonly StarClass.SpectralClass[] SpectralClasses =
    {
        StarClass.SpectralClass.O,
        StarClass.SpectralClass.B,
        StarClass.SpectralClass.A,
        StarClass.SpectralClass.F,
        StarClass.SpectralClass.G,
        StarClass.SpectralClass.K,
        StarClass.SpectralClass.M,
    };

    /// <summary>
    /// Generates a star body from a specification and RNG.
    /// </summary>
    public static CelestialBody Generate(StarSpec spec, SeededRng rng)
    {
        StarClass.SpectralClass spectralClass = DetermineSpectralClass(spec, rng);
        int subclass = DetermineSubclass(spec, rng);
        double massSolar = CalculateMass(spec, spectralClass, subclass, rng);
        double luminositySolar = CalculateLuminosity(spec, massSolar, rng);
        double temperatureK = CalculateTemperature(spec, spectralClass, subclass, rng);
        double radiusSolar = CalculateRadiusFromLuminosityTemperature(spec, luminositySolar, temperatureK);
        string spectralString = StarClass.BuildSpectralString(spectralClass, subclass, "V");
        double ageYears = DetermineAge(spec, spectralClass, rng);
        double metallicity = DetermineMetallicity(spec, rng);

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

    /// <summary>Picks spectral class from spec override or weighted RNG.</summary>
    private static StarClass.SpectralClass DetermineSpectralClass(StarSpec spec, SeededRng rng)
    {
        if (spec.HasSpectralClass())
        {
            return (StarClass.SpectralClass)spec.SpectralClass;
        }

        StarClass.SpectralClass? selected = rng.WeightedChoice(SpectralClasses, SpectralWeights);
        if (selected == null)
        {
            GD.PushError("StarGenerator.DetermineSpectralClass: WeightedChoice returned null — spectral weight table may be empty or invalid.");
            throw new InvalidOperationException("WeightedChoice returned null for SpectralClass.");
        }

        return selected.Value;
    }

    /// <summary>Picks subclass from spec override or random 0–9.</summary>
    private static int DetermineSubclass(StarSpec spec, SeededRng rng)
    {
        if (spec.HasSubclass())
        {
            return System.Math.Clamp(spec.Subclass, 0, 9);
        }

        return rng.RandiRange(0, 9);
    }

    /// <summary>Computes stellar mass in solar units from table and overrides.</summary>
    private static double CalculateMass(
        StarSpec spec,
        StarClass.SpectralClass spectralClass,
        int subclass,
        SeededRng rng)
    {
        double overrideMass = spec.GetOverrideFloat("physical.mass_solar", -1.0);
        if (overrideMass > 0.0)
        {
            return overrideMass;
        }

        Dictionary massRange = StarTable.GetMassRange(spectralClass);
        double baseMass = StarTable.InterpolateBySubclass(spectralClass, subclass, massRange);
        double variation = rng.RandfRange(0.95f, 1.05f);
        return baseMass * variation;
    }

    private static double CalculateLuminosity(StarSpec spec, double massSolar, SeededRng rng)
    {
        double overrideLuminosity = spec.GetOverrideFloat("stellar.luminosity_solar", -1.0);
        if (overrideLuminosity > 0.0)
        {
            return overrideLuminosity;
        }

        double baseLuminosity = StarTable.LuminosityFromMass(massSolar);
        double variation = rng.RandfRange(0.90f, 1.10f);
        return baseLuminosity * variation;
    }

    /// <summary>Derives radius from luminosity and temperature (or override).</summary>
    private static double CalculateRadiusFromLuminosityTemperature(
        StarSpec spec,
        double luminositySolar,
        double temperatureK)
    {
        double overrideRadius = spec.GetOverrideFloat("physical.radius_solar", -1.0);
        if (overrideRadius > 0.0)
        {
            return overrideRadius;
        }

        const double solarTemperature = 5778.0;
        return System.Math.Sqrt(luminositySolar) * System.Math.Pow(solarTemperature / temperatureK, 2.0);
    }

    /// <summary>Computes effective temperature from table and overrides.</summary>
    private static double CalculateTemperature(
        StarSpec spec,
        StarClass.SpectralClass spectralClass,
        int subclass,
        SeededRng rng)
    {
        double overrideTemperature = spec.GetOverrideFloat("stellar.temperature_k", -1.0);
        if (overrideTemperature > 0.0)
        {
            return overrideTemperature;
        }

        Dictionary temperatureRange = StarTable.GetTemperatureRange(spectralClass);
        double baseTemperature = StarTable.InterpolateBySubclass(spectralClass, subclass, temperatureRange);
        double variation = rng.RandfRange(0.97f, 1.03f);
        return baseTemperature * variation;
    }

    /// <summary>
    /// Picks age in years from spec or biased random within stellar lifetime.
    /// Capped at 13.5 Gyr (current age of the universe, Planck 2018) to prevent
    /// physically impossible ages for long-lived M-stars whose lifetimes far exceed
    /// the universe's current age.
    /// </summary>
    private static double DetermineAge(
        StarSpec spec,
        StarClass.SpectralClass spectralClass,
        SeededRng rng)
    {
        if (spec.HasAge())
        {
            return spec.AgeYears;
        }

        const double MaxUniverseAgeYears = 13.5e9;

        (double minLifetime, double maxLifetime) = StarTable.GetLifetimeRangeTuple(spectralClass);
        double maxAge = System.Math.Min(maxLifetime * 0.9, MaxUniverseAgeYears);
        double minAge = minLifetime * 0.1;
        double raw = rng.Randf();
        double biased = System.Math.Pow(raw, 0.7);
        return minAge + (maxAge - minAge) * biased;
    }

    /// <summary>Picks metallicity from spec or log-normal random.</summary>
    private static double DetermineMetallicity(StarSpec spec, SeededRng rng)
    {
        if (spec.HasMetallicity())
        {
            return spec.Metallicity;
        }

        double logMetallicity = rng.Randfn(0.0f, 0.2f);
        return System.Math.Clamp(System.Math.Exp(logMetallicity), 0.1, 3.0);
    }

    /// <summary>
    /// Builds physical properties from mass, radius, spectral class, and spec overrides.
    /// Rotation period is sampled from spectral-class-dependent ranges based on
    /// observational surveys (McQuillan et al. 2014; Reinhold &amp; Gizon 2015).
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

    /// <summary>Produces a unique star id from spec override or RNG.</summary>
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
}
