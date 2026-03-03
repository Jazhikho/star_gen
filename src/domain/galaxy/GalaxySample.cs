using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Container for sampled galaxy render points, split by bulge and disk populations.
/// </summary>
public partial class GalaxySample : RefCounted
{
    /// <summary>
    /// Positions of bulge-population sample points.
    /// </summary>
    public Vector3[] BulgePoints = System.Array.Empty<Vector3>();

    /// <summary>
    /// Positions of disk-population sample points.
    /// </summary>
    public Vector3[] DiskPoints = System.Array.Empty<Vector3>();

    /// <summary>
    /// Returns the total sampled point count.
    /// </summary>
    public int GetTotalCount()
    {
        return BulgePoints.Length + DiskPoints.Length;
    }
}
