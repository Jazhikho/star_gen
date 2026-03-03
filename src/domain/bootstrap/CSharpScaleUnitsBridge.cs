using Godot;
using StarGen.Domain.Math;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for radius and temperature conversions.
/// </summary>
[GlobalClass]
public partial class CSharpScaleUnitsBridge : RefCounted
{
    /// <summary>
    /// Converts solar radii to meters.
    /// </summary>
    public double SolarRadiiToMeters(double solarRadii) => Units.SolarRadiiToMeters(solarRadii);

    /// <summary>
    /// Converts meters to solar radii.
    /// </summary>
    public double MetersToSolarRadii(double meters) => Units.MetersToSolarRadii(meters);

    /// <summary>
    /// Converts Earth radii to meters.
    /// </summary>
    public double EarthRadiiToMeters(double earthRadii) => Units.EarthRadiiToMeters(earthRadii);

    /// <summary>
    /// Converts meters to Earth radii.
    /// </summary>
    public double MetersToEarthRadii(double meters) => Units.MetersToEarthRadii(meters);

    /// <summary>
    /// Converts Celsius to Kelvin.
    /// </summary>
    public double CelsiusToKelvin(double celsius) => Units.CelsiusToKelvin(celsius);

    /// <summary>
    /// Converts Kelvin to Celsius.
    /// </summary>
    public double KelvinToCelsius(double kelvin) => Units.KelvinToCelsius(kelvin);
}
