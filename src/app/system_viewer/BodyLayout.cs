using Godot;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Calculated layout data for a rendered body.
/// </summary>
public partial class BodyLayout : RefCounted
{
    /// <summary>
    /// Body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Current display position.
    /// </summary>
    public Vector3 Position = Vector3.Zero;

    /// <summary>
    /// Display radius in Godot units.
    /// </summary>
    public float DisplayRadius = 1.0f;

    /// <summary>
    /// Orbit radius from the parent center.
    /// </summary>
    public float OrbitRadius;

    /// <summary>
    /// Parent orbit center.
    /// </summary>
    public Vector3 OrbitCenter = Vector3.Zero;

    /// <summary>
    /// Parent body or host identifier.
    /// </summary>
    public string OrbitParentId = string.Empty;

    /// <summary>
    /// Current orbital angle in radians.
    /// </summary>
    public float OrbitalAngle;

    /// <summary>
    /// Orbital period in seconds.
    /// </summary>
    public float OrbitalPeriod = SystemDisplayLayout.BaseOrbitalPeriod;

    /// <summary>
    /// Whether this body is animated in orbit.
    /// </summary>
    public bool IsOrbiting;

    /// <summary>
    /// Creates a new body-layout entry.
    /// </summary>
    public BodyLayout(string bodyId = "")
    {
        BodyId = bodyId;
    }

    /// <summary>
    /// Updates the display position from the current orbital angle.
    /// </summary>
    public void UpdatePositionFromAngle()
    {
        if (!IsOrbiting || OrbitRadius <= 0.0f)
        {
            return;
        }

        Position = OrbitCenter + new Vector3(
            Mathf.Cos(OrbitalAngle) * OrbitRadius,
            0.0f,
            Mathf.Sin(OrbitalAngle) * OrbitRadius);
    }
}
