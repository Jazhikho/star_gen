using Godot;
using Godot.Collections;
using StarGen.App.Rendering;
using StarGen.Domain.Celestial;

namespace StarGen.App.Viewer;

/// <summary>
/// Manages moon display, orbital animation, and focus for the object viewer.
/// </summary>
public partial class ObjectViewerMoonSystem : RefCounted
{
    /// <summary>
    /// Emitted when focus shifts to a moon. Null means focus returned to the planet.
    /// </summary>
    [Signal]
    public delegate void MoonFocusedEventHandler(CelestialBody? moon);

    private const float OrbitBasePeriodS = 120.0f;
    private const float OrbitReferenceSmaM = 3.844e8f;

    private CelestialBody? _primaryBody;
    private readonly Array<CelestialBody> _moons = [];
    private CelestialBody? _focusedMoon;
    private readonly Array<BodyRenderer> _moonRenderers = [];
    private float _orbitVisualTime;
    private float _primaryDisplayScale = 1.0f;
    private Node3D? _moonSystemRig;
    private Node3D? _moonBodiesNode;
    private Node3D? _moonOrbitsNode;

    /// <summary>
    /// Sets up the moon system containers under the supplied body renderer.
    /// </summary>
    public void Setup(Node3D bodyRenderer)
    {
        _moonSystemRig = new Node3D { Name = "MoonSystemRig" };
        bodyRenderer.AddChild(_moonSystemRig);

        _moonBodiesNode = new Node3D { Name = "MoonBodies" };
        _moonSystemRig.AddChild(_moonBodiesNode);

        _moonOrbitsNode = new Node3D { Name = "MoonOrbits" };
        _moonSystemRig.AddChild(_moonOrbitsNode);
    }

    /// <summary>
    /// Sets the primary body and its display scale.
    /// </summary>
    public void SetPrimaryBody(CelestialBody? body, float displayScale)
    {
        _primaryBody = body;
        _primaryDisplayScale = displayScale;
    }

    /// <summary>
    /// Builds the moon display for the supplied moons.
    /// </summary>
    public void BuildMoonDisplay(Array<CelestialBody> moons, float axialTiltDeg)
    {
        Clear();
        foreach (CelestialBody moon in moons)
        {
            _moons.Add(moon);
        }

        _orbitVisualTime = 0.0f;
        if (_moons.Count == 0 || _primaryBody == null || _moonSystemRig == null || _moonBodiesNode == null || _moonOrbitsNode == null)
        {
            return;
        }

        _moonSystemRig.Basis = new Basis(Vector3.Forward, Mathf.DegToRad(axialTiltDeg));
        float moonScale = GetMoonSystemScale();

        for (int index = 0; index < _moons.Count; index += 1)
        {
            CelestialBody moon = _moons[index];
            BodyRenderer renderer = new()
            {
                Name = $"MoonRenderer_{index}",
                Position = GetMoonPositionAtMeanAnomaly(moon, ComputeLiveMeanAnomaly(moon)),
            };

            _moonBodiesNode.AddChild(renderer);
            renderer.RenderBody(moon, (float)(moon.Physical.RadiusM * moonScale));
            _moonRenderers.Add(renderer);

            MeshInstance3D? orbitLine = CreateMoonOrbitLine(moon, moonScale);
            if (orbitLine != null)
            {
                _moonOrbitsNode.AddChild(orbitLine);
            }
        }
    }

    /// <summary>
    /// Clears all moon renderers and orbit lines.
    /// </summary>
    public void Clear()
    {
        if (_moonBodiesNode != null)
        {
            foreach (Node child in _moonBodiesNode.GetChildren())
            {
                child.QueueFree();
            }
        }

        if (_moonOrbitsNode != null)
        {
            foreach (Node child in _moonOrbitsNode.GetChildren())
            {
                child.QueueFree();
            }
        }

        _moonRenderers.Clear();
        _moons.Clear();
        _focusedMoon = null;
        _orbitVisualTime = 0.0f;
    }

    /// <summary>
    /// Updates moon orbital positions for the current frame.
    /// </summary>
    public void UpdateOrbitalPositions(float delta)
    {
        if (_moons.Count == 0)
        {
            return;
        }

        _orbitVisualTime += delta;
        for (int index = 0; index < _moons.Count && index < _moonRenderers.Count; index += 1)
        {
            CelestialBody moon = _moons[index];
            BodyRenderer renderer = _moonRenderers[index];
            if (!moon.HasOrbital() || !GodotObject.IsInstanceValid(renderer))
            {
                continue;
            }

            renderer.Position = GetMoonPositionAtMeanAnomaly(moon, ComputeLiveMeanAnomaly(moon));
        }
    }

    /// <summary>
    /// Returns the currently focused moon, or null when the primary body is focused.
    /// </summary>
    public CelestialBody? GetFocusedMoon()
    {
        return _focusedMoon;
    }

    /// <summary>
    /// Returns the moons currently displayed.
    /// </summary>
    public Array<CelestialBody> GetMoons()
    {
        return _moons;
    }

    /// <summary>
    /// Returns whether any moons are currently displayed.
    /// </summary>
    public bool HasMoons()
    {
        return _moons.Count > 0;
    }

    /// <summary>
    /// Shifts focus to the supplied moon.
    /// </summary>
    public bool FocusOnMoon(CelestialBody? moon)
    {
        if (moon == null || !_moons.Contains(moon))
        {
            return false;
        }

        _focusedMoon = moon;
        EmitSignal(SignalName.MoonFocused, moon);
        return true;
    }

    /// <summary>
    /// Returns focus to the primary body.
    /// </summary>
    public void FocusOnPlanet()
    {
        _focusedMoon = null;
        EmitSignal(SignalName.MoonFocused, default(Variant));
    }

    /// <summary>
    /// Returns the world-space position of the focused moon for camera following.
    /// </summary>
    public Vector3 GetFocusedMoonPosition()
    {
        if (_focusedMoon == null)
        {
            return Vector3.Zero;
        }

        int index = _moons.IndexOf(_focusedMoon);
        if (index < 0 || index >= _moonRenderers.Count)
        {
            return Vector3.Zero;
        }

        BodyRenderer renderer = _moonRenderers[index];
        return GodotObject.IsInstanceValid(renderer) ? renderer.GlobalPosition : Vector3.Zero;
    }

    /// <summary>
    /// Returns the display radius of the focused moon.
    /// </summary>
    public float GetFocusedMoonDisplayRadius()
    {
        return _focusedMoon == null ? 0.0f : (float)(_focusedMoon.Physical.RadiusM * GetMoonSystemScale());
    }

    /// <summary>
    /// Calculates the camera distance needed to frame the full moon system.
    /// </summary>
    public float GetFramingDistance()
    {
        if (_primaryBody == null)
        {
            return 10.0f;
        }

        float minFrame = _primaryDisplayScale * 3.0f;
        if (_moons.Count == 0)
        {
            return Mathf.Max(minFrame, _primaryDisplayScale * 3.0f);
        }

        float moonScale = GetMoonSystemScale();
        float farthest = 0.0f;
        foreach (CelestialBody moon in _moons)
        {
            if (!moon.HasOrbital() || moon.Orbital == null)
            {
                continue;
            }

            float apoapsis = (float)(moon.Orbital.SemiMajorAxisM * (1.0 + moon.Orbital.Eccentricity) * moonScale);
            farthest = Mathf.Max(farthest, apoapsis);
        }

        return farthest <= 0.0f ? Mathf.Max(minFrame, _primaryDisplayScale * 3.0f) : Mathf.Max(minFrame, farthest * 1.5f);
    }

    /// <summary>
    /// Detects whether a click intersects a moon.
    /// </summary>
    public CelestialBody? DetectMoonClick(Camera3D? camera, Vector2 mousePosition)
    {
        if (_moons.Count == 0 || camera == null)
        {
            return null;
        }

        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
        Vector3 rayDirection = camera.ProjectRayNormal(mousePosition);
        CelestialBody? bestMoon = null;
        float bestT = float.PositiveInfinity;
        float moonScale = GetMoonSystemScale();

        for (int index = 0; index < _moons.Count && index < _moonRenderers.Count; index += 1)
        {
            CelestialBody moon = _moons[index];
            BodyRenderer renderer = _moonRenderers[index];
            if (!GodotObject.IsInstanceValid(renderer))
            {
                continue;
            }

            Vector3 center = renderer.GlobalPosition;
            float radius = (float)(moon.Physical.RadiusM * moonScale);
            Vector3 oc = rayOrigin - center;
            float b = oc.Dot(rayDirection);
            float c = oc.Dot(oc) - (radius * radius);
            float disc = (b * b) - c;
            if (disc < 0.0f)
            {
                continue;
            }

            float t = -b - Mathf.Sqrt(disc);
            if (t > 0.0f && t < bestT)
            {
                bestT = t;
                bestMoon = moon;
            }
        }

        return bestMoon;
    }

    /// <summary>
    /// Returns the scale factor converting physical meters to display units.
    /// </summary>
    private float GetMoonSystemScale()
    {
        if (_primaryBody == null || _primaryBody.Physical.RadiusM <= 0.0)
        {
            return 1.0f;
        }

        return (float)(_primaryDisplayScale / _primaryBody.Physical.RadiusM);
    }

    /// <summary>
    /// Returns the live mean anomaly for a moon at the current visual time.
    /// </summary>
    private float ComputeLiveMeanAnomaly(CelestialBody moon)
    {
        if (!moon.HasOrbital() || moon.Orbital == null)
        {
            return 0.0f;
        }

        float semiMajorAxis = (float)moon.Orbital.SemiMajorAxisM;
        float periodScale = Mathf.Pow(semiMajorAxis / OrbitReferenceSmaM, 1.5f);
        float visualPeriod = Mathf.Max(OrbitBasePeriodS * periodScale, 0.001f);
        float initialMeanAnomaly = Mathf.DegToRad((float)moon.Orbital.MeanAnomalyDeg);
        return initialMeanAnomaly + ((Mathf.Tau / visualPeriod) * _orbitVisualTime);
    }

    /// <summary>
    /// Computes the display-space position of a moon at a supplied mean anomaly.
    /// </summary>
    private Vector3 GetMoonPositionAtMeanAnomaly(CelestialBody moon, float meanAnomalyRad)
    {
        if (!moon.HasOrbital() || moon.Orbital == null)
        {
            return Vector3.Zero;
        }

        float moonScale = GetMoonSystemScale();
        float semiMajorAxis = (float)(moon.Orbital.SemiMajorAxisM * moonScale);
        float eccentricity = Mathf.Clamp((float)moon.Orbital.Eccentricity, 0.0f, 0.99f);
        float inclination = Mathf.DegToRad((float)moon.Orbital.InclinationDeg);
        float ascendingNode = Mathf.DegToRad((float)moon.Orbital.LongitudeOfAscendingNodeDeg);
        float periapsisArgument = Mathf.DegToRad((float)moon.Orbital.ArgumentOfPeriapsisDeg);

        float eccentricAnomaly = SolveKepler(meanAnomalyRad, eccentricity);
        float trueAnomaly = 2.0f * Mathf.Atan2(
            Mathf.Sqrt(1.0f + eccentricity) * Mathf.Sin(eccentricAnomaly / 2.0f),
            Mathf.Sqrt(1.0f - eccentricity) * Mathf.Cos(eccentricAnomaly / 2.0f));
        float radius = semiMajorAxis * (1.0f - (eccentricity * Mathf.Cos(eccentricAnomaly)));
        float px = radius * Mathf.Cos(trueAnomaly);
        float py = radius * Mathf.Sin(trueAnomaly);

        float cLan = Mathf.Cos(ascendingNode);
        float sLan = Mathf.Sin(ascendingNode);
        float cAop = Mathf.Cos(periapsisArgument);
        float sAop = Mathf.Sin(periapsisArgument);
        float cInc = Mathf.Cos(inclination);
        float sInc = Mathf.Sin(inclination);

        float x = ((cLan * cAop) - (sLan * sAop * cInc)) * px + ((-cLan * sAop) - (sLan * cAop * cInc)) * py;
        float z = ((sLan * cAop) + (cLan * sAop * cInc)) * px + ((-sLan * sAop) + (cLan * cAop * cInc)) * py;
        float y = (sAop * sInc) * px + (cAop * sInc) * py;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Solves Kepler's equation with a short Newton-Raphson iteration.
    /// </summary>
    private static float SolveKepler(float meanAnomaly, float eccentricity)
    {
        float eccentricAnomaly = meanAnomaly;
        for (int index = 0; index < 5; index += 1)
        {
            eccentricAnomaly -=
                (eccentricAnomaly - (eccentricity * Mathf.Sin(eccentricAnomaly)) - meanAnomaly) /
                (1.0f - (eccentricity * Mathf.Cos(eccentricAnomaly)));
        }

        return eccentricAnomaly;
    }

    /// <summary>
    /// Creates a closed elliptical orbit line for a moon.
    /// </summary>
    private static MeshInstance3D? CreateMoonOrbitLine(CelestialBody moon, float moonScale)
    {
        if (!moon.HasOrbital() || moon.Orbital == null)
        {
            return null;
        }

        float semiMajorAxis = (float)(moon.Orbital.SemiMajorAxisM * moonScale);
        float eccentricity = Mathf.Clamp((float)moon.Orbital.Eccentricity, 0.0f, 0.99f);
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1.0f - (eccentricity * eccentricity));
        float inclination = Mathf.DegToRad((float)moon.Orbital.InclinationDeg);
        float ascendingNode = Mathf.DegToRad((float)moon.Orbital.LongitudeOfAscendingNodeDeg);
        float periapsisArgument = Mathf.DegToRad((float)moon.Orbital.ArgumentOfPeriapsisDeg);
        float cLan = Mathf.Cos(ascendingNode);
        float sLan = Mathf.Sin(ascendingNode);
        float cAop = Mathf.Cos(periapsisArgument);
        float sAop = Mathf.Sin(periapsisArgument);
        float cInc = Mathf.Cos(inclination);
        float sInc = Mathf.Sin(inclination);
        float focusOffset = semiMajorAxis * eccentricity;

        ImmediateMesh orbitMesh = new();
        orbitMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        const int Segments = 128;
        for (int index = 0; index <= Segments; index += 1)
        {
            float angle = ((float)index / Segments) * Mathf.Tau;
            float px = (semiMajorAxis * Mathf.Cos(angle)) - focusOffset;
            float py = semiMinorAxis * Mathf.Sin(angle);
            float x = ((cLan * cAop) - (sLan * sAop * cInc)) * px + ((-cLan * sAop) - (sLan * cAop * cInc)) * py;
            float z = ((sLan * cAop) + (cLan * sAop * cInc)) * px + ((-sLan * sAop) + (cLan * cAop * cInc)) * py;
            float y = (sAop * sInc) * px + (cAop * sInc) * py;
            orbitMesh.SurfaceAddVertex(new Vector3(x, y, z));
        }

        orbitMesh.SurfaceEnd();

        MeshInstance3D instance = new()
        {
            Mesh = orbitMesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };

        StandardMaterial3D material = new()
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.45f, 0.65f, 0.85f, 0.55f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
        };
        instance.MaterialOverride = material;
        return instance;
    }
}
