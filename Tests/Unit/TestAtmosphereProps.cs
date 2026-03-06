#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Celestial.Components;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for AtmosphereProps component.
/// </summary>
public static class TestAtmosphereProps
{
    private const double DefaultTolerance = 0.001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        AtmosphereProps props = new AtmosphereProps();
        if (props.SurfacePressurePa != 0.0)
        {
            throw new InvalidOperationException($"Expected surface_pressure_pa 0.0, got {props.SurfacePressurePa}");
        }
        if (props.ScaleHeightM != 0.0)
        {
            throw new InvalidOperationException($"Expected scale_height_m 0.0, got {props.ScaleHeightM}");
        }
        if (props.Composition.Count != 0)
        {
            throw new InvalidOperationException($"Expected empty composition, got {props.Composition.Count} entries");
        }
        if (props.GreenhouseFactor != 1.0)
        {
            throw new InvalidOperationException($"Expected greenhouse_factor 1.0, got {props.GreenhouseFactor}");
        }
    }

    /// <summary>
    /// Tests composition sum calculation.
    /// </summary>
    public static void TestCompositionSum()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["N2"] = 0.78, ["O2"] = 0.21, ["Ar"] = 0.01 };
        AtmosphereProps props = new AtmosphereProps(101325.0, 8500.0, comp, 1.0);
        if (System.Math.Abs(props.GetCompositionSum() - 1.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected composition sum 1.0, got {props.GetCompositionSum()}");
        }
    }

    /// <summary>
    /// Tests dominant gas detection.
    /// </summary>
    public static void TestDominantGas()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["N2"] = 0.78, ["O2"] = 0.21, ["Ar"] = 0.01 };
        AtmosphereProps props = new AtmosphereProps(101325.0, 8500.0, comp);
        if (props.GetDominantGas() != "N2")
        {
            throw new InvalidOperationException($"Expected dominant gas 'N2', got '{props.GetDominantGas()}'");
        }
    }

    /// <summary>
    /// Tests dominant gas with single gas.
    /// </summary>
    public static void TestDominantGasSingle()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["CO2"] = 0.95 };
        AtmosphereProps props = new AtmosphereProps(9.2e6, 15900.0, comp);
        if (props.GetDominantGas() != "CO2")
        {
            throw new InvalidOperationException($"Expected dominant gas 'CO2', got '{props.GetDominantGas()}'");
        }
    }

    /// <summary>
    /// Tests empty composition.
    /// </summary>
    public static void TestEmptyComposition()
    {
        AtmosphereProps props = new AtmosphereProps();
        if (props.GetCompositionSum() != 0.0)
        {
            throw new InvalidOperationException($"Expected composition sum 0.0, got {props.GetCompositionSum()}");
        }
        if (props.GetDominantGas() != "")
        {
            throw new InvalidOperationException($"Expected empty dominant gas, got '{props.GetDominantGas()}'");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        Godot.Collections.Dictionary comp = new Godot.Collections.Dictionary { ["H2"] = 0.9, ["He"] = 0.1 };
        AtmosphereProps original = new AtmosphereProps(1.0e5, 27000.0, comp, 1.5);
        Godot.Collections.Dictionary data = original.ToDictionary();
        AtmosphereProps restored = AtmosphereProps.FromDictionary(data);

        if (System.Math.Abs(restored.SurfacePressurePa - original.SurfacePressurePa) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected surface_pressure_pa {original.SurfacePressurePa}, got {restored.SurfacePressurePa}");
        }
        if (System.Math.Abs(restored.ScaleHeightM - original.ScaleHeightM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected scale_height_m {original.ScaleHeightM}, got {restored.ScaleHeightM}");
        }
        if (System.Math.Abs(restored.GreenhouseFactor - original.GreenhouseFactor) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected greenhouse_factor {original.GreenhouseFactor}, got {restored.GreenhouseFactor}");
        }
        if (System.Math.Abs(restored.Composition["H2"].AsDouble() - 0.9) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected H2 0.9, got {restored.Composition["H2"]}");
        }
        if (System.Math.Abs(restored.Composition["He"].AsDouble() - 0.1) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected He 0.1, got {restored.Composition["He"]}");
        }
    }
}
