#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Editing;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for PropertyConstraintSolver.
/// </summary>
public static class TestPropertyConstraintSolver
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Earth-like current values in base SI units.
    /// </summary>
    private static System.Collections.Generic.Dictionary<string, double> EarthValues()
    {
        return new System.Collections.Generic.Dictionary<string, double>
        {
            ["physical.mass_kg"] = 5.972e24,
            ["physical.radius_m"] = 6.371e6,
            ["physical.rotation_period_s"] = 86164.0,
            ["physical.axial_tilt_deg"] = 23.44,
            ["physical.oblateness"] = 0.00335,
        };
    }

    /// <summary>
    /// Tests seeds physical constraints for planet.
    /// </summary>
    public static void TestSeedsPhysicalConstraintsForPlanet()
    {
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, EarthValues(), new List<string>()
        );
        if (!cs.HasConstraint("physical.mass_kg"))
        {
            throw new InvalidOperationException("Expected physical.mass_kg constraint");
        }
        if (!cs.HasConstraint("physical.radius_m"))
        {
            throw new InvalidOperationException("Expected physical.radius_m constraint");
        }
        if (!cs.HasConstraint("physical.rotation_period_s"))
        {
            throw new InvalidOperationException("Expected physical.rotation_period_s constraint");
        }
        if (!cs.HasConstraint("physical.axial_tilt_deg"))
        {
            throw new InvalidOperationException("Expected physical.axial_tilt_deg constraint");
        }
        if (!cs.HasConstraint("physical.oblateness"))
        {
            throw new InvalidOperationException("Expected physical.oblateness constraint");
        }
    }

    /// <summary>
    /// Tests earth values fall within planet bounds.
    /// </summary>
    public static void TestEarthValuesFallWithinPlanetBounds()
    {
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, EarthValues(), new List<string>()
        );
        if (!cs.IsConsistent())
        {
            throw new InvalidOperationException("Earth should satisfy planet bounds");
        }
    }

    /// <summary>
    /// Tests star has stellar constraints planet does not.
    /// </summary>
    public static void TestStarHasStellarConstraintsPlanetDoesNot()
    {
        ConstraintSet starCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Star, new System.Collections.Generic.Dictionary<string, double>(), new List<string>()
        );
        ConstraintSet planetCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, new System.Collections.Generic.Dictionary<string, double>(), new List<string>()
        );
        if (!starCs.HasConstraint("stellar.temperature_k"))
        {
            throw new InvalidOperationException("Expected stellar.temperature_k constraint for star");
        }
        if (planetCs.HasConstraint("stellar.temperature_k"))
        {
            throw new InvalidOperationException("Planet should not have stellar.temperature_k constraint");
        }
    }

    /// <summary>
    /// Tests locking mass narrows radius range.
    /// </summary>
    public static void TestLockingMassNarrowsRadiusRange()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> noLocks = new List<string>();
        List<string> massLocked = new List<string> { "physical.mass_kg" };

        ConstraintSet unlockedCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, noLocks
        );
        ConstraintSet lockedCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, massLocked
        );

        PropertyConstraint rFree = unlockedCs.GetConstraint("physical.radius_m");
        PropertyConstraint rConstrained = lockedCs.GetConstraint("physical.radius_m");

        if (rConstrained.MinValue < rFree.MinValue)
        {
            throw new InvalidOperationException("locking mass should not lower radius min");
        }
        if (rConstrained.MaxValue > rFree.MaxValue)
        {
            throw new InvalidOperationException("locking mass should not raise radius max");
        }

        bool narrowed = (rConstrained.MinValue > rFree.MinValue) || (rConstrained.MaxValue < rFree.MaxValue);
        if (!narrowed)
        {
            throw new InvalidOperationException("locking mass should narrow radius range");
        }
    }

    /// <summary>
    /// Tests earth radius still valid after mass lock.
    /// </summary>
    public static void TestEarthRadiusStillValidAfterMassLock()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> massLocked = new List<string> { "physical.mass_kg" };
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, massLocked
        );
        PropertyConstraint r = cs.GetConstraint("physical.radius_m");
        if (!r.IsValueInRange())
        {
            throw new InvalidOperationException("Earth radius must remain valid when Earth mass is locked");
        }
    }

    /// <summary>
    /// Tests locking radius narrows mass range.
    /// </summary>
    public static void TestLockingRadiusNarrowsMassRange()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> radiusLocked = new List<string> { "physical.radius_m" };

        ConstraintSet unlockedCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, new List<string>()
        );
        ConstraintSet lockedCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, radiusLocked
        );

        PropertyConstraint mFree = unlockedCs.GetConstraint("physical.mass_kg");
        PropertyConstraint mConstrained = lockedCs.GetConstraint("physical.mass_kg");

        if (mConstrained.MinValue < mFree.MinValue)
        {
            throw new InvalidOperationException("locking radius should not lower mass min");
        }
        if (mConstrained.MaxValue > mFree.MaxValue)
        {
            throw new InvalidOperationException("locking radius should not raise mass max");
        }
    }

    /// <summary>
    /// Tests locking slow rotation caps oblateness.
    /// </summary>
    public static void TestLockingSlowRotationCapsOblateness()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        values["physical.rotation_period_s"] = 500.0 * 3600.0;
        List<string> rotLocked = new List<string> { "physical.rotation_period_s" };

        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, rotLocked
        );
        PropertyConstraint obl = cs.GetConstraint("physical.oblateness");
        if (obl.MaxValue >= 0.1)
        {
            throw new InvalidOperationException($"Very slow rotation should cap oblateness well below 0.1, got {obl.MaxValue}");
        }
    }

    /// <summary>
    /// Tests locking fast rotation allows high oblateness.
    /// </summary>
    public static void TestLockingFastRotationAllowsHighOblateness()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        values["physical.rotation_period_s"] = 1.5 * 3600.0;
        List<string> rotLocked = new List<string> { "physical.rotation_period_s" };

        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, rotLocked
        );
        PropertyConstraint obl = cs.GetConstraint("physical.oblateness");
        if (System.Math.Abs(obl.MaxValue - 0.5) > 0.01)
        {
            throw new InvalidOperationException($"Fast rotation should keep the full range available. Expected ~0.5, got {obl.MaxValue}");
        }
    }

    /// <summary>
    /// Tests both mass and radius locked no narrowing.
    /// </summary>
    public static void TestBothMassAndRadiusLockedNoNarrowing()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> bothLocked = new List<string> { "physical.mass_kg", "physical.radius_m" };

        ConstraintSet freeCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, new List<string>()
        );
        ConstraintSet bothCs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, bothLocked
        );

        PropertyConstraint rFree = freeCs.GetConstraint("physical.radius_m");
        PropertyConstraint rBoth = bothCs.GetConstraint("physical.radius_m");
        if (System.Math.Abs(rBoth.MinValue - rFree.MinValue) > 1.0)
        {
            throw new InvalidOperationException($"Expected min_value ~{rFree.MinValue}, got {rBoth.MinValue}");
        }
        if (System.Math.Abs(rBoth.MaxValue - rFree.MaxValue) > 1.0)
        {
            throw new InvalidOperationException($"Expected max_value ~{rFree.MaxValue}, got {rBoth.MaxValue}");
        }
    }

    /// <summary>
    /// Tests locked paths are marked locked in output.
    /// </summary>
    public static void TestLockedPathsAreMarkedLockedInOutput()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> massLocked = new List<string> { "physical.mass_kg" };
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, massLocked
        );
        if (!cs.GetConstraint("physical.mass_kg").IsLocked)
        {
            throw new InvalidOperationException("Expected physical.mass_kg locked");
        }
        if (cs.GetConstraint("physical.radius_m").IsLocked)
        {
            throw new InvalidOperationException("Expected physical.radius_m unlocked");
        }
    }

    /// <summary>
    /// Tests determinism same input same output.
    /// </summary>
    public static void TestDeterminismSameInputSameOutput()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        List<string> locked = new List<string> { "physical.mass_kg" };

        ConstraintSet a = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, locked
        );
        ConstraintSet b = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, locked
        );

        foreach (string path in a.GetAllPaths())
        {
            PropertyConstraint ca = a.GetConstraint(path);
            PropertyConstraint cb = b.GetConstraint(path);
            if (cb == null)
            {
                throw new InvalidOperationException($"path present in both: {path}");
            }
            if (System.Math.Abs(ca.MinValue - cb.MinValue) > DefaultTolerance)
            {
                throw new InvalidOperationException($"{path} min mismatch");
            }
            if (System.Math.Abs(ca.MaxValue - cb.MaxValue) > DefaultTolerance)
            {
                throw new InvalidOperationException($"{path} max mismatch");
            }
        }
    }

    /// <summary>
    /// Tests axial tilt matches validator bounds.
    /// </summary>
    public static void TestAxialTiltMatchesValidatorBounds()
    {
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, new System.Collections.Generic.Dictionary<string, double>(), new List<string>()
        );
        PropertyConstraint tilt = cs.GetConstraint("physical.axial_tilt_deg");
        if (System.Math.Abs(tilt.MinValue - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 0.0, got {tilt.MinValue}");
        }
        if (System.Math.Abs(tilt.MaxValue - 180.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max 180.0, got {tilt.MaxValue}");
        }
    }

    /// <summary>
    /// Tests albedo matches validator bounds.
    /// </summary>
    public static void TestAlbedoMatchesValidatorBounds()
    {
        ConstraintSet cs = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, new System.Collections.Generic.Dictionary<string, double>(), new List<string>()
        );
        PropertyConstraint albedo = cs.GetConstraint("surface.albedo");
        if (System.Math.Abs(albedo.MinValue - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 0.0, got {albedo.MinValue}");
        }
        if (System.Math.Abs(albedo.MaxValue - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max 1.0, got {albedo.MaxValue}");
        }
    }

    /// <summary>
    /// Tests extra constraints narrow radius.
    /// </summary>
    public static void TestExtraConstraintsNarrowRadius()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        System.Collections.Generic.Dictionary<string, Vector2> extra = new System.Collections.Generic.Dictionary<string, Vector2>
        {
            ["physical.radius_m"] = new Vector2(6.0e6f, 6.5e6f)
        };
        ConstraintSet cs = PropertyConstraintSolver.SolveWithExtraConstraints(
            CelestialType.Type.Planet, values, new List<string>(), extra
        );
        PropertyConstraint r = cs.GetConstraint("physical.radius_m");
        if (r.MinValue < 6.0e6)
        {
            throw new InvalidOperationException("extra min applied");
        }
        if (r.MaxValue > 6.5e6)
        {
            throw new InvalidOperationException("extra max applied");
        }
    }

    /// <summary>
    /// Tests extra constraints feed into coupling.
    /// </summary>
    public static void TestExtraConstraintsFeedIntoCoupling()
    {
        System.Collections.Generic.Dictionary<string, double> values = EarthValues();
        System.Collections.Generic.Dictionary<string, Vector2> extra = new System.Collections.Generic.Dictionary<string, Vector2>
        {
            ["physical.radius_m"] = new Vector2(6.3e6f, 6.4e6f)
        };
        List<string> locked = new List<string> { "physical.radius_m" };
        ConstraintSet cs = PropertyConstraintSolver.SolveWithExtraConstraints(
            CelestialType.Type.Planet, values, locked, extra
        );
        PropertyConstraint m = cs.GetConstraint("physical.mass_kg");
        PropertyConstraint mFree = PropertyConstraintSolver.Solve(
            CelestialType.Type.Planet, values, new List<string>()
        ).GetConstraint("physical.mass_kg");
        if (m.MaxValue >= mFree.MaxValue)
        {
            throw new InvalidOperationException("coupling should tighten mass from narrowed radius");
        }
    }
}
