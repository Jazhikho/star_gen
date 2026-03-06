using Godot;
using Godot.Collections;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders jump-lane connections for the neighborhood view.
/// </summary>
public partial class SectorJumpLaneRenderer : Node3D
{
	private static readonly Color ColorGreen = new(0.2f, 0.9f, 0.2f, 1.0f);
	private static readonly Color ColorYellow = new(0.9f, 0.9f, 0.1f, 1.0f);
	private static readonly Color ColorOrange = new(0.9f, 0.5f, 0.1f, 1.0f);
	private static readonly Color ColorRed = new(0.9f, 0.15f, 0.15f, 1.0f);

	private const float EmissionEnergy = 0.7f;

	private readonly Array<MeshInstance3D> _meshInstances = new();

	/// <summary>
	/// Clears all rendered connection meshes.
	/// </summary>
	public void clear()
	{
		Clear();
	}

	/// <summary>
	/// Clears all rendered connection meshes.
	/// </summary>
	public void Clear()
	{
		foreach (MeshInstance3D meshInstance in _meshInstances)
		{
			meshInstance.QueueFree();
		}

		_meshInstances.Clear();
	}

	/// <summary>
	/// Renders a jump-lane result payload.
	/// </summary>
	public void render(Variant resultVariant)
	{
		JumpLaneResult? result = resultVariant.As<JumpLaneResult>();
		Render(result);
	}

	/// <summary>
	/// Renders a jump-lane result payload.
	/// </summary>
	public void Render(JumpLaneResult? result)
	{
		Clear();
		if (result == null)
		{
			return;
		}

		System.Collections.Generic.Dictionary<JumpLaneConnection.ConnectionType, System.Collections.Generic.List<Vector3>> lines = new()
		{
			[JumpLaneConnection.ConnectionType.Green] = new System.Collections.Generic.List<Vector3>(),
			[JumpLaneConnection.ConnectionType.Yellow] = new System.Collections.Generic.List<Vector3>(),
			[JumpLaneConnection.ConnectionType.Orange] = new System.Collections.Generic.List<Vector3>(),
			[JumpLaneConnection.ConnectionType.Red] = new System.Collections.Generic.List<Vector3>(),
		};

		foreach (JumpLaneConnection connection in result.Connections)
		{
			JumpLaneSystem? source = result.GetSystem(connection.SourceId);
			JumpLaneSystem? destination = result.GetSystem(connection.DestinationId);

			if (source == null || destination == null)
			{
				GD.PushWarning($"SectorJumpLaneRenderer: missing system for connection {connection.SourceId} -> {connection.DestinationId}");
				continue;
			}

			if (!lines.TryGetValue(connection.Type, out System.Collections.Generic.List<Vector3>? vertices))
			{
				GD.PushWarning($"SectorJumpLaneRenderer: unknown connection type {(int)connection.Type}");
				continue;
			}

			vertices.Add(source.Position);
			vertices.Add(destination.Position);
		}

		foreach (System.Collections.Generic.KeyValuePair<JumpLaneConnection.ConnectionType, System.Collections.Generic.List<Vector3>> pair in lines)
		{
			if (pair.Value.Count < 2)
			{
				continue;
			}

			AddLineMesh(pair.Value.ToArray(), GetColor(pair.Key));
		}
	}

	/// <summary>
	/// Adds a single line-mesh batch for one connection type.
	/// </summary>
	private void AddLineMesh(Vector3[] vertices, Color color)
	{
		Godot.Collections.Array arrays = new();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices;

		ArrayMesh arrayMesh = new();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

		StandardMaterial3D material = new()
		{
			AlbedoColor = color,
			EmissionEnabled = true,
			Emission = color,
			EmissionEnergyMultiplier = EmissionEnergy,
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			NoDepthTest = false,
		};

		MeshInstance3D meshInstance = new()
		{
			Mesh = arrayMesh,
			MaterialOverride = material,
		};

		AddChild(meshInstance);
		_meshInstances.Add(meshInstance);
	}

	/// <summary>
	/// Returns the display color for a connection type.
	/// </summary>
	private static Color GetColor(JumpLaneConnection.ConnectionType connectionType)
	{
		return connectionType switch
		{
			JumpLaneConnection.ConnectionType.Green => ColorGreen,
			JumpLaneConnection.ConnectionType.Yellow => ColorYellow,
			JumpLaneConnection.ConnectionType.Orange => ColorOrange,
			JumpLaneConnection.ConnectionType.Red => ColorRed,
			_ => Colors.White,
		};
	}
}
