#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneCalculator.
/// </summary>
public static class TestJumpLaneCalculator
{
    /// <summary>
    /// Helper to create a system at a position.
    /// </summary>
    private static JumpLaneSystem MakeSystem(string id, double x, double y, double z, int pop)
    {
        return new JumpLaneSystem(id, new Vector3((float)x, (float)y, (float)z), pop);
    }

    /// <summary>
    /// Tests empty region returns empty result.
    /// </summary>
    public static void TestEmptyRegionReturnsEmptyResult()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests single system is marked as orphan.
    /// </summary>
    public static void TestSingleSystemIsOrphan()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("only", 0, 0, 0, 1000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 1)
        {
            throw new InvalidOperationException($"Expected 1 orphan, got {result.GetTotalOrphans()}");
        }
        if (!result.IsOrphan("only"))
        {
            throw new InvalidOperationException("Expected 'only' to be an orphan");
        }
    }

    /// <summary>
    /// Tests two systems within 3pc create green connection.
    /// </summary>
    public static void TestTwoSystemsWithin3pcGreenConnection()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 2, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }

        JumpLaneConnection conn = result.Connections[0];
        if (conn.SourceId != "low")
        {
            throw new InvalidOperationException($"Expected source_id 'low', got '{conn.SourceId}'");
        }
        if (conn.DestinationId != "high")
        {
            throw new InvalidOperationException($"Expected destination_id 'high', got '{conn.DestinationId}'");
        }
        if (conn.Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {conn.Type}");
        }
        if (System.Math.Abs(conn.DistancePc - 2.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 2.0, got {conn.DistancePc}");
        }
    }

    /// <summary>
    /// Tests two systems within 5pc create green connection.
    /// </summary>
    public static void TestTwoSystemsWithin5pcGreenConnection()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 4, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        JumpLaneConnection conn = result.Connections[0];
        if (conn.Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {conn.Type}");
        }
    }

    /// <summary>
    /// Tests two systems at 7pc without bridge create orange connection.
    /// </summary>
    public static void TestTwoSystemsAt7pcNoBridgeOrangeConnection()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 7, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        JumpLaneConnection conn = result.Connections[0];
        if (conn.Type != JumpLaneConnection.ConnectionType.Orange)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Orange, got {conn.Type}");
        }
        if (System.Math.Abs(conn.DistancePc - 7.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 7.0, got {conn.DistancePc}");
        }
    }

    /// <summary>
    /// Tests two systems at 9pc without bridge create red connection in extended phase.
    /// </summary>
    public static void TestTwoSystemsAt9pcNoBridgeNoConnection()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 9, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int redCount = 0;
        if (counts.ContainsKey((int)JumpLaneConnection.ConnectionType.Red))
        {
            redCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Red];
        }
        if (redCount != 1)
        {
            throw new InvalidOperationException($"Expected 1 red connection, got {redCount}");
        }
    }

    /// <summary>
    /// Tests two systems beyond 9pc have no connection.
    /// </summary>
    public static void TestTwoSystemsBeyond9pcNoConnection()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 15, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

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
    /// Tests bridge creates yellow connections.
    /// </summary>
    public static void TestBridgeCreatesYellowConnections()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 8, 0, 0, 5000));
        region.AddSystem(MakeSystem("bridge", 4, 0, 0, 0));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 2)
        {
            throw new InvalidOperationException($"Expected 2 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }

        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int yellowCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Yellow];
        if (yellowCount != 2)
        {
            throw new InvalidOperationException($"Expected 2 yellow connections, got {yellowCount}");
        }
    }

    /// <summary>
    /// Tests bridge gets assigned false population.
    /// </summary>
    public static void TestBridgeGetsFalsePopulation()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 8, 0, 0, 50000));
        region.AddSystem(MakeSystem("bridge", 4, 0, 0, 0));

        JumpLaneResult result = calculator.Calculate(region);

        JumpLaneSystem? bridgeSystem = result.GetSystem("bridge");
        if (bridgeSystem == null)
        {
            throw new InvalidOperationException("Expected bridge system to be registered");
        }
        if (!bridgeSystem.IsBridge)
        {
            throw new InvalidOperationException("Expected bridge system to have is_bridge true");
        }
        if (bridgeSystem.FalsePopulation != 40000)
        {
            throw new InvalidOperationException($"Expected false_population 40000, got {bridgeSystem.FalsePopulation}");
        }
    }

    /// <summary>
    /// Tests bridge is not used as source except for its yellow connection.
    /// </summary>
    public static void TestBridgeNotUsedAsSource()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 8, 0, 0, 50000));
        region.AddSystem(MakeSystem("bridge", 4, 0, 0, 0));
        region.AddSystem(MakeSystem("mid", 5, 1, 0, 20000));

        JumpLaneResult result = calculator.Calculate(region);

        int bridgeAsSourceCount = 0;
        foreach (JumpLaneConnection conn in result.Connections)
        {
            if (conn.SourceId == "bridge")
            {
                bridgeAsSourceCount += 1;
            }
        }

        if (bridgeAsSourceCount != 1)
        {
            throw new InvalidOperationException($"Bridge should only be source in its yellow connection to high, got {bridgeAsSourceCount}");
        }
    }

    /// <summary>
    /// Tests bridge must be within 5pc of both endpoints.
    /// </summary>
    public static void TestBridgeMustBeWithin5pcOfBoth()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("high", 8, 0, 0, 5000));
        region.AddSystem(MakeSystem("bad_bridge", 6, 0, 0, 0));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int redCount = 0;
        if (counts.ContainsKey((int)JumpLaneConnection.ConnectionType.Red))
        {
            redCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Red];
        }
        if (redCount != 1)
        {
            throw new InvalidOperationException($"Expected 1 red connection, got {redCount}");
        }
    }

    /// <summary>
    /// Tests calculator connects to highest populated system within threshold.
    /// </summary>
    public static void TestConnectsToHighestPopulatedWithinThreshold()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("source", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("near_low", 2, 0, 0, 2000));
        region.AddSystem(MakeSystem("near_high", 2.5, 0, 0, 8000));

        JumpLaneResult result = calculator.Calculate(region);

        Godot.Collections.Array<JumpLaneConnection> sourceConns = result.GetConnectionsForSystem("source");
        bool foundHigh = false;
        foreach (JumpLaneConnection conn in sourceConns)
        {
            if (conn.SourceId == "source" && conn.DestinationId == "near_high")
            {
                foundHigh = true;
            }
        }
        if (!foundHigh)
        {
            throw new InvalidOperationException("Should connect to highest populated within threshold");
        }
    }

    /// <summary>
    /// Tests calculator processes lowest population first.
    /// </summary>
    public static void TestProcessesLowestPopulationFirst()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("low", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("mid", 3, 0, 0, 5000));
        region.AddSystem(MakeSystem("high", 6, 0, 0, 10000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 2)
        {
            throw new InvalidOperationException($"Expected 2 connections, got {result.GetTotalConnections()}");
        }

        bool lowToMid = false;
        bool midToHigh = false;
        foreach (JumpLaneConnection conn in result.Connections)
        {
            if (conn.SourceId == "low" && conn.DestinationId == "mid")
            {
                lowToMid = true;
            }
            if (conn.SourceId == "mid" && conn.DestinationId == "high")
            {
                midToHigh = true;
            }
        }

        if (!lowToMid)
        {
            throw new InvalidOperationException("low should connect to mid");
        }
        if (!midToHigh)
        {
            throw new InvalidOperationException("mid should connect to high");
        }
    }

    /// <summary>
    /// Tests multiple orphans when systems are too far apart.
    /// </summary>
    public static void TestMultipleOrphans()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 20, 0, 0, 2000));
        region.AddSystem(MakeSystem("c", 40, 0, 0, 3000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 3)
        {
            throw new InvalidOperationException($"Expected 3 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests chain of connections forms correctly.
    /// </summary>
    public static void TestChainOfConnections()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 3, 0, 0, 2000));
        region.AddSystem(MakeSystem("c", 6, 0, 0, 3000));
        region.AddSystem(MakeSystem("d", 9, 0, 0, 4000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 3)
        {
            throw new InvalidOperationException($"Expected 3 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests mixed connection types are created correctly.
    /// </summary>
    public static void TestMixedConnectionTypes()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 2, 0, 0, 2000));
        region.AddSystem(MakeSystem("c", 9, 0, 0, 3000));
        region.AddSystem(MakeSystem("d", 17, 0, 0, 4000));
        region.AddSystem(MakeSystem("bridge", 13, 0, 0, 0));

        JumpLaneResult result = calculator.Calculate(region);

        Godot.Collections.Dictionary counts = result.GetConnectionCounts();
        int greenCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Green];
        int orangeCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Orange];
        int yellowCount = (int)counts[(int)JumpLaneConnection.ConnectionType.Yellow];

        if (greenCount <= 0)
        {
            throw new InvalidOperationException($"Expected at least 1 green connection, got {greenCount}");
        }
        if (orangeCount <= 0)
        {
            throw new InvalidOperationException($"Expected at least 1 orange connection, got {orangeCount}");
        }
        if (yellowCount <= 0)
        {
            throw new InvalidOperationException($"Expected at least 1 yellow connection, got {yellowCount}");
        }
    }

    /// <summary>
    /// Tests system can receive multiple inbound connections.
    /// </summary>
    public static void TestSystemCanReceiveMultipleInboundConnections()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("hub", 0, 0, 0, 100000));
        region.AddSystem(MakeSystem("spoke_a", 2, 0, 0, 1000));
        region.AddSystem(MakeSystem("spoke_b", 0, 2, 0, 2000));
        region.AddSystem(MakeSystem("spoke_c", 0, 0, 2, 3000));

        JumpLaneResult result = calculator.Calculate(region);

        Godot.Collections.Array<JumpLaneConnection> hubConnections = result.GetConnectionsForSystem("hub");
        if (hubConnections.Count != 3)
        {
            throw new InvalidOperationException($"Hub should have 3 inbound connections, got {hubConnections.Count}");
        }

        if (result.GetTotalOrphans() != 0)
        {
            throw new InvalidOperationException($"Expected 0 orphans, got {result.GetTotalOrphans()}");
        }
    }

    /// <summary>
    /// Tests highest population system is not orphan when receiving connections.
    /// </summary>
    public static void TestHighestPopSystemIsNotOrphanWhenReceivingConnections()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("top", 0, 0, 0, 100000));
        region.AddSystem(MakeSystem("low", 2, 0, 0, 1000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.IsOrphan("top"))
        {
            throw new InvalidOperationException("Top should not be orphan - it receives connection");
        }
        if (result.IsOrphan("low"))
        {
            throw new InvalidOperationException("Low should not be orphan - it makes connection");
        }
        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
    }

    /// <summary>
    /// Tests unpopulated systems are ignored as destinations.
    /// </summary>
    public static void TestUnpopulatedSystemsIgnoredAsDestinations()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("pop", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("unpop", 2, 0, 0, 0));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }
        if (result.GetTotalOrphans() != 1)
        {
            throw new InvalidOperationException($"Expected 1 orphan, got {result.GetTotalOrphans()}");
        }
        if (!result.IsOrphan("pop"))
        {
            throw new InvalidOperationException("Expected 'pop' to be an orphan");
        }
    }

    /// <summary>
    /// Tests 3D distance calculations.
    /// </summary>
    public static void Test3dDistances()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 2, 2, 1, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        JumpLaneConnection conn = result.Connections[0];
        if (conn.Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {conn.Type}");
        }
        if (System.Math.Abs(conn.DistancePc - 3.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 3.0, got {conn.DistancePc}");
        }
    }

    /// <summary>
    /// Tests equal population systems still connect.
    /// </summary>
    public static void TestEqualPopulationStillConnects()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 2, 0, 0, 1000));

        JumpLaneResult result = calculator.Calculate(region);

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
    /// Tests exactly at threshold boundaries.
    /// </summary>
    public static void TestExactlyAtThresholdBoundaries()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 3, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {result.Connections[0].Type}");
        }
    }

    /// <summary>
    /// Tests just beyond threshold still creates green connection.
    /// </summary>
    public static void TestJustBeyondThreshold()
    {
        JumpLaneCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("a", 0, 0, 0, 1000));
        region.AddSystem(MakeSystem("b", 3.01, 0, 0, 5000));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }
        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {result.Connections[0].Type}");
        }
    }
}
