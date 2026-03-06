#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for PhysicalProps component.
/// </summary>
public static class TestPhysicalProps
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        PhysicalProps props = new PhysicalProps();
        if (props.MassKg != 0.0)
        {
            throw new InvalidOperationException($"Expected mass_kg 0.0, got {props.MassKg}");
        }
        if (props.RadiusM != 0.0)
        {
            throw new InvalidOperationException($"Expected radius_m 0.0, got {props.RadiusM}");
        }
        if (props.RotationPeriodS != 0.0)
        {
            throw new InvalidOperationException($"Expected rotation_period_s 0.0, got {props.RotationPeriodS}");
        }
        if (props.AxialTiltDeg != 0.0)
        {
            throw new InvalidOperationException($"Expected axial_tilt_deg 0.0, got {props.AxialTiltDeg}");
        }
        if (props.Oblateness != 0.0)
        {
            throw new InvalidOperationException($"Expected oblateness 0.0, got {props.Oblateness}");
        }
        if (props.MagneticMoment != 0.0)
        {
            throw new InvalidOperationException($"Expected magnetic_moment 0.0, got {props.MagneticMoment}");
        }
        if (props.InternalHeatWatts != 0.0)
        {
            throw new InvalidOperationException($"Expected internal_heat_watts 0.0, got {props.InternalHeatWatts}");
        }
    }

    /// <summary>
    /// Tests creation with specified values.
    /// </summary>
    public static void TestInitialization()
    {
        PhysicalProps props = new PhysicalProps(1.0e24, 6.0e6, 86400.0, 23.5);
        if (props.MassKg != 1.0e24)
        {
            throw new InvalidOperationException($"Expected mass_kg 1.0e24, got {props.MassKg}");
        }
        if (props.RadiusM != 6.0e6)
        {
            throw new InvalidOperationException($"Expected radius_m 6.0e6, got {props.RadiusM}");
        }
        if (props.RotationPeriodS != 86400.0)
        {
            throw new InvalidOperationException($"Expected rotation_period_s 86400.0, got {props.RotationPeriodS}");
        }
        if (props.AxialTiltDeg != 23.5)
        {
            throw new InvalidOperationException($"Expected axial_tilt_deg 23.5, got {props.AxialTiltDeg}");
        }
    }

    /// <summary>
    /// Tests volume calculation.
    /// </summary>
    public static void TestVolumeCalculation()
    {
        PhysicalProps props = new PhysicalProps(1.0e24, 1000.0);
        double expectedVolume = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(1000.0, 3.0);
        if (System.Math.Abs(props.GetVolumeM3() - expectedVolume) > 1.0)
        {
            throw new InvalidOperationException($"Expected volume {expectedVolume}, got {props.GetVolumeM3()}");
        }
    }

    /// <summary>
    /// Tests volume with zero radius.
    /// </summary>
    public static void TestVolumeZeroRadius()
    {
        PhysicalProps props = new PhysicalProps(1.0e24, 0.0);
        if (props.GetVolumeM3() != 0.0)
        {
            throw new InvalidOperationException($"Expected volume 0.0, got {props.GetVolumeM3()}");
        }
    }

    /// <summary>
    /// Tests density calculation.
    /// </summary>
    public static void TestDensityCalculation()
    {
        double mass = 5.972e24;
        double radius = 6.371e6;
        PhysicalProps props = new PhysicalProps(mass, radius);
        double density = props.GetDensityKgM3();
        if (density < 5000.0 || density > 6000.0)
        {
            throw new InvalidOperationException($"Expected density in range [5000.0, 6000.0], got {density}");
        }
    }

    /// <summary>
    /// Tests surface gravity calculation.
    /// </summary>
    public static void TestSurfaceGravity()
    {
        double mass = 5.972e24;
        double radius = 6.371e6;
        PhysicalProps props = new PhysicalProps(mass, radius);
        double gravity = props.GetSurfaceGravityMS2();
        if (gravity < 9.5 || gravity > 10.1)
        {
            throw new InvalidOperationException($"Expected gravity in range [9.5, 10.1], got {gravity}");
        }
    }

    /// <summary>
    /// Tests escape velocity calculation.
    /// </summary>
    public static void TestEscapeVelocity()
    {
        double mass = 5.972e24;
        double radius = 6.371e6;
        PhysicalProps props = new PhysicalProps(mass, radius);
        double escapeV = props.GetEscapeVelocityMS();
        if (escapeV < 11000.0 || escapeV > 11400.0)
        {
            throw new InvalidOperationException($"Expected escape velocity in range [11000.0, 11400.0], got {escapeV}");
        }
    }

    /// <summary>
    /// Tests oblateness values.
    /// </summary>
    public static void TestOblateness()
    {
        PhysicalProps props = new PhysicalProps(1.898e27, 6.991e7, 35730.0, 3.1, 0.065);
        if (System.Math.Abs(props.Oblateness - 0.065) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected oblateness 0.065, got {props.Oblateness}");
        }

        double eqRadius = props.GetEquatorialRadiusM();
        if (eqRadius <= props.RadiusM)
        {
            throw new InvalidOperationException($"Expected equatorial radius > {props.RadiusM}, got {eqRadius}");
        }

        double polarRadius = props.GetPolarRadiusM();
        if (polarRadius >= props.RadiusM)
        {
            throw new InvalidOperationException($"Expected polar radius < {props.RadiusM}, got {polarRadius}");
        }
    }

    /// <summary>
    /// Tests internal heat.
    /// </summary>
    public static void TestInternalHeat()
    {
        PhysicalProps props = new PhysicalProps(5.972e24, 6.371e6, 86400.0, 23.5, 0.003, 0.0, 4.4e13);
        if (props.InternalHeatWatts != 4.4e13)
        {
            throw new InvalidOperationException($"Expected internal_heat_watts 4.4e13, got {props.InternalHeatWatts}");
        }
    }

    /// <summary>
    /// Tests to_dict produces correct structure.
    /// </summary>
    public static void TestToDict()
    {
        PhysicalProps props = new PhysicalProps(1.0e24, 6.0e6, 86400.0, 23.5);
        Godot.Collections.Dictionary data = props.ToDictionary();
        if (data["mass_kg"].AsDouble() != 1.0e24)
        {
            throw new InvalidOperationException($"Expected mass_kg 1.0e24, got {data["mass_kg"]}");
        }
        if (data["radius_m"].AsDouble() != 6.0e6)
        {
            throw new InvalidOperationException($"Expected radius_m 6.0e6, got {data["radius_m"]}");
        }
        if (data["rotation_period_s"].AsDouble() != 86400.0)
        {
            throw new InvalidOperationException($"Expected rotation_period_s 86400.0, got {data["rotation_period_s"]}");
        }
        if (data["axial_tilt_deg"].AsDouble() != 23.5)
        {
            throw new InvalidOperationException($"Expected axial_tilt_deg 23.5, got {data["axial_tilt_deg"]}");
        }
    }

    /// <summary>
    /// Tests from_dict correctly restores values.
    /// </summary>
    public static void TestFromDict()
    {
        Godot.Collections.Dictionary data = new Godot.Collections.Dictionary
        {
            ["mass_kg"] = 2.0e24,
            ["radius_m"] = 7.0e6,
            ["rotation_period_s"] = 43200.0,
            ["axial_tilt_deg"] = 15.0
        };
        PhysicalProps props = PhysicalProps.FromDictionary(data);
        if (props.MassKg != 2.0e24)
        {
            throw new InvalidOperationException($"Expected mass_kg 2.0e24, got {props.MassKg}");
        }
        if (props.RadiusM != 7.0e6)
        {
            throw new InvalidOperationException($"Expected radius_m 7.0e6, got {props.RadiusM}");
        }
        if (props.RotationPeriodS != 43200.0)
        {
            throw new InvalidOperationException($"Expected rotation_period_s 43200.0, got {props.RotationPeriodS}");
        }
        if (props.AxialTiltDeg != 15.0)
        {
            throw new InvalidOperationException($"Expected axial_tilt_deg 15.0, got {props.AxialTiltDeg}");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        PhysicalProps original = new PhysicalProps(3.5e24, 8.0e6, 72000.0, 45.0);
        Godot.Collections.Dictionary data = original.ToDictionary();
        PhysicalProps restored = PhysicalProps.FromDictionary(data);
        if (restored.MassKg != original.MassKg)
        {
            throw new InvalidOperationException($"Expected mass_kg {original.MassKg}, got {restored.MassKg}");
        }
        if (restored.RadiusM != original.RadiusM)
        {
            throw new InvalidOperationException($"Expected radius_m {original.RadiusM}, got {restored.RadiusM}");
        }
        if (restored.RotationPeriodS != original.RotationPeriodS)
        {
            throw new InvalidOperationException($"Expected rotation_period_s {original.RotationPeriodS}, got {restored.RotationPeriodS}");
        }
        if (restored.AxialTiltDeg != original.AxialTiltDeg)
        {
            throw new InvalidOperationException($"Expected axial_tilt_deg {original.AxialTiltDeg}, got {restored.AxialTiltDeg}");
        }
        if (restored.Oblateness != original.Oblateness)
        {
            throw new InvalidOperationException($"Expected oblateness {original.Oblateness}, got {restored.Oblateness}");
        }
        if (restored.MagneticMoment != original.MagneticMoment)
        {
            throw new InvalidOperationException($"Expected magnetic_moment {original.MagneticMoment}, got {restored.MagneticMoment}");
        }
        if (restored.InternalHeatWatts != original.InternalHeatWatts)
        {
            throw new InvalidOperationException($"Expected internal_heat_watts {original.InternalHeatWatts}, got {restored.InternalHeatWatts}");
        }
    }
}
