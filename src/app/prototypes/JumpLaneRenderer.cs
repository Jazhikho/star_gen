using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.Prototypes;

/// <summary>
/// Renders jump lane systems and connections in 3D.
/// </summary>
public partial class JumpLaneRenderer : Node3D
{
    /// <summary>Scale factor: 1 parsec = this many units in 3D space.</summary>
    public const float ParsecScale = 1.0f;

    private const float MarkerSizeBase = 0.3f;
    private const float MarkerSizeOrphan = 0.4f;
    private const float LineRadius = 0.05f;

    private Node3D? _systemsContainer;
    private Node3D? _connectionsContainer;

    public override void _Ready()
    {
        _systemsContainer = new Node3D { Name = "Systems" };
        AddChild(_systemsContainer);
        _connectionsContainer = new Node3D { Name = "Connections" };
        AddChild(_connectionsContainer);
    }

    /// <summary>Clears all rendered elements.</summary>
    public void Clear()
    {
        if (_systemsContainer != null)
        {
            foreach (Node child in _systemsContainer.GetChildren())
                child.QueueFree();
        }
        if (_connectionsContainer != null)
        {
            foreach (Node child in _connectionsContainer.GetChildren())
                child.QueueFree();
        }
    }

    /// <summary>Renders a complete jump lane result.</summary>
    public void Render(JumpLaneRegion region, JumpLaneResult result)
    {
        Clear();
        RenderConnections(result);
        RenderSystems(region, result);
    }

    private void RenderSystems(JumpLaneRegion region, JumpLaneResult result)
    {
        if (_systemsContainer == null)
            return;
        foreach (JumpLaneSystem system in region.Systems)
        {
            MeshInstance3D? marker = CreateSystemMarker(system, result);
            if (marker != null)
                _systemsContainer.AddChild(marker);
        }
    }

    private MeshInstance3D CreateSystemMarker(JumpLaneSystem system, JumpLaneResult result)
    {
        var marker = new MeshInstance3D { Name = system.Id };
        var sphere = new SphereMesh();
        bool isOrphan = result.IsOrphan(system.Id);
        if (isOrphan)
            sphere.Radius = MarkerSizeOrphan;
        else
            sphere.Radius = MarkerSizeBase;
        sphere.Height = sphere.Radius * 2;
        marker.Mesh = sphere;
        marker.MaterialOverride = CreateSystemMaterial(system, result);
        marker.Position = system.Position * ParsecScale;
        return marker;
    }

    private StandardMaterial3D CreateSystemMaterial(JumpLaneSystem system, JumpLaneResult result)
    {
        var material = new StandardMaterial3D();
        material.AlbedoColor = GetSystemColor(system, result);
        material.EmissionEnabled = true;
        material.Emission = material.AlbedoColor;
        material.EmissionEnergyMultiplier = 0.5f;
        return material;
    }

    private Color GetSystemColor(JumpLaneSystem system, JumpLaneResult result)
    {
        if (result.IsOrphan(system.Id))
            return Colors.Red;
        if (system.IsBridge)
            return Colors.Cyan;
        if (!system.IsPopulated())
            return Colors.Gray;
        float popFactor = Math.Clamp(system.Population / 100000.0f, 0.2f, 1.0f);
        return new Color(popFactor, popFactor, 1.0f);
    }

    private void RenderConnections(JumpLaneResult result)
    {
        if (_connectionsContainer == null)
            return;
        foreach (JumpLaneConnection conn in result.Connections)
        {
            Node3D? line = CreateConnectionLine(conn, result);
            if (line != null)
                _connectionsContainer.AddChild(line);
        }
    }

    private Node3D? CreateConnectionLine(JumpLaneConnection conn, JumpLaneResult result)
    {
        JumpLaneSystem? source = result.GetSystem(conn.SourceId);
        JumpLaneSystem? dest = result.GetSystem(conn.DestinationId);
        if (source == null || dest == null)
        {
            return null;
        }
        Vector3 startPos = source.Position * ParsecScale;
        Vector3 endPos = dest.Position * ParsecScale;
        return CreateLineBetweenPoints(startPos, endPos, GetConnectionColor(conn.Type));
    }

    private Node3D? CreateLineBetweenPoints(Vector3 start, Vector3 end, Color color)
    {
        float length = start.DistanceTo(end);
        if (length < 0.001f)
            return null;

        var meshInstance = new MeshInstance3D();
        var cylinder = new CylinderMesh();
        cylinder.TopRadius = LineRadius;
        cylinder.BottomRadius = LineRadius;
        cylinder.Height = length;
        meshInstance.Mesh = cylinder;
        meshInstance.MaterialOverride = CreateLineMaterial(color);
        Vector3 midpoint = (start + end) / 2.0f;
        meshInstance.Position = midpoint;
        Vector3 direction = (end - start).Normalized();
        meshInstance.Basis = CreateBasisFromDirection(direction);
        return meshInstance;
    }

    private Basis CreateBasisFromDirection(Vector3 direction)
    {
        Vector3 up = Vector3.Up;
        if (Math.Abs(direction.Dot(up)) > 0.99f)
            up = Vector3.Forward;
        Vector3 xAxis = up.Cross(direction).Normalized();
        Vector3 zAxis = direction.Cross(xAxis).Normalized();
        return new Basis(xAxis, direction, zAxis);
    }

    private StandardMaterial3D CreateLineMaterial(Color color)
    {
        var material = new StandardMaterial3D();
        material.AlbedoColor = color;
        material.EmissionEnabled = true;
        material.Emission = color;
        material.EmissionEnergyMultiplier = 0.8f;
        return material;
    }

    /// <summary>
    /// Returns the display color for a connection type.
    /// Kept in the app layer because color is a rendering concern, not domain logic.
    /// </summary>
    private static Color GetConnectionColor(JumpLaneConnection.ConnectionType connectionType)
    {
        if (connectionType == JumpLaneConnection.ConnectionType.Green)
        {
            return Colors.Green;
        }

        if (connectionType == JumpLaneConnection.ConnectionType.Yellow)
        {
            return Colors.Yellow;
        }

        if (connectionType == JumpLaneConnection.ConnectionType.Orange)
        {
            return Colors.Orange;
        }

        if (connectionType == JumpLaneConnection.ConnectionType.Red)
        {
            return Colors.Red;
        }

        return Colors.White;
    }
}
