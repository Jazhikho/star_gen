using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for stellar-property calculations.
/// </summary>
[GlobalClass]
public partial class CSharpStellarPropsBridge : RefCounted
{
    /// <summary>
    /// Returns luminosity in solar units.
    /// </summary>
    public double GetLuminositySolar(double luminosityWatts) => new StellarProps(luminosityWatts: luminosityWatts).GetLuminositySolar();

    /// <summary>
    /// Returns the inner habitable-zone distance in meters.
    /// </summary>
    public double GetHabitableZoneInnerM(double luminosityWatts) => new StellarProps(luminosityWatts: luminosityWatts).GetHabitableZoneInnerM();

    /// <summary>
    /// Returns the outer habitable-zone distance in meters.
    /// </summary>
    public double GetHabitableZoneOuterM(double luminosityWatts) => new StellarProps(luminosityWatts: luminosityWatts).GetHabitableZoneOuterM();

    /// <summary>
    /// Returns the frost-line distance in meters.
    /// </summary>
    public double GetFrostLineM(double luminosityWatts) => new StellarProps(luminosityWatts: luminosityWatts).GetFrostLineM();

    /// <summary>
    /// Extracts the spectral letter.
    /// </summary>
    public string GetSpectralLetter(string spectralClass) => new StellarProps(spectralClass: spectralClass).GetSpectralLetter();

    /// <summary>
    /// Extracts the luminosity class.
    /// </summary>
    public string GetLuminosityClass(string spectralClass) => new StellarProps(spectralClass: spectralClass).GetLuminosityClass();
}
