using Godot;
using Godot.Collections;

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
    public string SourceId = string.Empty;

    /// <summary>
    /// Destination system identifier.
    /// </summary>
    public string DestinationId = string.Empty;

    /// <summary>
    /// Connection type.
    /// </summary>
    public ConnectionType Type = ConnectionType.Green;

    /// <summary>
    /// Distance between systems in parsecs.
    /// </summary>
    public double DistancePc;

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
    /// Returns the display color for this connection.
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
    /// Returns a human-readable connection-type label.
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
            GetString(data, "source_id", string.Empty),
            GetString(data, "destination_id", string.Empty),
            (ConnectionType)GetInt(data, "connection_type", (int)ConnectionType.Green),
            GetDouble(data, "distance_pc", 0.0));
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => double.TryParse((string)value, out double parsed) ? parsed : fallback,
            _ => fallback,
        };
    }
}
