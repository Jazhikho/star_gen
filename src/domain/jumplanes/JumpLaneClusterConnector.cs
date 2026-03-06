using System.Collections.Generic;
using Godot;

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
    /// Represents the closest pair of systems found between two clusters.
    /// </summary>
    private readonly struct ClusterPair
    {
        /// <summary>Identifier of the system in the first cluster.</summary>
        public string SystemA { get; init; }

        /// <summary>Identifier of the system in the second cluster.</summary>
        public string SystemB { get; init; }

        /// <summary>Distance between the two systems in parsecs.</summary>
        public double Distance { get; init; }

        /// <summary>Returns true when no valid pair was found.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(SystemA);
    }

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
        List<List<string>> clusters = IdentifyClusters(result);
        if (clusters.Count < 2)
        {
            return false;
        }

        ClusterPair best = FindClosestClusterPair(clusters, result);
        if (best.IsEmpty)
        {
            return false;
        }

        CreateClusterConnection(best, region, result);
        return true;
    }

    /// <summary>
    /// Tries extended red links between remaining clusters.
    /// The extended-proximity graph is built once before the loop because spatial
    /// distances between systems are invariant; only the cluster membership changes.
    /// </summary>
    private void TryExtendedConnections(JumpLaneRegion region, JumpLaneResult result)
    {
        Dictionary<string, List<string>> extendedGraph = BuildExtendedGraph(result, MaxExtendedDistance);

        int iterations = 0;
        const int maxIterations = 100;
        while (iterations < maxIterations)
        {
            List<List<string>> clusters = IdentifyClusters(result);
            if (clusters.Count < 2)
            {
                break;
            }

            ClusterPair directPair = FindClosestClusterPairWithin(clusters, region, result, MaxExtendedDistance);
            if (!directPair.IsEmpty)
            {
                CreateDirectRedConnection(directPair, result);
                iterations += 1;
                continue;
            }

            List<string>? path = FindMultiHopPath(clusters, extendedGraph);
            if (path == null)
            {
                break;
            }

            CreateMultiHopRedConnections(path, result);
            iterations += 1;
        }
    }

    /// <summary>
    /// Finds the closest pair of clusters within a distance ceiling.
    /// </summary>
    private ClusterPair FindClosestClusterPairWithin(
        List<List<string>> clusters,
        JumpLaneRegion region,
        JumpLaneResult result,
        double maxDistance)
    {
        double bestDistance = double.PositiveInfinity;
        ClusterPair best = default;

        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                ClusterPair pair = FindClosestSystemsBetweenClusters(clusters[indexA], clusters[indexB], result);
                if (pair.Distance < bestDistance && pair.Distance <= maxDistance)
                {
                    bestDistance = pair.Distance;
                    best = pair;
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Creates a direct red connection between two clusters.
    /// </summary>
    private void CreateDirectRedConnection(ClusterPair pair, JumpLaneResult result)
    {
        JumpLaneSystem? systemA = result.GetSystem(pair.SystemA);
        JumpLaneSystem? systemB = result.GetSystem(pair.SystemB);
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
    /// Returns null when no path can be found.
    /// </summary>
    private List<string>? FindMultiHopPath(List<List<string>> clusters, Dictionary<string, List<string>> graph)
    {
        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                List<string>? path = BfsPath(clusters[indexA], clusters[indexB], graph);
                if (path != null)
                {
                    return path;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Builds adjacency for the extended-connection phase.
    /// Uses plain C# collections to avoid Godot boxing overhead.
    /// </summary>
    private Dictionary<string, List<string>> BuildExtendedGraph(JumpLaneResult result, double maxPc)
    {
        Dictionary<string, List<string>> graph = new();
        List<string> systemIds = new(result.Systems.Count);

        foreach (Variant idValue in result.Systems.Keys)
        {
            systemIds.Add(idValue.AsString());
        }

        foreach (string idA in systemIds)
        {
            List<string> neighbors = new();
            graph[idA] = neighbors;
            JumpLaneSystem? systemA = result.GetSystem(idA);
            if (systemA == null)
            {
                continue;
            }

            foreach (string idB in systemIds)
            {
                if (idA == idB)
                {
                    continue;
                }

                JumpLaneSystem? systemB = result.GetSystem(idB);
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
    /// Returns null when no path is found.
    /// </summary>
    private List<string>? BfsPath(
        List<string> clusterA,
        List<string> clusterB,
        Dictionary<string, List<string>> graph)
    {
        HashSet<string> clusterBSet = new(clusterB);
        Dictionary<string, string> parent = new();
        Queue<string> queue = new();

        foreach (string idA in clusterA)
        {
            queue.Enqueue(idA);
            parent[idA] = string.Empty;
        }

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            if (clusterBSet.Contains(current))
            {
                return ReconstructPath(current, parent);
            }

            if (!graph.TryGetValue(current, out List<string>? neighbors))
            {
                continue;
            }

            foreach (string neighbor in neighbors)
            {
                if (!parent.ContainsKey(neighbor))
                {
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Reconstructs a path from the BFS parent map.
    /// </summary>
    private List<string> ReconstructPath(string end, Dictionary<string, string> parent)
    {
        List<string> path = new();
        string node = end;
        while (!string.IsNullOrEmpty(node))
        {
            path.Insert(0, node);
            if (parent.TryGetValue(node, out string? prev))
            {
                node = prev;
            }
            else
            {
                break;
            }
        }

        return path;
    }

    /// <summary>
    /// Creates red connections along a multi-hop path.
    /// Builds a connection set once for O(1) duplicate checks instead of O(N) per hop.
    /// </summary>
    private void CreateMultiHopRedConnections(List<string> path, JumpLaneResult result)
    {
        if (path.Count < 2)
        {
            return;
        }

        HashSet<(string, string)> existingConnections = BuildConnectionSet(result);

        for (int index = 0; index < path.Count - 1; index += 1)
        {
            string idA = path[index];
            string idB = path[index + 1];
            if (existingConnections.Contains((idA, idB)) || existingConnections.Contains((idB, idA)))
            {
                continue;
            }

            JumpLaneSystem? systemA = result.GetSystem(idA);
            JumpLaneSystem? systemB = result.GetSystem(idB);
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

            JumpLaneSystem? fromSystem = result.GetSystem(idA);
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
    /// Builds a hash set of all existing connection pairs for O(1) duplicate detection.
    /// </summary>
    private HashSet<(string, string)> BuildConnectionSet(JumpLaneResult result)
    {
        HashSet<(string, string)> set = new(result.Connections.Count);
        foreach (JumpLaneConnection connection in result.Connections)
        {
            set.Add((connection.SourceId, connection.DestinationId));
        }

        return set;
    }

    /// <summary>
    /// Identifies connected clusters among populated systems and bridges.
    /// Uses C# generic collections to avoid Godot Variant boxing overhead.
    /// </summary>
    private List<List<string>> IdentifyClusters(JumpLaneResult result)
    {
        Dictionary<string, List<string>> adjacency = new();
        foreach (Variant systemIdValue in result.Systems.Keys)
        {
            string systemId = systemIdValue.AsString();
            JumpLaneSystem? system = result.GetSystem(systemId);
            if (system != null && (system.IsPopulated() || system.IsBridge))
            {
                adjacency[systemId] = new List<string>();
            }
        }

        foreach (JumpLaneConnection connection in result.Connections)
        {
            if (adjacency.ContainsKey(connection.SourceId) && adjacency.ContainsKey(connection.DestinationId))
            {
                adjacency[connection.SourceId].Add(connection.DestinationId);
                adjacency[connection.DestinationId].Add(connection.SourceId);
            }
        }

        HashSet<string> visited = new();
        List<List<string>> clusters = new();
        foreach (string systemId in adjacency.Keys)
        {
            if (visited.Contains(systemId))
            {
                continue;
            }

            List<string> cluster = new();
            FloodFill(systemId, adjacency, visited, cluster);
            if (cluster.Count > 0)
            {
                clusters.Add(cluster);
            }
        }

        return clusters;
    }

    /// <summary>
    /// Flood-fills a cluster from a starting system using an explicit stack.
    /// </summary>
    private void FloodFill(
        string startId,
        Dictionary<string, List<string>> adjacency,
        HashSet<string> visited,
        List<string> cluster)
    {
        Stack<string> stack = new();
        stack.Push(startId);
        while (stack.Count > 0)
        {
            string current = stack.Pop();
            if (visited.Contains(current))
            {
                continue;
            }

            visited.Add(current);
            cluster.Add(current);
            if (!adjacency.TryGetValue(current, out List<string>? neighbors))
            {
                continue;
            }

            foreach (string neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Finds the closest pair of clusters within normal connection range.
    /// </summary>
    private ClusterPair FindClosestClusterPair(List<List<string>> clusters, JumpLaneResult result)
    {
        double bestDistance = double.PositiveInfinity;
        ClusterPair best = default;
        for (int indexA = 0; indexA < clusters.Count; indexA += 1)
        {
            for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
            {
                ClusterPair pair = FindClosestSystemsBetweenClusters(clusters[indexA], clusters[indexB], result);
                if (pair.Distance < bestDistance && pair.Distance <= MaxClusterDistance)
                {
                    bestDistance = pair.Distance;
                    best = pair;
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Finds the closest systems between two clusters.
    /// </summary>
    private ClusterPair FindClosestSystemsBetweenClusters(
        List<string> clusterA,
        List<string> clusterB,
        JumpLaneResult result)
    {
        double bestDistance = double.PositiveInfinity;
        string bestSystemA = string.Empty;
        string bestSystemB = string.Empty;

        foreach (string idA in clusterA)
        {
            JumpLaneSystem? systemA = result.GetSystem(idA);
            if (systemA == null)
            {
                continue;
            }

            foreach (string idB in clusterB)
            {
                JumpLaneSystem? systemB = result.GetSystem(idB);
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

        return new ClusterPair { SystemA = bestSystemA, SystemB = bestSystemB, Distance = bestDistance };
    }

    /// <summary>
    /// Creates a normal-range connection between two clusters.
    /// </summary>
    private void CreateClusterConnection(ClusterPair pair, JumpLaneRegion region, JumpLaneResult result)
    {
        JumpLaneSystem? systemA = result.GetSystem(pair.SystemA);
        JumpLaneSystem? systemB = result.GetSystem(pair.SystemB);
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

}
