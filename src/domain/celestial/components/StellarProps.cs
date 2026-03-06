using Godot;
using Godot.Collections;
using StarGen.Domain.Math;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Stellar properties for star-type celestial bodies.
/// </summary>
public partial class StellarProps : RefCounted
{
    /// <summary>
    /// Solar luminosity in watts for reference.
    /// </summary>
    public const double SolarLuminosityWatts = 3.828e26;

    /// <summary>
    /// Luminosity in watts.
    /// </summary>
    public double LuminosityWatts;

    /// <summary>
    /// Effective photosphere temperature in Kelvin.
    /// </summary>
    public double EffectiveTemperatureK;

    /// <summary>
    /// Spectral classification string.
    /// </summary>
    public string SpectralClass;

    /// <summary>
    /// Legacy alias for spectral classification string.
    /// </summary>
    public string SpectralType
    {
        get => SpectralClass;
        set => SpectralClass = value;
    }

    /// <summary>
    /// Stellar type category.
    /// </summary>
    public string StellarType;

    /// <summary>
    /// Metallicity relative to solar.
    /// </summary>
    public double Metallicity;

    /// <summary>
    /// Age in years.
    /// </summary>
    public double AgeYears;

    /// <summary>
    /// Creates a new stellar-properties component.
    /// </summary>
    public StellarProps(
        double luminosityWatts = 0.0,
        double effectiveTemperatureK = 0.0,
        string spectralClass = "",
        string stellarType = "main_sequence",
        double metallicity = 1.0,
        double ageYears = 0.0)
    {
        LuminosityWatts = luminosityWatts;
        EffectiveTemperatureK = effectiveTemperatureK;
        SpectralClass = spectralClass;
        StellarType = stellarType;
        Metallicity = metallicity;
        AgeYears = ageYears;
    }

    /// <summary>
    /// Returns luminosity in solar luminosities.
    /// </summary>
    public double GetLuminositySolar()
    {
        return LuminosityWatts / SolarLuminosityWatts;
    }

    /// <summary>
    /// Calculates the habitable-zone inner edge in meters.
    /// </summary>
    public double GetHabitableZoneInnerM()
    {
        double luminositySolar = GetLuminositySolar();
        if (luminositySolar <= 0.0)
        {
            return 0.0;
        }

        return Units.AuToMeters(0.95 * System.Math.Sqrt(luminositySolar));
    }

    /// <summary>
    /// Calculates the habitable-zone outer edge in meters.
    /// </summary>
    public double GetHabitableZoneOuterM()
    {
        double luminositySolar = GetLuminositySolar();
        if (luminositySolar <= 0.0)
        {
            return 0.0;
        }

        return Units.AuToMeters(1.37 * System.Math.Sqrt(luminositySolar));
    }

    /// <summary>
    /// Calculates the frost-line distance in meters.
    /// </summary>
    public double GetFrostLineM()
    {
        double luminositySolar = GetLuminositySolar();
        if (luminositySolar <= 0.0)
        {
            return 0.0;
        }

        return Units.AuToMeters(2.7 * System.Math.Sqrt(luminositySolar));
    }

    /// <summary>
    /// Extracts the spectral letter from the spectral class.
    /// </summary>
    public string GetSpectralLetter()
    {
        if (string.IsNullOrEmpty(SpectralClass))
        {
            return string.Empty;
        }

        return SpectralClass[..1].ToUpperInvariant();
    }

    /// <summary>
    /// Extracts the luminosity class from the spectral class.
    /// </summary>
    public string GetLuminosityClass()
    {
        if (SpectralClass.Length < 3)
        {
            return string.Empty;
        }

        for (int index = 2; index < SpectralClass.Length; index += 1)
        {
            char current = SpectralClass[index];
            if (current == 'I' || current == 'V')
            {
                return SpectralClass[index..];
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["luminosity_watts"] = LuminosityWatts,
            ["effective_temperature_k"] = EffectiveTemperatureK,
            ["spectral_class"] = SpectralClass,
            ["stellar_type"] = StellarType,
            ["metallicity"] = Metallicity,
            ["age_years"] = AgeYears,
        };
    }

    /// <summary>
    /// Creates a stellar-properties component from a dictionary.
    /// </summary>
    public static StellarProps FromDictionary(Dictionary data)
    {
        return new StellarProps(
            GetDouble(data, "luminosity_watts", 0.0),
            GetDouble(data, "effective_temperature_k", 0.0),
            GetString(data, "spectral_class", ""),
            GetString(data, "stellar_type", "main_sequence"),
            GetDouble(data, "metallicity", 1.0),
            GetDouble(data, "age_years", 0.0));
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }
}
