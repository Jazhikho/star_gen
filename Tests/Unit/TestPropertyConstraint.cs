#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Editing;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for PropertyConstraint.
/// </summary>
public static class TestPropertyConstraint
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests in_range when inside.
    /// </summary>
    public static void TestInRangeWhenInside()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 10.0, 5.0);
        if (!c.IsValueInRange())
        {
            throw new InvalidOperationException("Expected value in range");
        }
    }

    /// <summary>
    /// Tests in_range at bounds.
    /// </summary>
    public static void TestInRangeAtBounds()
    {
        PropertyConstraint low = new PropertyConstraint("p", 0.0, 10.0, 0.0);
        PropertyConstraint high = new PropertyConstraint("p", 0.0, 10.0, 10.0);
        if (!low.IsValueInRange())
        {
            throw new InvalidOperationException("min bound is inclusive");
        }
        if (!high.IsValueInRange())
        {
            throw new InvalidOperationException("max bound is inclusive");
        }
    }

    /// <summary>
    /// Tests out of range.
    /// </summary>
    public static void TestOutOfRange()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 10.0, 11.0);
        if (c.IsValueInRange())
        {
            throw new InvalidOperationException("Expected value out of range");
        }
    }

    /// <summary>
    /// Tests clamp_value.
    /// </summary>
    public static void TestClampValue()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 10.0, 5.0);
        if (System.Math.Abs(c.ClampValue(-5.0) - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected clamped -5.0 to 0.0, got {c.ClampValue(-5.0)}");
        }
        if (System.Math.Abs(c.ClampValue(15.0) - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected clamped 15.0 to 10.0, got {c.ClampValue(15.0)}");
        }
        if (System.Math.Abs(c.ClampValue(5.0) - 5.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected clamped 5.0 to 5.0, got {c.ClampValue(5.0)}");
        }
    }

    /// <summary>
    /// Tests has_bounds with infinities.
    /// </summary>
    public static void TestHasBoundsWithInfinities()
    {
        PropertyConstraint unbounded = new PropertyConstraint("p", double.NegativeInfinity, double.PositiveInfinity, 0.0);
        PropertyConstraint bounded = new PropertyConstraint("p", 0.0, double.PositiveInfinity, 0.0);
        if (unbounded.HasBounds())
        {
            throw new InvalidOperationException("Expected unbounded to not have bounds");
        }
        if (!bounded.HasBounds())
        {
            throw new InvalidOperationException("Expected bounded to have bounds");
        }
    }

    /// <summary>
    /// Tests with_lock returns new instance.
    /// </summary>
    public static void TestWithLockReturnsNewInstance()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 10.0, 5.0, false);
        PropertyConstraint locked = c.WithLock(true);
        if (c.IsLocked)
        {
            throw new InvalidOperationException("original unchanged");
        }
        if (!locked.IsLocked)
        {
            throw new InvalidOperationException("Expected locked");
        }
        if (locked.PropertyPath != c.PropertyPath)
        {
            throw new InvalidOperationException($"Expected property_path '{c.PropertyPath}', got '{locked.PropertyPath}'");
        }
    }

    /// <summary>
    /// Tests with_value returns new instance.
    /// </summary>
    public static void TestWithValueReturnsNewInstance()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 10.0, 5.0);
        PropertyConstraint updated = c.WithValue(7.0);
        if (System.Math.Abs(c.CurrentValue - 5.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("original unchanged");
        }
        if (System.Math.Abs(updated.CurrentValue - 7.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected current_value 7.0, got {updated.CurrentValue}");
        }
    }

    /// <summary>
    /// Tests intersected_with narrows range.
    /// </summary>
    public static void TestIntersectedWithNarrowsRange()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 100.0, 50.0);
        PropertyConstraint narrowed = c.IntersectedWith(10.0, 80.0);
        if (System.Math.Abs(narrowed.MinValue - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min_value 10.0, got {narrowed.MinValue}");
        }
        if (System.Math.Abs(narrowed.MaxValue - 80.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max_value 80.0, got {narrowed.MaxValue}");
        }
    }

    /// <summary>
    /// Tests intersected_with cannot widen.
    /// </summary>
    public static void TestIntersectedWithCannotWiden()
    {
        PropertyConstraint c = new PropertyConstraint("p", 10.0, 20.0, 15.0);
        PropertyConstraint attempt = c.IntersectedWith(0.0, 100.0);
        if (System.Math.Abs(attempt.MinValue - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min_value 10.0, got {attempt.MinValue}");
        }
        if (System.Math.Abs(attempt.MaxValue - 20.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max_value 20.0, got {attempt.MaxValue}");
        }
    }

    /// <summary>
    /// Tests intersected_with appends reason only when narrowed.
    /// </summary>
    public static void TestIntersectedWithAppendsReasonOnlyWhenNarrowed()
    {
        PropertyConstraint c = new PropertyConstraint("p", 0.0, 100.0, 50.0, false, "base");
        PropertyConstraint narrowed = c.IntersectedWith(10.0, 90.0, "lock");
        if (!narrowed.ConstraintReason.Contains("lock"))
        {
            throw new InvalidOperationException("reason appended on narrow");
        }

        PropertyConstraint unchanged = c.IntersectedWith(-10.0, 200.0, "noop");
        if (unchanged.ConstraintReason.Contains("noop"))
        {
            throw new InvalidOperationException("reason NOT appended when no narrowing");
        }
    }

    /// <summary>
    /// Tests is_satisfiable.
    /// </summary>
    public static void TestIsSatisfiable()
    {
        PropertyConstraint good = new PropertyConstraint("p", 0.0, 10.0, 5.0);
        PropertyConstraint bad = new PropertyConstraint("p", 10.0, 0.0, 5.0);
        if (!good.IsSatisfiable())
        {
            throw new InvalidOperationException("Expected satisfiable");
        }
        if (bad.IsSatisfiable())
        {
            throw new InvalidOperationException("Expected unsatisfiable");
        }
    }

    /// <summary>
    /// Tests to_dict round trip fields.
    /// </summary>
    public static void TestToDictRoundTripFields()
    {
        PropertyConstraint c = new PropertyConstraint("x.y", 1.0, 2.0, 1.5, true, "reason");
        Godot.Collections.Dictionary d = c.ToDictionary();
        if (d["property_path"].AsString() != "x.y")
        {
            throw new InvalidOperationException($"Expected property_path 'x.y', got '{d["property_path"]}'");
        }
        if (System.Math.Abs(d["min_value"].AsDouble() - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min_value 1.0, got {d["min_value"]}");
        }
        if (System.Math.Abs(d["max_value"].AsDouble() - 2.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max_value 2.0, got {d["max_value"]}");
        }
        if (System.Math.Abs(d["current_value"].AsDouble() - 1.5) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected current_value 1.5, got {d["current_value"]}");
        }
        if (!d["is_locked"].AsBool())
        {
            throw new InvalidOperationException("Expected is_locked true");
        }
        if (d["constraint_reason"].AsString() != "reason")
        {
            throw new InvalidOperationException($"Expected constraint_reason 'reason', got '{d["constraint_reason"]}'");
        }
    }
}
