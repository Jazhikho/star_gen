#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for AsteroidBelt.
/// </summary>
public static class TestAsteroidBelt
{
    private const double DefaultTolerance = 0.01;

    /// <summary>
    /// Tests basic construction.
    /// </summary>
    public static void TestConstruction()
    {
        AsteroidBelt belt = new AsteroidBelt("belt_1", "Main Belt");

        if (belt.Id != "belt_1")
        {
            throw new InvalidOperationException("Expected id belt_1");
        }
        if (belt.Name != "Main Belt")
        {
            throw new InvalidOperationException("Expected name Main Belt");
        }
        if ((AsteroidBelt.CompositionType)belt.PrimaryComposition != AsteroidBelt.CompositionType.Rocky)
        {
            throw new InvalidOperationException("Expected default composition ROCKY");
        }
        if (belt.MajorAsteroidIds.Count != 0)
        {
            throw new InvalidOperationException("Expected empty major_asteroid_ids");
        }
    }

    /// <summary>
    /// Tests width calculations.
    /// </summary>
    public static void TestGetWidth()
    {
        AsteroidBelt belt = new AsteroidBelt("b1", "Test");
        belt.InnerRadiusM = 2.0 * Units.AuMeters;
        belt.OuterRadiusM = 3.5 * Units.AuMeters;

        double widthAu = belt.GetWidthAu();
        if (System.Math.Abs(widthAu - 1.5) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected width 1.5 AU, got {widthAu}");
        }
    }

    /// <summary>
    /// Tests center calculation.
    /// </summary>
    public static void TestGetCenter()
    {
        AsteroidBelt belt = new AsteroidBelt("b1", "Test");
        belt.InnerRadiusM = 2.0 * Units.AuMeters;
        belt.OuterRadiusM = 4.0 * Units.AuMeters;

        double centerAu = belt.GetCenterAu();
        if (System.Math.Abs(centerAu - 3.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected center 3.0 AU, got {centerAu}");
        }
    }

    /// <summary>
    /// Tests composition string conversion.
    /// </summary>
    public static void TestCompositionToString()
    {
        if (AsteroidBelt.CompositionToString(AsteroidBelt.CompositionType.Rocky) != "rocky")
        {
            throw new InvalidOperationException("Expected rocky");
        }
        if (AsteroidBelt.CompositionToString(AsteroidBelt.CompositionType.Icy) != "icy")
        {
            throw new InvalidOperationException("Expected icy");
        }
        if (AsteroidBelt.CompositionToString(AsteroidBelt.CompositionType.Mixed) != "mixed")
        {
            throw new InvalidOperationException("Expected mixed");
        }
        if (AsteroidBelt.CompositionToString(AsteroidBelt.CompositionType.Metallic) != "metallic")
        {
            throw new InvalidOperationException("Expected metallic");
        }
    }

    /// <summary>
    /// Tests string to composition parsing.
    /// </summary>
    public static void TestStringToComposition()
    {
        if (AsteroidBelt.StringToComposition("rocky") != AsteroidBelt.CompositionType.Rocky)
        {
            throw new InvalidOperationException("Expected ROCKY");
        }
        if (AsteroidBelt.StringToComposition("ICY") != AsteroidBelt.CompositionType.Icy)
        {
            throw new InvalidOperationException("Expected ICY");
        }
        if (AsteroidBelt.StringToComposition("Mixed") != AsteroidBelt.CompositionType.Mixed)
        {
            throw new InvalidOperationException("Expected MIXED");
        }
        if (AsteroidBelt.StringToComposition("unknown") != AsteroidBelt.CompositionType.Rocky)
        {
            throw new InvalidOperationException("Expected default ROCKY");
        }
    }

    /// <summary>
    /// Tests major asteroid tracking.
    /// </summary>
    public static void TestMajorAsteroidIds()
    {
        AsteroidBelt belt = new AsteroidBelt("b1", "Test");
        belt.MajorAsteroidIds.Add("ceres");
        belt.MajorAsteroidIds.Add("vesta");
        belt.MajorAsteroidIds.Add("pallas");

        if (belt.GetMajorAsteroidCount() != 3)
        {
            throw new InvalidOperationException("Expected 3 major asteroids");
        }
        if (!belt.MajorAsteroidIds.Contains("ceres"))
        {
            throw new InvalidOperationException("Should contain ceres");
        }
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestRoundTrip()
    {
        AsteroidBelt original = new AsteroidBelt("main_belt", "Main Asteroid Belt");
        original.OrbitHostId = "sol";
        original.InnerRadiusM = 2.2 * Units.AuMeters;
        original.OuterRadiusM = 3.2 * Units.AuMeters;
        original.TotalMassKg = 3.0e21;
        original.PrimaryComposition = (AsteroidBelt.Composition)AsteroidBelt.CompositionType.Mixed;
        original.MajorAsteroidIds = new Array<string> { "ceres", "vesta", "pallas" };

        Godot.Collections.Dictionary data = original.ToDictionary();
        AsteroidBelt restored = AsteroidBelt.FromDictionary(data);

        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
        if (restored.Name != original.Name)
        {
            throw new InvalidOperationException("Name should match");
        }
        if (restored.OrbitHostId != original.OrbitHostId)
        {
            throw new InvalidOperationException("Orbit host ID should match");
        }
        if (System.Math.Abs(restored.InnerRadiusM - original.InnerRadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Inner radius should match");
        }
        if (System.Math.Abs(restored.OuterRadiusM - original.OuterRadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Outer radius should match");
        }
        if (System.Math.Abs(restored.TotalMassKg - original.TotalMassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException("Total mass should match");
        }
        if (restored.PrimaryComposition != original.PrimaryComposition)
        {
            throw new InvalidOperationException("Composition should match");
        }
        if (restored.MajorAsteroidIds.Count != 3)
        {
            throw new InvalidOperationException("Expected 3 major asteroids");
        }
        if (!restored.MajorAsteroidIds.Contains("vesta"))
        {
            throw new InvalidOperationException("Should contain vesta");
        }
    }
}
