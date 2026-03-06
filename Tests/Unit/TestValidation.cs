#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Validation;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for validation functions.
/// </summary>
public static class TestValidation
{
    /// <summary>
    /// Tests is_positive_float with various values.
    /// </summary>
    public static void TestIsPositiveFloat()
    {
        if (!Validation.IsPositiveFloat(0.001))
        {
            throw new InvalidOperationException("0.001 should be positive");
        }
        if (!Validation.IsPositiveFloat(100.0))
        {
            throw new InvalidOperationException("100.0 should be positive");
        }
        if (Validation.IsPositiveFloat(0.0))
        {
            throw new InvalidOperationException("0.0 should not be positive");
        }
        if (Validation.IsPositiveFloat(-0.001))
        {
            throw new InvalidOperationException("-0.001 should not be positive");
        }
    }

    /// <summary>
    /// Tests is_non_negative_float with various values.
    /// </summary>
    public static void TestIsNonNegativeFloat()
    {
        if (!Validation.IsNonNegativeFloat(0.0))
        {
            throw new InvalidOperationException("0.0 should be non-negative");
        }
        if (!Validation.IsNonNegativeFloat(100.0))
        {
            throw new InvalidOperationException("100.0 should be non-negative");
        }
        if (Validation.IsNonNegativeFloat(-0.001))
        {
            throw new InvalidOperationException("-0.001 should not be non-negative");
        }
    }

    /// <summary>
    /// Tests is_positive_int with various values.
    /// </summary>
    public static void TestIsPositiveInt()
    {
        if (!Validation.IsPositiveInt(1))
        {
            throw new InvalidOperationException("1 should be positive");
        }
        if (!Validation.IsPositiveInt(100))
        {
            throw new InvalidOperationException("100 should be positive");
        }
        if (Validation.IsPositiveInt(0))
        {
            throw new InvalidOperationException("0 should not be positive");
        }
        if (Validation.IsPositiveInt(-1))
        {
            throw new InvalidOperationException("-1 should not be positive");
        }
    }

    /// <summary>
    /// Tests is_non_negative_int with various values.
    /// </summary>
    public static void TestIsNonNegativeInt()
    {
        if (!Validation.IsNonNegativeInt(0))
        {
            throw new InvalidOperationException("0 should be non-negative");
        }
        if (!Validation.IsNonNegativeInt(100))
        {
            throw new InvalidOperationException("100 should be non-negative");
        }
        if (Validation.IsNonNegativeInt(-1))
        {
            throw new InvalidOperationException("-1 should not be non-negative");
        }
    }

    /// <summary>
    /// Tests is_in_range_float with boundary and interior values.
    /// </summary>
    public static void TestIsInRangeFloat()
    {
        if (!Validation.IsInRangeFloat(5.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("5.0 should be in range [0.0, 10.0]");
        }
        if (!Validation.IsInRangeFloat(0.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("0.0 should be in range [0.0, 10.0]");
        }
        if (!Validation.IsInRangeFloat(10.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("10.0 should be in range [0.0, 10.0]");
        }
        if (Validation.IsInRangeFloat(-0.1, 0.0, 10.0))
        {
            throw new InvalidOperationException("-0.1 should not be in range [0.0, 10.0]");
        }
        if (Validation.IsInRangeFloat(10.1, 0.0, 10.0))
        {
            throw new InvalidOperationException("10.1 should not be in range [0.0, 10.0]");
        }
    }

    /// <summary>
    /// Tests is_in_range_int with boundary and interior values.
    /// </summary>
    public static void TestIsInRangeInt()
    {
        if (!Validation.IsInRangeInt(5, 0, 10))
        {
            throw new InvalidOperationException("5 should be in range [0, 10]");
        }
        if (!Validation.IsInRangeInt(0, 0, 10))
        {
            throw new InvalidOperationException("0 should be in range [0, 10]");
        }
        if (!Validation.IsInRangeInt(10, 0, 10))
        {
            throw new InvalidOperationException("10 should be in range [0, 10]");
        }
        if (Validation.IsInRangeInt(-1, 0, 10))
        {
            throw new InvalidOperationException("-1 should not be in range [0, 10]");
        }
        if (Validation.IsInRangeInt(11, 0, 10))
        {
            throw new InvalidOperationException("11 should not be in range [0, 10]");
        }
    }

    /// <summary>
    /// Tests is_not_empty_string with various strings.
    /// </summary>
    public static void TestIsNotEmptyString()
    {
        if (!Validation.IsNotEmptyString("hello"))
        {
            throw new InvalidOperationException("'hello' should not be empty");
        }
        if (!Validation.IsNotEmptyString(" "))
        {
            throw new InvalidOperationException("' ' should not be empty");
        }
        if (Validation.IsNotEmptyString(""))
        {
            throw new InvalidOperationException("'' should be empty");
        }
    }

    /// <summary>
    /// Tests is_not_empty_array with various arrays.
    /// </summary>
    public static void TestIsNotEmptyArray()
    {
        if (!Validation.IsNotEmptyArray(new int[] { 1, 2, 3 }))
        {
            throw new InvalidOperationException("[1, 2, 3] should not be empty");
        }
        if (!Validation.IsNotEmptyArray(new object[] { null }))
        {
            throw new InvalidOperationException("[null] should not be empty");
        }
        if (Validation.IsNotEmptyArray(Array.Empty<object>()))
        {
            throw new InvalidOperationException("[] should be empty");
        }
    }

    /// <summary>
    /// Tests is_valid_enum with mock enum size.
    /// </summary>
    public static void TestIsValidEnum()
    {
        int enumSize = 5;
        if (!Validation.IsValidEnum(0, enumSize))
        {
            throw new InvalidOperationException("0 should be valid for enum size 5");
        }
        if (!Validation.IsValidEnum(4, enumSize))
        {
            throw new InvalidOperationException("4 should be valid for enum size 5");
        }
        if (Validation.IsValidEnum(-1, enumSize))
        {
            throw new InvalidOperationException("-1 should not be valid for enum size 5");
        }
        if (Validation.IsValidEnum(5, enumSize))
        {
            throw new InvalidOperationException("5 should not be valid for enum size 5");
        }
    }

    /// <summary>
    /// Tests is_valid_seed accepts all integers.
    /// </summary>
    public static void TestIsValidSeed()
    {
        if (!Validation.IsValidSeed(0))
        {
            throw new InvalidOperationException("0 should be a valid seed");
        }
        if (!Validation.IsValidSeed(12345))
        {
            throw new InvalidOperationException("12345 should be a valid seed");
        }
        if (!Validation.IsValidSeed(-12345))
        {
            throw new InvalidOperationException("-12345 should be a valid seed");
        }
        if (!Validation.IsValidSeed(9223372036854775807))
        {
            throw new InvalidOperationException("9223372036854775807 should be a valid seed");
        }
    }
}
