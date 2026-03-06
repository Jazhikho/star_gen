using Godot;
using Godot.Collections;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Calculated layout data for the entire system viewer.
/// </summary>
public partial class SystemLayout : RefCounted
{
    /// <summary>
    /// Body layouts keyed by body identifier.
    /// </summary>
    public Dictionary<string, BodyLayout> BodyLayouts = new();

    /// <summary>
    /// Belt layouts keyed by belt identifier.
    /// </summary>
    public Dictionary<string, BeltLayout> BeltLayouts = new();

    /// <summary>
    /// Star orbit data for binary stars.
    /// </summary>
    public Dictionary<string, BodyLayout> StarOrbits = new();

    /// <summary>
    /// Node extent data keyed by node identifier.
    /// </summary>
    public Dictionary<string, NodeExtent> NodeExtents = new();

    /// <summary>
    /// Current host positions keyed by host identifier.
    /// </summary>
    public Dictionary<string, Vector3> HostPositions = new();

    /// <summary>
    /// Mapping from star body identifier to hierarchy node identifier.
    /// </summary>
    public Dictionary<string, string> StarToNode = new();

    /// <summary>
    /// Total system extent for camera framing.
    /// </summary>
    public float TotalExtent;

    /// <summary>
    /// Gets a body layout by identifier.
    /// </summary>
    public BodyLayout? GetBodyLayout(string bodyId)
    {
        if (BodyLayouts.ContainsKey(bodyId))
        {
            return BodyLayouts[bodyId];
        }

        return null;
    }

    /// <summary>
    /// Gets a belt layout by identifier.
    /// </summary>
    public BeltLayout? GetBeltLayout(string beltId)
    {
        if (BeltLayouts.ContainsKey(beltId))
        {
            return BeltLayouts[beltId];
        }

        return null;
    }

    /// <summary>
    /// Gets star orbit data by star identifier.
    /// </summary>
    public BodyLayout? GetStarOrbit(string starId)
    {
        if (StarOrbits.ContainsKey(starId))
        {
            return StarOrbits[starId];
        }

        return null;
    }

    /// <summary>
    /// Gets node extent data by node identifier.
    /// </summary>
    public NodeExtent? GetNodeExtent(string nodeId)
    {
        if (NodeExtents.ContainsKey(nodeId))
        {
            return NodeExtents[nodeId];
        }

        return null;
    }

    /// <summary>
    /// Gets the current position of an orbit host.
    /// </summary>
    public Vector3 GetHostPosition(string hostId)
    {
        if (HostPositions.ContainsKey(hostId))
        {
            return HostPositions[hostId];
        }

        return Vector3.Zero;
    }

    /// <summary>
    /// Returns all orbiting body layouts.
    /// </summary>
    public Array<BodyLayout> GetAllOrbitingBodies()
    {
        Array<BodyLayout> result = new();
        foreach (BodyLayout layout in BodyLayouts.Values)
        {
            if (layout.IsOrbiting)
            {
                result.Add(layout);
            }
        }

        foreach (BodyLayout layout in StarOrbits.Values)
        {
            if (layout.IsOrbiting)
            {
                result.Add(layout);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all belt layouts.
    /// </summary>
    public Array<BeltLayout> GetAllBelts()
    {
        Array<BeltLayout> result = new();
        foreach (BeltLayout layout in BeltLayouts.Values)
        {
            result.Add(layout);
        }

        return result;
    }
}
