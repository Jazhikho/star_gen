using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for cryosphere-property helpers.
/// </summary>
[GlobalClass]
public partial class CSharpCryospherePropsBridge : RefCounted
{
    /// <summary>
    /// Returns whether the body has significant ice features.
    /// </summary>
    public bool HasSignificantIce(double polarCapCoverage, double permafrostDepthM) => new CryosphereProps(polarCapCoverage: polarCapCoverage, permafrostDepthM: permafrostDepthM).HasSignificantIce();

    /// <summary>
    /// Returns whether cryovolcanism is active.
    /// </summary>
    public bool IsCryovolcanicallyActive(double cryovolcanismLevel) => new CryosphereProps(cryovolcanismLevel: cryovolcanismLevel).IsCryovolcanicallyActive();
}
