using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates star density for elliptical galaxies using a 3D ellipsoidal Gaussian.
/// </summary>
public partial class EllipticalDensityModel : DensityModelInterface
{
    private readonly float _sigmaMajor;
    private readonly float _sigmaMinor;
    private readonly float _axisRatio;
    private readonly float _peakDensity;

    /// <summary>
    /// Creates an elliptical density model from a galaxy specification.
    /// </summary>
    public EllipticalDensityModel(GalaxySpec spec)
    {
        _axisRatio = Mathf.Max(1.0f - (float)spec.Ellipticity, 0.3f);
        _sigmaMajor = (float)(spec.RadiusPc * 0.35);
        _sigmaMinor = _sigmaMajor * _axisRatio;
        _peakDensity = (float)(spec.BulgeIntensity * 2.0);
    }

    /// <inheritdoc />
    public override float GetDensity(Vector3 position)
    {
        double xTerm = (position.X * position.X) / (2.0 * _sigmaMajor * _sigmaMajor);
        double yTerm = (position.Y * position.Y) / (2.0 * _sigmaMinor * _sigmaMinor);
        double zTerm = (position.Z * position.Z) / (2.0 * _sigmaMajor * _sigmaMajor);
        double exponent = -(xTerm + yTerm + zTerm);
        exponent = System.Math.Clamp(exponent, -30.0, 0.0);
        return (float)(_peakDensity * System.Math.Exp(exponent));
    }

    /// <inheritdoc />
    public override float GetPeakDensity()
    {
        return _peakDensity;
    }

    /// <summary>
    /// Returns the effective major-axis sigma in parsecs.
    /// </summary>
    public float GetEffectiveRadius()
    {
        return _sigmaMajor;
    }

    /// <summary>
    /// Returns the ellipsoid axis ratio (b/a).
    /// </summary>
    public float GetAxisRatio()
    {
        return _axisRatio;
    }
}
