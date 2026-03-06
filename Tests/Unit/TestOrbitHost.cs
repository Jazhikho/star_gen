#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Systems;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for OrbitHost.
/// </summary>
public static class TestOrbitHost
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests basic construction.
    /// </summary>
    public static void TestConstruction()
    {
        OrbitHost host = new OrbitHost("node_1", OrbitHost.HostType.SType);

        if (host.NodeId != "node_1")
        {
            throw new InvalidOperationException("Expected node_id node_1");
        }
        if (host.HostTypeValue != OrbitHost.HostType.SType)
        {
            throw new InvalidOperationException("Expected host_type S_TYPE");
        }
        if (host.CombinedMassKg != 0.0)
        {
            throw new InvalidOperationException("Expected default combined_mass_kg 0.0");
        }
    }

    /// <summary>
    /// Tests has valid zone.
    /// </summary>
    public static void TestHasValidZone()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);

        if (host.HasValidZone())
        {
            throw new InvalidOperationException("Host without zone should not have valid zone");
        }

        host.InnerStabilityM = 1.0e10;
        host.OuterStabilityM = 1.0e12;
        if (!host.HasValidZone())
        {
            throw new InvalidOperationException("Host with valid zone should return true");
        }

        host.InnerStabilityM = 1.0e12;
        host.OuterStabilityM = 1.0e10;
        if (host.HasValidZone())
        {
            throw new InvalidOperationException("Host with invalid zone (inner >= outer) should return false");
        }
    }

    /// <summary>
    /// Tests zone width calculation.
    /// </summary>
    public static void TestGetZoneWidth()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.InnerStabilityM = 1.0e11;
        host.OuterStabilityM = 5.0e12;

        double width = host.GetZoneWidthM();
        if (System.Math.Abs(width - 4.9e12) > 1e10)
        {
            throw new InvalidOperationException($"Expected width ~4.9e12, got {width}");
        }
    }

    /// <summary>
    /// Tests is distance stable.
    /// </summary>
    public static void TestIsDistanceStable()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.InnerStabilityM = 1.0e11;
        host.OuterStabilityM = 5.0e12;

        if (host.IsDistanceStable(0.5e11))
        {
            throw new InvalidOperationException("Too close should be unstable");
        }
        if (!host.IsDistanceStable(1.0e11))
        {
            throw new InvalidOperationException("At inner edge should be stable");
        }
        if (!host.IsDistanceStable(1.0e12))
        {
            throw new InvalidOperationException("Middle should be stable");
        }
        if (!host.IsDistanceStable(5.0e12))
        {
            throw new InvalidOperationException("At outer edge should be stable");
        }
        if (host.IsDistanceStable(6.0e12))
        {
            throw new InvalidOperationException("Too far should be unstable");
        }
    }

    /// <summary>
    /// Tests calculate zones for Sun-like star.
    /// </summary>
    public static void TestCalculateZonesSunLike()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.CalculateZones();

        double hzInnerAu = host.HabitableZoneInnerM / Units.AuMeters;
        double hzOuterAu = host.HabitableZoneOuterM / Units.AuMeters;
        double frostAu = host.FrostLineM / Units.AuMeters;

        if (hzInnerAu < 0.9 || hzInnerAu > 1.0)
        {
            throw new InvalidOperationException($"Expected HZ inner 0.9-1.0 AU, got {hzInnerAu}");
        }
        if (hzOuterAu < 1.3 || hzOuterAu > 1.5)
        {
            throw new InvalidOperationException($"Expected HZ outer 1.3-1.5 AU, got {hzOuterAu}");
        }
        if (frostAu < 2.5 || frostAu > 3.0)
        {
            throw new InvalidOperationException($"Expected frost line 2.5-3.0 AU, got {frostAu}");
        }
    }

    /// <summary>
    /// Tests calculate zones for brighter star.
    /// </summary>
    public static void TestCalculateZonesBrightStar()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts * 4.0;
        host.CalculateZones();

        double hzInnerAu = host.HabitableZoneInnerM / Units.AuMeters;

        if (hzInnerAu < 1.8 || hzInnerAu > 2.1)
        {
            throw new InvalidOperationException($"Expected HZ inner 1.8-2.1 AU, got {hzInnerAu}");
        }
    }

    /// <summary>
    /// Tests is distance habitable.
    /// </summary>
    public static void TestIsDistanceHabitable()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.CalculateZones();

        if (host.IsDistanceHabitable(0.5 * Units.AuMeters))
        {
            throw new InvalidOperationException("Too hot should not be habitable");
        }
        if (!host.IsDistanceHabitable(1.0 * Units.AuMeters))
        {
            throw new InvalidOperationException("Earth-like distance should be habitable");
        }
        if (host.IsDistanceHabitable(5.0 * Units.AuMeters))
        {
            throw new InvalidOperationException("Too cold should not be habitable");
        }
    }

    /// <summary>
    /// Tests is beyond frost line.
    /// </summary>
    public static void TestIsBeyondFrostLine()
    {
        OrbitHost host = new OrbitHost("n1", OrbitHost.HostType.SType);
        host.CombinedLuminosityWatts = StellarProps.SolarLuminosityWatts;
        host.CalculateZones();

        if (host.IsBeyondFrostLine(1.0 * Units.AuMeters))
        {
            throw new InvalidOperationException("1 AU should be inside frost line");
        }
        if (!host.IsBeyondFrostLine(5.0 * Units.AuMeters))
        {
            throw new InvalidOperationException("5 AU should be beyond frost line");
        }
    }

    /// <summary>
    /// Tests get type string.
    /// </summary>
    public static void TestGetTypeString()
    {
        OrbitHost sType = new OrbitHost("n1", OrbitHost.HostType.SType);
        OrbitHost pType = new OrbitHost("n2", OrbitHost.HostType.PType);

        if (sType.GetTypeString() != "S-type")
        {
            throw new InvalidOperationException("Expected S-type");
        }
        if (pType.GetTypeString() != "P-type")
        {
            throw new InvalidOperationException("Expected P-type");
        }
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestRoundTrip()
    {
        OrbitHost original = new OrbitHost("node_42", OrbitHost.HostType.PType);
        original.CombinedMassKg = 2.0e30;
        original.CombinedLuminosityWatts = 5.0e26;
        original.EffectiveTemperatureK = 5500.0;
        original.InnerStabilityM = 1.0e11;
        original.OuterStabilityM = 5.0e12;
        original.CalculateZones();

        Godot.Collections.Dictionary data = original.ToDictionary();
        OrbitHost restored = OrbitHost.FromDictionary(data);

        if (restored.NodeId != original.NodeId)
        {
            throw new InvalidOperationException("Node ID should match");
        }
        if (restored.HostTypeValue != original.HostTypeValue)
        {
            throw new InvalidOperationException("Host type should match");
        }
        if (System.Math.Abs(restored.CombinedMassKg - original.CombinedMassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException("Combined mass should match");
        }
        if (System.Math.Abs(restored.CombinedLuminosityWatts - original.CombinedLuminosityWatts) > DefaultTolerance)
        {
            throw new InvalidOperationException("Combined luminosity should match");
        }
        if (System.Math.Abs(restored.InnerStabilityM - original.InnerStabilityM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Inner stability should match");
        }
        if (System.Math.Abs(restored.OuterStabilityM - original.OuterStabilityM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Outer stability should match");
        }
        if (System.Math.Abs(restored.HabitableZoneInnerM - original.HabitableZoneInnerM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Habitable zone inner should match");
        }
    }
}
