using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for ring-property helpers.
/// </summary>
[GlobalClass]
public partial class CSharpRingPropsBridge : RefCounted
{
    /// <summary>
    /// Returns ring-band width in meters.
    /// </summary>
    public double GetBandWidthM(double innerRadiusM, double outerRadiusM) => new RingBand(innerRadiusM: innerRadiusM, outerRadiusM: outerRadiusM).GetWidthM();

    /// <summary>
    /// Returns the dominant material in a ring-band composition.
    /// </summary>
    public string GetBandDominantMaterial(Dictionary composition) => new RingBand(composition: composition).GetDominantMaterial();

    /// <summary>
    /// Returns the innermost radius in a ring system.
    /// </summary>
    public double GetSystemInnerRadiusM(Array<RingBand> bands) => new RingSystemProps(bands: bands).GetInnerRadiusM();

    /// <summary>
    /// Returns the outermost radius in a ring system.
    /// </summary>
    public double GetSystemOuterRadiusM(Array<RingBand> bands) => new RingSystemProps(bands: bands).GetOuterRadiusM();

    /// <summary>
    /// Returns the total width in a ring system.
    /// </summary>
    public double GetSystemTotalWidthM(Array<RingBand> bands) => new RingSystemProps(bands: bands).GetTotalWidthM();
}
