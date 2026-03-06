using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Jump lane connection between two systems.
/// </summary>
public partial class JumpLaneConnection : RefCounted
{
    /// <summary>
    /// Connection categories based on range and bridging rules.
    /// </summary>
    public enum ConnectionType
    {
        Green,
        Yellow,
        Orange,
        Red,
    }

    /// <summary>
    /// Source system identifier.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Destination system identifier.
    /// </summary>
    public string DestinationId { get; set; } = string.Empty;

    /// <summary>
    /// Connection type.
    /// </summary>
    public ConnectionType Type { get; set; } = ConnectionType.Green;

    /// <summary>
    /// Distance between systems in parsecs.
    /// </summary>
    public double DistancePc { get; set; }

    /// <summary>
    /// Creates a new jump-lane connection.
    /// </summary>
    public JumpLaneConnection(
        string sourceId = "",
        string destinationId = "",
        ConnectionType type = ConnectionType.Green,
        double distancePc = 0.0)
    {
        SourceId = sourceId;
        DestinationId = destinationId;
        Type = type;
        DistancePc = distancePc;
    }

    /// <summary>
    /// Converts the connection to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["source_id"] = SourceId,
            ["destination_id"] = DestinationId,
            ["connection_type"] = (int)Type,
            ["distance_pc"] = DistancePc,
        };
    }

    /// <summary>
    /// Creates a connection from a dictionary payload.
    /// </summary>
    public static JumpLaneConnection FromDictionary(Dictionary data)
    {
        return new JumpLaneConnection(
            DomainDictionaryUtils.GetString(data, "source_id", string.Empty),
            DomainDictionaryUtils.GetString(data, "destination_id", string.Empty),
            (ConnectionType)DomainDictionaryUtils.GetInt(data, "connection_type", (int)ConnectionType.Green),
            DomainDictionaryUtils.GetDouble(data, "distance_pc", 0.0));
    }

    /// <summary>
    /// Returns display color for the connection type.
    /// </summary>
    public Color GetColor()
    {
        return Type switch
        {
            ConnectionType.Green => Colors.Green,
            ConnectionType.Yellow => Colors.Yellow,
            ConnectionType.Orange => Colors.Orange,
            ConnectionType.Red => Colors.Red,
            _ => Colors.White,
        };
    }

    /// <summary>
    /// Returns human-friendly connection type label.
    /// </summary>
    public string GetTypeName()
    {
        return Type switch
        {
            ConnectionType.Green => "Direct (3-5 pc)",
            ConnectionType.Yellow => "Bridged",
            ConnectionType.Orange => "Direct (7 pc)",
            ConnectionType.Red => "Extended (<=10 pc or multi-hop)",
            _ => "Unknown",
        };
    }

}
