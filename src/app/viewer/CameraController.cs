using Godot;

namespace StarGen.App.Viewer;

/// <summary>
/// Orbital camera controller for the object viewer.
/// </summary>
public partial class CameraController : Camera3D
{
    [Export]
    public float OrbitSpeed { get; set; } = 2.0f;

    [Export]
    public float PanSpeed { get; set; } = 1.0f;

    [Export]
    public float ZoomSpeed { get; set; } = 0.1f;

    [Export]
    public float ZoomSmooth { get; set; } = 5.0f;

    [Export]
    public float MinDistance { get; set; } = 0.5f;

    [Export]
    public float MaxDistance { get; set; } = 100.0f;

    [Export]
    public float MinPitch { get; set; } = -89.0f;

    [Export]
    public float MaxPitch { get; set; } = 89.0f;

    private float _distance = 10.0f;
    private float _targetDistance = 10.0f;
    private Vector2 _rotation = Vector2.Zero;
    private Vector3 _targetPosition = Vector3.Zero;
    private Vector2 _framingOffset = Vector2.Zero;
    private bool _orbiting;
    private bool _panning;
    private Vector2 _lastMousePosition = Vector2.Zero;

    /// <summary>
    /// Initializes the camera transform.
    /// </summary>
    public override void _Ready()
    {
        UpdateCameraTransform();
    }

    /// <summary>
    /// Handles mouse and keyboard input.
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

            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                _orbiting = false;
                _panning = false;
                return;
            }
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            HandleMouseButton(mouseEvent);
            return;
        }

        if (@event is InputEventMouseMotion motionEvent)
        {
            HandleMouseMotion(motionEvent);
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F)
        {
            FocusOnTarget();
        }
    }

    /// <summary>
    /// Advances smooth zoom and refreshes the transform.
    /// </summary>
    public override void _Process(double delta)
    {
        _distance = Mathf.Lerp(_distance, _targetDistance, ZoomSmooth * (float)delta);
        UpdateCameraTransform();
    }

    /// <summary>
    /// Focuses the camera on the default target.
    /// </summary>
    public void FocusOnTarget()
    {
        _targetPosition = Vector3.Zero;
        _targetDistance = Mathf.Max(10.0f, MinDistance);
        _rotation = Vector2.Zero;
    }

    /// <summary>
    /// Sets the orbit target point.
    /// </summary>
    public void SetTargetPosition(Vector3 position)
    {
        _targetPosition = position;
    }

    /// <summary>
    /// Sets the camera distance.
    /// </summary>
    public void SetDistance(float distance)
    {
        _distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        _targetDistance = _distance;
    }

    /// <summary>
    /// Applies a normalized screen-center offset so the visible render pane becomes the framing target.
    /// </summary>
    public void SetFramingOffset(Vector2 framingOffset)
    {
        _framingOffset = framingOffset;
    }

    /// <summary>
    /// Returns the current camera distance.
    /// </summary>
    public float GetDistance()
    {
        return _distance;
    }

    /// <summary>
    /// Checks whether the mouse is currently over a UI control.
    /// </summary>
    private bool IsMouseOverUi()
    {
        Viewport viewport = GetViewport();
        return viewport != null && viewport.GuiGetHoveredControl() != null;
    }

    /// <summary>
    /// Handles mouse button events for rotation and zoom.
    /// </summary>
    private void HandleMouseButton(InputEventMouseButton mouseEvent)
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
            case MouseButton.WheelUp:
                if (!IsMouseOverUi())
                {
                    _targetDistance = Mathf.Clamp(_targetDistance * (1.0f - ZoomSpeed), MinDistance, MaxDistance);
                }

                break;
            case MouseButton.WheelDown:
                if (!IsMouseOverUi())
                {
                    _targetDistance = Mathf.Clamp(_targetDistance * (1.0f + ZoomSpeed), MinDistance, MaxDistance);
                }

                break;
        }
    }

    /// <summary>
    /// Handles mouse motion for orbiting and panning.
    /// </summary>
    private void HandleMouseMotion(InputEventMouseMotion motionEvent)
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
            _rotation.X -= delta.X * OrbitSpeed * 0.01f;
            _rotation.Y -= delta.Y * OrbitSpeed * 0.01f;
            _rotation.Y = Mathf.Clamp(_rotation.Y, Mathf.DegToRad(MinPitch), Mathf.DegToRad(MaxPitch));
        }
        else if (_panning)
        {
            Vector3 panDelta = new(
                -delta.X * PanSpeed * 0.01f * _distance * 0.1f,
                delta.Y * PanSpeed * 0.01f * _distance * 0.1f,
                0.0f);
            panDelta = GlobalTransform.Basis * panDelta;
            _targetPosition += panDelta;
        }

        _lastMousePosition = motionEvent.Position;
    }

    /// <summary>
    /// Recomputes the camera transform from the current state.
    /// </summary>
    private void UpdateCameraTransform()
    {
        Vector3 cameraPosition = new(
            _distance * Mathf.Cos(_rotation.Y) * Mathf.Sin(_rotation.X),
            _distance * Mathf.Sin(_rotation.Y),
            _distance * Mathf.Cos(_rotation.Y) * Mathf.Cos(_rotation.X));

        cameraPosition += _targetPosition;
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
    /// Shifts the look-at point so the orbit target is centered in the visible render area.
    /// </summary>
    private Vector3 ComputeFocusPoint(Vector3 cameraPosition)
    {
        if (_framingOffset == Vector2.Zero)
        {
            return _targetPosition;
        }

        Viewport? viewport = GetViewport();
        Vector2 viewportSize = new Vector2(1280.0f, 720.0f);
        if (viewport != null)
        {
            viewportSize = viewport.GetVisibleRect().Size;
        }

        float aspect = viewportSize.X / Mathf.Max(1.0f, viewportSize.Y);
        float distance = cameraPosition.DistanceTo(_targetPosition);
        if (distance <= 0.001f)
        {
            return _targetPosition;
        }

        Vector3 forward = (_targetPosition - cameraPosition).Normalized();
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
        return _targetPosition - (right * shiftX) + (up * shiftY);
    }
}
