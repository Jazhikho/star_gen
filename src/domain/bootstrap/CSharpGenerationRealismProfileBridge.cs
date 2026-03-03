using Godot;
using Godot.Collections;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for generation realism profile factories.
/// </summary>
[GlobalClass]
public partial class CSharpGenerationRealismProfileBridge : RefCounted
{
    /// <summary>
    /// Builds a profile payload from a slider.
    /// </summary>
    public Dictionary FromSlider(double slider) => ToPayload(GenerationRealismProfile.FromSlider(slider));

    /// <summary>
    /// Returns a calibrated profile payload.
    /// </summary>
    public Dictionary Calibrated() => ToPayload(GenerationRealismProfile.Calibrated());

    /// <summary>
    /// Returns a balanced profile payload.
    /// </summary>
    public Dictionary Balanced() => ToPayload(GenerationRealismProfile.Balanced());

    /// <summary>
    /// Returns a stylized profile payload.
    /// </summary>
    public Dictionary Stylized() => ToPayload(GenerationRealismProfile.Stylized());

    private static Dictionary ToPayload(GenerationRealismProfile profile)
    {
        return new Dictionary
        {
            ["mode"] = (int)profile.Mode,
            ["realism_slider"] = profile.RealismSlider,
        };
    }
}
