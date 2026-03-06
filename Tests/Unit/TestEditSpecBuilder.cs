#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Editing;
using StarGen.Domain.Generation.Specs;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for EditSpecBuilder.
/// </summary>
public static class TestEditSpecBuilder
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Helper to make a constraint set with locked values.
    /// </summary>
    private static ConstraintSet MakeCs(System.Collections.Generic.Dictionary<string, double> locked)
    {
        ConstraintSet cs = new ConstraintSet();
        foreach (var kvp in locked)
        {
            cs.SetConstraint(new PropertyConstraint(kvp.Key, double.NegativeInfinity, double.PositiveInfinity, kvp.Value, true, ""));
        }
        return cs;
    }

    /// <summary>
    /// Tests planet locks pass through unchanged.
    /// </summary>
    public static void TestPlanetLocksPassThroughUnchanged()
    {
        ConstraintSet cs = MakeCs(new System.Collections.Generic.Dictionary<string, double>
        {
            ["physical.mass_kg"] = 5.972e24,
            ["orbital.eccentricity"] = 0.017
        });
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Planet, cs);
        if (System.Math.Abs(overrides["physical.mass_kg"].AsDouble() - 5.972e24) > 1.0)
        {
            throw new InvalidOperationException($"Expected mass 5.972e24, got {overrides["physical.mass_kg"]}");
        }
        if (System.Math.Abs(overrides["orbital.eccentricity"].AsDouble() - 0.017) > 1e-6)
        {
            throw new InvalidOperationException($"Expected eccentricity 0.017, got {overrides["orbital.eccentricity"]}");
        }
        if (overrides.ContainsKey("physical.mass_solar"))
        {
            throw new InvalidOperationException("Should not have physical.mass_solar");
        }
    }

    /// <summary>
    /// Tests star mass lock writes solar alias.
    /// </summary>
    public static void TestStarMassLockWritesSolarAlias()
    {
        double sunMassKg = 1.989e30;
        ConstraintSet cs = MakeCs(new System.Collections.Generic.Dictionary<string, double>
        {
            ["physical.mass_kg"] = sunMassKg
        });
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Star, cs);
        if (!overrides.ContainsKey("physical.mass_kg"))
        {
            throw new InvalidOperationException("base path present");
        }
        if (!overrides.ContainsKey("physical.mass_solar"))
        {
            throw new InvalidOperationException("alias present");
        }
        if (System.Math.Abs(overrides["physical.mass_solar"].AsDouble() - 1.0) > 0.01)
        {
            throw new InvalidOperationException($"Expected mass_solar 1.0, got {overrides["physical.mass_solar"]}");
        }
    }

    /// <summary>
    /// Tests star luminosity lock writes solar alias.
    /// </summary>
    public static void TestStarLuminosityLockWritesSolarAlias()
    {
        double sunLumW = 3.828e26;
        ConstraintSet cs = MakeCs(new System.Collections.Generic.Dictionary<string, double>
        {
            ["stellar.luminosity_watts"] = sunLumW
        });
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Star, cs);
        if (!overrides.ContainsKey("stellar.luminosity_solar"))
        {
            throw new InvalidOperationException("Expected stellar.luminosity_solar");
        }
        if (System.Math.Abs(overrides["stellar.luminosity_solar"].AsDouble() - 1.0) > 0.01)
        {
            throw new InvalidOperationException($"Expected luminosity_solar 1.0, got {overrides["stellar.luminosity_solar"]}");
        }
    }

    /// <summary>
    /// Tests star temperature k written as base path.
    /// </summary>
    public static void TestStarTemperatureKWrittenAsBasePath()
    {
        ConstraintSet cs = MakeCs(new System.Collections.Generic.Dictionary<string, double>
        {
            ["stellar.temperature_k"] = 5778.0
        });
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Star, cs);
        if (!overrides.ContainsKey("stellar.temperature_k"))
        {
            throw new InvalidOperationException("Expected stellar.temperature_k");
        }
        if (System.Math.Abs(overrides["stellar.temperature_k"].AsDouble() - 5778.0) > 0.1)
        {
            throw new InvalidOperationException($"Expected temperature 5778.0, got {overrides["stellar.temperature_k"]}");
        }
    }

    /// <summary>
    /// Tests unlocked properties excluded.
    /// </summary>
    public static void TestUnlockedPropertiesExcluded()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint("physical.mass_kg", 0.0, 1e31, 5.972e24, true, ""));
        cs.SetConstraint(new PropertyConstraint("physical.radius_m", 0.0, 1e10, 6.371e6, false, ""));
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Planet, cs);
        if (!overrides.ContainsKey("physical.mass_kg"))
        {
            throw new InvalidOperationException("Expected physical.mass_kg");
        }
        if (overrides.ContainsKey("physical.radius_m"))
        {
            throw new InvalidOperationException("unlocked radius must not become an override");
        }
    }

    /// <summary>
    /// Tests apply to spec clears and populates.
    /// </summary>
    public static void TestApplyToSpecClearsAndPopulates()
    {
        ConstraintSet cs = MakeCs(new System.Collections.Generic.Dictionary<string, double>
        {
            ["orbital.eccentricity"] = 0.5
        });
        BaseSpec spec = new BaseSpec(12345);
        spec.SetOverride("stale.key", 99.0);
        EditSpecBuilder.ApplyToSpec(spec, CelestialType.Type.Planet, cs);
        if (spec.HasOverride("stale.key"))
        {
            throw new InvalidOperationException("existing overrides cleared");
        }
        if (!spec.HasOverride("orbital.eccentricity"))
        {
            throw new InvalidOperationException("Expected orbital.eccentricity");
        }
        if (System.Math.Abs(spec.GetOverrideFloat("orbital.eccentricity", -1.0) - 0.5) > 1e-6)
        {
            throw new InvalidOperationException($"Expected eccentricity 0.5, got {spec.GetOverrideFloat("orbital.eccentricity", -1.0)}");
        }
    }

    /// <summary>
    /// Tests empty locks produce empty overrides.
    /// </summary>
    public static void TestEmptyLocksProduceEmptyOverrides()
    {
        ConstraintSet cs = new ConstraintSet();
        cs.SetConstraint(new PropertyConstraint("p", 0.0, 1.0, 0.5, false, ""));
        Godot.Collections.Dictionary overrides = EditSpecBuilder.BuildOverrides(CelestialType.Type.Planet, cs);
        if (overrides.Count != 0)
        {
            throw new InvalidOperationException($"Expected empty overrides, got {overrides.Count}");
        }
    }
}
