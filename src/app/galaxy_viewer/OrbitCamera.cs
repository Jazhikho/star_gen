using Godot;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Orbit camera for the galaxy viewer.
/// </summary>
public partial class OrbitCamera : Camera3D
{
    private const float RotateSpeed = 0.3f;
    private const float ZoomFactor = 0.1f;

    [Export]
    public float MinDistance { get; set; } = 500.0f;

    [Export]
    public float MaxDistance { get; set; } = 120000.0f;

    private float _yawDeg;
    private float _pitchDeg = -35.0f;
    private float _distance = 40000.0f;
    private Vector3 _target = Vector3.Zero;
    private bool _rotating;

    /// <summary>
    /// Initializes the camera transform.
    /// </summary>
    public override void _Ready()
    {
        UpdateTransform();
    }

    /// <summary>
    /// Returns the current yaw angle.
    /// </summary>
    public float GetYawDeg()
    {
        return _yawDeg;
    }

    /// <summary>
    /// Returns the current pitch angle.
    /// </summary>
    public float GetPitchDeg()
    {
        return _pitchDeg;
    }

    /// <summary>
    /// Returns the current orbit target.
    /// </summary>
    public Vector3 GetTarget()
    {
        return _target;
    }

    /// <summary>
    /// Returns the current orbit distance.
    /// </summary>
    public float GetDistance()
    {
        return _distance;
    }

    /// <summary>
    /// Sets the orbit target and distance.
    /// </summary>
    public void Configure(Vector3 target, float distance)
    {
        _target = target;
        _distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        UpdateTransform();
    }

    /// <summary>
    /// Reconfigures camera constraints for a different zoom level.
    /// </summary>
    public void ReconfigureConstraints(float newMin, float newMax, Vector3 newTarget)
    {
        MinDistance = newMin;
        MaxDistance = newMax;
        _target = newTarget;
        _distance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
        UpdateTransform();
    }

    /// <summary>
    /// Handles camera input.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseButton(mouseButton);
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion && _rotating)
        {
            HandleMouseMotion(mouseMotion);
        }
    }

    /// <summary>
    /// Handles mouse button events for rotation and zoom.
    /// </summary>
    private void HandleMouseButton(InputEventMouseButton mouseEvent)
    {
        switch (mouseEvent.ButtonIndex)
        {
            case MouseButton.Right:
                _rotating = mouseEvent.Pressed;
                break;
            case MouseButton.WheelUp:
                if (mouseEvent.Pressed)
                {
                    _distance *= 1.0f - ZoomFactor;
                    _distance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
                    UpdateTransform();
                }

                break;
            case MouseButton.WheelDown:
                if (mouseEvent.Pressed)
                {
                    _distance *= 1.0f + ZoomFactor;
                    _distance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
                    UpdateTransform();
                }

                break;
        }
    }

    /// <summary>
    /// Handles mouse motion for camera rotation.
    /// </summary>
    private void HandleMouseMotion(InputEventMouseMotion mouseEvent)
    {
        _yawDeg -= mouseEvent.Relative.X * RotateSpeed;
        _pitchDeg -= mouseEvent.Relative.Y * RotateSpeed;
        _pitchDeg = Mathf.Clamp(_pitchDeg, -89.0f, 89.0f);
        UpdateTransform();
    }

    /// <summary>
    /// Recomputes position and look-at from yaw, pitch, and distance.
    /// </summary>
    private void UpdateTransform()
    {
        if (!IsInsideTree())
        {
            return;
        }

        float yawRad = Mathf.DegToRad(_yawDeg);
        float pitchRad = Mathf.DegToRad(_pitchDeg);
        Vector3 offset = new(
            _distance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
            _distance * Mathf.Sin(pitchRad),
            _distance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad));

        GlobalPosition = _target + offset;
        LookAt(_target, Vector3.Up);
    }
}
