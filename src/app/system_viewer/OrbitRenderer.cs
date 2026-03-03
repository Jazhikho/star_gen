using Godot;
using System.Collections.Generic;
using StarGen.Domain.Celestial;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Renders orbital paths as 3D line meshes.
/// </summary>
public partial class OrbitRenderer : Node3D
{
    private sealed class OrbitData
    {
        public MeshInstance3D? MeshInstance;
        public string ParentId = string.Empty;
        public Vector3 CurrentCenter = Vector3.Zero;
        public Color BaseColor = new(0.3f, 0.4f, 0.6f, 0.6f);
    }

    private static readonly Color PlanetOrbitColor = new(0.3f, 0.4f, 0.6f, 0.6f);
    private static readonly Color MoonOrbitColor = new(0.4f, 0.4f, 0.5f, 0.4f);
    private static readonly Color SelectedOrbitColor = new(0.8f, 0.8f, 0.2f, 0.9f);
    private readonly Dictionary<string, OrbitData> _orbits = new();
    private MeshInstance3D? _selectedOrbit;
    private string _selectedOrbitId = string.Empty;

    /// <summary>
    /// Clears all rendered orbits.
    /// </summary>
    public void Clear()
    {
        foreach (OrbitData data in _orbits.Values)
        {
            data.MeshInstance?.QueueFree();
        }

        _orbits.Clear();
        _selectedOrbit = null;
        _selectedOrbitId = string.Empty;
    }

    /// <summary>
    /// Removes a single orbit by identifier.
    /// </summary>
    public void RemoveOrbit(string orbitId)
    {
        if (!_orbits.TryGetValue(orbitId, out OrbitData? data))
        {
            return;
        }

        data.MeshInstance?.QueueFree();
        _orbits.Remove(orbitId);

        if (_selectedOrbitId == orbitId)
        {
            _selectedOrbit = null;
            _selectedOrbitId = string.Empty;
        }
    }

    /// <summary>
    /// Adds one rendered orbit path.
    /// </summary>
    public MeshInstance3D? AddOrbit(
        string bodyId,
        Vector3[] points,
        int bodyType = (int)CelestialType.Type.Planet,
        string parentId = "",
        Vector3 center = default)
    {
        if (points == null || points.Length == 0)
        {
            return null;
        }

        if (_orbits.ContainsKey(bodyId))
        {
            RemoveOrbit(bodyId);
        }

        Color color = GetOrbitColor((CelestialType.Type)bodyType);
        bool useRelative = !string.IsNullOrEmpty(parentId) || center != Vector3.Zero;
        Vector3[] relativePoints = useRelative ? MakeRelativePoints(points, center) : points;

        MeshInstance3D meshInstance = CreateLineMesh(relativePoints, color);
        meshInstance.Name = "Orbit_" + bodyId;
        if (useRelative)
        {
            meshInstance.Position = center;
        }

        AddChild(meshInstance);

        _orbits[bodyId] = new OrbitData
        {
            MeshInstance = meshInstance,
            ParentId = parentId,
            CurrentCenter = center,
            BaseColor = color,
        };

        return meshInstance;
    }

    /// <summary>
    /// Adds a circular zone ring.
    /// </summary>
    public MeshInstance3D? AddZoneRing(string zoneName, float radiusUnits, Color color, int numPoints = 128)
    {
        if (radiusUnits <= 0.0f)
        {
            return null;
        }

        MeshInstance3D meshInstance = CreateLineMesh(GenerateCirclePoints(radiusUnits, numPoints), color);
        meshInstance.Name = "Zone_" + zoneName;
        AddChild(meshInstance);
        return meshInstance;
    }

    /// <summary>
    /// Highlights a specific orbit or clears the highlight.
    /// </summary>
    public void HighlightOrbit(string bodyId)
    {
        if (_selectedOrbit != null && !string.IsNullOrEmpty(_selectedOrbitId) && _orbits.TryGetValue(_selectedOrbitId, out OrbitData? previous))
        {
            _selectedOrbit.MaterialOverride = CreateLineMaterial(previous.BaseColor);
        }

        _selectedOrbit = null;
        _selectedOrbitId = string.Empty;

        if (string.IsNullOrEmpty(bodyId) || !_orbits.TryGetValue(bodyId, out OrbitData? data))
        {
            return;
        }

        _selectedOrbit = data.MeshInstance;
        _selectedOrbitId = bodyId;
        if (_selectedOrbit != null)
        {
            _selectedOrbit.MaterialOverride = CreateLineMaterial(SelectedOrbitColor);
        }
    }

    /// <summary>
    /// Toggles moon-orbit visibility.
    /// </summary>
    public void SetMoonOrbitsVisible(bool showMoons)
    {
        foreach (OrbitData data in _orbits.Values)
        {
            if (data.MeshInstance != null && data.MeshInstance.Name.ToString().StartsWith("Orbit_moon_", System.StringComparison.Ordinal))
            {
                data.MeshInstance.Visible = showMoons;
            }
        }
    }

    /// <summary>
    /// Updates moving-orbit centers from current host positions.
    /// </summary>
    public void UpdateOrbitPositions(Godot.Collections.Dictionary hostPositions)
    {
        foreach (OrbitData data in _orbits.Values)
        {
            if (string.IsNullOrEmpty(data.ParentId) || !hostPositions.ContainsKey(data.ParentId))
            {
                continue;
            }

            Variant centerVariant = hostPositions[data.ParentId];
            if (centerVariant.VariantType != Variant.Type.Vector3)
            {
                continue;
            }

            Vector3 newCenter = (Vector3)centerVariant;
            if (newCenter == data.CurrentCenter)
            {
                continue;
            }

            data.CurrentCenter = newCenter;
            if (data.MeshInstance != null)
            {
                data.MeshInstance.Position = newCenter;
            }
        }
    }

    /// <summary>
    /// Returns the number of rendered orbit entries.
    /// </summary>
    public int GetOrbitCount()
    {
        return _orbits.Count;
    }

    /// <summary>
    /// Returns true when the orbit exists.
    /// </summary>
    public bool HasOrbit(string orbitId)
    {
        return _orbits.ContainsKey(orbitId);
    }

    private static Vector3[] MakeRelativePoints(Vector3[] points, Vector3 center)
    {
        Vector3[] relativePoints = new Vector3[points.Length];
        for (int index = 0; index < points.Length; index += 1)
        {
            relativePoints[index] = points[index] - center;
        }

        return relativePoints;
    }

    private static MeshInstance3D CreateLineMesh(Vector3[] points, Color color)
    {
        MeshInstance3D meshInstance = new();
        ImmediateMesh immediateMesh = new();
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        foreach (Vector3 point in points)
        {
            immediateMesh.SurfaceAddVertex(point);
        }

        immediateMesh.SurfaceEnd();
        meshInstance.Mesh = immediateMesh;
        meshInstance.MaterialOverride = CreateLineMaterial(color);
        meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        return meshInstance;
    }

    private static StandardMaterial3D CreateLineMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = new Color(color.R, color.G, color.B),
            EmissionEnergyMultiplier = 0.3f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            NoDepthTest = false,
            RenderPriority = -1,
        };
    }

    private static Vector3[] GenerateCirclePoints(float radius, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints + 1];
        for (int index = 0; index <= numPoints; index += 1)
        {
            float angle = ((float)index / numPoints) * Mathf.Tau;
            points[index] = new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius);
        }

        return points;
    }

    private static Color GetOrbitColor(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Planet => PlanetOrbitColor,
            CelestialType.Type.Moon => MoonOrbitColor,
            CelestialType.Type.Asteroid => new Color(0.5f, 0.4f, 0.3f, 0.3f),
            _ => PlanetOrbitColor,
        };
    }
}
