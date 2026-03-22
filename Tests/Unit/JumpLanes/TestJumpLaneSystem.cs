#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneSystem data model.
/// </summary>
public static class TestJumpLaneSystem
{
    /// <summary>
    /// Tests default initialization values.
    /// </summary>
    public static void TestInitDefaults()
    {
        JumpLaneSystem system = new();

        if (system.Id != string.Empty)
        {
            throw new InvalidOperationException($"Expected empty id, got '{system.Id}'");
        }
        if (system.Position != Vector3.Zero)
        {
            throw new InvalidOperationException($"Expected Vector3.Zero, got {system.Position}");
        }
        if (system.Population != 0)
        {
            throw new InvalidOperationException($"Expected population 0, got {system.Population}");
        }
        if (system.FalsePopulation != -1)
        {
            throw new InvalidOperationException($"Expected false_population -1, got {system.FalsePopulation}");
        }
        if (system.IsBridge)
        {
            throw new InvalidOperationException("Expected is_bridge false");
        }
    }

    /// <summary>
    /// Tests initialization with provided values.
    /// </summary>
    public static void TestInitWithValues()
    {
        JumpLaneSystem system = new("sys_001", new Vector3(10, 0, 5), 50000);

        if (system.Id != "sys_001")
        {
            throw new InvalidOperationException($"Expected 'sys_001', got '{system.Id}'");
        }
        if (system.Position != new Vector3(10, 0, 5))
        {
            throw new InvalidOperationException($"Expected Vector3(10, 0, 5), got {system.Position}");
        }
        if (system.Population != 50000)
        {
            throw new InvalidOperationException($"Expected population 50000, got {system.Population}");
        }
    }

    /// <summary>
    /// Tests is_populated returns true for populated systems.
    /// </summary>
    public static void TestIsPopulatedTrue()
    {
        JumpLaneSystem system = new("sys_001", Vector3.Zero, 1000);

        if (!system.IsPopulated())
        {
            throw new InvalidOperationException("Expected is_populated to return true");
        }
    }

    /// <summary>
    /// Tests is_populated returns false for unpopulated systems.
    /// </summary>
    public static void TestIsPopulatedFalse()
    {
        JumpLaneSystem system = new("sys_001", Vector3.Zero, 0);

        if (system.IsPopulated())
        {
            throw new InvalidOperationException("Expected is_populated to return false");
        }
    }

    /// <summary>
    /// Tests effective population without false population.
    /// </summary>
    public static void TestEffectivePopulationWithoutFalse()
    {
        JumpLaneSystem system = new("sys_001", Vector3.Zero, 50000);

        if (system.GetEffectivePopulation() != 50000)
        {
            throw new InvalidOperationException($"Expected effective population 50000, got {system.GetEffectivePopulation()}");
        }
    }

    /// <summary>
    /// Tests effective population with false population set.
    /// </summary>
    public static void TestEffectivePopulationWithFalse()
    {
        JumpLaneSystem system = new("sys_001", Vector3.Zero, 0);
        system.FalsePopulation = 40000;

        if (system.GetEffectivePopulation() != 40000)
        {
            throw new InvalidOperationException($"Expected effective population 40000, got {system.GetEffectivePopulation()}");
        }
    }

    /// <summary>
    /// Tests distance calculation between two systems.
    /// </summary>
    public static void TestDistanceTo()
    {
        JumpLaneSystem systemA = new("a", new Vector3(0, 0, 0), 1000);
        JumpLaneSystem systemB = new("b", new Vector3(3, 4, 0), 2000);

        double distance = systemA.DistanceTo(systemB);
        if (System.Math.Abs(distance - 5.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance 5.0, got {distance}");
        }
    }

    /// <summary>
    /// Tests distance calculation in 3D space.
    /// </summary>
    public static void TestDistanceTo3d()
    {
        JumpLaneSystem systemA = new("a", new Vector3(0, 0, 0), 1000);
        JumpLaneSystem systemB = new("b", new Vector3(1, 2, 2), 2000);

        double distance = systemA.DistanceTo(systemB);
        if (System.Math.Abs(distance - 3.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance 3.0, got {distance}");
        }
    }

    /// <summary>
    /// Tests make_bridge sets bridge flag and false population.
    /// </summary>
    public static void TestMakeBridge()
    {
        JumpLaneSystem system = new("bridge", Vector3.Zero, 0);

        system.MakeBridge(50000);

        if (!system.IsBridge)
        {
            throw new InvalidOperationException("Expected is_bridge to be true");
        }
        if (system.FalsePopulation != 40000)
        {
            throw new InvalidOperationException($"Expected false_population 40000, got {system.FalsePopulation}");
        }
    }

    /// <summary>
    /// Tests make_bridge clamps negative false population to zero.
    /// </summary>
    public static void TestMakeBridgeClampsNegative()
    {
        JumpLaneSystem system = new("bridge", Vector3.Zero, 0);

        system.MakeBridge(5000);

        if (!system.IsBridge)
        {
            throw new InvalidOperationException("Expected is_bridge to be true");
        }
        if (system.FalsePopulation != 0)
        {
            throw new InvalidOperationException($"Expected false_population 0, got {system.FalsePopulation}");
        }
    }

    /// <summary>
    /// Tests serialization round-trip preserves all properties.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        JumpLaneSystem system = new("sys_001", new Vector3(10, 5, 3), 75000);
        system.MakeBridge(100000);
        system.CanExportColonists = true;
        system.ExportPressure = 0.8;
        system.ColonyTargetScore = 0.6;
        system.ColonyTargetCapacity = 1200000;
        system.ColonizationRangePc = 7.0;
        system.RouteTechnologyLevel = 10;

        Godot.Collections.Dictionary data = system.ToDictionary();
        JumpLaneSystem restored = JumpLaneSystem.FromDictionary(data);

        if (restored.Id != "sys_001")
        {
            throw new InvalidOperationException($"Expected id 'sys_001', got '{restored.Id}'");
        }
        if (restored.Position != new Vector3(10, 5, 3))
        {
            throw new InvalidOperationException($"Expected position Vector3(10, 5, 3), got {restored.Position}");
        }
        if (restored.Population != 75000)
        {
            throw new InvalidOperationException($"Expected population 75000, got {restored.Population}");
        }
        if (restored.FalsePopulation != 90000)
        {
            throw new InvalidOperationException($"Expected false_population 90000, got {restored.FalsePopulation}");
        }
        if (!restored.IsBridge)
        {
            throw new InvalidOperationException("Expected is_bridge to be true");
        }
        if (!restored.CanExportColonists)
        {
            throw new InvalidOperationException("Expected export capability to round-trip");
        }
        if (System.Math.Abs(restored.ExportPressure - 0.8) > 0.0001)
        {
            throw new InvalidOperationException($"Expected export_pressure 0.8, got {restored.ExportPressure}");
        }
        if (System.Math.Abs(restored.ColonyTargetScore - 0.6) > 0.0001)
        {
            throw new InvalidOperationException($"Expected colony_target_score 0.6, got {restored.ColonyTargetScore}");
        }
        if (restored.ColonyTargetCapacity != 1200000)
        {
            throw new InvalidOperationException($"Expected colony_target_capacity 1200000, got {restored.ColonyTargetCapacity}");
        }
        if (System.Math.Abs(restored.ColonizationRangePc - 7.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected colonization_range_pc 7.0, got {restored.ColonizationRangePc}");
        }
        if (restored.RouteTechnologyLevel != 10)
        {
            throw new InvalidOperationException($"Expected route_technology_level 10, got {restored.RouteTechnologyLevel}");
        }
    }

    public static void TestSerializationRoundTripPreservesTravellerProfile()
    {
        JumpLaneSystem system = new("sys_001", new Vector3(2, 0, 0), 5000000);
        system.TravellerProfile = new TravellerSystemProfile
        {
            MainworldBodyId = "planet_1",
            MainworldName = "Route World",
            SelectionReason = "Fixture",
            WorldProfile = new TravellerWorldProfile
            {
                StarportCode = "B",
                SizeCode = 7,
                AtmosphereCode = 6,
                HydrographicsCode = 7,
                PopulationCode = 8,
                GovernmentCode = 4,
                LawCode = 5,
                TechLevelCode = 10,
            },
            TradeCodes = TravellerTradeCodeSet.FromDictionary(new Godot.Collections.Dictionary
            {
                ["codes"] = new Godot.Collections.Array<string> { "Ag", "Ri" },
            }),
            RouteProfile = new TravellerRouteProfile
            {
                EstimatedPopulation = 500000000,
                PopulationCode = 8,
                StarportCode = "B",
                TechLevelCode = 10,
                Importance = 2,
                MaxJumpNumber = 1,
                RouteWeight = 3,
            },
        };

        Godot.Collections.Dictionary data = system.ToDictionary();
        JumpLaneSystem restored = JumpLaneSystem.FromDictionary(data);

        if (restored.TravellerProfile == null)
        {
            throw new InvalidOperationException("Traveller route profile should round-trip");
        }

        if (restored.TravellerProfile.MainworldName != "Route World")
        {
            throw new InvalidOperationException("Traveller system name should round-trip");
        }

        if (!restored.TravellerProfile.TradeCodes.Contains("Ag"))
        {
            throw new InvalidOperationException("Traveller trade codes should round-trip");
        }

        if (restored.TravellerProfile.RouteProfile.MaxJumpNumber != 1)
        {
            throw new InvalidOperationException("Traveller route metrics should round-trip");
        }
    }
}
