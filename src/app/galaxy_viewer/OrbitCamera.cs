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
    private Vector2 _framingOffset = Vector2.Zero;
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
    /// Applies a normalized screen-center offset so the orbit target stays centered in the visible pane.
    /// </summary>
    public void SetFramingOffset(Vector2 framingOffset)
    {
        _framingOffset = framingOffset;
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
        LookAt(ComputeFocusPoint(GlobalPosition), Vector3.Up);
    }

    /// <summary>
    /// Shifts the orbit camera look-at point to the center of the visible render area.
    /// </summary>
    private Vector3 ComputeFocusPoint(Vector3 cameraPosition)
    {
        if (_framingOffset == Vector2.Zero)
        {
            return _target;
        }

        Viewport? viewport = GetViewport();
        Vector2 viewportSize = new Vector2(1280.0f, 720.0f);
        if (viewport != null)
        {
            viewportSize = viewport.GetVisibleRect().Size;
        }

        float aspect = viewportSize.X / Mathf.Max(1.0f, viewportSize.Y);
        float distance = cameraPosition.DistanceTo(_target);
        if (distance <= 0.001f)
        {
            return _target;
        }

        Vector3 forward = (_target - cameraPosition).Normalized();
        Vector3 right = forward.Cross(Vector3.Up);
        if (right.LengthSquared() <= 0.000001f)
        {
            right = Vector3.Right;
        }
        else
        {
            right = right.Normalized();
        }

        Vector3 up = right.Cross(forward).Normalized();
        float halfHeight = Mathf.Tan(Mathf.DegToRad(Fov) * 0.5f) * distance;
        float halfWidth = halfHeight * aspect;
        float shiftX = _framingOffset.X * halfWidth;
        float shiftY = _framingOffset.Y * halfHeight;
        return _target - (right * shiftX) + (up * shiftY);
    }
}
