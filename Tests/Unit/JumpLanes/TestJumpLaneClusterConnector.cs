#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneClusterConnector.
/// </summary>
public static class TestJumpLaneClusterConnector
{
    /// <summary>
    /// Helper to create a system at a position.
    /// </summary>
    private static JumpLaneSystem MakeSystem(string id, double x, double y, double z, int pop)
    {
        return new JumpLaneSystem(id, new Vector3((float)x, (float)y, (float)z), pop);
    }

    /// <summary>
    /// Helper to create a region with systems.
    /// </summary>
    private static JumpLaneRegion MakeRegionWithSystems(Godot.Collections.Array<JumpLaneSystem> systems)
    {
        JumpLaneRegion region = new();
        foreach (JumpLaneSystem system in systems)
        {
            region.AddSystem(system);
        }
        return region;
    }

    /// <summary>
    /// Creates a result with systems registered. Only populated systems become orphans.
    /// </summary>
    private static JumpLaneResult MakeResultWithSystems(Godot.Collections.Array<JumpLaneSystem> systems)
    {
        JumpLaneResult result = new();
        foreach (JumpLaneSystem system in systems)
        {
            result.RegisterSystem(system);
            if (system.IsPopulated())
            {
                result.AddOrphan(system.Id);
            }
        }
        return result;
    }

    /// <summary>
    /// Tests single system is one cluster.
    /// </summary>
    public static void TestSingleSystemIsOneCluster()
    {
        JumpLaneClusterConnector connector = new();
        Godot.Collections.Array<JumpLaneSystem> systems = new()
        {
            MakeSystem("a", 0, 0, 0, 1000)
        };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
    }

    /// <summary>
    /// Tests two disconnected systems within range get connected.
    /// </summary>
    public static void TestTwoDisconnectedSystemsWithinRangeGetConnected()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 5, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests two disconnected systems beyond range stay disconnected.
    /// </summary>
    public static void TestTwoDisconnectedSystemsBeyondRangeStayDisconnected()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 15, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 2)
        {
            throw new InvalidOperationException($"Expected 2 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests three clusters connected iteratively.
    /// </summary>
    public static void TestThreeClustersConnectedIteratively()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 6, 0, 0, 2000);
        JumpLaneSystem sysC = MakeSystem("c", 12, 0, 0, 3000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB, sysC };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 2)
        {
            throw new InvalidOperationException($"Expected 2 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests already connected systems form single cluster.
    /// </summary>
    public static void TestAlreadyConnectedSystemsFormSingleCluster()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 3, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = new();
        result.RegisterSystem(sysA);
        result.RegisterSystem(sysB);

        result.AddConnection(new JumpLaneConnection("a", "b", JumpLaneConnection.ConnectionType.Green, 3.0));

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
    }

    /// <summary>
    /// Tests cluster connection uses green within 5pc.
    /// </summary>
    public static void TestClusterConnectionUsesGreenWithin5pc()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 4, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {result.Connections[0].Type}");
        }
    }

    /// <summary>
    /// Tests cluster connection uses orange at 7pc without bridge.
    /// </summary>
    public static void TestClusterConnectionUsesOrangeAt7pcNoBridge()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 7, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Orange)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Orange, got {result.Connections[0].Type}");
        }
    }

    /// <summary>
    /// Tests cluster connection uses bridge when available.
    /// </summary>
    public static void TestClusterConnectionUsesBridgeWhenAvailable()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 8, 0, 0, 2000);
        JumpLaneSystem bridge = MakeSystem("bridge", 4, 0, 0, 0);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB, bridge };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int yellowCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Yellow];
        if (yellowCount != 2)
        {
            throw new InvalidOperationException($"Expected 2 yellow connections, got {yellowCount}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests connector connects closest clusters first.
    /// </summary>
    public static void TestConnectsClosestClustersFirst()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 4, 0, 0, 2000);
        JumpLaneSystem sysC = MakeSystem("c", 20, 0, 0, 3000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB, sysC };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 1)
        {
            throw new InvalidOperationException($"Expected 1 orphan, got {result.GetTotalOrphans()}");
        }
        if (!result.IsOrphan("c"))
        {
            throw new InvalidOperationException("Expected 'c' to be an orphan");
        }
    }

    /// <summary>
    /// Tests orphan removed when connected.
    /// </summary>
    public static void TestOrphanRemovedWhenConnected()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 3, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        if (result.GetTotalOrphans() != 2)
        {
            throw new InvalidOperationException($"Expected 2 orphans before connection, got {result.GetTotalOrphans()}");
        }

        connector.ConnectClusters(region, result);

        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans after connection, got {result.GetTotalOrphans()}");
        }
        if (result.IsOrphan("a"))
        {
            throw new InvalidOperationException("Expected 'a' to not be an orphan");
        }
        if (result.IsOrphan("b"))
        {
            throw new InvalidOperationException("Expected 'b' to not be an orphan");
        }
    }

    /// <summary>
    /// Tests unpopulated systems are not in clusters.
    /// </summary>
    public static void TestUnpopulatedSystemsNotInClusters()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 8, 0, 0, 2000);
        JumpLaneSystem unpop = MakeSystem("unpop", 4, 0, 0, 0);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB, unpop };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() <= 0)
        {
            throw new InvalidOperationException($"Expected at least 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests extended direct red when clusters within 10pc.
    /// </summary>
    public static void TestExtendedDirectRedWhenClustersWithin10pc()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem sysB = MakeSystem("b", 10, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Red)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Red, got {result.Connections[0].Type}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests extended multi-hop red when stepping stones within 10pc.
    /// </summary>
    public static void TestExtendedMultiHopRedWhenSteppingStonesWithin10pc()
    {
        JumpLaneClusterConnector connector = new();
        JumpLaneSystem sysA = MakeSystem("a", 0, 0, 0, 1000);
        JumpLaneSystem mid = MakeSystem("mid", 8, 0, 0, 0);
        JumpLaneSystem sysB = MakeSystem("b", 16, 0, 0, 2000);

        Godot.Collections.Array<JumpLaneSystem> systems = new() { sysA, mid, sysB };
        JumpLaneRegion region = MakeRegionWithSystems(systems);
        JumpLaneResult result = MakeResultWithSystems(systems);

        connector.ConnectClusters(region, result);

        if (result.GetTotalConnections() != 2)
        {
            throw new InvalidOperationException($"Expected 2 connections, got {result.GetTotalConnections()}");
        }
        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int redCount = 0;
        if (counts.ContainsKey((int)JumpLaneConnection.ConnectionType.Red))
        {
            redCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Red];
        }
        if (redCount != 2)
        {
            throw new InvalidOperationException($"Expected 2 red connections, got {redCount}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }

        JumpLaneSystem? midSystem = result.GetSystem("mid");
        if (midSystem == null)
        {
            throw new InvalidOperationException("Expected 'mid' system to be registered");
        }
        if (!midSystem.IsBridge)
        {
            throw new InvalidOperationException("Expected 'mid' to be a bridge");
        }
    }
}
