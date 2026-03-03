namespace StarGen.Domain.Math;

/// <summary>
/// Shared math helpers for generation and validation.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Returns whether the supplied floating-point value is inside the inclusive range.
    /// </summary>
    public static bool IsInRangeFloat(double value, double minValue, double maxValue)
    {
        return value >= minValue && value <= maxValue;
    }

    /// <summary>
    /// Returns whether the supplied integer value is inside the inclusive range.
    /// </summary>
    public static bool IsInRangeInt(int value, int minValue, int maxValue)
    {
        return value >= minValue && value <= maxValue;
    }

    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    public static double Remap(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        if (fromMax == fromMin)
        {
            return toMin;
        }

        double normalized = (value - fromMin) / (fromMax - fromMin);
        return toMin + (normalized * (toMax - toMin));
    }

    /// <summary>
    /// Remaps a value from one range to another and clamps it to the target range.
    /// </summary>
    public static double RemapClamped(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        double remapped = Remap(value, fromMin, fromMax, toMin, toMax);
        double actualMin = System.Math.Min(toMin, toMax);
        double actualMax = System.Math.Max(toMin, toMax);
        return System.Math.Clamp(remapped, actualMin, actualMax);
    }

    /// <summary>
    /// Calculates inverse lerp for the supplied range.
    /// </summary>
    public static double InverseLerp(double from, double to, double value)
    {
        if (to == from)
        {
            return 0.0;
        }

        return (value - from) / (to - from);
    }

    /// <summary>
    /// Performs smooth interpolation using a smoothstep curve.
    /// </summary>
    public static double SmoothLerp(double from, double to, double weight)
    {
        double clamped = System.Math.Clamp(weight, 0.0, 1.0);
        double smoothed = clamped * clamped * (3.0 - (2.0 * clamped));
        return from + ((to - from) * smoothed);
    }
}
