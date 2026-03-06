#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for RingBand and RingSystemProps components.
/// </summary>
public static class TestRingSystemProps
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests RingBand creation with default values.
    /// </summary>
    public static void TestRingBandDefaultValues()
    {
        RingBand band = new RingBand();
        if (band.InnerRadiusM != 0.0)
        {
            throw new InvalidOperationException($"Expected inner_radius_m 0.0, got {band.InnerRadiusM}");
        }
        if (band.OuterRadiusM != 0.0)
        {
            throw new InvalidOperationException($"Expected outer_radius_m 0.0, got {band.OuterRadiusM}");
        }
        if (band.OpticalDepth != 0.0)
        {
            throw new InvalidOperationException($"Expected optical_depth 0.0, got {band.OpticalDepth}");
        }
        if (band.ParticleSizeM != 1.0)
        {
            throw new InvalidOperationException($"Expected particle_size_m 1.0, got {band.ParticleSizeM}");
        }
        if (band.Name != "")
        {
            throw new InvalidOperationException($"Expected empty name, got '{band.Name}'");
        }
    }

    /// <summary>
    /// Tests RingBand width calculation.
    /// </summary>
    public static void TestRingBandWidth()
    {
        RingBand band = new RingBand(1.0e8, 2.5e8);
        if (System.Math.Abs(band.GetWidthM() - 1.5e8) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected width 1.5e8, got {band.GetWidthM()}");
        }
    }

    /// <summary>
    /// Tests RingBand dominant material detection.
    /// </summary>
    public static void TestRingBandDominantMaterial()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["water_ice"] = 0.7, ["silicates"] = 0.2, ["organics"] = 0.1 };
        RingBand band = new RingBand(1.0e8, 2.0e8, 0.5, comp);
        if (band.GetDominantMaterial() != "water_ice")
        {
            throw new InvalidOperationException($"Expected dominant material 'water_ice', got '{band.GetDominantMaterial()}'");
        }
    }

    /// <summary>
    /// Tests RingSystemProps creation with empty bands.
    /// </summary>
    public static void TestRingSystemEmpty()
    {
        RingSystemProps system = new RingSystemProps();
        if (system.GetBandCount() != 0)
        {
            throw new InvalidOperationException($"Expected band count 0, got {system.GetBandCount()}");
        }
        if (system.GetInnerRadiusM() != 0.0)
        {
            throw new InvalidOperationException($"Expected inner radius 0.0, got {system.GetInnerRadiusM()}");
        }
        if (system.GetOuterRadiusM() != 0.0)
        {
            throw new InvalidOperationException($"Expected outer radius 0.0, got {system.GetOuterRadiusM()}");
        }
    }

    /// <summary>
    /// Tests RingSystemProps with multiple bands.
    /// </summary>
    public static void TestRingSystemMultipleBands()
    {
        RingBand bandA = new RingBand(1.0e8, 1.5e8, 0.5, new Dictionary(), 1.0, "A Ring");
        RingBand bandB = new RingBand(2.0e8, 3.0e8, 1.0, new Dictionary(), 1.0, "B Ring");
        Godot.Collections.Array<RingBand> bands = new Godot.Collections.Array<RingBand> { bandA, bandB };

        RingSystemProps system = new RingSystemProps(bands, 1.5e19);

        if (system.GetBandCount() != 2)
        {
            throw new InvalidOperationException($"Expected band count 2, got {system.GetBandCount()}");
        }
        if (System.Math.Abs(system.GetInnerRadiusM() - 1.0e8) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected inner radius 1.0e8, got {system.GetInnerRadiusM()}");
        }
        if (System.Math.Abs(system.GetOuterRadiusM() - 3.0e8) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected outer radius 3.0e8, got {system.GetOuterRadiusM()}");
        }
        if (System.Math.Abs(system.GetTotalWidthM() - 2.0e8) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected total width 2.0e8, got {system.GetTotalWidthM()}");
        }
    }

    /// <summary>
    /// Tests adding bands to ring system.
    /// </summary>
    public static void TestRingSystemAddBand()
    {
        RingSystemProps system = new RingSystemProps();
        if (system.GetBandCount() != 0)
        {
            throw new InvalidOperationException($"Expected initial band count 0, got {system.GetBandCount()}");
        }

        RingBand band = new RingBand(1.0e8, 2.0e8);
        system.AddBand(band);

        if (system.GetBandCount() != 1)
        {
            throw new InvalidOperationException($"Expected band count 1 after add, got {system.GetBandCount()}");
        }
    }

    /// <summary>
    /// Tests getting band by index.
    /// </summary>
    public static void TestRingSystemGetBand()
    {
        RingBand band = new RingBand(1.0e8, 2.0e8, 0.5, new Dictionary(), 1.0, "Test");
        Godot.Collections.Array<RingBand> bands = new Godot.Collections.Array<RingBand> { band };
        RingSystemProps system = new RingSystemProps(bands);

        RingBand retrieved = system.GetBand(0);
        if (retrieved == null)
        {
            throw new InvalidOperationException("Expected non-null band at index 0");
        }
        if (retrieved.Name != "Test")
        {
            throw new InvalidOperationException($"Expected band name 'Test', got '{retrieved.Name}'");
        }

        RingBand invalid = system.GetBand(5);
        if (invalid != null)
        {
            throw new InvalidOperationException("Expected null for invalid index");
        }
    }

    /// <summary>
    /// Tests RingBand round-trip serialization.
    /// </summary>
    public static void TestRingBandRoundTrip()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["water_ice"] = 0.8, ["rock"] = 0.2 };
        RingBand original = new RingBand(1.0e8, 2.0e8, 0.7, comp, 0.5, "Main");
        Godot.Collections.Dictionary data = original.ToDictionary();
        RingBand restored = RingBand.FromDictionary(data);

        if (System.Math.Abs(restored.InnerRadiusM - original.InnerRadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected inner_radius_m {original.InnerRadiusM}, got {restored.InnerRadiusM}");
        }
        if (System.Math.Abs(restored.OuterRadiusM - original.OuterRadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected outer_radius_m {original.OuterRadiusM}, got {restored.OuterRadiusM}");
        }
        if (System.Math.Abs(restored.OpticalDepth - original.OpticalDepth) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected optical_depth {original.OpticalDepth}, got {restored.OpticalDepth}");
        }
        if (System.Math.Abs(restored.ParticleSizeM - original.ParticleSizeM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected particle_size_m {original.ParticleSizeM}, got {restored.ParticleSizeM}");
        }
        if (restored.Name != original.Name)
        {
            throw new InvalidOperationException($"Expected name '{original.Name}', got '{restored.Name}'");
        }
        if (System.Math.Abs(restored.Composition["water_ice"].AsDouble() - 0.8) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected water_ice 0.8, got {restored.Composition["water_ice"]}");
        }
    }

    /// <summary>
    /// Tests RingSystemProps round-trip serialization.
    /// </summary>
    public static void TestRingSystemRoundTrip()
    {
        RingBand bandA = new RingBand(1.0e8, 1.5e8, 0.3, new Godot.Collections.Dictionary { ["ice"] = 1.0 }, 1.0, "A");
        RingBand bandB = new RingBand(2.0e8, 3.0e8, 0.8, new Godot.Collections.Dictionary { ["rock"] = 1.0 }, 2.0, "B");
        Godot.Collections.Array<RingBand> bands = new Godot.Collections.Array<RingBand> { bandA, bandB };
        RingSystemProps original = new RingSystemProps(bands, 1.0e19, 0.5);

        Godot.Collections.Dictionary data = original.ToDictionary();
        RingSystemProps restored = RingSystemProps.FromDictionary(data);

        if (restored.GetBandCount() != 2)
        {
            throw new InvalidOperationException($"Expected band count 2, got {restored.GetBandCount()}");
        }
        if (System.Math.Abs(restored.TotalMassKg - original.TotalMassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected total_mass_kg {original.TotalMassKg}, got {restored.TotalMassKg}");
        }
        if (System.Math.Abs(restored.InclinationDeg - original.InclinationDeg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected inclination_deg {original.InclinationDeg}, got {restored.InclinationDeg}");
        }
        if (restored.GetBand(0).Name != "A")
        {
            throw new InvalidOperationException($"Expected band 0 name 'A', got '{restored.GetBand(0).Name}'");
        }
        if (restored.GetBand(1).Name != "B")
        {
            throw new InvalidOperationException($"Expected band 1 name 'B', got '{restored.GetBand(1).Name}'");
        }
    }
}
