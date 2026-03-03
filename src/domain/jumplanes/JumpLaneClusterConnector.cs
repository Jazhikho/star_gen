using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Identifies connected clusters and links nearby clusters.
/// </summary>
public partial class JumpLaneClusterConnector : RefCounted
{
    /// <summary>
    /// Maximum distance for inter-cluster connections.
    /// </summary>
    public const double MaxClusterDistance = 9.0;

    /// <summary>
    /// Maximum per-hop distance for extended red connections.
    /// </summary>
    public const double MaxExtendedDistance = 10.0;

    /// <summary>
    /// Shared green-threshold distance.
    /// </summary>
    public const double ThresholdGreen = 5.0;

    /// <summary>
    /// Shared orange-threshold distance.
    /// </summary>
    public const double ThresholdOrange = 7.0;

    /// <summary>
    /// Maximum bridge distance to either endpoint.
    /// </summary>
    public const double BridgeMaxDistance = 5.0;

    /// <summary>
    /// Connects isolated clusters until no more connections can be made.
    /// </summary>
    public void ConnectClusters(JumpLaneRegion region, JumpLaneResult result)
    {
        int iterations = 0;
        const int maxIterations = 100;
        while (iterations < maxIterations)
        {
            bool newConnection = TryConnectOneClusterPair(region, result);
            if (!newConnection)
            {
                break;
            }

            iterations += 1;
        }

        TryExtendedConnections(region, result);
    }

    /// <summary>
    /// Attempts to connect one pair of clusters.
    /// </summary>
    private bool TryConnectOneClusterPair(JumpLaneRegion region, JumpLaneResult result)
    {
        Array<Array<string>> clusters = IdentifyClusters(result);
        if (clusters.Count < 2)
        {
            return false;
        }

        Dictionary bestPair = FindClosestClusterPair(clusters, region, result);
        if (bestPair.Count == 0)
        {
            return false;
        }

        CreateClusterConnection(bestPair, region, result);
        return true;
    }

    /// <summary>
    /// Tries extended red links between remaining clusters.
    /// </summary>
    private void TryExtendedConnections(JumpLaneRegion region, JumpLaneResult result)
    {
        int iterations = 0;
        const int maxIterations = 100;
        while (iterations < maxIterations)
        {
            Array<Array<string>> clusters = IdentifyClusters(result);
            if (clusters.Count < 2)
            {
                break;
            }

            Dictionary bestPair = FindClosestClusterPairWithin(
                clusters,
                region,
                result,
                MaxExtendedDistance);
            if (bestPair.Count > 0)
            {
                CreateDirectRedConnection(bestPair, result);
                iterations += 1;
                continue;
            }

            Dictionary pathInfo = FindMultiHopPath(clusters, result);
            if (pathInfo.Count == 0)
            {
                break;
            }

            CreateMultiHopRedConnections(pathInfo, result);
            iterations += 1;
        }
    }

    /// <summary>
    /// Finds the closest pair of clusters within a distance ceiling.
    /// </summary>
    private Dictionary FindClosestClusterPairWithin(
        Array<Array<string>> clusters,
        JumpLaneRegion region,
        JumpLaneResult result,
        double maxDistance)
    {
        double bestDistance = double.PositiveInfinity;
        Dictionary bestPair = new();

        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                Dictionary pairInfo = FindClosestSystemsBetweenClusters(
                    clusters[indexA],
                    clusters[indexB],
                    result);
                double distance = GetDouble(pairInfo, "distance", double.PositiveInfinity);
                if (distance < bestDistance && distance <= maxDistance)
                {
                    bestDistance = distance;
                    bestPair = pairInfo;
                    bestPair["cluster_a"] = clusters[indexA];
                    bestPair["cluster_b"] = clusters[indexB];
                    bestPair["region"] = region;
                }
            }
        }

        return bestPair;
    }

    /// <summary>
    /// Creates a direct red connection between two clusters.
    /// </summary>
    private void CreateDirectRedConnection(Dictionary pairInfo, JumpLaneResult result)
    {
        JumpLaneSystem? systemA = GetSystem(result, GetString(pairInfo, "system_a", string.Empty));
        JumpLaneSystem? systemB = GetSystem(result, GetString(pairInfo, "system_b", string.Empty));
        if (systemA == null || systemB == null)
        {
            return;
        }

        JumpLaneSystem source = systemA;
        JumpLaneSystem destination = systemB;
        if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
        {
            source = systemB;
            destination = systemA;
        }

        double distance = source.DistanceTo(destination);
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            destination.Id,
            JumpLaneConnection.ConnectionType.Red,
            distance));
        RemoveFromOrphans(source.Id, result);
        RemoveFromOrphans(destination.Id, result);
    }

    /// <summary>
    /// Finds a multi-hop path between clusters where each hop is within range.
    /// </summary>
    private Dictionary FindMultiHopPath(Array<Array<string>> clusters, JumpLaneResult result)
    {
        Dictionary graph = BuildExtendedGraph(result, MaxExtendedDistance);
        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                Array<string> path = BfsPath(clusters[indexA], clusters[indexB], graph);
                if (path.Count > 0)
                {
                    return new Dictionary
                    {
                        ["path"] = path,
                    };
                }
            }
        }

        return new Dictionary();
    }

    /// <summary>
    /// Builds adjacency for the extended-connection phase.
    /// </summary>
    private Dictionary BuildExtendedGraph(JumpLaneResult result, double maxPc)
    {
        Dictionary graph = new();
        foreach (Variant idAValue in result.Systems.Keys)
        {
            string idA = idAValue.AsString();
            Array<string> neighbors = new();
            graph[idA] = neighbors;
            JumpLaneSystem? systemA = GetSystem(result, idA);
            if (systemA == null)
            {
                continue;
            }

            foreach (Variant idBValue in result.Systems.Keys)
            {
                string idB = idBValue.AsString();
                if (idA == idB)
                {
                    continue;
                }

                JumpLaneSystem? systemB = GetSystem(result, idB);
                if (systemB == null)
                {
                    continue;
                }

                if (systemA.DistanceTo(systemB) <= maxPc)
                {
                    neighbors.Add(idB);
                }
            }
        }

        return graph;
    }

    /// <summary>
    /// Finds a BFS path from any node in cluster A to any node in cluster B.
    /// </summary>
    private Array<string> BfsPath(Array<string> clusterA, Array<string> clusterB, Dictionary graph)
    {
        Dictionary clusterBSet = new();
        foreach (string idB in clusterB)
        {
            clusterBSet[idB] = true;
        }

        List<string> queue = new();
        Dictionary parent = new();
        int queueIndex = 0;
        foreach (string idA in clusterA)
        {
            queue.Add(idA);
            parent[idA] = string.Empty;
        }

        while (queueIndex < queue.Count)
        {
            string current = queue[queueIndex];
            queueIndex += 1;
            if (clusterBSet.ContainsKey(current))
            {
                Array<string> path = new();
                string node = current;
                while (!string.IsNullOrEmpty(node))
                {
                    path.Insert(0, node);
                    node = parent.ContainsKey(node) ? parent[node].AsString() : string.Empty;
                }

                return path;
            }

            if (!graph.ContainsKey(current))
            {
                continue;
            }

            foreach (string neighbor in (Array<string>)graph[current])
            {
                if (!parent.ContainsKey(neighbor))
                {
                    parent[neighbor] = current;
                    queue.Add(neighbor);
                }
            }
        }

        return new Array<string>();
    }

    /// <summary>
    /// Creates red connections along a multi-hop path.
    /// </summary>
    private void CreateMultiHopRedConnections(Dictionary pathInfo, JumpLaneResult result)
    {
        if (!pathInfo.ContainsKey("path") || pathInfo["path"].VariantType != Variant.Type.Array)
        {
            return;
        }

        Array<string> path = (Array<string>)pathInfo["path"];
        if (path.Count < 2)
        {
            return;
        }

        for (int index = 0; index < path.Count - 1; index += 1)
        {
            string idA = path[index];
            string idB = path[index + 1];
            if (ConnectionExists(result, idA, idB))
            {
                continue;
            }

            JumpLaneSystem? systemA = GetSystem(result, idA);
            JumpLaneSystem? systemB = GetSystem(result, idB);
            if (systemA == null || systemB == null)
            {
                continue;
            }

            JumpLaneSystem source = systemA;
            JumpLaneSystem destination = systemB;
            if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
            {
                source = systemB;
                destination = systemA;
            }

            result.AddConnection(new JumpLaneConnection(
                source.Id,
                destination.Id,
                JumpLaneConnection.ConnectionType.Red,
                source.DistanceTo(destination)));

            JumpLaneSystem? fromSystem = GetSystem(result, idA);
            if (index > 0 && fromSystem != null)
            {
                fromSystem.IsBridge = true;
                int higherPopulation = destination.GetEffectivePopulation();
                if (fromSystem.GetEffectivePopulation() < higherPopulation)
                {
                    fromSystem.MakeBridge(higherPopulation);
                }
            }
        }

        RemoveFromOrphans(path[0], result);
        RemoveFromOrphans(path[path.Count - 1], result);
        for (int index = 1; index < path.Count - 1; index += 1)
        {
            RemoveFromOrphans(path[index], result);
        }
    }

    /// <summary>
    /// Returns whether a connection already exists between two systems.
    /// </summary>
    private bool ConnectionExists(JumpLaneResult result, string idA, string idB)
    {
        foreach (JumpLaneConnection connection in result.Connections)
        {
            bool forward = connection.SourceId == idA && connection.DestinationId == idB;
            bool reverse = connection.SourceId == idB && connection.DestinationId == idA;
            if (forward || reverse)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Identifies connected clusters among populated systems and bridges.
    /// </summary>
    private Array<Array<string>> IdentifyClusters(JumpLaneResult result)
    {
        Dictionary adjacency = new();
        foreach (Variant systemIdValue in result.Systems.Keys)
        {
            string systemId = systemIdValue.AsString();
            JumpLaneSystem? system = GetSystem(result, systemId);
            if (system != null && (system.IsPopulated() || system.IsBridge))
            {
                adjacency[systemId] = new Array<string>();
            }
        }

        foreach (JumpLaneConnection connection in result.Connections)
        {
            if (adjacency.ContainsKey(connection.SourceId) && adjacency.ContainsKey(connection.DestinationId))
            {
                ((Array<string>)adjacency[connection.SourceId]).Add(connection.DestinationId);
                ((Array<string>)adjacency[connection.DestinationId]).Add(connection.SourceId);
            }
        }

        Dictionary visited = new();
        Array<Array<string>> clusters = new();
        foreach (Variant systemIdValue in adjacency.Keys)
        {
            string systemId = systemIdValue.AsString();
            if (visited.ContainsKey(systemId))
            {
                continue;
            }

            Array<string> cluster = new();
            FloodFill(systemId, adjacency, visited, cluster);
            if (cluster.Count > 0)
            {
                clusters.Add(cluster);
            }
        }

        return clusters;
    }

    /// <summary>
    /// Flood-fills a cluster from a starting system.
    /// </summary>
    private void FloodFill(string startId, Dictionary adjacency, Dictionary visited, Array<string> cluster)
    {
        List<string> stack = new() { startId };
        while (stack.Count > 0)
        {
            int lastIndex = stack.Count - 1;
            string current = stack[lastIndex];
            stack.RemoveAt(lastIndex);

            if (visited.ContainsKey(current))
            {
                continue;
            }

            visited[current] = true;
            cluster.Add(current);
            if (!adjacency.ContainsKey(current))
            {
                continue;
            }

            foreach (string neighbor in (Array<string>)adjacency[current])
            {
                if (!visited.ContainsKey(neighbor))
                {
                    stack.Add(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Finds the closest pair of clusters within normal connection range.
    /// </summary>
    private Dictionary FindClosestClusterPair(
        Array<Array<string>> clusters,
        JumpLaneRegion region,
        JumpLaneResult result)
    {
        double bestDistance = double.PositiveInfinity;
        Dictionary bestPair = new();
        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                Dictionary pairInfo = FindClosestSystemsBetweenClusters(
                    clusters[indexA],
                    clusters[indexB],
                    result);
                double distance = GetDouble(pairInfo, "distance", double.PositiveInfinity);
                if (distance < bestDistance && distance <= MaxClusterDistance)
                {
                    bestDistance = distance;
                    bestPair = pairInfo;
                    bestPair["cluster_a"] = clusters[indexA];
                    bestPair["cluster_b"] = clusters[indexB];
                    bestPair["region"] = region;
                }
            }
        }

        return bestPair;
    }

    /// <summary>
    /// Finds the closest systems between two clusters.
    /// </summary>
    private Dictionary FindClosestSystemsBetweenClusters(
        Array<string> clusterA,
        Array<string> clusterB,
        JumpLaneResult result)
    {
        double bestDistance = double.PositiveInfinity;
        string bestSystemA = string.Empty;
        string bestSystemB = string.Empty;

        foreach (string idA in clusterA)
        {
            JumpLaneSystem? systemA = GetSystem(result, idA);
            if (systemA == null)
            {
                continue;
            }

            foreach (string idB in clusterB)
            {
                JumpLaneSystem? systemB = GetSystem(result, idB);
                if (systemB == null)
                {
                    continue;
                }

                double distance = systemA.DistanceTo(systemB);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSystemA = idA;
                    bestSystemB = idB;
                }
            }
        }

        return new Dictionary
        {
            ["system_a"] = bestSystemA,
            ["system_b"] = bestSystemB,
            ["distance"] = bestDistance,
        };
    }

    /// <summary>
    /// Creates a normal-range connection between two clusters.
    /// </summary>
    private void CreateClusterConnection(Dictionary pairInfo, JumpLaneRegion region, JumpLaneResult result)
    {
        JumpLaneSystem? systemA = GetSystem(result, GetString(pairInfo, "system_a", string.Empty));
        JumpLaneSystem? systemB = GetSystem(result, GetString(pairInfo, "system_b", string.Empty));
        if (systemA == null || systemB == null)
        {
            return;
        }

        JumpLaneSystem source = systemA;
        JumpLaneSystem destination = systemB;
        if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
        {
            source = systemB;
            destination = systemA;
        }

        double distance = source.DistanceTo(destination);
        if (distance <= ThresholdGreen)
        {
            AddGreenConnection(source, destination, distance, result);
        }
        else if (distance <= ThresholdOrange)
        {
            JumpLaneSystem? bridge = FindBridge(source, destination, region);
            if (bridge != null)
            {
                AddBridgedConnection(source, destination, bridge, result);
            }
            else
            {
                AddOrangeConnection(source, destination, distance, result);
            }
        }
        else
        {
            JumpLaneSystem? bridge = FindBridge(source, destination, region);
            if (bridge != null)
            {
                AddBridgedConnection(source, destination, bridge, result);
            }
        }
    }

    /// <summary>
    /// Finds a valid bridge between two systems.
    /// </summary>
    private JumpLaneSystem? FindBridge(
        JumpLaneSystem source,
        JumpLaneSystem destination,
        JumpLaneRegion region)
    {
        JumpLaneSystem? bestBridge = null;
        double bestTotalDistance = double.PositiveInfinity;
        foreach (JumpLaneSystem system in region.Systems)
        {
            if (system.Id == source.Id || system.Id == destination.Id)
            {
                continue;
            }

            if (system.IsPopulated() && !system.IsBridge)
            {
                continue;
            }

            double distanceToSource = system.DistanceTo(source);
            double distanceToDestination = system.DistanceTo(destination);
            if (distanceToSource <= BridgeMaxDistance && distanceToDestination <= BridgeMaxDistance)
            {
                double totalDistance = distanceToSource + distanceToDestination;
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
    /// Adds a green connection between two systems.
    /// </summary>
    private void AddGreenConnection(
        JumpLaneSystem source,
        JumpLaneSystem destination,
        double distance,
        JumpLaneResult result)
    {
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            destination.Id,
            JumpLaneConnection.ConnectionType.Green,
            distance));
        RemoveFromOrphans(source.Id, result);
        RemoveFromOrphans(destination.Id, result);
    }

    /// <summary>
    /// Adds an orange connection between two systems.
    /// </summary>
    private void AddOrangeConnection(
        JumpLaneSystem source,
        JumpLaneSystem destination,
        double distance,
        JumpLaneResult result)
    {
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            destination.Id,
            JumpLaneConnection.ConnectionType.Orange,
            distance));
        RemoveFromOrphans(source.Id, result);
        RemoveFromOrphans(destination.Id, result);
    }

    /// <summary>
    /// Adds a bridged yellow connection via an intermediate system.
    /// </summary>
    private void AddBridgedConnection(
        JumpLaneSystem source,
        JumpLaneSystem destination,
        JumpLaneSystem bridge,
        JumpLaneResult result)
    {
        bridge.MakeBridge(destination.GetEffectivePopulation());
        result.AddConnection(new JumpLaneConnection(
            source.Id,
            bridge.Id,
            JumpLaneConnection.ConnectionType.Yellow,
            source.DistanceTo(bridge)));
        result.AddConnection(new JumpLaneConnection(
            bridge.Id,
            destination.Id,
            JumpLaneConnection.ConnectionType.Yellow,
            bridge.DistanceTo(destination)));
        RemoveFromOrphans(source.Id, result);
        RemoveFromOrphans(destination.Id, result);
        RemoveFromOrphans(bridge.Id, result);
    }

    /// <summary>
    /// Removes a system identifier from the orphan list when present.
    /// </summary>
    private void RemoveFromOrphans(string systemId, JumpLaneResult result)
    {
        int index = result.OrphanIds.IndexOf(systemId);
        if (index >= 0)
        {
            result.OrphanIds.RemoveAt(index);
        }
    }

    /// <summary>
    /// Gets a registered system by identifier from the result.
    /// </summary>
    private static JumpLaneSystem? GetSystem(JumpLaneResult result, string systemId)
    {
        if (!result.Systems.ContainsKey(systemId))
        {
            return null;
        }

        Variant value = result.Systems[systemId];
        return value.Obj as JumpLaneSystem;
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
