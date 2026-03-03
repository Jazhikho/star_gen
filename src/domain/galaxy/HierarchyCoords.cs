using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Full hierarchy coordinates for a galaxy position.
/// </summary>
public partial class HierarchyCoords : RefCounted
{
    /// <summary>
    /// Quadrant grid coordinates.
    /// </summary>
    public Vector3I QuadrantCoords = Vector3I.Zero;

    /// <summary>
    /// Sector-local coordinates within the quadrant.
    /// </summary>
    public Vector3I SectorLocalCoords = Vector3I.Zero;

    /// <summary>
    /// Subsector-local coordinates within the sector.
    /// </summary>
    public Vector3I SubsectorLocalCoords = Vector3I.Zero;
}
