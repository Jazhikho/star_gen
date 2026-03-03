using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Combined star and subsector data for a subsector neighborhood.
/// </summary>
public partial class SubSectorNeighborhoodData : RefCounted
{
    /// <summary>
    /// Combined star positions across the neighborhood.
    /// </summary>
    public Vector3[] StarPositions = System.Array.Empty<Vector3>();

    /// <summary>
    /// Combined star seeds across the neighborhood.
    /// </summary>
    public long[] StarSeeds = System.Array.Empty<long>();

    /// <summary>
    /// Per-star Chebyshev shell distance.
    /// </summary>
    public int[] StarShells = System.Array.Empty<int>();

    /// <summary>
    /// World-space origins of all subsectors in the neighborhood.
    /// </summary>
    public Vector3[] SubsectorOrigins = System.Array.Empty<Vector3>();

    /// <summary>
    /// Per-subsector shell distance.
    /// </summary>
    public int[] SubsectorShells = System.Array.Empty<int>();

    /// <summary>
    /// World-space origin of the center subsector.
    /// </summary>
    public Vector3 CenterOrigin = Vector3.Zero;

    /// <summary>
    /// Returns the total star count.
    /// </summary>
    public int GetStarCount()
    {
        return StarPositions.Length;
    }
}
