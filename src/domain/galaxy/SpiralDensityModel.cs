using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for spiral galaxies using a Sérsic bulge, exponential disk, optional bar, and arm modulation.
/// </summary>
public partial class SpiralDensityModel : DensityModelInterface
{
    private readonly GalaxySpec _spec;
    private readonly double _pitchTan;
    private readonly double[] _armOffsets;
    private readonly FastNoiseLite _armNoise;

    /// <summary>
    /// Creates a spiral density model from a galaxy specification.
    /// </summary>
    public SpiralDensityModel(GalaxySpec spec)
    {
        _spec = spec;
        _pitchTan = System.Math.Tan(Mathf.DegToRad((float)spec.ArmPitchAngleDeg));
        _armOffsets = new double[System.Math.Max(spec.NumArms, 1)];
        for (int index = 0; index < _armOffsets.Length; index += 1)
        {
            _armOffsets[index] = index * Mathf.Tau / _armOffsets.Length;
        }

        _armNoise = new FastNoiseLite
        {
            Seed = spec.GalaxySeed + 431,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = 0.00035f,
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = 3,
            FractalGain = 0.55f,
            FractalLacunarity = 2.1f,
        };
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        float radialDistance = Mathf.Sqrt((position.X * position.X) + (position.Z * position.Z));
        float bulge = GetBulgeDensity(radialDistance, position.Y);
        float disk = GetDiskDensity(radialDistance, position.Y);
        float bar = GetBarDensity(position);
        float armModulation = GetCombinedArmFactor(radialDistance, position.X, position.Z);
        return Mathf.Max(bulge + bar + (disk * armModulation), 0.0f);
    }

    /// <inheritdoc />
    public override float GetArmFactor(float radialDistance, float x, float zPosition)
    {
        return GetCombinedArmFactor(radialDistance, x, zPosition);
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return (float)(_spec.BulgeIntensity + 1.0 + _spec.BarStrength);
    }

    private float GetBulgeDensity(float radialDistance, float height)
    {
        double axisRatio = System.Math.Max(0.35, 1.0 - _spec.Ellipticity);
        double ellipsoidRadius = System.Math.Sqrt(
            (radialDistance * radialDistance) + ((height * height) / (axisRatio * axisRatio)));
        double n = System.Math.Max(1.0, _spec.SersicIndex);
        double bn = (2.0 * n) - 0.324;
        double scaledRadius = ellipsoidRadius / System.Math.Max(50.0, _spec.EffectiveRadiusPc);
        double exponent = -bn * System.Math.Pow(scaledRadius, 1.0 / n);
        exponent = System.Math.Max(exponent, -40.0);
        return (float)(_spec.BulgeIntensity * System.Math.Exp(exponent));
    }

    private float GetDiskDensity(float radialDistance, float height)
    {
        double thinRadialTerm = System.Math.Exp(-radialDistance / System.Math.Max(1.0, _spec.ThinDiskScaleLengthPc));
        double thinVerticalTerm = System.Math.Exp(-System.Math.Abs(height) / System.Math.Max(1.0, _spec.ThinDiskScaleHeightPc));
        double thickRadialTerm = System.Math.Exp(-radialDistance / System.Math.Max(1.0, _spec.ThickDiskScaleLengthPc));
        double thickVerticalTerm = System.Math.Exp(-System.Math.Abs(height) / System.Math.Max(1.0, _spec.ThickDiskScaleHeightPc));
        double thinComponent = thinRadialTerm * thinVerticalTerm * 0.88;
        double thickComponent = thickRadialTerm * thickVerticalTerm * 0.12;
        double diskWeight = System.Math.Max(0.25, 1.0 - _spec.BulgeToTotal);
        return (float)(diskWeight * (thinComponent + thickComponent));
    }

    private float GetBarDensity(Vector3 position)
    {
        if (!_spec.IsBarred || _spec.BarStrength <= 0.0)
        {
            return 0.0f;
        }

        double major = System.Math.Max(_spec.BulgeRadiusPc, _spec.BarHalfLengthPc);
        double minor = System.Math.Max(_spec.BulgeRadiusPc * 0.40, major * 0.22);
        double vertical = System.Math.Max(120.0, _spec.ThinDiskScaleHeightPc * 0.9);
        double exponent = -0.5 * (
            (position.X * position.X) / (major * major) +
            (position.Z * position.Z) / (minor * minor) +
            (position.Y * position.Y) / (vertical * vertical));
        return (float)(_spec.BarStrength * 1.1 * System.Math.Exp(exponent));
    }

    private float GetCombinedArmFactor(float radialDistance, float x, float zPosition)
    {
        if (_spec.NumArms <= 0 || _spec.ArmAmplitude <= 0.0)
        {
            return 1.0f;
        }

        float baseline = (float)System.Math.Max(0.15, 1.0 - _spec.ArmAmplitude);
        if (radialDistance < 1.0f)
        {
            return 1.0f;
        }

        float armProximity = GetPeakArmProximity(radialDistance, x, zPosition);
        float noiseSample = (_armNoise.GetNoise3D(x, 0.0f, zPosition) + 1.0f) * 0.5f;

        // Hart et al. (2017) and Lingard et al. (2021) treat grand-design, multi-armed,
        // and flocculent structure as meaningful observational morphology distinctions.
        // Tuning: the `0.70 / 0.30` and `0.35 / 0.65` blends below are StarGen weights for
        // mixing arm coherence with stochastic structure, not published coefficients.
        if (_spec.ArmMechanism == GalaxyArmMechanism.GrandDesign)
        {
            return baseline + ((float)_spec.ArmAmplitude * armProximity);
        }

        if (_spec.ArmMechanism == GalaxyArmMechanism.MultiArmed)
        {
            float blended = (armProximity * 0.7f) + (noiseSample * 0.3f);
            return baseline + ((float)_spec.ArmAmplitude * blended);
        }

        float flocculentBlend = (armProximity * 0.35f) + (noiseSample * 0.65f);
        return baseline + ((float)_spec.ArmAmplitude * flocculentBlend);
    }

    private float GetPeakArmProximity(float radialDistance, float x, float zPosition)
    {
        double theta = System.Math.Atan2(zPosition, x);
        double logRadius = System.Math.Log(System.Math.Max(radialDistance, 1.0f));
        double inverseTwoWidthSquared = 0.5 / (_spec.ArmWidth * _spec.ArmWidth);
        double bestProximity = 0.0;

        for (int index = 0; index < _armOffsets.Length; index += 1)
        {
            double armTheta = _armOffsets[index] + (logRadius / System.Math.Max(0.001, _pitchTan));
            double delta = WrapAngle(theta - armTheta);
            double proximity = System.Math.Exp(-(delta * delta) * inverseTwoWidthSquared);
            if (proximity > bestProximity)
            {
                bestProximity = proximity;
            }
        }

        return (float)bestProximity;
    }

    private static double WrapAngle(double angle)
    {
        double tau = 2.0 * System.Math.PI;
        double wrapped = angle % tau;
        if (wrapped > System.Math.PI)
        {
            wrapped -= tau;
        }

        if (wrapped < -System.Math.PI)
        {
            wrapped += tau;
        }

        return wrapped;
    }
}
