using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for spiral galaxies using bulge, disk, and arm modulation.
/// </summary>
public partial class SpiralDensityModel : DensityModelInterface
{
    private readonly GalaxySpec _spec;
    private readonly double _pitchTan;
    private readonly double[] _armOffsets;

    /// <summary>
    /// Creates a spiral density model from a galaxy specification.
    /// </summary>
    public SpiralDensityModel(GalaxySpec spec)
    {
        _spec = spec;
        _pitchTan = System.Math.Tan(Mathf.DegToRad((float)spec.ArmPitchAngleDeg));
        _armOffsets = new double[spec.NumArms];
        for (int index = 0; index < spec.NumArms; index += 1)
        {
            _armOffsets[index] = index * Mathf.Tau / spec.NumArms;
        }
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        float radialDistance = GetRadialDistance(position);
        float height = position.Y;
        float bulge = GetBulgeDensity(radialDistance, height);
        float disk = GetDiskDensity(radialDistance, height);
        float armModulation = GetCombinedArmFactor(radialDistance, position.X, position.Z);
        return Mathf.Max(bulge + (disk * armModulation), 0.0f);
    }

    /// <inheritdoc />
    public override float GetArmFactor(float radialDistance, float x, float zPosition)
    {
        return GetCombinedArmFactor(radialDistance, x, zPosition);
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return (float)(_spec.BulgeIntensity + 1.0);
    }

    /// <summary>
    /// Returns the radial distance in the galactic plane.
    /// </summary>
    private static float GetRadialDistance(Vector3 position)
    {
        return Mathf.Sqrt((position.X * position.X) + (position.Z * position.Z));
    }

    /// <summary>
    /// Returns the Gaussian bulge density contribution.
    /// </summary>
    private float GetBulgeDensity(float radialDistance, float height)
    {
        double normalizedRadius = radialDistance / _spec.BulgeRadiusPc;
        double normalizedHeight = height / _spec.BulgeHeightPc;
        double exponent = -0.5 * ((normalizedRadius * normalizedRadius) + (normalizedHeight * normalizedHeight));
        return (float)(_spec.BulgeIntensity * System.Math.Exp(exponent));
    }

    /// <summary>
    /// Returns the exponential disk density contribution.
    /// </summary>
    private float GetDiskDensity(float radialDistance, float height)
    {
        double radialTerm = System.Math.Exp(-radialDistance / _spec.DiskScaleLengthPc);
        double verticalTerm = System.Math.Exp(-System.Math.Abs(height) / _spec.DiskScaleHeightPc);
        return (float)(radialTerm * verticalTerm);
    }

    /// <summary>
    /// Combines the inter-arm baseline with the nearest arm proximity.
    /// </summary>
    private float GetCombinedArmFactor(float radialDistance, float x, float zPosition)
    {
        float baseline = (float)(1.0 - _spec.ArmAmplitude);
        if (radialDistance < 1.0f)
        {
            return 1.0f;
        }

        float armProximity = GetPeakArmProximity(radialDistance, x, zPosition);
        return baseline + ((float)_spec.ArmAmplitude * armProximity);
    }

    /// <summary>
    /// Returns the strongest spiral-arm proximity at the supplied position.
    /// </summary>
    private float GetPeakArmProximity(float radialDistance, float x, float zPosition)
    {
        double theta = System.Math.Atan2(zPosition, x);
        double logRadius = System.Math.Log(radialDistance);
        double inverseTwoWidthSquared = 0.5 / (_spec.ArmWidth * _spec.ArmWidth);
        double bestProximity = 0.0;

        for (int index = 0; index < _armOffsets.Length; index += 1)
        {
            double armTheta = _armOffsets[index] + (logRadius / _pitchTan);
            double delta = WrapAngle((float)(theta - armTheta));
            double proximity = System.Math.Exp(-(delta * delta) * inverseTwoWidthSquared);
            if (proximity > bestProximity)
            {
                bestProximity = proximity;
            }
        }

        return (float)bestProximity;
    }

    /// <summary>
    /// Wraps an angle to the [-PI, PI] range.
    /// </summary>
    private static float WrapAngle(float angle)
    {
        float tau = (float)(2.0 * System.Math.PI);
        float wrapped = Mathf.PosMod(angle + (float)System.Math.PI, tau);
        return wrapped - (float)System.Math.PI;
    }
}
