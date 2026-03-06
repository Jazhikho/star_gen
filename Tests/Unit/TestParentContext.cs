#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for ParentContext.
/// </summary>
public static class TestParentContext
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        ParentContext ctx = new ParentContext();
        if (ctx.StellarMassKg != 0.0)
        {
            throw new InvalidOperationException($"Expected stellar_mass_kg 0.0, got {ctx.StellarMassKg}");
        }
        if (ctx.StellarLuminosityWatts != 0.0)
        {
            throw new InvalidOperationException($"Expected stellar_luminosity_watts 0.0, got {ctx.StellarLuminosityWatts}");
        }
        if (ctx.HasParentBody())
        {
            throw new InvalidOperationException("Expected no parent body");
        }
    }

    /// <summary>
    /// Tests for_planet factory method.
    /// </summary>
    public static void TestForPlanet()
    {
        ParentContext ctx = ParentContext.ForPlanet(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters
        );
        if (ctx.StellarMassKg != Units.SolarMassKg)
        {
            throw new InvalidOperationException($"Expected stellar_mass_kg {Units.SolarMassKg}, got {ctx.StellarMassKg}");
        }
        if (ctx.OrbitalDistanceFromStarM != Units.AuMeters)
        {
            throw new InvalidOperationException($"Expected orbital_distance_from_star_m {Units.AuMeters}, got {ctx.OrbitalDistanceFromStarM}");
        }
        if (ctx.HasParentBody())
        {
            throw new InvalidOperationException("Expected no parent body");
        }
    }

    /// <summary>
    /// Tests for_moon factory method.
    /// </summary>
    public static void TestForMoon()
    {
        ParentContext ctx = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters,
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            384400000.0
        );
        if (!ctx.HasParentBody())
        {
            throw new InvalidOperationException("Expected parent body");
        }
        if (ctx.ParentBodyMassKg != Units.EarthMassKg)
        {
            throw new InvalidOperationException($"Expected parent_body_mass_kg {Units.EarthMassKg}, got {ctx.ParentBodyMassKg}");
        }
        if (ctx.OrbitalDistanceFromParentM != 384400000.0)
        {
            throw new InvalidOperationException($"Expected orbital_distance_from_parent_m 384400000.0, got {ctx.OrbitalDistanceFromParentM}");
        }
    }

    /// <summary>
    /// Tests sun_like factory method.
    /// </summary>
    public static void TestSunLike()
    {
        ParentContext ctx = ParentContext.SunLike();
        if (System.Math.Abs(ctx.StellarMassKg - Units.SolarMassKg) > Units.SolarMassKg * 0.001)
        {
            throw new InvalidOperationException($"Expected stellar_mass_kg ~{Units.SolarMassKg}, got {ctx.StellarMassKg}");
        }
        if (System.Math.Abs(ctx.OrbitalDistanceFromStarM - Units.AuMeters) > Units.AuMeters * 0.001)
        {
            throw new InvalidOperationException($"Expected orbital_distance_from_star_m ~{Units.AuMeters}, got {ctx.OrbitalDistanceFromStarM}");
        }
    }

    /// <summary>
    /// Tests equilibrium temperature calculation.
    /// </summary>
    public static void TestEquilibriumTemperature()
    {
        ParentContext ctx = ParentContext.SunLike();
        double temp = ctx.GetEquilibriumTemperatureK(0.3);
        if (temp < 250.0 || temp > 260.0)
        {
            throw new InvalidOperationException($"Expected equilibrium temperature in range [250.0, 260.0], got {temp}");
        }
    }

    /// <summary>
    /// Tests Hill sphere calculation.
    /// </summary>
    public static void TestHillSphere()
    {
        ParentContext ctx = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters,
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            384400000.0
        );
        double hillRadius = ctx.GetHillSphereRadiusM();
        if (hillRadius < 1.0e9 || hillRadius > 2.0e9)
        {
            throw new InvalidOperationException($"Expected Hill sphere radius in range [1.0e9, 2.0e9], got {hillRadius}");
        }
    }

    /// <summary>
    /// Tests Roche limit calculation.
    /// </summary>
    public static void TestRocheLimit()
    {
        ParentContext ctx = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters,
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            384400000.0
        );
        double roche = ctx.GetRocheLimitM(3000.0);
        if (roche < 1.8e7 || roche > 2.0e7)
        {
            throw new InvalidOperationException($"Expected Roche limit in range [1.8e7, 2.0e7], got {roche}");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        ParentContext original = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            Units.AuMeters,
            Units.EarthMassKg,
            Units.EarthRadiusMeters,
            384400000.0
        );
        Godot.Collections.Dictionary data = original.ToDictionary();
        ParentContext restored = ParentContext.FromDictionary(data);

        if (System.Math.Abs(restored.StellarMassKg - original.StellarMassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected stellar_mass_kg {original.StellarMassKg}, got {restored.StellarMassKg}");
        }
        if (System.Math.Abs(restored.ParentBodyMassKg - original.ParentBodyMassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected parent_body_mass_kg {original.ParentBodyMassKg}, got {restored.ParentBodyMassKg}");
        }
        if (System.Math.Abs(restored.OrbitalDistanceFromParentM - original.OrbitalDistanceFromParentM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected orbital_distance_from_parent_m {original.OrbitalDistanceFromParentM}, got {restored.OrbitalDistanceFromParentM}");
        }
    }
}
