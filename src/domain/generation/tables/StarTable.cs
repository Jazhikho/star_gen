using Godot;
using Godot.Collections;
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
    public static Dictionary GetMassRange(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => BuildRange(16.0, 150.0),
            StarClass.SpectralClass.B => BuildRange(2.1, 16.0),
            StarClass.SpectralClass.A => BuildRange(1.4, 2.1),
            StarClass.SpectralClass.F => BuildRange(1.04, 1.4),
            StarClass.SpectralClass.G => BuildRange(0.8, 1.04),
            StarClass.SpectralClass.K => BuildRange(0.45, 0.8),
            StarClass.SpectralClass.M => BuildRange(0.08, 0.45),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the temperature range for a spectral class in Kelvin.
    /// </summary>
    public static Dictionary GetTemperatureRange(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => BuildRange(30000.0, 52000.0),
            StarClass.SpectralClass.B => BuildRange(10000.0, 30000.0),
            StarClass.SpectralClass.A => BuildRange(7500.0, 10000.0),
            StarClass.SpectralClass.F => BuildRange(6000.0, 7500.0),
            StarClass.SpectralClass.G => BuildRange(5200.0, 6000.0),
            StarClass.SpectralClass.K => BuildRange(3700.0, 5200.0),
            StarClass.SpectralClass.M => BuildRange(2400.0, 3700.0),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the luminosity range for a spectral class in solar luminosities.
    /// </summary>
    public static Dictionary GetLuminosityRange(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => BuildRange(30000.0, 1000000.0),
            StarClass.SpectralClass.B => BuildRange(25.0, 30000.0),
            StarClass.SpectralClass.A => BuildRange(5.0, 25.0),
            StarClass.SpectralClass.F => BuildRange(1.5, 5.0),
            StarClass.SpectralClass.G => BuildRange(0.6, 1.5),
            StarClass.SpectralClass.K => BuildRange(0.08, 0.6),
            StarClass.SpectralClass.M => BuildRange(0.0001, 0.08),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the radius range for a spectral class in solar radii.
    /// </summary>
    public static Dictionary GetRadiusRange(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => BuildRange(6.6, 20.0),
            StarClass.SpectralClass.B => BuildRange(1.8, 6.6),
            StarClass.SpectralClass.A => BuildRange(1.4, 1.8),
            StarClass.SpectralClass.F => BuildRange(1.15, 1.4),
            StarClass.SpectralClass.G => BuildRange(0.96, 1.15),
            StarClass.SpectralClass.K => BuildRange(0.7, 0.96),
            StarClass.SpectralClass.M => BuildRange(0.1, 0.7),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Returns the main-sequence lifetime range for a spectral class in years.
    /// </summary>
    public static Dictionary GetLifetimeRange(StarClass.SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            StarClass.SpectralClass.O => BuildRange(1.0e6, 10.0e6),
            StarClass.SpectralClass.B => BuildRange(10.0e6, 100.0e6),
            StarClass.SpectralClass.A => BuildRange(100.0e6, 1.0e9),
            StarClass.SpectralClass.F => BuildRange(1.0e9, 4.0e9),
            StarClass.SpectralClass.G => BuildRange(4.0e9, 10.0e9),
            StarClass.SpectralClass.K => BuildRange(15.0e9, 50.0e9),
            StarClass.SpectralClass.M => BuildRange(50.0e9, 200.0e9),
            _ => BuildRange(0.0, 0.0),
        };
    }

    /// <summary>
    /// Approximates luminosity from mass for a main-sequence star.
    /// </summary>
    public static double LuminosityFromMass(double massSolar)
    {
        if (massSolar <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(massSolar, 3.5);
    }

    /// <summary>
    /// Approximates radius from mass for a main-sequence star.
    /// </summary>
    public static double RadiusFromMass(double massSolar)
    {
        if (massSolar <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Pow(massSolar, 0.8);
    }

    /// <summary>
    /// Calculates temperature from luminosity and radius.
    /// </summary>
    public static double TemperatureFromLuminosityRadius(double luminositySolar, double radiusSolar)
    {
        if (luminositySolar <= 0.0 || radiusSolar <= 0.0)
        {
            return 0.0;
        }

        return 5778.0 * System.Math.Pow(luminositySolar, 0.25) / System.Math.Pow(radiusSolar, 0.5);
    }

    /// <summary>
    /// Interpolates a value within a spectral class by subclass.
    /// </summary>
    public static double InterpolateBySubclass(
        StarClass.SpectralClass spectralClass,
        int subclass,
        Dictionary rangeData)
    {
        float interpolation = subclass / 9.0f;
        float minValue = (float)(double)rangeData["min"];
        float maxValue = (float)(double)rangeData["max"];
        return Mathf.Lerp(maxValue, minValue, interpolation);
    }

    /// <summary>
    /// Infers a spectral class from temperature.
    /// </summary>
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

    private static Dictionary BuildRange(double min, double max)
    {
        return new Dictionary
        {
            ["min"] = min,
            ["max"] = max,
        };
    }
}
