using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Pure-logic navigation across occupied 3D grid coordinates.
/// </summary>
public partial class GridCursor : RefCounted
{
    /// <summary>
    /// Current cursor position.
    /// </summary>
    public Vector3I Position = Vector3I.Zero;

    /// <summary>
    /// Finds the nearest occupied coordinate in the given cardinal direction.
    /// </summary>
    public static Vector3I? FindNearestInDirection(
        Vector3I current,
        Vector3I direction,
        IReadOnlyList<Vector3I> occupied)
    {
        Vector3I? best = null;
        float bestDistanceSquared = float.PositiveInfinity;

        for (int index = 0; index < occupied.Count; index += 1)
        {
            Vector3I coords = occupied[index];
            if (coords == current)
            {
                continue;
            }

            Vector3I delta = coords - current;
            if (!IsInDirection(delta, direction))
            {
                continue;
            }

            float distanceSquared = GetDistanceSquared(delta);
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                best = coords;
            }
        }

        return best;
    }

    /// <summary>
    /// Finds the nearest occupied coordinate regardless of direction.
    /// </summary>
    public static Vector3I? FindNearest(Vector3I target, IReadOnlyList<Vector3I> occupied)
    {
        Vector3I? best = null;
        float bestDistanceSquared = float.PositiveInfinity;

        for (int index = 0; index < occupied.Count; index += 1)
        {
            Vector3I delta = occupied[index] - target;
            float distanceSquared = GetDistanceSquared(delta);
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                best = occupied[index];
            }
        }

        return best;
    }

    /// <summary>
    /// Moves the cursor to the nearest occupied coordinate in the given direction.
    /// </summary>
    public Vector3I? MoveInDirection(Vector3I direction, IReadOnlyList<Vector3I> occupied)
    {
        Vector3I? target = FindNearestInDirection(Position, direction, occupied);
        if (target.HasValue)
        {
            Position = target.Value;
        }

        return target;
    }

    /// <summary>
    /// Snaps the cursor to the nearest occupied coordinate.
    /// </summary>
    public Vector3I? SnapToNearest(IReadOnlyList<Vector3I> occupied)
    {
        Vector3I? target = FindNearest(Position, occupied);
        if (target.HasValue)
        {
            Position = target.Value;
        }

        return target;
    }

    /// <summary>
    /// Returns whether a displacement lies in the given cardinal direction.
    /// </summary>
    private static bool IsInDirection(Vector3I delta, Vector3I direction)
    {
        if (direction.X != 0 && System.Math.Sign(delta.X) != System.Math.Sign(direction.X))
        {
            return false;
        }

        if (direction.Y != 0 && System.Math.Sign(delta.Y) != System.Math.Sign(direction.Y))
        {
            return false;
        }

        if (direction.Z != 0 && System.Math.Sign(delta.Z) != System.Math.Sign(direction.Z))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the squared Euclidean distance for a displacement.
    /// </summary>
    private static float GetDistanceSquared(Vector3I delta)
    {
        return (delta.X * delta.X) + (delta.Y * delta.Y) + (delta.Z * delta.Z);
    }
}
