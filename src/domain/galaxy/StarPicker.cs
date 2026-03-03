using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Pure-math star picking by minimum distance to a ray.
/// </summary>
public static class StarPicker
{
    /// <summary>
    /// Returns the star closest to a ray within the supplied lateral distance threshold.
    /// </summary>
    public static StarPickResult? PickNearestToRay(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        IReadOnlyList<Vector3> positions,
        IReadOnlyList<long> starSeeds,
        float maxLateralDistance)
    {
        int bestIndex = -1;
        float bestLateral = float.PositiveInfinity;
        float bestRayDistance = 0.0f;

        for (int index = 0; index < positions.Count; index += 1)
        {
            Vector3 toStar = positions[index] - rayOrigin;
            float rayDistance = toStar.Dot(rayDirection);
            if (rayDistance < 0.0f)
            {
                continue;
            }

            Vector3 closestPoint = rayOrigin + (rayDirection * rayDistance);
            float lateralDistance = closestPoint.DistanceTo(positions[index]);
            if (lateralDistance < maxLateralDistance && lateralDistance < bestLateral)
            {
                bestIndex = index;
                bestLateral = lateralDistance;
                bestRayDistance = rayDistance;
            }
        }

        if (bestIndex < 0)
        {
            return null;
        }

        return new StarPickResult
        {
            StarIndex = bestIndex,
            WorldPosition = positions[bestIndex],
            StarSeed = starSeeds[bestIndex],
            RayDistance = bestRayDistance,
            LateralDistance = bestLateral,
        };
    }
}
