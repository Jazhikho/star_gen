using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders sector cells inside a selected quadrant.
/// </summary>
public partial class SectorRenderer : MultiMeshInstance3D
{
	private const int SectorsPerEdge = 10;
	private const float CellFillFraction = 0.7f;
	private const float DensityVisibilityThreshold = 0.005f;

	private static readonly Color ColorLow = new(0.2f, 0.1f, 0.6f);
	private static readonly Color ColorMid = new(0.2f, 0.7f, 0.9f);
	private static readonly Color ColorHigh = new(1.0f, 0.9f, 0.4f);
	private static readonly Color HighlightColor = new(0.5f, 0.8f, 1.0f, 0.35f);

	private readonly Dictionary<string, int> _coordsToIndex = new();
	private Color[] _originalColors = System.Array.Empty<Color>();
	private Vector3I[] _occupiedCoords = System.Array.Empty<Vector3I>();
	private Vector3I? _highlightedCoords;
	private Vector3I? _currentQuadrant;

	/// <summary>
	/// Returns occupied sector-local coordinates.
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
	/// Returns the quadrant this renderer represents.
	/// </summary>
	public Variant get_current_quadrant()
	{
		return _currentQuadrant.HasValue ? Variant.CreateFrom(_currentQuadrant.Value) : default;
	}

	/// <summary>
	/// Builds sector cells for a given quadrant.
	/// </summary>
	public void build_for_quadrant(Variant quadrantVariant, Variant densityModelVariant)
	{
		if (quadrantVariant.VariantType != Variant.Type.Vector3I)
		{
			return;
		}

		DensityModelInterface? densityModel = densityModelVariant.As<DensityModelInterface>();
		if (densityModel == null)
		{
			return;
		}

		BuildForQuadrant((Vector3I)quadrantVariant, densityModel);
	}

	/// <summary>
	/// Builds sector cells for a given quadrant.
	/// </summary>
	public void BuildForQuadrant(Vector3I quadrantCoords, DensityModelInterface densityModel)
	{
		_currentQuadrant = quadrantCoords;
		_highlightedCoords = null;
		_coordsToIndex.Clear();
		_occupiedCoords = System.Array.Empty<Vector3I>();

		Vector3 quadrantOrigin = GetQuadrantOrigin(quadrantCoords);
		float sectorSize = (float)GalaxyCoordinates.SectorSizePc;

		System.Collections.Generic.List<Vector3> positions = new();
		System.Collections.Generic.List<float> densities = new();
		System.Collections.Generic.List<Vector3I> coords = new();
		float maxDensity = 0.0f;

		for (int sx = 0; sx < SectorsPerEdge; sx++)
		{
			for (int sy = 0; sy < SectorsPerEdge; sy++)
			{
				for (int sz = 0; sz < SectorsPerEdge; sz++)
				{
					Vector3I localCoords = new(sx, sy, sz);
					Vector3 center = quadrantOrigin + new Vector3(
						((float)sx + 0.5f) * sectorSize,
						((float)sy + 0.5f) * sectorSize,
						((float)sz + 0.5f) * sectorSize);
					float density = densityModel.GetDensity(center);

					positions.Add(center);
					densities.Add(density);
					coords.Add(localCoords);
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
	}

	/// <summary>
	/// Returns the world-space center of a sector.
	/// </summary>
	public Vector3 get_sector_world_center(Vector3I sectorLocalCoords)
	{
		return GetSectorWorldCenter(sectorLocalCoords);
	}

	/// <summary>
	/// Returns the world-space center of a sector.
	/// </summary>
	public Vector3 GetSectorWorldCenter(Vector3I sectorLocalCoords)
	{
		if (!_currentQuadrant.HasValue)
		{
			return Vector3.Zero;
		}

		Vector3 quadrantOrigin = GetQuadrantOrigin(_currentQuadrant.Value);
		float sectorSize = (float)GalaxyCoordinates.SectorSizePc;
		return quadrantOrigin + new Vector3(
			((float)sectorLocalCoords.X + 0.5f) * sectorSize,
			((float)sectorLocalCoords.Y + 0.5f) * sectorSize,
			((float)sectorLocalCoords.Z + 0.5f) * sectorSize);
	}

	/// <summary>
	/// Returns the world-space AABB for a sector.
	/// </summary>
	public Array<Vector3> get_sector_world_aabb(Vector3I sectorLocalCoords)
	{
		if (!_currentQuadrant.HasValue)
		{
			return new Array<Vector3> { Vector3.Zero, Vector3.Zero };
		}

		Vector3 quadrantOrigin = GetQuadrantOrigin(_currentQuadrant.Value);
		float sectorSize = (float)GalaxyCoordinates.SectorSizePc;
		Vector3 min = quadrantOrigin + new Vector3(
			sectorLocalCoords.X * sectorSize,
			sectorLocalCoords.Y * sectorSize,
			sectorLocalCoords.Z * sectorSize);
		Vector3 max = min + new Vector3(sectorSize, sectorSize, sectorSize);
		return new Array<Vector3> { min, max };
	}

	/// <summary>
	/// Highlights a sector cell, or clears the current highlight.
	/// </summary>
	public void set_highlight(Variant coordsVariant)
	{
		SetHighlight(coordsVariant);
	}

	/// <summary>
	/// Highlights a sector cell, or clears the current highlight.
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

	private static Vector3 GetQuadrantOrigin(Vector3I quadrantCoords)
	{
		return new Vector3(
			(float)(quadrantCoords.X * GalaxyCoordinates.QuadrantSizePc),
			(float)(quadrantCoords.Y * GalaxyCoordinates.QuadrantSizePc),
			(float)(quadrantCoords.Z * GalaxyCoordinates.QuadrantSizePc));
	}

	private static Color DensityToColor(float normalizedDensity)
	{
		float t = Mathf.Clamp(normalizedDensity, 0.0f, 1.0f);
		float alpha = Mathf.Lerp(0.03f, 0.2f, t);
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

		float cellSize = (float)GalaxyCoordinates.SectorSizePc * CellFillFraction;
		BoxMesh box = new()
		{
			Size = Vector3.One,
		};

		ShaderMaterial material = new()
		{
			Shader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/sector_cell.gdshader"),
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
