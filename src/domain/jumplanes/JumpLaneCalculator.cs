using Godot;
using Godot.Collections;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Calculates jump-lane connections between star systems.
/// </summary>
public partial class JumpLaneCalculator : RefCounted
{
    /// <summary>
    /// Short direct threshold in parsecs.
    /// </summary>
    public const double ThresholdDirectShort = 3.0;

    /// <summary>
    /// Medium direct threshold in parsecs.
    /// </summary>
    public const double ThresholdDirectMedium = 5.0;

    /// <summary>
    /// Maximum range for orange or bridged linking.
    /// </summary>
    public const double ThresholdBridgeMax = 7.0;

    /// <summary>
    /// Maximum range where bridging is allowed.
    /// </summary>
    public const double ThresholdBridgeOnly = 9.0;

    /// <summary>
    /// Maximum bridge distance to either endpoint.
    /// </summary>
    public const double BridgeMaxDistance = 5.0;

    private readonly JumpLaneClusterConnector _clusterConnector = new();

    /// <summary>
    /// Calculates jump lanes for all systems in a region.
    /// </summary>
    public JumpLaneResult Calculate(JumpLaneRegion region)
    {
        JumpLaneResult result = new();
        foreach (JumpLaneSystem system in region.Systems)
        {
            result.RegisterSystem(system);
        }

        Dictionary connectedSystems = new();
        Array<JumpLaneSystem> sortedSystems = region.GetSystemsSortedByPopulation();
        Dictionary bridgeIds = new();

        foreach (JumpLaneSystem system in sortedSystems)
        {
            if (bridgeIds.ContainsKey(system.Id))
            {
                continue;
            }

            bool connectionMade = TryConnectSystem(
                system,
                region,
                result,
                connectedSystems,
                bridgeIds);
            if (connectionMade)
            {
                connectedSystems[system.Id] = true;
            }
        }

        foreach (JumpLaneSystem system in sortedSystems)
        {
            if (!connectedSystems.ContainsKey(system.Id))
            {
                result.AddOrphan(system.Id);
            }
        }

        _clusterConnector.ConnectClusters(region, result);
        return result;
    }

    /// <summary>
    /// Attempts to connect one system to a higher-populated system.
    /// </summary>
    private bool TryConnectSystem(
        JumpLaneSystem system,
        JumpLaneRegion region,
        JumpLaneResult result,
        Dictionary connectedSystems,
        Dictionary bridgeIds)
    {
        Array<JumpLaneSystem> candidates = GetHigherPopulatedSystems(system, region);
        if (candidates.Count == 0)
        {
            return false;
        }

        if (TryThreshold(system, candidates, ThresholdDirectShort, result, connectedSystems))
        {
            return true;
        }

        if (TryThreshold(system, candidates, ThresholdDirectMedium, result, connectedSystems))
        {
            return true;
        }

        if (TryExtendedThreshold(system, candidates, region, result, connectedSystems, bridgeIds))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries a direct threshold for a green connection.
    /// </summary>
    private bool TryThreshold(
        JumpLaneSystem system,
        Array<JumpLaneSystem> candidates,
        double threshold,
        JumpLaneResult result,
        Dictionary connectedSystems)
    {
        JumpLaneSystem? target = FindHighestPopulatedWithin(system, candidates, threshold);
        if (target == null)
        {
            return false;
        }

        AddDirectConnection(system, target, result, connectedSystems);
        return true;
    }

    /// <summary>
    /// Tries the extended thresholds for orange or bridged connections.
    /// </summary>
    private bool TryExtendedThreshold(
        JumpLaneSystem system,
        Array<JumpLaneSystem> candidates,
        JumpLaneRegion region,
        JumpLaneResult result,
        Dictionary connectedSystems,
        Dictionary bridgeIds)
    {
        JumpLaneSystem? target = FindHighestPopulatedWithin(system, candidates, ThresholdBridgeMax);
        if (target != null)
        {
            JumpLaneSystem? bridge = FindBridge(system, target, region);
            if (bridge != null)
            {
                AddBridgedConnection(system, target, bridge, result, connectedSystems, bridgeIds);
            }
            else
            {
                AddOrangeConnection(system, target, result, connectedSystems);
            }

            return true;
        }

        target = FindHighestPopulatedWithin(system, candidates, ThresholdBridgeOnly);
        if (target == null)
        {
            return false;
        }

        double distance = system.DistanceTo(target);
        JumpLaneSystem? extendedBridge = FindBridge(system, target, region);
        if (extendedBridge != null)
        {
            AddBridgedConnection(system, target, extendedBridge, result, connectedSystems, bridgeIds);
            return true;
        }

        if (distance <= ThresholdBridgeMax)
        {
            AddOrangeConnection(system, target, result, connectedSystems);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets systems with higher effective population than the source.
    /// </summary>
    private Array<JumpLaneSystem> GetHigherPopulatedSystems(JumpLaneSystem source, JumpLaneRegion region)
    {
        Array<JumpLaneSystem> systems = new();
        int sourcePopulation = source.GetEffectivePopulation();
        foreach (JumpLaneSystem system in region.Systems)
        {
            if (system.Id == source.Id)
            {
                continue;
            }

            if (!system.IsPopulated() && system.FalsePopulation < 0)
            {
                continue;
            }

            if (system.GetEffectivePopulation() > sourcePopulation)
            {
                systems.Add(system);
            }
        }

        return systems;
    }

    /// <summary>
    /// Finds the highest-populated system within a range threshold.
    /// </summary>
    private JumpLaneSystem? FindHighestPopulatedWithin(
        JumpLaneSystem source,
        Array<JumpLaneSystem> candidates,
        double maxDistance)
    {
        JumpLaneSystem? best = null;
        int bestPopulation = -1;
        foreach (JumpLaneSystem candidate in candidates)
        {
            double distance = source.DistanceTo(candidate);
            if (distance <= maxDistance)
            {
                int population = candidate.GetEffectivePopulation();
                if (population > bestPopulation)
                {
                    best = candidate;
                    bestPopulation = population;
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Finds a bridge system between two endpoints.
    /// </summary>
    private JumpLaneSystem? FindBridge(
        JumpLaneSystem source,
        JumpLaneSystem target,
        JumpLaneRegion region)
    {
        JumpLaneSystem? bestBridge = null;
        double bestTotalDistance = double.PositiveInfinity;
        foreach (JumpLaneSystem system in region.Systems)
        {
            if (system.Id == source.Id || system.Id == target.Id)
            {
                continue;
            }

            if (system.IsPopulated())
            {
                continue;
            }

            double distanceToSource = system.DistanceTo(source);
            double distanceToTarget = system.DistanceTo(target);
            if (distanceToSource <= BridgeMaxDistance && distanceToTarget <= BridgeMaxDistance)
            {
                double totalDistance = distanceToSource + distanceToTarget;
                if (totalDistance < bestTotalDistance)
                {
                    bestBridge = system;
                    bestTotalDistance = totalDistance;
                }
            }
        }

        return bestBridge;
    }

    /// <summary>
    /// Adds a green direct connection.
    /// </summary>
    private void AddDirectConnection(
        JumpLaneSystem source,
        JumpLaneSystem target,
        JumpLaneResult result,
        Dictionary connectedSystems)
    {
        double distance = source.DistanceTo(target);
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            target.Id,
            JumpLaneConnection.ConnectionType.Green,
            distance));
        connectedSystems[source.Id] = true;
        connectedSystems[target.Id] = true;
    }

    /// <summary>
    /// Adds an orange direct connection.
    /// </summary>
    private void AddOrangeConnection(
        JumpLaneSystem source,
        JumpLaneSystem target,
        JumpLaneResult result,
        Dictionary connectedSystems)
    {
        double distance = source.DistanceTo(target);
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            target.Id,
            JumpLaneConnection.ConnectionType.Orange,
            distance));
        connectedSystems[source.Id] = true;
        connectedSystems[target.Id] = true;
    }

    /// <summary>
    /// Adds a yellow bridged connection via an intermediate system.
    /// </summary>
    private void AddBridgedConnection(
        JumpLaneSystem source,
        JumpLaneSystem target,
        JumpLaneSystem bridge,
        JumpLaneResult result,
        Dictionary connectedSystems,
        Dictionary bridgeIds)
    {
        bridge.MakeBridge(target.GetEffectivePopulation());
        bridgeIds[bridge.Id] = true;

        result.AddConnection(new JumpLaneConnection(
            source.Id,
            bridge.Id,
            JumpLaneConnection.ConnectionType.Yellow,
            source.DistanceTo(bridge)));
        result.AddConnection(new JumpLaneConnection(
            bridge.Id,
            target.Id,
            JumpLaneConnection.ConnectionType.Yellow,
            bridge.DistanceTo(target)));

        connectedSystems[source.Id] = true;
        connectedSystems[target.Id] = true;
        connectedSystems[bridge.Id] = true;
    }
}
