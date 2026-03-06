using Godot;
using StarGen.Domain.Generation.Archetypes;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Lookup table for stellar properties by spectral class.
/// </summary>
public static class StarTable
{
    /// <summary>
    /// Returns the mass range for a spectral class in solar masses.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) mass range in solar masses.</returns>
    public static NumericRange GetMassRange(StarClass.SpectralClass spectralClass)
    {
        (double min, double max) = GetMassRangeTuple(spectralClass);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetMassRangeTuple(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (16.0, 150.0),
            StarClass.SpectralClass.B => (2.1, 16.0),
            StarClass.SpectralClass.A => (1.4, 2.1),
            StarClass.SpectralClass.F => (1.04, 1.4),
            StarClass.SpectralClass.G => (0.8, 1.04),
            StarClass.SpectralClass.K => (0.45, 0.8),
            StarClass.SpectralClass.M => (0.08, 0.45),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the temperature range for a spectral class in Kelvin.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) temperature range in Kelvin.</returns>
    public static NumericRange GetTemperatureRange(StarClass.SpectralClass spectralClass)
    {
        (double min, double max) = GetTemperatureRangeTuple(spectralClass);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetTemperatureRangeTuple(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (30000.0, 52000.0),
            StarClass.SpectralClass.B => (10000.0, 30000.0),
            StarClass.SpectralClass.A => (7500.0, 10000.0),
            StarClass.SpectralClass.F => (6000.0, 7500.0),
            StarClass.SpectralClass.G => (5200.0, 6000.0),
            StarClass.SpectralClass.K => (3700.0, 5200.0),
            StarClass.SpectralClass.M => (2400.0, 3700.0),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the luminosity range for a spectral class in solar luminosities.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) luminosity range in solar luminosities.</returns>
    public static NumericRange GetLuminosityRange(StarClass.SpectralClass spectralClass)
    {
        (double min, double max) = GetLuminosityRangeTuple(spectralClass);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetLuminosityRangeTuple(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (30000.0, 1000000.0),
            StarClass.SpectralClass.B => (25.0, 30000.0),
            StarClass.SpectralClass.A => (5.0, 25.0),
            StarClass.SpectralClass.F => (1.5, 5.0),
            StarClass.SpectralClass.G => (0.6, 1.5),
            StarClass.SpectralClass.K => (0.08, 0.6),
            StarClass.SpectralClass.M => (0.0001, 0.08),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the radius range for a spectral class in solar radii.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) radius range in solar radii.</returns>
    public static NumericRange GetRadiusRange(StarClass.SpectralClass spectralClass)
    {
        (double min, double max) = GetRadiusRangeTuple(spectralClass);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetRadiusRangeTuple(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (6.6, 20.0),
            StarClass.SpectralClass.B => (1.8, 6.6),
            StarClass.SpectralClass.A => (1.4, 1.8),
            StarClass.SpectralClass.F => (1.15, 1.4),
            StarClass.SpectralClass.G => (0.96, 1.15),
            StarClass.SpectralClass.K => (0.7, 0.96),
            StarClass.SpectralClass.M => (0.1, 0.7),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the main-sequence lifetime range for a spectral class in years.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) lifetime range in years.</returns>
    public static NumericRange GetLifetimeRange(StarClass.SpectralClass spectralClass)
    {
        (double min, double max) = GetLifetimeRangeTuple(spectralClass);
        return new NumericRange(min, max);
    }

    /// <summary>
    /// Tuple-returning helper used by C# internals.
    /// </summary>
    public static (double Min, double Max) GetLifetimeRangeTuple(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (1.0e6, 10.0e6),
            StarClass.SpectralClass.B => (10.0e6, 100.0e6),
            StarClass.SpectralClass.A => (100.0e6, 1.0e9),
            StarClass.SpectralClass.F => (1.0e9, 4.0e9),
            StarClass.SpectralClass.G => (4.0e9, 10.0e9),
            StarClass.SpectralClass.K => (15.0e9, 50.0e9),
            StarClass.SpectralClass.M => (50.0e9, 200.0e9),
            _ => (0.0, 0.0),
        };
    }

    /// <summary>
    /// Approximates luminosity from mass for a main-sequence star using the mass-luminosity relation L ∝ M^3.5.
    /// </summary>
    /// <param name="massSolar">Stellar mass in solar masses.</param>
    /// <returns>Luminosity in solar luminosities.</returns>
    public static double LuminosityFromMass(double massSolar)
    {
        if (massSolar <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(massSolar, 3.5);
    }

    /// <summary>
    /// Approximates radius from mass for a main-sequence star using R ∝ M^0.8.
    /// </summary>
    /// <param name="massSolar">Stellar mass in solar masses.</param>
    /// <returns>Radius in solar radii.</returns>
    public static double RadiusFromMass(double massSolar)
    {
        if (massSolar <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(massSolar, 0.8);
    }

    /// <summary>
    /// Calculates temperature from luminosity and radius via Stefan-Boltzmann: T ∝ (L/R²)^0.25.
    /// </summary>
    /// <param name="luminositySolar">Luminosity in solar luminosities.</param>
    /// <param name="radiusSolar">Radius in solar radii.</param>
    /// <returns>Effective temperature in Kelvin.</returns>
    public static double TemperatureFromLuminosityRadius(double luminositySolar, double radiusSolar)
    {
        if (luminositySolar <= 0.0 || radiusSolar <= 0.0)
        {
            return 0.0;
        }

        return 5778.0 * System.Math.Pow(luminositySolar, 0.25) / System.Math.Pow(radiusSolar, 0.5);
    }

    /// <summary>
    /// Interpolates a value within a spectral class by subclass index (0 = hottest, 9 = coolest).
    /// </summary>
    /// <param name="spectralClass">Target spectral class (used for context only).</param>
    /// <param name="subclass">Subclass index 0–9.</param>
    /// <param name="range">Min/max range to interpolate within.</param>
    /// <returns>Interpolated value.</returns>
    public static double InterpolateBySubclass(
        StarClass.SpectralClass spectralClass,
        int subclass,
        NumericRange range)
    {
        double rangeMin = range.Min;
        double rangeMax = range.Max;
        float interpolation = subclass / 9.0f;
        return Mathf.Lerp((float)rangeMax, (float)rangeMin, interpolation);
    }

    /// <summary>
    /// Returns the typical rotation period range in days for a main-sequence star of the given class.
    /// Ranges are drawn from observational surveys (Skumanich 1972; Barnes 2003; McQuillan et al. 2014;
    /// Reinhold &amp; Gizon 2015). Early-type stars are rapid rotators; late-type stars spin down over
    /// Gyr via magnetic braking and can reach 100+ days.
    /// </summary>
    /// <param name="spectralClass">Target spectral class.</param>
    /// <returns>Inclusive (Min, Max) rotation period range in days.</returns>
    public static (double Min, double Max) GetRotationPeriodRangeDays(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => (1.0, 5.0),
            StarClass.SpectralClass.B => (1.0, 10.0),
            StarClass.SpectralClass.A => (1.0, 15.0),
            StarClass.SpectralClass.F => (5.0, 30.0),
            StarClass.SpectralClass.G => (15.0, 35.0),
            StarClass.SpectralClass.K => (20.0, 55.0),
            StarClass.SpectralClass.M => (30.0, 150.0),
            _ => (10.0, 50.0),
        };
    }

    /// <summary>
    /// Infers a spectral class from temperature.
    /// </summary>
    /// <param name="temperatureK">Effective temperature in Kelvin.</param>
    /// <returns>Spectral class corresponding to the temperature.</returns>
    public static StarClass.SpectralClass ClassFromTemperature(double temperatureK)
    {
        return temperatureK switch
        {
            >= 30000.0 => StarClass.SpectralClass.O,
            > 10000.0 => StarClass.SpectralClass.B,
            > 7500.0 => StarClass.SpectralClass.A,
            > 6000.0 => StarClass.SpectralClass.F,
            > 5200.0 => StarClass.SpectralClass.G,
            > 3700.0 => StarClass.SpectralClass.K,
            _ => StarClass.SpectralClass.M,
        };
    }

}
