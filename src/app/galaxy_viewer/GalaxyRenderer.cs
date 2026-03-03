using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Renders sampled galaxy points as a billboard multimesh.
/// </summary>
public partial class GalaxyRenderer : MultiMeshInstance3D
{
	private const float DefaultStarSize = 80.0f;

	private static readonly Color BulgeColor = new(1.0f, 0.85f, 0.5f, 0.9f);
	private static readonly Color DiskColor = new(0.6f, 0.75f, 1.0f, 0.8f);
	private static readonly Color EllipticalCoreColor = new(1.0f, 0.8f, 0.4f, 0.9f);
	private static readonly Color EllipticalOuterColor = new(0.95f, 0.75f, 0.45f, 0.85f);
	private static readonly Color IrregularWarmColor = new(1.0f, 0.7f, 0.4f, 0.85f);
	private static readonly Color IrregularCoolColor = new(0.7f, 0.8f, 1.0f, 0.85f);

	private ShaderMaterial? _material;

	/// <summary>
	/// Builds and assigns the galaxy point cloud.
	/// </summary>
	public void build_from_sample(Variant sampleVariant, float starSize = DefaultStarSize, int galaxyType = (int)GalaxySpec.GalaxyType.Spiral)
	{
		GalaxySample? sample = sampleVariant.As<GalaxySample>();
		if (sample == null)
		{
			return;
		}

		BuildFromSample(sample, starSize, galaxyType);
	}

	/// <summary>
	/// Builds and assigns the galaxy point cloud.
	/// </summary>
	public void BuildFromSample(GalaxySample sample, float starSize = DefaultStarSize, int galaxyType = (int)GalaxySpec.GalaxyType.Spiral)
	{
		int total = sample.GetTotalCount();
		if (total == 0)
		{
			Multimesh = null;
			return;
		}

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
			InstanceCount = total,
		};

		int index = galaxyType switch
		{
			(int)GalaxySpec.GalaxyType.Elliptical => FillEllipticalGalaxy(multiMesh, sample, starSize),
			(int)GalaxySpec.GalaxyType.Irregular => FillIrregularGalaxy(multiMesh, sample, starSize),
			_ => FillSpiralGalaxy(multiMesh, sample, starSize),
		};

		if (index > 0)
		{
			Multimesh = multiMesh;
		}
	}

	/// <summary>
	/// Sets the global point-cloud opacity.
	/// </summary>
	public void set_opacity(float alpha)
	{
		SetOpacity(alpha);
	}

	/// <summary>
	/// Sets the global point-cloud opacity.
	/// </summary>
	public void SetOpacity(float alpha)
	{
		_material?.SetShaderParameter("global_alpha", Mathf.Clamp(alpha, 0.0f, 1.0f));
	}

	private static int FillSpiralGalaxy(MultiMesh multiMesh, GalaxySample sample, float starSize)
	{
		int index = 0;
		index = FillPopulation(multiMesh, sample.BulgePoints, BulgeColor, starSize, index);
		index = FillPopulation(multiMesh, sample.DiskPoints, DiskColor, starSize, index);
		return index;
	}

	private static int FillEllipticalGalaxy(MultiMesh multiMesh, GalaxySample sample, float starSize)
	{
		int index = 0;
		Basis scaleBasis = Basis.Identity.Scaled(new Vector3(starSize, starSize, starSize));
		float maxRadius = 1.0f;

		for (int i = 0; i < sample.BulgePoints.Length; i++)
		{
			float radius = sample.BulgePoints[i].Length();
			if (radius > maxRadius)
			{
				maxRadius = radius;
			}
		}

		for (int i = 0; i < sample.BulgePoints.Length; i++)
		{
			Vector3 point = sample.BulgePoints[i];
			float t = Mathf.Clamp(point.Length() / maxRadius, 0.0f, 1.0f);
			Color color = EllipticalCoreColor.Lerp(EllipticalOuterColor, t);
			multiMesh.SetInstanceTransform(index, new Transform3D(scaleBasis, point));
			multiMesh.SetInstanceColor(index, color);
			index += 1;
		}

		return index;
	}

	private static int FillIrregularGalaxy(MultiMesh multiMesh, GalaxySample sample, float starSize)
	{
		int index = 0;
		Basis scaleBasis = Basis.Identity.Scaled(new Vector3(starSize, starSize, starSize));

		for (int i = 0; i < sample.BulgePoints.Length; i++)
		{
			Vector3 point = sample.BulgePoints[i];
			float t = Mathf.Clamp(point.Length() / 5000.0f, 0.0f, 1.0f);
			Color color = IrregularWarmColor.Lerp(EllipticalCoreColor, t * 0.3f);
			multiMesh.SetInstanceTransform(index, new Transform3D(scaleBasis, point));
			multiMesh.SetInstanceColor(index, color);
			index += 1;
		}

		for (int i = 0; i < sample.DiskPoints.Length; i++)
		{
			Vector3 point = sample.DiskPoints[i];
			int hashInput = (i * 73856093) + ((int)point.X * 19349663) + ((int)point.Z * 83492791);
			float hashValue = (hashInput & 0xFFFF) / 65535.0f;

			Color color;
			if (hashValue < 0.35f)
			{
				color = IrregularCoolColor;
			}
			else if (hashValue < 0.65f)
			{
				float blend = (hashValue - 0.35f) / 0.3f;
				color = IrregularCoolColor.Lerp(IrregularWarmColor, blend);
			}
			else
			{
				color = IrregularWarmColor;
			}

			multiMesh.SetInstanceTransform(index, new Transform3D(scaleBasis, point));
			multiMesh.SetInstanceColor(index, color);
			index += 1;
		}

		return index;
	}

	private static int FillPopulation(MultiMesh multiMesh, Vector3[] points, Color baseColor, float starSize, int startIndex)
	{
		Basis scaleBasis = Basis.Identity.Scaled(new Vector3(starSize, starSize, starSize));
		int index = startIndex;

		for (int i = 0; i < points.Length; i++)
		{
			multiMesh.SetInstanceTransform(index, new Transform3D(scaleBasis, points[i]));
			multiMesh.SetInstanceColor(index, baseColor);
			index += 1;
		}

		return index;
	}
}
