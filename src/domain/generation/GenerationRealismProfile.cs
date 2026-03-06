namespace StarGen.Domain.Generation;

/// <summary>
/// Configures generation between calibrated and stylized output.
/// <para>
/// <b>Status:</b> Data model only — no generator currently reads this profile.
/// Wiring this into the generation pipeline is a future effort (see Docs/Roadmap.md).
/// The bridge class exposes it to GDScript so tooling can read and store the value in advance.
/// </para>
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
    /// Legacy alias enum.
    /// </summary>
    public enum RealismMode
    {
        Calibrated,
        Balanced,
        Stylized,
    }

    /// <summary>
    /// Current mode.
    /// </summary>
    public RealismMode Mode { get; set; }

    /// <summary>
    /// Legacy mode alias property.
    /// </summary>
    public ModeType ModeTypeValue
    {
        get => (ModeType)Mode;
        set => Mode = (RealismMode)value;
    }

    /// <summary>
    /// Slider value in the inclusive range [0, 1].
    /// </summary>
    public double RealismSlider { get; set; }

    /// <summary>
    /// Creates a new realism profile.
    /// </summary>
    public GenerationRealismProfile(
        RealismMode mode = RealismMode.Balanced,
        double realismSlider = 0.5)
    {
        Mode = mode;
        RealismSlider = realismSlider;
    }

    /// <summary>
    /// Compatibility constructor for legacy mode enum.
    /// </summary>
    public GenerationRealismProfile(
        ModeType mode,
        double realismSlider)
        : this((RealismMode)mode, realismSlider)
    {
    }

    /// <summary>
    /// Builds a profile from a [0, 1] slider.
    /// </summary>
    public static GenerationRealismProfile FromSlider(double slider)
    {
        double clampedSlider = System.Math.Clamp(slider, 0.0, 1.0);
        RealismMode mode = clampedSlider switch
        {
            <= 0.33 => RealismMode.Stylized,
            >= 0.67 => RealismMode.Calibrated,
            _ => RealismMode.Balanced,
        };

        return new GenerationRealismProfile(mode, clampedSlider);
    }

    /// <summary>
    /// Returns a calibrated profile.
    /// </summary>
    public static GenerationRealismProfile Calibrated() => new(RealismMode.Calibrated, 1.0);

    /// <summary>
    /// Returns a balanced profile.
    /// </summary>
    public static GenerationRealismProfile Balanced() => new(RealismMode.Balanced, 0.5);

    /// <summary>
    /// Returns a stylized profile.
    /// </summary>
    public static GenerationRealismProfile Stylized() => new(RealismMode.Stylized, 0.0);
}
