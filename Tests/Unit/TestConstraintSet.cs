#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Editing;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for ConstraintSet.
/// </summary>
public static class TestConstraintSet
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Helper to create a constraint.
    /// </summary>
    private static PropertyConstraint MakeConstraint(string path, double minv, double maxv, double cur)
    {
        return new PropertyConstraint(path, minv, maxv, cur);
    }

    /// <summary>
    /// Tests set and get.
    /// </summary>
    public static void TestSetAndGet()
    {
        ConstraintSet cs = new ConstraintSet();
        PropertyConstraint c = MakeConstraint("p", 0.0, 10.0, 5.0);
        cs.SetConstraint(c);
        if (!cs.HasConstraint("p"))
        {
            throw new InvalidOperationException("Expected constraint 'p'");
        }
        PropertyConstraint got = cs.GetConstraint("p");
        if (got == null)
        {
            throw new InvalidOperationException("Expected non-null constraint");
        }
        if (got.PropertyPath != "p")
        {
            throw new InvalidOperationException($"Expected property_path 'p', got '{got.PropertyPath}'");
        }
    }

    /// <summary>
    /// Tests get missing returns null.
    /// </summary>
    public static void TestGetMissingReturnsNull()
    {
        ConstraintSet cs = new ConstraintSet();
        if (cs.GetConstraint("missing") != null)
        {
            throw new InvalidOperationException("Expected null for missing constraint");
        }
        if (cs.HasConstraint("missing"))
        {
            throw new InvalidOperationException("Should not have missing constraint");
        }
    }

    /// <summary>
    /// Tests lock and unlock.
    /// </summary>
    public static void TestLockAndUnlock()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("p", 0.0, 10.0, 5.0));
        if (cs.GetConstraint("p").IsLocked)
        {
            throw new InvalidOperationException("Expected unlocked initially");
        }
        cs.Lock("p");
        if (!cs.GetConstraint("p").IsLocked)
        {
            throw new InvalidOperationException("Expected locked after lock");
        }
        cs.Unlock("p");
        if (cs.GetConstraint("p").IsLocked)
        {
            throw new InvalidOperationException("Expected unlocked after unlock");
        }
    }

    /// <summary>
    /// Tests lock missing is noop.
    /// </summary>
    public static void TestLockMissingIsNoop()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.Lock("missing");
        if (cs.Size() != 0)
        {
            throw new InvalidOperationException($"Expected size 0, got {cs.Size()}");
        }
    }

    /// <summary>
    /// Tests get_locked_paths.
    /// </summary>
    public static void TestGetLockedPaths()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("a", 0.0, 1.0, 0.5));
        cs.SetConstraint(MakeConstraint("b", 0.0, 1.0, 0.5));
        cs.Lock("a");
        List<string> locked = cs.GetLockedPaths();
        if (locked.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 locked path, got {locked.Count}");
        }
        if (locked[0] != "a")
        {
            throw new InvalidOperationException($"Expected locked path 'a', got '{locked[0]}'");
        }
    }

    /// <summary>
    /// Tests get_locked_overrides matches spec format.
    /// </summary>
    public static void TestGetLockedOverridesMatchesSpecFormat()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("physical.mass_kg", 0.0, 1.0e30, 5.0e24));
        cs.SetConstraint(MakeConstraint("physical.radius_m", 0.0, 1.0e8, 6.0e6));
        cs.Lock("physical.mass_kg");

        Godot.Collections.Dictionary overrides = cs.GetLockedOverrides();
        if (overrides.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 override, got {overrides.Count}");
        }
        if (!overrides.ContainsKey("physical.mass_kg"))
        {
            throw new InvalidOperationException("Expected override for 'physical.mass_kg'");
        }
        if (System.Math.Abs(overrides["physical.mass_kg"].AsDouble() - 5.0e24) > 1.0)
        {
            throw new InvalidOperationException($"Expected mass 5.0e24, got {overrides["physical.mass_kg"]}");
        }

        BaseSpec spec = new BaseSpec();
        spec.Overrides = overrides;
        if (!spec.HasOverride("physical.mass_kg"))
        {
            throw new InvalidOperationException("Expected override in BaseSpec");
        }
        if (System.Math.Abs(spec.GetOverrideFloat("physical.mass_kg", 0.0) - 5.0e24) > 1.0)
        {
            throw new InvalidOperationException($"Expected mass 5.0e24, got {spec.GetOverrideFloat("physical.mass_kg", 0.0)}");
        }
    }

    /// <summary>
    /// Tests set_value does not clamp.
    /// </summary>
    public static void TestSetValueDoesNotClamp()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("p", 0.0, 10.0, 5.0));
        cs.SetValue("p", 999.0);
        if (System.Math.Abs(cs.GetConstraint("p").CurrentValue - 999.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("set_value is raw write; caller decides whether to clamp");
        }
    }

    /// <summary>
    /// Tests is_consistent all good.
    /// </summary>
    public static void TestIsConsistentAllGood()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("a", 0.0, 10.0, 5.0));
        cs.SetConstraint(MakeConstraint("b", 0.0, 1.0, 0.5));
        if (!cs.IsConsistent())
        {
            throw new InvalidOperationException("Expected consistent");
        }
    }

    /// <summary>
    /// Tests is_consistent detects out of range.
    /// </summary>
    public static void TestIsConsistentDetectsOutOfRange()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("a", 0.0, 10.0, 15.0));
        if (cs.IsConsistent())
        {
            throw new InvalidOperationException("Expected inconsistent");
        }
        List<string> v = cs.GetViolations();
        if (v.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 violation, got {v.Count}");
        }
        if (v[0] != "a")
        {
            throw new InvalidOperationException($"Expected violation 'a', got '{v[0]}'");
        }
    }

    /// <summary>
    /// Tests is_consistent detects unsatisfiable.
    /// </summary>
    public static void TestIsConsistentDetectsUnsatisfiable()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint("a", 10.0, 0.0, 5.0));
        if (cs.IsConsistent())
        {
            throw new InvalidOperationException("Expected inconsistent");
        }
    }

    /// <summary>
    /// Tests clamp_unlocked clamps only unlocked.
    /// </summary>
    public static void TestClampUnlockedClampsOnlyUnlocked()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("free", 0.0, 10.0, 50.0));
        cs.SetConstraint(MakeConstraint("pinned", 0.0, 10.0, 50.0));
        cs.Lock("pinned");

        List<string> modified = cs.ClampUnlocked();
        if (modified.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 modified, got {modified.Count}");
        }
        if (modified[0] != "free")
        {
            throw new InvalidOperationException($"Expected modified 'free', got '{modified[0]}'");
        }
        if (System.Math.Abs(cs.GetConstraint("free").CurrentValue - 10.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected free clamped to 10.0, got {cs.GetConstraint("free").CurrentValue}");
        }
        if (System.Math.Abs(cs.GetConstraint("pinned").CurrentValue - 50.0) > DefaultTolerance)
        {
            throw new InvalidOperationException("locked values are authoritative and never clamped");
        }
    }

    /// <summary>
    /// Tests clamp_unlocked returns empty when all in range.
    /// </summary>
    public static void TestClampUnlockedReturnsEmptyWhenAllInRange()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(MakeConstraint("a", 0.0, 10.0, 5.0));
        List<string> modified = cs.ClampUnlocked();
        if (modified.Count != 0)
        {
            throw new InvalidOperationException($"Expected 0 modified, got {modified.Count}");
        }
    }
}
