using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for lenticular galaxies as an exponential disk with a Sérsic bulge and no active spiral arms.
/// </summary>
public partial class LenticularDensityModel : DensityModelInterface
{
    private readonly GalaxySpec _spec;
    private readonly float _peakDensity;

    /// <summary>
    /// Creates a lenticular density model from a galaxy specification.
    /// </summary>
    public LenticularDensityModel(GalaxySpec spec)
    {
        _spec = spec;
        _peakDensity = (float)System.Math.Max(1.0, spec.BulgeIntensity + 0.8);
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        float radialDistance = Mathf.Sqrt((position.X * position.X) + (position.Z * position.Z));
        float bulge = GetSersicBulgeDensity(radialDistance, position.Y);
        float disk = GetDiskDensity(radialDistance, position.Y);
        float bar = GetBarDensity(position);
        return Mathf.Max(bulge + disk + bar, 0.0f);
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return _peakDensity;
    }

    private float GetSersicBulgeDensity(float radialDistance, float height)
    {
        double axisRatio = System.Math.Max(0.3, 1.0 - _spec.Ellipticity);
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
        double radialTerm = System.Math.Exp(-radialDistance / System.Math.Max(1.0, _spec.DiskScaleLengthPc));
        double verticalTerm = System.Math.Exp(-System.Math.Abs(height) / System.Math.Max(1.0, _spec.DiskScaleHeightPc));
        return (float)((1.0 - (_spec.BulgeToTotal * 0.45)) * radialTerm * verticalTerm);
    }

    private float GetBarDensity(Vector3 position)
    {
        if (!_spec.IsBarred || _spec.BarStrength <= 0.0)
        {
            return 0.0f;
        }

        double major = _spec.BulgeRadiusPc * (1.3 + _spec.BarStrength);
        double minor = _spec.BulgeRadiusPc * 0.42;
        double vertical = System.Math.Max(120.0, _spec.DiskScaleHeightPc * 0.85);
        double exponent = -0.5 * (
            (position.X * position.X) / (major * major) +
            (position.Z * position.Z) / (minor * minor) +
            (position.Y * position.Y) / (vertical * vertical));
        return (float)(_spec.BarStrength * 0.9 * System.Math.Exp(exponent));
    }
}
