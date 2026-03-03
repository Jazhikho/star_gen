using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Pure functions for determining station placement in a system.
/// </summary>
public static class StationPlacementRules
{
    /// <summary>
    /// Threshold for resource-rich systems.
    /// </summary>
    public const double ResourceRichThreshold = 0.4;

    /// <summary>
    /// Threshold for highly resource-rich systems.
    /// </summary>
    public const double HighResourceThreshold = 0.7;

    /// <summary>
    /// Evaluates a system and returns placement recommendations.
    /// </summary>
    public static StationPlacementRecommendation EvaluateSystem(StationSystemContext context)
    {
        StationPlacementRecommendation recommendation = new();
        recommendation.Context = DeterminePrimaryContext(context);
        recommendation.Reasoning.Add("Primary context: " + StationPlacementContext.ToStringName(recommendation.Context));

        switch (recommendation.Context)
        {
            case StationPlacementContext.Context.BridgeSystem:
                ApplyBridgeSystemRules(context, recommendation);
                break;
            case StationPlacementContext.Context.NativeWorld:
                ApplyNativeWorldRules(context, recommendation);
                break;
            case StationPlacementContext.Context.ColonyWorld:
                ApplyColonyWorldRules(context, recommendation);
                break;
            case StationPlacementContext.Context.ResourceSystem:
                ApplyResourceSystemRules(context, recommendation);
                break;
            case StationPlacementContext.Context.Strategic:
                ApplyStrategicRules(context, recommendation);
                break;
            case StationPlacementContext.Context.Scientific:
                ApplyScientificRules(context, recommendation);
                break;
            default:
                ApplyDefaultRules(context, recommendation);
                break;
        }

        return recommendation;
    }

    /// <summary>
    /// Returns whether a world should have orbital stations.
    /// </summary>
    public static bool ShouldHaveOrbitalStations(
        bool hasNative,
        TechnologyLevel.Level? nativeTech,
        bool hasColony)
    {
        if (hasColony)
        {
            return true;
        }

        if (hasNative && nativeTech.HasValue)
        {
            return TechnologyLevel.CanSpaceflight(nativeTech.Value);
        }

        return false;
    }

    /// <summary>
    /// Estimates orbital-station count for a populated world.
    /// </summary>
    public static int EstimateOrbitalStationCount(int population, TechnologyLevel.Level techLevel)
    {
        if (!TechnologyLevel.CanSpaceflight(techLevel))
        {
            return 0;
        }

        int baseCount = System.Math.Max(1, (int)(population / 10_000_000.0));
        if (techLevel >= TechnologyLevel.Level.Interstellar)
        {
            baseCount = (int)(baseCount * 1.5);
        }

        return System.Math.Min(baseCount, 20);
    }

    /// <summary>
    /// Recommends a station class for a placement context.
    /// </summary>
    public static StationClass.Class RecommendStationClass(
        StationPlacementContext.Context context,
        bool hasLargePopulation)
    {
        if (context == StationPlacementContext.Context.BridgeSystem)
        {
            return StationClass.Class.U;
        }

        if (context == StationPlacementContext.Context.Scientific)
        {
            return StationClass.Class.O;
        }

        if (context == StationPlacementContext.Context.ResourceSystem && !hasLargePopulation)
        {
            return StationClass.Class.O;
        }

        if (hasLargePopulation)
        {
            if (context == StationPlacementContext.Context.ColonyWorld
                || context == StationPlacementContext.Context.NativeWorld)
            {
                return StationClass.Class.A;
            }
        }

        return StationClass.Class.B;
    }

    /// <summary>
    /// Recommends purposes for a station in a given context.
    /// </summary>
    public static Array<StationPurpose.Purpose> RecommendPurposes(
        StationPlacementContext.Context context,
        bool isUtility)
    {
        if (isUtility)
        {
            return StationPurpose.TypicalUtilityPurposes();
        }

        return context switch
        {
            StationPlacementContext.Context.BridgeSystem => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Utility, StationPurpose.Purpose.Trade },
            StationPlacementContext.Context.ColonyWorld => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Trade, StationPurpose.Purpose.Residential, StationPurpose.Purpose.Industrial },
            StationPlacementContext.Context.NativeWorld => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Trade, StationPurpose.Purpose.Administrative },
            StationPlacementContext.Context.ResourceSystem => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Mining, StationPurpose.Purpose.Industrial },
            StationPlacementContext.Context.Strategic => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Military },
            StationPlacementContext.Context.Scientific => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Science },
            _ => new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Utility },
        };
    }

    /// <summary>
    /// Calculates resource-richness from weighted resource abundance.
    /// </summary>
    public static double CalculateResourceRichness(Dictionary resourceAbundance)
    {
        if (resourceAbundance.Count == 0)
        {
            return 0.0;
        }

        Dictionary weights = new()
        {
            [(int)ResourceType.Type.Water] = 1.5,
            [(int)ResourceType.Type.Metals] = 1.2,
            [(int)ResourceType.Type.RareElements] = 2.0,
            [(int)ResourceType.Type.Radioactives] = 1.8,
            [(int)ResourceType.Type.Hydrocarbons] = 1.3,
            [(int)ResourceType.Type.Exotics] = 2.5,
        };

        double total = 0.0;
        int count = 0;
        foreach (Variant resourceKey in resourceAbundance.Keys)
        {
            double abundance = (double)resourceAbundance[resourceKey];
            double weight = weights.ContainsKey(resourceKey) ? (double)weights[resourceKey] : 1.0;
            total += abundance * weight;
            count += 1;
        }

        if (count == 0)
        {
            return 0.0;
        }

        double normalized = total / (count * 2.0);
        return System.Math.Clamp(normalized, 0.0, 1.0);
    }

    /// <summary>
    /// Creates a system context from simplified system data.
    /// </summary>
    public static StationSystemContext CreateSystemContext(
        string systemId,
        Array<string> planetIds,
        int habitableCount,
        Array<Dictionary> nativeData,
        Array<string> colonyIds,
        double resourceRichness,
        int beltCount,
        Array<string> resourceBodyIds,
        bool isBridge = false)
    {
        StationSystemContext context = new()
        {
            SystemId = systemId,
            IsBridgeSystem = isBridge,
            PlanetIds = CloneStringArray(planetIds),
            HabitablePlanetCount = habitableCount,
            ColonyWorldCount = colonyIds.Count,
            ColonyPlanetIds = CloneStringArray(colonyIds),
            ResourceRichness = resourceRichness,
            AsteroidBeltCount = beltCount,
            ResourceBodyIds = CloneStringArray(resourceBodyIds),
            NativeWorldCount = nativeData.Count,
        };

        int highestTech = -1;
        foreach (Dictionary data in nativeData)
        {
            string bodyId = data.ContainsKey("body_id") ? (string)data["body_id"] : string.Empty;
            if (!string.IsNullOrEmpty(bodyId))
            {
                context.NativePlanetIds.Add(bodyId);
            }

            TechnologyLevel.Level tech = data.ContainsKey("tech_level")
                ? (TechnologyLevel.Level)(int)data["tech_level"]
                : TechnologyLevel.Level.StoneAge;
            if ((int)tech > highestTech)
            {
                highestTech = (int)tech;
                context.HighestNativeTech = tech;
            }

            if (TechnologyLevel.CanSpaceflight(tech))
            {
                context.HasSpacefaringNatives = true;
            }
        }

        return context;
    }

    /// <summary>
    /// Determines the primary placement context for a system.
    /// </summary>
    private static StationPlacementContext.Context DeterminePrimaryContext(StationSystemContext context)
    {
        if (context.IsBridgeSystem)
        {
            return StationPlacementContext.Context.BridgeSystem;
        }

        if (context.HasSpacefaringNatives)
        {
            return StationPlacementContext.Context.NativeWorld;
        }

        if (context.ColonyWorldCount > 0)
        {
            return StationPlacementContext.Context.ColonyWorld;
        }

        if (context.ResourceRichness >= ResourceRichThreshold && context.HabitablePlanetCount == 0)
        {
            return StationPlacementContext.Context.ResourceSystem;
        }

        if (context.NativeWorldCount > 0)
        {
            return StationPlacementContext.Context.Scientific;
        }

        if (context.HabitablePlanetCount > 0)
        {
            return StationPlacementContext.Context.Strategic;
        }

        return StationPlacementContext.Context.Other;
    }

    /// <summary>
    /// Applies bridge-system placement rules.
    /// </summary>
    private static void ApplyBridgeSystemRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Bridge systems typically have utility stations for travelers");
        recommendation.UtilityStationCount = 1;
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Utility);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
        recommendation.AllowDeepSpace = true;

        if (context.ColonyWorldCount > 0)
        {
            recommendation.LargeStationCount = context.ColonyWorldCount;
            AppendStrings(recommendation.OrbitalCandidates, context.ColonyPlanetIds);
            recommendation.Reasoning.Add("Colony worlds present; adding orbital stations");
        }
    }

    /// <summary>
    /// Applies native-world placement rules.
    /// </summary>
    private static void ApplyNativeWorldRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Spacefaring natives can support orbital stations");
        recommendation.LargeStationCount = context.NativeWorldCount * 2;
        AppendStrings(recommendation.OrbitalCandidates, context.NativePlanetIds);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Residential);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Administrative);

        if (context.ColonyWorldCount > 0)
        {
            recommendation.LargeStationCount += context.ColonyWorldCount;
            AppendStrings(recommendation.OrbitalCandidates, context.ColonyPlanetIds);
        }
    }

    /// <summary>
    /// Applies colony-world placement rules.
    /// </summary>
    private static void ApplyColonyWorldRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Colony worlds typically have orbital support stations");
        recommendation.LargeStationCount = context.ColonyWorldCount;
        AppendStrings(recommendation.OrbitalCandidates, context.ColonyPlanetIds);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Utility);

        if (context.ResourceRichness >= ResourceRichThreshold)
        {
            recommendation.OutpostCount = CalculateMiningOutposts(context);
            recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Mining);
            recommendation.AllowBeltStations = context.AsteroidBeltCount > 0;
            recommendation.Reasoning.Add("Resource richness supports mining outposts");
        }
    }

    /// <summary>
    /// Applies resource-system placement rules.
    /// </summary>
    private static void ApplyResourceSystemRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Resource-rich system without habitable worlds; stations become primary settlements");
        recommendation.OutpostCount = CalculateMiningOutposts(context);
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Mining);

        if (context.ResourceRichness >= HighResourceThreshold)
        {
            recommendation.LargeStationCount = 1;
            recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Residential);
            recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Industrial);
            recommendation.Reasoning.Add("High resources can support colony-sized station");
        }

        recommendation.AllowDeepSpace = true;
        recommendation.AllowBeltStations = context.AsteroidBeltCount > 0;
        AppendStrings(recommendation.OrbitalCandidates, context.ResourceBodyIds);
    }

    /// <summary>
    /// Applies strategic placement rules.
    /// </summary>
    private static void ApplyStrategicRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Strategic location with habitable worlds");
        recommendation.OutpostCount = 1;
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Military);
        if (context.HabitablePlanetCount > 0)
        {
            AppendStrings(recommendation.OrbitalCandidates, context.PlanetIds);
        }

        recommendation.AllowDeepSpace = true;
    }

    /// <summary>
    /// Applies scientific-interest placement rules.
    /// </summary>
    private static void ApplyScientificRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = true;
        recommendation.Reasoning.Add("Non-spacefaring natives present; scientific observation stations");
        recommendation.OutpostCount = context.NativeWorldCount;
        recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Science);
        AppendStrings(recommendation.OrbitalCandidates, context.NativePlanetIds);
        recommendation.AllowDeepSpace = true;
    }

    /// <summary>
    /// Applies fallback placement rules.
    /// </summary>
    private static void ApplyDefaultRules(StationSystemContext context, StationPlacementRecommendation recommendation)
    {
        recommendation.ShouldHaveStations = false;
        recommendation.Reasoning.Add("No significant reason for station presence");

        if (context.ResourceRichness > 0.2)
        {
            recommendation.ShouldHaveStations = true;
            recommendation.OutpostCount = 1;
            recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Mining);
            recommendation.AllowBeltStations = context.AsteroidBeltCount > 0;
            recommendation.Reasoning.Add("Minor resources may support small mining outpost");
        }
    }

    /// <summary>
    /// Calculates mining-outpost counts from resource conditions.
    /// </summary>
    private static int CalculateMiningOutposts(StationSystemContext context)
    {
        int baseCount = context.ResourceBodyIds.Count;
        if (context.ResourceRichness >= HighResourceThreshold)
        {
            baseCount = System.Math.Max(baseCount, 3);
        }
        else if (context.ResourceRichness >= ResourceRichThreshold)
        {
            baseCount = System.Math.Max(baseCount, 2);
        }
        else
        {
            baseCount = System.Math.Max(baseCount, 1);
        }

        baseCount += context.AsteroidBeltCount;
        return System.Math.Min(baseCount, 10);
    }

    /// <summary>
    /// Clones a typed string array.
    /// </summary>
    private static Array<string> CloneStringArray(Array<string> source)
    {
        Array<string> clone = new();
        foreach (string value in source)
        {
            clone.Add(value);
        }

        return clone;
    }

    /// <summary>
    /// Appends string values from one array to another.
    /// </summary>
    private static void AppendStrings(Array<string> destination, Array<string> source)
    {
        foreach (string value in source)
        {
            destination.Add(value);
        }
    }
}
