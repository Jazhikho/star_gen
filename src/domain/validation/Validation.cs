using StarGen.Domain.Math;

namespace StarGen.Domain.Validation;

/// <summary>
/// Validation helpers for common value constraints.
/// </summary>
public static class Validation
{
    /// <summary>
    /// Returns whether the supplied floating-point value is positive.
    /// </summary>
    public static bool IsPositiveFloat(double value) => value > 0.0;

    /// <summary>
    /// Returns whether the supplied floating-point value is non-negative.
    /// </summary>
    public static bool IsNonNegativeFloat(double value) => value >= 0.0;

    /// <summary>
    /// Returns whether the supplied integer value is positive.
    /// </summary>
    public static bool IsPositiveInt(int value) => value > 0;

    /// <summary>
    /// Returns whether the supplied integer value is non-negative.
    /// </summary>
    public static bool IsNonNegativeInt(int value) => value >= 0;

    /// <summary>
    /// Returns whether the supplied floating-point value is inside the inclusive range.
    /// </summary>
    public static bool IsInRangeFloat(double value, double minValue, double maxValue)
    {
        return MathUtils.IsInRangeFloat(value, minValue, maxValue);
    }

    /// <summary>
    /// Returns whether the supplied integer value is inside the inclusive range.
    /// </summary>
    public static bool IsInRangeInt(int value, int minValue, int maxValue)
    {
        return MathUtils.IsInRangeInt(value, minValue, maxValue);
    }

    /// <summary>
    /// Returns whether the supplied string is not null or empty.
    /// </summary>
    public static bool IsNotEmptyString(string value) => !string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns whether the supplied collection contains at least one item.
    /// </summary>
    public static bool IsNotEmptyArray<T>(System.Collections.Generic.IReadOnlyCollection<T> value)
    {
        return value.Count > 0;
    }

    /// <summary>
    /// Returns whether the supplied integer is within the enum range.
    /// </summary>
    public static bool IsValidEnum(int value, int enumSize) => value >= 0 && value < enumSize;

    /// <summary>
    /// All integer seeds are currently considered valid.
    /// </summary>
    public static bool IsValidSeed(long value)
    {
        _ = value;
        return true;
    }
}
