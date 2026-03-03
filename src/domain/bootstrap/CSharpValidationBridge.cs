using Godot;
using Godot.Collections;
using ValidationHelpers = StarGen.Domain.Validation.Validation;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for validation helpers.
/// </summary>
[GlobalClass]
public partial class CSharpValidationBridge : RefCounted
{
    /// <summary>
    /// Returns whether a floating-point value is positive.
    /// </summary>
    public bool IsPositiveFloat(double value) => ValidationHelpers.IsPositiveFloat(value);

    /// <summary>
    /// Returns whether a floating-point value is non-negative.
    /// </summary>
    public bool IsNonNegativeFloat(double value) => ValidationHelpers.IsNonNegativeFloat(value);

    /// <summary>
    /// Returns whether an integer value is positive.
    /// </summary>
    public bool IsPositiveInt(int value) => ValidationHelpers.IsPositiveInt(value);

    /// <summary>
    /// Returns whether an integer value is non-negative.
    /// </summary>
    public bool IsNonNegativeInt(int value) => ValidationHelpers.IsNonNegativeInt(value);

    /// <summary>
    /// Returns whether a floating-point value is in the inclusive range.
    /// </summary>
    public bool IsInRangeFloat(double value, double minValue, double maxValue) => ValidationHelpers.IsInRangeFloat(value, minValue, maxValue);

    /// <summary>
    /// Returns whether an integer value is in the inclusive range.
    /// </summary>
    public bool IsInRangeInt(int value, int minValue, int maxValue) => ValidationHelpers.IsInRangeInt(value, minValue, maxValue);

    /// <summary>
    /// Returns whether a string is not empty.
    /// </summary>
    public bool IsNotEmptyString(string value) => ValidationHelpers.IsNotEmptyString(value);

    /// <summary>
    /// Returns whether an array has at least one item.
    /// </summary>
    public bool IsNotEmptyArray(Array value) => value.Count > 0;

    /// <summary>
    /// Returns whether an integer is within enum bounds.
    /// </summary>
    public bool IsValidEnum(int value, int enumSize) => ValidationHelpers.IsValidEnum(value, enumSize);

    /// <summary>
    /// Returns whether a seed is valid.
    /// </summary>
    public bool IsValidSeed(long value) => ValidationHelpers.IsValidSeed(value);
}
