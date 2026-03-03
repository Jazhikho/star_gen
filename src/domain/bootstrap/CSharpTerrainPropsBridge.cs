using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for terrain-property helpers.
/// </summary>
[GlobalClass]
public partial class CSharpTerrainPropsBridge : RefCounted
{
    /// <summary>
    /// Returns whether the surface is geologically active.
    /// </summary>
    public bool IsGeologicallyActive(double tectonicActivity) => new TerrainProps(tectonicActivity: tectonicActivity).IsGeologicallyActive();

    /// <summary>
    /// Returns whether the surface is heavily cratered.
    /// </summary>
    public bool IsHeavilyCratered(double craterDensity) => new TerrainProps(craterDensity: craterDensity).IsHeavilyCratered();
}
