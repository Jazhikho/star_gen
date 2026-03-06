using System.Collections.Generic;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Scene-attached renderer for a single celestial body.
/// </summary>
public partial class BodyRenderer : Node3D
{
    private const float StarAtmosphereScale = 1.25f;

    private MeshInstance3D? _bodyMesh;
    private MeshInstance3D? _atmosphereMesh;
    private MeshInstance3D? _starAtmosphereMesh;
    private OmniLight3D? _starLight;
    private Node3D? _ringSystemNode;
    private CelestialBody? _currentBody;
    private float _displayScale = 1.0f;

    /// <summary>
    /// Ensures required child nodes exist and starts hidden.
    /// </summary>
    public override void _Ready()
    {
        EnsureNodesExist();
        Clear();
    }

    /// <summary>
    /// Renders a body with the supplied display scale.
    /// </summary>
    public void RenderBody(CelestialBody? body, float scaleFactor = 1.0f)
    {
        if (body == null)
        {
            Clear();
            return;
        }

        _currentBody = body;
        _displayScale = scaleFactor;
        UpdateBodyMesh();
        UpdateStarLight();
        UpdateAtmosphere();
        UpdateRingSystem();
        ApplyAxialTilt();
    }

    /// <summary>
    /// Clears all visible render state.
    /// </summary>
    public void Clear()
    {
        _currentBody = null;

        if (_bodyMesh != null)
        {
            _bodyMesh.Visible = false;
            _bodyMesh.Scale = Vector3.One;
        }

        if (_atmosphereMesh != null)
        {
            _atmosphereMesh.Visible = false;
            _atmosphereMesh.Scale = Vector3.One;
        }

        if (_starAtmosphereMesh != null)
        {
            _starAtmosphereMesh.Visible = false;
            _starAtmosphereMesh.Scale = Vector3.One;
        }

        if (_starLight != null)
        {
            _starLight.Visible = false;
        }

        ClearRingSystem();
    }

    /// <summary>
    /// Advances the local visual rotation for the rendered body.
    /// </summary>
    public void RotateBody(float delta, float speedMultiplier = 1.0f)
    {
        if (_currentBody == null || _bodyMesh == null)
        {
            return;
        }

        float period = Mathf.Abs((float)_currentBody.Physical.RotationPeriodS);
        if (period < 1.0f)
        {
            period = 86400.0f;
        }

        float rotationSpeed = (Mathf.Tau / 10.0f) * speedMultiplier;
        if (_currentBody.Physical.RotationPeriodS < 0.0)
        {
            rotationSpeed = -rotationSpeed;
        }

        _bodyMesh.RotateObjectLocal(Vector3.Up, rotationSpeed * delta);

        if (_atmosphereMesh != null && _atmosphereMesh.Visible)
        {
            _atmosphereMesh.RotateObjectLocal(Vector3.Up, rotationSpeed * delta);
        }

        if (_starAtmosphereMesh != null && _starAtmosphereMesh.Visible)
        {
            _starAtmosphereMesh.RotateObjectLocal(Vector3.Up, rotationSpeed * delta);
        }
    }

    /// <summary>
    /// Ensures all renderer child nodes exist.
    /// </summary>
    private void EnsureNodesExist()
    {
        _bodyMesh = GetNodeOrNull<MeshInstance3D>("BodyMesh");
        if (_bodyMesh == null)
        {
            _bodyMesh = new MeshInstance3D
            {
                Name = "BodyMesh",
                Mesh = new SphereMesh(),
            };
            AddChild(_bodyMesh);
        }

        _atmosphereMesh = GetNodeOrNull<MeshInstance3D>("AtmosphereMesh");
        if (_atmosphereMesh == null)
        {
            SphereMesh atmosphereSphere = new()
            {
                RadialSegments = 32,
                Rings = 16,
            };
            _atmosphereMesh = new MeshInstance3D
            {
                Name = "AtmosphereMesh",
                Mesh = atmosphereSphere,
                Visible = false,
            };
            AddChild(_atmosphereMesh);
        }

        _starLight = GetNodeOrNull<OmniLight3D>("StarLight");
        if (_starLight == null)
        {
            _starLight = new OmniLight3D
            {
                Name = "StarLight",
                Visible = false,
                ShadowEnabled = false,
            };
            AddChild(_starLight);
        }

        Node3D? existingRingNode = GetNodeOrNull<Node3D>("RingSystem") ?? _bodyMesh.GetNodeOrNull<Node3D>("RingSystem");
        if (existingRingNode != null)
        {
            if (existingRingNode.GetParent() == this)
            {
                existingRingNode.Owner = null;
                RemoveChild(existingRingNode);
                _bodyMesh.AddChild(existingRingNode);
            }

            _ringSystemNode = existingRingNode;
        }
        else
        {
            _ringSystemNode = new Node3D
            {
                Name = "RingSystem",
            };
            _ringSystemNode.Owner = null;
            _bodyMesh.AddChild(_ringSystemNode);
        }

        if (_starAtmosphereMesh == null)
        {
            SphereMesh starAtmosphereSphere = new()
            {
                RadialSegments = 64,
                Rings = 32,
            };
            _starAtmosphereMesh = new MeshInstance3D
            {
                Name = "StarAtmosphereMesh",
                Mesh = starAtmosphereSphere,
                Visible = false,
                SortingOffset = -1.0f,
            };
            AddChild(_starAtmosphereMesh);
            MoveChild(_starAtmosphereMesh, 0);
        }
    }

    /// <summary>
    /// Updates the body sphere mesh and main material.
    /// </summary>
    private void UpdateBodyMesh()
    {
        if (_bodyMesh == null || _currentBody == null)
        {
            return;
        }

        SphereMesh sphere = _bodyMesh.Mesh as SphereMesh ?? new SphereMesh();
        _bodyMesh.Mesh = sphere;

        switch (_currentBody.Type)
        {
            case CelestialType.Type.Star:
                sphere.RadialSegments = 64;
                sphere.Rings = 32;
                break;
            case CelestialType.Type.Planet:
                sphere.RadialSegments = 48;
                sphere.Rings = 24;
                break;
            case CelestialType.Type.Moon:
                sphere.RadialSegments = 32;
                sphere.Rings = 16;
                break;
            case CelestialType.Type.Asteroid:
                sphere.RadialSegments = 24;
                sphere.Rings = 12;
                break;
        }

        _bodyMesh.Scale = new Vector3(_displayScale, _displayScale, _displayScale);
        _bodyMesh.MaterialOverride = MaterialFactory.CreateBodyMaterial(_currentBody);
        _bodyMesh.Visible = true;
    }

    /// <summary>
    /// Updates the point-light for star bodies.
    /// </summary>
    private void UpdateStarLight()
    {
        if (_starLight == null)
        {
            return;
        }

        if (_currentBody == null || _currentBody.Type != CelestialType.Type.Star)
        {
            _starLight.Visible = false;
            return;
        }

        float temperatureK;
        if (_currentBody.HasStellar() && _currentBody.Stellar != null)
        {
            temperatureK = (float)_currentBody.Stellar.EffectiveTemperatureK;
        }
        else
        {
            temperatureK = 5778.0f;
        }
        _starLight.LightColor = ColorUtils.TemperatureToBlackbodyColor(temperatureK);

        float energy = 2.0f;
        if (_currentBody.HasStellar() && _currentBody.Stellar != null)
        {
            float luminositySolar = (float)(_currentBody.Stellar.LuminosityWatts / 3.828e26);
            energy = 1.0f + ((Mathf.Log(Mathf.Max(luminositySolar, 0.01f)) / Mathf.Log(10.0f)) * 0.5f);
            energy = Mathf.Clamp(energy, 0.5f, 8.0f);
        }

        _starLight.LightEnergy = energy;
        _starLight.OmniRange = _displayScale * 20.0f;
        _starLight.Visible = true;
    }

    /// <summary>
    /// Updates the atmosphere mesh for planets or stars.
    /// </summary>
    private void UpdateAtmosphere()
    {
        if (_currentBody == null || _atmosphereMesh == null)
        {
            if (_atmosphereMesh != null)
            {
                _atmosphereMesh.Visible = false;
            }

            if (_starAtmosphereMesh != null)
            {
                _starAtmosphereMesh.Visible = false;
            }

            return;
        }

        if (_currentBody.Type == CelestialType.Type.Star)
        {
            _atmosphereMesh.Visible = false;
            UpdateStarAtmosphere();
            return;
        }

        if (!_currentBody.HasAtmosphere() || _currentBody.Atmosphere == null)
        {
            _atmosphereMesh.Visible = false;
            if (_starAtmosphereMesh != null)
            {
                _starAtmosphereMesh.Visible = false;
            }

            return;
        }

        float pressure = (float)_currentBody.Atmosphere.SurfacePressurePa;
        if (pressure < 100.0f)
        {
            _atmosphereMesh.Visible = false;
            return;
        }

        ShaderMaterial? material = MaterialFactory.CreateAtmosphereMaterial(_currentBody);
        if (material == null)
        {
            _atmosphereMesh.Visible = false;
            return;
        }

        float baseScale = 1.02f;
        float pressureFactor = Mathf.Clamp(pressure / 101325.0f, 0.1f, 3.0f);
        baseScale *= 1.0f + (pressureFactor * 0.03f);

        float scaledRadius = _displayScale * baseScale;
        _atmosphereMesh.Scale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
        _atmosphereMesh.MaterialOverride = material;
        _atmosphereMesh.Visible = true;

        if (_starAtmosphereMesh != null)
        {
            _starAtmosphereMesh.Visible = false;
        }
    }

    /// <summary>
    /// Updates the star-atmosphere mesh.
    /// </summary>
    private void UpdateStarAtmosphere()
    {
        if (_starAtmosphereMesh == null || _currentBody == null)
        {
            return;
        }

        ShaderMaterial? material = MaterialFactory.CreateStarAtmosphereMaterial(_currentBody);
        if (material == null)
        {
            _starAtmosphereMesh.Visible = false;
            return;
        }

        float scaledRadius = _displayScale * StarAtmosphereScale;
        _starAtmosphereMesh.Scale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
        _starAtmosphereMesh.MaterialOverride = material;
        _starAtmosphereMesh.Visible = true;
    }

    /// <summary>
    /// Rebuilds the ring meshes for the current body.
    /// </summary>
    private void UpdateRingSystem()
    {
        ClearRingSystem();

        if (_currentBody == null || !_currentBody.HasRingSystem() || _currentBody.RingSystem == null || _ringSystemNode == null)
        {
            return;
        }

        double planetRadiusM = _currentBody.Physical.RadiusM;
        if (planetRadiusM <= 0.0)
        {
            return;
        }

        _ringSystemNode.RotationDegrees = new Vector3((float)_currentBody.RingSystem.InclinationDeg, 0.0f, 0.0f);

        for (int index = 0; index < _currentBody.RingSystem.GetBandCount(); index += 1)
        {
            RingBand? band = _currentBody.RingSystem.GetBand(index);
            if (band != null)
            {
                CreateRingBandMesh(band, (float)planetRadiusM);
            }
        }
    }

    /// <summary>
    /// Creates one rendered ring band.
    /// </summary>
    private void CreateRingBandMesh(RingBand band, float planetRadiusM)
    {
        if (_ringSystemNode == null)
        {
            return;
        }

        float innerRadius = (float)(band.InnerRadiusM / planetRadiusM) * _displayScale;
        float outerRadius = (float)(band.OuterRadiusM / planetRadiusM) * _displayScale;

        MeshInstance3D ringMesh = new()
        {
            Name = $"RingBand_{band.Name}",
            Mesh = CreateRingMesh(innerRadius, outerRadius),
            MaterialOverride = MaterialFactory.CreateRingMaterial(band),
        };
        _ringSystemNode.AddChild(ringMesh);
    }

    /// <summary>
    /// Creates a flat annulus mesh for a ring band.
    /// </summary>
    private static ArrayMesh CreateRingMesh(float innerRadius, float outerRadius, int segments = 64)
    {
        ArrayMesh mesh = new();
        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<Vector3> normals = new();
        List<int> indices = new();

        for (int index = 0; index <= segments; index += 1)
        {
            float angle = ((float)index / segments) * Mathf.Tau;
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);

            vertices.Add(new Vector3(cosA * innerRadius, 0.0f, sinA * innerRadius));
            uvs.Add(new Vector2((float)index / segments, 0.0f));
            normals.Add(Vector3.Up);

            vertices.Add(new Vector3(cosA * outerRadius, 0.0f, sinA * outerRadius));
            uvs.Add(new Vector2((float)index / segments, 1.0f));
            normals.Add(Vector3.Up);
        }

        for (int index = 0; index < segments; index += 1)
        {
            int baseIndex = index * 2;
            indices.Add(baseIndex);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 3);
            indices.Add(baseIndex + 2);
        }

        Godot.Collections.Array arrays = [];
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }

    /// <summary>
    /// Clears all ring meshes.
    /// </summary>
    private void ClearRingSystem()
    {
        if (_ringSystemNode == null)
        {
            return;
        }

        foreach (Node child in _ringSystemNode.GetChildren())
        {
            child.QueueFree();
        }
    }

    /// <summary>
    /// Applies the current body's axial tilt.
    /// </summary>
    private void ApplyAxialTilt()
    {
        if (_currentBody == null)
        {
            return;
        }

        float tilt = (float)_currentBody.Physical.AxialTiltDeg;
        if (_bodyMesh != null)
        {
            Vector3 rotation = _bodyMesh.RotationDegrees;
            rotation.Z = tilt;
            _bodyMesh.RotationDegrees = rotation;
        }

        if (_atmosphereMesh != null)
        {
            Vector3 rotation = _atmosphereMesh.RotationDegrees;
            rotation.Z = tilt;
            _atmosphereMesh.RotationDegrees = rotation;
        }

        if (_starAtmosphereMesh != null)
        {
            Vector3 rotation = _starAtmosphereMesh.RotationDegrees;
            rotation.Z = tilt;
            _starAtmosphereMesh.RotationDegrees = rotation;
        }
    }
}
