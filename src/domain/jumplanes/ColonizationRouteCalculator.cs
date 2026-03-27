using System;
using System.Collections.Generic;
using StarGen.Domain.Colonization;
using StarGen.Domain.Population;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Builds deterministic non-Traveller jump routes from interstellar colonization pressure.
/// </summary>
public sealed class ColonizationRouteCalculator
{
    private const int MaxEpochs = 4;
    private const int MinRoutePopulation = 5000;
    private const double MinExportPressure = 0.18;

    /// <summary>
    /// Calculates a colonization-driven route network for the given region.
    /// </summary>
    public JumpLaneResult Calculate(JumpLaneRegion region)
    {
        ColonizationSimulationRequest request = new ColonizationSimulationRequest();
        request.RegionId = region.RegionId;
        request.Scope = region.Scope;
        request.Settings = ColonizationSimulationSettings.CreateDefault();
        request.Settings.ExpansionPermissiveness = 1.0;
        foreach (JumpLaneSystem system in region.Systems)
        {
            request.Systems.Add(system);
        }

        ColonizationSimulationState state = ColonizationSimulator.Run(request);
        return state.ToJumpLaneResult();
    }

    private static List<JumpLaneSystem> CloneSystems(JumpLaneRegion region)
    {
        List<JumpLaneSystem> systems = new(region.Systems.Count);
        foreach (JumpLaneSystem system in region.Systems)
        {
            JumpLaneSystem clone = new(system.Id, system.Position, system.Population);
            clone.FalsePopulation = system.FalsePopulation;
            clone.IsBridge = system.IsBridge;
            clone.TravellerProfile = system.TravellerProfile;
            clone.CanExportColonists = system.CanExportColonists;
            clone.ExportPressure = system.ExportPressure;
            clone.ColonyTargetScore = system.ColonyTargetScore;
            clone.ColonyTargetCapacity = system.ColonyTargetCapacity;
            clone.ColonizationRangePc = system.ColonizationRangePc;
            clone.RouteTechnologyLevel = system.RouteTechnologyLevel;
            systems.Add(clone);
        }

        return systems;
    }

    private static Dictionary<string, int> BuildInitialEpochMap(List<JumpLaneSystem> systems)
    {
        Dictionary<string, int> epochs = new(StringComparer.Ordinal);
        foreach (JumpLaneSystem system in systems)
        {
            if (IsColonizationExporter(system))
            {
                epochs[system.Id] = 0;
            }
        }

        return epochs;
    }

    private static List<JumpLaneSystem> GetExportersForEpoch(
        List<JumpLaneSystem> systems,
        Dictionary<string, int> colonizationEpochs,
        int currentEpoch)
    {
        List<JumpLaneSystem> exporters = new();
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

            if (!IsColonizationExporter(system))
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
        List<JumpLaneSystem> candidates = new();
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
        int attemptIndex)
    {
        double distance = exporter.DistanceTo(target);
        double rangeFactor = NormalizeRange(distance, exporter.ColonizationRangePc);
        double techFactor = NormalizeTechLevel(exporter.RouteTechnologyLevel);
        double successChance = 0.18
            + (exporter.ExportPressure * 0.42)
            + (target.ColonyTargetScore * 0.25)
            + (rangeFactor * 0.10)
            + (techFactor * 0.10);
        successChance = System.Math.Clamp(successChance, 0.05, 0.97);

        long seed = PopulationSeeding.GeneratePopulationSeed(
            exporter.Id + "->" + target.Id + ":route_epoch_" + currentEpoch.ToString(),
            exporter.Population + target.ColonyTargetCapacity + attemptIndex + 1L);
        double roll = PopulationLikelihood.DeriveRollValue(seed, currentEpoch + (attemptIndex * 17));
        return roll < successChance;
    }

    private static void ApplyColonization(JumpLaneSystem exporter, JumpLaneSystem target, int currentEpoch)
    {
        target.Population = DetermineRoutePopulation(exporter, target, currentEpoch);
        target.RouteTechnologyLevel = exporter.RouteTechnologyLevel;
        target.ColonizationRangePc = DetermineColonizationRange(target.RouteTechnologyLevel);
        target.ExportPressure = CalculateExportPressure(target.Population, target.ColonyTargetCapacity);
        target.CanExportColonists = target.ExportPressure >= MinExportPressure
            && TechnologyLevel.CanInterstellar((TechnologyLevel.Level)target.RouteTechnologyLevel);
    }

    private static int DetermineRoutePopulation(JumpLaneSystem exporter, JumpLaneSystem target, int currentEpoch)
    {
        if (target.ColonyTargetCapacity <= 0)
        {
            return MinRoutePopulation;
        }

        double epochFactor = 0.62 + (0.10 * currentEpoch);
        double exporterFactor = 0.35 + (0.35 * exporter.ExportPressure);
        double desirabilityFactor = 0.30 + (0.40 * target.ColonyTargetScore);
        double utilization = System.Math.Clamp(epochFactor + exporterFactor + desirabilityFactor, 0.25, 0.96);
        double rawPopulation = target.ColonyTargetCapacity * utilization;
        int population = (int)System.Math.Round(rawPopulation);
        if (population < MinRoutePopulation)
        {
            population = MinRoutePopulation;
        }

        if (population > target.ColonyTargetCapacity)
        {
            population = target.ColonyTargetCapacity;
        }

        return population;
    }

    private static int DetermineLaunchBudget(JumpLaneSystem exporter)
    {
        int budget = 1;
        if (exporter.ExportPressure >= 0.55)
        {
            budget += 1;
        }

        if (exporter.ExportPressure >= 0.80)
        {
            budget += 1;
        }

        if (exporter.RouteTechnologyLevel >= (int)TechnologyLevel.Level.Advanced)
        {
            budget += 1;
        }

        return budget;
    }

    private static bool IsColonizationExporter(JumpLaneSystem system)
    {
        if (!system.CanExportColonists)
        {
            return false;
        }

        if (system.Population <= 0)
        {
            return false;
        }

        if (system.ExportPressure < MinExportPressure)
        {
            return false;
        }

        if (system.ColonizationRangePc <= 0.0)
        {
            return false;
        }

        if (system.RouteTechnologyLevel < 0)
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

        if (distancePc <= JumpLaneCalculator.ThresholdBridgeMax)
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

        double normalized = 1.0 - System.Math.Clamp(distancePc / rangePc, 0.0, 1.0);
        return normalized;
    }

    private static double NormalizeTechLevel(int routeTechnologyLevel)
    {
        if (routeTechnologyLevel < (int)TechnologyLevel.Level.Interstellar)
        {
            return 0.0;
        }

        int maxSpan = (int)TechnologyLevel.Level.Advanced - (int)TechnologyLevel.Level.Interstellar;
        if (maxSpan <= 0)
        {
            return 1.0;
        }

        int adjustedLevel = routeTechnologyLevel - (int)TechnologyLevel.Level.Interstellar;
        return System.Math.Clamp(adjustedLevel / (double)maxSpan, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates colonization export pressure from current population versus carrying capacity.
    /// </summary>
    public static double CalculateExportPressure(int population, int carryingCapacity)
    {
        if (population <= 0)
        {
            return 0.0;
        }

        if (carryingCapacity <= 0)
        {
            return 1.0;
        }

        double occupancyRatio = population / (double)carryingCapacity;
        if (occupancyRatio < 0.65)
        {
            return 0.0;
        }

        if (occupancyRatio < 1.0)
        {
            double normalized = (occupancyRatio - 0.65) / 0.35;
            return System.Math.Clamp(normalized * normalized * 0.75, 0.0, 0.75);
        }

        double overflow = System.Math.Min(occupancyRatio - 1.0, 1.0);
        return System.Math.Clamp(0.75 + (overflow * 0.25), 0.0, 1.0);
    }

    /// <summary>
    /// Calculates target desirability from suitability and carrying capacity.
    /// </summary>
    public static double CalculateColonyTargetScore(ColonySuitability? suitability)
    {
        if (suitability == null || !suitability.IsColonizable() || suitability.CarryingCapacity <= 0)
        {
            return 0.0;
        }

        double suitabilityFactor = System.Math.Clamp(suitability.OverallScore / 100.0, 0.0, 1.0);
        double capacityFactor = System.Math.Clamp(System.Math.Log10(suitability.CarryingCapacity + 1.0) / 9.0, 0.0, 1.0);
        return System.Math.Clamp((suitabilityFactor * 0.65) + (capacityFactor * 0.35), 0.0, 1.0);
    }

    /// <summary>
    /// Calculates interstellar colonization range from route technology level.
    /// </summary>
    public static double DetermineColonizationRange(int routeTechnologyLevel)
    {
        if (routeTechnologyLevel < (int)TechnologyLevel.Level.Interstellar)
        {
            return 0.0;
        }

        if (routeTechnologyLevel >= (int)TechnologyLevel.Level.Advanced)
        {
            return JumpLaneClusterConnector.MaxExtendedDistance;
        }

        return JumpLaneCalculator.ThresholdBridgeMax;
    }
}
