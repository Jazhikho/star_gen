using System.Globalization;
using System.Text;
using StarGen.Domain.Celestial;
using StarGen.Domain.Math;
using StarGen.Domain.Population;

namespace StarGen.App.Viewer;

/// <summary>
/// Formats display-facing property values consistently for UI panels.
/// </summary>
public static class PropertyFormatter
{
    /// <summary>
    /// Formats mass with body-appropriate units.
    /// </summary>
    public static string FormatMass(double massKg, CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.000} M\u2609",
                massKg / Units.SolarMassKg),
            CelestialType.Type.Planet or CelestialType.Type.Moon => FormatPlanetaryMass(massKg),
            _ => FormatScientific(massKg, "kg"),
        };
    }

    /// <summary>
    /// Formats radius with body-appropriate units.
    /// </summary>
    public static string FormatRadius(double radiusM, CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.000} R\u2609",
                radiusM / Units.SolarRadiusMeters),
            CelestialType.Type.Planet or CelestialType.Type.Moon => string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.0000} R\u2295",
                radiusM / Units.EarthRadiusMeters),
            _ => FormatAsteroidRadius(radiusM),
        };
    }

    /// <summary>
    /// Formats a distance in AU or kilometers.
    /// </summary>
    public static string FormatDistance(double distanceM)
    {
        double au = distanceM / Units.AuMeters;
        if (au > 0.1)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", au);
        }

        double km = distanceM / 1000.0;
        if (km > 1000.0)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0} km", km);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.0} km", km);
    }

    /// <summary>
    /// Formats luminosity in solar units or scientific notation.
    /// </summary>
    public static string FormatLuminosity(double luminosityWatts)
    {
        double solar = luminosityWatts / 3.828e26;
        if (solar > 0.01)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0000} L\u2609", solar);
        }

        return FormatScientific(luminosityWatts, "W");
    }

    /// <summary>
    /// Formats age in years, Myr, or Gyr.
    /// </summary>
    public static string FormatAge(double years)
    {
        if (years > 1e9)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} Gyr", years / 1e9);
        }

        if (years > 1e6)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} Myr", years / 1e6);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0} years", years);
    }

    /// <summary>
    /// Formats pressure in atmospheres or Pascals.
    /// </summary>
    public static string FormatPressure(double pressurePa)
    {
        double atmospheres = pressurePa / 101325.0;
        if (atmospheres > 0.01)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0000} atm", atmospheres);
        }

        if (pressurePa > 1.0)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} Pa", pressurePa);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.00e+0} Pa", pressurePa);
    }

    /// <summary>
    /// Formats a particle size in mm, cm, or meters.
    /// </summary>
    public static string FormatParticleSize(double sizeM)
    {
        if (sizeM < 0.01)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} mm", sizeM * 1000.0);
        }

        if (sizeM < 1.0)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} cm", sizeM * 100.0);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.00} m", sizeM);
    }

    /// <summary>
    /// Formats a value in scientific notation with superscript exponent digits.
    /// </summary>
    public static string FormatScientific(double value, string unit)
    {
        if (value == 0.0)
        {
            return $"0 {unit}";
        }

        int exponent = (int)System.Math.Floor(System.Math.Log10(System.Math.Abs(value)));
        double mantissa = value / System.Math.Pow(10.0, exponent);
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.00} \u00D7 10{1} {2}",
            mantissa,
            FormatSuperscript(exponent),
            unit);
    }

    /// <summary>
    /// Converts an integer exponent to superscript digits.
    /// </summary>
    public static string FormatSuperscript(int number)
    {
        StringBuilder builder = new();
        string digits = number.ToString(CultureInfo.InvariantCulture);
        foreach (char current in digits)
        {
            builder.Append(current switch
            {
                '0' => '\u2070',
                '1' => '\u00B9',
                '2' => '\u00B2',
                '3' => '\u00B3',
                '4' => '\u2074',
                '5' => '\u2075',
                '6' => '\u2076',
                '7' => '\u2077',
                '8' => '\u2078',
                '9' => '\u2079',
                '-' => '\u207B',
                _ => current,
            });
        }

        return builder.ToString();
    }

    /// <summary>
    /// Formats a population count using K, M, or B suffixes.
    /// </summary>
    public static string FormatPopulation(int count)
    {
        if (count <= 0)
        {
            return "0";
        }

        if (count < 1_000)
        {
            return count.ToString(CultureInfo.InvariantCulture);
        }

        if (count < 1_000_000)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0}K", count / 1000.0);
        }

        if (count < 1_000_000_000)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00}M", count / 1_000_000.0);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.00}B", count / 1_000_000_000.0);
    }

    /// <summary>
    /// Formats a 0-10 habitability score with its category.
    /// </summary>
    public static string FormatHabitability(int score)
    {
        string category = HabitabilityCategory.ToStringName(HabitabilityCategory.FromScore(score));
        return string.Format(CultureInfo.InvariantCulture, "{0}/10 ({1})", score, category);
    }

    /// <summary>
    /// Formats a 0-100 suitability score with its category.
    /// </summary>
    public static string FormatSuitability(int score)
    {
        ColonySuitability suitability = new()
        {
            OverallScore = score,
        };
        return string.Format(CultureInfo.InvariantCulture, "{0}/100 ({1})", score, suitability.GetCategoryString());
    }

    /// <summary>
    /// Formats a technology-level enum value.
    /// </summary>
    public static string FormatTechLevel(TechnologyLevel.Level level)
    {
        return TechnologyLevel.ToStringName(level);
    }

    /// <summary>
    /// Formats a government regime enum value.
    /// </summary>
    public static string FormatRegime(GovernmentType.Regime regime)
    {
        return GovernmentType.ToStringName(regime);
    }

    /// <summary>
    /// Formats a colony type enum value.
    /// </summary>
    public static string FormatColonyType(ColonyType.Type type)
    {
        return ColonyType.ToStringName(type);
    }

    /// <summary>
    /// Formats a normalized 0-1 value as a percentage.
    /// </summary>
    public static string FormatPercent(double value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.0}%", value * 100.0);
    }

    /// <summary>
    /// Formats temperature in Kelvin and Celsius.
    /// </summary>
    public static string FormatTemperature(double tempK)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:0} K ({1:0} C)",
            tempK,
            tempK - 273.15);
    }

    /// <summary>
    /// Formats the political-situation string for display.
    /// </summary>
    public static string FormatPoliticalSituation(string situation)
    {
        return situation switch
        {
            "uninhabited" => "Uninhabited",
            "native_only" => "Native Only",
            "colony_only" => "Colony Only",
            "coexisting" => "Coexisting",
            "conflict" => "Conflict",
            _ => CapitalizeFallback(situation),
        };
    }

    /// <summary>
    /// Formats non-star masses using Earth or Jupiter units.
    /// </summary>
    private static string FormatPlanetaryMass(double massKg)
    {
        double earthMasses = massKg / Units.EarthMassKg;
        if (earthMasses > 100.0)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} MJ", massKg / 1.898e27);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.0000} M\u2295", earthMasses);
    }

    /// <summary>
    /// Formats non-planet radii in meters or kilometers.
    /// </summary>
    private static string FormatAsteroidRadius(double radiusM)
    {
        double km = radiusM / 1000.0;
        if (km < 1.0)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} m", radiusM);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.00} km", km);
    }

    /// <summary>
    /// Produces a simple display-case fallback for unmapped status strings.
    /// </summary>
    private static string CapitalizeFallback(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string normalized = text.Replace('_', ' ').ToLowerInvariant();
        return char.ToUpperInvariant(normalized[0]) + normalized[1..];
    }
}
