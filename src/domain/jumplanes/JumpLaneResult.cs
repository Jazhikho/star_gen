using Godot;
using Godot.Collections;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Result payload from a jump-lane calculation.
/// </summary>
public partial class JumpLaneResult : RefCounted
{
    /// <summary>
    /// Generated connections.
    /// </summary>
    public Array<JumpLaneConnection> Connections = new();

    /// <summary>
    /// Orphan system identifiers.
    /// </summary>
    public Array<string> OrphanIds = new();

    /// <summary>
    /// Registered systems by identifier.
    /// </summary>
    public Dictionary Systems = new();

    /// <summary>
    /// Adds a connection to the result.
    /// </summary>
    public void AddConnection(JumpLaneConnection connection)
    {
        Connections.Add(connection);
    }

    /// <summary>
    /// Adds an orphan system identifier.
    /// </summary>
    public void AddOrphan(string systemId)
    {
        OrphanIds.Add(systemId);
    }

    /// <summary>
    /// Registers a system by identifier.
    /// </summary>
    public void RegisterSystem(JumpLaneSystem system)
    {
        Systems[system.Id] = system;
    }

    /// <summary>
    /// Returns counts by connection type.
    /// </summary>
    public Dictionary GetConnectionCounts()
    {
        Dictionary counts = new()
        {
            [(int)JumpLaneConnection.ConnectionType.Green] = 0,
            [(int)JumpLaneConnection.ConnectionType.Yellow] = 0,
            [(int)JumpLaneConnection.ConnectionType.Orange] = 0,
            [(int)JumpLaneConnection.ConnectionType.Red] = 0,
        };

        foreach (JumpLaneConnection connection in Connections)
        {
            int key = (int)connection.Type;
            counts[key] = (int)counts[key] + 1;
        }

        return counts;
    }

    /// <summary>
    /// Returns connections involving a specific system.
    /// </summary>
    public Array<JumpLaneConnection> GetConnectionsForSystem(string systemId)
    {
        Array<JumpLaneConnection> matches = new();
        foreach (JumpLaneConnection connection in Connections)
        {
            if (connection.SourceId == systemId || connection.DestinationId == systemId)
            {
                matches.Add(connection);
            }
        }

        return matches;
    }

    /// <summary>
    /// Returns the <see cref="JumpLaneSystem"/> registered under the given identifier, or null when absent.
    /// </summary>
    /// <param name="systemId">Identifier of the system to retrieve.</param>
    /// <returns>The registered system, or null when the identifier is not found.</returns>
    public JumpLaneSystem? GetSystem(string systemId)
    {
        if (!Systems.ContainsKey(systemId))
        {
            return null;
        }

        return Systems[systemId].Obj as JumpLaneSystem;
    }

    /// <summary>
    /// Returns whether a system is marked as an orphan.
    /// </summary>
    public bool IsOrphan(string systemId)
    {
        return OrphanIds.Contains(systemId);
    }

    /// <summary>
    /// Returns the total connection count.
    /// </summary>
    public int GetTotalConnections()
    {
        return Connections.Count;
    }

    /// <summary>
    /// Returns the total orphan count.
    /// </summary>
    public int GetTotalOrphans()
    {
        return OrphanIds.Count;
    }

    /// <summary>
    /// Converts the result to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> connections = new();
        foreach (JumpLaneConnection connection in Connections)
        {
            connections.Add(connection.ToDictionary());
        }

        Array<string> orphanIds = new();
        foreach (string orphanId in OrphanIds)
        {
            orphanIds.Add(orphanId);
        }

        Array<Dictionary> systems = new();
        foreach (Variant value in Systems.Values)
        {
            if (value.Obj is JumpLaneSystem system)
            {
                systems.Add(system.ToDictionary());
            }
        }

        return new Dictionary
        {
            ["connections"] = connections,
            ["orphan_ids"] = orphanIds,
            ["systems"] = systems,
        };
    }

    /// <summary>
    /// Creates a result from a dictionary payload.
    /// </summary>
    public static JumpLaneResult FromDictionary(Dictionary data)
    {
        JumpLaneResult result = new();
        if (data.ContainsKey("connections") && data["connections"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["connections"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    result.Connections.Add(JumpLaneConnection.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("orphan_ids") && data["orphan_ids"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["orphan_ids"])
            {
                if (value.VariantType == Variant.Type.String)
                {
                    result.OrphanIds.Add((string)value);
                }
            }
        }

        if (data.ContainsKey("systems") && data["systems"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["systems"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    JumpLaneSystem system = JumpLaneSystem.FromDictionary((Dictionary)value);
                    result.Systems[system.Id] = system;
                }
            }
        }

        return result;
    }
}
