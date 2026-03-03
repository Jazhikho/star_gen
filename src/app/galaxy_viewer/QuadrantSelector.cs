using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Picks the nearest occupied quadrant along a camera ray.
/// </summary>
public partial class QuadrantSelector : RefCounted
{
    /// <summary>
    /// The currently selected coordinates, or null when no selection exists.
    /// </summary>
    public Variant SelectedCoords = default;

    /// <summary>
    /// Returns the nearest occupied quadrant intersected by the supplied ray.
    /// </summary>
    public Variant PickFromRay(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Array<Vector3I> occupiedCoords)
    {
        Variant bestCoords = default;
        float bestDistance = float.PositiveInfinity;
        float quadrantSize = (float)GalaxyCoordinates.QuadrantSizePc;

        foreach (Vector3I coords in occupiedCoords)
        {
            Vector3 aabbMin = new(
                coords.X * quadrantSize,
                coords.Y * quadrantSize,
                coords.Z * quadrantSize);
            Vector3 aabbMax = aabbMin + new Vector3(quadrantSize, quadrantSize, quadrantSize);

            float hitDistance = RaycastUtils.RayIntersectsAabb(rayOrigin, rayDirection, aabbMin, aabbMax);
            if (hitDistance >= 0.0f && hitDistance < bestDistance)
            {
                bestDistance = hitDistance;
                bestCoords = Variant.CreateFrom(coords);
            }
        }

        return bestCoords;
    }

    /// <summary>
    /// Stores the current selection.
    /// </summary>
    public void SetSelection(Variant coords)
    {
        SelectedCoords = coords;
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        SelectedCoords = default;
    }

    /// <summary>
    /// Returns whether a selection currently exists.
    /// </summary>
    public bool HasSelection()
    {
        return !SelectedCoords.VariantType.Equals(Variant.Type.Nil);
    }
}
