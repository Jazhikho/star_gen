namespace StarGen.Domain.Generation;

/// <summary>
/// Configures generation between calibrated and stylized output.
/// </summary>
public partial class GenerationRealismProfile : Godot.RefCounted
{
    /// <summary>
    /// Profile mode choices.
    /// </summary>
    public enum ModeType
    {
        Calibrated,
        Balanced,
        Stylized,
    }

    /// <summary>
    /// Current mode.
    /// </summary>
    public ModeType Mode;

    /// <summary>
    /// Slider value in the inclusive range [0, 1].
    /// </summary>
    public double RealismSlider;

    /// <summary>
    /// Creates a new realism profile.
    /// </summary>
    public GenerationRealismProfile(
        ModeType mode = ModeType.Balanced,
        double realismSlider = 0.5)
    {
        Mode = mode;
        RealismSlider = realismSlider;
    }

    /// <summary>
    /// Builds a profile from a [0, 1] slider.
    /// </summary>
    public static GenerationRealismProfile FromSlider(double slider)
    {
        double clampedSlider = System.Math.Clamp(slider, 0.0, 1.0);
        ModeType mode = clampedSlider switch
        {
            <= 0.33 => ModeType.Stylized,
            >= 0.67 => ModeType.Calibrated,
            _ => ModeType.Balanced,
        };

        return new GenerationRealismProfile(mode, clampedSlider);
    }

    /// <summary>
    /// Returns a calibrated profile.
    /// </summary>
    public static GenerationRealismProfile Calibrated() => new(ModeType.Calibrated, 1.0);

    /// <summary>
    /// Returns a balanced profile.
    /// </summary>
    public static GenerationRealismProfile Balanced() => new(ModeType.Balanced, 0.5);

    /// <summary>
    /// Returns a stylized profile.
    /// </summary>
    public static GenerationRealismProfile Stylized() => new(ModeType.Stylized, 0.0);
}
