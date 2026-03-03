using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders a subsector neighborhood with billboard stars and wireframe cells.
/// </summary>
public partial class NeighborhoodRenderer : Node3D
{
	private const float StarSize = 1.5f;
	private const float FadeNear = 25.0f;
	private const float FadeFar = 45.0f;
	private const float PickRadius = 2.0f;
	private const float TransitionDuration = 0.5f;

	private static readonly float[] ShellAlphas = { 1.0f, 0.85f, 0.65f, 0.4f, 0.2f, 0.08f };
	private static readonly float[] WireShellAlphas = { 0.2f, 0.15f, 0.10f, 0.06f, 0.03f, 0.01f };
	private static readonly Color WireBaseColor = new(0.3f, 0.5f, 0.9f);
	private static readonly Color WireCenterColor = new(0.4f, 0.7f, 1.0f);

	private MultiMeshInstance3D? _starMeshInstance;
	private ShaderMaterial? _starMaterial;
	private readonly List<MeshInstance3D> _wireframeNodes = new();
	private SubSectorNeighborhoodData? _neighborhoodData;
	private ArrayMesh? _wireframeMesh;
	private Shader? _wireShader;

	private readonly List<Dictionary> _transitioningWires = new();
	private float _transitionElapsed;
	private bool _isTransitioning;

	/// <summary>
	/// Builds persistent child nodes on ready.
	/// </summary>
	public override void _Ready()
	{
		_wireShader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/subsector_wire.gdshader");
		_wireframeMesh = CreateWireframeBox((float)GalaxyCoordinates.SubsectorSizePc);
		_starMeshInstance = new MultiMeshInstance3D
		{
			Name = "NeighborhoodStars",
		};
		AddChild(_starMeshInstance);
	}

	/// <summary>
	/// Advances fade transitions.
	/// </summary>
	public override void _Process(double delta)
	{
		float frameDelta = (float)delta;

		if (_isTransitioning)
		{
			_transitionElapsed += frameDelta;
			if (_transitionElapsed >= TransitionDuration)
			{
				_isTransitioning = false;
				FinalizeTransition();
			}
			else
			{
				UpdateTransition();
			}
		}

		UpdateWireTransitions(frameDelta);
	}

	/// <summary>
	/// Builds the full neighborhood for a camera position.
	/// </summary>
	public void build_neighborhood(Vector3 cameraPosition, long galaxySeed, Variant densityModelVariant, float referenceDensity)
	{
		DensityModelInterface? densityModel = densityModelVariant.As<DensityModelInterface>();
		if (densityModel == null)
		{
			return;
		}

		BuildNeighborhood(cameraPosition, galaxySeed, densityModel, referenceDensity);
	}

	/// <summary>
	/// Builds the full neighborhood for a camera position.
	/// </summary>
	public void BuildNeighborhood(Vector3 cameraPosition, long galaxySeed, DensityModelInterface densityModel, float referenceDensity)
	{
		_neighborhoodData = SubSectorNeighborhood.Build(cameraPosition, galaxySeed, densityModel, referenceDensity);
		RebuildStars();
		RebuildWireframes();
		_transitionElapsed = 0.0f;
		_isTransitioning = true;
	}

	/// <summary>
	/// Returns the cached neighborhood data.
	/// </summary>
	public SubSectorNeighborhoodData? get_neighborhood_data()
	{
		return _neighborhoodData;
	}

	/// <summary>
	/// Performs a ray pick against neighborhood stars.
	/// </summary>
	public Variant pick_star(Vector3 rayOrigin, Vector3 rayDirection)
	{
		if (_neighborhoodData == null || _neighborhoodData.GetStarCount() == 0)
		{
			return default;
		}

		StarPickResult? result = StarPicker.PickNearestToRay(
			rayOrigin,
			rayDirection,
			_neighborhoodData.StarPositions,
			_neighborhoodData.StarSeeds,
			PickRadius);

		return result == null ? default : Variant.CreateFrom(result);
	}

	/// <summary>
	/// Returns the shell alpha multiplier for a shell index.
	/// </summary>
	public static float get_shell_alpha(int shell)
	{
		return GetShellAlpha(shell);
	}

	/// <summary>
	/// Returns the shell alpha multiplier for a shell index.
	/// </summary>
	public static float GetShellAlpha(int shell)
	{
		return shell >= 0 && shell < ShellAlphas.Length ? ShellAlphas[shell] : 0.1f;
	}

	private void RebuildStars()
	{
		if (_starMeshInstance == null || _neighborhoodData == null)
		{
			return;
		}

		int count = _neighborhoodData.GetStarCount();
		if (count == 0)
		{
			_starMeshInstance.Multimesh = null;
			return;
		}

		QuadMesh quad = new()
		{
			Size = Vector2.One,
		};

		_starMaterial = new ShaderMaterial
		{
			Shader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/star_sector_view.gdshader"),
		};
		_starMaterial.SetShaderParameter("fade_near", FadeNear);
		_starMaterial.SetShaderParameter("fade_far", FadeFar);
		quad.Material = _starMaterial;

		MultiMesh multiMesh = new()
		{
			Mesh = quad,
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
			UseColors = true,
			UseCustomData = true,
			InstanceCount = count,
		};

		Basis scaleBasis = Basis.Identity.Scaled(new Vector3(StarSize, StarSize, StarSize));
		for (int i = 0; i < count; i++)
		{
			multiMesh.SetInstanceTransform(i, new Transform3D(scaleBasis, _neighborhoodData.StarPositions[i]));
			multiMesh.SetInstanceColor(i, ColorFromSeed(_neighborhoodData.StarSeeds[i]));
			float shellAlpha = GetShellAlpha(_neighborhoodData.StarShells[i]);
			multiMesh.SetInstanceCustomData(i, new Color(shellAlpha, 0.0f, 0.0f));
		}

		_starMeshInstance.Multimesh = multiMesh;
	}

	private void RebuildWireframes()
	{
		if (_neighborhoodData == null || _wireframeMesh == null || _wireShader == null)
		{
			return;
		}

		for (int i = 0; i < _wireframeNodes.Count; i++)
		{
			_wireframeNodes[i].QueueFree();
		}

		_wireframeNodes.Clear();
		_transitioningWires.Clear();

		Vector3 center = _neighborhoodData.CenterOrigin;
		float halfSize = (float)GalaxyCoordinates.SubsectorSizePc * 0.5f;

		for (int i = 0; i < _neighborhoodData.SubsectorOrigins.Length; i++)
		{
			Vector3 origin = _neighborhoodData.SubsectorOrigins[i];
			int shell = _neighborhoodData.SubsectorShells[i];
			bool isCenter = origin.IsEqualApprox(center);
			Color baseRgb = isCenter ? WireCenterColor : WireBaseColor;
			float wireAlpha = WireShellAlphas[Mathf.Min(shell, WireShellAlphas.Length - 1)];
			Color wireColor = new(baseRgb.R, baseRgb.G, baseRgb.B, wireAlpha);

			MeshInstance3D wireNode = new()
			{
				Mesh = _wireframeMesh,
				Position = origin + (Vector3.One * halfSize),
			};

			ShaderMaterial material = new()
			{
				Shader = _wireShader,
			};
			material.SetShaderParameter("wire_color", wireColor);
			wireNode.MaterialOverride = material;

			AddChild(wireNode);
			_wireframeNodes.Add(wireNode);

			if (shell == SubSectorNeighborhood.Extent)
			{
				Dictionary transition = new()
				{
					["material"] = material,
					["target_alpha"] = wireAlpha,
					["elapsed"] = 0.0f,
					["color_rgb"] = baseRgb,
				};
				_transitioningWires.Add(transition);
				material.SetShaderParameter("wire_color", new Color(baseRgb.R, baseRgb.G, baseRgb.B, 0.0f));
			}
		}
	}

	private void UpdateWireTransitions(float delta)
	{
		List<int> completed = new();

		for (int i = 0; i < _transitioningWires.Count; i++)
		{
			Dictionary transition = _transitioningWires[i];
			float elapsed = (float)transition["elapsed"] + delta;
			transition["elapsed"] = elapsed;

			float t = Mathf.Clamp(elapsed / TransitionDuration, 0.0f, 1.0f);
			float targetAlpha = (float)transition["target_alpha"];
			Color rgb = (Color)transition["color_rgb"];
			float currentAlpha = Mathf.Lerp(0.0f, targetAlpha, t);
			((ShaderMaterial)transition["material"]).SetShaderParameter(
				"wire_color",
				new Color(rgb.R, rgb.G, rgb.B, currentAlpha));

			if (t >= 1.0f)
			{
				completed.Add(i);
			}
		}

		for (int i = completed.Count - 1; i >= 0; i--)
		{
			_transitioningWires.RemoveAt(completed[i]);
		}
	}

	private void UpdateTransition()
	{
		_starMaterial?.SetShaderParameter("global_alpha", Mathf.Lerp(0.5f, 1.0f, Mathf.Clamp(_transitionElapsed / TransitionDuration, 0.0f, 1.0f)));
	}

	private void FinalizeTransition()
	{
		_starMaterial?.SetShaderParameter("global_alpha", 1.0f);
	}

	private static ArrayMesh CreateWireframeBox(float boxSize)
	{
		float half = boxSize * 0.5f;
		Vector3[] corners =
		{
			new(-half, -half, -half),
			new(half, -half, -half),
			new(half, half, -half),
			new(-half, half, -half),
			new(-half, -half, half),
			new(half, -half, half),
			new(half, half, half),
			new(-half, half, half),
		};

		int[] edgeIndices =
		{
			0, 1, 1, 2, 2, 3, 3, 0,
			4, 5, 5, 6, 6, 7, 7, 4,
			0, 4, 1, 5, 2, 6, 3, 7,
		};

		Vector3[] vertices = new Vector3[edgeIndices.Length];
		for (int i = 0; i < edgeIndices.Length; i++)
		{
			vertices[i] = corners[edgeIndices[i]];
		}

		Godot.Collections.Array arrays = new();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices;

		ArrayMesh mesh = new();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);
		return mesh;
	}

	private static Color ColorFromSeed(long starSeed)
	{
		int hash = (int)StableHash.HashIntegers(new Array<long> { starSeed });
		float t = (hash & 0xFFFF) / 65535.0f;
		float weightedT = t * t;

		if (weightedT < 0.1f)
		{
			return new Color(0.7f, 0.8f, 1.0f, 1.0f);
		}

		if (weightedT < 0.3f)
		{
			return new Color(0.95f, 0.95f, 1.0f, 1.0f);
		}

		if (weightedT < 0.7f)
		{
			return new Color(1.0f, 0.95f, 0.8f, 1.0f);
		}

		if (weightedT < 0.9f)
		{
			return new Color(1.0f, 0.8f, 0.5f, 1.0f);
		}

		return new Color(1.0f, 0.5f, 0.3f, 0.9f);
	}
}
