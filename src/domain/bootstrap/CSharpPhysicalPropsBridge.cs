using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for physical-property calculations.
/// </summary>
[GlobalClass]
public partial class CSharpPhysicalPropsBridge : RefCounted
{
    /// <summary>
    /// Calculates volume in cubic meters.
    /// </summary>
    public double GetVolumeM3(double radiusM) => new PhysicalProps(radiusM: radiusM).GetVolumeM3();

    /// <summary>
    /// Calculates mean density in kilograms per cubic meter.
    /// </summary>
    public double GetDensityKgM3(double massKg, double radiusM) => new PhysicalProps(massKg: massKg, radiusM: radiusM).GetDensityKgM3();

    /// <summary>
    /// Calculates surface gravity in meters per second squared.
    /// </summary>
    public double GetSurfaceGravityMS2(double massKg, double radiusM) => new PhysicalProps(massKg: massKg, radiusM: radiusM).GetSurfaceGravityMS2();

    /// <summary>
    /// Calculates escape velocity in meters per second.
    /// </summary>
    public double GetEscapeVelocityMS(double massKg, double radiusM) => new PhysicalProps(massKg: massKg, radiusM: radiusM).GetEscapeVelocityMS();

    /// <summary>
    /// Returns equatorial radius in meters.
    /// </summary>
    public double GetEquatorialRadiusM(double radiusM, double oblateness) => new PhysicalProps(radiusM: radiusM, oblateness: oblateness).GetEquatorialRadiusM();

    /// <summary>
    /// Returns polar radius in meters.
    /// </summary>
    public double GetPolarRadiusM(double radiusM, double oblateness) => new PhysicalProps(radiusM: radiusM, oblateness: oblateness).GetPolarRadiusM();
}
