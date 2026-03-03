using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Stellar spectral classifications for main sequence stars.
/// </summary>
public static class StarClass
{
    /// <summary>
    /// Main spectral classes from hottest to coolest.
    /// </summary>
    public enum SpectralClass
    {
        O,
        B,
        A,
        F,
        G,
        K,
        M,
    }

    /// <summary>
    /// Returns the single-letter designation for a spectral class.
    /// </summary>
    public static string ToLetter(SpectralClass spectralClass)
    {
        return spectralClass switch
        {
            SpectralClass.O => "O",
            SpectralClass.B => "B",
            SpectralClass.A => "A",
            SpectralClass.F => "F",
            SpectralClass.G => "G",
            SpectralClass.K => "K",
            SpectralClass.M => "M",
            _ => "?",
        };
    }

    /// <summary>
    /// Parses a spectral class letter.
    /// </summary>
    public static bool TryParseLetter(string letter, out SpectralClass spectralClass)
    {
        switch (letter.ToUpperInvariant())
        {
            case "O":
                spectralClass = SpectralClass.O;
                return true;
            case "B":
                spectralClass = SpectralClass.B;
                return true;
            case "A":
                spectralClass = SpectralClass.A;
                return true;
            case "F":
                spectralClass = SpectralClass.F;
                return true;
            case "G":
                spectralClass = SpectralClass.G;
                return true;
            case "K":
                spectralClass = SpectralClass.K;
                return true;
            case "M":
                spectralClass = SpectralClass.M;
                return true;
            default:
                spectralClass = default;
                return false;
        }
    }

    /// <summary>
    /// Builds a full spectral string such as G2V.
    /// </summary>
    public static string BuildSpectralString(
        SpectralClass spectralClass,
        int subclass,
        string luminosityClass = "V")
    {
        int clampedSubclass = System.Math.Clamp(subclass, 0, 9);
        return $"{ToLetter(spectralClass)}{clampedSubclass}{luminosityClass}";
    }

    /// <summary>
    /// Parses a spectral string into its components.
    /// </summary>
    public static Dictionary<string, Variant> ParseSpectralString(string spectralString)
    {
        if (spectralString.Length < 2)
        {
            return new Dictionary<string, Variant>();
        }

        string letter = spectralString[..1];
        if (!TryParseLetter(letter, out SpectralClass spectralClass))
        {
            return new Dictionary<string, Variant>();
        }

        string subclassToken = spectralString.Substring(1, 1);
        if (!int.TryParse(subclassToken, out int subclass))
        {
            return new Dictionary<string, Variant>();
        }

        string luminosityClass = spectralString.Length > 2 ? spectralString[2..] : "V";

        return new Dictionary<string, Variant>
        {
            ["spectral_class"] = (int)spectralClass,
            ["subclass"] = subclass,
            ["luminosity_class"] = luminosityClass,
        };
    }

    /// <summary>
    /// Returns the number of spectral classes.
    /// </summary>
    public static int Count() => 7;
}
