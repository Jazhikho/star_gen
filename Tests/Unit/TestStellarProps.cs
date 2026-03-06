#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for StellarProps component.
/// </summary>
public static class TestStellarProps
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        StellarProps props = new StellarProps();
        if (props.LuminosityWatts != 0.0)
        {
            throw new InvalidOperationException($"Expected luminosity_watts 0.0, got {props.LuminosityWatts}");
        }
        if (props.EffectiveTemperatureK != 0.0)
        {
            throw new InvalidOperationException($"Expected effective_temperature_k 0.0, got {props.EffectiveTemperatureK}");
        }
        if (props.SpectralClass != "")
        {
            throw new InvalidOperationException($"Expected empty spectral_class, got '{props.SpectralClass}'");
        }
        if (props.StellarType != "main_sequence")
        {
            throw new InvalidOperationException($"Expected stellar_type 'main_sequence', got '{props.StellarType}'");
        }
        if (props.Metallicity != 1.0)
        {
            throw new InvalidOperationException($"Expected metallicity 1.0, got {props.Metallicity}");
        }
        if (props.AgeYears != 0.0)
        {
            throw new InvalidOperationException($"Expected age_years 0.0, got {props.AgeYears}");
        }
    }

    /// <summary>
    /// Tests creation with Sun-like values.
    /// </summary>
    public static void TestSunLikeInitialization()
    {
        StellarProps props = new StellarProps(
            3.828e26,
            5778.0,
            "G2V",
            "main_sequence",
            1.0,
            4.6e9
        );
        if (props.LuminosityWatts != 3.828e26)
        {
            throw new InvalidOperationException($"Expected luminosity_watts 3.828e26, got {props.LuminosityWatts}");
        }
        if (props.EffectiveTemperatureK != 5778.0)
        {
            throw new InvalidOperationException($"Expected effective_temperature_k 5778.0, got {props.EffectiveTemperatureK}");
        }
        if (props.SpectralClass != "G2V")
        {
            throw new InvalidOperationException($"Expected spectral_class 'G2V', got '{props.SpectralClass}'");
        }
    }

    /// <summary>
    /// Tests luminosity solar conversion.
    /// </summary>
    public static void TestLuminositySolar()
    {
        StellarProps props = new StellarProps(StellarProps.SolarLuminosityWatts);
        if (System.Math.Abs(props.GetLuminositySolar() - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity_solar 1.0, got {props.GetLuminositySolar()}");
        }

        props.LuminosityWatts = StellarProps.SolarLuminosityWatts * 2.0;
        if (System.Math.Abs(props.GetLuminositySolar() - 2.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity_solar 2.0, got {props.GetLuminositySolar()}");
        }
    }

    /// <summary>
    /// Tests habitable zone calculation for Sun-like star.
    /// </summary>
    public static void TestHabitableZoneSunLike()
    {
        StellarProps props = new StellarProps(StellarProps.SolarLuminosityWatts);

        double inner = props.GetHabitableZoneInnerM();
        double outer = props.GetHabitableZoneOuterM();

        if (inner < 0.9 * Units.AuMeters || inner > 1.0 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected inner HZ in range [0.9 AU, 1.0 AU], got {inner / Units.AuMeters} AU");
        }
        if (outer < 1.3 * Units.AuMeters || outer > 1.45 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected outer HZ in range [1.3 AU, 1.45 AU], got {outer / Units.AuMeters} AU");
        }
    }

    /// <summary>
    /// Tests habitable zone scales with luminosity.
    /// </summary>
    public static void TestHabitableZoneBrighterStar()
    {
        StellarProps sunLike = new StellarProps(StellarProps.SolarLuminosityWatts);
        StellarProps brighter = new StellarProps(StellarProps.SolarLuminosityWatts * 4.0);

        double sunInner = sunLike.GetHabitableZoneInnerM();
        double brightInner = brighter.GetHabitableZoneInnerM();

        if (System.Math.Abs(brightInner / sunInner - 2.0) > 0.01)
        {
            throw new InvalidOperationException($"Expected bright/sun ratio 2.0, got {brightInner / sunInner}");
        }
    }

    /// <summary>
    /// Tests frost line calculation.
    /// </summary>
    public static void TestFrostLine()
    {
        StellarProps props = new StellarProps(StellarProps.SolarLuminosityWatts);
        double frostLine = props.GetFrostLineM();

        if (frostLine < 2.5 * Units.AuMeters || frostLine > 2.9 * Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected frost line in range [2.5 AU, 2.9 AU], got {frostLine / Units.AuMeters} AU");
        }
    }

    /// <summary>
    /// Tests non-positive luminosity collapses derived boundaries to zero.
    /// </summary>
    public static void TestNonPositiveLuminosityReturnsZeroBoundaries()
    {
        StellarProps zeroLuminosity = new StellarProps(0.0);
        if (zeroLuminosity.GetHabitableZoneInnerM() != 0.0)
        {
            throw new InvalidOperationException("Zero luminosity should return a zero habitable-zone inner edge");
        }
        if (zeroLuminosity.GetHabitableZoneOuterM() != 0.0)
        {
            throw new InvalidOperationException("Zero luminosity should return a zero habitable-zone outer edge");
        }
        if (zeroLuminosity.GetFrostLineM() != 0.0)
        {
            throw new InvalidOperationException("Zero luminosity should return a zero frost line");
        }

        StellarProps negativeLuminosity = new StellarProps(-1.0);
        if (negativeLuminosity.GetHabitableZoneInnerM() != 0.0)
        {
            throw new InvalidOperationException("Negative luminosity should return a zero habitable-zone inner edge");
        }
        if (negativeLuminosity.GetHabitableZoneOuterM() != 0.0)
        {
            throw new InvalidOperationException("Negative luminosity should return a zero habitable-zone outer edge");
        }
        if (negativeLuminosity.GetFrostLineM() != 0.0)
        {
            throw new InvalidOperationException("Negative luminosity should return a zero frost line");
        }
    }

    /// <summary>
    /// Tests spectral letter extraction.
    /// </summary>
    public static void TestSpectralLetter()
    {
        StellarProps props = new StellarProps();

        props.SpectralClass = "G2V";
        if (props.GetSpectralLetter() != "G")
        {
            throw new InvalidOperationException($"Expected spectral letter 'G', got '{props.GetSpectralLetter()}'");
        }

        props.SpectralClass = "M5V";
        if (props.GetSpectralLetter() != "M")
        {
            throw new InvalidOperationException($"Expected spectral letter 'M', got '{props.GetSpectralLetter()}'");
        }

        props.SpectralClass = "K0III";
        if (props.GetSpectralLetter() != "K")
        {
            throw new InvalidOperationException($"Expected spectral letter 'K', got '{props.GetSpectralLetter()}'");
        }

        props.SpectralClass = "";
        if (props.GetSpectralLetter() != "")
        {
            throw new InvalidOperationException($"Expected empty spectral letter, got '{props.GetSpectralLetter()}'");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        StellarProps original = new StellarProps(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9);
        Godot.Collections.Dictionary data = original.ToDictionary();
        StellarProps restored = StellarProps.FromDictionary(data);

        if (System.Math.Abs(restored.LuminosityWatts - original.LuminosityWatts) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity_watts {original.LuminosityWatts}, got {restored.LuminosityWatts}");
        }
        if (System.Math.Abs(restored.EffectiveTemperatureK - original.EffectiveTemperatureK) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected effective_temperature_k {original.EffectiveTemperatureK}, got {restored.EffectiveTemperatureK}");
        }
        if (restored.SpectralClass != original.SpectralClass)
        {
            throw new InvalidOperationException($"Expected spectral_class '{original.SpectralClass}', got '{restored.SpectralClass}'");
        }
        if (restored.StellarType != original.StellarType)
        {
            throw new InvalidOperationException($"Expected stellar_type '{original.StellarType}', got '{restored.StellarType}'");
        }
        if (System.Math.Abs(restored.Metallicity - original.Metallicity) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected metallicity {original.Metallicity}, got {restored.Metallicity}");
        }
        if (System.Math.Abs(restored.AgeYears - original.AgeYears) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected age_years {original.AgeYears}, got {restored.AgeYears}");
        }
    }
}
