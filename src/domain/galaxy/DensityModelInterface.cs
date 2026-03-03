using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Shared interface for all galaxy density models.
/// </summary>
public abstract partial class DensityModelInterface : RefCounted
{
    /// <summary>
    /// Returns the unnormalized density at a world-space parsec position.
    /// </summary>
    public abstract float GetDensity(Vector3 position);

    /// <summary>
    /// Returns a peak-density estimate used for normalization.
    /// </summary>
    public abstract float GetPeakDensity();

    /// <summary>
    /// Returns the spiral-arm modulation factor at the given position parameters.
    /// </summary>
    public virtual float GetArmFactor(float radialDistance, float x, float zPosition)
    {
        return 1.0f;
    }

    /// <summary>
    /// Creates the appropriate density model for the given galaxy specification.
    /// </summary>
    public static DensityModelInterface CreateForSpec(GalaxySpec spec)
    {
        return spec.Type switch
        {
            GalaxySpec.GalaxyType.Spiral => new SpiralDensityModel(spec),
            GalaxySpec.GalaxyType.Elliptical => new EllipticalDensityModel(spec),
            GalaxySpec.GalaxyType.Irregular => new IrregularDensityModel(spec),
            _ => new SpiralDensityModel(spec),
        };
    }
}
