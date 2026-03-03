using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems;

/// <summary>
/// Generates candidate orbital slots for planets within orbit hosts.
/// </summary>
public static class OrbitSlotGenerator
{
    /// <summary>
    /// Minimum spacing factor between adjacent slots.
    /// </summary>
    public const double MinSpacingFactor = 0.15;

    /// <summary>
    /// Practical cap on slots per host.
    /// </summary>
    public const int MaxSlotsPerHost = 20;

    /// <summary>
    /// Resonance variation applied to spacing.
    /// </summary>
    public const double ResonanceVariation = 0.20;

    /// <summary>
    /// Exponential decay constant for fill probability.
    /// </summary>
    public const double ProbabilityDecay = 0.15;

    /// <summary>
    /// Safety margin for stellar radius.
    /// </summary>
    public const double StarRadiusSafetyMargin = 3.0;

    /// <summary>
    /// Generates orbit slots for a single orbit host.
    /// </summary>
    public static OrbitSlotGenerationResult GenerateForHost(
        OrbitHost host,
        double starRadiusM,
        Array<double> companionPositionsM,
        Array<double> companionMassesKg,
        SeededRng rng)
    {
        OrbitSlotGenerationResult result = new()
        {
            OrbitHostId = host.NodeId,
        };

        if (!host.HasValidZone())
        {
            return result;
        }

        Array<OrbitSlot> slots = new();
        int slotIndex = 0;
        double innerLimit = host.InnerStabilityM;
        double outerLimit = host.OuterStabilityM;
        double minSafeDistance = starRadiusM * StarRadiusSafetyMargin;
        double startingDistance = System.Math.Max(innerLimit, minSafeDistance);
        double firstOrbitFactor = rng.RandfRange(1.05f, 1.2f);
        double currentDistance = startingDistance * firstOrbitFactor;
        Array<double> resonanceRatios = OrbitalMechanics.GetCommonResonanceRatios();

        while (currentDistance < outerLimit && slotIndex < MaxSlotsPerHost)
        {
            OrbitSlot slot = new(
                $"slot_{host.NodeId}_{slotIndex}",
                host.NodeId,
                currentDistance);
            slot.Zone = ClassifyZone(currentDistance, host);
            slot.SuggestedEccentricity = CalculateSuggestedEccentricity(currentDistance, host, rng);
            slot.IsStable = CheckSlotStability(currentDistance, host, companionPositionsM, companionMassesKg);
            slot.FillProbability = CalculateFillProbability(currentDistance, host);
            slots.Add(slot);
            slotIndex += 1;

            int ratioIndex = rng.RandiRange(0, resonanceRatios.Count - 1);
            double ratio = resonanceRatios[ratioIndex];
            double nextDistance = OrbitalMechanics.CalculateResonanceSpacing(
                currentDistance,
                ratio,
                ResonanceVariation,
                rng);

            double minSpacingFraction = currentDistance * MinSpacingFactor;
            double minHillSpacingM = OrbitalMechanics.CalculateMinimumPlanetSpacing(
                Units.JupiterMassKg,
                Units.JupiterMassKg,
                host.CombinedMassKg,
                currentDistance);
            double minSpacing = System.Math.Max(minSpacingFraction, minHillSpacingM);
            if (nextDistance - currentDistance < minSpacing)
            {
                nextDistance = currentDistance + minSpacing;
            }

            currentDistance = nextDistance;
        }

        result.Slots = slots;
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Generates orbit slots for all supplied orbit hosts.
    /// </summary>
    public static Godot.Collections.Dictionary<string, Array<OrbitSlot>> GenerateAllSlots(
        Array<OrbitHost> hosts,
        Array<CelestialBody> stars,
        SystemHierarchy hierarchy,
        SeededRng rng)
    {
        Godot.Collections.Dictionary<string, Array<OrbitSlot>> allSlots = new();
        foreach (OrbitHost host in hosts)
        {
            double starRadiusM = GetHostStarRadius(host, stars, hierarchy);
            OrbitSlotGenerationResult result = GenerateForHost(
                host,
                starRadiusM,
                new Array<double>(),
                new Array<double>(),
                rng);
            if (result.Success)
            {
                allSlots[host.NodeId] = result.Slots;
            }
        }

        return allSlots;
    }

    /// <summary>
    /// Marks slots as stable or unstable against companion perturbations.
    /// </summary>
    public static void CheckStability(
        Array<OrbitSlot> slots,
        OrbitHost host,
        Array<double> companionMassesKg,
        Array<double> companionDistancesM,
        double hostPositionM = 0.0)
    {
        foreach (OrbitSlot slot in slots)
        {
            slot.IsStable = OrbitalMechanics.IsOrbitStable(
                slot.SemiMajorAxisM,
                host.CombinedMassKg,
                hostPositionM,
                companionMassesKg,
                companionDistancesM);
        }
    }

    /// <summary>
    /// Returns stable slots only.
    /// </summary>
    public static Array<OrbitSlot> FilterStable(Array<OrbitSlot> slots)
    {
        Array<OrbitSlot> result = new();
        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsStable)
            {
                result.Add(slot);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns available slots only.
    /// </summary>
    public static Array<OrbitSlot> FilterAvailable(Array<OrbitSlot> slots)
    {
        Array<OrbitSlot> result = new();
        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsAvailable())
            {
                result.Add(slot);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns slots in a specific zone.
    /// </summary>
    public static Array<OrbitSlot> FilterByZone(Array<OrbitSlot> slots, OrbitZone.Zone zone)
    {
        Array<OrbitSlot> result = new();
        foreach (OrbitSlot slot in slots)
        {
            if (slot.Zone == zone)
            {
                result.Add(slot);
            }
        }

        return result;
    }

    /// <summary>
    /// Sorts slots by orbital distance.
    /// </summary>
    public static void SortByDistance(Array<OrbitSlot> slots)
    {
        List<OrbitSlot> sorted = new();
        foreach (OrbitSlot slot in slots)
        {
            sorted.Add(slot);
        }

        sorted.Sort((left, right) => left.SemiMajorAxisM.CompareTo(right.SemiMajorAxisM));
        slots.Clear();
        foreach (OrbitSlot slot in sorted)
        {
            slots.Add(slot);
        }
    }

    /// <summary>
    /// Sorts slots by fill probability descending.
    /// </summary>
    public static void SortByProbability(Array<OrbitSlot> slots)
    {
        List<OrbitSlot> sorted = new();
        foreach (OrbitSlot slot in slots)
        {
            sorted.Add(slot);
        }

        sorted.Sort((left, right) => right.FillProbability.CompareTo(left.FillProbability));
        slots.Clear();
        foreach (OrbitSlot slot in sorted)
        {
            slots.Add(slot);
        }
    }

    /// <summary>
    /// Returns aggregate statistics for a slot set.
    /// </summary>
    public static Dictionary GetStatistics(Array<OrbitSlot> slots)
    {
        Dictionary stats = new()
        {
            ["total"] = slots.Count,
            ["stable"] = 0,
            ["unstable"] = 0,
            ["filled"] = 0,
            ["available"] = 0,
            ["hot"] = 0,
            ["temperate"] = 0,
            ["cold"] = 0,
            ["min_distance_au"] = 0.0,
            ["max_distance_au"] = 0.0,
            ["avg_fill_probability"] = 0.0,
        };

        if (slots.Count == 0)
        {
            return stats;
        }

        double probabilitySum = 0.0;
        double minDistance = slots[0].SemiMajorAxisM;
        double maxDistance = slots[0].SemiMajorAxisM;
        int stableCount = 0;
        int unstableCount = 0;
        int filledCount = 0;
        int availableCount = 0;
        int hotCount = 0;
        int temperateCount = 0;
        int coldCount = 0;

        foreach (OrbitSlot slot in slots)
        {
            if (slot.IsStable)
            {
                stableCount += 1;
            }
            else
            {
                unstableCount += 1;
            }

            if (slot.IsFilled)
            {
                filledCount += 1;
            }

            if (slot.IsAvailable())
            {
                availableCount += 1;
            }

            switch (slot.Zone)
            {
                case OrbitZone.Zone.Hot:
                    hotCount += 1;
                    break;
                case OrbitZone.Zone.Temperate:
                    temperateCount += 1;
                    break;
                case OrbitZone.Zone.Cold:
                    coldCount += 1;
                    break;
            }

            probabilitySum += slot.FillProbability;
            minDistance = System.Math.Min(minDistance, slot.SemiMajorAxisM);
            maxDistance = System.Math.Max(maxDistance, slot.SemiMajorAxisM);
        }

        stats["stable"] = stableCount;
        stats["unstable"] = unstableCount;
        stats["filled"] = filledCount;
        stats["available"] = availableCount;
        stats["hot"] = hotCount;
        stats["temperate"] = temperateCount;
        stats["cold"] = coldCount;
        stats["min_distance_au"] = minDistance / Units.AuMeters;
        stats["max_distance_au"] = maxDistance / Units.AuMeters;
        stats["avg_fill_probability"] = probabilitySum / slots.Count;
        return stats;
    }

    /// <summary>
    /// Returns the effective stellar radius for an orbit host.
    /// </summary>
    private static double GetHostStarRadius(OrbitHost host, Array<CelestialBody> stars, SystemHierarchy hierarchy)
    {
        HierarchyNode? node = hierarchy.FindNode(host.NodeId);
        if (node == null)
        {
            return Units.SolarRadiusMeters;
        }

        Array<string> starIds = node.GetAllStarIds();
        if (starIds.Count == 0)
        {
            return Units.SolarRadiusMeters;
        }

        if (host.Type == OrbitHost.HostType.SType && starIds.Count == 1)
        {
            foreach (CelestialBody star in stars)
            {
                if (star.Id == starIds[0])
                {
                    return star.Physical.RadiusM;
                }
            }
        }

        double maxRadius = 0.0;
        foreach (string starId in starIds)
        {
            foreach (CelestialBody star in stars)
            {
                if (star.Id == starId)
                {
                    maxRadius = System.Math.Max(maxRadius, star.Physical.RadiusM);
                }
            }
        }

        return maxRadius > 0.0 ? maxRadius : Units.SolarRadiusMeters;
    }

    /// <summary>
    /// Classifies a distance into an orbit zone for a host.
    /// </summary>
    private static OrbitZone.Zone ClassifyZone(double distanceM, OrbitHost host)
    {
        if (distanceM < host.HabitableZoneInnerM)
        {
            return OrbitZone.Zone.Hot;
        }

        if (distanceM > host.FrostLineM)
        {
            return OrbitZone.Zone.Cold;
        }

        return OrbitZone.Zone.Temperate;
    }

    /// <summary>
    /// Calculates a suggested eccentricity for a slot.
    /// </summary>
    private static double CalculateSuggestedEccentricity(double distanceM, OrbitHost host, SeededRng rng)
    {
        double zoneWidth = host.OuterStabilityM - host.InnerStabilityM;
        if (zoneWidth <= 0.0)
        {
            return 0.0;
        }

        double distanceFraction = (distanceM - host.InnerStabilityM) / zoneWidth;
        double maxEccentricity = 0.05 + (0.25 * distanceFraction);
        double raw = rng.Randf();
        return raw * raw * maxEccentricity;
    }

    /// <summary>
    /// Calculates fill probability for a slot.
    /// </summary>
    private static double CalculateFillProbability(double distanceM, OrbitHost host)
    {
        if (distanceM < host.InnerStabilityM || distanceM > host.OuterStabilityM)
        {
            return 0.0;
        }

        double distanceAu = distanceM / Units.AuMeters;
        double probability = System.Math.Exp(-ProbabilityDecay * distanceAu);
        return System.Math.Clamp(probability, 0.02, 1.0);
    }

    /// <summary>
    /// Checks whether a candidate slot is dynamically stable.
    /// </summary>
    private static bool CheckSlotStability(
        double distanceM,
        OrbitHost host,
        Array<double> companionPositionsM,
        Array<double> companionMassesKg)
    {
        if (companionPositionsM.Count == 0)
        {
            return true;
        }

        return OrbitalMechanics.IsOrbitStable(
            distanceM,
            host.CombinedMassKg,
            0.0,
            companionMassesKg,
            companionPositionsM);
    }
}
