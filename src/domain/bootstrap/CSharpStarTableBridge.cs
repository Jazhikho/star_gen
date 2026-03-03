using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for pure star-table helpers.
/// </summary>
[GlobalClass]
public partial class CSharpStarTableBridge : RefCounted
{
    /// <summary>
    /// Returns the mass range for a spectral class.
    /// </summary>
    public Dictionary GetMassRange(int spectralClass) => StarTable.GetMassRange((StarClass.SpectralClass)spectralClass);

    /// <summary>
    /// Returns the temperature range for a spectral class.
    /// </summary>
    public Dictionary GetTemperatureRange(int spectralClass) => StarTable.GetTemperatureRange((StarClass.SpectralClass)spectralClass);

    /// <summary>
    /// Returns the luminosity range for a spectral class.
    /// </summary>
    public Dictionary GetLuminosityRange(int spectralClass) => StarTable.GetLuminosityRange((StarClass.SpectralClass)spectralClass);

    /// <summary>
    /// Returns the radius range for a spectral class.
    /// </summary>
    public Dictionary GetRadiusRange(int spectralClass) => StarTable.GetRadiusRange((StarClass.SpectralClass)spectralClass);

    /// <summary>
    /// Returns the lifetime range for a spectral class.
    /// </summary>
    public Dictionary GetLifetimeRange(int spectralClass) => StarTable.GetLifetimeRange((StarClass.SpectralClass)spectralClass);

    /// <summary>
    /// Approximates luminosity from stellar mass.
    /// </summary>
    public double LuminosityFromMass(double massSolar) => StarTable.LuminosityFromMass(massSolar);

    /// <summary>
    /// Approximates radius from stellar mass.
    /// </summary>
    public double RadiusFromMass(double massSolar) => StarTable.RadiusFromMass(massSolar);

    /// <summary>
    /// Calculates temperature from luminosity and radius.
    /// </summary>
    public double TemperatureFromLuminosityRadius(double luminositySolar, double radiusSolar)
    {
        return StarTable.TemperatureFromLuminosityRadius(luminositySolar, radiusSolar);
    }

    /// <summary>
    /// Interpolates a value within a spectral class by subclass.
    /// </summary>
    public double InterpolateBySubclass(int spectralClass, int subclass, Dictionary rangeData)
    {
        return StarTable.InterpolateBySubclass((StarClass.SpectralClass)spectralClass, subclass, rangeData);
    }

    /// <summary>
    /// Infers a spectral class from temperature.
    /// </summary>
    public int ClassFromTemperature(double temperatureK) => (int)StarTable.ClassFromTemperature(temperatureK);
}
