using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders quadrant cells as a density-colored multimesh.
/// </summary>
public partial class QuadrantRenderer : MultiMeshInstance3D
{
	private const float CellFillFraction = 0.7f;
	private const float DensityVisibilityThreshold = 0.01f;

	private static readonly Color ColorLow = new(0.1f, 0.2f, 0.8f);
	private static readonly Color ColorMid = new(0.1f, 0.8f, 0.8f);
	private static readonly Color ColorHigh = new(1.0f, 0.85f, 0.3f);
	private static readonly Color HighlightColor = new(0.5f, 0.8f, 1.0f, 0.4f);

	private readonly Dictionary<string, int> _coordsToIndex = new();
	private Color[] _originalColors = System.Array.Empty<Color>();
	private Vector3I[] _occupiedCoords = System.Array.Empty<Vector3I>();
	private Vector3I? _highlightedCoords;

	/// <summary>
	/// Returns occupied quadrant coordinates.
	/// </summary>
	public Array<Vector3I> get_occupied_coords()
	{
		Array<Vector3I> coords = new();
		for (int i = 0; i < _occupiedCoords.Length; i++)
		{
			coords.Add(_occupiedCoords[i]);
		}

		return coords;
	}

	/// <summary>
	/// Builds the quadrant multimesh from a density model.
	/// </summary>
	public void build_from_density(Variant specVariant, Variant densityModelVariant)
	{
		GalaxySpec? spec = specVariant.As<GalaxySpec>();
		DensityModelInterface? densityModel = densityModelVariant.As<DensityModelInterface>();
		if (spec == null || densityModel == null)
		{
			return;
		}

		BuildFromDensity(spec, densityModel);
	}

	/// <summary>
	/// Builds the quadrant multimesh from a density model.
	/// </summary>
	public void BuildFromDensity(GalaxySpec spec, DensityModelInterface densityModel)
	{
		Vector3I gridMin = GalaxyCoordinates.GetQuadrantGridMin(spec);
		Vector3I gridMax = GalaxyCoordinates.GetQuadrantGridMax(spec);

		System.Collections.Generic.List<Vector3> positions = new();
		System.Collections.Generic.List<float> densities = new();
		System.Collections.Generic.List<Vector3I> coords = new();
		float maxDensity = 0.0f;

		for (int qx = gridMin.X; qx <= gridMax.X; qx++)
		{
			for (int qy = gridMin.Y; qy <= gridMax.Y; qy++)
			{
				for (int qz = gridMin.Z; qz <= gridMax.Z; qz++)
				{
					Vector3I cellCoords = new(qx, qy, qz);
					Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(cellCoords);
					float density = densityModel.GetDensity(center);
					positions.Add(center);
					densities.Add(density);
					coords.Add(cellCoords);
					if (density > maxDensity)
					{
						maxDensity = density;
					}
				}
			}
		}

		if (maxDensity <= 0.0f)
		{
			Multimesh = null;
			_coordsToIndex.Clear();
			_originalColors = System.Array.Empty<Color>();
			_occupiedCoords = System.Array.Empty<Vector3I>();
			return;
		}

		System.Collections.Generic.List<Vector3> filteredPositions = new();
		System.Collections.Generic.List<Color> filteredColors = new();
		System.Collections.Generic.List<Vector3I> filteredCoords = new();

		for (int i = 0; i < positions.Count; i++)
		{
			float normalized = densities[i] / maxDensity;
			if (normalized < DensityVisibilityThreshold)
			{
				continue;
			}

			filteredPositions.Add(positions[i]);
			filteredColors.Add(DensityToColor(normalized));
			filteredCoords.Add(coords[i]);
		}

		BuildMultiMesh(filteredPositions, filteredColors);
		BuildCoordIndex(filteredCoords);
		_originalColors = filteredColors.ToArray();
		_occupiedCoords = filteredCoords.ToArray();
		_highlightedCoords = null;
	}

	/// <summary>
	/// Highlights a single quadrant cell, or clears the current highlight.
	/// </summary>
	public void set_highlight(Variant coordsVariant)
	{
		SetHighlight(coordsVariant);
	}

	/// <summary>
	/// Highlights a single quadrant cell, or clears the current highlight.
	/// </summary>
	public void SetHighlight(Variant coordsVariant)
	{
		if (_highlightedCoords.HasValue && Multimesh != null)
		{
			string prevKey = CoordsKey(_highlightedCoords.Value);
			if (_coordsToIndex.TryGetValue(prevKey, out int prevIndex) && prevIndex < _originalColors.Length)
			{
				Multimesh.SetInstanceColor(prevIndex, _originalColors[prevIndex]);
			}
		}

		_highlightedCoords = null;
		if (coordsVariant.VariantType != Variant.Type.Vector3I || Multimesh == null)
		{
			return;
		}

		Vector3I coords = (Vector3I)coordsVariant;
		_highlightedCoords = coords;
		if (_coordsToIndex.TryGetValue(CoordsKey(coords), out int index))
		{
			Multimesh.SetInstanceColor(index, HighlightColor);
		}
	}

	private static Color DensityToColor(float normalizedDensity)
	{
		float t = Mathf.Clamp(normalizedDensity, 0.0f, 1.0f);
		float alpha = Mathf.Lerp(0.03f, 0.25f, t);
		Color rgb = t < 0.5f
			? ColorLow.Lerp(ColorMid, t / 0.5f)
			: ColorMid.Lerp(ColorHigh, (t - 0.5f) / 0.5f);
		return new Color(rgb.R, rgb.G, rgb.B, alpha);
	}

	private void BuildMultiMesh(
		System.Collections.Generic.List<Vector3> positions,
		System.Collections.Generic.List<Color> colors)
	{
		if (positions.Count == 0)
		{
			Multimesh = null;
			return;
		}

		float cellSize = (float)GalaxyCoordinates.QuadrantSizePc * CellFillFraction;
		BoxMesh box = new()
		{
			Size = Vector3.One,
		};

		ShaderMaterial material = new()
		{
			Shader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/quadrant_cell.gdshader"),
		};
		box.Material = material;

		MultiMesh multiMesh = new()
		{
			Mesh = box,
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
			UseColors = true,
			InstanceCount = positions.Count,
		};

		Basis scaleBasis = Basis.Identity.Scaled(new Vector3(cellSize, cellSize, cellSize));
		for (int i = 0; i < positions.Count; i++)
		{
			multiMesh.SetInstanceTransform(i, new Transform3D(scaleBasis, positions[i]));
			multiMesh.SetInstanceColor(i, colors[i]);
		}

		Multimesh = multiMesh;
	}

	private void BuildCoordIndex(System.Collections.Generic.List<Vector3I> coords)
	{
		_coordsToIndex.Clear();
		for (int i = 0; i < coords.Count; i++)
		{
			_coordsToIndex[CoordsKey(coords[i])] = i;
		}
	}

	private static string CoordsKey(Vector3I coords)
	{
		return $"{coords.X},{coords.Y},{coords.Z}";
	}
}
