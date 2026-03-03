using System.Collections.Generic;
using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// 3D compass rose rendered in a SubViewport corner overlay.
/// </summary>
public partial class NavigationCompass : SubViewportContainer
{
    /// <summary>
    /// Emitted when the user clicks a compass direction arrow.
    /// </summary>
    [Signal]
    public delegate void DirectionPressedEventHandler(Vector3I direction);

    private const int ViewportSize = 150;
    private const float CameraDistance = 3.8f;
    private const float CameraFov = 45.0f;
    private const float ShaftLength = 0.7f;
    private const float ShaftThickness = 0.1f;
    private const float HeadSize = 0.22f;
    private const float ClickZoneNear = 0.15f;
    private const float ClickZoneFar = 1.3f;
    private const float ClickZoneWidth = 0.3f;

    private SubViewport? _subViewport;
    private Camera3D? _compassCamera;
    private Node3D? _compassRoot;
    private readonly List<ClickZone> _clickZones = [];

    /// <summary>
    /// One arrow click zone.
    /// </summary>
    private sealed class ClickZone
    {
        public Vector3I Direction;
        public Vector3 Min;
        public Vector3 Max;
    }

    /// <summary>
    /// Builds the compass viewport and scene.
    /// </summary>
    public override void _Ready()
    {
        ConfigureContainer();
        BuildSubViewport();
        BuildCompassScene();
    }

    /// <summary>
    /// Syncs the compass camera to match the main camera's viewing angle.
    /// </summary>
    public void SyncRotation(float yawDeg, float pitchDeg)
    {
        if (_compassCamera == null)
        {
            return;
        }

        float yawRad = Mathf.DegToRad(yawDeg);
        float pitchRad = Mathf.DegToRad(pitchDeg);
        Vector3 offset = new(
            CameraDistance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
            CameraDistance * Mathf.Sin(pitchRad),
            CameraDistance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad));

        _compassCamera.GlobalPosition = offset;
        _compassCamera.LookAt(Vector3.Zero, Vector3.Up);
    }

    /// <summary>
    /// Handles overlay GUI input.
    /// </summary>
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed)
        {
            HandleCompassClick(mouseButton.Position);
            AcceptEvent();
        }
    }

    /// <summary>
    /// Configures the container size and mouse behavior.
    /// </summary>
    private void ConfigureContainer()
    {
        Stretch = true;
        CustomMinimumSize = new Vector2(ViewportSize, ViewportSize);
        Size = new Vector2(ViewportSize, ViewportSize);
        MouseFilter = MouseFilterEnum.Stop;
    }

    /// <summary>
    /// Builds the SubViewport.
    /// </summary>
    private void BuildSubViewport()
    {
        _subViewport = new SubViewport
        {
            Size = new Vector2I(ViewportSize, ViewportSize),
            TransparentBg = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            HandleInputLocally = true,
        };
        AddChild(_subViewport);
    }

    /// <summary>
    /// Builds the full 3D compass scene.
    /// </summary>
    private void BuildCompassScene()
    {
        if (_subViewport == null)
        {
            return;
        }

        _compassRoot = new Node3D { Name = "CompassRoot" };
        _subViewport.AddChild(_compassRoot);

        BuildCompassCamera();
        BuildCenterSphere();
        BuildArrow(new Vector3I(1, 0, 0), new Color(0.9f, 0.2f, 0.2f));
        BuildArrow(new Vector3I(-1, 0, 0), new Color(0.5f, 0.1f, 0.1f));
        BuildArrow(new Vector3I(0, 1, 0), new Color(0.2f, 0.9f, 0.2f));
        BuildArrow(new Vector3I(0, -1, 0), new Color(0.1f, 0.5f, 0.1f));
        BuildArrow(new Vector3I(0, 0, 1), new Color(0.2f, 0.4f, 0.9f));
        BuildArrow(new Vector3I(0, 0, -1), new Color(0.1f, 0.15f, 0.5f));
    }

    /// <summary>
    /// Creates the compass camera.
    /// </summary>
    private void BuildCompassCamera()
    {
        if (_compassRoot == null)
        {
            return;
        }

        _compassCamera = new Camera3D
        {
            Name = "CompassCamera",
            Fov = CameraFov,
            Near = 0.01f,
            Far = 20.0f,
            Current = true,
        };
        _compassRoot.AddChild(_compassCamera);
    }

    /// <summary>
    /// Creates a small center sphere.
    /// </summary>
    private void BuildCenterSphere()
    {
        if (_compassRoot == null)
        {
            return;
        }

        SphereMesh sphereMesh = new()
        {
            Radius = 0.1f,
            Height = 0.2f,
        };

        StandardMaterial3D material = new()
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.4f, 0.4f, 0.4f),
        };
        sphereMesh.Material = material;

        MeshInstance3D meshInstance = new()
        {
            Mesh = sphereMesh,
        };
        _compassRoot.AddChild(meshInstance);
    }

    /// <summary>
    /// Builds one arrow and its click zone.
    /// </summary>
    private void BuildArrow(Vector3I direction, Color color)
    {
        if (_compassRoot == null)
        {
            return;
        }

        Vector3 directionFloat = new(direction.X, direction.Y, direction.Z);
        StandardMaterial3D material = MakeUnshadedMaterial(color);

        BoxMesh shaftMesh = new()
        {
            Size = GetShaftSize(direction),
            Material = material,
        };
        MeshInstance3D shaftInstance = new()
        {
            Mesh = shaftMesh,
            Position = directionFloat * 0.6f,
        };
        _compassRoot.AddChild(shaftInstance);

        BoxMesh headMesh = new()
        {
            Size = GetHeadSize(direction),
            Material = material,
        };
        MeshInstance3D headInstance = new()
        {
            Mesh = headMesh,
            Position = directionFloat * 1.05f,
        };
        _compassRoot.AddChild(headInstance);

        _clickZones.Add(MakeClickZone(direction));
    }

    /// <summary>
    /// Returns the shaft size for a given direction.
    /// </summary>
    private static Vector3 GetShaftSize(Vector3I direction)
    {
        if (direction.X != 0)
        {
            return new Vector3(ShaftLength, ShaftThickness, ShaftThickness);
        }

        if (direction.Y != 0)
        {
            return new Vector3(ShaftThickness, ShaftLength, ShaftThickness);
        }

        return new Vector3(ShaftThickness, ShaftThickness, ShaftLength);
    }

    /// <summary>
    /// Returns the head size for a given direction.
    /// </summary>
    private static Vector3 GetHeadSize(Vector3I direction)
    {
        if (direction.X != 0)
        {
            return new Vector3(0.12f, HeadSize, HeadSize);
        }

        if (direction.Y != 0)
        {
            return new Vector3(HeadSize, 0.12f, HeadSize);
        }

        return new Vector3(HeadSize, HeadSize, 0.12f);
    }

    /// <summary>
    /// Builds a click zone for a direction.
    /// </summary>
    private static ClickZone MakeClickZone(Vector3I direction)
    {
        Vector3 min = new(-ClickZoneWidth, -ClickZoneWidth, -ClickZoneWidth);
        Vector3 max = new(ClickZoneWidth, ClickZoneWidth, ClickZoneWidth);

        if (direction.X > 0)
        {
            min.X = ClickZoneNear;
            max.X = ClickZoneFar;
        }
        else if (direction.X < 0)
        {
            min.X = -ClickZoneFar;
            max.X = -ClickZoneNear;
        }

        if (direction.Y > 0)
        {
            min.Y = ClickZoneNear;
            max.Y = ClickZoneFar;
        }
        else if (direction.Y < 0)
        {
            min.Y = -ClickZoneFar;
            max.Y = -ClickZoneNear;
        }

        if (direction.Z > 0)
        {
            min.Z = ClickZoneNear;
            max.Z = ClickZoneFar;
        }
        else if (direction.Z < 0)
        {
            min.Z = -ClickZoneFar;
            max.Z = -ClickZoneNear;
        }

        return new ClickZone
        {
            Direction = direction,
            Min = min,
            Max = max,
        };
    }

    /// <summary>
    /// Creates an unshaded material.
    /// </summary>
    private static StandardMaterial3D MakeUnshadedMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = color,
        };
    }

    /// <summary>
    /// Handles a click inside the compass viewport.
    /// </summary>
    private void HandleCompassClick(Vector2 screenPosition)
    {
        if (_compassCamera == null)
        {
            return;
        }

        Vector3 rayOrigin = _compassCamera.ProjectRayOrigin(screenPosition);
        Vector3 rayDirection = _compassCamera.ProjectRayNormal(screenPosition);
        Vector3I? bestDirection = null;
        float bestDistance = float.PositiveInfinity;

        foreach (ClickZone zone in _clickZones)
        {
            float hitDistance = RaycastUtils.RayIntersectsAabb(rayOrigin, rayDirection, zone.Min, zone.Max);
            if (hitDistance >= 0.0f && hitDistance < bestDistance)
            {
                bestDistance = hitDistance;
                bestDirection = zone.Direction;
            }
        }

        if (bestDirection.HasValue)
        {
            EmitSignal(SignalName.DirectionPressed, bestDirection.Value);
        }
    }
}
