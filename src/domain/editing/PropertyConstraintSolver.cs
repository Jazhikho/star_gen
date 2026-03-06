using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;

namespace StarGen.Domain.Editing;

/// <summary>
/// Computes valid property ranges given a body type and a set of locked values.
/// Pure domain logic: no Nodes, no RNG, no file IO.
/// </summary>
public static class PropertyConstraintSolver
{
    private const double StarDensityMin = 0.0001;
    private const double StarDensityMax = 150000.0;
    private const double MoonDensityMin = 800.0;
    private const double MoonDensityMax = 6000.0;
    private const double AsteroidDensityMin = 500.0;
    private const double AsteroidDensityMax = 8000.0;
    private const double FallbackDensityMin = 500.0;
    private const double FallbackDensityMax = 10000.0;

    private static readonly double StarMassMin = 0.08 * 1.989e30;
    private static readonly double StarMassMax = 300.0 * 1.989e30;
    private static readonly double PlanetMassMin = 0.0001 * 5.972e24;
    private static readonly double PlanetMassMax = 5000.0 * 5.972e24;
    private static readonly double MoonMassMin = 1.0e15;
    private static readonly double MoonMassMax = 2.0 * 5.972e24;
    private static readonly double AsteroidMassMin = 1.0e10;
    private static readonly double AsteroidMassMax = 1.0e22;

    private static readonly double StarRadiusMin = 0.001 * 6.957e8;
    private static readonly double StarRadiusMax = 2000.0 * 6.957e8;
    private static readonly double PlanetRadiusMin = 0.01 * 6.371e6;
    private static readonly double PlanetRadiusMax = 30.0 * 6.371e6;
    private static readonly double MoonRadiusMin = 1.0e3;
    private static readonly double MoonRadiusMax = 5.0e6;
    private static readonly double AsteroidRadiusMin = 1.0;
    private static readonly double AsteroidRadiusMax = 1.0e6;

    private const double RotationMinS = 360.0;
    private const double RotationMaxS = 3.6e7;
    private const double StellarTempMinK = 2000.0;
    private const double StellarTempMaxK = 50000.0;
    private static readonly double StellarLumMinW = 1.0e-5 * 3.828e26;
    private static readonly double StellarLumMaxW = 1.0e7 * 3.828e26;
    private const double StellarAgeMinYr = 1.0e6;
    private const double StellarAgeMaxYr = 1.5e10;

    /// <summary>Builds a ConstraintSet for the given body type, current values, and locked paths.</summary>
    public static ConstraintSet Solve(
        CelestialType.Type bodyType,
        Dictionary currentValues,
        System.Collections.Generic.List<string> lockedPaths)
    {
        ConstraintSet cs = new ConstraintSet();
        SeedAbsoluteBounds(bodyType, currentValues, cs);

        foreach (string path in lockedPaths)
        {
            cs.Lock(path);
        }

        ApplyCoupling(bodyType, cs);
        return cs;
    }

    /// <summary>
    /// Compatibility overload that accepts plain .NET dictionaries for current values.
    /// </summary>
    public static ConstraintSet Solve(
        CelestialType.Type bodyType,
        System.Collections.Generic.Dictionary<string, double> currentValues,
        System.Collections.Generic.List<string> lockedPaths)
    {
        Dictionary values = new();
        foreach (KeyValuePair<string, double> kv in currentValues)
        {
            values[kv.Key] = kv.Value;
        }

        return Solve(bodyType, values, lockedPaths);
    }

    /// <summary>Builds a ConstraintSet with extra per-property bounds (e.g. from Traveller size code).</summary>
    public static ConstraintSet SolveWithExtraConstraints(
        CelestialType.Type bodyType,
        Dictionary currentValues,
        System.Collections.Generic.List<string> lockedPaths,
        Dictionary extraBounds)
    {
        ConstraintSet cs = new ConstraintSet();
        SeedAbsoluteBounds(bodyType, currentValues, cs);

        foreach (Variant pathV in extraBounds.Keys)
        {
            string pathStr = (string)pathV;
            PropertyConstraint? c = cs.GetConstraint(pathStr);
            if (c == null)
            {
                continue;
            }

            Vector2 bounds = (Vector2)extraBounds[pathV];
            cs.SetConstraint(c.IntersectedWith(bounds.X, bounds.Y, "Traveller UWP"));
        }

        foreach (string path in lockedPaths)
        {
            cs.Lock(path);
        }

        ApplyCoupling(bodyType, cs);
        return cs;
    }

    /// <summary>
    /// Compatibility overload that accepts .NET dictionaries for values and extra bounds.
    /// </summary>
    public static ConstraintSet SolveWithExtraConstraints(
        CelestialType.Type bodyType,
        System.Collections.Generic.Dictionary<string, double> currentValues,
        System.Collections.Generic.List<string> lockedPaths,
        System.Collections.Generic.Dictionary<string, Vector2> extraBounds)
    {
        Dictionary values = new();
        foreach (KeyValuePair<string, double> kv in currentValues)
        {
            values[kv.Key] = kv.Value;
        }

        Dictionary bounds = new();
        foreach (KeyValuePair<string, Vector2> kv in extraBounds)
        {
            bounds[kv.Key] = kv.Value;
        }

        return SolveWithExtraConstraints(bodyType, values, lockedPaths, bounds);
    }

    /// <summary>Applies all inter-property coupling rules to the constraint set.</summary>
    private static void ApplyCoupling(CelestialType.Type bodyType, ConstraintSet cs)
    {
        ApplyMassRadiusCoupling(bodyType, cs);
        ApplyOblatenessRotationCoupling(cs);
    }

    /// <summary>Narrows mass or radius bounds using density constraints when one is locked.</summary>
    private static void ApplyMassRadiusCoupling(CelestialType.Type bodyType, ConstraintSet cs)
    {
        PropertyConstraint? massC = cs.GetConstraint("physical.mass_kg");
        PropertyConstraint? radiusC = cs.GetConstraint("physical.radius_m");
        if (massC == null || radiusC == null)
        {
            return;
        }

        Vector2 densityBounds = DensityBoundsFor(bodyType, massC.CurrentValue);

        if (massC.IsLocked && !radiusC.IsLocked)
        {
            double rMin = RadiusFromMassDensity(massC.CurrentValue, densityBounds.Y);
            double rMax = RadiusFromMassDensity(massC.CurrentValue, densityBounds.X);
            cs.SetConstraint(radiusC.IntersectedWith(rMin, rMax, "density bounds from locked mass"));
        }
        else if (radiusC.IsLocked && !massC.IsLocked)
        {
            double vol = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(radiusC.CurrentValue, 3.0);
            double mMin = vol * densityBounds.X;
            double mMax = vol * densityBounds.Y;
            cs.SetConstraint(massC.IntersectedWith(mMin, mMax, "density bounds from locked radius"));
        }
    }

    /// <summary>Tightens oblateness upper bound when rotation period is locked — faster rotation permits greater flattening.</summary>
    private static void ApplyOblatenessRotationCoupling(ConstraintSet cs)
    {
        PropertyConstraint? rotC = cs.GetConstraint("physical.rotation_period_s");
        PropertyConstraint? oblC = cs.GetConstraint("physical.oblateness");
        if (rotC == null || oblC == null)
        {
            return;
        }

        if (!rotC.IsLocked)
        {
            return;
        }

        double hours = System.Math.Abs(rotC.CurrentValue) / 3600.0;
        double maxObl = 0.5;
        if (hours > 2.0)
        {
            double t = System.Math.Clamp((hours - 2.0) / 198.0, 0.0, 1.0);
            maxObl = 0.5 + (0.02 - 0.5) * t;
        }

        cs.SetConstraint(oblC.IntersectedWith(0.0, maxObl, "rotation period limits oblateness"));
    }

    /// <summary>Populates the constraint set with absolute physical and orbital bounds for the given body type.</summary>
    private static void SeedAbsoluteBounds(CelestialType.Type bodyType, Dictionary currentValues, ConstraintSet cs)
    {
        Vector2 massRange = AbsoluteMassRange(bodyType);
        Vector2 radiusRange = AbsoluteRadiusRange(bodyType);

        cs.SetConstraint(new PropertyConstraint(
            "physical.mass_kg", massRange.X, massRange.Y,
            GetCurrentValue(currentValues, "physical.mass_kg", 1.0), false, "body type"));
        cs.SetConstraint(new PropertyConstraint(
            "physical.radius_m", radiusRange.X, radiusRange.Y,
            GetCurrentValue(currentValues, "physical.radius_m", 1.0), false, "body type"));
        cs.SetConstraint(new PropertyConstraint(
            "physical.rotation_period_s", RotationMinS, RotationMaxS,
            GetCurrentValue(currentValues, "physical.rotation_period_s", 86400.0), false, ""));
        cs.SetConstraint(new PropertyConstraint(
            "physical.axial_tilt_deg", 0.0, 180.0,
            GetCurrentValue(currentValues, "physical.axial_tilt_deg", 0.0), false, "validator"));
        cs.SetConstraint(new PropertyConstraint(
            "physical.oblateness", 0.0, 0.5,
            GetCurrentValue(currentValues, "physical.oblateness", 0.0), false, ""));

        if (bodyType == CelestialType.Type.Star)
        {
            cs.SetConstraint(new PropertyConstraint(
                "stellar.temperature_k", StellarTempMinK, StellarTempMaxK,
                GetCurrentValue(currentValues, "stellar.temperature_k", 5778.0), false, ""));
            cs.SetConstraint(new PropertyConstraint(
                "stellar.luminosity_watts", StellarLumMinW, StellarLumMaxW,
                GetCurrentValue(currentValues, "stellar.luminosity_watts", 3.828e26), false, ""));
            cs.SetConstraint(new PropertyConstraint(
                "stellar.age_years", StellarAgeMinYr, StellarAgeMaxYr,
                GetCurrentValue(currentValues, "stellar.age_years", 4.6e9), false, ""));
            cs.SetConstraint(new PropertyConstraint(
                "stellar.metallicity", 0.001, 10.0,
                GetCurrentValue(currentValues, "stellar.metallicity", 1.0), false, ""));
        }

        cs.SetConstraint(new PropertyConstraint(
            "orbital.semi_major_axis_m", 1.0e3, 1.0e15,
            GetCurrentValue(currentValues, "orbital.semi_major_axis_m", Units.AuMeters), false, "unbounded (no parent context)"));
        cs.SetConstraint(new PropertyConstraint(
            "orbital.eccentricity", 0.0, 0.99,
            GetCurrentValue(currentValues, "orbital.eccentricity", 0.0), false, "validator"));
        cs.SetConstraint(new PropertyConstraint(
            "orbital.inclination_deg", 0.0, 180.0,
            GetCurrentValue(currentValues, "orbital.inclination_deg", 0.0), false, "validator"));

        cs.SetConstraint(new PropertyConstraint(
            "atmosphere.surface_pressure_pa", 0.0, 1.0e9,
            GetCurrentValue(currentValues, "atmosphere.surface_pressure_pa", 0.0), false, ""));
        cs.SetConstraint(new PropertyConstraint(
            "atmosphere.scale_height_m", 1.0, 5.0e5,
            GetCurrentValue(currentValues, "atmosphere.scale_height_m", 8500.0), false, ""));
        cs.SetConstraint(new PropertyConstraint(
            "atmosphere.greenhouse_factor", 1.0, 100.0,
            GetCurrentValue(currentValues, "atmosphere.greenhouse_factor", 1.0), false, ""));
        cs.SetConstraint(new PropertyConstraint(
            "surface.temperature_k", 0.0, 5000.0,
            GetCurrentValue(currentValues, "surface.temperature_k", 288.0), false, ""));
        cs.SetConstraint(new PropertyConstraint(
            "surface.albedo", 0.0, 1.0,
            GetCurrentValue(currentValues, "surface.albedo", 0.3), false, "validator"));
        cs.SetConstraint(new PropertyConstraint(
            "surface.volcanism_level", 0.0, 1.0,
            GetCurrentValue(currentValues, "surface.volcanism_level", 0.0), false, "validator"));
    }

    /// <summary>Returns the physical density bounds (kg/m³) for the given body type and current mass.</summary>
    private static Vector2 DensityBoundsFor(CelestialType.Type bodyType, double currentMassKg)
    {
        if (bodyType == CelestialType.Type.Star)
        {
            return new Vector2((float)StarDensityMin, (float)StarDensityMax);
        }

        if (bodyType == CelestialType.Type.Planet)
        {
            double massEarth = currentMassKg / Units.EarthMassKg;
            SizeCategory.Category cat = SizeTable.CategoryFromMass(massEarth);
            (double minD, double maxD) = SizeTable.GetDensityRangeTuple(cat);
            minD *= 0.7;
            maxD *= 1.3;
            return new Vector2((float)minD, (float)maxD);
        }

        if (bodyType == CelestialType.Type.Moon)
        {
            return new Vector2((float)MoonDensityMin, (float)MoonDensityMax);
        }

        if (bodyType == CelestialType.Type.Asteroid)
        {
            return new Vector2((float)AsteroidDensityMin, (float)AsteroidDensityMax);
        }

        return new Vector2((float)FallbackDensityMin, (float)FallbackDensityMax);
    }

    /// <summary>Returns the absolute mass range (kg) for the given body type.</summary>
    private static Vector2 AbsoluteMassRange(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => new Vector2((float)StarMassMin, (float)StarMassMax),
            CelestialType.Type.Planet => new Vector2((float)PlanetMassMin, (float)PlanetMassMax),
            CelestialType.Type.Moon => new Vector2((float)MoonMassMin, (float)MoonMassMax),
            CelestialType.Type.Asteroid => new Vector2((float)AsteroidMassMin, (float)AsteroidMassMax),
            _ => new Vector2(1.0f, 1.0e30f),
        };
    }

    /// <summary>Returns the absolute radius range (m) for the given body type.</summary>
    private static Vector2 AbsoluteRadiusRange(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => new Vector2((float)StarRadiusMin, (float)StarRadiusMax),
            CelestialType.Type.Planet => new Vector2((float)PlanetRadiusMin, (float)PlanetRadiusMax),
            CelestialType.Type.Moon => new Vector2((float)MoonRadiusMin, (float)MoonRadiusMax),
            CelestialType.Type.Asteroid => new Vector2((float)AsteroidRadiusMin, (float)AsteroidRadiusMax),
            _ => new Vector2(1.0f, 1.0e12f),
        };
    }

    /// <summary>Returns the radius of a sphere of the given mass and density.</summary>
    private static double RadiusFromMassDensity(double massKg, double densityKgM3)
    {
        if (massKg <= 0.0 || densityKgM3 <= 0.0)
        {
            return 0.0;
        }

        double vol = massKg / densityKgM3;
        return System.Math.Pow(vol * 3.0 / (4.0 * System.Math.PI), 1.0 / 3.0);
    }

    /// <summary>
    /// Reads a double from the current-values dictionary.
    /// Returns <paramref name="defaultValue"/> when the key is absent or the stored variant is not numeric.
    /// </summary>
    /// <param name="values">Dictionary of current property values.</param>
    /// <param name="path">Property path key.</param>
    /// <param name="defaultValue">Fallback value when the key is absent or the type is wrong.</param>
    /// <returns>Stored double, or <paramref name="defaultValue"/>.</returns>
    private static double GetCurrentValue(Dictionary values, string path, double defaultValue)
    {
        if (!values.ContainsKey(path))
        {
            return defaultValue;
        }

        Variant stored = values[path];
        if (stored.VariantType == Variant.Type.Float)
        {
            return (double)stored;
        }

        if (stored.VariantType == Variant.Type.Int)
        {
            return (int)stored;
        }

        GD.PushError(
            $"[PropertyConstraintSolver] GetCurrentValue: unexpected Variant type '{stored.VariantType}' for key '{path}' — using default {defaultValue}.");
        return defaultValue;
    }
}
