#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Editing;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for TravellerConstraintBuilder.
/// </summary>
public static class TestTravellerConstraintBuilder
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests code 8 covers earth.
    /// </summary>
    public static void TestCode8CoversEarth()
    {
        Godot.Collections.Dictionary cons = TravellerConstraintBuilder.BuildConstraintsForSize(8);
        if (!cons.ContainsKey("physical.radius_m"))
        {
            throw new InvalidOperationException("Expected physical.radius_m");
        }
        Vector2 r = (Vector2)cons["physical.radius_m"];
        if (System.Math.Abs(r.X - 6.0e6) > 1.0)
        {
            throw new InvalidOperationException($"Expected min 6.0e6, got {r.X}");
        }
        if (System.Math.Abs(r.Y - 6.8e6) > 1.0)
        {
            throw new InvalidOperationException($"Expected max 6.8e6, got {r.Y}");
        }
        if (6.371e6 < r.X || 6.371e6 > r.Y)
        {
            throw new InvalidOperationException("Earth radius in code-8 window");
        }
    }

    /// <summary>
    /// Tests code 0 handles tiny bodies.
    /// </summary>
    public static void TestCode0HandlesTinyBodies()
    {
        Godot.Collections.Dictionary cons = TravellerConstraintBuilder.BuildConstraintsForSize(0);
        if (!cons.ContainsKey("physical.radius_m"))
        {
            throw new InvalidOperationException("Expected physical.radius_m");
        }
        Vector2 r = (Vector2)cons["physical.radius_m"];
        if (System.Math.Abs(r.X - 0.0) > 1.0)
        {
            throw new InvalidOperationException("code 0 min diameter is 0");
        }
        if (System.Math.Abs(r.Y - 400000.0) > 1.0)
        {
            throw new InvalidOperationException("code 0 max diameter 800 km -> 400 000 m radius");
        }
    }

    /// <summary>
    /// Tests code E has finite upper bound.
    /// </summary>
    public static void TestCodeEHasFiniteUpperBound()
    {
        Godot.Collections.Dictionary cons = TravellerConstraintBuilder.BuildConstraintsForSize("E");
        Vector2 r = (Vector2)cons["physical.radius_m"];
        if (double.IsInfinity(r.Y))
        {
            throw new InvalidOperationException("code E should get a synthetic finite cap");
        }
        if (r.Y <= 60.0e6)
        {
            throw new InvalidOperationException($"code E cap should cover large gas giants. Expected > 60.0e6, got {r.Y}");
        }
    }

    /// <summary>
    /// Tests mass window is positive and ordered.
    /// </summary>
    public static void TestMassWindowIsPositiveAndOrdered()
    {
        Godot.Collections.Array codes = TravellerConstraintBuilder.AllCodes();
        foreach (Variant code in codes)
        {
            Godot.Collections.Dictionary cons = TravellerConstraintBuilder.BuildConstraintsForSize(code);
            if (!cons.ContainsKey("physical.mass_kg"))
            {
                throw new InvalidOperationException($"mass present for {code}");
            }
            Vector2 m = (Vector2)cons["physical.mass_kg"];
            if (m.Y <= m.X)
            {
                throw new InvalidOperationException($"mass max > min for code {code}");
            }
            if (m.X < 0.0)
            {
                throw new InvalidOperationException($"mass min non-negative for code {code}");
            }
        }
    }

    /// <summary>
    /// Tests code for radius round trip on earth.
    /// </summary>
    public static void TestCodeForRadiusRoundTripOnEarth()
    {
        Variant code = TravellerConstraintBuilder.CodeForRadius(6.371e6);
        if (code.VariantType != Variant.Type.Int || (int)code != 8)
        {
            throw new InvalidOperationException($"Earth radius should map to code 8, got {code}");
        }
    }

    /// <summary>
    /// Tests code for radius jupiter is E.
    /// </summary>
    public static void TestCodeForRadiusJupiterIsE()
    {
        Variant code = TravellerConstraintBuilder.CodeForRadius(6.9911e7);
        if (code.VariantType != Variant.Type.String || (string)code != "E")
        {
            throw new InvalidOperationException($"Jupiter should map to code E, got {code}");
        }
    }

    /// <summary>
    /// Tests invalid code returns empty.
    /// </summary>
    public static void TestInvalidCodeReturnsEmpty()
    {
        Godot.Collections.Dictionary badInt = TravellerConstraintBuilder.BuildConstraintsForSize(99);
        Godot.Collections.Dictionary badStr = TravellerConstraintBuilder.BuildConstraintsForSize("Z");
        if (badInt.Count != 0)
        {
            throw new InvalidOperationException("Expected empty for bad int code");
        }
        if (badStr.Count != 0)
        {
            throw new InvalidOperationException("Expected empty for bad string code");
        }
    }

    /// <summary>
    /// Tests describe code format.
    /// </summary>
    public static void TestDescribeCodeFormat()
    {
        string desc = TravellerConstraintBuilder.DescribeCode(8);
        if (!desc.StartsWith("8"))
        {
            throw new InvalidOperationException("starts with UWP digit");
        }
        if (!desc.Contains("km"))
        {
            throw new InvalidOperationException("contains km unit");
        }
    }

    /// <summary>
    /// Tests all codes order and count.
    /// </summary>
    public static void TestAllCodesOrderAndCount()
    {
        Godot.Collections.Array codes = TravellerConstraintBuilder.AllCodes();
        if (codes.Count != 15)
        {
            throw new InvalidOperationException($"0-9 plus A-E is 15 codes, got {codes.Count}");
        }
        if (codes[0].VariantType != Variant.Type.Int || (int)codes[0] != 0)
        {
            throw new InvalidOperationException($"Expected codes[0] = 0, got {codes[0]}");
        }
        if (codes[9].VariantType != Variant.Type.Int || (int)codes[9] != 9)
        {
            throw new InvalidOperationException($"Expected codes[9] = 9, got {codes[9]}");
        }
        if (codes[10].VariantType != Variant.Type.String || (string)codes[10] != "A")
        {
            throw new InvalidOperationException($"Expected codes[10] = 'A', got {codes[10]}");
        }
        if (codes[14].VariantType != Variant.Type.String || (string)codes[14] != "E")
        {
            throw new InvalidOperationException($"Expected codes[14] = 'E', got {codes[14]}");
        }
    }

    /// <summary>
    /// Tests adjacent codes have non-overlapping radius midpoints.
    /// </summary>
    public static void TestAdjacentCodesHaveNonOverlappingRadiusMidpoints()
    {
        Godot.Collections.Array codes = TravellerConstraintBuilder.AllCodes();
        double prevMid = -1.0;
        foreach (Variant code in codes)
        {
            Godot.Collections.Dictionary cons = TravellerConstraintBuilder.BuildConstraintsForSize(code);
            Vector2 r = (Vector2)cons["physical.radius_m"];
            double mid = (r.X + r.Y) * 0.5;
            if (mid <= prevMid)
            {
                throw new InvalidOperationException($"midpoint increases for code {code}");
            }
            prevMid = mid;
        }
    }

    /// <summary>
    /// Legacy parity alias for test_code_for_radius_jupiter_is_d.
    /// </summary>
    private static void TestCodeForRadiusJupiterIsD()
    {
        TestCodeForRadiusJupiterIsE();
    }
}

