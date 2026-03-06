using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Background jump-route graph calculation helpers for GalaxyViewer.
/// </summary>
public partial class GalaxyViewer
{
	private sealed class JumpRouteWorkingSystem
	{
		public string Id;
		public Vector3 Position;
		public int Population;
		public int FalsePopulation;
		public bool IsBridge;

		public JumpRouteWorkingSystem(string id, Vector3 position, int population)
		{
			Id = id;
			Position = position;
			Population = population;
			FalsePopulation = -1;
			IsBridge = false;
		}

		public JumpRouteWorkingSystem Clone()
		{
			JumpRouteWorkingSystem clone = new(Id, Position, Population);
			clone.FalsePopulation = FalsePopulation;
			clone.IsBridge = IsBridge;
			return clone;
		}

		public int GetEffectivePopulation()
		{
			if (FalsePopulation >= 0)
			{
				return FalsePopulation;
			}

			return Population;
		}

		public bool IsPopulated()
		{
			return Population > 0;
		}

		public double DistanceTo(JumpRouteWorkingSystem other)
		{
			return Position.DistanceTo(other.Position);
		}

		public void MakeBridge(int higherPopulation)
		{
			IsBridge = true;
			FalsePopulation = higherPopulation - 10000;
			if (FalsePopulation < 0)
			{
				FalsePopulation = 0;
			}
		}
	}

	private readonly struct JumpRouteConnectionData
	{
		public string SourceId { get; init; }
		public string DestinationId { get; init; }
		public JumpLaneConnection.ConnectionType ConnectionType { get; init; }
		public double DistancePc { get; init; }
	}

	private readonly struct JumpRouteClusterPair
	{
		public string SystemA { get; init; }
		public string SystemB { get; init; }
		public double Distance { get; init; }

		public bool IsEmpty
		{
			get
			{
				return string.IsNullOrEmpty(SystemA);
			}
		}
	}

	private sealed class JumpRouteBackgroundResult
	{
		public List<JumpRouteWorkingSystem> Systems { get; }
		public List<JumpRouteConnectionData> Connections { get; }
		public HashSet<string> OrphanIds { get; }

		public JumpRouteBackgroundResult(
			List<JumpRouteWorkingSystem> systems,
			List<JumpRouteConnectionData> connections,
			HashSet<string> orphanIds)
		{
			Systems = systems;
			Connections = connections;
			OrphanIds = orphanIds;
		}
	}

	private Task<JumpRouteBackgroundResult> CalculateJumpRouteGraphAsync(JumpLaneRegion region)
	{
		List<JumpRouteWorkingSystem> systems = CreateWorkingSystems(region);
		return Task.Run(() => CalculateJumpRouteGraph(systems));
	}

	private JumpRouteBackgroundResult CalculateJumpRouteGraphSynchronously(JumpLaneRegion region)
	{
		List<JumpRouteWorkingSystem> systems = CreateWorkingSystems(region);
		return CalculateJumpRouteGraph(systems);
	}

	private static List<JumpRouteWorkingSystem> CreateWorkingSystems(JumpLaneRegion region)
	{
		List<JumpRouteWorkingSystem> systems = new(region.Systems.Count);
		foreach (JumpLaneSystem system in region.Systems)
		{
			JumpRouteWorkingSystem workingSystem = new(system.Id, system.Position, system.Population);
			workingSystem.FalsePopulation = system.FalsePopulation;
			workingSystem.IsBridge = system.IsBridge;
			systems.Add(workingSystem);
		}

		return systems;
	}

	private static JumpRouteBackgroundResult CalculateJumpRouteGraph(List<JumpRouteWorkingSystem> sourceSystems)
	{
		List<JumpRouteWorkingSystem> systems = new(sourceSystems.Count);
		Dictionary<string, JumpRouteWorkingSystem> systemById = new(sourceSystems.Count, StringComparer.Ordinal);
		foreach (JumpRouteWorkingSystem sourceSystem in sourceSystems)
		{
			JumpRouteWorkingSystem clone = sourceSystem.Clone();
			systems.Add(clone);
			systemById[clone.Id] = clone;
		}

		List<JumpRouteConnectionData> connections = new();
		HashSet<string> connectedSystems = new(StringComparer.Ordinal);
		HashSet<string> bridgeIds = new(StringComparer.Ordinal);

		List<JumpRouteWorkingSystem> sortedSystems = GetSystemsSortedByPopulation(systems);
		foreach (JumpRouteWorkingSystem system in sortedSystems)
		{
			if (bridgeIds.Contains(system.Id))
			{
				continue;
			}

			bool connectionMade = TryConnectSystem(
				system,
				systems,
				connections,
				connectedSystems,
				bridgeIds);
			if (connectionMade)
			{
				connectedSystems.Add(system.Id);
			}
		}

		HashSet<string> orphanIds = new(StringComparer.Ordinal);
		foreach (JumpRouteWorkingSystem system in sortedSystems)
		{
			if (!connectedSystems.Contains(system.Id))
			{
				orphanIds.Add(system.Id);
			}
		}

		ConnectClusters(systems, systemById, connections, orphanIds);
		return new JumpRouteBackgroundResult(systems, connections, orphanIds);
	}

	private static List<JumpRouteWorkingSystem> GetSystemsSortedByPopulation(List<JumpRouteWorkingSystem> systems)
	{
		List<JumpRouteWorkingSystem> sortedSystems = new();
		foreach (JumpRouteWorkingSystem system in systems)
		{
			if (system.IsPopulated())
			{
				sortedSystems.Add(system);
			}
		}

		sortedSystems.Sort(CompareSystemsByPopulation);
		return sortedSystems;
	}

	private static int CompareSystemsByPopulation(JumpRouteWorkingSystem left, JumpRouteWorkingSystem right)
	{
		return left.GetEffectivePopulation().CompareTo(right.GetEffectivePopulation());
	}

	private static bool TryConnectSystem(
		JumpRouteWorkingSystem system,
		List<JumpRouteWorkingSystem> regionSystems,
		List<JumpRouteConnectionData> connections,
		HashSet<string> connectedSystems,
		HashSet<string> bridgeIds)
	{
		List<JumpRouteWorkingSystem> candidates = GetHigherPopulatedSystems(system, regionSystems);
		if (candidates.Count == 0)
		{
			return false;
		}

		if (TryThreshold(system, candidates, JumpLaneCalculator.ThresholdDirectShort, connections, connectedSystems))
		{
			return true;
		}

		if (TryThreshold(system, candidates, JumpLaneCalculator.ThresholdDirectMedium, connections, connectedSystems))
		{
			return true;
		}

		if (TryExtendedThreshold(system, candidates, regionSystems, connections, connectedSystems, bridgeIds))
		{
			return true;
		}

		return false;
	}

	private static List<JumpRouteWorkingSystem> GetHigherPopulatedSystems(
		JumpRouteWorkingSystem source,
		List<JumpRouteWorkingSystem> regionSystems)
	{
		List<JumpRouteWorkingSystem> candidates = new();
		int sourcePopulation = source.GetEffectivePopulation();
		foreach (JumpRouteWorkingSystem candidate in regionSystems)
		{
			if (candidate.Id == source.Id)
			{
				continue;
			}

			if (!candidate.IsPopulated() && candidate.FalsePopulation < 0)
			{
				continue;
			}

			if (candidate.GetEffectivePopulation() > sourcePopulation)
			{
				candidates.Add(candidate);
			}
		}

		return candidates;
	}

	private static bool TryThreshold(
		JumpRouteWorkingSystem source,
		List<JumpRouteWorkingSystem> candidates,
		double threshold,
		List<JumpRouteConnectionData> connections,
		HashSet<string> connectedSystems)
	{
		JumpRouteWorkingSystem? target = FindHighestPopulatedWithin(source, candidates, threshold);
		if (target == null)
		{
			return false;
		}

		AddDirectConnection(source, target, JumpLaneConnection.ConnectionType.Green, connections, connectedSystems);
		return true;
	}

	private static bool TryExtendedThreshold(
		JumpRouteWorkingSystem source,
		List<JumpRouteWorkingSystem> candidates,
		List<JumpRouteWorkingSystem> regionSystems,
		List<JumpRouteConnectionData> connections,
		HashSet<string> connectedSystems,
		HashSet<string> bridgeIds)
	{
		JumpRouteWorkingSystem? target = FindHighestPopulatedWithin(source, candidates, JumpLaneCalculator.ThresholdBridgeMax);
		if (target != null)
		{
			JumpRouteWorkingSystem? bridge = FindUnpopulatedBridge(source, target, regionSystems);
			if (bridge != null)
			{
				AddBridgedConnection(source, target, bridge, connections, connectedSystems, bridgeIds);
			}
			else
			{
				AddDirectConnection(source, target, JumpLaneConnection.ConnectionType.Orange, connections, connectedSystems);
			}

			return true;
		}

		target = FindHighestPopulatedWithin(source, candidates, JumpLaneCalculator.ThresholdBridgeOnly);
		if (target == null)
		{
			return false;
		}

		double distance = source.DistanceTo(target);
		JumpRouteWorkingSystem? extendedBridge = FindUnpopulatedBridge(source, target, regionSystems);
		if (extendedBridge != null)
		{
			AddBridgedConnection(source, target, extendedBridge, connections, connectedSystems, bridgeIds);
			return true;
		}

		if (distance <= JumpLaneCalculator.ThresholdBridgeMax)
		{
			AddDirectConnection(source, target, JumpLaneConnection.ConnectionType.Orange, connections, connectedSystems);
			return true;
		}

		return false;
	}

	private static JumpRouteWorkingSystem? FindHighestPopulatedWithin(
		JumpRouteWorkingSystem source,
		List<JumpRouteWorkingSystem> candidates,
		double maxDistance)
	{
		JumpRouteWorkingSystem? best = null;
		int bestPopulation = -1;
		foreach (JumpRouteWorkingSystem candidate in candidates)
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

	private static JumpRouteWorkingSystem? FindUnpopulatedBridge(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem target,
		List<JumpRouteWorkingSystem> regionSystems)
	{
		JumpRouteWorkingSystem? bestBridge = null;
		double bestTotalDistance = double.PositiveInfinity;
		foreach (JumpRouteWorkingSystem candidate in regionSystems)
		{
			if (candidate.Id == source.Id || candidate.Id == target.Id)
			{
				continue;
			}

			if (candidate.IsPopulated())
			{
				continue;
			}

			double distanceToSource = candidate.DistanceTo(source);
			double distanceToTarget = candidate.DistanceTo(target);
			if (distanceToSource <= JumpLaneCalculator.BridgeMaxDistance
				&& distanceToTarget <= JumpLaneCalculator.BridgeMaxDistance)
			{
				double totalDistance = distanceToSource + distanceToTarget;
				if (totalDistance < bestTotalDistance)
				{
					bestBridge = candidate;
					bestTotalDistance = totalDistance;
				}
			}
		}

		return bestBridge;
	}

	private static void AddDirectConnection(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem target,
		JumpLaneConnection.ConnectionType connectionType,
		List<JumpRouteConnectionData> connections,
		HashSet<string> connectedSystems)
	{
		JumpRouteConnectionData connection = new()
		{
			SourceId = source.Id,
			DestinationId = target.Id,
			ConnectionType = connectionType,
			DistancePc = source.DistanceTo(target),
		};
		connections.Add(connection);
		connectedSystems.Add(source.Id);
		connectedSystems.Add(target.Id);
	}

	private static void AddBridgedConnection(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem target,
		JumpRouteWorkingSystem bridge,
		List<JumpRouteConnectionData> connections,
		HashSet<string> connectedSystems,
		HashSet<string> bridgeIds)
	{
		bridge.MakeBridge(target.GetEffectivePopulation());
		bridgeIds.Add(bridge.Id);

		JumpRouteConnectionData firstConnection = new()
		{
			SourceId = source.Id,
			DestinationId = bridge.Id,
			ConnectionType = JumpLaneConnection.ConnectionType.Yellow,
			DistancePc = source.DistanceTo(bridge),
		};
		JumpRouteConnectionData secondConnection = new()
		{
			SourceId = bridge.Id,
			DestinationId = target.Id,
			ConnectionType = JumpLaneConnection.ConnectionType.Yellow,
			DistancePc = bridge.DistanceTo(target),
		};

		connections.Add(firstConnection);
		connections.Add(secondConnection);
		connectedSystems.Add(source.Id);
		connectedSystems.Add(target.Id);
		connectedSystems.Add(bridge.Id);
	}

	private static void ConnectClusters(
		List<JumpRouteWorkingSystem> regionSystems,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		int iterations = 0;
		const int MaxIterations = 100;
		while (iterations < MaxIterations)
		{
			bool connected = TryConnectOneClusterPair(regionSystems, systemById, connections, orphanIds);
			if (!connected)
			{
				break;
			}

			iterations += 1;
		}

		TryExtendedConnections(regionSystems, systemById, connections, orphanIds);
	}

	private static bool TryConnectOneClusterPair(
		List<JumpRouteWorkingSystem> regionSystems,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		List<List<string>> clusters = IdentifyClusters(systemById, connections);
		if (clusters.Count < 2)
		{
			return false;
		}

		JumpRouteClusterPair bestPair = FindClosestClusterPair(clusters, systemById);
		if (bestPair.IsEmpty)
		{
			return false;
		}

		CreateClusterConnection(bestPair, regionSystems, systemById, connections, orphanIds);
		return true;
	}

	private static void TryExtendedConnections(
		List<JumpRouteWorkingSystem> regionSystems,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		Dictionary<string, List<string>> extendedGraph = BuildExtendedGraph(systemById, JumpLaneClusterConnector.MaxExtendedDistance);
		int iterations = 0;
		const int MaxIterations = 100;
		while (iterations < MaxIterations)
		{
			List<List<string>> clusters = IdentifyClusters(systemById, connections);
			if (clusters.Count < 2)
			{
				break;
			}

			JumpRouteClusterPair directPair = FindClosestClusterPairWithin(
				clusters,
				systemById,
				JumpLaneClusterConnector.MaxExtendedDistance);
			if (!directPair.IsEmpty)
			{
				CreateDirectRedConnection(directPair, systemById, connections, orphanIds);
				iterations += 1;
				continue;
			}

			List<string>? path = FindMultiHopPath(clusters, extendedGraph);
			if (path == null)
			{
				break;
			}

			CreateMultiHopRedConnections(path, systemById, connections, orphanIds);
			iterations += 1;
		}
	}

	private static List<List<string>> IdentifyClusters(
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections)
	{
		Dictionary<string, List<string>> adjacency = new(StringComparer.Ordinal);
		foreach (KeyValuePair<string, JumpRouteWorkingSystem> pair in systemById)
		{
			JumpRouteWorkingSystem system = pair.Value;
			if (system.IsPopulated() || system.IsBridge)
			{
				adjacency[pair.Key] = new List<string>();
			}
		}

		foreach (JumpRouteConnectionData connection in connections)
		{
			if (adjacency.ContainsKey(connection.SourceId) && adjacency.ContainsKey(connection.DestinationId))
			{
				adjacency[connection.SourceId].Add(connection.DestinationId);
				adjacency[connection.DestinationId].Add(connection.SourceId);
			}
		}

		HashSet<string> visited = new(StringComparer.Ordinal);
		List<List<string>> clusters = new();
		foreach (string systemId in adjacency.Keys)
		{
			if (visited.Contains(systemId))
			{
				continue;
			}

			List<string> cluster = new();
			FloodFillCluster(systemId, adjacency, visited, cluster);
			if (cluster.Count > 0)
			{
				clusters.Add(cluster);
			}
		}

		return clusters;
	}

	private static void FloodFillCluster(
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

	private static JumpRouteClusterPair FindClosestClusterPair(
		List<List<string>> clusters,
		Dictionary<string, JumpRouteWorkingSystem> systemById)
	{
		double bestDistance = double.PositiveInfinity;
		JumpRouteClusterPair bestPair = default;
		for (int indexA = 0; indexA < clusters.Count; indexA += 1)
		{
			for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
			{
				JumpRouteClusterPair pair = FindClosestSystemsBetweenClusters(clusters[indexA], clusters[indexB], systemById);
				if (pair.Distance < bestDistance && pair.Distance <= JumpLaneClusterConnector.MaxClusterDistance)
				{
					bestDistance = pair.Distance;
					bestPair = pair;
				}
			}
		}

		return bestPair;
	}

	private static JumpRouteClusterPair FindClosestClusterPairWithin(
		List<List<string>> clusters,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		double maxDistance)
	{
		double bestDistance = double.PositiveInfinity;
		JumpRouteClusterPair bestPair = default;
		for (int indexA = 0; indexA < clusters.Count; indexA += 1)
		{
			for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
			{
				JumpRouteClusterPair pair = FindClosestSystemsBetweenClusters(clusters[indexA], clusters[indexB], systemById);
				if (pair.Distance < bestDistance && pair.Distance <= maxDistance)
				{
					bestDistance = pair.Distance;
					bestPair = pair;
				}
			}
		}

		return bestPair;
	}

	private static JumpRouteClusterPair FindClosestSystemsBetweenClusters(
		List<string> clusterA,
		List<string> clusterB,
		Dictionary<string, JumpRouteWorkingSystem> systemById)
	{
		double bestDistance = double.PositiveInfinity;
		string bestSystemA = string.Empty;
		string bestSystemB = string.Empty;
		foreach (string idA in clusterA)
		{
			if (!systemById.TryGetValue(idA, out JumpRouteWorkingSystem? systemA))
			{
				continue;
			}

			foreach (string idB in clusterB)
			{
				if (!systemById.TryGetValue(idB, out JumpRouteWorkingSystem? systemB))
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

		JumpRouteClusterPair pair = new()
		{
			SystemA = bestSystemA,
			SystemB = bestSystemB,
			Distance = bestDistance,
		};
		return pair;
	}

	private static void CreateClusterConnection(
		JumpRouteClusterPair pair,
		List<JumpRouteWorkingSystem> regionSystems,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		if (!systemById.TryGetValue(pair.SystemA, out JumpRouteWorkingSystem? systemA))
		{
			return;
		}

		if (!systemById.TryGetValue(pair.SystemB, out JumpRouteWorkingSystem? systemB))
		{
			return;
		}

		JumpRouteWorkingSystem source = systemA;
		JumpRouteWorkingSystem destination = systemB;
		if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
		{
			source = systemB;
			destination = systemA;
		}

		double distance = source.DistanceTo(destination);
		if (distance <= JumpLaneClusterConnector.ThresholdGreen)
		{
			AddClusterConnection(source, destination, JumpLaneConnection.ConnectionType.Green, connections, orphanIds);
			return;
		}

		if (distance <= JumpLaneClusterConnector.ThresholdOrange)
		{
			JumpRouteWorkingSystem? bridge = FindClusterBridge(source, destination, regionSystems);
			if (bridge != null)
			{
				AddClusterBridgedConnection(source, destination, bridge, connections, orphanIds);
			}
			else
			{
				AddClusterConnection(source, destination, JumpLaneConnection.ConnectionType.Orange, connections, orphanIds);
			}

			return;
		}

		JumpRouteWorkingSystem? extendedBridge = FindClusterBridge(source, destination, regionSystems);
		if (extendedBridge != null)
		{
			AddClusterBridgedConnection(source, destination, extendedBridge, connections, orphanIds);
		}
	}

	private static JumpRouteWorkingSystem? FindClusterBridge(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem destination,
		List<JumpRouteWorkingSystem> regionSystems)
	{
		JumpRouteWorkingSystem? bestBridge = null;
		double bestTotalDistance = double.PositiveInfinity;
		foreach (JumpRouteWorkingSystem candidate in regionSystems)
		{
			if (candidate.Id == source.Id || candidate.Id == destination.Id)
			{
				continue;
			}

			if (candidate.IsPopulated() && !candidate.IsBridge)
			{
				continue;
			}

			double distanceToSource = candidate.DistanceTo(source);
			double distanceToDestination = candidate.DistanceTo(destination);
			if (distanceToSource <= JumpLaneClusterConnector.BridgeMaxDistance
				&& distanceToDestination <= JumpLaneClusterConnector.BridgeMaxDistance)
			{
				double totalDistance = distanceToSource + distanceToDestination;
				if (totalDistance < bestTotalDistance)
				{
					bestBridge = candidate;
					bestTotalDistance = totalDistance;
				}
			}
		}

		return bestBridge;
	}

	private static void AddClusterConnection(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem destination,
		JumpLaneConnection.ConnectionType connectionType,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		JumpRouteConnectionData connection = new()
		{
			SourceId = source.Id,
			DestinationId = destination.Id,
			ConnectionType = connectionType,
			DistancePc = source.DistanceTo(destination),
		};
		connections.Add(connection);
		orphanIds.Remove(source.Id);
		orphanIds.Remove(destination.Id);
	}

	private static void AddClusterBridgedConnection(
		JumpRouteWorkingSystem source,
		JumpRouteWorkingSystem destination,
		JumpRouteWorkingSystem bridge,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		bridge.MakeBridge(destination.GetEffectivePopulation());

		JumpRouteConnectionData firstConnection = new()
		{
			SourceId = source.Id,
			DestinationId = bridge.Id,
			ConnectionType = JumpLaneConnection.ConnectionType.Yellow,
			DistancePc = source.DistanceTo(bridge),
		};
		JumpRouteConnectionData secondConnection = new()
		{
			SourceId = bridge.Id,
			DestinationId = destination.Id,
			ConnectionType = JumpLaneConnection.ConnectionType.Yellow,
			DistancePc = bridge.DistanceTo(destination),
		};

		connections.Add(firstConnection);
		connections.Add(secondConnection);
		orphanIds.Remove(source.Id);
		orphanIds.Remove(destination.Id);
		orphanIds.Remove(bridge.Id);
	}

	private static Dictionary<string, List<string>> BuildExtendedGraph(
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		double maxDistance)
	{
		Dictionary<string, List<string>> graph = new(systemById.Count, StringComparer.Ordinal);
		List<string> systemIds = new(systemById.Keys);
		foreach (string sourceId in systemIds)
		{
			List<string> neighbors = new();
			graph[sourceId] = neighbors;
			JumpRouteWorkingSystem systemA = systemById[sourceId];

			foreach (string destinationId in systemIds)
			{
				if (sourceId == destinationId)
				{
					continue;
				}

				JumpRouteWorkingSystem systemB = systemById[destinationId];
				if (systemA.DistanceTo(systemB) <= maxDistance)
				{
					neighbors.Add(destinationId);
				}
			}
		}

		return graph;
	}

	private static List<string>? FindMultiHopPath(
		List<List<string>> clusters,
		Dictionary<string, List<string>> graph)
	{
		for (int indexA = 0; indexA < clusters.Count; indexA += 1)
		{
			for (int indexB = indexA + 1; indexB < clusters.Count; indexB += 1)
			{
				List<string>? path = FindBfsPath(clusters[indexA], clusters[indexB], graph);
				if (path != null)
				{
					return path;
				}
			}
		}

		return null;
	}

	private static List<string>? FindBfsPath(
		List<string> clusterA,
		List<string> clusterB,
		Dictionary<string, List<string>> graph)
	{
		HashSet<string> clusterBSet = new(clusterB, StringComparer.Ordinal);
		Dictionary<string, string> parent = new(StringComparer.Ordinal);
		Queue<string> queue = new();

		foreach (string systemId in clusterA)
		{
			queue.Enqueue(systemId);
			parent[systemId] = string.Empty;
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

	private static List<string> ReconstructPath(string endSystemId, Dictionary<string, string> parent)
	{
		List<string> path = new();
		string current = endSystemId;
		while (!string.IsNullOrEmpty(current))
		{
			path.Insert(0, current);
			if (parent.TryGetValue(current, out string? previous))
			{
				current = previous;
			}
			else
			{
				break;
			}
		}

		return path;
	}

	private static void CreateDirectRedConnection(
		JumpRouteClusterPair pair,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		if (!systemById.TryGetValue(pair.SystemA, out JumpRouteWorkingSystem? systemA))
		{
			return;
		}

		if (!systemById.TryGetValue(pair.SystemB, out JumpRouteWorkingSystem? systemB))
		{
			return;
		}

		JumpRouteWorkingSystem source = systemA;
		JumpRouteWorkingSystem destination = systemB;
		if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
		{
			source = systemB;
			destination = systemA;
		}

		JumpRouteConnectionData connection = new()
		{
			SourceId = source.Id,
			DestinationId = destination.Id,
			ConnectionType = JumpLaneConnection.ConnectionType.Red,
			DistancePc = source.DistanceTo(destination),
		};
		connections.Add(connection);
		orphanIds.Remove(source.Id);
		orphanIds.Remove(destination.Id);
	}

	private static void CreateMultiHopRedConnections(
		List<string> path,
		Dictionary<string, JumpRouteWorkingSystem> systemById,
		List<JumpRouteConnectionData> connections,
		HashSet<string> orphanIds)
	{
		if (path.Count < 2)
		{
			return;
		}

		HashSet<(string, string)> existingConnections = BuildConnectionSet(connections);
		for (int index = 0; index < path.Count - 1; index += 1)
		{
			string sourceId = path[index];
			string destinationId = path[index + 1];
			if (existingConnections.Contains((sourceId, destinationId))
				|| existingConnections.Contains((destinationId, sourceId)))
			{
				continue;
			}

			if (!systemById.TryGetValue(sourceId, out JumpRouteWorkingSystem? systemA))
			{
				continue;
			}

			if (!systemById.TryGetValue(destinationId, out JumpRouteWorkingSystem? systemB))
			{
				continue;
			}

			JumpRouteWorkingSystem source = systemA;
			JumpRouteWorkingSystem destination = systemB;
			if (systemB.GetEffectivePopulation() < systemA.GetEffectivePopulation())
			{
				source = systemB;
				destination = systemA;
			}

			JumpRouteConnectionData connection = new()
			{
				SourceId = source.Id,
				DestinationId = destination.Id,
				ConnectionType = JumpLaneConnection.ConnectionType.Red,
				DistancePc = source.DistanceTo(destination),
			};
			connections.Add(connection);
			existingConnections.Add((connection.SourceId, connection.DestinationId));

			if (index > 0 && systemById.TryGetValue(sourceId, out JumpRouteWorkingSystem? bridgeSystem))
			{
				bridgeSystem.IsBridge = true;
				int higherPopulation = destination.GetEffectivePopulation();
				if (bridgeSystem.GetEffectivePopulation() < higherPopulation)
				{
					bridgeSystem.MakeBridge(higherPopulation);
				}
			}
		}

		orphanIds.Remove(path[0]);
		orphanIds.Remove(path[path.Count - 1]);
		for (int index = 1; index < path.Count - 1; index += 1)
		{
			orphanIds.Remove(path[index]);
		}
	}

	private static HashSet<(string, string)> BuildConnectionSet(List<JumpRouteConnectionData> connections)
	{
		HashSet<(string, string)> set = new(connections.Count);
		foreach (JumpRouteConnectionData connection in connections)
		{
			set.Add((connection.SourceId, connection.DestinationId));
		}

		return set;
	}

	private JumpLaneRegion BuildJumpLaneRegionFromBackground(
		JumpLaneRegion originalRegion,
		JumpRouteBackgroundResult backgroundResult)
	{
		JumpLaneRegion region = new(originalRegion.Scope, originalRegion.RegionId);
		foreach (JumpRouteWorkingSystem workingSystem in backgroundResult.Systems)
		{
			JumpLaneSystem system = CreateJumpLaneSystemFromWorking(workingSystem);
			region.AddSystem(system);
		}

		return region;
	}

	private static JumpLaneRegion CloneJumpLaneRegion(JumpLaneRegion sourceRegion)
	{
		JumpLaneRegion clone = new(sourceRegion.Scope, sourceRegion.RegionId);
		foreach (JumpLaneSystem system in sourceRegion.Systems)
		{
			JumpLaneSystem copy = new(system.Id, system.Position, system.Population);
			copy.FalsePopulation = system.FalsePopulation;
			copy.IsBridge = system.IsBridge;
			clone.AddSystem(copy);
		}

		return clone;
	}

	private static int MergeJumpLaneRegion(JumpLaneRegion targetRegion, JumpLaneRegion incomingRegion)
	{
		HashSet<string> existingIds = new(StringComparer.Ordinal);
		foreach (JumpLaneSystem existingSystem in targetRegion.Systems)
		{
			existingIds.Add(existingSystem.Id);
		}

		int addedCount = 0;
		foreach (JumpLaneSystem incomingSystem in incomingRegion.Systems)
		{
			if (existingIds.Contains(incomingSystem.Id))
			{
				continue;
			}

			JumpLaneSystem copy = new(incomingSystem.Id, incomingSystem.Position, incomingSystem.Population);
			copy.FalsePopulation = incomingSystem.FalsePopulation;
			copy.IsBridge = incomingSystem.IsBridge;
			targetRegion.AddSystem(copy);
			existingIds.Add(copy.Id);
			addedCount += 1;
		}

		return addedCount;
	}

	private JumpLaneResult BuildJumpLaneResultFromBackground(JumpRouteBackgroundResult backgroundResult)
	{
		JumpLaneResult result = new();
		foreach (JumpRouteWorkingSystem workingSystem in backgroundResult.Systems)
		{
			JumpLaneSystem system = CreateJumpLaneSystemFromWorking(workingSystem);
			result.RegisterSystem(system);
		}

		foreach (JumpRouteConnectionData connectionData in backgroundResult.Connections)
		{
			JumpLaneConnection connection = new(
				connectionData.SourceId,
				connectionData.DestinationId,
				connectionData.ConnectionType,
				connectionData.DistancePc);
			result.AddConnection(connection);
		}

		foreach (string orphanId in backgroundResult.OrphanIds)
		{
			result.AddOrphan(orphanId);
		}

		return result;
	}

	private static JumpLaneSystem CreateJumpLaneSystemFromWorking(JumpRouteWorkingSystem workingSystem)
	{
		JumpLaneSystem system = new(workingSystem.Id, workingSystem.Position, workingSystem.Population);
		system.FalsePopulation = workingSystem.FalsePopulation;
		system.IsBridge = workingSystem.IsBridge;
		return system;
	}
}
