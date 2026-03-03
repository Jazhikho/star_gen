using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Result of a ray-based star pick.
/// </summary>
public partial class StarPickResult : RefCounted
{
    /// <summary>
    /// Index of the picked star in the source arrays.
    /// </summary>
    public int StarIndex = -1;

    /// <summary>
    /// World-space position of the picked star.
    /// </summary>
    public Vector3 WorldPosition = Vector3.Zero;

    /// <summary>
    /// Deterministic star seed of the picked star.
    /// </summary>
    public long StarSeed;

    /// <summary>
    /// Distance along the ray to the closest point.
    /// </summary>
    public float RayDistance;

    /// <summary>
    /// Perpendicular distance from the star to the ray.
    /// </summary>
    public float LateralDistance;
}
