using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Visualization creation, clearing, and body-node management for SystemViewer.
/// </summary>
public partial class SystemViewer
{
    /// <summary>
    /// Fits the camera to the current system extent.
    /// </summary>
    public void FitCameraToSystem()
    {
        if (_cameraController == null)
        {
            return;
        }

        if (_currentSystem == null || _currentLayout == null)
        {
            if (_cameraController is SystemCameraController typedCameraController)
            {
                typedCameraController.FocusOnOrigin();
            }
            else
            {
                _cameraController.Call("focus_on_origin");
            }

            return;
        }

        float maxExtent = _currentLayout.TotalExtent;
        if (maxExtent < 10.0f)
        {
            maxExtent = 20.0f;
        }

        float targetHeight = maxExtent * 2.0f;
        if (_cameraController is SystemCameraController typedCamera)
        {
            if (typedCamera.MaxHeight > 0.0f)
            {
                targetHeight = Mathf.Min(targetHeight, typedCamera.MaxHeight);
            }

            targetHeight = Mathf.Max(targetHeight, 20.0f);
            typedCamera.MinHeight = Mathf.Min(10.0f, targetHeight * 0.1f);
            if (targetHeight > typedCamera.MaxHeight)
            {
                typedCamera.MaxHeight = targetHeight * 1.5f;
            }

            typedCamera.ApplyViewState(Vector3.Zero, targetHeight, Mathf.DegToRad(60.0f), 0.0f);
            return;
        }

        Variant maxHeightVariant = _cameraController.Get("max_height");
        if (maxHeightVariant.VariantType == Variant.Type.Float && (float)(double)maxHeightVariant > 0.0f)
        {
            targetHeight = Mathf.Min(targetHeight, (float)(double)maxHeightVariant);
        }

        targetHeight = Mathf.Max(targetHeight, 20.0f);
        _cameraController.Set("min_height", Mathf.Min(10.0f, targetHeight * 0.1f));

        Variant maxHeightNow = _cameraController.Get("max_height");
        if (maxHeightNow.VariantType == Variant.Type.Float && targetHeight > (float)(double)maxHeightNow)
        {
            _cameraController.Set("max_height", targetHeight * 1.5f);
        }

        _cameraController.Set("_target_position", Vector3.Zero);
        _cameraController.Set("_target_height", targetHeight);
        _cameraController.Set("_height", targetHeight);
        _cameraController.Set("_target_pitch", Mathf.DegToRad(60.0f));
        _cameraController.Set("_yaw", 0.0f);
        _cameraController.Set("_smooth_target", Vector3.Zero);
    }

    /// <summary>
    /// Creates viewer body nodes for all displayed bodies.
    /// </summary>
    private void CreateBodyNodes()
    {
        if (_currentSystem == null || _currentLayout == null || _bodiesContainer == null)
        {
            return;
        }

        foreach (CelestialBody star in _currentSystem.GetStars())
        {
            CreateBodyNodeFromLayout(star);
        }

        foreach (CelestialBody planet in _currentSystem.GetPlanets())
        {
            CreateBodyNodeFromLayout(planet);
        }

        if (_currentSystem.AsteroidBelts.Count > 0)
        {
            foreach (CelestialBody asteroid in _currentSystem.GetAsteroids())
            {
                Vector3 position = GetMajorAsteroidDisplayPosition(asteroid);
                if (position != InvalidPosition)
                {
                    CreateMajorAsteroidNodeAt(asteroid, position);
                }
            }
        }
    }

    /// <summary>
    /// Creates one body node from precomputed layout data.
    /// </summary>
    private void CreateBodyNodeFromLayout(CelestialBody body)
    {
        if (_currentLayout == null || _bodiesContainer == null || _systemBodyNodeScene == null)
        {
            return;
        }

        BodyLayout? layout = _currentLayout.GetBodyLayout(body.Id);
        if (layout == null)
        {
            GD.PushWarning($"No layout found for body: {body.Id}");
            return;
        }

        Node3D? bodyNode = _systemBodyNodeScene.Instantiate() as Node3D;
        if (bodyNode == null)
        {
            return;
        }

        if (bodyNode is SystemBodyNode typedBodyNode)
        {
            typedBodyNode.Setup(body, layout.DisplayRadius, layout.Position);
            typedBodyNode.BodySelected += OnBodyClicked;
        }
        else
        {
            bodyNode.Call("setup", body, layout.DisplayRadius, layout.Position);
            bodyNode.Connect("body_selected", Callable.From<string>(OnBodyClicked));
        }

        _bodiesContainer.AddChild(bodyNode);
        _bodyNodes[body.Id] = bodyNode;
    }

    /// <summary>
    /// Creates a major-asteroid node at a known display position.
    /// </summary>
    private void CreateMajorAsteroidNodeAt(CelestialBody asteroid, Vector3 displayPosition)
    {
        if (_systemBodyNodeScene == null || _bodiesContainer == null)
        {
            return;
        }

        float asteroidDisplayRadius = Mathf.Clamp(
            SystemDisplayLayout.CalculatePlanetDisplayRadius(asteroid.Physical.RadiusM) * 0.4f,
            0.08f,
            0.28f);

        Node3D? asteroidNode = _systemBodyNodeScene.Instantiate() as Node3D;
        if (asteroidNode == null)
        {
            return;
        }

        Node3D parentNode = _bodiesContainer;
        Vector3 localPosition = displayPosition;
        string beltId = GetMajorAsteroidBeltId(asteroid);
        if (_beltRenderer != null && !string.IsNullOrEmpty(beltId))
        {
            Node3D? beltRoot = _beltRenderer.GetNodeOrNull<Node3D>("Belt_" + beltId);
            if (beltRoot != null)
            {
                parentNode = beltRoot;
                localPosition = GetDisplayPositionRelativeToParent(beltRoot, displayPosition);
            }
        }

        if (asteroidNode is SystemBodyNode typedAsteroidNode)
        {
            typedAsteroidNode.Setup(asteroid, asteroidDisplayRadius, localPosition);
            typedAsteroidNode.BodySelected += OnBodyClicked;
        }
        else
        {
            asteroidNode.Call("setup", asteroid, asteroidDisplayRadius, localPosition);
            asteroidNode.Connect("body_selected", Callable.From<string>(OnBodyClicked));
        }

        parentNode.AddChild(asteroidNode);
    }

    /// <summary>
    /// Converts a display-space point into the chosen parent node's local space without
    /// touching global transforms before the node is inside the scene tree.
    /// </summary>
    private static Vector3 GetDisplayPositionRelativeToParent(Node3D parentNode, Vector3 displayPosition)
    {
        if (parentNode.IsInsideTree())
        {
            return parentNode.ToLocal(displayPosition);
        }

        return displayPosition - parentNode.Position;
    }

    /// <summary>
    /// Returns the belt identifier for a major asteroid.
    /// </summary>
    private string GetMajorAsteroidBeltId(CelestialBody asteroid)
    {
        if (_currentSystem == null)
        {
            return string.Empty;
        }

        foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
        {
            if (belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                return belt.Id;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Maps a major asteroid orbit into display coordinates.
    /// </summary>
    private Vector3 GetMajorAsteroidDisplayPosition(CelestialBody asteroid)
    {
        if (_currentSystem == null || _currentLayout == null || !asteroid.HasOrbital() || asteroid.Orbital == null)
        {
            return InvalidPosition;
        }

        AsteroidBelt? matchingBelt = null;
        foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
        {
            if (belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                matchingBelt = belt;
                break;
            }
        }

        if (matchingBelt == null)
        {
            return InvalidPosition;
        }

        BeltLayout? beltLayout = _currentLayout.GetBeltLayout(matchingBelt.Id);
        if (beltLayout == null)
        {
            return InvalidPosition;
        }

        float asteroidAu = (float)(asteroid.Orbital.SemiMajorAxisM / Units.AuMeters);
        float bandAu = Mathf.Max(0.001f, beltLayout.OuterAu - beltLayout.InnerAu);
        float radialT = Mathf.Clamp((asteroidAu - beltLayout.InnerAu) / bandAu, 0.0f, 1.0f);
        float displayRadius = Mathf.Lerp(beltLayout.InnerDisplayRadius, beltLayout.OuterDisplayRadius, radialT);
        float angle = Mathf.DegToRad((float)asteroid.Orbital.MeanAnomalyDeg);
        float inclination = Mathf.Clamp(Mathf.Abs((float)asteroid.Orbital.InclinationDeg), 0.0f, beltLayout.MaxInclinationDeg);
        float yOffset = Mathf.Sin(Mathf.DegToRad(inclination)) * Mathf.Sin(angle) * displayRadius;

        return beltLayout.HostCenter + new Vector3(
            Mathf.Cos(angle) * displayRadius,
            yOffset,
            Mathf.Sin(angle) * displayRadius);
    }

    /// <summary>
    /// Creates rendered belt visuals.
    /// </summary>
    private void CreateBeltVisualizations()
    {
        if (_currentSystem == null || _currentLayout == null || _beltRenderer == null)
        {
            return;
        }

        int baseSeed;
        if (_currentSystem.Provenance != null)
        {
            baseSeed = (int)_currentSystem.Provenance.GenerationSeed;
        }
        else
        {
            baseSeed = 0;
        }
        if (_beltRenderer is BeltRenderer typedBeltRenderer)
        {
            typedBeltRenderer.RenderBelts(_currentSystem, _currentLayout, baseSeed);
        }
        else if (_beltRenderer.HasMethod("render_belts"))
        {
            _beltRenderer.Call("render_belts", _currentSystem, _currentLayout, baseSeed);
        }
    }

    /// <summary>
    /// Creates orbit path visualizations.
    /// </summary>
    private void CreateOrbitVisualizations()
    {
        if (_currentSystem == null || _currentLayout == null || _orbitRenderer == null)
        {
            return;
        }

        foreach (CelestialBody planet in _currentSystem.GetPlanets())
        {
            BodyLayout? bodyLayout = _currentLayout.GetBodyLayout(planet.Id);
            if (bodyLayout != null && bodyLayout.OrbitRadius > 0.0f)
            {
                CreateCircleOrbit(planet.Id, bodyLayout.OrbitCenter, bodyLayout.OrbitRadius, (int)planet.Type, bodyLayout.OrbitParentId);
            }
        }

        foreach (CelestialBody star in _currentSystem.GetStars())
        {
            BodyLayout? starOrbit = _currentLayout.GetStarOrbit(star.Id);
            if (starOrbit != null && starOrbit.IsOrbiting && starOrbit.OrbitRadius > 0.0f)
            {
                CreateCircleOrbit(star.Id + "_orbit", starOrbit.OrbitCenter, starOrbit.OrbitRadius, (int)CelestialType.Type.Star, starOrbit.OrbitParentId);
            }
        }

        CreateBeltEdgeOrbits();
    }

    /// <summary>
    /// Creates a circular orbit visualization.
    /// </summary>
    private void CreateCircleOrbit(string orbitId, Vector3 center, float radius, int bodyType, string parentId)
    {
        if (_orbitRenderer == null)
        {
            return;
        }

        Vector3[] points = new Vector3[65];
        for (int index = 0; index < points.Length; index += 1)
        {
            float angle = ((float)index / (points.Length - 1)) * Mathf.Tau;
            points[index] = center + new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius);
        }

        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.AddOrbit(orbitId, points, bodyType, parentId, center);
        }
        else
        {
            _orbitRenderer.Call("add_orbit", orbitId, points, bodyType, parentId, center);
        }
    }

    /// <summary>
    /// Creates inner and outer edge orbit lines for each belt.
    /// </summary>
    private void CreateBeltEdgeOrbits()
    {
        if (_currentLayout == null)
        {
            return;
        }

        foreach (BeltLayout beltLayout in _currentLayout.GetAllBelts())
        {
            CreateCircleOrbit(beltLayout.BeltId + "_inner_edge", beltLayout.HostCenter, beltLayout.InnerDisplayRadius, (int)CelestialType.Type.Asteroid, beltLayout.HostId);
            CreateCircleOrbit(beltLayout.BeltId + "_outer_edge", beltLayout.HostCenter, beltLayout.OuterDisplayRadius, (int)CelestialType.Type.Asteroid, beltLayout.HostId);
        }
    }

    /// <summary>
    /// Creates zone visualizations when enabled.
    /// Stubbed pending the System Viewer Rendering effort; the call site is live
    /// so the method must remain to avoid a missing-member error.
    /// </summary>
    private void CreateZoneVisualizations()
    {
        // Intentionally empty — zone visualization deferred to the rendering effort.
    }

    /// <summary>
    /// Clears body nodes while preserving the belt renderer.
    /// </summary>
    private void ClearBodies()
    {
        _bodyNodes.Clear();
        if (_bodiesContainer == null)
        {
            return;
        }

        foreach (Node child in _bodiesContainer.GetChildren())
        {
            if (_beltRenderer != null && child == _beltRenderer)
            {
                continue;
            }

            child.QueueFree();
        }
    }

    /// <summary>
    /// Clears orbit visuals.
    /// </summary>
    private void ClearOrbits()
    {
        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.Clear();
        }
        else
        {
            _orbitRenderer?.Call("clear");
        }
    }

    /// <summary>
    /// Clears belt visuals.
    /// </summary>
    private void ClearBelts()
    {
        if (_beltRenderer is BeltRenderer typedBeltRenderer)
        {
            typedBeltRenderer.Clear();
        }
        else if (_beltRenderer != null && _beltRenderer.HasMethod("clear"))
        {
            _beltRenderer.Call("clear");
        }
    }

    /// <summary>
    /// Clears zone visuals.
    /// </summary>
    private void ClearZones()
    {
        if (_zonesContainer == null)
        {
            return;
        }

        foreach (Node child in _zonesContainer.GetChildren())
        {
            child.QueueFree();
        }
    }
}
