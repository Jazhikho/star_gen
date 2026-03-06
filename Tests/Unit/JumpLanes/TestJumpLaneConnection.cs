#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for JumpLaneConnection data model.
/// </summary>
public static class TestJumpLaneConnection
{
    /// <summary>
    /// Tests default initialization values.
    /// </summary>
    public static void TestInitDefaults()
    {
        JumpLaneConnection conn = new();

        if (conn.SourceId != string.Empty)
        {
            throw new InvalidOperationException($"Expected empty source_id, got '{conn.SourceId}'");
        }
        if (conn.DestinationId != string.Empty)
        {
            throw new InvalidOperationException($"Expected empty destination_id, got '{conn.DestinationId}'");
        }
        if (conn.Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Green, got {conn.Type}");
        }
        if (System.Math.Abs(conn.DistancePc - 0.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 0.0, got {conn.DistancePc}");
        }
    }

    /// <summary>
    /// Tests initialization with provided values.
    /// </summary>
    public static void TestInitWithValues()
    {
        JumpLaneConnection conn = new(
            "sys_a",
            "sys_b",
            JumpLaneConnection.ConnectionType.Yellow,
            4.5);

        if (conn.SourceId != "sys_a")
        {
            throw new InvalidOperationException($"Expected source_id 'sys_a', got '{conn.SourceId}'");
        }
        if (conn.DestinationId != "sys_b")
        {
            throw new InvalidOperationException($"Expected destination_id 'sys_b', got '{conn.DestinationId}'");
        }
        if (conn.Type != JumpLaneConnection.ConnectionType.Yellow)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Yellow, got {conn.Type}");
        }
        if (System.Math.Abs(conn.DistancePc - 4.5) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 4.5, got {conn.DistancePc}");
        }
    }

    /// <summary>
    /// Tests serialization round-trip preserves all properties.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        JumpLaneConnection conn = new(
            "source",
            "dest",
            JumpLaneConnection.ConnectionType.Orange,
            7.0);

        Godot.Collections.Dictionary data = conn.ToDictionary();
        JumpLaneConnection restored = JumpLaneConnection.FromDictionary(data);

        if (restored.SourceId != "source")
        {
            throw new InvalidOperationException($"Expected source_id 'source', got '{restored.SourceId}'");
        }
        if (restored.DestinationId != "dest")
        {
            throw new InvalidOperationException($"Expected destination_id 'dest', got '{restored.DestinationId}'");
        }
        if (restored.Type != JumpLaneConnection.ConnectionType.Orange)
        {
            throw new InvalidOperationException($"Expected ConnectionType.Orange, got {restored.Type}");
        }
        if (System.Math.Abs(restored.DistancePc - 7.0) > 0.0001)
        {
            throw new InvalidOperationException($"Expected distance_pc 7.0, got {restored.DistancePc}");
        }
    }

    public static void TestGetColorGreen()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Green;
        if (conn.GetColor() != Colors.Green)
        {
            throw new InvalidOperationException("Expected green connection color.");
        }
    }

    public static void TestGetColorYellow()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Yellow;
        if (conn.GetColor() != Colors.Yellow)
        {
            throw new InvalidOperationException("Expected yellow connection color.");
        }
    }

    public static void TestGetColorOrange()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Orange;
        if (conn.GetColor() != Colors.Orange)
        {
            throw new InvalidOperationException("Expected orange connection color.");
        }
    }

    public static void TestGetColorRed()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Red;
        if (conn.GetColor() != Colors.Red)
        {
            throw new InvalidOperationException("Expected red connection color.");
        }
    }

    public static void TestGetTypeNameGreen()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Green;
        if (conn.GetTypeName() != "Direct (3-5 pc)")
        {
            throw new InvalidOperationException($"Expected green type label, got '{conn.GetTypeName()}'");
        }
    }

    public static void TestGetTypeNameYellow()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Yellow;
        if (conn.GetTypeName() != "Bridged")
        {
            throw new InvalidOperationException($"Expected yellow type label, got '{conn.GetTypeName()}'");
        }
    }

    public static void TestGetTypeNameOrange()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Orange;
        if (conn.GetTypeName() != "Direct (7 pc)")
        {
            throw new InvalidOperationException($"Expected orange type label, got '{conn.GetTypeName()}'");
        }
    }

    public static void TestGetTypeNameRed()
    {
        JumpLaneConnection conn = new();
        conn.Type = JumpLaneConnection.ConnectionType.Red;
        if (conn.GetTypeName() != "Extended (<=10 pc or multi-hop)")
        {
            throw new InvalidOperationException($"Expected red type label, got '{conn.GetTypeName()}'");
        }
    }
}
