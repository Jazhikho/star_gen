using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// First-person exploration camera for the star view.
/// </summary>
public partial class StarViewCamera : Camera3D
{
    /// <summary>
    /// Emitted when the camera crosses into a new subsector.
    /// </summary>
    [Signal]
    public delegate void SubsectorChangedEventHandler(Vector3 newOrigin);

    private const float BaseMoveSpeed = 5.0f;
    private const float MinSpeed = 0.5f;
    private const float MaxSpeed = 50.0f;
    private const float SpeedScrollFactor = 1.25f;
    private const float MouseSensitivity = 0.3f;

    private float _yawDeg;
    private float _pitchDeg;
    private float _moveSpeed = BaseMoveSpeed;
    private bool _mouseLooking;
    private Vector3 _currentSubsectorOrigin = Vector3.Zero;

    /// <summary>
    /// Applies the current camera state when the node enters the scene tree.
    /// </summary>
    public override void _Ready()
    {
        UpdateOrientation();
    }

    /// <summary>
    /// Places the camera at a starting position and sets initial orientation.
    /// </summary>
    public void Configure(Vector3 startPosition, float initialYawDeg = 0.0f)
    {
        SetCameraPosition(startPosition);
        _yawDeg = initialYawDeg;
        _pitchDeg = 0.0f;
        _moveSpeed = BaseMoveSpeed;
        _currentSubsectorOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(startPosition);
        UpdateOrientation();
    }

    /// <summary>
    /// Returns the current movement speed.
    /// </summary>
    public float GetMoveSpeed()
    {
        return _moveSpeed;
    }

    /// <summary>
    /// Returns the current subsector origin.
    /// </summary>
    public Vector3 GetCurrentSubsectorOrigin()
    {
        return _currentSubsectorOrigin;
    }

    /// <summary>
    /// Returns the current camera position without requiring the node to be in the scene tree.
    /// </summary>
    public Vector3 GetCurrentPosition()
    {
        return GetCameraPosition();
    }

    /// <summary>
    /// Advances movement/orientation and subsector tracking.
    /// </summary>
    public override void _Process(double delta)
    {
        HandleMovement((float)delta);
        UpdateOrientation();
        CheckSubsectorChange();
    }

    /// <summary>
    /// Handles mouse input for look controls and speed changes.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseButton(mouseButton);
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion && _mouseLooking)
        {
            HandleMouseMotion(mouseMotion);
        }
    }

    /// <summary>
    /// Handles mouse button input.
    /// </summary>
    private void HandleMouseButton(InputEventMouseButton mouseEvent)
    {
        switch (mouseEvent.ButtonIndex)
        {
            case MouseButton.Right:
                _mouseLooking = mouseEvent.Pressed;
                break;
            case MouseButton.WheelUp:
                if (mouseEvent.Pressed)
                {
                    _moveSpeed = Mathf.Clamp(_moveSpeed * SpeedScrollFactor, MinSpeed, MaxSpeed);
                }

                break;
            case MouseButton.WheelDown:
                if (mouseEvent.Pressed)
                {
                    _moveSpeed = Mathf.Clamp(_moveSpeed / SpeedScrollFactor, MinSpeed, MaxSpeed);
                }

                break;
        }
    }

    /// <summary>
    /// Handles right-drag mouse motion for yaw and pitch.
    /// </summary>
    private void HandleMouseMotion(InputEventMouseMotion mouseEvent)
    {
        _yawDeg -= mouseEvent.Relative.X * MouseSensitivity;
        _pitchDeg -= mouseEvent.Relative.Y * MouseSensitivity;
        _pitchDeg = Mathf.Clamp(_pitchDeg, -89.0f, 89.0f);
    }

    /// <summary>
    /// Processes movement input each frame.
    /// </summary>
    private void HandleMovement(float delta)
    {
        Vector3 forward = GetForward();
        Vector3 right = GetRight();
        float moveDelta = _moveSpeed * delta;
        Vector3 currentPosition = GetCameraPosition();

        if (Input.IsKeyPressed(Key.W))
        {
            currentPosition += forward * moveDelta;
        }

        if (Input.IsKeyPressed(Key.S))
        {
            currentPosition -= forward * moveDelta;
        }

        if (Input.IsKeyPressed(Key.A))
        {
            currentPosition -= right * moveDelta;
        }

        if (Input.IsKeyPressed(Key.D))
        {
            currentPosition += right * moveDelta;
        }

        if (Input.IsKeyPressed(Key.E))
        {
            currentPosition += Vector3.Up * moveDelta;
        }

        if (Input.IsKeyPressed(Key.C))
        {
            currentPosition -= Vector3.Up * moveDelta;
        }

        SetCameraPosition(currentPosition);
    }

    /// <summary>
    /// Returns the camera's level forward direction.
    /// </summary>
    private Vector3 GetForward()
    {
        float yawRad = Mathf.DegToRad(_yawDeg);
        return new Vector3(-Mathf.Sin(yawRad), 0.0f, -Mathf.Cos(yawRad));
    }

    /// <summary>
    /// Returns the camera's level right direction.
    /// </summary>
    private Vector3 GetRight()
    {
        float yawRad = Mathf.DegToRad(_yawDeg);
        return new Vector3(Mathf.Cos(yawRad), 0.0f, -Mathf.Sin(yawRad));
    }

    /// <summary>
    /// Applies yaw and pitch to the camera orientation.
    /// </summary>
    private void UpdateOrientation()
    {
        Transform3D transform = Transform;
        transform.Basis = Basis.Identity;
        Transform = transform;
        RotateY(Mathf.DegToRad(_yawDeg));
        RotateObjectLocal(Vector3.Right, Mathf.DegToRad(_pitchDeg));
    }

    /// <summary>
    /// Emits when the camera enters a different subsector.
    /// </summary>
    private void CheckSubsectorChange()
    {
        Vector3 newOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(GetCameraPosition());
        if (!newOrigin.IsEqualApprox(_currentSubsectorOrigin))
        {
            _currentSubsectorOrigin = newOrigin;
            EmitSignal(SignalName.SubsectorChanged, newOrigin);
        }
    }

    /// <summary>
    /// Returns the camera position in the current context.
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
    /// Sets the camera position without requiring the node to be inside the scene tree.
    /// </summary>
    private void SetCameraPosition(Vector3 position)
    {
        if (IsInsideTree())
        {
            GlobalPosition = position;
            return;
        }

        Position = position;
    }
}
