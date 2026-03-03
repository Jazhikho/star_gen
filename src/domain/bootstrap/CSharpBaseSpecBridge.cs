using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for base-spec override helpers.
/// </summary>
[GlobalClass]
public partial class CSharpBaseSpecBridge : RefCounted
{
    /// <summary>
    /// Returns whether the overrides dictionary contains the field path.
    /// </summary>
    public bool HasOverride(Dictionary overrides, string fieldPath) => new BaseSpec(overrides: overrides).HasOverride(fieldPath);

    /// <summary>
    /// Returns an override value or the supplied default.
    /// </summary>
    public Variant GetOverride(Dictionary overrides, string fieldPath, Variant defaultValue) => new BaseSpec(overrides: overrides).GetOverride(fieldPath, defaultValue);

    /// <summary>
    /// Returns an override value as a floating-point value.
    /// </summary>
    public double GetOverrideFloat(Dictionary overrides, string fieldPath, double defaultValue) => new BaseSpec(overrides: overrides).GetOverrideFloat(fieldPath, defaultValue);

    /// <summary>
    /// Returns an override value as an integer.
    /// </summary>
    public int GetOverrideInt(Dictionary overrides, string fieldPath, int defaultValue) => new BaseSpec(overrides: overrides).GetOverrideInt(fieldPath, defaultValue);
}
