#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for TravellerSizeCode: diameter_km_to_code and code_to_diameter_range.
/// </summary>
public static class TestTravellerSizeCode
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Known diameters: Luna 3,474 km → 2; Earth 12,742 km → 8; Jupiter ~139,820 km → E.
    /// </summary>
    public static void TestKnownDiameters()
    {
        object luna = TravellerSizeCode.DiameterKmToCode(3474.0);
        if (!luna.Equals(2))
        {
            throw new InvalidOperationException($"Expected Luna code 2, got {luna}");
        }
        object earth = TravellerSizeCode.DiameterKmToCode(12742.0);
        if (!earth.Equals(8))
        {
            throw new InvalidOperationException($"Expected Earth code 8, got {earth}");
        }
        object jupiter = TravellerSizeCode.DiameterKmToCode(139820.0);
        if (!jupiter.Equals("E"))
        {
            throw new InvalidOperationException($"Expected Jupiter code E, got {jupiter}");
        }
    }

    /// <summary>
    /// Boundary at 0/800: &lt;800 → 0, 800 → 1.
    /// </summary>
    public static void TestBoundary0800()
    {
        if (!TravellerSizeCode.DiameterKmToCode(0.0).Equals(0))
        {
            throw new InvalidOperationException("Expected code 0 for 0.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(799.0).Equals(0))
        {
            throw new InvalidOperationException("Expected code 0 for 799.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(800.0).Equals(1))
        {
            throw new InvalidOperationException("Expected code 1 for 800.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(801.0).Equals(1))
        {
            throw new InvalidOperationException("Expected code 1 for 801.0");
        }
    }

    /// <summary>
    /// Boundary at 18,400: just below → 9, at/above → A then C.
    /// </summary>
    public static void TestBoundary18400()
    {
        if (!TravellerSizeCode.DiameterKmToCode(15199.0).Equals(9))
        {
            throw new InvalidOperationException("Expected code 9 for 15199.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(15200.0).Equals("A"))
        {
            throw new InvalidOperationException("Expected code A for 15200.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(18399.0).Equals("B"))
        {
            throw new InvalidOperationException("Expected code B for 18399.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(18400.0).Equals("C"))
        {
            throw new InvalidOperationException("Expected code C for 18400.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(39999.0).Equals("C"))
        {
            throw new InvalidOperationException("Expected code C for 39999.0");
        }
    }

    /// <summary>
    /// Boundary at 40,000 and 120,000: C/D and D/E.
    /// </summary>
    public static void TestBoundary40000120000()
    {
        if (!TravellerSizeCode.DiameterKmToCode(40000.0).Equals("D"))
        {
            throw new InvalidOperationException("Expected code D for 40000.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(119999.0).Equals("D"))
        {
            throw new InvalidOperationException("Expected code D for 119999.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(120000.0).Equals("E"))
        {
            throw new InvalidOperationException("Expected code E for 120000.0");
        }
        if (!TravellerSizeCode.DiameterKmToCode(240000.0).Equals("E"))
        {
            throw new InvalidOperationException("Expected code E for 240000.0");
        }
    }

    /// <summary>
    /// Negative diameter maps to 0.
    /// </summary>
    public static void TestNegativeDiameterReturns0()
    {
        if (!TravellerSizeCode.DiameterKmToCode(-1.0).Equals(0))
        {
            throw new InvalidOperationException("Expected code 0 for negative diameter");
        }
    }

    /// <summary>
    /// code_to_diameter_range returns correct min/max for numeric and letter codes.
    /// </summary>
    public static void TestCodeToDiameterRange()
    {
        Godot.Collections.Dictionary<string, double> r0 = TravellerSizeCode.CodeToDiameterRange(0);
        if (System.Math.Abs(r0["min"] - 0.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 0.0 for code 0, got {r0["min"]}");
        }
        if (System.Math.Abs(r0["max"] - 800.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max 800.0 for code 0, got {r0["max"]}");
        }

        Godot.Collections.Dictionary<string, double> r8 = TravellerSizeCode.CodeToDiameterRange(8);
        if (System.Math.Abs(r8["min"] - 12000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 12000.0 for code 8, got {r8["min"]}");
        }
        if (System.Math.Abs(r8["max"] - 13600.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max 13600.0 for code 8, got {r8["max"]}");
        }

        Godot.Collections.Dictionary<string, double> rD = TravellerSizeCode.CodeToDiameterRange("D");
        if (System.Math.Abs(rD["min"] - 40000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 40000.0 for code D, got {rD["min"]}");
        }
        if (System.Math.Abs(rD["max"] - 120000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max 120000.0 for code D, got {rD["max"]}");
        }

        Godot.Collections.Dictionary<string, double> rE = TravellerSizeCode.CodeToDiameterRange("E");
        if (System.Math.Abs(rE["min"] - 120000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected min 120000.0 for code E, got {rE["min"]}");
        }
        if (System.Math.Abs(rE["max"] - (-1.0)) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected max -1.0 for code E, got {rE["max"]}");
        }

        Godot.Collections.Dictionary<string, double> invalid = TravellerSizeCode.CodeToDiameterRange(99);
        if (invalid.Count != 0)
        {
            throw new InvalidOperationException("Expected empty for invalid int code");
        }
        Godot.Collections.Dictionary<string, double> invalidS = TravellerSizeCode.CodeToDiameterRange("X");
        if (invalidS.Count != 0)
        {
            throw new InvalidOperationException("Expected empty for invalid string code");
        }
    }

    /// <summary>
    /// to_string_uwp returns single character for UWP digit.
    /// </summary>
    public static void TestToStringUwp()
    {
        if (TravellerSizeCode.ToStringUwp(0) != "0")
        {
            throw new InvalidOperationException($"Expected '0', got '{TravellerSizeCode.ToStringUwp(0)}'");
        }
        if (TravellerSizeCode.ToStringUwp(8) != "8")
        {
            throw new InvalidOperationException($"Expected '8', got '{TravellerSizeCode.ToStringUwp(8)}'");
        }
        if (TravellerSizeCode.ToStringUwp("A") != "A")
        {
            throw new InvalidOperationException($"Expected 'A', got '{TravellerSizeCode.ToStringUwp("A")}'");
        }
        if (TravellerSizeCode.ToStringUwp("E") != "E")
        {
            throw new InvalidOperationException($"Expected 'E', got '{TravellerSizeCode.ToStringUwp("E")}'");
        }
    }
}
