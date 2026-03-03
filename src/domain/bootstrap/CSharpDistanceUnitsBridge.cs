using Godot;
using StarGen.Domain.Math;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for distance unit conversions.
/// </summary>
[GlobalClass]
public partial class CSharpDistanceUnitsBridge : RefCounted
{
    /// <summary>
    /// Converts astronomical units to meters.
    /// </summary>
    public double AuToMeters(double au) => Units.AuToMeters(au);

    /// <summary>
    /// Converts meters to astronomical units.
    /// </summary>
    public double MetersToAu(double meters) => Units.MetersToAu(meters);

    /// <summary>
    /// Converts light years to meters.
    /// </summary>
    public double LightYearsToMeters(double lightYears) => Units.LightYearsToMeters(lightYears);

    /// <summary>
    /// Converts meters to light years.
    /// </summary>
    public double MetersToLightYears(double meters) => Units.MetersToLightYears(meters);

    /// <summary>
    /// Converts parsecs to meters.
    /// </summary>
    public double ParsecsToMeters(double parsecs) => Units.ParsecsToMeters(parsecs);

    /// <summary>
    /// Converts meters to parsecs.
    /// </summary>
    public double MetersToParsecs(double meters) => Units.MetersToParsecs(meters);
}
