using Godot;
using Godot.Collections;
using StarGen.Domain.Jumplanes;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Deterministic colonization-simulation request for an explicit region.
/// </summary>
public partial class ColonizationSimulationRequest : RefCounted
{
    /// <summary>
    /// Region identifier chosen by the caller.
    /// </summary>
    public string RegionId { get; set; } = string.Empty;

    /// <summary>
    /// Region scope.
    /// </summary>
    public JumpLaneRegion.RegionScope Scope { get; set; } = JumpLaneRegion.RegionScope.Subsector;

    /// <summary>
    /// Ordered jump-lane systems that define the simulation domain.
    /// </summary>
    public Array<JumpLaneSystem> Systems { get; set; } = new();

    /// <summary>
    /// Explicit simulation settings.
    /// </summary>
    public ColonizationSimulationSettings Settings { get; set; } = ColonizationSimulationSettings.CreateDefault();
}
