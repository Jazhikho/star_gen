using Godot;
using StarGen.Domain.Math;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for mass unit conversions.
/// </summary>
[GlobalClass]
public partial class CSharpMassUnitsBridge : RefCounted
{
    /// <summary>
    /// Converts solar masses to kilograms.
    /// </summary>
    public double SolarMassesToKg(double solarMasses) => Units.SolarMassesToKg(solarMasses);

    /// <summary>
    /// Converts kilograms to solar masses.
    /// </summary>
    public double KgToSolarMasses(double kg) => Units.KgToSolarMasses(kg);

    /// <summary>
    /// Converts Earth masses to kilograms.
    /// </summary>
    public double EarthMassesToKg(double earthMasses) => Units.EarthMassesToKg(earthMasses);

    /// <summary>
    /// Converts kilograms to Earth masses.
    /// </summary>
    public double KgToEarthMasses(double kg) => Units.KgToEarthMasses(kg);

    /// <summary>
    /// Converts Jupiter masses to kilograms.
    /// </summary>
    public double JupiterMassesToKg(double jupiterMasses) => Units.JupiterMassesToKg(jupiterMasses);

    /// <summary>
    /// Converts kilograms to Jupiter masses.
    /// </summary>
    public double KgToJupiterMasses(double kg) => Units.KgToJupiterMasses(kg);
}
