using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for elliptical galaxies using a triaxial Sérsic-like spheroid.
/// </summary>
public partial class EllipticalDensityModel : DensityModelInterface
{
    private readonly float _axisRatioMinor;
    private readonly float _axisRatioIntermediate;
    private readonly float _effectiveRadius;
    private readonly float _peakDensity;
    private readonly double _sersicIndex;

    /// <summary>
    /// Creates an elliptical density model from a galaxy specification.
    /// </summary>
    public EllipticalDensityModel(GalaxySpec spec)
    {
        _axisRatioMinor = Mathf.Max(0.3f, 1.0f - (float)spec.Ellipticity);
        _axisRatioIntermediate = Mathf.Max(0.45f, _axisRatioMinor + 0.1f);
        _effectiveRadius = (float)System.Math.Max(600.0, spec.EffectiveRadiusPc);
        _peakDensity = (float)System.Math.Max(1.0, spec.BulgeIntensity * 2.0);
        _sersicIndex = System.Math.Max(2.0, spec.SersicIndex);
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        double ellipsoidRadius = System.Math.Sqrt(
            (position.X * position.X) +
            ((position.Y * position.Y) / (_axisRatioMinor * _axisRatioMinor)) +
            ((position.Z * position.Z) / (_axisRatioIntermediate * _axisRatioIntermediate)));
        double bn = (2.0 * _sersicIndex) - 0.324;
        double scaledRadius = ellipsoidRadius / _effectiveRadius;
        double exponent = -bn * System.Math.Pow(scaledRadius, 1.0 / _sersicIndex);
        exponent = System.Math.Max(exponent, -40.0);
        return (float)(_peakDensity * System.Math.Exp(exponent));
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return _peakDensity;
    }

    /// <summary>
    /// Returns the effective major-axis radius in parsecs.
    /// </summary>
    public float GetEffectiveRadius()
    {
        return _effectiveRadius;
    }

    /// <summary>
    /// Returns the ellipsoid minor-axis ratio.
    /// </summary>
    public float GetAxisRatio()
    {
        return _axisRatioMinor;
    }
}
