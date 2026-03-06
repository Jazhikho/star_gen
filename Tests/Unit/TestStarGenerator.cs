#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Constants;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for StarGenerator.
/// </summary>
public static class TestStarGenerator
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests that generation is deterministic.
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
            throw new InvalidOperationException($"Expected same id, got '{star1.Id}' vs '{star2.Id}'");
        }
        if (star1.Name != star2.Name)
        {
            throw new InvalidOperationException($"Expected same name, got '{star1.Name}' vs '{star2.Name}'");
        }
        if (System.Math.Abs(star1.Physical.MassKg - star2.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected same mass, got {star1.Physical.MassKg} vs {star2.Physical.MassKg}");
        }
        if (System.Math.Abs(star1.Physical.RadiusM - star2.Physical.RadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected same radius, got {star1.Physical.RadiusM} vs {star2.Physical.RadiusM}");
        }
        if (System.Math.Abs(star1.Stellar.LuminosityWatts - star2.Stellar.LuminosityWatts) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected same luminosity, got {star1.Stellar.LuminosityWatts} vs {star2.Stellar.LuminosityWatts}");
        }
        if (System.Math.Abs(star1.Stellar.EffectiveTemperatureK - star2.Stellar.EffectiveTemperatureK) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected same temperature, got {star1.Stellar.EffectiveTemperatureK} vs {star2.Stellar.EffectiveTemperatureK}");
        }
        if (star1.Stellar.SpectralClass != star2.Stellar.SpectralClass)
        {
            throw new InvalidOperationException($"Expected same spectral_class, got '{star1.Stellar.SpectralClass}' vs '{star2.Stellar.SpectralClass}'");
        }
    }

    /// <summary>
    /// Tests that different seeds produce different stars.
    /// </summary>
    public static void TestDifferentSeeds()
    {
        StarSpec spec1 = StarSpec.Random(11111);
        StarSpec spec2 = StarSpec.Random(22222);

        SeededRng rng1 = new SeededRng(spec1.GenerationSeed);
        SeededRng rng2 = new SeededRng(spec2.GenerationSeed);

        CelestialBody star1 = StarGenerator.Generate(spec1, rng1);
        CelestialBody star2 = StarGenerator.Generate(spec2, rng2);

        if (star1.Id == star2.Id)
        {
            throw new InvalidOperationException("IDs should differ");
        }
    }

    /// <summary>
    /// Tests that generated star passes validation.
    /// </summary>
    public static void TestValidationPasses()
    {
        StarSpec spec = StarSpec.Random(42);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);
        ValidationResult result = CelestialValidator.Validate(star);

        if (!result.IsValid())
        {
            throw new InvalidOperationException("Generated star should pass validation");
        }
    }

    /// <summary>
    /// Tests sun-like preset produces G-class star.
    /// </summary>
    public static void TestSunLikePreset()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (!star.Stellar.SpectralClass.StartsWith("G"))
        {
            throw new InvalidOperationException($"Expected G-class, got '{star.Stellar.SpectralClass}'");
        }
        if (star.Stellar.SpectralClass.Length < 2 || star.Stellar.SpectralClass.Substring(1, 1) != "2")
        {
            throw new InvalidOperationException($"Expected G2, got '{star.Stellar.SpectralClass}'");
        }
        if (star.Stellar.StellarType != "main_sequence")
        {
            throw new InvalidOperationException($"Expected main_sequence, got '{star.Stellar.StellarType}'");
        }
    }

    /// <summary>
    /// Tests red dwarf preset produces M-class star.
    /// </summary>
    public static void TestRedDwarfPreset()
    {
        StarSpec spec = StarSpec.RedDwarf(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (!star.Stellar.SpectralClass.StartsWith("M"))
        {
            throw new InvalidOperationException($"Expected M-class, got '{star.Stellar.SpectralClass}'");
        }
    }

    /// <summary>
    /// Tests hot blue preset produces B-class star.
    /// </summary>
    public static void TestHotBluePreset()
    {
        StarSpec spec = StarSpec.HotBlue(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (!star.Stellar.SpectralClass.StartsWith("B"))
        {
            throw new InvalidOperationException($"Expected B-class, got '{star.Stellar.SpectralClass}'");
        }
    }

    /// <summary>
    /// Tests temperature matches spectral class.
    /// </summary>
    public static void TestTemperatureMatchesClass()
    {
        int[] seedVals = new int[] { 100, 200, 300, 400, 500 };
        foreach (int seedVal in seedVals)
        {
            StarSpec spec = StarSpec.Random(seedVal);
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody star = StarGenerator.Generate(spec, rng);
            string spectralLetter = star.Stellar.SpectralClass.Substring(0, 1);
            if (!StarClass.TryParseLetter(spectralLetter, out StarClass.SpectralClass expectedClass))
            {
                throw new InvalidOperationException($"Could not parse spectral letter '{spectralLetter}'");
            }

            (double Min, double Max) tempRange = StarTable.GetTemperatureRange(expectedClass);

            double margin = (tempRange.Max - tempRange.Min) * 0.2;
            double tempMin = tempRange.Min - margin;
            double tempMax = tempRange.Max + margin;
            if (star.Stellar.EffectiveTemperatureK < tempMin || star.Stellar.EffectiveTemperatureK > tempMax)
            {
                throw new InvalidOperationException($"Temperature {star.Stellar.EffectiveTemperatureK} out of range [{tempMin}, {tempMax}]");
            }
        }
    }

    /// <summary>
    /// Tests mass-luminosity relationship is reasonable.
    /// </summary>
    public static void TestMassLuminosityRelationship()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double lumSolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;

        if (massSolar < 0.8 || massSolar > 1.2)
        {
            throw new InvalidOperationException($"Expected mass in range [0.8, 1.2], got {massSolar}");
        }
        if (lumSolar < 0.5 || lumSolar > 2.0)
        {
            throw new InvalidOperationException($"Expected luminosity in range [0.5, 2.0], got {lumSolar}");
        }
    }

    /// <summary>
    /// Tests provenance is stored correctly.
    /// </summary>
    public static void TestProvenance()
    {
        StarSpec spec = new StarSpec(99999, (int)StarClass.SpectralClass.K, 5, 1.2, 5.0e9, "Test Star");
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (star.Provenance == null)
        {
            throw new InvalidOperationException("Expected non-null provenance");
        }
        if (star.Provenance.GenerationSeed != 99999)
        {
            throw new InvalidOperationException($"Expected generation_seed 99999, got {star.Provenance.GenerationSeed}");
        }
        if (star.Provenance.GeneratorVersion != Versions.GeneratorVersion)
        {
            throw new InvalidOperationException($"Expected generator_version '{Versions.GeneratorVersion}', got '{star.Provenance.GeneratorVersion}'");
        }
        if (star.Provenance.SchemaVersion != Versions.SchemaVersion)
        {
            throw new InvalidOperationException($"Expected schema_version {Versions.SchemaVersion}, got {star.Provenance.SchemaVersion}");
        }

        Godot.Collections.Dictionary snapshot = star.Provenance.SpecSnapshot;
        if (snapshot["spectral_class"].AsInt32() != (int)StarClass.SpectralClass.K)
        {
            throw new InvalidOperationException($"Expected spectral_class K, got {snapshot["spectral_class"]}");
        }
        if (snapshot["subclass"].AsInt32() != 5)
        {
            throw new InvalidOperationException($"Expected subclass 5, got {snapshot["subclass"]}");
        }
    }

    /// <summary>
    /// Tests name hint is used when provided.
    /// </summary>
    public static void TestNameHint()
    {
        StarSpec spec = new StarSpec(12345, -1, -1, -1.0, -1.0, "Sol");
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (star.Name != "Sol")
        {
            throw new InvalidOperationException($"Expected name 'Sol', got '{star.Name}'");
        }
    }

    /// <summary>
    /// Tests overrides are respected.
    /// </summary>
    public static void TestOverrides()
    {
        StarSpec spec = StarSpec.Random(12345);
        spec.SetOverride("physical.mass_solar", 2.0);
        spec.SetOverride("stellar.luminosity_solar", 10.0);

        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody star = StarGenerator.Generate(spec, rng);

        double massSolar = star.Physical.MassKg / Units.SolarMassKg;
        double lumSolar = star.Stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts;

        if (System.Math.Abs(massSolar - 2.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected mass 2.0 solar, got {massSolar}");
        }
        if (System.Math.Abs(lumSolar - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity 10.0 solar, got {lumSolar}");
        }
    }

    /// <summary>
    /// Tests generated star has correct type.
    /// </summary>
    public static void TestStarType()
    {
        StarSpec spec = StarSpec.Random(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        if (star.Type != CelestialType.Type.Star)
        {
            throw new InvalidOperationException($"Expected type Star, got {star.Type}");
        }
        if (!star.HasStellar())
        {
            throw new InvalidOperationException("Expected stellar properties");
        }
        if (star.HasSurface())
        {
            throw new InvalidOperationException("Star should not have surface");
        }
    }

    /// <summary>
    /// Tests physical properties are positive.
    /// </summary>
    public static void TestPhysicalPropertiesPositive()
    {
        int[] seedVals = new int[] { 1, 2, 3, 4, 5 };
        foreach (int seedVal in seedVals)
        {
            StarSpec spec = StarSpec.Random(seedVal);
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody star = StarGenerator.Generate(spec, rng);

            if (star.Physical.MassKg <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive mass, got {star.Physical.MassKg}");
            }
            if (star.Physical.RadiusM <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive radius, got {star.Physical.RadiusM}");
            }
            if (star.Physical.RotationPeriodS <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive rotation_period, got {star.Physical.RotationPeriodS}");
            }
        }
    }

    /// <summary>
    /// Tests stellar properties are positive.
    /// </summary>
    public static void TestStellarPropertiesPositive()
    {
        int[] seedVals = new int[] { 10, 20, 30, 40, 50 };
        foreach (int seedVal in seedVals)
        {
            StarSpec spec = StarSpec.Random(seedVal);
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody star = StarGenerator.Generate(spec, rng);

            if (star.Stellar.LuminosityWatts <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive luminosity, got {star.Stellar.LuminosityWatts}");
            }
            if (star.Stellar.EffectiveTemperatureK <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive temperature, got {star.Stellar.EffectiveTemperatureK}");
            }
            if (star.Stellar.Metallicity <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive metallicity, got {star.Stellar.Metallicity}");
            }
            if (star.Stellar.AgeYears <= 0.0)
            {
                throw new InvalidOperationException($"Expected positive age, got {star.Stellar.AgeYears}");
            }
        }
    }

    /// <summary>
    /// Tests age is within main sequence lifetime.
    /// </summary>
    public static void TestAgeWithinLifetime()
    {
        int[] seedVals = new int[] { 111, 222, 333, 444, 555 };
        foreach (int seedVal in seedVals)
        {
            StarSpec spec = StarSpec.Random(seedVal);
            SeededRng rng = new SeededRng(spec.GenerationSeed);

            CelestialBody star = StarGenerator.Generate(spec, rng);

            string spectralLetter = star.Stellar.SpectralClass.Substring(0, 1);
            if (!StarClass.TryParseLetter(spectralLetter, out StarClass.SpectralClass spectralClass))
            {
                throw new InvalidOperationException($"Could not parse spectral letter '{spectralLetter}'");
            }
            (double Min, double Max) lifetimeRange = StarTable.GetLifetimeRange(spectralClass);

            if (star.Stellar.AgeYears >= lifetimeRange.Max)
            {
                throw new InvalidOperationException($"Age {star.Stellar.AgeYears} should be less than lifetime max {lifetimeRange.Max}");
            }
        }
    }

    /// <summary>
    /// Tests habitable zone calculation.
    /// </summary>
    public static void TestHabitableZone()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(spec.GenerationSeed);

        CelestialBody star = StarGenerator.Generate(spec, rng);

        double hzInner = star.Stellar.GetHabitableZoneInnerM();
        double hzOuter = star.Stellar.GetHabitableZoneOuterM();

        if (hzInner <= 0.8 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected HZ inner > 0.8 AU, got {hzInner / Units.AuMeters} AU");
        }
        if (hzInner >= 1.2 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected HZ inner < 1.2 AU, got {hzInner / Units.AuMeters} AU");
        }
        if (hzOuter <= 1.2 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected HZ outer > 1.2 AU, got {hzOuter / Units.AuMeters} AU");
        }
        if (hzOuter >= 1.6 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected HZ outer < 1.6 AU, got {hzOuter / Units.AuMeters} AU");
        }
    }
}
