using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for orbital-property calculations.
/// </summary>
[GlobalClass]
public partial class CSharpOrbitalPropsBridge : RefCounted
{
    /// <summary>
    /// Calculates periapsis distance in meters.
    /// </summary>
    public double GetPeriapsisM(double semiMajorAxisM, double eccentricity) => new OrbitalProps(semiMajorAxisM: semiMajorAxisM, eccentricity: eccentricity).GetPeriapsisM();

    /// <summary>
    /// Calculates apoapsis distance in meters.
    /// </summary>
    public double GetApoapsisM(double semiMajorAxisM, double eccentricity) => new OrbitalProps(semiMajorAxisM: semiMajorAxisM, eccentricity: eccentricity).GetApoapsisM();

    /// <summary>
    /// Calculates orbital period in seconds.
    /// </summary>
    public double GetOrbitalPeriodS(double semiMajorAxisM, double parentMassKg) => new OrbitalProps(semiMajorAxisM: semiMajorAxisM).GetOrbitalPeriodS(parentMassKg);
}
