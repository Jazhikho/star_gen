#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Celestial;
using StarGen.Domain.Editing;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for EditRegenerator.
/// </summary>
public static class TestEditRegenerator
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests planet regeneration produces valid body.
    /// </summary>
    public static void TestPlanetRegenerationProducesValidBody()
    {
        ConstraintSet cs = new ConstraintSet();
        RegenerateResult result = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 42, null);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
        if (result.Body == null)
        {
            throw new InvalidOperationException("Expected non-null body");
        }
        if (result.Body.Type != CelestialType.Type.Planet)
        {
            throw new InvalidOperationException($"Expected type Planet, got {result.Body.Type}");
        }
    }

    /// <summary>
    /// Tests star regeneration produces valid body.
    /// </summary>
    public static void TestStarRegenerationProducesValidBody()
    {
        ConstraintSet cs = new ConstraintSet();
        RegenerateResult result = EditRegenerator.Regenerate(CelestialType.Type.Star, cs, 42, null);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
        if (result.Body.Type != CelestialType.Type.Star)
        {
            throw new InvalidOperationException($"Expected type Star, got {result.Body.Type}");
        }
        if (!result.Body.HasStellar())
        {
            throw new InvalidOperationException("Expected stellar properties");
        }
    }

    /// <summary>
    /// Tests locked orbital eccentricity survives planet regeneration.
    /// </summary>
    public static void TestLockedOrbitalEccentricitySurvivesPlanetRegeneration()
    {
        double targetEcc = 0.42;
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint(
            "orbital.eccentricity", 0.0, 0.99, targetEcc, true, ""
        ));
        RegenerateResult result = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 7, null);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
        if (result.Body.Orbital == null)
        {
            throw new InvalidOperationException("Expected orbital properties");
        }
        if (System.Math.Abs(result.Body.Orbital.Eccentricity - targetEcc) > 1e-6)
        {
            throw new InvalidOperationException("locked eccentricity must survive regeneration unchanged");
        }
    }

    /// <summary>
    /// Tests locked semi major axis survives planet regeneration.
    /// </summary>
    public static void TestLockedSemiMajorAxisSurvivesPlanetRegeneration()
    {
        double targetSma = 2.0 * 1.496e11;
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint(
            "orbital.semi_major_axis_m", 1.0e9, 1.0e14, targetSma, true, ""
        ));
        RegenerateResult result = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 99, null);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
        if (System.Math.Abs(result.Body.Orbital.SemiMajorAxisM - targetSma) > 1.0)
        {
            throw new InvalidOperationException($"Expected SMA {targetSma}, got {result.Body.Orbital.SemiMajorAxisM}");
        }
    }

    /// <summary>
    /// Tests locked star mass survives regeneration.
    /// </summary>
    public static void TestLockedStarMassSurvivesRegeneration()
    {
        double targetMassKg = 2.0 * 1.989e30;
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint(
            "physical.mass_kg", 1e29, 1e32, targetMassKg, true, ""
        ));
        RegenerateResult result = EditRegenerator.Regenerate(CelestialType.Type.Star, cs, 13, null);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }
        if (System.Math.Abs(result.Body.Physical.MassKg - targetMassKg) > 1e20)
        {
            throw new InvalidOperationException("locked star mass must survive regeneration");
        }
    }

    /// <summary>
    /// Tests same seed same locks is deterministic.
    /// </summary>
    public static void TestSameSeedSameLocksIsDeterministic()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint(
            "orbital.eccentricity", 0.0, 0.99, 0.1, true, ""
        ));
        RegenerateResult a = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 1234, null);
        RegenerateResult b = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 1234, null);
        if (!a.Success || !b.Success)
        {
            throw new InvalidOperationException("Both regenerations should succeed");
        }
        if (System.Math.Abs(a.Body.Physical.MassKg - b.Body.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException("identical seed + locks must yield identical mass");
        }
        if (System.Math.Abs(a.Body.Physical.RadiusM - b.Body.Physical.RadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException("identical seed + locks must yield identical radius");
        }
    }

    /// <summary>
    /// Tests different seeds change unlocked properties.
    /// </summary>
    public static void TestDifferentSeedsChangeUnlockedProperties()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint(
            "orbital.eccentricity", 0.0, 0.99, 0.1, true, ""
        ));
        RegenerateResult a = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 1, null);
        RegenerateResult b = EditRegenerator.Regenerate(CelestialType.Type.Planet, cs, 2, null);
        if (!a.Success || !b.Success)
        {
            throw new InvalidOperationException("Both regenerations should succeed");
        }
        if (System.Math.Abs(a.Body.Orbital.Eccentricity - b.Body.Orbital.Eccentricity) > 1e-9)
        {
            throw new InvalidOperationException($"Expected locked eccentricity to match, got {a.Body.Orbital.Eccentricity} vs {b.Body.Orbital.Eccentricity}");
        }
        if (a.Body.Physical.MassKg == b.Body.Physical.MassKg)
        {
            throw new InvalidOperationException("unlocked mass should vary with seed");
        }
    }

    /// <summary>
    /// Tests unsupported type returns error.
    /// </summary>
    public static void TestUnsupportedTypeReturnsError()
    {
        ConstraintSet cs = new ConstraintSet();
        RegenerateResult result = EditRegenerator.Regenerate((CelestialType.Type)999, cs, 0, null);
        if (result.Success)
        {
            throw new InvalidOperationException("Expected failure for unsupported type");
        }
        if (string.IsNullOrEmpty(result.ErrorMessage))
        {
            throw new InvalidOperationException("Expected non-empty error message");
        }
    }
}
