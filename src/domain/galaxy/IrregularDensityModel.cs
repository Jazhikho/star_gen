using Godot;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for irregular and dwarf galaxies using layered noise, asymmetry, and subtype-dependent radial falloff.
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
        _peakDensity = (float)System.Math.Max(0.4, spec.BulgeIntensity * 1.35);
        _scaleRadius = (float)System.Math.Max(800.0, spec.EffectiveRadiusPc);

        _structureNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = (float)(0.00018 * spec.IrregularityScale),
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = 4,
            FractalLacunarity = 2.0f,
            FractalGain = 0.52f,
        };

        _clumpNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed + 1000,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular,
            Frequency = (float)(0.00045 * spec.IrregularityScale),
            CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.Euclidean,
            CellularReturnType = FastNoiseLite.CellularReturnTypeEnum.Distance2Div,
        };

        _asymmetryNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed + 2000,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Frequency = (float)(0.00012 * spec.IrregularityScale),
        };

        SeededRng rng = new SeededRng(spec.GalaxySeed);
        float offsetScale = (float)(spec.RadiusPc * 0.18 * spec.IrregularityScale);
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
        float subtypeWeight = GetSubtypeWeight();
        float density = baseDensity * structureModulation * subtypeWeight * (1.0f + (clumpBoost * 0.55f));
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

    private float GetBaseFalloff(float radialDistance)
    {
        double falloff = System.Math.Exp(-radialDistance / _scaleRadius);
        double halo = 0.25 * System.Math.Exp(-radialDistance / (_scaleRadius * 2.4f));
        if (_spec.ResolvedSubtype == GalaxyResolvedSubtype.DwarfSpheroidal)
        {
            falloff = System.Math.Exp(-radialDistance / (_scaleRadius * 0.75f));
            halo = 0.08 * System.Math.Exp(-radialDistance / (_scaleRadius * 1.8f));
        }

        return (float)(_spec.BulgeIntensity * (falloff + halo));
    }

    private float GetStructureModulation(Vector3 position)
    {
        float noiseValue = _structureNoise.GetNoise3D(position.X, position.Y, position.Z);
        float asymmetry = _asymmetryNoise.GetNoise3D(position.X * 0.5f, position.Y * 0.5f, position.Z * 0.5f);
        float combined = ((noiseValue + (asymmetry * 0.35f)) + 1.0f) * 0.5f;
        return Mathf.Clamp(0.28f + (combined * 0.72f), 0.22f, 1.0f);
    }

    private float GetClumpBoost(Vector3 position)
    {
        float noiseValue = _clumpNoise.GetNoise3D(position.X, position.Y, position.Z);
        float boost = 1.0f - Mathf.Clamp((noiseValue * 0.5f) + 0.5f, 0.0f, 1.0f);
        return boost * boost;
    }

    private float GetSubtypeWeight()
    {
        if (_spec.ResolvedSubtype == GalaxyResolvedSubtype.DwarfSpheroidal)
        {
            return 0.55f;
        }

        if (_spec.ResolvedSubtype == GalaxyResolvedSubtype.DwarfIrregular)
        {
            return 0.85f;
        }

        return 1.0f;
    }
}
