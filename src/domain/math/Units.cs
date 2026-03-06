namespace StarGen.Domain.Math;

/// <summary>
/// Physical unit constants and conversion helpers for celestial bodies.
/// </summary>
public static class Units
{
    /// <summary>
    /// Newtonian gravitational constant in m³ kg⁻¹ s⁻².
    /// NIST CODATA 2018: 6.67430 × 10⁻¹¹.
    /// </summary>
    public const double G = 6.674e-11;

    /// <summary>
    /// Mass of the Sun in kilograms.
    /// </summary>
    public const double SolarMassKg = 1.989e30;

    /// <summary>
    /// Mass of Earth in kilograms.
    /// </summary>
    public const double EarthMassKg = 5.972e24;

    /// <summary>
    /// Mass of Jupiter in kilograms.
    /// </summary>
    public const double JupiterMassKg = 1.898e27;

    /// <summary>
    /// One astronomical unit in meters.
    /// </summary>
    public const double AuMeters = 1.496e11;

    /// <summary>
    /// One light year in meters.
    /// </summary>
    public const double LightYearMeters = 9.461e15;

    /// <summary>
    /// One parsec in meters.
    /// </summary>
    public const double ParsecMeters = 3.086e16;

    /// <summary>
    /// Radius of the Sun in meters.
    /// </summary>
    public const double SolarRadiusMeters = 6.957e8;

    /// <summary>
    /// Radius of Earth in meters.
    /// </summary>
    public const double EarthRadiusMeters = 6.371e6;

    /// <summary>
    /// Radius of Jupiter in meters.
    /// </summary>
    public const double JupiterRadiusMeters = 6.991e7;

    /// <summary>
    /// Offset for Celsius to Kelvin conversion.
    /// </summary>
    public const double CelsiusToKelvinOffset = 273.15;

    /// <summary>
    /// Converts solar masses to kilograms.
    /// </summary>
    public static double SolarMassesToKg(double solarMasses) => solarMasses * SolarMassKg;

    /// <summary>
    /// Converts kilograms to solar masses.
    /// </summary>
    public static double KgToSolarMasses(double kg) => kg / SolarMassKg;

    /// <summary>
    /// Converts Earth masses to kilograms.
    /// </summary>
    public static double EarthMassesToKg(double earthMasses) => earthMasses * EarthMassKg;

    /// <summary>
    /// Converts kilograms to Earth masses.
    /// </summary>
    public static double KgToEarthMasses(double kg) => kg / EarthMassKg;

    /// <summary>
    /// Converts Jupiter masses to kilograms.
    /// </summary>
    public static double JupiterMassesToKg(double jupiterMasses) => jupiterMasses * JupiterMassKg;

    /// <summary>
    /// Converts kilograms to Jupiter masses.
    /// </summary>
    public static double KgToJupiterMasses(double kg) => kg / JupiterMassKg;

    /// <summary>
    /// Converts astronomical units to meters.
    /// </summary>
    public static double AuToMeters(double au) => au * AuMeters;

    /// <summary>
    /// Converts meters to astronomical units.
    /// </summary>
    public static double MetersToAu(double meters) => meters / AuMeters;

    /// <summary>
    /// Converts light years to meters.
    /// </summary>
    public static double LightYearsToMeters(double lightYears) => lightYears * LightYearMeters;

    /// <summary>
    /// Converts meters to light years.
    /// </summary>
    public static double MetersToLightYears(double meters) => meters / LightYearMeters;

    /// <summary>
    /// Converts parsecs to meters.
    /// </summary>
    public static double ParsecsToMeters(double parsecs) => parsecs * ParsecMeters;

    /// <summary>
    /// Converts meters to parsecs.
    /// </summary>
    public static double MetersToParsecs(double meters) => meters / ParsecMeters;

    /// <summary>
    /// Converts solar radii to meters.
    /// </summary>
    public static double SolarRadiiToMeters(double solarRadii) => solarRadii * SolarRadiusMeters;

    /// <summary>
    /// Converts meters to solar radii.
    /// </summary>
    public static double MetersToSolarRadii(double meters) => meters / SolarRadiusMeters;

    /// <summary>
    /// Converts Earth radii to meters.
    /// </summary>
    public static double EarthRadiiToMeters(double earthRadii) => earthRadii * EarthRadiusMeters;

    /// <summary>
    /// Converts meters to Earth radii.
    /// </summary>
    public static double MetersToEarthRadii(double meters) => meters / EarthRadiusMeters;

    /// <summary>
    /// Converts Celsius to Kelvin.
    /// </summary>
    public static double CelsiusToKelvin(double celsius) => celsius + CelsiusToKelvinOffset;

    /// <summary>
    /// Converts Kelvin to Celsius.
    /// </summary>
    public static double KelvinToCelsius(double kelvin) => kelvin - CelsiusToKelvinOffset;
}
