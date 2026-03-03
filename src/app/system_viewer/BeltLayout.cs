using Godot;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Calculated layout data for an asteroid belt.
/// </summary>
public partial class BeltLayout : RefCounted
{
    /// <summary>
    /// Belt identifier.
    /// </summary>
    public string BeltId = string.Empty;

    /// <summary>
    /// Orbit host identifier.
    /// </summary>
    public string HostId = string.Empty;

    /// <summary>
    /// Host center in display space.
    /// </summary>
    public Vector3 HostCenter = Vector3.Zero;

    /// <summary>
    /// Center display radius.
    /// </summary>
    public float CenterDisplayRadius;

    /// <summary>
    /// Inner display radius.
    /// </summary>
    public float InnerDisplayRadius;

    /// <summary>
    /// Outer display radius.
    /// </summary>
    public float OuterDisplayRadius;

    /// <summary>
    /// Center in AU.
    /// </summary>
    public float CenterAu;

    /// <summary>
    /// Inner edge in AU.
    /// </summary>
    public float InnerAu;

    /// <summary>
    /// Outer edge in AU.
    /// </summary>
    public float OuterAu;

    /// <summary>
    /// Maximum rendered inclination.
    /// </summary>
    public float MaxInclinationDeg = 6.0f;

    /// <summary>
    /// Creates a new belt-layout entry.
    /// </summary>
    public BeltLayout(string beltId = "")
    {
        BeltId = beltId;
    }
}
