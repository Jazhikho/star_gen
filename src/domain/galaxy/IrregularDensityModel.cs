using Godot;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for irregular galaxies using layered noise and radial falloff.
/// </summary>
public partial class IrregularDensityModel : DensityModelInterface
{
    private readonly GalaxySpec _spec;
    private readonly FastNoiseLite _structureNoise;
    private readonly FastNoiseLite _clumpNoise;
    private readonly FastNoiseLite _asymmetryNoise;
    private readonly Vector3 _centerOffset;
    private readonly float _peakDensity;
    private readonly float _scaleRadius;

    /// <summary>
    /// Creates an irregular density model from a galaxy specification.
    /// </summary>
    public IrregularDensityModel(GalaxySpec spec)
    {
        _spec = spec;
        _peakDensity = (float)(spec.BulgeIntensity * 1.5);
        _scaleRadius = (float)(spec.RadiusPc * 0.5);

        _structureNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = (float)(0.0002 * spec.IrregularityScale),
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = 3,
            FractalLacunarity = 2.0f,
            FractalGain = 0.5f,
        };

        _clumpNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed + 1000,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular,
            Frequency = (float)(0.0005 * spec.IrregularityScale),
            CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
            CellularReturnType = FastNoiseLite.CellularReturnTypeEnum.Distance2Div,
        };

        _asymmetryNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed + 2000,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Frequency = (float)(0.00015 * spec.IrregularityScale),
        };

        SeededRng rng = new(spec.GalaxySeed);
        float offsetScale = (float)(spec.RadiusPc * 0.15 * spec.IrregularityScale);
        _centerOffset = new Vector3(
            rng.RandfRange(-offsetScale, offsetScale),
            rng.RandfRange(-offsetScale, offsetScale),
            rng.RandfRange(-offsetScale, offsetScale));
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        Vector3 shifted = position - _centerOffset;
        float radialDistance = shifted.Length();
        float baseDensity = GetBaseFalloff(radialDistance);
        float structureModulation = GetStructureModulation(position);
        float clumpBoost = GetClumpBoost(position);
        float density = baseDensity * structureModulation * (1.0f + (clumpBoost * 0.5f));
        return Mathf.Max(density, 0.0f);
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return _peakDensity;
    }

    /// <summary>
    /// Returns the asymmetric center offset used by the model.
    /// </summary>
    public Vector3 GetCenterOffset()
    {
        return _centerOffset;
    }

    /// <summary>
    /// Returns the scale radius used for radial falloff.
    /// </summary>
    public float GetScaleRadius()
    {
        return _scaleRadius;
    }

    /// <summary>
    /// Returns the base radial falloff before noise modulation is applied.
    /// </summary>
    private float GetBaseFalloff(float radialDistance)
    {
        double falloff = System.Math.Exp(-radialDistance / _scaleRadius);
        double halo = 0.2 * System.Math.Exp(-radialDistance / (_scaleRadius * 2.5f));
        return (float)(_spec.BulgeIntensity * (falloff + halo));
    }

    /// <summary>
    /// Returns the large-scale structural modulation for a world-space position.
    /// </summary>
    private float GetStructureModulation(Vector3 position)
    {
        float noiseValue = _structureNoise.GetNoise3D(position.X, position.Y, position.Z);
        float asymmetry = _asymmetryNoise.GetNoise3D(position.X * 0.5f, position.Y * 0.5f, position.Z * 0.5f);
        float combined = ((noiseValue + (asymmetry * 0.3f)) + 1.0f) * 0.5f;
        return Mathf.Clamp(0.3f + (combined * 0.7f), 0.3f, 1.0f);
    }

    /// <summary>
    /// Returns the localized clump boost for star-forming regions.
    /// </summary>
    private float GetClumpBoost(Vector3 position)
    {
        float noiseValue = _clumpNoise.GetNoise3D(position.X, position.Y, position.Z);
        float boost = 1.0f - Mathf.Clamp((noiseValue * 0.5f) + 0.5f, 0.0f, 1.0f);
        return boost * boost;
    }
}
