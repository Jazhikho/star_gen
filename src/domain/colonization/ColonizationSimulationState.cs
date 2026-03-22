using Godot;
using Godot.Collections;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Persisted result of an explicit colonization simulation run.
/// </summary>
public partial class ColonizationSimulationState : RefCounted
{
    /// <summary>
    /// Region identifier for the simulation scope.
    /// </summary>
    public string RegionId { get; set; } = string.Empty;

    /// <summary>
    /// Region scope.
    /// </summary>
    public JumpLaneRegion.RegionScope Scope { get; set; } = JumpLaneRegion.RegionScope.Subsector;

    /// <summary>
    /// Settings used for the run.
    /// </summary>
    public ColonizationSimulationSettings Settings { get; set; } = ColonizationSimulationSettings.CreateDefault();

    /// <summary>
    /// Final simulated route-system summaries for the region.
    /// </summary>
    public Array<JumpLaneSystem> Systems { get; set; } = new();

    /// <summary>
    /// Persisted settlement events.
    /// </summary>
    public Array<ColonizationSettlementRecord> Settlements { get; set; } = new();

    /// <summary>
    /// Persisted route records derived from settlements.
    /// </summary>
    public Array<ColonizationRouteRecord> Routes { get; set; } = new();

    /// <summary>
    /// Orphan system identifiers for display.
    /// </summary>
    public Array<string> OrphanIds { get; set; } = new();

    /// <summary>
    /// Converts the state to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> systems = new();
        foreach (JumpLaneSystem system in Systems)
        {
            systems.Add(system.ToDictionary());
        }

        Array<Dictionary> settlements = new();
        foreach (ColonizationSettlementRecord settlement in Settlements)
        {
            settlements.Add(settlement.ToDictionary());
        }

        Array<Dictionary> routes = new();
        foreach (ColonizationRouteRecord route in Routes)
        {
            routes.Add(route.ToDictionary());
        }

        Array<string> orphanIds = new();
        foreach (string orphanId in OrphanIds)
        {
            orphanIds.Add(orphanId);
        }

        return new Dictionary
        {
            ["region_id"] = RegionId,
            ["scope"] = (int)Scope,
            ["settings"] = Settings.ToDictionary(),
            ["systems"] = systems,
            ["settlements"] = settlements,
            ["routes"] = routes,
            ["orphan_ids"] = orphanIds,
        };
    }

    /// <summary>
    /// Rebuilds state from a dictionary payload.
    /// </summary>
    public static ColonizationSimulationState FromDictionary(Dictionary data)
    {
        ColonizationSimulationState state = new ColonizationSimulationState
        {
            RegionId = DomainDictionaryUtils.GetString(data, "region_id", string.Empty),
            Scope = (JumpLaneRegion.RegionScope)DomainDictionaryUtils.GetInt(data, "scope", (int)JumpLaneRegion.RegionScope.Subsector),
        };

        if (data.ContainsKey("settings") && data["settings"].VariantType == Variant.Type.Dictionary)
        {
            state.Settings = ColonizationSimulationSettings.FromDictionary((Dictionary)data["settings"]);
        }

        if (data.ContainsKey("systems") && data["systems"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["systems"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    state.Systems.Add(JumpLaneSystem.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("settlements") && data["settlements"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["settlements"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    state.Settlements.Add(ColonizationSettlementRecord.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("routes") && data["routes"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["routes"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    state.Routes.Add(ColonizationRouteRecord.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("orphan_ids") && data["orphan_ids"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["orphan_ids"])
            {
                if (value.VariantType == Variant.Type.String)
                {
                    state.OrphanIds.Add((string)value);
                }
            }
        }

        return state;
    }

    /// <summary>
    /// Returns settlement records for the supplied destination system id.
    /// </summary>
    public Array<ColonizationSettlementRecord> GetSettlementsForDestination(string destinationSystemId)
    {
        Array<ColonizationSettlementRecord> matches = new();
        foreach (ColonizationSettlementRecord settlement in Settlements)
        {
            if (settlement.DestinationSystemId == destinationSystemId)
            {
                matches.Add(settlement);
            }
        }

        return matches;
    }

    /// <summary>
    /// Converts the state back into a route-system region for rendering.
    /// </summary>
    public JumpLaneRegion ToJumpLaneRegion()
    {
        JumpLaneRegion region = new JumpLaneRegion(Scope, RegionId);
        foreach (JumpLaneSystem system in Systems)
        {
            region.AddSystem(CloneSystem(system));
        }

        return region;
    }

    /// <summary>
    /// Converts the state into a jump-lane render result.
    /// </summary>
    public JumpLaneResult ToJumpLaneResult()
    {
        JumpLaneResult result = new JumpLaneResult();
        foreach (JumpLaneSystem system in Systems)
        {
            result.RegisterSystem(CloneSystem(system));
        }

        foreach (ColonizationRouteRecord route in Routes)
        {
            result.AddConnection(new JumpLaneConnection(
                route.SourceSystemId,
                route.DestinationSystemId,
                route.ConnectionType,
                route.DistancePc));
        }

        foreach (string orphanId in OrphanIds)
        {
            result.AddOrphan(orphanId);
        }

        return result;
    }

    private static JumpLaneSystem CloneSystem(JumpLaneSystem source)
    {
        JumpLaneSystem clone = new JumpLaneSystem(source.Id, source.Position, source.Population);
        clone.FalsePopulation = source.FalsePopulation;
        clone.IsBridge = source.IsBridge;
        clone.TravellerProfile = source.TravellerProfile;
        clone.CanExportColonists = source.CanExportColonists;
        clone.ExportPressure = source.ExportPressure;
        clone.ColonyTargetScore = source.ColonyTargetScore;
        clone.ColonyTargetCapacity = source.ColonyTargetCapacity;
        clone.ColonizationRangePc = source.ColonizationRangePc;
        clone.RouteTechnologyLevel = source.RouteTechnologyLevel;
        clone.ExportBodyId = source.ExportBodyId;
        clone.ColonyTargetBodyId = source.ColonyTargetBodyId;
        clone.ExportCivilizationId = source.ExportCivilizationId;
        clone.ExportCivilizationName = source.ExportCivilizationName;
        return clone;
    }
}
