using Godot;
using StarGen.App.Rendering;
using StarGen.Domain.Celestial;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Represents a single celestial body in the system viewer.
/// </summary>
public partial class SystemBodyNode : Node3D
{
    /// <summary>
    /// Emitted when the body is clicked.
    /// </summary>
    [Signal]
    public delegate void BodySelectedEventHandler(string bodyId);

    /// <summary>
    /// Emitted when the body is hovered.
    /// </summary>
    [Signal]
    public delegate void BodyHoveredEventHandler(string bodyId);

    /// <summary>
    /// Emitted when body hover ends.
    /// </summary>
    [Signal]
    public delegate void BodyUnhoveredEventHandler(string bodyId);

    /// <summary>
    /// The body represented by this node.
    /// </summary>
    public CelestialBody? Body;

    /// <summary>
    /// Unique body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Display radius in viewport units.
    /// </summary>
    public float DisplayRadius = 0.1f;

    /// <summary>
    /// Whether the body is selected.
    /// </summary>
    public bool IsSelected;

    /// <summary>
    /// Whether the body is hovered.
    /// </summary>
    public bool IsHovered;

    private MeshInstance3D? _meshInstance;
    private OmniLight3D? _starLight;
    private MeshInstance3D? _selectionRing;
    private Area3D? _clickArea;
    private CollisionShape3D? _collisionShape;

    /// <summary>
    /// Initializes this node with body data.
    /// </summary>
    public void Setup(CelestialBody? body, float displayRadius, Vector3 position)
    {
        if (body == null)
        {
            return;
        }

        Body = body;
        BodyId = body.Id;
        DisplayRadius = displayRadius;
        Position = position;

        CreateMesh();
        CreateClickArea();

        if (body.Type == CelestialType.Type.Star)
        {
            CreateStarLight();
        }

        Name = $"Body_{BodyId}";
    }

    /// <summary>
    /// Sets the selected state.
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        UpdateSelectionVisual();
    }

    /// <summary>
    /// Sets the hovered state.
    /// </summary>
    public void SetHovered(bool hovered)
    {
        if (hovered == IsHovered)
        {
            return;
        }

        IsHovered = hovered;
        UpdateHoverVisual();

        if (hovered)
        {
            EmitSignal(SignalName.BodyHovered, BodyId);
        }
        else
        {
            EmitSignal(SignalName.BodyUnhovered, BodyId);
        }
    }

    /// <summary>
    /// Returns the represented body type.
    /// </summary>
    public CelestialType.Type GetBodyType()
    {
        if (Body != null)
        {
            return Body.Type;
        }

        return CelestialType.Type.Planet;
    }

    /// <summary>
    /// Returns the display name for this body.
    /// </summary>
    public string GetDisplayName()
    {
        if (Body != null)
        {
            return Body.Name;
        }

        return "Unknown";
    }

    /// <summary>
    /// Returns the orbital parent identifier.
    /// </summary>
    public string GetParentId()
    {
        if (Body == null || !Body.HasOrbital() || Body.Orbital == null)
        {
            return string.Empty;
        }

        return Body.Orbital.ParentId;
    }

    /// <summary>
    /// Updates the visual scale and click radius.
    /// </summary>
    public void UpdateVisual()
    {
        if (_meshInstance != null)
        {
            _meshInstance.Scale = Vector3.One * DisplayRadius * 2.0f;
        }

        if (_collisionShape?.Shape is SphereShape3D sphereShape)
        {
            sphereShape.Radius = DisplayRadius * 1.5f;
        }

        UpdateSelectionVisual();
    }

    /// <summary>
    /// Clears signal hookups and body references.
    /// </summary>
    public void Cleanup()
    {
        Body = null;

        if (_clickArea != null)
        {
            _clickArea.MouseEntered -= OnMouseEntered;
            _clickArea.MouseExited -= OnMouseExited;
            _clickArea.InputEvent -= OnInputEvent;
        }
    }

    /// <summary>
    /// GDScript-compatible wrapper for setup.
    /// </summary>
    public void setup(CelestialBody? body, float displayRadius, Vector3 position)
    {
        Setup(body, displayRadius, position);
    }

    /// <summary>
    /// GDScript-compatible wrapper for selection updates.
    /// </summary>
    public void set_selected(bool selected)
    {
        SetSelected(selected);
    }

    /// <summary>
    /// GDScript-compatible wrapper for hover updates.
    /// </summary>
    public void set_hovered(bool hovered)
    {
        SetHovered(hovered);
    }

    /// <summary>
    /// GDScript-compatible wrapper for display-name access.
    /// </summary>
    public string get_display_name()
    {
        return GetDisplayName();
    }

    /// <summary>
    /// GDScript-compatible wrapper for parent-id access.
    /// </summary>
    public string get_parent_id()
    {
        return GetParentId();
    }

    /// <summary>
    /// GDScript-compatible wrapper for visual refresh.
    /// </summary>
    public void update_visual()
    {
        UpdateVisual();
    }

    /// <summary>
    /// GDScript-compatible wrapper for cleanup.
    /// </summary>
    public void cleanup()
    {
        Cleanup();
    }

    /// <summary>
    /// Creates or refreshes the body mesh.
    /// </summary>
    private void CreateMesh()
    {
        if (Body == null)
        {
            return;
        }

        if (_meshInstance == null)
        {
            _meshInstance = new MeshInstance3D
            {
                Name = "Mesh",
            };
            AddChild(_meshInstance);
        }

        SphereMesh sphere = new()
        {
            Radius = 1.0f,
            Height = 2.0f,
        };

        switch (Body.Type)
        {
            case CelestialType.Type.Star:
                sphere.RadialSegments = 32;
                sphere.Rings = 16;
                break;
            case CelestialType.Type.Planet:
                sphere.RadialSegments = 24;
                sphere.Rings = 12;
                break;
            case CelestialType.Type.Moon:
                sphere.RadialSegments = 16;
                sphere.Rings = 8;
                break;
            case CelestialType.Type.Asteroid:
                sphere.RadialSegments = 12;
                sphere.Rings = 6;
                break;
        }

        _meshInstance.Mesh = sphere;
        _meshInstance.Scale = Vector3.One * DisplayRadius * 2.0f;
        _meshInstance.MaterialOverride = MaterialFactory.CreateBodyMaterial(Body);
        _meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
    }

    /// <summary>
    /// Creates the click-detection area.
    /// </summary>
    private void CreateClickArea()
    {
        if (_clickArea != null)
        {
            return;
        }

        _clickArea = new Area3D
        {
            Name = "ClickArea",
        };
        _collisionShape = new CollisionShape3D
        {
            Name = "Shape",
            Shape = new SphereShape3D
            {
                Radius = DisplayRadius * 1.5f,
            },
        };

        _clickArea.AddChild(_collisionShape);
        AddChild(_clickArea);

        _clickArea.MouseEntered += OnMouseEntered;
        _clickArea.MouseExited += OnMouseExited;
        _clickArea.InputEvent += OnInputEvent;
    }

    /// <summary>
    /// Creates the point light used for star bodies.
    /// </summary>
    private void CreateStarLight()
    {
        if (Body == null)
        {
            return;
        }

        if (_starLight == null)
        {
            _starLight = new OmniLight3D
            {
                Name = "StarLight",
                ShadowEnabled = false,
            };
            AddChild(_starLight);
        }

        float temperatureK;
        if (Body.HasStellar() && Body.Stellar != null)
        {
            temperatureK = (float)Body.Stellar.EffectiveTemperatureK;
        }
        else
        {
            temperatureK = 5778.0f;
        }
        _starLight.LightColor = ColorUtils.TemperatureToBlackbodyColor(temperatureK);

        float energy = 2.0f;
        if (Body.HasStellar() && Body.Stellar != null)
        {
            float luminositySolar = (float)(Body.Stellar.LuminosityWatts / 3.828e26);
            energy = 1.0f + ((Mathf.Log(Mathf.Max(luminositySolar, 0.01f)) / Mathf.Log(10.0f)) * 0.5f);
            energy = Mathf.Clamp(energy, 0.5f, 6.0f);
        }

        _starLight.LightEnergy = energy;
        _starLight.OmniRange = DisplayRadius * 30.0f;
    }

    /// <summary>
    /// Updates the selection-ring visual.
    /// </summary>
    private void UpdateSelectionVisual()
    {
        if (IsSelected)
        {
            if (_selectionRing == null)
            {
                CreateSelectionRing();
            }

            if (_selectionRing != null)
            {
                _selectionRing.Visible = true;
            }
        }
        else if (_selectionRing != null)
        {
            _selectionRing.Visible = false;
        }
    }

    /// <summary>
    /// Updates the hover-state scale cue.
    /// </summary>
    private void UpdateHoverVisual()
    {
        if (_meshInstance == null)
        {
            return;
        }

        if (IsHovered)
        {
            _meshInstance.Scale = Vector3.One * DisplayRadius * 2.3f;
        }
        else
        {
            _meshInstance.Scale = Vector3.One * DisplayRadius * 2.0f;
        }
    }

    /// <summary>
    /// Creates the selection ring mesh.
    /// </summary>
    private void CreateSelectionRing()
    {
        _selectionRing = new MeshInstance3D
        {
            Name = "SelectionRing",
            Mesh = new TorusMesh
            {
                InnerRadius = DisplayRadius * 2.2f,
                OuterRadius = DisplayRadius * 2.5f,
                Rings = 16,
                RingSegments = 32,
            },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Visible = false,
        };

        StandardMaterial3D material = new()
        {
            AlbedoColor = new Color(1.0f, 0.9f, 0.3f, 0.8f),
            EmissionEnabled = true,
            Emission = new Color(1.0f, 0.9f, 0.3f),
            EmissionEnergyMultiplier = 0.5f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        };
        _selectionRing.MaterialOverride = material;
        AddChild(_selectionRing);
    }

    /// <summary>
    /// Handles mouse-enter events from the click area.
    /// </summary>
    private void OnMouseEntered()
    {
        SetHovered(true);
    }

    /// <summary>
    /// Handles mouse-exit events from the click area.
    /// </summary>
    private void OnMouseExited()
    {
        SetHovered(false);
    }

    /// <summary>
    /// Handles click input on the body area.
    /// </summary>
    private void OnInputEvent(Node camera, InputEvent inputEvent, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (inputEvent is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.BodySelected, BodyId);
        }
    }
}
