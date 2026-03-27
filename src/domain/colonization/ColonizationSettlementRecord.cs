using Godot;
using Godot.Collections;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Persisted settlement event created by the colonization simulator.
/// </summary>
public partial class ColonizationSettlementRecord : RefCounted
{
    /// <summary>
    /// Source system identifier.
    /// </summary>
    public string SourceSystemId { get; set; } = string.Empty;

    /// <summary>
    /// Source body identifier.
    /// </summary>
    public string SourceBodyId { get; set; } = string.Empty;

    /// <summary>
    /// Destination system identifier.
    /// </summary>
    public string DestinationSystemId { get; set; } = string.Empty;

    /// <summary>
    /// Destination body identifier.
    /// </summary>
    public string DestinationBodyId { get; set; } = string.Empty;

    /// <summary>
    /// Simulation epoch when the settlement was founded.
    /// </summary>
    public int Epoch { get; set; }

    /// <summary>
    /// Settled colony population.
    /// </summary>
    public int Population { get; set; }

    /// <summary>
    /// Exporter technology level.
    /// </summary>
    public int TechnologyLevel { get; set; } = -1;

    /// <summary>
    /// Founder civilization identifier.
    /// </summary>
    public string FoundingCivilizationId { get; set; } = string.Empty;

    /// <summary>
    /// Founder civilization display name.
    /// </summary>
    public string FoundingCivilizationName { get; set; } = string.Empty;

    /// <summary>
    /// Route type used to reach the destination.
    /// </summary>
    public JumpLaneConnection.ConnectionType RouteConnectionType { get; set; } = JumpLaneConnection.ConnectionType.Green;

    /// <summary>
    /// Distance between settlement endpoints in parsecs.
    /// </summary>
    public double RouteDistancePc { get; set; }

    /// <summary>
    /// Converts the record to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["source_system_id"] = SourceSystemId,
            ["source_body_id"] = SourceBodyId,
            ["destination_system_id"] = DestinationSystemId,
            ["destination_body_id"] = DestinationBodyId,
            ["epoch"] = Epoch,
            ["population"] = Population,
            ["technology_level"] = TechnologyLevel,
            ["founding_civilization_id"] = FoundingCivilizationId,
            ["founding_civilization_name"] = FoundingCivilizationName,
            ["route_connection_type"] = (int)RouteConnectionType,
            ["route_distance_pc"] = RouteDistancePc,
        };
    }

    /// <summary>
    /// Rebuilds the record from a dictionary payload.
    /// </summary>
    public static ColonizationSettlementRecord FromDictionary(Dictionary data)
    {
        return new ColonizationSettlementRecord
        {
            SourceSystemId = DomainDictionaryUtils.GetString(data, "source_system_id", string.Empty),
            SourceBodyId = DomainDictionaryUtils.GetString(data, "source_body_id", string.Empty),
            DestinationSystemId = DomainDictionaryUtils.GetString(data, "destination_system_id", string.Empty),
            DestinationBodyId = DomainDictionaryUtils.GetString(data, "destination_body_id", string.Empty),
            Epoch = DomainDictionaryUtils.GetInt(data, "epoch", 0),
            Population = DomainDictionaryUtils.GetInt(data, "population", 0),
            TechnologyLevel = DomainDictionaryUtils.GetInt(data, "technology_level", -1),
            FoundingCivilizationId = DomainDictionaryUtils.GetString(data, "founding_civilization_id", string.Empty),
            FoundingCivilizationName = DomainDictionaryUtils.GetString(data, "founding_civilization_name", string.Empty),
            RouteConnectionType = (JumpLaneConnection.ConnectionType)DomainDictionaryUtils.GetInt(data, "route_connection_type", (int)JumpLaneConnection.ConnectionType.Green),
            RouteDistancePc = DomainDictionaryUtils.GetDouble(data, "route_distance_pc", 0.0),
        };
    }
}
