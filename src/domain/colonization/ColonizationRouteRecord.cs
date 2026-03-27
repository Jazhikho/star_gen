using Godot;
using Godot.Collections;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Persisted jump-route record derived from colonization outcomes.
/// </summary>
public partial class ColonizationRouteRecord : RefCounted
{
    /// <summary>
    /// Source system identifier.
    /// </summary>
    public string SourceSystemId { get; set; } = string.Empty;

    /// <summary>
    /// Destination system identifier.
    /// </summary>
    public string DestinationSystemId { get; set; } = string.Empty;

    /// <summary>
    /// Route connection type for rendering.
    /// </summary>
    public JumpLaneConnection.ConnectionType ConnectionType { get; set; } = JumpLaneConnection.ConnectionType.Green;

    /// <summary>
    /// Distance between route endpoints in parsecs.
    /// </summary>
    public double DistancePc { get; set; }

    /// <summary>
    /// Converts the route record to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["source_system_id"] = SourceSystemId,
            ["destination_system_id"] = DestinationSystemId,
            ["connection_type"] = (int)ConnectionType,
            ["distance_pc"] = DistancePc,
        };
    }

    /// <summary>
    /// Rebuilds a route record from a dictionary payload.
    /// </summary>
    public static ColonizationRouteRecord FromDictionary(Dictionary data)
    {
        return new ColonizationRouteRecord
        {
            SourceSystemId = DomainDictionaryUtils.GetString(data, "source_system_id", string.Empty),
            DestinationSystemId = DomainDictionaryUtils.GetString(data, "destination_system_id", string.Empty),
            ConnectionType = (JumpLaneConnection.ConnectionType)DomainDictionaryUtils.GetInt(data, "connection_type", (int)JumpLaneConnection.ConnectionType.Green),
            DistancePc = DomainDictionaryUtils.GetDouble(data, "distance_pc", 0.0),
        };
    }
}
