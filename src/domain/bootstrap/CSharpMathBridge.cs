using Godot;
using StarGen.Domain.Math;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for math helpers.
/// </summary>
[GlobalClass]
public partial class CSharpMathBridge : RefCounted
{
    /// <summary>
    /// Returns whether a floating-point value is in the inclusive range.
    /// </summary>
    public bool IsInRangeFloat(double value, double minValue, double maxValue) => MathUtils.IsInRangeFloat(value, minValue, maxValue);

    /// <summary>
    /// Returns whether an integer value is in the inclusive range.
    /// </summary>
    public bool IsInRangeInt(int value, int minValue, int maxValue) => MathUtils.IsInRangeInt(value, minValue, maxValue);

    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    public double Remap(double value, double fromMin, double fromMax, double toMin, double toMax) => MathUtils.Remap(value, fromMin, fromMax, toMin, toMax);

    /// <summary>
    /// Remaps and clamps a value from one range to another.
    /// </summary>
    public double RemapClamped(double value, double fromMin, double fromMax, double toMin, double toMax) => MathUtils.RemapClamped(value, fromMin, fromMax, toMin, toMax);

    /// <summary>
    /// Calculates inverse lerp.
    /// </summary>
    public double InverseLerp(double from, double to, double value) => MathUtils.InverseLerp(from, to, value);

    /// <summary>
    /// Performs smooth interpolation.
    /// </summary>
    public double SmoothLerp(double from, double to, double weight) => MathUtils.SmoothLerp(from, to, weight);
}
