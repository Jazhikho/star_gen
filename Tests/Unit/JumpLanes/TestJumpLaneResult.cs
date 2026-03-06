#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneResult data model.
/// </summary>
public static class TestJumpLaneResult
{
    /// <summary>
    /// Tests adding connections and counting total.
    /// </summary>
    public static void TestAddConnectionAndTotal()
    {
        JumpLaneResult result = new();
        result.AddConnection(new JumpLaneConnection("a", "b", JumpLaneConnection.ConnectionType.Green, 2.5));
        result.AddConnection(new JumpLaneConnection("b", "c", JumpLaneConnection.ConnectionType.Yellow, 4.0));

        if (result.GetTotalConnections() != 2)
        {
            throw new InvalidOperationException($"Expected total connections 2, got {result.GetTotalConnections()}");
        }
    }

    /// <summary>
    /// Tests adding orphans and counting total.
    /// </summary>
    public static void TestAddOrphanAndTotal()
    {
        JumpLaneResult result = new();
        result.AddOrphan("orphan_1");
        result.AddOrphan("orphan_2");

        if (result.GetTotalOrphans() != 2)
        {
            throw new InvalidOperationException($"Expected total orphans 2, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests checking if a system is an orphan.
    /// </summary>
    public static void TestIsOrphan()
    {
        JumpLaneResult result = new();
        result.AddOrphan("orphan_x");

        if (!result.IsOrphan("orphan_x"))
        {
            throw new InvalidOperationException("Expected IsOrphan('orphan_x') to return true");
        }
        if (result.IsOrphan("other"))
        {
            throw new InvalidOperationException("Expected IsOrphan('other') to return false");
        }
    }

    /// <summary>
    /// Tests counting connections by type.
    /// </summary>
    public static void TestGetConnectionCounts()
    {
        JumpLaneResult result = new();
        result.AddConnection(new JumpLaneConnection("a", "b", JumpLaneConnection.ConnectionType.Green));
        result.AddConnection(new JumpLaneConnection("b", "c", JumpLaneConnection.ConnectionType.Green));
        result.AddConnection(new JumpLaneConnection("c", "d", JumpLaneConnection.ConnectionType.Yellow));
        result.AddConnection(new JumpLaneConnection("d", "e", JumpLaneConnection.ConnectionType.Orange));

        Godot.Collections.Dictionary counts = result.GetConnectionCounts();

        int greenCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Green];
        int yellowCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Yellow];
        int orangeCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Orange];

        if (greenCount != 2)
        {
            throw new InvalidOperationException($"Expected 2 green connections, got {greenCount}");
        }
        if (yellowCount != 1)
        {
            throw new InvalidOperationException($"Expected 1 yellow connection, got {yellowCount}");
        }
        if (orangeCount != 1)
        {
            throw new InvalidOperationException($"Expected 1 orange connection, got {orangeCount}");
        }
    }

    /// <summary>
    /// Tests retrieving connections for a specific system.
    /// </summary>
    public static void TestGetConnectionsForSystem()
    {
        JumpLaneResult result = new();
        result.AddConnection(new JumpLaneConnection("a", "b", JumpLaneConnection.ConnectionType.Green));
        result.AddConnection(new JumpLaneConnection("b", "c", JumpLaneConnection.ConnectionType.Green));
        result.AddConnection(new JumpLaneConnection("d", "e", JumpLaneConnection.ConnectionType.Orange));

        Godot.Collections.Array<JumpLaneConnection> bConnections = result.GetConnectionsForSystem("b");

        if (bConnections.Count != 2)
        {
            throw new InvalidOperationException($"Expected 2 connections for system 'b', got {bConnections.Count}");
        }
    }

    /// <summary>
    /// Tests registering a system.
    /// </summary>
    public static void TestRegisterSystem()
    {
        JumpLaneResult result = new();
        JumpLaneSystem system = new("s1", Vector3.Zero, 100);
        result.RegisterSystem(system);

        if (!result.Systems.ContainsKey("s1"))
        {
            throw new InvalidOperationException("Expected systems dictionary to contain 's1'");
        }

        JumpLaneSystem? retrieved = result.GetSystem("s1");
        if (retrieved == null)
        {
            throw new InvalidOperationException("Expected GetSystem('s1') to return non-null");
        }
        if (retrieved.Id != "s1")
        {
            throw new InvalidOperationException($"Expected id 's1', got '{retrieved.Id}'");
        }
    }

    /// <summary>
    /// Tests serialization round-trip preserves all properties.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        JumpLaneResult result = new();
        result.RegisterSystem(new JumpLaneSystem("s1", Vector3.Zero, 100));
        result.RegisterSystem(new JumpLaneSystem("s2", Vector3.One, 200));
        result.AddConnection(new JumpLaneConnection("s1", "s2", JumpLaneConnection.ConnectionType.Green, 2.0));
        result.AddOrphan("orphan_x");

        Godot.Collections.Dictionary data = result.ToDictionary();
        JumpLaneResult restored = JumpLaneResult.FromDictionary(data);

        if (restored.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected total connections 1, got {restored.GetTotalConnections()}");
        }
        if (restored.GetTotalOrphans() != 1)
        {
            throw new InvalidOperationException($"Expected total orphans 1, got {restored.GetTotalOrphans()}");
        }
        if (restored.Systems.Count != 2)
        {
            throw new InvalidOperationException($"Expected systems count 2, got {restored.Systems.Count}");
        }
        if (!restored.Systems.ContainsKey("s1"))
        {
            throw new InvalidOperationException("Expected systems to contain 's1'");
        }
        if (!restored.Systems.ContainsKey("s2"))
        {
            throw new InvalidOperationException("Expected systems to contain 's2'");
        }
        if (!restored.IsOrphan("orphan_x"))
        {
            throw new InvalidOperationException("Expected IsOrphan('orphan_x') to return true");
        }
    }
}
