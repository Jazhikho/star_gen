using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Container for generated star positions and derived star seeds.
/// </summary>
public partial class SectorStarData : RefCounted
{
    /// <summary>
    /// World-space star positions.
    /// </summary>
    public Vector3[] Positions = System.Array.Empty<Vector3>();

    /// <summary>
    /// Per-star deterministic seeds aligned with <see cref="Positions"/>.
    /// </summary>
    public long[] StarSeeds = System.Array.Empty<long>();

    /// <summary>
    /// Returns the number of generated stars.
    /// </summary>
    public int GetCount()
    {
        return Positions.Length;
    }

    /// <summary>
    /// Appends another result payload into this one.
    /// </summary>
    public void Merge(SectorStarData other)
    {
        List<Vector3> mergedPositions = new(Positions);
        mergedPositions.AddRange(other.Positions);
        Positions = mergedPositions.ToArray();

        List<long> mergedSeeds = new(StarSeeds);
        mergedSeeds.AddRange(other.StarSeeds);
        StarSeeds = mergedSeeds.ToArray();
    }
}
