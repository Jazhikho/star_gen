#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Math;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for MathUtils functions.
/// </summary>
public static class TestMathUtils
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests is_in_range_float with values inside the range.
    /// </summary>
    public static void TestIsInRangeFloatInside()
    {
        if (!MathUtils.IsInRangeFloat(5.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("5.0 should be in range [0.0, 10.0]");
        }
        if (!MathUtils.IsInRangeFloat(0.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("0.0 should be in range [0.0, 10.0]");
        }
        if (!MathUtils.IsInRangeFloat(10.0, 0.0, 10.0))
        {
            throw new InvalidOperationException("10.0 should be in range [0.0, 10.0]");
        }
    }

    /// <summary>
    /// Tests is_in_range_float with values outside the range.
    /// </summary>
    public static void TestIsInRangeFloatOutside()
    {
        if (MathUtils.IsInRangeFloat(-0.1, 0.0, 10.0))
        {
            throw new InvalidOperationException("-0.1 should not be in range [0.0, 10.0]");
        }
        if (MathUtils.IsInRangeFloat(10.1, 0.0, 10.0))
        {
            throw new InvalidOperationException("10.1 should not be in range [0.0, 10.0]");
        }
    }

    /// <summary>
    /// Tests is_in_range_int with values inside the range.
    /// </summary>
    public static void TestIsInRangeIntInside()
    {
        if (!MathUtils.IsInRangeInt(5, 0, 10))
        {
            throw new InvalidOperationException("5 should be in range [0, 10]");
        }
        if (!MathUtils.IsInRangeInt(0, 0, 10))
        {
            throw new InvalidOperationException("0 should be in range [0, 10]");
        }
        if (!MathUtils.IsInRangeInt(10, 0, 10))
        {
            throw new InvalidOperationException("10 should be in range [0, 10]");
        }
    }

    /// <summary>
    /// Tests is_in_range_int with values outside the range.
    /// </summary>
    public static void TestIsInRangeIntOutside()
    {
        if (MathUtils.IsInRangeInt(-1, 0, 10))
        {
            throw new InvalidOperationException("-1 should not be in range [0, 10]");
        }
        if (MathUtils.IsInRangeInt(11, 0, 10))
        {
            throw new InvalidOperationException("11 should not be in range [0, 10]");
        }
    }

    /// <summary>
    /// Tests remap from one range to another.
    /// </summary>
    public static void TestRemapStandard()
    {
        double result = MathUtils.Remap(5.0, 0.0, 10.0, 0.0, 100.0);
        if (System.Math.Abs(result - 50.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 50.0, got {result}");
        }
    }

    /// <summary>
    /// Tests remap at range boundaries.
    /// </summary>
    public static void TestRemapBoundaries()
    {
        double result1 = MathUtils.Remap(0.0, 0.0, 10.0, 0.0, 100.0);
        if (System.Math.Abs(result1 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result1}");
        }

        double result2 = MathUtils.Remap(10.0, 0.0, 10.0, 0.0, 100.0);
        if (System.Math.Abs(result2 - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result2}");
        }
    }

    /// <summary>
    /// Tests remap with inverted target range.
    /// </summary>
    public static void TestRemapInvertedRange()
    {
        double result = MathUtils.Remap(0.0, 0.0, 10.0, 100.0, 0.0);
        if (System.Math.Abs(result - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result}");
        }
    }

    /// <summary>
    /// Tests remap with same source min and max (edge case).
    /// </summary>
    public static void TestRemapZeroSourceRange()
    {
        double result = MathUtils.Remap(5.0, 5.0, 5.0, 0.0, 100.0);
        if (System.Math.Abs(result - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Should return to_min when source range is zero. Expected 0.0, got {result}");
        }
    }

    /// <summary>
    /// Tests remap_clamped stays within target range.
    /// </summary>
    public static void TestRemapClamped()
    {
        double result1 = MathUtils.RemapClamped(15.0, 0.0, 10.0, 0.0, 100.0);
        if (System.Math.Abs(result1 - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result1}");
        }

        double result2 = MathUtils.RemapClamped(-5.0, 0.0, 10.0, 0.0, 100.0);
        if (System.Math.Abs(result2 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result2}");
        }
    }

    /// <summary>
    /// Tests inverse_lerp calculates correct position.
    /// </summary>
    public static void TestInverseLerp()
    {
        double result1 = MathUtils.InverseLerp(0.0, 10.0, 5.0);
        if (System.Math.Abs(result1 - 0.5) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.5, got {result1}");
        }

        double result2 = MathUtils.InverseLerp(0.0, 10.0, 0.0);
        if (System.Math.Abs(result2 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result2}");
        }

        double result3 = MathUtils.InverseLerp(0.0, 10.0, 10.0);
        if (System.Math.Abs(result3 - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 1.0, got {result3}");
        }
    }

    /// <summary>
    /// Tests inverse_lerp with same from and to (edge case).
    /// </summary>
    public static void TestInverseLerpZeroRange()
    {
        double result = MathUtils.InverseLerp(5.0, 5.0, 5.0);
        if (System.Math.Abs(result - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result}");
        }
    }

    /// <summary>
    /// Tests smooth_lerp produces values in expected range.
    /// </summary>
    public static void TestSmoothLerpRange()
    {
        double result1 = MathUtils.SmoothLerp(0.0, 100.0, 0.0);
        if (System.Math.Abs(result1 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result1}");
        }

        double result2 = MathUtils.SmoothLerp(0.0, 100.0, 1.0);
        if (System.Math.Abs(result2 - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result2}");
        }

        double result3 = MathUtils.SmoothLerp(0.0, 100.0, 0.5);
        if (System.Math.Abs(result3 - 50.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 50.0, got {result3}");
        }
    }

    /// <summary>
    /// Tests smooth_lerp clamps weight to [0, 1].
    /// </summary>
    public static void TestSmoothLerpClampedWeight()
    {
        double result1 = MathUtils.SmoothLerp(0.0, 100.0, -1.0);
        if (System.Math.Abs(result1 - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 0.0, got {result1}");
        }

        double result2 = MathUtils.SmoothLerp(0.0, 100.0, 2.0);
        if (System.Math.Abs(result2 - 100.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected 100.0, got {result2}");
        }
    }
}
