#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneRegion data model.
/// </summary>
public static class TestJumpLaneRegion
{
    /// <summary>
    /// Tests default initialization values.
    /// </summary>
    public static void TestInitDefaults()
    {
        JumpLaneRegion region = new();

        if (region.Scope != JumpLaneRegion.RegionScope.Subsector)
        {
            throw new InvalidOperationException($"Expected RegionScope.Subsector, got {region.Scope}");
        }
        if (region.RegionId != string.Empty)
        {
            throw new InvalidOperationException($"Expected empty region_id, got '{region.RegionId}'");
        }
        if (region.Systems.Count != 0)
        {
            throw new InvalidOperationException($"Expected systems count 0, got {region.Systems.Count}");
        }
    }

    /// <summary>
    /// Tests initialization with provided values.
    /// </summary>
    public static void TestInitWithValues()
    {
        JumpLaneRegion region = new(
            JumpLaneRegion.RegionScope.Sector,
            "Spinward Marches");

        if (region.Scope != JumpLaneRegion.RegionScope.Sector)
        {
            throw new InvalidOperationException($"Expected RegionScope.Sector, got {region.Scope}");
        }
        if (region.RegionId != "Spinward Marches")
        {
            throw new InvalidOperationException($"Expected region_id 'Spinward Marches', got '{region.RegionId}'");
        }
    }

    /// <summary>
    /// Tests adding a system to the region.
    /// </summary>
    public static void TestAddSystem()
    {
        JumpLaneRegion region = new();
        JumpLaneSystem system = new("sys_001", Vector3.Zero, 1000);

        region.AddSystem(system);

        if (region.GetSystemCount() != 1)
        {
            throw new InvalidOperationException($"Expected system count 1, got {region.GetSystemCount()}");
        }
    }

    /// <summary>
    /// Tests removing a system from the region.
    /// </summary>
    public static void TestRemoveSystem()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("sys_001", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("sys_002", Vector3.Zero, 2000));

        bool removed = region.RemoveSystem("sys_001");

        if (!removed)
        {
            throw new InvalidOperationException("Expected RemoveSystem to return true");
        }
        if (region.GetSystemCount() != 1)
        {
            throw new InvalidOperationException($"Expected system count 1, got {region.GetSystemCount()}");
        }
        if (region.GetSystem("sys_001") != null)
        {
            throw new InvalidOperationException("Expected GetSystem('sys_001') to return null");
        }
    }

    /// <summary>
    /// Tests removing a non-existent system returns false.
    /// </summary>
    public static void TestRemoveSystemNotFound()
    {
        JumpLaneRegion region = new();

        bool removed = region.RemoveSystem("nonexistent");

        if (removed)
        {
            throw new InvalidOperationException("Expected RemoveSystem to return false");
        }
    }

    /// <summary>
    /// Tests retrieving a system by identifier.
    /// </summary>
    public static void TestGetSystem()
    {
        JumpLaneRegion region = new();
        JumpLaneSystem system = new("sys_001", new Vector3(1, 2, 3), 5000);
        region.AddSystem(system);

        JumpLaneSystem? found = region.GetSystem("sys_001");

        if (found == null)
        {
            throw new InvalidOperationException("Expected GetSystem to return non-null");
        }
        if (found.Id != "sys_001")
        {
            throw new InvalidOperationException($"Expected id 'sys_001', got '{found.Id}'");
        }
        if (found.Population != 5000)
        {
            throw new InvalidOperationException($"Expected population 5000, got {found.Population}");
        }
    }

    /// <summary>
    /// Tests retrieving a non-existent system returns null.
    /// </summary>
    public static void TestGetSystemNotFound()
    {
        JumpLaneRegion region = new();

        JumpLaneSystem? found = region.GetSystem("nonexistent");

        if (found != null)
        {
            throw new InvalidOperationException("Expected GetSystem to return null");
        }
    }

    /// <summary>
    /// Tests filtering populated systems.
    /// </summary>
    public static void TestGetPopulatedSystems()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("pop_1", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("unpop", Vector3.Zero, 0));
        region.AddSystem(new JumpLaneSystem("pop_2", Vector3.Zero, 2000));

        Godot.Collections.Array<JumpLaneSystem> populated = region.GetPopulatedSystems();

        if (populated.Count != 2)
        {
            throw new InvalidOperationException($"Expected 2 populated systems, got {populated.Count}");
        }
    }

    /// <summary>
    /// Tests filtering unpopulated systems.
    /// </summary>
    public static void TestGetUnpopulatedSystems()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("pop_1", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("unpop_1", Vector3.Zero, 0));
        region.AddSystem(new JumpLaneSystem("unpop_2", Vector3.Zero, 0));

        Godot.Collections.Array<JumpLaneSystem> unpopulated = region.GetUnpopulatedSystems();

        if (unpopulated.Count != 2)
        {
            throw new InvalidOperationException($"Expected 2 unpopulated systems, got {unpopulated.Count}");
        }
    }

    /// <summary>
    /// Tests sorting systems by population ascending.
    /// </summary>
    public static void TestGetSystemsSortedByPopulation()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("high", Vector3.Zero, 50000));
        region.AddSystem(new JumpLaneSystem("low", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("unpop", Vector3.Zero, 0));
        region.AddSystem(new JumpLaneSystem("mid", Vector3.Zero, 10000));

        Godot.Collections.Array<JumpLaneSystem> sorted = region.GetSystemsSortedByPopulation();

        if (sorted.Count != 3)
        {
            throw new InvalidOperationException($"Expected 3 populated systems, got {sorted.Count}");
        }
        if (sorted[0].Id != "low")
        {
            throw new InvalidOperationException($"Expected first system 'low', got '{sorted[0].Id}'");
        }
        if (sorted[1].Id != "mid")
        {
            throw new InvalidOperationException($"Expected second system 'mid', got '{sorted[1].Id}'");
        }
        if (sorted[2].Id != "high")
        {
            throw new InvalidOperationException($"Expected third system 'high', got '{sorted[2].Id}'");
        }
    }

    /// <summary>
    /// Tests counting populated systems.
    /// </summary>
    public static void TestGetPopulatedCount()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("pop_1", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("unpop", Vector3.Zero, 0));
        region.AddSystem(new JumpLaneSystem("pop_2", Vector3.Zero, 2000));

        if (region.GetPopulatedCount() != 2)
        {
            throw new InvalidOperationException($"Expected populated count 2, got {region.GetPopulatedCount()}");
        }
    }

    /// <summary>
    /// Tests clearing all systems from the region.
    /// </summary>
    public static void TestClear()
    {
        JumpLaneRegion region = new();
        region.AddSystem(new JumpLaneSystem("sys_001", Vector3.Zero, 1000));
        region.AddSystem(new JumpLaneSystem("sys_002", Vector3.Zero, 2000));

        region.Clear();

        if (region.GetSystemCount() != 0)
        {
            throw new InvalidOperationException($"Expected system count 0, got {region.GetSystemCount()}");
        }
    }

    /// <summary>
    /// Tests serialization round-trip preserves all properties.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        JumpLaneRegion region = new(
            JumpLaneRegion.RegionScope.Sector,
            "Test Sector");
        region.AddSystem(new JumpLaneSystem("sys_001", new Vector3(1, 2, 3), 10000));
        region.AddSystem(new JumpLaneSystem("sys_002", new Vector3(4, 5, 6), 20000));

        Godot.Collections.Dictionary data = region.ToDictionary();
        JumpLaneRegion restored = JumpLaneRegion.FromDictionary(data);

        if (restored.Scope != JumpLaneRegion.RegionScope.Sector)
        {
            throw new InvalidOperationException($"Expected RegionScope.Sector, got {restored.Scope}");
        }
        if (restored.RegionId != "Test Sector")
        {
            throw new InvalidOperationException($"Expected region_id 'Test Sector', got '{restored.RegionId}'");
        }
        if (restored.GetSystemCount() != 2)
        {
            throw new InvalidOperationException($"Expected system count 2, got {restored.GetSystemCount()}");
        }
        if (restored.GetSystem("sys_001") == null)
        {
            throw new InvalidOperationException("Expected GetSystem('sys_001') to return non-null");
        }
        if (restored.GetSystem("sys_002") == null)
        {
            throw new InvalidOperationException("Expected GetSystem('sys_002') to return non-null");
        }
    }
}
