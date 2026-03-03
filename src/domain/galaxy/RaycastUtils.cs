using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Pure-math ray intersection utilities.
/// </summary>
public static class RaycastUtils
{
    /// <summary>
    /// Sentinel value returned when no hit occurs.
    /// </summary>
    public const float NoHit = -1.0f;

    /// <summary>
    /// Tolerance for treating a ray component as parallel to a slab.
    /// </summary>
    public const float ParallelEpsilon = 1.0e-8f;

    /// <summary>
    /// Tests whether a ray intersects an axis-aligned bounding box using the slab method.
    /// </summary>
    public static float RayIntersectsAabb(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Vector3 aabbMin,
        Vector3 aabbMax)
    {
        float tNear = -1.0e20f;
        float tFar = 1.0e20f;

        for (int axis = 0; axis < 3; axis += 1)
        {
            if (Mathf.Abs(rayDirection[axis]) < ParallelEpsilon)
            {
                if (rayOrigin[axis] < aabbMin[axis] || rayOrigin[axis] > aabbMax[axis])
                {
                    return NoHit;
                }
            }
            else
            {
                float inverseDirection = 1.0f / rayDirection[axis];
                float t1 = (aabbMin[axis] - rayOrigin[axis]) * inverseDirection;
                float t2 = (aabbMax[axis] - rayOrigin[axis]) * inverseDirection;

                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                }

                tNear = Mathf.Max(tNear, t1);
                tFar = Mathf.Min(tFar, t2);
                if (tNear > tFar)
                {
                    return NoHit;
                }
            }
        }

        if (tFar < 0.0f)
        {
            return NoHit;
        }

        return Mathf.Max(tNear, 0.0f);
    }
}
