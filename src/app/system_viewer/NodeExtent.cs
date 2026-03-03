using Godot;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Intermediate extent data for a hierarchy node.
/// </summary>
public partial class NodeExtent : RefCounted
{
    /// <summary>
    /// Hierarchy node identifier.
    /// </summary>
    public string NodeId = string.Empty;

    /// <summary>
    /// Radius from the node center to its outermost content.
    /// </summary>
    public float ExtentRadius;

    /// <summary>
    /// Inner extent before circum-binary content.
    /// </summary>
    public float InnerExtentRadius;

    /// <summary>
    /// Current node center in display space.
    /// </summary>
    public Vector3 CenterPosition = Vector3.Zero;

    /// <summary>
    /// Number of S-type orbit contents.
    /// </summary>
    public int STypePlanetCount;

    /// <summary>
    /// Number of P-type orbit contents.
    /// </summary>
    public int PTypePlanetCount;

    /// <summary>
    /// First orbit radius around this host.
    /// </summary>
    public float FirstOrbitRadius;

    /// <summary>
    /// Display radius for star nodes.
    /// </summary>
    public float StarDisplayRadius;

    /// <summary>
    /// Binary separation for barycenter nodes.
    /// </summary>
    public float BinarySeparation;

    /// <summary>
    /// Largest orbiting planet display radius.
    /// </summary>
    public float MaxPlanetRadius = SystemDisplayLayout.PlanetMaxRadius;

    /// <summary>
    /// Star body identifier for star nodes.
    /// </summary>
    public string StarBodyId = string.Empty;

    /// <summary>
    /// Maximum swept radius while orbiting a parent.
    /// </summary>
    public float MaxSweepRadius;

    /// <summary>
    /// Distance from this node to its parent barycenter.
    /// </summary>
    public float OrbitRadiusAroundParent;

    /// <summary>
    /// Creates a new node-extent entry.
    /// </summary>
    public NodeExtent(string nodeId = "")
    {
        NodeId = nodeId;
    }
}
