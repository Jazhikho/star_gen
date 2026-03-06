using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.AsteroidFields;
using StarGen.Domain.Math;
using System.Collections.Generic;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Renders asteroid-belt discs and sampled asteroid fields.
/// </summary>
public partial class BeltRenderer : Node3D
{
    /// <summary>
    /// Emitted when a belt's click region is selected.
    /// </summary>
    [Signal]
    public delegate void BeltClickedEventHandler(string beltId);

    private const float BeltRotationSpeedRadPerSec = 0.15f;
    private readonly Dictionary<string, Node3D> _beltRoots = new();
    private readonly Dictionary<string, string> _beltHostIds = new();

    /// <summary>
    /// Clears all rendered belt visuals.
    /// </summary>
    public void Clear()
    {
        foreach (Node3D root in _beltRoots.Values)
        {
            root.QueueFree();
        }

        _beltRoots.Clear();
        _beltHostIds.Clear();
    }

    /// <summary>
    /// Renders all belts for the current system layout.
    /// </summary>
    public void RenderBelts(SolarSystem? system, SystemLayout? layout, int baseSeed)
    {
        Clear();
        if (system == null || layout == null)
        {
            return;
        }

        foreach (AsteroidBelt belt in system.AsteroidBelts)
        {
            BeltLayout? beltLayout = layout.GetBeltLayout(belt.Id);
            if (beltLayout == null)
            {
                continue;
            }

            Node3D root = CreateBeltRoot(beltLayout);
            RenderBeltTorus(beltLayout, root);
            RenderBeltClickArea(beltLayout, root);
            RenderBeltBackground(system, belt, beltLayout, baseSeed, root);
        }
    }

    /// <summary>
    /// Updates belt root positions when hosts move.
    /// </summary>
    public void UpdateBeltPositions(Godot.Collections.Dictionary hostPositions)
    {
        foreach (KeyValuePair<string, Node3D> pair in _beltRoots)
        {
            string hostId;
            if (_beltHostIds.TryGetValue(pair.Key, out string? value))
            {
                hostId = value;
            }
            else
            {
                hostId = string.Empty;
            }
            if (string.IsNullOrEmpty(hostId) || !hostPositions.ContainsKey(hostId))
            {
                continue;
            }

            Variant positionVariant = hostPositions[hostId];
            if (positionVariant.VariantType == Variant.Type.Vector3)
            {
                pair.Value.Position = (Vector3)positionVariant;
            }
        }
    }

    /// <summary>
    /// Rotates belt visuals around their local centers.
    /// </summary>
    public void UpdateBeltRotation(float delta)
    {
        float angleStep = BeltRotationSpeedRadPerSec * delta;
        foreach (Node3D root in _beltRoots.Values)
        {
            root.RotateY(-angleStep);
        }
    }

    private Node3D CreateBeltRoot(BeltLayout beltLayout)
    {
        Node3D root = new()
        {
            Name = "Belt_" + beltLayout.BeltId,
            Position = beltLayout.HostCenter,
        };

        AddChild(root);
        _beltRoots[beltLayout.BeltId] = root;
        _beltHostIds[beltLayout.BeltId] = beltLayout.HostId;
        return root;
    }

    private static void RenderBeltTorus(BeltLayout beltLayout, Node3D root)
    {
        TorusMesh torusMesh = new()
        {
            InnerRadius = beltLayout.InnerDisplayRadius,
            OuterRadius = beltLayout.OuterDisplayRadius,
            Rings = 48,
            RingSegments = 16,
        };

        MeshInstance3D meshInstance = new()
        {
            Mesh = torusMesh,
            Scale = new Vector3(1.0f, 0.12f, 1.0f),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.55f, 0.62f, 0.78f, 0.25f),
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            },
        };

        root.AddChild(meshInstance);
    }

    private void RenderBeltClickArea(BeltLayout beltLayout, Node3D root)
    {
        Area3D clickArea = new()
        {
            Name = "BeltClickArea",
            InputRayPickable = true,
        };

        CylinderShape3D cylinder = new()
        {
            Radius = beltLayout.OuterDisplayRadius,
            Height = 0.5f,
        };

        CollisionShape3D collision = new()
        {
            Shape = cylinder,
        };

        clickArea.AddChild(collision);
        root.AddChild(clickArea);

        float innerRadius = beltLayout.InnerDisplayRadius;
        string beltId = beltLayout.BeltId;
        clickArea.InputEvent += (_camera, @event, position, _normal, _shapeIndex) =>
        {
            if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            {
                return;
            }

            Vector3 localPosition = position - root.GlobalPosition;
            float distance = new Vector2(localPosition.X, localPosition.Z).Length();
            if (distance >= innerRadius)
            {
                EmitSignal(SignalName.BeltClicked, beltId);
            }
        };
    }

    private static void RenderBeltBackground(
        SolarSystem system,
        AsteroidBelt belt,
        BeltLayout beltLayout,
        int baseSeed,
        Node3D root)
    {
        BeltFieldSpec spec = BuildFieldSpec(system, belt, beltLayout);
        SeededRng rng = new(DeriveBeltSeed(baseSeed, belt.Id));
        BeltFieldData data = BeltFieldGenerator.GenerateField(spec, rng);
        Godot.Collections.Array<BeltAsteroidData> background = data.GetBackgroundAsteroids();
        if (background.Count == 0)
        {
            GD.PushWarning($"BeltRenderer: No background asteroids generated for belt {belt.Id}");
            return;
        }

        SphereMesh mesh = new()
        {
            Radius = 1.0f,
            Height = 2.0f,
            RadialSegments = 5,
            Rings = 4,
        };

        MultiMesh multiMesh = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            InstanceCount = background.Count,
            Mesh = mesh,
        };

        float minLog = Mathf.Log(Mathf.Max((float)spec.MinBodyRadiusKm, 0.01f));
        float maxLog = Mathf.Log(Mathf.Max((float)spec.MaxBodyRadiusKm, 1.0f));
        float logRange = Mathf.Max(maxLog - minLog, 0.001f);
        float bandAu = Mathf.Max(0.001f, beltLayout.OuterAu - beltLayout.InnerAu);

        for (int index = 0; index < background.Count; index += 1)
        {
            BeltAsteroidData asteroid = background[index];
            Vector3 displayPosition = MapAuPositionToDisplay(asteroid.PositionAu, beltLayout, bandAu);
            float sizeT = Mathf.Clamp((Mathf.Log(Mathf.Max((float)asteroid.BodyRadiusKm, 0.01f)) - minLog) / logRange, 0.0f, 1.0f);
            float visualRadius = Mathf.Lerp(0.10f, 0.30f, sizeT);
            multiMesh.SetInstanceTransform(index, new Transform3D(Basis.Identity.Scaled(Vector3.One * visualRadius), displayPosition));
        }

        MultiMeshInstance3D meshInstance = new()
        {
            Multimesh = multiMesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.7f, 0.65f, 0.55f),
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
            },
        };

        root.AddChild(meshInstance);
    }

    private static BeltFieldSpec BuildFieldSpec(SolarSystem system, AsteroidBelt belt, BeltLayout beltLayout)
    {
        BeltFieldSpec spec = new()
        {
            InnerRadiusAu = beltLayout.InnerAu,
            OuterRadiusAu = beltLayout.OuterAu,
            MaxInclinationDeg = beltLayout.MaxInclinationDeg,
            MaxEccentricity = 0.18,
            AsteroidCount = (int)Mathf.Clamp((float)(belt.GetWidthAu() * 450.0), 250.0f, 1500.0f),
            MinBodyRadiusKm = 0.25,
            MaxBodyRadiusKm = 180.0,
            RadialConcentration = 1.8,
            SizePowerLawExponent = 2.4,
        };

        spec.MajorAsteroidInputs = BuildMajorInputs(system, belt);
        return spec;
    }

    private static Godot.Collections.Array<BeltMajorAsteroidInput> BuildMajorInputs(SolarSystem system, AsteroidBelt belt)
    {
        Godot.Collections.Array<BeltMajorAsteroidInput> inputs = new();
        foreach (string asteroidId in belt.MajorAsteroidIds)
        {
            CelestialBody? body = system.GetBody(asteroidId);
            if (body == null || !body.HasOrbital() || body.Orbital == null || body.Physical == null)
            {
                continue;
            }

            inputs.Add(new BeltMajorAsteroidInput
            {
                BodyId = body.Id,
                SemiMajorAxisM = body.Orbital.SemiMajorAxisM,
                Eccentricity = body.Orbital.Eccentricity,
                InclinationDeg = body.Orbital.InclinationDeg,
                LongitudeAscendingNodeDeg = body.Orbital.LongitudeOfAscendingNodeDeg,
                ArgumentPeriapsisDeg = body.Orbital.ArgumentOfPeriapsisDeg,
                MeanAnomalyDeg = body.Orbital.MeanAnomalyDeg,
                BodyRadiusKm = body.Physical.RadiusM / 1000.0,
                AsteroidType = -1,
            });
        }

        return inputs;
    }

    private static Vector3 MapAuPositionToDisplay(Vector3 positionAu, BeltLayout beltLayout, float bandAu)
    {
        float radialAu = new Vector2(positionAu.X, positionAu.Z).Length();
        float angle = Mathf.Atan2(positionAu.Z, positionAu.X);
        float radialT = Mathf.Clamp((radialAu - beltLayout.InnerAu) / bandAu, 0.0f, 1.0f);
        float displayRadius = Mathf.Lerp(beltLayout.InnerDisplayRadius, beltLayout.OuterDisplayRadius, radialT);

        float yNorm = 0.0f;
        if (radialAu > 1.0e-6f)
        {
            yNorm = positionAu.Y / radialAu;
        }

        float maxYNorm = Mathf.Sin(Mathf.DegToRad(beltLayout.MaxInclinationDeg));
        yNorm = Mathf.Clamp(yNorm, -maxYNorm, maxYNorm);

        return new Vector3(
            Mathf.Cos(angle) * displayRadius,
            yNorm * displayRadius,
            Mathf.Sin(angle) * displayRadius);
    }

    private static int DeriveBeltSeed(int baseSeed, string beltId)
    {
        int hashValue = baseSeed;
        for (int index = 0; index < beltId.Length; index += 1)
        {
            hashValue = (int)((hashValue * 31L + beltId[index]) % 2147483647L);
        }

        return Mathf.Abs(hashValue);
    }
}
