#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for OrbitalProps component.
/// </summary>
public static class TestOrbitalProps
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        OrbitalProps props = new OrbitalProps();
        if (props.SemiMajorAxisM != 0.0)
        {
            throw new InvalidOperationException("Expected default semi_major_axis_m 0.0");
        }
        if (props.Eccentricity != 0.0)
        {
            throw new InvalidOperationException("Expected default eccentricity 0.0");
        }
        if (props.InclinationDeg != 0.0)
        {
            throw new InvalidOperationException("Expected default inclination_deg 0.0");
        }
        if (props.ParentId != "")
        {
            throw new InvalidOperationException("Expected default parent_id empty");
        }
    }

    /// <summary>
    /// Tests periapsis calculation.
    /// </summary>
    public static void TestPeriapsis()
    {
        OrbitalProps props = new OrbitalProps(1.0e11, 0.2);
        double expected = 1.0e11 * (1.0 - 0.2);
        if (System.Math.Abs(props.GetPeriapsisM() - expected) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected periapsis {expected}, got {props.GetPeriapsisM()}");
        }
    }

    /// <summary>
    /// Tests apoapsis calculation.
    /// </summary>
    public static void TestApoapsis()
    {
        OrbitalProps props = new OrbitalProps(1.0e11, 0.2);
        double expected = 1.0e11 * (1.0 + 0.2);
        if (System.Math.Abs(props.GetApoapsisM() - expected) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected apoapsis {expected}, got {props.GetApoapsisM()}");
        }
    }

    /// <summary>
    /// Tests circular orbit (eccentricity = 0).
    /// </summary>
    public static void TestCircularOrbit()
    {
        OrbitalProps props = new OrbitalProps(1.0e11, 0.0);
        if (System.Math.Abs(props.GetPeriapsisM() - props.GetApoapsisM()) > DefaultTolerance)
        {
            throw new InvalidOperationException("Circular orbit periapsis should equal apoapsis");
        }
    }

    /// <summary>
    /// Tests orbital period calculation with Sun-like parent.
    /// </summary>
    public static void TestOrbitalPeriod()
    {
        OrbitalProps props = new OrbitalProps(Units.AuMeters);
        double period = props.GetOrbitalPeriodS(Units.SolarMassKg);
        double oneYearS = 365.25 * 24.0 * 3600.0;
        if (period < oneYearS * 0.99 || period > oneYearS * 1.01)
        {
            throw new InvalidOperationException($"Expected period near {oneYearS}, got {period}");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        OrbitalProps original = new OrbitalProps(
            1.5e11, 0.1, 5.0, 100.0, 200.0, 50.0, "star_001"
        );
        Godot.Collections.Dictionary data = original.ToDictionary();
        OrbitalProps restored = OrbitalProps.FromDictionary(data);

        if (System.Math.Abs(restored.SemiMajorAxisM - original.SemiMajorAxisM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Semi-major axis should match");
        }
        if (System.Math.Abs(restored.Eccentricity - original.Eccentricity) > DefaultTolerance)
        {
            throw new InvalidOperationException("Eccentricity should match");
        }
        if (System.Math.Abs(restored.InclinationDeg - original.InclinationDeg) > DefaultTolerance)
        {
            throw new InvalidOperationException("Inclination should match");
        }
        if (restored.ParentId != original.ParentId)
        {
            throw new InvalidOperationException("Parent ID should match");
        }
    }
}
