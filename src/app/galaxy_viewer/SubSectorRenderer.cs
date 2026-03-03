using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders star systems inside a sector as billboard points.
/// </summary>
public partial class SubSectorRenderer : MultiMeshInstance3D
{
	private const float StarSize = 1.5f;

	private ShaderMaterial? _material;
	private SectorStarData? _starData;

	/// <summary>
	/// Builds the star point cloud for a sector.
	/// </summary>
	public void build_for_sector(
		long galaxySeed,
		Vector3I quadrantCoords,
		Vector3I sectorLocalCoords,
		Variant densityModelVariant,
		float referenceDensity)
	{
		DensityModelInterface? densityModel = densityModelVariant.As<DensityModelInterface>();
		if (densityModel == null)
		{
			return;
		}

		BuildForSector(galaxySeed, quadrantCoords, sectorLocalCoords, densityModel, referenceDensity);
	}

	/// <summary>
	/// Builds the star point cloud for a sector.
	/// </summary>
	public void BuildForSector(
		long galaxySeed,
		Vector3I quadrantCoords,
		Vector3I sectorLocalCoords,
		DensityModelInterface densityModel,
		float referenceDensity)
	{
		_starData = SubSectorGenerator.GenerateSectorStars(
			galaxySeed,
			quadrantCoords,
			sectorLocalCoords,
			densityModel,
			referenceDensity);

		if (_starData.GetCount() == 0)
		{
			Multimesh = null;
			return;
		}

		BuildMultiMesh(_starData);
	}

	/// <summary>
	/// Returns the generated star data.
	/// </summary>
	public SectorStarData? get_star_data()
	{
		return _starData;
	}

	/// <summary>
	/// Sets the global opacity of the star points.
	/// </summary>
	public void set_opacity(float alpha)
	{
		_material?.SetShaderParameter("global_alpha", Mathf.Clamp(alpha, 0.0f, 1.0f));
	}

	/// <summary>
	/// Builds the backing multimesh.
	/// </summary>
	private void BuildMultiMesh(SectorStarData starData)
	{
		int count = starData.GetCount();

		QuadMesh quad = new()
		{
			Size = Vector2.One,
		};

		_material = new ShaderMaterial
		{
			Shader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/star_billboard.gdshader"),
		};
		quad.Material = _material;

		MultiMesh multiMesh = new()
		{
			Mesh = quad,
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
			UseColors = true,
			InstanceCount = count,
		};

		Basis starBasis = Basis.Identity.Scaled(new Vector3(StarSize, StarSize, StarSize));
		for (int index = 0; index < count; index += 1)
		{
			multiMesh.SetInstanceTransform(index, new Transform3D(starBasis, starData.Positions[index]));
			multiMesh.SetInstanceColor(index, ColorFromSeed(starData.StarSeeds[index]));
		}

		Multimesh = multiMesh;
	}

	/// <summary>
	/// Derives a rough display color from a deterministic star seed.
	/// </summary>
	private static Color ColorFromSeed(long starSeed)
	{
		int hash = (int)StableHash.HashIntegers(new Godot.Collections.Array<long> { starSeed });
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
