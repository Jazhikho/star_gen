using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for hydrosphere-property helpers.
/// </summary>
[GlobalClass]
public partial class CSharpHydrospherePropsBridge : RefCounted
{
    /// <summary>
    /// Returns the fraction of surface that is liquid and not ice-covered.
    /// </summary>
    public double GetLiquidCoverage(double oceanCoverage, double iceCoverage) => new HydrosphereProps(oceanCoverage: oceanCoverage, iceCoverage: iceCoverage).GetLiquidCoverage();

    /// <summary>
    /// Returns whether the body qualifies as an ocean world.
    /// </summary>
    public bool IsOceanWorld(double oceanCoverage) => new HydrosphereProps(oceanCoverage: oceanCoverage).IsOceanWorld();

    /// <summary>
    /// Returns whether the body is mostly frozen.
    /// </summary>
    public bool IsFrozen(double iceCoverage) => new HydrosphereProps(iceCoverage: iceCoverage).IsFrozen();
}
