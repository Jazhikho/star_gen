using Godot;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Top-down camera controller used by the system viewer.
/// </summary>
public partial class SystemCameraController : Camera3D
{
    /// <summary>
    /// Emitted whenever the camera position changes.
    /// </summary>
    [Signal]
    public delegate void CameraMovedEventHandler(Vector3 position, float height);

    /// <summary>
    /// Zoom speed applied to scroll-wheel input.
    /// </summary>
    [Export]
    public float ZoomSpeed = 0.15f;

    /// <summary>
    /// Pan speed multiplier.
    /// </summary>
    [Export]
    public float PanSpeed = 1.0f;

    /// <summary>
    /// Orbit speed multiplier.
    /// </summary>
    [Export]
    public float OrbitSpeed = 2.0f;

    /// <summary>
    /// Zoom smoothing factor.
    /// </summary>
    [Export]
    public float ZoomSmooth = 5.0f;

    /// <summary>
    /// Movement smoothing factor.
    /// </summary>
    [Export]
    public float MoveSmooth = 8.0f;

    /// <summary>
    /// Minimum camera height.
    /// </summary>
    [Export]
    public float MinHeight = 0.5f;

    /// <summary>
    /// Maximum camera height.
    /// </summary>
    [Export]
    public float MaxHeight = 1000.0f;

    /// <summary>
    /// Minimum pitch angle in degrees.
    /// </summary>
    [Export]
    public float MinPitchDeg = 10.0f;

    /// <summary>
    /// Maximum pitch angle in degrees.
    /// </summary>
    [Export]
    public float MaxPitchDeg = 89.0f;

    private float _height = 20.0f;
    private float _targetHeight = 20.0f;
    private Vector3 _targetPosition = Vector3.Zero;
    private Vector3 _smoothTarget = Vector3.Zero;
    private Vector2 _framingOffset = Vector2.Zero;
    private float _yaw;
    private float _pitch = Mathf.DegToRad(60.0f);
    private float _targetPitch = Mathf.DegToRad(60.0f);
    private bool _orbiting;
    private bool _panning;
    private Vector2 _lastMousePosition = Vector2.Zero;

    /// <summary>
    /// Initializes the camera transform.
    /// </summary>
    public override void _Ready()
    {
        UpdateTransform();
    }

    /// <summary>
    /// Handles camera input.
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (IsMouseOverUi())
        {
            if (@event is InputEventMouseMotion)
            {
                _orbiting = false;
                _panning = false;
                return;
            }

            if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
            {
                _orbiting = false;
                _panning = false;
                return;
            }
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    _orbiting = mouseEvent.Pressed;
                    _lastMousePosition = mouseEvent.Position;
                    break;
                case MouseButton.Right:
                    _panning = mouseEvent.Pressed;
                    _lastMousePosition = mouseEvent.Position;
                    break;
                case MouseButton.Middle:
                    _orbiting = mouseEvent.Pressed;
                    _lastMousePosition = mouseEvent.Position;
                    break;
                case MouseButton.WheelUp:
                    if (!IsMouseOverUi())
                    {
                        _targetHeight *= 1.0f - ZoomSpeed;
                        _targetHeight = Mathf.Clamp(_targetHeight, MinHeight, MaxHeight);
                    }

                    break;
                case MouseButton.WheelDown:
                    if (!IsMouseOverUi())
                    {
                        _targetHeight *= 1.0f + ZoomSpeed;
                        _targetHeight = Mathf.Clamp(_targetHeight, MinHeight, MaxHeight);
                    }

                    break;
            }

            return;
        }

        if (@event is InputEventMouseMotion motionEvent)
        {
            if (IsMouseOverUi())
            {
                _orbiting = false;
                _panning = false;
                return;
            }

            Vector2 delta = motionEvent.Position - _lastMousePosition;
            if (_orbiting)
            {
                _yaw -= delta.X * OrbitSpeed * 0.005f;
                _targetPitch += delta.Y * OrbitSpeed * 0.005f;
                _targetPitch = Mathf.Clamp(_targetPitch, Mathf.DegToRad(MinPitchDeg), Mathf.DegToRad(MaxPitchDeg));
            }
            else if (_panning)
            {
                float panScale = _height * PanSpeed * 0.002f;
                float panX = -delta.X * panScale;
                float panZ = -delta.Y * panScale;

                _targetPosition.X += (panX * Mathf.Cos(_yaw)) + (panZ * Mathf.Sin(_yaw));
                _targetPosition.Z += (-panX * Mathf.Sin(_yaw)) + (panZ * Mathf.Cos(_yaw));
            }

            _lastMousePosition = motionEvent.Position;
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.F:
                    FocusOnOrigin();
                    break;
                case Key.T:
                    ToggleViewAngle();
                    break;
            }
        }
    }

    /// <summary>
    /// Smoothly updates camera motion.
    /// </summary>
    public override void _Process(double delta)
    {
        float step = (float)delta;
        _height = Mathf.Lerp(_height, _targetHeight, ZoomSmooth * step);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, MoveSmooth * step);
        _smoothTarget = _smoothTarget.Lerp(_targetPosition, MoveSmooth * step);

        UpdateTransform();
        Near = Mathf.Max(0.001f, _height * 0.001f);
        Far = Mathf.Max(100.0f, _height * 50.0f);
        EmitSignal(SignalName.CameraMoved, GetCameraPosition(), _height);
    }

    /// <summary>
    /// Focuses the camera on the system origin.
    /// </summary>
    public void FocusOnOrigin()
    {
        ApplyViewState(Vector3.Zero, 20.0f, Mathf.DegToRad(60.0f), 0.0f);
    }

    /// <summary>
    /// Focuses the camera on a specific position.
    /// </summary>
    public void FocusOnPosition(Vector3 target, float zoomToDistance = -1.0f)
    {
        _targetPosition = new Vector3(target.X, 0.0f, target.Z);
        if (zoomToDistance > 0.0f)
        {
            _targetHeight = Mathf.Clamp(zoomToDistance * 1.5f, MinHeight, MaxHeight);
        }
    }

    /// <summary>
    /// Sets the camera height immediately.
    /// </summary>
    public void SetHeight(float height)
    {
        _height = Mathf.Clamp(height, MinHeight, MaxHeight);
        _targetHeight = _height;
    }

    /// <summary>
    /// Returns the current camera height.
    /// </summary>
    public float GetHeightValue()
    {
        return _height;
    }

    /// <summary>
    /// Applies a normalized screen-center offset so framing respects the visible render pane.
    /// </summary>
    public void SetFramingOffset(Vector2 framingOffset)
    {
        _framingOffset = framingOffset;
    }

    /// <summary>
    /// Applies a full target state and snaps the smooth target to it.
    /// </summary>
    public void ApplyViewState(Vector3 targetPosition, float height, float pitchRadians, float yaw)
    {
        _targetPosition = new Vector3(targetPosition.X, 0.0f, targetPosition.Z);
        _smoothTarget = _targetPosition;
        _targetHeight = Mathf.Clamp(height, MinHeight, MaxHeight);
        _height = _targetHeight;
        _targetPitch = Mathf.Clamp(pitchRadians, Mathf.DegToRad(MinPitchDeg), Mathf.DegToRad(MaxPitchDeg));
        _pitch = _targetPitch;
        _yaw = yaw;
        UpdateTransform();
    }

    /// <summary>
    /// Updates the camera transform from the current target state.
    /// </summary>
    private void UpdateTransform()
    {
        float horizontalDistance = _height / Mathf.Tan(_pitch);
        Vector3 cameraOffset = new(
            horizontalDistance * Mathf.Sin(_yaw),
            _height,
            horizontalDistance * Mathf.Cos(_yaw));

        Vector3 cameraPosition = _smoothTarget + cameraOffset;
        Vector3 focusPoint = ComputeFocusPoint(cameraPosition);
        if (!IsInsideTree())
        {
            Position = cameraPosition;
            return;
        }

        GlobalPosition = cameraPosition;
        LookAt(focusPoint, Vector3.Up);
    }

    /// <summary>
    /// Toggles between the shallow and steep default pitch angles.
    /// </summary>
    private void ToggleViewAngle()
    {
        if (_targetPitch > Mathf.DegToRad(70.0f))
        {
            _targetPitch = Mathf.DegToRad(30.0f);
        }
        else
        {
            _targetPitch = Mathf.DegToRad(80.0f);
        }
    }

    /// <summary>
    /// Returns true when the pointer is hovering UI.
    /// </summary>
    private bool IsMouseOverUi()
    {
        Viewport? viewport = GetViewport();
        if (viewport == null)
        {
            return false;
        }

        return viewport.GuiGetHoveredControl() != null;
    }

    /// <summary>
    /// Returns the current camera position without requiring scene-tree membership.
    /// </summary>
    private Vector3 GetCameraPosition()
    {
        if (IsInsideTree())
        {
            return GlobalPosition;
        }

        return Position;
    }

    /// <summary>
    /// Shifts the look-at point so camera focus aligns to the visible render area.
    /// </summary>
    private Vector3 ComputeFocusPoint(Vector3 cameraPosition)
    {
        if (_framingOffset == Vector2.Zero)
        {
            return _smoothTarget;
        }

        Viewport? viewport = GetViewport();
        Vector2 viewportSize = new Vector2(1280.0f, 720.0f);
        if (viewport != null)
        {
            viewportSize = viewport.GetVisibleRect().Size;
        }

        float aspect = viewportSize.X / Mathf.Max(1.0f, viewportSize.Y);
        float distance = cameraPosition.DistanceTo(_smoothTarget);
        if (distance <= 0.001f)
        {
            return _smoothTarget;
        }

        Vector3 forward = (_smoothTarget - cameraPosition).Normalized();
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
        return _smoothTarget - (right * shiftX) + (up * shiftY);
    }

    /// <summary>
    /// GDScript-compatible min-height accessor.
    /// </summary>
    public float min_height => MinHeight;

    /// <summary>
    /// GDScript-compatible max-height accessor.
    /// </summary>
    public float max_height => MaxHeight;

    /// <summary>
    /// GDScript-compatible height setter wrapper.
    /// </summary>
    public void set_height(float height)
    {
        SetHeight(height);
    }

    /// <summary>
    /// GDScript-compatible height getter wrapper.
    /// </summary>
    public float get_height()
    {
        return GetHeightValue();
    }

    /// <summary>
    /// GDScript-compatible origin focus wrapper.
    /// </summary>
    public void focus_on_origin()
    {
        FocusOnOrigin();
    }

    /// <summary>
    /// GDScript-compatible position focus wrapper.
    /// </summary>
    public void focus_on_position(Vector3 target, float zoomToDistance = -1.0f)
    {
        FocusOnPosition(target, zoomToDistance);
    }
}
