using System;
using System.Collections.Generic;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Population;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Deterministic subsector-level colonization simulator.
/// </summary>
public static class ColonizationSimulator
{
    private const int MinimumRoutePopulation = 5000;
    private const double MinimumExportPressure = 0.18;

    /// <summary>
    /// Runs colonization for an explicit region and returns persisted state.
    /// </summary>
    public static ColonizationSimulationState Run(ColonizationSimulationRequest request)
    {
        ColonizationSimulationState state = new ColonizationSimulationState
        {
            RegionId = request.RegionId,
            Scope = request.Scope,
            Settings = request.Settings.Clone(),
        };

        List<JumpLaneSystem> systems = CloneAndSortSystems(request.Systems);
        Dictionary<string, int> colonizationEpochs = BuildInitialEpochMap(systems, request.Settings);
        HashSet<string> connectedIds = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> claimedTargets = new HashSet<string>(StringComparer.Ordinal);

        for (int currentEpoch = 0; currentEpoch < request.Settings.MaxEpochs; currentEpoch += 1)
        {
            List<JumpLaneSystem> exporters = GetExportersForEpoch(systems, colonizationEpochs, currentEpoch, request.Settings);
            foreach (JumpLaneSystem exporter in exporters)
            {
                int launchBudget = DetermineLaunchBudget(exporter, request.Settings);
                int launchesCompleted = 0;
                int attemptIndex = 0;
                List<JumpLaneSystem> candidates = GetCandidateTargets(exporter, systems, claimedTargets);
                foreach (JumpLaneSystem target in candidates)
                {
                    if (launchesCompleted >= launchBudget)
                    {
                        break;
                    }

                    if (!TryColonize(exporter, target, currentEpoch, attemptIndex, request.Settings))
                    {
                        attemptIndex += 1;
                        continue;
                    }

                    ColonizationSettlementRecord settlement = ApplyColonization(exporter, target, currentEpoch, request.Settings);
                    ColonizationRouteRecord route = new ColonizationRouteRecord
                    {
                        SourceSystemId = settlement.SourceSystemId,
                        DestinationSystemId = settlement.DestinationSystemId,
                        ConnectionType = settlement.RouteConnectionType,
                        DistancePc = settlement.RouteDistancePc,
                    };

                    state.Settlements.Add(settlement);
                    state.Routes.Add(route);
                    claimedTargets.Add(target.Id);
                    connectedIds.Add(exporter.Id);
                    connectedIds.Add(target.Id);
                    colonizationEpochs[target.Id] = currentEpoch + 1;
                    launchesCompleted += 1;
                    attemptIndex += 1;
                }
            }
        }

        foreach (JumpLaneSystem system in systems)
        {
            state.Systems.Add(CloneSystem(system));
            if (!connectedIds.Contains(system.Id))
            {
                state.OrphanIds.Add(system.Id);
            }
        }

        return state;
    }

    private static List<JumpLaneSystem> CloneAndSortSystems(Godot.Collections.Array<JumpLaneSystem> source)
    {
        List<JumpLaneSystem> systems = new List<JumpLaneSystem>(source.Count);
        foreach (JumpLaneSystem system in source)
        {
            systems.Add(CloneSystem(system));
        }

        systems.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
        return systems;
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

    private static Dictionary<string, int> BuildInitialEpochMap(
        List<JumpLaneSystem> systems,
        ColonizationSimulationSettings settings)
    {
        Dictionary<string, int> epochs = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (JumpLaneSystem system in systems)
        {
            if (IsColonizationExporter(system, settings))
            {
                epochs[system.Id] = 0;
            }
        }

        return epochs;
    }

    private static List<JumpLaneSystem> GetExportersForEpoch(
        List<JumpLaneSystem> systems,
        Dictionary<string, int> colonizationEpochs,
        int currentEpoch,
        ColonizationSimulationSettings settings)
    {
        List<JumpLaneSystem> exporters = new List<JumpLaneSystem>();
        foreach (JumpLaneSystem system in systems)
        {
            if (!colonizationEpochs.TryGetValue(system.Id, out int epoch))
            {
                continue;
            }

            if (epoch != currentEpoch)
            {
                continue;
            }

            if (!IsColonizationExporter(system, settings))
            {
                continue;
            }

            exporters.Add(system);
        }

        exporters.Sort(CompareExporters);
        return exporters;
    }

    private static int CompareExporters(JumpLaneSystem left, JumpLaneSystem right)
    {
        int pressureComparison = right.ExportPressure.CompareTo(left.ExportPressure);
        if (pressureComparison != 0)
        {
            return pressureComparison;
        }

        int populationComparison = right.Population.CompareTo(left.Population);
        if (populationComparison != 0)
        {
            return populationComparison;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private static List<JumpLaneSystem> GetCandidateTargets(
        JumpLaneSystem exporter,
        List<JumpLaneSystem> systems,
        HashSet<string> claimedTargets)
    {
        List<JumpLaneSystem> candidates = new List<JumpLaneSystem>();
        foreach (JumpLaneSystem candidate in systems)
        {
            if (candidate.Id == exporter.Id)
            {
                continue;
            }

            if (candidate.Population > 0)
            {
                continue;
            }

            if (claimedTargets.Contains(candidate.Id))
            {
                continue;
            }

            if (candidate.ColonyTargetCapacity <= 0 || candidate.ColonyTargetScore <= 0.0)
            {
                continue;
            }

            double distance = exporter.DistanceTo(candidate);
            if (distance > exporter.ColonizationRangePc)
            {
                continue;
            }

            candidates.Add(candidate);
        }

        candidates.Sort((left, right) => CompareTargets(exporter, left, right));
        return candidates;
    }

    private static int CompareTargets(JumpLaneSystem exporter, JumpLaneSystem left, JumpLaneSystem right)
    {
        double leftDistance = exporter.DistanceTo(left);
        double rightDistance = exporter.DistanceTo(right);
        int distanceComparison = leftDistance.CompareTo(rightDistance);
        if (distanceComparison != 0)
        {
            return distanceComparison;
        }

        int targetScoreComparison = right.ColonyTargetScore.CompareTo(left.ColonyTargetScore);
        if (targetScoreComparison != 0)
        {
            return targetScoreComparison;
        }

        int capacityComparison = right.ColonyTargetCapacity.CompareTo(left.ColonyTargetCapacity);
        if (capacityComparison != 0)
        {
            return capacityComparison;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private static bool TryColonize(
        JumpLaneSystem exporter,
        JumpLaneSystem target,
        int currentEpoch,
        int attemptIndex,
        ColonizationSimulationSettings settings)
    {
        double distance = exporter.DistanceTo(target);
        double rangeFactor = NormalizeRange(distance, exporter.ColonizationRangePc);
        double techFactor = NormalizeTechLevel(exporter.RouteTechnologyLevel, settings);
        double permissiveness = System.Math.Clamp(settings.ExpansionPermissiveness, 0.0, 1.0);
        double successChance = 0.10
            + (permissiveness * 0.10)
            + (exporter.ExportPressure * (0.34 + (0.08 * permissiveness)))
            + (target.ColonyTargetScore * (0.22 + (0.06 * permissiveness)))
            + (rangeFactor * 0.10)
            + (techFactor * 0.10);
        successChance = System.Math.Clamp(successChance, 0.03, 0.97);

        long seed = PopulationSeeding.GeneratePopulationSeed(
            exporter.Id + "->" + target.Id + ":route_epoch_" + currentEpoch.ToString(),
            exporter.Population + target.ColonyTargetCapacity + attemptIndex + 1L);
        double roll = PopulationLikelihood.DeriveRollValue(seed, currentEpoch + (attemptIndex * 17));
        return roll < successChance;
    }

    private static ColonizationSettlementRecord ApplyColonization(
        JumpLaneSystem exporter,
        JumpLaneSystem target,
        int currentEpoch,
        ColonizationSimulationSettings settings)
    {
        int population = DetermineRoutePopulation(exporter, target, currentEpoch, settings);
        target.Population = population;
        target.RouteTechnologyLevel = exporter.RouteTechnologyLevel;
        target.ColonizationRangePc = ColonizationRouteCalculator.DetermineColonizationRange(target.RouteTechnologyLevel);
        target.ExportPressure = ColonizationRouteCalculator.CalculateExportPressure(target.Population, target.ColonyTargetCapacity);
        target.CanExportColonists = target.ExportPressure >= MinimumExportPressure
            && target.RouteTechnologyLevel >= settings.MinimumInterstellarTechLevel
            && TechnologyLevel.CanInterstellar((TechnologyLevel.Level)target.RouteTechnologyLevel);
        target.ExportBodyId = target.ColonyTargetBodyId;
        target.ExportCivilizationId = exporter.ExportCivilizationId;
        target.ExportCivilizationName = exporter.ExportCivilizationName;

        ColonizationSettlementRecord settlement = new ColonizationSettlementRecord
        {
            SourceSystemId = exporter.Id,
            SourceBodyId = exporter.ExportBodyId,
            DestinationSystemId = target.Id,
            DestinationBodyId = target.ColonyTargetBodyId,
            Epoch = currentEpoch + 1,
            Population = population,
            TechnologyLevel = exporter.RouteTechnologyLevel,
            FoundingCivilizationId = exporter.ExportCivilizationId,
            FoundingCivilizationName = exporter.ExportCivilizationName,
            RouteConnectionType = DetermineConnectionType(exporter.DistanceTo(target)),
            RouteDistancePc = exporter.DistanceTo(target),
        };
        return settlement;
    }

    private static int DetermineRoutePopulation(
        JumpLaneSystem exporter,
        JumpLaneSystem target,
        int currentEpoch,
        ColonizationSimulationSettings settings)
    {
        if (target.ColonyTargetCapacity <= 0)
        {
            return MinimumRoutePopulation;
        }

        double permissiveness = System.Math.Clamp(settings.ExpansionPermissiveness, 0.0, 1.0);
        double epochFactor = 0.56 + (0.10 * currentEpoch);
        double exporterFactor = 0.28 + (0.40 * exporter.ExportPressure);
        double desirabilityFactor = 0.24 + (0.44 * target.ColonyTargetScore);
        double permissiveLift = 0.06 + (0.10 * permissiveness);
        double utilization = System.Math.Clamp(epochFactor + exporterFactor + desirabilityFactor + permissiveLift, 0.20, 0.97);
        double rawPopulation = target.ColonyTargetCapacity * utilization;
        int population = (int)System.Math.Round(rawPopulation);
        if (population < MinimumRoutePopulation)
        {
            population = MinimumRoutePopulation;
        }

        if (population > target.ColonyTargetCapacity)
        {
            population = target.ColonyTargetCapacity;
        }

        return population;
    }

    private static int DetermineLaunchBudget(JumpLaneSystem exporter, ColonizationSimulationSettings settings)
    {
        int budget = 1;
        double permissiveness = System.Math.Clamp(settings.ExpansionPermissiveness, 0.0, 1.0);
        if (exporter.ExportPressure >= (0.55 - (0.10 * permissiveness)))
        {
            budget += 1;
        }

        if (exporter.ExportPressure >= (0.80 - (0.12 * permissiveness)))
        {
            budget += 1;
        }

        if (exporter.RouteTechnologyLevel >= (int)TechnologyLevel.Level.Advanced)
        {
            budget += 1;
        }

        return budget;
    }

    private static bool IsColonizationExporter(JumpLaneSystem system, ColonizationSimulationSettings settings)
    {
        if (!system.CanExportColonists)
        {
            return false;
        }

        if (system.Population <= 0)
        {
            return false;
        }

        if (system.ExportPressure < MinimumExportPressure)
        {
            return false;
        }

        if (system.ColonizationRangePc <= 0.0)
        {
            return false;
        }

        if (system.RouteTechnologyLevel < settings.MinimumInterstellarTechLevel)
        {
            return false;
        }

        return TechnologyLevel.CanInterstellar((TechnologyLevel.Level)system.RouteTechnologyLevel);
    }

    private static JumpLaneConnection.ConnectionType DetermineConnectionType(double distancePc)
    {
        if (distancePc <= JumpLaneCalculator.ThresholdDirectMedium)
        {
            return JumpLaneConnection.ConnectionType.Green;
        }

        if (distancePc <= JumpLaneClusterConnector.MaxExtendedDistance)
        {
            return JumpLaneConnection.ConnectionType.Orange;
        }

        return JumpLaneConnection.ConnectionType.Red;
    }

    private static double NormalizeRange(double distancePc, double rangePc)
    {
        if (rangePc <= 0.0)
        {
            return 0.0;
        }

        return 1.0 - System.Math.Clamp(distancePc / rangePc, 0.0, 1.0);
    }

    private static double NormalizeTechLevel(int routeTechnologyLevel, ColonizationSimulationSettings settings)
    {
        if (routeTechnologyLevel < settings.MinimumInterstellarTechLevel)
        {
            return 0.0;
        }

        int maxSpan = (int)TechnologyLevel.Level.Advanced - settings.MinimumInterstellarTechLevel;
        if (maxSpan <= 0)
        {
            return 1.0;
        }

        int adjustedLevel = routeTechnologyLevel - settings.MinimumInterstellarTechLevel;
        return System.Math.Clamp(adjustedLevel / (double)maxSpan, 0.0, 1.0);
    }
}
